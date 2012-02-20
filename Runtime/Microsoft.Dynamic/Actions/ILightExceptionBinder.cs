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

#if FEATURE_CORE_DLR
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Implemented by binders which support light exceptions.  Dynamic objects
    /// binding against a binder which implements this interface can check 
    /// SupportsLightThrow to see if the binder currently supports safely 
    /// returning a light exception.  Light exceptions can be created with
    /// LightException.Throw.
    ///
    /// Binders also need to implement GetlightBinder.  This method
    /// returns a new call site binder which may return light  exceptions if 
    /// the binder supports them.
    /// </summary>
    public interface ILightExceptionBinder {
        /// <summary>
        /// Returns true if a callsite binding against this binder can
        /// return light exceptions.
        /// </summary>
        bool SupportsLightThrow { get; }

        /// <summary>
        /// Gets a binder which will support light exception if one is
        /// available.
        /// </summary>
        CallSiteBinder GetLightExceptionBinder();
    }
}
