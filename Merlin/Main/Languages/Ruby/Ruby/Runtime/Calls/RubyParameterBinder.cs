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
        private readonly CallArguments/*!*/ _args;
        
        public RubyContext/*!*/ Context {
            get { return _args.RubyContext; }
        }

        public Expression/*!*/ ScopeExpression {
            get { return _args.MetaScope.Expression; }
        }

        public Expression/*!*/ ContextExpression {
            get { return _args.MetaContext.Expression; }
        }

        public RubyParameterBinder(CallArguments/*!*/ args)
            : base(args.RubyContext.Binder) {
            _args = args;
        }

        public override Expression/*!*/ ConvertExpression(Expression/*!*/ expr, ParameterInfo info, Type/*!*/ toType) {
            Type fromType = expr.Type;

            // block:
            if (fromType == typeof(MissingBlockParam)) {
                Debug.Assert(toType == typeof(BlockParam) || toType == typeof(MissingBlockParam));
                return AstUtils.Constant(null);
            }

            if (fromType == typeof(BlockParam) && toType == typeof(MissingBlockParam)) {
                return AstUtils.Constant(null);
            }

            // protocol conversions:
            if (info != null && info.IsDefined(typeof(DefaultProtocolAttribute), false)) {
                var action = RubyConversionAction.TryGetDefaultConversionAction(Context, toType);
                if (action != null) {
                    // TODO: once we work with MetaObjects, we could inline these dynamic sites:
                    return Ast.Dynamic(action, toType, expr);
                }

                throw new InvalidOperationException(String.Format("No default protocol conversion for type {0}.", toType));
            }

            return Binder.ConvertExpression(expr, toType, ConversionResultKind.ExplicitCast, null);
        }
    }
}
