using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;

namespace TestRunner
{
    class Program {
        private bool _verbose, _runLongRunning, _runDisabled, _admin, _quiet, _isUnix = false;
        private int _threadCount = 1;
        private List<TestResult> _results = new List<TestResult>();

        static int Main(string[] args) {
            return new Program().MainBody(args);
        }

        int MainBody(string[] args) {
            if (args.Length == 0) {
                Help();
                return -1;
            }

            _isUnix = Environment.OSVersion.Platform == PlatformID.Unix;

            // try and define DLR_ROOT if not already defined (makes it easier to debug TestRunner in an IDE)
            string dlrRoot = Environment.GetEnvironmentVariable("DLR_ROOT");

            if (dlrRoot == null) {
                dlrRoot = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../../../..");
                Environment.SetEnvironmentVariable("DLR_ROOT", dlrRoot);
            }

            // Parse the options
            List<string> inputFiles = new List<string>();
            List<string> categories = new List<string>();
            List<string> tests = new List<string>();
            bool runAll = false;
            string binPath = Path.Combine(dlrRoot, "bin", "Debug"); // Default to debug binaries
            string nunitOutputPath = null;

            for (int i = 0; i < args.Length; i++) {
                if (args[i].StartsWith("/category:")) {
                    categories.Add(args[i].Substring("/category:".Length));
                } else if (args[i].StartsWith("/test:")) {
                    tests.Add(args[i].Substring("/test:".Length));
                } else if (args[i].StartsWith("/binpath:")) {
                    binPath = Path.Combine(dlrRoot, args[i].Substring("/binpath:".Length));
                } else if (args[i].StartsWith("/nunitoutput:")) {
                    nunitOutputPath = args[i].Substring("/nunitoutput:".Length);
                } else if (args[i] == "/verbose") {
                    _verbose = true;
                } else if (args[i] == "/runlong") {
                    _runLongRunning = true;
                } else if (args[i] == "/rundisabled") {
                    _runDisabled = true;
                } else if (args[i] == "/admin") {
                    _admin = true;
                } else if (args[i] == "/quiet") {
                    _quiet = true;
                } else if (args[i].StartsWith("/threads:")) {
                    int threadCount;
                    if (!Int32.TryParse(args[i].Substring("/threads:".Length), out threadCount) || threadCount <= 0) {
                        Console.WriteLine("Bad thread count: {0}", args[i].Substring("/threads:".Length));
                        return -1;
                    }
                    _threadCount = threadCount; 
                } else if (args[i] == "/all") {
                    runAll = true;
                } else if(File.Exists(args[i])) {
                    inputFiles.Add(args[i]);
                } else {
                    Console.WriteLine("Unknown option: {0}", args[i]);
                    Help();
                    return -1;
                }
            }

            // Read the test list
            XmlSerializer serializer = new XmlSerializer(typeof(TestList));
            List<TestList> testLists = new List<TestList>();
            foreach (var file in inputFiles) {
                using (var fs = new FileStream(args[0], FileMode.Open, FileAccess.Read)) {
                    testLists.Add((TestList)serializer.Deserialize(fs));
                }
            }

            if (!runAll && categories.Count == 0 && tests.Count == 0) {
                Console.WriteLine("Available categories: ");
                foreach (var list in testLists) {
                    foreach (var cat in list.Categories) {
                        Console.WriteLine("    " + cat.Name);
                    }

                    Console.WriteLine("Use /all to run all tests, /category:pattern to run categories, /test:pattern");
                    Console.WriteLine("to run individual tests. Multiple category and test options can be combined.");
                }
                return -1;
            }

            // Filter the test list
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;

            var testNamesRegex = CompileGlobPatternsToRegex(tests);
            var categoryNamesRegex = CompileGlobPatternsToRegex(categories, startsWith: true);

            List<Test> testList = new List<Test>();
            List<Test> notParallelSafe = new List<Test>();
            foreach (var list in testLists) {
                foreach (var cat in list.Categories) {
                    if (runAll || categoryNamesRegex.IsMatch(cat.Name)) {
                        foreach (var test in cat.Tests) {
                            test.Category = cat;

                            if (runAll || testNamesRegex.IsMatch(test.Name)) {
                                if (test.NotParallelSafe) {
                                    notParallelSafe.Add(test);
                                } else {
                                    testList.Add(test);
                                }
                            }
                        }
                    }
                }
            }

            // Set DLR_BIN, some tests expect this to be defined.
            Environment.SetEnvironmentVariable("DLR_BIN", binPath);

            // start the test running threads
            DateTime start = DateTime.Now;

            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < _threadCount; i++) {
                Thread t = new Thread(() => {
                    Test curTest;

                    for(;;) {
                        lock (testList) {
                            if (testList.Count == 0) {
                                break;
                            }

                            curTest = testList[testList.Count - 1];
                            testList.RemoveAt(testList.Count - 1);
                        }

                        RunTestForConsole(curTest);
                    }
                });
                t.Start();
                threads.Add(t);
            }
            
            foreach (var thread in threads) {
                thread.Join();
            }

            foreach (var test in notParallelSafe) {
                RunTestForConsole(test);
            }

            var failures = _results.Where(r => r.IsFailure).ToList();

            if (failures.Count > 0) {
                if (!_quiet) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed test output:");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    foreach (var failedTest in failures) {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(failedTest.Test.Name);
                        Console.ForegroundColor = ConsoleColor.Gray;

                        foreach (var outputLine in failedTest.Output) {
                            Console.WriteLine(outputLine);
                        }
                    }
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed test summary:");
                Console.ForegroundColor = ConsoleColor.Gray;
                foreach (var failedTest in failures) {
                    Console.WriteLine(failedTest.Test.Name);
                }
            }

            var elapsedTime = (DateTime.Now - start);

            if (!string.IsNullOrWhiteSpace(nunitOutputPath))
            {
                var resultsWriter = new NUnitResultsWriter(_results, inputFiles.First(), elapsedTime, allMessages: _verbose);
                resultsWriter.Save(nunitOutputPath);
            }

            Console.WriteLine("Total time: {0} seconds", elapsedTime.TotalSeconds);
            Console.ForegroundColor = originalColor;

            return failures.Count;
        }

        private bool ConditionMatched(Test test) {
            bool result = true;
            if(!string.IsNullOrEmpty(test.Condition)) {
                try {
                    result = EvaluateExpression(test.Condition);
                } catch(Exception ex) {
                    Console.WriteLine("Error evaluating test condition '{0}', will run the test anyway: {1}", test.Condition, ex.Message);
                    result = true;
                }
            }

            return result;
        }

        private bool EvaluateExpression(string expression) {
            var dummy = new DataTable();
            string filter = expression;
            var replacements = new Dictionary<string, string>() {
                // variables
                { "$(OS)", Environment.OSVersion.Platform.ToString() },

                // operators
                { "==", "=" },
                { "||", "OR" },
                { "\"\"", "\"" },
                { "\"", "'" },
                { "&&", "AND" },
                { "!=", "<>" }
            };

            foreach (var replacement in replacements) {
                expression = expression.Replace(replacement.Key, replacement.Value);
            }

            try {
                object res = dummy.Compute(expression, null);
                if (res is bool) {
                    return (bool)res;
                }
            } catch (EvaluateException ex) {
                if (ex.Message.StartsWith("The expression contains undefined function call", StringComparison.Ordinal)) {
                    throw new Exception("A variable used in the filter expression is not defined");
                }
                throw new Exception(string.Format("Invalid filter: {0}", ex.Message));
            } catch(SyntaxErrorException ex) {
                throw new Exception(string.Format("Invalid filter: {0}", ex.Message));
            }

            throw new Exception(string.Format("Invalid filter, does not evaluate to true or false: {0}", filter));
        }

        private void RunTestForConsole(Test test) {
            lock (this) {
                if (!_quiet) {
                    Console.Write("{0,-80}", test.Category.Name + " " + test.Name);
                }
            }

            TestResult result = null;

            if(!string.IsNullOrEmpty(test.Condition) && !ConditionMatched(test)) {
                result = new TestResult(test, TestResultStatus.SkippedCondition, 0, null);
            } else {
                try {
                    result = RunTest(test);
                } catch (Exception e) {
                    result = new TestResult(test, TestResultStatus.Failed, 0, new List<string> { e.ToString() });
                }
            }

            lock (this) {
                if (!_quiet) {
                    const string resultFormat = "{0,-25}";
                    var originalColor = Console.ForegroundColor;
                    switch (result.Status) {
                        case TestResultStatus.Skipped:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(resultFormat, test.Disabled && _runDisabled ? "SKIPPED (DISABLED)" : "SKIPPED");
                            break;
                        case TestResultStatus.TimedOut:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(resultFormat, test.Disabled && _runDisabled ? "TIMEOUT (DISABLED)" : "TIMEOUT");
                            break;
                        case TestResultStatus.Passed:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(resultFormat, test.Disabled && _runDisabled ? "PASSED (DISABLED)" : "PASSED");
                            break;
                        case TestResultStatus.Failed:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(resultFormat, test.Disabled && _runDisabled ? "FAILED (DISABLED)" : "FAILED");
                            break;
                        case TestResultStatus.Disabled:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write(resultFormat, "DISABLED");
                            break;
                        case TestResultStatus.SkippedCondition:
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.Write(resultFormat, "SKIPPED (CONDITION)");
                            break;
                    }
                    Console.ForegroundColor = originalColor;
                    Console.WriteLine(result.EllapsedSeconds);
                }

                if (result.WasRun && (_verbose || (result.IsFailure && !_quiet))) {
                    DisplayOutput(test, result);
                }

                _results.Add(result);
            }
        }

        private void DisplayOutput(Test test, TestResult result) {
            Console.WriteLine("Repro:");
            if (test.EnvironmentVariables != null) {
                foreach (var envVar in test.EnvironmentVariables) {
                    Console.WriteLine("{0} {1}={2}", _isUnix ? "export" : "SET", envVar.Name, envVar.Value);
                }
            }

            if(_isUnix) {
                Console.WriteLine("cd {0}", test.WorkingDirectory.Replace("\\", "/"));
                Console.WriteLine("{0} {1}", test.Filename.Replace(".bat", ".sh").Replace("\\", "/"), test.Arguments.Replace("\\", "/"));
            } else {
                Console.WriteLine("CD /D {0}", test.WorkingDirectory);
                Console.WriteLine("{0} {1}", test.Filename, test.Arguments);
            }

            if (result != null && result.Output != null) {
                Console.WriteLine();
                Console.WriteLine("Result: ");
                foreach (var line in result.Output) {
                    Console.WriteLine("> {0}", line);
                }
            }
        }

        /// <summary>
        /// Runs a single test caseand returns the result.
        /// </summary>
        private TestResult RunTest(Test test) {
            if (test.Disabled && !_runDisabled) {
                return new TestResult(test, TestResultStatus.Disabled, 0, null);
            } else if ((test.LongRunning && !_runLongRunning) || (test.RequiresAdmin && !_admin)) {
                return new TestResult(test, TestResultStatus.Skipped, 0, null);
            }

            // start the process
            DateTime startTime = DateTime.Now;
            Process process = null;
            try {
                process = Process.Start(CreateProcessInfoFromTest(test, _isUnix));
            } catch (Win32Exception e) {
                return new TestResult(test, TestResultStatus.Failed, 0, new List<string> { e.Message });
            }
            
            // get the output asynchronously
            List<string> output = new List<string>();
            process.OutputDataReceived += (sender, e) => {
                var line = e.Data;
                if (line != null) {
                    lock (output) {
                        output.Add(line);
                    }
                }
            };
            process.BeginOutputReadLine();


            process.ErrorDataReceived += (sender, e) => {
                var line = e.Data;
                if (line != null) {
                    lock (output) {
                        output.Add(line);
                    }
                }
            };
            process.BeginErrorReadLine();

            // wait for it to exit
            if (test.MaxDuration > 0) {
                process.WaitForExit(test.MaxDuration);
            } else {
                process.WaitForExit();
            }

            process.CancelOutputRead();
            process.CancelErrorRead();

            // kill if it needed, save status
            TestResultStatus status;
            if (!process.HasExited) {
                status = TestResultStatus.TimedOut;
                process.Kill();
            } else if (process.ExitCode == 0) {
                status = TestResultStatus.Passed;
            } else {
                status = TestResultStatus.Failed;
            }

            return new TestResult(test, status, (DateTime.Now - startTime).TotalSeconds, output);
        }

        private static ProcessStartInfo CreateProcessInfoFromTest(Test test, bool isUnix) {
            ProcessStartInfo psi = new ProcessStartInfo();
            var args = test.Arguments.Contains("-X:Debug") ? test.Arguments : "-X:Debug " + test.Arguments;
            psi.Arguments = Environment.ExpandEnvironmentVariables(test.Arguments);
            psi.WorkingDirectory = Environment.ExpandEnvironmentVariables(test.WorkingDirectory);
            psi.FileName = Environment.ExpandEnvironmentVariables(test.Filename);
            if(isUnix) {
                if(string.Compare(Path.GetExtension(psi.FileName), ".bat", true) == 0) {
                    psi.FileName = Path.ChangeExtension(psi.FileName, ".sh").Replace("\\", "/");
                }
                psi.WorkingDirectory = psi.WorkingDirectory.Replace("\\", "/");
				psi.Arguments = psi.Arguments.Replace("\\", "/");
            }
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;

            if (test.EnvironmentVariables != null) {
                foreach (var envVar in test.EnvironmentVariables) {
                    psi.EnvironmentVariables.Add(envVar.Name, envVar.Value);
                }
            }
            return psi;
        }

        private static Regex CompileGlobPatternsToRegex(IEnumerable<string> patterns, bool startsWith = false) {
            var sb = new StringBuilder();

            foreach (var p in patterns) {
                if (sb.Length > 0)
                    sb.Append("|");

                sb.AppendFormat(
                    "(\\A{0}{1})",
                    Regex.Escape(p).Replace("\\*", ".*").Replace("\\?", "."),
                    startsWith ? "" : "\\Z");
            }

            return new Regex(sb.ToString(), RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        }

        private static void Help() {
            Console.WriteLine("Usage: ");
            Console.WriteLine("TestRunner.exe (inputFile ...) [/threads:6] [/quiet] [/admin] [/binpath:dir] [/runlong] [/verbose] [/nunitoutput:file] (/all | ([/category:pattern] | [/test:pattern])+)");
        }
    }
}
