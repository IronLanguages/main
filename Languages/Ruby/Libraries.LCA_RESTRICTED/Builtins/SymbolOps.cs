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

        #region to_s, inspect, to_sym, to_clr_string, to_proc

        [RubyMethod("id2name")]
        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToString(RubySymbol/*!*/ self) {
            return self.String.Clone();
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, RubySymbol/*!*/ self) {
            var str = self.ToString();
            bool allowMultiByteCharacters = context.RubyOptions.Compatibility >= RubyCompatibility.Ruby19 || context.KCode != null;

            var result = self.String.Clone();

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

                    case "!":
                    case "|":
                    case "^":
                    case "&":
                    case "<=>":
                    case "==":
                    case "===":
                    case "=~":
                    case "!=":
                    case "!~":
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

        #region <=>, ==, ===

        [RubyMethod("<=>")]
        public static int Compare(RubySymbol/*!*/ self, [NotNull]RubySymbol/*!*/ other) {
            return Math.Sign(self.CompareTo(other));
        }

        [RubyMethod("<=>")]
        public static int Compare(RubyContext/*!*/ context, RubySymbol/*!*/ self, [NotNull]ClrName/*!*/ other) {
            return -ClrNameOps.Compare(context, other, self);
        }

        [RubyMethod("<=>")]
        public static object Compare(RubySymbol/*!*/ self, object other) {
            return null;
        }

        [RubyMethod("==")]
        [RubyMethod("===")]
        public static bool Equals(RubySymbol/*!*/ lhs, [NotNull]RubySymbol/*!*/ rhs) {
            return lhs.Equals(rhs);
        }

        [RubyMethod("==")]
        [RubyMethod("===")]
        public static bool Equals(RubyContext/*!*/ context, RubySymbol/*!*/ lhs, [NotNull]ClrName/*!*/ rhs) {
            return ClrNameOps.IsEqual(context, rhs, lhs);
        }

        [RubyMethod("==")]
        [RubyMethod("===")]
        public static bool Equals(RubySymbol/*!*/ self, object other) {
            return false;
        }

        #endregion

        // casecmp
        
        #region =~, match

        [RubyMethod("=~", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(RubyScope/*!*/ scope, RubySymbol/*!*/ self, [NotNull]RubyRegex/*!*/ regex) {
            return MutableStringOps.Match(scope, self.String.Clone(), regex);
        }

        [RubyMethod("=~", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(ClrName/*!*/ self, [NotNull]RubySymbol/*!*/ str) {
            throw RubyExceptions.CreateTypeError("type mismatch: Symbol given");
        }

        [RubyMethod("=~", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(BinaryOpStorageWithScope/*!*/ storage, RubyScope/*!*/ scope, RubySymbol/*!*/ self, object obj) {
            return MutableStringOps.Match(storage, scope, self.String.Clone(), obj);
        }

        [RubyMethod("match", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(BinaryOpStorageWithScope/*!*/ storage, RubyScope/*!*/ scope, RubySymbol/*!*/ self, [NotNull]RubyRegex/*!*/ regex) {
            return MutableStringOps.Match(storage, scope, self.String.Clone(), regex);
        }

        [RubyMethod("match", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(BinaryOpStorageWithScope/*!*/ storage, RubyScope/*!*/ scope, RubySymbol/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ pattern) {
            return MutableStringOps.Match(storage, scope, self.String.Clone(), pattern);
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

        #region all_symbols

        [RubyMethod("all_symbols", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetAllSymbols(RubyClass/*!*/ self) {
            return self.ImmediateClass.Context.GetAllSymbols();
        }

        #endregion
    }
}
