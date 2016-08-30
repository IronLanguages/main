using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronPython.Runtime {
    public class PythonFrozen {
        private static readonly byte[] M__hello__ = {
            99,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,
            0,115,9,0,0,0,100,0,0,71,72,100,1,0,83,40,
            2,0,0,0,115,14,0,0,0,72,101,108,108,111,32,119,
            111,114,108,100,46,46,46,78,40,0,0,0,0,40,0,0,
            0,0,40,0,0,0,0,40,0,0,0,0,115,8,0,0,
            0,104,101,108,108,111,46,112,121,115,1,0,0,0,63,1,
            0,0,0,115,0,0,0,0, };

        private static readonly int SIZE = PythonFrozen.M__hello__.Length;

        public string Name { get; private set; }
        public byte[] Code { get; private set; }
        public int Size { get; private set; }

        public static IEnumerable<PythonFrozen> FrozenModules {
            get {
                return _frozenModules;
            }
        }

        public static PythonFrozen FindFrozen(string name) {
            foreach (PythonFrozen f in FrozenModules) {
                if (string.Compare(f.Name, name) == 0) {
                    return f;
                }
            }
            return null;
        }

        private static PythonFrozen[] _frozenModules = {
            new PythonFrozen { Name = "__hello__", Code = M__hello__, Size = SIZE },
            new PythonFrozen { Name = "__phello__", Code = M__hello__, Size = -SIZE },
            new PythonFrozen { Name = "__phello__.spam", Code = M__hello__, Size = SIZE } };
    }
}