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
using System.IO;
using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace IronRuby.StandardLibrary.Yaml {

    public class Serializer {
        private readonly Emitter/*!*/ _emitter;
        private readonly bool _useExplicitStart;
        private readonly bool _useExplicitEnd;
        private readonly Version _useVersion;
        private readonly string _anchorTemplate;

        private readonly Dictionary<Node, object>/*!*/ _serializedNodes = new Dictionary<Node, object>(ReferenceEqualityComparer<Node>.Instance);
        private readonly Dictionary<Node, string>/*!*/ _anchors = new Dictionary<Node, string>(ReferenceEqualityComparer<Node>.Instance);
        private int _lastAnchorId;
        private bool _closed;

        public Serializer(TextWriter/*!*/ writer, YamlOptions/*!*/ opts)
            : this(new Emitter(writer, opts), opts) {
        }

        public Serializer(Emitter/*!*/ emitter, YamlOptions/*!*/ opts) {
            ContractUtils.RequiresNotNull(emitter, "emitter");

            _emitter = emitter;
            _useExplicitStart = opts.ExplicitStart;
            _useExplicitEnd = opts.ExplicitEnd;
            if (opts.UseVersion) {
                _useVersion = opts.Version;
            }
            _anchorTemplate = opts.AnchorFormat ?? "id{0:000}";
            _emitter.Emit(StreamStartEvent.Instance);
        }

        public Emitter/*!*/ Emitter {
            get { return _emitter; }
        }

        protected virtual bool IgnoreAnchor(Node node) {
            // This is possibly Ruby specific.
            // but it didn't seem worth subclassing Serializer for this one method
            return !(node is CollectionNode);
            //return false;
        }

        public void Close() {
            if (!_closed) {
                _emitter.Emit(StreamEndEvent.Instance);
                _closed = true;
            }
        }

        public void Serialize(Node node) {
            if (_closed) {
                throw new SerializerException("serializer is closed");
            }
            _emitter.Emit(new DocumentStartEvent(_useExplicitStart, _useVersion, null));
            AnchorNode(node);
            SerializeNode(node, null, null);
            _emitter.Emit(_useExplicitEnd ? DocumentEndEvent.ExplicitInstance : DocumentEndEvent.ImplicitInstance);

            _serializedNodes.Clear();
            _anchors.Clear();
            _lastAnchorId = 0;
        }

        private void AnchorNode(Node node) {
            while (node is LinkNode) {
                node = ((LinkNode)node).Linked;
            }
            if (!IgnoreAnchor(node)) {
                string anchor;
                if (_anchors.TryGetValue(node, out anchor)) {
                    if (null == anchor) {
                        _anchors[node] = GenerateAnchor(node);
                    }
                } else {
                    _anchors.Add(node, null);

                    SequenceNode seq;
                    MappingNode map;
                    if ((seq = node as SequenceNode) != null) {
                        foreach (Node n in seq.Nodes) {
                            AnchorNode(n);
                        }
                    } else if ((map = node as MappingNode) != null) {
                        foreach (KeyValuePair<Node, Node> e in map.Nodes) {
                            AnchorNode(e.Key);
                            AnchorNode(e.Value);
                        }
                    }
                }
            }
        }

        private string GenerateAnchor(Node node) {
            return string.Format(_anchorTemplate, ++_lastAnchorId);
        }

        private void SerializeNode(Node node, Node parent, object index) {
            while (node is LinkNode) {
                node = ((LinkNode)node).Linked;
            }

            string tAlias;
            _anchors.TryGetValue(node, out tAlias);

            if (_serializedNodes.ContainsKey(node) && tAlias != null) {
                _emitter.Emit(new AliasEvent(tAlias));
            } else {

                _serializedNodes[node] = null;
                //_resolver.descendResolver(parent, index);

                ScalarNode scalar;
                SequenceNode seq;
                MappingNode map;

                if ((scalar = node as ScalarNode) != null) {
                    string tag = node.Tag;
                    ScalarQuotingStyle style = scalar.Style;
                    ScalarValueType type;
                    if (tag == null) {
                        // quote an untagged sctring scalar that might be parsed as a different scalar type if not quoted:
                        if (style == ScalarQuotingStyle.None && ResolverScanner.Recognize(scalar.Value) != null) {
                            style = ScalarQuotingStyle.Double;
                        }
                        type = ScalarValueType.String;
                    } else if (tag == Tags.Str) {
                        // omit the tag for strings that are not recognizable as other scalars:
                        if (ResolverScanner.Recognize(scalar.Value) == null) {
                            tag = null;
                        }
                        type = ScalarValueType.String;
                    } else if (scalar.Value == null) {
                        tag = null;
                        type = ScalarValueType.Other;
                    } else {
                        // omit the tag for non-string scalars whose type can be recognized from their value:
                        string detectedTag = ResolverScanner.Recognize(scalar.Value);
                        if (detectedTag != null && tag.StartsWith(detectedTag, StringComparison.Ordinal)) {
                            tag = null;
                        }
                        type = ScalarValueType.Other;
                    }

                    _emitter.Emit(new ScalarEvent(tAlias, tag, type, scalar.Value, style));
                } else if ((seq = node as SequenceNode) != null) {
                    _emitter.Emit(new SequenceStartEvent(tAlias, node.Tag, seq.FlowStyle));
                    int ix = 0;
                    foreach (Node n in seq.Nodes) {
                        SerializeNode(n, node, ix++);
                    }
                    _emitter.Emit(SequenceEndEvent.Instance);

                } else if ((map = node as MappingNode) != null) {
                    _emitter.Emit(new MappingStartEvent(tAlias, node.Tag, map.FlowStyle));
                    foreach (KeyValuePair<Node, Node> e in map.Nodes) {
                        SerializeNode(e.Key, node, null);
                        SerializeNode(e.Value, node, e.Key);
                    }
                    _emitter.Emit(MappingEndEvent.Instance);
                }
            }
        }
    }
}