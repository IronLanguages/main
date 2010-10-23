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
        private readonly CallSite<Func<CallSite, object, object>>/*!*/ _hashSite;
        private readonly CallSite<Func<CallSite, object, object, object>>/*!*/ _eqlSite;

        // friend: RubyContext
        internal EqualityComparer(RubyContext/*!*/ context)
            : this(
                CallSite<Func<CallSite, object, object>>.Create(RubyCallAction.Make(context, "hash", RubyCallSignature.WithImplicitSelf(0))),
                CallSite<Func<CallSite, object, object, object>>.Create(RubyCallAction.Make(context, "eql?", RubyCallSignature.WithImplicitSelf(1)))
            ) {
        }

        public EqualityComparer(UnaryOpStorage/*!*/ hashStorage, BinaryOpStorage/*!*/ eqlStorage) 
            : this(hashStorage.GetCallSite("hash"), eqlStorage.GetCallSite("eql?")) {
        }

        public EqualityComparer(CallSite<Func<CallSite, object, object>>/*!*/ hashSite, CallSite<Func<CallSite, object, object, object>>/*!*/ eqlSite) {
            ContractUtils.RequiresNotNull(hashSite, "hashSite");
            ContractUtils.RequiresNotNull(eqlSite, "eqlSite");
            _hashSite = hashSite;
            _eqlSite = eqlSite;
        }

        bool IEqualityComparer<object>.Equals(object x, object y) {
            if (x == y) {
                return true;
            }

            if (x is int) {
                return y is int && (int)x == (int)y;
            }

            return RubyOps.IsTrue(_eqlSite.Target(_eqlSite, x, y));
        }

        int IEqualityComparer<object>.GetHashCode(object obj) {
            if (obj is int) {
                return (int)obj;
            }

            return Protocols.ToHashCode(_hashSite.Target(_hashSite, obj));
        }
    }
}
