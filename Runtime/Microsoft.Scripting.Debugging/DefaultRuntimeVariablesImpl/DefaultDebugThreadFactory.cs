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

using System.Collections.Generic;

namespace Microsoft.Scripting.Debugging {
    using Ast = MSAst.Expression;

    /// <summary>
    /// Default implementation of IDebugThreadFactory, which uses DLR's RuntimeVariablesExpression for lifting locals.
    /// </summary>
    internal sealed class DefaultDebugThreadFactory : IDebugThreadFactory {
        public DebugThread CreateDebugThread(Microsoft.Scripting.Debugging.CompilerServices.DebugContext debugContext) {
            return new DefaultDebugThread(debugContext);
        }

        public MSAst.Expression CreatePushFrameExpression(MSAst.ParameterExpression functionInfo, MSAst.ParameterExpression debugMarker, IList<MSAst.ParameterExpression> locals, IList<VariableInfo> varInfos, MSAst.Expression runtimeThread) {
            MSAst.ParameterExpression[] args = new MSAst.ParameterExpression[2 + locals.Count];
            args[0] = functionInfo;
            args[1] = debugMarker;
            for (int i = 0; i < locals.Count; i++) {
                args[i + 2] = locals[i];
            }

            return Ast.Call(
                typeof(RuntimeOps).GetMethod("LiftVariables"),
                runtimeThread,
                Ast.RuntimeVariables(args)
            );
        }
    }
}
