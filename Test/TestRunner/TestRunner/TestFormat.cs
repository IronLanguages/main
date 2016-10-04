using System.Collections.Generic;
using System.Xml.Serialization;

namespace TestRunner
{
    [XmlRoot("Categories")]
    public class TestList {
        public List<TestCategory> Categories = new List<TestCategory>();
    }

    [XmlRoot("Tests")]
    public class TestCategory {
        public string Name;
        public List<Test> Tests = new List<Test>();
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
        public string Condition;
    }

    public class EnvironmentVariable {
        public string Name;
        public string Value;
    }
}
