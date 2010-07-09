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

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using IronRuby.Builtins;
using IronRuby.Compiler;
using System.Diagnostics;
using System;

namespace IronRuby.Runtime.Calls {

    public delegate void RuleGenerator(MetaObjectBuilder/*!*/ result, CallArguments/*!*/ args, string/*!*/ name);

    public static class RuleGenerators {
        public static void InstanceConstructor(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            ((RubyClass)args.Target).BuildObjectConstruction(metaBuilder, args, name);
        }

        public static void InstanceAllocator(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            ((RubyClass)args.Target).BuildObjectAllocation(metaBuilder, args, name);
        }

        public static void MethodCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            ((RubyMethod)args.Target).BuildInvoke(metaBuilder, args);
        }
    }
}
