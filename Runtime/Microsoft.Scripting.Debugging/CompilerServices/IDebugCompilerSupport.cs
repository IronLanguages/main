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

namespace Microsoft.Scripting.Debugging.CompilerServices {
    /// <summary>
    /// Implemented by compilers to allow the traceback engine to get additional information.
    /// </summary>
    public interface IDebugCompilerSupport {
        bool DoesExpressionNeedReduction(MSAst.Expression expression);
        MSAst.Expression QueueExpressionForReduction(MSAst.Expression expression);
        bool IsCallToDebuggableLambda(MSAst.Expression expression);
    }
}
