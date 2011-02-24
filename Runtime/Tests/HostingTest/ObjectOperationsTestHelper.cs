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
using NUnit.Framework;

namespace HostingTest
{
    public partial class ObjectOperationsTest : HAPITestBase {

        private TestContext testContextInstance;
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

       public ObjectOperationsTest()
            : base() {
           
       }

       internal object GetVariableValue(string code, string varname) {
            ScriptScope scope = _testEng.CreateScope();
            ScriptSource source = scope.Engine.CreateScriptSourceFromString(code, Microsoft.Scripting.SourceCodeKind.Statements);
            source.Execute(scope);
            return scope.GetVariable(varname);
       }

       internal void ValidateConvertTo<T> (object objectInScope, T expectedValue){
            // Get Operations for associated Engine
            ObjectOperations operations = _testEng.CreateOperations();
            Assert.AreEqual(expectedValue, operations.ConvertTo<T>(objectInScope));
       }

       internal void ValidateTryConvertTo<T>(object objectInScope, T expectedValue) {
           // Get Operations for associated Engine
           ObjectOperations operations = _testEng.CreateOperations();
           Assert.IsTrue(operations.TryConvertTo<T>(objectInScope, out expectedValue));
       }

       internal void ValidateCallSignatures(object objectFromScope, string[] expectedValue){
           string[] result = (string[])_testEng.Operations.GetCallSignatures(objectFromScope);
           TestHelpers.AreEqualArrays(expectedValue, result);
       }
    }
}
