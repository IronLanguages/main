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
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Microsoft.Scripting.Generation {
    sealed class ReturnFixer {
        private readonly LocalBuilder _refSlot;
        private readonly int _argIndex;
        private readonly Type _argType;

        private ReturnFixer(LocalBuilder refSlot, int argIndex, Type argType) {
            Debug.Assert(refSlot.LocalType.IsGenericType && refSlot.LocalType.GetGenericTypeDefinition() == typeof(StrongBox<>));
            _refSlot = refSlot;
            _argIndex = argIndex;
            _argType = argType;
        }

        internal static ReturnFixer EmitArgument(ILGen cg, int argIndex, Type argType) {
            cg.EmitLoadArg(argIndex);

            if (!argType.IsByRef) {
                cg.EmitBoxing(argType);
                return null;
            }

            Type elementType = argType.GetElementType();
            cg.EmitLoadValueIndirect(elementType);
            Type concreteType = typeof(StrongBox<>).MakeGenericType(elementType);
            cg.EmitNew(concreteType, new Type[] { elementType });

            LocalBuilder refSlot = cg.DeclareLocal(concreteType);
            cg.Emit(OpCodes.Dup);
            cg.Emit(OpCodes.Stloc, refSlot);
            return new ReturnFixer(refSlot, argIndex, argType);
        }

        internal void FixReturn(ILGen cg) {
            cg.EmitLoadArg(_argIndex);
            cg.Emit(OpCodes.Ldloc, _refSlot);
            cg.Emit(OpCodes.Ldfld, _refSlot.LocalType.GetField("Value"));
            cg.EmitStoreValueIndirect(_argType.GetElementType());
        }
    }
}
