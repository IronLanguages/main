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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace IronRuby.StandardLibrary.Yaml {

    public interface IEmitter {
        void Emit(YamlEvent @event);
    }

    public class Emitter : IEmitter {
        enum EmitterState {
            STREAM_START = 0,
            FIRST_DOCUMENT_START,
            DOCUMENT_ROOT,
            NOTHING,
            DOCUMENT_START,
            DOCUMENT_END,
            FIRST_FLOW_SEQUENCE_ITEM,
            FLOW_SEQUENCE_ITEM,
            FIRST_FLOW_MAPPING_KEY,
            FLOW_MAPPING_SIMPLE_VALUE,
            FLOW_MAPPING_VALUE,
            FLOW_MAPPING_KEY,
            BLOCK_SEQUENCE_ITEM,
            FIRST_BLOCK_MAPPING_KEY,
            BLOCK_MAPPING_SIMPLE_VALUE,
            BLOCK_MAPPING_VALUE,
            BLOCK_MAPPING_KEY,
            FIRST_BLOCK_SEQUENCE_ITEM,
        }

        private class ScalarAnalysis {
            internal string Scalar;
            internal bool Empty;
            internal bool Multiline;
            internal bool AllowFlowPlain;
            internal bool AllowBlockPlain;
            internal bool AllowSingleQuoted;
            internal bool AllowDoubleQuoted;
            internal bool AllowBlock;
            internal bool SpecialCharacters;
            internal ScalarAnalysis(string scalar, bool empty, bool multiline, bool allowFlowPlain, bool allowBlockPlain, bool allowSingleQuoted, bool allowDoubleQuoted, bool allowBlock, bool specialCharacters) {
                Scalar = scalar;
                Empty = empty;
                Multiline = multiline;
                AllowFlowPlain = allowFlowPlain;
                AllowBlockPlain = allowBlockPlain;
                AllowSingleQuoted = allowSingleQuoted;
                AllowDoubleQuoted = allowDoubleQuoted;
                AllowBlock = allowBlock;
                SpecialCharacters = specialCharacters;
            }
        }
        
        #region private instance fields

        private readonly TextWriter _stream;
        private readonly YamlOptions _options;

        private readonly Stack<EmitterState> _states = new Stack<EmitterState>();
        private EmitterState _state = EmitterState.STREAM_START;
        private readonly Queue<YamlEvent> _events = new Queue<YamlEvent>();
        private YamlEvent _event;
        private int _flowLevel;
        private readonly Stack<int> _indents = new Stack<int>();
        private int _indent = -1;
        private bool _rootContext;
        private bool _sequenceContext;
        private bool _mappingContext;
        private bool _simpleKeyContext;

        private int _line;
        private int _column;
        private bool _whitespace;
        private bool _indentation;

        private bool canonical;
        private int _bestIndent = 2;
        private int _bestWidth = 80;

        private char _bestLinebreak = '\n';

        private readonly Dictionary<string, string> _tagPrefixes = new Dictionary<string, string>();

        private string _preparedAnchor;
        private string _preparedTag;

        private ScalarAnalysis _analysis;
        private char _style = '\0';

        private bool _isVersion10;

        #endregion

        public Emitter(TextWriter stream, YamlOptions opts) {
            _stream = stream;
            _options = opts;
            canonical = _options.Canonical;
            int propIndent = _options.Indent;
            if (propIndent>=2 && propIndent<10) {
                _bestIndent = propIndent;
            }
            int propWidth = _options.BestWidth;
            if (propWidth != 0 && propWidth > (_bestIndent*2)) {
                _bestWidth = propWidth;
            }
        }

        public void Emit(YamlEvent @event) {
            _events.Enqueue(@event);
            while (!needMoreEvents()) {
                _event = _events.Dequeue();
                switch (_state) {
                    case EmitterState.STREAM_START:
                        expectStreamStart(); 
                        break;
                    case EmitterState.FIRST_DOCUMENT_START: 
                        expectDocumentStart(true); 
                        break;
                    case EmitterState.DOCUMENT_ROOT: 
                        expectDocumentRoot(); 
                        break;
                    case EmitterState.NOTHING: 
                        expectNothing(); 
                        break;
                    case EmitterState.DOCUMENT_START: 
                        expectDocumentStart(false); 
                        break;
                    case EmitterState.DOCUMENT_END: 
                        expectDocumentEnd(); 
                        break;
                    case EmitterState.FIRST_FLOW_SEQUENCE_ITEM: 
                        expectFlowSequenceItem(true);
                        break;
                    case EmitterState.FLOW_SEQUENCE_ITEM: 
                        expectFlowSequenceItem(false);
                        break;
                    case EmitterState.FIRST_FLOW_MAPPING_KEY: 
                        expectFlowMappingKey(true); 
                        break;
                    case EmitterState.FLOW_MAPPING_SIMPLE_VALUE: 
                        expectFlowMappingSimpleValue();
                        break;
                    case EmitterState.FLOW_MAPPING_VALUE: 
                        expectFlowMappingValue();
                        break;
                    case EmitterState.FLOW_MAPPING_KEY: 
                        expectFlowMappingKey(false);
                        break;
                    case EmitterState.BLOCK_SEQUENCE_ITEM: 
                        expectBlockSequenceItem(false); 
                        break;
                    case EmitterState.FIRST_BLOCK_MAPPING_KEY:
                        expectBlockMappingKey(true);
                        break;
                    case EmitterState.BLOCK_MAPPING_SIMPLE_VALUE: 
                        expectBlockMappingSimpleValue();
                        break;
                    case EmitterState.BLOCK_MAPPING_VALUE: 
                        expectBlockMappingValue();
                        break;
                    case EmitterState.BLOCK_MAPPING_KEY: 
                        expectBlockMappingKey(false); 
                        break;
                    case EmitterState.FIRST_BLOCK_SEQUENCE_ITEM: 
                        expectBlockSequenceItem(true); 
                        break;
                    default:
                        Debug.Assert(false, "unreachable");
                        throw new InvalidOperationException("unreachable");
                }
                _event = null;
            }
        }

        void writeStreamStart() {
        }

        void writeStreamEnd() {
            _stream.Flush();
        }
        
        void writeIndicator(string indicator, bool needWhitespace, bool whitespace, bool indentation) {
            if (!_whitespace && needWhitespace) {
                indicator = " " + indicator;
            }
            _whitespace = whitespace;
            _indentation = _indentation && indentation;
            _column += indicator.Length;
            _stream.Write(indicator);
        }

        void writeIndent() {
            int indent = 0;
            if (_indent != -1) {
                indent = _indent;
            }

            if (!_indentation || _column > indent || (_column == indent && !_whitespace)) {
                writeLineBreak();
            }

            if (_column < indent) {
                _whitespace = true;
                string data = new string(' ', indent - _column);
                _column = indent;
                _stream.Write(data);
            }
        }

        void writeVersionDirective(string version_text) {
            _stream.Write("%Yaml " + version_text);
            writeLineBreak();
        }
        
        void writeTagDirective(string handle, string prefix) {
            _stream.Write("%TAG " + handle + " " + prefix);
            writeLineBreak();
        }

        void writeDoubleQuoted(string text, bool split) {
            writeIndicator("\"",true,false,false);
            int start = 0;
            int ending = 0;

            string data = null;
            while (ending <= text.Length) {
                char ch = (char)0;
                if (ending < text.Length) {
                    ch = text[ending];
                }
                if (ch==0 || "\"\\".IndexOf(ch) != -1 || !('\u0020' <= ch && ch <= '\u007E')) {
                    if (start < ending) {
                        data = text.Substring(start,ending-start);
                        _column+=data.Length;
                        _stream.Write(data);
                        start = ending;
                    }
                    if (ch != 0) {
                        string replace = ESCAPE_REPLACEMENTS(ch);
                        if (replace != null) {
                            data = "\\" + replace;
                        } else if (ch <= '\u00FF') {
                            string str = ((byte)ch).ToString("X2");
                            data = "\\x" + str;
                        } else {
                            string str = ((ushort)ch).ToString("X4");
                            data = "\\u" + str;
                        }
                        _column += data.Length;
                        _stream.Write(data);
                        start = ending+1;
                    }
                }

                if ((0 < ending && ending < (text.Length-1)) && (ch == ' ' || start >= ending) && (_column+(ending-start)) > _bestWidth && split) {
                    if (start < ending) {
                        data = text.Substring(start, ending-start) + " \\";
                        start = ending+1;
                    } else {
                        data = "\\";
                    }

                    _column += data.Length;
                    _stream.Write(data);
                    writeIndent();
                    _whitespace = false;
                    _indentation = false;

                    if (start < (text.Length+1) && text[start] == ' ') {
                        _stream.Write('\\');
                    }
                }
                ending += 1;
            }

            writeIndicator("\"",false,false,false);
        }

        void writeSingleQuoted(string text, bool split) {
            writeIndicator("'",true,false,false);
            bool spaces = false;
            bool breaks = false;
            int start=0,ending=0;
            char ceh = '\0';

            string data;
            while (ending <= text.Length) {
                ceh = '\0';
                if (ending < text.Length) {
                    ceh = text[ending];
                }
                if (spaces) {
                    if (ceh == 0 || ceh != 32) {
                        if (start+1 == ending && _column > _bestWidth && split && start != 0 && ending != text.Length) {
                            writeIndent();
                        } else {
                            data = text.Substring(start,ending-start);
                            _column += data.Length;
                            _stream.Write(data);
                        }
                        start = ending;
                    }
                } else if (breaks) {
                    if (ceh == 0 || !('\n' == ceh)) {
                        data = text.Substring(start,ending-start);
                        for(int i=0,j=data.Length;i<j;i++) {
                            char cha = data[i];
                            if ('\n' == cha) {
                                writeLineBreak();
                            } else {
                                writeLineBreak(cha);
                            }
                        }
                        writeIndent();
                        start = ending;
                    }
                } else {
                    if (ceh == 0 || !('\n' == ceh)) {
                        if (start < ending) {
                            data = text.Substring(start,ending-start);
                            _column += data.Length;
                            _stream.Write(data);
                            start = ending;
                        }
                    }
                }
                if (ceh == '\'') {
                    data = "''";
                    _column += 2;
                    _stream.Write(data);
                    start = ending + 1;
                }
                if (ceh != 0) {
                    spaces = ceh == ' ';
                    breaks = ceh == '\n';
                }
                ending++;
            }
            writeIndicator("'",false,false,false);
        }

        void writeFolded(string text) {
            string chomp = determineChomp(text);
            writeIndicator(">" + chomp, true, false, false);
            writeIndent();
            bool leadingSpace = false;
            bool spaces = false;
            bool breaks = false;
            int start=0,ending=0;

            string data;
            while (ending <= text.Length) {
                char ceh = '\0';
                if (ending < text.Length) {
                    ceh = text[ending];
                }
                if (breaks) {
                    if (ceh == 0 || !('\n' == ceh)) {
                        if (!leadingSpace && ceh != 0 && ceh != ' ' && text[start] == '\n') {
                            writeLineBreak();
                        }
                        leadingSpace = ceh == ' ';
                        data = text.Substring(start,ending-start);
                        for(int i=0,j=data.Length;i<j;i++) {
                            char cha = data[i];
                            if ('\n' == cha) {
                                writeLineBreak();
                            } else {
                                writeLineBreak(cha);
                            }
                        }
                        if (ceh != 0) {
                            writeIndent();
                        }
                        start = ending;
                    }
                } else if (spaces) {
                    if (ceh != ' ') {
                        if (start+1 == ending && _column > _bestWidth) {
                            writeIndent();
                        } else {
                            data = text.Substring(start,ending-start);
                            _column += data.Length;
                            _stream.Write(data);
                        }
                        start = ending;
                    }
                } else {
                    if (ceh == 0 || ' ' == ceh || '\n' == ceh) {
                        data = text.Substring(start,ending-start);
                        _stream.Write(data);
                        if (ceh == 0) {
                            writeLineBreak();
                        } 
                        start = ending;
                    }
                }
                if (ceh != 0) {
                    breaks = '\n' == ceh;
                    spaces = ceh == ' ';
                }
                ending++;
            }
        }

        void writeLiteral(string text) {
            string chomp = determineChomp(text);
            writeIndicator("|" + chomp, true, false, false);
            writeIndent();
            bool breaks = false;
            int start=0,ending=0;
            string data;
            while(ending <= text.Length) {
                char ceh = '\0';
                if (ending < text.Length) {
                    ceh = text[ending];
                }
                if (breaks) {
                    if (ceh == 0 || !('\n' == ceh)) {
                        data = text.Substring(start,ending-start);
                        for(int i=0,j=data.Length;i<j;i++) {
                            char cha = data[i];
                            if ('\n' == cha) {
                                writeLineBreak();
                            } else {
                                writeLineBreak(cha);
                            }
                        }
                        if (ceh != 0) {
                            writeIndent();
                        }
                        start = ending;
                    }
                } else {
                    if (ceh == 0 || '\n' == ceh) {
                        data = text.Substring(start,ending-start);
                        _stream.Write(data);
                        if (ceh == 0) {
                            writeLineBreak();
                        }
                        start = ending;
                    }
                }
                if (ceh != 0) {
                    breaks = '\n' == ceh;
                }
                ending++;
            }
        }

        void writePlain(string text, bool split) {
            if (text == null || text.Length == 0) {
                return;
            }
            if (!_whitespace) {
                _column += 1;
                _stream.Write(' ');
            }
            _whitespace = false;
            _indentation = false;
            bool spaces=false, breaks = false;
            int start=0,ending=0;
            string data;
            while (ending <= text.Length) {
                char ceh = '\0';
                if (ending < text.Length) {
                    ceh = text[ending];
                }
                if (spaces) {
                    if (ceh != ' ') {
                        if (start+1 == ending && _column > _bestWidth && split) {
                            writeIndent();
                            _whitespace = false;
                            _indentation = false;
                        } else {
                            data = text.Substring(start, ending-start);
                            _column += data.Length;
                            _stream.Write(data);
                        }
                        start = ending;
                    }
                } else if (breaks) {
                    if (ceh != '\n') {
                        if (text[start] == '\n') {
                            writeLineBreak();
                        }
                        data = text.Substring(start, ending-start);
                        for(int i=0,j=data.Length;i<j;i++) {
                            char cha = data[i];
                            if ('\n' == cha) {
                                writeLineBreak();
                            } else {
                                writeLineBreak(cha);
                            }
                        }
                        writeIndent();
                        _whitespace = false;
                        _indentation = false;
                        start = ending;
                    }
                } else {
                    if (ceh == 0 || ' ' == ceh || '\n' == ceh) {
                        data = text.Substring(start, ending-start);
                        _column += data.Length;
                        _stream.Write(data);
                        start = ending;
                    }
                }
                if (ceh != 0) {
                    spaces = ceh == ' ';
                    breaks = ceh == '\n';
                }
                ending++;
            }
        }

        void writeLineBreak() {
            writeLineBreak(_bestLinebreak);
        }

        void writeLineBreak(char data) {
            _whitespace = true;
            _indentation = true;
            _line++;
            _column = 0;
            _stream.Write(data);
        }

        static string prepareVersion(Version version) {
            if (version.Major != 1) {
                throw new EmitterException("unsupported Yaml version (must be 1.*): " + version);
            }
            return " " + version.Major + "." + version.Minor;
        }
        private static Regex HANDLE_FORMAT = new Regex("^![-\\w]*!$", RegexOptions.Compiled);
        static string prepareTagHandle(string handle) {
            if (string.IsNullOrEmpty(handle)) {
                throw new EmitterException("tag handle must not be empty");
            } else if (handle[0] != '!' || handle[handle.Length-1] != '!') {
                throw new EmitterException("tag handle must start and end with '!': " + handle);
            } else if ("!" != handle && !HANDLE_FORMAT.IsMatch(handle)) {
                throw new EmitterException("invalid syntax for tag handle: " + handle);
            }
            return handle;
        }

        static string prepareTagPrefix(string prefix) {
            if (string.IsNullOrEmpty(prefix)) {
                throw new EmitterException("tag prefix must not be empty");
            }
            StringBuilder chunks = new StringBuilder();
            int start=0,ending=0;
            if (prefix[0] == '!') {
                ending = 1;
            }
            while(ending < prefix.Length) {
                ending++;
            }
            if (start < ending) {
                chunks.Append(prefix.Substring(start,ending-start));
            }
            return chunks.ToString();
        }

        private static Regex ANCHOR_FORMAT = new Regex("^[-\\w]*$");
        static string prepareAnchor(string anchor) {
            if (string.IsNullOrEmpty(anchor)) {
                throw new EmitterException("anchor must not be empty");
            }
            if (!ANCHOR_FORMAT.IsMatch(anchor)) {
                throw new EmitterException("invalid syntax for anchor: " + anchor);
            }
            return anchor;
        }

        string prepareTag(string tag) {
            if (string.IsNullOrEmpty(tag)) {
                throw new EmitterException("tag must not be empty");
            }
            if (tag == "!") {
                return tag;
            }
            string handle = null;
            string suffix = tag;
            foreach (string prefix in _tagPrefixes.Keys) {
                if ((prefix == "!" || prefix.Length < tag.Length) && Regex.IsMatch(tag, "^" + prefix + ".+$")) {
                    handle = _tagPrefixes[prefix];
                    suffix = tag.Substring(prefix.Length);
                }
            }
            if (handle == null) {
                if (tag.StartsWith("tag:") && tag.IndexOf(':', 4) != -1) {
                    int doti = tag.IndexOf('.',4);
                    string first = tag.Substring(4,doti-4);
                    string rest = tag.Substring(tag.IndexOf(':', 4)+1);
                    handle = "!" + first + "/";
                    suffix = rest;
                }
            }

            StringBuilder chunks = new StringBuilder();
            int start=0,ending=0;
            while(ending < suffix.Length) {
                ending++;
            }
            if (start < ending) {
                chunks.Append(suffix.Substring(start,ending-start));
            }
            string suffixText = chunks.ToString();
            if (tag[0] == '!' && _isVersion10) {
                return tag;
            }
            if (handle != null) {
                return handle + suffixText;
            } else {
                return "!<" + suffixText + ">";
            }
        }

        private static Regex DOC_INDIC = new Regex("^(---|\\.\\.\\.)", RegexOptions.Compiled);
        private static Regex FIRST_SPACE = new Regex("(^|\n) ", RegexOptions.Compiled);
        private static string NULL_BL_T_LINEBR = "\0 \t\r\n";
        private static string SPECIAL_INDIC = "#,[]{}#&*!|>'\"%@`";
        private static string FLOW_INDIC = ",?[]{}";

        static ScalarAnalysis analyzeScalar(string scalar) {
            if (scalar == null || scalar.Length == 0) {
                return new ScalarAnalysis(scalar,true,false,false,true,true,true,false,false);
            }

            bool blockIndicators = false;
            bool flowIndicators = false;
            bool lineBreaks = false;
            bool specialCharacters = false;

            // Whitespaces.
            bool inlineBreaks = false;          // non-space break+ non-space
            bool leadingSpaces = false;         // ^ space+ (non-space | $)
            bool leadingBreaks = false;         // ^ break+ (non-space | $)
            bool trailingSpaces = false;        // (^ | non-space) space+ $
            bool trailingBreaks = false;        // (^ | non-space) break+ $
            bool inlineBreaksSpaces = false;   // non-space break+ space+ non-space
            bool mixedBreaksSpaces = false;    // anything else
            
            if (DOC_INDIC.IsMatch(scalar)) {
                blockIndicators = true;
                flowIndicators = true;
            }

            bool preceededBySpace = true;
            bool followedBySpace = scalar.Length == 1 || NULL_BL_T_LINEBR.IndexOf(scalar[1]) != -1;

            bool spaces = false;
            bool breaks = false;
            bool mixed = false;
            bool leading = false;
            
            int index = 0;

            while(index < scalar.Length) {
                char ceh = scalar[index];
                if (index == 0) {
                    if (SPECIAL_INDIC.IndexOf(ceh) != -1) {
                        flowIndicators = true;
                        blockIndicators = true;
                    }
                    if (ceh == '?' || ceh == ':') {
                        flowIndicators = true;
                        if (followedBySpace) {
                            blockIndicators = true;
                        }
                    }
                    if (ceh == '-' && followedBySpace) {
                        flowIndicators = true;
                        blockIndicators = true;
                    }
                } else {
                    if (FLOW_INDIC.IndexOf(ceh) != -1) {
                        flowIndicators = true;
                    }
                    if (ceh == ':') {
                        flowIndicators = true;
                        if (followedBySpace) {
                            blockIndicators = true;
                        }
                    }
                    if (ceh == '#' && preceededBySpace) {
                        flowIndicators = true;
                        blockIndicators = true;
                    }
                }
                if (ceh == '\n') {
                    lineBreaks = true;
                }
                if (!(ceh == '\n' || ('\u0020' <= ceh && ceh <= '\u007E'))) {
                    specialCharacters = true;

                }
                if (' ' == ceh || '\n' == ceh) {
                    if (spaces && breaks) {
                        if (ceh != ' ') {
                            mixed = true;
                        }
                    } else if (spaces) {
                        if (ceh != ' ') {
                            breaks = true;
                            mixed = true;
                        }
                    } else if (breaks) {
                        if (ceh == ' ') {
                            spaces = true;
                        }
                    } else {
                        leading = (index == 0);
                        if (ceh == ' ') {
                            spaces = true;
                        } else {
                            breaks = true;
                        }
                    }
                } else if (spaces || breaks) {
                    if (leading) {
                        if (spaces && breaks) {
                            mixedBreaksSpaces = true;
                        } else if (spaces) {
                            leadingSpaces = true;
                        } else if (breaks) {
                            leadingBreaks = true;
                        }
                    } else {
                        if (mixed) {
                            mixedBreaksSpaces = true;
                        } else if (spaces && breaks) {
                            inlineBreaksSpaces = true;
                        } else if (breaks) {
                            inlineBreaks = true;
                        }
                    }
                    spaces = breaks = mixed = leading = false;
                }

                if ((spaces || breaks) && (index == scalar.Length-1)) {
                    if (spaces && breaks) {
                        mixedBreaksSpaces = true;
                    } else if (spaces) {
                        trailingSpaces = true;
                        if (leading) {
                            leadingSpaces = true;
                        }
                    } else if (breaks) {
                        trailingBreaks = true;
                        if (leading) {
                            leadingBreaks = true;
                        }
                    }
                    spaces = breaks = mixed = leading = false;
                }
                index++;
                preceededBySpace = NULL_BL_T_LINEBR.IndexOf(ceh) != -1;
                followedBySpace = index+1 >= scalar.Length || NULL_BL_T_LINEBR.IndexOf(scalar[index+1]) != -1;
            }
            bool allowFlowPlain = true;
            bool allowBlockPlain = true;
            bool allowSingleQuoted = true;
            bool allowDoubleQuoted = true;
            bool allowBlock = true;
            
            if (leadingSpaces || leadingBreaks || trailingSpaces) {
                allowFlowPlain = allowBlockPlain = allowBlock = allowSingleQuoted = false;
            }

            if (trailingBreaks) {
                allowFlowPlain = allowBlockPlain = false;
            }

            if (inlineBreaksSpaces) {
                allowFlowPlain = allowBlockPlain = allowSingleQuoted = false;
            }

            if (mixedBreaksSpaces || specialCharacters) {
                allowFlowPlain = allowBlockPlain = allowSingleQuoted = allowBlock = false;
            }

            if (inlineBreaks) {
                allowFlowPlain = allowBlockPlain = allowSingleQuoted = false;
            }
            
            if (trailingBreaks) {
                allowSingleQuoted = false;
            }

            if (lineBreaks) {
                allowFlowPlain = allowBlockPlain = false;
            }

            if (flowIndicators) {
                allowFlowPlain = false;
            }
            
            if (blockIndicators) {
                allowBlockPlain = false;
            }

            return new ScalarAnalysis(scalar,false,lineBreaks,allowFlowPlain,allowBlockPlain,allowSingleQuoted,allowDoubleQuoted,allowBlock,specialCharacters);
        }

        static string determineChomp(string text) {
            char ceh = ' ';
            char ceh2 = ' ';
            if (text.Length > 0) {
                ceh = text[text.Length-1];
                if (text.Length > 1) {
                    ceh2 = text[text.Length-2];
                }
            }
            return (ceh == '\n') ? ((ceh2 == '\n') ? "+" : "") : "-";
        }

        #region helper methods

        private bool needMoreEvents() {
            if (_events.Count == 0) {
                return true;
            }
            _event = _events.Peek();
            if (_event is DocumentStartEvent) {
                return needEvents(1);
            } else if (_event is SequenceStartEvent) {
                return needEvents(2);
            } else if (_event is MappingStartEvent) {
                return needEvents(3);
            } else {
                return false;
            }
        }

        private bool needEvents(int count) {
            int level = 0;
            foreach (YamlEvent e in _events) {
                if (e is DocumentStartEvent || e is CollectionStartEvent) {
                    level++;
                } else if (e is DocumentEndEvent || e is CollectionEndEvent) {
                    level--;
                } else if (e is StreamEndEvent) {
                    level = -1;
                }
                if (level < 0) {
                    return false;
                }
            }
            return _events.Count < count + 1;
        }

        private void increaseIndent(bool flow, bool indentless) {
            _indents.Push(_indent);
            if (_indent == -1) {
                if (flow) {
                    _indent = _bestIndent;
                } else {
                    _indent = 0;
                }
            } else if (!indentless) {
                _indent += _bestIndent;
            }
        }

        private void expectStreamStart() {
            if (_event is StreamStartEvent) {
                writeStreamStart();
                _state = EmitterState.FIRST_DOCUMENT_START;
            } else {
                throw new EmitterException("expected StreamStartEvent, but got " + _event);
            }
        }

        private void expectNothing() {
            throw new EmitterException("expecting nothing, but got " + _event);
        }

        private void expectDocumentStart(bool first) {
            if (_event is DocumentStartEvent) {
                DocumentStartEvent ev = (DocumentStartEvent)_event;
                if (first) {
                    if (null != ev.Version) {
                        writeVersionDirective(prepareVersion(ev.Version));
                    }

                    if ((null != ev.Version && ev.Version.Equals(new Version(1, 0))) || _options.Version.Equals(new Version(1, 0))) {
                        _isVersion10 = true;
                        _tagPrefixes.Clear();
                        _tagPrefixes.Add("tag:yaml.org,2002:", "!");
                    } else {
                        _tagPrefixes.Clear();
                        _tagPrefixes.Add("!", "!");
                        _tagPrefixes.Add("tag:yaml.org,2002:", "!!");
                    }

                    if (null != ev.Tags) {
                        foreach(KeyValuePair<string, string> tags in new SortedList<string, string>(ev.Tags)) {
                            string handle = tags.Key;
                            string prefix = tags.Value;
                            _tagPrefixes.Add(prefix, handle);
                            string handleText = prepareTagHandle(handle);
                            string prefixText = prepareTagPrefix(prefix);
                            writeTagDirective(handleText, prefixText);
                        }
                    }
                }

                bool @implicit = first && !ev.Explicit && !canonical && ev.Version == null && ev.Tags == null && !checkEmptyDocument();
                if (!@implicit) {
                    if (!first) {
                        writeIndent();
                    }
                    writeIndicator("--- ", !first, true, false);
                    if (canonical) {
                        writeIndent();
                    }
                }
                _state = EmitterState.DOCUMENT_ROOT;
            } else if (_event is StreamEndEvent) {
                writeStreamEnd();
                _state = EmitterState.NOTHING;
            } else {
                throw new EmitterException("expected DocumentStartEvent, but got " + _event);
            }
        }

        private void expectDocumentRoot() {
            _states.Push(EmitterState.DOCUMENT_END);
            expectNode(true, false, false, false);
        }

        private void expectDocumentEnd() {
            if (_event is DocumentEndEvent) {
                writeIndent();
                if (((DocumentEndEvent)_event).Explicit) {
                    writeIndicator("...", true, false, false);
                    writeIndent();
                }
                _stream.Flush();
                _state = EmitterState.DOCUMENT_START;
            } else {
                throw new EmitterException("expected DocumentEndEvent, but got " + _event);
            }
        }

        private void expectFlowSequenceItem(bool first) {
            if (_event is SequenceEndEvent) {
                _indent = _indents.Pop();
                _flowLevel--;
                if (canonical && !first) {
                    writeIndicator(",", false, false, false);
                    writeIndent();
                }
                writeIndicator("]", false, false, false);
                _state = _states.Pop();
            } else {
                if (!first) {
                    writeIndicator(",", false, false, false);
                }
                if (canonical || _column > _bestWidth) {
                    writeIndent();
                }
                _states.Push(EmitterState.FLOW_SEQUENCE_ITEM);
                expectNode(false, true, false, false);
            }
        }

        private void expectFlowMappingSimpleValue() {
            writeIndicator(": ", false, true, false);
            _states.Push(EmitterState.FLOW_MAPPING_KEY);
            expectNode(false, false, true, false);
        }

        private void expectFlowMappingValue() {
            if (canonical || _column > _bestWidth) {
                writeIndent();
            }
            writeIndicator(": ", false, true, false);
            _states.Push(EmitterState.FLOW_MAPPING_KEY);
            expectNode(false, false, true, false);
        }

        private void expectFlowMappingKey(bool first) {
            if (_event is MappingEndEvent) {
                _indent = _indents.Pop();
                _flowLevel--;
                if (canonical && !first) {
                    writeIndicator(",", false, false, false);
                    writeIndent();
                }
                writeIndicator("}", false, false, false);
                _state = _states.Pop();
            } else {
                if (!first) {
                    writeIndicator(",", false, false, false);
                }
                if (canonical || _column > _bestWidth) {
                    writeIndent();
                }
                if (!canonical && checkSimpleKey()) {
                    _states.Push(EmitterState.FLOW_MAPPING_SIMPLE_VALUE);
                    expectNode(false, false, true, true);
                } else {
                    writeIndicator("?", true, false, false);
                    _states.Push(EmitterState.FLOW_MAPPING_VALUE);
                    expectNode(false, false, true, false);
                }
            }
        }

        private void expectBlockSequenceItem(bool first) {
            if (!first && _event is SequenceEndEvent) {
                _indent = _indents.Pop();
                _state = _states.Pop();
            } else {
                writeIndent();
                writeIndicator("-", true, false, true);
                _states.Push(EmitterState.BLOCK_SEQUENCE_ITEM);
                expectNode(false, true, false, false);
            }
        }

        private void expectBlockMappingSimpleValue() {
            writeIndicator(": ", false, true, false);
            _states.Push(EmitterState.BLOCK_MAPPING_KEY);
            expectNode(false, false, true, false);
        }

        private void expectBlockMappingValue() {
            writeIndent();
            writeIndicator(": ", true, true, true);
            _states.Push(EmitterState.BLOCK_MAPPING_KEY);
            expectNode(false, false, true, false);
        }

        private void expectBlockMappingKey(bool first) {
            if (!first && _event is MappingEndEvent) {
                _indent = _indents.Pop();
                _state = _states.Pop();
            } else {
                writeIndent();
                if (checkSimpleKey()) {
                    _states.Push(EmitterState.BLOCK_MAPPING_SIMPLE_VALUE);
                    expectNode(false, false, true, true);
                } else {
                    writeIndicator("?", true, false, true);
                    _states.Push(EmitterState.BLOCK_MAPPING_VALUE);
                    expectNode(false, false, true, false);
                }
            }
        }

        private void expectNode(bool root, bool sequence, bool mapping, bool simpleKey) {
            _rootContext = root;
            _sequenceContext = sequence;
            _mappingContext = mapping;
            _simpleKeyContext = simpleKey;
            if (_event is AliasEvent) {
                expectAlias();
            } else if (_event is ScalarEvent || _event is CollectionStartEvent) {
                processAnchor("&");
                processTag();
                if (_event is ScalarEvent) {
                    expectScalar();
                } else if (_event is SequenceStartEvent) {
                    if (_flowLevel != 0 || canonical || ((SequenceStartEvent)_event).FlowStyle || checkEmptySequence()) {
                        expectFlowSequence();
                    } else {
                        expectBlockSequence();
                    }
                } else if (_event is MappingStartEvent) {
                    if (_flowLevel != 0 || canonical || ((MappingStartEvent)_event).FlowStyle || checkEmptyMapping()) {
                        expectFlowMapping();
                    } else {
                        expectBlockMapping();
                    }
                }
            } else {
                throw new EmitterException("expected NodeEvent, but got " + _event);
            }
        }

        private void expectAlias() {
            if (((NodeEvent)_event).Anchor == null) {
                throw new EmitterException("anchor is not specified for alias");
            }
            processAnchor("*");
            _state = _states.Pop();
        }

        private void expectScalar() {
            increaseIndent(true, false);
            processScalar();
            _indent = _indents.Pop();
            _state = _states.Pop();
        }

        private void expectFlowSequence() {
            writeIndicator("[", true, true, false);
            _flowLevel++;
            increaseIndent(true, false);
            _state = EmitterState.FIRST_FLOW_SEQUENCE_ITEM;
        }

        private void expectBlockSequence() {
            increaseIndent(false, _mappingContext && !_indentation);
            _state = EmitterState.FIRST_BLOCK_SEQUENCE_ITEM;
        }

        private void expectFlowMapping() {
            writeIndicator("{", true, true, false);
            _flowLevel++;
            increaseIndent(true, false);
            _state = EmitterState.FIRST_FLOW_MAPPING_KEY;
        }

        private void expectBlockMapping() {
            increaseIndent(false, false);
            _state = EmitterState.FIRST_BLOCK_MAPPING_KEY;
        }

        private bool checkEmptySequence() {
            return _event is SequenceStartEvent && _events.Count != 0 && _events.Peek() is SequenceEndEvent;
        }

        private bool checkEmptyMapping() {
            return _event is MappingStartEvent && _events.Count != 0 && _events.Peek() is MappingEndEvent;
        }

        private bool checkEmptyDocument() {
            if (!(_event is DocumentStartEvent) || _events.Count == 0) {
                return false;
            }
            ScalarEvent ev = _events.Peek() as ScalarEvent;
            return ev != null && ev.Anchor == null && ev.Tag == null && ev.Implicit != null && ev.Value.Length == 0;
        }

        private bool checkSimpleKey() {
            int length = 0;
            if (_event is NodeEvent && null != ((NodeEvent)_event).Anchor) {
                if (null == _preparedAnchor) {
                    _preparedAnchor = prepareAnchor(((NodeEvent)_event).Anchor);
                }
                length += _preparedAnchor.Length;
            }
            string tag = null;
            if (_event is ScalarEvent) {
                tag = ((ScalarEvent)_event).Tag;
            } else if (_event is CollectionStartEvent) {
                tag = ((CollectionStartEvent)_event).Tag;
            }
            if (tag != null) {
                if (null == _preparedTag) {
                    _preparedTag = prepareTag(tag);
                }
                length += _preparedTag.Length;
            }
            if (_event is ScalarEvent) {
                if (null == _analysis) {
                    _analysis = analyzeScalar(((ScalarEvent)_event).Value);
                    length += _analysis.Scalar.Length;
                }
            }

            return (length < 128 && (_event is AliasEvent || (_event is ScalarEvent && !_analysis.Multiline) || checkEmptySequence() || checkEmptyMapping()));
        }

        private void processAnchor(string indicator) {
            NodeEvent ev = (NodeEvent)_event;
            if (null == ev.Anchor) {
                _preparedAnchor = null;
                return;
            }
            if (null == _preparedAnchor) {
                _preparedAnchor = prepareAnchor(ev.Anchor);
            }
            if (!string.IsNullOrEmpty(_preparedAnchor)) {
                indicator += _preparedAnchor;
                if (ev is CollectionStartEvent) {
                    _indentation = true;
                }
                writeIndicator(indicator, true, false, true);
            }
            _preparedAnchor = null;
        }

        private void processTag() {
            string tag = null;
            if (_event is ScalarEvent) {
                ScalarEvent ev = (ScalarEvent)_event;
                tag = ev.Tag;
                if (_style == 0) {
                    _style = chooseScalarStyle();
                }
                if (((!canonical || tag == null) && ((0 == _style && ev.Implicit[0]) || (0 != _style && ev.Implicit[1])))) {
                    _preparedTag = null;
                    return;
                }
                if (ev.Implicit[0] && null == tag) {
                    tag = "!";
                    _preparedTag = null;
                }
            } else {
                CollectionStartEvent ev = (CollectionStartEvent)_event;
                tag = ev.Tag;
                if ((!canonical || tag == null) && ev.Implicit) {
                    _preparedTag = null;
                    return;
                }
                _indentation = true;
            }
            if (tag == null) {
                throw new EmitterException("tag is not specified");
            }
            if (null == _preparedTag) {
                _preparedTag = prepareTag(tag);
            }
            if (!string.IsNullOrEmpty(_preparedTag)) {
                writeIndicator(_preparedTag, true, false, true);
            }
            _preparedTag = null;
        }

        private char chooseScalarStyle() {
            ScalarEvent ev = (ScalarEvent)_event;

            if (null == _analysis) {
                _analysis = analyzeScalar(ev.Value);
            }

            if (ev.Style == '"' || canonical || (_analysis.Empty && ev.Tag == "tag:yaml.org,2002:str")) {
                return '"';
            }

            //            if (ev.Style == 0 && ev.Implicit[0]) {
            if (ev.Style == 0) {
                if (!(_simpleKeyContext && (_analysis.Empty || _analysis.Multiline)) && ((_flowLevel != 0 && _analysis.AllowFlowPlain) || (_flowLevel == 0 && _analysis.AllowBlockPlain))) {
                    return '\0';
                }
            }
            if (ev.Style == 0 && ev.Implicit[0] && (!(_simpleKeyContext && (_analysis.Empty || _analysis.Multiline)) && (_flowLevel != 0 && _analysis.AllowFlowPlain || (_flowLevel == 0 && _analysis.AllowBlockPlain)))) {
                return '\0';
            }
            if ((ev.Style == '|' || ev.Style == '>') && _flowLevel == 0 && _analysis.AllowBlock) {
                return '\'';
            }
            if ((ev.Style == 0 || ev.Style == '\'') && (_analysis.AllowSingleQuoted && !(_simpleKeyContext && _analysis.Multiline))) {
                return '\'';
            }
            if (_analysis.Multiline && (FIRST_SPACE.Matches(ev.Value).Count == 0) && !_analysis.SpecialCharacters) {
                return '|';
            }

            return '"';
        }

        private void processScalar() {
            ScalarEvent ev = (ScalarEvent)_event;

            if (null == _analysis) {
                _analysis = analyzeScalar(ev.Value);
            }
            if (0 == _style) {
                _style = chooseScalarStyle();
            }
            bool split = !_simpleKeyContext;
            if (_style == '"') {
                writeDoubleQuoted(_analysis.Scalar, split);
            } else if (_style == '\'') {
                writeSingleQuoted(_analysis.Scalar, split);
            } else if (_style == '>') {
                writeFolded(_analysis.Scalar);
            } else if (_style == '|') {
                writeLiteral(_analysis.Scalar);
            } else {
                writePlain(_analysis.Scalar, split);
            }
            _analysis = null;
            _style = '\0';
        }

        private string ESCAPE_REPLACEMENTS(char c) {
            switch (c) {
                case '\0':
                    return "0";
                case '\u0007':
                    return "a";
                case '\u0008':
                    return "b";
                case '\u0009':
                    return "t";
                case '\n':
                    return "n";
                case '\u000B':
                    return "v";
                case '\u000C':
                    return "f";
                case '\r':
                    return "r";
                case '\u001B':
                    return "e";
                case '"':
                    return "\"";
                case '\\':
                    return "\\";
                case '\u00A0':
                    return "_";
                default:
                    return null;
            }
        }

        #endregion
    }
}