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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Core;
using Microsoft.IronStudio.Intellisense;
using Microsoft.IronStudio.Library;
using Microsoft.IronStudio.Library.Intellisense;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Library;
using Microsoft.Scripting.Runtime;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.IronRubyTools.Intellisense {
    /// <summary>
    /// Performs centralized parsing and analysis of Python source code.
    /// </summary>
    public class RubyAnalyzer : IParser, IAnalyzer<AnalysisItem> {
        private readonly ParseQueue _queue;
        private readonly ScriptEngine _engine;
        private readonly IErrorProviderFactory _squiggleProvider;
      
        public RubyAnalyzer(IComponentModel/*!*/ componentModel) {
            _engine = componentModel.GetService<IRubyRuntimeHost>().RubyScriptEngine;
            _squiggleProvider = componentModel.GetService<IErrorProviderFactory>();
            _queue = new ParseQueue(this);

            _analysisQueue = new AnalysisQueue<AnalysisItem>(this);
            _projectFiles = new Dictionary<string, AnalysisItem>(StringComparer.OrdinalIgnoreCase);
        }

        private readonly AnalysisQueue<AnalysisItem> _analysisQueue;
        private readonly Dictionary<string, AnalysisItem> _projectFiles;
        private bool _implicitProject = true;

        public bool ImplicitProject {
            get {
                return _implicitProject;
            }
            set {
                _implicitProject = value;
            }
        }

        public AnalysisItem AnalyzeTextView(ITextView textView) {
            // Get an AnalysisItem for this file, creating one if necessary
            var res = textView.TextBuffer.Properties.GetOrCreateSingletonProperty<AnalysisItem>(() => {
                string path = textView.GetFilePath();
                AnalysisItem item;
                if (path != null && _projectFiles.TryGetValue(path, out item)) {
                    return item;
                }

                var initialSnapshot = textView.TextBuffer.CurrentSnapshot;
                var entry = new ProjectEntry(
                    SnapshotTextContentProvider.Make(_engine, initialSnapshot, path),
                    textView.GetFilePath(),
                    new SnapshotCookie(initialSnapshot)
                );

                item = new AnalysisItem(entry);
                if (path != null) {
                    _projectFiles[path] = item;

                    if (ImplicitProject) {
                        AddImplicitFiles(Path.GetDirectoryName(Path.GetFullPath(path)));
                    }
                }
                
                return item;
            });

            // kick off initial processing on the ITextWindow
            _queue.EnqueueBuffer(textView);

            return res;
        }

        private void AddImplicitFiles(string dir) {
            foreach (string filename in Directory.GetFiles(dir, "*.rb")) {
                AnalyzeFile(filename);
            }
        }

        public AnalysisItem AnalyzeFile(string path) {
            AnalysisItem item;
            if (!_projectFiles.TryGetValue(path, out item)) {
                var entry = new ProjectEntry(FileTextContentProvider.Make(_engine, path), path, null);

                _projectFiles[path] = item = new AnalysisItem(entry);
            }

            _queue.EnqueueFile(path);

            return item;
        }

        public void Analyze(AnalysisItem content) {
            content.Analyze();
        }

        public bool IsAnalyzing {
            get {
                return _queue.IsParsing || _analysisQueue.IsAnalyzing;
            }
        }

        #region IParser Members

        public void Parse(TextContentProvider/*!*/ content) {
            var errorSink = new CollectingErrorSink();
            SourceUnitTree ast = MakeParseTree(content, errorSink);

            ISnapshotTextContentProvider snapshotContent = content as ISnapshotTextContentProvider;
            if (snapshotContent != null) {
                // queue analysis of the parsed tree at High Pri so the active buffer is quickly re-analyzed
                var snapshot = snapshotContent.Snapshot;

                var analysis = AnalysisItem.GetAnalysis(snapshot.TextBuffer);
                
                // only update the AST when we're error free, this way we don't remove
                // a useful analysis with an incomplete and useless analysis.
                if (errorSink.Errors.Count == 0) {
                    analysis.UpdateTree(ast, new SnapshotCookie(snapshot));
                    _analysisQueue.Enqueue(analysis, AnalysisPriority.High);
                }

                SimpleTagger<ErrorTag> squiggles = _squiggleProvider.GetErrorTagger(snapshot.TextBuffer);
                squiggles.RemoveTagSpans(x => true);

                // update squiggles for the live buffer
                foreach (ErrorResult warning in errorSink.Warnings) {
                    var span = warning.Span;
                    var tspan = CreateSpan(snapshot, span);
                    squiggles.CreateTagSpan(tspan, new ErrorTag("Warning", warning.Message));
                }

                foreach (ErrorResult error in errorSink.Errors) {
                    var span = error.Span;
                    var tspan = CreateSpan(snapshot, span);
                    squiggles.CreateTagSpan(tspan, new ErrorTag("Error", error.Message));
                }
            } else {
                FileTextContentProvider fileContent = content as FileTextContentProvider;
                AnalysisItem analysis;
                if (fileContent != null && _projectFiles.TryGetValue(fileContent.Path, out analysis)) {
                    analysis.UpdateTree(ast, new FileCookie(fileContent.Path));
                    _analysisQueue.Enqueue(analysis, AnalysisPriority.Normal);
                }
            }
        }

        private SourceUnitTree MakeParseTree(TextContentProvider/*!*/ content, ErrorSink/*!*/ errorSink) {
            var source = new SourceUnit(HostingHelpers.GetLanguageContext(_engine), content, null, SourceCodeKind.File);
            var options = new RubyCompilerOptions();
            var parser = new Parser();
            try {
                int attempts = 10;
                while (true) {
                    try {
                        return parser.Parse(source, options, errorSink);
                    } catch (IOException) {
                        // file being copied, try again...
                        if (attempts > 0) {
                            Thread.Sleep(100);
                            attempts--;
                        } else {
                            throw;
                        }
                    }
                }
            } catch (Exception e) {
                Debug.Assert(false, String.Format("Failure in IronRuby parser: {0}", e.ToString()));
                return null;
            }
        }

        private static ITrackingSpan CreateSpan(ITextSnapshot snapshot, SourceSpan span) {
            var tspan = snapshot.CreateTrackingSpan(
                new Span(
                    span.Start.Index,
                    Math.Min(span.End.Index - span.Start.Index, Math.Max(snapshot.Length - span.Start.Index, 0))
                ), 
                SpanTrackingMode.EdgeInclusive
            );
            return tspan;
        }

        #endregion
    }
}
