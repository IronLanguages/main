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
using System.Text.RegularExpressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.StandardLibrary.Yaml {

    public delegate void RecursiveFixer(Node node, object real);
    public delegate object YamlConstructor(IConstructor self, Node node);
    public delegate object YamlMultiConstructor(IConstructor self, string pref, Node node);

    public interface IConstructor : IEnumerable<object> {
        bool CheckData();
        object GetData();
        object ConstructDocument(Node node);
        object ConstructObject(Node node);
        object ConstructPrimitive(Node node);
        object ConstructScalar(Node node);
        object ConstructPrivateType(Node node);
        RubyArray ConstructSequence(Node node);
        Hash ConstructMapping(Node node);
        object ConstructPairs(Node node);
        void DoRecursionFix(Node node, object obj);
        void AddFixer(Node node, RecursiveFixer fixer);

        RubyScope/*!*/ Scope { get; }
    }

    public class BaseConstructor : IConstructor {
        private readonly static Dictionary<string, YamlConstructor> _yamlConstructors = new Dictionary<string, YamlConstructor>();
        private readonly static Dictionary<string, YamlMultiConstructor> _yamlMultiConstructors = new Dictionary<string, YamlMultiConstructor>();
        private readonly static Dictionary<string, Regex> _yamlMultiRegexps = new Dictionary<string, Regex>();        
        
        private readonly Dictionary<Node, List<RecursiveFixer>>/*!*/ _recursiveObjects = new Dictionary<Node, List<RecursiveFixer>>();
        private readonly NodeProvider/*!*/ _nodeProvider;
        private readonly RubyScope/*!*/ _scope;

        public BaseConstructor(NodeProvider/*!*/ nodeProvider, RubyScope/*!*/ scope) {
            Assert.NotNull(nodeProvider, scope);
            _nodeProvider = nodeProvider;
            _scope = scope;
        }

        public RubyScope/*!*/ Scope {
            get { return _scope; }
        }

        public virtual YamlConstructor GetYamlConstructor(string key) {
            YamlConstructor result;
            _yamlConstructors.TryGetValue(key, out result);
            return result;
        }

        public virtual YamlMultiConstructor GetYamlMultiConstructor(string key) {
            YamlMultiConstructor result;
            _yamlMultiConstructors.TryGetValue(key, out result);
            return result;
        }

        public virtual Regex GetYamlMultiRegexp(string key) {
            Regex result;
            _yamlMultiRegexps.TryGetValue(key, out result);
            return result;
        }

        public virtual ICollection<string> GetYamlMultiRegexps() {
            return _yamlMultiRegexps.Keys;
        }

        public static void AddConstructor(string tag, YamlConstructor ctor) {
            _yamlConstructors.Add(tag,ctor);
        }

        public static void AddMultiConstructor(string tagPrefix, YamlMultiConstructor ctor) {
            _yamlMultiConstructors.Add(tagPrefix,ctor);
            _yamlMultiRegexps.Add(tagPrefix, new Regex("^" + tagPrefix, RegexOptions.Compiled));
        }

        public bool CheckData() {
            return _nodeProvider.CheckNode();
        }

        public object GetData() {
            if (_nodeProvider.CheckNode()) {
                Node node = _nodeProvider.GetNode();
                if(null != node) {
                    return ConstructDocument(node);
                }
            }
            return null;
        }

        public object ConstructDocument(Node node) {
            object data = ConstructObject(node);
            _recursiveObjects.Clear();
            return data;
        }

        private YamlConstructor yamlMultiAdapter(YamlMultiConstructor ctor, string prefix) {
            return delegate(IConstructor self, Node node) { return ctor(self, prefix, node); };
        }

        public static Node GetNullNode() {
            return new ScalarNode("tag:yaml.org,2002:null", null, (char)0);
        }

        public object ConstructObject(Node node) {
            if (node == null) {
                node = GetNullNode();
            }
            if(_recursiveObjects.ContainsKey(node)) {
                return new LinkNode(node);
            }
            _recursiveObjects.Add(node, new List<RecursiveFixer>());
            YamlConstructor ctor = GetYamlConstructor(node.Tag);
            if (ctor == null) {
                bool through = true;
                foreach (string tagPrefix in GetYamlMultiRegexps()) {
                    Regex reg = GetYamlMultiRegexp(tagPrefix);
                    if (reg.IsMatch(node.Tag)) {
                        string tagSuffix = node.Tag.Substring(tagPrefix.Length);
                        ctor = yamlMultiAdapter(GetYamlMultiConstructor(tagPrefix), tagSuffix);
                        through = false;
                        break;
                    }
                }
                if (through) {
                    YamlMultiConstructor xctor = GetYamlMultiConstructor("");
                    if(null != xctor) {
                        ctor = yamlMultiAdapter(xctor,node.Tag);
                    } else {
                        ctor = GetYamlConstructor("");
                        if(ctor == null) {
                            ctor = CONSTRUCT_PRIMITIVE;
                        }
                    }
                }
            }
            object data = ctor(this, node);
            DoRecursionFix(node,data);
            return data;
        }

        public void DoRecursionFix(Node node, object obj) {
            List<RecursiveFixer> ll;
            if (_recursiveObjects.TryGetValue(node, out ll)) {
                _recursiveObjects.Remove(node);
                foreach (RecursiveFixer fixer in ll) {
                    fixer(node, obj);
                }
            }
        }

        public object ConstructPrimitive(Node node) {
            if(node is ScalarNode) {
                return ConstructScalar(node);
            } else if(node is SequenceNode) {
                return ConstructSequence(node);            
            } else if(node is MappingNode) {
                return ConstructMapping(node);
            } else {
                Console.Error.WriteLine(node.Tag);
            }
            return null;
        }        

        public object ConstructScalar(Node node) {
            ScalarNode scalar = node as ScalarNode;
            if (scalar == null) {
                MappingNode mapNode = node as MappingNode;
                if (mapNode != null) {
                    foreach (KeyValuePair<Node, Node> entry in mapNode.Nodes) {
                        if ("tag:yaml.org,2002:value" == entry.Key.Tag) {
                            return ConstructScalar(entry.Value);
                        }
                    }
                }
                throw new ConstructorException("expected a scalar or mapping node, but found: " + node);
            }
            string value = scalar.Value;
            if (value.Length > 1 && value[0] == ':' && scalar.Style == '\0') {
                return SymbolTable.StringToId(value.Substring(1));
            }
            return value;
        }        

        public object ConstructPrivateType(Node node) {
            object val = null;
            ScalarNode scalar = node as ScalarNode;
            if (scalar != null) {
                val = scalar.Value;
            } else if (node is MappingNode) {
                val = ConstructMapping(node);
            } else if (node is SequenceNode) {
                val = ConstructSequence(node);
            } else {
                throw new ConstructorException("unexpected node type: " + node);
            }            
            return new PrivateType(node.Tag,val);
        }
        
        public RubyArray ConstructSequence(Node sequenceNode) {
            SequenceNode seq = sequenceNode as SequenceNode;
            if(seq == null) {
                throw new ConstructorException("expected a sequence node, but found: " + sequenceNode);
            }
            IList<Node> @internal = seq.Nodes;
            RubyArray val = new RubyArray(@internal.Count);
            foreach (Node node in @internal) {
                object obj = ConstructObject(node);
                LinkNode linkNode = obj as LinkNode;
                if (linkNode != null) {
                    int ix = val.Count;
                    AddFixer(linkNode.Linked, delegate (Node n, object real) {
                        val[ix] = real;
                    });
                }
                val.Add(obj);
            }
            return val;
        }

        //TODO: remove Ruby-specific stuff from this layer
        public Hash ConstructMapping(Node mappingNode) {
            MappingNode map = mappingNode as MappingNode;
            if (map == null) {
                throw new ConstructorException("expected a mapping node, but found: " + mappingNode);
            }
            Hash mapping = new Hash(_scope.RubyContext);
            LinkedList<Hash> merge = null;
            foreach (KeyValuePair<Node, Node> entry in map.Nodes) {
                Node key_v = entry.Key;
                Node value_v = entry.Value;

                if (key_v.Tag == "tag:yaml.org,2002:merge") {
                    if (merge != null) {
                        throw new ConstructorException("while constructing a mapping: found duplicate merge key");
                    }
                    SequenceNode sequence;
                    merge = new LinkedList<Hash>();
                    if (value_v is MappingNode) {
                        merge.AddLast(ConstructMapping(value_v));
                    } else if ((sequence = value_v as SequenceNode) != null) {
                        foreach (Node subNode in sequence.Nodes) {
                            if (!(subNode is MappingNode)) {
                                throw new ConstructorException("while constructing a mapping: expected a mapping for merging, but found: " + subNode);
                            }
                            merge.AddFirst(ConstructMapping(subNode));
                        }
                    } else {
                        throw new ConstructorException("while constructing a mapping: expected a mapping or list of mappings for merging, but found: " + value_v);
                    }
                } else if (key_v.Tag == "tag:yaml.org,2002:value") {
                    if(mapping.ContainsKey("=")) {
                        throw new ConstructorException("while construction a mapping: found duplicate value key");
                    }
                    mapping.Add("=", ConstructObject(value_v));
                } else {
                    object kk = ConstructObject(key_v);
                    object vv = ConstructObject(value_v);
                    LinkNode linkNode = vv as LinkNode;
                    if (linkNode != null) {
                        AddFixer(linkNode.Linked, delegate (Node node, object real) {
                            IDictionaryOps.SetElement(_scope.RubyContext, mapping, kk, real);
                        });
                    }
                    IDictionaryOps.SetElement(_scope.RubyContext, mapping, kk, vv);
                }
            }
            if (null != merge) {
                merge.AddLast(mapping);
                mapping = new Hash(_scope.RubyContext);
                foreach (Hash m in merge) {                    
                    foreach (KeyValuePair<object, object> e in m) {
                        IDictionaryOps.SetElement(_scope.RubyContext, mapping, e.Key, e.Value);
                    }
                }
            }
            return mapping;
        }

        public void AddFixer(Node node, RecursiveFixer fixer) {
            List<RecursiveFixer> ll;
            if (!_recursiveObjects.TryGetValue(node, out ll)) {
                _recursiveObjects.Add(node, ll = new List<RecursiveFixer>());
            }
            ll.Add(fixer);
        }

        public object ConstructPairs(Node mappingNode) {
            MappingNode map = mappingNode as MappingNode;
            if (map == null) {
                throw new ConstructorException("expected a mapping node, but found: " + mappingNode);
            }

            List<object[]> result = new List<object[]>();
            foreach (KeyValuePair<Node, Node> entry in map.Nodes) {
                result.Add(new object[] { ConstructObject(entry.Key), ConstructObject(entry.Value) });
            }
            return result;
        }

        public static YamlConstructor CONSTRUCT_PRIMITIVE = delegate(IConstructor self, Node node) { return self.ConstructPrimitive(node); };
        public static YamlConstructor CONSTRUCT_SCALAR = delegate(IConstructor self, Node node) { return self.ConstructScalar(node); };
        public static YamlConstructor CONSTRUCT_PRIVATE = delegate(IConstructor self, Node node) { return self.ConstructPrivateType(node); };
        public static YamlConstructor CONSTRUCT_SEQUENCE = delegate(IConstructor self, Node node) { return self.ConstructSequence(node); };
        public static YamlConstructor CONSTRUCT_MAPPING = delegate(IConstructor self, Node node) { return self.ConstructMapping(node); };

        #region IEnumerable<object> Members

        public IEnumerator<object> GetEnumerator() {
            while (CheckData()) {
                yield return GetData();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }
}
