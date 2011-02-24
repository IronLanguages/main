/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Generic;
using IronRuby.Compiler.Ast;
using Microsoft.Scripting;
using Microsoft.Scripting.Library;

namespace Microsoft.IronRubyTools.Intellisense {
    public class ProjectEntry {
        private SourceUnit _sourceUnit;
        private readonly string _filePath;
        private IAnalysisCookie _cookie;
        private Node _tree;
        private CollectingErrorSink _errorSink;

        internal ProjectEntry(SourceUnit sourceUnit, string filePath, IAnalysisCookie cookie) {
            _sourceUnit = sourceUnit;
            _filePath = filePath;
            _cookie = cookie;
        }

        public void Parse() {
            if (_tree == null) {
                return;
            }
        }

        public void Prepare() {
            _errorSink = new CollectingErrorSink();
            // TODO:
            //using (var parser = Utils.CreateParser(_sourceUnit, _errorSink)) {
            //    Prepare(parser.ParseFile(true));
            //}
        }

        public void Prepare(Node tree) {
            // TODO:
            //_tree = tree;
            //var walker = new OverviewWalker(this, unit);
            //_tree.Walk(walker);
            //_scopeTree = walker.ScopeTree;
        }

        internal Node Tree {
            get { return _tree; }
        }

        public string FilePath {
            get { return _filePath; }
        }

        public void ReplaceSourceUnit(SourceUnit sourceUnit) {
            // TODO: thread-safety?
            _sourceUnit = sourceUnit;
            _tree = null;
        }

        // TODO: thread-safety?
        public IAnalysisCookie Cookie {
            get { return _cookie; }
            set { _cookie = value; }
        }
    }
}
