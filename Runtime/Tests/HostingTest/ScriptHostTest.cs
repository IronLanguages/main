using System;
using System.Collections.Generic;
using System.IO;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;

namespace HostingTest    {


    // A very simple sub derived ScriptHost
    class MyHost : ScriptHost {
            public MyHost() { }
            public MyHost(string foo, int bar) {}
     }

     
    /// <summary>
    /// This is a test class for ScriptHostTest and is intended
    /// to contain all ScriptHostTest Unit Tests
    ///
    /// From the spec : 
    ///     The DLR instantiates the ScriptHost when the DLR initializes a ScriptRuntime.  
    ///     The host can get at the instance with ScriptRuntime.Host.
    ///
    /// This class depends on on the DerivedHostTest class that I created in order
    /// to fully test the host.
    /// 
    /// Other possible test scenario : 
    ///     Creating more then one derived Host as well as target each
    ///     override-able (i.e., virtual) and abstract methods.
    ///</summary>
    [TestFixture]
    public partial class ScriptHostTest : HAPITestBase {

        
        /// <summary>
        /// Test     : Use a custom host to create a runtime.  Compare the original 
        ///            runtime with this property.
        /// Expected : Values are the same.
        /// </summary>
        [Test]
        public void Runtime_HostRuntimeProperty() {

            // Todo - Investigate this to verify correctness.
            // Setup derived With basic DerivedHostTest
            ScriptRuntime runtimeEnv = CreateHostRuntime(typeof(ScriptHostBasicSubTest), Path.GetFullPath("."));
            
            // Verify runtime's are equal
            ValidateProperties(runtimeEnv, runtimeEnv.Host.Runtime);
            
        }


        /// <summary>
        ///  Test     : Invoke the property	
        ///  Expected : Correct PAL is returned
        /// </summary>
        [Test]
        public void PAL_HostPalProperty() {
            
            // Setup runtime with specific path
            string testPath = Path.GetFullPath(".");
            ScriptRuntime runtimeEnv = CreateHostRuntime(typeof(ScriptHostBasicSubTest), testPath);

            // Verify correct PAL property -- Maybe blocked by bug 462717
            ValidateProperties(runtimeEnv.Host.PlatformAdaptationLayer, testPath);
            
        }
        
        
   
        /// <summary>
        ///  Test     : Basic smoke test for derived host invocation.
        ///  Expected : Verify that properties return correct references.
        /// </summary>
        [Test]
        public void Host_BasicInvokeFromRuntime(){

            string testPath = Path.GetFullPath(".");

            // Todo - Investigate this to verify correctness.
            // Setup derived With basic DerivedHostTest
            ScriptRuntime runtimeEnv = CreateHostRuntime(typeof(ScriptHostBasicSubTest), testPath);
            
            // Verify correct PAL property -- Maybe blocked by bug 462717
            ValidateProperties(runtimeEnv.Host.PlatformAdaptationLayer, testPath);
            
            // Verify correct runtime property.
            ValidateProperties(runtimeEnv, runtimeEnv.Host.Runtime);
        }

        /// <summary>
        /// Test     :  Create a Host that is not associated with a ScriptRuntime
        /// Expected :  Throw not initialized exception 
        /// </summary>
        [Test]
        [Negative]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Host_UninitializedRuntimeProperty() {

            // Setup test values
            // Create derived host instance but this is not associated with runtime?
            ScriptHostBasicSubTest myHost = new ScriptHostBasicSubTest(Path.GetFullPath("."));

            // Verify correct runtime property.
            ScriptRuntime runtimeEnv = myHost.Runtime;
        
        }

        /// <summary>
        /// Test     :  Create a Host that is not associated with a ScriptRuntime
        ///             and Get PAL property.
        /// Expected :  Throw not initialized exception 
        /// 
        /// Note     :  Missing documentation this may not be the expected behaviour
        /// </summary>
        [Test]
        [Negative]
        [ExpectedException(typeof(InvalidOperationException))]
        [Ignore] // BUG - Currently blocked by lack of documentation for ScriptHost|PAL 
                 // (Investigate | file bug)
        public void Host_UninitializedPALProperty() {

            // Setup test values
            // Create derived host instance but this is not associated with runtime?
            ScriptHostBasicSubTest myHost = new ScriptHostBasicSubTest(Path.GetFullPath("."));

            // Todo - Investigate this failure this could be by design

            // Verify correct PAL property -- Maybe blocked by bug 462717
            PlatformAdaptationLayer testPAL = myHost.PlatformAdaptationLayer;

        }
        //TODO : fix this obsolete tag
#if OBSOLETE

        /// <summary>
        /// Test     : Env var pointerd by ‘PathEnvironmentVariableName’ is null; Invoke the property	
        /// Expected : Value is ‘.’
        /// </summary>
        [Test]
        //[Ignore]
        public void SourceFileSearchPath_NullPathEnvironmentVariableName() {

            // Todo - Investigate this to verify correctness.
            // Setup derived With basic DerivedHostTest
            ScriptRuntime runtimeEnv = CreateHostRuntime(typeof(ScriptHostBasicSubTest), "-nothing");

            // Setup Env Var to null/empty
            TestHelpers.EnvSetupTearDown EnvTest = new TestHelpers.EnvSetupTearDown("DLRPATH", "");

            // Setup host
            ScriptHostBasicSubTest host = (ScriptHostBasicSubTest)runtimeEnv.Host;

            // Setup expected paths
            List<string> expectedPaths = new List<string> {"."};

            // Verify results
            ValidateSourceFileSearchPathValues(host.GetSourceFileSearchPath(), expectedPaths);
        
        }

        /// <summary>
        /// Test     : Load a script from a file using the host.
        /// Expected : Execute with expected results 
        /// </summary>
        [Test]
        //[Ignore]
        public void TryGetSourceFile_WithEncodingAndKind() {

            // Setup test env 
            string tmpFileName = TestHelpers.CreateTempSourceFile(_codeSnippets[CodeType.OneLineAssignmentStatement], 
                                                                  ".py");
            // Setup expected test vars
            string lookupVarName = "x";
            object expectedResult = 3;
            
            ScriptHostBasicSubTest host = (ScriptHostBasicSubTest)CreateTestHost(typeof(ScriptHostBasicSubTest), 
                                                                                 tmpFileName);
            // Verify results
            ValidateTryGetSourceFile(host, tmpFileName, lookupVarName, expectedResult);
            

        }

        /// <summary>
        /// Test     : invoke the ‘ResolveSourceFile’ method
        /// Expected : This method is invoked by the DLR; name is passed through without any modifications
        /// </summary>
        [Test]
        //[Ignore] // BUG - This test is blocked by lack of DLRPATH DLR/Config support
        public void ResolveSourceFileName_OverrideTestNameIsUnchanged() {
            
            // Setup env for search.
            string tmpFileName = TestHelpers.CreateTempSourceFile("1+1", ".py");
            
            // This could possble enable the host to be aware of other search path
            ScriptRuntime newRuntimeEnv = CreateHostRuntime(typeof(ScriptHostBasicSubTest), 
                                                            Path.GetDirectoryName(tmpFileName));
            ScriptHostBasicSubTest host = (ScriptHostBasicSubTest)newRuntimeEnv.Host;
            // Using abs path tmpFileName
            ValidateResolveSourceFileNameSearchResult(host, tmpFileName);
        
        }

        /// <summary>
        /// Test     : Try to resolve a file that should not be in the path
        /// Expected : Should throw exceptions
        /// 
        /// Note     : This test doesn't necessarily give real information since
        ///            we can't change the DLR Path currently thus almost any file
        ///            will fail - weather it there or not! 
        /// </summary>
        [Test]
        [Negative]
        [ExpectedException(typeof(FileNotFoundException))]
        //[Ignore] // See|File Host Config Bug/ResolveSourceFile
        public void ResolveSourceFileName_LookForMissingFile() {
            
            //Need to find a name that is less likely to be in the path
            string missingFileName = "123sadfsssp__foo__.py";

            // This could possble enable the host to be aware of other search path
            ScriptRuntime newRuntimeEnv = CreateHostRuntime(typeof(ScriptHostBasicSubTest),".");
            ScriptHostBasicSubTest host = (ScriptHostBasicSubTest)newRuntimeEnv.Host;
            // Verify search result
            ValidateResolveSourceFileNameSearchResult(host, missingFileName);
        
        }


        /// <summary>
        /// See bug entry, this method and test might go away.
        /// </summary>
        [Test]
        //[Ignore] // Bug # 475921
        public void ResolveSourceFileName_LookForFilePysicallyAddFileAndDLRPATH() {

            // Adjust DLRPATH and place a file in this directory
            // After looking at the source code I see that I need to leave off the file extension.
            string fileExt = ".py";
            string tmpFileName = TestHelpers.CreateTempSourceFile(_codeSnippets[CodeType.OneLineAssignmentStatement],
                                                                  fileExt);
            string FileNameInDLRPath = Path.GetFileName(tmpFileName).Replace(fileExt, "");
            string testDLRPath = Path.GetDirectoryName(tmpFileName); 
            
            TestHelpers.EnvSetupTearDown EnvTest = new TestHelpers.EnvSetupTearDown("DLRPATH", testDLRPath);

            
            // This could possble enable the host to be aware of other search path
            ScriptRuntime newRuntimeEnv = CreateHostRuntime(typeof(ScriptHostBasicSubTest), "-nothing");
            ScriptHostBasicSubTest host = (ScriptHostBasicSubTest)newRuntimeEnv.Host;

            // Verify search result
            ValidateResolveSourceFileNameSearchResult(host, FileNameInDLRPath,
                                                      testDLRPath + "\\" + FileNameInDLRPath + fileExt);
            
        }

        

        [Test]
        //[Ignore]
        public void ResolveSourceFileTest() {

            // Adjust DLRPATH and place a file in this directory
            // After looking at the source code I see that I need to leave off 
            // the file extension.
            Environment.SetEnvironmentVariable("DLRPATH", "c:\\dlr\\dlrpath\\");

            
            // Create Runtime with attached Host
           // ScriptRuntime aRuntime = FooTest.CreateHostRuntime(typeof(ADerivedHost), "-f foo");
            //ADerivedHost host = (ADerivedHost)aRuntime.Host;
            
            // This finds a file
            //host.ResolveSourceFile("foo");

            // This fails
            //host.ResolveSourceFile("foo.py");
        
        }

        [Test]
        //[Ignore] // Bug # 475921
        public void ResolveSourceFile_Test() {

            // Adjust DLRPATH and place a file in this directory
            // After looking at the source code I see that I need to leave off 
            // the file extension.

            string tempFile = Path.GetTempFileName();
            string newFile = Path.ChangeExtension(tempFile, ".py");
            File.WriteAllText(newFile, "print \"Hello\"");

            //e.g., 'foo.py' string in dlrpath
            string fileNameWithExtension = Path.GetFileName(newFile);
            //e.g., 'foo'  string though this file name doesn't exists
            string fileNameWithoutExtension = fileNameWithExtension.Replace(".py", "");

            // Adjust the environment
            Environment.SetEnvironmentVariable("DLRPATH", Path.GetDirectoryName(newFile));
            
            ScriptRuntimeSetup setup = new ScriptRuntimeSetup(true);
            setup.HostType = typeof(ScriptHostBasicSubTest);
            // Are HostArgs ment to be arguments to a loaded script i.e. 
            // passing paramters to argv?
            setup.HostArguments = new object[] { "-f foo" };
            // This throws exception 
            ScriptRuntime aRuntime = ScriptRuntime.Create(setup);
            ScriptHostBasicSubTest host = (ScriptHostBasicSubTest)aRuntime.Host;
            
            // Search for 'foo' type name that is a success
            host.ResolveSourceFile(fileNameWithoutExtension);
            
            // Search for 'foo.py' type file fails
            host.ResolveSourceFile(fileNameWithExtension);
        }

        /// <summary>
        /// Test : Resolve file that is not in path
        /// Expected : Throw file not found exception
        /// </summary>
        [Test]
        [Negative]
        [ExpectedException(typeof(FileNotFoundException))]
        //[Ignore] // See|File Host Config Bug/ResolveSourceFile
        public void ResolveSourceFile_WithSingleArg() {

            // Setup env for search.
            string tmpFileName = TestHelpers.CreateTempSourceFile("1+1", ".py");

            // This could possbly enable the host to be aware of other search path
            ScriptRuntime newRuntimeEnv = CreateHostRuntime(typeof(ScriptHostBasicSubTest),
                                                            Path.GetDirectoryName(tmpFileName));
            ScriptHostBasicSubTest host = (ScriptHostBasicSubTest)newRuntimeEnv.Host;
            ValidateResolveSourceFileSearchResult(host, tmpFileName, Path.GetFullPath(tmpFileName));
       
        }


        /// <summary>
        /// Test     : Pass a valid ‘name’ that was used to execute in the current runtime 
        /// Expected : The corresponding ScriptSource object is returned
        /// 
        /// Note     : See Test Plan BUG?/ See Config/ResolveSourceFileName(...) bug
        /// </summary>
        [Test]
        //[Ignore] // Bug see ResolveSourceFileName bug
        public void ResolveSourceFile_PassValidName() {
            // Get source
            string testSrc = _codeSnippets[CodeType.OneLineAssignmentStatement];
            
            // Setup env for search.
            string tmpFileName = TestHelpers.CreateTempSourceFile(testSrc, ".py");

            // This could possble enable the host to be aware of other search path
            ScriptRuntime newRuntimeEnv = CreateHostRuntime(typeof(ScriptHostBasicSubTest),
                                                            Path.GetDirectoryName(tmpFileName));
            ScriptHostBasicSubTest host = (ScriptHostBasicSubTest)newRuntimeEnv.Host;
            ScriptScope scope = newRuntimeEnv.ExecuteFile(tmpFileName);
            
            // Using abs path tmpFileName
            ValidateResolveSourceFileSearchResult(host, tmpFileName, Path.GetDirectoryName(tmpFileName));
       
        }

        /// <summary>
        /// Test      : Override this method in the derived class and create a ScriptRuntime object
        /// Expected  : This method should be invoked by the DLR automatically.
        /// </summary>
        [Test]
        //[Ignore] // Bug This is block due to lack of virtual function to subclass investigate file bug
        public void RuntimeAttached_DerivedHostOverrideInvoke() {

            // TODO - Finish this if I can
            throw new NotImplementedException();
        }

#endif

    }
}
