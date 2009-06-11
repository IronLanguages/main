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
using System.Text;
using IronRuby.Builtins;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls {
    
    public abstract partial class MethodDispatcher {
        internal int Version;

        internal static MethodDispatcher CreateRubyObjectDispatcher(Type/*!*/ func, Delegate/*!*/ method, int mandatoryParamCount, 
            bool hasScope, int version) {

            var dispatcher = CreateDispatcher(func, mandatoryParamCount, hasScope, version,
                () => hasScope ? (MethodDispatcher)new RubyObjectMethodDispatcherWithScope() : new RubyObjectMethodDispatcher(),
                hasScope ? RubyObjectMethodDispatchersWithScope : RubyObjectMethodDispatchers
            );

            if (dispatcher != null) {
                dispatcher.Initialize(method, version);
            }
            return dispatcher;
        }

        internal static MethodDispatcher CreateDispatcher(Type/*!*/ func, int mandatoryParamCount, bool hasScope, int version,
            Func<MethodDispatcher> parameterlessFactory, Type[] genericFactories) {
            Type[] funcArgs = func.GetGenericArguments();

            // Func<CallSite, (RubyScope)?, TSelf, T1, ... TN, object>
            int firstParameterIndex = hasScope ? 3 : 2;
            int parameterCount = funcArgs.Length - firstParameterIndex - 1;

            // invalid number of arguments passed to the site:
            if (parameterCount != mandatoryParamCount) {
                return null;
            }

            if (parameterCount > PrecompiledParameterCount) {
                return null;
            }

            // self must be an object:
            if (funcArgs[firstParameterIndex - 1] != typeof(object)) {
                return null;
            }

            if (parameterCount == 0) {
                return parameterlessFactory();
            } else {
                // TODO: cache?

                // remove "self":
                var types = funcArgs.GetSlice(firstParameterIndex, parameterCount);
                return (MethodDispatcher)Activator.CreateInstance(genericFactories[parameterCount - 1].MakeGenericType(types));
            }
        }

        public abstract object/*!*/ CreateDelegate();
        internal abstract void Initialize(Delegate/*!*/ method, int version);

        internal virtual void InitializeSingleton(object singleton, VersionHandle/*!*/ versionHandle) {
            throw Assert.Unreachable;
        }
    }

    public abstract class MethodDispatcher<TRubyFunc> : MethodDispatcher {
        internal TRubyFunc/*!*/ Method;

        internal override void Initialize(Delegate/*!*/ method, int version) {
            Assert.NotNull(method);
            Method = (TRubyFunc)(object)method;
            Version = version;
        }
    }

    public abstract class SingletonMethodDispatcherBase<TRubyFunc> : MethodDispatcher<TRubyFunc> {
        internal object Singleton;
        internal VersionHandle/*!*/ VersionHandle;

        internal override void InitializeSingleton(object singleton, VersionHandle/*!*/ versionHandle) {
            Assert.NotNull(versionHandle);
            Singleton = singleton;
            VersionHandle = versionHandle;
        }
    }
}
