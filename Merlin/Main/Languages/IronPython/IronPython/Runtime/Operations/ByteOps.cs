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
using System.Collections.Generic;

using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Math;

namespace IronPython.Runtime.Operations {
    public static partial class ByteOps {
        internal static byte ToByteChecked(this int item) {
            try {
                return checked((byte)item);
            } catch (OverflowException) {
                throw PythonOps.ValueError("byte must be in range(0, 256)");
            }
        }

        internal static bool IsSign(this byte ch) {
            return ch == '+' || ch == '-';
        }

        internal static byte ToUpper(this byte p) {
            if (p >= 'a' && p <= 'z') {
                p -= ('a' - 'A');
            }
            return p;
        }

        internal static byte ToLower(this byte p) {
            if (p >= 'A' && p <= 'Z') {
                p += ('a' - 'A');
            }
            return p;
        }

        internal static bool IsLower(this byte p) {
            return p >= 'a' && p <= 'z';
        }

        internal static bool IsUpper(this byte p) {
            return p >= 'A' && p <= 'Z';
        }

        internal static bool IsDigit(this byte b) {
            return b >= '0' && b <= '9';
        }

        internal static bool IsLetter(this byte b) {
            return IsLower(b) || IsUpper(b);
        }

        internal static bool IsWhiteSpace(this byte b) {
            return b == ' ' ||
                    b == '\t' ||
                    b == '\n' ||
                    b == '\r' ||
                    b == '\f' ||
                    b == 11;
        }

        internal static IList<byte> GetBytes(object obj) {
            IList<byte> ret = obj as IList<byte>;
            if (ret == null) {
                throw PythonOps.TypeError("expected string , got {0} Type", DynamicHelpers.GetPythonType(obj).Name);
            }
            return ret;
        }

        internal static void AppendJoin(object value, int index, List<byte> byteList) {
            IList<byte> strVal;

            if ((strVal = value as IList<byte>) != null) {
                byteList.AddRange(strVal);
            } else {
                throw PythonOps.TypeError("sequence item {0}: expected bytes or byte array, {1} found", index.ToString(), PythonOps.GetPythonTypeName(value));
            }
        }

        internal static List<byte> GetBytes(List bytes) {
            List<byte> res = new List<byte>(bytes.Count);
            foreach (object o in bytes) {
                res.Add(GetByte(o));
            }
            return res;
        }

        
        internal static byte GetByteListOk(object o) {
            IList<byte> lbval = o as IList<byte>;
            if (lbval != null) {
                if (lbval.Count != 1) {
                    throw PythonOps.ValueError("string must be of size 1");
                }
                return lbval[0];
            }

            return GetByte(o);
        }

        internal static byte GetByte(object o) {
            byte b;
            Extensible<int> ei;
            BigInteger bi;
            int i;
            if (o is int) {
                b = ((int)o).ToByteChecked();
            } else if ((ei = o as Extensible<int>) != null) {
                b = ei.Value.ToByteChecked();
            } else if (!Object.ReferenceEquals(bi = o as BigInteger, null)) {
                int val;
                if (bi.AsInt32(out val)) {
                    b = ToByteChecked(val);
                } else {
                    // force error
                    ToByteChecked(257);
                    b = 0;
                }
            } else if(Converter.TryConvertToIndex(o, out i)) {
                b = i.ToByteChecked();
            } else{                
                throw PythonOps.TypeError("an integer or string of size 1 is required");
            }
            return b;
        }
    }
}
