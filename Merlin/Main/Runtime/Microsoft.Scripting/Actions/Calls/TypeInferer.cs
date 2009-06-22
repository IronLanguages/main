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
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    class TypeInferer {
        private static ArgumentInputs EnsureInputs(Dictionary<Type, ArgumentInputs> dict, Type type) {
            ArgumentInputs res;
            if (!dict.TryGetValue(type, out res)) {
                dict[type] = res = new ArgumentInputs(type);
            }
            return res;
        }

        internal static MethodCandidate InferGenericMethod(ApplicableCandidate/*!*/ candidate, ActualArguments/*!*/ actualArgs) {
            MethodInfo target = ((MethodInfo)candidate.Method.Method);
            Assert.NotNull(target);
            Debug.Assert(target.IsGenericMethodDefinition);
            Debug.Assert(target.IsGenericMethod && target.ContainsGenericParameters);

            List<DynamicMetaObject/*!*/> args = GetAllArguments(candidate, actualArgs);
            if (args == null) {
                return null;
            }

            Dictionary<Type, List<Type>> dependencies = GetDependencyMapping(target);
            Type[] genArgs = GetSortedGenericArguments(target, dependencies);
            Dictionary<Type, ArgumentInputs> inputs = GetArgumentToInputMapping(candidate.Method, args);

            // now process the inputs
            var constraints = new Dictionary<Type, Type>();
            var restrictions = new Dictionary<DynamicMetaObject, BindingRestrictions>();
            bool noMethod = false;
            foreach (Type t in genArgs) {
                ArgumentInputs inps;
                if (!inputs.TryGetValue(t, out inps)) {
                    continue;
                }

                Type bestType = inps.GetBestType(candidate.Method.Resolver, constraints, restrictions);
                if (bestType == null) {
                    // we conflict with possible constraints
                    noMethod = true;
                    break;
                }
            }

            if (!noMethod) {
                // finally build a new MethodCandidate for the generic method
                genArgs = GetGenericArgumentsForInferedMethod(target, constraints);
                if (genArgs == null) {
                    // not all types we're inferred
                    return null;
                }
                
                MethodInfo newMethod = target.MakeGenericMethod(genArgs);
                ParameterInfo[] newParams = newMethod.GetParameters();
                ParameterInfo[] oldParams = target.GetParameters();

                List<ParameterWrapper> newWrappers = CreateNewWrappers(candidate.Method, newParams, oldParams);

                List<ArgBuilder> argBuilders = CreateNewArgBuilders(candidate.Method, newParams);
                if (argBuilders == null) {
                    // one or more arg builders don't support type inference
                    return null;
                }

                if (restrictions.Count == 0) {
                    restrictions = null;
                }

                // create the new method candidate
                return candidate.Method.ReplaceMethod(newMethod, newWrappers, argBuilders, restrictions);
            }

            return null;
        }

        /// <summary>
        /// Gets the generic arguments for method based upon the constraints discovered during
        /// type inference.  Returns null if not all generic arguments had their types inferred.
        /// </summary>
        private static Type[] GetGenericArgumentsForInferedMethod(MethodInfo target, Dictionary<Type, Type> constraints) {
            Type[] genArgs = target.GetGenericArguments();
            for (int i = 0; i < genArgs.Length; i++) {
                Type newType;
                if (!constraints.TryGetValue(genArgs[i], out newType)) {
                    // we didn't discover any types for this type argument
                    return null;
                }
                genArgs[i] = newType;
            }
            return genArgs;
        }

        /// <summary>
        /// Creates a new set of arg builders for the given generic method definition which target the new
        /// parameters.
        /// </summary>
        private static List<ArgBuilder> CreateNewArgBuilders(MethodCandidate candidate, ParameterInfo[] newParams) {
            List<ArgBuilder> argBuilders = new List<ArgBuilder>();
            foreach (ArgBuilder oldArgBuilder in candidate.ArgBuilders) {
                var pi = oldArgBuilder.ParameterInfo;

                if (pi != null && (pi.ParameterType.IsGenericParameter || pi.ParameterType.ContainsGenericParameters)) {
                    ArgBuilder replacement = oldArgBuilder.Clone(newParams[pi.Position]);

                    if (replacement == null) {
                        return null;
                    }
                    argBuilders.Add(replacement);
                } else {
                    argBuilders.Add(oldArgBuilder);
                }
            }
            return argBuilders;
        }

        /// <summary>
        /// Creates a new list of ParameterWrappers for the generic method replacing the old parameters with the new ones.
        /// </summary>
        private static List<ParameterWrapper> CreateNewWrappers(MethodCandidate candidate, ParameterInfo[] newParams, ParameterInfo[] oldParams) {
            List<ParameterWrapper> newWrappers = new List<ParameterWrapper>();
            for (int i = 0; i < candidate.ParameterCount; i++) {
                ParameterWrapper oldWrap = candidate.GetParameter(i);
                ParameterInfo pi = null;
                Type newType = oldWrap.Type;
                if (oldWrap.ParameterInfo != null) {
                    pi = newParams[oldWrap.ParameterInfo.Position];
                    ParameterInfo oldParam = oldParams[oldWrap.ParameterInfo.Position];
                    if (oldParam.ParameterType == oldWrap.Type) {
                        newType = pi.ParameterType;
                    } else {
                        Debug.Assert(oldParam.ParameterType.GetElementType() == oldWrap.Type);
                        newType = pi.ParameterType.GetElementType();
                    }
                }

                newWrappers.Add(new ParameterWrapper(pi, newType, oldWrap.Name, oldWrap.ProhibitNull, oldWrap.IsParamsArray, oldWrap.IsParamsDict, oldWrap.IsHidden));
            }
            return newWrappers;
        }

        private static List<DynamicMetaObject> GetAllArguments(ApplicableCandidate candidate, ActualArguments actualArgs) {
            List<DynamicMetaObject> args = new List<DynamicMetaObject>();
            for (int i = 0; i < actualArgs.Count; i++) {
                int index = candidate.ArgumentBinding.ArgumentToParameter(i);
                if (index < actualArgs.Arguments.Count) {
                    args.Add(actualArgs.Arguments[index]);
                } else {
                    args.Add(actualArgs.NamedArguments[index - actualArgs.Arguments.Count]);
                }
            }
            return args;
        }

        /// <summary>
        /// Gets the generic type arguments sorted so that the type arguments
        /// that are depended upon by other type arguments are sorted before
        /// their dependencies.
        /// </summary>
        private static Type[] GetSortedGenericArguments(MethodBase mb, Dictionary<Type, List<Type>> dependencies) {
            Type[] genArgs = mb.GetGenericArguments();

            // Then sort the arguments based upon those dependencies
            Array.Sort(genArgs, (x, y) => {
                if (Object.ReferenceEquals(x, y)) {
                    return 0;
                }

                bool isDependent = IsDependentConstraint(dependencies, x, y);
                if (isDependent) {
                    return 1;
                }

                isDependent = IsDependentConstraint(dependencies, y, x);
                if (isDependent) {
                    return -1;
                }

                int xhash = x.GetHashCode(), yhash = y.GetHashCode();
                if (xhash != yhash) {
                    return xhash - yhash;
                }

                long idDiff = IdDispenser.GetId(x) - IdDispenser.GetId(y);
                return idDiff > 0 ? 1 : -1;
            });


            return genArgs;
        }

        /// <summary>
        /// Checks to see if the x type parameter is dependent upon the y type parameter.
        /// </summary>
        private static bool IsDependentConstraint(Dictionary<Type, List<Type>> dependencies, Type x, Type y) {
            List<Type> childDeps;
            if (dependencies.TryGetValue(x, out childDeps)) {
                foreach (Type t in childDeps) {
                    if (t == y) {
                        return true;
                    }

                    bool isDependent = IsDependentConstraint(dependencies, t, y);
                    if (isDependent) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Builds a mapping based upon generic parameter constraints between related generic
        /// parameters.  This is then used to sort the generic parameters so that we can process
        /// the least dependent parameters first.  For example given the method:
        /// 
        /// void Foo&lt;T0, T1&gt;(T0 x, T1 y) where T0 : T1 
        /// 
        /// We need to first infer the type information for T1 before we infer the type information
        /// for T0 so that we can ensure the constraints are correct.
        /// </summary>
        private static Dictionary<Type, List<Type>> GetDependencyMapping(MethodBase mb) {
            Type[] genArgs = mb.GetGenericArguments();
            Dictionary<Type, List<Type>> dependencies = new Dictionary<Type, List<Type>>();

            // need to calculate any dependencies between parameters.
            foreach (Type genArg in genArgs) {
                Type[] constraints = genArg.GetGenericParameterConstraints();
                foreach (Type t in constraints) {
                    if (t.IsGenericParameter) {
                        AddDependency(dependencies, genArg, t);
                    } else if (t.ContainsGenericParameters) {
                        AddNestedDependencies(dependencies, genArg, t);
                    }
                }
            }
            return dependencies;
        }

        private static void AddNestedDependencies(Dictionary<Type, List<Type>> dependencies, Type genArg, Type t) {
            Type[] innerArgs = t.GetGenericArguments();
            foreach (Type innerArg in innerArgs) {
                if (innerArg.IsGenericParameter) {
                    AddDependency(dependencies, genArg, innerArg);
                } else if (innerArg.ContainsGenericParameters) {
                    AddNestedDependencies(dependencies, genArg, innerArg);
                }
            }
        }

        private static void AddDependency(Dictionary<Type, List<Type>> dependencies, Type genArg, Type t) {
            List<Type> deps;
            if (!dependencies.TryGetValue(genArg, out deps)) {
                dependencies[genArg] = deps = new List<Type>();
            }

            deps.Add(t);
        }


        /// <summary>
        /// Returns a mapping from generic type parameter to the input DMOs which map to it.
        /// </summary>
        private static Dictionary<Type/*!*/, ArgumentInputs/*!*/>/*!*/ GetArgumentToInputMapping(MethodCandidate/*!*/ wrappers, IList<DynamicMetaObject/*!*/>/*!*/ args) {
            Dictionary<Type, ArgumentInputs> inputs = new Dictionary<Type, ArgumentInputs>();

            for (int curParam = 0; curParam < wrappers.ParameterCount; curParam++) {
                if (wrappers.GetParameter(curParam).IsParamsArray) {
                    AddOneInput(inputs, args[curParam], wrappers.GetParameter(curParam).Type.GetElementType());
                } else {
                    AddOneInput(inputs, args[curParam], wrappers.GetParameter(curParam).Type);
                }
            }

            return inputs;
        }

        /// <summary>
        /// Adds any additional ArgumentInputs entries for the given object and parameter type.
        /// </summary>
        private static void AddOneInput(Dictionary<Type, ArgumentInputs> inputs, DynamicMetaObject arg, Type paramType) {
            if (paramType.IsGenericParameter) {
                EnsureInputs(inputs, paramType).AddInput(arg, new GenericParameterInferer(paramType));
            } else if (paramType.ContainsGenericParameters) {
                List<Type> containedGenArgs = new List<Type>();
                CollectGenericParameters(paramType, containedGenArgs);

                foreach (Type type in containedGenArgs) {
                    EnsureInputs(inputs, type).AddInput(arg, new ConstructedParameterInferer(paramType));
                }
            }
        }

        /// <summary>
        /// Walks the nested generic hierarchy to construct all of the generic parameters referred
        /// to by this type.  For example if getting the generic parameters for the x parameter on
        /// the method:
        /// 
        /// void Foo&lt;T0, T1&gt;(Dictionary&lt;T0, T1&gt; x);
        /// 
        /// We would add both typeof(T0) and typeof(T1) to the list of generic arguments.
        /// </summary>
        private static void CollectGenericParameters(Type type, List<Type> containedGenArgs) {            
            if (type.IsGenericParameter) {
                if (!containedGenArgs.Contains(type)) {
                    containedGenArgs.Add(type);
                }
            } else if (type.ContainsGenericParameters) {
                Type[] genArgs = type.GetGenericArguments();
                for (int i = 0; i < genArgs.Length; i++) {
                    CollectGenericParameters(genArgs[i], containedGenArgs);
                }
            }

        }

        /// <summary>
        /// Maps a single type parameter to the possible parameters and DynamicMetaObjects
        /// we can get inference from.  For example for the signature:
        /// 
        /// void Foo&lt;T0, T1&gt;(T0 x, T1 y, IList&lt;T1&gt; z);
        /// 
        /// We would have one ArgumentInput for T0 which holds onto the DMO providing the argument
        /// value for x.  We would also have one ArgumentInput for T1 which holds onto the 2 DMOs
        /// for y and z.  Associated with y would be a GenericParameterInferer and associated with
        /// z would be a ConstructedParameterInferer.
        /// </summary>
        class ArgumentInputs {
            private readonly List<ParameterInferer> _mappings = new List<ParameterInferer>();
            private readonly List<DynamicMetaObject> _inputs = new List<DynamicMetaObject>();
            private readonly Type _genericParam;

            public ArgumentInputs(Type genericParam) {
                _genericParam = genericParam;
            }

            public void AddInput(DynamicMetaObject value, ParameterInferer inferer) {
                _mappings.Add(inferer);
                _inputs.Add(value);
            }

            public Type GetBestType(OverloadResolver resolver, Dictionary<Type, Type> prevConstraints, Dictionary<DynamicMetaObject, BindingRestrictions> restrictions) {
                Type curType = null;

                for (int i = 0; i < _mappings.Count; i++) {
                    ParameterInferer inpMapping = _mappings[i];
                    DynamicMetaObject input = _inputs[i];
                    Type nextType = inpMapping.GetInferedType(resolver, _genericParam, input, prevConstraints, restrictions);

                    if (nextType == null) {
                        // no mapping available
                        return null;
                    } else if (curType == null || curType.IsAssignableFrom(nextType)) {
                        curType = nextType;
                    } else if (!nextType.IsAssignableFrom(curType)) {
                        // inconsistent constraint.
                        return null;
                    } else {
                        curType = nextType;
                    }
                }

                return curType;
            }
        }

        /// <summary>
        /// Provides generic type inference for a single parameter.
        /// </summary>
        abstract class ParameterInferer {
            private readonly Type _type;

            public ParameterInferer(Type parameterType) {
                _type = parameterType;
            }

            public abstract Type GetInferedType(OverloadResolver resolver, Type genericParameter, DynamicMetaObject input, Dictionary<Type, Type> prevConstraints, Dictionary<DynamicMetaObject, BindingRestrictions> restrictions);

            /// <summary>
            /// The parameter type which inference is happening for.  This is the actual parameter type
            /// and not the generic parameter.  For example it could be IList&lt;T&gt; or T.
            /// </summary>
            public Type ParameterType {
                get {
                    return _type;
                }
            }

            /// <summary>
            /// Checks if the constraints are violated by the given input for the specified generic method parameter.
            /// 
            /// This method must be supplied with a mapping for any dependent generic method type parameters which
            /// this one can be constrained to.  For example for the signature "void Foo&lt;T0, T1&gt;(T0 x, T1 y) where T0 : T1".
            /// we cannot know if the constraints are violated unless we know what we have calculated T1 to be.
            /// </summary>
            protected static bool ConstraintsViolated(Type inputType, Type genericMethodParameterType, Dictionary<Type, Type> prevConstraints) {
                if ((genericMethodParameterType.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0 && inputType.IsValueType) {
                    // value type to parameter type constrained as class
                    return true;
                } else if ((genericMethodParameterType.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0 &&
                    (!inputType.IsValueType || (inputType.IsGenericType && inputType.GetGenericTypeDefinition() == typeof(Nullable<>)))) {
                    // nullable<T> or class/interface to parameter type constrained as struct
                    return true;
                } else if ((genericMethodParameterType.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0 &&
                    (!inputType.IsValueType && inputType.GetConstructor(Type.EmptyTypes) == null)) {
                    // reference type w/o a default constructor to type constrianed as new()
                    return true;
                }

                Type[] constraints = genericMethodParameterType.GetGenericParameterConstraints();
                for (int i = 0; i < constraints.Length; i++) {
                    Type t = constraints[i];
                    if (t.ContainsGenericParameters) {
                        t = ReplaceTypes(t, prevConstraints);
                        if (t == null) {
                            return true;
                        }
                    } 

                    if (!t.IsAssignableFrom(inputType)) {
                        // constraint cannot be satisfied
                        return true;
                    }
                }
                return false;
            }

            private static Type ReplaceTypes(Type t, Dictionary<Type, Type> prevConstraints) {
                Type res;
                if (prevConstraints.TryGetValue(t, out res)) {
                    return res;
                } else if (t.IsGenericParameter) {
                    return null;
                } else if (t.ContainsGenericParameters) {
                    Type[] genArgs = t.GetGenericArguments();
                    for (int i = 0; i < genArgs.Length; i++) {
                        genArgs[i] = ReplaceTypes(genArgs[i], prevConstraints);
                        if (genArgs[i] == null) {
                            return null;
                        }
                    }

                    return t.GetGenericTypeDefinition().MakeGenericType(genArgs);
                }

                return t;
            }
        }

        /// <summary>
        /// Provides type inference for a parameter which is typed to be a method type parameter.
        /// 
        /// For example: M&lt;T&gt;(T x)
        /// </summary>
        class GenericParameterInferer : ParameterInferer {
            public GenericParameterInferer(Type type)
                : base(type) {
            }

            public override Type GetInferedType(OverloadResolver resolver, Type genericParameter, DynamicMetaObject input, Dictionary<Type, Type> prevConstraints, Dictionary<DynamicMetaObject, BindingRestrictions> restrictions) {
                Type inputType = resolver.GetGenericInferenceType(input);
                if (inputType != null) {
                    prevConstraints[genericParameter] = inputType;
                    if (ConstraintsViolated(inputType, genericParameter, prevConstraints)) {
                        return null;
                    }
                }

                return inputType;
            }
        }

        /// <summary>
        /// Provides type inference for a parameter which is constructed from a method type parameter.
        /// 
        /// For example: M&lt;T&gt;(IList&lt;T&gt; x)
        /// </summary>
        class ConstructedParameterInferer : ParameterInferer {
            /// <summary>
            /// Constructs a new parameter inferer for the given parameter type which should
            /// contain generic parameters but not it's self be a generic parameter.
            /// </summary>
            /// <param name="parameterType"></param>
            public ConstructedParameterInferer(Type parameterType)
                : base(parameterType) {
                Debug.Assert(!parameterType.IsGenericParameter);
                Debug.Assert(parameterType.ContainsGenericParameters);
            }

            public override Type GetInferedType(OverloadResolver resolver, Type genericParameter, DynamicMetaObject input, Dictionary<Type, Type> prevConstraints, Dictionary<DynamicMetaObject, BindingRestrictions> restrictions) {
                Type inputType = resolver.GetGenericInferenceType(input);

                if (ParameterType.IsInterface) {
                    // see if we implement this interface exactly once
                    Type[] interfaces = inputType.GetInterfaces();
                    Type targetType = null;
                    Type genTypeDef = ParameterType.GetGenericTypeDefinition();
                    foreach (Type ifaceType in interfaces) {
                        if (ifaceType.IsGenericType && ifaceType.GetGenericTypeDefinition() == genTypeDef) {
                            if (targetType == null) {
                                // we may have a match, figure out the type...
                                targetType = InferGenericType(genericParameter, ifaceType, ParameterType, prevConstraints);
                            } else {
                                // multiple interface implementations match
                                return null;
                            }
                        }
                    }

                    prevConstraints[genericParameter] = targetType;
                    return targetType;
                } else if (ParameterType.IsSubclassOf(typeof(Delegate))) {
                    // see if we have an invokable object which can be used to infer into this delegate
                    IInferableInvokable invokeInfer = input as IInferableInvokable;
                    if (invokeInfer != null) {
                        InferenceResult inference = invokeInfer.GetInferredType(ParameterType, genericParameter);
                        if (inference != null) {
                            if (inference.Restrictions != BindingRestrictions.Empty) {
                                restrictions[input] = inference.Restrictions;
                            }

                            prevConstraints[genericParameter] = inference.Type;

                            if (ConstraintsViolated(inference.Type, genericParameter, prevConstraints)) {
                                return null;
                            }

                            return inference.Type;
                        }
                    }
                }

                // see if we're anywhere in our base class hierarchy
                Type curType = input.LimitType;
                Type genType = ParameterType.GetGenericTypeDefinition();
                while (curType != typeof(object)) {
                    if (curType.IsGenericType) {
                        if (curType.GetGenericTypeDefinition() == genType) {
                            // TODO: Merge w/ the interface logic above
                            Type unboundType = ParameterType;

                            Type res = InferGenericType(genericParameter, curType, unboundType, prevConstraints);
                            prevConstraints[genericParameter] = res;
                            return res;
                        }
                    }
                    curType = curType.BaseType;
                }
                

                return null;
            }

            /// <summary>
            /// Performs the actual inference by mapping any generic arguments which map onto method type parameters
            /// to the available type information for the incoming object.
            /// </summary>
            private static Type InferGenericType(Type genericParameter, Type curType, Type unboundType, Dictionary<Type, Type> prevConstraints) {
                Type[] concreteArgs = curType.GetGenericArguments();
                Type[] abstractArgs = unboundType.GetGenericArguments();

                Type curInferredType = null;
                for (int i = 0; i < abstractArgs.Length; i++) {
                    if (abstractArgs[i] == genericParameter) {
                        if (curInferredType == null) {
                            curInferredType = concreteArgs[i];
                        } else if (concreteArgs[i] != curInferredType) {
                            return null;
                        }
                    } else if (abstractArgs[i].ContainsGenericParameters) {
                        // IList<Func<T>>
                        Type newType = InferGenericType(genericParameter, concreteArgs[i], abstractArgs[i], prevConstraints);
                        if (curInferredType == null) {
                            curInferredType = newType;
                        } else if (newType != curInferredType) {
                            return null;
                        }
                    }
                }

                if (ConstraintsViolated(curInferredType, genericParameter, prevConstraints)) {
                    return null;
                }

                return curInferredType;
            }
        }
    }

    /// <summary>
    /// Implemented by DynamicMetaObject subclasses when the associated object
    /// can participate in generic method type inference.  This interface
    /// is used when the inference engine is attempting to perform type inference
    /// for a parameter which is typed to a delegate type.
    /// </summary>
    public interface IInferableInvokable {
        /// <summary>
        /// Returns the type inferred for parameterType when performing
        /// inference for a conversion to delegateType.
        /// </summary>
        InferenceResult GetInferredType(Type delegateType, Type parameterType);
    }

    /// <summary>
    /// Provides information about the result of a custom object which dynamically
    /// infers back types.
    /// 
    /// Currently only used for invokable objects to feedback the types for a delegate
    /// type.
    /// </summary>
    public class InferenceResult {
        private readonly Type _type;
        private readonly BindingRestrictions _restrictions;

        public InferenceResult(Type type, BindingRestrictions restrictions) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(restrictions, "restrictions");

            _type = type;
            _restrictions = restrictions;
        }

        public Type Type {
            get {
                return _type;
            }
        }

        public BindingRestrictions Restrictions {
            get {
                return _restrictions;
            }
        }
    }
}
