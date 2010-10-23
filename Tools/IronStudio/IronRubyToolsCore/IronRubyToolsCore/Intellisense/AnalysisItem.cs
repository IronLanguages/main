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

using System;
using IronRuby.Compiler.Ast;
using Microsoft.VisualStudio.Text;
using System.Diagnostics;

namespace Microsoft.IronRubyTools.Intellisense {
    /// <summary>
    /// One AnalysisItem is created for each file to be analyzed. This object
    /// is moved onto the AnalysisEngine queue as needed.
    /// 
    /// _state, _textBuffer and DeferTime should only be updated from within the
    /// engine's lock
    /// </summary>
    public class AnalysisItem {
        private ProjectEntry _entry;
        private IAnalysisCookie _curCookie;
        private SourceUnitTree _node;

        internal AnalysisItem(ProjectEntry entry) {
            _entry = entry;
            //_state = ItemState.Prepare;
        }
        
        public SourceUnitTree CurrentTree {
            get {
                return _node;
            }
        }

        public void UpdateTree(SourceUnitTree newAst, IAnalysisCookie newCookie) {
            lock (this) {
                _node = newAst;
                _curCookie = newCookie;
            }
            var newParse = OnNewParseTree;
            if (newParse != null) {
                newParse(this, EventArgs.Empty);
            }
        }

        public void GetTreeAndCookie(out SourceUnitTree tree, out IAnalysisCookie cookie) {
            lock (this) {
                tree = _node;
                cookie = _curCookie;
            }
        }

        public void Analyze() {
            _entry.Cookie = _curCookie;

            _entry.Prepare(_node);
            _entry.Parse();

            var newAnalysis = OnNewAnalysis;
            if (newAnalysis != null) {
                newAnalysis(this, EventArgs.Empty);
            }
        }

        public string Path {
            get { return _entry.FilePath; }
        }

        //internal ProjectEntry Entry {
        //    get { return _entry; }
        //}

        //public bool HasAnalysis {
        //    get {
        //        return _entry != null && _entry.CurrentAnalysis != null;
        //    }
        //}

        public event EventHandler<EventArgs> OnNewParseTree;
        public event EventHandler<EventArgs> OnNewAnalysis;

        public override string ToString() {
            return String.Format("AnalysisItem({0})", Path);
        }

        internal static bool TryGetAnalysis(ITextBuffer/*!*/ buffer, out AnalysisItem analysis) {
            return buffer.Properties.TryGetProperty<AnalysisItem>(typeof(AnalysisItem), out analysis);
        }

        internal static AnalysisItem/*!*/ GetAnalysis(ITextBuffer/*!*/ buffer) {
            AnalysisItem res;
            buffer.Properties.TryGetProperty<AnalysisItem>(typeof(AnalysisItem), out res);
            Debug.Assert(res != null);
            return res;
        }
    }
}
