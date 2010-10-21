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

using System.Collections.Generic;
using IronPython.Runtime;
using IronPython.Runtime.Types;

namespace Microsoft.PyAnalysis.Values {
    internal class BuiltinPropertyInfo : BuiltinNamespace {
        private readonly ReflectedGetterSetter _value;
        private string _doc;

        public BuiltinPropertyInfo(ReflectedGetterSetter value, ProjectState projectState)
            : base(new LazyDotNetDict(value.PropertyType, projectState, true)) {
            _value = value;
            _doc = null;
            _type = _value.PropertyType;
        }

        public override ISet<Namespace> GetDescriptor(Namespace instance, Interpreter.AnalysisUnit unit) {
            return ((BuiltinClassInfo)ProjectState.GetNamespaceFromObjects(_value.PropertyType)).Instance.SelfSet;
        }

        public override ISet<Namespace> GetStaticDescriptor(Interpreter.AnalysisUnit unit) {
            ReflectedProperty rp = _value as ReflectedProperty;
            if (rp != null && (rp.Info.GetGetMethod() ?? rp.Info.GetSetMethod()).IsStatic) {
                BuiltinClassInfo klass = (BuiltinClassInfo)ProjectState.GetNamespaceFromObjects(rp.PropertyType);
                return klass.Instance.SelfSet;
            }

            return base.GetStaticDescriptor(unit);
        }

        public override string Description {
            get {
                var typeName = _type.__repr__(ProjectState.CodeContext);
                return "property of type " + typeName;
            }
        }

        public override ResultType ResultType {
            get {
                return ResultType.Property;
            }
        }

        public override string Documentation {
            get {
                if (_doc == null) {
                    if (_value is ReflectedProperty) {
                        _doc = Utils.StripDocumentation(((ReflectedProperty)_value).__doc__);
                    } else {
                        _doc = Utils.StripDocumentation(((ReflectedExtensionProperty)_value).__doc__);
                    }
                }
                return _doc;
            }
        }
    }
}
