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
using System.Threading;
using Microsoft.IronStudio.RemoteEvaluation;

namespace Microsoft.IronStudio {
    class Program {
        /// <summary>
        /// Called when we start the remote server
        /// </summary>
        private static int Main(string[] args) {
            ApartmentState state;
            if (args.Length != 1 || !Enum.TryParse<ApartmentState>(args[0], out state)) {
                Console.WriteLine("Expected no arguments");
                return 1;
            }

            try {
                RemoteScriptFactory.RunServer(state);
                return 0;
            } catch (Exception e) {
                Console.WriteLine(e);
                return 2;
            }
        }
    }
}
