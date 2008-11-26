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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;

namespace IronRuby.Builtins {

    public class LibrarySites {
        private static CallSite<Func<CallSite, RubyContext, object, object, bool>> _EqlSite = CallSite<Func<CallSite, RubyContext, object, object, bool>>.Create(
            InstanceCallAction("eql?", 1));

        public static bool Eql(RubyContext/*!*/ context, object self, object other) {
            return _EqlSite.Target(_EqlSite, context, self, other);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> GreaterThanSharedSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction(">", 1));

        public static object GreaterThan(RubyContext/*!*/ context, object lhs, object rhs) {
            return GreaterThanSharedSite.Target(GreaterThanSharedSite, context, lhs, rhs);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> GreaterThanOrEqualSharedSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction(">=", 1));

        public static object GreaterThanOrEqual(RubyContext/*!*/ context, object lhs, object rhs) {
            return GreaterThanOrEqualSharedSite.Target(GreaterThanOrEqualSharedSite, context, lhs, rhs);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> LessThanSharedSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("<", 1));

        public static object LessThan(RubyContext/*!*/ context, object lhs, object rhs) {
            return LessThanSharedSite.Target(LessThanSharedSite, context, lhs, rhs);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> LessThanOrEqualSharedSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("<=", 1));

        public static object LessThanOrEqual(RubyContext/*!*/ context, object lhs, object rhs) {
            return LessThanOrEqualSharedSite.Target(LessThanOrEqualSharedSite, context, lhs, rhs);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> _CompareSharedSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("<=>", 1));

        /// <summary>
        /// Calls &lt;=&gt;, returning the actual result. You should probably use Protocols.Compare, unless you
        /// have some reason for wanting to call &lt;=&gt;directly (e.g. Range.Initialize does this).
        /// </summary>
        public static object Compare(RubyContext/*!*/ context, object lhs, object rhs) {
            return _CompareSharedSite.Target(_CompareSharedSite, context, lhs, rhs);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> _ModuloOpSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("%", 1));

        public static object ModuloOp(RubyContext/*!*/ context, object self, object other) {
            return _ModuloOpSite.Target(_ModuloOpSite, context, self, other);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> _ModuloSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("modulo", 1));

        public static object Modulo(RubyContext/*!*/ context, object self, object other) {
            return _ModuloSite.Target(_ModuloSite, context, self, other);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> _MultiplySite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("*", 1));

        public static object Multiply(RubyContext/*!*/ context, object self, object other) {
            return _MultiplySite.Target(_MultiplySite, context, self, other);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> _MinusSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("-", 1));
        public static object Minus(RubyContext/*!*/ context, object self, object other) {
            return _MinusSite.Target(_MinusSite, context, self, other);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object>> _UnaryMinusSite = CallSite<Func<CallSite, RubyContext, object, object>>.Create(
            InstanceCallAction("-@"));
        public static object UnaryMinus(RubyContext/*!*/ context, object self) {
            return _UnaryMinusSite.Target(_UnaryMinusSite, context, self);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> _PowerSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("**", 1));

        public static object Power(RubyContext/*!*/ context, object self, object other) {
            return _PowerSite.Target(_PowerSite, context, self, other);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> _AddSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("+", 1));

        public static object Add(RubyContext/*!*/ context, object self, object other) {
            return _AddSite.Target(_AddSite, context, self, other);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> _SubtractSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("-", 1));

        public static object Subtract(RubyContext/*!*/ context, object self, object other) {
            return _SubtractSite.Target(_SubtractSite, context, self, other);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> _DivideSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("/", 1));

        public static object Divide(RubyContext/*!*/ context, object self, object other) {
            return _DivideSite.Target(_DivideSite, context, self, other);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> _DivSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("div", 1));

        public static object Div(RubyContext/*!*/ context, object self, object other) {
            return _DivSite.Target(_DivSite, context, self, other);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> _RemainderSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("remainder", 1));

        public static object Remainder(RubyContext/*!*/ context, object self, object other) {
            return _RemainderSite.Target(_RemainderSite, context, self, other);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, RubyArray>> _DivModSite = CallSite<Func<CallSite, RubyContext, object, object, RubyArray>>.Create(
            InstanceCallAction("divmod", 1));
        public static RubyArray DivMod(RubyContext/*!*/ context, object self, object other) {
            return _DivModSite.Target(_DivModSite, context, self, other);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> _QuoSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("quo", 1));
        public static object Quo(RubyContext/*!*/ context, object self, object other) {
            return _QuoSite.Target(_QuoSite, context, self, other);
        }

        private static CallSite<Func<CallSite, RubyContext, object, object, int>> _BitRefSite = CallSite<Func<CallSite, RubyContext, object, object, int>>.Create(
            InstanceCallAction("[]", 1));
        public static int BitRef(RubyContext/*!*/ context, object self, object other) {
            return _BitRefSite.Target(_BitRefSite, context, self, other);
        }

        private static CallSite<Func<CallSite, RubyContext, object, bool>> _IsZeroSite = CallSite<Func<CallSite, RubyContext, object, bool>>.Create(
            InstanceCallAction("zero?"));
        public static bool IsZero(RubyContext/*!*/ context, object self) {
            return _IsZeroSite.Target(_IsZeroSite, context, self);
        }

        private static CallSite<Func<CallSite, RubyContext, RubyClass, object, object>> _InducedFromSite = CallSite<Func<CallSite, RubyContext, RubyClass, object, object>>.Create(
            InstanceCallAction("induced_from", 1));
        public static object InvokeInducedFrom(RubyContext/*!*/ context, RubyClass klass, object value) {
            return _InducedFromSite.Target(_InducedFromSite, context, klass, value);
        }

        private static CallSite<Func<CallSite, RubyContext, object, RubyClass, object>> _PrecSite = CallSite<Func<CallSite, RubyContext, object, RubyClass, object>>.Create(
            InstanceCallAction("prec", 1));
        public static object InvokePrec(RubyContext/*!*/ context, RubyClass klass, object value) {
            return _PrecSite.Target(_PrecSite, context, value, klass);
        }

        private static CallSite<Func<CallSite, RubyContext, IList, IList>> _FlattenSite = CallSite<Func<CallSite, RubyContext, IList, IList>>.Create(
            InstanceCallAction("flatten", 0));
        public static IList InvokeFlatten(RubyContext/*!*/ context, IList list) {
            return _FlattenSite.Target(_FlattenSite, context, list);
        }
        #region Helpers

        public static RubyCallAction InstanceCallAction(string/*!*/ name, RubyCallSignature signature) {
            return RubyCallAction.Make(name, signature);
        }
        
        public static RubyCallAction InstanceCallAction(string/*!*/ name) {
            return RubyCallAction.Make(name, RubyCallSignature.Simple(0));
        }

        public static RubyCallAction InstanceCallAction(string/*!*/ name, int argumentCount) {
            return RubyCallAction.Make(name, RubyCallSignature.Simple(argumentCount));
        }

        #endregion
    }
}
