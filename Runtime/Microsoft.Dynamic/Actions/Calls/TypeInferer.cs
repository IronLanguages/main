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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    public static class TypeInferer {
        private static ArgumentInputs EnsureInputs(Dictionary<Type, ArgumentInputs> dict, Type type) {
            ArgumentInputs res;
            if (!dict.TryGetValue(type, out res)) {
                dict[type] = res = new ArgumentInputs(type);
            }
            return res;
        }

        internal static MethodCandidate InferGenericMethod(ApplicableCandidate/*!*/ candidate, ActualArguments/*!*/ actualArgs) {
            OverloadInfo target = candidate.Method.Overload;
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
            var binding = new Dictionary<Type, Type>();
            var restrictions = new Dictionary<DynamicMetaObject, BindingRestrictions>();
            bool noMethod = false;
            foreach (Type t in genArgs) {
                ArgumentInputs inps;
                if (!inputs.TryGetValue(t, out inps)) {
                    continue;
                }

                Type bestType = inps.GetBestType(candidate.Method.Resolver, binding, restrictions);
                if (bestType == null) {
                    // we conflict with possible constraints
                    noMethod = true;
                    break;
                }
            }

            if (!noMethod) {
                // finally build a new MethodCandidate for the generic method
                genArgs = GetGenericArgumentsForInferedMethod(target, binding);
                if (genArgs == null) {
                    // not all types we're inferred
                    return null;
                }
                
                OverloadInfo newMethod = target.MakeGenericMethod(genArgs);

                List<ParameterWrapper> newWrappers = CreateNewWrappers(candidate.Method, newMethod, target);

                List<ArgBuilder> argBuilders = CreateNewArgBuilders(candidate.Method, newMethod);
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
        private static Type[] GetGenericArgumentsForInferedMethod(OverloadInfo target, Dictionary<Type, Type> constraints) {
            Type[] genArgs = ArrayUtils.MakeArray(target.GenericArguments);
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
        private static List<ArgBuilder> CreateNewArgBuilders(MethodCandidate candidate, OverloadInfo newOverload) {
            List<ArgBuilder> argBuilders = new List<ArgBuilder>();
            foreach (ArgBuilder oldArgBuilder in candidate.ArgBuilders) {
                var pi = oldArgBuilder.ParameterInfo;

                if (pi != null && (pi.ParameterType.IsGenericParameter || pi.ParameterType.ContainsGenericParameters)) {
                    ArgBuilder replacement = oldArgBuilder.Clone(newOverload.Parameters[pi.Position]);

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
        private static List<ParameterWrapper> CreateNewWrappers(MethodCandidate candidate, OverloadInfo newOverload, OverloadInfo oldOverload) {
            List<ParameterWrapper> newWrappers = new List<ParameterWrapper>();
            for (int i = 0; i < candidate.ParameterCount; i++) {
                ParameterWrapper oldWrap = candidate.GetParameter(i);
                ParameterInfo pi = null;
                Type newType = oldWrap.Type;
                if (oldWrap.ParameterInfo != null) {
                    pi = newOverload.Parameters[oldWrap.ParameterInfo.Position];
                    ParameterInfo oldParam = oldOverload.Parameters[oldWrap.ParameterInfo.Position];

                    if (oldParam.ParameterType == oldWrap.Type) {
                        newType = pi.ParameterType;
                    } else if (pi.ParameterType.IsByRef) {
                        newType = pi.ParameterType.GetElementType();
                        if (oldParam.ParameterType.GetElementType() != oldWrap.Type) {
                            Debug.Assert(CompilerHelpers.IsStrongBox(oldWrap.Type));
                            newType = typeof(StrongBox<>).MakeGenericType(newType);
                        }
                    } else {
                        Debug.Assert(oldParam.ParameterType.GetElementType() == oldWrap.Type);
                        newType = pi.ParameterType.GetElementType();
                    }
                }

                newWrappers.Add(new ParameterWrapper(pi, newType, oldWrap.Name, oldWrap.Flags));
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
        private static Type[] GetSortedGenericArguments(OverloadInfo info, Dictionary<Type, List<Type>> dependencies) {
            Type[] genArgs = ArrayUtils.MakeArray(info.GenericArguments);

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
        /// void Foo{T0, T1}(T0 x, T1 y) where T0 : T1 
        /// 
        /// We need to first infer the type information for T1 before we infer the type information
        /// for T0 so that we can ensure the constraints are correct.
        /// </summary>
        private static Dictionary<Type, List<Type>> GetDependencyMapping(OverloadInfo info) {
            Dictionary<Type, List<Type>> dependencies = new Dictionary<Type, List<Type>>();

            // need to calculate any dependencies between parameters.
            foreach (Type genArg in info.GenericArguments) {
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
        private static Dictionary<Type/*!*/, ArgumentInputs/*!*/>/*!*/ GetArgumentToInputMapping(MethodCandidate/*!*/ candidate, IList<DynamicMetaObject/*!*/>/*!*/ args) {
            Dictionary<Type, ArgumentInputs> inputs = new Dictionary<Type, ArgumentInputs>();

            for (int curParam = 0; curParam < candidate.ParameterCount; curParam++) {
                ParameterWrapper param = candidate.GetParameter(curParam);
                if (param.IsParamsArray) {
                    AddOneInput(inputs, args[curParam], param.Type.GetElementType());
                } else if (param.IsByRef) {
                    AddOneInput(inputs, args[curParam], param.ParameterInfo.ParameterType);
                } else {
                    AddOneInput(inputs, args[curParam], param.Type);
                }
            }

            return inputs;
        }

        /// <summary>
        /// Adds any additional ArgumentInputs entries for the given object and parameter type.
        /// </summary>
        private static void AddOneInput(Dictionary<Type, ArgumentInputs> inputs, DynamicMetaObject arg, Type paramType) {
            if (paramType.ContainsGenericParameters) {
                List<Type> containedGenArgs = new List<Type>();
                CollectGenericParameters(paramType, containedGenArgs);

                foreach (Type type in containedGenArgs) {
                    EnsureInputs(inputs, type).AddInput(arg, paramType);
                }
            }
        }

        /// <summary>
        /// Walks the nested generic hierarchy to construct all of the generic parameters referred
        /// to by this type.  For example if getting the generic parameters for the x parameter on
        /// the method:
        /// 
        /// void Foo{T0, T1}(Dictionary{T0, T1} x);
        /// 
        /// We would add both typeof(T0) and typeof(T1) to the list of generic arguments.
        /// </summary>
        private static void CollectGenericParameters(Type type, List<Type> containedGenArgs) {            
            if (type.IsGenericParameter) {
                if (!containedGenArgs.Contains(type)) {
                    containedGenArgs.Add(type);
                }
            } else if (type.ContainsGenericParameters) {
                if (type.IsArray || type.IsByRef) {
                    CollectGenericParameters(type.GetElementType(), containedGenArgs);
                } else {
                    Type[] genArgs = type.GetGenericArguments();
                    for (int i = 0; i < genArgs.Length; i++) {
                        CollectGenericParameters(genArgs[i], containedGenArgs);
                    }
                }
            }

        }

        /// <summary>
        /// Maps a single type parameter to the possible parameters and DynamicMetaObjects
        /// we can get inference from.  For example for the signature:
        /// 
        /// void Foo{T0, T1}(T0 x, T1 y, IList{T1} z);
        /// 
        /// We would have one ArgumentInput for T0 which holds onto the DMO providing the argument
        /// value for x.  We would also have one ArgumentInput for T1 which holds onto the 2 DMOs
        /// for y and z.  Associated with y would be a GenericParameterInferer and associated with
        /// z would be a ConstructedParameterInferer.
        /// </summary>
        class ArgumentInputs {
            private readonly List<Type>/*!*/ _parameterTypes = new List<Type>();
            private readonly List<DynamicMetaObject>/*!*/ _inputs = new List<DynamicMetaObject>();
            private readonly Type/*!*/ _genericParam;

            public ArgumentInputs(Type/*!*/ genericParam) {
                Assert.NotNull(genericParam);
                Debug.Assert(genericParam.IsGenericParameter);
                _genericParam = genericParam;
            }

            public void AddInput(DynamicMetaObject/*!*/ value, Type/*!*/ parameterType) {
                _parameterTypes.Add(parameterType);
                _inputs.Add(value);
            }

            public Type GetBestType(OverloadResolver/*!*/ resolver, Dictionary<Type, Type>/*!*/ binding, Dictionary<DynamicMetaObject, BindingRestrictions>/*!*/ restrictions) {
                Type curType = null;

                for (int i = 0; i < _parameterTypes.Count; i++) {
                    Type nextType = GetInferedType(resolver, _genericParam, _parameterTypes[i], _inputs[i], binding, restrictions);

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
        /// <remarks>
        /// For example: 
        ///   M{T}(T x)
        ///   M{T}(IList{T} x)
        ///   M{T}(ref T x)
        ///   M{T}(T[] x)
        ///   M{T}(ref Dictionary{T,T}[] x)
        /// </remarks>
        internal static Type GetInferedType(OverloadResolver/*!*/ resolver, Type/*!*/ genericParameter, Type/*!*/ parameterType,
            DynamicMetaObject/*!*/ input, Dictionary<Type, Type>/*!*/ binding, Dictionary<DynamicMetaObject, BindingRestrictions>/*!*/ restrictions) {

            if (parameterType.IsSubclassOf(typeof(Delegate))) {
                // see if we have an invokable object which can be used to infer into this delegate
                IInferableInvokable invokeInfer = input as IInferableInvokable;
                if (invokeInfer != null) {
                    InferenceResult inference = invokeInfer.GetInferredType(parameterType, genericParameter);
                    if (inference != null) {
                        if (inference.Restrictions != BindingRestrictions.Empty) {
                            restrictions[input] = inference.Restrictions;
                        }

                        binding[genericParameter] = inference.Type;

                        if (ConstraintsViolated(inference.Type, genericParameter, binding)) {
                            return null;
                        }

                        return inference.Type;
                    }
                }
            }

            return GetInferedType(genericParameter, parameterType, resolver.GetGenericInferenceType(input), input.LimitType, binding);
        }

        /// <summary>
        /// Provides generic type inference for a single parameter.
        /// </summary>
        /// <remarks>
        /// For example: 
        ///   M{T}(T x)
        ///   M{T}(IList{T} x)
        ///   M{T}(ref T x)
        ///   M{T}(T[] x)
        ///   M{T}(ref Dictionary{T,T}[] x)
        /// </remarks>
        public static Type GetInferedType(Type/*!*/ genericParameter, Type/*!*/ parameterType, Type inputType, Type/*!*/ argType, Dictionary<Type, Type>/*!*/ binding) {
            Debug.Assert(genericParameter.IsGenericParameter);

            if (parameterType.IsGenericParameter) {
                if (inputType != null) {
                    binding[genericParameter] = inputType;
                    if (ConstraintsViolated(inputType, genericParameter, binding)) {
                        return null;
                    }
                }

                return inputType;
            }

            if (parameterType.IsInterface) {
                return GetInferedTypeForInterface(genericParameter, parameterType, inputType, binding);
            }

            if (parameterType.IsArray) {
                return binding[genericParameter] = MatchGenericParameter(genericParameter, argType, parameterType, binding);
            }

            if (parameterType.IsByRef) {
                if (CompilerHelpers.IsStrongBox(argType)) {
                    argType = argType.GetGenericArguments()[0];
                }
                return binding[genericParameter] = MatchGenericParameter(genericParameter, argType, parameterType.GetElementType(), binding);
            }

            // see if we're anywhere in our base class hierarchy
            Type genType = parameterType.GetGenericTypeDefinition();
            while (argType != typeof(object)) {
                if (argType.IsGenericType && argType.GetGenericTypeDefinition() == genType) {
                    // TODO: Merge w/ the interface logic?
                    return binding[genericParameter] = MatchGenericParameter(genericParameter, argType, parameterType, binding);
                }
                argType = argType.BaseType;
            }
                
            return null;
        }

        //
        // The argument can implement multiple instantiations of the same generic interface definition, e.g.
        // ArgType : I<C<X>>, I<D<Y>>
        // ParamType == I<C<T>>
        //
        // Unless X == Y we can't infer T.
        //
        private static Type GetInferedTypeForInterface(Type/*!*/ genericParameter, Type/*!*/ interfaceType, Type inputType, Dictionary<Type, Type>/*!*/ binding) {
            Debug.Assert(interfaceType.IsInterface);

            Type[] interfaces = inputType.GetInterfaces();
            Type match = null;
            Type genTypeDef = interfaceType.GetGenericTypeDefinition();
            foreach (Type ifaceType in interfaces) {
                if (ifaceType.IsGenericType && ifaceType.GetGenericTypeDefinition() == genTypeDef) {
                    if (!MatchGenericParameter(genericParameter, ifaceType, interfaceType, binding, ref match)) {
                        return null;
                    }
                }
            }

            binding[genericParameter] = match;
            return match;
        }

        /// <summary>
        /// Checks if the constraints are violated by the given input for the specified generic method parameter.
        /// 
        /// This method must be supplied with a mapping for any dependent generic method type parameters which
        /// this one can be constrained to.  For example for the signature "void Foo{T0, T1}(T0 x, T1 y) where T0 : T1".
        /// we cannot know if the constraints are violated unless we know what we have calculated T1 to be.
        /// </summary>
        private static bool ConstraintsViolated(Type inputType, Type genericMethodParameterType, Dictionary<Type, Type> binding) {
            return ReflectionUtils.ConstraintsViolated(genericMethodParameterType, inputType, binding, false);
        }

        private static Type MatchGenericParameter(Type genericParameter, Type closedType, Type openType, Dictionary<Type, Type> binding) {
            Type match = null;
            return MatchGenericParameter(genericParameter, closedType, openType, binding, ref match) ? match : null;
        }

        /// <summary>
        /// Finds all occurences of <c>genericParameter</c> in <c>openType</c> and the corresponding concrete types in <c>closedType</c>.
        /// Returns true iff all occurences of the generic parameter in the open type correspond to the same concrete type in the closed type 
        /// and this type satisfies given <c>constraints</c>. Returns the concrete type in <c>match</c> if so.
        /// </summary>
        private static bool MatchGenericParameter(Type genericParameter, Type closedType, Type openType, Dictionary<Type, Type> binding, ref Type match) {
            Type m = match;

            bool result = ReflectionUtils.BindGenericParameters(openType, closedType, (parameter, type) => {
                if (parameter == genericParameter) {
                    if (m != null) {
                        return m == type;
                    }

                    if (ConstraintsViolated(type, genericParameter, binding)) {
                        return false;
                    }

                    m = type;
                }

                return true;
            });

            match = m;
            return result;
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
