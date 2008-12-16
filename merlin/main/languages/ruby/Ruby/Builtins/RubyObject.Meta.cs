/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Builtins {

    public partial class RubyObject : IDynamicObject {
        public virtual DynamicMetaObject/*!*/ GetMetaObject(Expression/*!*/ parameter) {
            return new Meta(parameter, BindingRestrictions.Empty, this);
        }

        internal class Meta : DynamicMetaObject {
            public Meta(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, IRubyObject/*!*/ value)
                : base(expression, restrictions, value) {
                ContractUtils.RequiresNotNull(value, "value");
            }

            public override DynamicMetaObject/*!*/ BindGetMember(GetMemberBinder/*!*/ binder) {
                var self = (IRubyObject)Value;
                return RubyGetMemberBinder.TryBind(self.Class.Context, binder, this) ?? binder.FallbackGetMember(this);
            }

            public override DynamicMetaObject/*!*/ BindInvokeMember(InvokeMemberBinder/*!*/ binder, params DynamicMetaObject/*!*/[]/*!*/ args) {
                var self = (IRubyObject)Value;
                return RubyInvokeMemberBinder.TryBind(self.Class.Context, binder, this, args) ?? binder.FallbackInvokeMember(this, args);
            }

            public override IEnumerable<string> GetDynamicMemberNames() {
                var self = (IRubyObject)Value;
                RubyInstanceData instanceData = self.GetInstanceData();
                RubyModule theClass = (instanceData != null ? instanceData.ImmediateClass : null);
                theClass = theClass ?? self.Class;
                var names = new List<string>();
                theClass.ForEachMember(true, RubyMethodAttributes.DefaultVisibility, delegate(string/*!*/ name, RubyMemberInfo/*!*/ member) {
                    names.Add(name);
                });
                return names;
            }
        }
    }
}
