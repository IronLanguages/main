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
using System.Dynamic;
using System.Reflection;
using IronRuby.Compiler;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Compiler.Generation;
using System.Linq.Expressions;
using System.Diagnostics;

namespace IronRuby.Runtime.Calls {
    public sealed class GenericConversionAction : RubyConversionAction {
        private readonly Type/*!*/ _type;

        public override Type ReturnType {
            get { return _type; }
        }

        internal GenericConversionAction(RubyContext context, Type/*!*/ type)
            : base(context) {
            Assert.NotNull(type);

            // Type must be visible so that we can serialize it in MakeShared.
            Debug.Assert(type.IsVisible);

            _type = type;
        }

        [Emitted]
        public static GenericConversionAction/*!*/ MakeShared(Type/*!*/ type) {
            return RubyMetaBinderFactory.Shared.GenericConversionAction(type);
        }

        public override Expression/*!*/ CreateExpression() {
            return Methods.GetMethod(typeof(GenericConversionAction), "MakeShared").OpCall(Ast.Constant(_type, typeof(Type)));
        }

        protected override DynamicMetaObjectBinder/*!*/ GetInteropBinder(RubyContext/*!*/ context, IList<DynamicMetaObject>/*!*/ args,
            out MethodInfo postProcessor) {

            postProcessor = null;
            return new InteropBinder.Convert(context, _type, true);
        }

        protected override bool Build(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, bool defaultFallback) {
            if (TryImplicitConversion(metaBuilder, args)) {
                metaBuilder.AddObjectTypeRestriction(args.Target, args.TargetExpression);
                return true;
            }

            // TODO: this is our meta object, should we add IRubyMetaConvertible interface instead of using interop-binder?
            if (args.Target is IDynamicMetaObjectProvider) {
                metaBuilder.SetMetaResult(args.MetaTarget.BindConvert(new InteropBinder.Convert(args.RubyContext, _type, true)), false);
                return true;
            }

            if (defaultFallback) {
                metaBuilder.AddObjectTypeRestriction(args.Target, args.TargetExpression);

                metaBuilder.SetError(Methods.MakeTypeConversionError.OpCall(
                    args.MetaContext.Expression,
                    AstUtils.Convert(args.TargetExpression, typeof(object)),
                    Ast.Constant(_type)
                ));
                return true;
            }

            return false;
        }

        internal bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            if (args.Target == null) {
                if (!_type.IsValueType || _type.IsGenericType && _type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    metaBuilder.Result = AstUtils.Constant(null, _type);
                    return true;
                } else {
                    return false;
                }
            }

            if (_type.IsAssignableFrom(args.Target.GetType())) {
                metaBuilder.Result = Ast.Convert(args.MetaTarget.Expression, _type);
                return true;
            }

            return false;
        }
    }


}
