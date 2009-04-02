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

namespace IronRuby.StandardLibrary.Yaml {

    public class Representer {
        private readonly Serializer/*!*/ _serializer;
        private readonly char _defaultStyle;
        private readonly Dictionary<object, Node>/*!*/ _representedObjects = new Dictionary<object, Node>(ReferenceEqualityComparer<object>.Instance);
        private readonly Dictionary<object, List<LinkNode>>/*!*/ _links = new Dictionary<object, List<LinkNode>>(ReferenceEqualityComparer<object>.Instance);
        private readonly static object _NullKey = new object();

        public Representer(Serializer/*!*/ serializer, YamlOptions opts)
            : this(serializer, opts.UseDouble ? '"' : (opts.UseSingle ? '\'' : '\0')) {
        }

        public Representer(Serializer/*!*/ serializer, char defaultStyle) {
            ContractUtils.RequiresNotNull(serializer, "serializer");
            if (defaultStyle != '"' && defaultStyle != '\'' && defaultStyle != '\0') {
                throw new ArgumentException("must be single quote, double quote, or zero", "defaultStyle");
            }

            _serializer = serializer;
            _defaultStyle = defaultStyle;
        }

        public Serializer/*!*/ Serializer {
            get { return _serializer; }
        }

        private Node RepresentData(object data) {
            Node node = null;

            bool ignoreAlias = IgnoreAliases(data);

            object dataKey = data ?? _NullKey;

            if (!ignoreAlias) {
                if (_representedObjects.TryGetValue(dataKey , out node)) {
                    if (null == node) {
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

        public Node Scalar(string tag, string value, char style) {
            return new ScalarNode(tag, value, style == 0 ? _defaultStyle : style);
        }

        public Node Sequence(string tag, IList sequence, bool flowStyle) {
            List<Node> value = new List<Node>(sequence.Count);
            foreach (object x in sequence) {
                value.Add(RepresentData(x));
            }
            return new SequenceNode(tag, value, flowStyle);
        }

        public Node/*!*/ Map(string tag, IDictionary/*!*/ mapping, bool flowStyle) {
            Dictionary<Node, Node> value = new Dictionary<Node, Node>(mapping.Count);
            foreach (DictionaryEntry entry in mapping) {
                value.Add(RepresentData(entry.Key), RepresentData(entry.Value));
            }
            return new MappingNode(tag, value, flowStyle);
        }

        public void Represent(object data) {
            _serializer.Serialize(RepresentData(data));
            _representedObjects.Clear();
        }

        protected virtual bool IgnoreAliases(object data) {
            return data == null || data is string || data is bool || data is int || data is float || data is double || data is decimal;
        }

        protected virtual Node CreateNode(object data) {
            return BaseCreateNode(data);
        }

        protected internal Node BaseCreateNode(object data) {
            if (data == null) {
                return Scalar("tag:yaml.org,2002:null", "", (char)0);
            }

            IDictionary map = data as IDictionary;
            if (map != null) {
                string taguri = map is Dictionary<object, object>
                    ? "tag:yaml.org,2002:map"
                    : "tag:yaml.org,2002:map:" + map.GetType().Name;
                return Map(taguri, map, false);
            }

            byte[] bytes = data as byte[];
            if (bytes != null) {
                return Scalar("tag:yaml.org,2002:binary", Convert.ToBase64String(bytes), (char)0);
            }

            IList seq = data as IList;
            if (seq != null) {
                string taguri = seq is List<object>
                    ? "tag:yaml.org,2002:seq"
                    : "tag:yaml.org,2002:seq:" + seq.GetType().Name;
                return Sequence(taguri, seq, false);
            }

            ICollection set = data as ICollection;
            if (set != null) {
                Dictionary<object, object> entries = new Dictionary<object, object>(set.Count);
                foreach (object x in set) {
                    entries.Add(x, null);
                }
                return Map("tag:yaml.org,2002:set", entries, false);
            }

            PrivateType pt = data as PrivateType;
            if (pt != null) {
                Node n = RepresentData(pt.Value);
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
                    tag = "tag:yaml.org,2002:int";
                    value = data.ToString();
                    break;
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    tag = "tag:yaml.org,2002:float";
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
                    tag = "tag:yaml.org,2002:str";
                    value = data.ToString();
                    break;
                case TypeCode.DateTime:
                    DateTime date = (DateTime)data;
                    string format = (date.Millisecond != 0) ? "yyyy-MM-dd HH:mm:ss.SSS Z" : "yyyy-MM-dd HH:mm:ss Z";
                    value = date.ToString(format);
                    // TODO: what is this code for?
                    if (value.Length >= 23) {
                        value = value.Substring(0, 23) + ":" + value.Substring(23);
                    }
                    tag = "tag:yaml.org,2002:timestamp";
                    break;
                default:
                    return CreateNodeForObject(data);
            }

            return Scalar(tag, value, (char)0);
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
            return Map("!cli/object:" + data.GetType().Name, values, false);
        }
    }
}