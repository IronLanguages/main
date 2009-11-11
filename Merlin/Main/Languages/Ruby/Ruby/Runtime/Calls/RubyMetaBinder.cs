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

using System.Dynamic;
using System.Diagnostics;
using System;
using Microsoft.Scripting.Utils;
using System.Collections.Generic;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using System.Runtime.CompilerServices;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;

    public abstract class RubyMetaBinder : DynamicMetaObjectBinder, IExpressionSerializable {
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
                Debug.Assert(_context == null);
                _context = value; 
            }
        }
        
        public abstract RubyCallSignature Signature { get; }

        protected abstract bool Build(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, bool defaultFallback);
        public abstract Expression CreateExpression();

        public override T BindDelegate<T>(System.Runtime.CompilerServices.CallSite<T> site, object[] args) {
            RubyContext context = _context ?? ((Signature.HasScope) ? ((RubyScope)args[0]).RubyContext : (RubyContext)args[0]);

            if (context.Options.NoAdaptiveCompilation) {
                return base.BindDelegate<T>(site, args);
            }

            InterpretedDispatcher dispatcher = MethodDispatcher.CreateInterpreted(typeof(T), args.Length);
            if (dispatcher == null) {
                // call site has too many arguments:
                PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "Ruby: ! No dispatcher for " + Signature.ToString());
                return base.BindDelegate<T>(site, args);
            } else {
                Expression binding = Bind(args, dispatcher.Parameters, dispatcher.ReturnLabel);

                if (binding == null) {
                    throw new InvalidImplementationException("DynamicMetaObjectBinder.Bind must return non-null meta-object");
                }

                T result = dispatcher.CreateDelegate<T>(binding, context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }
        }

        public override DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ scopeOrContextOrTarget, DynamicMetaObject/*!*/[]/*!*/ args) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "Ruby: " + GetType().Name + Signature.ToString() + ": Bind");

            var callArgs = new CallArguments(_context, scopeOrContextOrTarget, args, Signature);
            var metaBuilder = new MetaObjectBuilder(this, args);

            if (IsForeignMetaObject(callArgs.MetaTarget)) {
                return InteropBind(metaBuilder, callArgs);
            }

            Build(metaBuilder, callArgs, true);
            return metaBuilder.CreateMetaObject(this);
        }

        protected virtual DynamicMetaObjectBinder GetInteropBinder(RubyContext/*!*/ context, IList<DynamicMetaObject/*!*/>/*!*/ args,
            out MethodInfo postProcessor) {

            postProcessor = null;
            return null;
        }

        private DynamicMetaObject/*!*/ InteropBind(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            // TODO: argument count limit depends on the binder!

            // TODO: pass block as the last (before RHS arg?) parameter/ignore block if args not accepting block:
            var normalizedArgs = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, Int32.MaxValue);
            if (!metaBuilder.Error) {
                MethodInfo postConverter;
                var interopBinder = GetInteropBinder(args.RubyContext, normalizedArgs, out postConverter);
                if (interopBinder != null) {
                    Type resultType;
                    var result = interopBinder.Bind(args.MetaTarget, ArrayUtils.MakeArray(normalizedArgs));

                    metaBuilder.SetMetaResult(result, args);
                    if (postConverter != null) {
                        // TODO: do better?
                        var paramType = postConverter.GetParameters()[0].ParameterType;

                        metaBuilder.Result = Ast.Call(null, postConverter, AstUtils.Convert(metaBuilder.Result, paramType));
                        resultType = postConverter.ReturnType;
                    } else {
                        resultType = interopBinder.ReturnType;
                    }

                    return metaBuilder.CreateMetaObject(interopBinder, resultType);
                } else {
                    metaBuilder.SetError(Ast.New(
                       typeof(NotSupportedException).GetConstructor(new[] { typeof(string) }),
                       Ast.Constant(String.Format("{0} not supported on foreign meta-objects", this))
                    ));
                }
            }
            return metaBuilder.CreateMetaObject(this);
        }

        internal static bool IsForeignMetaObject(DynamicMetaObject/*!*/ metaObject) {
            return metaObject.Value is IDynamicMetaObjectProvider && !(metaObject is RubyMetaObject) || Utils.IsComObjectType(metaObject.LimitType);
        }
    }
}
