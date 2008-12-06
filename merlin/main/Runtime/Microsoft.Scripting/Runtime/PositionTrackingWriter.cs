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
using System.CodeDom;
using System.Collections.Generic;
using System.IO;

#if !SILVERLIGHT // requires CodeDom support

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Efficiently tracks (line,column) information as text is added, and
    /// collects line mappings between the original and generated source code
    /// so we can generate correct debugging information later
    /// </summary>
    public class PositionTrackingWriter : StringWriter {
        List<KeyValuePair<int, int>> _lineMap = new List<KeyValuePair<int, int>>();
        List<KeyValuePair<int, string>> _fileMap = new List<KeyValuePair<int, string>>();

        int _line = 1;
        int _column = 1;

        public PositionTrackingWriter() { }

        /// <summary>
        /// Marks the current position of the writer as corresponding to the
        /// original location passed in
        /// </summary>
        /// <param name="linePragma">the line pragma corresponding to the 
        /// current position in the generated code</param>
        public void MapLocation(CodeLinePragma linePragma) {
            _lineMap.Add(new KeyValuePair<int, int>(_line, linePragma.LineNumber));
            _fileMap.Add(new KeyValuePair<int, string>(_line, linePragma.FileName));
        }

        public KeyValuePair<int, int>[] GetLineMap() {
            return _lineMap.ToArray();
        }

        public KeyValuePair<int, string>[] GetFileMap() {
            return _fileMap.ToArray();
        }

        public override void Write(char value) {
            if (value != '\n') {
                ++_column;
            } else {
                _column = 1;
                ++_line;
            }
            base.Write(value);
        }

        public override void Write(string value) {
            UpdateLineColumn(value);
            base.Write(value);
        }

        public override void Write(char[] buffer, int index, int count) {
            UpdateLineColumn(buffer, index, count);
            base.Write(buffer, index, count);
        }

        private void UpdateLineColumn(string value) {
            int lastPos = 0, pos;
            while ((pos = 1 + value.IndexOf('\n', lastPos)) > 0) {
                ++_line;
                lastPos = pos;
            }

            if (lastPos > 0) {
                _column = value.Length - lastPos + 1;
            } else {
                _column += value.Length;
            }
        }

        private void UpdateLineColumn(char[] buffer, int index, int count) {
            int end = index + count;
            int lastPos = index, pos;
            while ((pos = 1 + Array.IndexOf(buffer, '\n', lastPos, end - lastPos)) > 0) {
                ++_line;
                lastPos = pos;
            }

            if (lastPos > 0) {
                _column = count - lastPos + 1;
            } else {
                _column += count;
            }
        }
    }
}

#endif
