/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Used as the value for the ScriptingRuntimeHelpers.GetDelegate method caching system
    /// </summary>
    internal sealed class DelegateInfo {
        private readonly Type _returnType;
        private readonly ParameterInfo[] _parameters;
        private readonly MethodInfo _method;
        private readonly object[] _constants;
        private WeakDictionary<object, WeakReference> _constantMap = new WeakDictionary<object, WeakReference>();
        private readonly InvokeBinder _invokeBinder;
        private readonly ConvertBinder _convertBinder;

        private static readonly object TargetPlaceHolder = new object();
        private static readonly object CallSitePlaceHolder = new object();
        private static readonly object ConvertSitePlaceHolder = new object();

        internal DelegateInfo(LanguageContext context, Type returnType, ParameterInfo[] parameters) {
            Assert.NotNull(returnType);
            Assert.NotNullItems(parameters);

            _returnType = returnType;
            _parameters = parameters;

            PerfTrack.NoteEvent(PerfTrack.Categories.DelegateCreate, ToString());

            if (_returnType != typeof(void)) {
                _convertBinder = context.CreateConvertBinder(_returnType, true);
            }

            _invokeBinder = context.CreateInvokeBinder(new CallInfo(_parameters.Length));

            Type[] delegateParams = new Type[_parameters.Length];
            for (int i = 0; i < _parameters.Length; i++) {
                delegateParams[i] = _parameters[i].ParameterType;
            }

            // Create the method with a special name so the langauge compiler knows that method's stack frame is not visible
            DynamicILGen cg = Snippets.Shared.CreateDynamicMethod("_Scripting_", _returnType, ArrayUtils.Insert(typeof(object[]), delegateParams), false);

            // Emit the stub
            _constants = EmitClrCallStub(cg);
            _method = cg.Finish();
        }

        internal Delegate CreateDelegate(Type delegateType, object target) {
            Assert.NotNull(delegateType, target);

            // to enable:
            // function x() { }
            // someClass.someEvent += delegateType(x) 
            // someClass.someEvent -= delegateType(x) 
            //
            // we need to avoid re-creating the object array because they won't
            // be compare equal when removing the delegate if they're difference 
            // instances.  Therefore we use a weak hashtable to get back the
            // original object array.  The values also need to be weak to avoid
            // creating a circular reference from the constants target back to the
            // target.  This is fine because as long as the delegate is referenced
            // the object array will stay alive.  Once the delegate is gone it's not
            // wired up anywhere and -= will never be used again.

            object[] clone;            
            lock (_constantMap) {
                WeakReference cloneRef;

                if (!_constantMap.TryGetValue(target, out cloneRef) || 
                    (clone = (object[])cloneRef.Target) == null) {
                    _constantMap[target] = new WeakReference(clone = (object[])_constants.Clone());

                    Type[] siteTypes = MakeSiteSignature();

                    CallSite callSite = CallSite.Create(DynamicSiteHelpers.MakeCallSiteDelegate(siteTypes), _invokeBinder);
                    Type siteType = callSite.GetType();

                    Type convertSiteType = null;
                    CallSite convertSite = null;

                    if (_returnType != typeof(void)) {
                        convertSite = CallSite.Create(DynamicSiteHelpers.MakeCallSiteDelegate(typeof(object), _returnType), _convertBinder);
                        convertSiteType = convertSite.GetType();
                    }

                    Debug.Assert(clone[0] == TargetPlaceHolder);
                    Debug.Assert(clone[1] == CallSitePlaceHolder);
                    Debug.Assert(clone[2] == ConvertSitePlaceHolder);

                    clone[0] = target;
                    clone[1] = callSite;
                    clone[2] = convertSite;
                }
            }

            return ReflectionUtils.CreateDelegate(_method, delegateType, clone);
        }

        /// <summary>
        /// Generates stub to receive the CLR call and then call the dynamic language code.
        /// </summary>
        private object[] EmitClrCallStub(ILGen cg) {

            List<ReturnFixer> fixers = new List<ReturnFixer>(0);
            // Create strongly typed return type from the site.
            // This will, among other things, generate tighter code.
            Type[] siteTypes = MakeSiteSignature();

            CallSite callSite = CallSite.Create(DynamicSiteHelpers.MakeCallSiteDelegate(siteTypes), _invokeBinder);
            Type siteType = callSite.GetType();

            Type convertSiteType = null;
            CallSite convertSite = null;

            if (_returnType != typeof(void)) {
                convertSite = CallSite.Create(DynamicSiteHelpers.MakeCallSiteDelegate(typeof(object), _returnType), _convertBinder);
                convertSiteType = convertSite.GetType();
            }

            // build up constants array
            object[] constants = new object[] { TargetPlaceHolder, CallSitePlaceHolder, ConvertSitePlaceHolder };
            const int TargetIndex = 0, CallSiteIndex = 1, ConvertSiteIndex = 2;

            LocalBuilder convertSiteLocal = null;
            FieldInfo convertTarget = null;
            if (_returnType != typeof(void)) {
                // load up the conversesion logic on the stack
                convertSiteLocal = cg.DeclareLocal(convertSiteType);
                EmitConstantGet(cg, ConvertSiteIndex, convertSiteType);

                cg.Emit(OpCodes.Dup);
                cg.Emit(OpCodes.Stloc, convertSiteLocal);

                convertTarget = convertSiteType.GetField("Target");
                cg.EmitFieldGet(convertTarget);
                cg.Emit(OpCodes.Ldloc, convertSiteLocal);
            }

            // load up the invoke logic on the stack
            LocalBuilder site = cg.DeclareLocal(siteType);
            EmitConstantGet(cg, CallSiteIndex, siteType);
            cg.Emit(OpCodes.Dup);
            cg.Emit(OpCodes.Stloc, site);

            FieldInfo target = siteType.GetField("Target");
            cg.EmitFieldGet(target);
            cg.Emit(OpCodes.Ldloc, site);

            EmitConstantGet(cg, TargetIndex, typeof(object));

            for (int i = 0; i < _parameters.Length; i++) {
                if (_parameters[i].ParameterType.IsByRef) {
                    ReturnFixer rf = ReturnFixer.EmitArgument(cg, i + 1, _parameters[i].ParameterType);
                    if (rf != null) fixers.Add(rf);
                } else {
                    cg.EmitLoadArg(i + 1);
                }
            }

            // emit the invoke for the call
            cg.EmitCall(target.FieldType, "Invoke");

            // emit the invoke for the convert
            if (_returnType == typeof(void)) {
                cg.Emit(OpCodes.Pop);
            } else {
                cg.EmitCall(convertTarget.FieldType, "Invoke");
            }

            // fixup any references
            foreach (ReturnFixer rf in fixers) {
                rf.FixReturn(cg);
            }

            cg.Emit(OpCodes.Ret);
            return constants;
        }

        private static void EmitConstantGet(ILGen il, int index, Type type) {
            il.Emit(OpCodes.Ldarg_0);
            il.EmitInt(index);
            il.Emit(OpCodes.Ldelem_Ref);
            if (type != typeof(object)) {
                il.Emit(OpCodes.Castclass, type);
            }
        }

        internal Type[] MakeSiteSignature() {
            Type[] sig = new Type[_parameters.Length + 2];

            // target object
            sig[0] = typeof(object);

            // arguments
            for (int i = 0; i < _parameters.Length; i++) {
                if (_parameters[i].IsByRefParameter()) {
                    sig[i + 1] = typeof(object);
                } else {
                    sig[i + 1] = _parameters[i].ParameterType;
                }
            }

            // return type
            sig[sig.Length - 1] = typeof(object);

            return sig;
        }
    }
}
