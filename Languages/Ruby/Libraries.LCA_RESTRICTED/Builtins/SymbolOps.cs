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
using Microsoft.Scripting;
using IronRuby.Runtime;
using IronRuby.Compiler;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Runtime.CompilerServices;

namespace IronRuby.Builtins {
    using BinaryOpStorageWithScope = CallSiteStorage<Func<CallSite, RubyScope, object, object, object>>;
    using System.Globalization;

    [RubyClass("Symbol", Extends = typeof(RubySymbol), Inherits = typeof(Object))]
    [HideMethod("==")]
    public static class SymbolOps {

        #region Public Instance Methods

        [RubyMethod("id2name")]
        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToString(RubySymbol/*!*/ self) {
            return self.ToMutableString();
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, RubySymbol/*!*/ self) {
            var str = self.ToString();
            bool allowMultiByteCharacters = context.RubyOptions.Compatibility >= RubyCompatibility.Ruby19 || context.KCode != null;

            var result = self.ToMutableString();

            // simple cases:
            if (
                Tokenizer.IsMethodName(str, allowMultiByteCharacters) ||
                Tokenizer.IsConstantName(str, allowMultiByteCharacters) ||
                Tokenizer.IsInstanceVariableName(str, allowMultiByteCharacters) ||
                Tokenizer.IsClassVariableName(str, allowMultiByteCharacters) ||
                Tokenizer.IsGlobalVariableName(str, allowMultiByteCharacters)
            ) {
                result.Insert(0, ':');
            } else {
                // TODO: this is neither efficient nor complete.
                // Any string that parses as 'sym' should not be quoted.
                switch (str) {
                    case null:
                        // Ruby doesn't allow empty symbols, we can get one from outside though:
                        return MutableString.CreateAscii(":\"\"");

                    case "|":
                    case "^":
                    case "&":
                    case "<=>":
                    case "==":
                    case "===":
                    case "=~":
                    case ">":
                    case ">=":
                    case "<":
                    case "<=":
                    case "<<":
                    case ">>":
                    case "+":
                    case "-":
                    case "*":
                    case "/":
                    case "%":
                    case "**":
                    case "~":
                    case "+@":
                    case "-@":
                    case "[]":
                    case "[]=":
                    case "`":

                    case "$!":
                    case "$@":
                    case "$,":
                    case "$;":
                    case "$/":
                    case "$\\":
                    case "$*":
                    case "$$":
                    case "$?":
                    case "$=":
                    case "$:":
                    case "$\"":
                    case "$<":
                    case "$>":
                    case "$.":
                    case "$~":
                    case "$&":
                    case "$`":
                    case "$'":
                    case "$+":
                        result.Insert(0, ':');
                        break;

                    default:
                        result.Insert(0, ":\"").Append('"');
                        break;
                }
            }

            if (context.RuntimeId != self.RuntimeId) {
                result.Append(" @").Append(self.RuntimeId.ToString(CultureInfo.InvariantCulture));
            }

            return result;
        }

        [RubyMethod("to_i")]
        [RubyMethod("to_int")]
        public static int ToInteger(RubySymbol/*!*/ self) {
            return self.Id;
        }

        [RubyMethod("to_sym")]
        [RubyMethod("intern", Compatibility = RubyCompatibility.Ruby19)]
        public static RubySymbol/*!*/ ToSymbol(RubySymbol/*!*/ self) {
            return self;
        }

        [RubyMethod("to_clr_string")]
        public static string/*!*/ ToClrString(RubySymbol/*!*/ self) {
            return self.ToString();
        }

        [RubyMethod("to_proc")]
        public static Proc/*!*/ ToProc(RubyScope/*!*/ scope, RubySymbol/*!*/ self) {
            return Proc.CreateMethodInvoker(scope, self.ToString());
        }

        #endregion

        #region 1.9 Methods

        // => 
        // <=>
        // ==
        // casecmp
        
        #region =~, match

        [RubyMethod("=~", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(RubyScope/*!*/ scope, RubySymbol/*!*/ self, [NotNull]RubyRegex/*!*/ regex) {
            return MutableStringOps.Match(scope, self.ToMutableString(), regex);
        }

        [RubyMethod("=~", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(ClrName/*!*/ self, [NotNull]RubySymbol/*!*/ str) {
            throw RubyExceptions.CreateTypeError("type mismatch: Symbol given");
        }

        [RubyMethod("=~", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(BinaryOpStorageWithScope/*!*/ storage, RubyScope/*!*/ scope, RubySymbol/*!*/ self, object obj) {
            return MutableStringOps.Match(storage, scope, self.ToMutableString(), obj);
        }

        [RubyMethod("match", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(BinaryOpStorageWithScope/*!*/ storage, RubyScope/*!*/ scope, RubySymbol/*!*/ self, [NotNull]RubyRegex/*!*/ regex) {
            return MutableStringOps.Match(storage, scope, self.ToMutableString(), regex);
        }

        [RubyMethod("match", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(BinaryOpStorageWithScope/*!*/ storage, RubyScope/*!*/ scope, RubySymbol/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ pattern) {
            return MutableStringOps.Match(storage, scope, self.ToMutableString(), pattern);
        }

        #endregion

        // []
        
        // encoding aware
        [RubyMethod("empty?", Compatibility = RubyCompatibility.Ruby19)]
        public static bool IsEmpty(RubySymbol/*!*/ self) {
            return self.IsEmpty;
        }

        // encoding aware
        [RubyMethod("encoding", Compatibility = RubyCompatibility.Ruby19)]
        public static RubyEncoding/*!*/ GetEncoding(RubySymbol/*!*/ self) {
            return self.Encoding;
        }

        // encoding aware
        [RubyMethod("size", Compatibility = RubyCompatibility.Ruby19)]
        [RubyMethod("length", Compatibility = RubyCompatibility.Ruby19)]
        public static int GetLength(RubySymbol/*!*/ self) {
            return (self.Encoding.IsKCoding) ? self.GetByteCount() : self.GetCharCount();
        }

        // next
        // succ
        // slice

        // swapcase
        // upcase
        // capitalize
        // downcase

        #endregion

        #region Public Singleton Methods

        [RubyMethod("all_symbols", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetAllSymbols(RubyClass/*!*/ self) {
            return self.ImmediateClass.Context.GetAllSymbols();
        }

        #endregion
    }
}
