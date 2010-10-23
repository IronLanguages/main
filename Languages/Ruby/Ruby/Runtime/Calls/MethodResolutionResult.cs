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
using System.Text;
using IronRuby.Builtins;
using System.Diagnostics;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls {
    // Method doesn't exist -> info == null, Found == false
    // Method exists but is not visible -> info != null, visible == false, Found == false
    // Method exists and is visible -> info != null, visible == true, Found == true
    public struct MethodResolutionResult {
        public static readonly MethodResolutionResult NotFound = new MethodResolutionResult();

        private readonly RubyMemberInfo _info;
        private readonly RubyModule _owner;
        private readonly bool _visible;

        public RubyMemberInfo Info { get { return _info; } }
        public RubyModule Owner { get { return _owner; } }

        public bool Found {
            get { return _info != null && _visible; }
        }

        public RubyMethodVisibility IncompatibleVisibility {
            get { return (_info == null || _visible) ? RubyMethodVisibility.None : _info.Visibility; }
        }

        public MethodResolutionResult(RubyMemberInfo/*!*/ info, RubyModule/*!*/ owner, bool visible) {
            Assert.NotNull(info, owner);
            _info = info;
            _owner = owner;
            _visible = visible;
        }

        internal MethodResolutionResult InvalidateSitesOnOverride() {
            // Mark method as used in DS regardless of visibility. Even if the method cannot be invoked due to its visibility
            // we need to invalidate the cached rule whenever the method is overridden or removed.
            if (_info != null) {
                _info.SetInvalidateSitesOnOverride();
                _owner.OwnedMethodCachedInSite();
            }
            return this;
        }

        // Only call on method missing:
        internal MethodResolutionResult InvalidateSitesOnMissingMethodAddition(string/*!*/ methodName, RubyContext/*!*/ context) {
            // mark that methodName is used in method_missing dynamic site:
            if (context.MissingMethodsCachedInSites == null) {
                context.MissingMethodsCachedInSites = new HashSet<string>();
            }
            context.MissingMethodsCachedInSites.Add(methodName);
            return this;
        }
    }
}
