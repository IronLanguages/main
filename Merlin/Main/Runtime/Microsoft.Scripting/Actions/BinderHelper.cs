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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;

    public class BinderHelper {
        internal BinderHelper() { }

        // This can produce a IsCallable rule that returns the immutable constant isCallable.
        // Beware that objects can have a mutable callable property. Eg, in Python, assign or delete the __call__ attribute.
        public static bool MakeIsCallableRule(CodeContext context, object self, bool isCallable, RuleBuilder rule) {
            rule.MakeTest(CompilerHelpers.GetType(self));
            rule.Target =
                rule.MakeReturn(
                    context.LanguageContext.Binder,
                    AstUtils.Constant(isCallable)
                );

            return true;
        }
    }

    public class BinderHelper<TAction> : BinderHelper
        where TAction : OldDynamicAction {

        private readonly CodeContext _context;
        private readonly TAction _action;

        public BinderHelper(CodeContext context, TAction action) {
            ContractUtils.RequiresNotNull(context, "context");
            ContractUtils.RequiresNotNull(action, "action");

            _context = context;
            _action = action;
        }

        protected CodeContext Context {
            get {
                return _context;
            }
        }

        protected TAction Action {
            get {
                return _action;
            }
        }

        protected DefaultBinder Binder {
            get {
                return (DefaultBinder)_context.LanguageContext.Binder;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")] // TODO: fix
        public static UnaryExpression GetParamsList(RuleBuilder rule) {
            return Ast.Convert(
                rule.Parameters[rule.ParameterCount - 1],
                typeof(IList<object>)
            );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")] // TODO: fix
        public static Expression MakeParamsTest(object paramArg, Expression listArg) {
            return Ast.AndAlso(
                Ast.TypeIs(listArg, CompilerHelpers.GetType(paramArg)),
                Ast.Equal(
                    Ast.Property(
                        Ast.Convert(listArg, typeof(ICollection<object>)),
                        typeof(ICollection<object>).GetProperty("Count")
                    ),
                    AstUtils.Constant(((IList<object>)paramArg).Count)
                )
            );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")] // TODO: fix
        public static Type[] GetArgumentTypes(OldCallAction action, object[] args) {
            List<Type> res = new List<Type>();
            for (int i = 1; i < args.Length; i++) {
                switch (action.Signature.GetArgumentKind(i - 1)) {
                    case ArgumentType.Simple:
                    case ArgumentType.Instance:
                    case ArgumentType.Named:
                        res.Add(CompilerHelpers.GetType(args[i]));
                        continue;

                    case ArgumentType.List:
                        IList<object> list = args[i] as IList<object>;
                        if (list == null) return null;

                        for (int j = 0; j < list.Count; j++) {
                            res.Add(CompilerHelpers.GetType(list[j]));
                        }
                        break;

                    case ArgumentType.Dictionary:
                        // caller needs to process these...
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            return res.ToArray();
        }

        internal MethodInfo GetMethod(Type type, string name) {
            // declaring type takes precedence
            MethodInfo mi = type.GetMethod(name);
            if (mi != null) {
                return mi;
            }

            // then search extension types.
            Type curType = type;
            do {
                IList<Type> extTypes = Binder.GetExtensionTypes(curType);
                foreach (Type t in extTypes) {
                    MethodInfo next = t.GetMethod(name);
                    if (next != null) {
                        if (mi != null) {
                            throw new AmbiguousMatchException(String.Format("Found multiple members for {0} on type {1}", name, curType));
                        }

                        mi = next;
                    }
                }

                if (mi != null) {
                    return mi;
                }

                curType = curType.BaseType;
            } while (curType != null);

            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")] // TODO: fix
        public static Expression MakeNecessaryTests(RuleBuilder rule, Type[] testTypes, IList<Expression> arguments) {
            Expression typeTest = AstUtils.Constant(true);

            if (testTypes != null) {
                for (int i = 0; i < testTypes.Length; i++) {
                    if (testTypes[i] != null) {
                        Debug.Assert(i < arguments.Count);
                        typeTest = Ast.AndAlso(typeTest, rule.MakeTypeTest(testTypes[i], arguments[i]));
                    }
                }
            }

            return typeTest;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")] // TODO: fix
        public static Expression MakeNecessaryTests(RuleBuilder rule, IList<Type[]> necessaryTests, Expression[] arguments) {
            if (necessaryTests.Count == 0) {
                return AstUtils.Constant(true);
            }

            Type[] mostSpecificTypes = null; // This is the final types that will be checked after inspecting all the sets

            for (int i = 0; i < necessaryTests.Count; i++) {
                Type[] currentSet = necessaryTests[i];
                if (currentSet == null) {
                    // The current set is missing. Ignore it
                    continue;
                }

                // All the sets should be of the same size
                Debug.Assert(currentSet.Length == arguments.Length);

                if (mostSpecificTypes == null) {
                    mostSpecificTypes = new Type[currentSet.Length];
                }

                // For each type in the current set, check the type with the corresponding type in the previous sets
                for (int j = 0; j < currentSet.Length; j++) {
                    if (mostSpecificTypes[j] == null || mostSpecificTypes[j].IsAssignableFrom(currentSet[j])) {
                        // no test yet or more specific test
                        mostSpecificTypes[j] = currentSet[j];
                    } else {
                        // All sets should have compatible types in each slot
                        Debug.Assert(currentSet[j] == null || currentSet[j].IsAssignableFrom(mostSpecificTypes[j]));
                    }
                }
            }

            return MakeNecessaryTests(rule, mostSpecificTypes, arguments);
        }

        protected bool PrivateBinding {
            get {
                return Context.LanguageContext.DomainManager.Configuration.PrivateBinding;
            }
        }
    }
}
