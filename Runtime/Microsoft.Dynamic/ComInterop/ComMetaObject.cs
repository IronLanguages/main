/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !SILVERLIGHT
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Dynamic;
using System.Collections.Generic;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.ComInterop {

    // Note: we only need to support the operations used by ComBinder
    internal class ComMetaObject : DynamicMetaObject {
        internal ComMetaObject(Expression expression, BindingRestrictions restrictions, object arg)
            : base(expression, restrictions, arg) {
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(args.AddFirst(WrapSelf()));
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(args.AddFirst(WrapSelf()));
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf());
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf(), value);
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf(), indexes);
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.Defer(WrapSelf(), indexes.AddLast(value));
        }

        private DynamicMetaObject WrapSelf() {
            return new DynamicMetaObject(
                ComObject.RcwToComObject(Expression),
                BindingRestrictions.GetExpressionRestriction(
                    Expression.Call(
                        typeof(ComObject).GetMethod("IsComObject", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic),
                        Helpers.Convert(Expression, typeof(object))
                    )
                )
            );
        }
    }
}

#endif
