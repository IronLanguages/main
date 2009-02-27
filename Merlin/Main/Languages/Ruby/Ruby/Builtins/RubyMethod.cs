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

using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;
using Ast = System.Linq.Expressions.Expression;
using System.Dynamic;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Builtins {
    
    public partial class RubyMethod {
        private readonly object _target;
        private readonly string/*!*/ _name;
        private readonly RubyMemberInfo/*!*/ _info;

        public object Target {
            get { return _target; }
        }

        public RubyMemberInfo/*!*/ Info {
            get { return _info; }
        }

        public string/*!*/ Name {
            get { return _name; } 
        }

        public RubyMethod(object target, RubyMemberInfo/*!*/ info, string/*!*/ name) {
            ContractUtils.RequiresNotNull(info, "info");
            ContractUtils.RequiresNotNull(name, "name");
                        
            _target = target;
            _info = info;
            _name = name;
        }

        public RubyClass/*!*/ GetTargetClass() {
            return _info.Context.GetClassOf(_target);
        }

        #region Dynamic Operations

        internal void SetRuleForCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            Assert.NotNull(metaBuilder, args);
            Debug.Assert(args.Target == this);

            // TODO: we could compare infos here:
            // first argument must be this method:
            metaBuilder.AddRestriction(Ast.Equal(args.TargetExpression, AstUtils.Constant(this)));

            // set the target (becomes self in the called method):
            args.SetTarget(AstUtils.Constant(_target), _target);

            _info.BuildCall(metaBuilder, args, _name);
        }

        #endregion
    }
}
