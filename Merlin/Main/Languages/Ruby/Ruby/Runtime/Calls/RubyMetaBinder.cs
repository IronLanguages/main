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
using System;
using Microsoft.Scripting.Utils;
using System.Collections.Generic;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using System.Reflection;
using Microsoft.Scripting;

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

        protected abstract bool Build(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, bool defaultFallback);

        protected virtual DynamicMetaObjectBinder GetInteropBinder(RubyContext/*!*/ context, IList<DynamicMetaObject/*!*/>/*!*/ args, 
            out MethodInfo postProcessor) {

            postProcessor = null;
            return null;
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
                        resultType = ((IInteropBinder)interopBinder).ResultType;
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
