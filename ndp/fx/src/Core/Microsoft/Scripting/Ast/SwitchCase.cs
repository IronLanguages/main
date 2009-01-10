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

using System.Dynamic.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// Represents one case of a <see cref="SwitchExpression"/>.
    /// </summary>
    public sealed class SwitchCase {
        private readonly bool _default;
        private readonly int _value;
        private readonly Expression _body;

        internal SwitchCase(bool @default, int value, Expression body) {
            _default = @default;
            _value = value;
            _body = body;
        }

        /// <summary>
        /// True if this is the default case.
        /// </summary>
        public bool IsDefault {
            get { return _default; }
        }

        /// <summary>
        /// Gets the value of this case.  This case is selected for execution when the <see cref="SwitchExpression.Test"/> matches this value.
        /// </summary>
        public int Value {
            get { return _value; }
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
        /// Creates a default <see cref="SwitchCase"/>.
        /// </summary>
        /// <param name="body">The body of the case.</param>
        /// <returns>The created <see cref="SwitchCase"/>.</returns>
        public static SwitchCase DefaultCase(Expression body) {
            RequiresCanRead(body, "body");
            return new SwitchCase(true, 0, body);
        }

        /// <summary>
        /// Creates a non-default <see cref="SwitchCase"/>.
        /// </summary>
        /// <param name="value">The test value of the case.</param>
        /// <param name="body">The body of the case.</param>
        /// <returns>The created <see cref="SwitchCase"/>.</returns>
        public static SwitchCase SwitchCase(int value, Expression body) {
            RequiresCanRead(body, "body");
            return new SwitchCase(false, value, body);
        }
    }
}
