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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {

    public sealed class IfStatementTest {
        private readonly Expression _test;
        private readonly Expression _body;

        internal IfStatementTest(Expression test, Expression body) {
            _test = test;
            _body = body;
        }

        public Expression Test {
            get { return _test; }
        }

        public Expression Body {
            get { return _body; }
        }
    }

    public partial class Utils {
        public static IfStatementTest IfCondition(Expression test, Expression body) {
            ContractUtils.RequiresNotNull(test, "test");
            ContractUtils.RequiresNotNull(body, "body");
            ContractUtils.Requires(test.Type == typeof(bool), "test", "Test must be boolean");

            return new IfStatementTest(test, body);
        }
    }
}
