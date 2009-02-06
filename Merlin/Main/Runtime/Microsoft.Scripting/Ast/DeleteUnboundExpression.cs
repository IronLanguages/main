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

using System.Linq.Expressions;
using System.Dynamic;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using System;

namespace Microsoft.Scripting.Ast {
    public class DeleteUnboundExpression : Expression {
        private readonly SymbolId _name;

        internal DeleteUnboundExpression(SymbolId name) {
            _name = name;
        }

        public override bool CanReduce {
            get { return true; }
        }

        protected override Type TypeImpl() {
            return typeof(object);
        }

        protected override ExpressionType NodeTypeImpl() {
            return ExpressionType.Extension;
        }

        public SymbolId Name {
            get { return _name; }
        }

        public override Expression Reduce() {
            return Expression.Call(
                typeof(ExpressionHelpers).GetMethod("RemoveName"),
                new [] {
                    Utils.CodeContext(),
                    AstUtils.Constant(_name)
                }
            );
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            return this;
        }
    }

    public static partial class Utils {
        public static DeleteUnboundExpression Delete(SymbolId name) {
            ContractUtils.Requires(!name.IsInvalid && !name.IsEmpty, "name");
            return new DeleteUnboundExpression(name);
        }

        [Obsolete("use Delete overload without SourceSpan")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "span")]
        public static DeleteUnboundExpression Delete(SymbolId name, SourceSpan span) {
            ContractUtils.Requires(!name.IsInvalid && !name.IsEmpty, "name");
            return new DeleteUnboundExpression(name);
        }
    }

    public static partial class ExpressionHelpers {
        /// <summary>
        /// Called from generated code, helper to remove a name
        /// </summary>
        public static object RemoveName(CodeContext context, SymbolId name) {
            return context.LanguageContext.RemoveName(context.Scope, name);
        }
    }
}
