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
using System.Linq;
using System.Collections.Generic;
using Microsoft.IronStudio.Library;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace Microsoft.IronStudio.Core {
    /// <summary>
    /// Provides classification based upon the DLR TokenCategory enum.
    /// </summary>
    internal class DlrClassifier : IDlrClassifier {
        private readonly TokenCache _tokenCache;
        private readonly DlrClassifierProvider _provider;
        private readonly TokenCategorizer _categorizer;
        private readonly ScriptEngine _engine;
        private readonly ITextBuffer _buffer;
        
        internal DlrClassifier(DlrClassifierProvider provider, ScriptEngine engine, ITextBuffer buffer) {
            buffer.Changed += BufferChanged;
            buffer.ContentTypeChanged += BufferContentTypeChanged;
            
            _tokenCache = new TokenCache();
            _categorizer = engine.GetService<TokenCategorizer>();
            _engine = engine;
            _provider = provider;
            _buffer = buffer;
        }

        #region IDlrClassifier

        // This event gets raised if the classification of existing test changes.
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        /// <summary>
        /// This method classifies the given snapshot span.
        /// </summary>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span) {
            var classifications = new List<ClassificationSpan>();
            var snapshot = span.Snapshot;

            IMixedBuffer mixedBuffer;
            if (snapshot.TextBuffer.Properties.TryGetProperty<IMixedBuffer>(typeof(IMixedBuffer), out mixedBuffer)) {
                foreach (SnapshotSpan codeSpan in mixedBuffer.GetLanguageSpans(snapshot)) {
                    SnapshotSpan? intersection = codeSpan.Overlap(span);
                    if (intersection != null) {
                        var firstCodeLine = codeSpan.Start.GetContainingLine();
                        AddClassifications(classifications, intersection.Value, firstCodeLine.LineNumber, codeSpan.Start - firstCodeLine.Start);
                    }
                }
            } else if(span.Length > 0) {
                AddClassifications(classifications, span, 0, 0);
            }

            return classifications;
        }

        public IDlrClassifierProvider Provider {
            get {
                return _provider;
            }
        }

        #endregion

        #region Private Members

        private Dictionary<TokenCategory, IClassificationType> CategoryMap {
            get {
                return _provider.CategoryMap;
            }
        }

        private void BufferContentTypeChanged(object sender, ContentTypeChangedEventArgs e) {
            _tokenCache.Clear();
            _buffer.Changed -= BufferChanged;
            _buffer.ContentTypeChanged -= BufferContentTypeChanged;
            _buffer.Properties.RemoveProperty(typeof(IDlrClassifier));
        }

        private void BufferChanged(object sender, TextContentChangedEventArgs e) {
            var snapshot = e.After;

            IMixedBuffer mixedBuffer;
            snapshot.TextBuffer.Properties.TryGetProperty<IMixedBuffer>(typeof(IMixedBuffer), out mixedBuffer);

            _tokenCache.EnsureCapacity(snapshot.LineCount);
            
            foreach (var change in e.Changes) {
                if (change.LineCountDelta > 0) {
                    _tokenCache.InsertLines(snapshot.GetLineNumberFromPosition(change.NewEnd) + 1 - change.LineCountDelta, change.LineCountDelta);
                } else if (change.LineCountDelta < 0) {
                    _tokenCache.DeleteLines(snapshot.GetLineNumberFromPosition(change.NewEnd) + 1, -change.LineCountDelta);
                }

                if (mixedBuffer != null) {
                    foreach (SnapshotSpan codeSpan in mixedBuffer.GetLanguageSpans(snapshot)) {
                        // we want the intersection here because we care about empty spans for deletes.
                        SnapshotSpan? intersection = codeSpan.Intersection(change.NewSpan);
                        if (intersection != null) {
                            var firstCodeLine = codeSpan.Start.GetContainingLine();
                            ApplyChange(snapshot, intersection.Value.Span, firstCodeLine.LineNumber, codeSpan.Start - firstCodeLine.Start);
                        }
                    }
                } else {
                    ApplyChange(snapshot, change.NewSpan, 0, 0);
                }
            }
        }

        /// <summary>
        /// Adds classification spans to the given collection.
        /// Scans a contiguous sub-<paramref name="span"/> of a larger code span which starts at <paramref name="codeStartLine"/>.
        /// </summary>
        private void AddClassifications(List<ClassificationSpan> classifications, SnapshotSpan span, int codeStartLine, int codeStartLineOffset) {
            Debug.Assert(span.Length > 0);

            var snapshot = span.Snapshot;            
            int firstLine = snapshot.GetLineNumberFromPosition(span.Start);
            int lastLine = snapshot.GetLineNumberFromPosition(span.End - 1);

            Contract.Assert(codeStartLineOffset >= 0);
            Contract.Assert(firstLine >= codeStartLine);

            _tokenCache.EnsureCapacity(snapshot.LineCount);

            // find the closest line preceding firstLine for which we know categorizer state, stop at the codeStartLine:
            LineTokenization lineTokenization;
            int currentLine = _tokenCache.IndexOfPreviousTokenization(firstLine, codeStartLine, out lineTokenization) + 1;
            object state = lineTokenization.State;

            while (currentLine <= lastLine) {
                if (!_tokenCache.TryGetTokenization(currentLine, out lineTokenization)) {
                    lineTokenization = TokenizeLine(snapshot, state, currentLine, (currentLine == codeStartLine) ? codeStartLineOffset : 0);
                    _tokenCache[currentLine] = lineTokenization;
                }

                state = lineTokenization.State;

                classifications.AddRange(
                    from token in lineTokenization.Tokens
                    let classification = ClassifyToken(span, token, currentLine)
                    where classification != null
                    select classification
                );

                currentLine++;
            }
        }

        /// <summary>
        /// Rescans the part of the buffer affected by a change. 
        /// Scans a contiguous sub-<paramref name="span"/> of a larger code span which starts at <paramref name="codeStartLine"/>.
        /// </summary>
        private void ApplyChange(ITextSnapshot snapshot, Span span, int codeStartLine, int codeStartLineOffset) {
            int firstLine = snapshot.GetLineNumberFromPosition(span.Start);
            int lastLine = snapshot.GetLineNumberFromPosition(span.Length > 0 ? span.End - 1 : span.End);

            Contract.Assert(codeStartLineOffset >= 0);
            Contract.Assert(firstLine >= codeStartLine);

            // find the closest line preceding firstLine for which we know categorizer state, stop at the codeStartLine:
            LineTokenization lineTokenization;
            firstLine = _tokenCache.IndexOfPreviousTokenization(firstLine, codeStartLine, out lineTokenization) + 1;
            object state = lineTokenization.State;

            int currentLine = firstLine;
            object previousState;
            while (currentLine < snapshot.LineCount) {
                previousState = _tokenCache.TryGetTokenization(currentLine, out lineTokenization) ? lineTokenization.State : null;
                _tokenCache[currentLine] = lineTokenization = TokenizeLine(snapshot, state, currentLine, (currentLine == codeStartLine) ? codeStartLineOffset : 0);
                state = lineTokenization.State;

                // stop if we visted all affected lines and the current line has no tokenization state or its previous state is the same as the new state:
                if (currentLine > lastLine && (previousState == null || previousState.Equals(state))) {
                    break;
                }

                currentLine++;
            }

            // classification spans might have changed between the start of the first and end of the last visited line:
            int changeStart = snapshot.GetLineFromLineNumber(firstLine).Start;
            int changeEnd = (currentLine < snapshot.LineCount) ? snapshot.GetLineFromLineNumber(currentLine).End : snapshot.Length;
            if (changeStart < changeEnd) {
                var classificationChanged = ClassificationChanged;
                if (classificationChanged != null) {
                    var args = new ClassificationChangedEventArgs(new SnapshotSpan(snapshot, new Span(changeStart, changeEnd - changeStart)));
                    classificationChanged(this, args);
                }
            }
        }

        internal LineTokenization TokenizeLine(ITextSnapshot snapshot, object previousLineState, int lineNo, int lineOffset) {
            ITextSnapshotLine line = snapshot.GetLineFromLineNumber(lineNo);
            SnapshotSpan lineSpan = new SnapshotSpan(snapshot, line.Start + lineOffset, line.LengthIncludingLineBreak - lineOffset);

            var tcp = new SnapshotSpanTextContentProvider(lineSpan);
            var scriptSource = _engine.CreateScriptSource(tcp, null, SourceCodeKind.File);

            _categorizer.Initialize(previousLineState, scriptSource, new SourceLocation(lineOffset, lineNo + 1, lineOffset + 1));
            var tokens = new List<TokenInfo>(_categorizer.ReadTokens(lineSpan.Length)).ToArray();
            return new LineTokenization(tokens, _categorizer.CurrentState);
        }

        private ClassificationSpan ClassifyToken(SnapshotSpan span, TokenInfo token, int lineNumber) {
            IClassificationType classification = null;

            if (token.Category == TokenCategory.Operator) {
                if (token.Trigger == TokenTriggers.MemberSelect) {
                    classification = _provider.DotClassification;
                }
            } else if (token.Category == TokenCategory.Grouping) {
                if (token.Trigger == (TokenTriggers.MatchBraces | TokenTriggers.ParameterStart)) {
                    classification = _provider.OpenGroupingClassification;
                } else if (token.Trigger == (TokenTriggers.MatchBraces | TokenTriggers.ParameterEnd)) {
                    classification = _provider.CloseGroupingClassification;
                }
            } else if (token.Category == TokenCategory.Delimiter) {
                if (token.Trigger == TokenTriggers.ParameterNext) {
                    classification = _provider.CommaClassification;
                }
            }

            if (classification == null) {
                CategoryMap.TryGetValue(token.Category, out classification);
            }

            if (classification != null) {
                var line = span.Snapshot.GetLineFromLineNumber(lineNumber);
                var index = line.Start.Position + token.SourceSpan.Start.Column - 1;
                var tokenSpan = new Span(index, token.SourceSpan.Length);
                var intersection = span.Intersection(tokenSpan);
                if (intersection != null && intersection.Value.Length > 0) {
                    return new ClassificationSpan(new SnapshotSpan(span.Snapshot, tokenSpan), classification);
                }
            }

            return null;
        }

        #endregion
    }

    public static class DlrClassifierExtensions {
        public static IDlrClassifier GetDlrClassifier(this ITextBuffer buffer) {
            IDlrClassifier res;
            if (buffer.Properties.TryGetProperty<IDlrClassifier>(typeof(IDlrClassifier), out res)) {
                return res;
            }
            return null;
        }
    }
}
