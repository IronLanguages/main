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
using System.Text;
using Microsoft.Contracts;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions.Calls {
    /// <summary>
    /// MethodCandidate represents the different possible ways of calling a method or a set of method overloads.
    /// A single method can result in multiple MethodCandidates. Some reasons include:
    /// - Every optional parameter or parameter with a default value will result in a candidate
    /// - The presence of ref and out parameters will add a candidate for languages which want to return the updated values as return values.
    /// - ArgumentKind.List and ArgumentKind.Dictionary can result in a new candidate per invocation since the list might be different every time.
    ///
    /// Each MethodCandidate represents the parameter type for the candidate using ParameterWrapper.
    /// 
    /// Contrast this with MethodTarget which represents the real physical invocation of a method
    /// </summary>
    class MethodCandidate {
        private MethodTarget _target;
        private List<ParameterWrapper> _parameters;
        private NarrowingLevel _narrowingLevel;

        internal MethodCandidate(MethodCandidate previous, NarrowingLevel narrowingLevel) {
            this._target = previous.Target;
            this._parameters = previous._parameters;
            _narrowingLevel = narrowingLevel;
        }

        internal MethodCandidate(MethodTarget target, List<ParameterWrapper> parameters) {
            Debug.Assert(target != null);

            _target = target;
            _parameters = parameters;
            _narrowingLevel = NarrowingLevel.None;
            parameters.TrimExcess();
        }

        public MethodTarget Target {
            get { return _target; }
        }

        public NarrowingLevel NarrowingLevel {
            get {
                return _narrowingLevel;
            }
        }

        [Confined]
        public override string ToString() {
            return string.Format("MethodCandidate({0})", Target);
        }

        internal bool IsApplicable(Type[] types, NarrowingLevel narrowingLevel, List<ConversionResult> conversionResults) {
            // attempt to convert each parameter
            bool res = true;
            for (int i = 0; i < types.Length; i++) {
                bool success = _parameters[i].HasConversionFrom(types[i], narrowingLevel);

                conversionResults.Add(new ConversionResult(types[i], _parameters[i].Type, i, !success));

                res &= success;
            }

            return res;
        }

        internal bool IsApplicable(DynamicMetaObject[] objects, NarrowingLevel narrowingLevel, List<ConversionResult> conversionResults) {
            // attempt to convert each parameter
            bool res = true;
            for (int i = 0; i < objects.Length; i++) {
                /*if (objects[i].NeedsDeferral) {
                    conversionResults.Add(new ConversionResult(typeof(Dynamic), _parameters[i].Type, i, false));
                } else*/ {
                    bool success = _parameters[i].HasConversionFrom(objects[i].GetLimitType(), narrowingLevel);

                    conversionResults.Add(new ConversionResult(objects[i].GetLimitType(), _parameters[i].Type, i, !success));

                    res &= success;
                }
            }

            return res;
        }

        internal static Candidate GetPreferredCandidate(MethodCandidate one, MethodCandidate two, CallTypes callType, Type[] actualTypes) {
            Candidate cmpParams = ParameterWrapper.GetPreferredParameters(one.Parameters, two.Parameters, actualTypes);
            if (cmpParams.Chosen()) {
                return cmpParams;
            }

            Candidate ret = MethodTarget.CompareEquivalentParameters(one.Target, two.Target);
            if (ret.Chosen()) {
                return ret;
            }

            if (CompilerHelpers.IsStatic(one.Target.Method) && !CompilerHelpers.IsStatic(two.Target.Method)) {
                return callType == CallTypes.ImplicitInstance ? Candidate.Two : Candidate.One;
            } else if (!CompilerHelpers.IsStatic(one.Target.Method) && CompilerHelpers.IsStatic(two.Target.Method)) {
                return callType == CallTypes.ImplicitInstance ? Candidate.One : Candidate.Two;
            }

            return Candidate.Equivalent;
        }

        internal static Candidate GetPreferredCandidate(MethodCandidate one, MethodCandidate two, CallTypes callType, DynamicMetaObject[] actualTypes) {
            Candidate cmpParams = ParameterWrapper.GetPreferredParameters(one.Parameters, two.Parameters, actualTypes);
            if (cmpParams.Chosen()) {
                return cmpParams;
            }

            Candidate ret = MethodTarget.CompareEquivalentParameters(one.Target, two.Target);
            if (ret.Chosen()) {
                return ret;
            }

            if (CompilerHelpers.IsStatic(one.Target.Method) && !CompilerHelpers.IsStatic(two.Target.Method)) {
                return callType == CallTypes.ImplicitInstance ? Candidate.Two : Candidate.One;
            } else if (!CompilerHelpers.IsStatic(one.Target.Method) && CompilerHelpers.IsStatic(two.Target.Method)) {
                return callType == CallTypes.ImplicitInstance ? Candidate.One : Candidate.Two;
            }

            return Candidate.Equivalent;
        }


        /// <summary>
        /// Builds a new MethodCandidate which takes count arguments and the provided list of keyword arguments.
        /// 
        /// The basic idea here is to figure out which parameters map to params or a dictionary params and
        /// fill in those spots w/ extra ParameterWrapper's.  
        /// </summary>
        internal MethodCandidate MakeParamsExtended(ActionBinder binder, int count, string[] names) {
            Debug.Assert(BinderHelpers.IsParamsMethod(_target.Method));

            List<ParameterWrapper> newParameters = new List<ParameterWrapper>(count);
            
            // keep track of which kw args map to a real argument, and which ones
            // map to the params dictionary.
            List<string> unusedNames = new List<string>(names);
            List<int> unusedNameIndexes = new List<int>();
            for (int i = 0; i < unusedNames.Count; i++) {
                unusedNameIndexes.Add(i);
            }

            // if we don't have a param array we'll have a param dict which is type object
            ParameterWrapper paramsArrayParameter = null, paramsDictParameter = null;
            int paramsArrayIndex = -1, paramsDictIndex = -1;

            for (int i = 0; i < _parameters.Count; i++) {
                ParameterWrapper parameter = _parameters[i];

                if (parameter.IsParamsDict) {
                    paramsDictParameter = parameter;
                    paramsDictIndex = i;
                } else if (parameter.IsParamsArray) {
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
                Type elementType = paramsArrayParameter.Type.GetElementType();
                bool nonNullItems = CompilerHelpers.ProhibitsNullItems(paramsArrayParameter.ParameterInfo);

                while (newParameters.Count < (count - unusedNames.Count)) {
                    ParameterWrapper param = new ParameterWrapper(binder, paramsArrayParameter.ParameterInfo, elementType, null, nonNullItems, false, false);
                    newParameters.Insert(System.Math.Min(paramsArrayIndex, newParameters.Count), param);
                }
            }

            if (paramsDictIndex != -1) {
                Type elementType = paramsDictParameter.Type.GetElementType();
                bool nonNullItems = CompilerHelpers.ProhibitsNullItems(paramsDictParameter.ParameterInfo);
                
                foreach (string si in unusedNames) {
                    newParameters.Add(new ParameterWrapper(binder, paramsDictParameter.ParameterInfo, typeof(object), si, nonNullItems, false, false));
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

            return new MethodCandidate(_target.MakeParamsExtended(count, unusedNames.ToArray(), unusedNameIndexes.ToArray()), newParameters);
        }

        internal string ToSignatureString(string name, CallTypes callType) {
            StringBuilder buf = new StringBuilder(name);
            buf.Append("(");
            bool isFirstArg = true;
            int i = 0;
            if (callType == CallTypes.ImplicitInstance) i = 1;
            for (; i < _parameters.Count; i++) {
                if (isFirstArg) isFirstArg = false;
                else buf.Append(", ");
                buf.Append(_parameters[i].ToSignatureString());
            }
            buf.Append(")");
            return buf.ToString(); //@todo add helper info for more interesting signatures
        }

        internal IList<ParameterWrapper> Parameters {
            get {
                return _parameters;
            }
        }

        internal bool HasParamsDictionary() {
            foreach (ParameterWrapper pw in _parameters) {
                // can't bind to methods that are params dictionaries, only to their extended forms.
                if (pw.IsParamsDict) return true;
            }
            return false;
        }

        internal bool TryGetNormalizedArguments<T>(T[] argTypes, string[] names, out T[] args, out CallFailure failure) {
            if (names.Length == 0) {
                // no named arguments, success!
                args = argTypes;
                failure = null;
                return true;
            }

            T[] res = new T[argTypes.Length];
            Array.Copy(argTypes, res, argTypes.Length - names.Length);
            List<string> unboundNames = null;
            List<string> duppedNames = null;

            for (int i = 0; i < names.Length; i++) {
                bool found = false;
                for (int j = 0; j < _parameters.Count; j++) {
                    if (_parameters[j].Name == names[i]) {
                        found = true;

                        if (res[j] != null) {
                            if (duppedNames == null) duppedNames = new List<string>();
                            duppedNames.Add(names[i]);
                        } else {
                            res[j] = argTypes[i + argTypes.Length - names.Length];
                        }
                        break;
                    }
                }

                if (!found) {
                    if (unboundNames == null) unboundNames = new List<string>();
                    unboundNames.Add(names[i]);
                }
            }

            if (unboundNames != null) {
                failure = new CallFailure(Target, unboundNames.ToArray(), true);
                args = null;
                return false;
            } else if (duppedNames != null) {
                failure = new CallFailure(Target, duppedNames.ToArray(), false);
                args = null;
                return false;
            }

            failure = null;
            args = res;
            return true;
        }
    }

}
