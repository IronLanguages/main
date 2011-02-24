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
using System.IO;
using IronPython.Compiler.Ast;
using Microsoft.IronPythonTools.Intellisense;
using Microsoft.IronStudio.Intellisense;
using Microsoft.PyAnalysis.Interpreter;
using Microsoft.PyAnalysis.Values;

namespace Microsoft.PyAnalysis {
    /// <summary>
    /// Provides interactions to analysis a single file in a project and get the results back.
    /// 
    /// To analyze a file the tree should be updated with a call to UpdateTree and then PreParse
    /// should be called on all files.  Finally Parse should then be called on all files.
    /// </summary>
    internal sealed class ProjectEntry : IPythonProjectEntry {
        private readonly ProjectState _projectState;
        private readonly string _moduleName;
        private readonly string _filePath;
        private IAnalysisCookie _cookie;
        private ModuleInfo _myScope;
        private PythonAst _tree;
        private Stack<ScopePositionInfo> _scopeTree;
        private ModuleAnalysis _currentAnalysis;
        private AnalysisUnit _unit;
        private int _version;

        internal ProjectEntry(ProjectState state, string moduleName, string filePath, IAnalysisCookie cookie) {
            Debug.Assert(moduleName != null);
            Debug.Assert(filePath != null);

            _projectState = state;
            _moduleName = moduleName ?? "";
            _filePath = filePath;
            _cookie = cookie;
            _myScope = new ModuleInfo(_moduleName, this);
            _unit = new AnalysisUnit(_tree, new InterpreterScope[] { _myScope.Scope }, null);
        }

        public event EventHandler<EventArgs> OnNewParseTree;
        public event EventHandler<EventArgs> OnNewAnalysis;

        public void UpdateTree(PythonAst newAst, IAnalysisCookie newCookie) {
            lock (this) {
                _tree = newAst;
                _cookie = newCookie;
            }

            var newParse = OnNewParseTree;
            if (newParse != null) {
                newParse(this, EventArgs.Empty);
            }
        }

        public void GetTreeAndCookie(out PythonAst tree, out IAnalysisCookie cookie) {
            lock (this) {
                tree = _tree;
                cookie = _cookie;
            }
        }

        public void Analyze() {
            lock (this) {
                _version++;
                
                Parse();

                var newAnalysis = OnNewAnalysis;
                if (newAnalysis != null) {
                    newAnalysis(this, EventArgs.Empty);
                }
            }
        }

        public int Version {
            get {
                return _version;
            }
        }

        public bool IsAnalyzed {
            get {
                return Analysis != null;
            }
        }

        private void Parse() {
            if (_tree == null) {
                return;
            }

            var oldParent = _myScope.ParentPackage;
            ProjectState.ModulesByFilename[_filePath] = _myScope;

            if (oldParent != null) {
                // update us in our parent package
                _myScope.ParentPackage = oldParent;
                oldParent.Scope.SetVariable(_tree, _unit, _moduleName.Substring(_moduleName.IndexOf('.') + 1), _myScope.SelfSet, false);
            }

            var unit = _unit = new AnalysisUnit(_tree, new InterpreterScope[] { _myScope.Scope }, null);

            // collect top-level definitions first
            var walker = new OverviewWalker(this, unit);
            _tree.Walk(walker);
            _scopeTree = walker.ScopeTree;

            PublishPackageChildrenInPackage();

            // create new analysis object and analyze the code.
            var newAnalysis = new ModuleAnalysis(_unit, _scopeTree);
            _unit.Enqueue();

            new DDG().Analyze(_projectState.Queue);

            // publish the analysis now that it's complete
            _currentAnalysis = newAnalysis;

            foreach (var variableInfo in _myScope.Scope.Variables) {
                variableInfo.Value.ClearOldValues(this);
            }
        }

        private void PublishPackageChildrenInPackage() {
            if (_filePath.EndsWith("__init__.py")) {
                string dir = Path.GetDirectoryName(_filePath);
                if (Directory.Exists(dir)) {
                    foreach (var file in Directory.GetFiles(dir)) {
                        if (file.EndsWith("__init__.py")) {
                            continue;
                        }

                        ModuleInfo childModule;
                        if (_projectState.ModulesByFilename.TryGetValue(file, out childModule)) {
                            _myScope.Scope.SetVariable(childModule.ProjectEntry.Tree, _unit, Path.GetFileNameWithoutExtension(file), childModule, false);
                            childModule.ParentPackage = _myScope;
                        }
                    }

                    foreach (var packageDir in Directory.GetDirectories(dir)) {
                        string package = Path.Combine(packageDir, "__init__.py");
                        ModuleInfo childPackage;
                        if (File.Exists(package) && _projectState.ModulesByFilename.TryGetValue(package, out childPackage)) {
                            _myScope.Scope.SetVariable(childPackage.ProjectEntry.Tree, _unit, Path.GetFileName(packageDir), childPackage, false);
                            childPackage.ParentPackage = _myScope;
                        }
                    }
                }
            }
        }

        public string GetLine(int lineNo) {
            return _cookie.GetLine(lineNo);
        }

        public ModuleAnalysis Analysis {
            get { return _currentAnalysis; }
        }

        public string FilePath {
            get { return _filePath; }
        }

        public IAnalysisCookie Cookie {
            get { return _cookie; }
        }

        internal ProjectState ProjectState {
            get { return _projectState; }
        }

        public PythonAst Tree {
            get { return _tree; }
        }

        internal ModuleInfo MyScope {
            get { return _myScope; }
        }
    }
    
    /// <summary>
    /// Represents a file which is capable of being analyzed.  Can be cast to other project entry types
    /// for more functionality.  See also IPythonProjectEntry and IXamlProjectEntry.
    /// </summary>
    public interface IProjectEntry {
        bool IsAnalyzed { get; }
        void Analyze();
        int Version {
            get;
        }

        string FilePath { get; }
        string GetLine(int lineNo);
    }

    public interface IPythonProjectEntry : IProjectEntry {
        PythonAst Tree {
            get;
        }

        ModuleAnalysis Analysis {
            get;
        }

        event EventHandler<EventArgs> OnNewParseTree;
        event EventHandler<EventArgs> OnNewAnalysis;

        void UpdateTree(PythonAst ast, IAnalysisCookie fileCookie);
        void GetTreeAndCookie(out PythonAst ast, out IAnalysisCookie cookie);
    }

    sealed class XamlProjectEntry : IXamlProjectEntry {
        private XamlAnalysis _analysis;
        private readonly string _filename;
        private int _version;
        private TextReader _content;
        private IAnalysisCookie _cookie;
        private readonly ProjectState _projectState;
        private HashSet<IProjectEntry> _dependencies = new HashSet<IProjectEntry>();

        public XamlProjectEntry(ProjectState projectState, string filename) {
            _projectState = projectState;
            _filename = filename;
        }

        public void UpdateContent(TextReader content, IAnalysisCookie fileCookie) {
            _content = content;
            _cookie = fileCookie;
        }

        public void AddDependency(AnalysisUnit unit) {
            _dependencies.Add(unit.ProjectEntry);
        }

        #region IProjectEntry Members

        public bool IsAnalyzed {
            get { return _analysis != null; }
        }

        public void Analyze() {
            lock (this) {
                if (_analysis == null) {
                    _analysis = new XamlAnalysis(_filename);
                    _cookie = new FileCookie(_filename);
                }
                _analysis = new XamlAnalysis(_content);

                _version++;

                // update any .py files which depend upon us.
                foreach (var dep in _dependencies) {
                    dep.Analyze();
                }
            }
        }

        public string FilePath { get { return _filename; } }

        public int Version {
            get {
                return _version;
            }
        }

        public string GetLine(int lineNo) {
            return _cookie.GetLine(lineNo);
        }

        #endregion

        #region IXamlProjectEntry Members

        public XamlAnalysis Analysis {
            get { return _analysis; }
        }

        #endregion
    }

    public interface IXamlProjectEntry : IProjectEntry {
        XamlAnalysis Analysis {
            get;
        }
    }
}
