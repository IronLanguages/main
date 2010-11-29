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
using System.Runtime.InteropServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace IronRuby.Builtins {

    [RubyClass("BasicObject", Extends = typeof(BasicObject), Restrictions = ModuleRestrictions.NoNameMapping | ModuleRestrictions.NotPublished | ModuleRestrictions.NoUnderlyingType)]
    public static class BasicObjectOps {
        // RubyConstructor implemented by BasicObject ctors

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
        public static object Reinitialize(object self, params object[]/*!*/ args) {
            // ignores args
            return self;
        }

        #region singleton_method_added, singleton_method_removed, singleton_method_undefined, method_missing

        [RubyMethod("singleton_method_added", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
        public static void MethodAdded(object self, object methodName) {
            // nop
        }

        [RubyMethod("singleton_method_removed", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
        public static void MethodRemoved(object self, object methodName) {
            // nop
        }

        [RubyMethod("singleton_method_undefined", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
        public static void MethodUndefined(object self, object methodName) {
            // nop
        }

        // This method is a binder intrinsic and the behavior of the binder needs to be adjusted appropriately if changed.
        [RubyMethod("method_missing", RubyMethodAttributes.PrivateInstance)]
        [RubyStackTraceHidden]
        public static object MethodMissing(RubyContext/*!*/ context, object/*!*/ self, [NotNull]RubySymbol/*!*/ name, params object[]/*!*/ args) {
            throw RubyExceptions.CreateMethodMissing(context, self, name.ToString());
        }

        #endregion

        #region __send__

        [RubyMethod("__send__")]
        public static object SendMessage(RubyScope/*!*/ scope, object self) {
            return KernelOps.SendMessage(scope, self);
        }

        // ARGS: 0
        [RubyMethod("__send__")]
        public static object SendMessage(RubyScope/*!*/ scope, object self, [DefaultProtocol, NotNull]string/*!*/ methodName) {
            return KernelOps.SendMessage(scope, self, methodName);
        }

        // ARGS: 0&
        [RubyMethod("__send__")]
        public static object SendMessage(RubyScope/*!*/ scope, BlockParam block, object self, [DefaultProtocol, NotNull]string/*!*/ methodName) {
            return KernelOps.SendMessage(scope, block, self, methodName);
        }

        // ARGS: 1
        [RubyMethod("__send__")]
        public static object SendMessage(RubyScope/*!*/ scope, object self, [DefaultProtocol, NotNull]string/*!*/ methodName,
            object arg1) {
            return KernelOps.SendMessage(scope, self, methodName, arg1);
        }

        // ARGS: 1&
        [RubyMethod("__send__")]
        public static object SendMessage(RubyScope/*!*/ scope, BlockParam block, object self, [DefaultProtocol, NotNull]string/*!*/ methodName,
            object arg1) {
            return KernelOps.SendMessage(scope, block, self, methodName, arg1);
        }

        // ARGS: 2
        [RubyMethod("__send__")]
        public static object SendMessage(RubyScope/*!*/ scope, object self, [DefaultProtocol, NotNull]string/*!*/ methodName,
            object arg1, object arg2) {
            return KernelOps.SendMessage(scope, self, methodName, arg1, arg2);
        }

        // ARGS: 2&
        [RubyMethod("__send__")]
        public static object SendMessage(RubyScope/*!*/ scope, BlockParam block, object self, [DefaultProtocol, NotNull]string/*!*/ methodName,
            object arg1, object arg2) {
            return KernelOps.SendMessage(scope, block, self, methodName, arg1, arg2);
        }

        // ARGS: 3
        [RubyMethod("__send__")]
        public static object SendMessage(RubyScope/*!*/ scope, object self, [DefaultProtocol, NotNull]string/*!*/ methodName,
            object arg1, object arg2, object arg3) {
            return KernelOps.SendMessage(scope, self, methodName, arg1, arg2, arg3);
        }

        // ARGS: 3&
        [RubyMethod("__send__")]
        public static object SendMessage(RubyScope/*!*/ scope, BlockParam block, object self, [DefaultProtocol, NotNull]string/*!*/ methodName,
            object arg1, object arg2, object arg3) {
            return KernelOps.SendMessage(scope, block, self, methodName, arg1, arg2, arg3);
        }

        // ARGS: N
        [RubyMethod("__send__")]
        public static object SendMessage(RubyScope/*!*/ scope, object self, [DefaultProtocol, NotNull]string/*!*/ methodName,
            params object[]/*!*/ args) {
            return KernelOps.SendMessage(scope, self, methodName, args);
        }

        // ARGS: N&
        [RubyMethod("__send__")]
        public static object SendMessage(RubyScope/*!*/ scope, BlockParam block, object self, [DefaultProtocol, NotNull]string/*!*/ methodName,
            params object[]/*!*/ args) {
            return KernelOps.SendMessage(scope, block, self, methodName, args);
        }

        #endregion

        #region ==, !=, !, equal?

        [RubyMethod("==")]
        public static bool ValueEquals([NotNull]IRubyObject/*!*/ self, object other) {
            return self.BaseEquals(other);
        }

        [RubyMethod("==")]
        public static bool ValueEquals(object self, object other) {
            return Object.Equals(self, other);
        }

        [RubyMethod("!")]
        public static bool Not(object self) {
            return RubyOps.IsFalse(self);
        }

        [RubyMethod("!=")]
        public static bool ValueNotEquals(BinaryOpStorage/*!*/ eql, object self, object other) {
            var site = eql.GetCallSite("==", 1);
            return RubyOps.IsFalse(site.Target(site, self, other));
        }
        
        [RubyMethod("equal?")]
        public static bool IsEqual(object self, object other) {
            // Comparing object IDs is (potentially) expensive because it forces us
            // to generate InstanceData and a new object ID
            if (self == other) {
                return true;
            }

            if (RubyUtils.IsRubyValueType(self) && RubyUtils.IsRubyValueType(other)) {
                return object.Equals(self, other);
            }

            return false;
        }

        #endregion

        #region instance_eval, instance_exec

        [RubyMethod("instance_eval")]
        public static object Evaluate(RubyScope/*!*/ scope, object self, [NotNull]MutableString/*!*/ code,
            [Optional, NotNull]MutableString file, [DefaultParameterValue(1)]int line) {

            RubyClass singleton = scope.RubyContext.GetOrCreateSingletonClass(self);
            return RubyUtils.Evaluate(code, scope, self, singleton, file, line);
        }

        [RubyMethod("instance_eval")]
        public static object InstanceEval([NotNull]BlockParam/*!*/ block, object self) {
            return RubyUtils.EvaluateInSingleton(self, block, null);
        }

        [RubyMethod("instance_exec")]
        public static object InstanceExec([NotNull]BlockParam/*!*/ block, object self, params object[]/*!*/ args) {
            return RubyUtils.EvaluateInSingleton(self, block, args);
        }

        #endregion
    }
}
