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

using Microsoft.IronPythonTools.Intellisense;
using Microsoft.PyAnalysis;
using Microsoft.Scripting.Hosting;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.IronPythonTools.Internal {
    public static class Extensions {
        public static StandardGlyphGroup ToGlyphGroup(this ResultType objectType) {
            StandardGlyphGroup group;
            switch (objectType) {
                case ResultType.Class: group = StandardGlyphGroup.GlyphGroupClass; break;
                case ResultType.DelegateInstance: 
                case ResultType.Delegate: group = StandardGlyphGroup.GlyphGroupDelegate; break;
                case ResultType.Enum: group = StandardGlyphGroup.GlyphGroupEnum; break;
                case ResultType.Namespace: group = StandardGlyphGroup.GlyphGroupNamespace; break;
                case ResultType.Multiple: group = StandardGlyphGroup.GlyphGroupOverload; break;
                case ResultType.Field: group = StandardGlyphGroup.GlyphGroupField; break;
                case ResultType.Module: group = StandardGlyphGroup.GlyphGroupModule; break;
                case ResultType.Property: group = StandardGlyphGroup.GlyphGroupProperty; break;
                case ResultType.Instance: group = StandardGlyphGroup.GlyphGroupVariable; break;
                case ResultType.Constant: group = StandardGlyphGroup.GlyphGroupConstant; break;
                case ResultType.EnumInstance: group = StandardGlyphGroup.GlyphGroupEnumMember; break;
                case ResultType.Event: group = StandardGlyphGroup.GlyphGroupEvent; break;
                case ResultType.Function:
                case ResultType.Method:
                default:
                    group = StandardGlyphGroup.GlyphGroupMethod;
                    break;
            }
            return group;
        }

        internal static bool TryGetAnalysis(this ITextBuffer buffer, out IProjectEntry analysis) {
            return buffer.Properties.TryGetProperty<IProjectEntry>(typeof(IProjectEntry), out analysis);
        }

        internal static bool TryGetPythonAnalysis(this ITextBuffer buffer, out IPythonProjectEntry analysis) {
            IProjectEntry entry;
            if (buffer.TryGetAnalysis(out entry) && (analysis = entry as IPythonProjectEntry) != null) {
                return true;
            }
            analysis = null;
            return false;
        }

        internal static IProjectEntry GetAnalysis(this ITextBuffer buffer) {
            IProjectEntry res;
            buffer.TryGetAnalysis(out res);
            return res;
        }

        internal static IPythonProjectEntry GetPythonAnalysis(this ITextBuffer buffer) {
            IPythonProjectEntry res;
            buffer.TryGetPythonAnalysis(out res);
            return res;
        }

        internal static string GetFilePath(this ITextView textView) {
            return textView.TextBuffer.GetFilePath();
        }

        internal static string GetFilePath(this ITextBuffer textBuffer) {
            ITextDocument textDocument;
            if (textBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDocument)) {
                return textDocument.FilePath;
            } else {
                return null;
            }
        }

        internal static ITrackingSpan CreateTrackingSpan(this IIntellisenseSession session, ITextBuffer buffer) {
            var triggerPoint = session.GetTriggerPoint(buffer);
            var position = session.GetTriggerPoint(buffer).GetPosition(session.TextView.TextSnapshot);

            var snapshot = buffer.CurrentSnapshot;
            if (position == snapshot.Length) {
                return snapshot.CreateTrackingSpan(position, 0, SpanTrackingMode.EdgeInclusive);
            } else {
                return snapshot.CreateTrackingSpan(position, 1, SpanTrackingMode.EdgeInclusive);
            }
        }

        internal static ITrackingSpan CreateTrackingSpan0(this IIntellisenseSession session, ITextBuffer buffer) {
            var triggerPoint = session.GetTriggerPoint(buffer);
            var position = session.GetTriggerPoint(buffer).GetPosition(session.TextView.TextSnapshot);

            var snapshot = buffer.CurrentSnapshot;
            return snapshot.CreateTrackingSpan(position, 0, SpanTrackingMode.EdgeInclusive);
        }
    }
}
