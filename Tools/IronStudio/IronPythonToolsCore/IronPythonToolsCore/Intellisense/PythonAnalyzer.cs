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
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using IronPython;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using Microsoft.IronPythonTools.Internal;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Intellisense;
using Microsoft.IronStudio.Library.Intellisense;
using Microsoft.IronStudio.Repl;
using Microsoft.PyAnalysis;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Library;
using Microsoft.Scripting.Runtime;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.IronPythonTools.Intellisense {
    /// <summary>
    /// Performs centralized parsing and analysis of Python source code.
    /// </summary>
    [Export(typeof(IPythonAnalyzer))]
    internal class PythonAnalyzer : IParser, IAnalyzer<IProjectEntry>, IPythonAnalyzer {
        private readonly ParseQueue _queue;
        private readonly AnalysisQueue<IProjectEntry> _analysisQueue;
        private readonly ScriptEngine _engine;
        private readonly IErrorProviderFactory _squiggleProvider;
        private readonly Dictionary<string, IProjectEntry> _projectFiles;
        private readonly ProjectState _analysisState;
        private bool _implicitProject = true;

        private static PythonOptions EmptyOptions = new PythonOptions();

        [ImportingConstructor]
        public PythonAnalyzer(IPythonRuntimeHost runtimeHost, IErrorProviderFactory errorProvider) {
            _engine = runtimeHost.ScriptEngine;
            _squiggleProvider = errorProvider;

            _queue = new ParseQueue(this);
            _analysisQueue = new AnalysisQueue<IProjectEntry>(this);
            _analysisState = new ProjectState(_engine);
            _projectFiles = new Dictionary<string, IProjectEntry>(StringComparer.OrdinalIgnoreCase);
        }

        #region IPythonAnalyzer

        public IProjectEntry AnalyzeTextView(ITextView textView) {
            // Get an AnalysisItem for this file, creating one if necessary
            var res = textView.TextBuffer.Properties.GetOrCreateSingletonProperty<IProjectEntry>(() => {
                string path = textView.GetFilePath();
                if (path == null) {
                    return null;
                }

                IProjectEntry entry;
                if (!_projectFiles.TryGetValue(path, out entry)) {
                    var modName = PathToModuleName(path);

                    var initialSnapshot = textView.TextBuffer.CurrentSnapshot;

                    if (textView.TextBuffer.ContentType.IsOfType(PythonCoreConstants.ContentType)) {
                        entry = _analysisState.AddModule(
                            modName,
                            textView.GetFilePath(),
                            new SnapshotCookie(initialSnapshot)
                        );
                    } else if (textView.TextBuffer.ContentType.IsOfType("xaml")) {
                        entry = _analysisState.AddXamlFile(path);
                    } else {
                        return null;
                    }

                    _projectFiles[path] = entry;

                    if (ImplicitProject) {
                        AddImplicitFiles(Path.GetDirectoryName(Path.GetFullPath(path)));
                    }
                }
                
                return entry;
            });

            // kick off initial processing on the ITextWindow
            _queue.EnqueueBuffer(textView);

            return res;
        }

        public IProjectEntry AnalyzeFile(string path) {
            IProjectEntry item;
            if (!_projectFiles.TryGetValue(path, out item)) {
                if (path.EndsWith(".py", StringComparison.OrdinalIgnoreCase)) {
                    var modName = PathToModuleName(path);

                    item = _analysisState.AddModule(
                        modName,
                        path,
                        null
                    );
                } else if (path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase)) {
                    item = _analysisState.AddXamlFile(path);
                }

                if (item != null) {
                    _projectFiles[path] = item;
                }
            }

            _queue.EnqueueFile(path);

            return item;
        }
        
        public IProjectEntry GetAnalysisFromFile(string path) {
            IProjectEntry res;
            if (_projectFiles.TryGetValue(path, out res)) {
                return res;
            }
            return null;
        }

        /// <summary>
        /// Gets a ExpressionAnalysis for the expression at the provided span.  If the span is in
        /// part of an identifier then the expression is extended to complete the identifier.
        /// </summary>
        public ExpressionAnalysis AnalyzeExpression(ITextSnapshot snapshot, ITextBuffer buffer, ITrackingSpan span) {
            ReverseExpressionParser parser = new ReverseExpressionParser(snapshot, buffer, span);

            var loc = parser.Span.GetSpan(parser.Snapshot.Version);
            var exprRange = parser.GetExpressionRange();
            if (exprRange == null) {
                return ExpressionAnalysis.Empty;
            }

            // extend right for any partial expression the user is hovering on, for example:
            // "x.Baz" where the user is hovering over the B in baz we want the complete
            // expression.
            var text = exprRange.Value.GetText();
            var endingLine = exprRange.Value.End.GetContainingLine();
            if (endingLine.End.Position - exprRange.Value.End.Position < 0) {
                return ExpressionAnalysis.Empty;
            }
            var endText = snapshot.GetText(exprRange.Value.End.Position, endingLine.End.Position - exprRange.Value.End.Position);
            bool allChars = true;
            for (int i = 0; i < endText.Length; i++) {
                if (!Char.IsLetterOrDigit(endText[i]) && endText[i] != '_') {
                    text += endText.Substring(0, i);
                    allChars = false;
                    break;
                }
            }
            if (allChars) {
                text += endText;
            }

            var applicableSpan = parser.Snapshot.CreateTrackingSpan(
                exprRange.Value.Span,
                SpanTrackingMode.EdgeExclusive
            );

            IProjectEntry analysisItem;
            if (buffer.TryGetAnalysis(out analysisItem)) {
                var analysis = ((IPythonProjectEntry)analysisItem).Analysis;
                if (analysis != null && text.Length > 0) {

                    var lineNo = parser.Snapshot.GetLineNumberFromPosition(loc.Start);
                    return new ExpressionAnalysis(
                        text,
                        analysis,
                        lineNo + 1,
                        applicableSpan);
                }
            }

            return ExpressionAnalysis.Empty;
        }

        /// <summary>
        /// Gets a CompletionList providing a list of possible members the user can dot through.
        /// </summary>
        public CompletionAnalysis GetCompletions(ITextSnapshot snapshot, ITextBuffer buffer, ITrackingSpan span, bool intersectMembers = true, bool hideAdvancedMembers = false) {
            ReverseExpressionParser parser = new ReverseExpressionParser(snapshot, buffer, span);

            var loc = parser.Span.GetSpan(parser.Snapshot.Version);
            var line = parser.Snapshot.GetLineFromPosition(loc.Start);
            var lineStart = line.Start;

            var textLen = loc.End - lineStart.Position;
            if (textLen <= 0) {
                // Ctrl-Space on an empty line, we just want to get global vars
                return new NormalCompletionAnalysis(String.Empty, loc.Start, parser.Snapshot, parser.Span, parser.Buffer, 0);
            }

            return TrySpecialCompletions(parser, loc) ??
                   GetNormalCompletionContext(parser, loc, intersectMembers, hideAdvancedMembers);
        }

        /// <summary>
        /// Gets a list of signatuers available for the expression at the provided location in the snapshot.
        /// </summary>
        public SignatureAnalysis GetSignatures(ITextSnapshot snapshot, ITextBuffer buffer, ITrackingSpan span) {
            ReverseExpressionParser parser = new ReverseExpressionParser(snapshot, buffer, span);

            var loc = parser.Span.GetSpan(parser.Snapshot.Version);

            int paramIndex;
            SnapshotPoint? sigStart;
            var exprRange = parser.GetExpressionRange(1, out paramIndex, out sigStart);
            if (exprRange == null || sigStart == null) {
                return new SignatureAnalysis("", 0, new ISignature[0]);
            }

            Debug.Assert(sigStart != null);
            var text = new SnapshotSpan(exprRange.Value.Snapshot, new Span(exprRange.Value.Start, sigStart.Value.Position - exprRange.Value.Start)).GetText();
            //var text = exprRange.Value.GetText();
            var applicableSpan = parser.Snapshot.CreateTrackingSpan(exprRange.Value.Span, SpanTrackingMode.EdgeInclusive);

            var liveSigs = TryGetLiveSignatures(snapshot, paramIndex, text, applicableSpan);
            if (liveSigs != null) {
                return liveSigs;
            }

            var start = Stopwatch.ElapsedMilliseconds;

            var analysisItem = buffer.GetAnalysis();
            if (analysisItem != null) {
                var analysis = ((IPythonProjectEntry)analysisItem).Analysis;
                if (analysis != null) {

                    var lineNo = parser.Snapshot.GetLineNumberFromPosition(loc.Start);

                    var sigs = analysis.GetSignatures(text, lineNo + 1);
                    var end = Stopwatch.ElapsedMilliseconds;

                    if (/*Logging &&*/ (end - start) > CompletionAnalysis.TooMuchTime) {
                        Trace.WriteLine(String.Format("{0} lookup time {1} for signatures", text, end - start));
                    }

                    var result = new List<ISignature>();
                    foreach (var sig in sigs) {
                        result.Add(new PythonSignature(applicableSpan, sig, paramIndex));
                    }

                    return new SignatureAnalysis(
                        text,
                        paramIndex,
                        result
                    );
                }
            }
            return new SignatureAnalysis(text, paramIndex, new ISignature[0]);
        }

        public bool IsAnalyzing {
            get {
                return _queue.IsParsing || _analysisQueue.IsAnalyzing;
            }
        }

        public bool ImplicitProject {
            get {
                return _implicitProject;
            }
            set {
                _implicitProject = value;
            }
        }

        #endregion

        #region IParser Members

        public void Parse(TextContentProvider content) {

            ISnapshotTextContentProvider snapshotContent = content as ISnapshotTextContentProvider;
            if (snapshotContent != null) {
                ParseSnapshot(snapshotContent);
            } else {
                FileTextContentProvider fileContent = content as FileTextContentProvider;
                if (fileContent != null) {
                    ParseFile(fileContent);
                }
            }

        }

        private void ParseFile(FileTextContentProvider fileContent) {
            if (fileContent.Path.EndsWith(".py", StringComparison.OrdinalIgnoreCase)) {
                PythonAst ast;
                CollectingErrorSink errorSink;
                ParsePythonCode(fileContent, out ast, out errorSink);

                if (ast != null) {
                    IProjectEntry analysis;
                    IPythonProjectEntry pyAnalysis;
                    if (fileContent != null &&
                        _projectFiles.TryGetValue(fileContent.Path, out analysis) &&
                        (pyAnalysis = analysis as IPythonProjectEntry) != null) {

                        pyAnalysis.UpdateTree(ast, new FileCookie(fileContent.Path));
                        _analysisQueue.Enqueue(analysis, AnalysisPriority.Normal);
                    }
                }
            } else if (fileContent.Path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase)) {
                IProjectEntry analysis;
                XamlProjectEntry xamlProject;
                if (_projectFiles.TryGetValue(fileContent.Path, out analysis) && 
                    (xamlProject = analysis as XamlProjectEntry) != null) {
                    xamlProject.UpdateContent(fileContent.GetReader(), new FileCookie(fileContent.Path));
                    _analysisQueue.Enqueue(analysis, AnalysisPriority.Normal);
                }
            }
        }

        private void ParseSnapshot(ISnapshotTextContentProvider snapshotContent) {
            
            // queue analysis of the parsed tree at High Pri so the active buffer is quickly re-analyzed
            var snapshot = snapshotContent.Snapshot;

            if (snapshot.TextBuffer.ContentType.IsOfType(PythonCoreConstants.ContentType)) {
                PythonAst ast;
                CollectingErrorSink errorSink;
                ParsePythonCode((TextContentProvider)snapshotContent, out ast, out errorSink);
                if (ast != null) {
                    IPythonProjectEntry analysis;
                    if (snapshot.TextBuffer.TryGetPythonAnalysis(out analysis)) {
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
                    }
                }
            } else if (snapshot.TextBuffer.ContentType.IsOfType("xaml")) {
                string path = snapshot.TextBuffer.GetFilePath();
                if (path != null) {
                    IProjectEntry analysis;
                    XamlProjectEntry xamlProject;
                    if (_projectFiles.TryGetValue(path, out analysis) &&
                        (xamlProject = analysis as XamlProjectEntry) != null) {
                        xamlProject.UpdateContent(((TextContentProvider)snapshotContent).GetReader(), new SnapshotCookie(snapshotContent.Snapshot));
                        _analysisQueue.Enqueue(analysis, AnalysisPriority.High);
                    }
                }

            }
        }

        private void ParsePythonCode(TextContentProvider content, out PythonAst ast, out CollectingErrorSink errorSink) {
            ast = null;
            errorSink = new CollectingErrorSink();

            // parse the tree
            var source = _engine.CreateScriptSource(content, "", SourceCodeKind.File);
            var compOptions = (PythonCompilerOptions)HostingHelpers.GetLanguageContext(_engine).GetCompilerOptions();
            var context = new CompilerContext(HostingHelpers.GetSourceUnit(source), compOptions, errorSink);
            //compOptions.Verbatim = true;
            using (var parser = MakeParser(context)) {
                if (parser != null) {
                    try {
                        ast = parser.ParseFile(false);
                    } catch (Exception e) {
                        Debug.Assert(false, String.Format("Failure in IronPython parser: {0}", e.ToString()));
                    }

                }
            }
        }

        private static Parser MakeParser(CompilerContext context) {
            for (int i = 0; i < 10; i++) {
                try {
                    return Parser.CreateParser(context, EmptyOptions);
                } catch (IOException) {
                    // file being copied, try again...
                    Thread.Sleep(100);
                }
            }
            return null;
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

        #region IAnalyzer<AnalysisItem> Members

        public void Analyze(IProjectEntry content) {
            content.Analyze();
        }

        #endregion

        #region Implementation Details

        private static Stopwatch _stopwatch = MakeStopWatch();

        internal static Stopwatch Stopwatch {
            get {
                return _stopwatch;
            }
        }

        private static SignatureAnalysis TryGetLiveSignatures(ITextSnapshot snapshot, int paramIndex, string text, ITrackingSpan applicableSpan) {
            IReplEvaluator eval;
            IDlrEvaluator dlrEval;
            if (snapshot.TextBuffer.Properties.TryGetProperty<IReplEvaluator>(typeof(IReplEvaluator), out eval) &&
                (dlrEval = eval as IDlrEvaluator) != null) {
                if (text.EndsWith("(")) {
                    text = text.Substring(0, text.Length - 1);
                }
                var liveSigs = dlrEval.GetSignatureDocumentation(text);

                if (liveSigs != null && liveSigs.Count > 0) {
                    return new SignatureAnalysis(text, paramIndex, GetLiveSignatures(text, liveSigs, paramIndex, applicableSpan));
                }
            }
            return null;
        }

        private static ISignature[] GetLiveSignatures(string text, ICollection<OverloadDoc> liveSigs, int paramIndex, ITrackingSpan span) {
            ISignature[] res = new ISignature[liveSigs.Count];
            int i = 0;
            foreach (var sig in liveSigs) {
                var parameters = new ParameterResult[sig.Parameters.Count];
                int j = 0;
                foreach (var param in sig.Parameters) {
                    parameters[j++] = new ParameterResult(param.Name);
                }

                res[i++] = new PythonSignature(
                    span,
                    new LiveOverloadResult(text, sig.Documentation, parameters),
                    paramIndex
                );
            }
            return res;
        }

        class LiveOverloadResult : IOverloadResult {
            private readonly string _name, _doc;
            private readonly ParameterResult[] _parameters;

            public LiveOverloadResult(string name, string documentation, ParameterResult[] parameters) {
                _name = name;
                _doc = documentation;
                _parameters = parameters;
            }

            #region IOverloadResult Members

            public string Name {
                get { return _name; }
            }

            public string Documentation {
                get { return _doc; }
            }

            public ParameterResult[] Parameters {
                get { return _parameters; }
            }

            #endregion
        }

        private static CompletionAnalysis TrySpecialCompletions(ReverseExpressionParser parser, Span loc) {
            if (parser.Tokens.Count > 0) {
                // Check for context-sensitive intellisense
                var lastClass = parser.Tokens[parser.Tokens.Count - 1];

                if (lastClass.ClassificationType == parser.Classifier.Provider.Comment) {
                    // No completions in comments
                    return CompletionAnalysis.EmptyCompletionContext;
                } else if (lastClass.ClassificationType == parser.Classifier.Provider.StringLiteral) {
                    // String completion
                    return new StringLiteralCompletionList(lastClass.Span.GetText(), loc.Start, parser.Span, parser.Buffer);
                }

                // Import completions
                var first = parser.Tokens[0];
                if (CompletionAnalysis.IsKeyword(first, "import")) {
                    return ImportCompletionAnalysis.Make(first, lastClass, loc, parser.Snapshot, parser.Span, parser.Buffer, IsSpaceCompletion(parser, loc));
                } else if (CompletionAnalysis.IsKeyword(first, "from")) {
                    return FromImportCompletionAnalysis.Make(parser.Tokens, first, loc, parser.Snapshot, parser.Span, parser.Buffer, IsSpaceCompletion(parser, loc));
                }
                return null;
            }

            return CompletionAnalysis.EmptyCompletionContext;
        }

        private static CompletionAnalysis GetNormalCompletionContext(ReverseExpressionParser parser, Span loc, bool intersectMembers = true, bool hideAdvancedMembers = false) {
            var exprRange = parser.GetExpressionRange();
            if (exprRange == null) {
                return CompletionAnalysis.EmptyCompletionContext;
            }
            if (IsSpaceCompletion(parser, loc)) {
                return CompletionAnalysis.EmptyCompletionContext;
            }

            var text = exprRange.Value.GetText();

            var applicableSpan = parser.Snapshot.CreateTrackingSpan(
                exprRange.Value.Span,
                SpanTrackingMode.EdgeExclusive
            );

            return new NormalCompletionAnalysis(
                text,
                loc.Start,
                parser.Snapshot,
                applicableSpan,
                parser.Buffer,
                -1,
                intersectMembers,
                hideAdvancedMembers
            );
        }

        private static bool IsSpaceCompletion(ReverseExpressionParser parser, Span loc) {
            var keySpan = new SnapshotSpan(parser.Snapshot, loc.Start - 1, 1);
            return (keySpan.GetText() == " ");
        }

        private static Stopwatch MakeStopWatch() {
            var res = new Stopwatch();
            res.Start();
            return res;
        }

        private void AddImplicitFiles(string dir) {
            foreach (string filename in Directory.GetFiles(dir, "*.py")) {
                AnalyzeFile(filename);
            }

            foreach (string innerDir in Directory.GetDirectories(dir)) {
                if (File.Exists(Path.Combine(innerDir, "__init__.py"))) {
                    AddImplicitFiles(innerDir);
                }
            }
        }

        internal static string PathToModuleName(string path) {
            string moduleName;
            string dirName;

            if (path == null) {
                return String.Empty;
            } else if (path.EndsWith("__init__.py")) {
                moduleName = Path.GetFileName(Path.GetDirectoryName(path));
                dirName = Path.GetDirectoryName(path);
            } else {
                moduleName = Path.GetFileNameWithoutExtension(path);
                dirName = path;
            }

            while (dirName.Length != 0 && (dirName = Path.GetDirectoryName(dirName)).Length != 0 &&
                File.Exists(Path.Combine(dirName, "__init__.py"))) {
                moduleName = Path.GetFileName(dirName) + "." + moduleName;
            }

            return moduleName;
        }

        #endregion

    }
}
