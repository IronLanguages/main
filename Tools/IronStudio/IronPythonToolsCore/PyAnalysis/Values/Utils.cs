/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using IronPython;
using IronPython.Compiler;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace Microsoft.PyAnalysis.Values {
    internal static class Utils {
        internal static IList<string> DirHelper(object obj, bool showClr) {
            NamespaceTracker nt = obj as NamespaceTracker;
            if (nt != null) {
                return nt.GetMemberNames();
            }

            var dir = showClr ? ClrModule.DirClr(obj) : ClrModule.Dir(obj);
            int len = dir.__len__();
            string[] result = new string[len];
            for (int i = 0; i < len; i++) {
                // TODO: validate
                result[i] = dir[i] as string;
            }
            return result;
        }

        internal static List<object> MakeList(object obj) {
            var result = new List<object>();
            result.Add(obj);
            return result;
        }

        internal static T[] RemoveFirst<T>(this T[] array) {
            if (array.Length < 1) {
                return new T[0];
            }
            T[] result = new T[array.Length - 1];
            Array.Copy(array, 1, result, 0, array.Length - 1);
            return result;
        }

        internal static string StripDocumentation(string doc) {
            if (doc == null) {
                return String.Empty;
            }
            StringBuilder result = new StringBuilder(doc.Length);
            foreach (string line in doc.Split('\n')) {
                if (result.Length > 0) {
                    result.Append("\r\n");
                }
                result.Append(line.Trim());
            }
            return result.ToString();
        }

        internal static string CleanDocumentation(string doc) {
            int ctr = 0;
            var result = new StringBuilder(doc.Length);
            foreach (char c in doc) {
                if (c == '\r') {
                    // pass
                } else if (c == '\n') {
                    ctr++;
                    if (ctr < 3) {
                        result.Append("\r\n");
                    }
                } else {
                    result.Append(c);
                    ctr = 0;
                }
            }
            return result.ToString().Trim();
        }

        internal static string GetDocumentation(ProjectState projectState, object obj) {
            object doc;
            if (!projectState.TryGetMember(obj, "__doc__", out doc)) {
                return String.Empty;
            }
            return StripDocumentation(doc as string);
        }

        internal static Parser CreateParser(SourceUnit sourceUnit, ErrorSink errorSink) {
            return Parser.CreateParser(
                new CompilerContext(sourceUnit, new PythonCompilerOptions(), errorSink),
                new PythonOptions()
                );
        }


        internal static ISet<Namespace> GetReturnTypes(BuiltinFunction func, ProjectState projectState) {
            var result = new HashSet<Namespace>();
            var found = new HashSet<Type>();
            foreach (var target in func.Overloads.Targets) {
                var targetInfo = (target as System.Reflection.MethodInfo);
                if (targetInfo != null && !found.Contains(targetInfo.ReturnType)) {
                    var pyType = ClrModule.GetPythonType(targetInfo.ReturnType);
                    result.Add(((BuiltinClassInfo)projectState.GetNamespaceFromObjects(pyType)).Instance);
                    found.Add(targetInfo.ReturnType);
                }
            }
            return result;
        }

        internal static T First<T>(IEnumerable<T> sequence) where T : class {
            if (sequence == null) {
                return null;
            }
            var enumerator = sequence.GetEnumerator();
            if (enumerator == null) {
                return null;
            }
            try {
                if (enumerator.MoveNext()) {
                    return enumerator.Current;
                } else {
                    return null;
                }
            } finally {
                enumerator.Dispose();
            }
        }

        internal static T[] Concat<T>(T firstArg, T[] args) {
            var newArgs = new T[args.Length + 1];
            args.CopyTo(newArgs, 1);
            newArgs[0] = firstArg;
            return newArgs;
        }

        internal static T Peek<T>(this List<T> stack) {
            return stack[stack.Count - 1];
        }

        internal static void Push<T>(this List<T> stack, T value) {
            stack.Add(value);
        }

        internal static T Pop<T>(this List<T> stack) {
            int pos = stack.Count - 1;
            var result = stack[pos];
            stack.RemoveAt(pos);
            return result;
        }
    }

    internal class ReferenceComparer<T> : IEqualityComparer<T> where T : class {
        int IEqualityComparer<T>.GetHashCode(T obj) {
            return RuntimeHelpers.GetHashCode(obj);
        }

        bool IEqualityComparer<T>.Equals(T x, T y) {
            return Object.ReferenceEquals(x, y);
        }
    }
}
