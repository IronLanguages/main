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

#if !SILVERLIGHT

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;

namespace Microsoft.Scripting.ComInterop {
    internal class ConvertArgBuilder : SimpleArgBuilder {
        private readonly Type _marshalType;

        internal ConvertArgBuilder(Type parameterType, Type marshalType)
            : base(parameterType) {
            _marshalType = marshalType;
        }

        internal override Expression Marshal(Expression parameter) {
            parameter = base.Marshal(parameter);
            return Expression.Convert(parameter, _marshalType);
        }

        internal override Expression UnmarshalFromRef(Expression newValue) {
            return base.UnmarshalFromRef(Expression.Convert(newValue, ParameterType));
        }
    }
}

#endif
