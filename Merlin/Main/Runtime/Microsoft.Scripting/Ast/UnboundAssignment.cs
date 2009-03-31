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
using System.Dynamic;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Ast {
    public class UnboundAssignment : Expression {
        private readonly SymbolId _name;
        private readonly Expression _value;

        internal UnboundAssignment(SymbolId name, Expression value) {
            _name = name;
            _value = value;
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

        public Expression Value {
            get { return _value; }
        }

        public override Expression Reduce() {
            return Expression.Call(
                null,
                typeof(ScriptingRuntimeHelpers).GetMethod("SetName"),
                new Expression[] {
                    Utils.CodeContext(), 
                    AstUtils.Constant(_name),
                    AstUtils.Convert(_value, typeof(object))
                }
            );
        }

        protected override Expression VisitChildren(Func<Expression, Expression> visitor) {
            Expression v = visitor(_value);
            if (v == _value) {
                return this;
            }
            return Utils.Assign(_name, v);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Utils {
        [Obsolete("use Assign overload without SourceSpan")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "span")]
        public static UnboundAssignment Assign(SymbolId name, Expression value, SourceSpan span) {
            return Assign(name, value);
        }
        public static UnboundAssignment Assign(SymbolId name, Expression value) {
            ContractUtils.Requires(!name.IsEmpty && !name.IsInvalid, "name", "Invalid or empty name is not allowed");
            ContractUtils.RequiresNotNull(value, "value");
            return new UnboundAssignment(name, value);
        }
    }
}
