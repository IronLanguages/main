using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;

namespace HostingTest {
    public partial class ScriptEngineTest : HAPITestBase {

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


        public ScriptEngineTest()
            : base() {

            _tempFilesToDelete = new List<string>();
        }



        List<string> _tempFilesToDelete;
        private ScriptSource CreateScriptSourceFromTempFile(string code, Encoding encoding) {
            string temp1 = Path.GetTempFileName();
            string filePath = Path.ChangeExtension(temp1, ".py");
            File.WriteAllText(filePath, code, encoding);

            _tempFilesToDelete.Add(filePath);

            return _testEng.CreateScriptSourceFromFile(filePath, encoding);
        }

        private void DeleteTempFiles() {
            foreach (string s in _tempFilesToDelete)
                File.Delete(s);

            _tempFilesToDelete.Clear();
        }

        private List<ScriptSource> CreateSourceListWithDifferentEncodings(string code) {
            List<ScriptSource> sources = new List<ScriptSource>();
            sources.Add(CreateScriptSourceFromTempFile(code, Encoding.Default));
            sources.Add(CreateScriptSourceFromTempFile(code, Encoding.Unicode));
            sources.Add(CreateScriptSourceFromTempFile(code, Encoding.UTF32));
            sources.Add(CreateScriptSourceFromTempFile(code, Encoding.UTF8));
            sources.Add(CreateScriptSourceFromTempFile(code, Encoding.Unicode));
            sources.Add(CreateScriptSourceFromTempFile(code, Encoding.BigEndianUnicode));

            //CreateScriptSourceFromFile positive cases
            //@TODO - FromFile with a Kind other than File

            //CreateScriptSourceFromString positive cases
            sources.Add(_testEng.CreateScriptSourceFromString(code, SourceCodeKind.Statements));
            sources.Add(_testEng.CreateScriptSourceFromString(code, "customId", SourceCodeKind.Statements));

            return sources;
        }



        /// <summary>
        /// This is a simple helper method to create a test valid
        /// CodeObject to be feed into the 
        ///     CreateScriptSource(CodeObject content, string id)
        ///     
        /// However, CodeObject is simply a base class so we probably
        /// need a specific type of CodeObject but what?
        /// 
        /// [Bill - has indicate that CodeObject parameter for CreateScriptSource
        ///  does in fact need to be a CodeMemberMethod - Maybe a spec BUG]
        /// 
        /// Probably need to put this somewhere else maybe put this in:
        ///     ScriptEngineTestHelper.cs
        /// </summary>
        /// <returns>A valid CodeObject boxes some kind of CompileUnit</returns>
        private static CodeObject BuildCountCode() {
            // Create a new CodeCompileUnit to contain 
            // the program graph.
            CodeCompileUnit compileUnit = new CodeCompileUnit();

            // Declare a new namespace called Samples.
            CodeNamespace samples = new CodeNamespace("Samples");
            // Add the new namespace to the compile unit.
            compileUnit.Namespaces.Add(samples);

            // Declare a new code entry point method.
            CodeEntryPointMethod start = new CodeEntryPointMethod();

            CodeConstructor codecon = new CodeConstructor();

            //
            CodeNamespace cns = new CodeNamespace("Test");
            CodeTypeDeclaration ctd = new CodeTypeDeclaration("testclass");
            ctd.IsClass = true;

            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            method.Name = "AddTenToNumber";
            method.ReturnType = new CodeTypeReference("Int32");
            method.Parameters.Add(new CodeParameterDeclarationExpression("int", "number"));
            method.Statements.Add(new CodeSnippetExpression("return number+10"));
            ctd.Members.Add(method);

            samples.Types.Add(ctd);
            // If we return just the method this will not throw exception
            // on CreateScriptSource
            // return method;
            return compileUnit;

        }
    }

    internal static class ScriptEngineExtensions {
        internal static bool IsValidPythonEngine(this ScriptEngine eng) {
            ScriptScope scope = eng.CreateScope();
            ScriptSource code = eng.CreateScriptSourceFromString("five=2+3", Microsoft.Scripting.SourceCodeKind.Statements);
            
            code.Execute(scope); 
            return (int)scope.GetVariable("five") == 5;
        }
    }

}
