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
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.PyAnalysis.Interpreter;
using Microsoft.Scripting.Utils;

namespace Microsoft.PyAnalysis.Values {
    internal class BuiltinEventInfo : BuiltinNamespace {
        private readonly ReflectedEvent _value;
        private string _doc;

        public BuiltinEventInfo(ReflectedEvent value, ProjectState projectState)
            : base(ClrModule.GetPythonType(value.Info.EventHandlerType), projectState) {
            _value = value;
            _doc = null;
            _type = ClrModule.GetPythonType(value.Info.EventHandlerType);
        }

        public override void AugmentAssign(AugmentedAssignStatement node, AnalysisUnit unit, ISet<Namespace> value) {
            base.AugmentAssign(node, unit, value);
            var args = GetEventInvokeArgs(ProjectState,  _type);
            foreach (var r in value) {
                r.Call(node, unit, args, ArrayUtils.EmptyStrings);
            }
        }

        internal static ISet<Namespace>[] GetEventInvokeArgs(ProjectState state, Type type) {
            var p = type.GetMethod("Invoke").GetParameters();

            var args = new ISet<Namespace>[p.Length];
            for (int i = 0; i < p.Length; i++) {
                args[i] = state.GetInstance(p[i].ParameterType).SelfSet;
            }
            return args;
        }

        public override string Description {
            get {
                return "event of type " + _value.Info.EventHandlerType.ToString();
            }
        }

        public override ResultType ResultType {
            get {
                return ResultType.Event;
            }
        }

        public override string Documentation {
            get {
                if (_doc == null) {
                    _doc = Utils.StripDocumentation(_value.__doc__);
                }
                return _doc;
            }
        }
    }
}
