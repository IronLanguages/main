using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading;
using System.Reflection;

namespace TestRunner {
    class Program {
        private bool _verbose, _runLongRunning, _admin, _quiet;
        private int _threadCount = 6;
        private List<TestResult> _results = new List<TestResult>();

        static int Main(string[] args) {
            return new Program().MainBody(args);
        }

        int MainBody(string[] args) {
            if (args.Length == 0) {
                Help();
                return -1;
            }

            List<string> inputFiles = new List<string>();
            List<string> categories = new List<string>();
            List<string> tests = new List<string>();

            // try and define DLR_ROOT if not already defined (makes it easier to debug TestRunner in an IDE)
            string dlrRoot = Environment.GetEnvironmentVariable("DLR_ROOT");
            if (dlrRoot == null) {
                dlrRoot = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..\\..\\..\\..\\..");
                Environment.SetEnvironmentVariable("DLR_ROOT", dlrRoot);
            }

            // Default to debug binaries
            string binPath = Path.Combine(dlrRoot, "bin\\Debug");
            bool runAll = false;
            string nunitOutputPath = null;

            // parse the options
            for (int i = 0; i < args.Length; i++) {
                if (args[i].StartsWith("/category:")) {
                    categories.Add(args[i].Substring("/category:".Length));
                } else if (args[i].StartsWith("/test:")) {
                    tests.Add(args[i].Substring("/test:".Length));
                } else if (args[i].StartsWith("/binpath:")) {
                    binPath = Path.Combine(dlrRoot, args[i].Substring("/binpath:".Length));
                }
                else if (args[i].StartsWith("/nunitoutput:"))
                {
                    nunitOutputPath = args[i].Substring("/nunitoutput:".Length);
                } else if (args[i] == "/verbose") {
                    _verbose = true;
                } else if (args[i] == "/runlong") {
                    _runLongRunning = true;
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

                    Console.WriteLine("Use /all to run all tests, /category:Catname to run a category, ");
                    Console.WriteLine("or /test: to run an individual test.  Multiple category and options");
                    Console.WriteLine("can be used.");
                }
                return -1;
            }

            // Set DLR_BIN, some tests expect this to be defined.
            Environment.SetEnvironmentVariable("DLR_BIN", binPath);

            DateTime start = DateTime.Now;
            // Run the tests
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;

            List<Test> testList = new List<Test>();
            List<Test> notParallelSafe = new List<Test>();
            foreach (var list in testLists) {
                foreach (var cat in list.Categories) {
                    if (runAll || ShouldRunCategory(cat.Name, categories)) {
                        foreach (var test in cat.Tests) {
                            test.Category = cat;

                            if (runAll || ShouldRunTest(test, tests)) {
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

            // start the test running threads
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
                if (_verbose) {
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
                var resultsWriter = new NUnitResultsWriter(_results, inputFiles.First(), elapsedTime);
                resultsWriter.Save(nunitOutputPath);
            }


            Console.WriteLine("Total time: {0} seconds", elapsedTime.TotalSeconds);

            Console.ForegroundColor = originalColor;

            return failures.Count;
        }

        private void RunTestForConsole(Test test) {
            var result = RunTest(test);

            lock (this) {
                if (!_quiet) {
                    Console.Write("{0,-100}", test.Category.Name + " " + test.Name);

                    const string resultFormat = "{0,-10}";
                    switch (result.Status) {
                        case TestResultStatus.Skipped:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(resultFormat, "SKIPPED");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                        case TestResultStatus.TimedOut:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(resultFormat, "TIMEOUT");
                            break;
                        case TestResultStatus.Passed:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(resultFormat, "PASSED");
                            break;
                        case TestResultStatus.Failed:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(resultFormat, "FAILED");
                            break;
                        case TestResultStatus.Disabled:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write(resultFormat, "DISABLED");
                            break;
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(result.EllapsedSeconds);
                }

                if (result.IsFailure) {
                    DisplayFailure(test, result);
                }

                _results.Add(result);
            }
        }

        private void DisplayFailure(Test test, TestResult result) {
            if (_verbose && !_quiet) {
                Console.WriteLine("Repro:");
                if (test.EnvironmentVariables != null) {
                    foreach (var envVar in test.EnvironmentVariables) {
                        Console.WriteLine("SET {0}={1}", envVar.Name, envVar.Value);
                    }
                }
                Console.WriteLine("CD /D {0}", test.WorkingDirectory);
                Console.WriteLine("{0} {1}", test.Filename, test.Arguments);

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
            if (test.Disabled) {
                return new TestResult(test, TestResultStatus.Disabled, 0, null);
            } else if ((test.LongRunning && !_runLongRunning) || (test.RequiresAdmin && !_admin)) {
                return new TestResult(test, TestResultStatus.Skipped, 0, null);
            }

            // start the process
            DateTime startTime = DateTime.Now;
            var process = Process.Start(CreateProcessInfoFromTest(test));

            // create the reader threads
            List<string> output = new List<string>();
            Thread outThread = new Thread(() => {
                while (!process.HasExited) {
                    var line = process.StandardOutput.ReadLine();
                    if (line != null) {
                        lock (output) {
                            output.Add(line);
                        }
                    }
                }
            });

            Thread errThread = new Thread(() => {
                while (!process.HasExited) {
                    var line = process.StandardError.ReadLine();
                    if (line != null) {
                        lock (output) {
                            output.Add(line);
                        }
                    }
                }
            });

            outThread.Start();
            errThread.Start();

            // wait for it to exit
            if (test.MaxDuration > 0) {
                process.WaitForExit(test.MaxDuration);
            } else {
                process.WaitForExit();
            }

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

            outThread.Join();
            errThread.Join();

            return new TestResult(test, status, (DateTime.Now - startTime).TotalSeconds, output);
        }

        private static ProcessStartInfo CreateProcessInfoFromTest(Test test) {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.Arguments = Environment.ExpandEnvironmentVariables(test.Arguments);
            psi.WorkingDirectory = Environment.ExpandEnvironmentVariables(test.WorkingDirectory);
            psi.FileName = Environment.ExpandEnvironmentVariables(test.Filename);
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

        private static bool ShouldRunTest(Test test, List<string> tests) {
            if (tests.Count == 0) {
                return true;
            }

            foreach (var testName in tests) {
                if (test.Name == testName) {
                    return true;
                }
            }
            return false;
        }

        private static bool ShouldRunCategory(string catName, List<string> categories) {
            if (categories.Count == 0) {
                return true;
            }

            foreach (var catToRun in categories) {
                if (catName.StartsWith(catToRun)) {
                    return true;
                }
            }

            return false;
        }

        private static void Help() {
            Console.WriteLine("Usage: ");
            Console.WriteLine("TestRunner.exe (inputFile ...) [/threads:6] [/quiet] [/admin] [/binpath:dir] [/runlong] [/verbose] [/nunitoutput:file] (/all | ([/category:CatName] | [/test:testName])+)");
        }
    }
}
