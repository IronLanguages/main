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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using IronRuby.Builtins;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;

    public abstract partial class MemberDispatcher {
        internal int Version;

        public abstract object/*!*/ CreateDelegate(bool isUntyped);

        internal static object CreateDispatcher(Type/*!*/ func, int mandatoryParamCount, bool hasScope, bool hasBlock, int version,
            Func<MethodDispatcher> parameterlessFactory, Type[] genericFactories) {
            Type[] funcArgs = func.GetGenericArguments();

            // Func<CallSite, (RubyScope)?, TSelf, (Proc)?, T1, ... TN, object>
            int selfIndex = 1 + (hasScope ? 1 : 0);
            int firstParameterIndex = selfIndex + 1 + (hasBlock ? 1 : 0);
            int parameterCount = funcArgs.Length - firstParameterIndex - 1;

            // invalid number of arguments passed to the site:
            if (parameterCount != mandatoryParamCount) {
                return null;
            }

            if (parameterCount > MethodDispatcher.MaxPrecompiledArity) {
                return null;
            }

            // self must be an object:
            if (funcArgs[selfIndex] != typeof(object)) {
                return null;
            }

            if (parameterCount == 0) {
                return parameterlessFactory();
            }

            // TODO: cache?
            // remove "self":
            var types = funcArgs.GetSlice(firstParameterIndex, parameterCount);
            return Activator.CreateInstance(genericFactories[parameterCount - 1].MakeGenericType(types));
        }
    }

    public abstract class MethodDispatcher : MemberDispatcher {
        internal static MethodDispatcher CreateRubyObjectDispatcher(Type/*!*/ func, Delegate/*!*/ method, int mandatoryParamCount, 
            bool hasScope, bool hasBlock, int version) {

            var dispatcher = (MethodDispatcher)CreateDispatcher(func, mandatoryParamCount, hasScope, hasBlock, version,
                () => 
                hasScope ?
                    (hasBlock ? (MethodDispatcher)new RubyObjectMethodDispatcherWithScopeAndBlock() : new RubyObjectMethodDispatcherWithScope()) :
                    (hasBlock ? (MethodDispatcher)new RubyObjectMethodDispatcherWithBlock() : new RubyObjectMethodDispatcher()),
                hasScope ? 
                    (hasBlock ? RubyObjectMethodDispatchersWithScopeAndBlock : RubyObjectMethodDispatchersWithScope) :
                    (hasBlock ? RubyObjectMethodDispatchersWithBlock : RubyObjectMethodDispatchers)
            );

            if (dispatcher != null) {
                dispatcher.Initialize(method, version);
            }
            return dispatcher;
        }

        internal abstract void Initialize(Delegate/*!*/ method, int version);
    }

    public abstract class MethodDispatcher<TRubyFunc> : MethodDispatcher {
        internal TRubyFunc Method;

        internal override void Initialize(Delegate/*!*/ method, int version) {
            Assert.NotNull(method);
            Method = (TRubyFunc)(object)method;
            Version = version;
        }
    }

    public abstract class AttributeDispatcher : MemberDispatcher {
        internal string/*!*/ Name;

        internal static AttributeDispatcher CreateRubyObjectWriterDispatcher(Type/*!*/ delegateType, string/*!*/ name, int version) {
            var dispatcher = (AttributeDispatcher)CreateDispatcher(delegateType, 1, true, false, version, null, RubyObjectAttributeWriterDispatchersWithScope);
            if (dispatcher != null) {
                dispatcher.Initialize(name, version);
            }
            return dispatcher;
        }

        internal abstract void Initialize(string/*!*/ name, int version);
    }

    public sealed class RubyObjectAttributeReaderDispatcherWithScope : AttributeDispatcher {
        internal override void Initialize(string/*!*/ name, int version) {
            Name = name;
            Version = version;
        }

        public override object/*!*/ CreateDelegate(bool isUntyped) {
            return isUntyped ?
                (object)new Func<CallSite, object, object, object>(Invoke<object>) :
                (object)new Func<CallSite, RubyScope, object, object>(Invoke<RubyScope>);
        }
        
        public object Invoke<TScope>(CallSite/*!*/ callSite, TScope/*!*/ scope, object self) {
            IRubyObject obj = self as IRubyObject;
            if (obj != null && obj.ImmediateClass.Version.Method == Version) {
                // TODO: optimize
                RubyInstanceData data = obj.TryGetInstanceData();
                return (data != null) ? data.GetInstanceVariable(Name) : null;
            } else {
                return ((CallSite<Func<CallSite, TScope, object, object>>)callSite).Update(callSite, scope, self);
            }
        }
    }

    public sealed class RubyObjectAttributeWriterDispatcherWithScope<T0> : AttributeDispatcher {
        internal override void Initialize(string/*!*/ name, int version) {
            Name = name;
            Version = version;
        }

        public override object/*!*/ CreateDelegate(bool isUntyped) {
            return isUntyped ?
                (object)new Func<CallSite, object, object, T0, object>(Invoke<object>) :
                (object)new Func<CallSite, RubyScope, object, T0, object>(Invoke<RubyScope>);
        }

        public object Invoke<TScope>(CallSite/*!*/ callSite, TScope/*!*/ scope, object self, T0 arg0) {
            IRubyObject obj = self as IRubyObject;
            if (obj != null && obj.ImmediateClass.Version.Method == Version) {
                var result = (object)arg0;
                // TODO: optimize
                obj.ImmediateClass.Context.SetInstanceVariable(obj, Name, result);
                return result;
            } else {
                return ((CallSite<Func<CallSite, TScope, object, T0, object>>)callSite).Update(callSite, scope, self, arg0);
            }
        }
    }
}
