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

using System.Linq.Expressions;
using System.Dynamic;
using System.Dynamic.Utils;
using System.Collections.Generic;

namespace System.Dynamic {

    // Note: we only need to support the operations used by ComBinder
    internal class ComMetaObject : MetaObject {
        internal ComMetaObject(Expression expression, Restrictions restrictions, object arg)
            : base(expression, restrictions, arg) {
        }

        public override MetaObject BindInvokeMember(InvokeMemberBinder binder, MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(args.AddFirst(WrapSelf()));
        }

        public override MetaObject BindInvoke(InvokeBinder binder, MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(args.AddFirst(WrapSelf()));
        }

        public override MetaObject BindGetMember(GetMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf());
        }

        public override MetaObject BindSetMember(SetMemberBinder binder, MetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf(), value);
        }

        public override MetaObject BindGetIndex(GetIndexBinder binder, MetaObject[] indexes) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf(), indexes);
        }

        public override MetaObject BindSetIndex(SetIndexBinder binder, MetaObject[] indexes, MetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf(), indexes.AddLast(value));
        }

        private MetaObject WrapSelf() {
            return new MetaObject(
                ComObject.RcwToComObject(Expression),
                Restrictions.GetExpressionRestriction(
                    Expression.AndAlso(
                        Expression.NotEqual(
                            Helpers.Convert(Expression, typeof(object)),
                            Expression.Constant(null)
                        ),
                        Expression.Call(
                            typeof(System.Runtime.InteropServices.Marshal).GetMethod("IsComObject"),
                            Helpers.Convert(Expression, typeof(object))
                        )
                    )
                )
            );
        }
    }
}

#endif
