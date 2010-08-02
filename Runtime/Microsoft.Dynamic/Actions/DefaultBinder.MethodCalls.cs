/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Dynamic;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = Expression;
    
    public partial class DefaultBinder : ActionBinder {

        #region Public APIs

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments.  The arguments are
        /// consumed as specified by the CallSignature object.
        /// </summary>
        /// <param name="resolver">Overload resolver.</param>
        /// <param name="targets">The methods to be called</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallMethod(DefaultOverloadResolver resolver, IList<MethodBase> targets) {
            return CallMethod(resolver, targets, BindingRestrictions.Empty, null);
        }

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments.  The arguments are
        /// consumed as specified by the CallSignature object.
        /// </summary>
        /// <param name="resolver">Overload resolver.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="name">The name of the method or null to use the name from targets.</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallMethod(DefaultOverloadResolver resolver, IList<MethodBase> targets, string name) {
            return CallMethod(resolver, targets, BindingRestrictions.Empty, name);
        }

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments.  The arguments are
        /// consumed as specified by the CallSignature object.
        /// </summary>
        /// <param name="resolver">Overload resolver.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="restrictions">Additional restrictions which should be applied to the resulting MetaObject.</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallMethod(DefaultOverloadResolver resolver, IList<MethodBase> targets, BindingRestrictions restrictions) {
            return CallMethod(
                resolver,
                targets,
                restrictions,
                null
            );
        }

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments.  The arguments are
        /// consumed as specified by the CallSignature object.
        /// </summary>
        /// <param name="resolver">Overload resolver.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="restrictions">Additional restrictions which should be applied to the resulting MetaObject.</param>
        /// <param name="name">The name of the method or null to use the name from targets.</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallMethod(DefaultOverloadResolver resolver, IList<MethodBase> targets, BindingRestrictions restrictions, string name) {
            BindingTarget target;
            return CallMethod(
                resolver,
                targets,
                restrictions,
                name,
                NarrowingLevel.None,
                NarrowingLevel.All,
                out target
            );
        }

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments.  The arguments are
        /// consumed as specified by the CallSignature object.
        /// </summary>
        /// <param name="minLevel">TODO.</param>
        /// <param name="maxLevel">TODO.</param>
        /// <param name="resolver">Overload resolver.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="restrictions">Additional restrictions which should be applied to the resulting MetaObject.</param>
        /// <param name="target">The resulting binding target which can be used for producing error information.</param>
        /// <param name="name">The name of the method or null to use the name from targets.</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallMethod(DefaultOverloadResolver resolver, IList<MethodBase> targets, BindingRestrictions restrictions, string name, 
            NarrowingLevel minLevel, NarrowingLevel maxLevel, out BindingTarget target) {
            ContractUtils.RequiresNotNull(resolver, "resolver");
            ContractUtils.RequiresNotNullItems(targets, "targets");
            ContractUtils.RequiresNotNull(restrictions, "restrictions");

            // attempt to bind to an individual method
            target = resolver.ResolveOverload(name ?? GetTargetName(targets), targets, minLevel, maxLevel);

            if (target.Success) {
                // if we succeed make the target for the rule
                return new DynamicMetaObject(
                    target.MakeExpression(),
                    restrictions.Merge(
                        MakeSplatTests(resolver.CallType, resolver.Signature, resolver.Arguments).
                            Merge(target.RestrictedArguments.GetAllRestrictions())
                    )
                );
            }

            // make an error rule
            return MakeInvalidParametersRule(resolver, restrictions, target);
        }

        internal static string GetTargetName(IList<MethodBase> targets) {
            return targets[0].IsConstructor ? targets[0].DeclaringType.Name : targets[0].Name;
        }

        // TODO: revisit
        private DynamicMetaObject MakeInvalidParametersRule(DefaultOverloadResolver binder, BindingRestrictions restrictions, BindingTarget bt) {
            var args = binder.Arguments;
            
            BindingRestrictions restriction = MakeSplatTests(binder.CallType, binder.Signature, true, args);

            // restrict to the exact type of all parameters for errors
            for (int i = 0; i < args.Count; i++) {
                args[i] = args[i].Restrict(args[i].GetLimitType());
            }

            return MakeError(
                binder.MakeInvalidParametersError(bt),
                restrictions.Merge(BindingRestrictions.Combine(args).Merge(restriction)),
                typeof(object)
            );
        }

        #endregion

        #region Restriction helpers (TODO: revisit)

        private static BindingRestrictions MakeSplatTests(CallTypes callType, CallSignature signature, IList<DynamicMetaObject> args) {
            return MakeSplatTests(callType, signature, false, args);
        }

        /// <summary>
        /// Makes test for param arrays and param dictionary parameters.
        /// </summary>
        private static BindingRestrictions MakeSplatTests(CallTypes callType, CallSignature signature, bool testTypes, IList<DynamicMetaObject> args) {
            BindingRestrictions res = BindingRestrictions.Empty;

            if (signature.HasListArgument()) {
                res = MakeParamsArrayTest(callType, signature, testTypes, args);
            }

            if (signature.HasDictionaryArgument()) {
                res = res.Merge(MakeParamsDictionaryTest(args, testTypes));
            }

            return res;
        }

        /// <summary>
        /// Pulls out the right argument to build the splat test.  MakeParamsTest makes the actual test.
        /// </summary>
        private static BindingRestrictions MakeParamsArrayTest(CallTypes callType, CallSignature signature, bool testTypes, IList<DynamicMetaObject> args) {
            int listIndex = signature.IndexOf(ArgumentType.List);
            Debug.Assert(listIndex != -1);
            if (callType == CallTypes.ImplicitInstance) {
                listIndex++;
            }

            return MakeParamsTest(args[listIndex], testTypes);
        }

        /// <summary>
        /// Builds the restrictions for calling with a splatted argument array.  Ensures that the
        /// argument is still an ICollection of object and that it has the same number of arguments.
        /// </summary>
        private static BindingRestrictions MakeParamsTest(DynamicMetaObject splattee, bool testTypes) {
            IList<object> list = splattee.Value as IList<object>;

            if (list == null) {
                if (splattee.Value == null) {
                    return BindingRestrictions.GetExpressionRestriction(Ast.Equal(splattee.Expression, AstUtils.Constant(null)));
                } else {
                   return BindingRestrictions.GetTypeRestriction(splattee.Expression, splattee.Value.GetType());
                }
            }

            BindingRestrictions res = BindingRestrictions.GetExpressionRestriction(
                Ast.AndAlso(
                    Ast.TypeIs(splattee.Expression, typeof(IList<object>)),
                    Ast.Equal(
                        Ast.Property(
                            Ast.Convert(splattee.Expression, typeof(IList<object>)),
                            typeof(ICollection<object>).GetProperty("Count")
                        ),
                        AstUtils.Constant(list.Count)
                    )
                )
            );

            if (testTypes) {
                for (int i = 0; i < list.Count; i++) {
                    res = res.Merge(
                        BindingRestrictionsHelpers.GetRuntimeTypeRestriction(
                            Ast.Call(
                                AstUtils.Convert(
                                    splattee.Expression,
                                    typeof(IList<object>)
                                ),
                                typeof(IList<object>).GetMethod("get_Item"),
                                AstUtils.Constant(i)
                            ),
                            CompilerHelpers.GetType(list[i])
                        )
                    );
                }
            }

            return res;
        }

        /// <summary>
        /// Builds the restrictions for calling with keyword arguments.  The restrictions include
        /// tests on the individual keys of the dictionary to ensure they have the same names.
        /// </summary>
        private static BindingRestrictions MakeParamsDictionaryTest(IList<DynamicMetaObject> args, bool testTypes) {
            IDictionary dict = (IDictionary)args[args.Count - 1].Value;
            IDictionaryEnumerator dictEnum = dict.GetEnumerator();

            // verify the dictionary has the same count and arguments.

            string[] names = new string[dict.Count];
            Type[] types = testTypes ? new Type[dict.Count] : null;
            int index = 0;
            while (dictEnum.MoveNext()) {
                string name = dictEnum.Entry.Key as string;
                if (name == null) {
                    throw ScriptingRuntimeHelpers.SimpleTypeError(String.Format("expected string for dictionary argument got {0}", dictEnum.Entry.Key));
                }
                names[index] = name;
                if (types != null) {
                    types[index] = CompilerHelpers.GetType(dictEnum.Entry.Value);
                }
                index++;
            }

            return BindingRestrictions.GetExpressionRestriction(
                Ast.AndAlso(
                    Ast.TypeIs(args[args.Count - 1].Expression, typeof(IDictionary)),
                    Ast.Call(
                        typeof(BinderOps).GetMethod("CheckDictionaryMembers"),
                        Ast.Convert(args[args.Count - 1].Expression, typeof(IDictionary)),
                        AstUtils.Constant(names),
                        testTypes ? AstUtils.Constant(types) : AstUtils.Constant(null, typeof(Type[]))
                    )
                )
            );
        }

        #endregion
    }
}
