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
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Scripting.Debugging {
    using Ast = MSAst.Expression;

    internal static class InvokeTargets {
        internal static Type GetGeneratorFactoryTarget(Type[] parameterTypes) {
            Type[] typeArgs = new Type[parameterTypes.Length + 2];
            typeArgs[0] = typeof(DebugFrame);
            parameterTypes.CopyTo(typeArgs, 1);
            typeArgs[parameterTypes.Length + 1] = typeof(IEnumerator);

            if (typeArgs.Length <= 16) {
                return Ast.GetFuncType(typeArgs);
            } else {
                return DelegateHelpers.MakeNewCustomDelegateType(typeArgs);
            }
        }
    }
}
