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
using System.Runtime.CompilerServices;
using System.Threading;
using IronRuby.Runtime;
using Microsoft.Scripting;
using System.Collections.ObjectModel;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using AstExpressions = ReadOnlyCollectionBuilder<MSA.Expression>;
    using AstParameters = ReadOnlyCollectionBuilder<MSA.ParameterExpression>;
    
    internal sealed class ScopeBuilder {
        private readonly AstParameters _parameters;
        private readonly int _firstClosureParam;
        private readonly int _localCount;
        private readonly LexicalScope/*!*/ _lexicalScope;
        private readonly ScopeBuilder _parent;

        private readonly AstParameters/*!*/ _hiddenVariables;

        // constant access site cache variables:
        private MSA.ParameterExpression _csCacheVariable;
        private MSA.ParameterExpression _csValueVariable;

        // defined? site cache variables:
        private MSA.ParameterExpression _dcsCacheVariable;

        // Local variables that hold on the closures used within this scope.
        // Closure #0 stores the parent scope's local variables, closure #1 stores grand parent scope's locals, etc.
        private List<MSA.ParameterExpression> _closures;
        private ScopeBuilder _outermostClosureReferredTo;

        // Stores this scope's local variables.
        private readonly MSA.ParameterExpression/*!*/ _localsTuple;
        
#if DEBUG
        private static int _Id;
        private int _id;
#endif

        public ScopeBuilder(int localCount, ScopeBuilder parent, LexicalScope/*!*/ lexicalScope)
            : this(null, -1, localCount, parent, lexicalScope) {
        }

        public ScopeBuilder(AstParameters parameters, int firstClosureParam, int localCount, 
            ScopeBuilder parent, LexicalScope/*!*/ lexicalScope) {
            Debug.Assert(parent == null || parent.LexicalScope == lexicalScope.OuterScope);
#if DEBUG
            _id = Interlocked.Increment(ref _Id);
#endif
            _parent = parent;
            _parameters = parameters;
            _localCount = localCount;
            _firstClosureParam = firstClosureParam;
            _lexicalScope = lexicalScope;
            _hiddenVariables = new AstParameters();
            _localsTuple = DefineHiddenVariable("#locals", MakeLocalsTupleType());
            _outermostClosureReferredTo = this;
        }

        private Type/*!*/ MakeLocalsTupleType() {
            // Note: The actual tuple type might be a subclass of the type used here. Accesses to the additional fields would need to down-cast.
            // This will only happen if a hidden lifted variable is defined, which is needed only for flip-flop operator so far.
            return RubyOps.MakeObjectTupleType(LiftedVisibleVariableCount);            
        }

        internal ScopeBuilder Parent {
            get { return _parent; }
        }

        internal LexicalScope/*!*/ LexicalScope {
            get { return _lexicalScope; }
        }

        internal MSA.ParameterExpression/*!*/ GetClosure(int definitionDepth) {
            Debug.Assert(definitionDepth >= 0);
            
            int delta = _lexicalScope.Depth - definitionDepth - 1;
            if (delta == -1) {
                return _localsTuple;
            }

            if (_closures == null) {
                _closures = new List<MSA.ParameterExpression>();
            }

            while (delta >= _closures.Count) {
                // next closure builder that hasn't been accessed yet:
                _outermostClosureReferredTo = _outermostClosureReferredTo.Parent;
                _closures.Add(DefineHiddenVariable("#closure" + delta, _outermostClosureReferredTo._localsTuple.Type));
            }
            return _closures[delta];
        }

        public MSA.ParameterExpression/*!*/ DefineHiddenVariable(string/*!*/ name, Type/*!*/ type) {
#if DEBUG
            name += "_" + _id + "_" + _hiddenVariables.Count;
#endif
            return AddHidden(Ast.Variable(type, name));
        }

        public MSA.ParameterExpression/*!*/ AddHidden(MSA.ParameterExpression/*!*/ variable) {
            _hiddenVariables.Add(variable);
            return variable;
        }

        public void GetConstantSiteCacheVariables(out MSA.ParameterExpression/*!*/ cacheVar, out MSA.ParameterExpression/*!*/ valueVar) {
            if (_csCacheVariable == null) {
                _csCacheVariable = DefineHiddenVariable("c_site", typeof(ConstantSiteCache));
                _csValueVariable = DefineHiddenVariable("c_value", typeof(object));
            }
            cacheVar = _csCacheVariable;
            valueVar = _csValueVariable;
        }

        public void GetIsDefinedConstantSiteCacheVariables(out MSA.ParameterExpression/*!*/ cacheVar) {
            if (_dcsCacheVariable == null) {
                _dcsCacheVariable = DefineHiddenVariable("dc_site", typeof(IsDefinedConstantSiteCache));
            }
            cacheVar = _dcsCacheVariable;
        }

        private int LiftedVisibleVariableCount {
            get { return (_parameters != null ? _parameters.Count - _firstClosureParam : 0) + _localCount; }
        }

        public MSA.Expression/*!*/ GetVariableAccessor(int definitionLexicalDepth, int closureIndex) {
            Debug.Assert(definitionLexicalDepth >= 0);
            Debug.Assert(closureIndex >= 0);

            return GetVariableAccessor(GetClosure(definitionLexicalDepth), closureIndex);
        }

        public static MSA.Expression/*!*/ GetVariableAccessor(MSA.Expression/*!*/ tupleVariable, int tupleFieldIndex) {
            MSA.Expression accessor = tupleVariable;

            foreach (var property in MutableTuple.GetAccessPath(tupleVariable.Type, tupleFieldIndex)) {
                accessor = Ast.Property(accessor, property);
            }

            return accessor;
        }

        public MSA.Expression/*!*/ MakeLocalsStorage() {
            // TODO: use a factory instead of Ast.New to improve interpreter perf:
            MSA.Expression result = Ast.Assign(
                _localsTuple, 
                LiftedVisibleVariableCount == 0 ? (MSA.Expression)Ast.Constant(null, _localsTuple.Type) : Ast.New(_localsTuple.Type)
            );
            
            if (_parameters != null) {
                var initializers = new AstExpressions();
                initializers.Add(result);
                
                // parameters map to the initial elements of the array:
                for (int i = _firstClosureParam, j = 0; i < _parameters.Count; i++, j++) {
                    initializers.Add(Ast.Assign(GetVariableAccessor(_localsTuple, j), _parameters[i]));
                }

                initializers.Add(_localsTuple);
                result = Ast.Block(initializers);
            }

            return result;
        }

        public MSA.Expression/*!*/ GetVariableNamesExpression() {
            if (LiftedVisibleVariableCount == 0) {
                return Ast.Constant(null, typeof(string[]));
            }

            string[] symbols = new string[LiftedVisibleVariableCount];

            foreach (var var in _lexicalScope) {
                symbols[var.Value.ClosureIndex] = var.Value.Name;
            }

            return Ast.Constant(symbols);
        }

        public MSA.Expression/*!*/ CreateScope(MSA.Expression/*!*/ body) {
            Debug.Assert(_closures == null);
            return Ast.Block(_hiddenVariables, body);
        }

        public MSA.Expression/*!*/ CreateScope(MSA.Expression/*!*/ scopeVariable, MSA.Expression/*!*/ scopeInitializer, MSA.Expression/*!*/ body) {
            // #locals variable already assigned (in MakeClosureDefinition).
            // We need to initialize #closureN variables.
            if (_closures != null) {
                if (_closures.Count == 1) {
                    return Ast.Block(_hiddenVariables, Ast.Block(
                        Ast.Assign(
                            _closures[0], 
                            AstUtils.Convert(Methods.GetParentLocals.OpCall(Ast.Assign(scopeVariable, scopeInitializer)), _closures[0].Type)
                        ),
                        body
                    ));
                } else {
                    var result = new AstExpressions();
                    MSA.Expression tempScope = DefineHiddenVariable("#s", typeof(RubyScope));
                    result.Add(Ast.Assign(scopeVariable, scopeInitializer));

                    for (int i = 0; i < _closures.Count; i++) {
                        result.Add(
                            Ast.Assign(
                                _closures[i],
                                AstUtils.Convert(
                                    Methods.GetLocals.OpCall(Ast.Assign(tempScope, Methods.GetParentScope.OpCall(i == 0 ? scopeVariable : tempScope))), 
                                    _closures[i].Type
                                )
                            )
                        );
                    }
                    result.Add(body);
                    return Ast.Block(_hiddenVariables, result);
                }
            } else {
                return Ast.Block(_hiddenVariables, Ast.Block(Ast.Assign(scopeVariable, scopeInitializer), body));
            }
        }
    }
}
