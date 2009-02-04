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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpreter {
    
    /// <summary>
    /// Manages creation of interpreted delegates. These delegates will get
    /// compiled if they are executed often enough.
    /// </summary>
    internal sealed class LightDelegateCreator {
        private readonly Interpreter _interpreter;
        private readonly LambdaExpression _lambda;
        private readonly IList<ParameterExpression> _closureVariables;

        // Adaptive compilation support:
        private Func<StrongBox<object>[], Delegate> _compiled;
        private bool _delegateTypesMatch;
        private int _executionCount;
        private int _startedCompile;

        // List of LightLambdas that need to be updated once we compile
        private WeakCollection<LightLambda> _lightLambdas = new WeakCollection<LightLambda>();

        private const int CompilationThreshold = 2;

        internal LightDelegateCreator(Interpreter interpreter, LambdaExpression lambda, IList<ParameterExpression> closureVariables) {
            _interpreter = interpreter;
            _lambda = lambda;
            _closureVariables = closureVariables;
        }

        internal IList<ParameterExpression> ClosureVariables {
            get { return _closureVariables; }
        }

        internal Func<StrongBox<object>[], Delegate> Compiled {
            get { return _compiled; }
        }

        internal Delegate CreateDelegate(StrongBox<object>[] closure) {
            if (_compiled != null) {
                return CreateCompiledDelegate(closure);
            }

            // Otherwise, we'll create an interpreted LightLambda
            var ret = new LightLambda(_interpreter, closure, this);
            
            lock (this) {
                // If this field is now null, it means a compile happened
                if (_lightLambdas != null) {
                    _lightLambdas.Add(ret);
                }
            }

            if (_lightLambdas == null) {
                return CreateCompiledDelegate(closure);
            }

            return ret.MakeDelegate(_lambda.Type);
        }

        private Delegate CreateCompiledDelegate(StrongBox<object>[] closure) {
            // It's already compiled, and the types match, just use the
            // delegate directly.
            Delegate d = _compiled(closure);

            // The types might not match, if the delegate type we want is
            // not a Func/Action. In that case, use LightLambda to create
            // a new delegate of the right type. This is not the most
            // efficient approach, but to do better we need the ability to
            // compile into a DynamicMethod that we created.
            if (d.GetType() != _lambda.Type) {
                var ret = new LightLambda(_interpreter, closure, this);
                ret.Compiled = d;
                d = ret.MakeDelegate(_lambda.Type);
            }

            return d;
        }

        /// <summary>
        /// Create a compiled delegate for the LightLambda, and saves it so
        /// future calls to Run will execute the compiled code instead of
        /// interpreting.
        /// </summary>
        internal void Compile(object state) {
            _compiled = LightLambdaClosureVisitor.BindLambda(_lambda, _closureVariables, out _delegateTypesMatch);

            // Get the list and replace it with null to free it.
            WeakCollection<LightLambda> list = _lightLambdas;
            lock (this) {
                _lightLambdas = null;
            }

            // Walk the list and set delegates for all of the lambdas
            foreach (LightLambda light in list) {
                light.Compiled = _compiled(light.Closure);
            }
        }

        /// <summary>
        /// Updates the execution count of this light delegate. If a certain
        /// threshold is reached, it will start a background compilation.
        /// </summary>
        internal void UpdateExecutionCount() {
            // Don't lock here because it's a frequently hit path.
            //
            // There could be multiple threads racing, but that is okay.
            // Two bad things can happen:
            //   * We miss an increment because one thread sets the counter back
            //   * We might enter the if branch more than once.
            //
            // The first is okay, it just means we take longer to compile.
            // The second we explicitly guard against inside the if.
            //
            if (++_executionCount == CompilationThreshold) {
                if (Interlocked.Exchange(ref _startedCompile, 1) == 0) {
                    // Kick off the compile on another thread so this one can keep going
                    ThreadPool.QueueUserWorkItem(Compile, null);
                }
            }
        }
    }
}
