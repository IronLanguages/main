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
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime.Calls;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Conversions {

    // 1) implicit conversion to MutableString
    // 2) calls to_s
    // 3) default conversion if (2) returns a non-string
    public sealed class ConvertToSAction : RubyConversionAction {
        public override Type/*!*/ ReturnType {
            get { return typeof(MutableString); }
        }

        public static ConvertToSAction/*!*/ Make(RubyContext/*!*/ context) {
            return context.MetaBinderFactory.Conversion<ConvertToSAction>();
        }

        [Emitted]
        public static ConvertToSAction/*!*/ MakeShared() {
            return RubyMetaBinderFactory.Shared.Conversion<ConvertToSAction>();
        }

        public override Expression/*!*/ CreateExpression() {
            return Methods.GetMethod(GetType(), "MakeShared").OpCall();
        }

        protected override DynamicMetaObjectBinder/*!*/ GetInteropBinder(RubyContext/*!*/ context, IList<DynamicMetaObject/*!*/>/*!*/ args, 
            out MethodInfo postConverter) {
            postConverter = Methods.StringToMutableString;
            return context.MetaBinderFactory.InteropInvokeMember("ToString", new CallInfo(0));
        }

        protected override bool Build(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, bool defaultFallback) {
            Debug.Assert(defaultFallback, "custom fallback not supported");
            BuildConversion(metaBuilder, args);
            return true;
        }

        internal static void BuildConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            const string ToS = "to_s";

            if (TryImplicitConversion(metaBuilder, args)) {
                metaBuilder.AddTypeRestriction(args.Target.GetType(), args.TargetExpression);
                return;
            }

            RubyMemberInfo conversionMethod, methodMissing = null;

            RubyClass targetClass = args.RubyContext.GetImmediateClassOf(args.Target);
            using (targetClass.Context.ClassHierarchyLocker()) {
                metaBuilder.AddTargetTypeTest(args.Target, targetClass, args.TargetExpression, args.MetaContext, 
                    new[] { ToS, Symbols.MethodMissing }
                );

                conversionMethod = targetClass.ResolveMethodForSiteNoLock(ToS, VisibilityContext.AllVisible).Info;

                // find method_missing - we need to add "to_s" method to the missing methods table:
                if (conversionMethod == null) {
                    methodMissing = targetClass.ResolveMethodMissingForSite(ToS, RubyMethodVisibility.None);
                }
            }
            
            // invoke target.to_s and if successful convert the result to string unless it is already:
            if (conversionMethod != null) {
                conversionMethod.BuildCall(metaBuilder, args, ToS);
            } else {
                RubyCallAction.BuildMethodMissingCall(metaBuilder, args, ToS, methodMissing, RubyMethodVisibility.None, false, true);
            }

            if (metaBuilder.Error) {
                return;
            }

            metaBuilder.Result = Methods.ToSDefaultConversion.OpCall(
                AstUtils.Convert(args.MetaContext.Expression, typeof(RubyContext)),
                AstUtils.Box(args.TargetExpression),
                AstUtils.Box(metaBuilder.Result)
            );
        }

        private static bool TryImplicitConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            var stringTarget = args.Target as MutableString;
            if (stringTarget != null) {
                metaBuilder.Result = AstUtils.Convert(args.TargetExpression, typeof(MutableString));
                return true;
            }

            return false;
        }
    }

}
