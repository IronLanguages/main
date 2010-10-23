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

using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;

using System.Dynamic;
using System.Collections.Generic;

namespace IronRuby.Builtins {
    using Ast = Expression;

    public partial class RubyEvent {
        private readonly object/*!*/ _target;
        private readonly string/*!*/ _name;
        private readonly RubyEventInfo/*!*/ _info;

        public object Target {
            get { return _target; }
        }

        public RubyEventInfo/*!*/ Info {
            get { return _info; }
        }

        public string/*!*/ Name {
            get { return _name; }
        }

        public RubyEvent(object/*!*/ target, RubyEventInfo/*!*/ info, string/*!*/ name) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNull(info, "info");
            ContractUtils.RequiresNotNull(name, "name");

            _target = target;
            _info = info;
            _name = name;
        }

        public void Add(object handler) {
            _info.Tracker.AddHandler(_target, handler, _info.DeclaringModule.Context.DelegateCreator);
        }

        public void Remove(object handler) {
            _info.Tracker.RemoveHandler(_target, handler, _info.DeclaringModule.Context.EqualityComparer);
        }
    }
}
