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

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using System.Runtime.CompilerServices;
using System;
using System.Threading;
using IronRuby.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    
    public partial class RangeExpression : Expression {
        private readonly Expression/*!*/ _begin;
        private readonly Expression/*!*/ _end;
        private readonly bool _isExclusive;
        
        public Expression/*!*/ Begin {
            get { return _begin; }
        }

        public Expression/*!*/ End {
            get { return _end; }
        }

        public bool IsExclusive {
            get { return _isExclusive; }
        }

        public RangeExpression(Expression/*!*/ begin, Expression/*!*/ end, bool isExclusive,SourceSpan location) 
            : base(location) {
            Assert.NotNull(begin, end);
            _begin = begin;
            _end = end;
            _isExclusive = isExclusive;
        }

        private bool IsIntegerRange(out int intBegin, out int intEnd) {
            Literal literalBegin, literalEnd;
            if ((literalBegin = _begin as Literal) != null && (literalBegin.Value is int) &&
                (literalEnd = _end as Literal) != null && (literalEnd.Value is int)) {
                intBegin = (int)literalBegin.Value;
                intEnd = (int)literalEnd.Value;
                return true;
            } else {
                intBegin = intEnd = 0;
                return false;
            }
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            int intBegin, intEnd;
            if (IsIntegerRange(out intBegin, out intEnd)) {
                return (_isExclusive ? Methods.CreateExclusiveIntegerRange : Methods.CreateInclusiveIntegerRange).OpCall(
                    AstUtils.Constant(intBegin), AstUtils.Constant(intEnd)
                );
            } else {
                return (_isExclusive ? Methods.CreateExclusiveRange : Methods.CreateInclusiveRange).OpCall(
                    AstUtils.Box(_begin.TransformRead(gen)),
                    AstUtils.Box(_end.TransformRead(gen)), 
                    gen.CurrentScopeVariable, 
                    AstUtils.Constant(new BinaryOpStorage(gen.Context))
                );
            }
        }

        private static int _flipFlopVariableId;

        internal override Expression/*!*/ ToCondition(LexicalScope/*!*/ currentScope) {
            int intBegin, intEnd;
            if (!IsIntegerRange(out intBegin, out intEnd)) {
                return new RangeCondition(
                    this,
                    currentScope.GetInnermostStaticTopScope().AddVariable("#FlipFlopState" + Interlocked.Increment(ref _flipFlopVariableId), Location)
                );
            }
            return this;
        }
    }

    public partial class RangeCondition : Expression {
        private readonly RangeExpression/*!*/ _range;
        private readonly LocalVariable/*!*/ _stateVariable;

        public RangeExpression/*!*/ Range {
            get { return _range; }
        }

        internal RangeCondition(RangeExpression/*!*/ range, LocalVariable/*!*/ stateVariable)
            : base(range.Location) {
            Assert.NotNull(range, stateVariable);
            _range = range;
            _stateVariable = stateVariable;
        }

        /// <code>
        /// End-exclusive range:
        ///   if state
        ///     state = IsFalse({end})  
        ///     true
        ///   else
        ///     state = IsTrue({begin})
        ///   end
        ///     
        /// End-inclusive range:
        ///   if state || IsTrue({begin})  
        ///     state = IsFalse({end})
        ///     true
        ///   else  
        ///     false
        ///   end  
        /// </code>
        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            var begin = AstUtils.Box(_range.Begin.TransformRead(gen));
            var end = AstUtils.Box(_range.End.TransformRead(gen));

            // state: 
            // false <=> null
            // true <=> non-null
            if (_range.IsExclusive) {
                return Ast.Condition(
                    Ast.ReferenceNotEqual(
                        _stateVariable.TransformReadVariable(gen, false), 
                        AstUtils.Constant(null)
                    ),
                    Ast.Block(
                        _stateVariable.TransformWriteVariable(gen, Methods.NullIfTrue.OpCall(end)), 
                        AstUtils.Constant(true)
                    ),
                    Ast.ReferenceNotEqual(
                        _stateVariable.TransformWriteVariable(gen, Methods.NullIfFalse.OpCall(begin)),
                        AstUtils.Constant(null)
                    )
                );
            } else {
                return Ast.Condition(
                    Ast.OrElse(
                        Ast.ReferenceNotEqual(
                            _stateVariable.TransformReadVariable(gen, false), 
                            AstUtils.Constant(null)
                        ), 
                        Methods.IsTrue.OpCall(begin)
                    ),
                    Ast.Block(
                        _stateVariable.TransformWriteVariable(gen, Methods.NullIfTrue.OpCall(end)),
                        AstUtils.Constant(true)
                    ),
                    AstUtils.Constant(false)
                );
            }
        }
    }
}
