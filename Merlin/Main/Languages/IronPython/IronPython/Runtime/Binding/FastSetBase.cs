/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Runtime.CompilerServices;


using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding {
    class FastSetBase {
        internal Delegate _func;
        internal Delegate _optimizedDelegate;
        internal int _version;
        internal int _hitCount;

        public FastSetBase(int version) {
            _version = version;
        }

        public bool ShouldUseNonOptimizedSite {
            get {
                return _hitCount < 100;
            }
        }
    }

    /// <summary>
    /// Base class for all of our fast set delegates.  This holds onto the
    /// delegate and provides the Update and Optimize functions.
    /// </summary>
    class FastSetBase<TValue> : FastSetBase {
        private readonly PythonSetMemberBinder/*!*/ _binder;

        public FastSetBase(PythonSetMemberBinder/*!*/ binder, int version) : base(version) {
            Assert.NotNull(binder);

            _binder = binder;
        }

        protected PythonSetMemberBinder Binder {
            get {
                return _binder;
            }
        }

        /// <summary>
        /// Updates the call site when the current rule is no longer applicable.
        /// </summary>
        protected static object Update(CallSite site, object self, TValue value) {
            return ((CallSite<Func<CallSite, object, TValue, object>>)site).Update(site, self, value);
        }

        /// <summary>
        /// Replaces the pre-compiled call site target with an optimized call site delegate
        /// which is always compiled into a DynamicMethod.
        /// </summary>
        public Func<CallSite, object, TValue, object> Optimize(CallSite<Func<CallSite, object, TValue, object>> site, object self, TValue value) {
            if (_optimizedDelegate == null) {
                _optimizedDelegate = _binder.OptimizeDelegate(site, self, value);
            }

            return (Func<CallSite, object, TValue, object>)_optimizedDelegate;
        }
    }
}