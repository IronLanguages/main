/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace IronRuby.StandardLibrary.Iconv {
    // TODO: inherits from class Data
    [RubyClass("Iconv", Inherits = typeof(Object))]
    public class Iconv {
        private Decoder _fromEncoding;
        private Encoder _toEncoding;

        [RubyConstructor]
        [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
        public static Iconv/*!*/ Create(RubyClass/*!*/ self,
            [DefaultProtocol]MutableString/*!*/ toEncoding, [DefaultProtocol]MutableString/*!*/ fromEncoding) {

            return Initialize(new Iconv(), toEncoding, fromEncoding);
        }

        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static Iconv/*!*/ Initialize(Iconv/*!*/ self,
            [DefaultProtocol]MutableString/*!*/ toEncoding, [DefaultProtocol]MutableString/*!*/ fromEncoding) {

            self._toEncoding = RubyEncoding.GetEncodingByRubyName(toEncoding.ConvertToString()).GetEncoder();
            self._fromEncoding = RubyEncoding.GetEncodingByRubyName(fromEncoding.ConvertToString()).GetDecoder();

            return self;
        }

        [RubyMethod("iconv")]
        public static MutableString/*!*/ iconv(Iconv/*!*/ self, [DefaultProtocol]MutableString/*!*/ str,
            [DefaultProtocol, DefaultParameterValue(0)]int startIndex, [DefaultProtocol, DefaultParameterValue(-1)]int endIndex) {

            // TODO:
            int bytesUsed, charsUsed;
            bool completed;

            byte[] source = str.ConvertToBytes();
            char[] buffer = new char[self._fromEncoding.GetCharCount(source, 0, source.Length)];
            self._fromEncoding.Convert(source, 0, source.Length, buffer, 0, buffer.Length, false, out bytesUsed, out charsUsed, out completed);
            Debug.Assert(charsUsed == buffer.Length && bytesUsed == source.Length);
            
            byte[] result = new byte[self._toEncoding.GetByteCount(buffer, 0, buffer.Length, false)];
            int bytesEncoded = self._toEncoding.GetBytes(buffer, 0, buffer.Length, result, 0, false);
            Debug.Assert(bytesEncoded == result.Length);

            return MutableString.CreateBinary(result);
        }

        [RubyMethod("close")]
        public static MutableString/*!*/ Close(Iconv/*!*/ self) {
            return null;
        }

        [RubyMethod("conv", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ Convert(RubyClass/*!*/ self, 
            [DefaultProtocol]MutableString/*!*/ toEncoding, [DefaultProtocol]MutableString/*!*/ fromEncoding, 
            [DefaultProtocol]MutableString/*!*/ str) {

            //return iconv(to, from, str).join;
            return null;
        }

        [RubyMethod("charset_map", RubyMethodAttributes.PublicSingleton)]
        public static Hash/*!*/ CharsetMap(RubyClass/*!*/ self) {
            // ???
            return new Hash(self.Context);
        }

        [RubyMethod("iconv", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ iconv(RubyClass/*!*/ self,
            [DefaultProtocol]MutableString/*!*/ toEncoding, [DefaultProtocol]MutableString/*!*/ fromEncoding,
            [NotNull]params MutableString[]/*!*/ strings) {

            //Iconv.open(to, from) { |cd|
            //    (strs + [nil]).collect { |s| cd.iconv(s) }
            //  }

            return null;
        }
        
        [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ Open([NotNull]BlockParam/*!*/ block, RubyClass/*!*/ self,
            [DefaultProtocol]MutableString/*!*/ toEncoding, [DefaultProtocol]MutableString/*!*/ fromEncoding) {
            // Equivalent to Iconv.new except that when it is called with a block, 
            // it yields with the new instance and closes it, and returns the result which returned from the block.

            // using Iconv.new(to, from) { yield block; } ensure close
            return null;
        }

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
    }
}
