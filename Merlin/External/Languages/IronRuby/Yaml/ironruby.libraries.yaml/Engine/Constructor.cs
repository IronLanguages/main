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

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Yaml {

    public class Constructor : SafeConstructor {
        private readonly static Dictionary<string, YamlConstructor> _yamlConstructors = new Dictionary<string, YamlConstructor>();
        private readonly static Dictionary<string, YamlMultiConstructor> _yamlMultiConstructors = new Dictionary<string, YamlMultiConstructor>();
        private readonly static Dictionary<string, Regex> _yamlMultiRegexps = new Dictionary<string, Regex>();

        public override YamlConstructor GetYamlConstructor(string key) {
            YamlConstructor result;
            if (_yamlConstructors.TryGetValue(key, out result)) {
                return result;
            }
            return base.GetYamlConstructor(key);
        }

        public override YamlMultiConstructor GetYamlMultiConstructor(string key) {
            YamlMultiConstructor result;
            if (_yamlMultiConstructors.TryGetValue(key, out result)) {
                return result;
            }
            return base.GetYamlMultiConstructor(key);
        }

        public override Regex GetYamlMultiRegexp(string key) {
            Regex result;
            if (_yamlMultiRegexps.TryGetValue(key, out result)) {
                return result;
            }
            return base.GetYamlMultiRegexp(key);
        }

        public override ICollection<string> GetYamlMultiRegexps() {
            return _yamlMultiRegexps.Keys;
        }

        public new static void AddConstructor(string tag, YamlConstructor ctor) {
            _yamlConstructors.Add(tag, ctor);
        }

        public new static void AddMultiConstructor(string tagPrefix, YamlMultiConstructor ctor) {
            _yamlMultiConstructors.Add(tagPrefix, ctor);
            _yamlMultiRegexps.Add(tagPrefix, new Regex("^" + tagPrefix, RegexOptions.Compiled));
        }

        public Constructor(NodeProvider/*!*/ nodeProvider, RubyGlobalScope/*!*/ scope)
            : base(nodeProvider, scope) {
        }

    }
}