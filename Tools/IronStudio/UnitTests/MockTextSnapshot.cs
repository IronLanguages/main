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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;

namespace UnitTests {
    class MockTextSnapshot : ITextSnapshot {
        private readonly MockTextBuffer _buffer;

        public MockTextSnapshot(MockTextBuffer mockTextBuffer) {
            _buffer = mockTextBuffer;
        }

        public Microsoft.VisualStudio.Utilities.IContentType ContentType {
            get { throw new NotImplementedException(); }
        }

        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count) {
            _buffer._text.CopyTo(sourceIndex, destination, destinationIndex, count);
        }

        public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode, TrackingFidelityMode trackingFidelity) {
            throw new NotImplementedException();
        }

        public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode) {
            throw new NotImplementedException();
        }

        public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity) {
            throw new NotImplementedException();
        }

        public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode) {
            throw new NotImplementedException();
        }

        public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity) {
            throw new NotImplementedException();
        }

        public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode) {
            return new MockTrackingSpan(this, span.Start, span.Length);
        }

        private string[] GetLines() {
            return _buffer._text.Split(new[] { "\r\n" }, StringSplitOptions.None);
        }

        public ITextSnapshotLine GetLineFromLineNumber(int lineNumber) {
            string[] lines = GetLines();
            for (int i = 0, curPosition = 0; i < lines.Length; i++) {
                if (i == lineNumber) {
                    return new MockTextSnapshotLine(this, lines[i], i, curPosition);
                }
                curPosition += lines[i].Length + 2;
            }
            return new MockTextSnapshotLine(this, "", lines.Length, _buffer._text.Length);
        }

        public ITextSnapshotLine GetLineFromPosition(int position) {
            string[] lines = GetLines();
            for(int i = 0, curPosition = 0; i<lines.Length;i++) {
                if(curPosition + lines[i].Length >= position) {
                    return new MockTextSnapshotLine(this, lines[i], i, curPosition);
                }
                curPosition += lines[i].Length + 2;
            }
            return new MockTextSnapshotLine(this, "", lines.Length, _buffer._text.Length);
        }

        public int GetLineNumberFromPosition(int position) {
            return GetLineFromPosition(position).LineNumber;
        }

        public string GetText() {
            return _buffer._text;
        }

        public string GetText(int startIndex, int length) {
            return GetText().Substring(startIndex, length);
        }

        public string GetText(Span span) {
            return GetText().Substring(span.Start, span.Length);
        }

        public int Length {
            get { return _buffer._text.Length; }
        }

        public int LineCount {
            get { return GetLines().Length; }
        }

        public IEnumerable<ITextSnapshotLine> Lines {
            get { throw new NotImplementedException(); }
        }

        public ITextBuffer TextBuffer {
            get { return _buffer; }
        }

        public char[] ToCharArray(int startIndex, int length) {
            throw new NotImplementedException();
        }

        public ITextVersion Version {
            get { return new MockTextVersion(); }
        }

        public void Write(System.IO.TextWriter writer) {
            throw new NotImplementedException();
        }

        public void Write(System.IO.TextWriter writer, Span span) {
            throw new NotImplementedException();
        }

        public char this[int position] {
            get { return _buffer._text[position]; }
        }
    }
}
