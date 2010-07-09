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
using System.Reflection;
using System.Dynamic;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Builtins {
    using Ast = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public partial class Proc : IRubyDynamicMetaObjectProvider {
        public DynamicMetaObject/*!*/ GetMetaObject(Expression/*!*/ parameter) {
            return new Meta(parameter, BindingRestrictions.Empty, this);
        }

        internal sealed class Meta : RubyMetaObject<Proc>, IConvertibleRubyMetaObject, IInferableInvokable {
            public override RubyContext/*!*/ Context {
                get { return Value.LocalScope.RubyContext; }
            }

            protected override MethodInfo/*!*/ ContextConverter {
                get { return Methods.GetContextFromProc; }
            }

            public Meta(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, Proc/*!*/ value)
                : base(expression, restrictions, value) {
            }

            bool IConvertibleMetaObject.CanConvertTo(Type/*!*/ type, bool @explicit) {
                return IsConvertibleTo(type, @explicit).IsConvertible;
            }

            // Conversion to a delegate.
            // Explicit: convertible to any delegate type.
            // Implicit: convertible to a delegate type of the same arity as the block.
            public Convertibility IsConvertibleTo(Type/*!*/ type, bool @explicit) {
                if (!typeof(Delegate).IsAssignableFrom(type)) {
                    return Convertibility.NotConvertible;
                }

                if (@explicit) {
                    return Convertibility.AlwaysConvertible;
                }

                if (!HasValue) {
                    return Convertibility.NotConvertible;
                }

                MethodInfo invoke = type.GetMethod("Invoke");
                if (invoke == null) {
                    return Convertibility.NotConvertible;
                }

                int delegateArity = invoke.GetParameters().Length;
                return new Convertibility(
                    delegateArity == Value.Dispatcher.ParameterCount || delegateArity > Value.Dispatcher.ParameterCount && Value.Dispatcher.HasUnsplatParameter,
                    Ast.Equal(Methods.GetProcArity.OpCall(AstUtils.Convert(Expression, typeof(Proc))), Ast.Constant(Value.Dispatcher.Arity))
                );
            }

            public override DynamicMetaObject/*!*/ BindConvert(ConvertBinder/*!*/ binder) {
                return InteropBinder.TryBindCovertToDelegate(this, binder, Methods.CreateDelegateFromProc)
                    ?? base.BindConvert(binder);
            }

            public override DynamicMetaObject/*!*/ BindInvoke(InvokeBinder/*!*/ binder, DynamicMetaObject/*!*/[]/*!*/ args) {
                return InteropBinder.Invoke.Bind(binder, this, args, Value.BuildInvoke);
            }

            #region IInvokableInferable Members

            InferenceResult IInferableInvokable.GetInferredType(Type delegateType, Type parameterType) {
                // a block can be called with any number of parameters, so we don't need to restrict the result:
                return new InferenceResult(
                    typeof(object),
                    BindingRestrictions.GetTypeRestriction(Expression, typeof(Proc))
                );
            }

            #endregion
        }
    }
}
