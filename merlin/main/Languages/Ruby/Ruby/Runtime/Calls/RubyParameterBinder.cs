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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using IronRuby.Builtins;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Calls {
    internal sealed class RubyParameterBinder : ParameterBinder {
        private Expression _scopeExpression;
        private Expression _contextExpression;

        public Expression/*!*/ ScopeExpression {
            get { return _scopeExpression ?? (_scopeExpression = Methods.GetEmptyScope.OpCall(_contextExpression)); }
        }

        public Expression/*!*/ ContextExpression {
            get { return _contextExpression ?? (_contextExpression = Methods.GetContextFromScope.OpCall(_scopeExpression)); }
        }

        public RubyParameterBinder(ActionBinder/*!*/ binder, Expression/*!*/ scopeOrContextExpression, bool isScope)
            : base(binder) {
            Assert.NotNull(binder, scopeOrContextExpression);

            if (isScope) {
                _scopeExpression = AstUtils.Convert(scopeOrContextExpression, typeof(RubyScope));
            } else {
                _contextExpression = AstUtils.Convert(scopeOrContextExpression, typeof(RubyContext));
            }
        }

        public override Expression/*!*/ ConvertExpression(Expression/*!*/ expr, ParameterInfo info, Type/*!*/ toType) {
            Type fromType = expr.Type;

            // block:
            if (fromType == typeof(MissingBlockParam)) {
                Debug.Assert(toType == typeof(BlockParam) || toType == typeof(MissingBlockParam));
                return Ast.Constant(null);
            }

            if (fromType == typeof(BlockParam) && toType == typeof(MissingBlockParam)) {
                return Ast.Constant(null);
            }

            // protocol conversions:
            if (info != null && info.IsDefined(typeof(DefaultProtocolAttribute), false)) {
                var action = ProtocolConversionAction.TryGetConversionAction(toType);
                if (action != null) {
                    // TODO: once we work with MetaObjects, we could inline these dynamic sites:
                    return Ast.Dynamic(action, toType, ScopeExpression, expr);
                }

                throw new InvalidOperationException(String.Format("No default protocol conversion for type {0}.", toType));
            }

            return Binder.ConvertExpression(expr, toType, ConversionResultKind.ExplicitCast, ScopeExpression);
        }

        public override Expression/*!*/ GetDynamicConversion(Expression/*!*/ value, Type/*!*/ type) {
            return Expression.Dynamic(OldConvertToAction.Make(Binder, type), type, ScopeExpression, value);
        }
    }
}
