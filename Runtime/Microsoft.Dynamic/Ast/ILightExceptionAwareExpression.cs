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

using System;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Implemented by expressions which can provide a version which is aware of light exceptions.  
    /// 
    /// Normally these expressions will simply reduce to a version which throws a real exception.
    /// When the expression is used inside of a region of code which supports light exceptions
    /// the light exception re-writer will call ReduceForLightExceptions.  The expression can
    /// then return a new expression which can return a light exception rather than throwing
    /// a real .NET exception.
    /// </summary>
    public interface ILightExceptionAwareExpression {
        Expression ReduceForLightExceptions();
    }
}
