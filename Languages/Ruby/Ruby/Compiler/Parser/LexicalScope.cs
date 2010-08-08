/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif
    
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler.Ast;
using IronRuby.Runtime;

namespace IronRuby.Compiler {
    /// <summary>
    /// Represents a lexical scope in Ruby AST.
    /// </summary>
    public abstract class LexicalScope : HybridStringDictionary<LocalVariable> {
        // Null if there is no parent lexical scope or runtime scope.
        // Scopes:
        // - method and module: non-null, local variable lookup stops here
        // - source unit: RuntimeLexicalScope if eval, null otherwise
        // - block: non-null
        private readonly LexicalScope _outerScope;

        //
        // Lexical depth relative to the inner-most scope that doesn't inherit locals from its parent.
        // If depth >= 0 the scope defines static variables, otherwise it defines dynamic variables. 
        // 
        private readonly int _depth;

        // Note on dynamic scopes. 
        // We don't statically define variables defined in top-level eval'd code so the depth of the top-level scope is -1 
        // if the outer scope is a runtime scope.
        //
        // eval('x = 1')   <-- this variable needs to be defined in containing runtime scope, not in top-level eval scope
        // eval('puts x')
        // 
        // eval('1.times { x = 1 }')  <-- x could be statically defined in the block since it is not visible outside the block
        //
        internal LexicalScope(LexicalScope outerScope) {
            _outerScope = outerScope;
            _depth = IsTop ? 0 : (outerScope.IsRuntimeScope ? -1 : outerScope._depth + 1);
        }

        protected LexicalScope(LexicalScope outerScope, int depth) {
            _outerScope = outerScope;
            _depth = depth;
        }

        public int Depth {
            get { return _depth; }
        }

        public LexicalScope OuterScope {
            get { return _outerScope; }
        }

        protected virtual bool IsRuntimeScope {
            get { return false; }
        }

        protected virtual bool AllowsVariableDefinitions {
            get { return true; }
        }

        /// <summary>
        /// Variable lookup ends in this scope.
        /// </summary>
        internal virtual bool IsTop {
            get { return true; }
        }

        /// <summary>
        /// The top static scope for local variable lookup.
        /// </summary>
        internal virtual bool IsStaticTop {
            get { return true; }
        }

        public LocalVariable/*!*/ AddVariable(string/*!*/ name, SourceSpan location) {
            Debug.Assert(AllowsVariableDefinitions);
            var var = new LocalVariable(name, location, _depth);
            Add(name, var);
            return var;
        }

        public LocalVariable/*!*/ ResolveOrAddVariable(string/*!*/ name, SourceSpan location) {
            var result = ResolveVariable(name);
            
            if (result != null) {
                return result;
            }

            var targetScope = this;
            while (!targetScope.AllowsVariableDefinitions) {
                targetScope = targetScope.OuterScope;
            }

            return targetScope.AddVariable(name, location);
        }

        /// <summary>
        /// Looks the scope chain for a variable of a given name.
        /// Includes runtime scope in the lookup if available.
        /// </summary>
        public LocalVariable ResolveVariable(string/*!*/ name) {
            LexicalScope scope = this;
            while (true) {
                LocalVariable result;
                if (scope.TryGetValue(name, out result)) {
                    return result;
                }

                if (scope.IsTop) {
                    break;
                }
                scope = scope.OuterScope;
            }
            return null;
        }

        internal LexicalScope/*!*/ GetInnermostStaticTopScope() {
            Debug.Assert(!IsRuntimeScope);

            LexicalScope scope = this;
            while (!scope.IsStaticTop) {
                scope = scope.OuterScope;
                Debug.Assert(scope != null);
            }
            return scope;
        }

        #region Transformation

        internal int AllocateClosureSlotsForLocals(int closureIndex) {
            int localCount = 0;
            foreach (var local in this) {
                if (local.Value.ClosureIndex == -1) {
                    local.Value.SetClosureIndex(closureIndex++);
                    localCount++;
                }
            }
            return localCount;
        }

        #endregion
    }

    /// <summary>
    /// BEGIN block and source unit scopes.
    /// </summary>
    internal sealed class TopStaticLexicalScope : LexicalScope {
        public TopStaticLexicalScope(LexicalScope outerScope)
            : base(outerScope) {
        }

        /// <summary>
        /// Local variable lookup ends here if there is no runtime outer scope.
        /// </summary>
        internal override bool IsTop {
            get { return OuterScope == null; }
        }
    }

    /// <summary>
    /// Instance method scope.
    /// </summary>
    internal sealed class MethodLexicalScope : LexicalScope {
        public MethodLexicalScope(LexicalScope/*!*/ outerScope)
            : base(outerScope) {
            Debug.Assert(outerScope != null);
        }
    }

    /// <summary>
    /// Class scope.
    /// </summary>
    internal sealed class ClassLexicalScope : LexicalScope {
        public ClassLexicalScope(LexicalScope/*!*/ outerScope)
            : base(outerScope) {
            Debug.Assert(outerScope != null);
        }
    }
    
    /// <summary>
    /// Singleton method, singleton class and module. 
    /// </summary>
    internal sealed class TopLocalDefinitionLexicalScope : LexicalScope {
        public TopLocalDefinitionLexicalScope(LexicalScope/*!*/ outerScope)
            : base(outerScope) {
            Debug.Assert(outerScope != null);
        }
    }

    /// <summary>
    /// Block scope.
    /// </summary>
    internal sealed class BlockLexicalScope : LexicalScope {
        public BlockLexicalScope(LexicalScope/*!*/ outerScope)
            : base(outerScope) {
            Debug.Assert(outerScope != null);
        }

        internal override bool IsTop {
            get { return false; }
        }

        internal override bool IsStaticTop {
            get { return false; }
        }
    }

    /// <summary>
    /// for-loop scope.
    /// </summary>
    internal sealed class PaddingLexicalScope : LexicalScope {
        public PaddingLexicalScope(LexicalScope/*!*/ outerScope) 
            : base(outerScope) {
            Debug.Assert(outerScope != null);
        }

        protected override bool AllowsVariableDefinitions {
            get { return false; }
        }

        internal override bool IsTop {
            get { return false; }
        }

        internal override bool IsStaticTop {
            get { return false; }
        }
    }

    // Scope contains variables defined outside of the current compilation unit. Used for assertion checks only.
    // (e.g. created for variables in the runtime scope of eval).
    internal sealed class RuntimeLexicalScope : LexicalScope {
        public RuntimeLexicalScope(List<string>/*!*/ names)
            : base(null, -1) {

            for (int i = 0; i < names.Count; i++) {
                AddVariable(names[i], SourceSpan.None);
            }
        }

        protected override bool IsRuntimeScope {
            get { return true; }
        }

        internal override bool IsTop {
            get { return true; }
        }

        internal override bool IsStaticTop {
            get { return false; }
        }
    }

    #region HybridStringDictionary

    public class HybridStringDictionary<TValue> : IEnumerable<KeyValuePair<string, TValue>> {
        // Number of variables in scopes during Rails startup:
        // #variables    0     1     2    3    4    5   6+
        // #scopes    4587  3814  1994  794  608  220  295
        private const int ListLength = 4;

        private Dictionary<string, TValue> _dict;
        private KeyValuePair<string, TValue>[] _list;
        private int _listSize;

        public int Count {
            get { return _listSize + (_dict != null ? _dict.Count : 0); }
        }

        public bool TryGetValue(string key, out TValue value) {
            for (int i = 0; i < _listSize; i++) {
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
            if (_listSize > 0) {
                if (_listSize < _list.Length) {
                    _list[_listSize++] = new KeyValuePair<string, TValue>(key, value);
                } else {
                    _dict = new Dictionary<string, TValue>();
                    for (int i = 0; i < _list.Length; i++) {
                        var entry = _list[i];
                        _dict.Add(entry.Key, entry.Value);
                    }
                    _dict.Add(key, value);
                    _list = null;
                    _listSize = -1;
                }
            } else if (_listSize == 0) {
                Debug.Assert(_list == null);
                _list = new KeyValuePair<string, TValue>[ListLength];
                _list[0] = new KeyValuePair<string, TValue>(key, value);
                _listSize = 1;
            } else {
                Debug.Assert(_listSize == -1 && _dict != null);
                _dict.Add(key, value);
            }
        }

        IEnumerator<KeyValuePair<string, TValue>>/*!*/ IEnumerable<KeyValuePair<string, TValue>>.GetEnumerator() {
            for (int i = 0; i < _listSize; i++) {
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

