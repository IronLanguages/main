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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Dynamic;
using Microsoft.Contracts;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions.Calls {
    /// <summary>
    /// Provides binding and overload resolution to .NET methods.
    /// 
    /// MethodBinder's can be used for:
    ///     generating new AST code for calling a method 
    ///     calling a method via reflection at runtime
    ///     (not implemented) performing an abstract call
    ///     
    /// MethodBinder's support default arguments, optional arguments, by-ref (in and out), and keyword arguments.
    /// 
    /// Implementation Details:
    /// 
    /// The MethodBinder works by building up a TargetSet for each number of effective arguments that can be
    /// passed to a set of overloads.  For example a set of overloads such as:
    ///     foo(object a, object b, object c)
    ///     foo(int a, int b)
    ///     
    /// would have 2 target sets - one for 3 parameters and one for 2 parameters.  For parameter arrays
    /// we fallback and create the appropriately sized TargetSet on demand.
    /// 
    /// Each TargetSet consists of a set of MethodCandidate's.  Each MethodCandidate knows the flattened
    /// parameters that could be received.  For example for a function such as:
    ///     foo(params int[] args)
    ///     
    /// When this method is in a TargetSet of size 3 the MethodCandidate takes 3 parameters - all of them
    /// ints; if it's in a TargetSet of size 4 it takes 4 parameters.  Effectively a MethodCandidate is 
    /// a simplified view that allows all arguments to be treated as required positional arguments.
    /// 
    /// Each MethodCandidate in turn refers to a MethodTarget.  The MethodTarget is composed of a set
    /// of ArgBuilder's and a ReturnBuilder which know how to consume the positional arguments and pass
    /// them to the appropriate argument of the destination method.  This includes routing keyword
    /// arguments to the correct position, providing the default values for optional arguments, etc...
    /// 
    /// After binding is finished the MethodCandidates are thrown away and a BindingTarget is returned. 
    /// The BindingTarget indicates whether the binding was successful and if not any additional information
    /// that should be reported to the user about the failed binding.  It also exposes the MethodTarget which
    /// allows consumers to get the flattened list of required parameters for the call.  MethodCandidates
    /// are not exposed and are an internal implementation detail of the MethodBinder.
    /// </summary>
    public sealed class MethodBinder {
        private readonly string _name;                           // the name of the method (possibly language specific name which isn't the same as the method base)
        private readonly Dictionary<int, TargetSet> _targetSets; // the methods as they map from # of arguments -> the possible TargetSet's.
        private readonly string[] _kwArgs;                     // the names of the keyword arguments being provided
        private readonly NarrowingLevel _minLevel, _maxLevel;         // specifies the minimum and maximum narrowing levels for conversions during binding
        internal readonly DefaultBinder _binder;                      // the ActionBinder which is being used for conversions
        private List<MethodCandidate> _paramsCandidates;              // the methods which are params methods which need special treatment because they don't have fixed # of args

        #region Constructors

        private MethodBinder(ActionBinder binder, string name, IList<MethodBase> methods, string[] kwArgs, NarrowingLevel minLevel, NarrowingLevel maxLevel) {
            ContractUtils.RequiresNotNull(binder, "binder");
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNullItems(methods, "methods");
            ContractUtils.RequiresNotNullItems(kwArgs, "kwArgs");

            _binder = binder as DefaultBinder;
            if (_binder == null) {
                throw new InvalidOperationException("MethodBinder requires an instance of DefaultBinder");
            }
            _name = name;
            _kwArgs = kwArgs;
            _targetSets = new Dictionary<int, TargetSet>(methods.Count);
            _minLevel = minLevel;
            _maxLevel = maxLevel;

            foreach (MethodBase method in methods) {
                if (IsUnsupported(method)) continue;

                AddBasicMethodTargets(binder, method);
            }

            if (_paramsCandidates != null) {
                // For all the methods that take a params array, create MethodCandidates that clash with the 
                // other overloads of the method
                foreach (MethodCandidate maker in _paramsCandidates) {
                    foreach (int count in _targetSets.Keys) {
                        MethodCandidate target = maker.MakeParamsExtended(binder, count, _kwArgs);
                        if (target != null) AddTarget(target);
                    }
                }
            }
        }

        #endregion

        #region Public APIs

        /// <summary>
        /// Creates a new MethodBinder for binding to the specified methods that will attempt to bind
        /// at all defined NarrowingLevels.
        /// 
        /// The provided ActionBinder is used for determining overload resolution.
        /// </summary>
        public static MethodBinder MakeBinder(ActionBinder binder, string name, IList<MethodBase> mis) {
            return new MethodBinder(binder, name, mis, ArrayUtils.EmptyStrings, NarrowingLevel.None, NarrowingLevel.All);
        }

        /// <summary>
        /// Creates a new MethodBinder for binding to the specified methods on a call which includes keyword arguments that
        /// will attempt to bind at all defined NarrowingLevels.
        /// 
        /// The provided ActionBinder is used for determining overload resolution.
        /// </summary>
        public static MethodBinder MakeBinder(ActionBinder binder, string name, IList<MethodBase> mis, string[] keywordArgs) {
            return new MethodBinder(binder, name, mis, keywordArgs, NarrowingLevel.None, NarrowingLevel.All);
        }

        /// <summary>
        /// Creates a new MethodBinder for binding to the specified methods this will attempt to bind at 
        /// the specified NarrowingLevels.
        /// 
        /// The provided ActionBinder is used for determining overload resolution.
        /// </summary>
        public static MethodBinder MakeBinder(ActionBinder binder, string name, IList<MethodBase> mis, NarrowingLevel minLevel, NarrowingLevel maxLevel) {
            return new MethodBinder(binder, name, mis, ArrayUtils.EmptyStrings, minLevel, maxLevel);
        }

        /// <summary>
        /// Creates a new MethodBinder for binding to the specified methods on a call which includes keyword arguments that
        /// will attempt to bind at the specified NarrowingLevels.
        /// 
        /// The provided ActionBinder is used for determining overload resolution.
        /// </summary>
        public static MethodBinder MakeBinder(ActionBinder binder, string name, IList<MethodBase> mis, string[] keywordArgs, NarrowingLevel minLevel, NarrowingLevel maxLevel) {
            return new MethodBinder(binder, name, mis, keywordArgs, minLevel, maxLevel);
        }

        /// <summary>
        /// Creates a BindingTarget given the specified CallType and parameter types.
        /// 
        /// The BindingTarget can then be tested for the success or particular type of
        /// failure that prevents the method from being called.  The BindingTarget can
        /// also be called reflectively at runtime, create an Expression for embedding in
        /// a RuleBuilder, or be used for performing an abstract call.
        /// </summary>
        public BindingTarget MakeBindingTarget(CallTypes callType, Type[] types) {
            ContractUtils.RequiresNotNull(types, "types");
            ContractUtils.RequiresNotNullItems(types, "types");

            TargetSet ts = GetTargetSet(types.Length);
            if (ts != null && !ts.IsParamsDictionaryOnly()) {
                return ts.MakeBindingTarget(callType, types, _kwArgs, _minLevel, _maxLevel);
            }

            // no target set is applicable, report an error and the expected # of arguments.
            int[] expectedArgs = new int[_targetSets.Count + (_paramsCandidates != null ? 1 : 0)];
            int i = 0;
            foreach (KeyValuePair<int, TargetSet> kvp in _targetSets) {
                int count = kvp.Key;

                foreach (MethodCandidate cand in kvp.Value._targets) {
                    foreach (var x in cand.Parameters) {
                        if (x.IsParamsArray || x.IsParamsDict) {
                            count--;
                        }
                    }
                }

                if (callType == CallTypes.ImplicitInstance) {
                    foreach (MethodCandidate cand in kvp.Value._targets) {
                        if (IsInstanceMethod(cand)) {
                            // dispatch includes an instance method, bump
                            // one parameter off.
                            count--;
                            break;
                        }
                    }                    
                }

                
                expectedArgs[i++] = count;
            }
            if (_paramsCandidates != null) {
                expectedArgs[expectedArgs.Length - 1] = Int32.MaxValue;
            }

            return new BindingTarget(Name, callType == CallTypes.None ? types.Length : types.Length - 1, expectedArgs);
        }

        /// <summary>
        /// Creates a BindingTarget given the specified CallType and parameter types.
        /// 
        /// The BindingTarget can then be tested for the success or particular type of
        /// failure that prevents the method from being called.  The BindingTarget can
        /// also be called reflectively at runtime, create an Expression for embedding in
        /// a RuleBuilder, or be used for performing an abstract call.
        /// </summary>
        public BindingTarget MakeBindingTarget(CallTypes callType, DynamicMetaObject[] metaObjects) {
            ContractUtils.RequiresNotNull(metaObjects, "types");
            ContractUtils.RequiresNotNullItems(metaObjects, "types");

            TargetSet ts = GetTargetSet(metaObjects.Length);
            if (ts != null && !ts.IsParamsDictionaryOnly()) {
                return ts.MakeBindingTarget(callType, metaObjects, _kwArgs, _minLevel, _maxLevel);
            }

            // no target set is applicable, report an error and the expected # of arguments.
            int[] expectedArgs = new int[_targetSets.Count + (_paramsCandidates != null ? 1 : 0)];
            int i = 0;
            foreach (KeyValuePair<int, TargetSet> kvp in _targetSets) {
                int count = kvp.Key;
                foreach (MethodCandidate cand in kvp.Value._targets) {
                    foreach (var x in cand.Parameters) {
                        if (x.IsParamsArray || x.IsParamsDict) {
                            count--;
                        }
                    }
                }

                if (callType == CallTypes.ImplicitInstance) {
                    foreach (MethodCandidate cand in kvp.Value._targets) {
                        if (IsInstanceMethod(cand)) {
                            // dispatch includes an instance method, bump
                            // one parameter off.
                            count--;
                            break;
                        }

                    }
                }
                expectedArgs[i++] = count;
            }
            if (_paramsCandidates != null) {
                expectedArgs[expectedArgs.Length - 1] = Int32.MaxValue;
            }

            return new BindingTarget(Name, callType == CallTypes.None ? metaObjects.Length : metaObjects.Length - 1, expectedArgs);
        }

        private static bool IsInstanceMethod(MethodCandidate cand) {
            return !CompilerHelpers.IsStatic(cand.Target.Method) ||
                                        (cand.Target.Method.IsDefined(typeof(ExtensionAttribute), false));
        }

        /// <summary>
        /// Gets the name of the MethodBinder as provided at construction time.
        /// 
        /// The name may differ from the name of the underlying method bases if the
        /// language provides some mapping from .NET method names to language specific
        /// method names.  It is flowed through the MethodBinder primarily for error
        /// reporting purposes.
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        [Confined]
        public override string ToString() {
            string res = "";
            foreach (TargetSet ts in _targetSets.Values) {
                res += ts + Environment.NewLine;
            }
            return res;
        }

        #endregion

        #region TargetSet construction

        private TargetSet GetTargetSet(int nargs) {
            TargetSet ts;

            // see if we've precomputed the TargetSet...
            if (_targetSets.TryGetValue(nargs, out ts)) {
                return ts;
            } else if (_paramsCandidates != null) {
                // build a new target set specific to the number of
                // arguments we have
                ts = BuildTargetSet(nargs);
                if (ts._targets.Count > 0) {
                    return ts;
                }
            }

            return null;
        }

        private TargetSet BuildTargetSet(int count) {
            TargetSet ts = new TargetSet(this, count);
            if (_paramsCandidates != null) {
                foreach (MethodCandidate maker in _paramsCandidates) {
                    MethodCandidate target = maker.MakeParamsExtended(_binder, count, _kwArgs);
                    if (target != null) ts.Add(target);
                }
            }

            return ts;
        }

        private void AddTarget(MethodCandidate target) {
            int count = target.Target.ParameterCount;
            TargetSet set;
            if (!_targetSets.TryGetValue(count, out set)) {
                set = new TargetSet(this, count);
                _targetSets[count] = set;
            }
            set.Add(target);
        }

        private void AddSimpleTarget(MethodCandidate target) {
            AddTarget(target);
            if (BinderHelpers.IsParamsMethod(target.Target.Method)) {
                if (_paramsCandidates == null) _paramsCandidates = new List<MethodCandidate>();
                _paramsCandidates.Add(target);
            }
        }

        private static ArgBuilder MakeInstanceBuilder(ActionBinder binder, MethodBase method, List<ParameterWrapper> parameters, ref int argIndex) {
            if (!CompilerHelpers.IsStatic(method)) {
                parameters.Add(new ParameterWrapper(binder, method.DeclaringType, null, true));
                return new SimpleArgBuilder(method.DeclaringType, argIndex++, false, false);
            } else {
                return new NullArgBuilder();
            }
        }

        private void AddBasicMethodTargets(ActionBinder binder, MethodBase method) {
            Assert.NotNull(binder, method);

            var parameterInfos = method.GetParameters();
            var parameters = new List<ParameterWrapper>();
            var arguments = new List<ArgBuilder>(parameterInfos.Length);
            var defaultArguments = new List<ArgBuilder>();
            int argIndex = 0;
            var instanceBuilder = MakeInstanceBuilder(binder, method, parameters, ref argIndex);

            bool hasByRefOrOut = false;
            bool hasDefaults = false;

            var infoIndex = binder.PrepareParametersBinding(parameterInfos, arguments, parameters, ref argIndex);
            for (; infoIndex < parameterInfos.Length; infoIndex++) {
                var pi = parameterInfos[infoIndex];

                if (binder.BindSpecialParameter(pi, arguments, parameters, ref argIndex)) {
                    continue;
                }

                int indexForArgBuilder, kwIndex = GetKeywordIndex(pi);
                if (kwIndex == ParameterNotPassedByKeyword) {
                    // positional argument, we simply consume the next argument
                    indexForArgBuilder = argIndex++;
                } else {
                    // keyword argument, we just tell the simple arg builder to consume arg 0.
                    // KeywordArgBuilder will then pass in the correct single argument based 
                    // upon the actual argument number provided by the user.
                    indexForArgBuilder = 0;
                }

                // if the parameter is default we need to build a default arg builder and then
                // build a reduced method at the end.  
                if (!CompilerHelpers.IsMandatoryParameter(pi)) {
                    // We need to build the default builder even if we have a parameter for it already to
                    // get good consistency of our error messages.  But consider a method like 
                    // def foo(a=1, b=2) and the user calls it as foo(b=3). Then adding the default
                    // value breaks an otherwise valid call.  This is because we only generate MethodCandidates
                    // filling in the defaults from right to left (so the method - 1 arg requires a,
                    // and the method minus 2 args requires b).  So we only add the default if it's 
                    // a positional arg or we don't already have a default value.
                    if (kwIndex == -1 || !hasDefaults) {
                        defaultArguments.Add(new DefaultArgBuilder(pi));
                        hasDefaults = true;
                    } else {
                        defaultArguments.Add(null);
                    }
                } else if (defaultArguments.Count > 0) {
                    // non-contigious default parameter
                    defaultArguments.Add(null);
                }

                ArgBuilder ab;
                if (pi.ParameterType.IsByRef) {
                    hasByRefOrOut = true;
                    Type refType = typeof(StrongBox<>).MakeGenericType(pi.ParameterType.GetElementType());
                    var param = new ParameterWrapper(_binder, pi, refType, pi.Name, true, false, false);
                    parameters.Add(param);
                    ab = new ReferenceArgBuilder(pi, refType, indexForArgBuilder);
                } else {
                    hasByRefOrOut |= CompilerHelpers.IsOutParameter(pi);
                    var param = new ParameterWrapper(_binder, pi);
                    parameters.Add(param);
                    ab = new SimpleArgBuilder(pi, indexForArgBuilder);
                }

                if (kwIndex == ParameterNotPassedByKeyword) {
                    arguments.Add(ab);
                } else {
                    Debug.Assert(KeywordArgBuilder.BuilderExpectsSingleParameter(ab));
                    arguments.Add(new KeywordArgBuilder(ab, _kwArgs.Length, kwIndex));
                }
            }

            ReturnBuilder returnBuilder = MakeKeywordReturnBuilder(
                new ReturnBuilder(CompilerHelpers.GetReturnType(method)),
                parameterInfos,
                parameters,
                _binder.AllowKeywordArgumentSetting(method));

            if (hasDefaults) {
                for (int defaultsUsed = 1; defaultsUsed < defaultArguments.Count + 1; defaultsUsed++) {
                    // if the left most default we'll use is not present then don't add a default.  This happens in cases such as:
                    // a(a=1, b=2, c=3) and then call with a(a=5, c=3).  We'll come through once for c (no default, skip),
                    // once for b (default present, emit) and then a (no default, skip again).  W/o skipping we'd generate the same
                    // method multiple times.  This also happens w/ non-contigious default values, e.g. foo(a, b=3, c) where we don't want
                    // to generate a default candidate for just c which matches the normal method.
                    if (defaultArguments[defaultArguments.Count - defaultsUsed] != null) {
                        AddSimpleTarget(MakeDefaultCandidate(
                            method,
                            parameters,
                            instanceBuilder,
                            arguments,
                            defaultArguments,
                            returnBuilder,
                            defaultsUsed));
                    }
                }
            }

            if (hasByRefOrOut) {
                AddSimpleTarget(MakeByRefReducedMethodTarget(binder, parameterInfos, method));
            }

            AddSimpleTarget(MakeMethodCandidate(method, parameters, instanceBuilder, arguments, returnBuilder));
        }

        private MethodCandidate MakeDefaultCandidate(MethodBase method, List<ParameterWrapper> parameters, ArgBuilder instanceBuilder, List<ArgBuilder> argBuilders, List<ArgBuilder> defaultBuilders, ReturnBuilder returnBuilder, int defaultsUsed) {
            List<ArgBuilder> defaultArgBuilders = new List<ArgBuilder>(argBuilders);
            List<ParameterWrapper> necessaryParams = parameters.GetRange(0, parameters.Count - defaultsUsed);

            for (int curDefault = 0; curDefault < defaultsUsed; curDefault++) {
                int readIndex = defaultBuilders.Count - defaultsUsed + curDefault;
                int writeIndex = defaultArgBuilders.Count - defaultsUsed + curDefault;

                if (defaultBuilders[readIndex] != null) {
                    defaultArgBuilders[writeIndex] = defaultBuilders[readIndex];
                } else {
                    necessaryParams.Add(parameters[parameters.Count - defaultsUsed + curDefault]);
                }
            }

            // shift any arguments forward that need to be...
            int curArg = CompilerHelpers.IsStatic(method) ? 0 : 1;
            for (int i = 0; i < defaultArgBuilders.Count; i++) {
                SimpleArgBuilder sab = defaultArgBuilders[i] as SimpleArgBuilder;
                if (sab != null) {
                    defaultArgBuilders[i] = sab.MakeCopy(curArg++);
                }
            }

            return MakeMethodCandidate(method, necessaryParams, instanceBuilder, defaultArgBuilders, returnBuilder);
        }

        private MethodCandidate MakeByRefReducedMethodTarget(ActionBinder binder, ParameterInfo[] parameterInfos, MethodBase method) {
            Assert.NotNull(binder, parameterInfos, method);

            var parameters = new List<ParameterWrapper>();
            var arguments = new List<ArgBuilder>();
            int argIndex = 0;
            var instanceBuilder = MakeInstanceBuilder(binder, method, parameters, ref argIndex);            
            
            List<int> returnArgs = new List<int>();
            if (CompilerHelpers.GetReturnType(method) != typeof(void)) {
                returnArgs.Add(-1);
            }

            var infoIndex = binder.PrepareParametersBinding(parameterInfos, arguments, parameters, ref argIndex);
            for (; infoIndex < parameterInfos.Length; infoIndex++) {
                var pi = parameterInfos[infoIndex];

                if (binder.BindSpecialParameter(pi, arguments, parameters, ref argIndex)) {
                    continue;
                }
                
                // See KeywordArgBuilder.BuilderExpectsSingleParameter
                int indexForArgBuilder = 0;

                int kwIndex = ParameterNotPassedByKeyword;
                if (!CompilerHelpers.IsOutParameter(pi)) {
                    kwIndex = GetKeywordIndex(pi);
                    if (kwIndex == ParameterNotPassedByKeyword) {
                        indexForArgBuilder = argIndex++;
                    }
                }

                ArgBuilder ab;
                if (CompilerHelpers.IsOutParameter(pi)) {
                    returnArgs.Add(arguments.Count);
                    ab = new OutArgBuilder(pi);
                } else if (pi.ParameterType.IsByRef) {
                    // if the parameter is marked as [In] it is not returned.
                    if ((pi.Attributes & (ParameterAttributes.In | ParameterAttributes.Out)) != ParameterAttributes.In) {
                        returnArgs.Add(arguments.Count);
                    }
                    ParameterWrapper param = new ParameterWrapper(_binder, pi, pi.ParameterType.GetElementType(), pi.Name, false, false, false);
                    parameters.Add(param);
                    ab = new ReturnReferenceArgBuilder(pi, indexForArgBuilder);
                } else {
                    ParameterWrapper param = new ParameterWrapper(_binder, pi);
                    parameters.Add(param);
                    ab = new SimpleArgBuilder(pi, indexForArgBuilder);
                }

                if (kwIndex == ParameterNotPassedByKeyword) {
                    arguments.Add(ab);
                } else {
                    Debug.Assert(KeywordArgBuilder.BuilderExpectsSingleParameter(ab));
                    arguments.Add(new KeywordArgBuilder(ab, _kwArgs.Length, kwIndex));
                }
            }

            ReturnBuilder returnBuilder = MakeKeywordReturnBuilder(
                new ByRefReturnBuilder(returnArgs),
                parameterInfos,
                parameters,
                _binder.AllowKeywordArgumentSetting(method));

            return MakeMethodCandidate(method, parameters, instanceBuilder, arguments, returnBuilder);
        }

        private MethodCandidate MakeMethodCandidate(MethodBase method, List<ParameterWrapper> parameters, ArgBuilder instanceBuilder, List<ArgBuilder> argBuilders, ReturnBuilder returnBuilder) {
            return new MethodCandidate(
                new MethodTarget(this, method, parameters.Count, instanceBuilder, argBuilders, returnBuilder),
                parameters);
        }

        private void GetMinAndMaxArgs(out int minArgs, out int maxArgs) {
            List<int> argCounts = new List<int>(_targetSets.Keys);
            argCounts.Sort();
            minArgs = argCounts[0];
            maxArgs = argCounts[argCounts.Count - 1];
        }

        private static bool IsUnsupported(MethodBase method) {
            return (method.CallingConvention & CallingConventions.VarArgs) != 0 || method.ContainsGenericParameters;
        }

        #endregion

        #region Keyword arg binding support

        private ReturnBuilder MakeKeywordReturnBuilder(ReturnBuilder returnBuilder, ParameterInfo[] methodParams, List<ParameterWrapper> parameters, bool isConstructor) {
            if (isConstructor) {
                List<string> unusedNames = GetUnusedKeywordParameters(methodParams);
                List<MemberInfo> bindableMembers = GetBindableMembers(returnBuilder, unusedNames);
                List<int> kwArgIndexs = new List<int>();
                if (unusedNames.Count == bindableMembers.Count) {

                    foreach (MemberInfo mi in bindableMembers) {
                        ParameterWrapper pw = new ParameterWrapper(
                            _binder,
                            mi.MemberType == MemberTypes.Property ? 
                                ((PropertyInfo)mi).PropertyType :
                                ((FieldInfo)mi).FieldType,
                            mi.Name,
                            false);

                        parameters.Add(pw);
                        kwArgIndexs.Add(GetKeywordIndex(mi.Name));
                    }

                    KeywordConstructorReturnBuilder kwBuilder = new KeywordConstructorReturnBuilder(returnBuilder,
                        _kwArgs.Length,
                        kwArgIndexs.ToArray(),
                        bindableMembers.ToArray(),
                        _binder.PrivateBinding);

                    return kwBuilder;
                }

            }
            return returnBuilder;
        }

        private static List<MemberInfo> GetBindableMembers(ReturnBuilder returnBuilder, List<string> unusedNames) {
            List<MemberInfo> bindableMembers = new List<MemberInfo>();

            foreach (string name in unusedNames) {
                Type curType = returnBuilder.ReturnType;
                MemberInfo[] mis = curType.GetMember(name);
                while (mis.Length != 1 && curType != null) {
                    // see if we have a single member defined as the closest level
                    mis = curType.GetMember(name, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.Instance);

                    if (mis.Length > 1) {
                        break;
                    }

                    curType = curType.BaseType;
                }

                if (mis.Length == 1) {
                    switch (mis[0].MemberType) {
                        case MemberTypes.Property:
                        case MemberTypes.Field:
                            bindableMembers.Add(mis[0]);
                            break;
                    }
                }
            }
            return bindableMembers;
        }

        private List<string> GetUnusedKeywordParameters(ParameterInfo[] methodParams) {
            List<string> unusedNames = new List<string>();
            foreach (string name in _kwArgs) {
                bool found = false;
                foreach (ParameterInfo pi in methodParams) {
                    if (pi.Name == name) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    unusedNames.Add(name);
                }
            }
            return unusedNames;
        }

        private const int ParameterNotPassedByKeyword = -1;

        // Check if the given parameter from the candidate's signature matches a keyword argument from the callsite.
        // Return ParameterNotPassedByKeyword if no match. Else returns index into callsite's keyword arglist if there is a match.
        private int GetKeywordIndex(ParameterInfo pi) {
            return GetKeywordIndex(pi.Name);
        }

        private int GetKeywordIndex(string kwName) {
            for (int i = 0; i < _kwArgs.Length; i++) {
                if (kwName == _kwArgs[i]) {
                    return i;
                }
            }
            return ParameterNotPassedByKeyword;
        }

        #endregion

        #region TargetSet

        /// <summary>
        /// Represents a collection of MethodCandidate's which all accept the
        /// same number of logical parameters.  For example a params method
        /// and a method with 3 parameters would both be a TargetSet for 3 parameters.
        /// </summary>
        internal class TargetSet {
            private MethodBinder _binder;
            private int _count;
            internal List<MethodCandidate> _targets;

            internal TargetSet(MethodBinder binder, int count) {
                _count = count;
                _targets = new List<MethodCandidate>();
                _binder = binder;
            }

            internal bool IsParamsDictionaryOnly() {
                foreach (MethodCandidate target in _targets) {
                    if (!target.HasParamsDictionary()) {
                        return false;
                    }
                }
                return true;
            }

            internal BindingTarget MakeBindingTarget(CallTypes callType, Type[] types, string[] names, NarrowingLevel minLevel, NarrowingLevel maxLevel) {
                List<ConversionResult> lastFail = new List<ConversionResult>();
                List<CallFailure> failures = null;

                // go through all available narrowing levels selecting candidates.  
                for (NarrowingLevel level = minLevel; level <= maxLevel; level++) {
                    List<MethodCandidate> applicableTargets = new List<MethodCandidate>();
                    if (failures != null) {
                        failures.Clear();
                    }

                    foreach (MethodCandidate target in _targets) {
                        // skip params dictionaries - we want to only pick up the methods normalized
                        // to have argument names (which we created because the MethodBinder gets 
                        // created w/ keyword arguments).
                        if (!target.HasParamsDictionary()) {
                            Type[] normalizedTypes;
                            CallFailure callFailure;

                            if (!target.TryGetNormalizedArguments(types, names, out normalizedTypes, out callFailure)) {
                                // dup keyword arguments or unassigned keyword argument
                                if (failures == null) failures = new List<CallFailure>(1);
                                failures.Add(callFailure);
                            } else if (target.IsApplicable(normalizedTypes, level, lastFail)) {
                                // success, remember the candidate...
                                applicableTargets.Add(GetCandidate(target, level));
                            } else {
                                // conversion failure, remember the failures...
                                if (failures == null) failures = new List<CallFailure>(1);

                                failures.Add(new CallFailure(target.Target, lastFail.ToArray()));
                                lastFail.Clear();
                            }
                        }
                    }

                    // see if we managed to get a single method or if one method is better...
                    List<MethodCandidate> result;
                    if (TryGetApplicableTarget(callType, applicableTargets, types, out result)) {
                        if (result.Count == 1) {
                            // only a single method is callable, success!
                            return MakeSuccessfulBindingTarget(callType, types, result);
                        }

                        // more than one method found, no clear best method, report an ambigious match
                        return MakeAmbiguousBindingTarget(callType, types, result);
                    }
                }

                Debug.Assert(failures != null);
                return new BindingTarget(_binder.Name, callType == CallTypes.None ? types.Length : types.Length - 1, failures.ToArray());
            }

            internal BindingTarget MakeBindingTarget(CallTypes callType, DynamicMetaObject[] metaObjects, string[] names, NarrowingLevel minLevel, NarrowingLevel maxLevel) {
                List<ConversionResult> lastFail = new List<ConversionResult>();
                List<CallFailure> failures = null;

                // go through all available narrowing levels selecting candidates.  
                for (NarrowingLevel level = minLevel; level <= maxLevel; level++) {
                    List<MethodCandidate> applicableTargets = new List<MethodCandidate>();
                    if (failures != null) {
                        failures.Clear();
                    }

                    foreach (MethodCandidate target in _targets) {
                        // skip params dictionaries - we want to only pick up the methods normalized
                        // to have argument names (which we created because the MethodBinder gets 
                        // created w/ keyword arguments).
                        if (!target.HasParamsDictionary()) {
                            DynamicMetaObject[] normalizedObjects;
                            CallFailure callFailure;

                            if (!target.TryGetNormalizedArguments(metaObjects, names, out normalizedObjects, out callFailure)) {
                                // dup keyword arguments or unassigned keyword argument
                                if (failures == null) failures = new List<CallFailure>(1);
                                failures.Add(callFailure);
                            } else if (target.IsApplicable(normalizedObjects, level, lastFail)) {
                                // success, remember the candidate...
                                applicableTargets.Add(GetCandidate(target, level));
                            } else {
                                // conversion failure, remember the failures...
                                if (failures == null) failures = new List<CallFailure>(1);

                                failures.Add(new CallFailure(target.Target, lastFail.ToArray()));
                                lastFail.Clear();
                            }
                        }
                    }

                    // see if we managed to get a single method or if one method is better...
                    List<MethodCandidate> result;
                    if (TryGetApplicableTarget(callType, applicableTargets, metaObjects, out result)) {
                        if (result.Count == 1) {
                            // only a single method is callable, success!
                            return MakeSuccessfulBindingTarget(callType, metaObjects, result);
                        }

                        // more than one method found, no clear best method, report an ambigious match
                        return MakeAmbiguousBindingTarget(callType, metaObjects, result);
                    }
                }

                Debug.Assert(failures != null);
                return new BindingTarget(_binder.Name, callType == CallTypes.None ? metaObjects.Length : metaObjects.Length - 1, failures.ToArray());
            }

            private static bool TryGetApplicableTarget(CallTypes callType, List<MethodCandidate> applicableTargets, Type[] actualTypes, out List<MethodCandidate> result) {
                result = null;
                if (applicableTargets.Count == 1) {
                    result = applicableTargets;
                    return true;
                }
                if (applicableTargets.Count > 1) {
                    MethodCandidate target = FindBest(callType, applicableTargets, actualTypes);
                    if (target != null) {
                        result = new List<MethodCandidate>(new MethodCandidate[] { target });
                        return true;
                    } else {
                        result = applicableTargets;
                        return true;
                    }
                }
                return false;
            }

            private static bool TryGetApplicableTarget(CallTypes callType, List<MethodCandidate> applicableTargets, DynamicMetaObject[] actualTypes, out List<MethodCandidate> result) {
                result = null;
                if (applicableTargets.Count == 1) {
                    result = applicableTargets;
                    return true;
                }
                if (applicableTargets.Count > 1) {
                    MethodCandidate target = FindBest(callType, applicableTargets, actualTypes);
                    if (target != null) {
                        result = new List<MethodCandidate>(new MethodCandidate[] { target });
                        return true;
                    } else {
                        result = applicableTargets;
                        return true;
                    }
                }
                return false;
            }

            private Type[] GetTypesForTest(Type[] types, IList<MethodCandidate> candidates) {
                // if we have a single target we need no tests.
                if (_targets.Count == 1) return null;

                Type[] tests = new Type[types.Length];
                for (int i = 0; i < types.Length; i++) {
                    if (AreArgumentTypesOverloaded(i, types.Length, candidates)) {
                        tests[i] = types[i];
                    }
                }

                return tests;
            }

            private DynamicMetaObject[] GetRestrictedMetaObjects(MethodCandidate target, DynamicMetaObject[] objects, IList<MethodCandidate> candidates) {
                IList<ParameterWrapper> parameters = target.Parameters;

                Debug.Assert(parameters.Count == objects.Length);

                DynamicMetaObject[] resObjects = new DynamicMetaObject[objects.Length];
                for (int i = 0; i < objects.Length; i++) {
                    if (_targets.Count > 0 && AreArgumentTypesOverloaded(i, objects.Length, candidates)) {                                                
                        resObjects[i] = RestrictOne(objects[i], parameters[i]);
                    } else if (parameters[i].Type.IsAssignableFrom(objects[i].Expression.Type)) {
                        // we have a strong enough type already
                        resObjects[i] = objects[i];
                    } else {
                        resObjects[i] = RestrictOne(objects[i], parameters[i]);
                    }
                }

                return resObjects;
            }

            private DynamicMetaObject RestrictOne(DynamicMetaObject obj, ParameterWrapper forParam) {
                if (forParam.Type == typeof(object)) {
                    // don't use Restrict as it'll box & unbox.
                    return new DynamicMetaObject(obj.Expression, BindingRestrictionsHelpers.GetRuntimeTypeRestriction(obj.Expression, obj.GetLimitType()));
                } else {
                    return obj.Restrict(obj.GetLimitType());
                }
            }

            private static bool AreArgumentTypesOverloaded(int argIndex, int argCount, IList<MethodCandidate> methods) {
                Type argType = null;
                for (int i = 0; i < methods.Count; i++) {
                    IList<ParameterWrapper> pis = methods[i].Parameters;
                    if (pis.Count == 0) continue;

                    int readIndex = argIndex;
                    if (pis[0].Type == typeof(CodeContext)) {
                        readIndex++;
                    }

                    Type curType;
                    if (readIndex < pis.Count) {
                        if (readIndex == -1) {
                            curType = methods[i].Target.Method.DeclaringType;
                        } else if (pis[readIndex].IsParamsArray) {
                            if (argIndex == argCount - (pis.Count - readIndex)) {
                                // We're the params array argument and a single value is being passed
                                // directly to it.  The params array could be in the middle for
                                // a params setter.  so pis.Count - readIndex is usually 1 for the
                                // params at the end, and therefore types.Length - 1 is usually if we're
                                // the last argument.  We always have to check this type to disambiguate
                                // between passing an object which is compatible with the arg array and
                                // passing an object which goes into the arg array.  Maybe we could do 
                                // better sometimes.
                                return true;
                            }
                            curType = pis[pis.Count - 1].Type.GetElementType();
                        } else {
                            curType = pis[readIndex].Type;
                        }
                    } else if (pis[pis.Count - 1].IsParamsArray) {
                        curType = pis[pis.Count - 1].Type.GetElementType();
                    } else {
                        continue;
                    }

                    if (argType == null) {
                        argType = curType;
                    } else if (argType != curType) {
                        return true;
                    }
                }
                return false;
            }

            private static bool IsBest(MethodCandidate candidate, List<MethodCandidate> applicableTargets, CallTypes callType, Type[] actualTypes) {
                foreach (MethodCandidate target in applicableTargets) {
                    if (candidate == target) {
                        continue;
                    }

                    if (MethodCandidate.GetPreferredCandidate(candidate, target, callType, actualTypes) != Candidate.One) {
                        return false;
                    }
                }
                return true;
            }

            private static MethodCandidate FindBest(CallTypes callType, List<MethodCandidate> applicableTargets, Type[] actualTypes) {
                foreach (MethodCandidate candidate in applicableTargets) {
                    if (IsBest(candidate, applicableTargets, callType, actualTypes)) return candidate;
                }
                return null;
            }

            private static bool IsBest(MethodCandidate candidate, List<MethodCandidate> applicableTargets, CallTypes callType, DynamicMetaObject[] actualTypes) {
                foreach (MethodCandidate target in applicableTargets) {
                    if (candidate == target) {
                        continue;
                    }

                    if (MethodCandidate.GetPreferredCandidate(candidate, target, callType, actualTypes) != Candidate.One) {
                        return false;
                    }
                }
                return true;
            }

            private static MethodCandidate FindBest(CallTypes callType, List<MethodCandidate> applicableTargets, DynamicMetaObject[] actualTypes) {
                foreach (MethodCandidate candidate in applicableTargets) {
                    if (IsBest(candidate, applicableTargets, callType, actualTypes)) return candidate;
                }
                return null;
            }

            internal void Add(MethodCandidate target) {
                Debug.Assert(target.Parameters.Count == _count);

                _targets.Add(target);
            }

            private static MethodCandidate GetCandidate(MethodCandidate target, NarrowingLevel level) {
                if (level == NarrowingLevel.None) return target;

                return new MethodCandidate(target, level);
            }

            private BindingTarget MakeSuccessfulBindingTarget(CallTypes callType, Type[] types, List<MethodCandidate> result) {
                MethodCandidate resTarget = result[0];
                return new BindingTarget(_binder.Name, callType == CallTypes.None ? types.Length : types.Length - 1, resTarget.Target, resTarget.NarrowingLevel, GetTypesForTest(types, _targets));
            }

            private BindingTarget MakeSuccessfulBindingTarget(CallTypes callType, DynamicMetaObject[] objects, List<MethodCandidate> result) {
                MethodCandidate resTarget = result[0];
                return new BindingTarget(_binder.Name, callType == CallTypes.None ? objects.Length : objects.Length - 1, resTarget.Target, resTarget.NarrowingLevel, GetRestrictedMetaObjects(resTarget, objects, _targets));
            }

            private BindingTarget MakeAmbiguousBindingTarget<T>(CallTypes callType, T[] types, List<MethodCandidate> result) {
                MethodTarget[] methods = new MethodTarget[result.Count];
                for (int i = 0; i < result.Count; i++) {
                    methods[i] = result[i].Target;
                }

                return new BindingTarget(_binder.Name, callType == CallTypes.None ? types.Length : types.Length - 1, methods);
            }

            [Confined]
            public override string ToString() {
                return string.Format("TargetSet({0} on {1}, nargs={2})", _targets[0].Target.Method.Name, _targets[0].Target.Method.DeclaringType.FullName, _count);
            }
        }

        #endregion
    }
}
