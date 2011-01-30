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

using System.IO;



#if SILVERLIGHT
using Microsoft.Silverlight.TestHostCritical;
#endif

using NUnit.Framework;


namespace HostingTest
{
    
    
    /// <summary>
    ///This is a test class for ScriiptIO and is intended
    ///to contain all ScriptIO Unit Tests
    ///</summary>
    public partial class ScriptIOTest : HAPITestBase {

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

       /// <summary>
       /// 
       /// </summary>
       /// <param name="testStream"></param>
       /// <param name="expectedOutput"></param>
       public void ValidateAttachedStreamOutput(string fname, string expectedOutput){

           string[] lines = File.ReadAllLines(fname);
           int n = lines.Length;
           Assert.AreEqual(lines[n - 1], expectedOutput);

       }
    
    }
}
