using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Reflection;
using System.Globalization;
using System.IO;

namespace TestRunner
{
    public class NUnitResultsWriter
    {
        public List<TestResult> Results { get; set; }
        public string InputFile { get; set; }
        public TimeSpan TotalTime { get; set; }

        const string ElapsedTimeFormat = "0.000";

        public NUnitResultsWriter(List<TestResult> results, string inputFile, TimeSpan totalTestTime)
        {
            Results = results;
            InputFile = inputFile;
            TotalTime = totalTestTime;
        }

        public void Save(string path)
        {
            var testResults = CreateTestResults();

            var categories = from t in Results
                             group t by t.Test.Category.Name into g
                             select new { Name = g.Key, Results = g.ToList() };

            var testSuiteResults = testResults.Descendants().Last();

            foreach(var category in categories)
                testSuiteResults.Add(CreateCategory(category.Name, category.Results));

            testResults.Save(path);
        }

        XDocument CreateTestResults()
        {
            var now = DateTime.Now;
            
            return new XDocument(new XElement("test-results",
                        new XAttribute("name", Assembly.GetExecutingAssembly().Location),
                        new XAttribute("total", Results.Count),
                        new XAttribute("errors", 0),
                        new XAttribute("failures", Results.Count(t => t.IsFailure)),
                        new XAttribute("not-run", Results.Count(t => !t.WasRun)),
                        new XAttribute("inconclusive", 0),
                        new XAttribute("ignored", Results.Count(t => t.Status == TestResultStatus.Disabled)),
                        new XAttribute("skipped", Results.Count(t => t.Status == TestResultStatus.Skipped)),
                        new XAttribute("invalid", 0),
                        new XAttribute("date", now.ToString("yyyy-MM-dd")),
                        new XAttribute("time", now.ToString("HH:mm:ss")),
                        new XElement("environment",
                            new XAttribute("nunit-version", new Version(0, 0, 0, 0)),
                            new XAttribute("clr-version", Environment.Version),
                            new XAttribute("os-version", Environment.OSVersion),
                            new XAttribute("platform", Environment.OSVersion.Platform),
                            new XAttribute("cwd", Environment.CurrentDirectory),
                            new XAttribute("machine-name", Environment.MachineName),
                            new XAttribute("user", Environment.UserName),
                            new XAttribute("user-domain", Environment.UserDomainName)
                        ),
                        new XElement("culture-info",
                            new XAttribute("current-culture", CultureInfo.CurrentCulture),
                            new XAttribute("current-uiculture", CultureInfo.CurrentUICulture)
                        ),
                        CreateTestSuiteResults("TestRunner", InputFile, TotalTime.TotalSeconds, Results)
                    ));
        }

        XElement CreateTestSuiteResults(string type, string name, double time, IEnumerable<TestResult> results)
        {
            var anyFailures = results.Any(t => t.IsFailure);
            return new XElement("test-suite",
                        new XElement("results"),
                        new XAttribute("type", type),
                        new XAttribute("name", name),
                        new XAttribute("executed", "True"),
                        new XAttribute("result", anyFailures ? "Failure" : "Success"),
                        new XAttribute("success", anyFailures ? "True" : "False"),
                        new XAttribute("time", time.ToString(ElapsedTimeFormat)),
                        new XAttribute("asserts", results.Count(r => r.WasRun))
                    );
        }

        XElement CreateCategory(string categoryName, List<TestResult> testResults)
        {
            var testSuite = CreateTestSuiteResults("Category", categoryName, testResults.Sum(t => t.EllapsedSeconds), testResults);
            var results = testSuite.Descendants().First();

            foreach (var testResult in testResults)
            {
                var result = "Error";
                switch (testResult.Status) {
                    case TestResultStatus.Disabled:
                        result = "Ignored";
                        break;
                    case TestResultStatus.Skipped:
                        result = "Skipped";
                        break;
                    case TestResultStatus.TimedOut:
                    case TestResultStatus.Failed:
                        result = "Failure";
                        break;
                    case TestResultStatus.Passed:
                        result = "Success";
                        break;
                }

                var testCase = new XElement("test-case",
                                 new XAttribute("name", testResult.Test.Name),
                                 new XAttribute("executed", testResult.WasRun ? "True" : "False"),
                                 new XAttribute("result", result)
                               );

                if (testResult.WasRun)
                {
                    testCase.Add(new XAttribute("success", !testResult.IsFailure ? "True" : "False"),
                                 new XAttribute("time", testResult.EllapsedSeconds.ToString(ElapsedTimeFormat)),
                                 new XAttribute("asserts", "1"));
                }

                if (testResult.IsFailure)
                {
                    var message = string.Join(Environment.NewLine, testResult.Output);
                    testCase.Add(new XElement("failure"), new XElement("message", new XCData(message)));
                }

                results.Add(testCase);
            }

            return testSuite;
        }
    }
}
