using System;
using System.Collections.Generic;
using System.Text;

namespace IronPython.Runtime {
    [PythonType]
    public sealed class NullImporter {
        public static string __module__ = "imp";        // logically lives in imp, but physically lives in IronPython.dll so Importer.cs can access it

        public NullImporter(string path_string) {
        }

        public object find_module(params object[] args) {
            return null;
        }
    }
}
