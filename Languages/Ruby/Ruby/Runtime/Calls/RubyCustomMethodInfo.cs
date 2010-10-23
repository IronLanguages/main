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

using System;
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls {
    internal sealed class RubyCustomMethodInfo : RubyMemberInfo {
        private readonly RuleGenerator/*!*/ _ruleGenerator;

        public RubyCustomMethodInfo(RuleGenerator/*!*/ ruleGenerator, RubyMemberFlags flags, RubyModule/*!*/ declaringModule)
            : base(flags, declaringModule) {

            Assert.NotNull(ruleGenerator, declaringModule);
            _ruleGenerator = ruleGenerator;
        }

        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            _ruleGenerator(metaBuilder, args, name);
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyCustomMethodInfo(_ruleGenerator, flags, module);
        }

        public override MemberInfo/*!*/[]/*!*/ GetMembers() {
            return new MemberInfo[] { _ruleGenerator.Method };
        }

        public override RubyMemberInfo TrySelectOverload(Type/*!*/[]/*!*/ parameterTypes) {
            return this;
        }
    }
}
