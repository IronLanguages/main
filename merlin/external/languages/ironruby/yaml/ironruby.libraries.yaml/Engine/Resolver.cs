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
using System.Text;

namespace IronRuby.StandardLibrary.Yaml {
    // TODO: the rest of the ResolverImpl class didn't seem to do anything useful
    public static class Resolver {
        public static string Resolve(Type kind, string value, bool[] @implicit) {
            return Resolve(kind, value, @implicit[0]);
        }

        public static string Resolve(Type kind, string value, bool @implicit) {
            if (kind == typeof(ScalarNode) && @implicit) {
                string resolv = ResolverScanner.Recognize(value);
                if (resolv != null) {
                    return resolv;
                }
            }
            if (kind == typeof(ScalarNode)) {
                return "tag:yaml.org,2002:str";
            } else if (kind == typeof(SequenceNode)) {
                return "tag:yaml.org,2002:seq";
            } else if (kind == typeof(MappingNode)) {
                return "tag:yaml.org,2002:map";
            }
            return null;
        }
    }
}
