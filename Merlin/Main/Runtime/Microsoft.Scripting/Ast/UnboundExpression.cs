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
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Ast {
    public class UnboundExpression : Expression {
        private readonly SymbolId _name;

        internal UnboundExpression(SymbolId name) {
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
                typeof(ScriptingRuntimeHelpers).GetMethod("LookupName"),
                Utils.CodeContext(),
                AstUtils.Constant(_name)
            );
        }

        protected override Expression VisitChildren(Func<Expression, Expression> visitor) {
            return this;
        }
    }

    /// <summary>
    /// Factory methods
    /// </summary>
    public static partial class Utils {
        public static UnboundExpression Read(SymbolId name) {
            ContractUtils.Requires(!name.IsInvalid && !name.IsEmpty, "name", "Invalid or empty name is not allowed");
            return new UnboundExpression(name);
        }
    }
}
