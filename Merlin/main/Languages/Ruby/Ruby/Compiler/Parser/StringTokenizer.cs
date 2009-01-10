/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;

namespace IronRuby.Compiler {

    internal abstract class StringTokenizer {
        internal abstract Tokens Tokenize(Tokenizer/*!*/ tokenizer);
    }

    [Flags]
    public enum StringType {
        Default = 0,
        ExpandsEmbedded = 1,
        RegularExpression = 2,
        Words = 4,
        Symbol = 8,
        IndentedHeredoc = 16,
        FinalWordSeparator = 32,
    }

    internal sealed class StringContentTokenizer : StringTokenizer {
        private StringType _properties;

        // The number of opening parentheses that need to be closed before the string terminates.
        // The level is tracked for all currently opened strings.
        // For example, %< .. < .. < #{...} .. > .. > .. > comprises of 3 parts:
        //   %< .. < .. <     
        //   #{...}
        //   .. > .. > .. >
        // After reading the first part the nested level for the string is set to 2.
        private int _nestingLevel;

        // The character terminating the string:
        private readonly char _terminator;

        // Parenthesis opening the string; if non-zero parenthesis of this kind is balanced before the string is closed:
        private readonly char _openingParenthesis;

        public StringContentTokenizer(StringType properties, char terminator) 
            : this(properties, terminator, '\0') {
        }

        public StringContentTokenizer(StringType properties, char terminator, char openingParenthesis) {
            Debug.Assert(!Tokenizer.IsLetterOrDigit(terminator));

            _properties = properties;
            _terminator = terminator;
            _openingParenthesis = openingParenthesis;
            _nestingLevel = 0;
        }

        public StringType Properties {
            get { return _properties; }
            set { _properties = value; }
        }

        public int NestingLevel {
            get { return _nestingLevel; }
            set { _nestingLevel = value; }
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

        internal override Tokens Tokenize(Tokenizer/*!*/ tokenizer) {
            return tokenizer.TokenizeString(this);
        }
    }

    internal sealed class HeredocTokenizer : StringTokenizer {

        private readonly StringType _properties;
        private readonly string _label;
        private readonly int _resumePosition;
        private readonly int _resumeLineLength;
        private readonly char[] _resumeLine;
        private readonly int _firstLine;
        private readonly int _firstLineIndex;

        public HeredocTokenizer(StringType properties, string label, int resumePosition, char[] resumeLine, int resumeLineLength, int firstLine, int firstLineIndex) {
            _properties = properties;
            _label = label;
            _resumePosition = resumePosition;
            _resumeLine = resumeLine;
            _resumeLineLength = resumeLineLength;
            _firstLine = firstLine;
            _firstLineIndex = firstLineIndex;
        }

        public StringType Properties {
            get { return _properties; }
        }

        public string Label {
            get { return _label; }
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

        public override string ToString() {
            return String.Format("Heredoc({0},'{1}',{2},'{2}')", _properties, _label, _resumePosition, new String(_resumeLine));
        }

        internal override Tokens Tokenize(Tokenizer/*!*/ tokenizer) {
            return tokenizer.TokenizeHeredoc(this);
        }
    }
}
