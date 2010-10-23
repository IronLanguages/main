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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Scripting.Utils;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronStudio.Core.Repl {
    /// <summary>
    /// Any <see cref="ITextBuffer"/> with content type <see cref="CoreConstants.DlrContentTypeName"/>, role <see cref="CoreConstants.ReplTextViewRole"/> 
    /// and property <see cref="IReplPromptProvider"/> gets prompt glyphs in its glyph margin.
    /// </summary>
    public interface IReplPromptProvider {
        /// <summary>
        /// The prompt text to display in the margin.
        /// </summary>
        string/*!*/ Prompt { get; }

        /// <summary>
        /// The control that hosts the text view.
        /// </summary>
        Control/*!*/ HostControl { get; }

        /// <summary>
        /// Should we draw a prompt glyph for given line.
        /// </summary>
        bool HasPromptForLine(ITextSnapshot/*!*/ snapshot, int lineNumber);

        /// <summary>
        /// Notifies glyph margin that prompt glyph(s) need to be updated.
        /// </summary>
        event Action<SnapshotSpan> PromptChanged;
    }

    /// <summary>
    /// Implements prompt glyphs in a GlyphMargin. 
    /// </summary>
    internal static class ReplPrompt {
        internal const string GlyphName = "ReplPromptGlyph";

        internal sealed class ReplGlyphTag : IGlyphTag {
            internal static readonly ReplGlyphTag Instance = new ReplGlyphTag();
        }

        internal sealed class Tagger : ITagger<ReplGlyphTag> {
            private readonly IReplPromptProvider/*!*/ _promptProvider;

            public Tagger(IReplPromptProvider/*!*/ promptProvider) {
                Assert.NotNull(promptProvider);
                _promptProvider = promptProvider;
                _promptProvider.PromptChanged += new Action<SnapshotSpan>((span) => {
                    var tagsChanged = TagsChanged;
                    if (tagsChanged != null) {
                        tagsChanged(this, new SnapshotSpanEventArgs(span));
                    }
                });
            }

            public IEnumerable<ITagSpan<ReplGlyphTag>>/*!*/ GetTags(NormalizedSnapshotSpanCollection/*!*/ spans) {
                foreach (SnapshotSpan span in spans) {
                    if (_promptProvider.HasPromptForLine(span.Snapshot, span.Start.GetContainingLine().LineNumber)) {
                        yield return new TagSpan<ReplGlyphTag>(span, ReplGlyphTag.Instance);
                    }
                }
            }

            public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
        }

        [Export(typeof(ITaggerProvider))]
        [TagType(typeof(ReplGlyphTag))]
        [ContentType(CoreConstants.DlrContentTypeName)]
        [TextViewRole(CoreConstants.ReplTextViewRole)]
        internal sealed class TaggerProvider : ITaggerProvider {
            public ITagger<T> CreateTagger<T>(ITextBuffer/*!*/ buffer) where T : ITag {
                IReplPromptProvider promptProvider;
                if (buffer.Properties.TryGetProperty(typeof(IReplPromptProvider), out promptProvider)) {
                    return (ITagger<T>)(object)new Tagger(promptProvider);
                }
                return null;
            }
        }

        internal sealed class GlyphFactory : IGlyphFactory {
            private readonly IReplPromptProvider/*!*/ _promptProvider;
            private static readonly FontFamily _Consolas = new FontFamily("Consolas");

            public GlyphFactory(IReplPromptProvider/*!*/ promptProvider) {
                Assert.NotNull(promptProvider);
                _promptProvider = promptProvider;
            }

            public UIElement/*!*/ GenerateGlyph(IWpfTextViewLine/*!*/ line, IGlyphTag tag) {
                TextBlock block = new TextBlock();
                block.Text = _promptProvider.Prompt;
                block.Foreground = _promptProvider.HostControl.Foreground;
                block.FontSize = _promptProvider.HostControl.FontSize;
                block.FontFamily = _Consolas; // TODO: get the font family from the editor?
                return block;
            }
        }

        [Export(typeof(IGlyphFactoryProvider))]
        [Name(GlyphName)]
        [Order(After = "VsTextMarker")]
        [TagType(typeof(ReplGlyphTag))]
        [ContentType(CoreConstants.DlrContentTypeName)]
        [TextViewRole(CoreConstants.ReplTextViewRole)]
        internal sealed class GlyphFactoryProvider : IGlyphFactoryProvider {
            public IGlyphFactory GetGlyphFactory(IWpfTextView/*!*/ view, IWpfTextViewMargin/*!*/ margin) {
                IReplPromptProvider promptProvider;
                if (view.TextBuffer.Properties.TryGetProperty(typeof(IReplPromptProvider), out promptProvider)) {
                    return new GlyphFactory(promptProvider);
                }
                return null;
            }
        }
    }
}
