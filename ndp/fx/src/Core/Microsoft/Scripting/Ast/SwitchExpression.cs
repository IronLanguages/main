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
using System.Dynamic.Utils;
using System.Reflection;

namespace System.Linq.Expressions {
    /// <summary>
    /// Represents a control expression that handles multiple selections by passing control to a <see cref="SwitchCase"/>.
    /// </summary>
    public sealed class SwitchExpression : Expression {
        private readonly Expression _switchValue;
        private readonly ReadOnlyCollection<SwitchCase> _cases;
        private readonly Expression _defaultBody;
        private readonly MethodInfo _comparison;

        internal SwitchExpression(Expression switchValue, Expression defaultBody, MethodInfo comparison, ReadOnlyCollection<SwitchCase> cases) {
            _switchValue = switchValue;
            _defaultBody = defaultBody;
            _comparison = comparison;
            _cases = cases;
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents.
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        protected override Type TypeImpl() {
            return _cases[0].Body.Type;
        }

        /// <summary>
        /// Returns the node type of this Expression. Extension nodes should return
        /// ExpressionType.Extension when overriding this method.
        /// </summary>
        /// <returns>The <see cref="ExpressionType"/> of the expression.</returns>
        protected override ExpressionType NodeTypeImpl() {
            return ExpressionType.Switch;
        }

        /// <summary>
        /// Gets the test for the switch.
        /// </summary>
        public Expression SwitchValue {
            get { return _switchValue; }
        }

        /// <summary>
        /// Gets the collection of <see cref="SwitchCase"/> objects for the switch.
        /// </summary>
        public ReadOnlyCollection<SwitchCase> Cases {
            get { return _cases; }
        }

        /// <summary>
        /// Gets the test for the switch.
        /// </summary>
        public Expression DefaultBody {
            get { return _defaultBody; }
        }

        /// <summary>
        /// Gets the equality comparison method, if any.
        /// </summary>
        public MethodInfo Comparison {
            get { return _comparison; }
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitSwitch(this);
        }
    }

    public partial class Expression {
        /// <summary>
        /// Creates a <see cref="SwitchExpression"/>.
        /// </summary>
        /// <param name="switchValue">The value to be tested against each case.</param>
        /// <param name="cases">The valid cases for this switch.</param>
        /// <returns>The created <see cref="SwitchExpression"/>.</returns>
        public static SwitchExpression Switch(Expression switchValue, params SwitchCase[] cases) {
            return Switch(switchValue, null, null, (IEnumerable<SwitchCase>)cases);
        }

        /// <summary>
        /// Creates a <see cref="SwitchExpression"/>.
        /// </summary>
        /// <param name="switchValue">The value to be tested against each case.</param>
        /// <param name="defaultBody">The result of the switch if no cases are matched.</param>
        /// <param name="cases">The valid cases for this switch.</param>
        /// <returns>The created <see cref="SwitchExpression"/>.</returns>
        public static SwitchExpression Switch(Expression switchValue, Expression defaultBody, params SwitchCase[] cases) {
            return Switch(switchValue, defaultBody, null, (IEnumerable<SwitchCase>)cases);
        }

        /// <summary>
        /// Creates a <see cref="SwitchExpression"/>.
        /// </summary>
        /// <param name="switchValue">The value to be tested against each case.</param>
        /// <param name="defaultBody">The result of the switch if no cases are matched.</param>
        /// <param name="comparison">The equality comparison method to use.</param>
        /// <param name="cases">The valid cases for this switch.</param>
        /// <returns>The created <see cref="SwitchExpression"/>.</returns>
        public static SwitchExpression Switch(Expression switchValue, Expression defaultBody, MethodInfo comparison, params SwitchCase[] cases) {
            return Switch(switchValue, defaultBody, comparison, (IEnumerable<SwitchCase>)cases);
        }

        /// <summary>
        /// Creates a <see cref="SwitchExpression"/>.
        /// </summary>
        /// <param name="switchValue">The value to be tested against each case.</param>
        /// <param name="defaultBody">The result of the switch if no cases are matched.</param>
        /// <param name="comparison">The equality comparison method to use.</param>
        /// <param name="cases">The valid cases for this switch.</param>
        /// <returns>The created <see cref="SwitchExpression"/>.</returns>
        public static SwitchExpression Switch(Expression switchValue, Expression defaultBody, MethodInfo comparison, IEnumerable<SwitchCase> cases) {
            RequiresCanRead(switchValue, "switchValue");
            ContractUtils.Requires(switchValue.Type != typeof(void), "switchValue", Strings.ArgumentCannotBeOfTypeVoid);

            var caseList = cases.ToReadOnly();
            ContractUtils.RequiresNotEmpty(caseList, "cases");
            ContractUtils.RequiresNotNullItems(caseList, "cases");

            Type switchType = caseList[0].Body.Type;
            Type testValueType = caseList[0].TestValues[0].Type;

            foreach (var c in caseList) {
                ContractUtils.RequiresNotNull(c, "cases");
                ContractUtils.Requires(switchType == c.Body.Type, "cases", Strings.AllCaseBodiesMustHaveSameType);
                ContractUtils.Requires(testValueType == c.TestValues[0].Type, "cases", Strings.AllTestValuesMustHaveSameType);                
            }

            if (defaultBody == null) {
                ContractUtils.Requires(switchType == typeof(void), "defaultBody", Strings.DefaultBodyMustBeSupplied);
            } else {
                ContractUtils.Requires(switchType == defaultBody.Type, "cases", Strings.AllCaseBodiesMustHaveSameType);
            }

            // Now we need to validate that switchValue.Type and testValueType
            // make sense in an Equal node. Fortunately, Equal throws a
            // reasonable error, so just call it.
            var equal = Equal(switchValue, caseList[0].TestValues[0], false, comparison);

            // Get the comparison function from equals node.
            comparison = equal.Method;

            // if we found a non-boolean userdefined equals, we don't want it.
            if (comparison != null && comparison.ReturnType != typeof(bool)) {
                throw Error.EqualityMustReturnBoolean(comparison);
            }

            return new SwitchExpression(switchValue, defaultBody, comparison, caseList);
        }
    }
}
