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
using System.Text;
using System.Collections;

namespace IronRuby.StandardLibrary.Yaml {
    public abstract class YamlEvent {
        internal YamlEvent() { }
    }

    public abstract class NodeEvent : YamlEvent {
        private readonly string _anchor;

        internal NodeEvent(string anchor) {
            _anchor = anchor;
        }

        public string Anchor { get { return _anchor; } }
    }

    public abstract class DocumentEvent : YamlEvent {
        private readonly bool _explicit;

        internal DocumentEvent(bool @explicit) {
            _explicit = @explicit;
        }

        public bool Explicit { get { return _explicit; } }
    }

    public abstract class CollectionStartEvent : NodeEvent {
        private readonly string _tag;
        private readonly FlowStyle _flowStyle;

        internal CollectionStartEvent(string anchor, string tag, FlowStyle flowStyle)
            : base(anchor) {
            _tag = tag != "!" ? tag : null;
            _flowStyle = flowStyle;
        }

        /// <summary>
        /// A tag or null if the implicit tag should be used.
        /// </summary>
        public string Tag { get { return _tag; } }

        /// <summary>
        /// False if formatted as a block (each item on its own line). 
        /// </summary>
        public FlowStyle FlowStyle { get { return _flowStyle; } }
    }

    public abstract class CollectionEndEvent : YamlEvent {
    }

    public sealed class AliasEvent : NodeEvent {
        public AliasEvent(string anchor) : base(anchor) { }
    }

    public sealed class DocumentEndEvent : DocumentEvent {
        public static readonly DocumentEndEvent ExplicitInstance = new DocumentEndEvent(true);
        public static readonly DocumentEndEvent ImplicitInstance = new DocumentEndEvent(false);
        private DocumentEndEvent(bool @explicit) : base(@explicit) { }
    }

    public sealed class DocumentStartEvent : DocumentEvent {
        private readonly Version _version;
        private readonly Dictionary<string, string> _tags;

        public DocumentStartEvent(bool @explicit, Version version, Dictionary<string, string> tags)
            : base(@explicit) {
            _version = version;
            _tags = tags;
        }

        public Version Version {
            get { return _version; }
        }

        public IDictionary<string, string> Tags {
            get { return _tags; }
        }
    }

    public sealed class MappingEndEvent : CollectionEndEvent {
        public static readonly MappingEndEvent Instance = new MappingEndEvent();
        private MappingEndEvent() { }
    }

    public sealed class MappingStartEvent : CollectionStartEvent {
        public MappingStartEvent(string anchor, string tag, FlowStyle flowStyle)
            : base(anchor, tag, flowStyle) {
        }
    }

    public enum ScalarValueType {
        Unknown,
        String,
        Other
    }
    
    public sealed class ScalarEvent : NodeEvent {
        private readonly string _tag;
        private readonly ScalarValueType _type;
        private readonly string _value;
        private readonly ScalarQuotingStyle _style;
        private ScalarProperties? _analysis; // lazy

        public ScalarEvent(string anchor, string tag, ScalarValueType type, string value, ScalarQuotingStyle style)
            : base(anchor) {
            _tag = tag;
            _type = type;
            _value = value;
            _style = style;
        }

        public string Tag { get { return _tag; } }
        public ScalarValueType Type { get { return _type; } }
        public string Value { get { return _value; } }
        public ScalarQuotingStyle Style { get { return _style; } }

        public bool IsEmpty { get { return (Analysis & ScalarProperties.Empty) != 0; } }
        public bool IsMultiline { get { return (Analysis & ScalarProperties.Multiline) != 0; } }
        public bool AllowFlowPlain { get { return (Analysis & ScalarProperties.AllowFlowPlain) != 0; } }
        public bool AllowBlockPlain { get { return (Analysis & ScalarProperties.AllowBlockPlain) != 0; } }
        public bool AllowSingleQuoted { get { return (Analysis & ScalarProperties.AllowSingleQuoted) != 0; } }
        public bool AllowDoubleQuoted { get { return (Analysis & ScalarProperties.AllowDoubleQuoted) != 0; } }
        public bool AllowBlock { get { return (Analysis & ScalarProperties.AllowBlock) != 0; } }
        public bool HasSpecialCharacters { get { return (Analysis & ScalarProperties.SpecialCharacters) != 0; } }

        internal ScalarProperties Analysis {
            get {
                if (_analysis == null) {
                    _analysis = Emitter.AnalyzeScalar(_value);
                }
                return _analysis.Value;
            }
        }

        internal bool IsBinary {
            get { return _tag == Tags.Binary; }
        }
    }

    public sealed class SequenceEndEvent : CollectionEndEvent {
        public static readonly SequenceEndEvent Instance = new SequenceEndEvent();
        private SequenceEndEvent() { }
    }

    public sealed class SequenceStartEvent : CollectionStartEvent {
        public SequenceStartEvent(string anchor, string tag, FlowStyle flowStyle)
            : base(anchor, tag, flowStyle) {
        }
    }

    public sealed class StreamStartEvent : YamlEvent {
        public static readonly StreamStartEvent Instance = new StreamStartEvent();
        private StreamStartEvent() { }
    }

    public sealed class StreamEndEvent : YamlEvent {
        public static readonly StreamEndEvent Instance = new StreamEndEvent();
        private StreamEndEvent() { }
    }

}
