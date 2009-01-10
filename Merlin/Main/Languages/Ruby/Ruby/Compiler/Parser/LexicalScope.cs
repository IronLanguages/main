/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler.Ast;
using MSA = System.Linq.Expressions;
using IronRuby.Runtime;

namespace IronRuby.Compiler {

    // Scope contains variables defined outside of the current compilation unit. Used for assertion checks only.
    // (e.g. created for variables in the runtime scope of eval).
    internal sealed class RuntimeLexicalScope : LexicalScope {
        public RuntimeLexicalScope(List<string>/*!*/ names) 
            : base(null) {

            for (int i = 0; i < names.Count; i++) {
                AddVariable(names[i], SourceSpan.None);
            }
        }

        protected override bool IsRuntimeScope {
            get { return true; }
        }
    }

    public class LexicalScope : HybridStringDictionary<LocalVariable> {
        private readonly LexicalScope _outerScope;

        internal LexicalScope(LexicalScope outerScope) {
            _outerScope = outerScope;
        }

        public LexicalScope OuterScope {
            get { return _outerScope; }
        }

        protected virtual bool IsRuntimeScope {
            get { return false; }
        }

        public LocalVariable/*!*/ AddVariable(string/*!*/ name, SourceSpan location) {
            var var = new LocalVariable(name, location);
            Add(name, var);
            return var;
        }

        public LocalVariable/*!*/ ResolveOrAddVariable(string/*!*/ name, SourceSpan location) {
            var result = ResolveVariable(name);
            
            if (result != null) {
                return result;
            }

            return AddVariable(name, location);
        }

        public LocalVariable ResolveVariable(string/*!*/ name) {
            LexicalScope scope = this;
            do {
                LocalVariable result;
                if (scope.TryGetValue(name, out result)) {
                    return result;
                }
                scope = scope.OuterScope;
            } while (scope != null);
            return null;
        }

        #region Transformation

        internal void TransformLocals(ScopeBuilder/*!*/ locals) {
            Assert.NotNull(locals);
            Debug.Assert(!IsRuntimeScope);

            // Do not statically define variables defined in top-level eval'd code:
            //
            // eval('x = 1')   <-- this variable needs to be defined in containing runtime scope, not in top-level eval scope
            // eval('puts x')
            // 
            // eval('1.times { x = 1 }')  <-- x could be statically defined in the block since it is not visible outside the block
            //
            if (_outerScope == null || !_outerScope.IsRuntimeScope) {
                foreach (var entry in this) {
                    entry.Value.TransformDefinition(locals);
                }
            }
        }

        /// <summary>
        /// Updates local variable table on this scope with transformed parameters.
        /// </summary>
        internal MSA.ParameterExpression[]/*!*/ TransformParameters(Parameters parameters, int hiddenParameterCount) {

            int paramCount = hiddenParameterCount;

            if (parameters == null) {
                return new MSA.ParameterExpression[0];
            }

            if (parameters.Mandatory != null) {
                paramCount += parameters.Mandatory.Count;
            }

            if (parameters.Optional != null) {
                paramCount += parameters.Optional.Count;
            }

            if (parameters.Array != null) {
                paramCount += 1;
            }

            var result = new MSA.ParameterExpression[paramCount];

            int dlrParamIndex = hiddenParameterCount;

            if (parameters.Mandatory != null) {
                for (int i = 0; i < parameters.Mandatory.Count; i++) {
                    result[dlrParamIndex++] = parameters.Mandatory[i].TransformParameterDefinition();
                }
            }

            if (parameters.Optional != null) {
                for (int i = 0; i < parameters.Optional.Count; i++) {
                    result[dlrParamIndex++] = ((LocalVariable)parameters.Optional[i].Left).TransformParameterDefinition();
                }
            }

            if (parameters.Array != null) {
                result[dlrParamIndex++] = parameters.Array.TransformParameterDefinition();
            }

            Debug.Assert(dlrParamIndex == result.Length);

            return result;
        }

        internal static void TransformParametersToSuperCall(AstGenerator/*!*/ gen, CallBuilder/*!*/ callBuilder, Parameters parameters) {
            if (parameters == null) {
                return;
            }

            if (parameters.Mandatory != null) {
                foreach (Variable v in parameters.Mandatory) {
                    callBuilder.Add(v.TransformRead(gen));
                }
            }

            if (parameters.Optional != null) {
                foreach (SimpleAssignmentExpression s in parameters.Optional) {
                    callBuilder.Add(s.Left.TransformRead(gen));
                }
            }

            if (parameters.Array != null) {
                callBuilder.SplattedArgument = parameters.Array.TransformRead(gen);
            }
        }

        #endregion
    }

    #region HybridStringDictionary

    public class HybridStringDictionary<TValue> : IEnumerable<KeyValuePair<string, TValue>> {
        // Number of variables in scopes during Rails startup:
        // #variables    0     1     2    3    4    5   6+
        // #scopes    4587  3814  1994  794  608  220  295
        private const int ListLength = 4;

        private Dictionary<string, TValue> _dict;
        private KeyValuePair<string, TValue>[] _list;
        private int _size;

        public bool TryGetValue(string key, out TValue value) {
            for (int i = 0; i < _size; i++) {
                var entry = _list[i];
                if (entry.Key == key) {
                    value = entry.Value;
                    return true;
                }
            }

            if (_dict != null) {
                return _dict.TryGetValue(key, out value);
            }

            value = default(TValue);
            return false;
        }

        public void Add(string key, TValue value) {
            if (_size > 0) {
                if (_size < _list.Length) {
                    _list[_size++] = new KeyValuePair<string, TValue>(key, value);
                } else {
                    _dict = new Dictionary<string, TValue>();
                    for (int i = 0; i < _list.Length; i++) {
                        var entry = _list[i];
                        _dict.Add(entry.Key, entry.Value);
                    }
                    _dict.Add(key, value);
                    _list = null;
                    _size = -1;
                }
            } else if (_size == 0) {
                Debug.Assert(_list == null);
                _list = new KeyValuePair<string, TValue>[ListLength];
                _list[0] = new KeyValuePair<string, TValue>(key, value);
                _size = 1;
            } else {
                Debug.Assert(_size == -1 && _dict != null);
                _dict.Add(key, value);
            }
        }

        IEnumerator<KeyValuePair<string, TValue>>/*!*/ IEnumerable<KeyValuePair<string, TValue>>.GetEnumerator() {
            for (int i = 0; i < _size; i++) {
                yield return _list[i];
            }

            if (_dict != null) {
                foreach (var entry in _dict) {
                    yield return entry;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return ((IEnumerable<KeyValuePair<string, TValue>>)this).GetEnumerator();
        }
    }

    #endregion
}

