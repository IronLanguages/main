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

namespace Microsoft.Scripting.Debugging {
    public sealed class FunctionInfo {
        private readonly Delegate _generatorFactory;
        private readonly string _name;
        private int _version;
        private FunctionInfo _prevVersion;
        private FunctionInfo _nextVersion;
        private readonly DebugSourceSpan[] _sequencePoints;
        private readonly IList<VariableInfo>[] _variableScopeMap;
        private readonly IList<VariableInfo> _variables;
        private readonly object _customPayload;
        private readonly bool[] _traceLocations;

        internal FunctionInfo(
            Delegate generatorFactory,
            string name,
            DebugSourceSpan[] sequencePoints,
            IList<VariableInfo>[] scopedVariables,
            IList<VariableInfo> variables,
            object customPayload) {

            _generatorFactory = generatorFactory;
            _name = name;
            _sequencePoints = sequencePoints;
            _variableScopeMap = scopedVariables;
            _variables = variables;
            _customPayload = customPayload;
            _traceLocations = new bool[sequencePoints.Length];
        }

        internal Delegate GeneratorFactory {
            get { return _generatorFactory; }
        }

        internal IList<VariableInfo> Variables {
            get { return _variables; }
        }

        internal IList<VariableInfo>[] VariableScopeMap {
            get { return _variableScopeMap; }
        }

        internal FunctionInfo PreviousVersion {
            get {
                return _prevVersion;
            }
            set {
                _prevVersion = value;
            }
        }

        internal FunctionInfo NextVersion {
            get {
                return _nextVersion;
            }
            set {
                _nextVersion = value;
            }
        }

        internal int Version {
            get {
                return _version;
            }
            set {
                _version = value;
            }
        }

        /// <summary>
        /// SequencePoints
        /// </summary>
        internal DebugSourceSpan[] SequencePoints {
            get { return _sequencePoints; }
        }

        /// <summary>
        /// Name
        /// </summary>
        internal string Name {
            get { return _name; }
        }

        /// <summary>
        /// CustomPayload
        /// </summary>
        internal object CustomPayload {
            get { return _customPayload; }
        }

        /// <summary>
        /// GetTraceLocations
        /// </summary>
        /// <returns></returns>
        internal bool[] GetTraceLocations() {
            return _traceLocations;
        }
    }
}
