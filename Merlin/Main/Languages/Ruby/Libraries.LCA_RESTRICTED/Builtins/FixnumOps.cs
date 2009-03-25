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
using IronRuby.Runtime;
using Microsoft.Scripting;

namespace IronRuby.Builtins {
    [RubyClass("Fixnum", Extends = typeof(int), Inherits = typeof(Integer)), Includes(typeof(ClrInteger), Copy = true)]
    public static partial class Int32Ops {
        #region to_sym

        /// <summary>
        /// Returns the Symbol whose integer value is self
        /// <seealso cref="FixnumOps.id2name"/>
        /// </summary>
        /// <returns>Symbol or nil if there is no symbol with id of self</returns>
        /// <example>
        /// fred = :fred.to_i
        /// fred.id2name   #=> "fred"
        /// fred.to_sym    #=> :fred
        /// </example>
        [RubyMethod("to_sym")]
        public static object ToSymbol(int self) {
            SymbolId result;
            return RubyOps.TryConvertFixnumToSymbol(self, out result) ? (object)result : null;
        }

        #endregion

        #region id2name

        /// <summary>
        /// Returns the name of the object whose symbol id is self.
        /// </summary>
        /// <returns>MutableString or nil if there is no symbol with id of self.</returns>
        /// <example>
        /// symbol = :@inst_var    #=> :@inst_var
        /// id     = symbol.to_i   #=> 9818
        /// id.id2name             #=> "@inst_var"
        /// </example>
        [RubyMethod("id2name")]
        public static object Id2Name(int self) {
            SymbolId id = new SymbolId(self);
            return SymbolTable.ContainsId(id) ? (object)SymbolOps.ToString(id) : null;
        }

        #endregion

        [RubyMethod("size")]
        public static int Size(int self) {
            return sizeof(int);
        }

        [RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
        public static int InducedFrom(RubyClass/*!*/ self, [DefaultProtocol]int value) {
            return value;
        }

        [RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
        public static int InducedFrom(RubyClass/*!*/ self, double value) {
            if (value >= Int32.MinValue && value <= Int32.MaxValue) {
                return (Int32)value;
            }
            throw RubyExceptions.CreateRangeError(String.Format("Float {0} out of range of {1}", value, self.Name));
        }
    }
}
