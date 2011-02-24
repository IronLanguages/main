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
using System.Collections.Generic;
using System.Runtime.Remoting;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;

namespace HostingTest    {
    using Assert = NUnit.Framework.Assert;

    [TestFixture]
    public partial class ObjectOperationsTest : HAPITestBase {

        [Test]
        //[Ignore] // Bug # 479046
        public void Engine_GetAccess() {
            //  Setup....
            ObjectOperations objOps = _testEng.Operations;
            ScriptEngine engine = objOps.Engine;
        }

        /// <summary>
        ///  Test     : Null object
        ///  Expected : ArgumentNullException
        /// </summary>
        [Test]
        [Negative]
        public void IsCallable_NullObjectArgument() {
            _testEng.Operations.IsCallable((object)null);
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsCallable_NullObjectHandleArgument() {
            _testEng.Operations.IsCallable((ObjectHandle)null);
        }


        /// <summary>
        /// Test     : Obj of a delegate instance
        /// Expected : True
        /// 
        /// Notes    : Look for example in existing code.
        /// </summary>
        [Test]
        public void IsCallable_ObjOfDelegateInstance() {

            // Setup tests
            // Create scope
            ScriptScope scope = _testEng.CreateScope();
            ScriptSource code = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.IsOddFunction], SourceCodeKind.Statements);

            // Execute source in scope 
            code.Execute(scope);
            
            //Get a Delegate Instance using the Func<> Generic declaration and GetVariable
            Func<int, bool> isodd = scope.GetVariable<Func<int, bool>>("isodd");
            
            Assert.IsTrue(_testEng.Operations.IsCallable(isodd));

            // Call the function to validate IsCallable
            Assert.IsTrue(isodd(1));
            Assert.IsFalse(isodd(2));

            //Get a Delegate Instance using the Func<> Generic declaration and GetVariable
            var isodd2 = scope.GetVariable<F1<int, bool>>("isodd");

            Assert.IsTrue(_testEng.Operations.IsCallable(isodd2));

            // Call the function to validate IsCallable
            Assert.IsTrue(isodd2(1));
            Assert.IsFalse(isodd2(2));
        }
        
        private delegate TRet F1<T1, TRet>(T1 value);

        /// <summary>
        /// Test     : Null object	
        /// Expected : ArgumentNullException
        /// </summary>
        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentTypeException))]
        public void Call_NullObject() {
            object[] parameters = new object[]{"-a", "-b", "-c",  "foo.py"};
            _testEng.Operations.Invoke((object)null,parameters);
        }

        /// <summary>
        /// Test     : Null object[]	
        /// Expected : ArgumentNullException
        /// </summary>
        [Test]
        [Negative]
        [ExpectedException(typeof(NullReferenceException))]
        public void Call_NullObjectParams() {
            String varName = "pyf";
            object fooFun = GetVariableValue(_codeSnippets[CodeType.SimpleMethod],
                                                           varName);
            _testEng.Operations.Invoke(fooFun, (object[])null);
        }


        [Test]
        [Negative]
        [ExpectedException(typeof(MissingMemberException))]
        public void GetMember_NullObject() {
            _testEng.Operations.GetMember((object)null, "foo");
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetMember_NullName() {
            string varName = "FooClass";
            
            object FooClass = GetVariableValue(_codeSnippets[CodeType.SimpleFooClassDefinition],
                                                           varName);
            _testEng.Operations.GetMember(FooClass, (string)null);
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(MissingMemberException))]
        public void GenericGetMember_NullObject() {
            _testEng.Operations.GetMember<string>((object)null, "foo");
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenericGetMember_NullName() {
            String varName = "FooClass";
            object fooClasObj = GetVariableValue(_codeSnippets[CodeType.SimpleFooClassDefinition],
                                                           varName);
            _testEng.Operations.GetMember<string>(fooClasObj, (string)null);
        }

        [Test]
        [Negative]
        public void TryGetMember_NullObject() {
            // Spec say this should throw NullArgumentException for null object or null name
            object outObj;
            Assert.IsFalse(_testEng.Operations.TryGetMember((object)null, "foo", out outObj));
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TryGetMember_NullName() {
            String varName = "FooClass";
            object fooClassObj = GetVariableValue(_codeSnippets[CodeType.SimpleFooClassDefinition],
                                                           varName);
            object outObj;
            _testEng.Operations.TryGetMember(fooClassObj, (string)null, out outObj);
        }


        /// <summary>
        ///  Test     : Do a simple opperations that does not exist in the current language
        ///  Expected : Raise the correct exception
        /// </summary>
        [Negative]
        [Test]
        [ExpectedException(typeof(TypeErrorException))]
        public void DoOperation_NoOperatorForLanguage() {
            object expectedResult = 2;
            String varName = "x";
            object objectVar = GetVariableValue(_codeSnippets[CodeType.OneLineAssignmentStatement],
                                                           varName);

            object result = _testEng.Operations.DoOperation(System.Linq.Expressions.ExpressionType.Decrement, objectVar);
            Assert.AreEqual(expectedResult, result);
        }


        /// <summary>
        ///  Test     : GetCallSignatures with null object
        ///  Expected : Returns empty string array
        /// </summary>
        [Test]
        public void GetCallSignature_NullArg() {
            int expectedResult = 0;
            string[] result = (string[])_testEng.Operations.GetCallSignatures((object)null);
            
            // Should be empty array of Length zero
            Assert.AreEqual(result.Length, expectedResult);
        }

      
        [Test]
        [Negative]
        public void GetMemberNames_NullObject() {
            _testEng.Operations.GetMemberNames((object)null);
        }

        [Test]
        public void GetMemberNames_MemberObjects() {
            List<string> expectedResult = new List<string>() { "concat", "add", "__doc__", "__module__", "__init__", "f", "someInstanceAttribute" }; 
            String varName = "FooClass";
            object tmpFoo = GetVariableValue(_codeSnippets[CodeType.SimpleFooClassDefinition],
                                                              varName);
            
            object FooClass = _testEng.Operations.Invoke(tmpFoo); // create new FooClass
            
            // BUG - Not same return value as Spec
            List<string> result = new List<string>(_testEng.Operations.GetMemberNames(FooClass));

            // Verify the list is equal
            Assert.AreEqual(result.Count, expectedResult.Count);
            result.ForEach(delegate(string name) {
                Assert.IsTrue(expectedResult.Contains(name));
            });
        }

        [Test]
        public void GetCallSignature_PassValidClassObject() {
            // depending on how the object stores objects 
            // internally python stores 4 members maybe?
            string[] expectedResult = new string[4];
            String varName = "FooClass";

            object objectVar = GetVariableValue(_codeSnippets[CodeType.SimpleFooClassDefinition],
                                                              varName);

            object fooClass = _testEng.Operations.Invoke(objectVar); // create new FooClass
            string[] result = (string[])_testEng.Operations.GetCallSignatures(fooClass);
            Assert.AreEqual(result.Length, expectedResult.Length);
        }

        
        [Test]
        public void GetCallSignature_PassValidMethodObject() {
            string varName = "concat";
            string[] expectedResult = new string[] { "concat(a, b, c)" };

            object concat = GetVariableValue(_codeSnippets[CodeType.MethodWithThreeArgs],
                                                           varName);

            ValidateCallSignatures(concat, expectedResult);
        }



        /// <summary>
        /// Test      :
        /// Expected  :
        /// </summary>
        [Test]
        public void ConvertTo_IntegerToDouble() {
            // Setup input values
            string lookupObjectName = "x";
            double expectedValue = 3;

            // Setup Input Source, return object
            object objectInScope = GetVariableValue(_codeSnippets[CodeType.OneLineAssignmentStatement],
                                                                  lookupObjectName);

            // Verify Convertions with expected value and type
            ValidateConvertTo(objectInScope, expectedValue);
        }


        [Test]
        [Negative]// Bug # 478206
        [ExpectedException(typeof(TypeErrorException))]
        public void GenericConvertTo_NullObject(){
            Double newValue = _testEng.Operations.ConvertTo<Double>((object)null);
        }


        [Test]
        [Negative]// Bug # 478206
        [ExpectedException(typeof(TypeErrorException))]
        public void ConvertTo_NullObject() {
            object newValue = _testEng.Operations.ConvertTo((object)null, typeof(int));
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConvertTo_NullType() {

            // Lookup value in scope
            string lookupObjectName = "x";
            
            // Get object from the scope
            object ScopeObjectValue = GetVariableValue(_codeSnippets[CodeType.OneLineAssignmentStatement],
                                                                     lookupObjectName);
            // Test Null value arg exception
            object newValue = _testEng.Operations.ConvertTo(ScopeObjectValue, (Type)null);
        }

       /// <summary>
       /// Test      :
       /// Expected  :
       /// </summary>
        [Test]
        public void GenericConvertTo_IntegerToDouble() {
            string lookupObjectName = "x";
            double expectedValue = 3;

            object objectInScope = GetVariableValue(_codeSnippets[CodeType.OneLineAssignmentStatement],
                                                                  lookupObjectName);

            ValidateConvertTo<double>(objectInScope, expectedValue);
        }

        /// <summary>
        /// Test     : Same cases as ConvertTo<T>
        /// Expected : Returns true on successful conversion, and on failure returns false and result of T.default
        /// </summary>
        [Test]
        public void GenericTryConvertTo_IntegerToDouble(){
            
            string lookupObjectName = "x";
            double expectedValue = 3;

            object objectInScope = GetVariableValue(_codeSnippets[CodeType.OneLineAssignmentStatement],
                                                                  lookupObjectName);
            ValidateTryConvertTo<double>(objectInScope, expectedValue);
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(MissingMemberException))]
        public void SetMember_NullObjectArg(){
            (_testEng.CreateOperations()).SetMember((object)null, "foo", 0);
        }

        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetMember_NullNameArg() {
            (_testEng.CreateOperations()).SetMember("foo", (string)null, 0);
        }

        /// <summary>
        /// Test      : For each Operations language and each source language, 
        ///             set an existing and settable member	
        /// Expected  : The member is updated with the new value
        /// 
        /// Note      : This code fails for languages that do not have read/write objects!
        /// </summary>
        [Test]
        public void SetMember_BasicMemberTest() {

            // BUG - File bug/Investigate either I don't understand or this is a bug.
            // Starting over - this method needs a object member to be operated on not just
            // any member in the scope.
            string lookupClassVarName = "FooClass";
            string lookupMemberName = "add";

            object objectVar = GetVariableValue(_codeSnippets[CodeType.SimpleFooClassDefinition],
                                                              lookupClassVarName);

            object fooClass = _testEng.Operations.Invoke(objectVar); // create new FooClass
            // Verify that the member function exists
            Assert.IsTrue(_testEng.Operations.ContainsMember(fooClass, lookupMemberName));
            
            // Set the member 
            // Get some new code to put in replace/set.
            ScriptScope scope = _testEng.CreateScope();
            ScriptSource code = _testEng.CreateScriptSourceFromString("new_add=1", SourceCodeKind.Statements);

            code.Execute(scope);

            object newObjectValue = scope.GetVariable("new_add");
            _testEng.Operations.SetMember(fooClass, lookupMemberName, newObjectValue);
            // Verify that the member does not exists anymore
            Assert.IsFalse(_testEng.Operations.ContainsMember(fooClass, "new_add"));
       
        }

        /// <summary>
        /// Test     : Lookup the a member of an object 
        /// Expected : validate that ContainsMember returns true for a known member
        /// </summary>
        [Test]
        public void ContainsMember_BasicLookup() {

            string lookupClassVarName = "FooClass";
            string lookupMemberName = "add";

            object objectVar = GetVariableValue(_codeSnippets[CodeType.SimpleFooClassDefinition],
                                                              lookupClassVarName);

            object fooClass = _testEng.Operations.Invoke(objectVar); // create new FooClass
            Assert.IsTrue(_testEng.Operations.ContainsMember(fooClass, lookupMemberName));
        }


        /// <summary>
        /// Test     : Lookup the a member that is not in an object 
        /// Expected : validate that ContainsMember returns false
        /// </summary>
        [Negative]
        [Test]
        public void ContainsMember_LookForMemberThatDoesNotExist() {

            string lookupClassVarName = "FooClass";
            string lookupMemberName = "__zzzzzkdsloopqqqq___";

            object objectVar = GetVariableValue(_codeSnippets[CodeType.SimpleFooClassDefinition],
                                                              lookupClassVarName);

            object fooClass = _testEng.Operations.Invoke(objectVar); // create new FooClass
            Assert.IsFalse(_testEng.Operations.ContainsMember(fooClass, lookupMemberName));
        }

        
        /// <summary>
        /// Test     : Null object	
        /// Expected : ArgumentNullException
        /// </summary>
        [Negative]
        [Test]
        [ExpectedException(typeof(MissingMemberException))]
        public void RemoveMember_NullObjectArg(){
            _testEng.Operations.RemoveMember((object)null, "x");
        }

        /// <summary>
        /// Test     : Null object	
        /// Expected : ArgumentNullException
        /// </summary>
        [Negative]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveMember_NullStringNameArg() {
            // BUG -  investigate/file bug
            string lookupName = "x";
            object objectInScope = GetVariableValue(_codeSnippets[CodeType.OneLineAssignmentStatement], lookupName);
            _testEng.Operations.RemoveMember(objectInScope, (string)null);
        }

        /// <summary>
        /// Test     : Create a FooClass object in Hosted script language
        /// Expected : Verify that ObjectOperations is removed
        /// 
        /// Notes    : This will not work for every language but it should work for
        ///            Python. The Object Operations RemoveMember would be equivelent to 
        ///            this Python:
        ///             class FooClass:
        ///                 def inc(self, x):
        ///                     return x+1;
        ///             N=FooClass()
        ///             N.inc(4) ==> 5
        ///             del N.inc(4)
        ///             N.inc(4) ==> Error
        ///            
        /// </summary>
        [Test]
        public void RemoveMember_BaiscObjectRemovalFn() {
            string lookupClassVarName = "FooClass";
            string lookupMemberName = "someInstanceAttribute";

            object objectVar = GetVariableValue(_codeSnippets[CodeType.SimpleFooClassDefinition],
                                                              lookupClassVarName);

            object fooClass = _testEng.Operations.Invoke(objectVar); // create new FooClass
            // Verify that the member function exists
            Assert.IsTrue(_testEng.Operations.ContainsMember(fooClass, lookupMemberName));
            // Remove the member function
            _testEng.Operations.RemoveMember(fooClass, lookupMemberName);
            // Verify that the member does not exists anymore
            Assert.IsFalse(_testEng.Operations.ContainsMember(fooClass, lookupMemberName));
       
        }


        /// <summary>
        /// Test     : remove member from object
        /// Expected : object is remove
        /// Note     : this might not make sense for vars but only class members.
        /// </summary>
        [Test]
        [Ignore] // Bug # 478257 - This might not be valid though IP code like "x=4\ndel x\n" works in IP
        public void RemoveMember_BaiscObjectRemovalVar() {
            // BUG -  investigate/file bug
            string varName = "x";
            
            object objectVar = GetVariableValue(_codeSnippets[CodeType.OneLineAssignmentStatement],
                                                              varName);

            _testEng.Operations.RemoveMember(objectVar, varName);
        }

        
        /// <summary>
        /// Test     : GetMembers of imported module from script
        /// Expected : check subset of member names exist
        /// </summary>
        [Test]
        public void GetMemberNames_LocalFromImportedModule() {

            // Setup tests
            string varName = "date";
            // Get sub set of members that are not likely to change
            List<string> expectedPyModuleDateTimeMembersSubSet = new List<string>(){
                         "astimezone", "combine", "ctime", "date", "day", "dst",
                         "fromordinal", "fromtimestamp", "hour",
                         "isocalendar", "isoformat", "isoweekday", "max", "microsecond", "min",
                         "minute", "month", "now", "replace", "resolution",
                         "second", "strftime", "time", "timetuple", "timetz", "today",
                         "toordinal", "tzinfo", "tzname", "utcfromtimestamp",
                         "utcnow", "utcoffset", "utctimetuple", "weekday", "year"};


            // Setup the date time dot net object
            object dotNetObject = GetVariableValue(_codeSnippets[CodeType.ImportCPythonDateTimeModule],
                                                                  varName);
            
            // Verify that this is the Spec'd signature
            List<string> members = new List<string>(_testEng.Operations.GetMemberNames(dotNetObject));

            // Verify a subset exists in members
            expectedPyModuleDateTimeMembersSubSet.ForEach(delegate(string name) {
                Assert.IsTrue(members.Contains(name));
            });

            
            //Assert.AreEqual(result.Length, expectedResult.Length);
        }
        /// <summary>
        /// Test     : Pass null value to method
        /// Expected : empty string return
        /// </summary>
        [Test]
        [Negative]
        [ExpectedException(typeof(NullReferenceException))]
        public void GetDocumentation_NullParameter() {
            string doc = _testEng.Operations.GetDocumentation((object)null);
            Assert.AreEqual(0, doc.Length);
        }

        [Test]
        public void GetDocumentation_AssemblyModule() {
            ///Setup
            string varName = "date";
            // Expected value
            string expectedDocs = "datetime(year: int, month: int, day: int, hour: int, minute: int, second: int, microsecond: int, tzinfo: tzinfo)";
            object objectVar = GetVariableValue(_codeSnippets[CodeType.ImportCPythonDateTimeModule],
                                                              varName);

            // Get the associated documentation
            string doc = _testEng.Operations.GetDocumentation(objectVar);
            // Verify values
            Assert.IsTrue( doc.Contains(expectedDocs));
        }
      
        [Test]
        public void GetDocumentation_FromScriptMethod() {
           
            string varName = "doc";
            // Expected value
            string expectedDocs = "This function does nothing";
            object objectVar = GetVariableValue(_codeSnippets[CodeType.MethodWithDocumentationAttached],
                                                              varName);

            string doc = _testEng.Operations.GetDocumentation(objectVar);
            Assert.AreEqual(expectedDocs, doc);
        }
        
        /// <summary>
        /// Test      : Import .Net DateTime assembly
        /// Expected  : Get the correct doc string attached 
        /// 
        /// Note      : Could be a problem if the .Net version changes and this specific doc string is changed
        /// </summary>
        [Test]
        public void GetDocumentation_FromDotNetObject() {
            string varName = "DotNetDate";
            ScriptScope scope = _testEng.CreateScope();
            ScriptSource code = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.ImportDotNetAssemblyDateTimeModule], SourceCodeKind.Statements);

            code.Execute(scope);
            
            // This could break if the Underlining DotNet documentation changes.
            //http://dlr.codeplex.com/WorkItem/View.aspx?WorkItemId=6071
            //string expectedDocs = "Represents an instant in time, typically expressed as a date and time of day";
            string expectedDocs = "DateTime(year: int, month: int, day: int, hour: int, minute: int, second: int, millisecond: int, calendar: Calendar, kind: DateTimeKind)";
            object testObject = scope.GetVariable(varName);
            string doc = _testEng.Operations.GetDocumentation(testObject);
            Assert.IsTrue(doc.Contains(expectedDocs));
        }

        [Test]
        public void AddMethod() {
            //System.Dynamic.Runtime.Operators.And
            object left = 4;
            object right = 3;
            object expectedResult = (object)(((int)left) + ((int)right));
            object result = _testEng.Operations.Add(left, right);
            Assert.AreEqual(expectedResult, result );
        }

        [Test]
        public void SubMethod() {
            object left = 4;
            object right = 3;
            object expectedResult = (object)(((int)left) - ((int)right));
            object result = _testEng.Operations.Subtract(left, right);
            Assert.AreEqual(expectedResult, result);
        }


        /// <summary>
        /// Test     : Test env change using engine.CreateOperations with module change in scope 
        /// Expected : New env change should give correct __future__ division type.
        /// </summary>
        [Test]
        [Ignore] // BUG 476154
        public void TestFromFuture_UsingOperations() {

            ScriptRuntime sr = CreateRuntime();
            ScriptScope futureScope = _testEng.CreateScope();

            futureScope.SetVariable("division", true);
            sr.Globals.SetVariable("__future__", futureScope);

            ScriptSource source = _testEng.CreateScriptSourceFromString(_codeSnippets[CodeType.ImportFutureDiv], 
                                                                     SourceCodeKind.Statements);
            ScriptScope localScope = _testEng.CreateScope();
            source.Execute(localScope);

            ObjectOperations operation = _testEng.CreateOperations(localScope);
            
            // now do div operations and check result
            object divResult = operation.Divide(1, 2);

            // if this is future style then the result should be 0.5
            Assert.AreEqual( 0.5, (double)divResult);
        }

        // Bug # 466027 - regression test
        [Test]
        public void FormatException_Test()
        {
            ScriptEngine engine = _runtime.GetEngine("py");
            ExceptionOperations es = engine.GetService<ExceptionOperations>();
            OutOfMemoryException e = new OutOfMemoryException();
            string result = es.FormatException(e);
            Assert.AreNotEqual(e.Message, result);


        }

        [Test]
        public void Create_Op2()
        {
            ScriptEngine engine = _runtime.GetEngine("py");
            ScriptScope scope = engine.CreateScope();
            ObjectOperations operation = engine.CreateOperations(scope);

         
            string pyCode = @"class TC(object):
     i = -1
     def what(self):
         return 1";

            ScriptSource src = engine.CreateScriptSourceFromString(pyCode, SourceCodeKind.Statements);
            src.Execute(scope);
            
            object FooClass = scope.GetVariable("TC");
            object[] param = new object[] { };
            object newObjectInstance = engine.Operations.CreateInstance(FooClass, param); // create new FooClass

            var what = engine.Operations.GetMember<Func<int>>(newObjectInstance, "what");
            int n = what();

        }

        [Test]// BUG # 479046 - regression test
        public void Create_Op1(){

            ScriptEngine engine = _runtime.GetEngine("py");
            ScriptScope scope = engine.CreateScope();
            ObjectOperations operation = engine.CreateOperations(scope);

            // now do div operations and check result
            // "x = object()"
            string pyCode = @"class TC(object):
     i = -1
     def what(self):
         return 1";

            ScriptSource src = engine.CreateScriptSourceFromString(pyCode, SourceCodeKind.Statements);
            src.Execute(scope);
            
            object FooClass = scope.GetVariable("TC");
            

            object[] param = new object[] {  };
            object testClassInstance0 = engine.Operations.CreateInstance(FooClass, param); // create new FooClass
            object testClassInstance1 = engine.Operations.CreateInstance(FooClass, param); // create new FooClass


            Assert.AreNotEqual(testClassInstance0, testClassInstance1);
        }
    }
}
