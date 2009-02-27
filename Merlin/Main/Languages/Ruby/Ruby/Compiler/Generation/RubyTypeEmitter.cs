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
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Compiler.Generation {
    public class RubyTypeEmitter : ClsTypeEmitter {
        private FieldBuilder _classField;

        public RubyTypeEmitter(TypeBuilder tb)
            : base(tb) {
        }

        internal FieldBuilder ClassField {
            get { return _classField; }
            set { _classField = value; }
        }

        public static bool TryGetNonInheritedMethodHelper(object clsObject, object instance, string/*!*/ name, out object callTarget) {
            // In Ruby, this simply returns the instance object
            // It's the callable site that's bound to the name through a RubyCallAction
            // Properties are equivalent to Ruby getter and setter methods
            RubyClass cls = clsObject as RubyClass;
            RubyMemberInfo method;
            // TODO: visibility
            if (cls == null || (method = cls.ResolveMethod(name, RubyClass.IgnoreVisibility).Info) == null || (method is RubyMethodGroupInfo)) {
                callTarget = null;
                return false;
            }
            callTarget = instance;
            return true;
        }

        protected override MethodInfo NonInheritedMethodHelper() {
            return typeof(RubyTypeEmitter).GetMethod("TryGetNonInheritedMethodHelper");
        }

        protected override MethodInfo NonInheritedValueHelper() {
            return typeof(RubyTypeEmitter).GetMethod("TryGetNonInheritedMethodHelper");
        }

        public static void AddRemoveEventHelper(object method, object instance, object dt, object eventValue, string name) {
            throw new NotImplementedException();
        }

        protected override MethodInfo EventHelper() {
            return typeof(RubyTypeEmitter).GetMethod("AddRemoveEventHelper");
        }

        protected override MethodInfo GetFastConvertMethod(Type toType) {
            return RubyBinder.GetFastConvertMethod(toType);
        }

        protected override MethodInfo GetGenericConvertMethod(Type toType) {
            return RubyBinder.GetGenericConvertMethod(toType);
        }

        public static Exception InvokeMethodMissing(object o, string/*!*/ name) {
            return RubyExceptions.CreateMethodMissing(RubyContext._Default, o, name);
        }

        protected override MethodInfo MissingInvokeMethodException() {
            return typeof(RubyTypeEmitter).GetMethod("InvokeMethodMissing");
        }

        protected override MethodInfo ConvertToDelegate() {
            return typeof(Converter).GetMethod("ConvertToDelegate");
        }

        protected override void EmitMakeCallAction(string name, int nargs, bool isList) {
            ILGen cctor = GetCCtor();
            cctor.Emit(OpCodes.Ldstr, name);
            cctor.EmitInt(nargs);
            cctor.EmitCall(typeof(RubyCallAction), "Make", new Type[] { typeof(string), typeof(int) });
        }

        protected override void EmitPropertyGet(ILGen il, MethodInfo mi, string name, LocalBuilder callTarget) {
            EmitClrCallStub(il, mi, callTarget, name);
        }

        protected override void EmitPropertySet(ILGen il, MethodInfo mi, string name, LocalBuilder callTarget) {
            EmitClrCallStub(il, mi, callTarget, name);
            il.Emit(OpCodes.Pop);
        }

        protected override void EmitImplicitContext(ILGen il) {
            il.EmitLoadArg(0);
            EmitClassObjectFromInstance(il);
            il.EmitPropertyGet(typeof(RubyModule), "Context");
        }

        protected override void EmitClassObjectFromInstance(ILGen il) {
            if (typeof(IRubyObject).IsAssignableFrom(BaseType)) {
                il.EmitPropertyGet(typeof(IRubyObject), "Class");
            } else {
                il.EmitFieldGet(_classField);
            }
        }

        protected override bool TryGetName(Type clrType, MethodInfo mi, out string name) {
            name = RubyUtils.MangleName(mi.Name);
            return true;
        }

        protected override bool TryGetName(Type clrType, EventInfo ei, MethodInfo mi, out string name) {
            // TODO: Determine naming convention?
            name = RubyUtils.MangleName(ei.Name);
            return true;
        }

        protected override bool TryGetName(Type clrType, PropertyInfo pi, MethodInfo mi, out string name) {
            if (mi.Name.StartsWith("get_")) {
                name = RubyUtils.MangleName(pi.Name);
            } else if (mi.Name.StartsWith("set_")) {
                name = RubyUtils.MangleName(pi.Name) + "=";
            } else {
                name = null;
                return false;
            }
            return true;
        }

        protected override Type/*!*/[]/*!*/ MakeSiteSignature(int nargs) {
            Type[] sig = new Type[nargs + 4];
            sig[0] = typeof(CallSite);
            sig[1] = typeof(RubyContext);
            for (int i = 2; i < sig.Length; i++) {
                sig[i] = typeof(object);
            }
            return sig;
        }

        protected override Type/*!*/ ContextType {
            get {
                return typeof(RubyContext);
            }
        }
    }
}
