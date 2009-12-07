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

    [RubyClass("Symbol", Extends = typeof(SymbolId), Inherits = typeof(Object))]
    [HideMethod("==")]
    public static class SymbolOps {

        #region Public Instance Methods

        [RubyMethod("id2name")]
        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToString(SymbolId self) {
            return MutableString.Create(SymbolTable.IdToString(self), RubyEncoding.Symbol);
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, SymbolId self) {
            var str = SymbolTable.IdToString(self);
            bool allowMultiByteCharacters = context.RubyOptions.Compatibility >= RubyCompatibility.Ruby19 || context.KCode != null;

            // simple cases:
            if (
                Tokenizer.IsMethodName(str, allowMultiByteCharacters) ||
                Tokenizer.IsConstantName(str, allowMultiByteCharacters) ||
                Tokenizer.IsInstanceVariableName(str, allowMultiByteCharacters) ||
                Tokenizer.IsClassVariableName(str, allowMultiByteCharacters) ||
                Tokenizer.IsGlobalVariableName(str, allowMultiByteCharacters)
            ) {
                return MutableString.CreateMutable(1 + str.Length, RubyEncoding.Obsolete).Append(':').Append(str);
            }

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
                    return MutableString.CreateMutable(1 + str.Length, RubyEncoding.Obsolete).Append(':').Append(str);
            }

            return MutableString.CreateMutable(3 + str.Length, RubyEncoding.Obsolete).Append(":\"").Append(str).Append('"');
        }

        [RubyMethod("to_i")]
        [RubyMethod("to_int")]
        public static int ToInteger(SymbolId self) {
            return self.Id;
        }

        [RubyMethod("to_sym")]
        public static SymbolId ToSymbol(SymbolId self) {
            return self;
        }

        [RubyMethod("to_clr_string")]
        public static string ToClrString(SymbolId self) {
            return SymbolTable.IdToString(self);
        }

        #endregion

        #region 1.9 Methods

        // => 
        // <=>
        // ==

        #region =~, match

        [RubyMethod("=~", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(RubyScope/*!*/ scope, SymbolId self, [NotNull]RubyRegex/*!*/ regex) {
            return MutableStringOps.Match(scope, ToString(self), regex);
        }

        [RubyMethod("=~", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(ClrName/*!*/ self, SymbolId str) {
            throw RubyExceptions.CreateTypeError("type mismatch: Symbol given");
        }

        [RubyMethod("=~", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(BinaryOpStorageWithScope/*!*/ storage, RubyScope/*!*/ scope, SymbolId self, object obj) {
            return MutableStringOps.Match(storage, scope, ToString(self), obj);
        }

        [RubyMethod("match", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(BinaryOpStorageWithScope/*!*/ storage, RubyScope/*!*/ scope, SymbolId self, [NotNull]RubyRegex/*!*/ regex) {
            return MutableStringOps.Match(storage, scope, ToString(self), regex);
        }

        [RubyMethod("match", Compatibility = RubyCompatibility.Ruby19)]
        public static object Match(BinaryOpStorageWithScope/*!*/ storage, RubyScope/*!*/ scope, SymbolId self, [DefaultProtocol, NotNull]MutableString/*!*/ pattern) {
            return MutableStringOps.Match(storage, scope, ToString(self), pattern);
        }

        #endregion

        // []
        // capitalize
        // casecmp
        // downcase
        // empty?
        // encoding
        // intern
        // length
        // match
        // next
        // size
        // slice
        // succ
        // swapcase
        // to_proc
        // upcase

        #endregion

        #region Public Singleton Methods

        [RubyMethod("all_symbols", RubyMethodAttributes.PublicSingleton)]
        public static List<object>/*!*/ GetAllSymbols(object self) {
            // TODO:
            throw new NotImplementedError();
        }

        #endregion
    }
}
