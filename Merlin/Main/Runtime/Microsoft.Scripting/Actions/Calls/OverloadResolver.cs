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
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Contracts;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    using Ast = System.Linq.Expressions.Expression;
    
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
    /// The MethodBinder works by building up a CandidateSet for each number of effective arguments that can be
    /// passed to a set of overloads.  For example a set of overloads such as:
    ///     foo(object a, object b, object c)
    ///     foo(int a, int b)
    ///     
    /// would have 2 target sets - one for 3 parameters and one for 2 parameters.  For parameter arrays
    /// we fallback and create the appropriately sized CandidateSet on demand.
    /// 
    /// Each CandidateSet consists of a set of MethodCandidate's.  Each MethodCandidate knows the flattened
    /// parameters that could be received.  For example for a function such as:
    ///     foo(params int[] args)
    ///     
    /// When this method is in a CandidateSet of size 3 the MethodCandidate takes 3 parameters - all of them
    /// ints; if it's in a CandidateSet of size 4 it takes 4 parameters.  Effectively a MethodCandidate is 
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
    public abstract partial class OverloadResolver {
        private readonly ActionBinder _binder;               

        // built as target sets are built:
        private string _methodName;
        private NarrowingLevel _minLevel, _maxLevel;             // specifies the minimum and maximum narrowing levels for conversions during binding
        private IList<string> _argNames;
        private Dictionary<int, CandidateSet> _candidateSets;    // the methods as they map from # of arguments -> the possible CandidateSet's.
        private List<MethodCandidate> _paramsCandidates;         // the methods which are params methods which need special treatment because they don't have fixed # of args
        
        // built as arguments are processed:
        private ActualArguments _actualArguments;
        private int _maxAccessedCollapsedArg;
        private List<ParameterExpression> _temps;

        protected OverloadResolver(ActionBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");

            _binder = binder;
            _maxAccessedCollapsedArg = -1;
        }

        public ActionBinder Binder {
            get { return _binder; }
        }

        internal List<ParameterExpression> Temps {
            get { return _temps; }
        }

        internal ParameterExpression GetTemporary(Type type, string name) {
            Assert.NotNull(type);

            if (_temps == null) {
                _temps = new List<ParameterExpression>();
            }

            ParameterExpression res = Expression.Variable(type, name);
            _temps.Add(res);
            return res;
        }

        #region ResolveOverload

        /// <summary>
        /// Resolves a method overload and returns back a BindingTarget.
        /// 
        /// The BindingTarget can then be tested for the success or particular type of
        /// failure that prevents the method from being called. If successfully bound the BindingTarget
        /// contains a list of argument meta-objects with additional restrictions that ensure the selection
        /// of the particular overload.
        /// </summary>
        public BindingTarget ResolveOverload(string methodName, IList<MethodBase> methods, NarrowingLevel minLevel, NarrowingLevel maxLevel) {
            ContractUtils.RequiresNotNullItems(methods, "methods");
            ContractUtils.Requires(minLevel <= maxLevel);

            if (_candidateSets != null) {
                throw new InvalidOperationException("Overload resolver cannot be reused.");
            }

            _methodName = methodName;
            _minLevel = minLevel;
            _maxLevel = maxLevel;
            
            // step 1:
            IList<DynamicMetaObject> namedArgs;
            GetNamedArguments(out namedArgs, out _argNames);
            
            // uses arg names:
            BuildCandidateSets(methods);

            // uses target sets:
            int preSplatLimit, postSplatLimit;
            GetSplatLimits(out preSplatLimit, out postSplatLimit);

            // step 2:
            _actualArguments = CreateActualArguments(namedArgs, _argNames, preSplatLimit, postSplatLimit);
            if (_actualArguments == null) {
                return new BindingTarget(methodName, BindingResult.InvalidArguments);
            }

            // steps 3, 4:
            var candidateSet = GetCandidateSet();
            if (candidateSet != null && !candidateSet.IsParamsDictionaryOnly()) {
                return MakeBindingTarget(candidateSet);
            }

            // step 5:
            return new BindingTarget(methodName, _actualArguments.VisibleCount, GetExpectedArgCounts());
        }

        #endregion

        #region Step 1: TargetSet construction, custom special parameters handling

        /// <summary>
        /// Checks to see if the language allows keyword arguments to be bound to instance fields or
        /// properties and turned into sets.  By default this is only allowed on contructors.
        /// </summary>
        internal protected virtual bool AllowKeywordArgumentSetting(MethodBase method) {
            return CompilerHelpers.IsConstructor(method);
        }

        /// <summary>
        /// Gets an expression that evaluates to the result of GetByRefArray operation.
        /// </summary>
        internal protected virtual Expression GetByRefArrayExpression(Expression argumentArrayExpression) {
            return argumentArrayExpression;
        }

        /// <summary>
        /// Handles binding of special parameters.
        /// </summary>
        /// <returns>True if the argument is handled by this method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        protected virtual bool BindSpecialParameter(ParameterInfo parameterInfo, List<ArgBuilder> arguments,
            List<ParameterWrapper> parameters, ref int index) {
            return false;
        }

        /// <summary>
        /// Called before arguments binding.
        /// </summary>
        /// <returns>The number of parameter infos to skip.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        protected virtual int PrepareParametersBinding(ParameterInfo[] parameterInfos, List<ArgBuilder> arguments,
            List<ParameterWrapper> parameters, ref int index) {
            return 0;
        }

        private void BuildCandidateSets(IList<MethodBase> methods) {
            Debug.Assert(_candidateSets == null);
            Debug.Assert(_argNames != null);

            _candidateSets = new Dictionary<int, CandidateSet>();

            foreach (MethodBase method in methods) {
                if (IsUnsupported(method)) continue;

                AddBasicMethodTargets(method);
            }
            
            if (_paramsCandidates != null) {
                // For all the methods that take a params array, create MethodCandidates that clash with the 
                // other overloads of the method
                foreach (MethodCandidate candidate in _paramsCandidates) {
                    foreach (int count in _candidateSets.Keys) {
                        MethodCandidate target = candidate.MakeParamsExtended(count, _argNames);
                        if (target != null) {
                            AddTarget(target);
                        }
                    }
                }
            }
        }

        private CandidateSet GetCandidateSet() {
            Debug.Assert(_candidateSets != null && _actualArguments != null);

            CandidateSet result;

            // use precomputed set if arguments are fully expanded and we have one:
            if (_actualArguments.CollapsedCount == 0 && _candidateSets.TryGetValue(_actualArguments.Count, out result)) {
                return result;
            }

            if (_paramsCandidates != null) {
                // build a new target set specific to the number of arguments we have:
                result = BuildExpandedTargetSet(_actualArguments.Count);
                if (result.Candidates.Count > 0) {
                    return result;
                }
            }

            return null;
        }

        private CandidateSet BuildExpandedTargetSet(int count) {
            var set = new CandidateSet(this, count);
            if (_paramsCandidates != null) {
                foreach (MethodCandidate maker in _paramsCandidates) {
                    MethodCandidate target = maker.MakeParamsExtended(count, _argNames);
                    if (target != null) {
                        set.Add(target);
                    }
                }
            }

            return set;
        }

        private void AddTarget(MethodCandidate target) {
            int count = target.ParameterCount;
            CandidateSet set;
            if (!_candidateSets.TryGetValue(count, out set)) {
                set = new CandidateSet(this, count);
                _candidateSets[count] = set;
            }
            set.Add(target);
        }

        private void AddSimpleTarget(MethodCandidate target) {
            AddTarget(target);
            if (BinderHelpers.IsParamsMethod(target.Method)) {
                if (_paramsCandidates == null) {
                    _paramsCandidates = new List<MethodCandidate>();
                }
                _paramsCandidates.Add(target);
            }
        }

        // TODO: revisit
        protected virtual ArgBuilder MakeInstanceBuilder(MethodBase method, List<ParameterWrapper> parameters, ref int argIndex) {
            if (!CompilerHelpers.IsStatic(method)) {
                parameters.Add(new ParameterWrapper(null, method.DeclaringType, null, true, false, false, false));
                return new SimpleArgBuilder(method.DeclaringType, argIndex++, false, false);
            } else {
                return new NullArgBuilder();
            }
        }

        private void AddBasicMethodTargets(MethodBase method) {
            Assert.NotNull(method);

            var parameterInfos = method.GetParameters();
            var parameters = new List<ParameterWrapper>();
            ParameterWrapper paramsDict = null;
            var arguments = new List<ArgBuilder>(parameterInfos.Length);
            var defaultArguments = new List<ArgBuilder>();
            int argIndex = 0;
            var instanceBuilder = MakeInstanceBuilder(method, parameters, ref argIndex);

            bool hasByRefOrOut = false;
            bool hasDefaults = false;

            var infoIndex = PrepareParametersBinding(parameterInfos, arguments, parameters, ref argIndex);
            for (; infoIndex < parameterInfos.Length; infoIndex++) {
                var pi = parameterInfos[infoIndex];

                if (BindSpecialParameter(pi, arguments, parameters, ref argIndex)) {
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
                    parameters.Add(new ParameterWrapper(pi, refType, pi.Name, true, false, false, false));
                    ab = new ReferenceArgBuilder(pi, refType, indexForArgBuilder);
                } else if (BinderHelpers.IsParamDictionary(pi)) {
                    paramsDict = new ParameterWrapper(pi);
                    ab = new SimpleArgBuilder(pi, indexForArgBuilder);
                } else if (infoIndex == 0 && CompilerHelpers.IsExtension(pi.Member)) {
                    parameters.Add(new ParameterWrapper(pi, pi.ParameterType, pi.Name, true, false, false, true));
                    ab = new SimpleArgBuilder(pi, indexForArgBuilder);
                } else {
                    hasByRefOrOut |= CompilerHelpers.IsOutParameter(pi);
                    parameters.Add(new ParameterWrapper(pi));
                    ab = new SimpleArgBuilder(pi, indexForArgBuilder);
                }

                if (kwIndex == ParameterNotPassedByKeyword) {
                    arguments.Add(ab);
                } else {
                    Debug.Assert(KeywordArgBuilder.BuilderExpectsSingleParameter(ab));
                    arguments.Add(new KeywordArgBuilder(ab, _argNames.Count, kwIndex));
                }
            }

            ReturnBuilder returnBuilder = MakeKeywordReturnBuilder(
                new ReturnBuilder(CompilerHelpers.GetReturnType(method)),
                parameterInfos,
                parameters,
                AllowKeywordArgumentSetting(method)
            );

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
                            paramsDict,
                            instanceBuilder,
                            arguments,
                            defaultArguments,
                            returnBuilder,
                            defaultsUsed));
                    }
                }
            }

            if (hasByRefOrOut) {
                AddSimpleTarget(MakeByRefReducedMethodTarget(parameterInfos, method));
            }

            AddSimpleTarget(new MethodCandidate(this, method, parameters, paramsDict, returnBuilder, instanceBuilder, arguments));
        }

        private MethodCandidate MakeDefaultCandidate(MethodBase method, List<ParameterWrapper> parameters, ParameterWrapper paramsDict,
            ArgBuilder instanceBuilder, List<ArgBuilder> argBuilders, List<ArgBuilder> defaultBuilders, ReturnBuilder returnBuilder, int defaultsUsed) {
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

            return new MethodCandidate(this, method, necessaryParams, paramsDict, returnBuilder, instanceBuilder, defaultArgBuilders);
        }

        private MethodCandidate MakeByRefReducedMethodTarget(ParameterInfo[] parameterInfos, MethodBase method) {
            Assert.NotNull(parameterInfos, method);

            var parameters = new List<ParameterWrapper>();
            ParameterWrapper paramsDict = null;
            var arguments = new List<ArgBuilder>();
            int argIndex = 0;
            var instanceBuilder = MakeInstanceBuilder(method, parameters, ref argIndex);            
            
            List<int> returnArgs = new List<int>();
            if (CompilerHelpers.GetReturnType(method) != typeof(void)) {
                returnArgs.Add(-1);
            }

            var infoIndex = PrepareParametersBinding(parameterInfos, arguments, parameters, ref argIndex);
            for (; infoIndex < parameterInfos.Length; infoIndex++) {
                var pi = parameterInfos[infoIndex];

                if (BindSpecialParameter(pi, arguments, parameters, ref argIndex)) {
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
                    parameters.Add(new ParameterWrapper(pi, pi.ParameterType.GetElementType(), pi.Name, false, false, false, false));
                    ab = new ReturnReferenceArgBuilder(pi, indexForArgBuilder);
                } else if (BinderHelpers.IsParamDictionary(pi)) {
                    paramsDict = new ParameterWrapper(pi);
                    ab = new SimpleArgBuilder(pi, indexForArgBuilder);
                } else {
                    parameters.Add(new ParameterWrapper(pi));
                    ab = new SimpleArgBuilder(pi, indexForArgBuilder);
                }

                if (kwIndex == ParameterNotPassedByKeyword) {
                    arguments.Add(ab);
                } else {
                    Debug.Assert(KeywordArgBuilder.BuilderExpectsSingleParameter(ab));
                    arguments.Add(new KeywordArgBuilder(ab, _argNames.Count, kwIndex));
                }
            }

            ReturnBuilder returnBuilder = MakeKeywordReturnBuilder(
                new ByRefReturnBuilder(returnArgs),
                parameterInfos,
                parameters,
                AllowKeywordArgumentSetting(method)
            );

            return new MethodCandidate(this, method, parameters, paramsDict, returnBuilder, instanceBuilder, arguments);
        }

        private static bool IsUnsupported(MethodBase method) {
            return (method.CallingConvention & CallingConventions.VarArgs) != 0 || method.ContainsGenericParameters;
        }

        #endregion

        #region Step 2: Actual Arguments

        public ActualArguments GetActualArguments() {
            if (_actualArguments == null) {
                throw new InvalidOperationException("Actual arguments have not been built yet.");
            }
            return _actualArguments; 
        }

        protected virtual void GetNamedArguments(out IList<DynamicMetaObject> namedArgs, out IList<string> argNames) {
            // language doesn't support named arguments:
            argNames = ArrayUtils.EmptyStrings;
            namedArgs = DynamicMetaObject.EmptyMetaObjects;
        }

        /// <summary>
        /// Return null if arguments cannot be constructed and overload resolution should produce an error.
        /// </summary>
        protected abstract ActualArguments CreateActualArguments(IList<DynamicMetaObject> namedArgs, IList<string> argNames, int preSplatLimit, int postSplatLimit);

        #endregion

        #region Step 3: Resolution

        internal BindingTarget MakeBindingTarget(CandidateSet targetSet) {
            List<CallFailure> failures = null;
            List<CallFailure> nameBindingFailures = null;

            // get candidates whose named arguments can be bind to the parameters:
            var potential = EnsureMatchingNamedArgs(targetSet.Candidates, ref nameBindingFailures);

            if (potential.Count == 0) {
                return MakeFailedBindingTarget(nameBindingFailures.ToArray());
            }

            // go through all available narrowing levels selecting candidates.  
            for (NarrowingLevel level = _minLevel; level <= _maxLevel; level++) {
                if (failures != null) {
                    failures.Clear();
                }

                // only allow candidates whose non-collapsed arguments are convertible to the parameter types:
                var applicable = SelectCandidatesWithConvertibleArgs(potential, level, ref failures);

                if (applicable.Count == 0) {
                    continue;
                } else if (applicable.Count == 1) {
                    return MakeSuccessfulBindingTarget(applicable[0], potential, level, targetSet);
                }

                // see if collapsed arguments be converted to the corresponding element types:
                applicable = SelectCandidatesWithConvertibleCollapsedArgs(applicable, level, ref failures);

                if (applicable.Count == 0) {
                    continue;
                } else if (applicable.Count == 1) {
                    return MakeSuccessfulBindingTarget(applicable[0], potential, level, targetSet);
                }

                var bestCandidate = SelectBestCandidate(applicable);
                if (bestCandidate != null) {
                    return MakeSuccessfulBindingTarget(bestCandidate, potential, level, targetSet);
                } else {
                    return MakeAmbiguousBindingTarget(applicable);
                }
            }

            Debug.Assert(failures != null);
            if (nameBindingFailures != null) {
                failures.AddRange(nameBindingFailures);
            }
            return MakeFailedBindingTarget(failures.ToArray());
        }

        private List<ApplicableCandidate> EnsureMatchingNamedArgs(List<MethodCandidate> candidates, ref List<CallFailure> failures) {
            var result = new List<ApplicableCandidate>();
            foreach (MethodCandidate candidate in candidates) {
                // skip params dictionaries - we want to only pick up the methods normalized
                // to have argument names (which we created because the MethodBinder gets 
                // created w/ keyword arguments).
                if (!candidate.HasParamsDictionary) {
                    CallFailure callFailure;
                    ArgumentBinding namesBinding;

                    if (_actualArguments.TryBindNamedArguments(candidate, out namesBinding, out callFailure)) {
                        result.Add(new ApplicableCandidate(candidate, namesBinding));
                    } else {
                        AddFailure(ref failures, callFailure);
                    }
                }
            }
            return result;
        }

        private List<ApplicableCandidate> SelectCandidatesWithConvertibleArgs(List<ApplicableCandidate> candidates, NarrowingLevel level, 
            ref List<CallFailure> failures) {

            var result = new List<ApplicableCandidate>();
            foreach (ApplicableCandidate candidate in candidates) {
                CallFailure callFailure;
                if (TryConvertArguments(candidate.Method, candidate.ArgumentBinding, level, out callFailure)) {
                    result.Add(candidate);
                } else {
                    AddFailure(ref failures, callFailure);
                }
            }
            return result;
        }

        private List<ApplicableCandidate> SelectCandidatesWithConvertibleCollapsedArgs(List<ApplicableCandidate> candidates,
            NarrowingLevel level, ref List<CallFailure> failures) {

            if (_actualArguments.CollapsedCount == 0) {
                return candidates;
            }

            var result = new List<ApplicableCandidate>();
            foreach (ApplicableCandidate candidate in candidates) {
                CallFailure callFailure;
                if (TryConvertCollapsedArguments(candidate.Method, level, out callFailure)) {
                    result.Add(candidate);
                } else {
                    AddFailure(ref failures, callFailure);
                }
            }
            return result;
        }

        private static void AddFailure(ref List<CallFailure> failures, CallFailure failure) {
            if (failures == null) {
                failures = new List<CallFailure>(1);
            }
            failures.Add(failure);
        }

        private bool TryConvertArguments(MethodCandidate candidate, ArgumentBinding namesBinding, NarrowingLevel narrowingLevel, out CallFailure failure) {
            Debug.Assert(_actualArguments.Count == candidate.ParameterCount);

            BitArray hasConversion = new BitArray(_actualArguments.Count);

            bool success = true;
            for (int i = 0; i < _actualArguments.Count; i++) {
                success &= (hasConversion[i] = CanConvertFrom(_actualArguments[i].GetLimitType(), candidate.GetParameter(i, namesBinding), narrowingLevel));
            }

            if (!success) {
                var conversionResults = new ConversionResult[_actualArguments.Count];
                for (int i = 0; i < _actualArguments.Count; i++) {
                    conversionResults[i] = new ConversionResult(_actualArguments[i].GetLimitType(), candidate.GetParameter(i, namesBinding).Type, !hasConversion[i]);
                }
                failure = new CallFailure(candidate, conversionResults);
            } else {
                failure = null;
            }

            return success;
        }

        private bool TryConvertCollapsedArguments(MethodCandidate candidate, NarrowingLevel narrowingLevel, out CallFailure failure) {
            Debug.Assert(_actualArguments.CollapsedCount > 0);

            // There must be at least one expanded parameter preceding splat index (see MethodBinder.GetSplatLimits):
            ParameterWrapper parameter = candidate.GetParameter(_actualArguments.SplatIndex - 1);
            Debug.Assert(parameter.ParameterInfo != null && CompilerHelpers.IsParamArray(parameter.ParameterInfo));

            for (int i = 0; i < _actualArguments.CollapsedCount; i++) {
                Type argType = CompilerHelpers.GetType(GetCollapsedArgumentValue(i));

                if (!CanConvertFrom(argType, parameter, narrowingLevel)) {
                    failure = new CallFailure(candidate, new[] { new ConversionResult(argType, parameter.Type, false) });
                    return false;
                }
            }

            failure = null;
            return true;
        }

        private RestrictionInfo GetRestrictedArgs(ApplicableCandidate selectedCandidate, IList<ApplicableCandidate> candidates, int targetSetSize) {
            Debug.Assert(selectedCandidate.Method.ParameterCount == _actualArguments.Count);

            int argCount = _actualArguments.Count;
            var restrictedArgs = new DynamicMetaObject[argCount];
            var types = new Type[argCount];

            for (int i = 0; i < argCount; i++) {
                var arg = _actualArguments[i];

                if (targetSetSize > 0 && IsOverloadedOnParameter(i, argCount, candidates) ||
                    !selectedCandidate.GetParameter(i).Type.IsAssignableFrom(arg.Expression.Type)) {

                    restrictedArgs[i] = RestrictArgument(arg, selectedCandidate.GetParameter(i));
                    types[i] = arg.GetLimitType();
                } else {
                    restrictedArgs[i] = arg;
                }
            }

            return new RestrictionInfo(restrictedArgs, types);
        }

        private DynamicMetaObject RestrictArgument(DynamicMetaObject arg, ParameterWrapper parameter) {
            if (parameter.Type == typeof(object)) {
                // don't use Restrict as it'll box & unbox.
                return new DynamicMetaObject(arg.Expression, BindingRestrictionsHelpers.GetRuntimeTypeRestriction(arg.Expression, arg.GetLimitType()));
            } else {
                return arg.Restrict(arg.GetLimitType());
            }
        }

        /// <summary>
        /// Determines whether given overloads are overloaded on index-th parameter (the types of the index-th parameters are the same).
        /// </summary>
        private static bool IsOverloadedOnParameter(int argIndex, int argCount, IList<ApplicableCandidate> overloads) {
            Debug.Assert(argIndex >= 0);

            Type seenParametersType = null;
            foreach (var overload in overloads) {
                int parameterCount = overload.Method.ParameterCount;
                if (parameterCount == 0) {
                    continue;
                }

                var lastParameter = overload.Method.GetParameter(parameterCount - 1);

                Type parameterType;
                if (argIndex < parameterCount) {
                    var parameter = overload.GetParameter(argIndex);
                    if (parameter.IsParamsArray) {
                        if (parameterCount == argCount) {
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
                        parameterType = lastParameter.Type.GetElementType();
                    } else {
                        parameterType = parameter.Type;
                    }
                } else if (lastParameter.IsParamsArray) {
                    parameterType = lastParameter.Type.GetElementType();
                } else {
                    continue;
                }

                if (seenParametersType == null) {
                    seenParametersType = parameterType;
                } else if (seenParametersType != parameterType) {
                    return true;
                }
            }
            return false;
        }

        private bool IsBest(ApplicableCandidate candidate, List<ApplicableCandidate> candidates) {
            foreach (ApplicableCandidate other in candidates) {
                if (candidate == other) {
                    continue;
                }

                if (GetPreferredCandidate(candidate, other) != Candidate.One) {
                    return false;
                }
            }
            return true;
        }

        internal Candidate GetPreferredCandidate(ApplicableCandidate one, ApplicableCandidate two) {
            Candidate cmpParams = GetPreferredParameters(one, two);
            if (cmpParams.Chosen()) {
                return cmpParams;
            }

            return CompareEquivalentCandidates(one, two);
        }

        internal protected virtual Candidate CompareEquivalentCandidates(ApplicableCandidate one, ApplicableCandidate two) {
            Candidate ret = CompareEquivalentParameters(one.Method, two.Method);
            if (ret.Chosen()) {
                return ret;
            }

            return Candidate.Equivalent;
        }

        internal static Candidate CompareEquivalentParameters(MethodCandidate one, MethodCandidate two) {
            // Prefer normal methods over explicit interface implementations
            if (two.Method.IsPrivate && !one.Method.IsPrivate) return Candidate.One;
            if (one.Method.IsPrivate && !two.Method.IsPrivate) return Candidate.Two;

            // Prefer non-generic methods over generic methods
            if (one.Method.IsGenericMethod) {
                if (!two.Method.IsGenericMethod) {
                    return Candidate.Two;
                } else {
                    //!!! Need to support selecting least generic method here
                    return Candidate.Equivalent;
                }
            } else if (two.Method.IsGenericMethod) {
                return Candidate.One;
            }

            //prefer methods without out params over those with them
            switch (Compare(one.ReturnBuilder.CountOutParams, two.ReturnBuilder.CountOutParams)) {
                case 1: return Candidate.Two;
                case -1: return Candidate.One;
            }

            //prefer methods using earlier conversions rules to later ones            
            for (int i = Int32.MaxValue; i >= 0; ) {
                int maxPriorityThis = FindMaxPriority(one.ArgBuilders, i);
                int maxPriorityOther = FindMaxPriority(two.ArgBuilders, i);

                if (maxPriorityThis < maxPriorityOther) return Candidate.One;
                if (maxPriorityOther < maxPriorityThis) return Candidate.Two;

                i = maxPriorityThis - 1;
            }

            return Candidate.Equivalent;
        }

        private static int Compare(int x, int y) {
            if (x < y) return -1;
            else if (x > y) return +1;
            else return 0;
        }

        private static int FindMaxPriority(IList<ArgBuilder> abs, int ceiling) {
            int max = 0;
            foreach (ArgBuilder ab in abs) {
                if (ab.Priority > ceiling) continue;

                max = System.Math.Max(max, ab.Priority);
            }
            return max;
        }

        internal Candidate GetPreferredParameters(ApplicableCandidate one, ApplicableCandidate two) {
            Debug.Assert(one.Method.ParameterCount == two.Method.ParameterCount);
            var args = GetActualArguments();

            Candidate result = Candidate.Equivalent;
            for (int i = 0; i < args.Count; i++) {
                Candidate preferred = GetPreferredParameter(one.GetParameter(i), two.GetParameter(i), args[i].GetLimitType());

                switch (result) {
                    case Candidate.Equivalent:
                        result = preferred;
                        break;

                    case Candidate.One:
                        if (preferred == Candidate.Two) return Candidate.Ambiguous;
                        break;

                    case Candidate.Two:
                        if (preferred == Candidate.One) return Candidate.Ambiguous;
                        break;

                    case Candidate.Ambiguous:
                        if (preferred != Candidate.Equivalent) {
                            result = preferred;
                        }
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            // TODO: process collapsed arguments:

            return result;
        }

        internal Candidate GetPreferredParameter(ParameterWrapper candidateOne, ParameterWrapper candidateTwo, Type argType) {
            Assert.NotNull(candidateOne, candidateTwo, argType);

            if (ParametersEquivalent(candidateOne, candidateTwo)) {
                return Candidate.Equivalent;
            }

            for (NarrowingLevel curLevel = NarrowingLevel.None; curLevel <= NarrowingLevel.All; curLevel++) {
                Candidate candidate = SelectBestConversionFor(argType, candidateOne, candidateTwo, curLevel);
                if (candidate.Chosen()) {
                    return candidate;
                }
            }

            return GetPreferredParameter(candidateOne, candidateTwo);
        }

        internal Candidate GetPreferredParameter(ParameterWrapper candidateOne, ParameterWrapper candidateTwo) {
            Assert.NotNull(candidateOne, candidateTwo);

            if (ParametersEquivalent(candidateOne, candidateTwo)) {
                return Candidate.Equivalent;
            }

            Type t1 = candidateOne.Type;
            Type t2 = candidateTwo.Type;

            if (CanConvertFrom(t2, candidateOne, NarrowingLevel.None)) {
                if (CanConvertFrom(t1, candidateTwo, NarrowingLevel.None)) {
                    return Candidate.Ambiguous;
                } else {
                    return Candidate.Two;
                }
            }

            if (CanConvertFrom(t1, candidateTwo, NarrowingLevel.None)) {
                return Candidate.One;
            }

            // Special additional rules to order numeric value types
            Candidate preferred = PreferConvert(t1, t2);
            if (preferred.Chosen()) {
                return preferred;
            }

            preferred = PreferConvert(t2, t1).TheOther();
            if (preferred.Chosen()) {
                return preferred;
            }

            return Candidate.Ambiguous;
        }

        private ApplicableCandidate SelectBestCandidate(List<ApplicableCandidate> candidates) {
            foreach (var candidate in candidates) {
                if (IsBest(candidate, candidates)) {
                    return candidate;
                }
            }
            return null;
        }

        private BindingTarget MakeSuccessfulBindingTarget(ApplicableCandidate result, List<ApplicableCandidate> potentialCandidates,
            NarrowingLevel level, CandidateSet targetSet) {

            return new BindingTarget(
                _methodName,
                _actualArguments.VisibleCount,
                result.Method,
                level,
                GetRestrictedArgs(result, potentialCandidates, targetSet.Arity)
            );
        }

        private BindingTarget MakeFailedBindingTarget(CallFailure[] failures) {
            return new BindingTarget(_methodName, _actualArguments.VisibleCount, failures);
        }

        private BindingTarget MakeAmbiguousBindingTarget(List<ApplicableCandidate> result) {
            var methods = new MethodCandidate[result.Count];
            for (int i = 0; i < result.Count; i++) {
                methods[i] = result[i].Method;
            }

            return new BindingTarget(_methodName, _actualArguments.VisibleCount, methods);
        }

        #endregion

        #region Step 4: Argument Building, Conversions

        public virtual bool ParametersEquivalent(ParameterWrapper parameter1, ParameterWrapper parameter2) {
            return parameter1.Type == parameter2.Type && parameter1.ProhibitNull == parameter2.ProhibitNull;
        }

        public virtual bool CanConvertFrom(Type fromType, ParameterWrapper toParameter, NarrowingLevel level) {
            Assert.NotNull(fromType, toParameter);

            Type toType = toParameter.Type;

            if (fromType == typeof(DynamicNull)) {
                if (toParameter.ProhibitNull) {
                    return false;
                }

                if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    return true;
                }

                if (!toType.IsValueType) {
                    return true;
                }
            }

            if (fromType == toType) {
                return true;
            }

            return _binder.CanConvertFrom(fromType, toType, toParameter.ProhibitNull, level);
        }

        /// <summary>
        /// Selects the best (of two) candidates for conversion from actualType
        /// </summary>
        public virtual Candidate SelectBestConversionFor(Type actualType, ParameterWrapper candidateOne, ParameterWrapper candidateTwo, NarrowingLevel level) {
            return Candidate.Equivalent;
        }

        /// <summary>
        /// Provides ordering for two parameter types if there is no conversion between the two parameter types.
        /// </summary>
        public virtual Candidate PreferConvert(Type t1, Type t2) {
            return _binder.PreferConvert(t1, t2);
        }

        // TODO: revisit
        public virtual Expression ConvertExpression(Expression expr, ParameterInfo info, Type toType) {
            Assert.NotNull(expr, toType);

            return _binder.ConvertExpression(expr, toType, ConversionResultKind.ExplicitCast, null);
        }

        // TODO: revisit
        public virtual Func<object[], object> ConvertObject(int index, DynamicMetaObject knownType, ParameterInfo info, Type toType) {
            throw new NotImplementedException();
        }

        // TODO: revisit
        public virtual Expression GetDynamicConversion(Expression value, Type type) {
            return Expression.Dynamic(OldConvertToAction.Make(_binder, type), type, value);
        }

        #endregion

        #region Step 5: Results, Errors
        
        private int[] GetExpectedArgCounts() {
            if (_candidateSets.Count == 0) {
                return new int[0];
            }
            
            int minParamsArray = Int32.MaxValue;
            var arities = new BitArray(_candidateSets.Keys.Max() + 1);
            foreach (var targetSet in _candidateSets.Values) {
                foreach (var candidate in targetSet.Candidates) {
                    int visibleCount = candidate.GetVisibleParameterCount();
                    arities[visibleCount] = true;

                    if (candidate.HasParamsArray) {
                        arities[visibleCount - 1] = true;
                        minParamsArray = System.Math.Min(minParamsArray, visibleCount);
                    }
                }
            }

            var result = new List<int>();
            for (int i = 0; i < System.Math.Min(arities.Count, minParamsArray); i++) {
                if (arities[i]) {
                    result.Add(i);
                }
            }

            // all arities starting from minParamsArray are available:
            if (minParamsArray < Int32.MaxValue) {
                result.Add(Int32.MaxValue);
            }

            return result.ToArray();
        }

        public virtual ErrorInfo MakeInvalidParametersError(BindingTarget target) {
            switch (target.Result) {
                case BindingResult.CallFailure: return MakeCallFailureError(target);
                case BindingResult.AmbiguousMatch: return MakeAmbiguousCallError(target);
                case BindingResult.IncorrectArgumentCount: return MakeIncorrectArgumentCountError(target);
                case BindingResult.InvalidArguments: return MakeInvalidArgumentsError();
                default: throw new InvalidOperationException();
            }
        }

        private static ErrorInfo MakeIncorrectArgumentCountError(BindingTarget target) {
            int minArgs = Int32.MaxValue;
            int maxArgs = Int32.MinValue;
            foreach (int argCnt in target.ExpectedArgumentCount) {
                minArgs = System.Math.Min(minArgs, argCnt);
                maxArgs = System.Math.Max(maxArgs, argCnt);
            }

            return ErrorInfo.FromException(
                Ast.Call(
                    typeof(BinderOps).GetMethod("TypeErrorForIncorrectArgumentCount", new Type[] {
                                typeof(string), typeof(int), typeof(int) , typeof(int), typeof(int), typeof(bool), typeof(bool)
                            }),
                    AstUtils.Constant(target.Name, typeof(string)),  // name
                    AstUtils.Constant(minArgs),                      // min formal normal arg cnt
                    AstUtils.Constant(maxArgs),                      // max formal normal arg cnt
                    AstUtils.Constant(0),                            // default cnt
                    AstUtils.Constant(target.ActualArgumentCount),   // args provided
                    AstUtils.Constant(false),                        // hasArgList
                    AstUtils.Constant(false)                         // kwargs provided
                )
            );
        }

        private ErrorInfo MakeAmbiguousCallError(BindingTarget target) {
            StringBuilder sb = new StringBuilder("Multiple targets could match: ");
            string outerComma = "";
            foreach (MethodCandidate candidate in target.AmbiguousMatches) {
                Type[] types = candidate.GetParameterTypes();
                string innerComma = "";

                sb.Append(outerComma);
                sb.Append(target.Name);
                sb.Append('(');
                foreach (Type t in types) {
                    sb.Append(innerComma);
                    sb.Append(_binder.GetTypeName(t));
                    innerComma = ", ";
                }

                sb.Append(')');
                outerComma = ", ";
            }

            return ErrorInfo.FromException(
                Ast.Call(
                    typeof(BinderOps).GetMethod("SimpleTypeError"),
                    AstUtils.Constant(sb.ToString(), typeof(string))
                )
            );
        }

        private ErrorInfo MakeCallFailureError(BindingTarget target) {
            foreach (CallFailure cf in target.CallFailures) {
                switch (cf.Reason) {
                    case CallFailureReason.ConversionFailure:
                        foreach (ConversionResult cr in cf.ConversionResults) {
                            if (cr.Failed) {
                                return ErrorInfo.FromException(
                                    Ast.Call(
                                        typeof(BinderOps).GetMethod("SimpleTypeError"),
                                        AstUtils.Constant(String.Format("expected {0}, got {1}", _binder.GetTypeName(cr.To), _binder.GetTypeName(cr.From)))
                                    )
                                );
                            }
                        }
                        break;
                    case CallFailureReason.DuplicateKeyword:
                        return ErrorInfo.FromException(
                                Ast.Call(
                                    typeof(BinderOps).GetMethod("TypeErrorForDuplicateKeywordArgument"),
                                    AstUtils.Constant(target.Name, typeof(string)),
                                    AstUtils.Constant(cf.KeywordArguments[0], typeof(string))    // TODO: Report all bad arguments?
                            )
                        );
                    case CallFailureReason.UnassignableKeyword:
                        return ErrorInfo.FromException(
                                Ast.Call(
                                    typeof(BinderOps).GetMethod("TypeErrorForExtraKeywordArgument"),
                                    AstUtils.Constant(target.Name, typeof(string)),
                                    AstUtils.Constant(cf.KeywordArguments[0], typeof(string))    // TODO: Report all bad arguments?
                            )
                        );
                    default: throw new InvalidOperationException();
                }
            }
            throw new InvalidOperationException();
        }

        private ErrorInfo MakeInvalidArgumentsError() {
            return ErrorInfo.FromException(Ast.Call(typeof(BinderOps).GetMethod("SimpleTypeError"), AstUtils.Constant("Invalid arguments.")));
        }

        #endregion

        #region Splatting

        // Get minimal number of arguments that must precede/follow splat mark in actual arguments.
        private void GetSplatLimits(out int preSplatLimit, out int postSplatLimit) {
            Debug.Assert(_candidateSets != null);

            if (_paramsCandidates != null) {
                int preCount = -1;
                int postCount = -1;

                // For all the methods that take a params array, create MethodCandidates that clash with the 
                // other overloads of the method
                foreach (MethodCandidate candidate in _paramsCandidates) {
                    preCount = System.Math.Max(preCount, candidate.ParamsArrayIndex);
                    postCount = System.Math.Max(postCount, candidate.ParameterCount - candidate.ParamsArrayIndex - 1);
                }

                int maxArity = _candidateSets.Keys.Max();
                if (preCount + postCount < maxArity) {
                    preCount = maxArity - postCount;
                }

                // +1 ensures that there is at least one expanded parameter before splatIndex (see MethodCandidate.TryConvertCollapsedArguments):
                preSplatLimit = preCount + 1;
                postSplatLimit = postCount;
            } else {
                // no limits, expand splatted arg fully:
                postSplatLimit = Int32.MaxValue;
                preSplatLimit = Int32.MaxValue;
            }
        }

        /// <summary>
        /// The method is called each time an item of lazily splatted argument is needed.
        /// </summary>
        internal Expression GetSplattedItemExpression(Expression indexExpression) {
            // TODO: move up?
            return Expression.Call(GetSplattedExpression(), typeof(IList).GetMethod("get_Item"), indexExpression);
        }

        protected abstract Expression GetSplattedExpression();
        protected abstract object GetSplattedItem(int index);

        internal object GetCollapsedArgumentValue(int collapsedArgIndex) {
            var result = GetSplattedItem(_actualArguments.ToSplattedItemIndex(collapsedArgIndex));
            _maxAccessedCollapsedArg = System.Math.Max(_maxAccessedCollapsedArg, collapsedArgIndex);
            return result;
        }

        public int MaxAccessedCollapsedArg {
            get { return _maxAccessedCollapsedArg; }
        }

        internal Type[] GetAccessedCollapsedArgTypes() {
            Type[] types = new Type[_maxAccessedCollapsedArg + 1];
            for (int i = 0; i < types.Length; i++) {
                var arg = GetSplattedItem(_actualArguments.ToSplattedItemIndex(i));
                types[i] = (arg != null) ? arg.GetType() : null;
            }
            return types;
        }

        // TODO: move up?
        public Expression GetCollapsedArgsCondition() {
            // collapsed args:
            if (_maxAccessedCollapsedArg >= 0) {
                Type[] collapsedTypes = GetAccessedCollapsedArgTypes();

                return Ast.Call(null, typeof(CompilerHelpers).GetMethod("TypesEqual"),
                    GetSplattedExpression(),
                    AstUtils.Constant(_actualArguments.ToSplattedItemIndex(0)),
                    Ast.Constant(collapsedTypes)
                );
            } else {
                return null;
            }
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
                            mi.MemberType == MemberTypes.Property ? 
                                ((PropertyInfo)mi).PropertyType :
                                ((FieldInfo)mi).FieldType,
                            mi.Name,
                            false);

                        parameters.Add(pw);
                        kwArgIndexs.Add(GetKeywordIndex(mi.Name));
                    }

                    KeywordConstructorReturnBuilder kwBuilder = new KeywordConstructorReturnBuilder(returnBuilder,
                        _argNames.Count,
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
            foreach (string name in _argNames) {
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
            for (int i = 0; i < _argNames.Count; i++) {
                if (kwName == _argNames[i]) {
                    return i;
                }
            }
            return ParameterNotPassedByKeyword;
        }

        #endregion

        [Confined]
        public override string ToString() {
            string res = "";
            foreach (CandidateSet set in _candidateSets.Values) {
                res += set + Environment.NewLine;
            }
            return res;
        }
    }
}
