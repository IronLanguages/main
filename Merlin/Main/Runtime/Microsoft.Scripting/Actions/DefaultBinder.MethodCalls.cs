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
using System.Dynamic;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;
    
    public partial class DefaultBinder : ActionBinder {

        #region Public APIs

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments.  All arguments 
        /// are treated as positional arguments.
        /// </summary>
        /// <param name="parameterBinder">ParameterBinder used to map arguments to parameters.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="args">The arguments for the call</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<DynamicMetaObject> args) {
            return CallMethod(
                parameterBinder,
                targets,
                args,
                new CallSignature(args.Count),
                BindingRestrictions.Empty
            );
        }

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments.  All arguments 
        /// are treated as positional arguments.
        /// </summary>
        /// <param name="parameterBinder">ParameterBinder used to map arguments to parameters.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="args">The arguments for the call</param>
        /// <param name="maxLevel">The maximum narrowing level for arguments.  The current narrowing level is flowed thorugh to the DefaultBinder.</param>
        /// <param name="minLevel">The minimum narrowing level for the arguments.  The current narrowing level is flowed thorugh to the DefaultBinder.</param>        
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<DynamicMetaObject> args, NarrowingLevel minLevel, NarrowingLevel maxLevel) {
            return CallWorker(
                parameterBinder,
                targets,
                args,
                new CallSignature(args.Count),
                CallTypes.None,
                BindingRestrictions.Empty,
                minLevel,
                maxLevel,
                null
            );
        }

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments.  The arguments are
        /// consumed as specified by the CallSignature object.
        /// </summary>
        /// <param name="parameterBinder">ParameterBinder used to map arguments to parameters.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="args">The arguments for the call</param>
        /// <param name="signature">The call signature which specified how the arguments will be consumed</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<DynamicMetaObject> args, CallSignature signature) {
            return CallMethod(parameterBinder, targets, args, signature, BindingRestrictions.Empty);
        }

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments.  The arguments are
        /// consumed as specified by the CallSignature object.
        /// </summary>
        /// <param name="parameterBinder">ParameterBinder used to map arguments to parameters.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="args">The arguments for the call</param>
        /// <param name="signature">The call signature which specified how the arguments will be consumed</param>
        /// <param name="name">The name of the method or null to use the name from targets.</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<DynamicMetaObject> args, CallSignature signature, string name) {
            return CallMethod(parameterBinder, targets, args, signature, BindingRestrictions.Empty, name);
        }

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments.  The arguments are
        /// consumed as specified by the CallSignature object.
        /// </summary>
        /// <param name="parameterBinder">ParameterBinder used to map arguments to parameters.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="args">The arguments for the call</param>
        /// <param name="signature">The call signature which specified how the arguments will be consumed</param>
        /// <param name="restrictions">Additional restrictions which should be applied to the resulting MetaObject.</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<DynamicMetaObject> args, CallSignature signature, BindingRestrictions restrictions) {
            return CallWorker(
                parameterBinder,
                targets,
                args,
                signature,
                CallTypes.None,
                restrictions,
                NarrowingLevel.None,
                NarrowingLevel.All,
                null
            );
        }

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments.  The arguments are
        /// consumed as specified by the CallSignature object.
        /// </summary>
        /// <param name="parameterBinder">ParameterBinder used to map arguments to parameters.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="args">The arguments for the call</param>
        /// <param name="signature">The call signature which specified how the arguments will be consumed</param>
        /// <param name="restrictions">Additional restrictions which should be applied to the resulting MetaObject.</param>
        /// <param name="name">The name of the method or null to use the name from targets.</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<DynamicMetaObject> args, CallSignature signature, BindingRestrictions restrictions, string name) {
            return CallWorker(
                parameterBinder,
                targets,
                args,
                signature,
                CallTypes.None,
                restrictions,
                NarrowingLevel.None,
                NarrowingLevel.All,
                name
            );
        }

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments and the specified
        /// instance argument.  The arguments are consumed as specified by the CallSignature object.
        /// </summary>
        /// <param name="parameterBinder">ParameterBinder used to map arguments to parameters.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="args">The arguments for the call</param>
        /// <param name="instance">The instance which will be provided for dispatching to an instance method.</param>
        /// <param name="signature">The call signature which specified how the arguments will be consumed</param>
        /// <param name="restrictions">Additional restrictions which should be applied to the resulting MetaObject.</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallInstanceMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, DynamicMetaObject instance, IList<DynamicMetaObject> args, CallSignature signature, BindingRestrictions restrictions) {
            ContractUtils.RequiresNotNull(instance, "instance");
            ContractUtils.RequiresNotNull(parameterBinder, "parameterBinder");

            return CallWorker(
                parameterBinder,
                targets,
                ArrayUtils.Insert(instance, args),
                signature,
                CallTypes.ImplicitInstance,
                restrictions,
                NarrowingLevel.None,
                NarrowingLevel.All,
                null
            );
        }

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments.  The arguments are
        /// consumed as specified by the CallSignature object.
        /// </summary>
        /// <param name="parameterBinder">ParameterBinder used to map arguments to parameters.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="args">The arguments for the call</param>
        /// <param name="signature">The call signature which specified how the arguments will be consumed</param>
        /// <param name="restrictions">Additional restrictions which should be applied to the resulting MetaObject.</param>
        /// <param name="maxLevel">The maximum narrowing level for arguments.  The current narrowing level is flowed thorugh to the DefaultBinder.</param>
        /// <param name="minLevel">The minimum narrowing level for the arguments.  The current narrowing level is flowed thorugh to the DefaultBinder.</param>        
        /// <param name="target">The resulting binding target which can be used for producing error information.</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<DynamicMetaObject> args, CallSignature signature, BindingRestrictions restrictions, NarrowingLevel minLevel, NarrowingLevel maxLevel, out BindingTarget target) {
            return CallWorker(
                parameterBinder,
                targets,
                args,
                signature,
                CallTypes.None,
                restrictions,
                minLevel,
                maxLevel,
                null,
                out target
            );
        }

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments.  The arguments are
        /// consumed as specified by the CallSignature object.
        /// </summary>
        /// <param name="parameterBinder">ParameterBinder used to map arguments to parameters.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="args">The arguments for the call</param>
        /// <param name="signature">The call signature which specified how the arguments will be consumed</param>
        /// <param name="restrictions">Additional restrictions which should be applied to the resulting MetaObject.</param>
        /// <param name="maxLevel">The maximum narrowing level for arguments.  The current narrowing level is flowed thorugh to the DefaultBinder.</param>
        /// <param name="minLevel">The minimum narrowing level for the arguments.  The current narrowing level is flowed thorugh to the DefaultBinder.</param>        
        /// <param name="target">The resulting binding target which can be used for producing error information.</param>
        /// <param name="name">The name of the method or null to use the name from targets.</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<DynamicMetaObject> args, CallSignature signature, BindingRestrictions restrictions, NarrowingLevel minLevel, NarrowingLevel maxLevel, string name, out BindingTarget target) {
            return CallWorker(
                parameterBinder,
                targets,
                args,
                signature,
                CallTypes.None,
                restrictions,
                minLevel,
                maxLevel,
                name,
                out target
            );
        }

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments and the specified
        /// instance argument.  The arguments are consumed as specified by the CallSignature object.
        /// </summary>
        /// <param name="parameterBinder">ParameterBinder used to map arguments to parameters.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="args">The arguments for the call</param>
        /// <param name="signature">The call signature which specified how the arguments will be consumed</param>
        /// <param name="restrictions">Additional restrictions which should be applied to the resulting MetaObject.</param>
        /// <param name="instance">The instance which will be provided for dispatching to an instance method.</param>
        /// <param name="maxLevel">The maximum narrowing level for arguments.  The current narrowing level is flowed thorugh to the DefaultBinder.</param>
        /// <param name="minLevel">The minimum narrowing level for the arguments.  The current narrowing level is flowed thorugh to the DefaultBinder.</param>        
        /// <param name="target">The resulting binding target which can be used for producing error information.</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallInstanceMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, DynamicMetaObject instance, IList<DynamicMetaObject> args, CallSignature signature, BindingRestrictions restrictions, NarrowingLevel minLevel, NarrowingLevel maxLevel, out BindingTarget target) {
            return CallWorker(
                parameterBinder,
                targets,
                ArrayUtils.Insert(instance, args),
                signature,
                CallTypes.ImplicitInstance,
                restrictions,
                minLevel,
                maxLevel,
                null,
                out target
            );
        }

        /// <summary>
        /// Performs binding against a set of overloaded methods using the specified arguments and the specified
        /// instance argument.  The arguments are consumed as specified by the CallSignature object.
        /// </summary>
        /// <param name="parameterBinder">ParameterBinder used to map arguments to parameters.</param>
        /// <param name="targets">The methods to be called</param>
        /// <param name="args">The arguments for the call</param>
        /// <param name="signature">The call signature which specified how the arguments will be consumed</param>
        /// <param name="restrictions">Additional restrictions which should be applied to the resulting MetaObject.</param>
        /// <param name="instance">The instance which will be provided for dispatching to an instance method.</param>
        /// <param name="maxLevel">The maximum narrowing level for arguments.  The current narrowing level is flowed thorugh to the DefaultBinder.</param>
        /// <param name="minLevel">The minimum narrowing level for the arguments.  The current narrowing level is flowed thorugh to the DefaultBinder.</param>        
        /// <param name="target">The resulting binding target which can be used for producing error information.</param>
        /// <param name="name">The name of the method or null to use the name from targets.</param>
        /// <returns>A meta object which results from the call.</returns>
        public DynamicMetaObject CallInstanceMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, DynamicMetaObject instance, IList<DynamicMetaObject> args, CallSignature signature, BindingRestrictions restrictions, NarrowingLevel minLevel, NarrowingLevel maxLevel, string name, out BindingTarget target) {
            return CallWorker(
                parameterBinder,
                targets,
                ArrayUtils.Insert(instance, args),
                signature,
                CallTypes.ImplicitInstance,
                restrictions,
                minLevel,
                maxLevel,
                name,
                out target
            );
        }

        private DynamicMetaObject CallWorker(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType, BindingRestrictions restrictions, NarrowingLevel minLevel, NarrowingLevel maxLevel, string name) {
            BindingTarget dummy;
            return CallWorker(parameterBinder, targets, args, signature, callType, restrictions, minLevel, maxLevel, name, out dummy);
        }

        private DynamicMetaObject CallWorker(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType, BindingRestrictions restrictions, NarrowingLevel minLevel, NarrowingLevel maxLevel, string name, out BindingTarget target) {
            ContractUtils.RequiresNotNull(parameterBinder, "parameterBinder");
            ContractUtils.RequiresNotNullItems(args, "args");
            ContractUtils.RequiresNotNullItems(targets, "targets");
            ContractUtils.RequiresNotNull(restrictions, "restrictions");

            DynamicMetaObject[] finalArgs;
            string[] argNames;

            if (callType == CallTypes.ImplicitInstance) {
                GetArgumentNamesAndTypes(signature, ArrayUtils.RemoveFirst(args), out argNames, out finalArgs);
                finalArgs = ArrayUtils.Insert(args[0], finalArgs);
            } else {
                GetArgumentNamesAndTypes(signature, args, out argNames, out finalArgs);
            }

            // attempt to bind to an individual method
            MethodBinder binder = MethodBinder.MakeBinder(
                this,
                name ?? GetTargetName(targets),
                targets,
                argNames,
                minLevel,
                maxLevel);
            target = binder.MakeBindingTarget(callType, finalArgs);

            if (target.Success) {
                // if we succeed make the target for the rule
                return new DynamicMetaObject(
                    target.MakeExpression(parameterBinder),
                    restrictions.Merge(MakeSplatTests(callType, signature, args).Merge(BindingRestrictions.Combine(target.RestrictedArguments.Objects)))
                );
            }
            // make an error rule
            return MakeInvalidParametersRule(callType, signature, this, args, restrictions, target);
        }

        #endregion

        #region Restriction helpers

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

            return MakeParamsTest(args[listIndex].Value, args[listIndex].Expression, testTypes);
        }

        /// <summary>
        /// Builds the restrictions for calling with a splatted argument array.  Ensures that the
        /// argument is still an ICollection of object and that it has the same number of arguments.
        /// </summary>
        private static BindingRestrictions MakeParamsTest(object paramArg, Expression listArg, bool testTypes) {
            IList<object> coll = (IList<object>)paramArg;

            BindingRestrictions res = BindingRestrictions.GetExpressionRestriction(
                Ast.AndAlso(
                    Ast.TypeIs(listArg, typeof(IList<object>)),
                    Ast.Equal(
                        Ast.Property(
                            Ast.Convert(listArg, typeof(IList<object>)),
                            typeof(ICollection<object>).GetProperty("Count")
                        ),
                        AstUtils.Constant(coll.Count)
                    )
                )
            );

            if (testTypes) {
                for (int i = 0; i < coll.Count; i++) {
                    res = res.Merge(
                        BindingRestrictionsHelpers.GetRuntimeTypeRestriction(
                            Ast.Call(
                                AstUtils.Convert(
                                    listArg,
                                    typeof(IList<object>)
                                ),
                                typeof(IList<object>).GetMethod("get_Item"),
                                AstUtils.Constant(i)
                            ),
                            CompilerHelpers.GetType(coll[i])
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

        #region Misc. Helpers

        /// <summary>
        /// Gets all of the argument names and types. The instance argument is not included
        /// </summary>
        /// <param name="argNames">The names correspond to the end of argTypes.
        /// ArgumentKind.Dictionary is unpacked in the return value.
        /// This is set to an array of size 0 if there are no keyword arguments</param>
        /// <param name="resultingArgs">Non named arguments are returned at the beginning.
        /// ArgumentKind.List is unpacked in the return value. </param>
        /// <param name="args">The MetaObject array which has the arguments for the call</param>
        /// <param name="signature">The signature we're building the call for</param>
        private static void GetArgumentNamesAndTypes(CallSignature signature, IList<DynamicMetaObject> args, out string[] argNames, out DynamicMetaObject[] resultingArgs) {
            // Get names of named arguments
            argNames = signature.GetArgumentNames();

            resultingArgs = GetArgumentTypes(signature, args);

            if (signature.HasDictionaryArgument()) {
                // need to get names from dictionary argument...
                GetDictionaryNamesAndTypes(args, ref argNames, ref resultingArgs);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")] // TODO: fix
        private static DynamicMetaObject[] GetArgumentTypes(CallSignature signature, IList<DynamicMetaObject> args) {
            List<DynamicMetaObject> res = new List<DynamicMetaObject>();
            List<DynamicMetaObject> namedObjects = null;
            for (int i = 0; i < args.Count; i++) {
                switch (signature.GetArgumentKind(i)) {
                    case ArgumentType.Named:
                        if (namedObjects == null) {
                            namedObjects = new List<DynamicMetaObject>();
                        }
                        namedObjects.Add(args[i]);
                        break;
                    case ArgumentType.Simple:
                    case ArgumentType.Instance:
                        res.Add(args[i]);
                        break;
                    case ArgumentType.List:
                        IList<object> list = args[i].Value as IList<object>;
                        if (list == null) return null;

                        for (int j = 0; j < list.Count; j++) {
                            res.Add(
                                new DynamicMetaObject(
                                        Ast.Call(
                                            Ast.Convert(
                                                args[i].Expression,
                                                typeof(IList<object>)
                                            ),
                                            typeof(IList<object>).GetMethod("get_Item"),
                                            AstUtils.Constant(j)
                                        ),
                                        args[i].Restrictions,
                                        list[j]
                                    )
                            );
                        }
                        break;
                    case ArgumentType.Dictionary:
                        // caller needs to process these...
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            if (namedObjects != null) {
                res.AddRange(namedObjects);
            }

            return res.ToArray();
        }

        private static void GetDictionaryNamesAndTypes(IList<DynamicMetaObject> args, ref string[] argNames, ref DynamicMetaObject[] argTypes) {
            List<string> names = new List<string>(argNames);
            List<DynamicMetaObject> types = new List<DynamicMetaObject>(argTypes);

            IDictionary dict = (IDictionary)args[args.Count - 1].Value;
            DynamicMetaObject dictMo = args[args.Count - 1];
            IDictionaryEnumerator dictEnum = dict.GetEnumerator();
            while (dictEnum.MoveNext()) {
                DictionaryEntry de = dictEnum.Entry;

                if (de.Key is string) {
                    names.Add((string)de.Key);
                    types.Add(
                        new DynamicMetaObject(
                            Ast.Call(
                                AstUtils.Convert(dictMo.Expression, typeof(IDictionary)),
                                typeof(IDictionary).GetMethod("get_Item"),
                                AstUtils.Constant(de.Key as string)
                            ),
                            dictMo.Restrictions,
                            de.Value
                        )
                    );
                }
            }

            argNames = names.ToArray();
            argTypes = types.ToArray();
        }

        private static DynamicMetaObject MakeInvalidParametersRule(CallTypes callType, CallSignature signature, DefaultBinder binder, IList<DynamicMetaObject> args, BindingRestrictions restrictions, BindingTarget bt) {
            BindingRestrictions restriction = MakeSplatTests(callType, signature, true, args);

            // restrict to the exact type of all parameters for errors
            for (int i = 0; i < args.Count; i++) {
                args[i] = args[i].Restrict(args[i].GetLimitType());
            }

            return MakeError(
                binder.MakeInvalidParametersError(bt),
                restrictions.Merge(BindingRestrictions.Combine(args).Merge(restriction))
            );
        }

        private static string GetTargetName(IList<MethodBase> targets) {
            return targets[0].IsConstructor ? targets[0].DeclaringType.Name : targets[0].Name;
        }

        #endregion
    }
}
