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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO.IsolatedStorage;
using System.Reflection;
using Microsoft.Contracts;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    using Ast = Expression;

    /// <summary>
    /// MethodCandidate represents the different possible ways of calling a method or a set of method overloads.
    /// A single method can result in multiple MethodCandidates. Some reasons include:
    /// - Every optional parameter or parameter with a default value will result in a candidate
    /// - The presence of ref and out parameters will add a candidate for languages which want to return the updated values as return values.
    /// - ArgumentKind.List and ArgumentKind.Dictionary can result in a new candidate per invocation since the list might be different every time.
    ///
    /// Each MethodCandidate represents the parameter type for the candidate using ParameterWrapper.
    /// </summary>
    public sealed class MethodCandidate {
        private readonly OverloadResolver _resolver;
        private readonly OverloadInfo _overload;

        private readonly List<ParameterWrapper> _parameters;
        private readonly ParameterWrapper _paramsDict;
        private readonly int _paramsArrayIndex;

        private readonly IList<ArgBuilder> _argBuilders;
        private readonly InstanceBuilder _instanceBuilder;
        private readonly ReturnBuilder _returnBuilder;
        private readonly Dictionary<DynamicMetaObject, BindingRestrictions> _restrictions;

        internal MethodCandidate(OverloadResolver resolver, OverloadInfo method, List<ParameterWrapper> parameters, ParameterWrapper paramsDict,
            ReturnBuilder returnBuilder, InstanceBuilder instanceBuilder, IList<ArgBuilder> argBuilders, Dictionary<DynamicMetaObject, BindingRestrictions> restrictions) {

            Assert.NotNull(resolver, method, instanceBuilder, returnBuilder);
            Assert.NotNullItems(parameters);
            Assert.NotNullItems(argBuilders);

            _resolver = resolver;
            _overload = method;
            _instanceBuilder = instanceBuilder;
            _argBuilders = argBuilders;
            _returnBuilder = returnBuilder;
            _parameters = parameters;
            _paramsDict = paramsDict;
            _restrictions = restrictions;

            _paramsArrayIndex = ParameterWrapper.IndexOfParamsArray(parameters);

            parameters.TrimExcess();
        }

        internal MethodCandidate ReplaceMethod(OverloadInfo newMethod, List<ParameterWrapper> parameters, IList<ArgBuilder> argBuilders, Dictionary<DynamicMetaObject, BindingRestrictions> restrictions) {
            return new MethodCandidate(_resolver, newMethod, parameters, _paramsDict, _returnBuilder, _instanceBuilder, argBuilders, restrictions);
        }

        internal ReturnBuilder ReturnBuilder {
            get { return _returnBuilder; }
        }

        internal IList<ArgBuilder> ArgBuilders {
            get { return _argBuilders; }
        }

        public OverloadResolver Resolver {
            get { return _resolver; }
        }

        [Obsolete("Use Overload instead")]
        public MethodBase Method {
            get { return _overload.ReflectionInfo; }
        }

        public OverloadInfo Overload {
            get { return _overload; }
        }

        internal Dictionary<DynamicMetaObject, BindingRestrictions> Restrictions {
            get { return _restrictions; }
        }

        public Type ReturnType {
            get { return _returnBuilder.ReturnType; }
        }

        public int ParamsArrayIndex {
            get { return _paramsArrayIndex; }
        }

        public bool HasParamsArray {
            get { return _paramsArrayIndex != -1; }
        }

        public bool HasParamsDictionary {
            get { return _paramsDict != null; }
        }

        public ActionBinder Binder {
            get { return _resolver.Binder; }
        }

        internal ParameterWrapper GetParameter(int argumentIndex, ArgumentBinding namesBinding) {
            return _parameters[namesBinding.ArgumentToParameter(argumentIndex)];
        }

        internal ParameterWrapper GetParameter(int parameterIndex) {
            return _parameters[parameterIndex];
        }

        internal int ParameterCount {
            get { return _parameters.Count; }
        }

        internal int IndexOfParameter(string name) {
            for (int i = 0; i < _parameters.Count; i++) {
                if (_parameters[i].Name == name) {
                    return i;
                }
            }
            return -1;
        }

        public int GetVisibleParameterCount() {
            int result = 0;
            foreach (var parameter in _parameters) {
                if (!parameter.IsHidden) {
                    result++;
                }
            }
            return result;
        }

        public IList<ParameterWrapper> GetParameters() {
            return new ReadOnlyCollection<ParameterWrapper>(_parameters);
        }

        /// <summary>
        /// Builds a new MethodCandidate which takes count arguments and the provided list of keyword arguments.
        /// 
        /// The basic idea here is to figure out which parameters map to params or a dictionary params and
        /// fill in those spots w/ extra ParameterWrapper's.  
        /// </summary>
        internal MethodCandidate MakeParamsExtended(int count, IList<string> names) {
            Debug.Assert(_overload.IsVariadic);

            List<ParameterWrapper> newParameters = new List<ParameterWrapper>(count);
            
            // keep track of which named args map to a real argument, and which ones
            // map to the params dictionary.
            List<string> unusedNames = new List<string>(names);
            List<int> unusedNameIndexes = new List<int>();
            for (int i = 0; i < unusedNames.Count; i++) {
                unusedNameIndexes.Add(i);
            }

            // if we don't have a param array we'll have a param dict which is type object
            ParameterWrapper paramsArrayParameter = null;
            int paramsArrayIndex = -1;

            for (int i = 0; i < _parameters.Count; i++) {
                ParameterWrapper parameter = _parameters[i];

                if (parameter.IsParamsArray) {
                    paramsArrayParameter = parameter;
                    paramsArrayIndex = i;
                } else {
                    int j = unusedNames.IndexOf(parameter.Name);
                    if (j != -1) {
                        unusedNames.RemoveAt(j);
                        unusedNameIndexes.RemoveAt(j);
                    }
                    newParameters.Add(parameter);
                }
            }

            if (paramsArrayIndex != -1) {
                ParameterWrapper expanded = paramsArrayParameter.Expand();
                while (newParameters.Count < (count - unusedNames.Count)) {
                    newParameters.Insert(System.Math.Min(paramsArrayIndex, newParameters.Count), expanded);
                }
            }

            if (_paramsDict != null) {
                var flags = (_overload.ProhibitsNullItems(_paramsDict.ParameterInfo.Position) ? ParameterBindingFlags.ProhibitNull : 0) |
                            (_paramsDict.IsHidden ? ParameterBindingFlags.IsHidden : 0);

                foreach (string name in unusedNames) {
                    newParameters.Add(new ParameterWrapper(_paramsDict.ParameterInfo, typeof(object), name, flags));
                }
            } else if (unusedNames.Count != 0) {
                // unbound kw args and no where to put them, can't call...
                // TODO: We could do better here because this results in an incorrect arg # error message.
                return null;
            }

            // if we have too many or too few args we also can't call
            if (count != newParameters.Count) {
                return null;
            }

            return MakeParamsExtended(unusedNames.ToArray(), unusedNameIndexes.ToArray(), newParameters);
        }

        private MethodCandidate MakeParamsExtended(string[] names, int[] nameIndices, List<ParameterWrapper> parameters) {
            Debug.Assert(Overload.IsVariadic);

            List<ArgBuilder> newArgBuilders = new List<ArgBuilder>(_argBuilders.Count);

            // current argument that we consume, initially skip this if we have it.
            int curArg = _overload.IsStatic ? 0 : 1;
            int kwIndex = -1;
            ArgBuilder paramsDictBuilder = null;

            foreach (ArgBuilder ab in _argBuilders) {
                // TODO: define a virtual method on ArgBuilder implementing this functionality:

                SimpleArgBuilder sab = ab as SimpleArgBuilder;
                if (sab != null) {
                    // we consume one or more incoming argument(s)
                    if (sab.IsParamsArray) {
                        // consume all the extra arguments
                        int paramsUsed = parameters.Count -
                            GetConsumedArguments() -
                            names.Length +
                            (_overload.IsStatic ? 1 : 0);

                        newArgBuilders.Add(new ParamsArgBuilder(
                            sab.ParameterInfo,
                            sab.Type.GetElementType(),
                            curArg,
                            paramsUsed
                        ));

                        curArg += paramsUsed;
                    } else if (sab.IsParamsDict) {
                        // consume all the kw arguments
                        kwIndex = newArgBuilders.Count;
                        paramsDictBuilder = sab;
                    } else {
                        // consume the argument, adjust its position:
                        newArgBuilders.Add(sab.MakeCopy(curArg++));
                    }
                } else if (ab is KeywordArgBuilder) {
                    newArgBuilders.Add(ab);
                    curArg++;
                } else {
                    // CodeContext, null, default, etc...  we don't consume an 
                    // actual incoming argument.
                    newArgBuilders.Add(ab);
                }
            }

            if (kwIndex != -1) {
                newArgBuilders.Insert(kwIndex, new ParamsDictArgBuilder(paramsDictBuilder.ParameterInfo, curArg, names, nameIndices));
            }

            return new MethodCandidate(_resolver, _overload, parameters, null, _returnBuilder, _instanceBuilder, newArgBuilders, null);
        }

        private int GetConsumedArguments() {
            int consuming = 0;
            foreach (ArgBuilder argb in _argBuilders) {
                SimpleArgBuilder sab = argb as SimpleArgBuilder;
                if (sab != null && !sab.IsParamsDict || argb is KeywordArgBuilder) {
                    consuming++;
                }
            }
            return consuming;
        }

        public Type[] GetParameterTypes() {
            List<Type> res = new List<Type>(_argBuilders.Count);
            for (int i = 0; i < _argBuilders.Count; i++) {
                Type t = _argBuilders[i].Type;
                if (t != null) {
                    res.Add(t);
                }
            }

            return res.ToArray();
        }

        #region MakeDelegate

        internal OptimizingCallDelegate MakeDelegate(RestrictedArguments restrictedArgs) {
            if (restrictedArgs.HasUntypedRestrictions) {
                return null;
            }

            MethodInfo mi = Overload.ReflectionInfo as MethodInfo;
            if (mi == null) {
                return null;
            }

            Type declType = mi.GetBaseDefinition().DeclaringType;
            if (IsRestrictedType(declType)) {
                // members of reflection are off limits via reflection in partial trust
                return null;
            }

            if (_returnBuilder.CountOutParams > 0) {
                return null;
            }

            Func<object[], object>[] builders = new Func<object[], object>[_argBuilders.Count];
            bool[] hasBeenUsed = new bool[restrictedArgs.Length];

            for (int i = 0; i < _argBuilders.Count; i++) {
                var builder = _argBuilders[i].ToDelegate(_resolver, restrictedArgs, hasBeenUsed);
                if (builder == null) {
                    return null;
                }

                builders[i] = builder;
            }

            if (_instanceBuilder.HasValue) {
                var instance = _instanceBuilder.ToDelegate(ref mi, _resolver, restrictedArgs, hasBeenUsed);
                return new Caller(mi, builders, instance).CallWithInstance;
            } else {
                return new Caller(mi, builders, null).Call;
            }
        }

        private static bool IsRestrictedType(Type declType) {
            return declType != null &&
                            declType.Assembly == typeof(string).Assembly &&
                            (declType.IsSubclassOf(typeof(MemberInfo)) || declType == typeof(IsolatedStorageFile));
        }

        private sealed class Caller {
            private readonly Func<object[], object>[] _argBuilders;
            private readonly Func<object[], object> _instanceBuilder;
            private readonly MethodInfo _mi;
            private CallInstruction _caller;
            private int _hitCount;

            public Caller(MethodInfo mi, Func<object[], object>[] argBuilders, Func<object[], object> instanceBuilder) {
                _mi = mi;
                _argBuilders = argBuilders;
                _instanceBuilder = instanceBuilder;
            }

            public object Call(object[] args, out bool shouldOptimize) {
                shouldOptimize = TrackUsage(args);

                try {
                    if (_caller != null) {
                        return _caller.Invoke(GetArguments(args));
                    }
                    return _mi.Invoke(null, GetArguments(args));
                } catch (TargetInvocationException tie) {
                    ExceptionHelpers.UpdateForRethrow(tie.InnerException);
                    throw tie.InnerException;
                }
            }

            public object CallWithInstance(object[] args, out bool shouldOptimize) {
                shouldOptimize = TrackUsage(args);

                try {
                    if (_caller != null) {
                        return _caller.InvokeInstance(_instanceBuilder(args), GetArguments(args));
                    }

                    return _mi.Invoke(_instanceBuilder(args), GetArguments(args));
                } catch (TargetInvocationException tie) {
                    ExceptionHelpers.UpdateForRethrow(tie.InnerException);
                    throw tie.InnerException;
                }
            }

            private object[] GetArguments(object[] args) {
                object[] finalArgs = new object[_argBuilders.Length];
                for (int i = 0; i < finalArgs.Length; i++) {
                    finalArgs[i] = _argBuilders[i](args);
                }
                return finalArgs;
            }

            private bool TrackUsage(object[] args) {
                bool shouldOptimize;
                _hitCount++;
                shouldOptimize = false;

                bool forceCaller = false;
                if (_hitCount <= 100 && _caller == null) {
                    foreach (object o in args) {
                        // can't pass Missing.Value via reflection, use a ReflectedCaller
                        if (o == Missing.Value) {
                            forceCaller = true;
                        }
                    }
                }

                if (_hitCount > 100) {
                    shouldOptimize = true;
                } else if ((_hitCount > 5 || forceCaller) && _caller == null) {
                    _caller = CallInstruction.Create(_mi, _mi.GetParameters());
                }
                return shouldOptimize;
            }
        }

        #endregion

        #region MakeExpression

        internal Expression MakeExpression(RestrictedArguments restrictedArgs) {
            bool[] usageMarkers;
            Expression[] spilledArgs;
            Expression[] callArgs = GetArgumentExpressions(restrictedArgs, out usageMarkers, out spilledArgs);

            Expression call;
            MethodBase mb = _overload.ReflectionInfo;

            // TODO: make MakeExpression virtual on OverloadInfo?
            if (mb == null) {
                throw new InvalidOperationException("Cannot generate an expression for an overload w/o MethodBase");
            }

            MethodInfo mi = mb as MethodInfo;
            if (mi != null) {
                Expression instance;
                if (mi.IsStatic) {
                    instance = null;
                } else {
                    Debug.Assert(mi != null);
                    instance = _instanceBuilder.ToExpression(ref mi, _resolver, restrictedArgs, usageMarkers);
                    Debug.Assert(instance != null, "Can't skip instance expression");
                }

                if (CompilerHelpers.IsVisible(mi)) {
                    call = AstUtils.SimpleCallHelper(instance, mi, callArgs);
                } else {
                    call = Ast.Call(
                        typeof(BinderOps).GetMethod("InvokeMethod"),
                        AstUtils.Constant(mi),
                        instance != null ? AstUtils.Convert(instance, typeof(object)) : AstUtils.Constant(null),
                        AstUtils.NewArrayHelper(typeof(object), callArgs)
                    );
                }
            } else {
                ConstructorInfo ci = (ConstructorInfo)mb;
                if (CompilerHelpers.IsVisible(ci)) {
                    call = AstUtils.SimpleNewHelper(ci, callArgs);
                } else {
                    call = Ast.Call(
                        typeof(BinderOps).GetMethod("InvokeConstructor"),
                        AstUtils.Constant(ci),
                        AstUtils.NewArrayHelper(typeof(object), callArgs)
                    );
                }
            }

            if (spilledArgs != null) {
                call = Expression.Block(spilledArgs.AddLast(call));
            }

            Expression ret = _returnBuilder.ToExpression(_resolver, _argBuilders, restrictedArgs, call);

            List<Expression> updates = null;
            for (int i = 0; i < _argBuilders.Count; i++) {
                Expression next = _argBuilders[i].UpdateFromReturn(_resolver, restrictedArgs);
                if (next != null) {
                    if (updates == null) {
                        updates = new List<Expression>();
                    }
                    updates.Add(next);
                }
            }

            if (updates != null) {
                if (ret.Type != typeof(void)) {
                    ParameterExpression temp = Ast.Variable(ret.Type, "$ret");
                    updates.Insert(0, Ast.Assign(temp, ret));
                    updates.Add(temp);
                    ret = Ast.Block(new[] { temp }, updates.ToArray());
                } else {
                    updates.Insert(0, ret);
                    ret = Ast.Block(typeof(void), updates.ToArray());
                }
            }

            if (_resolver.Temps != null) {
                ret = Ast.Block(_resolver.Temps, ret);
            }

            return ret;
        }

        private Expression[] GetArgumentExpressions(RestrictedArguments restrictedArgs, out bool[] usageMarkers, out Expression[] spilledArgs) {
            int minPriority = Int32.MaxValue;
            int maxPriority = Int32.MinValue;
            foreach (ArgBuilder ab in _argBuilders) {
                minPriority = System.Math.Min(minPriority, ab.Priority);
                maxPriority = System.Math.Max(maxPriority, ab.Priority);
            }

            var args = new Expression[_argBuilders.Count];
            Expression[] actualArgs = null;
            usageMarkers = new bool[restrictedArgs.Length];
            for (int priority = minPriority; priority <= maxPriority; priority++) {
                for (int i = 0; i < _argBuilders.Count; i++) {
                    if (_argBuilders[i].Priority == priority) {
                        args[i] = _argBuilders[i].ToExpression(_resolver, restrictedArgs, usageMarkers);

                        // see if this has a temp that needs to be passed as the actual argument
                        Expression byref = _argBuilders[i].ByRefArgument;
                        if (byref != null) {
                            if (actualArgs == null) {
                                actualArgs = new Expression[_argBuilders.Count];
                            }
                            actualArgs[i] = byref;
                        }
                    }
                }
            }

            if (actualArgs != null) {
                for (int i = 0; i < args.Length; i++) {
                    if (args[i] != null && actualArgs[i] == null) {
                        actualArgs[i] = _resolver.GetTemporary(args[i].Type, null);
                        args[i] = Expression.Assign(actualArgs[i], args[i]);
                    }
                }

                spilledArgs = RemoveNulls(args);
                return RemoveNulls(actualArgs);
            }

            spilledArgs = null;
            return RemoveNulls(args);
        }

        private static Expression[] RemoveNulls(Expression[] args) {
            int newLength = args.Length;
            for (int i = 0; i < args.Length; i++) {
                if (args[i] == null) {
                    newLength--;
                }
            }

            var result = new Expression[newLength];
            for (int i = 0, j = 0; i < args.Length; i++) {
                if (args[i] != null) {
                    result[j++] = args[i];
                }
            }
            return result;
        }

        #endregion

        [Confined]
        public override string ToString() {
            return string.Format("MethodCandidate({0} on {1})", Overload.ReflectionInfo, Overload.DeclaringType.FullName);
        }
    }
}
