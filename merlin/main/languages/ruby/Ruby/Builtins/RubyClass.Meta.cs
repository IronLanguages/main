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
using System.Dynamic.Binders;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Builtins {

    public partial class RubyClass {
        public override MetaObject/*!*/ GetMetaObject(Expression/*!*/ parameter) {
            return new Meta(parameter, Restrictions.Empty, this);
        }

        internal new sealed class Meta : RubyModule.Meta {
            public Meta(Expression/*!*/ expression, Restrictions/*!*/ restrictions, RubyClass/*!*/ value)
                : base(expression, restrictions, value) {
                ContractUtils.RequiresNotNull(value, "value");
            }

            public override MetaObject/*!*/ BindInvokeMember(InvokeMemberBinder/*!*/ binder, params MetaObject/*!*/[]/*!*/ args) {
                var self = (RubyClass)Value;
                return RubyInvokeMemberBinder.TryBind(self.Context, binder, this, args) ?? binder.FallbackInvokeMember(this, args);
            }

            public override MetaObject/*!*/ BindGetMember(GetMemberBinder/*!*/ binder) {
                var self = (RubyClass)Value;
                return RubyGetMemberBinder.TryBind(self.Context, binder, this) ?? binder.FallbackGetMember(this);
            }

            public override IEnumerable<string> GetDynamicMemberNames() {
                var self = (RubyClass)Value;
                var names = new List<string>();
                self.SingletonClass.ForEachMember(true, RubyMethodAttributes.DefaultVisibility, delegate(string/*!*/ name, RubyMemberInfo/*!*/ member) {
                    names.Add(name);
                });
                return names;
            }
        }
    }
}
