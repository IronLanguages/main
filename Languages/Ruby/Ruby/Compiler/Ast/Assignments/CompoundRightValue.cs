/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {

    public class CompoundRightValue {
        private readonly Expression/*!*/[]/*!*/ _rightValues;
        private readonly Expression _splattedValue;

        public Expression/*!*/[]/*!*/ RightValues {
            get { return _rightValues; }
        }

        public Expression SplattedValue {
            get { return _splattedValue; }
        }

        public CompoundRightValue(Expression/*!*/[]/*!*/ rightValues, Expression splattedValue) {
            Assert.NotNull(rightValues);

            _rightValues = rightValues;
            _splattedValue = splattedValue;
        }
    }
}
