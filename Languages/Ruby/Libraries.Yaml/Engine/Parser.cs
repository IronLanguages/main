/***** BEGIN LICENSE BLOCK *****
 * Version: CPL 1.0
 *
 * The contents of this file are subject to the Common Public
 * License Version 1.0 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.eclipse.org/legal/cpl-v10.html
 *
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 *
 * Copyright (C) 2007 Ola Bini <ola@ologix.com>
 * Copyright (c) Microsoft Corporation.
 * 
 ***** END LICENSE BLOCK *****/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;

namespace IronRuby.StandardLibrary.Yaml {
    public class Parser : IEnumerable<YamlEvent> {
        // Memnonics for the production table
        private enum Production {
            STREAM = 0,
            STREAM_START = 1, // TERMINAL
            STREAM_END = 2, // TERMINAL
            IMPLICIT_DOCUMENT = 3,
            EXPLICIT_DOCUMENT = 4,
            DOCUMENT_START = 5,
            DOCUMENT_START_IMPLICIT = 6,
            DOCUMENT_END = 7,
            BLOCK_NODE = 8,
            BLOCK_CONTENT = 9,
            PROPERTIES = 10,
            PROPERTIES_END = 11,
            FLOW_CONTENT = 12,
            BLOCK_SEQUENCE = 13,
            BLOCK_MAPPING = 14,
            FLOW_SEQUENCE = 15,
            FLOW_MAPPING = 16,
            SCALAR = 17,
            BLOCK_SEQUENCE_ENTRY = 18,
            BLOCK_MAPPING_ENTRY = 19,
            BLOCK_MAPPING_ENTRY_VALUE = 20,
            BLOCK_NODE_OR_INDENTLESS_SEQUENCE = 21,
            BLOCK_SEQUENCE_START = 22,
            BLOCK_SEQUENCE_END = 23,
            BLOCK_MAPPING_START = 24,
            BLOCK_MAPPING_END = 25,
            INDENTLESS_BLOCK_SEQUENCE = 26,
            BLOCK_INDENTLESS_SEQUENCE_START = 27,
            INDENTLESS_BLOCK_SEQUENCE_ENTRY = 28,
            BLOCK_INDENTLESS_SEQUENCE_END = 29,
            FLOW_SEQUENCE_START = 30,
            FLOW_SEQUENCE_ENTRY = 31,
            FLOW_SEQUENCE_END = 32,
            FLOW_MAPPING_START = 33,
            FLOW_MAPPING_ENTRY = 34,
            FLOW_MAPPING_END = 35,
            FLOW_INTERNAL_MAPPING_START = 36,
            FLOW_INTERNAL_CONTENT = 37,
            FLOW_INTERNAL_VALUE = 38,
            FLOW_INTERNAL_MAPPING_END = 39,
            FLOW_ENTRY_MARKER = 40,
            FLOW_NODE = 41,
            FLOW_MAPPING_INTERNAL_CONTENT = 42,
            FLOW_MAPPING_INTERNAL_VALUE = 43,
            ALIAS = 44,
            EMPTY_SCALAR = 45,
        }

        private readonly Stack<Production>/*!*/ _parseStack = new Stack<Production>();
        private readonly LinkedList<string>/*!*/ _tags = new LinkedList<string>();
        private readonly LinkedList<string>/*!*/ _anchors = new LinkedList<string>();
        private readonly Dictionary<string, string>/*!*/ _tagHandles = new Dictionary<string, string>();
        private readonly Scanner/*!*/ _scanner;
        private readonly Version/*!*/ _defaultYamlVersion;

        private Version _yamlVersion;
        private bool _done;
        private YamlEvent _currentEvent;
        private string _familyTypePrefix;

        public Parser(Scanner scanner, YamlOptions opts)
            : this(scanner, opts.Version) {
        }

        public Parser(Scanner scanner, Version defaultYamlVersion) {
            _scanner = scanner;
            _defaultYamlVersion = defaultYamlVersion;
            _parseStack.Push(Production.STREAM);
        }

        private void ReportError(string/*!*/ message, params object[]/*!*/ args) {
            throw new ParserException(
                String.Format(CultureInfo.InvariantCulture, message, args) +
                String.Format(CultureInfo.InvariantCulture, " (line {0}, column {1})", _scanner.Line + 1, _scanner.Column + 1)
            );
        }

        public Encoding/*!*/ Encoding {
            get { return _scanner.Encoding; }
        }

        public YamlEvent PeekEvent() {
            if (_currentEvent == null) {
                _currentEvent = ParseStreamNext();
            }
            return _currentEvent;
        }

        public YamlEvent GetEvent() {
            YamlEvent value = PeekEvent();
            _currentEvent = null;
            return value;
        }

        private YamlEvent ParseStreamNext() {
            if (_done) {
                return null;
            }
            while (_parseStack.Count > 0) {
                YamlEvent value = Produce();
                if (null != value) {
                    return value;
                }
            }
            _done = true;
            return null;
        }

        private YamlEvent Produce() {
            var prod = _parseStack.Pop();
            //Console.WriteLine(prod);
            switch (prod) {
                case Production.STREAM: {
                        _parseStack.Push(Production.STREAM_END);
                        _parseStack.Push(Production.EXPLICIT_DOCUMENT);
                        _parseStack.Push(Production.IMPLICIT_DOCUMENT);
                        _parseStack.Push(Production.STREAM_START);
                        return null;
                    }
                case Production.STREAM_START: {
                        _scanner.GetToken();
                        return StreamStartEvent.Instance;
                    }
                case Production.STREAM_END: {
                        _scanner.GetToken();
                        return StreamEndEvent.Instance;
                    }
                case Production.IMPLICIT_DOCUMENT: {
                        Token curr = _scanner.PeekToken();
                        if (!(curr is DirectiveToken || curr is DocumentStartToken || curr is StreamEndToken)) {
                            _parseStack.Push(Production.DOCUMENT_END);
                            _parseStack.Push(Production.BLOCK_NODE);
                            _parseStack.Push(Production.DOCUMENT_START_IMPLICIT);
                        }
                        return null;
                    }
                case Production.EXPLICIT_DOCUMENT: {
                        if (!(_scanner.PeekToken() is StreamEndToken)) {
                            _parseStack.Push(Production.EXPLICIT_DOCUMENT);
                            _parseStack.Push(Production.DOCUMENT_END);
                            _parseStack.Push(Production.BLOCK_NODE);
                            _parseStack.Push(Production.DOCUMENT_START);
                        }
                        return null;
                    }
                case Production.DOCUMENT_START: {
                        Token tok = _scanner.PeekToken();
                        Version version;
                        Dictionary<string, string> tags;
                        ProcessDirectives(out version, out tags);
                        if (!(_scanner.PeekToken() is DocumentStartToken)) {
                            ReportError("expected '<document start>', but found: {0}", tok);
                        }
                        _scanner.GetToken();
                        return new DocumentStartEvent(true, version, tags);
                    }
                case Production.DOCUMENT_START_IMPLICIT: {
                        Version version;
                        Dictionary<string, string> tags;
                        ProcessDirectives(out version, out tags);
                        return new DocumentStartEvent(false, version, tags);
                    }
                case Production.DOCUMENT_END: {
                        bool @explicit = false;
                        while (_scanner.PeekToken() is DocumentEndToken) {
                            _scanner.GetToken();
                            @explicit = true;
                        }
                        return @explicit ? DocumentEndEvent.ExplicitInstance : DocumentEndEvent.ImplicitInstance;
                    }
                case Production.BLOCK_NODE: {
                        Token curr = _scanner.PeekToken();
                        if (curr is DirectiveToken || curr is DocumentStartToken || curr is DocumentEndToken || curr is StreamEndToken) {
                            _parseStack.Push(Production.EMPTY_SCALAR);
                        } else {
                            if (curr is AliasToken) {
                                _parseStack.Push(Production.ALIAS);
                            } else {
                                _parseStack.Push(Production.PROPERTIES_END);
                                _parseStack.Push(Production.BLOCK_CONTENT);
                                _parseStack.Push(Production.PROPERTIES);
                            }
                        }
                        return null;
                    }
                case Production.BLOCK_CONTENT: {
                        Token tok = _scanner.PeekToken();
                        if (tok is BlockSequenceStartToken) {
                            _parseStack.Push(Production.BLOCK_SEQUENCE);
                        } else if (tok is BlockMappingStartToken) {
                            _parseStack.Push(Production.BLOCK_MAPPING);
                        } else if (tok is FlowSequenceStartToken) {
                            _parseStack.Push(Production.FLOW_SEQUENCE);
                        } else if (tok is FlowMappingStartToken) {
                            _parseStack.Push(Production.FLOW_MAPPING);
                        } else if (tok is ScalarToken) {
                            _parseStack.Push(Production.SCALAR);
                        } else {
                            // Part of solution for JRUBY-718
                            return new ScalarEvent(_anchors.First.Value, _tags.First.Value ?? Tags.Str, ScalarValueType.String, "", ScalarQuotingStyle.Single);
                        }
                        return null;
                    }
                case Production.PROPERTIES: {
                        string anchor = null;
                        string tag = null;
                        if (_scanner.PeekToken() is AnchorToken) {
                            anchor = ((AnchorToken)_scanner.GetToken()).Value;
                            if (_scanner.PeekToken() is TagToken) {
                                tag = GetTag((TagToken)_scanner.GetToken());
                            }
                        } else if (_scanner.PeekToken() is TagToken) {
                            tag = GetTag((TagToken)_scanner.GetToken());
                            if (_scanner.PeekToken() is AnchorToken) {
                                anchor = ((AnchorToken)_scanner.GetToken()).Value;
                            }
                        }
                        _anchors.AddFirst(anchor);
                        _tags.AddFirst(tag);
                        return null;
                    }
                case Production.PROPERTIES_END: {
                        _anchors.RemoveFirst();
                        _tags.RemoveFirst();
                        return null;
                    }
                case Production.FLOW_CONTENT: {
                        Token tok = _scanner.PeekToken();
                        if (tok is FlowSequenceStartToken) {
                            _parseStack.Push(Production.FLOW_SEQUENCE);
                        } else if (tok is FlowMappingStartToken) {
                            _parseStack.Push(Production.FLOW_MAPPING);
                        } else if (tok is ScalarToken) {
                            _parseStack.Push(Production.SCALAR);
                        } else {
                            ReportError("while scanning a flow node: expected the node content, but found: {0}", tok);
                        }
                        return null;
                    }
                case Production.BLOCK_SEQUENCE: {
                        _parseStack.Push(Production.BLOCK_SEQUENCE_END);
                        _parseStack.Push(Production.BLOCK_SEQUENCE_ENTRY);
                        _parseStack.Push(Production.BLOCK_SEQUENCE_START);
                        return null;
                    }
                case Production.BLOCK_MAPPING: {
                        _parseStack.Push(Production.BLOCK_MAPPING_END);
                        _parseStack.Push(Production.BLOCK_MAPPING_ENTRY);
                        _parseStack.Push(Production.BLOCK_MAPPING_START);
                        return null;
                    }
                case Production.FLOW_SEQUENCE: {
                        _parseStack.Push(Production.FLOW_SEQUENCE_END);
                        _parseStack.Push(Production.FLOW_SEQUENCE_ENTRY);
                        _parseStack.Push(Production.FLOW_SEQUENCE_START);
                        return null;
                    }
                case Production.FLOW_MAPPING: {
                        _parseStack.Push(Production.FLOW_MAPPING_END);
                        _parseStack.Push(Production.FLOW_MAPPING_ENTRY);
                        _parseStack.Push(Production.FLOW_MAPPING_START);
                        return null;
                    }
                case Production.SCALAR: {
                        ScalarToken tok = (ScalarToken)_scanner.GetToken();
                        ScalarValueType scalarType;
                        if ((tok.Style == ScalarQuotingStyle.None && _tags.First.Value == null) || "!" == _tags.First.Value) {
                            scalarType = ScalarValueType.Unknown;
                        } else if (_tags.First.Value == null) {
                            scalarType = ScalarValueType.String;
                        } else {
                            scalarType = ScalarValueType.Other;
                        }
                        return new ScalarEvent(_anchors.First.Value, _tags.First.Value, scalarType, tok.Value, tok.Style);
                    }
                case Production.BLOCK_SEQUENCE_ENTRY: {
                        if (_scanner.PeekToken() is BlockEntryToken) {
                            _scanner.GetToken();
                            if (!(_scanner.PeekToken() is BlockEntryToken || _scanner.PeekToken() is BlockEndToken)) {
                                _parseStack.Push(Production.BLOCK_SEQUENCE_ENTRY);
                                _parseStack.Push(Production.BLOCK_NODE);
                            } else {
                                _parseStack.Push(Production.BLOCK_SEQUENCE_ENTRY);
                                _parseStack.Push(Production.EMPTY_SCALAR);
                            }
                        }
                        return null;
                    }
                case Production.BLOCK_MAPPING_ENTRY: {
                        if (_scanner.PeekToken() is KeyToken || _scanner.PeekToken() is ValueToken) {
                            if (_scanner.PeekToken() is KeyToken) {
                                _scanner.GetToken();
                                Token curr = _scanner.PeekToken();
                                if (!(curr is KeyToken || curr is ValueToken || curr is BlockEndToken)) {
                                    _parseStack.Push(Production.BLOCK_MAPPING_ENTRY);
                                    _parseStack.Push(Production.BLOCK_MAPPING_ENTRY_VALUE);
                                    _parseStack.Push(Production.BLOCK_NODE_OR_INDENTLESS_SEQUENCE);
                                } else {
                                    _parseStack.Push(Production.BLOCK_MAPPING_ENTRY);
                                    _parseStack.Push(Production.BLOCK_MAPPING_ENTRY_VALUE);
                                    _parseStack.Push(Production.EMPTY_SCALAR);
                                }
                            } else {
                                _parseStack.Push(Production.BLOCK_MAPPING_ENTRY);
                                _parseStack.Push(Production.BLOCK_MAPPING_ENTRY_VALUE);
                                _parseStack.Push(Production.EMPTY_SCALAR);
                            }
                        }
                        return null;
                    }
                case Production.BLOCK_MAPPING_ENTRY_VALUE: {
                        if (_scanner.PeekToken() is KeyToken || _scanner.PeekToken() is ValueToken) {
                            if (_scanner.PeekToken() is ValueToken) {
                                _scanner.GetToken();
                                Token curr = _scanner.PeekToken();
                                if (!(curr is KeyToken || curr is ValueToken || curr is BlockEndToken)) {
                                    _parseStack.Push(Production.BLOCK_NODE_OR_INDENTLESS_SEQUENCE);
                                } else {
                                    _parseStack.Push(Production.EMPTY_SCALAR);
                                }
                            } else {
                                _parseStack.Push(Production.EMPTY_SCALAR);
                            }
                        }
                        return null;
                    }
                case Production.BLOCK_NODE_OR_INDENTLESS_SEQUENCE: {
                        if (_scanner.PeekToken() is AliasToken) {
                            _parseStack.Push(Production.ALIAS);
                        } else {
                            if (_scanner.PeekToken() is BlockEntryToken) {
                                _parseStack.Push(Production.INDENTLESS_BLOCK_SEQUENCE);
                                _parseStack.Push(Production.PROPERTIES);
                            } else {
                                _parseStack.Push(Production.BLOCK_CONTENT);
                                _parseStack.Push(Production.PROPERTIES);
                            }
                        }
                        return null;
                    }
                case Production.BLOCK_SEQUENCE_START: {
                        _scanner.GetToken();
                        return new SequenceStartEvent(_anchors.First.Value, _tags.First.Value != "!" ? _tags.First.Value : null, FlowStyle.Block);
                    }
                case Production.BLOCK_SEQUENCE_END: {
                        Token tok = _scanner.PeekToken();
                        if (!(tok is BlockEndToken)) {
                            ReportError("while scanning a block collection: expected <block end>, but found: {0}", tok);
                        }
                        _scanner.GetToken();
                        return SequenceEndEvent.Instance;
                    }
                case Production.BLOCK_MAPPING_START: {
                        _scanner.GetToken();
                        return new MappingStartEvent(_anchors.First.Value, _tags.First.Value != "!" ? _tags.First.Value : null, FlowStyle.Block);
                    }
                case Production.BLOCK_MAPPING_END: {
                        Token tok = _scanner.PeekToken();
                        if (!(tok is BlockEndToken)) {
                            ReportError("while scanning a block mapping: expected <block end>, but found: {0}", tok);
                        }
                        _scanner.GetToken();
                        return MappingEndEvent.Instance;
                    }
                case Production.INDENTLESS_BLOCK_SEQUENCE: {
                        _parseStack.Push(Production.BLOCK_INDENTLESS_SEQUENCE_END);
                        _parseStack.Push(Production.INDENTLESS_BLOCK_SEQUENCE_ENTRY);
                        _parseStack.Push(Production.BLOCK_INDENTLESS_SEQUENCE_START);
                        return null;
                    }
                case Production.BLOCK_INDENTLESS_SEQUENCE_START: {
                        return new SequenceStartEvent(_anchors.First.Value, _tags.First.Value != "!" ? _tags.First.Value : null, FlowStyle.Block);
                    }
                case Production.INDENTLESS_BLOCK_SEQUENCE_ENTRY: {
                        if (_scanner.PeekToken() is BlockEntryToken) {
                            _scanner.GetToken();
                            Token curr = _scanner.PeekToken();
                            if (!(curr is BlockEntryToken || curr is KeyToken || curr is ValueToken || curr is BlockEndToken)) {
                                _parseStack.Push(Production.INDENTLESS_BLOCK_SEQUENCE_ENTRY);
                                _parseStack.Push(Production.BLOCK_NODE);
                            } else {
                                _parseStack.Push(Production.INDENTLESS_BLOCK_SEQUENCE_ENTRY);
                                _parseStack.Push(Production.EMPTY_SCALAR);
                            }
                        }
                        return null;
                    }
                case Production.BLOCK_INDENTLESS_SEQUENCE_END: {
                        return SequenceEndEvent.Instance;
                    }
                case Production.FLOW_SEQUENCE_START: {
                        _scanner.GetToken();
                        return new SequenceStartEvent(_anchors.First.Value, _tags.First.Value != "!" ? _tags.First.Value : null, FlowStyle.Inline);
                    }
                case Production.FLOW_SEQUENCE_ENTRY: {
                        if (!(_scanner.PeekToken() is FlowSequenceEndToken)) {
                            if (_scanner.PeekToken() is KeyToken) {
                                _parseStack.Push(Production.FLOW_SEQUENCE_ENTRY);
                                _parseStack.Push(Production.FLOW_ENTRY_MARKER);
                                _parseStack.Push(Production.FLOW_INTERNAL_MAPPING_END);
                                _parseStack.Push(Production.FLOW_INTERNAL_VALUE);
                                _parseStack.Push(Production.FLOW_INTERNAL_CONTENT);
                                _parseStack.Push(Production.FLOW_INTERNAL_MAPPING_START);
                            } else {
                                _parseStack.Push(Production.FLOW_SEQUENCE_ENTRY);
                                _parseStack.Push(Production.FLOW_NODE);
                                _parseStack.Push(Production.FLOW_ENTRY_MARKER);
                            }
                        }
                        return null;
                    }
                case Production.FLOW_SEQUENCE_END: {
                        _scanner.GetToken();
                        return SequenceEndEvent.Instance;
                    }
                case Production.FLOW_MAPPING_START: {
                        _scanner.GetToken();
                        return new MappingStartEvent(_anchors.First.Value, _tags.First.Value != "!" ? _tags.First.Value : null, FlowStyle.Inline);
                    }
                case Production.FLOW_MAPPING_ENTRY: {
                        if (!(_scanner.PeekToken() is FlowMappingEndToken)) {
                            if (_scanner.PeekToken() is KeyToken) {
                                _parseStack.Push(Production.FLOW_MAPPING_ENTRY);
                                _parseStack.Push(Production.FLOW_ENTRY_MARKER);
                                _parseStack.Push(Production.FLOW_MAPPING_INTERNAL_VALUE);
                                _parseStack.Push(Production.FLOW_MAPPING_INTERNAL_CONTENT);
                            } else {
                                _parseStack.Push(Production.FLOW_MAPPING_ENTRY);
                                _parseStack.Push(Production.FLOW_NODE);
                                _parseStack.Push(Production.FLOW_ENTRY_MARKER);
                            }
                        }
                        return null;
                    }
                case Production.FLOW_MAPPING_END: {
                        _scanner.GetToken();
                        return MappingEndEvent.Instance;
                    }
                case Production.FLOW_INTERNAL_MAPPING_START: {
                        _scanner.GetToken();
                        return new MappingStartEvent(null, null, FlowStyle.Inline);
                    }
                case Production.FLOW_INTERNAL_CONTENT: {
                        Token curr = _scanner.PeekToken();
                        if (!(curr is ValueToken || curr is FlowEntryToken || curr is FlowSequenceEndToken)) {
                            _parseStack.Push(Production.FLOW_NODE);
                        } else {
                            _parseStack.Push(Production.EMPTY_SCALAR);
                        }
                        return null;
                    }
                case Production.FLOW_INTERNAL_VALUE: {
                        if (_scanner.PeekToken() is ValueToken) {
                            _scanner.GetToken();
                            if (!((_scanner.PeekToken() is FlowEntryToken) || (_scanner.PeekToken() is FlowSequenceEndToken))) {
                                _parseStack.Push(Production.FLOW_NODE);
                            } else {
                                _parseStack.Push(Production.EMPTY_SCALAR);
                            }
                        } else {
                            _parseStack.Push(Production.EMPTY_SCALAR);
                        }
                        return null;
                    }
                case Production.FLOW_INTERNAL_MAPPING_END: {
                        return MappingEndEvent.Instance;
                    }
                case Production.FLOW_ENTRY_MARKER: {
                        if (_scanner.PeekToken() is FlowEntryToken) {
                            _scanner.GetToken();
                        }
                        return null;
                    }
                case Production.FLOW_NODE: {
                        if (_scanner.PeekToken() is AliasToken) {
                            _parseStack.Push(Production.ALIAS);
                        } else {
                            _parseStack.Push(Production.PROPERTIES_END);
                            _parseStack.Push(Production.FLOW_CONTENT);
                            _parseStack.Push(Production.PROPERTIES);
                        }
                        return null;
                    }
                case Production.FLOW_MAPPING_INTERNAL_CONTENT: {
                        Token curr = _scanner.PeekToken();
                        if (!(curr is ValueToken || curr is FlowEntryToken || curr is FlowMappingEndToken)) {
                            _scanner.GetToken();
                            _parseStack.Push(Production.FLOW_NODE);
                        } else {
                            _parseStack.Push(Production.EMPTY_SCALAR);
                        }
                        return null;
                    }
                case Production.FLOW_MAPPING_INTERNAL_VALUE: {
                        if (_scanner.PeekToken() is ValueToken) {
                            _scanner.GetToken();
                            if (!(_scanner.PeekToken() is FlowEntryToken || _scanner.PeekToken() is FlowMappingEndToken)) {
                                _parseStack.Push(Production.FLOW_NODE);
                            } else {
                                _parseStack.Push(Production.EMPTY_SCALAR);
                            }
                        } else {
                            _parseStack.Push(Production.EMPTY_SCALAR);
                        }
                        return null;
                    }
                case Production.ALIAS: {
                        AliasToken tok = (AliasToken)_scanner.GetToken();
                        return new AliasEvent(tok.Value);
                    }
                case Production.EMPTY_SCALAR: {
                        return new ScalarEvent(null, null, ScalarValueType.Other, null, ScalarQuotingStyle.None);
                    }
            }

            return null;
        }

        private static Regex ONLY_WORD = YamlUtils.CompiledRegex("^\\w+$");       

        private string GetTag(TagToken tagToken) {
            if (tagToken == null) { // check against "!"?
                return null;
            }
            string tag = null;
            string handle = tagToken.Handle;
            string suffix = tagToken.Suffix;
            int ix = -1;
            if ((ix = suffix.IndexOf('^')) != -1) {
                if (ix > 0) {
                    _familyTypePrefix = suffix.Substring(0, ix);
                }                
                suffix = _familyTypePrefix + suffix.Substring(ix + 1);
            }
            if (handle != null) {
                if (!_tagHandles.ContainsKey(handle)) {
                    ReportError("while parsing a node: found undefined tag handle: {0}", handle);
                }
                if ((ix = suffix.IndexOf('/')) != -1) {
                    string before = suffix.Substring(0, ix);
                    string after = suffix.Substring(ix + 1);
                    if (ONLY_WORD.IsMatch(before)) {
                        tag = "tag:" + before + ".yaml.org,2002:" + after;
                    } else {
                        if (before.StartsWith("tag:", StringComparison.Ordinal)) {
                            tag = before + ":" + after;
                        } else {
                            tag = "tag:" + before + ":" + after;
                        }
                    }
                } else {
                    tag = _tagHandles[handle] + suffix;
                }
            } else {
                tag = suffix;
            }
            return tag;
        }

        private void ProcessDirectives(out Version version, out Dictionary<string, string> tagHandles) {
            while (_scanner.PeekToken() is DirectiveToken) {
                DirectiveToken tok = (DirectiveToken)_scanner.GetToken();
                if (tok.Name == "Yaml") {
                    if (_yamlVersion != null) {
                        ReportError("found duplicate Yaml directive");
                    }
                    int major = int.Parse(tok.Value[0]);
                    int minor = int.Parse(tok.Value[1]);
                    if (major != 1) {
                        ReportError("found incompatible Yaml document (version 1.* is required)");
                    }
                    _yamlVersion = new Version(major, minor);

                } else if (tok.Name == "TAG") {
                    string handle = tok.Value[0];
                    string prefix = tok.Value[1];
                    if (_tagHandles.ContainsKey(handle)) {
                        ReportError("duplicate tag handle: {0}", handle);
                    }
                    _tagHandles.Add(handle, prefix);
                }
            }

            version = _yamlVersion ?? _defaultYamlVersion;
            tagHandles = (_tagHandles.Count > 0) ? new Dictionary<string, string>(_tagHandles) : null;

            if (version.Major == 1 && version.Minor == 0) {
                // == 1.0
                if (!_tagHandles.ContainsKey("!")) {
                    _tagHandles.Add("!", "tag:yaml.org,2002:");
                }
                if (!_tagHandles.ContainsKey("!!")) {
                    _tagHandles.Add("!!", "");
                }
            } else {
                // > 1.0
                if (!_tagHandles.ContainsKey("!")) {
                    _tagHandles.Add("!", "!");
                }
                if (!_tagHandles.ContainsKey("!!")) {
                    _tagHandles.Add("!!", "tag:yaml.org,2002:");
                }
            }
        }


        #region IEnumerable<YamlEvent> Members

        public IEnumerator<YamlEvent> GetEnumerator() {
            YamlEvent e;
            while ((e = GetEvent()) != null) {
                yield return e;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }
}
