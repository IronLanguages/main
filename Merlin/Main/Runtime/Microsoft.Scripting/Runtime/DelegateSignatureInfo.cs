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
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Contracts;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Used as the key for the ScriptingRuntimeHelpers.GetDelegate method caching system
    /// </summary>
    internal sealed class DelegateSignatureInfo {
        private readonly LanguageContext _context;
        private readonly Type _returnType;
        private readonly ParameterInfo[] _parameters;
        private readonly ConvertBinder _convert;
        private readonly InvokeBinder _invoke;

        internal static readonly object TargetPlaceHolder = new object();
        internal static readonly object CallSitePlaceHolder = new object();
        internal static readonly object ConvertSitePlaceHolder = new object();

        internal DelegateSignatureInfo(LanguageContext context, Type returnType, ParameterInfo[] parameters) {
            Assert.NotNull(context, returnType);
            Assert.NotNullItems(parameters);

            _context = context;
            _parameters = parameters;
            _returnType = returnType;
            
            if (_returnType != typeof(void)) {
                _convert = _context.CreateConvertBinder(_returnType, true);
            }
            
            _invoke = _context.CreateInvokeBinder(new CallInfo(_parameters.Length));
        }

        [Confined]
        public override bool Equals(object obj) {
            DelegateSignatureInfo dsi = obj as DelegateSignatureInfo;

            if (dsi == null || 
                dsi._context != _context ||
                dsi._parameters.Length != _parameters.Length ||
                dsi._returnType != _returnType) {
                return false;
            }

            for (int i = 0; i < _parameters.Length; i++) {
                if (dsi._parameters[i] != _parameters[i]) {
                    return false;
                }
            }

            return true;
        }

        [Confined]
        public override int GetHashCode() {
            int hashCode = 5331;

            for (int i = 0; i < _parameters.Length; i++) {
                hashCode ^= _parameters[i].GetHashCode();
            }
            hashCode ^= _returnType.GetHashCode() ^ _context.GetHashCode();
            return hashCode;
        }

        [Confined]
        public override string ToString() {
            StringBuilder text = new StringBuilder();
            text.Append(_returnType.ToString());
            text.Append("(");
            for (int i = 0; i < _parameters.Length; i++) {
                if (i != 0) text.Append(", ");
                text.Append(_parameters[i].ParameterType.Name);
            }
            text.Append(")");
            return text.ToString();
        }

        internal DelegateInfo GenerateDelegateStub() {
            PerfTrack.NoteEvent(PerfTrack.Categories.DelegateCreate, ToString());

            Type[] delegateParams = new Type[_parameters.Length];
            for (int i = 0; i < _parameters.Length; i++) {
                delegateParams[i] = _parameters[i].ParameterType;
            }

            // Create the method with a special name so the langauge compiler knows that method's stack frame is not visible
            DynamicILGen cg = Snippets.Shared.CreateDynamicMethod("_Scripting_", _returnType, ArrayUtils.Insert(typeof(object[]), delegateParams), false);

            // Emit the stub
            object[] constants = EmitClrCallStub(cg);

            // Save the constants in the delegate info class
            return new DelegateInfo(cg.Finish(), constants, this);
        }

        /// <summary>
        /// Generates stub to receive the CLR call and then call the dynamic language code.
        /// </summary>
        private object[] EmitClrCallStub(ILGen cg) {

            List<ReturnFixer> fixers = new List<ReturnFixer>(0);
            // Create strongly typed return type from the site.
            // This will, among other things, generate tighter code.
            Type[] siteTypes = MakeSiteSignature();

            CallSite callSite = CallSite.Create(DynamicSiteHelpers.MakeCallSiteDelegate(siteTypes), InvokeBinder);
            Type siteType = callSite.GetType();

            Type convertSiteType = null;
            CallSite convertSite = null;

            if (_returnType != typeof(void)) {
                convertSite = CallSite.Create(DynamicSiteHelpers.MakeCallSiteDelegate(typeof(object), _returnType), ConvertBinder);
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

        internal Type ReturnType {
            get {
                return _returnType;
            }
        }

        internal ConvertBinder ConvertBinder {
            get {
                return _convert;
            }
        }

        internal InvokeBinder InvokeBinder {
            get {
                return _invoke;
            }
        }
    }
}
