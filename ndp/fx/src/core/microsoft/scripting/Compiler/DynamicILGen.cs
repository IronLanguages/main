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

using System.Reflection;
using System.Reflection.Emit;
using System.Dynamic.Utils;

namespace System.Linq.Expressions.Compiler {

    internal abstract class DynamicILGen : ILGen {
        internal DynamicILGen(ILGenerator il)
            : base(il) {
        }

        internal T CreateDelegate<T>() {
            MethodInfo mi;
            return CreateDelegate<T>(out mi);
        }

        internal abstract T CreateDelegate<T>(out MethodInfo mi);

        internal abstract MethodInfo Finish();
    }

    internal class DynamicILGenMethod : DynamicILGen {
        private readonly DynamicMethod _dm;

        internal DynamicILGenMethod(DynamicMethod dm, ILGenerator il)
            : base(il) {
            _dm = dm;
        }

        internal override T CreateDelegate<T>(out MethodInfo mi) {
            ContractUtils.Requires(typeof(T).IsSubclassOf(typeof(Delegate)), "T");
            mi = _dm;
            return (T)(object)_dm.CreateDelegate(typeof(T), null);
        }

        internal override MethodInfo Finish() {
            return _dm;
        }
    }

    internal class DynamicILGenType : DynamicILGen {
        private readonly TypeBuilder _tb;
        private readonly MethodBuilder _mb;

        internal DynamicILGenType(TypeBuilder tb, MethodBuilder mb, ILGenerator il)
            : base(il) {
            _tb = tb;
            _mb = mb;
        }

        internal override T CreateDelegate<T>(out MethodInfo mi) {
            ContractUtils.Requires(typeof(T).IsSubclassOf(typeof(Delegate)), "T");
            mi = CreateMethod();
            return (T)(object)Delegate.CreateDelegate(typeof(T), mi);
        }

        private MethodInfo CreateMethod() {
            Type t = _tb.CreateType();
            return t.GetMethod(_mb.Name);
        }

        internal override MethodInfo Finish() {
            return CreateMethod();
        }
    }
}
