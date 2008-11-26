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

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Dynamic.Utils;
using System.Threading;

namespace System.Linq.Expressions.Compiler {
    internal static partial class DelegateHelpers {

        // TODO: suspect we don't need this cache, since we're already caching with
        // _DelegateCache
        private static Dictionary<ICollection<Type>, Type> _DelegateTypes;

        private static Type MakeCustomDelegate(Type[] types) {
            if (_DelegateTypes == null) {
                Interlocked.CompareExchange(
                    ref _DelegateTypes,
                    new Dictionary<ICollection<Type>, Type>(ListEqualityComparer<Type>.Instance),
                    null
                );
            }

            bool found;
            Type type;

            //
            // LOCK to retrieve the delegate type, if any
            //

            lock (_DelegateTypes) {
                found = _DelegateTypes.TryGetValue(types, out type);
            }

            if (!found && type != null) {
                return type;
            }

            //
            // Create new delegate type
            //

            type = MakeNewCustomDelegate(types);

            //
            // LOCK to insert new delegate into the cache. If we already have one (racing threads), use the one from the cache
            //

            lock (_DelegateTypes) {
                Type conflict;
                if (_DelegateTypes.TryGetValue(types, out conflict) && conflict != null) {
                    type = conflict;
                } else {
                    _DelegateTypes[types] = type;
                }
            }

            return type;
        }

        private const MethodAttributes CtorAttributes = MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public;
        private const MethodImplAttributes ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed;
        private const MethodAttributes InvokeAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

        private static readonly Type[] _DelegateCtorSignature = new Type[] { typeof(object), typeof(IntPtr) };

        private static Type MakeNewCustomDelegate(Type[] types) {
            Type returnType = types[types.Length - 1];
            Type[] parameters = types.RemoveLast();

            TypeBuilder builder = Snippets.Shared.DefineDelegateType("Delegate" + types.Length);
            builder.DefineConstructor(CtorAttributes, CallingConventions.Standard, _DelegateCtorSignature).SetImplementationFlags(ImplAttributes);
            builder.DefineMethod("Invoke", InvokeAttributes, returnType, parameters).SetImplementationFlags(ImplAttributes);
            return builder.CreateType();
        }
    }
}
