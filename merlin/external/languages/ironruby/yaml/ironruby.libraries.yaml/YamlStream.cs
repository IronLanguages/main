/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using IronRuby.Builtins;

namespace IronRuby.StandardLibrary.Yaml {
    
    /// <summary>
    /// YAML stream - a collection of YAML documents.
    /// </summary>
    public class YamlStream {
        private Hash _options;
        private RubyArray _documents;

        public YamlStream(RubyClass/*!*/ rubyClass) 
            : this(new Hash(rubyClass.Context)) { 
        }

        public YamlStream(Hash/*!*/ options) {
            _options = options;
            _documents = new RubyArray();
        }

        public RubyArray Documents {
            get { return _documents; }
            set { _documents = value; }
        }

        public Hash Options {
            get { return _options; }
            set { _options = value; }
        }                               
    }
}