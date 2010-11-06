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
    public sealed class NamespaceTreeNode {
        private readonly MetadataNamePart _name;
        private List<TypeDef> _typeDefs; // TODO: Use Sequence<TypeDef> (typedefs from the same assembly per chunk/assemblies index)
        private NamespaceTreeNode _lastChild;
        private NamespaceTreeNode _firstChild;
        private NamespaceTreeNode _nextSibling;

        internal NamespaceTreeNode(MetadataNamePart name) {
            _name = name;
        }

        internal void AddType(TypeDef typeDef) {
            if (_typeDefs == null) {
                _typeDefs = new List<TypeDef>();
            }
            _typeDefs.Add(typeDef);
        }

        internal void AddNamespace(NamespaceTreeNode ns) {
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

        public MetadataNamePart Name {
            get { return _name; }
        }

        public IEnumerable<TypeDef> GetTypeDefs() {
            if (_typeDefs != null) {
                for (int i = 0; i < _typeDefs.Count; i++) {
                    yield return _typeDefs[i];
                }
            }
        }

        public IEnumerable<NamespaceTreeNode> GetNamespaces() {
            NamespaceTreeNode current = _firstChild;
            while (current != null) {
                yield return current;
                current = current._nextSibling;
            }
        }

        /// <summary>
        /// Merges given node into this node. Removes the child nodes from the other node.
        /// </summary>
        public void Merge(NamespaceTreeNode other) {
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
    
    public sealed class NamespaceTree {
        // Maps every prefix of every namespace name to the corresponding namespace:
        private readonly Dictionary<MetadataNamePart, NamespaceTreeNode> _names;
        private readonly NamespaceTreeNode _root;

        public NamespaceTree() {
            _names = new Dictionary<MetadataNamePart, NamespaceTreeNode>();
            _names.Add(MetadataNamePart.Empty, _root = new NamespaceTreeNode(MetadataNamePart.Empty));
        }

        public NamespaceTreeNode Root {
            get { return _root; }
        }

        public IEnumerable<NamespaceTreeNode> GetAllNamespaces() {
            return _names.Values;
        }

        private static byte[] _Module = Encoding.UTF8.GetBytes("<Module>");

        public void Add(MetadataTables tables) {
            ContractUtils.Requires(tables != null);
            
            foreach (TypeDef typeDef in tables.TypeDefs) {
                if (typeDef.IsGlobal || typeDef.Attributes.IsNested()) {
                    continue;
                }

                MetadataNamePart prefix = typeDef.Namespace.GetExtent();
                NamespaceTreeNode ns = null;

                while (true) {
                    NamespaceTreeNode existing;
                    if (_names.TryGetValue(prefix, out existing)) {
                        if (ns == null) {
                            existing.AddType(typeDef);
                        } else {
                            existing.AddNamespace(ns);
                        }
                        break;
                    }


                    ContractUtils.Assert(prefix.Length > 0);
                    int lastDot = prefix.LastIndexOf((byte)'.', prefix.Length - 1, prefix.Length);
                    
                    MetadataNamePart name = (lastDot >= 0) ? prefix.GetPart(lastDot + 1) : prefix;
                    NamespaceTreeNode newNs = new NamespaceTreeNode(name);
                    if (ns == null) {
                        newNs.AddType(typeDef);
                    } else {
                        newNs.AddNamespace(ns);
                    }
                    ns = newNs;

                    _names.Add(prefix, ns);

                    prefix = (lastDot >= 0) ? prefix.GetPart(0, lastDot) : MetadataNamePart.Empty;
                }
            }
        }
    }
}
