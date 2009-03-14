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
using System.Diagnostics;
using System.Dynamic.Utils;
using System.Reflection;

namespace System.Linq.Expressions {
    /// <summary>
    /// Represents a control expression that handles multiple selections by passing control to a <see cref="SwitchCase"/>.
    /// </summary>
#if !SILVERLIGHT
    [DebuggerTypeProxy(typeof(Expression.SwitchExpressionProxy))]
#endif
    public sealed class SwitchExpression : Expression {
        private readonly Type _type;
        private readonly Expression _switchValue;
        private readonly ReadOnlyCollection<SwitchCase> _cases;
        private readonly Expression _defaultBody;
        private readonly MethodInfo _comparison;

        internal SwitchExpression(Type type, Expression switchValue, Expression defaultBody, MethodInfo comparison, ReadOnlyCollection<SwitchCase> cases) {
            _type = type;
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
            return _type;
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

        internal bool IsLifted {
            get {
                if (_switchValue.Type.IsNullableType()) {
                    return (_comparison == null) ||
                        _switchValue.Type != _comparison.GetParametersCached()[0].ParameterType.GetNonRefType();
                }
                return false;
            }
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
        /// <param name="type">The result type of the switch.</param>
        /// <param name="switchValue">The value to be tested against each case.</param>
        /// <param name="defaultBody">The result of the switch if no cases are matched.</param>
        /// <param name="comparison">The equality comparison method to use.</param>
        /// <param name="cases">The valid cases for this switch.</param>
        /// <returns>The created <see cref="SwitchExpression"/>.</returns>
        public static SwitchExpression Switch(Type type, Expression switchValue, Expression defaultBody, MethodInfo comparison, params SwitchCase[] cases) {
            return Switch(type, switchValue, defaultBody, comparison, (IEnumerable<SwitchCase>)cases);
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
            return Switch(null, switchValue, defaultBody, comparison, cases);
        }

        /// <summary>
        /// Creates a <see cref="SwitchExpression"/>.
        /// </summary>
        /// <param name="type">The result type of the switch.</param>
        /// <param name="switchValue">The value to be tested against each case.</param>
        /// <param name="defaultBody">The result of the switch if no cases are matched.</param>
        /// <param name="comparison">The equality comparison method to use.</param>
        /// <param name="cases">The valid cases for this switch.</param>
        /// <returns>The created <see cref="SwitchExpression"/>.</returns>
        public static SwitchExpression Switch(Type type, Expression switchValue, Expression defaultBody, MethodInfo comparison, IEnumerable<SwitchCase> cases) {
            RequiresCanRead(switchValue, "switchValue");
            ContractUtils.Requires(switchValue.Type != typeof(void), "switchValue", Strings.ArgumentCannotBeOfTypeVoid);

            var caseList = cases.ToReadOnly();
            ContractUtils.RequiresNotEmpty(caseList, "cases");
            ContractUtils.RequiresNotNullItems(caseList, "cases");

            Type switchType = caseList[0].Body.Type;

            if (comparison != null) {
                var pms = comparison.GetParametersCached();
                if (pms.Length != 2) {
                    throw Error.IncorrectNumberOfMethodCallArguments(comparison);
                }
                // Validate that the switch value's type matches the comparison method's 
                // left hand side parameter type.
                var leftParam = pms[0];
                bool liftedCall = false;
                if (!ParameterIsAssignable(leftParam, switchValue.Type)) {
                    liftedCall = ParameterIsAssignable(leftParam, switchValue.Type.GetNonNullableType());
                    if (!liftedCall) {
                        throw Error.SwitchValueTypeDoesNotMatchComparisonMethodParameter(switchValue.Type, leftParam.ParameterType);
                    }
                }

                var rightParam = pms[1];
                foreach (var c in caseList) {
                    ContractUtils.RequiresNotNull(c, "cases");
                    ValidateSwitchCaseType(c, type, switchType, "cases");
                    for (int i = 0; i < c.TestValues.Count; i++) {
                        // When a comparison method is provided, test values can have different type but have to
                        // be reference assignable to the right hand side parameter of the method.
                        Type rightOperandType = c.TestValues[i].Type;
                        if (liftedCall) {
                            if (!rightOperandType.IsNullableType()) {
                                throw Error.TestValueTypeDoesNotMatchComparisonMethodParameter(c.TestValues[i].Type, rightParam.ParameterType);
                            }
                            rightOperandType = rightOperandType.GetNonNullableType();
                        }
                        if (!ParameterIsAssignable(rightParam, rightOperandType)) {
                            throw Error.TestValueTypeDoesNotMatchComparisonMethodParameter(c.TestValues[i].Type, rightParam.ParameterType);
                        }
                    }
                }
            } else {
                // When comparison method is not present, all the test values must have
                // the same type. Use the first test value's type as the baseline.
                var firstTestValue = caseList[0].TestValues[0];
                foreach (var c in caseList) {
                    ContractUtils.RequiresNotNull(c, "cases");
                    ValidateSwitchCaseType(c, type, switchType, "cases");
                    // When no comparison method is provided, require all test values to have the same type.
                    for (int i = 0; i < c.TestValues.Count; i++) {
                        if (firstTestValue.Type != c.TestValues[i].Type) {
                            throw new ArgumentException(Strings.AllTestValuesMustHaveSameType, "cases");
                        }
                    }
                }

                // Now we need to validate that switchValue.Type and testValueType
                // make sense in an Equal node. Fortunately, Equal throws a
                // reasonable error, so just call it.
                var equal = Equal(switchValue, firstTestValue, false, comparison);

                // Get the comparison function from equals node.
                comparison = equal.Method;
            }

            if (defaultBody == null) {
                if (type != null) {
                    ContractUtils.Requires(type == typeof(void), "defaultBody", Strings.DefaultBodyMustBeSupplied);
                } else {
                    ContractUtils.Requires(switchType == typeof(void), "defaultBody", Strings.DefaultBodyMustBeSupplied);
                }
            } else {
                if (type != null) {
                    ContractUtils.Requires(TypeUtils.AreReferenceAssignable(type, defaultBody.Type), "defaultBody", Strings.ArgumentTypesMustMatch);
                } else {
                    ContractUtils.Requires(switchType == defaultBody.Type, "cases", Strings.AllCaseBodiesMustHaveSameType);
                }
            }

            // if we have a non-boolean userdefined equals, we don't want it.
            if (comparison != null && comparison.ReturnType != typeof(bool)) {
                throw Error.EqualityMustReturnBoolean(comparison);
            }

            return new SwitchExpression(type ?? switchType, switchValue, defaultBody, comparison, caseList);
        }


        /// <summary>
        /// When nodeType is not null, validate that the type of the switch case's body is reference assignable to
        /// the nodeType. Otherwise, validate the type of the switch case's body is the same as switchType.
        /// </summary>
        private static void ValidateSwitchCaseType(SwitchCase switchCase, Type nodeType, Type switchType, string casesParamName) {
            if (nodeType != null) {
                if (!TypeUtils.AreReferenceAssignable(nodeType, switchCase.Body.Type)) {
                    throw new ArgumentException(Strings.ArgumentTypesMustMatch, casesParamName);
                }
            } else {
                if (switchType != switchCase.Body.Type) {
                    throw new ArgumentException(Strings.AllCaseBodiesMustHaveSameType, casesParamName);
                }
            }
        }
    }
}
