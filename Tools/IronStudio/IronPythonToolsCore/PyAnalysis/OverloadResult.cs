/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace Microsoft.PyAnalysis {
    public class OverloadResult : IOverloadResult {
        private readonly ParameterResult[] _parameters;
        private readonly string _name;

        public OverloadResult(ParameterResult[] parameters, string name) {
            _parameters = parameters;
            _name = name;
        }

        public string Name {
            get { return _name; }
        }
        public virtual string Documentation {
            get { return null; }
        }
        public virtual ParameterResult[] Parameters {
            get { return _parameters; }
        }
    }

    public class SimpleOverloadResult : OverloadResult {
        private readonly string _documentation;
        public SimpleOverloadResult(ParameterResult[] parameters, string name, string documentation)
            : base(parameters, name) {
            _documentation = ParameterResult.Trim(documentation);
        }

        public override string Documentation {
            get {
                return _documentation;
            }
        }
    }

    public class BuiltinFunctionOverloadResult : OverloadResult {
        private static readonly string _codeCtxType = "IronPython.Runtime.CodeContext";
        private readonly BuiltinFunction _overload;
        private ParameterResult[] _parameters;
        private readonly ParameterResult[] _extraParameters;
        private readonly int _removedParams;
        private readonly ProjectState _projectState;
        private string _doc;
        private static readonly string _calculating = "Documentation is still being calculated, please try again soon.";

        internal BuiltinFunctionOverloadResult(ProjectState state, BuiltinFunction overload, int removedParams, params ParameterResult[] extraParams)
            : base(null, overload.__name__) {
            _overload = overload;
            _extraParameters = extraParams;
            _removedParams = removedParams;
            _projectState = state;

            CalculateDocumentation();
        }

        internal BuiltinFunctionOverloadResult(ProjectState state, BuiltinFunction overload, int removedParams, string name, params ParameterResult[] extraParams)
            : base(null, name) {
            _overload = overload;
            _extraParameters = extraParams;
            _removedParams = removedParams;
            _projectState = state;

            CalculateDocumentation();
        }

        public override string Documentation {
            get {
                return _doc;
            }
        }

        private void CalculateDocumentation() {
            // initially fill in w/ a string saying we don't yet have the documentation
            _doc = _calculating;
            
            // and then asynchronously calculate the documentation
            ThreadPool.QueueUserWorkItem(
                x => {
                    var overloadDoc = _projectState.DocProvider.GetOverloads(_overload).First();
                    StringBuilder doc = new StringBuilder();
                    if (!String.IsNullOrEmpty(overloadDoc.Documentation)) {
                        doc.AppendLine(overloadDoc.Documentation);
                    }

                    foreach (var param in overloadDoc.Parameters) {
                        if (!String.IsNullOrEmpty(param.Documentation)) {
                            doc.AppendLine();
                            doc.Append(param.Name);
                            doc.Append(": ");
                            doc.Append(param.Documentation);
                        }
                    }

                    if (!String.IsNullOrEmpty(overloadDoc.ReturnParameter.Documentation)) {
                        doc.AppendLine();
                        doc.AppendLine();
                        doc.Append("Returns: ");
                        doc.Append(overloadDoc.ReturnParameter.Documentation);
                    }
                    _doc = doc.ToString();
                }
            );
        }

        public override ParameterResult[] Parameters {
            get {
                if (_parameters == null) {
                    if (_overload != null && _overload.Targets.Count > 0) {
                        Debug.Assert(_overload.Targets.Count == 1, "we should always get BuiltinFunctions via .Overloads.Functions which should only have 1 function each");
                        var target = _overload.Targets[0];

                        bool isInstanceExtensionMethod = false;
                        if (!target.DeclaringType.IsAssignableFrom(_overload.DeclaringType)) {
                            // extension method
                            isInstanceExtensionMethod = !target.IsDefined(typeof(StaticExtensionMethodAttribute), false);
                        }

                        var pinfo = _overload.Targets[0].GetParameters();
                        var result = new List<ParameterResult>(pinfo.Length + _extraParameters.Length);
                        int ignored = 0;
                        ParameterResult kwDict = null;
                        foreach (var param in pinfo) {
                            if (result.Count == 0 && param.ParameterType.FullName == _codeCtxType) {
                                continue;
                            } else if (result.Count == 0 && isInstanceExtensionMethod) {
                                // skip instance parameter
                                isInstanceExtensionMethod = false;
                                continue;
                            } else if (ignored < _removedParams) {
                                ignored++;
                            } else {
                                var paramResult = GetParameterResultFromParameterInfo(param);
                                if (param.IsDefined(typeof(ParamDictionaryAttribute), false)) {
                                    kwDict = paramResult;
                                } else {
                                    result.Add(paramResult);
                                }
                            }
                        }

                        result.InsertRange(0, _extraParameters);

                        // always add kw dict last.  When defined in C# and combined w/ params 
                        // it has to come earlier than it's legally allowed in Python so we 
                        // move it to the end for intellisense purposes here.
                        if (kwDict != null) {
                            result.Add(kwDict);
                        }
                        _parameters = result.ToArray();
                    } else {
                        _parameters = new ParameterResult[0];
                    }
                }
                return _parameters;
            }
        }

        internal static ParameterResult GetParameterResultFromParameterInfo(ParameterInfo param) {
            // TODO: Get parameter documentation
            var pyType = ClrModule.GetPythonType(param.ParameterType);

            string name = param.Name;
            string typeName = PythonType.Get__name__(pyType);
            if (param.IsDefined(typeof(ParamArrayAttribute), false)) {
                name = "*" + name;
                if (param.ParameterType.IsArray) {
                    var elemType = param.ParameterType.GetElementType();
                    if (elemType == typeof(object)) {
                        typeName = "sequence";
                    } else {
                        typeName = PythonType.Get__name__(DynamicHelpers.GetPythonTypeFromType(elemType)) + " sequence";
                    }
                }
            } else if (param.IsDefined(typeof(ParamDictionaryAttribute), false)) {
                name = "**" + name;
                typeName = "object";
            }

            bool isOptional = false;
            if (param.DefaultValue != DBNull.Value && !(param.DefaultValue is Missing)) {
                name = name + " = " + PythonOps.Repr(DefaultContext.Default, param.DefaultValue);
            } else if (param.IsOptional) {
                object missing = CompilerHelpers.GetMissingValue(param.ParameterType);
                if (missing != Missing.Value) {
                    name = name + " = " + PythonOps.Repr(DefaultContext.Default, missing);
                } else {
                    isOptional = true;
                }
            }

            return new ParameterResult(name, "", typeName, isOptional);
        }
    }
}
