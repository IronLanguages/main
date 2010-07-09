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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Debugging.CompilerServices;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Debugging {
    public sealed class DebugFrame {
        private readonly DebugThread _thread;
        private FunctionInfo _funcInfo;
        private int _stackDepth;
        private Exception _thrownException;
        private IRuntimeVariables _liftedLocals;
        private IDebuggableGenerator _generator;
        private int _lastKnownGeneratorYieldMarker = Int32.MaxValue;
        private bool _inTraceBack;
        private bool _inGeneratorLoop;
        private bool _forceToGeneratorLoop;
        private Dictionary<IList<VariableInfo>, ScopeData> _variables;

        // Symbol used to set "$exception" variable when exceptions are thrown
        private const string _exceptionVariableSymbol = "$exception";

        internal DebugFrame(
            DebugThread thread,
            FunctionInfo funcInfo) {
            _thread = thread;
            _funcInfo = funcInfo;
            _variables = new  Dictionary<IList<VariableInfo>, ScopeData>();
        }

        internal DebugFrame(
            DebugThread thread,
            FunctionInfo funcInfo,
            IRuntimeVariables liftedLocals,
            int frameOrder)
            : this(thread, funcInfo) {
            _liftedLocals = liftedLocals;
            _stackDepth = frameOrder;
        }

        #region Internal members

        /// <summary>
        /// Thread
        /// </summary>
        internal DebugThread Thread {
            get { return _thread; }
        }

        /// <summary>
        /// FrameOrder
        /// </summary>
        internal int StackDepth {
            get { return _stackDepth; }
            set { _stackDepth = value; }
        }
        
        /// <summary>
        /// Variables
        /// </summary>
        internal VariableInfo[] Variables {
            get {
                ScopeData scopeData = CurrentScopeData;
                VariableInfo[] variables;
                if (_thrownException == null) {
                    variables = scopeData.VarInfos;
                } else {
                    variables = scopeData.VarInfosWithException;
                }
                if (variables == null) {
                    List<VariableInfo> visibleInfos = new List<VariableInfo>();
                    List<VariableInfo> visibleInfosWithException;

                    // Add parameters
                    foreach (VariableInfo varInfo in _funcInfo.Variables) {
                        if (varInfo.IsParameter && !varInfo.Hidden) {
                            visibleInfos.Add(varInfo);
                        }
                    }

                    // Add locals
                    foreach (VariableInfo varInfo in LocalsInCurrentScope) {
                        if (!varInfo.Hidden) {
                            visibleInfos.Add(varInfo);
                        }
                    }

                    visibleInfosWithException = new List<VariableInfo>(visibleInfos);
                    visibleInfosWithException.Add(new VariableInfo(_exceptionVariableSymbol, typeof(Exception), false, false, false));

                    scopeData.VarInfos = visibleInfos.ToArray();
                    scopeData.VarInfosWithException = visibleInfosWithException.ToArray();

                    if (_thrownException == null) {
                        variables = scopeData.VarInfos;
                    } else {
                        variables = scopeData.VarInfosWithException;
                    }
                }

                return variables;
            }
        }

        /// <summary>
        /// CurrentSequencePointIndex
        /// </summary>
        internal int CurrentSequencePointIndex {
            get {
                int debugMarker = CurrentLocationCookie;
                if (debugMarker >= _funcInfo.SequencePoints.Length) {
#if !SILVERLIGHT
                    Debug.Fail("DebugMarker doesn't match any location");
#endif
                    debugMarker = 0;
                }

                return debugMarker;
            }
            set {
                if (value < 0 || value >= _funcInfo.SequencePoints.Length) {
                    throw new ArgumentOutOfRangeException("value");
                }

                // The location can only be changed in leaf frames which are inside trace events
                if (!_inTraceBack) {
                    Debug.Assert(false, "frame not in trace event");
                    throw new InvalidOperationException(ErrorStrings.JumpNotAllowedInNonLeafFrames);
                }

                bool needsGenerator = (value != CurrentLocationCookie || _thrownException != null);

                // Remap to a generator if we're changing to a different location or if there's a thrown exception
                if (_generator == null && needsGenerator) {
                    RemapToGenerator(_funcInfo.Version);
                    Debug.Assert(_generator != null);
                }

                // change location only if really needed
                if (value != CurrentLocationCookie) {
                    Debug.Assert(_generator != null);
                    _generator.YieldMarkerLocation = value;
                }

                // Regardless of whether the location is changed or not, the pending
                // exception needs to be canceled.
                ThrownException = null;

                // If the current event is not coming from the generator loop,
                // we need to force it to go into the loop.
                if (!_inGeneratorLoop && needsGenerator)
                    _forceToGeneratorLoop = true;
            }
        }

        internal void RemapToLatestVersion() {
            RemapToGenerator(Int32.MaxValue);

            // Force to generator loop
            if (!_inGeneratorLoop)
                _forceToGeneratorLoop = true;
        }

        internal FunctionInfo FunctionInfo {
            get {
                return _funcInfo;
            }
        }

        internal Exception ThrownException {
            get { return _thrownException; }
            set {
                if (_thrownException != null && value == null) {
                    _thrownException = null;
                    GetLocalsScope().Remove(_exceptionVariableSymbol);
                } else if (value != null && !GetLocalsScope().ContainsKey(_exceptionVariableSymbol)) {
                    _thrownException = value;
                    GetLocalsScope()[_exceptionVariableSymbol] = _thrownException;
                }
            }
        }

        internal IDebuggableGenerator Generator {
            get { return _generator; }
        }

        internal bool IsInTraceback {
            get { return _inTraceBack; }
            set { _inTraceBack = value; }
        }

        internal bool InGeneratorLoop {
            get { return _inGeneratorLoop; }
            set { _inGeneratorLoop = value; }
        }

        internal bool ForceSwitchToGeneratorLoop {
            get { return _forceToGeneratorLoop; }
            set { _forceToGeneratorLoop = value; }
        }

        internal DebugContext DebugContext {
            get { return _thread.DebugContext; }
        }

        internal int CurrentLocationCookie {
            get {
                Debug.Assert(_generator != null || _liftedLocals is IDebugRuntimeVariables);
                return (_generator == null ? ((IDebugRuntimeVariables)_liftedLocals).DebugMarker : 
                    (_generator.YieldMarkerLocation != Int32.MaxValue ? _generator.YieldMarkerLocation : _lastKnownGeneratorYieldMarker));
            }
        }

        internal int LastKnownGeneratorYieldMarker {
            get { return _lastKnownGeneratorYieldMarker; }
            set { _lastKnownGeneratorYieldMarker = value; }
        }

        /// <summary>
        /// // This method is called from the generator to update the frame with generator's locals
        /// </summary>
        internal void ReplaceLiftedLocals(IRuntimeVariables liftedLocals) {
            Debug.Assert(_liftedLocals == null || liftedLocals.Count >= _liftedLocals.Count);

            IRuntimeVariables oldLiftecLocals = _liftedLocals;

            // Replace the list of IStrongBoxes with the new list
            _liftedLocals = liftedLocals;

            if (oldLiftecLocals != null) {
                for (int i = 0; i < oldLiftecLocals.Count; i++) {
                    if (!_funcInfo.Variables[i].IsParameter && i < _liftedLocals.Count)
                        _liftedLocals[i] = oldLiftecLocals[i];
                }
            }

            // Null out scope/variable states to force creation of new ones
            _variables.Clear();
        }

        /// <summary>
        /// Remaps the frame's state to use the generator for execution.
        /// </summary>
        /// <param name="version">Int32.MaxValue to map to latest version</param>
        internal void RemapToGenerator(int version) {
            Debug.Assert(_generator == null || _funcInfo.Version != version);

            // Try to find the target FunctionInfo for the specified version
            FunctionInfo targetFuncInfo = GetFunctionInfo(version);
            if (targetFuncInfo == null) {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ErrorStrings.InvalidFunctionVersion, version));
            }

            // Create the new generator
            CreateGenerator(targetFuncInfo);

            // Run to the first yield point
            ((IEnumerator)_generator).MoveNext();
        }

        internal IDictionary<object, object> GetLocalsScope() {
            ScopeData scopeData = CurrentScopeData;
            IDictionary<object, object> scope = scopeData.Scope;
            if (scope == null) {
                Debug.Assert(_liftedLocals != null);

                List<string> visibleSymbols = new List<string>();
                List<VariableInfo> visibleLocals = new List<VariableInfo>();

                // Add parameters
                for (int i = 0; i < _funcInfo.Variables.Count; i++) {
                    if (_funcInfo.Variables[i].IsParameter && !_funcInfo.Variables[i].Hidden) {
                        visibleSymbols.Add(_funcInfo.Variables[i].Name);
                        visibleLocals.Add(_funcInfo.Variables[i]);
                    }
                }

                // Add locals
                foreach (VariableInfo varInfo in LocalsInCurrentScope) {
                    if (!varInfo.Hidden) {
                        visibleSymbols.Add(varInfo.Name);
                        visibleLocals.Add(varInfo);
                    }
                }

                IRuntimeVariables scopedLocals = new ScopedRuntimeVariables(visibleLocals, _liftedLocals);

                scope = new LocalsDictionary(scopedLocals, visibleSymbols.ToArray());

                scopeData.Scope = scope;
            }

            return scope;
        }

        #endregion

        #region Private helpers
        private void CreateGenerator(FunctionInfo targetFuncInfo) {
            object[] paramValues = GetParamValuesForGenerator();
            _generator = (IDebuggableGenerator)targetFuncInfo.GeneratorFactory.GetType().GetMethod("Invoke").Invoke(targetFuncInfo.GeneratorFactory, paramValues);

            // Update funcInfo to the new version
            if (_funcInfo != targetFuncInfo) {
                _funcInfo = targetFuncInfo;
            }
        }

        private object[] GetParamValuesForGenerator() {
            List<object> paramValues = new List<object>();

            // First parameter is frame
            paramValues.Add(this);

            for (int i = 0; i < _funcInfo.Variables.Count; i++) {
                if (_funcInfo.Variables[i].IsParameter) {
                    paramValues.Add(_liftedLocals[i]);
                }
            }

            return paramValues.ToArray();
        }

        private FunctionInfo GetFunctionInfo(int version) {
            if (version == _funcInfo.Version)
                return _funcInfo;

            FunctionInfo funcInfo = _funcInfo;
            FunctionInfo lastFuncInfo = null;
            while (funcInfo != null) {
                if (funcInfo.Version == version) {
                    return funcInfo;
                }

                lastFuncInfo = funcInfo;

                if (version > funcInfo.Version) {
                    funcInfo = funcInfo.NextVersion;
                } else {
                    funcInfo = funcInfo.PreviousVersion;
                }
            }

            // if version is Int32.MaxValue return the latest factory
            if (version == Int32.MaxValue)
                return lastFuncInfo;

            return null;
        }

        private ScopeData CurrentScopeData {
            get {
                IList<VariableInfo> scopedVars = CurrentLocationCookie < _funcInfo.VariableScopeMap.Length ? _funcInfo.VariableScopeMap[CurrentLocationCookie] : null;
                if (scopedVars == null) {
#if !SILVERLIGHT
                    Debug.Fail("DebugMarker doesn't match any scope");
#endif
                    // We use null as a key into the tuple that holds variables for "invalid" locations
                    scopedVars = _funcInfo.VariableScopeMap[0];
                }

                ScopeData scopeData;
                if (!_variables.TryGetValue(scopedVars, out scopeData)) {
                    scopeData = new ScopeData();
                    _variables.Add(scopedVars, scopeData);
                }

                return scopeData;
            }
        }

        private IList<VariableInfo> LocalsInCurrentScope {
            get {
                IList<VariableInfo> locals = CurrentLocationCookie < _funcInfo.VariableScopeMap.Length ? _funcInfo.VariableScopeMap[CurrentLocationCookie] : null;
                if (locals == null) {
#if !SILVERLIGHT                    
                    Debug.Fail("DebugMarker doesn't match any scope");
#endif
                    locals = _funcInfo.VariableScopeMap[0];
                }

                return locals;
            }
        }

        #endregion

        private class ScopeData {
            public VariableInfo[] VarInfos;
            public VariableInfo[] VarInfosWithException;
            public IDictionary<object, object> Scope;
        }
    }
}
