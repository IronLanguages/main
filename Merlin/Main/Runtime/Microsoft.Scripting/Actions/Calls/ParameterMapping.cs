using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.Scripting.Generation;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;
using System.Collections;

namespace Microsoft.Scripting.Actions.Calls {
    public sealed class ParameterMapping {
        private readonly OverloadResolver _resolver;
        private readonly MethodBase _method;
        private readonly IList<string> _argNames;

        private readonly ParameterInfo[] _parameterInfos;
        private readonly List<ParameterWrapper> _parameters;
        private readonly List<ArgBuilder> _arguments;

        // the next argument to consume
        private int _argIndex;
        
        private List<int> _returnArgs;
        private ArgBuilder _instanceBuilder;
        private ReturnBuilder _returnBuilder;

        private List<ArgBuilder> _defaultArguments;
        private bool _hasByRefOrOut;
        private bool _hasDefaults;
        private ParameterWrapper _paramsDict;

        public MethodBase Method { get { return _method; } }
        public ParameterInfo[] ParameterInfos { get { return _parameterInfos; } }
        public int ArgIndex { get { return _argIndex; } } 

        internal ParameterMapping(OverloadResolver resolver, MethodBase method, ParameterInfo[] parameterInfos, IList<string> argNames) {
            Assert.NotNull(resolver, method);
            _resolver = resolver;
            _method = method;
            _argNames = argNames;
            _parameterInfos = parameterInfos ?? method.GetParameters();
            _parameters = new List<ParameterWrapper>();
            _arguments = new List<ArgBuilder>(_parameterInfos.Length);
            _defaultArguments = new List<ArgBuilder>();
	    }

        internal void MapParameters(bool reduceByRef) {
            if (reduceByRef) {
                _returnArgs = new List<int>();
                if (CompilerHelpers.GetReturnType(_method) != typeof(void)) {
                    _returnArgs.Add(-1);
                }
            }

            BitArray specialParameters = _resolver.MapSpecialParameters(this);

            if (_instanceBuilder == null) {
                _instanceBuilder = new NullArgBuilder();
            }

            for (int infoIndex = 0; infoIndex < _parameterInfos.Length; infoIndex++) {
                if (!IsSpecialParameter(specialParameters, infoIndex)) {
                    if (reduceByRef) {
                        MapParameterReduceByRef(_parameterInfos[infoIndex]);
                    } else {
                        MapParameter(_parameterInfos[infoIndex]);
                    }
                }
            }

            _returnBuilder = MakeReturnBuilder(specialParameters);
        }

        private bool IsSpecialParameter(BitArray specialParameters, int infoIndex) {
            return specialParameters != null && infoIndex < specialParameters.Length && specialParameters[infoIndex];
        }

        public void AddInstanceBuilder(ArgBuilder builder) {
            ContractUtils.Requires(_instanceBuilder == null);
            ContractUtils.Requires(builder.ConsumedArgumentCount == 1);
            _instanceBuilder = builder;
            _argIndex += 1;
        }

        // TODO: We might want to add bitmap of all consumed arguments and allow to consume an arbitrary argument, not just the next one.
        public void AddBuilder(ArgBuilder builder) {
            ContractUtils.Requires(builder.ConsumedArgumentCount != ArgBuilder.AllArguments);

            _arguments.Add(builder);
            _argIndex += builder.ConsumedArgumentCount;
        }

        public void AddParameter(ParameterWrapper parameter) {
            _parameters.Add(parameter);
        }

        public void MapParameter(ParameterInfo pi) {
            int indexForArgBuilder;
            int nameIndex = _argNames.IndexOf(pi.Name);
            if (nameIndex == -1) {
                // positional argument, we simply consume the next argument
                indexForArgBuilder = _argIndex++;
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
                if (nameIndex == -1 || !_hasDefaults) {
                    _defaultArguments.Add(new DefaultArgBuilder(pi));
                    _hasDefaults = true;
                } else {
                    _defaultArguments.Add(null);
                }
            } else if (_defaultArguments.Count > 0) {
                // non-contigious default parameter
                _defaultArguments.Add(null);
            }

            ArgBuilder ab;
            if (pi.ParameterType.IsByRef) {
                _hasByRefOrOut = true;
                Type refType = typeof(StrongBox<>).MakeGenericType(pi.ParameterType.GetElementType());
                _parameters.Add(new ParameterWrapper(pi, refType, pi.Name, true, false, false, false));
                ab = new ReferenceArgBuilder(pi, refType, indexForArgBuilder);
            } else if (BinderHelpers.IsParamDictionary(pi)) {
                _paramsDict = new ParameterWrapper(pi);
                ab = new SimpleArgBuilder(pi, indexForArgBuilder);
            } else if (pi.Position == 0 && CompilerHelpers.IsExtension(pi.Member)) {
                _parameters.Add(new ParameterWrapper(pi, pi.ParameterType, pi.Name, true, false, false, true));
                ab = new SimpleArgBuilder(pi, indexForArgBuilder);
            } else {
                _hasByRefOrOut |= CompilerHelpers.IsOutParameter(pi);
                _parameters.Add(new ParameterWrapper(pi));
                ab = new SimpleArgBuilder(pi, indexForArgBuilder);
            }

            if (nameIndex == -1) {
                _arguments.Add(ab);
            } else {
                Debug.Assert(KeywordArgBuilder.BuilderExpectsSingleParameter(ab));
                _arguments.Add(new KeywordArgBuilder(ab, _argNames.Count, nameIndex));
            }
        }

        private void MapParameterReduceByRef(ParameterInfo pi) {
            Debug.Assert(_returnArgs != null);

            // See KeywordArgBuilder.BuilderExpectsSingleParameter
            int indexForArgBuilder = 0;

            int nameIndex = -1;
            if (!CompilerHelpers.IsOutParameter(pi)) {
                nameIndex = _argNames.IndexOf(pi.Name);
                if (nameIndex == -1) {
                    indexForArgBuilder = _argIndex++;
                }
            }

            ArgBuilder ab;
            if (CompilerHelpers.IsOutParameter(pi)) {
                _returnArgs.Add(_arguments.Count);
                ab = new OutArgBuilder(pi);
            } else if (pi.ParameterType.IsByRef) {
                // if the parameter is marked as [In] it is not returned.
                if ((pi.Attributes & (ParameterAttributes.In | ParameterAttributes.Out)) != ParameterAttributes.In) {
                    _returnArgs.Add(_arguments.Count);
                }
                _parameters.Add(new ParameterWrapper(pi, pi.ParameterType.GetElementType(), pi.Name, false, false, false, false));
                ab = new ReturnReferenceArgBuilder(pi, indexForArgBuilder);
            } else if (BinderHelpers.IsParamDictionary(pi)) {
                _paramsDict = new ParameterWrapper(pi);
                ab = new SimpleArgBuilder(pi, indexForArgBuilder);
            } else {
                _parameters.Add(new ParameterWrapper(pi));
                ab = new SimpleArgBuilder(pi, indexForArgBuilder);
            }

            if (nameIndex == -1) {
                _arguments.Add(ab);
            } else {
                Debug.Assert(KeywordArgBuilder.BuilderExpectsSingleParameter(ab));
                _arguments.Add(new KeywordArgBuilder(ab, _argNames.Count, nameIndex));
            }
        }

        internal MethodCandidate CreateCandidate() {
            return new MethodCandidate(_resolver, _method, _parameters, _paramsDict, _returnBuilder, _instanceBuilder, _arguments);
        }

        internal MethodCandidate CreateByRefReducedCandidate() {
            if (!_hasByRefOrOut) {
                return null;
            }

            var reducedMapping = new ParameterMapping(_resolver, _method, _parameterInfos, _argNames);
            reducedMapping.MapParameters(true);
            return reducedMapping.CreateCandidate();
        }

        #region Candidates with Default Parameters

        internal IEnumerable<MethodCandidate> CreateDefaultCandidates() {
            if (!_hasDefaults) {
                yield break;
            }

            for (int defaultsUsed = 1; defaultsUsed < _defaultArguments.Count + 1; defaultsUsed++) {
                // if the left most default we'll use is not present then don't add a default.  This happens in cases such as:
                // a(a=1, b=2, c=3) and then call with a(a=5, c=3).  We'll come through once for c (no default, skip),
                // once for b (default present, emit) and then a (no default, skip again).  W/o skipping we'd generate the same
                // method multiple times.  This also happens w/ non-contigious default values, e.g. foo(a, b=3, c) where we don't want
                // to generate a default candidate for just c which matches the normal method.
                if (_defaultArguments[_defaultArguments.Count - defaultsUsed] != null) {
                    yield return CreateDefaultCandidate(defaultsUsed);
                }
            }
        }

        private MethodCandidate CreateDefaultCandidate(int defaultsUsed) {
            List<ArgBuilder> defaultArgBuilders = new List<ArgBuilder>(_arguments);
            List<ParameterWrapper> necessaryParams = _parameters.GetRange(0, _parameters.Count - defaultsUsed);

            for (int curDefault = 0; curDefault < defaultsUsed; curDefault++) {
                int readIndex = _defaultArguments.Count - defaultsUsed + curDefault;
                int writeIndex = defaultArgBuilders.Count - defaultsUsed + curDefault;

                if (_defaultArguments[readIndex] != null) {
                    defaultArgBuilders[writeIndex] = _defaultArguments[readIndex];
                } else {
                    necessaryParams.Add(_parameters[_parameters.Count - defaultsUsed + curDefault]);
                }
            }

            // shift any arguments forward that need to be...
            int curArg = CompilerHelpers.IsStatic(_method) ? 0 : 1;
            for (int i = 0; i < defaultArgBuilders.Count; i++) {
                SimpleArgBuilder sab = defaultArgBuilders[i] as SimpleArgBuilder;
                if (sab != null) {
                    defaultArgBuilders[i] = sab.MakeCopy(curArg++);
                }
            }

            return new MethodCandidate(_resolver, _method, necessaryParams, _paramsDict, _returnBuilder, _instanceBuilder, defaultArgBuilders);
        }

        #endregion

        #region ReturnBuilder, Member Assigned Arguments

        private ReturnBuilder MakeReturnBuilder(BitArray specialParameters) {
            ReturnBuilder returnBuilder = (_returnArgs != null) ?
                new ByRefReturnBuilder(_returnArgs) :
                new ReturnBuilder(CompilerHelpers.GetReturnType(_method));
            
            if (_argNames.Count > 0 && _resolver.AllowKeywordArgumentSetting(_method)) {
                List<string> unusedNames = GetUnusedArgNames(specialParameters);
                List<MemberInfo> bindableMembers = GetBindableMembers(returnBuilder.ReturnType, unusedNames);
                if (unusedNames.Count == bindableMembers.Count) {
                    List<int> nameIndices = new List<int>();

                    foreach (MemberInfo mi in bindableMembers) {
                        var type = (mi.MemberType == MemberTypes.Property) ? ((PropertyInfo)mi).PropertyType : ((FieldInfo)mi).FieldType;
                        
                        _parameters.Add(new ParameterWrapper(type, mi.Name, false));
                        nameIndices.Add(_argNames.IndexOf(mi.Name));
                    }

                    return new KeywordConstructorReturnBuilder(
                        returnBuilder,
                        _argNames.Count,
                        nameIndices.ToArray(),
                        bindableMembers.ToArray(),
                        _resolver.Binder.PrivateBinding
                    );
                }

            }
            return returnBuilder;
        }

        private static List<MemberInfo> GetBindableMembers(Type returnType, List<string> unusedNames) {
            List<MemberInfo> bindableMembers = new List<MemberInfo>();

            foreach (string name in unusedNames) {
                Type curType = returnType;
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

        private List<string> GetUnusedArgNames(BitArray specialParameters) {
            List<string> unusedNames = new List<string>();
            foreach (string name in _argNames) {
                bool found = false;
                foreach (ParameterInfo pi in _parameterInfos) {
                    if (!IsSpecialParameter(specialParameters, pi.Position) && pi.Name == name) {
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

        #endregion
    }
}
