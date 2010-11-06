using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using System.Text;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Dynamic;



namespace HostingTest {
    /// <summary>
    ///This is a test class for ScriptEngineTest and is intended
    ///to contain all ScriptEngineTest Unit Tests
    ///</summary>
    [TestClass()]
    public partial class ScriptEngineTest {


        [TestMethod()]
        public void Runtime_Test() {
            //@TODO - Custom language
            Assert.AreEqual(_runTime.GetEngine("python").Runtime, _runTime);
            Assert.AreEqual(_runTime.GetEngine("ruby").Runtime, _runTime);
        }


        [TestMethod()]
        public void LanguageDisplayName_Test() {
            Assert.AreEqual(_runTime.GetEngine("python").Setup.DisplayName, "IronPython 2.7 Beta 1");
            Assert.AreEqual(_runTime.GetEngine("ruby").Setup.DisplayName, "IronRuby");
        }


        [TestMethod()]
        public void GetRegisteredIdentifiers_Test() {
            var result = _runTime.GetEngine("python").Setup.Names;
            TestHelpers.AreEqualArrays(new string[] { "IronPython", "Python", "py" }, result);
            result = _runTime.GetEngine("ruby").Setup.Names;
            TestHelpers.AreEqualArrays(new string[] { "IronRuby", "Ruby", "rb" }, result);
        }

        [TestMethod()]
        public void GetRegisteredExtensions_Test() {
            var result = _runTime.GetEngine("python").Setup.FileExtensions;
            TestHelpers.AreEqualArrays(new string[] { ".py" }, result);

            result = _runTime.GetEngine("ruby").Setup.FileExtensions;
            TestHelpers.AreEqualArrays(new string[] { ".rb" }, result);
        }
        
        [TestMethod()]
        public void LanguageVersion_Test() {
            //LanguageVersion
            Version ver = _runTime.GetEngine("python").LanguageVersion;
            Assert.AreEqual(ver, typeof(PythonContext).Assembly.GetName().Version);
            //ver = _runTime.GetEngine("ruby").LanguageVersion;
            //Assert.AreEqual(ver, Ruby.Runtime.RubyContext.IronRubyVersion);
        }

        [TestMethod()]
        public void CreateScriptSource_DifferentEncoding_Test() {

            List<ScriptSource> sources = CreateSourceListWithDifferentEncodings(_codeSnippets[CodeType.Valid1]);

            foreach (ScriptSource src in sources) {

                //Exec the code in a new scope
                ScriptScope scope = _runTime.CreateScope();
                Assert.AreEqual(null, src.Execute(scope));
                Assert.AreEqual(_codeSnippets[CodeType.Valid1], src.GetCode());

                //Verify the scope's contents after execution
                Assert.IsTrue(!scope.ContainsVariable("local"));
                Assert.IsTrue(!scope.ContainsVariable("local2"));
                Assert.IsTrue(scope.ContainsVariable("global2"));
                Assert.IsTrue(scope.ContainsVariable("increment"));
                Assert.AreEqual(4, scope.GetVariable<int>("global1"));

                //Add the identifier to the scope
                scope.SetVariable("srcid", src.Path);
            }

            DeleteTempFiles();
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetScope_Null_InvalidCode_Test() {
            _testEng.GetScope(null);
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSource_NullCodeMemberMethod_Test() {
            _testEng.CreateScriptSource((CodeMemberMethod)null);
        }


        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(SyntaxErrorException))]
        public void CreateScriptSource_InvalidCode_Test() {
            //Ensure that creating a ScriptSource doesn't trigger any parsing or execution of code
            //by constructing ScriptSource objects over an invalid code snippet.
            string code = _codeSnippets[CodeType.Junk];
            List<ScriptSource> sources = CreateSourceListWithDifferentEncodings(code);
            foreach (ScriptSource src in sources) {

                Assert.AreEqual(code, src.GetCode());
                ScriptScope scope = _runTime.CreateScope();

                src.Execute(scope);
                
            }

            DeleteTempFiles();
        }

        [TestMethod()]
        public void CreateScriptSourceFromFile_InvalidName_Test() {
            Assert.IsNotNull(_testEng.CreateScriptSourceFromFile("invalid?file|<name>"));
        }

        [TestMethod()]
        public void CreateScriptSourceFromFile_FileIsADir_Test() {
            Assert.IsNotNull(_testEng.CreateScriptSourceFromFile(Path.GetTempPath()));
        }

        [TestMethod()]
        public void CreateScriptSourceFromFile_NonExistentFile_Test() {
            string tempFile = Path.GetTempFileName();
            File.Delete(tempFile);

            Assert.IsNotNull(_testEng.CreateScriptSourceFromFile(tempFile));
        }



        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSourceFromString_NullString_Test() {
            _testEng.CreateScriptSourceFromString(null);
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSourceFromString_NullString2_Test() {
            _testEng.CreateScriptSourceFromString(null, SourceCodeKind.Statements);
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSourceFromString_NullString3_Test() {
            _testEng.CreateScriptSourceFromString(null, "id");
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSourceFromString_NullString4_Test() {
            _testEng.CreateScriptSourceFromString(null, "id", SourceCodeKind.Statements);
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSource_NullString_Test() {
            _testEng.CreateScriptSource(null);
        }


        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSource_NullCodeObject_Test() {
            _testEng.CreateScriptSource((CodeObject)null, SourceCodeKind.Statements);
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSource_NullCodeObject2_Test() {
            _testEng.CreateScriptSource((CodeObject)null, "some_path.py");
        }


        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSource_NullStreamContentProvider_Test() {
            _testEng.CreateScriptSource((StreamContentProvider)null, "some_path.py");
        }



        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSource_NullCodeObject3_Test() {
            _testEng.CreateScriptSource((CodeObject)null, "some_path.py", SourceCodeKind.Statements);
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSource_NullStreamContentProvider2_Test() {
            _testEng.CreateScriptSource((StreamContentProvider)null, "some_path.py", Encoding.Unicode);
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSource_NullTextContentProvider_Test() {
            _testEng.CreateScriptSource((TextContentProvider)null, "some_path.py", SourceCodeKind.Statements);
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSource_NullStreamContentProvider3_Test() {
            _testEng.CreateScriptSource((StreamContentProvider)null, "some_path.py", 
                                            Encoding.Unicode, SourceCodeKind.Statements);
        }


        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSourceFromFile_NullArgs_Test() {
            _testEng.CreateScriptSourceFromFile(null);
        }

        [Negative]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSourceFromFile_NullArgsUnicode_Test() {
            _testEng.CreateScriptSourceFromFile(null, Encoding.Unicode);
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSourceFromFile_InvalidFile_Test() {
            _testEng.CreateScriptSourceFromFile("some_path.py", null);
        }

        [Negative]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSourceFromFile_NullArgsUnicode_Test2() {
            _testEng.CreateScriptSourceFromFile(null, Encoding.Unicode, SourceCodeKind.File);
        }

        [Negative]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSourceFromFile_InvalidFileUnicode_Test() {
            _testEng.CreateScriptSourceFromFile("some_path.py", null, SourceCodeKind.File);
        }
        
        [Negative]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Execute_WithNullString() {
            _testEng.Execute(null);
        }

        [Negative]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Execute_WithNullScope() {
            _testEng.Execute("", null);
        }

        [Negative]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteOfT_WithNullString() {
            _testEng.Execute<object>(null);
        }

        [Negative]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteOfT_WithNullScope() {
            _testEng.Execute<object>("", null);
        }

        [Negative]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteAndWrap_WithNullString() {
            _testEng.ExecuteAndWrap(null);
        }

        [Negative]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteAndWrap_WithNullScope() {
            _testEng.ExecuteAndWrap("", null);
        }

        [Negative]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteFile_WithNullNullPath() {
            _testEng.ExecuteFile(null);
        }

        [Negative]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteFile_WithNullScope() {
            _testEng.ExecuteFile("", null);
        }

        [Negative]
        [TestMethod]
        [ExpectedException(typeof(SyntaxErrorException))]
        public void Execute_WithInvalidCode() {
            _testEng.Execute(_codeSnippets[CodeType.InCompleteExpression2]);
        }

        [Negative]
        [TestMethod]
        public void Execute_WithScope() {
            ScriptScope scope = _testEng.CreateScope();
            scope.SetVariable("x", 1);
            scope.SetVariable("y", 1);
            var result = _PYEng.Execute<int>("x+y", scope);
            Assert.AreEqual(result, 2);
        }

        [TestMethod]
        public void ExecuteFile_WithoutScope() {
            // the extension should be ignored:
            String tmpFile = TestHelpers.CreateTempSourceFile("x = 2 + 2", ".foo");

            ScriptScope result = _testEng.ExecuteFile(tmpFile);
            var x = result.GetVariable<int>("x");
            Assert.AreEqual(x, 4);

            // resulting scope should be associated with the engine that executed the code:
            Assert.AreEqual(_testEng, result.Engine);
        }

        [TestMethod]
        public void ExecuteFile_WithScope() {
            // the extension should be ignored:
            String tmpFile = TestHelpers.CreateTempSourceFile("x = y + 1", ".bar");

            ScriptScope scope = _testEng.CreateScope();
            scope.SetVariable("y", 1);
            
            ScriptScope result = _testEng.ExecuteFile(tmpFile, scope);
            Assert.AreEqual(result, scope);

            var x = scope.GetVariable<int>("x");
            Assert.AreEqual(x, 2);
        }
        
        //TODO : these tests should move to script source. 
        #region CompileExpression tests
        /// <summary>
        /// Covers ScriptEngine.CompileExpression for various basic expressions
        /// </summary>
        [TestMethod()]
        public void Compile_Expression_Basic1_Test() {
            ScriptScope scope = _runTime.CreateScope();

            //Compile and execution a normal expression
            CompiledCode e1 = _testEng.CreateScriptSourceFromString("2+2").Compile();
            TestHelpers.AssertOutput(_runTime, delegate() { e1.Execute(scope); }, "");
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(SyntaxErrorException))]
        public void Compile_Expression_SyntaxError_Test() {
            ScriptScope scope = _runTime.CreateScope();

            //Compile an invalid expression
            _testEng.CreateScriptSourceFromString("? 2+2").Compile();
            
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(UnboundNameException))]
        public void Compile_Expression_UnboundVar_Test() {
            ScriptScope scope = _runTime.CreateScope();

            //Compile an expression referencing an unbound variable, which generates a runtime error
            CompiledCode e1 = _testEng.CreateScriptSourceFromString("unbound + 2").Compile();
            e1.Execute(scope);
            
        }

        [TestMethod()]
        public void Compile_Expression_ModuleBoundVar_Test() {
            ScriptScope scope = _runTime.CreateScope();
            scope.SetVariable("modulevar", 5);


            //Compile an expression referencing a module bound variable
            CompiledCode e1 = _testEng.CreateScriptSourceFromString("modulevar+2").Compile();
            TestHelpers.AssertOutput(_runTime, delegate() { e1.Execute(scope); }, "");
        }

        [TestMethod()]
        public void Compile_Expression_TODO() {
            ScriptScope scope = _runTime.CreateScope();
            scope.SetVariable("modulevar", 5);


            //@TODO - CompileExpression with meaningful module argument

            //@TODO - Note, JScript doesn't currently support CompileExpression, add tests when they do
        }
        #endregion


        /// <summary>
        /// Covers ScriptEngine.CompileStatement for various basic statements
        /// </summary>
        #region CompileStatement Tests
        [TestMethod()]
        public void Compile_Statement_Basic1_Test() {
            ScriptScope scope = _runTime.CreateScope();
            scope.SetVariable("modulevar", 5);

            //Compile and execute a normal statement
            CompiledCode e1 = _testEng.CreateScriptSourceFromString("print 2+2", SourceCodeKind.Statements).Compile();
            TestHelpers.AssertOutput(_runTime, delegate() { e1.Execute(scope); }, "4");
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(SyntaxErrorException))]
        public void Compile_Statement_SyntaxError_Test() {
            ScriptScope scope = _runTime.CreateScope();

            //Compile an invalid statement
            _testEng.CreateScriptSourceFromString("? 2+2", SourceCodeKind.Statements).Compile();
            
        }

        [Negative]
        [TestMethod()]
        [ExpectedException(typeof(UnboundNameException))]
        public void Compile_Statement_UnboundVar_Test() {
            ScriptScope scope = _runTime.CreateScope();

            //Compile a statement referencing an unbound variable, which generates a runtime error
            CompiledCode e1 = _testEng.CreateScriptSourceFromString("print unbound+2", SourceCodeKind.Statements).Compile();
            e1.Execute(scope);
            
        }

        [TestMethod()]
        public void Compile_Statement_ModuleBoundVar_Test() {
            ScriptScope scope = _runTime.CreateScope();
            scope.SetVariable("modulevar", 5);

            //Compile a statement referencing a module bound variable
            CompiledCode e1 = _testEng.CreateScriptSourceFromString("print modulevar+2", SourceCodeKind.Statements).Compile();
            TestHelpers.AssertOutput(_runTime, delegate() { e1.Execute(scope); }, "7");
        }

        [TestMethod()]
        public void Compile_Statement_StatementBoundVar_Test() {
            ScriptScope scope = _runTime.CreateScope();

            //Bind a module variable in a statement and then reference it
            _testEng.CreateScriptSourceFromString("pythonvar='This is a python variable'", SourceCodeKind.Statements).Execute(scope);
            CompiledCode e1 = _testEng.CreateScriptSourceFromString("print pythonvar", SourceCodeKind.Statements).Compile();
            TestHelpers.AssertOutput(_runTime, delegate() { e1.Execute(scope); }, "This is a python variable");
            Assert.IsTrue(scope.ContainsVariable("pythonvar"), "Bound variable isn't visible in the module dict");

        }

        [TestMethod()]
        public void Compile_Statement_TODO() {
            ScriptScope scope = _runTime.CreateScope();
            scope.SetVariable("modulevar", 5);


            //@TODO - CompileStatement with meaningful module argument

            //@TODO - JScript also doesn't support CompileStatement yet
        }
        #endregion

        [TestMethod()]
        public void Compile_Basic2_Test() {
            ScriptScope scope = _runTime.CreateScope();
            scope.SetVariable("modulevar", 5);

            //Define methods in the module we'll use for code
            _testEng.CreateScriptSourceFromString(@"
def f(arg):
    print arg
", SourceCodeKind.Statements).Execute(scope);

            //Compile and execute valid code
            CompiledCode e1 = _testEng.CreateScriptSourceFromString("f('Hello World!')").Compile();
            TestHelpers.AssertOutput(_runTime, delegate() { e1.Execute(scope); }, "Hello World!");
        }

        [TestMethod()]
        public void Compile_TODO_Tests() {
            ScriptScope scope = _runTime.CreateScope();
            scope.SetVariable("modulevar", 5);

            _testEng.CreateScriptSourceFromString(@"
def f(arg):
    print arg
", SourceCodeKind.Statements).Execute(scope);
        }

        [TestMethod()]
        [Negative]
        [ExpectedException(typeof(ArgumentException))]
        public void Runtime_Exception_Test() {
            ScriptRuntime sRuntime = _runTime.GetEngine("IronButterfly").Runtime;
        }

        [TestMethod]
        [Negative]
        [ExpectedException(typeof(ArgumentException))]
        public void LanguageDisplay_WrongLanguage() {
            string result = _runTime.GetEngine("Iron Butterfly").Setup.DisplayName;
        }

        [TestMethod]
        [Negative]
        [ExpectedException(typeof(ArgumentException))]
        public void LanguageDisplayName_EmptyLanguage() {
            var name = _runTime.GetEngine("").Setup.DisplayName;
        }
        
        [TestMethod]
        public void GetRegisteredIdentifiers_LangWithNoIDs_Test() {
            ScriptRuntime runtime = ScriptRuntimeTest.CreatePythonOnlyRuntime(new string[] { }, new string[]{".py"});
            ScriptEngine engine = runtime.GetEngineByFileExtension("py");
            Assert.IsTrue(0 == engine.Setup.Names.Count);
        }

        [TestMethod]
        public void GetRegisteredExtensions_LangWithNoExt() {
            ScriptRuntime runtime = ScriptRuntimeTest.CreatePythonOnlyRuntime(new string[] {"py" }, new string[] { });
            ScriptEngine engine = runtime.GetEngine("py");
            Assert.IsTrue(0 == engine.Setup.FileExtensions.Count);
        }


        /// <summary>
        ///  Invoke accessor method to test for a valid return object.
        /// </summary>
        [TestMethod]
        public void Operations_InvokeIsNotNull_Test() {
            
            Assert.IsNotNull(_testEng.Operations);
            Assert.IsNotNull(_RBEng.Operations);
            Assert.IsNotNull(_runTime.Operations);
        }

        /// <summary>
        /// Ensure there are no side effects when calling CreateOperations.
        /// The Operations property shouldn't be affected during a call to this method
        /// </summary>
        [TestMethod]
        public void Operations_MultiAccess_Test() {
                        
            ObjectOperations operations = _testEng.Operations;
            ObjectOperations dummyResult = _testEng.CreateOperations();

            //Ensure the previous call didn't have any side effects.
            Assert.AreEqual(operations, _testEng.Operations);

            
        }
        /// <summary>
        /// Verify Successfully Invoke a usable new instance.
        /// </summary>
        [TestMethod]
        public void CreateOperations_Invoke_Test() {
            Assert.IsNotNull(_testEng.CreateOperations());
        }

        /// <summary>
        /// Make multiple instances of an object and 
        /// and make sure that they are different.
        /// 
        /// Only test that the next instance is not the
        /// same as the last instance
        /// </summary>
        [TestMethod]
        public void CreateOperations_MultipleTimes() {
            
            const int n = 5;
            ObjectOperations[] OpA = new ObjectOperations[n];
            for (int i = 0; i < n; i++) {
                Assert.IsNotNull(OpA[i] = _testEng.CreateOperations());
                if (i > 0) Assert.IsTrue(OpA[i] != OpA[i - 1]);
            }
        }
        
        [TestMethod]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateOperations_NullScopeArg_Test() {
            _testEng.CreateOperations(null);
        }


        [TestMethod]
        public void TestFromFuture_localScope() {
            string testSrc = _codeSnippets[CodeType.ImportFutureDiv];
            ScriptRuntime sr = CreateRuntime();
            ScriptEngine pyeng = sr.GetEngine("py");
            ScriptScope futureScope = pyeng.CreateScope();
            futureScope.SetVariable("division", true);
            sr.Globals.SetVariable("__future__", futureScope);
            ScriptSource source = pyeng.CreateScriptSourceFromString(testSrc, SourceCodeKind.Statements);
            
            object result = source.Execute(futureScope);
            object r2 = futureScope.GetVariable("r");
            Assert.AreEqual((double)r2, 0.5);

        }

        /// <summary>
        /// Test     : Trying to access an unmodified global environment module
        /// Expected : Should throw ImportException 
        /// 
        /// Note     : Might be testing IPY?
        /// </summary>
        [TestMethod]
        [Negative]
        [ExpectedException(typeof(IronPython.Runtime.Exceptions.ImportException))]
        //[Ignore]//this test is useless from HAPI perspective. Should be deleted altogether
        public void TestFromFuture_RepoFailure() {

            // Get future import code
            string testSrc = _codeSnippets[CodeType.ImportFutureDiv];
            ScriptRuntime sr = CreateRuntime();

            ScriptScope globalScope = sr.CreateScope();

            // Set __future__ module with value in Script Runtime associated
            // with this engine
            globalScope.SetVariable("division", true);
            sr.Globals.SetVariable("__future__", globalScope);
            

            ScriptEngine pyeng = sr.GetEngine("py");

            // Exception is cause by this helper function CreateScriptSourceFRomTempFile 
            // since this method uses the default _PYEng who's env has not been modified 
            // by the module being set in the local env.
            ScriptSource source = CreateScriptSourceFromTempFile(testSrc, Encoding.ASCII);

            ScriptScope localScope = pyeng.CreateScope();

            object result = source.Execute(localScope);
            //object result = source.Execute(globalScope);

            // Wouldn't expect global scope to se local "r" value! unless
            // globalScope is used in the Execute statement
            object r2 = globalScope.GetVariable("r");
            //object r2 = localScope.GetVariable("r");
            Assert.AreEqual((double)r2, 0.5);

        }

        [TestMethod]
        public void TestFromFuture_globalScope() {

            //string testSrc = "from __future__ import division\nr= 1/2\n";
            string testSrc = _codeSnippets[CodeType.ImportFutureDiv];
            ScriptRuntime sr = _runtime;
            ScriptScope globalScope = sr.CreateScope();
            ScriptEngine pyeng = sr.GetEngine("py");
            //ScriptScope futureScope = pyeng.CreateScope();
            //futureScope.SetVariable("division", true);
            globalScope.SetVariable("division", true);
            sr.Globals.SetVariable("__future__", globalScope);
            
            ScriptSource source = pyeng.CreateScriptSourceFromString(testSrc, SourceCodeKind.Statements);
            //ScriptScope globalScope = sr.CreateScope();

            object result = source.Execute(globalScope);
            object r2 = globalScope.GetVariable("r");
            Assert.AreEqual((double)r2, 0.5);

        }


        
        /// <summary>
        /// Make sure this throws the Null Argument Exception
        /// </summary>
        [TestMethod]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetScope_NullArg_Test() {
            ScriptScope scope = _testEng.GetScope(null);
        }

        /// <summary>
        /// Give Invalid Path - does this test need to be used
        /// with PAL path?
        /// </summary>
        [TestMethod]
        public void GetScope_InvalidPath() {
            string wrongPath = "~//root/blah/blah";
            Assert.IsNull(_testEng.GetScope(wrongPath));
            Assert.IsNull(_RBEng.GetScope(wrongPath));
        }

        /// <summary>
        /// A ScriptSource Needs a valid path?
        /// </summary>
        [TestMethod]
        public void GetScope_NonExistentScriptSourcePath() {
                    
            //As long as the engine's scope is not associated with
            //a scope that has a valid path this should work or
            //If you just give an invalide script path.

            // Probably dont' need each of these tests since 
            // they invalid path's are probably tested else where!

            string[] wrongPath = {"c:\\lsslsl\\slslslsl\\lslslsa/a/a/ssswww/foo.py",
                                     "c:\\tmp\\foo.py"};
            foreach (string i in wrongPath) {
                Assert.IsNull(_testEng.GetScope(i), "This should always return null");
            }
        }

        /// <summary>
        /// Need to ask shes where we can place this script file?
        /// </summary>
        [TestMethod]
        public void GetScope_ValidPathOfUnExecutedScript() {

            //Create a temp source with some code
            string testSrc = "from math import sqrt\nx=3\ny=5\n";
            testSrc += "c=sqrt(x*x+y*y)\n";
            testSrc += "c";

            //Create a temp source with some code
            ScriptSource sSrc = CreateScriptSourceFromTempFile(testSrc, Encoding.ASCII);
            Assert.IsNull(_testEng.GetScope(sSrc.Path));
        }

        /// <summary>
        /// Test case :
        ///    In a supported language, the valid path of an existing ScriptSource that has been executed	
        /// Expected result:
        ///    Return of the ScriptScope in which the given ScriptSource most recently ran
        ///    
        /// </summary>
        [TestMethod()]
        public void GetScope_VerifyScriptScopeFromScriptFileCanBeRetrieved(){

            // Create a temp source with some simple code that adds 1+2 to the var 'x'
            string testSrc = _codeSnippets[CodeType.OneLineAssignmentStatement];
            // The expected result from the above code
            object expResult = 3;

            // Create a temp python file.
            String tmpFilePath = TestHelpers.CreateTempSourceFile(testSrc, ".py");

            ScriptSource source = _testEng.CreateScriptSourceFromFile(tmpFilePath);
            ScriptScope scope = _testEng.CreateScope();

            // Execute the file source
            //object excResult = source.Execute();
            object excResult = source.Execute(scope);

            // A couple of sanity checks
            //Assert.AreEqual(scope.GetVariable("x"), expResult);
            Assert.IsTrue(File.Exists(tmpFilePath));

            // Get the scope associated with the current engine from the existing file script
            ScriptScope fileScope = _testEng.GetScope(tmpFilePath);

            // We should have a valid instance of the scope
            Assert.IsNotNull(fileScope);

            // If we get a valid scope check the expected contents. 
            Assert.AreEqual(expResult, fileScope.GetVariable("x"));
        }

        /// <summary>
        /// Valid path to a junk file.
        /// --- What about different encoding -- maybe this is testing DOT net.
        /// </summary>
        [TestMethod]
        public void CreateScriptSourceFromFile_ValidPathToJunkFile() {
            
            //Create a temp source with some invalid code
            string testSrc = "Moby-Dick[1] is an 1851 novel by Herman Melville. The story"
                            + " tells the adventures of the wandering sailor Ishmael and his"
                            + " voyage on the whaling ship Pequod, commanded by Captain "
                            + " Ahab. Ishmael soon learns that Ahab does not mean to use the"
                            + " Pequod and her crew to hunt whales for market trade, as whaling";

            // Much of this code is tested in the helper function CreateScriptSourceFromTempFile!!!
            ScriptSource sSrc = CreateScriptSourceFromTempFile(testSrc, Encoding.ASCII);

            ScriptSource sSrcTest = _testEng.CreateScriptSourceFromFile(sSrc.Path);
            ScriptCodeParseResult SrcCodeProp = sSrcTest.GetCodeProperties();
            bool result = SrcCodeProp.Equals(ScriptCodeParseResult.Invalid);
            Assert.IsTrue(result, String.Format("Is this code invalid {0}", result));


        }

        /// <summary>
        /// Test to ensure the CreateScriptSource doesnt throw any exception irrespective 
        /// of how bad the input path is...
        /// </summary>
        [TestMethod]
        public void CreateScriptSourceFromFile_NoExceptions() {

            foreach (string path in StandardTestPaths.AllPaths) {
                _testEng.CreateScriptSourceFromFile(path);
            }
                        
        }

        /// <summary>
        /// Blast the method with Junk Strings As Bad Code
        /// </summary>
        [TestMethod]
        public void CreateScriptSourceFromString_JunkStringsTest() {
            int cnt = 0;
            ScriptSource sSrc;
            ScriptCodeParseResult sCodeProp = new ScriptCodeParseResult();

            foreach (string junk in StandardTestStrings.AllStrings) {
                // Count the loop sanity check!
                cnt++;
                string msg = string.Format("junk string index {0}", cnt);
                Assert.IsNotNull(sSrc = _testEng.CreateScriptSourceFromString(junk));
                Assert.IsNotNull(sCodeProp = sSrc.GetCodeProperties(), msg);
                Assert.IsTrue(sCodeProp.Equals(ScriptCodeParseResult.Invalid), msg);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void CreateScriptSourceFromFile_JunkStringsTest() {
            

            int cnt = 0;
            ScriptSource sSrc;
            ScriptCodeParseResult sCodeProp = new ScriptCodeParseResult();

            foreach (string junk in StandardTestStrings.AllStrings) {
            
                // Count the loop sanity check!
                cnt++;
                string msg = string.Format("junk string index {0}", cnt);
                Assert.IsNotNull(sSrc = CreateScriptSourceFromTempFile(junk, Encoding.ASCII), msg);
                Assert.IsNotNull(sCodeProp = sSrc.GetCodeProperties(), msg);
                Assert.IsTrue(sCodeProp.Equals(ScriptCodeParseResult.Invalid), msg);


            }

        }


        /// <summary>
        /// Even if the CodeDOM created is completely wrong - this should still not throw an exception?
        /// Expecting CodeMemberMethod
        /// I am not sure why you think the method shouldn't through when the codedom is wrong.
        /// Anyway, we should probably have a very basic sanity test for this and not test too much
        /// Let's discuss about this test
        /// 
        /// CodeObject and id are passed up the chain of function until they hit ScriptSource(...)
        /// </summary>
        
        [TestMethod]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        //public void CreateScriptSource_NullIdValidCodeObject() {
        public void CreateScriptSource_NullCodeObject() {
            
            //Why is this called 'BuildCountCode'?
            CodeObject cObj = BuildCountCode();
            // If we return just the method from BuildCountCode()
            // CreateScriptSource will not throw an exception
            ScriptSource ss = _testEng.CreateScriptSource(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScriptSource() {
            _testEng.CreateScriptSourceFromString(null, "id", SourceCodeKind.Statements);
        }

        /// <summary>
        /// Given a null scope CreateScope should throw an ArgumentNullException
        /// </summary>
        [TestMethod]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScope_NullGlobals() {
            _testEng.CreateScope((IDynamicMetaObjectProvider)null);
        }

        [TestMethod]
        [Ignore]// BUG # 446911
        public void SourceSearchPaths_SpecificPathsCheck() {
            //_testEng.SetSearchPaths
            Assert.Fail("Missing Property definition");
        }


        [TestMethod]
        public void LanguageVersion_Call() {
            Assert.IsNotNull(_testEng.LanguageVersion);
        }
    }
}
