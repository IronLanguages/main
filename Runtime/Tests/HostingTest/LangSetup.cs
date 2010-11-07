using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace HostingTest{

    internal struct LangSetup {

        internal string[] Names{get;private set;}
        internal string[] Extensions { get; private set; }
        internal string DisplayName { get; private set; }
        internal string TypeName { get; private set; }
        internal string AssemblyString { get; private set; }

        public override string ToString() {
            return string.Format("<language names=\"{0}\" extensions=\"{1}\" displayName=\"{2}\" type=\"{3}, {4}\"/>",
                        GetAsString(Names), GetAsString(Extensions), DisplayName, TypeName, AssemblyString);
        }

        private string GetAsString(string[] items) {
            if (items == null) return "";

            StringBuilder retString = new StringBuilder();
            foreach (var item in items) {
                if (item != null) {
                    if (retString.Length != 0)
                        retString.Append(',');
                    retString.Append(item);
                }
            }

            return retString.ToString();
        }

        internal LangSetup(string[] names, string[] exts, string displayName, string typeName, string assemblyString):this() {
            Names = names; Extensions = exts; DisplayName = displayName; TypeName = typeName;
            AssemblyString = assemblyString;
        }

        static LangSetup() {
            Python = new LangSetup( new[] { "IronPython","Python","py" },new[] { ".py" }, "IronPython 2.6",
                "IronPython.Runtime.PythonContext", "IronPython, Version=2.7.0.10, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
            );

            Ruby = new LangSetup( new[] { "IronRuby", "Ruby", "rb" }, new[] { ".rb" },"IronRuby",
                "IronRuby.Runtime.RubyContext", "IronRuby, Version=1.1.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
            );
        }

        internal static LangSetup Python, Ruby;
    }
}
