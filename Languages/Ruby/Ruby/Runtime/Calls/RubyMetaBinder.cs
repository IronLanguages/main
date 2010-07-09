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
using System.Dynamic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Runtime.Calls {
	using AstUtils = Microsoft.Scripting.Ast.Utils;
    using Ast = Expression;

    public abstract class RubyMetaBinder : DynamicMetaObjectBinder, ILightCallSiteBinder, IExpressionSerializable {
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

        bool ILightCallSiteBinder.AcceptsArgumentArray {
            get { return true; }
        }

        public abstract RubyCallSignature Signature { get; }

        protected abstract bool Build(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, bool defaultFallback);
        public abstract Expression CreateExpression();

        public sealed override T BindDelegate<T>(CallSite<T> site, object[] args) {
            object firstArg = args[0];
            ArgumentArray argArray = firstArg as ArgumentArray;
            Type delegateType = typeof(T);
            T result;

            if (argArray != null) {
                firstArg = argArray.GetArgument(0);
            } else {
                object precompiled = BindPrecompiled(delegateType, args);
                if (precompiled != null) {
                    result = (T)precompiled;
                    CacheTarget(result);
                    return result;
                }
            }

            RubyContext context = _context ?? ((Signature.HasScope) ? ((RubyScope)firstArg).RubyContext : (RubyContext)firstArg);

            if (context.Options.NoAdaptiveCompilation) {
                return null;
            }

#if DEBUG
            if (RubyOptions.ShowRules) {
	            var sb = new StringBuilder();
	            for (int i = 1; i < args.Length; i++) {
	                sb.Append(RubyUtils.ObjectToMutableString(context, args[i]));
	                sb.Append(", ");
	            }

                Utils.Log(String.Format(
                    "{0}: {1}; {2} <: {3}",
                    this,
                    sb,
                    args.Length > 1 ? context.GetImmediateClassOf(args[1]).DebugName : null,
                    args.Length > 1 ? context.GetImmediateClassOf(args[1]).SuperClass.DebugName : null
                ), "BIND");
            }
#endif
            result = this.LightBind<T>(args, context.Options.CompilationThreshold);
            CacheTarget(result);
            return result;
        }

        /// <summary>
        /// Returns a precompiled rule delegate or null, if not available for the given delegate type and arguments.
        /// </summary>
        protected virtual object BindPrecompiled(Type/*!*/ delegateType, object[]/*!*/ args) {
            return null;
        } 

        public override DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ scopeOrContextOrTargetOrArgArray, DynamicMetaObject/*!*/[]/*!*/ args) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "Ruby: " + GetType().Name + Signature.ToString() + ": Bind");

            var callArgs = new CallArguments(_context, scopeOrContextOrTargetOrArgArray, args, Signature);
            var metaBuilder = new MetaObjectBuilder(this, args);

            DynamicMetaObject interopBinding;
            if (IsForeignMetaObject(callArgs.MetaTarget) && (interopBinding = InteropBind(metaBuilder, callArgs)) != null) {
                return interopBinding;
            }

            Build(metaBuilder, callArgs, true);
            return metaBuilder.CreateMetaObject(this);
        }

        protected virtual DynamicMetaObjectBinder GetInteropBinder(RubyContext/*!*/ context, IList<DynamicMetaObject/*!*/>/*!*/ args,
            out MethodInfo postProcessor) {

            postProcessor = null;
            return null;
        }

        private DynamicMetaObject InteropBind(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
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
                    // interop protocol not supported for this binder -> ignore IDO and treat it as a CLR object:
                    return null;
                }
            }
            return metaBuilder.CreateMetaObject(this);
        }

        internal static bool IsForeignMetaObject(DynamicMetaObject/*!*/ metaObject) {
            return metaObject.Value is IDynamicMetaObjectProvider && !(metaObject is RubyMetaObject) || TypeUtils.IsComObjectType(metaObject.LimitType);
        }
    }
}
