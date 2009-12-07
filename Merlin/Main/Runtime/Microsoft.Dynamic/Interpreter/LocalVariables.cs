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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Scripting.Interpreter {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")] // TODO: fix
    public struct LocalVariable {
        private const int IsBoxedFlag = 1;
        private const int InClosureFlag = 2;

        public readonly int Index;
        private readonly int _flags;

        public bool IsBoxed {
            get { return (_flags & IsBoxedFlag) != 0; }
        }

        public bool InClosure {
            get { return (_flags & InClosureFlag) != 0; }
        }

        public bool InClosureOrBoxed {
            get { return InClosure | IsBoxed; }
        }

        internal LocalVariable(int index, bool closure, bool boxed) {
            Index = index;
            _flags = (closure ? InClosureFlag : 0) | (boxed ? IsBoxedFlag : 0);
        }

        internal Expression LoadFromArray(Expression frameData, Expression closure) {
            Expression result = Expression.ArrayAccess(InClosure ? closure : frameData, Expression.Constant(Index));
            return IsBoxed ? Expression.Convert(result, typeof(StrongBox<object>)) : result;
        }

        public override string ToString() {
            return String.Format("{0}: {1} {2}", Index, IsBoxed ? "boxed" : null, InClosure ? "in closure" : null);
        }
    }

    // TODO: variable shadowing:  block(x) { block(x) { ... x ... } } 
    //       update LoopCompiler when implemented
    // TODO: slot reusing:        block { block(x) { ... x ... } block(y) { ... y ... } }
    public sealed class LocalVariables {
        private readonly Dictionary<ParameterExpression, LocalVariable> _variables = new Dictionary<ParameterExpression, LocalVariable>();

        private int _localCount;
        private int _closureSize;
        private int _boxedCount;

        internal LocalVariables() {
        }

        internal LocalVariable AddClosureVariable(ParameterExpression variable) {
            LocalVariable result = new LocalVariable(_closureSize++, true, false);
            _variables.Add(variable, result);
            return result;
        }

        public LocalVariable DefineLocal(ParameterExpression variable) {
            // the current variable definition shadows the existing one:
            if (_variables.ContainsKey(variable)) {
                throw new NotSupportedException("Variable shadowing not supported");
            }

            LocalVariable result = new LocalVariable(_localCount++, false, false);
            _variables.Add(variable, result);
            return result;
        }

        internal void Box(ParameterExpression variable) {
            LocalVariable local = _variables[variable];
            Debug.Assert(!local.IsBoxed && !local.InClosure);
            _variables[variable] = new LocalVariable(local.Index, false, true);
            _boxedCount++;
        }

        public int LocalCount {
            get { return _localCount; }
        }

        public int ClosureSize {
            get { return _closureSize; }
        }

        internal int[] GetBoxed() {
            if (_boxedCount == 0) {
                return null;
            }

            var result = new int[_boxedCount];
            int j = 0;
            foreach (LocalVariable local in _variables.Values) {
                if (local.IsBoxed) {
                    result[j++] = local.Index;
                }
            }
            Debug.Assert(j == result.Length);

            return result;
        }

        public int GetVariableIndex(ParameterExpression var) {
            LocalVariable loc;
            return _variables.TryGetValue(var, out loc) ? loc.Index : -1;
        }

        public bool TryGetLocal(ParameterExpression var, out LocalVariable local) {
            return _variables.TryGetValue(var, out local);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        public LocalVariable this[ParameterExpression var] {
            get { return _variables[var]; }
        }

        public bool ContainsVariable(ParameterExpression variable) {
            return _variables.ContainsKey(variable);
        }

        internal IEnumerable<ParameterExpression> GetClosureVariables() {
            foreach (var variable in _variables) {
                if (variable.Value.InClosure) {
                    yield return variable.Key;
                }
            }
        }
    }
}
