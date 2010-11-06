using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace TestRunner {
    [XmlRoot("Categories")]
    public class TestList {
        public List<TestCategory> Categories = new List<TestCategory>();
    }

    [XmlRoot("Tests")]
    public class TestCategory {
        public string Name;
        public List<Test> Tests = new List<Test>();
        public TestCategory() {
        }

        public TestCategory(string name) {
            Name = name;
        }
    }

    public class Test {
        public string Name;
        public string Filename;
        public string Arguments;
        public int MaxDuration;
        public bool LongRunning, Disabled, RequiresAdmin, NotParallelSafe;
        public string WorkingDirectory;
        public EnvironmentVariable[] EnvironmentVariables;
        public TestCategory Category;

        public Test() { }
        public Test(string name, string filename, string workingDir, string strArgs, int maxDuration, EnvironmentVariable[] envVars, bool longRunning, bool disabled, bool requiersAdmin, bool notParallelSafe) {
            Name = name;
            Filename = filename;
            WorkingDirectory = workingDir;
            Arguments = strArgs;
            MaxDuration = maxDuration;
            EnvironmentVariables = envVars;
            LongRunning = longRunning;
            Disabled = disabled;
            RequiresAdmin = requiersAdmin;
            NotParallelSafe = notParallelSafe;
        }
    }

    public class EnvironmentVariable {
        public string Name;
        public string Value;

        public EnvironmentVariable() {
        }
        public EnvironmentVariable(string name, string value) {
            Name = name;
            Value = value;
        }
    }
}
