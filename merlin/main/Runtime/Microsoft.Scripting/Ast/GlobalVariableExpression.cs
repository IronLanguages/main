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
using System.Diagnostics;
using System.Linq.Expressions;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// Represents a variable accessed from the host's scope
    /// Note: this node isn't reducible; it needs a tree rewrite to work
    /// See GlobalsRewriter
    /// </summary>
    public sealed class GlobalVariableExpression : Expression {
        private readonly string _name;
        private readonly bool _local;
        private readonly Type _type;

        internal GlobalVariableExpression(Type type, string name, bool local) {
            Debug.Assert(type != typeof(void));

            _name = name;
            _local = local;
            _type = type;
        }

        public override bool CanReduce {
            get { return false; }
        }

        protected override Type GetExpressionType() {
            return _type;
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Extension;
        }

        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// If using dynamic lookup, indicates that the variable should be
        /// looked up in the innermost Scope rather than the top level scope
        /// 
        /// TODO: Python specific, can it be removed?
        /// </summary>
        public bool IsLocal {
            get { return _local; }
        }

        // TODO: Remove? Useful for debugging
        public override string ToString() {
            return string.Format("Global {0} {1}", Type.Name, _name);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            return this;
        }
    }

    public partial class Utils {
        public static GlobalVariableExpression GlobalVariable(Type type, string name) {
            return GlobalVariable(type, name, false);
        }

        public static GlobalVariableExpression GlobalVariable(Type type, string name, bool local) {
            return new GlobalVariableExpression(type, name, local);
        }
    }
}
