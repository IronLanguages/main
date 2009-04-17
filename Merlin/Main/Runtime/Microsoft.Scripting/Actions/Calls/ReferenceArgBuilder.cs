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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using RuntimeHelpers = Microsoft.Scripting.Runtime.ScriptingRuntimeHelpers;

namespace Microsoft.Scripting.Actions.Calls {

    /// <summary>
    /// An argument that the user wants to explicitly pass by-reference (with copy-in copy-out semantics).
    /// The user passes a StrongBox[T] object whose value will get updated when the call returns.
    /// </summary>
    internal sealed class ReferenceArgBuilder : SimpleArgBuilder {
        private readonly Type _elementType;
        private ParameterExpression _tmp;

        public ReferenceArgBuilder(ParameterInfo info, Type parameterType, int index)
            : base(info, parameterType, index, false, false) {
            Debug.Assert(parameterType.GetGenericTypeDefinition() == typeof(StrongBox<>));
            _elementType = parameterType.GetGenericArguments()[0];
        }

        protected override SimpleArgBuilder Copy(int newIndex) {
            return new ReferenceArgBuilder(ParameterInfo, Type, newIndex);
        }

        public override int Priority {
            get { return 5; }
        }

        internal protected override Expression ToExpression(OverloadResolver resolver, IList<Expression> parameters, bool[] hasBeenUsed) {
            if (_tmp == null) {
                _tmp = resolver.GetTemporary(_elementType, "outParam");
            }

            Debug.Assert(!hasBeenUsed[Index]);
            hasBeenUsed[Index] = true;
            Type boxType = typeof(StrongBox<>).MakeGenericType(_elementType);
            return Expression.Condition(
                Expression.TypeIs(parameters[Index], Type),
                Expression.Assign(
                    _tmp,
                    Expression.Field(
                        AstUtils.Convert(parameters[Index], boxType),
                        boxType.GetField("Value")
                    )
                ),
                Expression.Call(
                    typeof(RuntimeHelpers).GetMethod("IncorrectBoxType").MakeGenericMethod(_elementType),
                    AstUtils.Convert(parameters[Index], typeof(object))
                )
            );
        }

        protected internal override Func<object[], object> ToDelegate(OverloadResolver resolver, IList<System.Dynamic.DynamicMetaObject> knownTypes, bool[] hasBeenUsed) {
            return null;
        }

        internal override Expression UpdateFromReturn(OverloadResolver resolver, IList<Expression> parameters) {
            return Expression.Assign(
                Expression.Field(
                    Expression.Convert(parameters[Index], Type),
                    Type.GetField("Value")
                ),
                _tmp
            );
        }

        internal override Expression ByRefArgument {
            get { return _tmp; }
        }
    }
}
