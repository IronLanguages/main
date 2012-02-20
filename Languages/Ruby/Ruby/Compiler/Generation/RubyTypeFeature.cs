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
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using System.Diagnostics;

namespace IronRuby.Compiler.Generation {
    internal sealed class RubyTypeFeature : ITypeFeature {
        internal static readonly RubyTypeFeature/*!*/ Instance = new RubyTypeFeature();

        public bool CanInherit {
            get { return true; }
        }

        public bool IsImplementedBy(Type/*!*/ type) {
            return typeof(IRubyObject).IsAssignableFrom(type);
        }

        public override int GetHashCode() {
            return typeof(RubyTypeFeature).GetHashCode();
        }

        public override bool Equals(object obj) {
            return Object.ReferenceEquals(obj, Instance);
        }

#if FEATURE_REFEMIT
        public IFeatureBuilder/*!*/ MakeBuilder(TypeBuilder/*!*/ tb) {
            return new RubyTypeBuilder(tb);
        }
#endif
    }
}
