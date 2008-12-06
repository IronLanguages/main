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
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Microsoft.Scripting.Utils {
    public static class IOUtils {
        /// <summary>
        /// Seeks the first character of a specified line in the text stream.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="line">Line number. The current position is assumed to be line #1.</param>
        /// <returns>
        /// Returns <c>true</c> if the line is found, <b>false</b> otherwise.
        /// </returns>
        public static bool SeekLine(TextReader reader, int line) {
            ContractUtils.RequiresNotNull(reader, "reader");
            if (line < 1) throw new ArgumentOutOfRangeException("line");
            if (line == 1) return true;

            int current_line = 1;

            for (; ; ) {
                int c = reader.Read();

                if (c == '\r') {
                    if (reader.Peek() == '\n') {
                        reader.Read();
                    }

                    current_line++;
                    if (current_line == line) return true;

                } else if (c == '\n') {
                    current_line++;
                    if (current_line == line) return true;
                } else if (c == -1) {
                    return false;
                }
            }
        }

        /// <summary>
        /// Reads characters to a string until end position or a terminator is reached. 
        /// Doesn't include the terminator into the resulting string.
        /// Returns <c>null</c>, if the reader is at the end position.
        /// </summary>
        public static string ReadTo(TextReader reader, char terminator) {
            ContractUtils.RequiresNotNull(reader, "reader");

            StringBuilder result = new StringBuilder();
            int ch;
            for (; ; ) {
                ch = reader.Read();

                if (ch == -1) break;
                if (ch == terminator) return result.ToString();

                result.Append((char)ch);
            }
            return (result.Length > 0) ? result.ToString() : null;
        }

        /// <summary>
        /// Reads characters until end position or a terminator is reached.
        /// Returns <c>true</c> if the character has been found (the reader is positioned right behind the character), 
        /// <c>false</c> otherwise.
        /// </summary>
        public static bool SeekTo(TextReader reader, char c) {
            ContractUtils.RequiresNotNull(reader, "reader");

            for (; ; ) {
                int ch = reader.Read();
                if (ch == -1) return false;
                if (ch == c) return true;
            }
        }

        public static string ToValidPath(string path) {
            return ToValidPath(path, false, true);
        }

        public static string ToValidPath(string path, bool isMask) {
            return ToValidPath(path, isMask, true);
        }

        public static string ToValidFileName(string path) {
            return ToValidPath(path, false, false);
        }

        private static string ToValidPath(string path, bool isMask, bool isPath) {
            Debug.Assert(!isMask || isPath);

            if (String.IsNullOrEmpty(path)) {
                return "_";
            }

            StringBuilder sb = new StringBuilder(path);

            if (isPath) {
                foreach (char c in Path.GetInvalidPathChars()) {
                    sb.Replace(c, '_');
                }
            } else {
#if SILVERLIGHT
                foreach (char c in Path.GetInvalidPathChars()) {
                    sb.Replace(c, '_');
                }
                sb.Replace(':', '_').Replace('*', '_').Replace('?', '_').Replace('\\', '_').Replace('/', '_');
#else
                foreach (char c in Path.GetInvalidFileNameChars()) {
                    sb.Replace(c, '_');
                }
#endif
            }

            if (!isMask) {
                sb.Replace('*', '_').Replace('?', '_');
            }

            return sb.ToString();
        }
    }
}
