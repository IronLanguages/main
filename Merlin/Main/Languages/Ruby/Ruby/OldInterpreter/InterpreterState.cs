/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Dynamic;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace Microsoft.Scripting.Interpretation {

    internal sealed class LambdaState {
        internal Dictionary<Expression, object> SpilledStack;
        internal YieldExpression CurrentYield;

        internal readonly IInterpretedScriptCode ScriptCode;
        internal readonly LambdaExpression Lambda;
        internal readonly InterpreterState Caller;
        internal SourceLocation CurrentLocation;

        public LambdaState(IInterpretedScriptCode scriptCode, LambdaExpression lambda, InterpreterState caller) {
            Assert.NotNull(scriptCode, lambda);
            ScriptCode = scriptCode;
            Lambda = lambda;
            Caller = caller;
        }
    }

    /// <summary>
    /// Represents variable storage for one lambda/scope expression in the
    /// interpreter.
    /// </summary>
    internal sealed class InterpreterState {
        // Current thread's interpreted method frame.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly ThreadLocal<InterpreterState> Current = new ThreadLocal<InterpreterState>();

        private readonly InterpreterState _lexicalParent;
        private readonly Dictionary<Expression, object> _vars = new Dictionary<Expression, object>();

        // per-lambda state, not-null
        private readonly LambdaState _lambdaState;

        private InterpreterState(InterpreterState parent, LambdaState lambdaState) {
            Assert.NotNull(lambdaState);
            _lexicalParent = parent;
            _lambdaState = lambdaState;
        }

        internal static InterpreterState CreateForTopLambda(IInterpretedScriptCode scriptCode, LambdaExpression lambda, InterpreterState caller, params object[] args) {
            return CreateForLambda(scriptCode, lambda, null, caller, args);
        }

        internal InterpreterState CreateForLambda(LambdaExpression lambda, InterpreterState caller, object[] args) {
            return CreateForLambda(_lambdaState.ScriptCode, lambda, this, caller, args);
        }

        internal InterpreterState CreateForGenerator(InterpreterState caller) {
            return new InterpreterState(this, new LambdaState(_lambdaState.ScriptCode, caller.Lambda, caller));
        }

        private static InterpreterState CreateForLambda(IInterpretedScriptCode scriptCode, LambdaExpression lambda, 
            InterpreterState lexicalParent, InterpreterState caller, object[] args) {

            InterpreterState state = new InterpreterState(lexicalParent, new LambdaState(scriptCode, lambda, caller));

            Debug.Assert(args.Length == lambda.Parameters.Count, "number of parameters should match number of arguments");
            
            //
            // Populate all parameters ...
            //
            for (int i = 0; i < lambda.Parameters.Count; i++ ) {
                state._vars.Add(lambda.Parameters[i], args[i]);
            }

            return state;
        }

        internal InterpreterState CreateForScope(BlockExpression scope) {
            InterpreterState state = new InterpreterState(this, _lambdaState);
            foreach (ParameterExpression v in scope.Variables) {
                // initialize variables to default(T)
                object value;
                if (v.Type.IsValueType) {
                    value = Activator.CreateInstance(v.Type);
                } else {
                    value = null;
                }
                state._vars.Add(v, value);
            }
            return state;
        }

        public InterpreterState Caller {
            get { return _lambdaState.Caller; }
        }

        public LambdaExpression Lambda {
            get { return _lambdaState.Lambda; }
        }

        public IInterpretedScriptCode ScriptCode {
            get { return _lambdaState.ScriptCode; }
        }

        public SourceLocation CurrentLocation {
            get { return _lambdaState.CurrentLocation; }
            internal set {
                _lambdaState.CurrentLocation = value; 
            }
        }    

        internal LambdaState LambdaState {
            get { return _lambdaState; }
        }

        internal YieldExpression CurrentYield {
            get { return _lambdaState.CurrentYield; }
            set { _lambdaState.CurrentYield = value; }
        }

        internal bool TryGetStackState<T>(Expression node, out T value) {
            object val;
            if (_lambdaState.SpilledStack != null && _lambdaState.SpilledStack.TryGetValue(node, out val)) {
                _lambdaState.SpilledStack.Remove(node);

                value = (T)val;
                return true;
            }

            value = default(T);
            return false;
        }

        internal void SaveStackState(Expression node, object value) {
            Debug.Assert(_lambdaState.CurrentYield != null);

            if (_lambdaState.SpilledStack == null) {
                _lambdaState.SpilledStack = new Dictionary<Expression, object>();
            }

            _lambdaState.SpilledStack[node] = value;
        }

        internal object GetValue(Expression variable) {
            InterpreterState state = this;
            for (; ; ) {
                object value;
                if (state._vars.TryGetValue(variable, out value)) {
                    return value;
                }
                state = state._lexicalParent;

                // Couldn't find variable
                if (state == null) {
                    throw InvalidVariableReference(variable);
                }
            }
        }

        internal void SetValue(Expression variable, object value) {
            InterpreterState state = this;
            for (; ; ) {
                if (state._vars.ContainsKey(variable)) {
                    state._vars[variable] = value;
                    return;
                }
                state = state._lexicalParent;

                // Couldn't find variable
                if (state == null) {
                    throw InvalidVariableReference(variable);
                }
            }
        }

        private static Exception InvalidVariableReference(Expression variable) {
            return new InvalidOperationException(string.Format("Variable '{0}' is not defined in an outer scope", variable));
        }
    }
}
