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

#if !SILVERLIGHT // ComObject

using System.Collections.Generic;
using System.Dynamic;
using System.Dynamic.Utils;
using System.Linq.Expressions;

namespace System.Dynamic.ComInterop {

    internal sealed class IDispatchMetaObject : ComFallbackMetaObject {
        private readonly IDispatchComObject _self;

        internal IDispatchMetaObject(Expression expression, IDispatchComObject self)
            : base(expression, Restrictions.Empty, self) {
            _self = self;
        }

        public override MetaObject BindInvokeMember(InvokeMemberBinder binder, MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");

            if (args.Any(arg => ComBinderHelpers.IsStrongBoxArg(arg))) {
                return ComBinderHelpers.RewriteStrongBoxAsRef(binder, this, args, false);
            }

            ComMethodDesc method;
            if (_self.TryGetMemberMethod(binder.Name, out method) ||
                _self.TryGetMemberMethodExplicit(binder.Name, out method)) {

                return BindComInvoke(args, method, binder.Arguments);
            }

            return base.BindInvokeMember(binder, args);
        }

        private MetaObject BindComInvoke(MetaObject[] args, ComMethodDesc method, IList<ArgumentInfo> arguments) {
            return new ComInvokeBinder(
                arguments,
                args,
                IDispatchRestriction(),
                Expression.Constant(method),
                Expression.Property(
                    Helpers.Convert(Expression, typeof(IDispatchComObject)),
                    typeof(IDispatchComObject).GetProperty("DispatchObject")
                ),
                method
            ).Invoke();
        }

        public override MetaObject BindGetMember(GetMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");

            ComMethodDesc method;
            ComEventDesc @event;

            // 1. Try methods
            if (_self.TryGetMemberMethod(binder.Name, out method)) {
                return BindGetMember(method);
            }

            // 2. Try events
            if (_self.TryGetMemberEvent(binder.Name, out @event)) {
                return BindEvent(@event);
            }

            // 3. Try methods explicitly by name
            if (_self.TryGetMemberMethodExplicit(binder.Name, out method)) {
                return BindGetMember(method);
            }

            // 4. Fallback
            return base.BindGetMember(binder);
        }

        private MetaObject BindGetMember(ComMethodDesc method) {
            if (method.IsDataMember) {
                if (method.Parameters.Length == 0) {
                    return BindComInvoke(MetaObject.EmptyMetaObjects, method,new ArgumentInfo[0]);
                }
            }

            return new MetaObject(
                Expression.Call(
                    typeof(ComRuntimeHelpers).GetMethod("CreateDispCallable"),
                    Helpers.Convert(Expression, typeof(IDispatchComObject)),
                    Expression.Constant(method)
                ),
                IDispatchRestriction()
            );
        }

        private MetaObject BindEvent(ComEventDesc @event) {
            // BoundDispEvent CreateComEvent(object rcw, Guid sourceIid, int dispid)
            Expression result =
                Expression.Call(
                    typeof(ComRuntimeHelpers).GetMethod("CreateComEvent"),
                    ComObject.RcwFromComObject(Expression),
                    Expression.Constant(@event.sourceIID),
                    Expression.Constant(@event.dispid)
                );

            return new MetaObject(
                result,
                IDispatchRestriction()
            );
        }

        public override MetaObject BindGetIndex(GetIndexBinder binder, MetaObject[] indexes) {           
            ContractUtils.RequiresNotNull(binder, "binder");
            if (indexes.Any(arg => ComBinderHelpers.IsStrongBoxArg(arg))) {
                return ComBinderHelpers.RewriteStrongBoxAsRef(binder, this, indexes, false);
            }

            ComMethodDesc getItem;
            if (_self.TryGetGetItem(out getItem)){
                return BindComInvoke(indexes, getItem, binder.Arguments);
            }

            return base.BindGetIndex(binder, indexes);
        }

        public override MetaObject BindSetIndex(SetIndexBinder binder, MetaObject[] indexes, MetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");

            if (indexes.Any(arg => ComBinderHelpers.IsStrongBoxArg(arg))) {
                return ComBinderHelpers.RewriteStrongBoxAsRef(binder, this, indexes.AddLast(value), true);
            }

            ComMethodDesc setItem;
            if (_self.TryGetSetItem(out setItem)) {
                return BindComInvoke(indexes.AddLast(value), setItem, binder.Arguments);
            }

            return base.BindSetIndex(binder, indexes, value);
        }
        
        public override MetaObject BindSetMember(SetMemberBinder binder, MetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");

            return
                // 1. Check for simple property put
                TryPropertyPut(binder, value) ??

                // 2. Check for event handler hookup where the put is dropped
                TryEventHandlerNoop(binder, value) ??

                // 3. Fallback
                base.BindSetMember(binder, value);
        }

        private MetaObject TryPropertyPut(SetMemberBinder binder, MetaObject value) {
            ComMethodDesc method;
            if (_self.TryGetPropertySetter(binder.Name, out method, value.LimitType) ||
                _self.TryGetPropertySetterExplicit(binder.Name, out method, value.LimitType)) {
                Restrictions restrictions = IDispatchRestriction();
                Expression dispatch =
                    Expression.Property(
                        Helpers.Convert(Expression, typeof(IDispatchComObject)),
                        typeof(IDispatchComObject).GetProperty("DispatchObject")
                    );

                return new ComInvokeBinder(
                    new ArgumentInfo[0],
                    new[] { value },
                    restrictions,
                    Expression.Constant(method),
                    dispatch,
                    method
                ).Invoke();
            }

            return null;
        }

        private MetaObject TryEventHandlerNoop(SetMemberBinder binder, MetaObject value) {
            ComEventDesc @event;
            if (_self.TryGetEventHandler(binder.Name, out @event) && value.LimitType == typeof(BoundDispEvent)) {
                // Drop the event property set.
                return new MetaObject(
                    Expression.Constant(null),
                    value.Restrictions.Merge(IDispatchRestriction()).Merge(Restrictions.GetTypeRestriction(value.Expression, typeof(BoundDispEvent)))
                );
            }

            return null;
        }

        private Restrictions IDispatchRestriction() {
            return IDispatchRestriction(Expression, _self.ComTypeDesc);
        }

        internal static Restrictions IDispatchRestriction(Expression expr, ComTypeDesc typeDesc) {
            return Restrictions.GetTypeRestriction(
                expr, typeof(IDispatchComObject)
            ).Merge(
                Restrictions.GetExpressionRestriction(
                    Expression.Equal(
                        Expression.Property(
                            Helpers.Convert(expr, typeof(IDispatchComObject)),
                            typeof(IDispatchComObject).GetProperty("ComTypeDesc")
                        ),
                        Expression.Constant(typeDesc)
                    )
                )
            );
        }

        protected override ComUnwrappedMetaObject UnwrapSelf() {
            return new ComUnwrappedMetaObject(
                ComObject.RcwFromComObject(Expression),
                IDispatchRestriction(),
                _self.RuntimeCallableWrapper
            );
        }
    }
}

#endif
