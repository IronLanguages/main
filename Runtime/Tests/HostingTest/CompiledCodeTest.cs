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
using System.Dynamic;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;
using Microsoft.Scripting;

namespace HostingTest {
    using Microsoft.Scripting.Runtime;

    class MyErrorListener : ErrorListener {
        public ScriptSource _source;
        public string _message;
        public SourceSpan _span;
        public int _errorCode;
        //public Severity _severity;

        public MyErrorListener() { }
        public override void ErrorReported(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity) {
            _source = source;
            _message = message;
            _span = span;
            _errorCode = errorCode;
            //  _severity = severity;
        }
    }
    /// <summary>
    /// This is a test class for CompiledCode and is intended
    /// to contain all CompiledCode Unit Tests
    ///
    ///</summary>
    [TestFixture]
    public partial class CompiledCodeTest : HAPITestBase {


        /// <summary>
        /// Constructor sets up specific code for the tests for
        /// this class.
        /// </summary>
        public CompiledCodeTest() {

        }

        [Test]
        public void DefaultScope_ValidateAccess() {
            ScriptSource source = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.OneLineAssignmentStatement],
                                                                        SourceCodeKind.Statements);
            CompiledCode compiled = source.Compile();
            //ScriptScope scope = compiled.DefaultScope();
        }


        [Ignore]
        [Test]
        public void Execute_ManyTimesUsingDelegate() {

            ScriptSource source = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.FactorialFunc], SourceCodeKind.Statements);
            MyErrorListener errorListen = new MyErrorListener();

            // Check the values of in errorListen for specific error info
            CompiledCode compiled = source.Compile(errorListen);
            Assert.IsTrue(errorListen._message == null);
            ScriptScope scope = _testEng.CreateScope();

            compiled.Execute(scope);
            Func<double, double> fact = scope.GetVariable<Func<double, double>>("fact");

            int testLoops = 10;

            double result;
            for (int i = 0; i < testLoops; i++) {
                result = fact((double)i);
                Assert.IsTrue(result > 0);
            }

        }

        /// <summary>
        /// Test     : Valid scope as input param - Script invokes a variable pre defined in the scope
        /// Expected : Script executes against the preset value of the var
        /// </summary>
        [Test]
        public void Execute_AccessVarPreDefinedInScope() {

            ScriptScopeDictionary env = new ScriptScopeDictionary();
            env["k"] = (object)1;

            ScriptSource source = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.OneLineAssignmentStatement] + "\nw=k+x", SourceCodeKind.Statements);
            MyErrorListener errorListen = new MyErrorListener();
            CompiledCode compiled = source.Compile(errorListen);
            Assert.IsTrue(errorListen._message == null);
            ScriptScope scope = _testEng.CreateScope(new ObjectDictionaryExpando(env));

            compiled.Execute(scope);

            object result = scope.GetVariable("x");
            Assert.AreEqual(result, (object)3);
            result = scope.GetVariable("w");
            Assert.AreEqual(result, (object)4);

        }


        /// <summary>
        /// Test      : Validate that file is only re-read on Execute, change file between Executes
        /// Expected  : Validate that Executes have updated source.
        /// 
        /// Note      : A new test case based on spec for compiledcode
        /// </summary>
        [Test]
        [Ignore]
        public void Execute_ValidateFileIsReReadOnExecute() {
            throw new NotImplementedException();

        }

        /// <summary>
        /// Test      : Engine Property is accessible from Compiled scope
        /// Expected  : Engine is same as engine that created scope, source, etc
        /// 
        /// Note      : validate the scope value is the same in the engine as the 
        ///             scope.
        /// </summary>
        [Test]
        public void Engine_Property_Validation() {

            ScriptSource source = _testEng.CreateScriptSourceFromFile(TestHelpers.CreateTempSourceFile(
                                                                      _codeSnippets[CodeType.OneLineAssignmentStatement], ".py"));
            MyErrorListener errorListen = new MyErrorListener();
            CompiledCode compiled = source.Compile(errorListen);
            Assert.IsTrue(errorListen._message == null);

            ScriptScope scope = compiled.Engine.CreateScope();
            compiled.Execute(scope);
            ScriptEngine engine = compiled.Engine;
            object result = scope.GetVariable("x");
            // Verify that engine has the same scope?
            Assert.AreEqual(engine, _testEng);
        }

        [Test]
        public void Execute_GenericTypeCastSmokeTest() {
            ScriptSource source = _PYEng.CreateScriptSourceFromFile(TestHelpers.CreateTempSourceFile(_codeSnippets[CodeType.SimpleExpressionOnePlusOne], ".py"));
            MyErrorListener errorListen = new MyErrorListener();
            CompiledCode compiled = source.Compile(errorListen);
            Assert.IsTrue(errorListen._message == null);
            // ScriptScope scope = _PYEng.CreateScope();
            // double result =  compiled.Execute<double>(scope);
            // Assert.AreEqual(result, (double)2);

        }

        /// <summary>
        /// Test     : Test the behavior off all the Executes
        /// Expected : They all work correct.
        /// </summary>
        [Test]
        public void Execute_SmokeTest_Execute_TestOfOtherExecutes() {
            ScriptSource source = _PYEng.CreateScriptSourceFromString("1+1");
            ScriptScope scope = _PYEng.CreateScope();
            object result;
            // This works as spec'd
            result = source.Execute(scope);
            Assert.AreEqual(result, (object)2);

            result = _PYEng.Execute("2+2", scope);
            Assert.AreEqual(result, (object)4);
        }


        /// <summary>
        /// Test     : Given a compiled expression Execute with scope arg
        /// Expected : Validate that correct value is returned
        /// </summary>
        [Test]
        public void Execute_ReturnExpressionValueForScopeArg() {

            ScriptSource source = _testEng.CreateScriptSourceFromString(
                                                _codeSnippets[CodeType.SimpleExpressionOnePlusOne]);
            MyErrorListener errorListen = new MyErrorListener();

            object result;
            int expectedResult = 2;

            CompiledCode compiled = source.Compile(errorListen);
            Assert.IsTrue(errorListen._message == null);
            ScriptScope scope = _testEng.CreateScope();
            result = compiled.Execute(scope);
            Assert.AreEqual(result, (object)expectedResult);

        }


        /// <summary>
        /// Test     : Give execute with null scope
        /// Expected : Should throw ArgumentNullException
        /// </summary>
        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Execute_NullScopeArgumentTest() {

            ScriptSource source = _testEng.CreateScriptSourceFromString(
                                                _codeSnippets[CodeType.SimpleExpressionOnePlusOne]);
            MyErrorListener errorListen = new MyErrorListener();

            CompiledCode compiled = source.Compile(errorListen);
            Assert.IsTrue(errorListen._message == null);
            compiled.Execute((ScriptScope)null);


        }



        /// <summary>
        /// Test     : Execute a compiled expression multiple times
        /// Expected : Validate that the return values is correct always the same
        /// </summary>
        [Test]
        public void Execute_ReturnExpressionValueForScopeArg_MultipleCalls() {

            ScriptSource source = _testEng.CreateScriptSourceFromString(
                                            _codeSnippets[CodeType.SimpleExpressionOnePlusOne]);
            MyErrorListener errorListen = new MyErrorListener();
            CompiledCode compiled = source.Compile(errorListen);
            Assert.IsTrue(errorListen._message == null);

            object result;
            int testLoops = 10;
            int expectedResult = 2;
            ScriptScope scope = _testEng.CreateScope();

            for (int i = 0; i < testLoops; i++) {

                result = compiled.Execute(scope);
                Assert.AreEqual(result, (object)expectedResult);
            }

        }


        /// <summary>
        /// Test     : Execute a compiled expression
        /// Expected : Validate that the return values is correct.
        /// </summary>
        [Test]
        public void Execute_ReturnExpressionValueNoArgs() {

            ScriptSource source = _PYEng.CreateScriptSourceFromString("1+1");
            MyErrorListener errorListen = new MyErrorListener();
            CompiledCode compiled = source.Compile(errorListen);
            Assert.IsTrue(errorListen._message == null);
            object result = compiled.Execute();
            Assert.AreEqual(result, (object)2);

        }


        /// <summary>
        /// Test     : Execute a compiled expression multiple times
        /// Expected : Validate that the return values is correct always the same
        /// </summary>
        [Test]
        public void Execute_ReturnExpressionValueNoArgs_MultipleCalls() {

            ScriptSource source = _PYEng.CreateScriptSourceFromString(
                                            _codeSnippets[CodeType.SimpleExpressionOnePlusOne]);
            MyErrorListener errorListen = new MyErrorListener();
            CompiledCode compiled = source.Compile(errorListen);
            Assert.IsTrue(errorListen._message == null);

            object result;
            int testLoops = 10;
            int expectedResult = 2;

            for (int i = 0; i < testLoops; i++) {
                result = compiled.Execute();
                Assert.AreEqual(result, (object)expectedResult);
            }

        }


        [Test]
        public void Execute_ReturnExpressionValueForDefaultScopeArg() {

            ScriptSource source = _PYEng.CreateScriptSourceFromString("x=1+1", SourceCodeKind.Statements);
            MyErrorListener errorListen = new MyErrorListener();
            CompiledCode compiled = source.Compile(errorListen);
            Assert.IsTrue(errorListen._message == null);
            compiled.Execute();
            // ScriptScope scope = compiled.DefaultScope();
            // object result = scope.GetVariable("x");
            // Assert.AreEqual(result, (object)2);

        }

    }
}
