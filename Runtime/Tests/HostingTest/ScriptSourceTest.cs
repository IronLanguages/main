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
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;

#if SILVERLIGHT
using Microsoft.Silverlight.TestHostCritical;
#endif

namespace HostingTest {
    using Assert = NUnit.Framework.Assert;
 
    [TestFixture]
    public partial class ScriptSourceTest : HAPITestBase {
        
        [Test]
        public void ScriptEngine_CreateScriptSource1() {
        }

        [Test]
        public void ScriptSource_Kind() {

            ValidateKind(_codeSnippets[CodeType.Valid1],SourceCodeKind.Statements, SourceCodeKind.Statements);
            ValidateKind(_codeSnippets[CodeType.ValidExpression1], SourceCodeKind.Expression, SourceCodeKind.Expression);
            ValidateKind(_codeSnippets[CodeType.Interactive1], SourceCodeKind.InteractiveCode, SourceCodeKind.InteractiveCode);
            ValidateKind(_codeSnippets[CodeType.ValidStatement1], SourceCodeKind.SingleStatement, SourceCodeKind.SingleStatement);
            ValidateKind(_codeSnippets[CodeType.ValidExpression1], SourceCodeKind.AutoDetect, SourceCodeKind.AutoDetect);
            ValidateKind(_codeSnippets[CodeType.ValidStatement1], SourceCodeKind.AutoDetect, SourceCodeKind.AutoDetect);
            ValidateKind(_codeSnippets[CodeType.Valid1], SourceCodeKind.AutoDetect, SourceCodeKind.AutoDetect);
            ValidateKindAsFile(_codeSnippets[CodeType.Valid1]);
        }

        [Test]
        [Ignore()]//Bug # 484856
        public void ScriptSource_GetCodeProperties() {
            ValidateGetCodeProperties( _codeSnippets[CodeType.Valid1], ScriptCodeParseResult.Complete);
            ValidateGetCodeProperties( _codeSnippets[CodeType.InCompleteStatement1], ScriptCodeParseResult.IncompleteStatement);
            ValidateGetCodeProperties( _codeSnippets[CodeType.InCompleteExpression1], ScriptCodeParseResult.IncompleteToken);
            ValidateGetCodeProperties( _codeSnippets[CodeType.Junk], ScriptCodeParseResult.Invalid);
            ValidateGetCodeProperties( _codeSnippets[CodeType.Junk], ScriptCodeParseResult.Invalid);
            ValidateGetCodeProperties( _codeSnippets[CodeType.Comment], ScriptCodeParseResult.Invalid);
            ValidateGetCodeProperties( _codeSnippets[CodeType.WhiteSpace1], ScriptCodeParseResult.Invalid);
        }

        
        
        [Test]
        public void ScriptSource_Engine() {
            foreach (CodeSnippet cs in _codeSnippets.AllSnippets) {
                if (cs.Code != null)
                    ValidateEngine(cs.Code);
            }
        }

        [Test]
        public void ScriptSource_GetCode() {
            foreach (CodeSnippet cs in _codeSnippets.AllSnippets) {
                if (cs.Code != null)
                    ValidateGetCode(cs.Code);
            }
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentException))]
        public void GetCodeLine_NegativeOutOfBoundsOfActualLineNumbers()
        {
            ScriptSource sSrc = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.OneLineAssignmentStatement], 
                                                                SourceCodeKind.SingleStatement);
            //throws
            sSrc.GetCodeLine(-1);
        }
        
        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentException))]
        public void GetCodeLine_GetZeroBasedCodeLine() {
            ScriptSource sSrc = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.OneLineAssignmentStatement],
                                                                SourceCodeKind.SingleStatement);
            // This should throw an exception as define by spec.
            sSrc.GetCodeLine(0);
        }

        [Test]
        public void GetCodeLine_GetBeyondCodeLine()
        {
            ScriptSource sSrc = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.OneLineAssignmentStatement],
                                                                SourceCodeKind.SingleStatement);
            // This should NOT throw an exception as define by spec - should be able to check beyond actual lines.
            int beyondActualCode = 100;
            sSrc.GetCodeLine(beyondActualCode);
        }

        [Test]
        public void GetCodeLine_GetValidCodeLine()
        {
            ScriptSource sSrc = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.OneLineAssignmentStatement],
                                                                SourceCodeKind.SingleStatement);
            // This should the line of code input during CreateScriptSourceFromString
            int actualCode = 1;
            string codeLine = sSrc.GetCodeLine(actualCode);
            Assert.AreEqual(codeLine, _codeSnippets[CodeType.OneLineAssignmentStatement]);
        }
        
        /// <summary>
        /// Verify that the \r is valid LinefeedTerminator in a string.
        /// </summary>
        [Test]
        public void GetCodeLine_GetValidCodeLineWithLinefeedTerminatorR()
        {
            //Source example using \r
            string strTestSrc = "x =  1+2\ry= 3+4";
            string strExpectedTestLine = "y= 3+4";
            // The second line of code input during CreateScriptSourceFromString 
            // if '\r' is a valid LinefeedTerminator
            int expectedCodeLine = 2;
            
            ScriptSource sSrc = _testEng.CreateScriptSourceFromString(strTestSrc,
                                                                SourceCodeKind.Statements);
            // Get the second line 
            string codeLine = sSrc.GetCodeLine(expectedCodeLine);
            // Check the expected value
            Assert.AreEqual(codeLine, strExpectedTestLine);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetCodeLines_NegativeIndex1() {
            ValidateGetCodeLines(_codeSnippets[CodeType.SevenLinesOfAssignemtStatements], -1, 0, null);
            ValidateGetCodeLines(_codeSnippets[CodeType.SevenLinesOfAssignemtStatements], -2, 0, null);
            ValidateGetCodeLines(_codeSnippets[CodeType.SevenLinesOfAssignemtStatements], -2, 4, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetCodeLines_NegativeIndex2() {
            ValidateGetCodeLines(_codeSnippets[CodeType.SevenLinesOfAssignemtStatements], 2, -1, null);
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentException))]
        public void GetCodeLines_NegativeIndex3() {
            ValidateGetCodeLines(_codeSnippets[CodeType.SevenLinesOfAssignemtStatements], 0, 4, null);
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentException))]
        public void GetCodeLines_ZeroIndex4() {
            ValidateGetCodeLines(_codeSnippets[CodeType.SevenLinesOfAssignemtStatements], 4, 0, null);
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentException))]
        public void GetCodeLines_NegativeCountArg() {
            ValidateGetCodeLines(_codeSnippets[CodeType.SevenLinesOfAssignemtStatements], 1, -1, null);
        }


        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetCodeLines_ZeroIndex() {
            ValidateGetCodeLines(_codeSnippets[CodeType.SevenLinesOfAssignemtStatements], 0, 0, null);
        }

        [Test]
        public void GetCodeLines_Basic1() {
            string input = _codeSnippets[CodeType.SevenLinesOfAssignemtStatements];
            string[] expected = input.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            ValidateGetCodeLines(input, 1, expected.Length, expected);
        }

        [Test]
        public void GetCodeLines_Basic2() {
            string input = _codeSnippets[CodeType.SevenLinesOfAssignemtStatements];
            string[] expected = input.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            ValidateGetCodeLines(input, 1, 100, expected);
        }

        [Test]
        public void GetCodeLines_Basic3() {
            string input = _codeSnippets[CodeType.SevenLinesOfAssignemtStatements];
            string[] temp = input.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            string[] expected = new[] { temp[2], temp[3] };
            ValidateGetCodeLines(input, 3, 2, expected);
        }

        [Test]
        public void GetCodeLines_Basic4() {
            string input = _codeSnippets[CodeType.SevenLinesOfAssignemtStatements];
            string[] temp = input.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            ValidateGetCodeLines(input, temp.Length, 1, new string[] { temp[6] });
        }

        [Test]
        public void GetCodeLines_GetCodeLinesInRangeFromFile()
        {
            string sourceInput = _codeSnippets[CodeType.SevenLinesOfAssignemtStatements];
            

            string scriptName = TestHelpers.CreateTempSourceFile(sourceInput, ".py");
            ScriptSource source = _testEng.CreateScriptSourceFromFile(scriptName);

            string[] expected = sourceInput.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            ValidateGetCodeLines(sourceInput, 1, expected.Length, expected);
        }

        [Test]
        public void Path_CheckDefaultValueIsNull()
        {
            ScriptSource source = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.OneLineAssignmentStatement],
                                                                SourceCodeKind.SingleStatement);
            Assert.IsNull(source.Path);
        }

        [Test]
        [Ignore()]//Bug 464777 - the test actually succeeds, but we want to check for the path->scope mapping 
        //and that is blocked by this bug
        public void Path_ExplicitSetDuringConstruction()
        {
            string path = "UniquePath1";
            ScriptSource src = _testEng.CreateScriptSourceFromString(
                                    _codeSnippets[CodeType.SimpleExpressionOnePlusOne], 
                                    path);
            Assert.AreEqual(path, src.Path);

            var scope = _testEng.CreateScope();
            src.Execute(scope);

            var newScope = _testEng.GetScope(src.Path);

            Assert.AreEqual(scope, newScope);
        }

        [Test]
        public void Path_ExplicitSetDuringConstructionFromFile()
        {
            string srcPath = TestHelpers.CreateTempSourceFile(_codeSnippets[CodeType.ValidMultiLineMixedType], ".py");
            ScriptSource source = _testEng.CreateScriptSourceFromFile(srcPath);
            Assert.AreEqual(source.Path, srcPath, "'The Path is null if not set explicitly on construction.'");
        }

        [Test]
        public void Path_NotExplicitlySet1()
        {
            ScriptSource source = _testEng.CreateScriptSourceFromString( _codeSnippets[CodeType.ValidStatement1],
                                                           SourceCodeKind.Statements);
            Assert.IsNull(source.Path);
        }

        [Test]
        public void Path_NotExplicitlySet2()
        {
            ScriptSource source = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.ValidExpressionWithMethodCalls],
                                                                      SourceCodeKind.Expression);
            Assert.IsNull(source.Path);
        }

        [Test]
        public void Path_NotExplicitlySet3()
        {
            ScriptSource source = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.ValidExpressionWithMethodCalls]);
            Assert.IsNull(source.Path);
        }

        [Test]
        public void Path_NotExplicitlySet4()
        {
            ScriptSource source = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.SimpleExpressionOnePlusOne],
                                                                      SourceCodeKind.SingleStatement);
            Assert.IsNull(source.Path);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Compile_Invoke()
        {
            string singleExp = _codeSnippets[CodeType.SimpleExpressionOnePlusOne];
            int expectedResult = 2;
            ScriptSource source = _testEng.CreateScriptSourceFromString(singleExp,
                                                                      SourceCodeKind.Expression);
            CompiledCode ccode = source.Compile();
            object result = ccode.Execute();
            Assert.AreEqual(expectedResult, (int)result);
        }


        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Compile_ArgNullCompilerOptions()
        {
            ScriptSource source = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.SimpleExpressionOnePlusOne],
                                                                      SourceCodeKind.Expression);
            CompiledCode ccode = source.Compile((CompilerOptions)null);
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Compile_ArgNullErrorSink()
        {

            // BUG - ErrorSink is currently undefined
            string singleExp = _codeSnippets[CodeType.SimpleExpressionOnePlusOne];
            ScriptSource source = _testEng.CreateScriptSourceFromString(singleExp,
                                                                      SourceCodeKind.Expression);
            source.Compile((ErrorListener)null);
            //CompiledCode ccode = source.Compile((ErrorListener)null);
            //Assert.Fail("ErrorSink is currently undefined");
        }

        [Ignore]//Bug # 450336
        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Compile_ArgNullOpAndErrorSink()
        {

            // BUG - ErrorSink is currently undefined
            string singleExp = _codeSnippets[CodeType.SimpleExpressionOnePlusOne];
            ScriptSource source = _testEng.CreateScriptSourceFromString(singleExp,
                                                                      SourceCodeKind.Expression);
            //CompiledCode ccode = source.Compile((ErrorListener)null);
            Assert.Inconclusive("ErrorSink is not imeplementedundefined can not run this test");
        }

        [Test]
        [Ignore] //blocked by bug #450125
        public void Compile_InvokeWithOptions()
        {
            // BUG - CompiledCode is only a stub currently.

            string singleExp = _codeSnippets[CodeType.SimpleExpressionOnePlusOne];
            ScriptSource source = _testEng.CreateScriptSourceFromString(singleExp,
                                                                      SourceCodeKind.Expression);
            // This is only a stub - I think.
            CompilerOptions options = new CompilerOptions();
            
            CompiledCode ccode = source.Compile(options);
            object results = ccode.Execute();
            Assert.Fail("This test is block by the missing CompiledCode class");
        }

        /// <summary>
        /// Test Case:	
        /// Multiple invocation of the same call	
        /// Verification:
        /// Same correct value is returned 
        /// </summary>
        [Test]
        public void Execute_MultipleInvocation()
        {
            string singleExp = _codeSnippets[CodeType.SimpleExpressionOnePlusOne];
            int expResult = 2;

            ScriptSource source = _testEng.CreateScriptSourceFromString(singleExp,
                                        SourceCodeKind.Expression);
            int testRuns = 10;
            object result;

            for (int i = 0; i < testRuns; i++)
            {
                result = source.Execute(_testEng.CreateScope());
                Assert.AreEqual(expResult, (int)result, "Test Multiple Calls to Execute");
            }
        }

        /// <summary>
        /// Test Case:	
        ///   Multiple invocation of the same call doesn't return reference to the same object
        ///   We need a method that would return a reference type. Simple ints and strings wont cut it.
        /// Verification:
        ///   Different objects and not references to the same object
        /// </summary>
        [Test]
        public void Execute_MultipleInvocationValidDifferentObject()
        {
            //setup code in python such that it returns a reference type

            //invoke the function multiple times

            //ensure all of the returns are different objects and not references to the same type.

            // TODO : Finish this test
        }

        [Test]
        public void Execute_CallingPythonMethod()
        {
            ScriptScope scope = _testEng.CreateScope();

            // Execute source in scope 
            ScriptSource source = scope.Engine.CreateScriptSourceFromString(_codeSnippets[CodeType.IsOddFunction], Microsoft.Scripting.SourceCodeKind.Statements);
            source.Execute(scope);

            //Get a Delegate Instance using the Func<> Generic declaration and GetVariable
            Func<int, bool> isodd = scope.GetVariable<Func<int, bool>>("isodd");

            // Call the function to validate IsCallable
            Assert.IsTrue(isodd(1));
            Assert.IsFalse(isodd(2));

        }
        
        /// <summary>
        /// Test Case:	
        ///   Python defines a method and we call it from C# multiple times
        /// Verification:
        ///   Validate that a defined method is called.
        ///   
        /// This example is more of a ScriptScope example
        /// </summary>
        [Ignore]// Bug #466321
        [Test]
        public void Execute_SingleInvocationMultipleCallsOfLoadedFn()
        {
            // Setup tests simple result of rot13 on string and it's result
            //string testInput = "shone";
            //string expResult = "";
//            int testRuns = 1; 
            
            // load script
            ScriptSource source = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.Rot13Function],
                                                                     SourceCodeKind.Statements);
            // Create scope
            ScriptScope scope = _testEng.CreateScope();

            // Execute for this scope (return val is null since code is statement)
            source.Execute(scope);
            
            // From scope GetVariables<T> using predefined generic delegate
            Func<string, string> rot13 = scope.GetVariable<Func<string, string>>("rot13");

            // result of function call
//            string fnResult;
////            string newTestInput, newExpResult;
//            for (int i = 0; i < testRuns; i++)
//            {
//                // call function defined by python script over and over and 
                // verify that the new result is different.
                //newTestInput = string.Format(testInput + "{0}", i);
                //newExpResult = string.Format(expResult + "{0}", i);
                
                string fnResult = rot13("newTestInput");
                
                // check the results.
                //Assert.AreEqual(fnResult, newExpResult);
            //}
        }

        /// <summary>
        /// Test Case:	
        ///   Multiple invocation of the same call
        /// Verification:
        ///   Validate that a defined method is called.
        ///   
        /// </summary>
        //with a different python code (or removed altogether)
        [Test]
        public void Execute_MultipleInvocationValidDefinedMethodIsCalled()
        {
            // Call validation method multiple times.
            for (int i = 0; i < 5; i++)
                ValidateExecute("abs(-1)", 1);
        }

        
        /// <summary>
        /// Load a python script that has a valid class and methods.
        /// This tests that we can get access to these function and call them
        /// from within C#. 
        /// 
        /// Case 1 : calling a function defined in C#
        /// </summary>
        [Test]
        public void Execute_CallingPythonObjectMethodWrappedInAFun()
        {
            ScriptSource source = _testEng.CreateScriptSourceFromString(
                            _codeSnippets[CodeType.SimpleFooClassDefinition],
                            SourceCodeKind.Statements);
            
            ScriptScope scope = _testEng.CreateScope();
            source.Execute(scope);

            Func<string> sayHello = scope.GetVariable<Func<string>>("bar");
            Assert.AreEqual(sayHello(), "Hello World");
        }

        /// <summary>
        /// Load a python script that has a valid class and methods.
        /// This tests that we can get access to these function and call them
        /// from within C#. 
        /// 
        /// Case 2 : calling an object that has been created of type FooClass and
        ///          calling it's member function. 
        /// </summary>
        [Test]
        public void Execute_CallingInstanceMethodDefinedInAPythonObject() 
        {
            // Setup tests
            string testSrc = _codeSnippets[CodeType.SimpleFooClassDefinition];
            string expResult = "Hello World";

            // load script with bar() function def
            ScriptSource source = _testEng.CreateScriptSourceFromString(testSrc, SourceCodeKind.Statements);
            // Create scope
            ScriptScope scope = _testEng.CreateScope();

            source.Execute(scope);

            object fooTest = scope.GetVariable("fooTest");
            Func<string> sayHello = _testEng.Operations.GetMember<Func<string>>(fooTest, "f");

            // Now call fooTest's object member function 'f'
            string result = sayHello();
            Assert.AreEqual(result, expResult);
            
        }
        /// <summary>
        /// Test   : Load a python script that has a valid class and method(s).
        ///          This tests that we can get access to these function and call them
        ///          from within C#. 
        /// 
        /// Result : Successfully, calling the FooClass member from C#.
        /// </summary>
        [Test]
        public void Execute_CallingPythonClassMemberMethod()
        {
            // Setup tests
            string testSrc = _codeSnippets[CodeType.SimpleFooClassDefinition];
            string expResult = "Hello World";
            ScriptSource source = _testEng.CreateScriptSourceFromString(testSrc,
                                                                     SourceCodeKind.Statements);
            
            ScriptScope scope = _testEng.CreateScope();

            // Execute for this scope (return val is null since code is statement)
            source.Execute(scope);
            
            object FooClass = scope.GetVariable("FooClass");
            object fooTest = _testEng.Operations.Invoke(FooClass); // create new FooClass
            Func<object, string> sayHello = _testEng.Operations.GetMember<Func<object, string>>(FooClass, "f");
            string result = sayHello(fooTest);

            Assert.AreEqual(result, expResult);
        }

        /// <summary>
        /// Test Case:	
        /// Verification:
        /// </summary>
        [Test]
        public void Execute_AccessValidVarInScope()
        {
            // Setup tests data
            ScriptScopeDictionary env = new ScriptScopeDictionary();
            env["test1"] = -10;
            
            ValidateExecute(env, "abs(test1)", 10);
        }

        
        /// <summary>
        /// Test Case:	
        ///     Script invokes a variable pre defined in the scope  
        /// Verification:
        ///     Script executes against the preset value of the var 
        /// </summary>
        [Test]
        public void Execute_UseVarDefinedInScope()
        {
            ScriptScope scope = _runTime.CreateScope();
            scope.SetVariable("someVar", 1);

            ValidateExecute(scope, "someVar+1", 2);
        }

        // <summary>
        /// Test Case:	
        ///     Execute
        /// Verification:
        ///     Script executes against the preset value of the var 
        /// </summary>
        [Test]
        public void Execute_ResultAvailableInScope()
        {
            // Setup tests data
            // Python abs method on var defined in default script scope
            string testInput = @"
test1 = -10
test1 = abs(test1)"; 
            
            // Exp result is the absolute value of test1
            ScriptSource source = _testEng.CreateScriptSourceFromString(testInput,
                                                                     SourceCodeKind.Statements);
            // Create scope
            ScriptScope scope = _testEng.CreateScope();
            
            // Setup var to hold result and execute for this scope 
            source.Execute(scope);
            
            // Check affect of Execution
            object testResult = scope.GetVariable("test1");
            Assert.AreEqual(testResult, 10);
        }


        // <summary>
        /// Test Case:	Null ScriptScope arg in Execute(...) should throw exception
        /// Verification: Verify Exception is thrown
        /// </summary>
        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Execute_NullScriptScope()
        {
            string testInput = _codeSnippets[CodeType.ValidExpression1];
            ScriptSource source = _testEng.CreateScriptSourceFromString(testInput,
                                                                     SourceCodeKind.Expression);
            source.Execute((ScriptScope)null);
        }

        
        [Test]
        public void ExecuteAndGetAsHandle_CheckExpectedReturnValue()
        {
            string inputSrc  = @"pow(64, 0.5)";
            ScriptSource source = _testEng.CreateScriptSourceFromString(inputSrc,
                                                                     SourceCodeKind.Expression);
            ScriptScope scope = _testEng.CreateScope();

            Assert.AreEqual(8.0, source.Execute<double>(scope));
        }

        [Test]
        public void ExecuteProgram_Basic1()
        {
            string testInput = _codeSnippets[CodeType.UpdateVarWithAbsValue];
            ValidateExecuteProgram(testInput, 0, SourceCodeKind.Statements);
        }

        [Test]
        public void GetReader_MoveIntoStreamCreateNewReaderAndCheckPosition()
        {

            // Setup tests data
            string testInput = _codeSnippets[CodeType.SevenLinesOfAssignemtStatements];

            // Count the code lines
            Regex RE = new Regex("\n", RegexOptions.Multiline);
            MatchCollection theMatches = RE.Matches(testInput);
            int lines = theMatches.Count + 1;

            // Create script source
            ScriptSource source = _testEng.CreateScriptSourceFromString(testInput,
                                                                     SourceCodeKind.Statements);
            // Create first reader
            SourceCodeReader srcFirstUnitReader = source.GetReader();

            // This could be a little fragile. Might be better just to hard code 
            // expected value. - Save first line with first reader.
            Assert.IsTrue(srcFirstUnitReader.SeekLine(1));
            string expValue = srcFirstUnitReader.ReadLine();
            // Move to the middle of the stream (approximately).
            Assert.IsTrue(srcFirstUnitReader.SeekLine(lines / 2));

            // Create second unit reader
            SourceCodeReader srcSecondUnitReader = source.GetReader();
            Assert.AreEqual(srcSecondUnitReader.ReadLine(), expValue);

        }

        /// <summary>
        ///Test: Create 2 reader objects and read from them independently 
        ///(ex ): Read 20% using the first reader, 40% using the second reader
        ///Expected Result: The readers are unaffected and don’t interfere in the other’s instance
        /// </summary>
        [Test]
        public void GetReader_TwoIndependentReadersAccessingSameData()
        {
            string testInput = _codeSnippets[CodeType.SevenLinesOfAssignemtStatements];
            StringBuilder strBuffer = new StringBuilder();

            //Get the aprox midpoint length of codinput.
            int codeMidPoint = testInput.Length / 2;

            ScriptSource source = _testEng.CreateScriptSourceFromString(testInput,
                                                                     SourceCodeKind.Statements);
            // Create the Readers
            SourceCodeReader srcFirstUnitReader = source.GetReader();
            SourceCodeReader srcSecondUnitReader = source.GetReader();

            int chrnbr = 0;
            int cnt = 0;
            // Read first half of stream with first stream reader
            while (((chrnbr = srcFirstUnitReader.Read()) > 0) && (cnt < codeMidPoint)){
               
                strBuffer.Append((char)chrnbr);
                cnt++; // inc cnt
                // Increment Second Reader
                srcSecondUnitReader.Read();
            }

            // Now get the second half of the input stream with second reader
            while ((chrnbr = srcSecondUnitReader.Read()) > 0){
                strBuffer.Append((char)chrnbr);
                cnt++;
            }

            Assert.AreEqual(cnt, testInput.Length);
            Assert.AreEqual(testInput, strBuffer.ToString());
        }

        [Test]
        public void GetReader_CreateMultipleDifferentInstances(){
            string testInput = _codeSnippets[CodeType.SevenLinesOfAssignemtStatements];
            ScriptSource source = _testEng.CreateScriptSourceFromString(testInput,
                                                                      SourceCodeKind.Statements);
            // Storage for readers to test
            SourceCodeReader prevStream = null, tmpStream = null;

            for(int i = 0; i < 10; i++){
                if (i > 0){
                    prevStream = tmpStream;
                }
                tmpStream = source.GetReader();
                Assert.AreNotEqual(prevStream, tmpStream);
            }
        }

        [Test]
        [Ignore]//Not implemented
        public void GetReader_ThreadSafetyTest()
        {
            Assert.Inconclusive("test not yet implemented");
        }

    }
}

