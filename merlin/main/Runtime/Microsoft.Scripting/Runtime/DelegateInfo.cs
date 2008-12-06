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
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Used as the value for the ScriptingRuntimeHelpers.GetDelegate method caching system
    /// </summary>
    internal sealed class DelegateInfo {
        private readonly MethodInfo _method;
        private readonly object[] _constants;
        private WeakDictionary<object, WeakReference> _constantMap = new WeakDictionary<object, WeakReference>();

        internal DelegateInfo(MethodInfo method, object[] constants) {
            Assert.NotNull(method, constants);

            _method = method;
            _constants = constants;
        }

        internal Delegate CreateDelegate(Type delegateType, object target) {
            Assert.NotNull(delegateType, target);

            // to enable:
            // function x() { }
            // someClass.someEvent += delegateType(x) 
            // someClass.someEvent -= delegateType(x) 
            //
            // we need to avoid re-creating the object array because they won't
            // be compare equal when removing the delegate if they're difference 
            // instances.  Therefore we use a weak hashtable to get back the
            // original object array.  The values also need to be weak to avoid
            // creating a circular reference from the constants target back to the
            // target.  This is fine because as long as the delegate is referenced
            // the object array will stay alive.  Once the delegate is gone it's not
            // wired up anywhere and -= will never be used again.

            object[] clone;            
            lock (_constantMap) {
                WeakReference cloneRef;

                if (!_constantMap.TryGetValue(target, out cloneRef) || 
                    (clone = (object[])cloneRef.Target) == null) {
                    _constantMap[target] = new WeakReference(clone = (object[])_constants.Clone());

                    Debug.Assert(clone[0] == DelegateSignatureInfo.TargetPlaceHolder);

                    clone[0] = target;
                }
            }

            return ReflectionUtils.CreateDelegate(_method, delegateType, clone);
        }
    }
}
