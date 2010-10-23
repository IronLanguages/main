/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.PyAnalysis;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using IronPython.Compiler;
using IronPython;
using IronPython.Compiler.Ast;

namespace AnalysisTest {
    public partial class AnalysisTest {
        [PerfMethod]
        public void TestLookupPerf_Namespaces() {
            var entry = ProcessText(@"
import System
            ");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000; i++) {
                foreach (var varRef in entry.GetMembers("System", 1)) {
                    foreach (var innerRef in entry.GetMembers("System." + varRef.Name, 1)) {
                    }
                }
            }
            sw.Stop();
            Console.WriteLine("{0} ms", sw.ElapsedMilliseconds);
        }

        [PerfMethod]
        public void TestParsePerf_Decimal() {
            string merlin = Environment.GetEnvironmentVariable("DLR_ROOT") ?? @"C:\Product\0\dlr";
            var text = File.ReadAllText(Path.Combine(merlin + @"\External.LCA_RESTRICTED\Languages\IronPython\27\Lib\decimal.py"));

            var sourceUnit = GetSourceUnit(text);
            var projectState = new ProjectState(_engine);
            Stopwatch sw = new Stopwatch();
            var entry = ParseText(projectState, sourceUnit, "decimal");

            sw.Start();
            for (int i = 0; i < 5; i++) {
                Prepare(entry, sourceUnit);
                entry.Analyze();
            }

            sw.Stop();
            Console.WriteLine("{0}", sw.ElapsedMilliseconds);
        }

        private static ProjectEntry ParseText(ProjectState state, SourceUnit sourceUnit, string moduleName) {
            var newEntry = state.AddModule(moduleName, moduleName, null);

            Prepare(newEntry, sourceUnit);

            foreach (var entry in state.ProjectEntries) {
                entry.Analyze();
            }

            return newEntry;
        }


        [PerfMethod]
        public void TestLookupPerf_Modules_Class() {
            var entry = ProcessText(@"
import System
            ");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (var result in entry.ProjectState.GetModuleMembers(new[] { "System", "IO", "BinaryReader" })) {
            }
            foreach (var result in entry.ProjectState.GetModuleMembers(new[] { "System", "IO", "BinaryWriter" })) {
            }
            foreach (var result in entry.ProjectState.GetModuleMembers(new[] { "System", "IO", "BufferedStream" })) {
            }
            foreach (var result in entry.ProjectState.GetModuleMembers(new[] { "System", "IO", "Stream" })) {
            }
            foreach (var result in entry.ProjectState.GetModuleMembers(new[] { "System", "IO", "Directory" })) {
            }
            foreach (var result in entry.ProjectState.GetModuleMembers(new[] { "System", "IO", "File" })) {
            }
            foreach (var result in entry.ProjectState.GetModuleMembers(new[] { "System", "IO", "FileStream" })) {
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        [PerfMethod]
        public void TestLookupPerf_Namespaces2() {
            var entry = ProcessText(@"
import System
            ");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var varRef in entry.GetMembers("System.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Collections.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Collections.Generic.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.CodeDom.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Configuration.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.ComponentModel.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Deployment.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Diagnostics.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Dynamic.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Globalization.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Linq.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Management.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Media.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Net.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Runtime.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Security.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Text.", 1)) {
            }
            foreach (var varRef in entry.GetMembers("System.Threading.", 1)) {
            }

            sw.Stop();
            Console.WriteLine("{0} ms", sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Gets all members from a large number of types
        /// </summary>
        [PerfMethod]
        public void TestLookupPerf_Types() {
            var entry = ProcessText(@"
import System
            ");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var varRef in entry.GetMembers("System.", 1)) {
                foreach (var innerRef in entry.GetMembers("System." + varRef.Name + ".", 1)) {
                }
            }
            sw.Stop();
            Console.WriteLine("{0} ms", sw.ElapsedMilliseconds);
        }

        [PerfMethod]
        public void TestLookupPerf_BuiltinModules() {
            var builtin_module_names = new[] { "sys", "__builtin__", "exceptions", "clr", "future_builtins", "imp", "array", "binascii", "_sha512", "cmath", "_winreg", "_weakref", "_warnings", "_sre", "_random", "_functools", "xxsubtype", "time", "thread", "_struct", "_heapq", "_ctypes_test", "_ctypes", "socket", "_sha256", "_sha", "select", "re", "operator", "nt", "_md5", "_fileio", "math", "marshal", "_locale", "itertools", "gc", "errno", "datetime", "cStringIO", "cPickle", "copy_reg", "_collections", "_bytesio", "_codecs" };
            StringBuilder text = new StringBuilder();
            foreach (var name in builtin_module_names) {
                text.AppendLine("import " + name);
            }
            var entry = ProcessText(text.ToString());

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 50; i++) {
                foreach (var name in builtin_module_names) {
                    foreach (var varRef in entry.GetMembers(name, 1)) {
                        foreach (var innerRef in entry.GetMembers(name + "." + varRef.Name, 1)) {
                        }
                    }
                }
            }
            sw.Stop();
            Console.WriteLine("{0} ms", sw.ElapsedMilliseconds);
        }

        [PerfMethod]
        public void TestAnalyzeStdLib() {
            //string dir = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "IronPython 2.6 for .NET 4.0 RC\\Lib");
            string dir = Path.Combine("C:\\Python27\\Lib");
            List<string> files = new List<string>();
            CollectFiles(dir, files);

            List<SourceUnit> sourceUnits = new List<SourceUnit>();
            foreach (string file in files) {
                sourceUnits.Add(
                    new SourceUnit(
                        HostingHelpers.GetLanguageContext(_engine),
                        new FileTextContentProvider(new FileStreamContentProvider(file)),
                        Path.GetFileNameWithoutExtension(file),
                        SourceCodeKind.File
                    )
                );
            }

            Stopwatch sw = new Stopwatch();

            sw.Start();
            long start0 = sw.ElapsedMilliseconds;
            var projectState = new ProjectState(_engine);
            List<ProjectEntry> modules = new List<ProjectEntry>();
            PythonOptions EmptyOptions = new PythonOptions();
            foreach (var sourceUnit in sourceUnits) {
                modules.Add(projectState.AddModule(Path.GetFileNameWithoutExtension(sourceUnit.Path), sourceUnit.Path, null));
            }
            long start1 = sw.ElapsedMilliseconds;
            Console.WriteLine("AddSourceUnit: {0} ms", start1 - start0);

            List<PythonAst> nodes = new List<PythonAst>();
            for (int i = 0; i < modules.Count; i++) {
                PythonAst ast = null;
                try {
                    var sourceUnit = sourceUnits[i];
                    
                    var context = new CompilerContext(sourceUnit, HostingHelpers.GetLanguageContext(_engine).GetCompilerOptions(), ErrorSink.Null);
                    ast = Parser.CreateParser(context, EmptyOptions).ParseFile(false);
                } catch (Exception) {
                }
                nodes.Add(ast);
            }
            long start2 = sw.ElapsedMilliseconds;
            Console.WriteLine("Parse: {0} ms", start2 - start1);

            for (int i = 0; i < modules.Count; i++) {
                var ast = nodes[i];

                if (ast != null) {
                    modules[i].UpdateTree(ast, null);
                }
            }

            long start3 = sw.ElapsedMilliseconds;
            for (int i = 0; i < modules.Count; i++) {
                Console.WriteLine("Analyzing {1}: {0} ms", sw.ElapsedMilliseconds - start3, sourceUnits[i].Path);
                var ast = nodes[i];
                if (ast != null) {
                    modules[i].Analyze();
                }
            }
            long start4 = sw.ElapsedMilliseconds;
            Console.WriteLine("Analyze: {0} ms", start4 - start3);
            Console.ReadLine();
        }

        internal sealed class FileTextContentProvider : TextContentProvider {
            private readonly FileStreamContentProvider _provider;

            public FileTextContentProvider(FileStreamContentProvider fileStreamContentProvider) {
                _provider = fileStreamContentProvider;
            }

            public override SourceCodeReader GetReader() {
                return new SourceCodeReader(new StreamReader(_provider.GetStream(), Encoding.ASCII), Encoding.ASCII);
            }
        }

        internal sealed class FileStreamContentProvider : StreamContentProvider {
            private readonly string _path;

            internal string Path {
                get { return _path; }
            }

            #region Construction

            internal FileStreamContentProvider(string path) {
                _path = path;
            }

            #endregion

            public override Stream GetStream() {
                return new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
        }


        private static void CollectFiles(string dir, List<string> files) {
            foreach (string file in Directory.GetFiles(dir)) {
                if (file.EndsWith(".py", StringComparison.OrdinalIgnoreCase)) {
                    files.Add(file);
                }
            }
            foreach (string nestedDir in Directory.GetDirectories(dir)) {
                CollectFiles(nestedDir, files);
            }
        }
    }
}
