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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using System.Dynamic;
using Microsoft.Scripting.Math;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Generation;
using IronRuby.Compiler.Generation;

namespace IronRuby.Runtime {

    public abstract class RubyCallSiteStorage {
        private readonly RubyContext/*!*/ _context;

        public RubyContext/*!*/ Context {
            get { return _context; } 
        }

        protected RubyCallSiteStorage(RubyContext/*!*/ context) {
            Assert.NotNull(context);
            _context = context;
        }
    }

    public class CallSiteStorage<TCallSiteFunc> : RubyCallSiteStorage where TCallSiteFunc : class {
        protected CallSite<TCallSiteFunc> Site;

        [Emitted]
        public CallSiteStorage(RubyContext/*!*/ context) : base(context) { }

        public CallSite<TCallSiteFunc>/*!*/ GetCallSite(string/*!*/ methodName, int argumentCount) {
            return RubyUtils.GetCallSite(ref Site, Context, methodName, argumentCount);
        }

        public CallSite<TCallSiteFunc>/*!*/ GetCallSite(string/*!*/ methodName, RubyCallSignature signature) {
            return RubyUtils.GetCallSite(ref Site, Context, methodName, signature);
        }
    }

    public sealed class BinaryOpStorage : CallSiteStorage<Func<CallSite, object, object, object>> {
        [Emitted]
        public BinaryOpStorage(RubyContext/*!*/ context) : base(context) { }

        public CallSite<Func<CallSite, object, object, object>>/*!*/ GetCallSite(string/*!*/ methodName) {
            return GetCallSite(methodName, 1);
        }
    }

    public sealed class UnaryOpStorage : CallSiteStorage<Func<CallSite, object, object>> {
        [Emitted]
        public UnaryOpStorage(RubyContext/*!*/ context) : base(context) { }

        public CallSite<Func<CallSite, object, object>>/*!*/ GetCallSite(string/*!*/ methodName) {
            return GetCallSite(methodName, 0);
        }
    }

    public sealed class RespondToStorage : CallSiteStorage<Func<CallSite, object, SymbolId, object>> {
        [Emitted]
        public RespondToStorage(RubyContext/*!*/ context) : base(context) { }

        public CallSite<Func<CallSite, object, SymbolId, object>>/*!*/ GetCallSite() {
            return GetCallSite("respond_to?", 1);
        }
    }

    public sealed class ConversionStorage<TResult> : CallSiteStorage<Func<CallSite, object, TResult>> {
        [Emitted]
        public ConversionStorage(RubyContext/*!*/ context) : base(context) { }

        public CallSite<Func<CallSite, object, TResult>>/*!*/ GetSite(RubyConversionAction/*!*/ conversion) {
            return RubyUtils.GetCallSite(ref Site, conversion);
        }
    }
}
