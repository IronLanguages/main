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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions.Calls {

    /// <summary>
    /// ArgBuilder which provides a default parameter value for a method call.
    /// </summary>
    internal sealed class DefaultArgBuilder : ArgBuilder {
        public DefaultArgBuilder(ParameterInfo info) 
            : base(info) {
            Assert.NotNull(info);
        }

        public override int Priority {
            get { return 2; }
        }

        public override int ConsumedArgumentCount {
            get { return 0; }
        }

        internal protected override Expression ToExpression(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed) {
            object value = ParameterInfo.DefaultValue;
            if (value is Missing) {
                value = CompilerHelpers.GetMissingValue(ParameterInfo.ParameterType);
            }

            if (ParameterInfo.ParameterType.IsByRef) {
                return AstUtils.Constant(value, ParameterInfo.ParameterType.GetElementType());
            }

            var metaValue = new DynamicMetaObject(AstUtils.Constant(value), BindingRestrictions.Empty, value);
            return resolver.Convert(metaValue, CompilerHelpers.GetType(value), ParameterInfo, ParameterInfo.ParameterType);
        }

        protected internal override Func<object[], object> ToDelegate(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed) {
            if (ParameterInfo.ParameterType.IsByRef) {
                return null;
            } else if (ParameterInfo.DefaultValue is Missing && CompilerHelpers.GetMissingValue(ParameterInfo.ParameterType) is Missing) {
                // reflection throws when we do this
                return null;
            }
            
            object val = ParameterInfo.DefaultValue;
            if (val is Missing) {
                val = CompilerHelpers.GetMissingValue(ParameterInfo.ParameterType);
            }
            Debug.Assert(val != Missing.Value);
            return (_) => val;
        }
    }
}
