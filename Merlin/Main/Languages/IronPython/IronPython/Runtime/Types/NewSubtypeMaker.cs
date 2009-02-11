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
using System.Reflection;
using System.Reflection.Emit;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Generation;

namespace IronPython.Runtime.Types {

    /// <summary>
    /// Creates sub-types of new-types.  Sub-types of new types are created when
    /// the new-type is created with slots, and therefore has a concrete object
    /// layout which the subtype also inherits.
    /// </summary>
    class NewSubtypeMaker : NewTypeMaker {
        public NewSubtypeMaker(PythonTuple bases, NewTypeInfo ti)
            : base(bases, ti) {
        }

        protected override string GetName() {
            return base.GetName().Substring(TypePrefix.Length);
        }

        protected override void ImplementInterfaces() {
            // only implement interfaces defined in our newly derived type
            IList<Type> baseInterfaces = _baseType.GetInterfaces();
            foreach (Type interfaceType in _interfaceTypes) {
                if (!baseInterfaces.Contains(interfaceType)) {
                    ImplementInterface(interfaceType);
                }
            }
        }

        protected override ParameterInfo[] GetOverrideCtorSignature(ParameterInfo[] original) {
            return original;
        }

        protected override bool ShouldOverrideVirtual(MethodInfo mi) {
            return !IsInstanceType(mi.DeclaringType);
        }

        protected override void ImplementPythonObject() {
            if (NeedsPythonObject && NeedsDictionary) {
                // override our bases slots implementation w/ one that
                // can use dicts
                MethodInfo decl; MethodBuilder impl;
                ILGen il = DefineMethodOverride(typeof(IPythonObject), "get_Dict", out decl, out impl);
                il.EmitLoadArg(0);
                EmitGetDict(il);
                il.Emit(OpCodes.Ret);
                _tg.DefineMethodOverride(impl, decl);

                il = DefineMethodOverride(typeof(IPythonObject), "get_HasDictionary", out decl, out impl);
                il.EmitBoolean(true);
                il.Emit(OpCodes.Ret);
                _tg.DefineMethodOverride(impl, decl);

                il = DefineMethodOverride(typeof(IPythonObject), "ReplaceDict", out decl, out impl);
                il.EmitLoadArg(0);
                il.EmitLoadArg(1);
                EmitSetDict(il);
                il.EmitBoolean(true);
                il.Emit(OpCodes.Ret);
                _tg.DefineMethodOverride(impl, decl);

                il = DefineMethodOverride(typeof(IPythonObject), "SetDict", out decl, out impl);
                il.EmitLoadArg(0);
                il.EmitFieldAddress(_dictField);
                il.EmitLoadArg(1);
                il.EmitCall(typeof(UserTypeOps), "SetDictHelper");
                il.Emit(OpCodes.Ret);
                _tg.DefineMethodOverride(impl, decl);
            }
        }

        private bool NeedsNewWeakRef() {
            foreach (PythonType dt in _baseClasses) {
                PythonTypeSlot dts;
                if (dt.TryLookupSlot(DefaultContext.Default, Symbols.WeakRef, out dts))
                    return false;
            }
            return true;
        }

        protected override void ImplementWeakReference() {
            if (NeedsNewWeakRef()
                && (_slots == null || _slots.Contains("__weakref__"))) {
                // base type didn't have slots, but it's there now...
                base.ImplementWeakReference();
            }
        }
    }
}
