/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Runtime.InteropServices;

namespace IronRuby.StandardLibrary.Iconv {
    // TODO: inherits from class Data
    [RubyClass("Iconv", Inherits = typeof(Object))]
    public class Iconv {
        private Decoder _fromEncoding;
        private Encoder _toEncoding;
        private string _toEncodingString;
        private bool _emitBom;
        private bool _isClosed;

        [RubyConstructor]
        [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
        public static Iconv/*!*/ Create(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ toEncoding, [DefaultProtocol, NotNull]MutableString/*!*/ fromEncoding) {

            Iconv converter = new Iconv();
            return Initialize(self.Context, converter, toEncoding, fromEncoding);
        }

        private void ResetByteOrderMark() {
            if (_toEncodingString == "UTF-16") {
                _emitBom = true;
            }
        }

        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static Iconv/*!*/ Initialize(RubyContext/*!*/ context, Iconv/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ toEncoding, [DefaultProtocol, NotNull]MutableString/*!*/ fromEncoding) {

            self._toEncodingString = toEncoding.ConvertToString().ToUpperInvariant();

            try {
                self._toEncoding = context.GetEncodingByRubyName(self._toEncodingString).GetEncoder();
            } catch (ArgumentException e) {
                throw new InvalidEncoding(self._toEncodingString, e);
            }

            try {
                self._fromEncoding = context.GetEncodingByRubyName(fromEncoding.ConvertToString()).GetDecoder();
            } catch (ArgumentException e) {
                throw new InvalidEncoding(fromEncoding.ConvertToString(), e);
            }

            self.ResetByteOrderMark();

            return self;
        }

        [RubyMethod("iconv")]
        public static MutableString/*!*/ iconv(Iconv/*!*/ self,
            [DefaultProtocol]MutableString str,
            [DefaultProtocol, DefaultParameterValue(0)]int startIndex,
            object length) {

            if (length == null) {
                return iconv(self, str, startIndex, -1);
            }
            throw new ArgumentException();
        }

        [RubyMethod("iconv")]
        public static MutableString/*!*/ iconv(Iconv/*!*/ self, 
            [DefaultProtocol]MutableString str,
            [DefaultProtocol, DefaultParameterValue(0)]int startIndex, 
            [DefaultProtocol, NotNull, DefaultParameterValue(-1)]int length) {

            if (self._isClosed) {
                throw RubyExceptions.CreateArgumentError("closed stream");
            }

            if (str == null) {
                return self.Close(true);
            }

            // TODO:
            int bytesUsed, charsUsed;
            bool completed;

            byte[] source = str.ConvertToBytes();
            if (startIndex < 0) {
                startIndex = source.Length + startIndex;
                if (startIndex < 0) {
                    //throw new IllegalSequence("start index is too large of a negative number");
                    startIndex = 0;
                    length = 0;
                }
            } else if (startIndex > source.Length) {
                startIndex = 0;
                length = 0;
            }

            if ((length < 0) || (startIndex + length > source.Length)) {
                length = source.Length - startIndex;
            }

            char[] buffer = new char[self._fromEncoding.GetCharCount(source, startIndex, length)];
            self._fromEncoding.Convert(source, startIndex, length, buffer, 0, buffer.Length, false, out bytesUsed, out charsUsed, out completed);
            Debug.Assert(charsUsed == buffer.Length && bytesUsed == length);
            
            byte[] result = new byte[self._toEncoding.GetByteCount(buffer, 0, buffer.Length, false)];
            int bytesEncoded = self._toEncoding.GetBytes(buffer, 0, buffer.Length, result, 0, false);
            Debug.Assert(bytesEncoded == result.Length);

            if (self._emitBom && result.Length > 0) {
                byte[] resultWithBom = new byte[2 + result.Length];
                resultWithBom[0] = 0xff;
                resultWithBom[1] = 0xfe;
                Array.Copy(result, 0, resultWithBom, 2, result.Length);
                result = resultWithBom;
                self._emitBom = false;
            }

            return MutableString.CreateBinary(result);
        }

        private MutableString/*!*/ Close(bool resetEncoder) {
            char[] buffer = new char[0];
            byte[] result = new byte[_toEncoding.GetByteCount(buffer, 0, 0, true)];
            int bytesEncoded = _toEncoding.GetBytes(buffer, 0, 0, result, 0, true);
            Debug.Assert(bytesEncoded == result.Length);
            if (resetEncoder) {
#if SILVERLIGHT
                // TODO - Create a new encoder
                throw new NotImplementedException();
#else
                _toEncoding.Reset();
                ResetByteOrderMark();
#endif
            } else {
                _isClosed = true;
            }
            return MutableString.CreateBinary(result);
        }

        [RubyMethod("close")]
        public static MutableString/*!*/ Close(Iconv/*!*/ self) {
            if (!self._isClosed) {
                return self.Close(false);
            } else {
                return null;
            }
        }

        private static MutableString[]/*!*/ Convert(
            RubyClass/*!*/ self,
            MutableString/*!*/ toEncoding, 
            MutableString/*!*/ fromEncoding, 
            MutableString[]/*!*/ strings) {

            Iconv conveter = Create(self, toEncoding, fromEncoding);
            MutableString[] convertedStrings = new MutableString[strings.Length];
            for (int i = 0; i < strings.Length; i++) {
                convertedStrings[i] = iconv(conveter, strings[i], 0, -1);
            }
            MutableString closingString = conveter.Close(false);
            if (closingString.IsEmpty) {
                return convertedStrings;
            } else {
                Array.Resize(ref convertedStrings, strings.Length + 1);
                convertedStrings[strings.Length] = closingString;
            }
            return convertedStrings;
        }

        [RubyMethod("conv", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ Convert(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ toEncoding, [DefaultProtocol, NotNull]MutableString/*!*/ fromEncoding, 
            [DefaultProtocol]MutableString str) {

            MutableString[] convertedStrings = Convert(self, toEncoding, fromEncoding, new MutableString[] { str, null });
            MutableString result = MutableString.CreateEmpty();
            foreach (MutableString s in convertedStrings) {
                result.Append(s);
            }
            return result;
        }

        [RubyMethod("charset_map", RubyMethodAttributes.PublicSingleton)]
        public static Hash/*!*/ CharsetMap(RubyClass/*!*/ self) {
            // ???
            return new Hash(self.Context);
        }

        [RubyMethod("iconv", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ iconv(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ toEncoding, [DefaultProtocol, NotNull]MutableString/*!*/ fromEncoding,
            params MutableString[]/*!*/ strings) {

            MutableString[] convertedStrings = Convert(self, toEncoding, fromEncoding, strings);
            return new RubyArray(convertedStrings);
        }
        
        [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
        public static object Open([NotNull]BlockParam/*!*/ block, RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ toEncoding, [DefaultProtocol, NotNull]MutableString/*!*/ fromEncoding) {

            Iconv converter = Create(self, toEncoding, fromEncoding);
            if (block == null) {
                return converter;
            } else {
                try {
                    object blockResult;
                    block.Yield(converter, out blockResult);
                    return blockResult;
                } finally {
                    Close(converter);
                }
            }
        }

        #region Exceptions

        [RubyModule("Failure")]
        public static class Failure {
        }

        private static string GetMessage(object arg2, object arg3) {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1}", KernelOps.ToS(arg2), KernelOps.ToS(arg3));
        }

        [RubyException("BrokenLibrary"), Includes(typeof(Failure)), Serializable]
        public class BrokenLibrary : RuntimeError {
            public BrokenLibrary() : this(null, null) { }
            public BrokenLibrary(string message) : this(message, null) { }
            public BrokenLibrary(string message, Exception inner) : base(message ?? "BrokenLibrary", inner) { }

#if !SILVERLIGHT
            protected BrokenLibrary(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif

            [RubyConstructor]
            public static BrokenLibrary/*!*/ Factory(RubyClass/*!*/ self, object arg1, object arg2, object arg3) {
                BrokenLibrary result = new BrokenLibrary(GetMessage(arg2, arg3));
                // RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        [RubyException("InvalidEncoding"), Includes(typeof(Failure)), Serializable]
        public class InvalidEncoding : ArgumentException {
            public InvalidEncoding() : this(null, null) { }
            public InvalidEncoding(string message) : this(message, null) { }
            public InvalidEncoding(string message, Exception inner) : base(message ?? "InvalidEncoding", inner) { }

#if !SILVERLIGHT
            protected InvalidEncoding(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif

            [RubyConstructor]
            public static InvalidEncoding/*!*/ Factory(RubyClass/*!*/ self, object arg1, object arg2, object arg3) {
                InvalidEncoding result = new InvalidEncoding(GetMessage(arg2, arg3));
                // RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        [RubyException("InvalidCharacter"), Includes(typeof(Failure)), Serializable]
        public class InvalidCharacter : ArgumentException {
            public InvalidCharacter() : this(null, null) { }
            public InvalidCharacter(string message) : this(message, null) { }
            public InvalidCharacter(string message, Exception inner) : base(message ?? "InvalidCharacter", inner) { }

#if !SILVERLIGHT
            protected InvalidCharacter(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif

            [RubyConstructor]
            public static InvalidCharacter/*!*/ Factory(RubyClass/*!*/ self, object arg1, object arg2, object arg3) {
                InvalidCharacter result = new InvalidCharacter(GetMessage(arg2, arg3));
                // RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        [RubyException("IllegalSequence"), Includes(typeof(Failure)), Serializable]
        public class IllegalSequence : ArgumentException {
            public IllegalSequence() : this(null, null) { }
            public IllegalSequence(string message) : this(message, null) { }
            public IllegalSequence(string message, Exception inner) : base(message ?? "IllegalSequence", inner) { }

#if !SILVERLIGHT
            protected IllegalSequence(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif

            [RubyConstructor]
            public static IllegalSequence/*!*/ Factory(RubyClass/*!*/ self, object arg1, object arg2, object arg3) {
                IllegalSequence result = new IllegalSequence(GetMessage(arg2, arg3));
                // RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        [RubyException("OutOfRange"), Includes(typeof(Failure)), Serializable]
        public class OutOfRange : RuntimeError {
            public OutOfRange() : this(null, null) { }
            public OutOfRange(string message) : this(message, null) { }
            public OutOfRange(string message, Exception inner) : base(message ?? "OutOfRange", inner) { }

#if !SILVERLIGHT
            protected OutOfRange(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
#endif

            [RubyConstructor]
            public static OutOfRange/*!*/ Factory(RubyClass/*!*/ self, object arg1, object arg2, object arg3) {
                OutOfRange result = new OutOfRange(GetMessage(arg2, arg3));
                // RubyExceptionData.InitializeException(result, message);
                return result;
            }
        }

        #endregion
    }
}
