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
using System.Reflection;
using System.Dynamic;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Builtins {
    using Ast = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public partial class Proc : IRubyDynamicMetaObjectProvider {
        public DynamicMetaObject/*!*/ GetMetaObject(Expression/*!*/ parameter) {
            return new Meta(parameter, BindingRestrictions.Empty, this);
        }

        internal sealed class Meta : RubyMetaObject<Proc>, IConvertibleMetaObject, IInferableInvokable {
            public override RubyContext/*!*/ Context {
                get { return Value.LocalScope.RubyContext; }
            }

            protected override MethodInfo/*!*/ ContextConverter {
                get { return Methods.GetContextFromProc; }
            }

            public Meta(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, Proc/*!*/ value)
                : base(expression, restrictions, value) {
            }

            // Conversion to a delegate.
            public bool CanConvertTo(Type/*!*/ type, bool @explicit) {
                return typeof(Delegate).IsAssignableFrom(type);
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
