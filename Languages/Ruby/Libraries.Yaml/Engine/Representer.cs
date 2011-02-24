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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Globalization;

namespace IronRuby.StandardLibrary.Yaml {
    public class Representer {
        private readonly Dictionary<object, Node>/*!*/ _representedObjects = new Dictionary<object, Node>(ReferenceEqualityComparer<object>.Instance);
        private readonly Dictionary<object, List<LinkNode>>/*!*/ _links = new Dictionary<object, List<LinkNode>>(ReferenceEqualityComparer<object>.Instance);
        private readonly static object _NullKey = new object();

        // recursion level as we build YAML representation of an object graph:
        private int _level;

        public Representer() {
        }

        public int Level {
            get { return _level; }
            set { _level = value; }
        }

        public void ForgetObjects() {
            _representedObjects.Clear();
        }

        public Node/*!*/ RepresentItem(object item) {
            int oldLevel = _level++;
            try {
                return Represent(item);
            } finally {
                _level = oldLevel;
            }
        }

        public Node/*!*/ Represent(object data) {
            Node node;

            bool ignoreAlias = HasIdentity(data);

            object dataKey = data ?? _NullKey;

            if (!ignoreAlias) {
                if (_representedObjects.TryGetValue(dataKey, out node)) {
                    if (node == null) {
                        LinkNode link = new LinkNode();
                        List<LinkNode> list;
                        if (!_links.TryGetValue(dataKey, out list)) {
                            _links.Add(dataKey, list = new List<LinkNode>());
                        }
                        list.Add(link);
                        return link;
                    }
                    return node;
                }
                _representedObjects.Add(dataKey, null);
            }

            node = CreateNode(data);

            if (!ignoreAlias) {
                _representedObjects[dataKey] = node;

                List<LinkNode> list;
                if (_links.TryGetValue(dataKey, out list)) {
                    _links.Remove(dataKey);
                    foreach (LinkNode n in list) {
                        n.Linked = node;
                    }
                }
            }
            return node;
        }

        public ScalarNode/*!*/ Scalar(string tag, string value, ScalarQuotingStyle style) {
            return new ScalarNode(tag, value, style);
        }

        public ScalarNode/*!*/ Scalar(bool value) {
            return Scalar(value ? Tags.True : Tags.False, value ? "true" : "false", ScalarQuotingStyle.None);
        }

        public SequenceNode/*!*/ Sequence(string tag, IList/*!*/ sequence, FlowStyle flowStyle) {
            List<Node> value = new List<Node>(sequence.Count);
            foreach (object x in sequence) {
                value.Add(RepresentItem(x));
            }
            return new SequenceNode(tag, value, flowStyle);
        }

        public MappingNode/*!*/ Map(string tag, IDictionary/*!*/ mapping, FlowStyle flowStyle) {
            return Map(new Dictionary<Node, Node>(mapping.Count), tag, mapping, flowStyle);
        }

        public MappingNode/*!*/ Map(Dictionary<Node, Node> value, string tag, IDictionary/*!*/ mapping, FlowStyle flowStyle) {
            foreach (DictionaryEntry entry in mapping) {
                value.Add(RepresentItem(entry.Key), RepresentItem(entry.Value));
            }
            return new MappingNode(tag, value, flowStyle);
        }

        protected virtual bool HasIdentity(object data) {
            return data == null || data is string || data is bool || data is int || data is float || data is double || data is decimal;
        }

        protected virtual Node CreateNode(object data) {
            return BaseCreateNode(data);
        }

        protected internal Node BaseCreateNode(object data) {
            if (data == null) {
                return Scalar(Tags.Null, null, ScalarQuotingStyle.None);
            }

            IDictionary map = data as IDictionary;
            if (map != null) {
                string taguri = map is Dictionary<object, object> ? Tags.Map : Tags.Map + ":" + map.GetType().Name;
                return Map(taguri, map, FlowStyle.Block);
            }

            byte[] bytes = data as byte[];
            if (bytes != null) {
                return Scalar(Tags.Binary, Convert.ToBase64String(bytes) + "\n", ScalarQuotingStyle.None);
            }

            IList seq = data as IList;
            if (seq != null) {
                string taguri = seq is List<object> ? Tags.Seq : Tags.Seq + ":" + seq.GetType().Name;
                return Sequence(taguri, seq, FlowStyle.Block);
            }

            ICollection set = data as ICollection;
            if (set != null) {
                Dictionary<object, object> entries = new Dictionary<object, object>(set.Count);
                foreach (object x in set) {
                    entries.Add(x, null);
                }
                return Map("tag:yaml.org,2002:set", entries, FlowStyle.Block);
            }

            PrivateType pt = data as PrivateType;
            if (pt != null) {
                Node n = Represent(pt.Value);
                n.Tag = pt.Tag;
                return n;
            }

            string tag, value;
            switch (Type.GetTypeCode(data.GetType())) {
                case TypeCode.Boolean:
                    tag = "tag:yaml.org,2002:bool";
                    value = data.ToString();
                    break;

                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Char:
                    tag = Tags.Int;
                    value = data.ToString();
                    break;

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    tag = Tags.Float;
                    value = data.ToString();
                    if (value == "Infinity") {
                        value = ".inf";
                    } else if (value == "-Infinity") {
                        value = "-.inf";
                    } else if (value == "NaN") {
                        value = ".nan";
                    }
                    break;

                case TypeCode.String:
                    tag = Tags.Str;
                    value = data.ToString();
                    break;

                case TypeCode.DateTime:
                    DateTime date = (DateTime)data;
                    string format = (date.Millisecond != 0) ? "yyyy-MM-dd HH:mm:ss.SSS Z" : "yyyy-MM-dd HH:mm:ss Z";
                    value = date.ToString(format, CultureInfo.InvariantCulture);
                    // TODO: what is this code for?
                    if (value.Length >= 23) {
                        value = value.Substring(0, 23) + ":" + value.Substring(23);
                    }
                    tag = Tags.Timestamp;
                    break;

                default:
                    return CreateNodeForObject(data);
            }

            return Scalar(tag, value, ScalarQuotingStyle.None);
        }

        // TODO: use some type of standard .NET serialization
        private Node CreateNodeForObject(object data) {
            Dictionary<object, object> values = new Dictionary<object, object>();

            foreach (PropertyInfo prop in data.GetType().GetProperties()) {
                MethodInfo getter = prop.GetGetMethod();
                if (getter != null && getter.GetParameters().Length == 0) {
                    try {
                        values.Add(prop.Name, prop.GetValue(data, null));
                    } catch (Exception) {
                        values.Add(prop.Name, null);
                    }
                }
            }
            return Map("!cli/object:" + data.GetType().Name, values, FlowStyle.Block);
        }
    }
}