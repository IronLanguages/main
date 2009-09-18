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
using System.Dynamic;
using System.Reflection;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    using Ast = Expression;

    /// <summary>
    /// Builds the argument for an out argument when not passed a StrongBox.  The out parameter
    /// is returned as an additional return value.
    /// </summary>
    internal sealed class OutArgBuilder : ArgBuilder {
        private readonly Type _parameterType;
        private readonly bool _isRef;
        private ParameterExpression _tmp;

        public OutArgBuilder(ParameterInfo info) 
            : base(info) {

            _parameterType = info.ParameterType.IsByRef ? info.ParameterType.GetElementType() : info.ParameterType;
            _isRef = info.ParameterType.IsByRef;
        }

        public override int ConsumedArgumentCount {
            get { return 0; }
        }

        public override int Priority {
            get { return 5; }
        }

        internal protected override Expression ToExpression(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed) {
            if (_isRef) {
                if (_tmp == null) {
                    _tmp = resolver.GetTemporary(_parameterType, "outParam");
                }
                return _tmp;
            }

            return GetDefaultValue();
        }

        internal override Expression ToReturnExpression(OverloadResolver resolver) {
            if (_isRef) {
                return _tmp;
            }

            return GetDefaultValue();
        }

        internal override Expression ByRefArgument {
            get { return _isRef ? _tmp : null; }
        }

        private Expression GetDefaultValue() {
            if (_parameterType.IsValueType) {
                // default(T)                
                return AstUtils.Constant(Activator.CreateInstance(_parameterType));
            }
            return AstUtils.Constant(null);
        }
    }
}
