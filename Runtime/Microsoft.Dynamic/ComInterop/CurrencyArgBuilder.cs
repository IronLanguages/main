/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/


#if !SILVERLIGHT // ComObject
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.Scripting.ComInterop {
    internal sealed class CurrencyArgBuilder : SimpleArgBuilder {
        internal CurrencyArgBuilder(Type parameterType)
            : base(parameterType) {
            Debug.Assert(parameterType == typeof(CurrencyWrapper));
        }

        internal override Expression Marshal(Expression parameter) {
            // parameter.WrappedObject
            return Expression.Property(
                Helpers.Convert(base.Marshal(parameter), typeof(CurrencyWrapper)),
                "WrappedObject"
            );
        }

        internal override Expression MarshalToRef(Expression parameter) {
            // Decimal.ToOACurrency(parameter.WrappedObject)
            return Expression.Call(
                typeof(Decimal).GetMethod("ToOACurrency"),
                Marshal(parameter)
            );
        }

        internal override Expression UnmarshalFromRef(Expression value) {
            // Decimal.FromOACurrency(value)
            return base.UnmarshalFromRef(
                Expression.New(
                    typeof(CurrencyWrapper).GetConstructor(new Type[] { typeof(Decimal) }),
                    Expression.Call(
                        typeof(Decimal).GetMethod("FromOACurrency"),
                        value
                    )
                )
            );
        }
    }
}

#endif