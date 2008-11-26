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
using System.Text;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Scripting.Actions.Calls {

    // TODO: this should move to Python
    public class ParameterBinderWithCodeContext : ParameterBinder {
        private readonly Expression _context;

        public Expression ContextExpression {
            get { return _context; }
        }

        public ParameterBinderWithCodeContext(ActionBinder actionBinder, Expression codeContext)
            : base(actionBinder) {
            Assert.NotNull(actionBinder, codeContext);

            _context = codeContext;
        }

        public override Expression ConvertExpression(Expression expr, ParameterInfo info, Type toType) {
            return Binder.ConvertExpression(expr, toType, ConversionResultKind.ExplicitCast, _context);
        }

        public override Expression GetDynamicConversion(Expression value, Type type) {
            return Expression.Dynamic(OldConvertToAction.Make(Binder, type), type, _context, value);
        }
    }
}
