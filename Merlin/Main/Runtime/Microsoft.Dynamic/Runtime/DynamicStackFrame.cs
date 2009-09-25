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

using System.Reflection;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Helper for storing information about stack frames.
    /// </summary>
    public class DynamicStackFrame {
        private string _funcName;
        private string _filename;
        private int _lineNo;
        private MethodBase _method;

        public DynamicStackFrame(MethodBase method, string funcName, string filename, int line) {
            _funcName = funcName;
            _filename = filename;
            _lineNo = line;
            _method = method;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public MethodBase GetMethod() {
            return _method;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetMethodName() {
            return _funcName;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetFileName() {
            return _filename;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetFileLineNumber() {
            return _lineNo;
        }

        public override string ToString() {
            return string.Format(
                "{0} in {1}:{2}, {3}",
                _funcName ?? "<function unknown>",
                _filename ?? "<filename unknown>",
                _lineNo,
                (_method != null ? _method.ToString() : "<method unknown>")
            );
        }
    }
}
