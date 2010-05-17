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
            _tag = tag;
        }

        public string Tag {
            get { return _tag; }
            set { _tag = value; }
        }

        public abstract string/*!*/ DefaultTag { get; }
    }

    public abstract class CollectionNode : Node {
        public FlowStyle FlowStyle { get; set; }

        internal CollectionNode(string tag, FlowStyle flowStyle)
            : base(tag) {
            FlowStyle = flowStyle;
        }
    }

    public sealed class ScalarNode : Node {
        public ScalarQuotingStyle Style { get; set; }

        // Can only be an ASCII string. Non-ascii values must be base-64 encoded or escaped.
        // A null reference represents null scalar.
        // Otherwise represents a string or stringified int, bool, etc.
        private readonly string _value;

        public ScalarNode(string tag, string value, ScalarQuotingStyle style)
            : base(tag) {
            Style = style;
            _value = value;
        }

        public string Value {
            get { return _value; }
        }

        public override string/*!*/ DefaultTag {
            get { return Tags.Str; }
        }
    }

    public sealed class LinkNode : Node {
        private Node/*!*/ _linked;

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

        public override string/*!*/ DefaultTag {
            get { return _linked.DefaultTag; }
        }
    }

    public sealed class SequenceNode : CollectionNode {
        private readonly IList<Node>/*!*/ _nodes;

        public SequenceNode(string tag, IList<Node>/*!*/ nodes, FlowStyle flowStyle)
            : base(tag, flowStyle) {
            _nodes = nodes;
        }

        public IList<Node>/*!*/ Nodes {
            get { return _nodes; }
        }

        public override string/*!*/ DefaultTag {
            get { return Tags.Seq; }
        }
    }

    public sealed class MappingNode : CollectionNode {
        private readonly IDictionary<Node, Node>/*!*/ _nodes;

        public MappingNode(string tag, IDictionary<Node, Node>/*!*/ nodes, FlowStyle flowStyle)
            : base(tag, flowStyle) {
            _nodes = nodes;
        }

        public IDictionary<Node, Node>/*!*/ Nodes {
            get { return _nodes; }
        }

        public override string/*!*/ DefaultTag {
            get { return Tags.Map; }
        }
    }
}
