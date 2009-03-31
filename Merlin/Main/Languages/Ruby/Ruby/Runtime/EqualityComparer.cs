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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;

namespace IronRuby.Runtime {
    // Even though Ruby types overload Equals & GetHashCode, we can't use them
    // because monkeypatching allows for implementing "hash" and "eql?" on any type
    // (including instances of arbitrary .NET types via singleton methods)
    // TODO: optimize this by caching hash values?
    public class EqualityComparer : IEqualityComparer<object> {
        private readonly RubyContext/*!*/ _context;

        private readonly CallSite<Func<CallSite, object, object>>/*!*/ _hashSite;
        private readonly CallSite<Func<CallSite, object, object, bool>>/*!*/ _eqlSite;

        // friend: RubyContext
        internal EqualityComparer(RubyContext/*!*/ context) {
            Assert.NotNull(context);
            _context = context;
            _hashSite = CallSite<Func<CallSite, object, object>>.Create(
                RubyCallAction.Make(context, "hash", RubyCallSignature.WithImplicitSelf(0))
             );
            _eqlSite = CallSite<Func<CallSite, object, object, bool>>.Create(
                RubyCallAction.Make(context, "eql?", RubyCallSignature.WithImplicitSelf(1))
            );
        }

        bool IEqualityComparer<object>.Equals(object x, object y) {
            return x == y || _eqlSite.Target(_eqlSite, x, y);
        }

        int IEqualityComparer<object>.GetHashCode(object obj) {
            object result = _hashSite.Target(_hashSite, obj);
            if (result is int) {
                return (int)result;
            }
            return result.GetHashCode();
        }
    }
}
