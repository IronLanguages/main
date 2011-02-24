using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestRunner {
    public class TestResult {
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

        public bool IsFailure
        {
            get
            {
                return Status == TestResultStatus.Failed || Status == TestResultStatus.TimedOut;
            }
        }

        public bool WasRun
        {
            get
            {
                return Status != TestResultStatus.Disabled && Status != TestResultStatus.Skipped && Status != TestResultStatus.None;
            }
        }
    }

    public enum TestResultStatus {
        None,
        Passed,
        Failed,
        Skipped,
        TimedOut,
        Disabled,
    }
}
