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
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
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

            if (_driver.SaveToAssemblies) {
                Environment.SetEnvironmentVariable("DLR_AssembliesFileName", _testName);
            }

            var runtimeSetup = ScriptRuntimeSetup.ReadConfiguration();
            var languageSetup = runtimeSetup.AddRubySetup();

            runtimeSetup.DebugMode = _driver.IsDebug;
            runtimeSetup.PrivateBinding = testCase.Options.PrivateBinding;
            runtimeSetup.HostType = typeof(TestHost);
            runtimeSetup.HostArguments = new object[] { testCase.Options };
            languageSetup.Options["ApplicationBase"] = _driver.BaseDirectory;
            languageSetup.Options["NoAdaptiveCompilation"] = _driver.NoAdaptiveCompilation;
            languageSetup.Options["CompilationThreshold"] = _driver.CompilationThreshold;
            languageSetup.Options["Verbosity"] = 2;

            _runtime = Ruby.CreateRuntime(runtimeSetup);
            _engine = Ruby.GetEngine(_runtime);
            _context = (RubyContext)HostingHelpers.GetLanguageContext(_engine);
        }
    }

    public class TestHost : ScriptHost {
        private readonly OptionsAttribute/*!*/ _options;
        private readonly PlatformAdaptationLayer/*!*/ _pal;

        public TestHost(OptionsAttribute/*!*/ options) {
            _options = options;
            _pal = options.Pal != null ?
                (PlatformAdaptationLayer)Activator.CreateInstance(options.Pal) :
                PlatformAdaptationLayer.Default;
        }

        public override PlatformAdaptationLayer PlatformAdaptationLayer {
            get { return _pal; }
        }
    }

    public class Driver {

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
        private static bool _runPython = true;
        private readonly string/*!*/ _baseDirectory;

        public Driver(string/*!*/ baseDirectory) {
            _baseDirectory = baseDirectory;
        }

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

        private static bool ParseArguments(List<string>/*!*/ args) {
            if (args.Contains("/help") || args.Contains("-?") || args.Contains("/?") || args.Contains("-help")) {
                Console.WriteLine("Verbose                      : /verbose");
                Console.WriteLine("Partial trust                : /partial");
                Console.WriteLine("No adaptive compilation      : /noadaptive");
                Console.WriteLine("Synchronous compilation      : /sync0            (-X:CompilationThreshold 0)");
                Console.WriteLine("Synchronous compilation      : /sync1            (-X:CompilationThreshold 1)");
                Console.WriteLine("Interpret only               : /interpret        (-X:CompilationThreshold Int32.MaxValue)");
                Console.WriteLine("Save to assemblies           : /save");
                Console.WriteLine("Debug Mode                   : /debug");
                Console.WriteLine("Disable Python interop tests : /py-");
                Console.WriteLine("Run Specific Tests           : [/exclude] [test_to_run ...]");
                Console.WriteLine("List Tests                   : /list");
                Console.WriteLine("Tokenizer baseline           : /tokenizer <target-dir> <sources-file>");
                Console.WriteLine("Productions dump             : /tokenizer /prod <target-dir> <sources-file>");
                Console.WriteLine("Benchmark                    : /tokenizer /bm <target-dir> <sources-file>");
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

        public static void Main(string[]/*!*/ arguments) {
            List<string> args = new List<string>(arguments);
            string culture = Environment.GetEnvironmentVariable("IR_CULTURE");

            if (args.Contains("/partial")) {
                Console.WriteLine("Running in partial trust");

                PermissionSet ps = CreatePermissionSet();
                AppDomainSetup setup = new AppDomainSetup();

                setup.ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                AppDomain domain = AppDomain.CreateDomain("Tests", null, setup, ps);

                Loader loader = new Loader(args, setup.ApplicationBase);
                domain.DoCallBack(new CrossAppDomainDelegate(loader.Run));
                
                Environment.ExitCode = loader.ExitCode;
            } else {
                if (!String.IsNullOrEmpty(culture)) {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(culture, false);
                }
                Environment.ExitCode = Run(args, AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        public sealed class Loader : MarshalByRefObject {
            public int ExitCode;
            public readonly List<string>/*!*/ Args;
            public readonly string/*!*/ BaseDirectory;

            public Loader(List<string>/*!*/ args, string/*!*/ baseDirectory) {
                Args = args;
                BaseDirectory = baseDirectory;
            }

            public void Run() {
                ExitCode = Driver.Run(Args, BaseDirectory);
            }
        }

        private static PermissionSet/*!*/ CreatePermissionSet() {
#if CLR2
            string name = "Internet";
            bool foundName = false;
            PermissionSet setIntersection = new PermissionSet(PermissionState.Unrestricted);

            // iterate over each policy level
            IEnumerator e = SecurityManager.PolicyHierarchy();
            while (e.MoveNext()) {
                PolicyLevel level = (PolicyLevel)e.Current;
                PermissionSet levelSet = level.GetNamedPermissionSet(name);
                if (levelSet != null) {
                    foundName = true;
                    setIntersection = setIntersection.Intersect(levelSet);
                }
            }

            if (setIntersection == null || !foundName) {
                setIntersection = new PermissionSet(PermissionState.None);
            } else {
                setIntersection = new NamedPermissionSet(name, setIntersection);
            }

            return setIntersection;
#else
            // this functionality is not available on Mono (AddHostEvidence is undefined), use dynamic to resolve it at runtime
            dynamic e = new Evidence();
            e.AddHostEvidence(new Zone(SecurityZone.Internet));
            return SecurityManager.GetStandardSandbox((Evidence)e);
#endif
        }       

        public static int Run(List<string>/*!*/ args, string/*!*/ baseDirectory) {
            if (Thread.CurrentThread.CurrentCulture.ToString() != "en-US") {
                Console.WriteLine("Current culture: {0}", Thread.CurrentThread.CurrentCulture);
            }

            if (!ParseArguments(args)) {
                return -3;
            }

            int status = 0;

            if (_runTokenizerDriver) {
                TokenizerTestDriver driver = new TokenizerTestDriver((RubyContext)HostingHelpers.GetLanguageContext(Ruby.CreateEngine()));
                if (!driver.ParseArgs(args)) {
                    return -3;
                }

                status = driver.RunTests();
            } else {
                InitializeDomain();
                Driver driver = new Driver(baseDirectory);
                
                if (Manual.TestCode.Trim().Length == 0) {
                    status = driver.RunUnitTests(args);
                } else {
                    driver.RunManualTest();

                    // for case the test is forgotten, this would fail the test suite:
                    status = -2;
                }
            }

            // return failure on bad filter (any real failures throw)
            return status;
        }

        private static void InitializeDomain() {
            if (_saveToAssemblies) {
                string _dumpDir = Path.Combine(Path.GetTempPath(), "RubyTests");

                if (Directory.Exists(_dumpDir)) {
                    Array.ForEach(Directory.GetFiles(_dumpDir), delegate(string file) {
                        try { File.Delete(Path.Combine(_dumpDir, file)); } catch { /* nop */ }
                    });
                } else {
                    Directory.CreateDirectory(_dumpDir);
                }
                    
                Console.WriteLine("Generating binaries to {0}", _dumpDir);

                Snippets.SetSaveAssemblies(true, _dumpDir);
            }
        }
        
        private void RunManualTest() {
            Console.WriteLine("Running hardcoded test case");
            
            if (Manual.ParseOnly) {
                _testRuntime = new TestRuntime(this, new TestCase { Name = "<manual>" });
                Tests.GetRubyTokens(_testRuntime.Context, new LoggingErrorSink(false), Manual.TestCode, !Manual.DumpReductions, Manual.DumpReductions);                
            } else {
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
        }

        private int RunUnitTests(List<string>/*!*/ largs) {

            _tests = new Tests(this);
            
            if (_displayList) {
                for (int i = 0; i < _tests.TestMethods.Length; i++) {
                    Console.WriteLine(_tests.TestMethods[i].Method.Name);
                }
                return -1;
            }

            // check whether there is a preselected case:
            IList<TestCase> selectedCases = new List<TestCase>();

            foreach (var m in _tests.TestMethods) {
                if (m.Method.IsDefined(typeof(RunAttribute), false)) {
                    AddTestCases(selectedCases, m);
                }
            }

            if (selectedCases.Count == 0 && largs.Count > 0) {
                foreach (var m in _tests.TestMethods) {
                    bool caseIsSpecified = largs.Contains(m.Method.Name);
                    if ((caseIsSpecified && !_excludeSelectedCases) || (!caseIsSpecified && _excludeSelectedCases)) {
                        AddTestCases(selectedCases, m);
                    }
                }
            } else if (selectedCases.Count > 0 && largs.Count > 0) {
                Console.WriteLine("Arguments overrided by Run attribute.");
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

                    Console.Error.WriteLine();
                    if (_partialTrust) {
                        ColorWrite(ConsoleColor.Red, "{0}) {1}", failedCases.Count, test);
                    } else {
                        ColorWrite(ConsoleColor.Red, "{0}) {1} {2} : {3}", failedCases.Count, test, frame.GetFileName(), frame.GetFileLineNumber());
                    }
                    Console.Error.WriteLine(message);
                }
            }

            if (_unexpectedExceptions.Count > 0) {
                for (int i = 0; i < _unexpectedExceptions.Count; i++) {
                    string test = _unexpectedExceptions[i].Item000;
                    Exception exception = _unexpectedExceptions[i].Item001;

                    Console.Error.WriteLine();
                    ColorWrite(ConsoleColor.Red, "{0}) {1} (unexpected exception)", failedCases.Count, test);
                    Console.Error.WriteLine(exception);
                    failedCases.Add(test);
                }
            }

            if (failedCases.Count == 0) {
                ColorWrite(ConsoleColor.Green, "PASSED");
            } else {
                Console.WriteLine();
                // TODO:
                if (!_partialTrust) { 
                    Console.Write("Repro: {0}", Environment.CommandLine);
                } else {
                    Console.Write("Repro: IronRuby.Tests.exe /partial{0}{1}", 
                        _noAdaptiveCompilation ? " /noadaptive" : "",
                        _isDebug ? " /debug" : "");
                }
                if (largs.Count == 0) {
                    Console.Write(" {0}", String.Join(" ", failedCases.ToArray()));
                }
                Console.WriteLine();
            }
            return failedCases.Count;
        }

        private void AddTestCases(IList<TestCase>/*!*/ cases, Action/*!*/ testMethod) {
            var attrs = testMethod.Method.GetCustomAttributes(typeof(OptionsAttribute), false);
            if (attrs.Length > 0) {
                foreach (OptionsAttribute options in attrs) {
                    cases.Add(new TestCase {
                        Name = testMethod.Method.Name,
                        TestMethod = testMethod,
                        Options = options,
                    });
                }
            } else {
                cases.Add(new TestCase {
                    Name = testMethod.Method.Name,
                    TestMethod = testMethod,
                    Options = new OptionsAttribute(),
                });
            }
        }

        private void RunTestCase(TestCase/*!*/ testCase) {
            _testRuntime = new TestRuntime(this, testCase);

            if (_verbose) {
                Console.WriteLine("Executing {0}", testCase.Name);
            } else {
                Console.Write('.');
            }

            try {
                testCase.TestMethod();
            } catch (Exception e) {
                PrintTestCaseFailed();
                _unexpectedExceptions.Add(new MutableTuple<string, Exception>(testCase.Name, e));
            } finally {
                Snippets.SaveAndVerifyAssemblies();
            }
        }

        private void PrintTestCaseFailed() {
            ColorWrite(ConsoleColor.Red, "\n> FAILED: {0}", _testRuntime.TestName);
        }

        internal static void ColorWrite(ConsoleColor color, string/*!*/ str, params object[] args) {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(str, args);
            Console.ForegroundColor = oldColor;
        }

        [DebuggerHiddenAttribute]
        internal void AssertionFailed(string/*!*/ msg) {
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
            PrintTestCaseFailed();
        }

        internal string/*!*/ MakeTempDir() {
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

        internal sealed class TempFile : IDisposable {
            private readonly string/*!*/ _file;

            public TempFile(string/*!*/ file) {
                Assert.NotNull(file);
                _file = file;
            }

            public void Dispose() {
                File.Delete(_file);
            }
        }
    }
}
