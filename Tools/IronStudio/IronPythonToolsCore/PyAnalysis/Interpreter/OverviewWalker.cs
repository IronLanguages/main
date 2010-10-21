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
using Microsoft.PyAnalysis.Values;

namespace Microsoft.PyAnalysis.Interpreter {
    /// <summary>
    /// Performs the 1st pass over the AST to gather all of the classes and
    /// function definitions.
    /// </summary>
    internal class OverviewWalker : PythonWalker {
        private readonly List<InterpreterScope> _scopes;
        private readonly ProjectEntry _entry;
        private readonly Stack<ScopePositionInfo> _scopeTree;
        private readonly Stack<AnalysisUnit> _analysisStack = new Stack<AnalysisUnit>();
        private AnalysisUnit _curUnit;

        public OverviewWalker(ProjectEntry entry, AnalysisUnit topAnalysis) {
            _entry = entry;
            _curUnit = topAnalysis;

            _scopes = new List<InterpreterScope>();
            _scopes.Push(entry.MyScope.Scope);

            _scopeTree = new Stack<ScopePositionInfo>();
            _scopeTree.Push(new ScopePositionInfo(1, Int32.MaxValue, null));
        }       

        // TODO: What about names being redefined?
        // remember classes/functions as they start new scopes
        public override bool Walk(ClassDefinition node) {
            if (node.Body == null || node.Name == null) {
                return false;
            }

            var queue = _entry.ProjectState.Queue;

            var scopes = new InterpreterScope[_scopes.Count + 1];
            _scopes.CopyTo(scopes);
            
            _analysisStack.Push(_curUnit);
            var unit = _curUnit = new AnalysisUnit(node, scopes, _curUnit);
            var klass = new ClassInfo(unit, _entry);
            var classScope = klass.Scope;

            var scope = _scopes.Peek();
            scope.SetVariable(node, unit, node.Name, klass.SelfSet);

            _scopes.Push(classScope);
            scopes[scopes.Length - 1] = classScope;            

            // TODO: Add parameters for __new__/__init__
            PushPositionScope(node, classScope);

            return true;
        }

        public override void PostWalk(ClassDefinition node) {
            if (node.Body != null && node.Name != null) {
                _scopes.Pop();
                _scopeTree.Pop();
                _curUnit = _analysisStack.Pop();
            }
        }

        public override bool Walk(FunctionDefinition node) {
            if (node.Body == null || node.Name == null) {
                return false;
            }

            var queue = _entry.ProjectState.Queue;
            var scopes = new InterpreterScope[_scopes.Count + 1];
            _scopes.CopyTo(scopes);

            _analysisStack.Push(_curUnit);
            var unit = _curUnit = new AnalysisUnit(node, scopes, _curUnit);
            var function = new FunctionInfo(unit, _entry);
            var funcScope = new FunctionScope(function);

            _entry.MyScope.GetOrMakeNodeVariable(node, x => function.SelfSet);
            _scopes.Push(funcScope);
            scopes[scopes.Length - 1] = funcScope;

            if (!node.IsLambda) {
                // lambdas don't have their names published
                var scope = _scopes[_scopes.Count - 2];
                scope.SetVariable(node, unit, node.Name, function.SelfSet);
            }

            var newParams = new VariableDef[node.Parameters.Count];
            int index = 0;
            foreach (var param in node.Parameters) {
                newParams[index++] = funcScope.DefineVariable(param, _curUnit);
            }
            function.SetParameters(newParams);

            PushPositionScope(node, funcScope);
            
            return true;
        }

        public override void PostWalk(FunctionDefinition node) {
            if (node.Body != null && node.Name != null) {
                _scopes.Pop();
                _scopeTree.Pop();
                _curUnit = _analysisStack.Pop();
            }
        }

        private void PushPositionScope(Node node, InterpreterScope newScope) {
            var newPositionInfo = new ScopePositionInfo(node.Start.Line, node.End.Line, newScope);
            _scopeTree.Peek().Children.Add(newPositionInfo);
            _scopeTree.Push(newPositionInfo);
        }

        public Stack<ScopePositionInfo> ScopeTree {
            get { return _scopeTree; }
        }
    }
}
