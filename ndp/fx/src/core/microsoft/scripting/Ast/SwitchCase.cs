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
    public sealed class SwitchCase {
        private readonly bool _default;
        private readonly int _value;
        private readonly Expression _body;

        internal SwitchCase(bool @default, int value, Expression body) {
            _default = @default;
            _value = value;
            _body = body;
        }

        public bool IsDefault {
            get { return _default; }
        }

        public int Value {
            get { return _value; }
        }

        public Expression Body {
            get { return _body; }
        }
    }

    public partial class Expression {
        public static SwitchCase DefaultCase(Expression body) {
            RequiresCanRead(body, "body");
            return new SwitchCase(true, 0, body);
        }

        public static SwitchCase SwitchCase(int value, Expression body) {
            RequiresCanRead(body, "body");
            return new SwitchCase(false, value, body);
        }
    }
}
