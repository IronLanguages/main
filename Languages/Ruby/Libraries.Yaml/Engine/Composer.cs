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
using System.Text;

namespace IronRuby.StandardLibrary.Yaml {
   
    public class Composer : NodeProvider {
        private readonly Parser _parser;
        private readonly Dictionary<string, Node> _anchors;

        public Composer(Parser parser) {
            _parser = parser;
            _anchors = new Dictionary<string, Node>();
        }

        public override Encoding/*!*/ Encoding {
            get { return _parser.Encoding; }
        }

        private Node ComposeDocument() {
            if (_parser.PeekEvent() is StreamStartEvent) {
                //Drop STREAM-START event
                _parser.GetEvent();
            }
            //Drop DOCUMENT-START event
            _parser.GetEvent();
            Node node = ComposeNode(null, null);
            //Drop DOCUMENT-END event
            _parser.GetEvent();
            this._anchors.Clear();
            return node;
        }

        private void AddAnchor(string anchor, Node node) {
            if (_anchors.ContainsKey(anchor)) {
                _anchors[anchor] = node;
            } else {
                _anchors.Add(anchor, node);
            }
        }

        private Node ComposeNode(Node parent, object index) {
            Node result;
            YamlEvent @event = _parser.PeekEvent();
            NodeEvent nodeEvent = @event as NodeEvent;

            string anchor = (nodeEvent != null) ? nodeEvent.Anchor : null;

            if (nodeEvent is AliasEvent) {
                _parser.GetEvent();
                if (!_anchors.TryGetValue(anchor, out result)) {
                    throw new ComposerException("found undefined alias: " + anchor);
                }
                return result;
            }

            result = null;
            //_resolver.descendResolver(parent, index);
            if (@event is ScalarEvent) {
                ScalarEvent ev = (ScalarEvent)_parser.GetEvent();

                string tag = ev.Tag;
                if (ev.Type == ScalarValueType.Unknown) {
                    Debug.Assert(tag == null || tag == "!");
                    tag = ResolverScanner.Recognize(ev.Value) ?? Tags.Str;
                }

                result = new ScalarNode(tag, ev.Value, ev.Style);
                if (anchor != null) {
                    AddAnchor(anchor, result);
                }
            } else if (@event is SequenceStartEvent) {
                SequenceStartEvent start = (SequenceStartEvent)_parser.GetEvent();
                SequenceNode seqResult = new SequenceNode(start.Tag != "!" ? start.Tag : null, new List<Node>(), start.FlowStyle);
                result = seqResult;
                if (anchor != null) {
                    AddAnchor(anchor, seqResult);
                }
                int ix = 0;
                while (!(_parser.PeekEvent() is SequenceEndEvent)) {
                    seqResult.Nodes.Add(ComposeNode(seqResult, ix++));
                }
                _parser.GetEvent();
            } else if (@event is MappingStartEvent) {
                MappingStartEvent start = (MappingStartEvent)_parser.GetEvent();
                MappingNode mapResult = new MappingNode(start.Tag != "!" ? start.Tag : null, new Dictionary<Node, Node>(), start.FlowStyle);
                result = mapResult;
                if (anchor != null) {
                    AddAnchor(anchor, result);
                }
                while (!(_parser.PeekEvent() is MappingEndEvent)) {
                    YamlEvent key = _parser.PeekEvent();
                    Node itemKey = ComposeNode(mapResult, key);
                    Node composed = ComposeNode(mapResult, itemKey);
                    if (!mapResult.Nodes.ContainsKey(itemKey)) {
                        mapResult.Nodes.Add(itemKey, composed);
                    }
                }
                _parser.GetEvent();
            }
            //_resolver.ascendResolver();
            return result;
        }

        #region NodeProvider Members

        public override bool CheckNode() {
            return !(_parser.PeekEvent() is StreamEndEvent);
        }

        public override Node GetNode() {
            return CheckNode() ? ComposeDocument() : (Node)null;
        }        

        #endregion
    }
}
