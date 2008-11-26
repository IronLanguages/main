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
using System.Dynamic.Binders;
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
        public MetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<MetaObject> args) {
            return CallMethod(
                parameterBinder,
                targets,
                args,
                new CallSignature(args.Count),
                Restrictions.Empty
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
        public MetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<MetaObject> args, NarrowingLevel minLevel, NarrowingLevel maxLevel) {
            return CallWorker(
                parameterBinder,
                targets,
                args,
                new CallSignature(args.Count),
                CallTypes.None,
                Restrictions.Empty,
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
        public MetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<MetaObject> args, CallSignature signature) {
            return CallMethod(parameterBinder, targets, args, signature, Restrictions.Empty);
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
        public MetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<MetaObject> args, CallSignature signature, string name) {
            return CallMethod(parameterBinder, targets, args, signature, Restrictions.Empty, name);
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
        public MetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<MetaObject> args, CallSignature signature, Restrictions restrictions) {
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
        public MetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<MetaObject> args, CallSignature signature, Restrictions restrictions, string name) {
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
        public MetaObject CallInstanceMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, MetaObject instance, IList<MetaObject> args, CallSignature signature, Restrictions restrictions) {
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
        public MetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<MetaObject> args, CallSignature signature, Restrictions restrictions, NarrowingLevel minLevel, NarrowingLevel maxLevel, out BindingTarget target) {
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
        public MetaObject CallMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<MetaObject> args, CallSignature signature, Restrictions restrictions, NarrowingLevel minLevel, NarrowingLevel maxLevel, string name, out BindingTarget target) {
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
        public MetaObject CallInstanceMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, MetaObject instance, IList<MetaObject> args, CallSignature signature, Restrictions restrictions, NarrowingLevel minLevel, NarrowingLevel maxLevel, out BindingTarget target) {
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
        public MetaObject CallInstanceMethod(ParameterBinder parameterBinder, IList<MethodBase> targets, MetaObject instance, IList<MetaObject> args, CallSignature signature, Restrictions restrictions, NarrowingLevel minLevel, NarrowingLevel maxLevel, string name, out BindingTarget target) {
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

        private MetaObject CallWorker(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<MetaObject> args, CallSignature signature, CallTypes callType, Restrictions restrictions, NarrowingLevel minLevel, NarrowingLevel maxLevel, string name) {
            BindingTarget dummy;
            return CallWorker(parameterBinder, targets, args, signature, callType, restrictions, minLevel, maxLevel, name, out dummy);
        }

        private MetaObject CallWorker(ParameterBinder parameterBinder, IList<MethodBase> targets, IList<MetaObject> args, CallSignature signature, CallTypes callType, Restrictions restrictions, NarrowingLevel minLevel, NarrowingLevel maxLevel, string name, out BindingTarget target) {
            ContractUtils.RequiresNotNull(parameterBinder, "parameterBinder");
            ContractUtils.RequiresNotNullItems(args, "args");
            ContractUtils.RequiresNotNullItems(targets, "targets");
            ContractUtils.RequiresNotNull(restrictions, "restrictions");

            MetaObject[] finalArgs;
            SymbolId[] argNames;

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
                return new MetaObject(
                    target.MakeExpression(parameterBinder),
                    restrictions.Merge(MakeSplatTests(callType, signature, args).Merge(Restrictions.Combine(target.RestrictedArguments)))
                );
            }
            // make an error rule
            return MakeInvalidParametersRule(callType, signature, this, args, restrictions, target);
        }

        #endregion

        #region Restriction helpers

        private static Restrictions MakeSplatTests(CallTypes callType, CallSignature signature, IList<MetaObject> args) {
            return MakeSplatTests(callType, signature, false, args);
        }

        /// <summary>
        /// Makes test for param arrays and param dictionary parameters.
        /// </summary>
        private static Restrictions MakeSplatTests(CallTypes callType, CallSignature signature, bool testTypes, IList<MetaObject> args) {
            Restrictions res = Restrictions.Empty;

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
        private static Restrictions MakeParamsArrayTest(CallTypes callType, CallSignature signature, bool testTypes, IList<MetaObject> args) {
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
        private static Restrictions MakeParamsTest(object paramArg, Expression listArg, bool testTypes) {
            IList<object> coll = (IList<object>)paramArg;

            Restrictions res = Restrictions.GetExpressionRestriction(
                Ast.AndAlso(
                    Ast.TypeIs(listArg, typeof(IList<object>)),
                    Ast.Equal(
                        Ast.Property(
                            Ast.Convert(listArg, typeof(IList<object>)),
                            typeof(ICollection<object>).GetProperty("Count")
                        ),
                        Ast.Constant(coll.Count)
                    )
                )
            );

            if (testTypes) {
                for (int i = 0; i < coll.Count; i++) {
                    res = res.Merge(
                        Restrictions.GetTypeRestriction(
                            Ast.Call(
                                AstUtils.Convert(
                                    listArg,
                                    typeof(IList<object>)
                                ),
                                typeof(IList<object>).GetMethod("get_Item"),
                                Ast.Constant(i)
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
        private static Restrictions MakeParamsDictionaryTest(IList<MetaObject> args, bool testTypes) {
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

            return Restrictions.GetExpressionRestriction(
                Ast.AndAlso(
                    Ast.TypeIs(args[args.Count - 1].Expression, typeof(IDictionary)),
                    Ast.Call(
                        typeof(BinderOps).GetMethod("CheckDictionaryMembers"),
                        Ast.Convert(args[args.Count - 1].Expression, typeof(IDictionary)),
                        Ast.Constant(names),
                        testTypes ? Ast.Constant(types) : Ast.Constant(null, typeof(Type[]))
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
        private static void GetArgumentNamesAndTypes(CallSignature signature, IList<MetaObject> args, out SymbolId[] argNames, out MetaObject[] resultingArgs) {
            // Get names of named arguments
            argNames = signature.GetArgumentNames();

            resultingArgs = GetArgumentTypes(signature, args);

            if (signature.HasDictionaryArgument()) {
                // need to get names from dictionary argument...
                GetDictionaryNamesAndTypes(args, ref argNames, ref resultingArgs);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")] // TODO: fix
        private static MetaObject[] GetArgumentTypes(CallSignature signature, IList<MetaObject> args) {
            List<MetaObject> res = new List<MetaObject>();
            List<MetaObject> namedObjects = null;
            for (int i = 0; i < args.Count; i++) {
                switch (signature.GetArgumentKind(i)) {
                    case ArgumentType.Named:
                        if (namedObjects == null) {
                            namedObjects = new List<MetaObject>();
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
                                new MetaObject(
                                        Ast.Call(
                                            Ast.Convert(
                                                args[i].Expression,
                                                typeof(IList<object>)
                                            ),
                                            typeof(IList<object>).GetMethod("get_Item"),
                                            Ast.Constant(j)
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

        private static void GetDictionaryNamesAndTypes(IList<MetaObject> args, ref SymbolId[] argNames, ref MetaObject[] argTypes) {
            List<SymbolId> names = new List<SymbolId>(argNames);
            List<MetaObject> types = new List<MetaObject>(argTypes);

            IDictionary dict = (IDictionary)args[args.Count - 1].Value;
            MetaObject dictMo = args[args.Count - 1];
            IDictionaryEnumerator dictEnum = dict.GetEnumerator();
            while (dictEnum.MoveNext()) {
                DictionaryEntry de = dictEnum.Entry;

                if (de.Key is string) {
                    names.Add(SymbolTable.StringToId((string)de.Key));
                    types.Add(
                        new MetaObject(
                            Ast.Call(
                                AstUtils.Convert(dictMo.Expression, typeof(IDictionary)),
                                typeof(IDictionary).GetMethod("get_Item"),
                                Ast.Constant(de.Key as string)
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

        private static MetaObject MakeInvalidParametersRule(CallTypes callType, CallSignature signature, DefaultBinder binder, IList<MetaObject> args, Restrictions restrictions, BindingTarget bt) {
            Restrictions restriction = MakeSplatTests(callType, signature, true, args);

            // restrict to the exact type of all parameters for errors
            for (int i = 0; i < args.Count; i++) {
                args[i] = args[i].Restrict(args[i].LimitType);
            }

            return MakeError(
                binder.MakeInvalidParametersError(bt),
                restrictions.Merge(Restrictions.Combine(args).Merge(restriction))
            );
        }

        private static string GetTargetName(IList<MethodBase> targets) {
            return targets[0].IsConstructor ? targets[0].DeclaringType.Name : targets[0].Name;
        }

        #endregion
    }
}
