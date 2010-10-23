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
using System.Reflection;
using IronRuby.Compiler;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Compiler.Generation;
using System.Diagnostics;
using IronRuby.Runtime.Calls;

namespace IronRuby.Runtime.Conversions {
    using Ast = Expression;
    using Microsoft.Scripting.Generation;

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
            return context.MetaBinderFactory.InteropConvert(_type, true);
        }

        protected override bool Build(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, bool defaultFallback) {
            // TODO: this is our meta object, should we add IRubyMetaConvertible interface instead of using interop-binder?
            if (args.Target is IDynamicMetaObjectProvider) {
                metaBuilder.SetMetaResult(args.MetaTarget.BindConvert(args.RubyContext.MetaBinderFactory.InteropConvert(_type, true)), false);
                return true;
            }

            return BuildConversion(metaBuilder, args.MetaTarget, args.MetaContext.Expression, _type, defaultFallback);
        }

        internal static bool BuildConversion(MetaObjectBuilder/*!*/ metaBuilder, DynamicMetaObject/*!*/ target, Expression/*!*/ contextExpression, 
            Type/*!*/ toType, bool defaultFallback) {

            Expression expr = TryImplicitConversion(target, toType);
            if (expr != null) {
                metaBuilder.Result = expr;
                metaBuilder.AddObjectTypeRestriction(target.Value, target.Expression);
                return true;
            }

            if (defaultFallback) {
                metaBuilder.AddObjectTypeRestriction(target.Value, target.Expression);

                metaBuilder.SetError(Methods.MakeTypeConversionError.OpCall(
                    contextExpression,
                    AstUtils.Convert(target.Expression, typeof(object)),
                    Ast.Constant(toType, typeof(Type))
                ));
                return true;
            }

            return false;
        }

        private static Expression TryImplicitConversion(DynamicMetaObject/*!*/ target, Type/*!*/ toType) {
            // TODO: include this into ImplicitConvert?
            if (target.Value == null) {
                if (!toType.IsValueType || toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    return AstUtils.Constant(null, toType);
                } else {
                    return null;
                }
            }

            Type fromType = target.Value.GetType();
            return 
                Converter.ImplicitConvert(target.Expression, fromType, toType) ??
                Converter.ExplicitConvert(target.Expression, fromType, toType);
        }
    }


}
