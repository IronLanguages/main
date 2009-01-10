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
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;
using System.Reflection;

namespace Microsoft.Scripting.Actions.Calls {
    public sealed class SiteLocalStorageBuilder : ArgBuilder {
        public override int Priority {
            get { return -1; }
        }
                
        public SiteLocalStorageBuilder(ParameterInfo info) 
            : base(info) {
        }

        internal protected override Expression ToExpression(ParameterBinder parameterBinder, IList<Expression> parameters, bool[] hasBeenUsed) {
            return Expression.Constant(Activator.CreateInstance(ParameterInfo.ParameterType));
        }
    }
}
