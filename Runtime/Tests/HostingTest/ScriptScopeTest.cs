using System;
using System.Collections.Generic;
using System.IO;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;
using Microsoft.Scripting.Runtime;
using System.Runtime.Remoting;
using System.Text;

namespace HostingTest {

    [TestFixture]
    public partial class ScriptScopeTest : HAPITestBase {
        /// <summary>
        /// No Default Engine test existence of a variable contained in the scope
        /// </summary>
        [Test]
        public void ContainsVariable_NoDefaultEngineContainsVariable() {
            ScriptScope scope = _runtime.CreateScope();

            // One case sensitive var
            scope.SetVariable("v1", 1);

            Assert.IsTrue(scope.ContainsVariable("v1"));
        }


        /// <summary>
        /// Test : No Default Engine test for existence of a variable contained in the scope with wrong 
        /// case lookup string.
        /// Expected : Fail
        /// </summary>
        [Negative()]
        [Test]
        public void ContainsVariable_NoDefaultEngineWrongCaseLookup() {
            ScriptScope scope = _runtime.CreateScope();

            // One case sensitive var
            scope.SetVariable("v1", 1);
            Assert.IsFalse(scope.ContainsVariable("V1"));
        }

        /// <summary>
        /// Test : Case sensitive default engine (Python), existing name with correct casing
        /// Expected : True
        /// </summary>
        [Test]
        public void ContainsVariable_BasicLookup() {
            ScriptScope scope = _testEng.CreateScope();
            scope.SetVariable("v1", 1);
            Assert.IsTrue(scope.ContainsVariable("v1"));
        }

        /// <summary>
        /// Test     : Null name	
        /// Expected : ArgumentNullException
        /// </summary>
        [Negative]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetVariable_NameWithNullValue() {
            _runtime.CreateScope().GetVariable((string)null);
        }

        [Test]
        public void GetVariable_WithEmptyString() {
            ScriptScope scope = _runtime.CreateScope();

            KeyValuePair<string, object> testVar = new KeyValuePair<string, object>(string.Empty, 1);
            scope.SetVariable(testVar.Key, testVar.Value);

            Assert.AreEqual(testVar.Value, scope.GetVariable(testVar.Key));
        }

        [Test]
        public void GetVariable_CaseSensitiveDefaultEngine() {
            ScriptScope scope = _PYEng.CreateScope();

            var testVar = new KeyValuePair<string, object>("one", 1);
            scope.SetVariable(testVar.Key, testVar.Value);

            Assert.AreEqual(testVar.Value, scope.GetVariable(testVar.Key));
        }


        [Negative]
        [Test]
        [ExpectedException(typeof(MissingMemberException))]
        public void GetVariable_WithNonExistentVariable() {
            _runtime.CreateScope().GetVariable("MissingVar");
        }

        [Ignore]        
        [Test]
        public void DefaultEngine_GetDefaultEngineTest() {
            ScriptRuntime defaultRuntime = CreateRuntime();
            ScriptScope scope = defaultRuntime.CreateScope();
            //scope.DefualtEngine

            // Bug - Spec versus Code - DefaultEngine is currently called 'scope.Engine'
        }


        [Negative]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetVariable_WithNullName() {
            _runtime.CreateScope().SetVariable((string)null, 0);
        }

        /// <summary>
        /// Test : No default engine, empty string name, any value	
        /// Expect : the value to be inserted properly with an empty 
        ///          string name identifier (or arguably an exception)
        /// </summary>
        [Test]
        public void SetVariable_WithEmptyName() {
            ScriptScope scope = _runtime.CreateScope();

            var testPair = new KeyValuePair<string, object>("", 1);
            scope.SetVariable(testPair.Key, testPair.Value);

            Assert.AreEqual(testPair.Value, scope.GetVariable(testPair.Key));
        }

        /// <summary>
        /// Test : No default engine and case sensitive engine, existing name with a new casing, 
        ///        any value	
        /// Expected : New casing inserted properly, old casing undisturbed
        /// </summary>
        [Test]
        public void SetVariable_CreateVariableWithNewCaseNewValue() {
            ScriptScope scope = _runtime.CreateScope();

            var testPair = new KeyValuePair<string, object>("SomeVar", 1);
            scope.SetVariable(testPair.Key, testPair.Value);

            Assert.AreEqual(testPair.Value, scope.GetVariable(testPair.Key));

            var newCaseVar = new KeyValuePair<string, object>("someVar", "abc");
            scope.SetVariable(newCaseVar.Key, newCaseVar.Value);

            Assert.AreEqual(newCaseVar.Value, scope.GetVariable(newCaseVar.Key));
            Assert.AreEqual(testPair.Value, scope.GetVariable(testPair.Key));
        }

        /// <summary>
        /// Test : No default engine and case sensitive engine, existing name, new value
        /// Expected : Variable’s value overwritten
        /// </summary>
        [Test]
        public void SetVariable_SetExistingVarToNewValue() {
            ScriptScope scope = _runtime.CreateScope();

            var testVar = new KeyValuePair<string, object>("SomeVar", 1);
            scope.SetVariable(testVar.Key, testVar.Value);

            Assert.AreEqual(testVar.Value, scope.GetVariable(testVar.Key));

            // New value
            var expectedVar = new KeyValuePair<string, object>("SomeVar", 2);
            scope.SetVariable(expectedVar.Key, expectedVar.Value);

            Assert.AreEqual(expectedVar.Value, scope.GetVariable(expectedVar.Key));
            Assert.AreEqual(1, scope.GetVariableCount());
        }

        /// <summary>
        /// Test : ‘name’ already exists; but value’s actual type is different from the 
        ///         current one.
        /// Expected : New value and new  type is available subsequently
        /// </summary>
        [Test]
        public void SetVariable_SetExistingVarToNewType() {
            ScriptScope scope = _runtime.CreateScope();

            var testVar = new KeyValuePair<string, object>("var1", 1);
            scope.SetVariable(testVar.Key, testVar.Value);

            Assert.AreEqual(testVar.Value, scope.GetVariable(testVar.Key));

            scope.SetVariable(testVar.Key, "HelloWorld");
            Assert.AreEqual("HelloWorld", scope.GetVariable(testVar.Key));
        }

        [Test]
        public void ExecuteGeneric_CallEmptyArg() {
            object testResult = _testEng.Execute<object>(string.Empty);
            Assert.IsNull(testResult);
        }

        /// <summary>
        /// Test     : Correct T value and valid Expression 
        /// Expected : Returns a expresion as type T.
        /// </summary>
        [Test]
        public void ExecuteGeneric_ValidExpression() {
            // Setup test
            // Call Execute this throws exception
            int testResult0 = _testEng.Execute<int>("1+1");

            object testResult1 = _testEng.Execute<object>("1+1");

            // Expected Result
            int expectedResult = 2;

            // Verify value and type are as expected
            Assert.AreEqual(testResult0, expectedResult);
            Assert.AreEqual(testResult1, expectedResult);
        }

        /// <summary>
        /// Test     : Basic GetVarialbelGeneric smoke test return correct value and type
        /// Expected : Correct value and type are returned.
        /// </summary>
        [Test]
        public void GetVariableGeneric_CaseSensitiveEngine() {
            ScriptScope scope = _PYEng.CreateScope();

            scope.SetVariable("var1", 1);

            int expectedResult = scope.GetVariable<int>("var1");
            Assert.AreEqual(1, expectedResult);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetVariableGeneric_Null() {
            _runTime.CreateScope().GetVariable<int>(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentTypeException))]
        public void GetVariableGeneric_IncorrectType() {
            ScriptScope scope = _runtime.CreateScope();

            scope.SetVariable("var1", 1);

            string result = scope.GetVariable<string>("var1");
            Assert.IsNull(result);
        }


        /// <summary>
        /// Name that exists, T value requiring conversion for
        /// for the value in this language.
        /// </summary>
        [Test]
        public void GetVariableGenericT_ExistingVarGenericUnboxLookupCheck() {
            ScriptScope TestScope = _testEng.CreateScope();
            //ScriptScope TestNullScope = null;
            object obj = new object();
            obj = "Hello";
            //ObjectHandle oResult = new ObjectHandle(obj);
            string key = "test1";

            TestScope.SetVariable(key, obj);
            object objRtn = TestScope.GetVariable<object>(key);

            // Check un boxing of object retrieved
            Assert.IsTrue((string)objRtn == (string)obj);
        }

        [Test]
        public void GetVariableGeneric_TypeUpConvert() {
            ScriptScope scope = _PYEng.CreateScope();

            scope.SetVariable("var1", 1);

            double result = scope.GetVariable<double>("var1");
            Assert.AreEqual(1.0, result);
        }

        [Test]
        [ExpectedException(typeof(TypeErrorException))]
        public void GetVariableGeneric_TypeDownConvert() {
            ScriptScope scope = _PYEng.CreateScope();

            scope.SetVariable("var1", 1.0);

            scope.GetVariable<int>("var1");
        }

        [Negative]
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveVariable_NullArg() {
            _runtime.CreateScope().RemoveVariable(null);
        }

        [Test]
        public void RemoveVariable_OneOfManyVars() {
            ScriptScope scope = _runtime.CreateScope();

            scope.SetVariable("v1", 1);
            scope.SetVariable("v2", 2);
            scope.SetVariable("v3", 3);

            Assert.IsTrue(scope.RemoveVariable("v2") && (2 == scope.GetVariableCount()));
        }

        [Test]
        public void RemoveVariable_OneVar() {
            ScriptScope scope = _runtime.CreateScope();

            scope.SetVariable("v2", 2);
            Assert.IsTrue(scope.RemoveVariable("v2") && (0 == scope.GetVariableCount()));
        }

        [Test]
        public void RemoveVariable_EmptyScope() {
            Assert.IsFalse(_runtime.CreateScope().RemoveVariable("v2"));
        }

        [Test]
        public void RemoveVariable_WrongCase() {
            ScriptScope scope = _PYEng.CreateScope();
            scope.SetVariable("V2", 4);

            Assert.IsFalse(scope.RemoveVariable("v2"));
            Assert.IsTrue((1 == scope.GetVariableCount()) && scope.ContainsVariable("V2"));
        }



        /// <summary>
        /// Scope bound to thsi engine, new name
        /// 
        /// New variable is created matching the given name and value.
        /// </summary>
        [Test]
        public void SetVariable_ScopeBoundToEngineSetNewName() {

            ScriptScopeDictionary global = new ScriptScopeDictionary();
            string[] key = { "Test1", "test2", "TEST3" };
            global[key[0]] = 1111;
            global[key[1]] = 2222;
            global[key[2]] = 3333;

            string newName = "test4";
            object newValue = 4444;
            _testEng.CreateScope(new ObjectDictionaryExpando(global)).SetVariable(newName, newValue);

            // Verify new Variable has been created.
            Assert.IsTrue(global.Contains(newName));
            Assert.IsTrue(global[newName].Equals(newValue));
        }

        /// <summary>
        ///  
        ///  Test for empty string value lookup
        /// 
        /// </summary>
        [Test]
        public void ContainVariable_EmptyStringValue() {
            ScriptScopeDictionary global = new ScriptScopeDictionary();
            // Populate with some test data
            // string > 20
            string[] key = { "test1", "", "test3" };
            int tExpectedVal1 = 1111;
            global[key[0]] = tExpectedVal1;
            int tExpectedVal2 = 2222;
            global[key[1]] = tExpectedVal2;

            // Get default scope with set values
            ScriptScope TestScope = _testEng.CreateScope(new ObjectDictionaryExpando(global));
            // Test for empty var!
            Assert.IsTrue(TestScope.ContainsVariable(key[1]));

        }

        [Test]
        public void SetVariable_ScopeBoundToEngineSetExistingName() {

            ScriptScopeDictionary global = new ScriptScopeDictionary();

            string[] key = { "Test1", "test2", "TEST3" };
            global[key[0]] = 1111;
            global[key[1]] = 2222;
            global[key[2]] = 3333;

            object tNewValue = 4444;
            _testEng.CreateScope(new ObjectDictionaryExpando(global)).SetVariable(key[2], tNewValue);

            Assert.IsTrue(global[key[2]].Equals(tNewValue));
        }

        /// <summary>
        ///  name.Length > 255 and name not declared in scope
        ///  
        /// Expected return false
        /// </summary>
        [Test]
        public void ContainVariable_NameLengthGT256WithNonExistingName() {
            ScriptScopeDictionary global = new ScriptScopeDictionary();
            // Populate with some test data
            // string > 20
            string[] key = { "test123456789ABCDEFGHIJ", "test2", "test3" };
            int tExpectedVal1 = 1111;
            global[key[0]] = tExpectedVal1;
            int tExpectedVal2 = 2222;
            global[key[1]] = tExpectedVal2;

            int BigValueToTestBeyond = 256;
            StringBuilder longStrTest = new StringBuilder("test_var_");
            for (int i = 0; i < BigValueToTestBeyond + 1; i++) {
                longStrTest.Append(string.Format("{0}", i));
            }

            string tLongKey = longStrTest.ToString();

            // Validate that this key is longer then 256 
            Assert.IsTrue(tLongKey.Length > BigValueToTestBeyond);

            // Get default scope with set values
            ScriptScope TestScope = _testEng.CreateScope(new ObjectDictionaryExpando(global));
            // Lookup Long key 
            Assert.IsFalse(TestScope.ContainsVariable(tLongKey));


        }

        /// <summary>
        /// name.Length > 20 and name declared in scope 
        /// expected return true.
        /// </summary>
        [Test]
        public void ContainVariable_NameLengthTwentyWithExistingName() {
            ScriptScopeDictionary global = new ScriptScopeDictionary();
            // Populate with some test data
            // string > 20
            string[] key = { "test123456789ABCDEFGHIJ", "test2", "test3" };
            int tExpectedVal1 = 1111;
            global[key[0]] = tExpectedVal1;
            int tExpectedVal2 = 2222;
            global[key[1]] = tExpectedVal2;


            // Get default scope with set values
            ScriptScope TestScope = _testEng.CreateScope(new ObjectDictionaryExpando(global));

            Assert.IsTrue(TestScope.ContainsVariable(key[0]));
        }
        
        /// <summary>
        /// Try Get Variable for the casses of GetVariableGenericT(...)
        /// 
        /// 1) Get When var exists - Get value set to its value and a return of true
        /// 2) Get when it doesn't exist - value set to null and a 
        ///    return of false
        /// 3) When it doesn't or cannot be converted to T, value set to null 
        ///    and return of false - HOW CAN WE TEST THIS CASE?
        /// </summary>
        [Test]
        public void TryGetVariableGenericT_MultipleCases() {
            ScriptScopeDictionary global = new ScriptScopeDictionary();
            // Populate with some test data
            string[] key = { "test1", "test2", "test3" };
            int tExpectedVal1 = 1111;
            global[key[0]] = tExpectedVal1; // case 1


            // Get default scope with set values
            ScriptScope TestScope = _testEng.CreateScope(new ObjectDictionaryExpando(global));
            // out value;
            int value = -1;

            ObjectHandle objHVal = new ObjectHandle(value);

            // Case 1
            //int outV;
            Assert.IsTrue(TestScope.TryGetVariable<int>(key[0], out value));
            Assert.IsTrue(value == tExpectedVal1);

            // reset
            value = -1;
            // Case 2 - Not sure about this
            Assert.IsFalse(TestScope.TryGetVariable<int>(key[1], out value));
            Assert.IsTrue(value == 0);

            //@TODO - Add Case 3 - HOW CAN I TEST THIS CASE need more info.

        }

        /// <summary>
        /// 1) Get When var exists - Get value set to its value and a return of true
        /// 2) Get when it doesn't exist - value set to null and a 
        ///    return of false
        /// 
        /// </summary>
        [Test]
        public void TryGetVariable_multipleCases() {

            ScriptScopeDictionary global = new ScriptScopeDictionary();
            // Populate with some test data
            string[] key = { "test1", "test2", "test3" };
            int tExpectedVal1 = 1111;
            global[key[0]] = tExpectedVal1; // case 1
            //int tExpectedVal2 = 2222;
            //global[key[1]] = 2222; // case 2


            ScriptScope testScope = _testEng.CreateScope(new ObjectDictionaryExpando(global));
            // out value;
            int value = -1;

            // Case 1
            Assert.IsTrue(testScope.TryGetVariable(key[0], out value));
            Assert.IsTrue(value == tExpectedVal1);

            // Not null!
            int tExpectedRtnValueOfNonExistentVar = 0;
            // reset
            value = -1;
            // Case 2

            Assert.IsFalse(testScope.TryGetVariable(key[1], out value));
            Assert.IsTrue(value == tExpectedRtnValueOfNonExistentVar);

        }



        /// <summary>
        /// 1) Get When var exists - Get value set to its value and a return of true
        /// 2) Get when it doesn't exist - value set to null and a 
        ///    return of false
        /// 
        /// </summary>
        [Test]
        public void TryGetVariableAsHandle_MultipleCases() {

            ScriptScopeDictionary global = new ScriptScopeDictionary();
            // Populate with some test data
            string[] key = { "test1", "test2", "test3" };
            int tExpectedVal1 = 1111;
            global[key[0]] = tExpectedVal1; // case 1


            ScriptScope TestScope = _testEng.CreateScope(new ObjectDictionaryExpando(global));
            // out value;
            int value = -1;

            ObjectHandle objHVal = new ObjectHandle(value);

            // Case 1
            Assert.IsTrue(TestScope.TryGetVariableHandle(key[0], out objHVal));
            Assert.IsTrue((int)(objHVal.Unwrap()) == tExpectedVal1);

            // reset
            value = -1;
            objHVal = new ObjectHandle(value);
            // Case 2
            Assert.IsFalse(TestScope.TryGetVariableHandle(key[1], out objHVal));
            Assert.IsNull(objHVal);

        }

        /// <summary>
        /// Name that exists, T value requiring no conversion for
        /// for the value in this language.
        /// </summary>
        [Test]
        public void GetVariableGenericT_ExistingVarGenericTypeCheck() {

            ScriptScopeDictionary global = new ScriptScopeDictionary();
            // Populate with some test data
            string[] key = { "test1", "test2", "test3" };
            int tExpectedVal1 = 1111;
            global[key[0]] = tExpectedVal1; // case 1
            int tExpectedVal2 = 2222;
            global[key[1]] = tExpectedVal2;


            ScriptScope TestScope = _testEng.CreateScope(new ObjectDictionaryExpando(global));
            // ScriptScope TestNullScope = null;
            object obj = new object();
            // What is ObjectHandle used for?
            ObjectHandle oResult = new ObjectHandle(obj);
            int val = TestScope.GetVariable<int>(key[0]);

            // Check return type and value
            Assert.IsTrue(val.GetType() == tExpectedVal1.GetType());
            Assert.IsTrue(val == tExpectedVal1);

        }

        /// <summary>
        /// Test     : Set some variables in the scope and then invoke
        /// Expected : All variables are returned with correct type when fully enumerated
        /// </summary>
        [Test]
        public void GetItems_IEnumerable() {
            ScriptScope scope = _runtime.CreateScope();
            KeyValuePair<string, object>[] dict = {new KeyValuePair<string, object>("v1", 1),
                                                    new KeyValuePair<string, object>("v2", 'a'),
                                                    new KeyValuePair<string, object>("v3", (byte)15)};
            for (int j = 0; j < dict.Length; j++)
                scope.SetVariable(dict[j].Key, dict[j].Value);

            TestHelpers.AreEqualCollections<KeyValuePair<string, object>>(dict, scope.GetItems());
        }

        //regression test for DDB:488990
        [Test]
        public void GetItems_RemoteAD() {
            ScriptScope scope = _remoteRuntime.CreateScope();

            KeyValuePair<string, object>[] dict = { new KeyValuePair<string, object>("v1", 1) };
            scope.SetVariable("v1", 1);

            TestHelpers.AreEqualCollections<KeyValuePair<string, object>>(dict, scope.GetItems());
        }

        [Test]
        public void GetItems_EmptyScope() {
            ScriptScope scope = _runtime.CreateScope();

            KeyValuePair<string, object>[] dict = { };
            TestHelpers.AreEqualCollections<KeyValuePair<string, object>>(dict, scope.GetItems());
        }

        [Test]
        public void GetItems_MultipleCalls() {
            ValidateGetItems(_runtime);
        }


        
        [Test]
        public void GetItems_MultipleCallsRemoteRuntime() {
            ValidateGetItems(_remoteRuntime);
        }

        // Bug # 482429 validation
        [Test]
        public void Execute_ValidExpressionResult()
        {
            Assert.AreEqual((int)(_testEng.Execute("1 + 1")), 2);
        }
       
        // Bug # 482429 validation
        // TODO : Replace existing Generic Execute 
        [Test]
        public void ExecuteGeneric_ValidExpressionResult()
        {
            Assert.AreEqual((int)(_testEng.Execute<int>("1 + 1")), 2);
        }

        // Bug # 485727
        [Test]
        public void TryGetVariable_ValidateExistingScopeItem()
        {
            ScriptScope scope = _runtime.CreateScope();
            scope.SetVariable("var1", 1);
            int expected1;
            Assert.IsTrue(scope.TryGetVariable<int>("var1", out expected1));
            Assert.AreEqual(expected1, 1);
            
        }

        // Bug # 485727
        [Test]
        public void TryGetVariable_ValidateNonExistingScopeItem()
        {
            ScriptScope scope = _runtime.CreateScope();
            string expected1;
            Assert.IsFalse(scope.TryGetVariable<string>("var1", out expected1));
            Assert.AreEqual(expected1, null);
            
        }

        // Bug # 485727
        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentTypeException))]
        public void TryGetVaiable_TryWrongTypeGet()
        {
            ScriptScope scope = _runtime.CreateScope();
            scope.SetVariable("var1", 1);
            string expected1;
            Assert.IsTrue(scope.TryGetVariable<string>("var1", out expected1));

        }



        /// <summary>
        /// Verify that that null name will throw exception
        /// </summary>
        [Test]
        [Negative]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetVariableAsHandle_NullName() {

            // Create a valid Scope Dictionary 
            ScriptScopeDictionary global = new ScriptScopeDictionary();
            // Populate with some test data
            global["Test1"] = 1111;

            // Create the Scope
            ScriptScope TestScope = _testEng.CreateScope(new ObjectDictionaryExpando(global));
            // Create a null string to lookup
            string tNullName = null;
            ObjectHandle ObjHdl = TestScope.GetVariableHandle(tNullName);
        }


        /// <summary>
        /// Local engine, with non-existent name should throw an exception
        /// MissingMemberException or UnboundNameException?
        /// </summary>
        [Test]
        [Negative]
        [ExpectedException(typeof(MissingMemberException))]
        public void GetVariableAsHandle_LocalEngine_NonExistentName() {

            // Create the Scope
            ScriptScope TestScope = _testEng.CreateScope();
            // Create a null string to lookup
            string tNullName = "I_Do_Not_Exist";
            ObjectHandle ObjHdl = TestScope.GetVariableHandle(tNullName);
        }

        /// <summary>
        /// Local engine, existing name returns an wrapped ObjectHandle
        /// </summary>
        [Test]
        public void GetVariableAsHandle_LocalEngineExistingName() {

            // Create a valid Scope Dictionary 
            ScriptScopeDictionary global = new ScriptScopeDictionary();
            // Populate with some test data
            global["Test1"] = 1111;

            // Create the Scope
            ScriptScope TestScope = _testEng.CreateScope(new ObjectDictionaryExpando(global));
            // Create a null string to lookup
            string tKey = "Test1";
            ObjectHandle ObjHdl = TestScope.GetVariableHandle(tKey);
            // Verify that we we can access "Test1"
            string msg = string.Format("The remote lookup scope is {0}", ObjHdl);
            // Unwrap the value in the ObjectHandle
            object tObj = ObjHdl.Unwrap();
            // Validate they are equal
            Assert.IsTrue(tObj == global[tKey], msg);
        }


        /// <summary>
        /// Remote engine, remote value, return var value returned 
        /// wrapped in ObjectHandle.
        /// </summary>
        [Test]
        public void GetVariableAsHandle_RemoteEngineRemoteValue() {

            ScriptRuntime env = ScriptRuntimeTest.CreateRuntime(TestHelpers.CreateAppDomain("new domain"));
            ScriptEngine engine = env.GetEngine("py");

            ScriptScope scope = engine.CreateScope();
            ScriptSource code = engine.CreateScriptSourceFromString("two=1+1", SourceCodeKind.Statements);
            code.Execute(scope);

            ObjectHandle obj = scope.GetVariableHandle("two");

            int two = (int)obj.Unwrap();
            Assert.AreEqual(two, 2);

        }




        /// <summary>
        /// Use a Non-null globals dictonary should Returns a new, usable, 
        /// ScriptScope with globals as the backing dictionary.  Scope updates s
        /// hold be reflected in globals.
        /// 
        /// Also make changes with Execute and verify they are global
        /// scope is updated.
        /// </summary>
        [Test]
        public void CreateScope_UsingValidScopeDic() {

            // Create a Scope Dictionary 
            ScriptScopeDictionary global = new ScriptScopeDictionary();
            // Populate with some test data
            global["One"] = 1;
            global["Two"] = 2;
            global["Three"] = 3;


            // Greate a new Scope and or get default scope...
            ScriptScope testScope = _testEng.Runtime.CreateScope(new ObjectDictionaryExpando(global));
            foreach (string str in testScope.GetVariableNames()) {
                Assert.AreEqual(global[str], testScope.GetVariable(str));
            }

            // Now verify that changes to the TestScope are reflected in the global scope.
            string updateTestKey = "Four";
            testScope.SetVariable(updateTestKey, 4);
            Assert.AreEqual(global[updateTestKey], testScope.GetVariable(updateTestKey));

        }

        /// <summary>
        /// Do GetVariable(...) with Empty string as value in name.
        /// </summary>
        [Test]
        public void GetVariable_EmptyString() {

            // Create a Scope Dictionary 
            ScriptScopeDictionary global = new ScriptScopeDictionary();
            // Populate with some test data
            global["Test1"] = 1111;
            global["Test2"] = 2222;
            global["Test3"] = 3333;

            StringBuilder TestName = new StringBuilder();
            // TestName = String.Empty;
            string tName = TestName.ToString();
            // Test value
            object testVal = 7777;
            global[tName] = testVal;

            string msg = string.Format("Make sure our test data is a valid empty string '{0}",
                                        tName);
            Assert.IsTrue(string.IsNullOrEmpty(tName), msg);

            // Setup up the default scope.
            ScriptScope TestScope = _testEng.CreateScope(new ObjectDictionaryExpando(global));

            // Now try to retrieve empty variable.
            object expVal = TestScope.GetVariable(tName);

            msg = string.Format("Trying to retrieve the empty string from the local scope");
            Assert.IsTrue(expVal.Equals(testVal), msg);

        }
    }
}
