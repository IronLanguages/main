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
using Microsoft.IronStudio.Core;
using Microsoft.IronPythonTools;

namespace UnitTests {
    class MockDlrRuntimeHost : IPythonRuntimeHost {
        public Microsoft.Scripting.Hosting.ScriptEngine ScriptEngine {
            get { return Program.PythonEngine; }
        }

        public Microsoft.VisualStudio.Utilities.IContentType ContentType {
            get { return Program.PythonContentType; }
        }

        public bool EnterOutliningModeOnOpen {
            get { return false; }
            set { }
        }

        public bool IntersectMembers {
            get { return true; }
            set { }
        }

        public bool HideAdvancedMembers {
            get { return false; }
            set { }
        }
    }
}
