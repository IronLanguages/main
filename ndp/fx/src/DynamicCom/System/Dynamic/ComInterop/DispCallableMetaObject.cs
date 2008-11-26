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

#if !SILVERLIGHT

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Dynamic.Binders;
using System.Dynamic.Utils;

namespace System.Dynamic.ComInterop {
    internal class DispCallableMetaObject : MetaObject {
        private readonly DispCallable _callable;

        internal DispCallableMetaObject(Expression expression, DispCallable callable)
            : base(expression, Restrictions.Empty, callable) {
            _callable = callable;
        }

        public override MetaObject BindGetIndex(GetIndexBinder binder, MetaObject[] indexes) {
            if (_callable.ComMethodDesc.IsPropertyGet) {
                if (indexes.Any(arg => ComBinderHelpers.IsStrongBoxArg(arg))) {
                    return ComBinderHelpers.RewriteStrongBoxAsRef(binder, this, indexes);
                }
                return BindComInvoke(binder.Arguments, indexes);
            }
            return base.BindGetIndex(binder, indexes);
        }

        public override MetaObject BindSetIndex(SetIndexBinder binder, MetaObject[] indexes, MetaObject value) {
            if (_callable.ComMethodDesc.IsPropertyPut) {
                if (indexes.Any(arg => ComBinderHelpers.IsStrongBoxArg(arg))) {
                    return ComBinderHelpers.RewriteStrongBoxAsRef(binder, this, indexes.AddLast(value));
                }
                return BindComInvoke(binder.Arguments, indexes.AddLast(value));
            }
            return base.BindSetIndex(binder, indexes, value);
        }

        public override MetaObject BindInvoke(InvokeBinder binder, MetaObject[] args) {
            if (args.Any(arg => ComBinderHelpers.IsStrongBoxArg(arg))) {
                return ComBinderHelpers.RewriteStrongBoxAsRef(binder, this, args);
            }
            return BindComInvoke(binder.Arguments, args);
        }

        private MetaObject BindComInvoke(IList<ArgumentInfo> argInfo, MetaObject[] args) {
            var callable = Expression;
            var dispCall = Expression.Convert(callable, typeof(DispCallable));
            var methodDesc = Expression.Property(dispCall, typeof(DispCallable).GetProperty("ComMethodDesc"));
            var methodRestriction = Expression.Equal(methodDesc, Expression.Constant(_callable.ComMethodDesc));

            return new ComInvokeBinder(
                argInfo,
                args,
                Restrictions.GetTypeRestriction(callable, Value.GetType()).Merge(
                    Restrictions.GetExpressionRestriction(methodRestriction)
                ),
                methodDesc,
                Expression.Property(dispCall, typeof(DispCallable).GetProperty("DispatchObject")),
                _callable.ComMethodDesc
            ).Invoke();
        }
    }
}

#endif
