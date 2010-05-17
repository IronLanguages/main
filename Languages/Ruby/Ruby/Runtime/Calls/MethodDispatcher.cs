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
    
    public abstract partial class MethodDispatcher {
        internal int Version;

        internal static MethodDispatcher CreateRubyObjectDispatcher(Type/*!*/ func, Delegate/*!*/ method, int mandatoryParamCount, 
            bool hasScope, bool hasBlock, int version) {

            var dispatcher = CreateDispatcher(func, mandatoryParamCount, hasScope, hasBlock, version,
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

        internal static MethodDispatcher CreateDispatcher(Type/*!*/ func, int mandatoryParamCount, bool hasScope, bool hasBlock, int version,
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

            if (parameterCount > MaxPrecompiledArity) {
                return null;
            }

            // self must be an object:
            if (funcArgs[selfIndex] != typeof(object)) {
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

        public abstract object/*!*/ CreateDelegate(bool isUntyped);
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

    public abstract class InterpretedDispatcher {
        internal object _rule;
        internal Delegate _compiled;

        internal T/*!*/ CreateDelegate<T>(Expression/*!*/ binding, int compilationThreshold) where T : class {
            Delegate d = Stitch<T>(binding).LightCompile(compilationThreshold);
            T result = (T)(object)d;

            LightLambda lambda = d.Target as LightLambda;
            if (lambda != null) {
                _rule = result;
                lambda.Compile += (_, e) => _compiled = e.Compiled;
                return (T)GetInterpretingDelegate();
            } else {
                PerfTrack.NoteEvent(PerfTrack.Categories.Rules, "Rule not interpreted");
                return result;
            }
        }

        // TODO: This is a copy of CallSiteBinder.Stitch.
        private LambdaExpression/*!*/ Stitch<T>(Expression/*!*/ binding) where T : class {
            Expression updLabel = Expression.Label(CallSiteBinder.UpdateLabel);

            var site = Expression.Parameter(typeof(CallSite), "$site");
            var @params = ArrayUtils.Insert(site, Parameters);

            var body = Expression.Block(
                binding,
                updLabel,
                Expression.Label(
                    ReturnLabel,
                    Expression.Invoke(
                        Expression.Property(
                            Ast.Convert(site, typeof(CallSite<T>)),
                            typeof(CallSite<T>).GetProperty("Update")
                        ),
                        @params
                    )
                )
            );

            return Expression.Lambda<T>(
                body,
                "CallSite.Target",
                true, // always compile the rules with tail call optimization
                @params
            );
        }

        internal abstract ReadOnlyCollection<ParameterExpression> Parameters { get; }
        internal abstract LabelTarget ReturnLabel { get; }
        internal abstract object/*!*/ GetInterpretingDelegate();
    }
}
