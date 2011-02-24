using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using System.Text;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using NUnit.Framework;
using System.Diagnostics;

namespace HostingTest {

    [TestFixture]
    public partial class ScriptRuntimeSetupTest {

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Cons_NoArg1() {
            var srs = new ScriptRuntimeSetup();
            Assert.AreEqual(0, srs.LanguageSetups.Count);

            var sr = new ScriptRuntime(srs);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Cons_NoArg2() {
            var srs = new ScriptRuntimeSetup();
            var sr = new ScriptRuntime(srs);

            Assert.Fail("shouldn't be able to create a runtime without any langsetups");
        }

        [Test]
        public void ReadConfiguration_All4Langs() {
            var srs = ScriptRuntimeSetup.ReadConfiguration(TestHelpers.StandardConfigFile);
            Assert.AreEqual(2, srs.LanguageSetups.Count);

            var runtime = new ScriptRuntime(srs);
            foreach (var lang in srs.LanguageSetups) {
                Assert.IsTrue((lang.Names != null) && (lang.Names.Count != 0));

                //ensure this doesn't throw
                var engine = runtime.GetEngine(lang.Names[0]);
                Assert.AreEqual(lang.DisplayName, engine.Setup.DisplayName);
            }
        }

        [Test]
        public void ReadConfiguration_1Lang() {
            string configFile = GetTempConfigFile(new[]{LangSetup.Python});

            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);
            Assert.AreEqual(1, srs.LanguageSetups.Count);

            var sr = new ScriptRuntime(srs);
            Assert.AreEqual(1, sr.Setup.LanguageSetups.Count);

            var pythonEngine = sr.GetEngine("py");
            Assert.IsTrue(pythonEngine.IsValidPythonEngine());
        }

        [Test]
        public void ReadConfiguration_Multiple() {
            string configFile = GetTempConfigFile(new[] { LangSetup.Python });
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);
            Assert.AreEqual(1, srs.LanguageSetups.Count);

            var sr = new ScriptRuntime(srs);
            Assert.AreEqual(1, sr.Setup.LanguageSetups.Count);

            //create a config file, srs and runtime with 2 langsetups
            configFile = GetTempConfigFile(new[] { LangSetup.Python, LangSetup.Ruby });
            var srs2 = ScriptRuntimeSetup.ReadConfiguration(configFile);
            Assert.AreEqual(2, srs2.LanguageSetups.Count);

            var sr2 = new ScriptRuntime(srs2);
            Assert.AreEqual(2, sr2.Setup.LanguageSetups.Count);

            //older ones still have only 1 lang
            Assert.AreEqual(1, srs.LanguageSetups.Count);
            Assert.AreEqual(1, sr.Setup.LanguageSetups.Count);
        }

        [Test]
        public void ReadConfiguration_DuplicateLang() {
            string configFile = GetTempConfigFile(new[] { LangSetup.Python, LangSetup.Python });
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);
            Assert.AreEqual(1, srs.LanguageSetups.Count);

            var sr = new ScriptRuntime(srs);
            Assert.AreEqual(1, sr.Setup.LanguageSetups.Count);
        }

        [Test]
        public void ReadConfiguration_Multi_SameTypeDifferentName() {
            LangSetup py1 = LangSetup.Python;
            LangSetup py2 = new LangSetup( new[]{"NewPython"}, py1.Extensions, py1.DisplayName,
                                py1.TypeName, py1.AssemblyString);

            string configFile = GetTempConfigFile(new[] { py1, py2 });
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);
            Assert.AreEqual(1, srs.LanguageSetups.Count);

            var sr = new ScriptRuntime(srs);
            Assert.AreEqual(1, sr.Setup.LanguageSetups.Count);
            Assert.AreEqual("NewPython", sr.Setup.LanguageSetups[0].Names[0]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadConfiguration_Multi_SameNameDifferentType() {
            LangSetup py1 = LangSetup.Python;
            LangSetup py2 = new LangSetup( py1.Names, py1.Extensions, py1.DisplayName, 
                                LangSetup.Ruby.TypeName, LangSetup.Ruby.AssemblyString);

            string configFile = GetTempConfigFile(new[] { py1, py2 });
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);
            Assert.AreEqual(2, srs.LanguageSetups.Count);

            var sr = new ScriptRuntime(srs);
            Assert.Fail("some exception should have been thrown");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadConfiguration_DuplicateExtensions() {
            LangSetup py1 = LangSetup.Python;
            LangSetup py2 = new LangSetup(py1.Names, LangSetup.Ruby.Extensions, py1.DisplayName,
                                py1.TypeName, py1.AssemblyString);

            string configFile = GetTempConfigFile(new[] { py2, LangSetup.Ruby });
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);
            Assert.AreEqual(2, srs.LanguageSetups.Count);

            var sr = new ScriptRuntime(srs);
            var eng = sr.GetEngine("py");
            Assert.Fail("some exception should have been thrown");
        }

        [Test]
        [ExpectedException(typeof( ArgumentException))]
        public void ReadConfiguration_DuplicateNames() {
            LangSetup py1 = LangSetup.Python;
            LangSetup py2 = new LangSetup(LangSetup.Ruby.Names, py1.Extensions, py1.DisplayName,
                                py1.TypeName, py1.AssemblyString);

            string configFile = GetTempConfigFile(new[] { py2, LangSetup.Ruby });
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);
            Assert.AreEqual(2, srs.LanguageSetups.Count);

            var sr = new ScriptRuntime(srs);
            Assert.Fail("some exception should have been thrown");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadConfiguration_MissingAssembly() {
            LangSetup lang = new LangSetup(new[]{"SomeName"}, new[]{".sn"}, "Somename",
                    "SomeLang.Runtime.LangContext", 
                    "SomeLang, Version=8.0.0.5050, Culture=neutral, PublicKeyToken=31345fgsd4344e35");

            string configFile = GetTempConfigFile(new[] { lang});
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);

            //this should throw..error message should be meaningful
            var sr = new ScriptRuntime(srs);
            Assert.Fail("some exception should have been thrown");
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadConfiguration_MisMatchedTypeAssembly() {
            LangSetup py1 = LangSetup.Python;
            LangSetup py2 = new LangSetup(py1.Names, py1.Extensions, py1.DisplayName,
                                LangSetup.Ruby.TypeName, py1.AssemblyString);

            string configFile = GetTempConfigFile(new[] { py2});
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);
            
            var sr = new ScriptRuntime(srs);
            var eng = sr.GetEngine("py");
            Assert.Fail("some exception should have been thrown");
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadConfiguration_IncorrectType() {
            LangSetup py1 = LangSetup.Python;
            LangSetup py2 = new LangSetup(py1.Names, py1.Extensions, py1.DisplayName,
                                "IronPython.Runtime.PythonBuffer", py1.AssemblyString);

            string configFile = GetTempConfigFile(new[] { py2 });
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);

            var sr = new ScriptRuntime(srs);
            var eng = sr.GetEngine("py");

            Assert.Fail("some exception should have been thrown");
        }

        [Test]
        public void ReadConfiguration_EmptyExtensions() {
            LangSetup py1 = LangSetup.Python;
            LangSetup py2 = new LangSetup(py1.Names, new[]{"",""}, py1.DisplayName,
                                py1.TypeName, py1.AssemblyString);

            string configFile = GetTempConfigFile(new[] { py2 });
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);
            var runtime = new ScriptRuntime(srs);

            Assert.AreEqual(5, runtime.GetEngine("py").Execute("2+3"));
        }

        [Test]
        public void ReadConfiguration_DuplicateEmptyExtensions() {

            LangSetup rb1 = new LangSetup(LangSetup.Ruby.Names, new[] { "", "" }, LangSetup.Ruby.DisplayName,
                    LangSetup.Ruby.TypeName, LangSetup.Ruby.AssemblyString);

            LangSetup py2 = new LangSetup(LangSetup.Python.Names, new[] { "", "" }, LangSetup.Python.DisplayName,
                                LangSetup.Python.TypeName, LangSetup.Python.AssemblyString);

            string configFile = GetTempConfigFile(new[] { rb1, py2 });
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);

            var sr = new ScriptRuntime(srs);
            var eng = sr.GetEngine("py");
            var eng2 = sr.GetEngine("rb");

            Assert.AreEqual(5, eng.Execute("2+3"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadConfiguration_EmptyConfigEntries() {
            string configFile = GetTempConfigFile(new[]{new LangSetup()});
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);

            var sr = new ScriptRuntime(srs);
            Assert.Fail("some exception should have been thrown");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadConfiguration_EmptySetup() {
            var srs = new ScriptRuntimeSetup();
            var sr = new ScriptRuntime(srs);

            Assert.AreEqual(0, sr.Setup.LanguageSetups.Count);
            Assert.Fail("some exception should have been thrown");
        }

        [Test]
        public void Configuration_MutateAndCheck() {
            string configFile = GetTempConfigFile(new[] { LangSetup.Python });

            var sr = new ScriptRuntime(ScriptRuntimeSetup.ReadConfiguration(configFile));
            var config1 = sr.Setup;
            config1 = null;

            Assert.IsNotNull(sr.Setup);
        }

        [Test]
        public void Configuration_MutateAndCheck2() {
            string configFile = GetTempConfigFile(new[] { LangSetup.Python });

            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);
            var sr = new ScriptRuntime(srs);

            var config1 = sr.Setup;
            var config2 = new ScriptRuntime(ScriptRuntimeSetup.ReadConfiguration(GetTempConfigFile(new[] { LangSetup.Ruby }))).Setup;

            Assert.AreEqual(LangSetup.Python.DisplayName, config1.LanguageSetups[0].DisplayName);
            TestHelpers.AreEqualArrays(LangSetup.Python.Names, config1.LanguageSetups[0].Names);
        }

        [Test]
        public void Configuration_MutateAndCheck3() {
            string configFile = GetTempConfigFile(new[] { LangSetup.Python });

            var sr = new ScriptRuntime(ScriptRuntimeSetup.ReadConfiguration(configFile));
            var eng = sr.GetEngine("py");

            var config = eng.Setup;
            config = null;

            Assert.IsNotNull(eng.Setup);
        }

        [Test]
        public void ReadConfiguration_NullDisplayName() {
            LangSetup py2 = new LangSetup(LangSetup.Python.Names, LangSetup.Python.Extensions, null,
                                LangSetup.Python.TypeName, LangSetup.Python.AssemblyString);

            string configFile = GetTempConfigFile(new[] { py2 });
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);
            var eng = new ScriptRuntime(srs).GetEngine("py");

            Assert.AreEqual("", eng.Setup.DisplayName);
        }

        [Test]
        public void ReadConfiguration_EmptyDisplayName() {
            LangSetup py2 = new LangSetup(LangSetup.Python.Names, LangSetup.Python.Extensions, "",
                                LangSetup.Python.TypeName, LangSetup.Python.AssemblyString);

            string configFile = GetTempConfigFile(new[] { py2 });
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);
            var eng = new ScriptRuntime(srs).GetEngine("py");

            Assert.AreEqual("", eng.Setup.DisplayName);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadConfiguration_EmptyTypeName() {
            LangSetup py2 = new LangSetup(LangSetup.Python.Names, LangSetup.Python.Extensions, LangSetup.Python.DisplayName,
                                "", LangSetup.Python.AssemblyString);

            string configFile = GetTempConfigFile(new[] { py2 });
            var srs = ScriptRuntimeSetup.ReadConfiguration(configFile);
            var eng = new ScriptRuntime(srs).GetEngine("py");

            Assert.AreEqual(LangSetup.Python.Names[0], eng.Setup.DisplayName);
        }
    }
}
