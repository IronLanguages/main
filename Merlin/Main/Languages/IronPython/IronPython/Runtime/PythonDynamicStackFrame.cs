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
using System.Reflection;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime {
    /// <summary>
    /// A DynamicStackFrame which has Python specific data.  Currently this
    /// includes the code context which may provide access to locals and the
    /// function code object which is needed to build frame objects from.
    /// </summary>
    class PythonDynamicStackFrame : DynamicStackFrame {
        private readonly CodeContext/*!*/ _context;
        private readonly FunctionCode/*!*/ _code;

        public PythonDynamicStackFrame(CodeContext/*!*/ context, FunctionCode/*!*/ funcCode, MethodBase method, string funcName, string filename, int line)
            : base(method, funcName, filename, line) {
            Assert.NotNull(context, funcCode);

            _context = context;
            _code = funcCode;
        }

        /// <summary>
        /// Gets the code context of the function.
        /// 
        /// If the function included a call to locals() or the FullFrames
        /// option is enabled then the code context includes all local variables.
        /// </summary>
        public CodeContext/*!*/ CodeContext {
            get {
                return _context;
            }
        }

        /// <summary>
        /// Gets the code object for this frame.  This is used in creating
        /// the trace back.
        /// </summary>
        public FunctionCode/*!*/ Code {
            get {
                return _code;
            }
        }
    }
}
