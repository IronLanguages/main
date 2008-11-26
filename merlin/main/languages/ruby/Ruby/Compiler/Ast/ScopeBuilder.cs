/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Scripting.Utils;
using System.Threading;
using MSA = System.Linq.Expressions;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    using Microsoft.Scripting.Ast;

    internal sealed class ScopeBuilder {
        private static readonly List<MSA.ParameterExpression> _FinishedVariables = new List<MSA.ParameterExpression>(0);

        // local variables and parameters exposed at runtime via dictionary:
        private List<MSA.ParameterExpression> _visibleLocals;

        // all local variables except for parameters that are used in the scope:
        private List<MSA.ParameterExpression> _allLocals;
        
#if DEBUG
        private static int _Id;
        private int _id;

        public ScopeBuilder() {
            _id = Interlocked.Increment(ref _Id);
        }
#endif

        private bool VisibleFinished {
            get { return _visibleLocals == _FinishedVariables; }
        }

        private bool AllFinished {
            get { return _allLocals == _FinishedVariables; }
        }

        public MSA.ParameterExpression/*!*/ DefineHiddenVariable(string/*!*/ name, Type/*!*/ type) {
#if DEBUG
            int hiddenCount = ((_allLocals != null) ? _allLocals.Count : 0) - ((_visibleLocals != null) ? _visibleLocals.Count : 0);
            name += "_" + _id + "_" + hiddenCount;
#endif
            return AddHidden(Ast.Variable(type, name));
        }

        // Defines visible user variable (of object type).
        public MSA.ParameterExpression/*!*/ DefineVariable(string/*!*/ name) {
            return AddVisible(Ast.Variable(typeof(object), name));
        }
        
        public MSA.ParameterExpression/*!*/ AddVisible(MSA.ParameterExpression/*!*/ variable) {
            ContractUtils.Requires(!VisibleFinished);
            Debug.Assert(!AllFinished);

            if (_visibleLocals == null) {
                _visibleLocals = new List<MSA.ParameterExpression>();                
            }

            if (_allLocals == null) {
                _allLocals = new List<MSA.ParameterExpression>();
            }

            _visibleLocals.Add(variable);
            _allLocals.Add(variable);
            return variable;
        }

        public void AddVisibleParameters(MSA.ParameterExpression[]/*!*/ parameters, int hiddenCount) {
            ContractUtils.Requires(_visibleLocals == null, "Parameters should be added prior to other visible variables");

            _visibleLocals = new List<MSA.ParameterExpression>(parameters.Length - hiddenCount);
            for (int i = hiddenCount; i < parameters.Length; i++) {
                _visibleLocals.Add(parameters[i]);
            }
        }

        public MSA.ParameterExpression/*!*/ AddHidden(MSA.ParameterExpression/*!*/ variable) {
            ContractUtils.Requires(!AllFinished);
            if (_allLocals == null) {
                _allLocals = new List<MSA.ParameterExpression>();
            }
            _allLocals.Add(variable);
            return variable;
        }

        public MSA.Expression/*!*/ VisibleVariables() {
            MSA.Expression result;
            if (_visibleLocals != null) {
                result = Utils.VariableDictionary(new ReadOnlyCollection<MSA.ParameterExpression>(_visibleLocals.ToArray()));
            } else {
                result = Utils.VariableDictionary();
            }
            _visibleLocals = _FinishedVariables;
            return result;
        }

        public MSA.Expression/*!*/ CreateScope(MSA.Expression/*!*/ body) {
            MSA.Expression result;
            if (_allLocals != null) {
                result = Ast.Block(new ReadOnlyCollection<MSA.ParameterExpression>(_allLocals.ToArray()), body);
            } else {
                result = body;
            }
            _allLocals = _FinishedVariables;
            return result;
        }
    }
}
