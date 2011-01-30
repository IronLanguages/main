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
using System.IO;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;

#if SILVERLIGHT
using Microsoft.Silverlight.TestHostCritical;
#endif

namespace HostingTest
{
    
    
    /// <summary>
    /// This is a test a sub class for ScriptHostTest and is intended
    /// to contain many of the ScriptHost overridden members
    ///</summary>
    public partial class ScriptHostTest : HAPITestBase {

        private TestContext testContextInstance;

        
        /// <summary>
        /// This gets or sets the test context which provides
        /// information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }


        public ScriptHostTest()
            : base() {

            //_tempFilesToDelete = new List<string>();
        }


       /// <summary>
       /// </summary>
       /// <param name="testPath"></param>
       /// <returns></returns>
       private ScriptRuntimeSetup/*!*/ CreateScriptRuntimeSetup(string/*!*/ testPath) {
           return CreateHostTypeScriptRuntimeSetup(typeof(ScriptHostBasicSubTest), testPath);
       }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="HostType">The type of host we have derived</param>
        /// <param name="testPath">Host Search Path</param>
        /// <returns></returns>
        private ScriptRuntimeSetup/*!*/ CreateHostTypeScriptRuntimeSetup(Type HostType, string/*!*/ testPath) {
           ScriptRuntimeSetup setup = CreateSetup();            
            setup.HostType = HostType;
            setup.HostArguments = new object[] { testPath };
            return setup;
        }


        /// <summary>
        ///  BUG - This helper function is blocked by the lack of HostConfig documentation/implementation
        ///        for example "4.16	ScriptRuntimeSetup" is undefined in DLR Hosting Spec 
        ///        from DLR Hosting doc "Design expected 19 MAY 08. XXX"
        /// </summary>
        /// <param name="HostType"></param>
        /// <param name="hostArgs"></param>
        /// <returns></returns>
        private ScriptRuntimeSetup/*!*/ CreateHostTypeScriptRuntimeSetup(Type HostType, Object[] hostArgs) {
            ScriptRuntimeSetup setup = CreateSetup();
            setup.HostType = HostType;
            object[] tmpHostArgs = new object[hostArgs.Length];
            setup.HostArguments = tmpHostArgs;
            return setup;
        }

       internal ScriptRuntime CreateHostRuntime(Type HostType, string path) {
           return new ScriptRuntime(CreateHostTypeScriptRuntimeSetup(HostType, path));
       }


       internal ScriptRuntime CreateHostPAL(Type HostType, string path) {
           return new ScriptRuntime(CreateHostTypeScriptRuntimeSetup(HostType, path));
       }

       internal ScriptRuntime CreateTestRuntime(string path) {
           return new ScriptRuntime(CreateScriptRuntimeSetup(path));
       }

       private ScriptRuntime CreateRemoteAppDomain(string/*!*/ testPath) {
           AppDomain newAppDomain;
           ScriptRuntime newScriptRuntime;
           newAppDomain = AppDomain.CreateDomain("TestHostRemoteScriptDomain");
           newScriptRuntime = ScriptRuntime.CreateRemote(newAppDomain, CreateScriptRuntimeSetup(testPath));
           return newScriptRuntime;
       }

       /// <summary>
       /// ValidateProperties associated with Host -- compare test and expected PAL value(s)
       /// </summary>
       internal void ValidateProperties(PlatformAdaptationLayer testHostPAL, PlatformAdaptationLayer expectedPAL) {
           Assert.AreEqual(testHostPAL, expectedPAL);
       }

       /// <summary>
       /// ValidateProperties associated with Host -- Maybe blocked by bug 462717
       /// </summary>
       internal void ValidateProperties(PlatformAdaptationLayer testHostPAL, string expectedPath) {
           Assert.AreEqual(testHostPAL.CurrentDirectory, expectedPath);
       }

       /// <summary>
       /// ValidateProperties associated with Host -- compare test and expected Runtime values
       /// </summary>
       internal void ValidateProperties(ScriptRuntime testHostRuntime, ScriptRuntime expectedRuntime) {
           Assert.AreEqual(testHostRuntime, expectedRuntime);
       }
        /// <summary>
        ///  host.GetSourceFileSearchPath() calls the protected virtual function but - it may
        ///  not be a protected virtual function in the code maybe a Spec Diff BUG.
        /// </summary>
        /// <param name="testPaths"></param>
        /// <param name="expectedPaths"></param>
       internal void ValidateSourceFileSearchPathValues(IList<string> testPaths, IList<string> expectedPaths) {
           Assert.AreEqual(testPaths.Count, expectedPaths.Count);
           Assert.IsTrue(testPaths.Count > 0);
           Assert.IsTrue(expectedPaths.Contains(testPaths[0]));
       }


       ScriptHost CreateTestHost(Type HostType, string existingSourceTmpFileName) {
           // CreateTestHost(HostType, existingSourceTmpFileName)
           // This could possble enable the host to be aware of other search path
           ScriptRuntime newRuntimeEnv = CreateHostRuntime(HostType,
                                                           Path.GetDirectoryName(existingSourceTmpFileName));
           return newRuntimeEnv.Host;
       }


    }
}
