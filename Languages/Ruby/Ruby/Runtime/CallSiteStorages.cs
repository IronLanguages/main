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
using System.Collections;
using System.Runtime.CompilerServices;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;

namespace IronRuby.Runtime {
    using UnaryOp = Func<CallSite, object, object>;
    using BinaryOp = Func<CallSite, object, object, object>;

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
        public CallSite<TCallSiteFunc> Site;

        [Emitted]
        public CallSiteStorage(RubyContext/*!*/ context) : base(context) { }

        public CallSite<TCallSiteFunc>/*!*/ GetCallSite(string/*!*/ methodName, int argumentCount) {
            return RubyUtils.GetCallSite(ref Site, Context, methodName, argumentCount);
        }

        public CallSite<TCallSiteFunc>/*!*/ GetCallSite(string/*!*/ methodName, RubyCallSignature signature) {
            return RubyUtils.GetCallSite(ref Site, Context, methodName, signature);
        }
    }

    public sealed class BinaryOpStorage : CallSiteStorage<BinaryOp> {
        [Emitted]
        public BinaryOpStorage(RubyContext/*!*/ context) : base(context) { }

        public CallSite<BinaryOp>/*!*/ GetCallSite(string/*!*/ methodName) {
            return GetCallSite(methodName, 1);
        }
    }

    public sealed class UnaryOpStorage : CallSiteStorage<UnaryOp> {
        [Emitted]
        public UnaryOpStorage(RubyContext/*!*/ context) : base(context) { }

        public CallSite<UnaryOp>/*!*/ GetCallSite(string/*!*/ methodName) {
            return GetCallSite(methodName, 0);
        }
    }

    public sealed class RespondToStorage : CallSiteStorage<BinaryOp> {
        [Emitted]
        public RespondToStorage(RubyContext/*!*/ context) : base(context) { }

        public CallSite<BinaryOp>/*!*/ GetCallSite() {
            return GetCallSite("respond_to?", 1);
        }
    }

    public sealed class ConversionStorage<TResult> : CallSiteStorage<Func<CallSite, object, TResult>> {
        [Emitted]
        public ConversionStorage(RubyContext/*!*/ context) : base(context) { }

        public CallSite<Func<CallSite, object, TResult>>/*!*/ GetSite(RubyConversionAction/*!*/ conversion) {
            return RubyUtils.GetCallSite(ref Site, conversion);
        }

        internal CallSite<Func<CallSite, object, TResult>>/*!*/ GetDefaultConversionSite() {
            return RubyUtils.GetCallSite(ref Site, ProtocolConversionAction.GetConversionAction(Context, typeof(TResult), true));

        }
    }

    public class ComparisonStorage : RubyCallSiteStorage {
        private CallSite<BinaryOp> _compareSite;     // <=>
        private CallSite<BinaryOp> _lessThanSite;    // <
        private CallSite<BinaryOp> _greaterThanSite; // >

        [Emitted]
        public ComparisonStorage(RubyContext/*!*/ context) : base(context) { }

        public CallSite<BinaryOp>/*!*/ CompareSite {
            get { return RubyUtils.GetCallSite(ref _compareSite, Context, "<=>", 1); }
        }

        public CallSite<BinaryOp>/*!*/ LessThanSite {
            get { return RubyUtils.GetCallSite(ref _lessThanSite, Context, "<", 1); }
        }

        public CallSite<BinaryOp>/*!*/ GreaterThanSite {
            get { return RubyUtils.GetCallSite(ref _greaterThanSite, Context, ">", 1); }
        }
    }



    public class JoinConversionStorage : RubyCallSiteStorage {
        private CallSite<Func<CallSite, object, MutableString>> _tosSite;   // to_s
        private CallSite<Func<CallSite, object, MutableString>> _toStrSite; // try to_str
        private CallSite<Func<CallSite, object, IList>> _toArySite;         // try to_ary

        [Emitted]
        public JoinConversionStorage(RubyContext/*!*/ context) : base(context) { }

        public CallSite<Func<CallSite, object, MutableString>>/*!*/ ToStr {
            get {
                return _toStrSite ?? (_toStrSite = RubyUtils.GetCallSite(ref _toStrSite, TryConvertToStrAction.Make(Context)));
            }
        }

        public CallSite<Func<CallSite, object, MutableString>>/*!*/ ToS {
            get {
                return _tosSite ?? (_tosSite = RubyUtils.GetCallSite(ref _tosSite, ConvertToSAction.Make(Context)));
            }
        }

        public CallSite<Func<CallSite, object, IList>>/*!*/ ToAry {
            get {
                return _toArySite ?? (_toArySite = RubyUtils.GetCallSite(ref _toArySite, TryConvertToArrayAction.Make(Context)));
            }
        }
    }
}
