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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System.Reflection;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public static partial class Utils {
        public static NewExpression SimpleNewHelper(ConstructorInfo constructor, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(constructor, "constructor");
            ContractUtils.RequiresNotNullItems(arguments, "arguments");

            ParameterInfo[] parameters = constructor.GetParameters();
            ContractUtils.Requires(arguments.Length == parameters.Length, "arguments", "Incorrect number of arguments");

            return Expression.New(constructor, ArgumentConvertHelper(arguments, parameters));
        }
    }
}
