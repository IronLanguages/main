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
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    public class RestrictionInfo {
        public readonly DynamicMetaObject[] Objects;
        public readonly Type[] Types;

        public RestrictionInfo(DynamicMetaObject[] objects, Type[] types) {
            Assert.NotNullItems(objects);
            Assert.NotNull(types);

            Objects = objects;
            Types = types;
        }
    }
}
