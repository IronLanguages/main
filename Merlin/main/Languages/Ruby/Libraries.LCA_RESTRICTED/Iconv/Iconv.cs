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
    }
}
