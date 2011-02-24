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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.IronStudio.Repl;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Threading;

namespace Microsoft.IronStudio.Core.Repl {
    [Export(typeof(IReplCommand))]
    class EchoReplCommand : IReplCommand {
        #region IReplCommand Members

        public void Execute(IReplWindow window, string arguments) {
            arguments = arguments .ToLowerInvariant();
            if (arguments == "on") {
                window.ShowOutput = true;
            } else {
                window.ShowOutput = false;
            }
        }

        public string Description {
            get { return "Suppress or unsuppress output to the buffer"; }
        }

        public string Command {
            get { return "echo"; }
        }

        public object ButtonContent {
            get {
                return null;
            }
        }

        #endregion
    }
}
