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

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Builtins {
    /// <summary>
    /// Mixin. Implements print, puts, putc methods.
    /// </summary>
    [RubyModule("Print", DefineIn = typeof(IronRubyOps))]
    public static class PrintOps {
        [RubyMethod("<<")]
        public static object/*!*/ Output(BinaryOpStorage/*!*/ writeStorage, object self, object value) {
            Protocols.Write(writeStorage, self, value);
            return self;
        }

        [RubyMethod("print")]
        public static void Print(BinaryOpStorage/*!*/ writeStorage, RubyScope/*!*/ scope, object self) {
            Print(writeStorage, self, scope.GetInnerMostClosureScope().LastInputLine);
        }

        [RubyMethod("print")]
        public static void Print(BinaryOpStorage/*!*/ writeStorage, object self, params object[]/*!*/ args) {
            foreach (object arg in args) {
                Print(writeStorage, self, arg);
            }
        }

        [RubyMethod("print")]
        public static void Print(BinaryOpStorage/*!*/ writeStorage, object self, object value) {
            Protocols.Write(writeStorage, self, value ?? MutableString.CreateAscii("nil"));

            MutableString delimiter = writeStorage.Context.OutputSeparator;
            if (delimiter != null) {
                Protocols.Write(writeStorage, self, delimiter);
            }
        }

        [RubyMethod("putc")]
        public static MutableString/*!*/ Putc(BinaryOpStorage/*!*/ writeStorage, object self, [NotNull]MutableString/*!*/ val) {
            if (val.IsEmpty) {
                throw RubyExceptions.CreateTypeError("can't convert String into Integer");
            }

            // writes a single byte into the output stream:
            var c = MutableString.CreateBinary(val.GetBinarySlice(0, 1));
            Protocols.Write(writeStorage, self, c);
            return val;
        }

        [RubyMethod("putc")]
        public static int Putc(BinaryOpStorage/*!*/ writeStorage, object self, [DefaultProtocol]int c) {
            MutableString str = MutableString.CreateBinary(1).Append(unchecked((byte)c));
            Protocols.Write(writeStorage, self, str);
            return c;
        }

        public static MutableString/*!*/ ToPrintedString(ConversionStorage<MutableString>/*!*/ tosConversion, object obj) {
            if (obj == null) {
                return MutableString.CreateAscii("nil");
            } else {
                return Protocols.ConvertToString(tosConversion, obj);
            }
        }

        [RubyMethod("puts")]
        public static void PutsEmptyLine(BinaryOpStorage/*!*/ writeStorage, object self) {
            Protocols.Write(writeStorage, self, MutableString.CreateAscii("\n"));
        }

        [RubyMethod("puts")]
        public static void Puts(BinaryOpStorage/*!*/ writeStorage, object self, [NotNull]MutableString/*!*/ str) {
            Protocols.Write(writeStorage, self, str);

            if (!str.EndsWith('\n')) {
                PutsEmptyLine(writeStorage, self);
            }
        }

        [RubyMethod("puts")]
        public static void Puts(BinaryOpStorage/*!*/ writeStorage, ConversionStorage<MutableString>/*!*/ tosConversion,
            ConversionStorage<IList>/*!*/ tryToAry, object self, [NotNull]object/*!*/ val) {

            IList list = Protocols.TryCastToArray(tryToAry, val);
            if (list != null) {
                IEnumerable recEnum = IListOps.EnumerateRecursively(tryToAry, list, -1, (_) => MutableString.CreateAscii("[...]"));
                foreach (object item in recEnum ?? list) {
                    Puts(writeStorage, self, ToPrintedString(tosConversion, item));
                }
            } else {
                Puts(writeStorage, self, ToPrintedString(tosConversion, val));
            }
        }

        [RubyMethod("puts")]
        public static void Puts(BinaryOpStorage/*!*/ writeStorage, ConversionStorage<MutableString>/*!*/ tosConversion,
            ConversionStorage<IList>/*!*/ tryToAry, object self, params object[]/*!*/ vals) {

            for (int i = 0; i < vals.Length; i++) {
                Puts(writeStorage, tosConversion, tryToAry, self, vals[i]);
            }
        }

        [RubyMethod("printf")]
        public static void PrintFormatted(
            StringFormatterSiteStorage/*!*/ storage,
            ConversionStorage<MutableString>/*!*/ stringCast,
            BinaryOpStorage/*!*/ writeStorage,
            object/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ format, params object[]/*!*/ args) {

            KernelOps.PrintFormatted(storage, stringCast, writeStorage, null, self, format, args);
        }

        internal static void ReportWarning(BinaryOpStorage/*!*/ writeStorage, ConversionStorage<MutableString>/*!*/ tosConversion, object message) {
            if (writeStorage.Context.Verbose != null) {
                var output = writeStorage.Context.StandardErrorOutput;
                // MRI: unlike Kernel#puts this outputs \n even if the message ends with \n:
                var site = writeStorage.GetCallSite("write", 1);
                site.Target(site, output, PrintOps.ToPrintedString(tosConversion, message));
                PrintOps.PutsEmptyLine(writeStorage, output);
            }
        }

    }
}
