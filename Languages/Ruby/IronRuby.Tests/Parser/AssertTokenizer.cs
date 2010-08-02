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
using System.Dynamic;
using Microsoft.Scripting;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace IronRuby.Tests {
    internal class AssertTokenizer {
        private readonly RubyContext/*!*/ _context;
        private readonly Tests/*!*/ _tests;
        private Tokenizer _tokenizer;
        private Tokens _actualToken;
        private TokenValue _actualValue;
        private SourceSpan _actualSpan;
        private LoggingErrorSink _log;
        private List<Tokens>/*!*/ _allTokens;
        private List<object>/*!*/ _allValues;
        public RubyCompatibility Compatibility { get; set; }
        public bool Verbatim { get; set; }

        public AssertTokenizer(Tests/*!*/ tests) {
            _log = new LoggingErrorSink();
            _tests = tests;
            _context = tests.Context;
            DefaultEncoding = RubyEncoding.UTF8;
            Compatibility = tests.Context.RubyOptions.Compatibility;
        }

        public Tokenizer Tokenizer {
            get { return _tokenizer; }
        }

        public List<Tokens>/*!*/ AllTokens {
            get { return _allTokens; }
        }

        public List<object>/*!*/ AllValues {
            get { return _allValues; }
        }

        public AssertTokenizer/*!*/ B {
            get { if (Debugger.IsAttached) Debugger.Break(); return this; }
        }

        public RubyEncoding/*!*/ DefaultEncoding { get; set; }

        public void EOF() {
            Read(Tokens.EndOfFile);
            Expect();
        }

        public AssertTokenizer/*!*/ Load(object/*!*/ source, Action<Tokenizer>/*!*/ tokenizerInit) { // source: byte[] or string
            var result = Load(source);
            tokenizerInit(result.Tokenizer);
            return result;
        }

        public AssertTokenizer/*!*/ Load(object/*!*/ source) { // source: byte[] or string
            _tests.Assert(_log.Errors.Count == 0, "Previous test case reported unexpected error/warning(s)");

            SourceUnit sourceUnit;
            RubyEncoding encoding;
            byte[] binarySource = source as byte[];
            if (binarySource != null) {
                encoding = RubyEncoding.Binary;
                sourceUnit = _context.CreateSourceUnit(new BinaryContentProvider(binarySource), null, encoding.Encoding, SourceCodeKind.File);
            } else {
                encoding = DefaultEncoding;
                sourceUnit = _context.CreateSnippet((string)source, SourceCodeKind.File);
            }

            _tokenizer = new Tokenizer(false, DummyVariableResolver.AllMethodNames) {
                ErrorSink = _log,
                Compatibility = Compatibility,
                Encoding = encoding,
                Verbatim = Verbatim,
            };

            _tokenizer.Initialize(sourceUnit);
            _allTokens = new List<Tokens>();
            _allValues = new List<object>();
            return this;
        }

        public AssertTokenizer/*!*/ Skip(int count) {
            while (count-- > 0) {
                Next();
            }
            return this;
        }

        public AssertTokenizer/*!*/ Next() {
            _actualToken = _tokenizer.GetNextToken();
            _actualValue = _tokenizer.TokenValue;
            _actualSpan = _tokenizer.TokenSpan;
            _allTokens.Add(_actualToken);
            _allValues.Add(_actualValue);
            return this;
        }

        public AssertTokenizer/*!*/ Read(Tokens token) {
            Next();
            _tests.Assert(_actualToken == token);
            return this;
        }

        public AssertTokenizer/*!*/ Read(int expected) {
            Next();
            _tests.Assert(_actualToken == Tokens.Integer);
            _tests.Assert(expected == _actualValue.Integer1);
            return this;
        }

        public AssertTokenizer/*!*/ Read(string/*!*/ expected) {
            Next();
            _tests.Assert(_actualToken == Tokens.StringContent);
            _tests.Assert(_actualValue.StringContent is string);
            _tests.Assert(expected == (string)_actualValue.StringContent);
            return this;
        }

        public AssertTokenizer/*!*/ Read(byte[]/*!*/ expected) {
            Next();
            _tests.Assert(_actualToken == Tokens.StringContent);
            _tests.Assert(_actualValue.StringContent is byte[]);
            _tests.Assert(expected.ValueCompareTo(expected.Length, (byte[])_actualValue.StringContent) == 0);
            return this;
        }

        public AssertTokenizer/*!*/ ReadSymbol(Tokens token, string expected) {
            Next();
            _tests.Assert(_actualToken == token);
            _tests.Assert(expected == _actualValue.String);
            return this;
        }

        public AssertTokenizer/*!*/ Read(RubyRegexOptions expected) {
            Next();
            _tests.Assert(_actualToken == Tokens.RegexpEnd);
            _tests.Assert(expected == _actualValue.RegExOptions);
            return this;
        }

        public AssertTokenizer/*!*/ ReadBigInteger(string/*!*/ expected, int @base) {
            Next();
            _tests.Assert(_actualToken == Tokens.BigInteger);
            _tests.Assert(StringComparer.OrdinalIgnoreCase.Compare(_actualValue.BigInteger.ToString(@base), expected) == 0);
            return this;
        }

        public AssertTokenizer/*!*/ Read(double expected) {
            Next();
            _tests.Assert(_actualToken == Tokens.Float);

            if (Double.IsNaN(expected)) {
                _tests.Assert(Double.IsNaN(_actualValue.Double));
            } else if (Double.IsNegativeInfinity(expected)) {
                _tests.Assert(Double.IsNegativeInfinity(_actualValue.Double));
            } else if (Double.IsPositiveInfinity(expected)) {
                _tests.Assert(Double.IsPositiveInfinity(_actualValue.Double));
            } else {
                // TODO: is this correct?
                _tests.Assert(System.Math.Abs(_actualValue.Double - expected) < Double.Epsilon);
            }
            return this;
        }

        public AssertTokenizer/*!*/ Expect(params ErrorInfo[] errors) {
            if (errors == null || errors.Length == 0) {
                _tests.Assert(_log.Errors.Count == 0, "Unexpected error/warning(s)");
            } else {
                _tests.Assert(_log.Errors.Count == errors.Length, String.Format("Expected {0} error/warning(s)", errors.Length));
                for (int i = 0; i < errors.Length; i++) {
                    _tests.Assert(_log.Errors[i].Code == errors[i].Code);
                }
            }
            _log.Errors.Clear();
            return this;
        }

        public AssertTokenizer/*!*/ State(LexicalState expected) {
            _tests.Assert(Tokenizer.LexicalState == expected);
            return this;
        }

        public AssertTokenizer/*!*/ this[Tokens expected] {
            get { return Read(expected); }
        }

        public AssertTokenizer/*!*/ this[string/*!*/ expected] {
            get { return Read(expected); }
        }

        public AssertTokenizer/*!*/ this[string/*!*/ expected, Encoding/*!*/ encoding] {
            get { return Read(encoding.GetBytes(expected)); }
        }

        public AssertTokenizer/*!*/ this[byte[]/*!*/ expected] {
            get { return Read(expected); }
        }

        public AssertTokenizer/*!*/ this[int expected] {
            get { return Read(expected); }
        }

        public AssertTokenizer/*!*/ this[Tokens token, string/*!*/ expected] {
            get {
                Next();
                _tests.Assert(_actualToken == token);
                _tests.Assert(_actualValue.StringContent is string);
                _tests.Assert(expected == (string)_actualValue.StringContent);
                return this; 
            }
        }

    }
}
