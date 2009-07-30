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
using System.Linq.Expressions;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Compiler.Generation {
    public class RubyTypeEmitter : ClsTypeEmitter {
        private FieldBuilder _immediateClassField;

        public RubyTypeEmitter(TypeBuilder tb)
            : base(tb) {
        }

        internal FieldBuilder ImmediateClassField {
            get { return _immediateClassField; }
            set { _immediateClassField = value; }
        }

        public static void AddRemoveEventHelper(object method, object instance, object dt, object eventValue, string name) {
            throw new NotImplementedException();
        }

        protected override MethodInfo EventHelper() {
            return typeof(RubyTypeEmitter).GetMethod("AddRemoveEventHelper");
        }

        [Emitted]
        public static Exception InvokeMethodMissing(object o, string/*!*/ name) {
            return RubyExceptions.CreateMethodMissing(RubyContext._Default, o, name);
        }

        protected override MethodInfo MissingInvokeMethodException() {
            return typeof(RubyTypeEmitter).GetMethod("InvokeMethodMissing");
        }

        [Emitted]
        public static RubyCallAction/*!*/ MakeRubyCallSite(string/*!*/ methodName, int argumentCount) {
            // TODO: load context from class field?
            return RubyCallAction.MakeShared(methodName, new RubyCallSignature(argumentCount, RubyCallFlags.HasImplicitSelf | RubyCallFlags.IsVirtualCall));
        }

        protected override void EmitMakeCallAction(string name, int nargs, bool isList) {
            ILGen cctor = GetCCtor();
            cctor.Emit(OpCodes.Ldstr, name);
            cctor.EmitInt(nargs);
            cctor.EmitCall(typeof(RubyTypeEmitter), "MakeRubyCallSite");
        }

        protected override FieldInfo GetConversionSite(Type toType) {
            return AllocateDynamicSite(
                new Type[] { typeof(CallSite), typeof(RubyContext), typeof(object), toType },
                (site) => Expression.Assign(
                    Expression.Field(null, site),
                    Expression.Call(
                        null,
                        site.FieldType.GetMethod("Create"),
                        ProtocolConversionAction.GetConversionAction(null, toType, true).CreateExpression()
                    )
                )
            );
        }

        protected override void EmitImplicitContext(ILGen il) {
            il.EmitLoadArg(0);
            EmitClassObjectFromInstance(il);
            il.EmitPropertyGet(typeof(RubyModule), "Context");
        }

        protected override void EmitClassObjectFromInstance(ILGen il) {
            if (typeof(IRubyObject).IsAssignableFrom(BaseType)) {
                il.EmitCall(Methods.IRubyObject_get_ImmediateClass);
            } else {
                il.EmitFieldGet(_immediateClassField);
            }
        }

        protected override bool TryGetName(Type clrType, MethodInfo mi, out string name) {
            name = mi.Name;
            return true;
        }

        protected override bool TryGetName(Type clrType, EventInfo ei, MethodInfo mi, out string name) {
            // TODO: Determine naming convention?
            name = ei.Name;
            return true;
        }

        protected override bool TryGetName(Type clrType, PropertyInfo pi, MethodInfo mi, out string name) {
            if (mi.Name.StartsWith("get_")) {
                name = pi.Name;
            } else if (mi.Name.StartsWith("set_")) {
                name = pi.Name + "=";
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
