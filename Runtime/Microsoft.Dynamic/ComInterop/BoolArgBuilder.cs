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

using System.Collections.Generic;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Scripting.ComInterop {
    internal sealed class BoolArgBuilder : SimpleArgBuilder {
        internal BoolArgBuilder(Type parameterType)
            : base(parameterType) {
            Debug.Assert(parameterType == typeof(bool));
        }

        internal override Expression MarshalToRef(Expression parameter) {
            // parameter  ? -1 : 0
            return Expression.Condition(
                Marshal(parameter),
                Expression.Constant((Int16)(-1)),
                Expression.Constant((Int16)0)
            );
        }

        internal override Expression UnmarshalFromRef(Expression value) {
            //parameter = temp != 0
            return base.UnmarshalFromRef(
                Expression.NotEqual(
                     value,
                     Expression.Constant((Int16)0)
                )
            );
        }
    }
}

#endif