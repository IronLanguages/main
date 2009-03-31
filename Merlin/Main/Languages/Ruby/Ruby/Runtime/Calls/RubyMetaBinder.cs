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

using System.Dynamic;
using System.Diagnostics;

namespace IronRuby.Runtime.Calls {
    public abstract class RubyMetaBinder : DynamicMetaObjectBinder {
        /// <summary>
        /// Cross-runtime checks are emitted if the action is not bound to the context.
        /// </summary>
        private RubyContext _context;

        protected RubyMetaBinder(RubyContext context) {
            _context = context;
        }
        
        internal RubyContext Context { 
            get { return _context; }
            set {
                Debug.Assert(_context == null && value != null);
                _context = value; 
            }
        }
        
        public abstract RubyCallSignature Signature { get; }
        protected abstract void Build(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args);

        protected abstract DynamicMetaObject/*!*/ InteropBind(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args);

        public override DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ scopeOrContextOrTarget, DynamicMetaObject/*!*/[]/*!*/ args) {
            var callArgs = new CallArguments(_context, scopeOrContextOrTarget, args, Signature);
            var metaBuilder = new MetaObjectBuilder(this, args);

            // TODO: COM interop
            if (IsForeignMetaObject(callArgs.MetaTarget)) {
                return InteropBind(metaBuilder, callArgs);
            }

            Build(metaBuilder, callArgs);
            return metaBuilder.CreateMetaObject(this);
        }

        internal static bool IsForeignMetaObject(DynamicMetaObject/*!*/ metaObject) {
            return metaObject.Value is IDynamicMetaObjectProvider && !(metaObject is RubyMetaObject);
        }
    }
}
