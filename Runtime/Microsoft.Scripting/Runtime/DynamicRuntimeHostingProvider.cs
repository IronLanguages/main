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

using System;
using System.IO;
using System.Text;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// DLR requires any Hosting API provider to implement this class and provide its instance upon Runtime initialization.
    /// DLR calls on it to perform basic host/system dependent operations.
    /// </summary>
    [Serializable]
    public abstract class DynamicRuntimeHostingProvider {
        /// <summary>
        /// Abstracts system operations that are used by DLR and could potentially be platform specific.
        /// </summary>
        public abstract PlatformAdaptationLayer PlatformAdaptationLayer { get; }
    }
}
