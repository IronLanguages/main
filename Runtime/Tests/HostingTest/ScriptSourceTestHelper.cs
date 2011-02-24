using System;
using System.IO;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;
using Microsoft.Scripting.Runtime;


namespace HostingTest {
    public partial class ScriptSourceTest {

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        public ScriptSourceTest(): base(){
        }

        private string CreateTempFile(string contents) {
            string tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, contents);

            return tempFile;
        }

        internal ScriptSource CreateFileBasedScriptSource(string pythonCode) {
            string fileName = CreateTempFile(pythonCode);
            return _testEng.CreateScriptSourceFromFile(fileName);
        }

        internal ScriptSource CreateStringBasedScriptSource(string code) {
            return _testEng.CreateScriptSourceFromString(code);
        }
        
        internal ScriptSource CreateScriptSource(string code) {
            return CreateStringBasedScriptSource(code);
        }

        internal ScriptSource CreateScriptSource(string code, SourceCodeKind inputKind) {
            return _testEng.CreateScriptSourceFromString(code, inputKind);
        }


        #region ValidateMethods - each of the following methods test a specific member
        /// <summary>
        /// validates the 'Kind' property
        /// </summary>
        /// <param name="InputCode">Code to create ScriptSource object from</param>
        /// <param name="sourceCodeKind">The value with which the enum should be verified against</param>
        internal void ValidateKind(string InputCode, SourceCodeKind input, SourceCodeKind ExpectedValue) {
            ScriptSource ss = CreateScriptSource(InputCode, input);
            
            Assert.AreEqual(ss.Kind, ExpectedValue);
        }

        internal void ValidateKindAsFile(string InputCode) {
            ScriptSource ss = CreateFileBasedScriptSource(InputCode);
            Assert.AreEqual(ss.Kind, SourceCodeKind.File);
        }

        internal void ValidateGetCodeProperties(string InputCode, ScriptCodeParseResult ExpectedValue) {
            ScriptSource ss = CreateStringBasedScriptSource(InputCode);
            Assert.AreEqual( ExpectedValue, ss.GetCodeProperties());

            //test the same method on a file based ScriptSource object
            ss = CreateFileBasedScriptSource(InputCode);
            Assert.AreEqual( ExpectedValue, ss.GetCodeProperties());
        }

        public void ValidateEngine(string inputCode) {
            ScriptSource ss = CreateStringBasedScriptSource(inputCode);
            Assert.AreEqual( _testEng, ss.Engine );

            //test the same method on a file based ScriptSource object
            ss = CreateFileBasedScriptSource(inputCode);
            Assert.AreEqual( _testEng, ss.Engine);
        }

        internal void ValidateGetCode(string inputCode) {
            ScriptSource ss = CreateStringBasedScriptSource(inputCode);
            Assert.AreEqual(inputCode, ss.GetCode());

            //test the same method on a file based ScriptSource object
            ss = CreateFileBasedScriptSource(inputCode);
            Assert.AreEqual( inputCode, ss.GetCode());
        }

        internal void ValidateGetCodeLine(string inputCode, int lineIndex, string expectedOutput) {
            //int LineIndex = -1;

            //check for valid code snippet
            ScriptSource ss = CreateScriptSource(inputCode);
            string temp1 = ss.GetCodeLine(lineIndex);

            //  string temp23 = ss.GetCodeLine(100098797982342342342349867);
            Assert.AreEqual(expectedOutput, temp1);

        }

        internal void ValidateGetCodeLineNegative(string inputCode, int lineIndex, Exception expectedAssertion) {

            ScriptSource ss = CreateScriptSource(inputCode);

            bool exceptionThrown = false;
            try {
                string temp1 = ss.GetCodeLine(lineIndex);
            }
            catch (Exception ex) {
                Assert.AreEqual(expectedAssertion, ex);
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown);

        }

        internal void ValidateGetCodeLinesNegative(string inputCode, int startIndex, int count, Exception expectedAssertion) {
            ScriptSource ss = CreateScriptSource(inputCode);

            bool exceptionThrown = false;
            try {
                string[] temp1 = ss.GetCodeLines(startIndex, count);
            }
            catch (Exception ex) {
                Assert.AreEqual(expectedAssertion, ex);
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown);
        }

        internal void ValidateGetCodeLines(string statements, int startIndex, int count, string[] expectedOutput) {
            ScriptSource source = _testEng.CreateScriptSourceFromString(statements, SourceCodeKind.Statements);
            string[] actual = source.GetCodeLines(startIndex, count);

            TestHelpers.AreEqualArrays(expectedOutput, actual);
        }

        private void ValidateExecute( string expression, int expResult) {
            ScriptSource source = _testEng.CreateScriptSourceFromString(expression,
                                                                     SourceCodeKind.Expression);
            
            // Execute source using scope and capture result
            object testResult = source.Execute(_testEng.CreateScope());
            // test result against expected result
            Assert.AreEqual(expResult, (int)testResult);
        }

        private void ValidateExecute(ScriptScopeDictionary env, string expression, int expResult) {
            ScriptSource source = _testEng.CreateScriptSourceFromString(expression,
                                                                     SourceCodeKind.Expression);
            // Create scope
            ScriptScope scope = _testEng.CreateScope(new ObjectDictionaryExpando(env));

            // Execute source using scope and capture result
            object testResult = source.Execute(scope);
            // test result against expected result
            Assert.AreEqual(expResult, (int)testResult);
        }

        private void ValidateExecute(ScriptScope scope, string expression, int expResult) {
            ScriptSource source = _testEng.CreateScriptSourceFromString(expression,
                                                                     SourceCodeKind.Expression);
            
            // Execute source using scope and capture result
            object testResult = source.Execute(scope);

            // test result against expected result
            Assert.AreEqual(expResult, (int)testResult);
        }

        private void ValidateExecute(ScriptScope scope, string statements) {
            ScriptSource source = _testEng.CreateScriptSourceFromString(statements,
                                                                     SourceCodeKind.Expression);

            // Execute source using scope and capture result
            object testResult = source.Execute(scope);

            // test result against expected result
            Assert.IsNull(testResult);
        }

        private void ValidateExecuteProgram(string expression, int expectedResult, SourceCodeKind kind) {
            // Create Script source
            ScriptSource source = _testEng.CreateScriptSourceFromString(expression, kind);

            // Call the ExecuteProgram code and check the exit code
            int exitCode = source.ExecuteProgram();
            Assert.AreEqual(expectedResult, exitCode);
        }

     

        #endregion

    }
     
}
