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

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// A useful interface for taking slices of numeric arrays, inspired by Python's Slice objects.
    /// </summary>
    public interface ISlice {
        /// <summary>
        /// The starting index of the slice or null if no first index defined
        /// </summary>
        object Start { get; }

        /// <summary>
        /// The ending index of the slice or null if no ending index defined
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Stop")] // TODO: fix
        object Stop { get; }

        /// <summary>
        /// The length of step to take
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Step")] // TODO: fix
        object Step { get; }
    }
}