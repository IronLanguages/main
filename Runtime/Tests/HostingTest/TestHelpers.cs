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
using System.IO;
using System.Text;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;
using NUnit.Framework;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace HostingTest {
    using Assert = NUnit.Framework.Assert;

    internal class TestHelpers {

        /// <summary>
        /// Config file containing the tested languages - py,rb,ts
        /// </summary>
        public static string StandardConfigFile { get; private set; }

        /// <summary>
        ///Directory where tests execute and binaries are loaded from 
        /// </summary>
        public static string BinDirectory { get; private set; }

        static TestHelpers() {
            BinDirectory = Path.GetDirectoryName(Uri.UnescapeDataString(new Uri(typeof(HAPITestBase).Assembly.CodeBase).AbsolutePath));
            StandardConfigFile = GetStandardConfigFile();
        }

        private static string GetStandardConfigFile() {
            var configFile = Path.GetFullPath(Uri.UnescapeDataString(new Uri(typeof(HAPITestBase).Assembly.CodeBase).AbsolutePath)) + ".config";
            Debug.Assert(File.Exists(configFile), configFile);
            return configFile;
        }

        internal static T AssertExceptionThrown<T>(System.Action f) where T : Exception {
            try {
                f();
            }
            catch (T ex) {
                return ex;
            }

            Assert.Fail("Expecting exception '" + typeof(T) + "'.");
            return null;
        }

        internal static void RedirectOutput(ScriptRuntime runTime, TextWriter output, System.Action f) {
            runTime.IO.SetOutput(Stream.Null, output);
            runTime.IO.SetErrorOutput(Stream.Null, output);

            try {
                f();
            }
            finally {
                runTime.IO.RedirectToConsole();
            }
        }
        
        [Flags]
        internal enum OutputFlags {
            None = 0,
            Raw = 1
        }

        internal static void AssertOutput(ScriptRuntime runTime, System.Action f, string expectedOutput) {
            AssertOutput(runTime, f, expectedOutput, OutputFlags.None);
        }


        internal static void AssertOutput(ScriptRuntime runTime, System.Action f, string expectedOutput, OutputFlags flags) {
            StringBuilder builder = new StringBuilder();

            using (StringWriter output = new StringWriter(builder)) {
                RedirectOutput(runTime, output, f);
            }

            string actualOutput = builder.ToString();

            if ((flags & OutputFlags.Raw) == 0) {
                actualOutput = actualOutput.Trim();
                expectedOutput = expectedOutput.Trim();
            }

            Assert.IsTrue(actualOutput == expectedOutput, "Unexpected output: '" +
                builder.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t") + "'.");
        }
        
        internal static void AreEqualArrays<T>(IList<T> expected, IList<T> actual) {
            Assert.AreEqual(actual.Count, expected.Count);
            for (int i = 0; i < actual.Count; i++) {
                Assert.AreEqual(actual[i], expected[i]);
            }
        }

        internal static void AreEqualIEnumerables<T>(IEnumerable<T> expected, IEnumerable<T> actual) {
            TestHelpers.AreEqualArrays(  new List<T>(expected).ToArray(), new List<T>(actual).ToArray());
        }

        internal static void AreEqualCollections<T>(T[] expected, IEnumerable<T> actual) {
            TestHelpers.AreEqualArrays(expected, (new List<T>(actual).ToArray()));
        }

        /// <summary>
        /// Create a temp file
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        internal static string CreateTempFile(string contents) {
            // TODO: Add temp file to a list for tear down(deletion)
            string tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, contents);
            return tempFile;
        }

        /// <summary>
        /// Create a temp source file
        /// </summary>
        /// <param name="contents">Contents of code</param>
        /// <param name="extention">File extension like ".py" or ".js"</param>
        /// <returns></returns>
        internal static string CreateTempSourceFile(string contents, string extention) {
            // TODO: Add temp file to a list for tear down(deletion)
            string tempFile = Path.GetTempFileName();
            string newFile = Path.ChangeExtension(tempFile, extention);
            File.WriteAllText(newFile, contents);
            return newFile;
        }


        public static AppDomain CreateAppDomain(string name) {
            return AppDomain.CreateDomain(name, null, BinDirectory, BinDirectory, false);
        }

        public class EnvSetupTearDown {
            string _envName;
            string _oldEnvEntry;

            public EnvSetupTearDown(string name, string newValue) {
                _envName = name;
                _oldEnvEntry = Environment.GetEnvironmentVariable(name);
                
                Environment.SetEnvironmentVariable(name, newValue);
            }
            ~EnvSetupTearDown() {
                //Rest old values
                Environment.SetEnvironmentVariable(_envName, _oldEnvEntry);
            }
        }
    }
}