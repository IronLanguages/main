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

using Microsoft.Scripting.Hosting;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.IronRubyTools {
    static class Extensions {
        internal static string GetFilePath(this ITextView textView) {
            ITextDocument textDocument;
            if (textView.TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDocument)) {
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
