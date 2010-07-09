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
using System.Reflection;
using IronRuby.Compiler;

namespace IronRuby.Builtins {

    public partial class RubyObject : IRubyDynamicMetaObjectProvider {
        public virtual DynamicMetaObject/*!*/ GetMetaObject(Expression/*!*/ parameter) {
            return new Meta(parameter, BindingRestrictions.Empty, this);
        }

        internal class Meta : RubyMetaObject<IRubyObject> {
            public override RubyContext/*!*/ Context {
                get { return Value.ImmediateClass.Context; }
            }

            protected override MethodInfo/*!*/ ContextConverter {
                get { return Methods.GetContextFromIRubyObject; }
            }

            public Meta(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, IRubyObject/*!*/ value)
                : base(expression, restrictions, value) {
            }

            public override IEnumerable<string>/*!*/ GetDynamicMemberNames() {
                var self = Value;
                var names = new List<string>();
                var cls = self.ImmediateClass;

                using (cls.Context.ClassHierarchyLocker()) {
                    cls.ForEachMember(true, RubyMethodAttributes.DefaultVisibility, (name, module, member) => names.Add(name));
                }

                return names;
            }
        }
    }
}
