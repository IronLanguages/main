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

namespace System.Linq.Expressions {
    /// <summary>
    /// Represents one case of a <see cref="SwitchExpression"/>.
    /// </summary>
    public sealed class SwitchCase {
        private readonly ReadOnlyCollection<Expression> _testValues;
        private readonly Expression _body;

        internal SwitchCase(Expression body, ReadOnlyCollection<Expression> testValues) {
            _body = body;
            _testValues = testValues;
        }

        /// <summary>
        /// Gets the values of this case. This case is selected for execution when the <see cref="SwitchExpression.SwitchValue"/> matches any of these values.
        /// </summary>
        public ReadOnlyCollection<Expression> TestValues {
            get { return _testValues; }
        }

        /// <summary>
        /// Gets the body of this case.
        /// </summary>
        public Expression Body {
            get { return _body; }
        }
    }

    public partial class Expression {
        /// <summary>
        /// Creates a <see cref="System.Linq.Expressions.SwitchCase">SwitchCase</see> for use in a <see cref="SwitchExpression"/>.
        /// </summary>
        /// <param name="body">The body of the case.</param>
        /// <param name="testValues">The test values of the case.</param>
        /// <returns>The created <see cref="System.Linq.Expressions.SwitchCase">SwitchCase</see>.</returns>
        public static SwitchCase SwitchCase(Expression body, params Expression[] testValues) {
            return SwitchCase(body, (IEnumerable<Expression>)testValues);
        }

        /// <summary>
        /// Creates a <see cref="System.Linq.Expressions.SwitchCase">SwitchCase</see> for use in a <see cref="SwitchExpression"/>.
        /// </summary>
        /// <param name="body">The body of the case.</param>
        /// <param name="testValues">The test values of the case.</param>
        /// <returns>The created <see cref="System.Linq.Expressions.SwitchCase">SwitchCase</see>.</returns>
        public static SwitchCase SwitchCase(Expression body, IEnumerable<Expression> testValues) {
            RequiresCanRead(body, "body");
            
            var values = testValues.ToReadOnly();
            RequiresCanRead(values, "testValues");
            ContractUtils.RequiresNotEmpty(values, "testValues");
            if (values.Count > 1) {
                // All test values must have the same type.
                var type = values[0].Type;
                for (int i = 1, n = values.Count; i < n; i++) {
                    ContractUtils.Requires(type == values[i].Type, "testValues", Strings.AllTestValuesMustHaveSameType);
                }
            }

            return new SwitchCase(body, values);
        }
    }
}
