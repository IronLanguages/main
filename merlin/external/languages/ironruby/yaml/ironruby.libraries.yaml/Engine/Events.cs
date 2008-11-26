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

        public override string ToString() {
            return "#<" + GetType().Name + ">";
        }
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

        public override string ToString() {
            return string.Format("#<{0} Explicit={1}", GetType().Name, Explicit);
        }
    }

    public abstract class CollectionStartEvent : NodeEvent {
        private readonly string _tag;
        private readonly bool _implicit;
        private readonly bool _flowStyle;

        internal CollectionStartEvent(string anchor, string tag, bool @implicit, bool flowStyle)
            : base(anchor) {
            _tag = tag;
            _implicit = @implicit;
            _flowStyle = flowStyle;
        }

        public string Tag { get { return _tag; } }
        public bool Implicit { get { return _implicit; } }
        public bool FlowStyle { get { return _flowStyle; } }

        public override string ToString() {
            return string.Format("#<{0} Tag=\"{1}\", Implicit={2} FlowStyle={3}>", GetType().Name, Tag, Implicit, FlowStyle);
        }
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

        public override string ToString() {
            string tags = "null";
            if (_tags != null) {
                StringBuilder str = new StringBuilder('{');
                foreach (KeyValuePair<string, string> t in _tags) {
                    if (str.Length != 1) {
                        str.Append(", ");
                    }
                    str.Append('\'').Append(t.Key).Append("': '").Append(t.Value).Append('\'');
                }
                str.Append('}');
                tags = str.ToString();
            }
            return string.Format("#<DocumentStartEvent Version={0}, Tags={1}>", Version, tags);
        }
    }

    public sealed class MappingEndEvent : CollectionEndEvent {
        public static readonly MappingEndEvent Instance = new MappingEndEvent();
        private MappingEndEvent() { }
    }

    public sealed class MappingStartEvent : CollectionStartEvent {
        public MappingStartEvent(string anchor, string tag, bool @implicit, bool flowStyle)
            : base(anchor, tag, @implicit, flowStyle) {
        }
    }
    
    public sealed class ScalarEvent : NodeEvent {
        // TODO: can tag, implicit merge with CollectionStartEvent?
        private readonly string _tag;
        private readonly bool[] _implicit; // TODO: one element array?
        private readonly string _value;
        private readonly char _style;

        public ScalarEvent(string anchor, string tag, bool[] @implicit, string value, char style)
            : base(anchor) {
            if (@implicit == null || @implicit.Length != 2) {
                throw new ArgumentException("requires a 2 element array", "@implicit");
            }
            _tag = tag;
            _implicit = @implicit;
            _value = value;
            _style = style;
        }

        public string Tag { get { return _tag; } }
        public bool[] Implicit { get { return _implicit; } }
        public string Value { get { return _value; } }
        public char Style { get { return _style; } }


        public override string ToString() {
            string value = Value.Replace("\r", "\\r").Replace("\n", "\\n");
            if (value.Length > 30) {
                value = value.Substring(0, 27) + "...";
            }
            string @implicit = (_implicit[0] ? "T" : "F") + "," + (_implicit[1] ? "T" : "F");
            return string.Format("#<ScalarEvent Tag=\"{0}\", Implicit={1} Style='{2}' Value=\"{3}\">", Tag, @implicit, Style, value);
        }
    }

    public sealed class SequenceEndEvent : CollectionEndEvent {
        public static readonly SequenceEndEvent Instance = new SequenceEndEvent();
        private SequenceEndEvent() { }
    }

    public sealed class SequenceStartEvent : CollectionStartEvent {
        public SequenceStartEvent(string anchor, string tag, bool @implicit, bool flowStyle)
            : base(anchor, tag, @implicit, flowStyle) {
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
