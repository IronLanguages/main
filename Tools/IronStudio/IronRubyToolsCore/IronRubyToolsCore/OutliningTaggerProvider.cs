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
using System.ComponentModel.Composition;
using System.Diagnostics;
using IronRuby.Compiler.Ast;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Core;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronRubyTools {
    [Export(typeof(ITaggerProvider)), ContentType(RubyCoreConstants.ContentType)]
    [TagType(typeof(IOutliningRegionTag))]
    class OutliningTaggerProvider : ITaggerProvider {
        private readonly IRubyRuntimeHost _host;

        [ImportingConstructor]
        public OutliningTaggerProvider(IRubyRuntimeHost host) {
            _host = host;
        }

        #region ITaggerProvider Members

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag {
            return (ITagger<T>)(buffer.GetOutliningTagger() ?? new OutliningTagger(buffer, _host));
        }

        #endregion

        internal class OutliningTagger : ITagger<IOutliningRegionTag> {
            private readonly ITextBuffer _buffer;
            private bool _enabled;

            public OutliningTagger(ITextBuffer buffer, IRubyRuntimeHost host) {
                _buffer = buffer;
                _buffer.Properties[typeof(OutliningTagger)] = this;
                _enabled = host.EnterOutliningModeOnOpen;
            }

            public bool Enabled {
                get {
                    return _enabled;
                }
            }

            public void Enable() {
                _enabled = true;
                var snapshot = _buffer.CurrentSnapshot;
                var tagsChanged = TagsChanged;
                if (tagsChanged != null) {
                    tagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, new Span(0, snapshot.Length))));
                }
            }

            public void Disable() {
                _enabled = false;
                var snapshot = _buffer.CurrentSnapshot;
                var tagsChanged = TagsChanged;
                if (tagsChanged != null) {
                    tagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, new Span(0, snapshot.Length))));
                }
            }

            #region ITagger<IOutliningRegionTag> Members

            public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans) {
                return new ITagSpan<IOutliningRegionTag>[0];
            }

            public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

            #endregion
        }

        class TagSpan : ITagSpan<IOutliningRegionTag> {
            private readonly SnapshotSpan _span;
            private readonly OutliningTag _tag;
            
            public TagSpan(SnapshotSpan span, OutliningTag tag) {
                _span = span;
                _tag = tag;
            }

            #region ITagSpan<IOutliningRegionTag> Members

            public SnapshotSpan Span {
                get { return _span; }
            }

            public IOutliningRegionTag Tag {
                get { return _tag; }
            }

            #endregion
        }

        class OutliningTag : IOutliningRegionTag {
            private readonly ITextSnapshot _snapshot;
            private readonly Span _span;
            private readonly bool _isImplementation;

            public OutliningTag(ITextSnapshot iTextSnapshot, Span span, bool isImplementation) {
                _snapshot = iTextSnapshot;
                _span = span;
                _isImplementation = isImplementation;
            }

            #region IOutliningRegionTag Members

            public object CollapsedForm {
                get { return "..."; }
            }

            public object CollapsedHintForm {
                get {
                    string collapsedHint = _snapshot.GetText(_span);

                    string[] lines = collapsedHint.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    // remove any leading white space for the preview
                    if (lines.Length > 0) {
                        int smallestWhiteSpace = Int32.MaxValue;
                        for (int i = 0; i < lines.Length; i++) {
                            string curLine = lines[i];

                            for (int j = 0; j < curLine.Length; j++) {
                                if (curLine[j] != ' ') {
                                    smallestWhiteSpace = Math.Min(j, smallestWhiteSpace);
                                }
                            }
                        }

                        for (int i = 0; i < lines.Length; i++) {
                            if (lines[i].Length >= smallestWhiteSpace) {
                                lines[i] = lines[i].Substring(smallestWhiteSpace);
                            }
                        }

                        return String.Join("\r\n", lines);
                    }
                    return collapsedHint; 
                }
            }

            public bool IsDefaultCollapsed {
                get { return false; }
            }

            public bool IsImplementation {
                get { return _isImplementation; }
            }

            #endregion
        }
    }

    static class OutliningTaggerProviderExtensions {
        public static OutliningTaggerProvider.OutliningTagger GetOutliningTagger(this ITextView self) {
            return self.TextBuffer.GetOutliningTagger();
        }

        public static OutliningTaggerProvider.OutliningTagger GetOutliningTagger(this ITextBuffer self) {
            OutliningTaggerProvider.OutliningTagger res;
            if (self.Properties.TryGetProperty<OutliningTaggerProvider.OutliningTagger>(typeof(OutliningTaggerProvider.OutliningTagger), out res)) {
                return res;
            }
            return null;
        }
    }
}
