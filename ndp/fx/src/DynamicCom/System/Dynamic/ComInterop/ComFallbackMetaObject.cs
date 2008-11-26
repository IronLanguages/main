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
using System.Dynamic.Binders;
using System.Dynamic.Utils;
using System.Diagnostics;

namespace System.Dynamic.ComInterop {
    //
    // ComFallbackMetaObject just delegates everything to the binder.
    //
    // Note that before performing FallBack on a ComObject we need to unwrap it so that
    // binder would act upon the actual object (typically Rcw)
    //
    // Also: we don't need to implement these for any operations other than those
    // supported by ComBinder
    internal class ComFallbackMetaObject : MetaObject {
        internal ComFallbackMetaObject(Expression expression, Restrictions restrictions, object arg)
            : base(expression, restrictions, arg) {
        }

        public override MetaObject BindGetIndex(GetIndexBinder binder, MetaObject[] indexes) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackGetIndex(UnwrapSelf(), indexes);
        }

        public override MetaObject BindSetIndex(SetIndexBinder binder, MetaObject[] indexes, MetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackSetIndex(UnwrapSelf(), indexes, value);
        }

        public override MetaObject BindGetMember(GetMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackGetMember(UnwrapSelf());
        }

        public override MetaObject BindInvokeMember(InvokeMemberBinder binder, MetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackInvokeMember(UnwrapSelf(), args);
        }

        public override MetaObject BindSetMember(SetMemberBinder binder, MetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackSetMember(UnwrapSelf(), value);
        }

        protected virtual ComUnwrappedMetaObject UnwrapSelf() {
            return new ComUnwrappedMetaObject(
                ComObject.RcwFromComObject(Expression),
                Restrictions.Merge(Restrictions.GetTypeRestriction(Expression, LimitType)),
                ((ComObject)Value).RuntimeCallableWrapper
            );
        }
    }

    // This type exists as a signal type, so ComBinder knows not to try to bind
    // again when we're trying to fall back
    internal sealed class ComUnwrappedMetaObject : MetaObject {
        internal ComUnwrappedMetaObject(Expression expression, Restrictions restrictions, object value)
            : base(expression, restrictions, value) {
        }
    }
}

#endif
