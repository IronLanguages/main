/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using System.Security;
using IronPython.Runtime.Types;

[assembly: PythonModule("sys", typeof(IronPython.Runtime.SysModule))]
namespace IronPython.Runtime {
    public static class SysModule {
        public const int api_version = 0;
        // argv is set by PythonContext and only on the initial load
        public static readonly string byteorder = BitConverter.IsLittleEndian ? "little" : "big";
        // builtin_module_names is set by PythonContext and updated on reload
        public const string copyright = "Copyright (c) Microsoft Corporation. All rights reserved.";

        static SysModule() {
#if SILVERLIGHT
            prefix = String.Empty;
#else
            try {
                prefix = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            } catch (SecurityException) {
                prefix = String.Empty;
            }
#endif
        }

        /// <summary>
        /// Handles output of the expression statement.
        /// Prints the value and sets the __builtin__._
        /// </summary>
        public static void displayhook(CodeContext/*!*/ context, object value) {
            if (value != null) {
                PythonOps.Print(context, PythonOps.Repr(context, value));
                ScopeOps.SetMember(context, PythonContext.GetContext(context).BuiltinModuleInstance, "_", value);
            }
        }

        public const int dllhandle = 0;

        public static void excepthook(CodeContext/*!*/ context, object exctype, object value, object traceback) {
            PythonContext pc = PythonContext.GetContext(context);

            PythonOps.PrintWithDest(
                context,
                pc.SystemStandardError,
                pc.FormatException(PythonExceptions.ToClr(value))
            );
        }

        public static int getcheckinterval() {
            throw PythonOps.NotImplementedError("IronPython does not support sys.getcheckinterval");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public static void setcheckinterval(int value) {
            throw PythonOps.NotImplementedError("IronPython does not support sys.setcheckinterval");
        }

        // warnoptions is set by PythonContext and updated on each reload        

        [Python3Warning("'sys.exc_clear() not supported in 3.x; use except clauses'")]
        public static void exc_clear() {
            PythonOps.ClearCurrentException();
        }

        public static PythonTuple exc_info(CodeContext/*!*/ context) {
            return PythonOps.GetExceptionInfo(context);
        }

        // exec_prefix and executable are set by PythonContext and updated on each reload

        public static void exit() {
            exit(null);
        }

        public static void exit(object code) {
            if (code == null) {
                throw new PythonExceptions._SystemExit().InitAndGetClrException();
            } else {
                PythonTuple pt = code as PythonTuple;
                if (pt != null && pt.__len__() == 1) {
                    code = pt[0];
                }

                // throw as a python exception here to get the args set.
                throw new PythonExceptions._SystemExit().InitAndGetClrException(code);
            }
        }

        public static string getdefaultencoding(CodeContext/*!*/ context) {
            return PythonContext.GetContext(context).GetDefaultEncodingName();
        }

        public static object getfilesystemencoding() {
            return null;
        }

        [PythonHidden]
        public static TraceBackFrame/*!*/ _getframeImpl(CodeContext/*!*/ context) {
            return _getframeImpl(context, 0);
        }

        [PythonHidden]
        public static TraceBackFrame/*!*/ _getframeImpl(CodeContext/*!*/ context, int depth) {
            var stack = PythonOps.GetFunctionStack();

            if (depth < stack.Count) {
                TraceBackFrame cur = null;
                for (int i = 0; i < stack.Count - depth; i++) {
                    var elem = stack[i];

                    cur = new TraceBackFrame(
                        context,
                        Builtin.globals(elem.Context),
                        Builtin.locals(elem.Context),
                        elem.Function != null ?
                            elem.Function.func_code :
                            null,
                        cur
                    );
                }
                return cur; 
            }

            throw PythonOps.ValueError("call stack is not deep enough");
        }

        // hex_version is set by PythonContext
        public const int maxint = Int32.MaxValue;
        public const int maxsize = Int32.MaxValue;
        public const int maxunicode = (int)ushort.MaxValue;

        // modules is set by PythonContext and only on the initial load

        // path is set by PythonContext and only on the initial load

#if SILVERLIGHT
        public const string platform = "silverlight";
#else
        public const string platform = "cli";
#endif

        public static readonly string prefix;

        // ps1 and ps2 are set by PythonContext and only on the initial load

        public static void setdefaultencoding(CodeContext context, object name) {
            if (name == null) throw PythonOps.TypeError("name cannot be None");
            string strName = name as string;
            if (strName == null) throw PythonOps.TypeError("name must be a string");

            PythonContext pc = PythonContext.GetContext(context);
            Encoding enc;
            if (!StringOps.TryGetEncoding(strName, out enc)) {
                throw PythonOps.LookupError("'{0}' does not match any available encodings", strName);
            }

            pc.DefaultEncoding = enc;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "o")]
        public static void settrace(object o) {
            throw PythonOps.NotImplementedError("sys.settrace is not yet supported by IronPython");
        }

        public static void setrecursionlimit(int limit) {
            PythonFunction.SetRecursionLimit(limit);
        }

        public static int getrecursionlimit() {
            return PythonFunction._MaximumDepth;
        }

        // stdin, stdout, stderr, __stdin__, __stdout__, and __stderr__ added by PythonContext

        // version and version_info are set by PythonContext

        public const string winver = "2.6";

        #region Special types

        [PythonHidden, PythonType("flags")]
        public sealed class SysFlags : ISequence, IList<object> {
            private const string _className = "sys.flags"; 
            
            internal SysFlags() { }

            private const int INDEX_DEBUG = 0;
            private const int INDEX_PY3K_WARNING = 1;
            private const int INDEX_DIVISION_WARNING = 2;
            private const int INDEX_DIVISION_NEW = 3;
            private const int INDEX_INSPECT = 4;
            private const int INDEX_INTERACTIVE = 5;
            private const int INDEX_OPTIMIZE = 6;
            private const int INDEX_DONT_WRITE_BYTECODE = 7;
            private const int INDEX_NO_USER_SITE = 8;
            private const int INDEX_NO_SITE = 9;
            private const int INDEX_IGNORE_ENVIRONMENT = 10;
            private const int INDEX_TABCHECK = 11;
            private const int INDEX_VERBOSE = 12;
            private const int INDEX_UNICODE = 13;
            private const int INDEX_BYTES_WARNING = 14;

            public const int n_fields = 15;
            public const int n_sequence_fields = 15;
            public const int n_unnamed_fields = 0;

            private static readonly string[] _keys = new string[] {
                "debug", "py3k_warning", "division_warning", "division_new", "inspect",
                "interactive", "optimize", "dont_write_bytecode", "no_user_site", "no_site",
                "ignore_environment", "tabcheck", "verbose", "unicode", "bytes_warning"
            };
            private object[] _values = new object[n_fields] {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };

            private PythonTuple __tuple = null;
            private PythonTuple _tuple {
                get {
                    _Refresh();
                    return __tuple;
                }
            }

            private string __string = null;
            private string _string {
                get {
                    _Refresh();
                    return __string;
                }
            }
            public override string ToString() {
                return _string;
            }
            public string __repr__() {
                return _string;
            }

            private bool _modified = true;
            private void _Refresh() {
                if (_modified) {
                    __tuple = PythonTuple.MakeTuple(_values);

                    StringBuilder sb = new StringBuilder("sys.flags(");
                    for (int i = 0; i < n_fields; i++) {
                        if (_keys[i] == null) {
                            sb.Append(_values[i]);
                        } else {
                            sb.AppendFormat("{0}={1}", _keys[i], _values[i]);
                        }
                        if (i < n_fields - 1) {
                            sb.Append(", ");
                        } else {
                            sb.Append(')');
                        }
                    }
                    __string = sb.ToString();

                    _modified = false;
                }
            }

            private int _GetVal(int index) {
                return (int)_values[index];
            }
            private void _SetVal(int index, int value) {
                if ((int)_values[index] != value) {
                    _modified = true;
                    _values[index] = value;
                }
            }

            #region ICollection<object> Members

            void ICollection<object>.Add(object item) {
                throw new InvalidOperationException(_className + " is readonly");
            }

            void ICollection<object>.Clear() {
                throw new InvalidOperationException(_className + " is readonly");
            }

            [PythonHidden]
            public bool Contains(object item) {
                return _tuple.Contains(item);
            }

            [PythonHidden]
            public void CopyTo(object[] array, int arrayIndex) {
                _tuple.CopyTo(array, arrayIndex);
            }

            public int Count {
                [PythonHidden]
                get {
                    return n_fields;
                }
            }

            bool ICollection<object>.IsReadOnly {
                get { return true; }
            }

            bool ICollection<object>.Remove(object item) {
                throw new InvalidOperationException(_className + " is readonly");
            }

            #endregion

            #region IEnumerable Members

            [PythonHidden]
            public IEnumerator GetEnumerator() {
                return _tuple.GetEnumerator();
            }

            #endregion

            #region IEnumerable<object> Members

            IEnumerator<object> IEnumerable<object>.GetEnumerator() {
                return ((IEnumerable<object>)_tuple).GetEnumerator();
            }

            #endregion

            #region ISequence Members

            public int __len__() {
                return n_fields;
            }

            public object this[int i] {
                get {
                    return _tuple[i];
                }
            }

            public object this[BigInteger i] {
                get {
                    return this[i.ToInt32()];
                }
            }

            public object __getslice__(int start, int end) {
                return _tuple.__getslice__(start, end);
            }

            public object this[Slice s] {
                get {
                    return _tuple[s];
                }
            }

            public object this[object o] {
                get {
                    return this[Converter.ConvertToIndex(o)];
                }
            }

            #endregion

            #region IList<object> Members

            [PythonHidden]
            public int IndexOf(object item) {
                return _tuple.IndexOf(item);
            }

            void IList<object>.Insert(int index, object item) {
                throw new InvalidOperationException(_className + " is readonly");
            }

            void IList<object>.RemoveAt(int index) {
                throw new InvalidOperationException(_className + " is readonly");
            }

            object IList<object>.this[int index] {
                get {
                    return _tuple[index];
                }
                set {
                    throw new InvalidOperationException(_className + " is readonly");
                }
            }

            #endregion

            #region binary ops

            public static PythonTuple operator +([NotNull]SysFlags f, [NotNull]PythonTuple t) {
                return f._tuple + t;
            }

            public static PythonTuple operator +([NotNull]PythonTuple t, [NotNull]SysFlags f) {
                return t + f._tuple;
            }

            public static PythonTuple operator *([NotNull]SysFlags f, int n) {
                return f._tuple * n;
            }

            public static PythonTuple operator *(int n, [NotNull]SysFlags f) {
                return f._tuple * n;
            }

            public static object operator *([NotNull]SysFlags f, [NotNull]Index n) {
                return f._tuple * n;
            }

            public static object operator *([NotNull]Index n, [NotNull]SysFlags f) {
                return f._tuple * n;
            }

            public static object operator *([NotNull]SysFlags f, object n) {
                return f._tuple * n;
            }

            public static object operator *(object n, [NotNull]SysFlags f) {
                return f._tuple * n;
            }

            #endregion

            # region comparison and hashing methods

            public static bool operator >(SysFlags f, PythonTuple t) {
                return f._tuple > t;
            }

            public static bool operator <(SysFlags f, PythonTuple t) {
                return f._tuple < t;
            }

            public static bool operator >=(SysFlags f, PythonTuple t) {
                return f._tuple >= t;
            }

            public static bool operator <=(SysFlags f, PythonTuple t) {
                return f._tuple <= t;
            }

            public override bool Equals(object obj) {
                if (obj is SysFlags) {
                    return _tuple.Equals(((SysFlags)obj)._tuple);
                }
                return _tuple.Equals(obj);
            }

            public override int GetHashCode() {
                return _tuple.GetHashCode();
            }

            # endregion

            #region sys.flags API

            public int debug {
                get { return _GetVal(INDEX_DEBUG); }
                internal set { _SetVal(INDEX_DEBUG, value); }
            }

            public int py3k_warning {
                get { return _GetVal(INDEX_PY3K_WARNING); }
                internal set { _SetVal(INDEX_PY3K_WARNING, value); }
            }

            public int division_warning {
                get { return _GetVal(INDEX_DIVISION_WARNING); }
                internal set { _SetVal(INDEX_DIVISION_WARNING, value); }
            }

            public int division_new {
                get { return _GetVal(INDEX_DIVISION_NEW); }
                internal set { _SetVal(INDEX_DIVISION_NEW, value); }
            }

            public int inspect {
                get { return _GetVal(INDEX_INSPECT); }
                internal set { _SetVal(INDEX_INSPECT, value); }
            }

            public int interactive {
                get { return _GetVal(INDEX_INTERACTIVE); }
                internal set { _SetVal(INDEX_INTERACTIVE, value); }
            }

            public int optimize {
                get { return _GetVal(INDEX_OPTIMIZE); }
                internal set { _SetVal(INDEX_OPTIMIZE, value); }
            }

            public int dont_write_bytecode {
                get { return _GetVal(INDEX_DONT_WRITE_BYTECODE); }
                internal set { _SetVal(INDEX_DONT_WRITE_BYTECODE, value); }
            }

            public int no_user_site {
                get { return _GetVal(INDEX_NO_USER_SITE); }
                internal set { _SetVal(INDEX_NO_USER_SITE, value); }
            }

            public int no_site {
                get { return _GetVal(INDEX_NO_SITE); }
                internal set { _SetVal(INDEX_NO_SITE, value); }
            }

            public int ignore_environment {
                get { return _GetVal(INDEX_IGNORE_ENVIRONMENT); }
                internal set { _SetVal(INDEX_IGNORE_ENVIRONMENT, value); }
            }

            public int tabcheck {
                get { return _GetVal(INDEX_TABCHECK); }
                internal set { _SetVal(INDEX_TABCHECK, value); }
            }

            public int verbose {
                get { return _GetVal(INDEX_VERBOSE); }
                internal set { _SetVal(INDEX_VERBOSE, value); }
            }

            public int unicode {
                get { return _GetVal(INDEX_UNICODE); }
                internal set { _SetVal(INDEX_UNICODE, value); }
            }

            public int bytes_warning {
                get { return _GetVal(INDEX_BYTES_WARNING); }
                internal set { _SetVal(INDEX_BYTES_WARNING, value); }
            }

            #endregion
        }

        #endregion

        [SpecialName]
        public static void PerformModuleReload(PythonContext/*!*/ context, IAttributesCollection/*!*/ dict) {
            dict[SymbolTable.StringToId("stdin")] = dict[SymbolTable.StringToId("__stdin__")];
            dict[SymbolTable.StringToId("stdout")] = dict[SymbolTable.StringToId("__stdout__")];
            dict[SymbolTable.StringToId("stderr")] = dict[SymbolTable.StringToId("__stderr__")];

            // !!! These fields do need to be reset on "reload(sys)". However, the initial value is specified by the 
            // engine elsewhere. For now, we initialize them just once to some default value
            dict[SymbolTable.StringToId("warnoptions")] = new List(0);

            PublishBuiltinModuleNames(context, dict);
            context.SetHostVariables(dict);

            dict[SymbolTable.StringToId("meta_path")] = new List(0);
            dict[SymbolTable.StringToId("path_hooks")] = new List(0);
            dict[SymbolTable.StringToId("path_importer_cache")] = new PythonDictionary();
        }

        private static void PublishBuiltinModuleNames(PythonContext/*!*/ context, IAttributesCollection/*!*/ dict) {
            object[] keys = new object[context.Builtins.Keys.Count];
            int index = 0;
            foreach (object key in context.Builtins.Keys) {
                keys[index++] = key;
            }
            dict[SymbolTable.StringToId("builtin_module_names")] = PythonTuple.MakeTuple(keys);
        }

    }
}
