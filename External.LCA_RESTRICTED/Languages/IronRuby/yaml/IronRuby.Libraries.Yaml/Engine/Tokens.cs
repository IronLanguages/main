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
using System.Globalization;

namespace IronRuby.StandardLibrary.Yaml {

    public abstract class Token {
        internal Token() { }

        public override string ToString() {
            return "#<" + GetType().Name + ">";
        }
    }

    public sealed class AliasToken : Token {
        private readonly string _value;

        public AliasToken(string value) {
            _value = value;
        }

        public string Value { get { return _value; } }

        public override string ToString() {
            return string.Format("#<AliasToken Value=\"{0}\">", Value);
        }
    }

    public sealed class AnchorToken : Token {
        private readonly string _value;

        public AnchorToken(string value) {
            _value = value;
        }

        public string Value { get { return _value; } }

        public override string ToString() {
            return String.Format(CultureInfo.InvariantCulture, "#<AnchorToken Value=\"{0}\">", Value);
        }
    }

    public sealed class TagToken : Token {
        private readonly string _handle, _suffix;

        public TagToken(string handle, string suffix) {
            _handle = handle;
            _suffix = suffix;
        }

        public string Handle { get { return _handle; } }
        public string Suffix { get { return _suffix; } }
    }

    public sealed class ScalarToken : Token {
        private readonly string _value;
        private readonly ScalarQuotingStyle _style;

        public ScalarToken(string value, ScalarQuotingStyle style) {
            _value = value;
            _style = style;
        }

        public string Value { get { return _value; } }
        public ScalarQuotingStyle Style { get { return _style; } }

        public override string ToString() {
            return String.Format(CultureInfo.InvariantCulture, "#<ScalarToken Value=\"{0}\" Style=\"{1}\">", 
                Value, Style);
        }
    }

    public sealed class DirectiveToken : Token {
        private readonly string _name;
        private readonly string[] _value;

        public DirectiveToken(string name, string[] value) {
            if (value != null && value.Length != 2) {
                throw new ArgumentException("must be null or a 2 element array", "value");
            }
            _name = name;
            _value = value;
        }

        public string Name { get { return _name; } }
        public string[] Value { get { return _value; } }
    }

    public sealed class BlockEndToken : Token {
        public static readonly BlockEndToken Instance = new BlockEndToken();
        private BlockEndToken() { }
    }
    public sealed class BlockEntryToken : Token {
        public static readonly BlockEntryToken Instance = new BlockEntryToken();
        private BlockEntryToken() { }
    }
    public sealed class BlockMappingStartToken : Token {
        public static readonly BlockMappingStartToken Instance = new BlockMappingStartToken();
        private BlockMappingStartToken() { }
    }
    public sealed class BlockSequenceStartToken : Token {
        public static readonly BlockSequenceStartToken Instance = new BlockSequenceStartToken();
        private BlockSequenceStartToken() { }
    }
    public sealed class DocumentEndToken : Token {
        public static readonly DocumentEndToken Instance = new DocumentEndToken();
        private DocumentEndToken() { }
    }
    public sealed class DocumentStartToken : Token {
        public static readonly DocumentStartToken Instance = new DocumentStartToken();
        private DocumentStartToken() { }
    }
    public sealed class FlowEntryToken : Token {
        public static readonly FlowEntryToken Instance = new FlowEntryToken();
        private FlowEntryToken() { }
    }
    public sealed class FlowMappingEndToken : Token {
        public static readonly FlowMappingEndToken Instance = new FlowMappingEndToken();
        private FlowMappingEndToken() { }
    }
    public sealed class FlowMappingStartToken : Token {
        public static readonly FlowMappingStartToken Instance = new FlowMappingStartToken();
        private FlowMappingStartToken() { }
    }
    public sealed class FlowSequenceEndToken : Token {
        public static readonly FlowSequenceEndToken Instance = new FlowSequenceEndToken();
        private FlowSequenceEndToken() { }
    }
    public sealed class FlowSequenceStartToken : Token {
        public static readonly FlowSequenceStartToken Instance = new FlowSequenceStartToken();
        private FlowSequenceStartToken() { }
    }
    public sealed class KeyToken : Token {
        public static readonly KeyToken Instance = new KeyToken();
        private KeyToken() { }
    }
    public sealed class ValueToken : Token {
        public static readonly ValueToken Instance = new ValueToken();
        private ValueToken() { }
    }
    public sealed class StreamEndToken : Token {
        public static readonly StreamEndToken Instance = new StreamEndToken();
        private StreamEndToken() { }
    }
    public sealed class StreamStartToken : Token {
        public static readonly StreamStartToken Instance = new StreamStartToken();
        private StreamStartToken() { }
    }

}
