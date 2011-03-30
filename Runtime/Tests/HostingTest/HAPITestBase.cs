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

using Microsoft.Scripting.Hosting;
using IronRuby.Runtime;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Scripting.Generation;


namespace HostingTest {
    public class HAPITestBase {

        static internal PreDefinedCodeSnippets _codeSnippets;
//      static string _testLanguage;

        //Don't use this member. Use _runtime instead. 
        //This will be deleted once all the files are switched over to the id with correct casing
        protected ScriptRuntime _runTime;
        protected ScriptRuntime _runtime, _remoteRuntime;

        protected ScriptEngine _testEng;

        protected ScriptEngine _PYEng;
        protected ScriptEngine _RBEng;

        protected ScriptScope _defaultScope;

        protected HAPITestBase() {
            
            var ses = CreateSetup();
            ses.HostType = typeof(TestHost);
            _runtime = new ScriptRuntime(ses);
            _remoteRuntime = ScriptRuntime.CreateRemote(TestHelpers.CreateAppDomain("Alternate"), ses);

            _runTime = _runtime;// _remoteRuntime;

            _PYEng = _runTime.GetEngine("py");
            _RBEng = _runTime.GetEngine("rb");

            SetTestLanguage();
            
            _defaultScope = _runTime.CreateScope();
            _codeSnippets = new PreDefinedCodeSnippets();
        }

        public static ScriptRuntime CreateRuntime() {
            return new ScriptRuntime(CreateSetup());
        }

        public static ScriptRuntime CreateRemoteRuntime(AppDomain domain) {
            return ScriptRuntime.CreateRemote(domain, CreateSetup());
        }

        public static ScriptRuntimeSetup CreateSetup() {
            var configFile = TestHelpers.StandardConfigFile;
            Debug.Assert(File.Exists(configFile), configFile);
            return ScriptRuntimeSetup.ReadConfiguration(configFile);
        }

        private void SetTestLanguage() {
//          _testLanguage = "ironpython";
            _testEng = _PYEng; 
        }

    }
}
