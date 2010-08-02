/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;

namespace Microsoft.Scripting.Debugging {
    public sealed class DebugSourceFile {
        private readonly string _fileName;
        private DebugMode _debugMode;
        private readonly Dictionary<DebugSourceSpan, FunctionInfo> _functionInfoMap;

        internal DebugSourceFile(string fileName, DebugMode debugMode) {
            _fileName = fileName;
            _debugMode = debugMode;
            _functionInfoMap = new Dictionary<DebugSourceSpan, FunctionInfo>();
        }

        internal Dictionary<DebugSourceSpan, FunctionInfo> FunctionInfoMap {
            get { return _functionInfoMap; }
        }

        internal string Name {
            get { return _fileName; }
        }

        internal DebugMode DebugMode {
            get { return _debugMode; }
            set { _debugMode = value; }
        }

        internal FunctionInfo LookupFunctionInfo(DebugSourceSpan span) {
            foreach (var entry in _functionInfoMap) {
                if (entry.Key.Intersects(span)) {
                    return entry.Value;
                }
            }

            return null;
        }

        [Obsolete("do not call this property", true)]
        public int Mode {
            get { return (int)_debugMode; }
        }
    }
}
