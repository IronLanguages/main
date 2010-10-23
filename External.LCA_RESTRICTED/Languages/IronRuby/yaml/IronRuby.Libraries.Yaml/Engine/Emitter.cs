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
using Microsoft.Scripting.Utils;

namespace IronRuby.StandardLibrary.Yaml {
    public sealed class Emitter {
        private readonly TextWriter/*!*/ _writer;
        private readonly YamlOptions/*!*/ _options;

        private readonly Stack<EmitterState> _states = new Stack<EmitterState>();
        private EmitterState _state = EmitterState.STREAM_START;
        private readonly Queue<YamlEvent> _events = new Queue<YamlEvent>();
        private YamlEvent _event;
        private int _flowLevel;
        private readonly Stack<int> _indents = new Stack<int>();
        private int _indent = -1;
        private bool _mappingContext;
        
        private int _line;
        private int _column;
        private bool _whitespace;
        private bool _indentation;

        private readonly bool _canonical;
        private readonly int _bestIndent = 2;
        private readonly int _bestWidth = 80;

        private readonly Dictionary<string, string> _tagPrefixes = new Dictionary<string, string>();

        private string _preparedAnchor;
        private string _preparedTag;

        private bool _isVersion10;

        public Emitter(TextWriter/*!*/ writer, YamlOptions/*!*/ opts) {
            ContractUtils.RequiresNotNull(writer, "writer");
            ContractUtils.RequiresNotNull(opts, "opts");

            _writer = writer;
            _options = opts;
            _canonical = _options.Canonical;
            int propIndent = _options.Indent;
            if (propIndent>=2 && propIndent<10) {
                _bestIndent = propIndent;
            }
            int propWidth = _options.BestWidth;
            if (propWidth != 0 && propWidth > (_bestIndent*2)) {
                _bestWidth = propWidth;
            }
        }

        public TextWriter/*!*/ Writer {
            get { return _writer; }
        }

        public int Level {
            get { return _flowLevel; }
        }

        public void Emit(YamlEvent/*!*/ @event) {
            _events.Enqueue(@event);
            while (!needMoreEvents()) {
                _event = _events.Dequeue();
                switch (_state) {
                    case EmitterState.STREAM_START:
                        EmitStreamStart(); 
                        break;
                    case EmitterState.FIRST_DOCUMENT_START: 
                        EmitDocumentStart(true); 
                        break;
                    case EmitterState.DOCUMENT_ROOT: 
                        EmitDocumentRoot(); 
                        break;
                    case EmitterState.NOTHING: 
                        EmitNothing(); 
                        break;
                    case EmitterState.DOCUMENT_START: 
                        EmitDocumentStart(false); 
                        break;
                    case EmitterState.DOCUMENT_END: 
                        EmitDocumentEnd(); 
                        break;

                    case EmitterState.FIRST_FLOW_SEQUENCE_ITEM: 
                        EmitFlowSequenceItem(true);
                        break;

                    case EmitterState.FLOW_SEQUENCE_ITEM: 
                        EmitFlowSequenceItem(false);
                        break;

                    case EmitterState.FIRST_FLOW_MAPPING_KEY: 
                        EmitFlowMappingKey(true); 
                        break;

                    case EmitterState.FLOW_MAPPING_SIMPLE_VALUE: 
                        EmitFlowMappingSimpleValue();
                        break;

                    case EmitterState.FLOW_MAPPING_VALUE: 
                        EmitFlowMappingValue();
                        break;

                    case EmitterState.FLOW_MAPPING_KEY: 
                        EmitFlowMappingKey(false);
                        break;

                    case EmitterState.BLOCK_SEQUENCE_ITEM:
                        if (_event is SequenceEndEvent) {
                            _indent = _indents.Pop();
                            _state = _states.Pop();
                        } else {
                            EmitBlockSequenceItem(); 
                        }
                        break;

                    case EmitterState.FIRST_BLOCK_SEQUENCE_ITEM:
                        EmitBlockSequenceItem();
                        break;

                    case EmitterState.BLOCK_MAPPING_KEY:
                        if (_event is MappingEndEvent) {
                            _indent = _indents.Pop();
                            _state = _states.Pop();
                        } else {
                            EmitBlockMappingKey();
                        }
                        break;

                    case EmitterState.FIRST_BLOCK_MAPPING_KEY:
                        EmitBlockMappingKey();
                        break;

                    case EmitterState.BLOCK_MAPPING_SIMPLE_VALUE: 
                        EmitBlockMappingSimpleValue();
                        break;

                    case EmitterState.BLOCK_MAPPING_VALUE: 
                        EmitBlockMappingValue();
                        break;

                    default:
                        Debug.Assert(false, "unreachable");
                        throw new InvalidOperationException("unreachable");
                }
                _event = null;
            }
        }

        private enum EmitterState {
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
        
        private void WriteStreamStart() {
        }

        private void WriteStreamEnd() {
            _writer.Flush();
        }

        private void WriteIndicator(string indicator, bool needWhitespace, bool whitespace, bool indentation) {
            if (!_whitespace && needWhitespace) {
                indicator = " " + indicator;
            }
            _whitespace = whitespace;
            _indentation = _indentation && indentation;
            _column += indicator.Length;
            _writer.Write(indicator);
        }

        private void WriteIndent() {
            int indent = 0;
            if (_indent != -1) {
                indent = _indent;
            }

            if (!_indentation || _column > indent || (_column == indent && !_whitespace)) {
                WriteLineBreak();
            }

            if (_column < indent) {
                _whitespace = true;
                string data = new string(' ', indent - _column);
                _column = indent;
                _writer.Write(data);
            }
        }

        private void WriteVersionDirective(string version_text) {
            _writer.Write("%Yaml " + version_text);
            WriteLineBreak();
        }

        private void WriteTagDirective(string handle, string prefix) {
            _writer.Write("%TAG " + handle + " " + prefix);
            WriteLineBreak();
        }

        private void WriteDoubleQuoted(string text, bool split) {
            WriteIndicator("\"", true, false, false);
            int start = 0;
            int ending = 0;

            string data = null;
            while (ending <= text.Length) {
                char ch = (char)0;
                if (ending < text.Length) {
                    ch = text[ending];
                }
                if (ch == 0 || "\"\\".IndexOf(ch) != -1 || !('\u0020' <= ch && ch <= '\u007E')) {
                    if (start < ending) {
                        data = text.Substring(start, ending - start);
                        _column += data.Length;
                        _writer.Write(data);
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
                        _writer.Write(data);
                        start = ending + 1;
                    }
                }

                if ((0 < ending && ending < (text.Length - 1)) && (ch == ' ' || start >= ending) && (_column + (ending - start)) > _bestWidth && split) {
                    if (start < ending) {
                        data = text.Substring(start, ending - start) + " \\";
                        start = ending + 1;
                    } else {
                        data = "\\";
                    }

                    _column += data.Length;
                    _writer.Write(data);
                    WriteIndent();
                    _whitespace = false;
                    _indentation = false;

                    if (start < (text.Length + 1) && text[start] == ' ') {
                        _writer.Write('\\');
                    }
                }
                ending += 1;
            }

            WriteIndicator("\"", false, false, false);
        }

        private void WriteSingleQuoted(string text, bool split) {
            WriteIndicator("'", true, false, false);
            bool spaces = false;
            bool breaks = false;
            int start = 0, ending = 0;
            char c = '\0';

            string data;
            while (ending <= text.Length) {
                c = '\0';
                if (ending < text.Length) {
                    c = text[ending];
                }
                if (spaces) {
                    if (c == 0 || c != 32) {
                        if (start + 1 == ending && _column > _bestWidth && split && start != 0 && ending != text.Length) {
                            WriteIndent();
                        } else {
                            data = text.Substring(start, ending - start);
                            _column += data.Length;
                            _writer.Write(data);
                        }
                        start = ending;
                    }
                } else if (breaks) {
                    if (c == 0 || c != '\n') {
                        // q sequence of eolns followed by non-eoln:
                        for (int i = start; i < ending; i++) {
                            WriteLineBreak();
                        }
                        WriteIndent();
                        start = ending;
                    }
                } else {
                    if (c == 0 || c != '\n') {
                        if (start < ending) {
                            data = text.Substring(start, ending - start);
                            _column += data.Length;
                            _writer.Write(data);
                            start = ending;
                        }
                    }
                }
                if (c == '\'') {
                    data = "''";
                    _column += 2;
                    _writer.Write(data);
                    start = ending + 1;
                }
                if (c != 0) {
                    spaces = c == ' ';
                    breaks = c == '\n';
                }
                ending++;
            }
            WriteIndicator("'", false, false, false);
        }

        private void WriteFolded(string/*!*/ text) {
            WriteIndicator(">" + DetermineChomp(text), true, false, false);
            WriteIndent();
            bool leadingSpace = false;
            bool spaces = false;
            bool breaks = false;
            int start = 0, ending = 0;

            string data;
            while (ending <= text.Length) {
                char ceh = '\0';
                if (ending < text.Length) {
                    ceh = text[ending];
                }
                if (breaks) {
                    if (ceh == 0 || !('\n' == ceh)) {
                        if (!leadingSpace && ceh != 0 && ceh != ' ' && text[start] == '\n') {
                            WriteLineBreak();
                        }
                        leadingSpace = ceh == ' ';

                        // q sequence of eolns followed by non-eoln:
                        for (int i = start; i < ending; i++) {
                            WriteLineBreak();
                        }

                        if (ceh != 0) {
                            WriteIndent();
                        }
                        start = ending;
                    }
                } else if (spaces) {
                    if (ceh != ' ') {
                        if (start + 1 == ending && _column > _bestWidth) {
                            WriteIndent();
                        } else {
                            data = text.Substring(start, ending - start);
                            _column += data.Length;
                            _writer.Write(data);
                        }
                        start = ending;
                    }
                } else {
                    if (ceh == 0 || ' ' == ceh || '\n' == ceh) {
                        data = text.Substring(start, ending - start);
                        _writer.Write(data);
                        if (ceh == 0) {
                            WriteLineBreak();
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

        private void WriteLiteral(string/*!*/ text, bool indent) {
            string chomp = DetermineChomp(text);
            WriteIndicator("|" + chomp, true, false, false);
            increaseIndent(false, false);
            WriteIndent();
            bool breaks = false;
            int start = 0, ending = 0;
            while (ending <= text.Length) {
                char c = '\0';
                if (ending < text.Length) {
                    c = text[ending];
                }
                if (breaks) {
                    if (c == 0 || c != '\n') {
                        // q sequence of eolns followed by non-eoln:
                        for (int i = start; i < ending; i++) {
                            WriteLineBreak();
                        }
                        if (c != 0) {
                            WriteIndent();
                        } else if (chomp.Length == 0) {
                            WriteLineBreak();
                        }
                        start = ending;
                    }
                } else if (c == 0 || c == '\n') {
                    // non-empty line:
                    _writer.Write(text.Substring(start, ending - start));
                    if (c == 0) {
                        WriteLineBreak();
                    }
                    start = ending;
                }

                if (c != 0) {
                    breaks = c == '\n';
                }
                ending++;
            }
            _indent = _indents.Pop();
        }

        private void WritePlain(string/*!*/ text, bool split) {
            if (String.IsNullOrEmpty(text)) {
                return;
            }

            if (!_whitespace) {
                _column += 1;
                _writer.Write(' ');
            }
            _whitespace = false;
            _indentation = false;
            bool spaces = false, breaks = false;
            int start = 0, ending = 0;
            string data;
            while (ending <= text.Length) {
                char c = '\0';
                if (ending < text.Length) {
                    c = text[ending];
                }
                if (spaces) {
                    if (c != ' ') {
                        if (start + 1 == ending && _column > _bestWidth && split) {
                            WriteIndent();
                            _whitespace = false;
                            _indentation = false;
                        } else {
                            data = text.Substring(start, ending - start);
                            _column += data.Length;
                            _writer.Write(data);
                        }
                        start = ending;
                    }
                } else if (breaks) {
                    if (c != '\n') {
                        if (text[start] == '\n') {
                            WriteLineBreak();
                        }

                        // q sequence of eolns followed by non-eoln:
                        for (int i = start; i < ending; i++) {
                            WriteLineBreak();
                        }

                        WriteIndent();
                        _whitespace = false;
                        _indentation = false;
                        start = ending;
                    }
                } else {
                    if (c == 0 || ' ' == c || '\n' == c) {
                        data = text.Substring(start, ending - start);
                        _column += data.Length;
                        _writer.Write(data);
                        start = ending;
                    }
                }
                if (c != 0) {
                    spaces = c == ' ';
                    breaks = c == '\n';
                }
                ending++;
            }
        }

        private void WriteLineBreak() {
            _whitespace = true;
            _indentation = true;
            _line++;
            _column = 0;
            _writer.Write('\n');
        }

        static string prepareVersion(Version version) {
            if (version.Major != 1) {
                throw new EmitterException("unsupported Yaml version (must be 1.*): " + version);
            }
            return " " + version.Major + "." + version.Minor;
        }

        private static Regex HANDLE_FORMAT = YamlUtils.CompiledRegex("^![-\\w]*!$");

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

        private string PrepareTag(string tag) {
            string handle = null;
            string suffix = tag;
            foreach (string prefix in _tagPrefixes.Keys) {
                if (prefix.Length < tag.Length && tag.StartsWith(prefix, StringComparison.Ordinal)) {
                    handle = _tagPrefixes[prefix];
                    suffix = tag.Substring(prefix.Length);
                }
            }

            // use short form if applicable ("tag:ruby.yaml.org,2002:foobar" -> "ruby/foobar")
            if (handle == null) {
                int colonIdx;
                if (tag.StartsWith("tag:", StringComparison.Ordinal) && (colonIdx = tag.IndexOf(':', 4)) != -1) {
                    string first = tag.Substring(4, tag.IndexOf('.', 4) - 4);
                    string rest = tag.Substring(colonIdx + 1);
                    handle = "!" + first + "/";
                    suffix = rest;
                }
            }

            // e.g. "!ruby/exception:IOError"
            if (tag.Length == 0 || tag[0] == '!' && _isVersion10) {
                return tag;
            }

            if (handle != null) {
                return handle + suffix;
            } else {
                return "!<" + suffix + ">";
            }
        }

        private static Regex DOC_INDIC = YamlUtils.CompiledRegex("^(---|\\.\\.\\.)");
        private static string NULL_BL_T_LINEBR = "\0 \t\r\n";
        private static string SPECIAL_INDIC = "#,[]{}#&*!|>'\"%@";
        private static string FLOW_INDIC = ",?[]{}";

        internal static ScalarProperties AnalyzeScalar(string scalar) {
            if (scalar == null || scalar.Length == 0) {
                return ScalarProperties.Empty | ScalarProperties.AllowBlockPlain
                    | ScalarProperties.AllowSingleQuoted | ScalarProperties.AllowDoubleQuoted;
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

                if (ceh != '\n' && (ceh < 0x20 || ceh > 0x7e)) {
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

            return
                (lineBreaks ? ScalarProperties.Multiline : 0) |
                (allowFlowPlain ? ScalarProperties.AllowFlowPlain : 0) |
                (allowBlockPlain ? ScalarProperties.AllowBlockPlain : 0) |
                (allowSingleQuoted ? ScalarProperties.AllowSingleQuoted : 0) |
                (allowDoubleQuoted ? ScalarProperties.AllowDoubleQuoted : 0) |
                (allowBlock ? ScalarProperties.AllowBlock : 0) |
                (specialCharacters ? ScalarProperties.SpecialCharacters : 0);
        }

        /// <summary>
        /// Chomping controls how final line breaks and trailing empty lines are interpreted.
        /// 1) Clip ("")
        ///    The final line break is preserved in the scalar’s content. Any trailing empty lines are excluded from the scalar’s content. 
        /// 2) Strip ("-")
        ///    The final line break and any trailing empty lines are excluded from the scalar’s content. 
        /// 3) Keep ("+")
        ///    The final line break and any trailing empty lines are considered to be part of the scalar’s content. 
        ///    These additional lines are not subject to folding. 
        /// </summary>
        private static string DetermineChomp(string/*!*/ text) {
            char last = ' ';
            char secondLast = ' ';
            if (text.Length > 0) {
                last = text[text.Length - 1];
                if (text.Length > 1) {
                    secondLast = text[text.Length - 2];
                }
            }
            
            // Ends with 
            //   0 eolns => use "-" to remove any trailing eolns
            //   1 eolns => use "" to preserve a single trailing eoln
            // >=2 eolns => use "+" to preserve all trailing eolns
            return (last != '\n') ? "-" : (secondLast != '\n') ? "" : "+";
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

        private void EmitStreamStart() {
            if (_event is StreamStartEvent) {
                WriteStreamStart();
                _state = EmitterState.FIRST_DOCUMENT_START;
            } else {
                throw new EmitterException("Emited StreamStartEvent, but got " + _event);
            }
        }

        private void EmitNothing() {
            throw new EmitterException("Emiting nothing, but got " + _event);
        }

        private void EmitDocumentStart(bool first) {
            if (_event is DocumentStartEvent) {
                DocumentStartEvent ev = (DocumentStartEvent)_event;
                if (first) {
                    if (null != ev.Version) {
                        WriteVersionDirective(prepareVersion(ev.Version));
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
                        var entries = new List<KeyValuePair<string, string>>(ev.Tags);
                        entries.Sort((x, y) => StringComparer.Ordinal.Compare(x.Key, y.Key));

                        foreach (KeyValuePair<string, string> tags in entries) {
                            string handle = tags.Key;
                            string prefix = tags.Value;
                            _tagPrefixes.Add(prefix, handle);
                            string handleText = prepareTagHandle(handle);
                            string prefixText = prepareTagPrefix(prefix);
                            WriteTagDirective(handleText, prefixText);
                        }
                    }
                }

                bool @implicit = first && !ev.Explicit && !_canonical && ev.Version == null && ev.Tags == null && !checkEmptyDocument();
                if (!@implicit) {
                    if (!first) {
                        WriteIndent();
                    }
                    WriteIndicator("--- ", !first, true, false);
                    if (_canonical) {
                        WriteIndent();
                    }
                }
                _state = EmitterState.DOCUMENT_ROOT;
            } else if (_event is StreamEndEvent) {
                WriteStreamEnd();
                _state = EmitterState.NOTHING;
            } else {
                throw new EmitterException("Emited DocumentStartEvent, but got " + _event);
            }
        }

        private void EmitDocumentRoot() {
            _states.Push(EmitterState.DOCUMENT_END);
            EmitNode(true, false, false, false);
        }

        private void EmitDocumentEnd() {
            if (_event is DocumentEndEvent) {
                WriteIndent();
                if (((DocumentEndEvent)_event).Explicit) {
                    WriteIndicator("...", true, false, false);
                    WriteIndent();
                }
                _writer.Flush();
                _state = EmitterState.DOCUMENT_START;
            } else {
                throw new EmitterException("Emited DocumentEndEvent, but got " + _event);
            }
        }

        private void EmitFlowSequenceItem(bool first) {
            if (_event is SequenceEndEvent) {
                _indent = _indents.Pop();
                _flowLevel--;
                if (_canonical && !first) {
                    WriteIndicator(",", false, false, false);
                    WriteIndent();
                }
                WriteIndicator("]", false, false, false);
                _state = _states.Pop();
            } else {
                if (!first) {
                    WriteIndicator(",", false, false, false);
                }
                if (_canonical || _column > _bestWidth) {
                    WriteIndent();
                }
                _states.Push(EmitterState.FLOW_SEQUENCE_ITEM);
                EmitNode(false, true, false, false);
            }
        }

        private void EmitFlowMappingSimpleValue() {
            WriteIndicator(": ", false, true, false);
            _states.Push(EmitterState.FLOW_MAPPING_KEY);
            EmitNode(false, false, true, false);
        }

        private void EmitFlowMappingValue() {
            if (_canonical || _column > _bestWidth) {
                WriteIndent();
            }
            WriteIndicator(": ", false, true, false);
            _states.Push(EmitterState.FLOW_MAPPING_KEY);
            EmitNode(false, false, true, false);
        }

        private void EmitFlowMappingKey(bool first) {
            if (_event is MappingEndEvent) {
                _indent = _indents.Pop();
                _flowLevel--;
                if (_canonical && !first) {
                    WriteIndicator(",", false, false, false);
                    WriteIndent();
                }
                WriteIndicator("}", false, false, false);
                _state = _states.Pop();
            } else {
                if (!first) {
                    WriteIndicator(",", false, false, false);
                }
                if (_canonical || _column > _bestWidth) {
                    WriteIndent();
                }
                if (!_canonical && CheckSimpleKey()) {
                    _states.Push(EmitterState.FLOW_MAPPING_SIMPLE_VALUE);
                    EmitNode(false, false, true, true);
                } else {
                    WriteIndicator("?", true, false, false);
                    _states.Push(EmitterState.FLOW_MAPPING_VALUE);
                    EmitNode(false, false, true, false);
                }
            }
        }

        private void EmitBlockSequenceItem() {
            WriteIndent();
            WriteIndicator("-", true, false, true);
            _states.Push(EmitterState.BLOCK_SEQUENCE_ITEM);
            EmitNode(false, true, false, false);            
        }

        private void EmitBlockMappingSimpleValue() {
            WriteIndicator(": ", false, true, false);
            _states.Push(EmitterState.BLOCK_MAPPING_KEY);
            EmitNode(false, false, true, false);
        }

        private void EmitBlockMappingValue() {
            WriteIndent();
            WriteIndicator(": ", true, true, true);
            _states.Push(EmitterState.BLOCK_MAPPING_KEY);
            EmitNode(false, false, true, false);
        }

        private void EmitBlockMappingKey() {
            WriteIndent();
            if (CheckSimpleKey()) {
                _states.Push(EmitterState.BLOCK_MAPPING_SIMPLE_VALUE);
                EmitNode(false, false, true, true);
            } else {
                WriteIndicator("?", true, false, true);
                _states.Push(EmitterState.BLOCK_MAPPING_VALUE);
                EmitNode(false, false, true, false);
            }
        }

        private void EmitNode(bool root, bool sequence, bool mapping, bool simpleKey) {
            _mappingContext = mapping;
            if (_event is AliasEvent) {
                EmitAlias();
            } else if (_event is ScalarEvent || _event is CollectionStartEvent) {
                ProcessAnchor("&");
                ProcessTag(simpleKey);
                if (_event is ScalarEvent) {
                    EmitScalar(simpleKey);
                } else if (_event is SequenceStartEvent) {
                    if (_flowLevel != 0 || _canonical || ((SequenceStartEvent)_event).FlowStyle == FlowStyle.Inline || checkEmptySequence()) {
                        EmitFlowSequence();
                    } else {
                        EmitBlockSequence();
                    }
                } else if (_event is MappingStartEvent) {
                    if (_flowLevel != 0 || _canonical || ((MappingStartEvent)_event).FlowStyle == FlowStyle.Inline || checkEmptyMapping()) {
                        EmitFlowMapping();
                    } else {
                        EmitBlockMapping();
                    }
                }
            } else {
                throw new EmitterException("Emited NodeEvent, but got " + _event);
            }
        }

        private void EmitAlias() {
            if (((NodeEvent)_event).Anchor == null) {
                throw new EmitterException("anchor is not specified for alias");
            }
            ProcessAnchor("*");
            _state = _states.Pop();
        }

        private void EmitScalar(bool simpleKey) {
            ProcessScalar(simpleKey);
            _state = _states.Pop();
        }

        private void EmitFlowSequence() {
            WriteIndicator("[", true, true, false);
            _flowLevel++;
            increaseIndent(true, false);
            _state = EmitterState.FIRST_FLOW_SEQUENCE_ITEM;
        }

        private void EmitBlockSequence() {
            increaseIndent(false, _mappingContext && !_indentation);
            _state = EmitterState.FIRST_BLOCK_SEQUENCE_ITEM;
        }

        private void EmitFlowMapping() {
            WriteIndicator("{", true, true, false);
            _flowLevel++;
            increaseIndent(true, false);
            _state = EmitterState.FIRST_FLOW_MAPPING_KEY;
        }

        private void EmitBlockMapping() {
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
            return ev != null && ev.Anchor == null && ev.Tag == null && ev.Value.Length == 0;
        }

        private bool CheckSimpleKey() {
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
                if (_preparedTag == null) {
                    _preparedTag = PrepareTag(tag);
                }
                length += _preparedTag.Length;
            }

            if (_event is ScalarEvent) {
               length += ((ScalarEvent)_event).Value.Length;
            }

            return (length < 128 && (_event is AliasEvent || (_event is ScalarEvent && !((ScalarEvent)_event).IsMultiline) || checkEmptySequence() || checkEmptyMapping()));
        }

        private void ProcessAnchor(string indicator) {
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
                WriteIndicator(indicator, true, false, true);
            }
            _preparedAnchor = null;
        }

        private void ProcessTag(bool simpleKey) {
            string tag = null;
            if (_event is ScalarEvent) {
                // TODO: canonical?

                ScalarEvent ev = (ScalarEvent)_event;
                tag = ev.Tag;
                if (tag == null) {
                    _preparedTag = null;
                    // non-string quoted scalars marked by "!" tag will be parsed:
                    if (ChooseScalarStyle(_flowLevel, simpleKey) != ScalarQuotingStyle.None && ev.Type == ScalarValueType.Other) {
                        tag = "!";
                    } else {
                        return;
                    }
                }
            } else {
                CollectionStartEvent ev = (CollectionStartEvent)_event;
                tag = ev.Tag;
                if (tag == null) {
                    _preparedTag = null;
                    return;
                }
                _indentation = true;
            }

            if (_preparedTag == null) {
                _preparedTag = PrepareTag(tag);
            }
            if (!String.IsNullOrEmpty(_preparedTag)) {
                WriteIndicator(_preparedTag + " ", true, true, true);
            }
            _preparedTag = null;
        }

        private ScalarQuotingStyle ChooseScalarStyle(int flowLevel, bool simpleKey) {
            ScalarEvent ev = (ScalarEvent)_event;

            switch (ev.Style) {
                case ScalarQuotingStyle.Double:
                    if (_canonical || ev.IsEmpty) {
                        return ScalarQuotingStyle.Double;
                    }
                    break;

                case ScalarQuotingStyle.None:
                    if (ev.IsEmpty) {
                        return ev.Value != null ? ScalarQuotingStyle.Double : ScalarQuotingStyle.None;
                    }
                    
                    if ((!simpleKey || !ev.IsEmpty && !ev.IsMultiline) &&
                        (flowLevel != 0 && ev.AllowFlowPlain || flowLevel == 0 && ev.AllowBlockPlain)) {
                        return ScalarQuotingStyle.None;
                    }
                    break;

                case ScalarQuotingStyle.Literal:
                case ScalarQuotingStyle.Folded:
                    if (flowLevel == 0 && ev.AllowBlock) {
                        return ScalarQuotingStyle.Single;
                    }
                    break;
            }

            if (ev.IsMultiline && !ev.HasSpecialCharacters) {
                return ScalarQuotingStyle.Literal;
            }

            return ScalarQuotingStyle.Double;
        }

        private void ProcessScalar(bool simpleKey) {
            ScalarEvent ev = (ScalarEvent)_event;
            var style = ChooseScalarStyle(_flowLevel, simpleKey);

            switch (style) {
                case ScalarQuotingStyle.Single:
                    WriteSingleQuoted(ev.Value, !simpleKey);
                    break;

                case ScalarQuotingStyle.Double:
                    WriteDoubleQuoted(ev.Value, !simpleKey);
                    break;

                case ScalarQuotingStyle.Folded:
                    WriteFolded(ev.Value);
                    break;

                case ScalarQuotingStyle.Literal:
                    WriteLiteral(ev.Value, !ev.IsBinary);
                    break;

                case ScalarQuotingStyle.None:
                    WritePlain(ev.Value, !simpleKey);
                    break;

                default:
                    throw Assert.Unreachable;
            }
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