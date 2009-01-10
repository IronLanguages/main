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
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace System.Linq.Expressions.Compiler {
    /// <summary>
    /// Prepends an argument and stands in as an IArgumentProvider.  Avoids
    /// creation of a ReadOnlyCollection or making a temporary array copy.
    /// 
    /// Note this is always as better than allocating an array because an empty
    /// array has 16 bytes of overhead - and so does this.
    /// </summary>
    class ArgumentPrepender : IArgumentProvider {
        private IArgumentProvider _expression;
        private Expression _first;

        internal ArgumentPrepender(Expression first, IArgumentProvider provider) {
            _first = first;
            _expression = provider;
        }

        #region IArgumentProvider Members

        public Expression GetArgument(int index) {
            if (index == 0) {
                return _first;
            }

            return _expression.GetArgument(index - 1);
        }

        public int ArgumentCount {
            get { return _expression.ArgumentCount + 1; }
        }

        #endregion
    }
}
