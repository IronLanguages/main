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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

[assembly: PythonModule("binascii", typeof(IronPython.Modules.PythonBinaryAscii))]
namespace IronPython.Modules {
    public static class PythonBinaryAscii {
        [SpecialName]
        public static void PerformModuleReload(PythonContext/*!*/ context, IAttributesCollection/*!*/ dict) {
            context.EnsureModuleException("binasciierror", dict, "Error", "binascii");
        }

        public static string a2b_uu(string data) {
            if (data == null) throw PythonOps.TypeError("expected string, got NoneType");
            if (data.Length < 1) throw PythonOps.ValueError("data is too short");

            StringBuilder res = DecodeWorker(data.Substring(1), Char.MinValue, delegate(char val) {
                return val - 32;
            });


            return res.ToString(0, data[0] - 32);
        }

        public static string b2a_uu(string data) {
            if (data == null) throw PythonOps.TypeError("expected string, got NoneType");
            if (data.Length > 45) throw PythonOps.ValueError("at most 45 characters");

            StringBuilder res = EncodeWorker(data, Int32.MaxValue, ' ', delegate(int val) {
                return (char)(' ' + val);
            });

            res.Insert(0, ((char)(32 + data.Length)).ToString());

            res.Append('\n');
            return res.ToString();
        }

        public static object a2b_base64(string data) {
            if (data == null) throw PythonOps.TypeError("expected string, got NoneType");
            if (data.Length == 0) return String.Empty;

            // remove all newline and carriage feeds before processing...
            data = RemoveWhiteSpace(data);

            StringBuilder res = DecodeWorker(data, '=', delegate(char val) {
                if (val >= 'A' && val <= 'Z') return val - 'A';
                if (val >= 'a' && val <= 'z') return val - 'a' + 26;
                if (val >= '0' && val <= '9') return val - '0' + 52;
                if (val == '+') return 62;
                if (val == '/') return 63; 

                return -1;
            });

            return res.ToString();
        }

        public static object b2a_base64(string data) {
            if (data == null) throw PythonOps.TypeError("expected string, got NoneType");
            if (data.Length == 0) return String.Empty;

            StringBuilder res = EncodeWorker(data, 76, '=', delegate(int val) {
                if (val < 26) return (char)('A' + val);
                if (val < 52) return (char)('a' + val - 26);
                if (val < 62) return (char)('0' + val - 52);
                if (val == 62) return '+';
                if (val == 63) return '/';
                throw new InvalidOperationException(String.Format("Bad int val: {0}", val));
            });

            if (res[res.Length - 1] != '\n') res.Append('\n');
            return res.ToString();
        }

        public static object a2b_qp(object data) {
            throw new NotImplementedException();
        }
        public static object a2b_qp(object data, object header) {
            throw new NotImplementedException();
        }
        public static object b2a_qp(object data) {
            throw new NotImplementedException();
        }
        public static object b2a_qp(object data, object quotetabs) {
            throw new NotImplementedException();
        }
        public static object b2a_qp(object data, object quotetabs, object istext) {
            throw new NotImplementedException();
        }

        public static object b2a_qp(object data, object quotetabs, object istext, object header) {
            throw new NotImplementedException();
        }
        public static object a2b_hqx(object data) {
            throw new NotImplementedException();
        }
        public static object rledecode_hqx(object data) {
            throw new NotImplementedException();
        }
        public static object rlecode_hqx(object data) {
            throw new NotImplementedException();
        }
        public static object b2a_hqx(object data) {
            throw new NotImplementedException();
        }
        public static object crc_hqx(object data, object crc) {
            throw new NotImplementedException();
        }

        [Documentation("crc32(string[, value]) -> string\n\nComputes a CRC (Cyclic Redundancy Check) checksum of string.")]
        public static int crc32(string buffer, [DefaultParameterValue(0)] int baseValue) {
            byte[] data = buffer.MakeByteArray();
            uint result = crc32(data, 0, data.Length, unchecked((uint)baseValue));
            return unchecked((int)result);
        }

        [Documentation("crc32(string[, value]) -> string\n\nComputes a CRC (Cyclic Redundancy Check) checksum of string.")]
        public static int crc32(string buffer, uint baseValue) {
            byte[] data = buffer.MakeByteArray();
            uint result = crc32(data, 0, data.Length, baseValue);
            return unchecked((int)result);
        }

        [Documentation("crc32(byte_array[, value]) -> string\n\nComputes a CRC (Cyclic Redundancy Check) checksum of byte_array.")]
        public static int crc32(byte[] buffer, [DefaultParameterValue(0)] int baseValue) {
            uint result = crc32(buffer, 0, buffer.Length, unchecked((uint)baseValue));
            return unchecked((int)result);
        }

        [Documentation("crc32(byte_array[, value]) -> string\n\nComputes a CRC (Cyclic Redundancy Check) checksum of byte_array.")]
        public static int crc32(byte[] buffer, uint baseValue) {
            uint result = crc32(buffer, 0, buffer.Length, baseValue);
            return unchecked((int)result);
        }

        internal static uint crc32(byte[] buffer, int offset, int count, uint baseValue) {
            uint remainder = (baseValue ^ 0xffffffff);
            for (int i = offset; i < offset + count; i++) {
                remainder = remainder ^ buffer[i];
                for (int j = 0; j < 8; j++) {
                    if ((remainder & 0x01) != 0) {
                        remainder = (remainder >> 1) ^ 0xEDB88320;
                    } else {
                        remainder = (remainder >> 1);
                    }
                }
            }
            return (remainder ^ 0xffffffff);
        }

        public static object b2a_hex(string data) {
            StringBuilder sb = new StringBuilder(data.Length * 2);
            for (int i = 0; i < data.Length; i++) {
                sb.AppendFormat("{0:x2}", (int)data[i]);
            }
            return sb.ToString();
        }

        public static object hexlify(string data) {
            return b2a_hex(data);
        }
        public static object a2b_hex(string data) {
            if (data == null) throw PythonOps.TypeError("expected string, got NoneType");
            if ((data.Length & 0x01) != 0) throw PythonOps.ValueError("string must be even lengthed");
            StringBuilder res = new StringBuilder(data.Length / 2);

            for (int i = 0; i < data.Length; i += 2) {
                byte b1, b2;
                if (Char.IsDigit(data[i])) b1 = (byte)(data[i] - '0');
                else b1 = (byte)(Char.ToUpper(data[i]) - 'A' + 10);

                if (Char.IsDigit(data[i + 1])) b2 = (byte)(data[i + 1] - '0');
                else b2 = (byte)(Char.ToUpper(data[i + 1]) - 'A' + 10);

                res.Append((char)(b1 * 16 + b2));
            }
            return res.ToString();
        }

        public static object unhexlify(string hexstr) {
            return a2b_hex(hexstr);
        }

        #region Private implementation

        private delegate char EncodeChar(int val);
        private delegate int DecodeByte(char val);

        private static StringBuilder EncodeWorker(string data, int lineBreak, char empty, EncodeChar encFunc) {
            StringBuilder res = new StringBuilder();

            int bits;
            int lineCount = 0;
            for (int i = 0; i < data.Length; i += 3) {
                switch (data.Length - i) {
                    case 1:
                        // only one char, emit 2 bytes &
                        // padding
                        bits = (data[i] & 0xff) << 16;
                        res.Append(encFunc((bits >> 18) & 0x3f));
                        res.Append(encFunc((bits >> 12) & 0x3f));
                        res.Append(empty);
                        res.Append(empty);
                        break;
                    case 2:
                        // only two chars, emit 3 bytes &
                        // padding
                        bits = ((data[i] & 0xff) << 16) | ((data[i + 1] & 0xff) << 8);
                        res.Append(encFunc((bits >> 18) & 0x3f));
                        res.Append(encFunc((bits >> 12) & 0x3f));
                        res.Append(encFunc((bits >> 6) & 0x3f));
                        res.Append(empty);
                        break;
                    default:
                        // got all 3 bytes, just emit it.
                        bits = ((data[i] & 0xff) << 16) |
                            ((data[i + 1] & 0xff) << 8) |
                            ((data[i + 2] & 0xff));
                        res.Append(encFunc((bits >> 18) & 0x3f));
                        res.Append(encFunc((bits >> 12) & 0x3f));
                        res.Append(encFunc((bits >> 6) & 0x3f));
                        res.Append(encFunc(bits & 0x3f));
                        break;
                }
                if (((res.Length - (lineCount)) % lineBreak) == 0) {
                    res.Append('\n');
                    lineCount++;
                }
            }
            return res;
        }

        const int NoMoreData = -1;
        const int EmptyChar = -2;

        private static int GetVal(DecodeByte decFunc, char empty, string data, ref int index) {
            int res = NoMoreData;
            do {
                if (index >= data.Length) break;

                char curChar = data[index++];
                if (curChar == empty) return EmptyChar;

                res = decFunc(curChar);
            } while (res == NoMoreData);
            if (res < 0 && empty != Char.MinValue) throw PythonOps.TypeError("Incorrect padding");
            return res;
        }

        private static StringBuilder DecodeWorker(string data, char empty, DecodeByte decFunc) {
            StringBuilder res = new StringBuilder();

            int i = 0;
            while (i < data.Length) {
                int intVal;

                int val1 = GetVal(decFunc, empty, data, ref i);
                if (val1 < 0) break;  // no more bytes...                

                int val2 = GetVal(decFunc, empty, data, ref i);
                if (val1 < 0) break;  // no more bytes...

                int val3 = GetVal(decFunc, empty, data, ref i);
                if (val3 < 0) {
                    // 2 byte partial
                    intVal = (val1 << 18) | (val2 << 12);

                    res.Append((char)((intVal >> 16) & 0xff));
                    break;
                }

                int val4 = GetVal(decFunc, empty, data, ref i);
                if (val4 < 0) {
                    // 3 byte partial
                    intVal = (val1 << 18) | (val2 << 12) | (val3 << 6);

                    res.Append((char)((intVal >> 16) & 0xff));
                    res.Append((char)((intVal >> 8) & 0xff));
                    break;
                }

                // full 4-bytes
                intVal = (val1 << 18) | (val2 << 12) | (val3 << 6) | (val4);
                res.Append((char)((intVal >> 16) & 0xff));
                res.Append((char)((intVal >> 8) & 0xff));
                res.Append((char)(intVal & 0xff));
            }

            return res;
        }

        private static string RemoveWhiteSpace(string data) {
            StringBuilder str = null;
            for (int i = 0; i < data.Length; i++) {
                if (data[i] == '\r' || data[i] == '\n' || data[i] == ' ') {
                    if (str == null) {
                        str = new StringBuilder(data, 0, i, data.Length);
                    }
                } else if (str != null) {
                    str.Append(data[i]);
                }
            }
            if (str != null) data = str.ToString();
            return data;
        }

        #endregion
    }
}
