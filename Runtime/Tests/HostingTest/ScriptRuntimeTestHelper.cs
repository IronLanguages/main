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
using Microsoft.Scripting.Hosting;
using NUnit.Framework;

namespace HostingTest
{
    public partial class ScriptRuntimeTest : HAPITestBase {

        public ScriptRuntimeTest() {

        }

        private TestContext testContextInstance;
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        internal static ScriptRuntime CreateRuntime(AppDomain appDomain) {
            return ScriptRuntime.CreateRemote(appDomain, CreateSetup());
        }

        internal static ScriptRuntime CreatePythonOnlyRuntime(string[] ids, string[] exts) {
            ScriptRuntimeSetup srs = new ScriptRuntimeSetup();
            srs.LanguageSetups.Add(new LanguageSetup(
                typeof(IronPython.Runtime.PythonContext).AssemblyQualifiedName,
                "python", ids, exts
            ));

            return new ScriptRuntime(srs);
        }
    }
	
    internal static class ScriptRuntimeExtensions {
        internal static bool IsValid(this ScriptRuntime sr) {
            ScriptEngine se = sr.GetEngine("py");
            ScriptScope ss = se.CreateScope();

            ScriptSource code = se.CreateScriptSourceFromString("five=2+3", Microsoft.Scripting.SourceCodeKind.Statements);
            code.Execute(ss);

            return (int)ss.GetVariable("five") == 5;
        }
    }
}
