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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Dynamic.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// An expression that provides runtime read/write access to variables.
    /// Needed to implement "eval" in dynamic languages.
    /// Evaluates to an instance of ILocalVariables at run time.
    /// </summary>
    public sealed class RuntimeVariablesExpression : Expression {
        private readonly ReadOnlyCollection<ParameterExpression> _variables;

        internal RuntimeVariablesExpression(ReadOnlyCollection<ParameterExpression> variables) {
            _variables = variables;
        }

        protected override Type GetExpressionType() {
            return typeof(IList<IStrongBox>);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.RuntimeVariables;
        }

        /// <summary>
        /// The variables or parameters to provide access to
        /// </summary>
        public ReadOnlyCollection<ParameterExpression> Variables {
            get { return _variables; }
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitRuntimeVariables(this);
        }
    }

    public partial class Expression {

        public static RuntimeVariablesExpression RuntimeVariables(params ParameterExpression[] variables) {
            return RuntimeVariables((IEnumerable<ParameterExpression>)variables);
        }
        public static RuntimeVariablesExpression RuntimeVariables(IEnumerable<ParameterExpression> variables) {
            ContractUtils.RequiresNotNull(variables, "variables");

            var vars = variables.ToReadOnly();
            for (int i = 0; i < vars.Count; i++) {
                Expression v = vars[i];
                if (v == null) {
                    throw new ArgumentNullException("variables[" + i + "]");
                }
            }

            return new RuntimeVariablesExpression(vars);
        }
    }
}
