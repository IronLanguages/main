using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Policy;
using System.Text;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;
using System.Reflection;
using Microsoft.Scripting.Runtime;

namespace HostingTest{
    [TestFixture]
    public partial class ScriptRuntimeTest : HAPITestBase
    {
        [Test]
        public void Create_WithCurrentAppDomain(){
            ScriptRuntime currentSR = CreateRemoteRuntime(AppDomain.CurrentDomain);
            Assert.IsTrue( currentSR.IsValid());
        }

        [Test]
        public void Create_WithSecondAppDomain(){
            AppDomain secondAppDomain = TestHelpers.CreateAppDomain("SecondAppDomain");
            ScriptRuntime secondSR = CreateRemoteRuntime(secondAppDomain);

            Assert.IsTrue(secondSR.IsValid());
        }
        
        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Create_NullAppDomain() {
            ScriptRuntimeTest.CreateRuntime(null);
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(AppDomainUnloadedException))]
        public void Create_PassUnloadedAppDomain(){
            AppDomain otherAD = TestHelpers.CreateAppDomain("SecondAppDomain"); 

            //this call is not really needed, but let's have it and use it to check if the domain was
            //created properly and we were able to execute a create call successfully
            ScriptRuntime runtime1 = ScriptRuntimeTest.CreateRuntime(otherAD);

            AppDomain.Unload(otherAD);

            // This should throw
            ScriptRuntime runtime2 = ScriptRuntimeTest.CreateRuntime(otherAD);
        }

        //this test seems quite complex to me. Let's discuss about this test
        [Test]
        [Negative]
        [ExpectedException(typeof(AppDomainUnloadedException))]
        public void Create_CallMethodsOnSecondUnloadedAppDomain(){
            AppDomain appDomainOne = TestHelpers.CreateAppDomain("AppDomain 1");
            AppDomain appDomainTwo = TestHelpers.CreateAppDomain("AppDomain 2");

            // This throws exception 'System.IO.FileNotFoundException'
            ScriptRuntime scpRunTimeOne = CreateRemoteRuntime(appDomainOne);
            
            // This should NOT throw an exception
            ScriptRuntime scpRunTimeTwo = CreateRemoteRuntime(appDomainTwo);

            // Unload the AppDomain
            AppDomain.Unload(appDomainOne);

            // This should work fine
            ScriptScope sScpOne = scpRunTimeOne.CreateScope();
            
            // *** USABILITY *** can be simpler then test plan
            // indicates - perhaps could gather test from other code 
            // into one place to do light weight end to end tests.

            // Usability tests - Never Get Here because of earlier exception.
            // Put this in an other peace of code to valid the code.
            sScpOne.SetVariable("foo", 42);
            int exptRtnVal = 4;
            ScriptEngine pEng = scpRunTimeTwo.GetEngine("py");
            // Could add this to PreDefinedCodeSnippets
            ScriptSource sSrc = pEng.CreateScriptSourceFromString("a = 1 + 3");
            object rObj = sSrc.Execute(_testEng.CreateScope());
            Assert.IsNotNull(rObj);
            ScriptScope sScpEng = pEng.CreateScope();
            object fooObj = sScpEng.GetVariable("foo");
            Assert.IsTrue(fooObj.Equals(exptRtnVal), "Did the operation work");
           
            // This should throw AppDomainUnloadedException
            ScriptScope sScpTwo = scpRunTimeTwo.CreateScope();
        }

        [Test]
        public void Create_PartialTrust() {
            // basic check of running a host in partial trust
            AppDomainSetup info = new AppDomainSetup();
            info.ApplicationBase = TestHelpers.BinDirectory;
            info.ApplicationName = "Test";
            Evidence evidence = new Evidence();
            evidence.AddHost(new Zone(SecurityZone.Internet));
            System.Security.PermissionSet permSet = SecurityManager.GetStandardSandbox(evidence);
            AppDomain newDomain = AppDomain.CreateDomain("test", evidence, info, permSet, null);

            ScriptRuntime runtime = CreateRemoteRuntime(newDomain);
            ScriptScope scope = runtime.CreateScope();
            scope.SetVariable("test", "value");

            AppDomain.Unload(newDomain);
        }
        
        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentException))]
        [Ignore] //bug ID : 446714
        public void ExecuteFile_InvalidPath(){
            ScriptRuntime runTime = CreateRuntime();

            // BUG : An invalid path throws an 'Microsoft.Scripting.SyntaxErrorException'
            string[] paths = StandardTestPaths.CreateBadPathCombinations(Path.GetTempPath());
            foreach(string p in paths)
                runTime.ExecuteFile(p + "foo.py");
        }
        
        [Test]
        [Negative]
        [ExpectedException(typeof(SyntaxErrorException))]
        public void ExecuteFile_ValidPathToInvalidScript(){
            String tmpFile = TestHelpers.CreateTempSourceFile(_codeSnippets[CodeType.InCompleteStatement1], ".py");
            _runtime.ExecuteFile(tmpFile);
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentException))]
        public void ExecuteFile_ValidPathToScriptWithUnRegExtension(){
            // Throws : ArgumentException: File extension '.tmp' is not associated with any language.
            String tmpFile = TestHelpers.CreateTempFile(_codeSnippets[CodeType.Valid1]);
            _runtime.ExecuteFile(tmpFile);
        }
        
        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteFile_NullPath(){
            _runtime.ExecuteFile(null);
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentException))]
        public void ExecuteFile_EmptyStringPath(){
            _runtime.ExecuteFile("");
        }

        //todo add basic ExecuteFile test

        [Test]
        public void ExecuteFile_CallValidPathMultiple(){
            String tmpFile = TestHelpers.CreateTempSourceFile( _codeSnippets[CodeType.ValidExpression1], ".py");
            Assert.IsTrue(File.Exists(tmpFile));

            int numberOfExecutions = 10;
            ScriptScope scope = null;
            for (int i = 0; i < numberOfExecutions; i++){
                scope = _runtime.ExecuteFile(tmpFile);
                Assert.IsNotNull(scope);
            }
        }

        [Test]
        [Ignore]//Test not yet implemented
        public void ExecuteFile_CallFromMultipleThreads(){
            Assert.Inconclusive("Test not yet implemented");
        }

        [ExpectedException(typeof(SyntaxErrorException))]
        [Test]//regression test for DDB 488971
        public void ExecuteFile_RemoteADAndSyntaxError() {
            AppDomain ad = TestHelpers.CreateAppDomain("alternate 1");
            ScriptRuntime runtime = CreateRemoteRuntime(ad);

            string tmpFile = TestHelpers.CreateTempSourceFile("if", "py");

            runtime.ExecuteFile(tmpFile); //throws 'SourceUnit not serializable exception'
        }


        [Test]
        public void UseFile_CallValidPathMultiple() {
            string tmpFile = TestHelpers.CreateTempSourceFile(_codeSnippets[CodeType.ValidExpression1], ".py");
            Assert.IsTrue(File.Exists(tmpFile));

            // Add the temp path to the search paths
            var paths = _PYEng.GetSearchPaths();
            var newPaths = new List<string>(paths);
            newPaths.Add(Path.GetTempPath());
            _PYEng.SetSearchPaths(newPaths);

            try {
                ScriptScope scope = _runtime.UseFile(tmpFile);
                Assert.IsNotNull(scope);
                for (int i = 0; i < 10; i++) {
                    ScriptScope scope2 = _runtime.UseFile(tmpFile);
                    Assert.IsNotNull(scope2);
                    // Note: can't compare object identity because
                    // ScriptEngine.GetScope doesn't return the same instance
                    TestHelpers.AreEqualIEnumerables(scope.GetVariableNames(), scope2.GetVariableNames());
                }
            } finally {
                // restore paths
                _PYEng.SetSearchPaths(paths);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UseFile_NullPath() {
            var scope = _runtime.UseFile(null);
        }

        [Test]
        [ExpectedException(typeof( ArgumentException))]
        public void UseFile_EmptyPath() {
            var scope = _runtime.UseFile("");
        }

        [Test]
        public void UseFile_ValidPath() {
            string tmpFile = TestHelpers.CreateTempSourceFile(_codeSnippets[CodeType.IsOddFunction], ".py");
            var scope = _runtime.UseFile(tmpFile);

            Func<int, bool> isodd = scope.GetVariable<Func<int, bool>>("isodd");
            Assert.IsFalse(isodd(12));

            File.Delete(tmpFile);
            Assert.IsFalse(File.Exists(tmpFile));

            scope = _runtime.UseFile(tmpFile);
            isodd = scope.GetVariable<Func<int, bool>>("isodd");
            Assert.IsFalse(isodd(12));

            var v = File.CreateText(tmpFile);
            v.Close();

            File.WriteAllText(tmpFile, _codeSnippets[CodeType.IsEvenFunction]);
            
            scope = _runtime.UseFile(tmpFile);
            isodd = scope.GetVariable<Func<int, bool>>("isodd");
            Assert.IsFalse(isodd(12));
        }

        [Test]
        [ExpectedException( typeof(FileNotFoundException))]
        public void UseFile_PartialPath() {
            string tmpFile = TestHelpers.CreateTempSourceFile(_codeSnippets[CodeType.IsOddFunction], ".py");
            var scope = _runtime.UseFile(Path.GetFileName(tmpFile));

            Func<int, bool> isodd = scope.GetVariable<Func<int, bool>>("isodd");
            Assert.IsFalse(isodd(12));
        }

        /// <summary>
        /// Ensure multiple sets result in the latest 'set' having the effect 
        /// (this test is pretty obvious. but doesnt hurt to have here)
        /// </summary>
        [Test]
        public void Globals_TryToAccessNamesSet(){
            ScriptScopeDictionary dict = new ScriptScopeDictionary();
            string key = "foo";
            dict[key] = 1;
            
            _runtime.Globals = _testEng.CreateScope(new ObjectDictionaryExpando(dict));
            Assert.AreEqual(1, _runtime.Globals.GetVariable( key));

            dict[key] = 2;
            _runtime.Globals = _testEng.CreateScope(new ObjectDictionaryExpando(dict));

            Assert.AreEqual(2, _runtime.Globals.GetVariable(key));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Globals_Null(){
            ScriptRuntime runtime = CreateRuntime();
            runtime.Globals = null;
        }

        /// <summary>
        /// Test to ensure CreateScope doesnt have side effects on the 'Globals' prop
        /// </summary>
        [Test]
        public void Globals_NoSideEffectsFromCreateScope(){
            ScriptScope temp = _runtime.Globals;
            ScriptScope newScope = _testEng.CreateScope();

            Assert.AreEqual(temp, _runtime.Globals);
        }

        [Test]
        public void CreateScope_WithZeroExistingScopes(){
            ScriptScope scope = _runtime.CreateScope();

            Assert.IsTrue(!scope.HasNoDefaultLanguage());
            Assert.IsTrue(scope.IsEmpty());
        }

        [Test]
        public void CreateScope_WithManyPreExistingScopes(){
            ScriptScope curScope, prevScope = null;

            // Create a 'numberOfScopeTests' and make
            // each scope is unique - 
            for (int i = 0; i < 10; i++)
            {
                curScope = _runtime.CreateScope();

                Assert.IsTrue(curScope.IsValid());
                Assert.IsTrue(!curScope.HasNoDefaultLanguage());

                curScope.SetVariable( i.ToString(), i);

                if(i > 0){
                    Assert.IsFalse(prevScope.ContainsVariable(i.ToString()));
                    Assert.IsTrue(curScope.ContainsVariable(i.ToString()));
                }

                prevScope = curScope;
            }
        }

        [Test]
        public void CreateScope_LinkBetweenReturnedScopeAndArg() {
            ScriptScopeDictionary dict = new ScriptScopeDictionary();
            ScriptScope attachedScope = _testEng.CreateScope(new ObjectDictionaryExpando(dict));
            string key = "how";
            string editDict = "through dict", editScope = "through scope";

            //edit a) dictionary directly b)through the scope
            dict[key] = editDict;
            attachedScope.SetVariable(key, editScope);

            Assert.AreEqual(dict[key], editScope);

            dict[key] = editDict;
            Assert.AreEqual(editDict, attachedScope.GetVariable(key));
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScope_PassNull(){
            ObjectDictionaryExpando global = null;
            _runtime.CreateScope(global);
        }

        /// <summary>
        /// Pass a valid global IAttributesCollection object and validate 
        /// all lookups in the scope go through the provided IAttributesCollection
        /// </summary>
        [Test]
        public void CreateScope_PassValidScope(){
            ScriptScopeDictionary dict = new ScriptScopeDictionary();
           
            dict["foo_1"] = 1000;
            dict["foo_2"] = 2000;

            ScriptScope linkedScope = _runtime.CreateScope(new ObjectDictionaryExpando(dict));

            linkedScope.SetVariable("foo_3", 3000);
            linkedScope.SetVariable("foo_4", 4000);

            Assert.IsTrue(linkedScope.IsValid());

            Assert.IsTrue(dict["foo_3"].Equals(3000));
            Assert.IsTrue(dict["foo_4"].Equals(4000));
            Assert.IsTrue(linkedScope.ContainsVariable("foo_1"));
            Assert.IsTrue(linkedScope.ContainsVariable("foo_2"));
        }


        /// <summary>
        /// Invoke the method twice with the same 'globals'
        /// 
        /// 2 different ScriptScopes with no side effect created.
        /// Verify the first returned value has no changes after
        /// the second call.
        /// </summary>
        [Test]
        public void CreateScope_InvokeMethodTwiceWithSameGlobals(){
            ScriptScopeDictionary dict = new ScriptScopeDictionary();

            ScriptRuntime runTimeOne = CreateRuntime();
            ScriptRuntime runTimeTwo = CreateRuntime();

            dict["foo_1"] = 1000;

            ScriptScope scope1 = runTimeOne.CreateScope(new ObjectDictionaryExpando (dict));
            ScriptScope scope2 = runTimeTwo.CreateScope(new ObjectDictionaryExpando (dict));

            Assert.IsTrue( scope1.IsSimilarButNotSameAs(scope2));
        }

        [Negative]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetEngine_PassNull() {
            _runtime.GetEngine(null);
        }

        [Negative]
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetEngine_PassEmptyString(){
            _runtime.GetEngine("");
        }

        [Negative]
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetEngine_PassUnRegisteredLangID(){
            _runtime.GetEngine("__sdfsllslsslslslsiiiiiiuuppppwmxxmxmss___");
        }

        [Test]
        public void GetEngine_PassAllRegisteredLanguages(){
            Dictionary<ScriptEngine, string[]> idDict = new Dictionary<ScriptEngine, string[]>() {
                {_PYEng, new string[]{ "py", "python", "ironpython" }},
                {_RBEng, new string[]{ "rb", "ruby", "ironruby" }},
            };

            foreach (var key in idDict.Keys) {
                foreach (var id in idDict[key]) {
                    Assert.AreEqual(key, _runtime.GetEngine(id));
                }
            }
        }

        [Test]
        public void GetEngine_StandardStringInputTest(){
            foreach (string str in StandardTestStrings.AllStrings) {
                try {
                    _runtime.GetEngine(str);
                    Assert.Fail("shouldn't reach here. The prev line should throw");
                }
                catch (ArgumentException) {
                }
            }
        }

        [Negative]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetEngineByFileExtension_PassNull(){
            _runtime.GetEngineByFileExtension((string)null);
        }

        [Negative]
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetEngineByFileExtension_PassEmptyString(){
            _runtime.GetEngineByFileExtension("");
        }

        [Negative]
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetEngineByFileExtension_UnRegisteredLangID(){
            _runtime.GetEngineByFileExtension("__sdfsllslsslslslsiiiiiiuuppppwmxxmxmss___");
        }


        [Test]
        public void GetEngineByFileExtension_AllRegisteredExtensions(){
            Dictionary<ScriptEngine, string[]> fileExtDict = new Dictionary<ScriptEngine, string[]>() {
                {_PYEng, new string[] { ".py" } },
                {_RBEng, new string[] { ".rb" }},
            };

            foreach (var key in fileExtDict.Keys) {
                foreach (var id in fileExtDict[key]) {
                    Assert.AreEqual(key, _runtime.GetEngineByFileExtension(id));
                }
            }
        }

        [Test]
        [Ignore]//Unregistering a language is not supported. So this test may have to be removed
        public void GetEngineByFileExtension_UnRegisteredExtensions(){
            ScriptRuntime runtime = ScriptRuntimeTest.CreatePythonOnlyRuntime( new[]{"py"}, new[]{".py"});

            Assert.IsTrue(1 == runtime.Setup.LanguageSetups.Count);
            // @TODO - How do we un-register a language? what
            //         are the expected results.
            Assert.Inconclusive(
                "How do we un-register a language? what are the expected results");
        }

        [Test]
        public void GetEngineByFileExtension_PrecedingAPeriodRegisteredExtensions(){
            Assert.IsTrue(_runtime.GetEngineByFileExtension(".py").Setup.DisplayName.Contains("IronPython"));
        }

        [Negative]
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetEngineByFileExtension_PrecedingPeriodsRegisteredExtensions(){
            string lang = _runtime.GetEngineByFileExtension("..py").Setup.DisplayName;
        }

        [Test]
        public void Configuration_ManyRegisteredLanguages() {
            //the default runtime should have more than 1 registered languages
            Assert.IsTrue(_runtime.Setup.LanguageSetups.Count > 1);
        }

        [Test]
        public void Configuration_OneRegisteredFileExtension(){
            ScriptRuntime runtime = ScriptRuntimeTest.CreatePythonOnlyRuntime(new[] { "py" }, new[] { ".py" });
            Assert.IsTrue(1 == runtime.Setup.LanguageSetups[0].FileExtensions.Count);
        }

        [Test]
        public void Configuration_OneRegisteredLanguageName() {
            ScriptRuntime runtime = ScriptRuntimeTest.CreatePythonOnlyRuntime(new[] { "py" }, new[] { ".py" });
            Assert.IsTrue(1 == runtime.Setup.LanguageSetups[0].Names.Count);
        }

        [Test]
        public void Configuration_ManyRegisteredLanguageNames() {
            //default runtime has more than 1 registered lang id
            Assert.IsTrue(_runtime.Setup.LanguageSetups[0].Names.Count > 1);
        }

        [Test]
        public void ScriptHost_Invoke(){
            Assert.IsNotNull( _runtime.Host);
        }

        [Test]
        public void ScriptIO_Get(){
            Assert.IsNotNull(_runtime.IO);
        }

        [Test, Negative, ExpectedException(typeof(ArgumentException))]
        public void Setup_EmptySetup() {
            
            new ScriptRuntime(new ScriptRuntimeSetup());
        }

        [Test]
        public void Setup_EmptyDisplayName() {
            
            var setup = ScriptRuntimeTest.CreateSetup();
            setup.LanguageSetups[0].DisplayName = "";
            var runtime = new ScriptRuntime(setup);
            Assert.AreEqual("", runtime.Setup.LanguageSetups[0].DisplayName);
        }

        [Test, Negative, ExpectedException(typeof(InvalidOperationException))]
        public void Setup_InvalidTypeName() {            
            // Dev10 bug 502234
            var setup = ScriptRuntimeTest.CreateSetup();
            setup.LanguageSetups[0].TypeName = setup.LanguageSetups[0].TypeName.Replace("PythonContext", "PythonBuffer");
            var runtime = new ScriptRuntime(setup);
            runtime.GetEngine("py");
        }

        [Test]
        public void Setup_AssemblyNameWithoutPublicKeyToken() {
            // Dev10 bug 502278
            var setup = new ScriptRuntimeSetup();
            setup.LanguageSetups.Add(new LanguageSetup("IronPython.Runtime.PythonContext,IronPython", "IronPython", new[]{"py"}, new[] {"py"}));
            var python = new ScriptRuntime(setup).GetEngine("py");
            Assert.AreEqual(python.Setup.DisplayName, "IronPython");
        }

        [Test]
        public void CreateScope_GetEngineWithNoLanguage() {
            var runtime = ScriptRuntimeTest.CreateRuntime();
            var scope = runtime.CreateScope();
            Assert.AreNotEqual(scope.Engine, null);
        }
    }
}
