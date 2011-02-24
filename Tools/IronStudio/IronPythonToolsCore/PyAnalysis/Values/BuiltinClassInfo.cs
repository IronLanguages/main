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
using System.Reflection;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.PyAnalysis.Interpreter;
using Microsoft.Scripting.Utils;

namespace Microsoft.PyAnalysis.Values {
    internal class BuiltinClassInfo : BuiltinNamespace, IReferenceableContainer {
        private BuiltinInstanceInfo _inst;
        private string _doc;
        private readonly MemberReferences _referencedMembers = new MemberReferences();
        private ReferenceDict _references;

        public BuiltinClassInfo(PythonType classObj, ProjectState projectState)
            : base(new LazyDotNetDict(classObj, projectState, true)) {
            // TODO: Get parameters from ctor
            // TODO: All types should be shared via projectState
            _type = classObj;
            _doc = null;
        }

        public override ISet<Namespace> Call(Node node, AnalysisUnit unit, ISet<Namespace>[] args, string[] keywordArgNames) {
            // TODO: More Type propagation

            if (args.Length == 1 && typeof(Delegate).IsAssignableFrom(_type.__clrtype__())) {
                var invokeArgs = BuiltinEventInfo.GetEventInvokeArgs(ProjectState, _type.__clrtype__());
                foreach (var arg in args) {
                    arg.Call(node, unit, invokeArgs, ArrayUtils.EmptyStrings);
                }
            }
            return Instance.SelfSet;
        }

        public override bool IsBuiltin {
            get {
                return true;
            }
        }

        public BuiltinInstanceInfo Instance {
            get {
                return _inst ?? (_inst = MakeInstance());
            }
        }

        private BuiltinInstanceInfo MakeInstance() {
            if (_type == TypeCache.Int32 || _type == TypeCache.BigInteger || _type == TypeCache.Double || _type == TypeCache.Complex) {
                return new NumericInstanceInfo(this);
            }

            return new BuiltinInstanceInfo(this);
        }

        /// <summary>
        /// Returns the overloads avaialble for calling the constructor of the type.
        /// </summary>
        public override ICollection<OverloadResult> Overloads {
            get {
                // TODO: sometimes might have a specialized __init__.
                // This just covers typical .NET types
                var clrType = ClrModule.GetClrType(_type);
                if (!IsPythonType) {
                    var newMethods = clrType.GetMember("__new__", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static);
                    var initMethods = clrType.GetMember("__init__", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance);
                    if (newMethods.Length == 0 && initMethods.Length == 0) {
                        return GetClrOverloads();
                    }
                } else if (clrType == typeof(object)) {
                    return GetClrOverloads();
                }

                object ctor;
                if (!ProjectState.TryGetMember(_type, "__new__", out ctor)) {
                    ctor = null;
                }
                var func = ctor as BuiltinFunction;
                if (func == null) {
                    return new OverloadResult[0];
                }

                var result = new OverloadResult[func.Overloads.Functions.Count];
                for (int i = 0; i < result.Length; i++) {
                    var bf = func.Overloads.Functions[i] as BuiltinFunction;
                    result[i] = new BuiltinFunctionOverloadResult(ProjectState, bf, 1, PythonType.Get__name__(_type));
                }
                return result;
            }
        }

        /// <summary>
        /// Returns the overloads for a normal .NET type
        /// </summary>
        private OverloadResult[] GetClrOverloads() {
            Type clrType = ClrModule.GetClrType(_type);
            // just a normal .NET type...
            var ctors = clrType.GetConstructors(BindingFlags.Public | BindingFlags.CreateInstance | BindingFlags.Instance);

            var overloads = new OverloadResult[ctors.Length];
            for (int i = 0; i < ctors.Length; i++) {
                // TODO: Docs, python type name
                var parameters = ctors[i].GetParameters();
                bool hasContext = parameters.Length > 0 && parameters[0].ParameterType == typeof(CodeContext);

                var paramResult = new ParameterResult[hasContext ? parameters.Length - 1 : parameters.Length];

                for (int j = 0; j < paramResult.Length; j++) {
                    var curParam = parameters[j + (hasContext ? 1 : 0)];
                    // TODO: Docs
                    paramResult[j] = BuiltinFunctionOverloadResult.GetParameterResultFromParameterInfo(curParam);
                }
                overloads[i] = new SimpleOverloadResult(paramResult, PythonType.Get__name__(_type), "");
            }

            return overloads;
        }

        public override ISet<Namespace> GetMember(Node node, AnalysisUnit unit, string name) {
            var res = base.GetMember(node, unit, name);
            if (res.Count > 0) {
                _referencedMembers.AddReference(node, unit, name);
                return res.GetStaticDescriptor(unit);
            }
            return res;
        }

        public override void SetMember(Node node, AnalysisUnit unit, string name, ISet<Namespace> value) {
            var res = base.GetMember(node, unit, name);
            if (res.Count > 0) {
                _referencedMembers.AddReference(node, unit, name);
            }
        }

        public override ISet<Namespace> GetIndex(Node node, AnalysisUnit unit, ISet<Namespace> index) {
            // TODO: Needs to actually do indexing on type
            var clrType = _type.__clrtype__();
            if (!clrType.IsGenericTypeDefinition) {
                return EmptySet<Namespace>.Instance;
            }
            
            var result = new HashSet<Namespace>();
            foreach (var indexType in index) {
                if (indexType is BuiltinClassInfo) {
                    var clrIndexType = indexType.PythonType.__clrtype__();
                    try {
                        var klass = ProjectState.MakeGenericType(clrType, clrIndexType);
                        result.Add(klass);
                    } catch {
                        // wrong number of type args, violated constraint, etc...
                    }
                } else if (indexType is SequenceInfo) {
                    List<Type>[] types = GetSequenceTypes(indexType as SequenceInfo);

                    if (!MissingType(types)) {
                        foreach (Type[] indexTypes in GetTypeCombinations(types)) {                            
                            try {
                                var klass = ProjectState.MakeGenericType(clrType, indexTypes);
                                result.Add(klass);
                            } catch {
                                // wrong number of type args, violated constraint, etc...
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static IEnumerable<Type[]> GetTypeCombinations(List<Type>[] types) {
            List<Type> res = new List<Type>();
            for (int i = 0; i < types.Length; i++) {
                res.Add(null);
            }

            return GetTypeCombinationsWorker(types, res, 0);
        }

        private static IEnumerable<Type[]> GetTypeCombinationsWorker(List<Type>[] types, List<Type> res, int curIndex) {
            if (curIndex == types.Length) {
                yield return res.ToArray();
            } else {
                foreach (Type t in types[curIndex]) {
                    res[curIndex] = t;

                    foreach (var finalRes in GetTypeCombinationsWorker(types, res, curIndex + 1)) {
                        yield return finalRes;
                    }
                }
            }
        }

        private static List<Type>[] GetSequenceTypes(SequenceInfo seq) {
            List<Type>[] types = new List<Type>[seq.IndexTypes.Length];
            for (int i = 0; i < types.Length; i++) {
                foreach (var seqIndexType in seq.IndexTypes[i]) {
                    if (seqIndexType is BuiltinClassInfo) {
                        if (types[i] == null) {
                            types[i] = new List<Type>();
                        }

                        types[i].Add(seqIndexType.PythonType.__clrtype__());
                    }
                }
            }
            return types;
        }

        private static bool MissingType(List<Type>[] types) {
            for (int i = 0; i < types.Length; i++) {
                if (types[i] == null) {
                    return true;
                }
            }
            return false;
        }

        public override string Description {
            get {
                return "type " + PythonType.Get__name__(_type);
            }
        }

        public override string Documentation {
            get {
                if (_doc == null) {
                    try {
                        var doc = PythonType.Get__doc__(ProjectState.CodeContext, _type);
                        _doc = Utils.StripDocumentation(doc.ToString());
                    } catch {
                        _doc = String.Empty;
                    }
                }
                return _doc;
            }
        }

        public override ResultType ResultType {
            get {
                var type = _type.__clrtype__();
                if (type.IsEnum) {
                    return ResultType.Enum;
                } else if (typeof(Delegate).IsAssignableFrom(type)) {
                    return ResultType.Delegate;
                } else {
                    return ResultType.Class;
                }
            }
        }

        public override string ToString() {
            // return 'Class#' + hex(id(self)) + ' ' + self.clrType.__name__
            return "Class " + PythonType.Get__name__(_type);
        }

        public bool IsPythonType {
            get {
                return _type == TypeCache.String ||
                    _type == TypeCache.Object ||
                    _type == TypeCache.Double ||
                    _type == TypeCache.Complex ||
                    _type == TypeCache.Boolean;
            }
        }

        #region IReferenceableContainer Members

        public IEnumerable<IReferenceable> GetDefinitions(string name) {
            return _referencedMembers.GetDefinitions(name);
        }

        #endregion

        internal void AddMemberReference(Node node, AnalysisUnit unit, string name) {
            _referencedMembers.AddReference(node, unit, name);
        }

        internal override void AddReference(Node node, AnalysisUnit unit) {
            if (!unit.ForEval) {
                if (_references == null) {
                    _references = new ReferenceDict();
                }
                _references.GetReferences(unit.DeclaringModule.ProjectEntry).References.Add(new SimpleSrcLocation(node.Span));
            }
        }

        public override IEnumerable<LocationInfo> References {
            get {
                if (_references != null) {
                    return _references.AllReferences;
                }
                return new LocationInfo[0];
            }
        }
    }
}
