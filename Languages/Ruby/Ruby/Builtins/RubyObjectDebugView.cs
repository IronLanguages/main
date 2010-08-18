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

using IronRuby.Runtime;
using Microsoft.Scripting.Utils;
using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace IronRuby.Builtins {
    public class RubyObjectDebugView {
        private readonly IRubyObject/*!*/ _obj;

        public RubyObjectDebugView(IRubyObject/*!*/ obj) {
            Assert.NotNull(obj);
            _obj = obj;
        }

        [DebuggerDisplay("{GetModuleName(A),nq}", Name = "{GetClassKind(),nq}", Type = "")]
        public object A {
            get { return _obj.ImmediateClass; }
        }

        [DebuggerDisplay("{B}", Name = "tainted?", Type = "")]
        public bool B {
            get { return _obj.IsTainted; }
            set { _obj.IsTainted = value; }
        }

        [DebuggerDisplay("{C}", Name = "untrusted?", Type = "")]
        public bool C {
            get { return _obj.IsUntrusted; }
            set { _obj.IsUntrusted = value; }
        }

        [DebuggerDisplay("{D}", Name = "frozen?", Type = "")]
        public bool D {
            get { return _obj.IsFrozen; }
            set { if (value) { _obj.Freeze(); } }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object/*!*/ E {
            get {
                var instanceData = _obj.TryGetInstanceData();
                if (instanceData == null) {
                    return new RubyInstanceData.VariableDebugView[0];
                }

                return instanceData.GetInstanceVariablesDebugView(_obj.ImmediateClass.Context);
            }
        }

        private string GetClassKind() {
            return _obj.ImmediateClass.IsSingletonClass ? "singleton class" : "class";
        }

        private static string GetModuleName(object module) {
            var m = (RubyModule)module;
            return m != null ? m.GetDisplayName(m.Context, false).ToString() : null;
        }
    }
}
