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

using System;
using Microsoft.IronPythonTools.Library.Repl;
using Microsoft.IronPythonTools.Internal;

namespace Microsoft.IronPythonTools.Commands {
    class SendToDefiningModuleCommand : SendToReplCommand {
        public override void DoCommand(object sender, EventArgs args) {
            var window = ExecuteInReplCommand.EnsureReplWindow();
            var eval = window.Evaluator as RemotePythonEvaluator;
            var activeView = IronPythonToolsPackage.GetActiveTextView();

            string path = activeView.GetFilePath();
            if (path != null) {
                var scope = eval.Engine.GetScope(path);
                if (scope != null) {
                    // we're now in the correct module, execute the code
                    string scopeName = eval.SetScope(scope);
                    window.Cancel();
                    if (scopeName != String.Empty) {
                        window.WriteLine(eval.Prompt + " %module " + scopeName);
                        window.WriteLine(String.Format("Current scope changed to {0}", scopeName));
                    } else {
                        window.WriteLine(eval.Prompt + " %module (unknown name)");
                        window.WriteLine("Current scope changed to (unknown name)");
                    }
                    
                    base.DoCommand(sender, args);
                } else {
                    window.WriteLine(String.Format("Could not find module: {0}", path));
                }
            } else {
                window.WriteLine("Could not find module");
            }
        }

        public override int CommandId {
            get { return (int)PkgCmdIDList.cmdidSendToDefiningModule; }
        }
    }
}
