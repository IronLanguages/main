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

namespace Microsoft.Scripting.Hosting.Shell {
    /// <summary>
    /// Used to dispatch a single interactive command. It can be used to control things like which Thread
    /// the command is executed on, how long the command is allowed to execute, etc
    /// </summary>
    public interface ICommandDispatcher {
        object Execute(CompiledCode compiledCode, ScriptScope scope);
    }
}