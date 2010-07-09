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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.ComInterop {

    internal class ConversionArgBuilder : ArgBuilder {
        private SimpleArgBuilder _innerBuilder;
        private Type _parameterType;

        internal ConversionArgBuilder(Type parameterType, SimpleArgBuilder innerBuilder) {
            _parameterType = parameterType;
            _innerBuilder = innerBuilder;
        }

        internal override Expression Marshal(Expression parameter) {
            return _innerBuilder.Marshal(Helpers.Convert(parameter, _parameterType));
        }

        internal override Expression MarshalToRef(Expression parameter) {
            //we are not supporting conversion InOut
            throw Assert.Unreachable;
        }
    }
}

#endif
