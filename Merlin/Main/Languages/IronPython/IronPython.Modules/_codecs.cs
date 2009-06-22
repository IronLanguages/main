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
using System.Runtime.InteropServices;
using Microsoft.Scripting.Runtime;
using System.Text;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

[assembly: PythonModule("_codecs", typeof(IronPython.Modules.PythonCodecs))]
namespace IronPython.Modules {
    public static class PythonCodecs {        
        internal const int EncoderIndex = 0;
        internal const int DecoderIndex = 1;
        internal const int StreamReaderIndex = 2;
        internal const int StreamWriterIndex = 3;

        #region ASCII Encoding
        public static object ascii_decode(object input) {
            return ascii_decode(input, "strict");
        }

        public static object ascii_decode(object input, string errors) {
            return DoDecode(PythonAsciiEncoding.Instance, input, errors, true);
        }

        public static object ascii_encode(object input) {
            return ascii_encode(input, "strict");
        }

        public static object ascii_encode(object input, string errors) {
            return DoEncode(PythonAsciiEncoding.Instance, input, errors);
        }

        #endregion

        public static object charbuffer_encode() {
            throw PythonOps.NotImplementedError("charbuffer_encode");
        }

        public static object charmap_decode(string input, [Optional]string errors, [Optional]IDictionary<object, object> map) {
            return CharmapDecodeWorker(input, errors, map, true);
        }

        private static object CharmapDecodeWorker(string input, string errors, IDictionary<object, object> map, bool isDecode) {
            if (input.Length == 0) {
                return PythonTuple.MakeTuple(String.Empty, 0);
            }

            StringBuilder res = new StringBuilder();
            for (int i = 0; i < input.Length; i++) {
                object val;

                if (map == null) {
                    res.Append(input[i]);
                    continue;
                }

                if (!map.TryGetValue((int)input[i], out val)) {
                    if (errors == "strict") {
                        if (isDecode) {
                            throw PythonOps.UnicodeDecodeError("failed to find key in mapping");
                        }
                        throw PythonOps.UnicodeEncodeError("failed to find key in mapping");
                    }
                    continue;
                }

                res.Append((char)(int)val);
            }
            return PythonTuple.MakeTuple(res.ToString(), res.Length);
        }

        public static object charmap_encode(string input, [DefaultParameterValue("strict")]string errors, [DefaultParameterValue(null)]IDictionary<object, object> map) {
            return CharmapDecodeWorker(input, errors, map, false);
        }

        public static object decode(CodeContext/*!*/ context, object obj) {
            PythonTuple t = lookup(context, PythonContext.GetContext(context).GetDefaultEncodingName());

            return PythonOps.GetIndex(context, PythonCalls.Call(context, t[DecoderIndex], obj, null), 0);
        }

        public static object decode(CodeContext/*!*/ context, object obj, string encoding) {
            PythonTuple t = lookup(context, encoding);

            return PythonOps.GetIndex(context, PythonCalls.Call(context, t[DecoderIndex], obj, null), 0);
        }

        public static object decode(CodeContext/*!*/ context, object obj, string encoding, string errors) {
            PythonTuple t = lookup(context, encoding);

            return PythonOps.GetIndex(context, PythonCalls.Call(context, t[DecoderIndex], obj, errors), 0);
        }

        public static object encode(CodeContext/*!*/ context, object obj) {
            PythonTuple t = lookup(context, PythonContext.GetContext(context).GetDefaultEncodingName());

            return PythonOps.GetIndex(context, PythonCalls.Call(context, t[EncoderIndex], obj, null), 0);
        }

        public static object encode(CodeContext/*!*/ context, object obj, string encoding) {
            PythonTuple t = lookup(context, encoding);

            return PythonOps.GetIndex(context, PythonCalls.Call(context, t[EncoderIndex], obj, null), 0);
        }

        public static object encode(CodeContext/*!*/ context, object obj, string encoding, string errors) {
            PythonTuple t = lookup(context, encoding);

            return PythonOps.GetIndex(context, PythonCalls.Call(context, t[EncoderIndex], obj, errors), 0);
        }

        public static object escape_decode(string text, [DefaultParameterValue("strict")]string errors) {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < text.Length; i++) {

                if (text[i] == '\\') {
                    if (i == text.Length - 1) throw PythonOps.ValueError("\\ at end of string");

                    switch (text[++i]) {
                        case 'a': res.Append((char)0x07); break;
                        case 'b': res.Append((char)0x08); break;
                        case 't': res.Append('\t'); break;
                        case 'n': res.Append('\n'); break;
                        case 'r': res.Append('\r'); break;
                        case '\\': res.Append('\\'); break;
                        case 'f': res.Append((char)0x0c); break;
                        case 'v': res.Append((char)0x0b); break;
                        case '\n': break;
                        case 'x':
                            int dig1, dig2;
                            if (i >= text.Length - 2 || !CharToInt(text[i], out dig1) || !CharToInt(text[i + 1], out dig2)) {
                                switch (errors) {
                                    case "strict":
                                        if (i >= text.Length - 2) {
                                            throw PythonOps.ValueError("invalid character value");
                                        } else {
                                            throw PythonOps.ValueError("invalid hexadecimal digit");
                                        }
                                    case "replace":
                                        res.Append("?");
                                        i--;
                                        while (i < (text.Length - 1)) {
                                            res.Append(text[i++]);
                                        }
                                        continue;
                                    default:
                                        throw PythonOps.ValueError("decoding error; unknown error handling code: " + errors);
                                }
                            }

                            res.Append(dig1 * 16 + dig2);
                            i += 2;
                            break;
                        default:
                            res.Append("\\" + text[i]);
                            break;
                    }
                } else {
                    res.Append(text[i]);
                }

            }
            return PythonTuple.MakeTuple(res.ToString(), text.Length);
        }

        private static bool CharToInt(char ch, out int val) {
            if (Char.IsDigit(ch)) {
                val = ch - '0';
                return true;
            }
            ch = Char.ToUpper(ch);
            if (ch >= 'A' && ch <= 'F') {
                val = ch - 'A' + 10;
                return true;
            }

            val = 0;
            return false;            
        }

        public static PythonTuple/*!*/ escape_encode(string text, [DefaultParameterValue("strict")]string errors) {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < text.Length; i++) {
                switch (text[i]) {
                    case '\n': res.Append("\\n"); break;
                    case '\r': res.Append("\\r"); break;
                    case '\t': res.Append("\\t"); break;
                    case '\\': res.Append("\\\\"); break;
                    case '\'': res.Append("\\'"); break;
                    default:
                        if (text[i] < 0x20 || text[i] >= 0x7f) {
                            res.AppendFormat("\\x{0:x2}", (int)text[i]);
                        } else {
                            res.Append(text[i]);
                        }
                        break;
                }
            }
            return PythonTuple.MakeTuple(res.ToString(), ScriptingRuntimeHelpers.Int32ToObject(res.Length));
        }

        #region Latin-1 Functions

#if !SILVERLIGHT
        
        public static object latin_1_decode(object input) {
            return latin_1_decode(input, "strict");
        }

        public static object latin_1_decode(object input, string errors) {
            return DoDecode(Encoding.GetEncoding("iso-8859-1"), input, errors);
        }

        public static object latin_1_encode(object input) {
            return latin_1_encode(input, "strict");
        }

        public static object latin_1_encode(object input, string errors) {
            return DoEncode(Encoding.GetEncoding("iso-8859-1"), input, errors);
        }

#endif

        #endregion

        public static PythonTuple lookup(CodeContext/*!*/ context, string encoding) {
            return PythonOps.LookupEncoding(context, encoding);
        }

#if !SILVERLIGHT
        public static object lookup_error(CodeContext/*!*/ context, string name) {
            return PythonOps.LookupEncodingError(context, name);
        }
#endif

        #region MBCS Functions

        public static object mbcs_decode() {
            throw PythonOps.NotImplementedError("mbcs_decode");
        }

        public static object mbcs_encode() {
            throw PythonOps.NotImplementedError("mbcs_encode");
        }

        #endregion

        public static PythonTuple raw_unicode_escape_decode(CodeContext/*!*/ context, object input, [DefaultParameterValue("strict")]string errors) {
            return PythonTuple.MakeTuple(
                StringOps.decode(context, Converter.ConvertToString(input), "raw-unicode-escape", errors),
                Builtin.len(input)
            );
        }

        public static PythonTuple raw_unicode_escape_encode(CodeContext/*!*/ context, object input, [DefaultParameterValue("strict")]string errors) {
            return PythonTuple.MakeTuple(
                StringOps.encode(context, Converter.ConvertToString(input), "raw-unicode-escape", errors),
                Builtin.len(input)
            );
        }

        public static object readbuffer_encode() {
            throw PythonOps.NotImplementedError("readbuffer_encode");
        }

        public static void register(CodeContext/*!*/ context, object search_function) {
            PythonOps.RegisterEncoding(context, search_function);
        }

#if !SILVERLIGHT
        public static void register_error(CodeContext/*!*/ context, string name, object handler) {
            PythonOps.RegisterEncodingError(context, name, handler);
        }
#endif

        #region Unicode Escape Encoding

        public static PythonTuple unicode_escape_decode() {
            throw PythonOps.NotImplementedError("unicode_escape_decode");
        }

        public static PythonTuple unicode_escape_encode() {
            throw PythonOps.NotImplementedError("unicode_escape_encode");
        }

        public static PythonTuple unicode_internal_decode(object input, [Optional]string errors) {
            return utf_16_decode(input, errors);
        }

        public static PythonTuple unicode_internal_encode(object input, [Optional]string errors) {
            // length consumed is returned in bytes and for a UTF-16 string that is 2 bytes per char
            PythonTuple res = DoEncode(Encoding.Unicode, input, errors, false);
            return PythonTuple.MakeTuple(
                res[0],
                ((int)res[1]) * 2
            );
        }

        #endregion

        #region Utf-16 Big Endian Functions

        public static PythonTuple utf_16_be_decode(object input) {
            return utf_16_be_decode(input, "strict", false);
        }

        public static PythonTuple utf_16_be_decode(object input, string errors, [Optional]bool ignored) {
            return DoDecode(Encoding.BigEndianUnicode, input, errors);
        }

        public static PythonTuple utf_16_be_encode(object input) {
            return utf_16_be_encode(input, "strict");
        }

        public static PythonTuple utf_16_be_encode(object input, string errors) {
            return DoEncode(Encoding.BigEndianUnicode, input, errors);
        }

        #endregion

        #region Utf-16 Functions

        public static PythonTuple utf_16_decode(object input) {
            return utf_16_decode(input, "strict");
        }

        public static PythonTuple utf_16_decode(object input, string errors) {
            return DoDecode(Encoding.Unicode, input, errors);
        }

        public static PythonTuple utf_16_encode(object input) {
            return utf_16_encode(input, "strict");
        }

        public static PythonTuple utf_16_encode(object input, string errors) {
            return DoEncode(Encoding.Unicode, input, errors, true);
        }

        #endregion

        public static PythonTuple utf_16_ex_decode(object input, [Optional]string errors) {
            return utf_16_ex_decode(input, errors, null, null);
        }

        public static PythonTuple utf_16_ex_decode(object input, string errors, object unknown1, object unknown2) {
            byte[] lePre = Encoding.Unicode.GetPreamble();
            byte[] bePre = Encoding.BigEndianUnicode.GetPreamble();

            string instr = Converter.ConvertToString(input);
            bool match = true;
            if (instr.Length > lePre.Length) {
                for (int i = 0; i < lePre.Length; i++) {
                    if ((byte)instr[i] != lePre[i]) {
                        match = false;
                        break;
                    }
                }
                if (match) {
                    return PythonTuple.MakeTuple(String.Empty, lePre.Length, -1);
                }
                match = true;
            }

            if (instr.Length > bePre.Length) {
                for (int i = 0; i < bePre.Length; i++) {
                    if ((byte)instr[i] != bePre[i]) {
                        match = false;
                        break;
                    }
                }

                if (match) {
                    return PythonTuple.MakeTuple(String.Empty, bePre.Length, 1);
                }
            }

            PythonTuple res = utf_16_decode(input, errors) as PythonTuple;
            return PythonTuple.MakeTuple(res[0], res[1], 0);
        }

        #region Utf-16 Le Functions

        public static PythonTuple utf_16_le_decode(object input) {
            return utf_16_le_decode(input, "strict", false);
        }

        public static PythonTuple utf_16_le_decode(object input, string errors, [Optional]bool ignored) {
            return utf_16_decode(input, errors);
        }

        public static PythonTuple utf_16_le_encode(object input) {
            return utf_16_le_encode(input, "strict");
        }

        public static PythonTuple utf_16_le_encode(object input, string errors) {
            return DoEncode(Encoding.Unicode, input, errors);
        }

        #endregion

        #region Utf-7 Functions

#if !SILVERLIGHT
        public static PythonTuple utf_7_decode(object input) {
            return utf_7_decode(input, "strict", false);
        }

        public static PythonTuple utf_7_decode(object input, string errors, [Optional]bool ignored) {
            return DoDecode(Encoding.UTF7, input, errors);
        }

        public static PythonTuple utf_7_encode(object input) {
            return utf_7_encode(input, "strict");
        }

        public static PythonTuple utf_7_encode(object input, string errors) {
            return DoEncode(Encoding.UTF7, input, errors);
        }
#endif

        #endregion

        #region Utf-8 Functions

        public static PythonTuple utf_8_decode(object input) {
            return utf_8_decode(input, "strict", false);
        }

        public static PythonTuple utf_8_decode(object input, string errors, [Optional]bool ignored) {
            return DoDecode(Encoding.UTF8, input, errors);
        }

        public static PythonTuple utf_8_encode(object input) {
            return utf_8_encode(input, "strict");
        }

        public static PythonTuple utf_8_encode(object input, string errors) {
            return DoEncode(Encoding.UTF8, input, errors);
        }

        #endregion

        #region Private implementation

        private static PythonTuple DoDecode(Encoding encoding, object input, string errors) {
            return DoDecode(encoding, input, errors, false);
        }

        private static PythonTuple DoDecode(Encoding encoding, object input, string errors, bool fAlwaysThrow) {
            // input should be character buffer of some form...
            string res;

            if (!Converter.TryConvertToString(input, out res)) {
                Bytes bs = input as Bytes;
                if (bs == null) {
                    throw PythonOps.TypeErrorForBadInstance("argument 1 must be string, got {0}", input);
                } else {
                    res = bs.ToString();
                }
            }

            int preOffset = CheckPreamble(encoding, res);

            byte[] bytes = new byte[res.Length - preOffset];
            for (int i = 0; i < bytes.Length; i++) {
                bytes[i] = (byte)res[i + preOffset];
            }

#if !SILVERLIGHT    // DecoderFallback
            encoding = (Encoding)encoding.Clone();

            ExceptionFallBack fallback = null;
            if (fAlwaysThrow) {
                encoding.DecoderFallback = DecoderFallback.ExceptionFallback;
            } else {
                fallback = new ExceptionFallBack(bytes);
                encoding.DecoderFallback = fallback;
            }
#endif
            string decoded = encoding.GetString(bytes, 0, bytes.Length);
            int badByteCount = 0;

#if !SILVERLIGHT    // DecoderFallback
            if (!fAlwaysThrow) {
                byte[] badBytes = fallback.buffer.badBytes;
                if (badBytes != null) {
                    badByteCount = badBytes.Length;
                }
            }
#endif

            PythonTuple tuple = PythonTuple.MakeTuple(decoded, bytes.Length - badByteCount);
            return tuple;
        }

        private static int CheckPreamble(Encoding enc, string buffer) {
            byte[] preamble = enc.GetPreamble();

            if (preamble.Length != 0 && buffer.Length >= preamble.Length) {
                bool hasPreamble = true;
                for (int i = 0; i < preamble.Length; i++) {
                    if (preamble[i] != (byte)buffer[i]) {
                        hasPreamble = false;
                        break;
                    }
                }
                if (hasPreamble) {
                    return preamble.Length;
                }
            }
            return 0;
        }

        private static PythonTuple DoEncode(Encoding encoding, object input, string errors) {
            return DoEncode(encoding, input, errors, false);
        }

        private static PythonTuple DoEncode(Encoding encoding, object input, string errors, bool includePreamble) {
            // input should be some Unicode object
            string res;
            if (Converter.TryConvertToString(input, out res)) {
                StringBuilder sb = new StringBuilder();

                encoding = (Encoding)encoding.Clone();

#if !SILVERLIGHT // EncoderFallback
                encoding.EncoderFallback = EncoderFallback.ExceptionFallback;
#endif

                if (includePreamble) {
                    byte[] preamble = encoding.GetPreamble();
                    for (int i = 0; i < preamble.Length; i++) {
                        sb.Append((char)preamble[i]);
                    }
                }

                byte[] bytes = encoding.GetBytes(res);
                for (int i = 0; i < bytes.Length; i++) {
                    sb.Append((char)bytes[i]);
                }
                return PythonTuple.MakeTuple(sb.ToString(), res.Length);
            }
            throw PythonOps.TypeErrorForBadInstance("cannot decode {0}", input);
        }

        #endregion
    }

#if !SILVERLIGHT    // Encoding
    class ExceptionFallBack : DecoderFallback {
        internal ExceptionFallbackBuffer buffer;

        public ExceptionFallBack(byte[] bytes) {
            buffer = new ExceptionFallbackBuffer(bytes);
        }

        public override DecoderFallbackBuffer CreateFallbackBuffer() {
            return buffer;
        }

        public override int MaxCharCount {
            get { return 100; }
        }
    }

    class ExceptionFallbackBuffer : DecoderFallbackBuffer {
        internal byte[] badBytes;
        private byte[] inputBytes;
        public ExceptionFallbackBuffer(byte[] bytes) {
            inputBytes = bytes;
        }

        public override bool Fallback(byte[] bytesUnknown, int index) {
            if (index > 0 && index + bytesUnknown.Length != inputBytes.Length) {
                throw PythonOps.UnicodeEncodeError("failed to decode bytes at index {0}", index);
            }

            // just some bad bytes at the end
            badBytes = bytesUnknown;
            return false;
        }

        public override char GetNextChar() {
            return ' ';
        }

        public override bool MovePrevious() {
            return false;
        }

        public override int Remaining {
            get { return 0; }
        }
    }
#endif

}
