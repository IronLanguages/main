/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;

namespace IronRuby.Compiler {
    internal class TokenSequenceState {
        internal static readonly TokenSequenceState None = new TokenSequenceState();
        
        internal virtual Tokens TokenizeAndMark(Tokenizer/*!*/ tokenizer) {
            Tokens result = tokenizer.Tokenize();
            tokenizer.CaptureTokenSpan();
            return result;
        }

        public override string ToString() {
            return "";
        }
    }

    [Flags]
    public enum StringProperties : byte {
        Default = 0,
        ExpandsEmbedded = 1,
        RegularExpression = 2,
        Words = 4,
        Symbol = 8,
        IndentedHeredoc = 16,
    }

    internal sealed class CodeState : TokenSequenceState, IEquatable<CodeState> {
        internal readonly LexicalState LexicalState;

        // true if the following identifier is treated as a command name (sets LexicalState.CMDARG):
        internal readonly byte CommandMode;

        // true if the previous token is Tokens.Whitespace:
        internal readonly byte WhitespaceSeen;

        public CodeState(LexicalState lexicalState, byte commandMode, byte whitespaceSeen) {
            LexicalState = lexicalState;
            CommandMode = commandMode;
            WhitespaceSeen = whitespaceSeen;
        }

        public override bool Equals(object other) {
            return Equals(other as CodeState);
        }

        public bool Equals(CodeState other) {
            return ReferenceEquals(other, this) || (other != null
                && LexicalState == other.LexicalState
                && CommandMode == other.CommandMode
                && WhitespaceSeen == other.WhitespaceSeen
            );
        }

        public override int GetHashCode() {
            return (int)LexicalState
                 ^ (int)CommandMode
                 ^ (int)WhitespaceSeen;
        }
    }

    internal sealed class StringState : TokenSequenceState, IEquatable<StringState> {
        // The number of opening parentheses that need to be closed before the string terminates.
        // The level is tracked for all currently opened strings.
        // For example, %< .. < .. < #{...} .. > .. > .. > comprises of 3 parts:
        //   %< .. < .. <     
        //   #{...}
        //   .. > .. > .. >
        // After reading the first part the nested level for the string is set to 2.
        private readonly int _nestingLevel;

        private readonly StringProperties _properties;

        // The character terminating the string:
        private readonly char _terminator;

        // Parenthesis opening the string; if non-zero parenthesis of this kind is balanced before the string is closed:
        private readonly char _openingParenthesis;

        public StringState(StringProperties properties, char terminator) 
            : this(properties, terminator, '\0', 0) {
        }

        public StringState(StringProperties properties, char terminator, char openingParenthesis, int nestingLevel) {
            Debug.Assert(!Tokenizer.IsLetterOrDigit(terminator));

            _properties = properties;
            _terminator = terminator;
            _openingParenthesis = openingParenthesis;
            _nestingLevel = nestingLevel;
        }

        public override bool Equals(object other) {
            return Equals(other as StringState);
        }

        public bool Equals(StringState other) {
            return ReferenceEquals(other, this) || (other != null  
                && _nestingLevel == other._nestingLevel 
                && _properties == other._properties
                && _terminator == other._terminator
                && _openingParenthesis == other._openingParenthesis
            );
        }

        public override int GetHashCode() {
            return _nestingLevel
                ^ (int)_properties 
                ^ (int)_terminator 
                ^ (int)_openingParenthesis;
        }

        public StringState/*!*/ SetNesting(int level) {
            return (_nestingLevel == level) ? this : new StringState(_properties, _terminator, _openingParenthesis, level);
        }

        public StringProperties Properties {
            get { return _properties; }
        }

        public int NestingLevel {
            get { return _nestingLevel; }
        }

        public char TerminatingCharacter {
            get { return _terminator; }
        }

        public char OpeningParenthesis {
            get { return _openingParenthesis; }
        }

        public override string ToString() {
            return String.Format("StringTerminator({0},{1},{2},{3},{4})", _properties, (int)_terminator, (int)_openingParenthesis, 0, _nestingLevel);
        }

        internal override Tokens TokenizeAndMark(Tokenizer/*!*/ tokenizer) {
            Tokens result = tokenizer.TokenizeString(this);
            tokenizer.CaptureTokenSpan();
            return result;
        }
    }

    internal abstract class HeredocStateBase : TokenSequenceState {
        private readonly StringProperties _properties;
        private readonly string/*!*/ _label;

        public HeredocStateBase(StringProperties properties, string/*!*/ label) {
            _properties = properties;
            _label = label;
        }

        protected bool Equals(HeredocStateBase/*!*/ other) {
            return _properties == other._properties
                && _label == other._label;
        }

        protected int GetBaseHashCode() {
            return (int)_properties
                ^ _label.GetHashCode();
        }

        public StringProperties Properties {
            get { return _properties; }
        }

        public string Label {
            get { return _label; }
        }

        internal abstract Tokens Finish(Tokenizer/*!*/ tokenizer, int labelStart);
    }

    internal sealed class VerbatimHeredocState : HeredocStateBase, IEquatable<VerbatimHeredocState> {
        public VerbatimHeredocState(StringProperties properties, string/*!*/ label) 
            : base(properties, label) {
        }

        public override bool Equals(object other) {
            return Equals(other as VerbatimHeredocState);
        }

        public bool Equals(VerbatimHeredocState other) {
            return ReferenceEquals(other, this) || (other != null
                && base.Equals(other)
            );
        }

        public override int GetHashCode() {
            return GetBaseHashCode();
        }

        internal override Tokens TokenizeAndMark(Tokenizer/*!*/ tokenizer) {
            return tokenizer.TokenizeAndMarkHeredoc(this);
        }

        internal override Tokens Finish(Tokenizer tokenizer, int labelStart) {
            return tokenizer.FinishVerbatimHeredoc(this, labelStart);
        }

        public override string ToString() {
            return String.Format("VerbatimHeredoc({0},'{1}')", Properties, Label);
        }
    }

    internal sealed class HeredocState : HeredocStateBase, IEquatable<HeredocState> {
        private readonly int _resumePosition;
        private readonly int _resumeLineLength;
        private readonly char[] _resumeLine;
        private readonly int _firstLine;
        private readonly int _firstLineIndex;

        internal HeredocState(StringProperties properties, string/*!*/ label, int resumePosition, char[] resumeLine, int resumeLineLength, int firstLine, int firstLineIndex)
            : base(properties, label) {
            _resumePosition = resumePosition;
            _resumeLine = resumeLine;
            _resumeLineLength = resumeLineLength;
            _firstLine = firstLine;
            _firstLineIndex = firstLineIndex;
        }

        public override bool Equals(object other) {
            return Equals(other as HeredocState);
        }

        public bool Equals(HeredocState other) {
            return ReferenceEquals(other, this) || (other != null  
                && base.Equals(other)
                && _resumePosition == other._resumePosition
                // TODO: ??? && _resumeLineLength == other._resumeLineLength
                // TODO: ??? && _resumeLine.ValueEquals(other._resumeLine)
                && _firstLine == other._firstLine
                && _firstLineIndex == other._firstLineIndex
            );
        }

        public override int GetHashCode() {
            return GetBaseHashCode()
                ^ _resumePosition
                ^ _firstLine
                ^ _firstLineIndex;
        }
        
        public int ResumePosition {
            get { return _resumePosition; }
        }


        public char[] ResumeLine {
            get { return _resumeLine; }
        }

        public int ResumeLineLength {
            get { return _resumeLineLength; }
        }
        
        public int FirstLine {
            get { return _firstLine; }
        }
        
        public int FirstLineIndex {
            get { return _firstLineIndex; }
        }

        internal override Tokens TokenizeAndMark(Tokenizer/*!*/ tokenizer) {
            return tokenizer.TokenizeAndMarkHeredoc(this);
        }

        internal override Tokens Finish(Tokenizer/*!*/ tokenizer, int labelStart) {
            return tokenizer.FinishHeredoc(this, labelStart);
        }

        public override string ToString() {
            return String.Format("Heredoc({0},'{1}',{2},'{3}')", Properties, Label, _resumePosition, new String(_resumeLine));
        }
    }

    internal sealed class MultiLineCommentState : TokenSequenceState {
        internal static readonly MultiLineCommentState Instance = new MultiLineCommentState();

        private MultiLineCommentState() {
        }

        internal override Tokens TokenizeAndMark(Tokenizer/*!*/ tokenizer) {
            tokenizer.MarkTokenStart();
            Tokens token = tokenizer.TokenizeMultiLineComment(false);
            tokenizer.CaptureTokenSpan();
            return token;
        }
    }
}
