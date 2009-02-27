/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;

    public class CreateInstanceBinderHelper : CallBinderHelper<OldCreateInstanceAction> {
        public CreateInstanceBinderHelper(CodeContext context, OldCreateInstanceAction action, object[] args, RuleBuilder rule)
            : base(context, action, args, rule) {
        }

        public override void MakeRule() {
            base.MakeRule();

            if (Rule.IsError) {
                // Constructing a delegate?
                Type t = GetTargetType(Callable);

                if (typeof(Delegate).IsAssignableFrom(t) && Arguments.Length == 2) {
                    MethodInfo dc = GetDelegateCtor(t);

                    // ScriptingRuntimeHelpers.CreateDelegate<T>(CodeContext context, object callable);

                    Rule.IsError = false;
                    Rule.Target = Rule.MakeReturn(
                        Binder,
                        Expression.Call(null, dc, Rule.Context, Rule.Parameters[1])
                    );
                }
            }
        }

        private static MethodInfo GetDelegateCtor(Type t) {
            return typeof(BinderOps).GetMethod("CreateDelegate").MakeGenericMethod(t);
        }

        protected override MethodBase[] GetTargetMethods() {
            object target = Arguments[0];
            Type t = GetTargetType(target);

            if (t != null) {
                Test = Ast.AndAlso(Test, Ast.Equal(Rule.Parameters[0], AstUtils.Constant(target)));

                return CompilerHelpers.GetConstructors(t, Binder.PrivateBinding);
            }

            return null;
        }
        
        private static Type GetTargetType(object target) {
            TypeTracker tt = target as TypeTracker;
            if (tt != null) {
                return tt.Type;
            }
            return target as Type;
        }

        protected override void MakeCannotCallRule(Type type) {
            string name = type.Name;
            Type t = Arguments[0] as Type;
            if (t != null) name = t.Name;

            Rule.Target =
                Rule.MakeError(
                    Ast.New(
                        typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                        AstUtils.Constant("Cannot create instances of " + name)
                    )
                );
        }
    }
}
