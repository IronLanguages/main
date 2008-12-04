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

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using System.Runtime.CompilerServices;
using System;
using IronRuby.Runtime;

namespace IronRuby.Compiler.Ast {
    using MSA = System.Linq.Expressions;
    using Ast = System.Linq.Expressions.Expression;
    
    public partial class RangeExpression : Expression {
        private readonly Expression/*!*/ _begin;
        private readonly Expression/*!*/ _end;
        private readonly bool _isExclusive;
        private bool _isCondition;
        
        public Expression/*!*/ Begin {
            get { return _begin; }
        }

        public Expression/*!*/ End {
            get { return _end; }
        }

        public bool IsExclusive {
            get { return _isExclusive; }
        }

        public bool IsCondition {
            get { return _isCondition; }
        }

        public RangeExpression(Expression/*!*/ begin, Expression/*!*/ end, bool isExclusive, bool isCondition, SourceSpan location) 
            : base(location) {
            Assert.NotNull(begin, end);
            _begin = begin;
            _end = end;
            _isExclusive = isExclusive;
            _isCondition = isCondition;
        }
    
        public RangeExpression(Expression/*!*/ begin, Expression/*!*/ end, bool isExclusive, SourceSpan location) 
            : this(begin, end, isExclusive, false, location) {
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
            if (_isCondition) {
                return TransformReadCondition(gen);
            } else if (IsIntegerRange(out intBegin, out intEnd)) {
                return (_isExclusive ? Methods.CreateExclusiveIntegerRange : Methods.CreateInclusiveIntegerRange).OpCall(
                    Ast.Constant(intBegin), Ast.Constant(intEnd)
                );
            } else {
                return (_isExclusive ? Methods.CreateExclusiveRange : Methods.CreateInclusiveRange).OpCall(
                    AstFactory.Box(_begin.TransformRead(gen)), 
                    AstFactory.Box(_end.TransformRead(gen)), 
                    gen.CurrentScopeVariable, 
                    Ast.Constant(new SiteLocalStorage<CallSite<Func<CallSite, RubyContext, object, object, object>>>())
                );
            }
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
        private MSA.Expression/*!*/ TransformReadCondition(AstGenerator/*!*/ gen) {
            // Define state variable in the inner most method scope.
            var stateVariable = gen.CurrentMethod.Builder.DefineHiddenVariable("#in_range", typeof(bool));

            var begin = AstFactory.Box(_begin.TransformRead(gen));
            var end = AstFactory.Box(_end.TransformRead(gen));

            if (_isExclusive) {
                return Ast.Condition(
                    stateVariable,
                    Ast.Block(Ast.Assign(stateVariable, Methods.IsFalse.OpCall(end)), Ast.Constant(true)),
                    Ast.Assign(stateVariable, Methods.IsTrue.OpCall(begin))
                );  
            } else {
                return Ast.Condition(
                    Ast.OrElse(stateVariable, Methods.IsTrue.OpCall(begin)),
                    Ast.Block(Ast.Assign(stateVariable, Methods.IsFalse.OpCall(end)), Ast.Constant(true)),
                    Ast.Constant(false)
                );
                                  
            }
        }

        internal override Expression/*!*/ ToCondition() {
            int intBegin, intEnd;
            _isCondition = !IsIntegerRange(out intBegin, out intEnd);
            return this;
        }
    }
}
