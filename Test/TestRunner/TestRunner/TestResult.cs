using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestRunner {
    class TestResult {
        public readonly Test Test;
        public readonly TestResultStatus Status;
        public readonly double EllapsedSeconds;
        public readonly List<string> Output;

        public TestResult(Test test, TestResultStatus testResultStatus, double ellapsedSeconds, List<string> output) {
            Test = test;
            Status = testResultStatus;
            EllapsedSeconds = ellapsedSeconds;
            Output = output;
        }
    }

    enum TestResultStatus {
        None,
        Passed,
        Failed,
        Skipped,
        TimedOut,
        Disabled,
    }
}
