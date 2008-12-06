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
using System.Dynamic;
using System.Dynamic.Utils;
using System.Runtime.CompilerServices;

namespace System.Dynamic {
    internal class DispCallableMetaObject : MetaObject {
        private readonly DispCallable _callable;

        internal DispCallableMetaObject(Expression expression, DispCallable callable)
            : base(expression, Restrictions.Empty, callable) {
            _callable = callable;
        }

        public override MetaObject BindGetIndex(GetIndexBinder binder, MetaObject[] indexes) {
            return BindGetOrInvoke(binder, indexes, binder.Arguments) ??
                base.BindGetIndex(binder, indexes);
        }

        public override MetaObject BindInvoke(InvokeBinder binder, MetaObject[] args) {
            return BindGetOrInvoke(binder, args, binder.Arguments) ??
                base.BindInvoke(binder, args);
        }

        private MetaObject BindGetOrInvoke(CallSiteBinder binder, MetaObject[] args, IList<ArgumentInfo> argInfos) {
            if (args.Any(arg => ComBinderHelpers.IsStrongBoxArg(arg))) {
                return ComBinderHelpers.RewriteStrongBoxAsRef(binder, this, args, false);
            }

            ComMethodDesc method;
            var target = _callable.DispatchComObject;
            var name = _callable.MemberName;
            
            if (target.TryGetMemberMethod(name, out method) ||
                target.TryGetMemberMethodExplicit(name, out method)) {

                return BindComInvoke(method, args, argInfos);
            }
            return null;
        }


        public override MetaObject BindSetIndex(SetIndexBinder binder, MetaObject[] indexes, MetaObject value) {
            if (indexes.Any(arg => ComBinderHelpers.IsStrongBoxArg(arg))) {
                return ComBinderHelpers.RewriteStrongBoxAsRef(binder, this, indexes.AddLast(value), true);
            }

            ComMethodDesc method;
            var target = _callable.DispatchComObject;
            var name = _callable.MemberName;

            if (target.TryGetPropertySetter(name, out method, value.LimitType) ||
                target.TryGetPropertySetterExplicit(name, out method, value.LimitType)) {

                return BindComInvoke(method, indexes.AddLast(value), binder.Arguments);
            }

            return base.BindSetIndex(binder, indexes, value);
        }

        private MetaObject BindComInvoke(ComMethodDesc method, MetaObject[] indexes, IList<ArgumentInfo> argInfos) {
            var callable = Expression;
            var dispCall = Helpers.Convert(callable, typeof(DispCallable));

            return new ComInvokeBinder(
                argInfos,
                indexes,
                DispCallableRestrictions(),
                Expression.Constant(method),
                Expression.Property(
                    dispCall,
                    typeof(DispCallable).GetProperty("DispatchObject")
                ),
                method
            ).Invoke();
        }

        private Restrictions DispCallableRestrictions() {
            var callable = Expression;

            var callableTypeRestrictions = Restrictions.GetTypeRestriction(callable, typeof(DispCallable));
            var dispCall = Helpers.Convert(callable, typeof(DispCallable));
            var dispatch = Expression.Property(dispCall, typeof(DispCallable).GetProperty("DispatchComObject"));
            var dispId = Expression.Property(dispCall, typeof(DispCallable).GetProperty("DispId"));

            var dispatchRestriction = IDispatchMetaObject.IDispatchRestriction(dispatch, _callable.DispatchComObject.ComTypeDesc);
            var memberRestriction = Restrictions.GetExpressionRestriction(
                Expression.Equal(dispId, Expression.Constant(_callable.DispId))
            );

            return callableTypeRestrictions.Merge(dispatchRestriction).Merge(memberRestriction);
        }
    }
}

#endif
