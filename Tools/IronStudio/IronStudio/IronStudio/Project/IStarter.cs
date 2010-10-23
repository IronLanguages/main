/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace Microsoft.IronStudio.Project {
    /// <summary>
    /// Defines an interface for starting a project or a file with
    /// or without debugging.
    /// </summary>
    public interface IStarter {
        /// <summary>
        /// Starts a project with or without debugging.
        /// </summary>
        void StartProject(CommonProjectNode project, bool debug);

        /// <summary>
        /// Starts a file in a project with or without debugging.
        /// </summary>
        void StartFile(CommonProjectNode project, string file, bool debug);

        /// <summary>
        /// Starts standalone file with or without debugging.
        /// Relative file paths are considered to be relative to the current directory.
        /// </summary>
        void StartFile(string/*!*/ file, bool debug);
    }
}
