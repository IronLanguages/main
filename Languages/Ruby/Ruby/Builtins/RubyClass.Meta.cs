/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System.Collections.Generic;
using System.Dynamic;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Builtins {

    public partial class RubyClass {
        public override DynamicMetaObject/*!*/ GetMetaObject(Expression/*!*/ parameter) {
            return new Meta(parameter, BindingRestrictions.Empty, this);
        }

        internal new sealed class Meta : RubyModule.Meta {
            public Meta(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, RubyClass/*!*/ value)
                : base(expression, restrictions, value) {
                ContractUtils.RequiresNotNull(value, "value");
            }

            public override DynamicMetaObject/*!*/ BindCreateInstance(CreateInstanceBinder/*!*/ binder, DynamicMetaObject/*!*/[]/*!*/ args) {
                return InteropBinder.InvokeMember.Bind(CreateMetaContext(), binder, this, args, binder.FallbackCreateInstance);
            }

            public override IEnumerable<string>/*!*/ GetDynamicMemberNames() {
                var names = new List<string>();

                using (Context.ClassHierarchyLocker()) {
                    Value.ImmediateClass.ForEachMember(true, RubyMethodAttributes.DefaultVisibility, (name, module, member) => names.Add(name));
                }

                return names;
            }
        }
    }
}
