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
using IronRuby.StandardLibrary.Yaml;
using System.Collections.Generic;
using System.Text;

namespace IronRuby.StandardLibrary.Yaml {
    /// <summary>
    /// Provides YAML nodes for Ruby object constructor.
    /// </summary>
    public abstract class NodeProvider : IEnumerable<Node> {
        public abstract bool CheckNode();
        public abstract Node GetNode();
        public abstract Encoding Encoding { get; }

        #region IEnumerable<Node> Members

        public IEnumerator<Node> GetEnumerator() {
            while (CheckNode()) {
                yield return GetNode();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// Simple NodeProvider implementation. Provides only one given node.
    /// </summary>
    public class SimpleNodeProvider : NodeProvider {
        private Node _node;
        private readonly Encoding/*!*/ _encoding;

        public SimpleNodeProvider(Node/*!*/ node, Encoding/*!*/ encoding) {
            _node = node;
            _encoding = encoding;
        }        

        public override bool CheckNode() {
            return _node != null;
        }

        public override Encoding/*!*/ Encoding {
            get { return _encoding; }
        }

        public override Node GetNode() {
            if (CheckNode()) {
                Node tmp = _node;
                _node = null;
                return tmp;
            } else {
                return null;
            }            
        }              
    }
}
