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
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using Microsoft.Scripting.Runtime;
using System.Linq.Expressions;
using Ast = System.Linq.Expressions.Expression;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    // 1) implicit conversion to MutableString
    // 2) calls to_s
    // 3) default conversion if (2) returns a non-string
    public sealed class ConvertToSAction : RubyConversionAction, IExpressionSerializable, IEquatable<ConvertToSAction> {
        public static readonly ConvertToSAction Instance = new ConvertToSAction();

        private ConvertToSAction() {
        }

        [Emitted]
        public static ConvertToSAction/*!*/ Make() {
            return Instance;
        }

        public bool Equals(ConvertToSAction other) {
            return other != null;
        }

        Expression/*!*/ IExpressionSerializable.CreateExpression() {
            return Ast.Call(Methods.GetMethod(GetType(), "Make"));
        }

        protected override void BuildConversion(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            const string ToS = "to_s";

            // no conversion for a subclass of string:
            var stringTarget = args.Target as MutableString;
            if (stringTarget != null) {
                metaBuilder.AddTypeRestriction(args.Target.GetType(), args.TargetExpression);
                metaBuilder.Result = AstUtils.Convert(args.TargetExpression, typeof(MutableString));
                return;
            }

            RubyMemberInfo conversionMethod;

            RubyClass targetClass = args.RubyContext.GetImmediateClassOf(args.Target);
            using (targetClass.Context.ClassHierarchyLocker()) {
                metaBuilder.AddTargetTypeTest(args.Target, targetClass, args.TargetExpression, args.RubyContext, args.ContextExpression);
                conversionMethod = targetClass.ResolveMethodForSiteNoLock(ToS, false).Info;
            }
            
            // invoke target.to_s and if successful convert the result to string unless it is already:
            if (conversionMethod != null) {
                conversionMethod.BuildCall(metaBuilder, args, ToS);

                if (metaBuilder.Error) {
                    return;
                }
            } else {
                metaBuilder.Result = args.TargetExpression;
            }

            metaBuilder.Result = Methods.ToSDefaultConversion.OpCall(args.ContextExpression, metaBuilder.Result);
        }
    }
}
