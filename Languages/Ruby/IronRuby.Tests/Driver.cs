/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using Microsoft.Scripting;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Globalization;
using System.Threading;
using Microsoft.Scripting.Hosting.Providers;
using System.Runtime.InteropServices;

#if WIN8
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace IronRuby.Tests {
    public class TestCase {
        public Action TestMethod { get; set; }
        public OptionsAttribute Options { get; set; }
        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RunAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Method)]
    [Serializable]
    public sealed class OptionsAttribute : Attribute {
        public bool PrivateBinding { get; set; }
        public bool NoRuntime { get; set; }
        public Type Pal { get; set; }
    }

    public class TestRuntime {
        private readonly Driver/*!*/ _driver;
        private readonly string/*!*/ _testName;
        private readonly ScriptRuntime _runtime;
        private readonly ScriptEngine _engine;
        private readonly RubyContext _context;

        public string/*!*/ TestName { get { return _testName; } }
        public ScriptEngine Engine { get { return _engine; } }
        public ScriptRuntime ScriptRuntime { get {  return _runtime; } }
        public RubyContext Context { get { return _context; } }

        public TestRuntime(Driver/*!*/ driver, TestCase/*!*/ testCase) {
            _driver = driver;
            _testName = testCase.Name;

            if (testCase.Options.NoRuntime) {
                return;
            }
            
#if !WIN8
            if (_driver.SaveToAssemblies) {
                Environment.SetEnvironmentVariable("DLR_AssembliesFileName", _testName);
            }
#endif
            _engine = _driver.CreateRubyEngine(testCase.Options.PrivateBinding, testCase.Options);
            _runtime = _engine.Runtime;
            _context = (RubyContext)HostingHelpers.GetLanguageContext(_engine);
        }
    }
    
    public class TestHost : ScriptHost {
        private readonly OptionsAttribute/*!*/ _options;
        private readonly PlatformAdaptationLayer/*!*/ _pal;

        public TestHost(OptionsAttribute/*!*/ options) {
            _options = options;
            _pal = options.Pal != null ? (PlatformAdaptationLayer)Activator.CreateInstance(options.Pal) :
                   Driver.IsWin8 ? new Win8PAL() :
                   PlatformAdaptationLayer.Default;
        }

        public override PlatformAdaptationLayer PlatformAdaptationLayer {
            get { return _pal; }
        }

        public class Win8PAL : PlatformAdaptationLayer
        {
            private string cwd;

            public Win8PAL()
            {
                StringBuilder buffer = new StringBuilder(300);
                if (GetCurrentDirectory(buffer.Capacity, buffer) == 0) {
                    throw new IOException();
                }

                cwd = buffer.ToString();
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
            internal static extern int GetCurrentDirectory(int nBufferLength, [Out] StringBuilder lpBuffer);

            public override Assembly LoadAssembly(string name)
            {
                if (name.StartsWith("mscorlib")) {
                    return typeof(object).GetTypeInfo().Assembly;
                }
                
                if (name == "IronRuby, Version=1.1.4.0, Culture=neutral, PublicKeyToken=7f709c5b713576e1") {
                    return typeof(Ruby).GetTypeInfo().Assembly;
                }
                
                if (name == "IronRuby.Libraries, Version=1.1.4.0, Culture=neutral, PublicKeyToken=7f709c5b713576e1") {
                    return typeof(IronRuby.Builtins.Integer).GetTypeInfo().Assembly;
                }

                return base.LoadAssembly(name);
            }

            public override string CurrentDirectory {
                get { return cwd; }
                set { cwd = value; }
            }

            public override bool FileExists(string path) {
                return false;
            }

            public override bool DirectoryExists(string path) {
                return false;
            }
        }
    }

    public class Driver {

#if WIN8
        public class StackFrame
        {
            public string GetFileName()
            {
                return "N/A";
            }

            public int GetFileLineNumber()
            {
                return 0;
            }
        }
#endif

        private Tests _tests;

        private readonly List<MutableTuple<string, StackFrame, string, object>>/*!*/ _failedAssertions = new List<MutableTuple<string, StackFrame, string, object>>();
        private readonly List<MutableTuple<string, Exception>>/*!*/ _unexpectedExceptions = new List<MutableTuple<string, Exception>>();

        private static TestRuntime _testRuntime; 
        private static bool _excludeSelectedCases;
        private static bool _verbose;
        private static bool _isDebug;
        private static bool _saveToAssemblies;
        private static bool _runTokenizerDriver;
        private static bool _displayList;
        private static bool _partialTrust;
        private static bool _noAdaptiveCompilation;
        private static int _compilationThreshold;
#if WIN8
        // TODO: enable when IronPython is ported to WIN8
        private static bool _runPython = false;
#else
        private static bool _runPython = true;
#endif
        private readonly string _baseDirectory;

        public Driver(string baseDirectory) {
            _baseDirectory = baseDirectory;
        }

#if FEATURE_REFEMIT
        public const bool FeatureRefEmit = true;
#else
        public const bool FeatureRefEmit = false;
#endif

        public TestRuntime TestRuntime {
            get { return _testRuntime; }
        }

        public List<MutableTuple<string, StackFrame, string, object>>/*!*/ FailedAssertions {
            get { return _failedAssertions; }
        }

        public List<MutableTuple<string, Exception>>/*!*/ UnexpectedExceptions {
            get { return _unexpectedExceptions; }
        }

        public bool Verbose {
            get { return _verbose; }
        }

        public bool IsDebug {
            get { return _isDebug; }
        }

        public bool SaveToAssemblies {
            get { return _saveToAssemblies; }
        }

        public bool PartialTrust {
            get { return _partialTrust; }
        }

        public bool NoAdaptiveCompilation {
            get { return _noAdaptiveCompilation; }
        }

        public int CompilationThreshold {
            get { return _compilationThreshold; }
        }

        public bool RunPython {
            get { return _runPython; }
        }

        public string BaseDirectory {
            get { return _baseDirectory; }
        }

        public ScriptEngine CreateRubyEngine(bool privateBinding = false, OptionsAttribute options = null)
        {
            var runtimeSetup = ScriptRuntimeSetup.ReadConfiguration();
            var languageSetup = runtimeSetup.AddRubySetup();

            runtimeSetup.DebugMode = IsDebug;
            runtimeSetup.PrivateBinding = privateBinding;
            runtimeSetup.HostType = typeof(TestHost);
            runtimeSetup.HostArguments = new object[] { options ?? new OptionsAttribute() };

            languageSetup.Options["ApplicationBase"] = BaseDirectory;
            languageSetup.Options["NoAdaptiveCompilation"] = NoAdaptiveCompilation;
            languageSetup.Options["CompilationThreshold"] = CompilationThreshold;
            languageSetup.Options["Verbosity"] = 2;

            var runtime = Ruby.CreateRuntime(runtimeSetup);
            return Ruby.GetEngine(runtime);
        }

#if WIN8
        private static TextWriter logWriter;
        private static Stream logStream;

        private static void EnsureOutputInitialized()
        {
            if (logWriter == null)
            {
                logStream = ConsoleOutputStream.Instance;
                logWriter = new StreamWriter(logStream);
            }
        }

        public static TextWriter Output
        {
            get
            {
                EnsureOutputInitialized();
                return logWriter;
            }
        }

        public static Stream OpenOutputStream()
        {
            EnsureOutputInitialized();
            return logStream;
        }

        public static void WriteOutput(string/*!*/ str, params object[] args)
        {
            Output.WriteLine(str, args);
            Output.Flush();
        }

        public static void WriteSuccess(string/*!*/ str, params object[] args)
        {
            Output.WriteLine(str, args);
            Output.Flush();
        }

        public static void WriteError(string/*!*/ str, params object[] args)
        {
            Output.WriteLine(str, args);
            Output.Flush();
        }

        public static void WriteWarning(string/*!*/ str, params object[] args)
        {
            Output.WriteLine(str, args);
            Output.Flush();
        }
#else
        public static TextWriter Output
        {
            get 
            { 
                return Console.Out; 
            }
        }

        public static Stream OpenOutputStream()
        {
            return Console.OpenStandardOutput();
        }

        public static void WriteOutput(string/*!*/ str, params object[] args)
        {
            Console.WriteLine(str, args);
        }

        public static void WriteSuccess(string/*!*/ str, params object[] args)
        {
            ColorWrite(ConsoleColor.Green, str, args);
        }

        public static void WriteError(string/*!*/ str, params object[] args)
        {
            ColorWrite(ConsoleColor.Red, str, args);
        }

        public static void WriteWarning(string/*!*/ str, params object[] args)
        {
            ColorWrite(ConsoleColor.Yellow, str, args);
        }

        internal static void ColorWrite(ConsoleColor color, string/*!*/ str, params object[] args)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(str, args);
            Console.ForegroundColor = oldColor;
        }
#endif

        private static bool ParseArguments(List<string>/*!*/ args) {
            if (args.Contains("/help") || args.Contains("-?") || args.Contains("/?") || args.Contains("-help")) {
                WriteOutput("Verbose                      : /verbose");
                WriteOutput("Partial trust                : /partial");
                WriteOutput("No adaptive compilation      : /noadaptive");
                WriteOutput("Synchronous compilation      : /sync0            (-X:CompilationThreshold 0)");
                WriteOutput("Synchronous compilation      : /sync1            (-X:CompilationThreshold 1)");
                WriteOutput("Interpret only               : /interpret        (-X:CompilationThreshold Int32.MaxValue)");
                WriteOutput("Save to assemblies           : /save");
                WriteOutput("Debug Mode                   : /debug");
                WriteOutput("Disable Python interop tests : /py-");
                WriteOutput("Run Specific Tests           : [/exclude] [test_to_run ...]");
                WriteOutput("List Tests                   : /list");
                WriteOutput("Tokenizer baseline           : /tokenizer <target-dir> <sources-file>");
                WriteOutput("Productions dump             : /tokenizer /prod <target-dir> <sources-file>");
                WriteOutput("Benchmark                    : /tokenizer /bm <target-dir> <sources-file>");
            }

            if (args.Contains("/list")) {
                _displayList = true;
                return true;
            }

            if (args.Contains("/verbose")) {
                args.Remove("/verbose");
                _verbose = true;
            }

            if (args.Contains("/debug")) {
                args.Remove("/debug");
                _isDebug = true;
            }

            if (args.Contains("-D")) {
                args.Remove("-D");
                _isDebug = true;
            }

            if (args.Contains("/save")) {
                args.Remove("/save");
                _saveToAssemblies = true;
            }

            if (args.Contains("/partial")) {
                args.Remove("/partial");
                _partialTrust = true;
            }

            if (args.Contains("-X:NoAdaptiveCompilation")) {
                args.Remove("-X:NoAdaptiveCompilation");
                _noAdaptiveCompilation = true;
            }

            if (args.Contains("/noadaptive")) {
                args.Remove("/noadaptive");
                _noAdaptiveCompilation = true;
            }

            if (args.Contains("/sync0")) {
                args.Remove("/sync0");
                _compilationThreshold = 0;
            }

            if (args.Contains("/sync1")) {
                args.Remove("/sync1");
                _compilationThreshold = 1;
            }

            if (args.Contains("/interpret")) {
                args.Remove("/interpret");
                _compilationThreshold = Int32.MaxValue;
            }

            if (args.Contains("/py-")) {
                args.Remove("/py-");
                _runPython = false;
            }

            if (args.Contains("/py")) {
                args.Remove("/py");
                _runPython = true;
            }

            if (args.Contains("/exclude")) {
                _excludeSelectedCases = true;
                args.Remove("/exclude");
            }

            if (args.Contains("/tokenizer")) {
                args.Remove("/tokenizer");
                _runTokenizerDriver = true;
            }

            return true;
        }

        public static int Main(string[]/*!*/ arguments) {
            List<string> args = new List<string>(arguments);
            try
            {
#if WIN8
                return Run(args, baseDirectory: null);
#else
                string culture = Environment.GetEnvironmentVariable("IR_CULTURE");

                if (args.Contains("/partial")) {
                    WriteOutput("Running in partial trust");
                    return PartialTrustDriver.Run(args); 
                }
                if (!String.IsNullOrEmpty(culture)) {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(culture, false);
                }

                return Run(args, AppDomain.CurrentDomain.BaseDirectory);
#endif
            }
            finally
            {
                Output.Flush();
            }
        }

        public static int Run(List<string>/*!*/ args, string baseDirectory) {
#if !WIN8
            if (Thread.CurrentThread.CurrentCulture.ToString() != "en-US") {
                WriteOutput("Current culture: {0}", Thread.CurrentThread.CurrentCulture);
            }
#endif
            if (!ParseArguments(args)) {
                return -3;
            }

#if !WIN8
            if (_runTokenizerDriver) {
                TokenizerTestDriver tokenizerDriver = new TokenizerTestDriver((RubyContext)HostingHelpers.GetLanguageContext(Ruby.CreateEngine()));
                if (!tokenizerDriver.ParseArgs(args)) {
                    return -3;
                }

                return tokenizerDriver.RunTests();
            } 
#endif
            InitializeDomain();
            Driver driver = new Driver(baseDirectory);
                
            if (Manual.TestCode.Trim().Length == 0) {
                return driver.RunUnitTests(args);
            } 

#if !WIN8
            driver.RunManualTest();
#endif
            // for case the test is forgotten, this would fail the test suite:
            return -2;
        }

        private static void InitializeDomain() {
#if !WIN8
            if (_saveToAssemblies) {
                string _dumpDir = Path.Combine(Path.GetTempPath(), "RubyTests");

                if (Directory.Exists(_dumpDir)) {
                    Array.ForEach(Directory.GetFiles(_dumpDir), delegate(string file) {
                        try { File.Delete(Path.Combine(_dumpDir, file)); } catch { /* nop */ }
                    });
                } else {
                    Directory.CreateDirectory(_dumpDir);
                }
                    
                WriteOutput("Generating binaries to {0}", _dumpDir);

                Snippets.SetSaveAssemblies(true, _dumpDir);
            }
#endif
        }

#if !WIN8
        private void RunManualTest() {
            Output.WriteLine("Running hardcoded test case");
            if (Manual.ParseOnly) {
                _testRuntime = new TestRuntime(this, new TestCase { Name = "<manual>" });
                Tests.GetRubyTokens(_testRuntime.Context, new LoggingErrorSink(false), Manual.TestCode, !Manual.DumpReductions, Manual.DumpReductions);
                return;
            } 

            try {
                for (int i = 0; i < Manual.RequiredFiles.Length; i += 2) {
                    File.CreateText(Manual.RequiredFiles[i]).WriteLine(Manual.RequiredFiles[i + 1]);
                }

                _tests = new Tests(this);
                RunTestCase(new TestCase() {
                    Name = "$manual$",
                    TestMethod = () => _tests.CompilerTest(Manual.TestCode),
                });

            } finally {
                for (int i = 0; i < Manual.RequiredFiles.Length; i += 2) {
                    try {
                        File.Delete(Manual.RequiredFiles[i]);
                    } catch {
                        // nop
                    }
                }
            }
        }
#endif

        private int RunUnitTests(List<string>/*!*/ largs) {

            _tests = new Tests(this);
            
            if (_displayList) {
                for (int i = 0; i < _tests.TestMethods.Length; i++) {
                    Output.WriteLine(_tests.TestMethods[i].GetMethodInfo().Name);
                }
                return -1;
            }

            // check whether there is a preselected case:
            IList<TestCase> selectedCases = new List<TestCase>();

            foreach (var m in _tests.TestMethods) {
                if (m.GetMethodInfo().IsDefined(typeof(RunAttribute), false)) {
                    AddTestCases(selectedCases, m);
                }
            }

            if (selectedCases.Count == 0 && largs.Count > 0) {
                foreach (var m in _tests.TestMethods) {
                    bool caseIsSpecified = largs.Contains(m.GetMethodInfo().Name);
                    if ((caseIsSpecified && !_excludeSelectedCases) || (!caseIsSpecified && _excludeSelectedCases)) {
                        AddTestCases(selectedCases, m);
                    }
                }
            } else if (selectedCases.Count > 0 && largs.Count > 0) {
                Output.WriteLine("Arguments overrided by Run attribute.");
            } else if (selectedCases.Count == 0 && largs.Count == 0) {
                foreach (var m in _tests.TestMethods) {
                    AddTestCases(selectedCases, m);
                }
            }

            foreach (TestCase testCase in selectedCases) {
                RunTestCase(testCase);
            }

            var failedCases = new List<string>();
            if (_failedAssertions.Count > 0) {
                for (int i = 0; i < _failedAssertions.Count; i++) {
                    string test = _failedAssertions[i].Item000;
                    StackFrame frame = _failedAssertions[i].Item001;
                    string message = _failedAssertions[i].Item002;
                    failedCases.Add(test);

                    Output.WriteLine();
                    if (_partialTrust) {
                        WriteError("{0}) {1}", failedCases.Count, test);
                    } else {
                        WriteError("{0}) {1} {2} : {3}", failedCases.Count, test, frame.GetFileName(), frame.GetFileLineNumber());
                    }

                    Output.WriteLine(message);
                }
            }

            if (_unexpectedExceptions.Count > 0) {
                for (int i = 0; i < _unexpectedExceptions.Count; i++) {
                    string test = _unexpectedExceptions[i].Item000;
                    Exception exception = _unexpectedExceptions[i].Item001;

                    Output.WriteLine();
                    WriteError("{0}) {1} (unexpected exception)", failedCases.Count, test);
                    Output.WriteLine(exception);
                    failedCases.Add(test);
                }
            }

            if (failedCases.Count == 0) {
                WriteSuccess("PASSED");
            } else {
                Output.WriteLine();
                // TODO:
                if (!_partialTrust) {
#if WIN8
                    Output.Write("Repro: {0}", string.Join(" ", largs));
#else
                    Output.Write("Repro: {0}", Environment.CommandLine);
#endif
                } else {
                    Output.Write("Repro: IronRuby.Tests.exe /partial{0}{1}", 
                        _noAdaptiveCompilation ? " /noadaptive" : "",
                        _isDebug ? " /debug" : "");
                }
                if (largs.Count == 0) {
                    Output.Write(" {0}", String.Join(" ", failedCases.ToArray()));
                }
                Output.WriteLine();
            }
            return failedCases.Count;
        }

        private void AddTestCases(IList<TestCase>/*!*/ cases, Action/*!*/ testMethod) {
            var attrs = testMethod.GetMethodInfo().GetCustomAttributes<OptionsAttribute>();
            if (attrs.Any()) {
                foreach (OptionsAttribute options in attrs) {
                    cases.Add(new TestCase {
                        Name = testMethod.GetMethodInfo().Name,
                        TestMethod = testMethod,
                        Options = options,
                    });
                }
            } else {
                cases.Add(new TestCase {
                    Name = testMethod.GetMethodInfo().Name,
                    TestMethod = testMethod,
                    Options = new OptionsAttribute(),
                });
            }
        }

        private void RunTestCase(TestCase/*!*/ testCase) {
            _testRuntime = new TestRuntime(this, testCase);

            if (_verbose) {
                Output.WriteLine("Executing {0}", testCase.Name);
            } else {
                Output.Write('.');
            }

            try {
                testCase.TestMethod();
            } catch (Exception e) {
                PrintTestCaseFailed();
                _unexpectedExceptions.Add(new MutableTuple<string, Exception>(testCase.Name, e));
            } finally {
#if !WIN8
                Snippets.SaveAndVerifyAssemblies();
#endif
            }
        }

        private void PrintTestCaseFailed() {
            WriteError("\n> FAILED: {0}", _testRuntime.TestName);
        }

        [DebuggerHiddenAttribute]
        internal void AssertionFailed(string/*!*/ msg) {
#if !WIN8
            var trace = new StackTrace(true);
            StackFrame frame = null;
            for (int i = 0; i < trace.FrameCount; i++) {
                frame = trace.GetFrame(i);
                var method = frame.GetMethod();
                if (!method.IsDefined(typeof(DebuggerHiddenAttribute), true)) {
                    break;
                }
            }

            Debug.Assert(frame != null);

            _failedAssertions.Add(new MutableTuple<string, StackFrame, string, object>(_testRuntime.TestName, frame, msg, null));
#endif
            PrintTestCaseFailed();
        }

#if WIN8
        public static readonly bool IsWin8 = true;

        internal string/*!*/ MakeTempDir() {
            throw new NotSupportedException();
        }

        internal TempFile/*!*/ MakeTempFile(string/*!*/ globalVariableName, string/*!*/ suffix, string/*!*/ content) {
            throw new NotSupportedException();
        }

        internal static string/*!*/ MakeTempFile(string/*!*/ suffix, string/*!*/ content) {
            throw new NotSupportedException();
        }
#else
        public static readonly bool IsWin8 = false;

        internal string/*!*/ MakeTempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), _testRuntime.TestName);
            Directory.CreateDirectory(dir);
            return dir;
        }

        internal TempFile/*!*/ MakeTempFile(string/*!*/ globalVariableName, string/*!*/ suffix, string/*!*/ content) {
            var fileName = Driver.MakeTempFile(suffix, content);
            TestRuntime.Context.DefineGlobalVariable(globalVariableName, fileName);
            return new TempFile(fileName);
        }

        internal static string/*!*/ MakeTempFile(string/*!*/ suffix, string/*!*/ content) {
            var dir = Path.GetTempPath();
            int pid = Process.GetCurrentProcess().Id;

            while (true) {
                string path = Path.Combine(dir, "IR_" + pid + "_" + DateTime.Now.Ticks.ToString("X") + suffix);
                if (!File.Exists(path)) {
                    try {
                        using (var file = File.Open(path, FileMode.CreateNew)) {
                            var writer = new StreamWriter(file);
                            writer.Write(content);
                            writer.Close();
                        }
                        return path;
                    } catch (IOException) {
                        // nop
                    }
                }
            }
        }
#endif
        internal sealed class TempFile : IDisposable {
            private readonly string/*!*/ _file;

            public TempFile(string/*!*/ file) {
                Assert.NotNull(file);
                _file = file;
            }

            public void Dispose() {
#if !WIN8
                File.Delete(_file);
#endif
            }
        }
    }
}
