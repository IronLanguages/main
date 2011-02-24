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

namespace Microsoft.IronPythonTools.Commands {
    /// <summary>
    /// List of all commands defined for IronPython package
    /// </summary>
    static class CommandTable {
        public static Command[] Commands = new Command[] { 
            new ExecuteInReplCommand(),
            new OpenReplCommand(),
            new SendToReplCommand(),
            new SendToDefiningModuleCommand(),
            new FillParagraphCommand()
        };
    }
}
