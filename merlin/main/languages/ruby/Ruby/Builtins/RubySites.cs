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
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins {

    // TODO: Many of these methods have strongly typed results (int, bool, etc).
    //       While this is nice for using them in C#, it is incorrect, because the
    //       method could return an arbitrary object. Currently the binder will
    //       generate calls into Converter, which will not have the right behavior.
    public static class RubySites {
        private static readonly CallSite<Func<CallSite, RubyContext, object, MutableString>> InspectSharedSite =
            CallSite<Func<CallSite, RubyContext, object, MutableString>>.Create(InstanceCallAction("inspect"));

        public static MutableString Inspect(RubyContext/*!*/ context, object obj) {
            return InspectSharedSite.Target(InspectSharedSite, context, obj);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, RubyClass, object>> AllocateSharedSite =
            CallSite<Func<CallSite, RubyContext, RubyClass, object>>.Create(InstanceCallAction("allocate"));

        public static object Allocate(RubyClass/*!*/ classObj) {
            return AllocateSharedSite.Target(AllocateSharedSite, classObj.Context, classObj);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, object, object>> CaseEqualSharedSite =
            CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(InstanceCallAction("===", 1));

        public static bool CaseEqual(RubyContext/*!*/ context, object lhs, object rhs) {
            return RubyOps.IsTrue(CaseEqualSharedSite.Target(CaseEqualSharedSite, context, lhs, rhs));
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, object, object>> EqualSharedSite =
            CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(InstanceCallAction("==", 1));

        public static bool Equal(RubyContext/*!*/ context, object lhs, object rhs) {
            return RubyOps.IsTrue(EqualSharedSite.Target(EqualSharedSite, context, lhs, rhs));
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, SymbolId, object>> RespondToSharedSite =
            CallSite<Func<CallSite, RubyContext, object, SymbolId, object>>.Create(InstanceCallAction("respond_to?", 1));

        public static bool RespondTo(RubyContext/*!*/ context, object obj, SymbolId name) {
            return RubyOps.IsTrue(RespondToSharedSite.Target(RespondToSharedSite, context, obj, name));
        }

        public static bool RespondTo(RubyContext/*!*/ context, object obj, string name) {
            return RubyOps.IsTrue(RespondToSharedSite.Target(RespondToSharedSite, context, obj, SymbolTable.StringToId(name)));
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, object>> _ToISharedSite =
            CallSite<Func<CallSite, RubyContext, object, object>>.Create(InstanceCallAction("to_i"));

        public static object ToI(RubyContext/*!*/ context, object value) {
            return _ToISharedSite.Target(_ToISharedSite, context, value);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, object>> _ToIntSharedSite = CallSite<Func<CallSite, RubyContext, object, object>>.Create(
            InstanceCallAction("to_int"));

        public static object ToInt(RubyContext/*!*/ context, object value) {
            return _ToIntSharedSite.Target(_ToIntSharedSite, context, value);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, double>> _ToFSharedSite = CallSite<Func<CallSite, RubyContext, object, double>>.Create(
            InstanceCallAction("to_f"));

        public static double ToF(RubyContext/*!*/ context, object value) {
            return _ToFSharedSite.Target(_ToFSharedSite, context, value);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, MutableString>> ToStrSharedSite = CallSite<Func<CallSite, RubyContext, object, MutableString>>.Create(
            InstanceCallAction("to_str"));

        public static MutableString ToStr(RubyContext/*!*/ context, object value) {
            return ToStrSharedSite.Target(ToStrSharedSite, context, value);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, Range>> ToRangeSharedSite = CallSite<Func<CallSite, RubyContext, object, Range>>.Create(
            InstanceCallAction("to_range"));

        public static Range ToRange(RubyContext/*!*/ context, object value) {
            return ToRangeSharedSite.Target(ToRangeSharedSite, context, value);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, MutableString>> ToSSharedSite = CallSite<Func<CallSite, RubyContext, object, MutableString>>.Create(
            InstanceCallAction("to_s"));

        public static MutableString ToS(RubyContext/*!*/ context, object value) {
            return ToSSharedSite.Target(ToSSharedSite, context, value);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, Range, object>> RangeBeginSharedSite = CallSite<Func<CallSite, RubyContext, Range, object>>.Create(
            InstanceCallAction("begin"));

        public static object RangeBegin(RubyContext/*!*/ context, Range range) {
            return RangeBeginSharedSite.Target(RangeBeginSharedSite, context, range);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, Range, object>> RangeEndSharedSite = CallSite<Func<CallSite, RubyContext, Range, object>>.Create(
            InstanceCallAction("end"));

        public static object RangeEnd(RubyContext/*!*/ context, Range range) {
            return RangeEndSharedSite.Target(RangeEndSharedSite, context, range);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, Range, bool>> RangeExcludeEndSharedSite = CallSite<Func<CallSite, RubyContext, Range, bool>>.Create(
            InstanceCallAction("exclude_end?"));

        public static bool RangeExcludeEnd(RubyContext/*!*/ context, Range range) {
            return RangeExcludeEndSharedSite.Target(RangeExcludeEndSharedSite, context, range);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, RubyModule, SymbolId, object>> ModuleConstMissingSharedSite = CallSite<Func<CallSite, RubyContext, RubyModule, SymbolId, object>>.Create(
            InstanceCallAction("const_missing", 1));

        public static object ModuleConstMissing(RubyContext/*!*/ context, RubyModule self, string/*!*/ name) {
            return ModuleConstMissingSharedSite.Target(ModuleConstMissingSharedSite, context, self, SymbolTable.StringToId(name));
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, IDictionary<object, object>>> _ToHashSharedSite = CallSite<Func<CallSite, RubyContext, object, IDictionary<object, object>>>.Create(
            InstanceCallAction("to_hash"));

        public static IDictionary<object, object> ToHash(RubyContext/*!*/ context, object obj) {
            return _ToHashSharedSite.Target(_ToHashSharedSite, context, obj);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, Proc, object>> _EachSharedSite = CallSite<Func<CallSite, RubyContext, object, Proc, object>>.Create(
            InstanceCallAction("each", RubyCallSignature.WithBlock(0)));

        public static object Each(RubyContext/*!*/ context, object self, Proc block) {
            return _EachSharedSite.Target(_EachSharedSite, context, self, block);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, object>> _SuccSharedSite = CallSite<Func<CallSite, RubyContext, object, object>>.Create(InstanceCallAction("succ"));

        public static object Successor(RubyContext/*!*/ context, object self) {
            return _SuccSharedSite.Target(_SuccSharedSite, context, self);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, object>> _CallSharedSite = CallSite<Func<CallSite, RubyContext, object, object>>.Create(InstanceCallAction("call"));

        public static object Call(RubyContext/*!*/ context, object self) {
            return _CallSharedSite.Target(_CallSharedSite, context, self);
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, object, object>> _CompareSharedSite = CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
            InstanceCallAction("<=>", 1));

        /// <summary>
        /// Calls &lt;=&gt;, returning the actual result. You should probably use Protocols.Compare, unless you
        /// have some reason for wanting to call &lt;=&gt;directly (e.g. Range.Initialize does this).
        /// </summary>
        public static object Compare(RubyContext/*!*/ context, object lhs, object rhs) {
            return _CompareSharedSite.Target(_CompareSharedSite, context, lhs, rhs);
        }

        #region Helpers

        public static RubyCallAction InstanceCallAction(string/*!*/ name) {
            return RubyCallAction.Make(name, 0);
        }
        
        public static RubyCallAction InstanceCallAction(string/*!*/ name, int argumentCount) {
            return RubyCallAction.Make(name, argumentCount);
        }

        public static RubyCallAction InstanceCallAction(string/*!*/ name, RubyCallSignature callSignature) {
            return RubyCallAction.Make(name, callSignature);
        }

        #endregion

        /// <summary>
        /// Dynamically invoke the coerce method on the object specified by self.
        /// </summary>
        /// <param name="context">The current context</param>
        /// <param name="self">The object on which to invoke the method</param>
        /// <param name="other">The object that we are coercing</param>
        /// <returns>A two element array containing [other, self] coerced by self</returns>
        /// <exception cref="InvalidOperationException">Raises TypeError if coerce does not return a two element array - [x,y].</exception>
        public static RubyArray Coerce(RubyContext/*!*/ context, object self, object other) {
            RubyArray array;
            array = _CoerceSite.Target(_CoerceSite, context, self, other);
            if (array == null || array.Count != 2) {
                throw RubyExceptions.CreateTypeError("coerce must return [x, y]");
            }
            return array;
        }

        private static readonly CallSite<Func<CallSite, RubyContext, object, object, RubyArray>> _CoerceSite = CallSite<Func<CallSite, RubyContext, object, object, RubyArray>>.Create(
            InstanceCallAction("coerce", 1));
    }
}
