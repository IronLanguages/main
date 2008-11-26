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
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.Yaml {

    public class RubyRepresenter : Representer {
        private readonly RubyContext/*!*/ _context;
        private RubyMemberInfo _objectToYamlMethod;

        public RubyContext/*!*/ Context {
            get { return _context; }
        }

        public RubyRepresenter(RubyContext/*!*/ context, ISerializer/*!*/ serializer, YamlOptions/*!*/ opts)
            : base(serializer, opts) {
            _context = context;
            _objectToYamlMethod = context.GetClass(typeof(object)).ResolveMethod("to_yaml", false);
        }

        #region dynamic sites

        private static readonly CallSite<Func<CallSite, RubyContext, object, MutableString>> _TagUri = CallSite<Func<CallSite, RubyContext, object, MutableString>>.Create(LibrarySites.InstanceCallAction("taguri"));
        private static readonly CallSite<Func<CallSite, RubyContext, object, object>> _ToYamlStyle = CallSite<Func<CallSite, RubyContext, object, object>>.Create(LibrarySites.InstanceCallAction("to_yaml_style"));
        private static readonly CallSite<Func<CallSite, RubyContext, object, RubyRepresenter, Node>> _ToYamlNode = CallSite<Func<CallSite, RubyContext, object, RubyRepresenter, Node>>.Create(LibrarySites.InstanceCallAction("to_yaml_node", 1));
        private static readonly CallSite<Func<CallSite, RubyContext, object, RubyRepresenter, Node>> _ToYaml = CallSite<Func<CallSite, RubyContext, object, RubyRepresenter, Node>>.Create(LibrarySites.InstanceCallAction("to_yaml", 1));
        private static CallSite<Func<CallSite, RubyContext, object, RubyArray>> _ToYamlProperties = CallSite<Func<CallSite, RubyContext, object, RubyArray>>.Create(LibrarySites.InstanceCallAction("to_yaml_properties"));

        internal static object ToYamlStyle(RubyContext/*!*/ context, object self) {
            return _ToYamlStyle.Target(_ToYamlStyle, context, self);
        }

        internal static RubyArray ToYamlProperties(RubyContext/*!*/ context, object self) {
            return _ToYamlProperties.Target(_ToYamlProperties, context, self);
        }

        internal static MutableString TagUri(RubyContext/*!*/ context, object self) {
            return _TagUri.Target(_TagUri, context, self);
        }

        #endregion

        protected override Node CreateNode(object data) {
            RubyMemberInfo method = _context.ResolveMethod(data, "to_yaml", false).InvalidateSitesOnOverride();

            if (method == _objectToYamlMethod) {
                return _ToYamlNode.Target(_ToYamlNode, _context, data, this);
            } else {
                // TODO: this doesn't seem right
                // (we're passing the extra argument, but the callee might not take it?)
                return _ToYaml.Target(_ToYaml, _context, data, this);
            }
        }

        protected override bool IgnoreAliases(object data) {
 	         return RubyUtils.IsRubyValueType(data) || base.IgnoreAliases(data);
        }

        internal Node Scalar(MutableString taguri, MutableString value, SymbolId style) {
            return Scalar(
                taguri != null ? taguri.ConvertToString() : "",
                value != null ? value.ConvertToString() : "",
                //It's not clear what style argument really means, it seems to be always :plain
                //for now we are ignoring it, defaulting to \0 (see Representer class)
                '\0'
            );
        }

        internal Node Scalar(object self, MutableString value) {
            MutableString taguri = _TagUri.Target(_TagUri, _context, self);
            MutableString styleStr = ToYamlStyle(_context, self) as MutableString;

            char style = '\0';
            if (!MutableString.IsNullOrEmpty(styleStr)) {
                style = styleStr.GetChar(0);
            }

            return Scalar(
                taguri != null ? taguri.ConvertToString() : "",
                value != null ? value.ConvertToString() : "",
                style
            );
        }

        internal Node Map(object self, Hash map) {
            MutableString taguri = _TagUri.Target(_TagUri, _context, self);
            object style = _ToYamlStyle.Target(_ToYamlStyle, _context, self);

            return Map(
                taguri != null ? taguri.ConvertToString() : "",
                map,
                RubyOps.IsTrue(style)
            );
        }

        internal Node Sequence(object self, RubyArray seq) {
            MutableString taguri = _TagUri.Target(_TagUri, _context, self);
            object style = _ToYamlStyle.Target(_ToYamlStyle, _context, self);

            return Sequence(
                taguri != null ? taguri.ConvertToString() : "",
                seq,
                RubyOps.IsTrue(style)
            );
        }


        internal static void AddYamlProperties(RubyContext/*!*/ context, object self, Hash map) {
            AddYamlProperties(context, self, map, ToYamlProperties(context, self));
        }

        internal static void AddYamlProperties(RubyContext/*!*/ context, object self, Hash map, RubyArray props) {
            foreach (object prop in props) {
                string p = prop.ToString();
                IDictionaryOps.SetElement(
                    context, 
                    map, 
                    MutableString.Create(p.Substring(1)),
                    KernelOps.InstanceVariableGet(context, self, p)
                );
            }
        }
    }
}