/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using Microsoft.IronRubyTools.Library.Repl;
using Microsoft.IronStudio.Repl;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.IronRubyTools.Commands {
    /// <summary>
    /// Provides the command for starting the IronRuby REPL window.
    /// </summary>
    class OpenReplCommand : Command {
        public override void DoCommand(object sender, EventArgs args) {
            var window = ExecuteInReplCommand.EnsureReplWindow();

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
            ((IReplWindow)window).Focus();
        }

        public override int CommandId {
            get { return (int)PkgCmdIDList.cmdidReplWindow; }
        }
    }
}
