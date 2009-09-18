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

#if !SILVERLIGHT
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Diagnostics;

namespace Microsoft.Scripting.ComInterop {
    /// <summary>
    /// SimpleArgBuilder produces the value produced by the user as the argument value.  It
    /// also tracks information about the original parameter and is used to create extended
    /// methods for params arrays and param dictionary functions.
    /// </summary>
    internal class SimpleArgBuilder : ArgBuilder {
        private readonly Type _parameterType;

        internal SimpleArgBuilder(Type parameterType) {
            _parameterType = parameterType;
        }

        internal Type ParameterType {
            get { return _parameterType; }
        }

        internal override Expression Marshal(Expression parameter) {
            Debug.Assert(parameter != null);
            return Helpers.Convert(parameter, _parameterType);
        }

        internal override Expression UnmarshalFromRef(Expression newValue) {
            Debug.Assert(newValue != null && newValue.Type.IsAssignableFrom(_parameterType));

            return base.UnmarshalFromRef(newValue);
        }
    }
}

#endif
