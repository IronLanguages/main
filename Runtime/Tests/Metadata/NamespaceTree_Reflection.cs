/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Scripting.Metadata;
using Microsoft.Scripting.Utils;

namespace Metadata {
    public sealed class RNamespaceTreeNode {
        private readonly string _name;
        private List<Type> _typeDefs; // TODO: Use Sequence<TypeDef> (typedefs from the same assembly per chunk/assemblies index)
        private RNamespaceTreeNode _lastChild;
        private RNamespaceTreeNode _firstChild;
        private RNamespaceTreeNode _nextSibling;

        internal RNamespaceTreeNode(string name) {
            _name = name;
        }

        internal void AddType(Type typeDef) {
            if (_typeDefs == null) {
                _typeDefs = new List<Type>();
            }
            _typeDefs.Add(typeDef);
        }

        internal void AddNamespace(RNamespaceTreeNode ns) {
            ContractUtils.Assert(ns != null && ns._nextSibling == null);
            ContractUtils.Assert((_firstChild == null) == (_lastChild == null));

            if (_firstChild == null) {
                // our first child:
                _firstChild = _lastChild = ns;
            } else {
                // add to the end of the children linked list:
                _lastChild._nextSibling = ns;
                _lastChild = ns;
            }
            
        }

        public string Name {
            get { return _name; }
        }

        public IEnumerable<Type> GetTypeDefs() {
            if (_typeDefs != null) {
                for (int i = 0; i < _typeDefs.Count; i++) {
                    yield return _typeDefs[i];
                }
            }
        }

        public IEnumerable<RNamespaceTreeNode> GetNamespaces() {
            RNamespaceTreeNode current = _firstChild;
            while (current != null) {
                yield return current;
                current = current._nextSibling;
            }
        }

        /// <summary>
        /// Merges given node into this node. Removes the child nodes from the other node.
        /// </summary>
        public void Merge(RNamespaceTreeNode other) {
            ContractUtils.Requires(other != null);
            
            if (other._typeDefs != null) {
                _typeDefs.AddRange(other._typeDefs);
                other._typeDefs = null;
            }

            if (other._firstChild != null) {
                ContractUtils.Assert(other._lastChild != null);
                if (_firstChild == null) {
                    // this namespace has no subnamespaces:
                    _firstChild = other._firstChild;
                    _lastChild = other._lastChild;
                } else {
                    // concat the lists:
                    _lastChild._nextSibling = other._firstChild;
                    _lastChild = other._lastChild;
                }
                other._firstChild = other._lastChild = null;
            }
        }
    }

    public sealed class RNamespaceTree {
        // Maps every prefix of every namespace name to the corresponding namespace:
        private readonly Dictionary<string, RNamespaceTreeNode> _names;
        private readonly RNamespaceTreeNode _root;

        public RNamespaceTree() {
            _names = new Dictionary<string, RNamespaceTreeNode>();
            _names.Add("", _root = new RNamespaceTreeNode(""));
        }

        public RNamespaceTreeNode Root {
            get { return _root; }
        }

        public IEnumerable<RNamespaceTreeNode> GetAllNamespaces() {
            return _names.Values;
        }

        public void Add(Module module) {
            ContractUtils.Requires(module != null);

            Type[] types;
            try {
                types = module.GetTypes();
            } catch (Exception) {
                Console.WriteLine(module.Assembly.Location);
                return;
            }

            foreach (Type type in types) {
                if (type.Attributes.IsNested()) {
                    continue;
                }

                string prefix = type.Namespace ?? "";
                RNamespaceTreeNode ns = null;

                while (true) {
                    RNamespaceTreeNode existing;
                    if (_names.TryGetValue(prefix, out existing)) {
                        if (ns == null) {
                            existing.AddType(type);
                        } else {
                            existing.AddNamespace(ns);
                        }
                        break;
                    }

                    ContractUtils.Assert(prefix.Length > 0);
                    int lastDot = prefix.LastIndexOf('.', prefix.Length - 1, prefix.Length);
                    
                    string name = (lastDot >= 0) ? prefix.Substring(lastDot + 1) : prefix;
                    RNamespaceTreeNode newNs = new RNamespaceTreeNode(name);
                    if (ns == null) {
                        newNs.AddType(type);
                    } else {
                        newNs.AddNamespace(ns);
                    }
                    ns = newNs;

                    _names.Add(prefix, ns);

                    prefix = (lastDot >= 0) ? prefix.Substring(0, lastDot) : "";
                }
            }
        }
    }
}
