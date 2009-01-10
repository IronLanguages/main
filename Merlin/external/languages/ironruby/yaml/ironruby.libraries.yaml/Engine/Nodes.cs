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
using System.Collections.ObjectModel;

namespace IronRuby.StandardLibrary.Yaml {

    public abstract class Node {
        private string _tag;

        internal Node(string tag) {
            _tag = tag ?? "";
        }

        public string Tag {
            get { return _tag; }
            set { _tag = value; }
        }

        public override string ToString() {
            return string.Format("#<{0} Tag=\"{1}\">", GetType().Name, Tag);
        }
    }

    public abstract class CollectionNode : Node {
        private bool _flowStyle;

        internal CollectionNode(string tag, bool flowStyle)
            : base(tag) {
            _flowStyle = flowStyle;
        }

        public bool FlowStyle {
            get { return _flowStyle; }
        }
    }

    public sealed class ScalarNode : Node {
        private readonly char _style;
        private readonly string _value;

        public ScalarNode(string tag, string value, char style)
            : base(tag) {
            _style = style;
            _value = value;
        }

        public char Style {
            get { return _style; }
        }

        public string Value {
            get { return _value; }
        }

        public override string ToString() {
            string value = Value.Replace("\r", "\\r").Replace("\n", "\\n");
            if (value.Length > 35) {
                value = value.Substring(0, 32) + "...";
            }
            return string.Format("#<ScalarNode Tag=\"{0}\" Style='{1}' Value=\"{2}\">", Tag, Style, value);
        }
    }

    public sealed class LinkNode : Node {
        private Node _linked;

        public LinkNode()
            : base(null) {
        }

        public LinkNode(Node linked)
            : base(null) {
            _linked = linked;
        }

        public Node Linked {
            get { return _linked; }
            set { _linked = value; }
        }
    }


    public sealed class SequenceNode : CollectionNode {
        private readonly IList<Node> _nodes;

        public SequenceNode(string tag, IList<Node> nodes, bool flowStyle)
            : base(tag, flowStyle) {
            _nodes = nodes;
        }

        public IList<Node> Nodes {
            get { return _nodes; }
        }
    }

    public sealed class MappingNode : CollectionNode {
        private readonly IDictionary<Node, Node> _nodes;

        public MappingNode(string tag, IDictionary<Node, Node> nodes, bool flowStyle)
            : base(tag, flowStyle) {
            _nodes = nodes;
        }

        public IDictionary<Node, Node> Nodes {
            get { return _nodes; }
        }
    }
}
