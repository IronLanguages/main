/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using System.Collections;
using System.Collections.Generic;

namespace IronRuby.StandardLibrary.Yaml {

    public class RubyRepresenter : Representer {
        private readonly RubyContext/*!*/ _context;

        private RubyMemberInfo _objectToYamlMethod;

        public RubyContext/*!*/ Context {
            get { return _context; }
        }

        public RubyRepresenter(RubyContext/*!*/ context, Serializer/*!*/ serializer, YamlOptions/*!*/ opts)
            : base(serializer, opts) {
            _context = context;
            _objectToYamlMethod = context.GetClass(typeof(object)).ResolveMethod("to_yaml", RubyClass.IgnoreVisibility).Info;
        }

        #region dynamic sites

        private readonly CallSite<Func<CallSite, RubyContext, object, MutableString>> _TagUri = 
            CallSite<Func<CallSite, RubyContext, object, MutableString>>.Create(
            RubyCallAction.Make("taguri", RubyCallSignature.WithImplicitSelf(0))
        );

        private readonly CallSite<Func<CallSite, RubyContext, object, MutableString>> _ToYamlStyle =
            CallSite<Func<CallSite, RubyContext, object, MutableString>>.Create(
            RubyCallAction.Make("to_yaml_style", RubyCallSignature.WithImplicitSelf(0))
        );

        private readonly CallSite<Func<CallSite, RubyContext, object, RubyRepresenter, Node>> _ToYamlNode = 
            CallSite<Func<CallSite, RubyContext, object, RubyRepresenter, Node>>.Create(
            RubyCallAction.Make("to_yaml_node", RubyCallSignature.WithImplicitSelf(1))
        );

        private readonly CallSite<Func<CallSite, RubyContext, object, RubyRepresenter, Node>> _ToYaml =
            CallSite<Func<CallSite, RubyContext, object, RubyRepresenter, Node>>.Create(
            RubyCallAction.Make("to_yaml", RubyCallSignature.WithImplicitSelf(0))
        );

        private CallSite<Func<CallSite, RubyContext, object, RubyArray>> _ToYamlProperties = 
            CallSite<Func<CallSite, RubyContext, object, RubyArray>>.Create(
            RubyCallAction.Make("to_yaml_properties", RubyCallSignature.WithImplicitSelf(0))
        );

        internal MutableString GetTagUri(object obj) {
            return _TagUri.Target(_TagUri, _context, obj);
        }

        internal MutableString ToYamlStyle(object obj) {
            return _ToYamlStyle.Target(_ToYamlStyle, _context, obj);
        }

        internal RubyArray ToYamlProperties(object obj) {
            return _ToYamlProperties.Target(_ToYamlProperties, _context, obj);
        }

        #endregion

        protected override Node CreateNode(object data) {
            RubyMemberInfo method = _context.GetImmediateClassOf(data).ResolveMethodForSite("to_yaml", RubyClass.IgnoreVisibility).Info;

            if (method == _objectToYamlMethod) {
                return _ToYamlNode.Target(_ToYamlNode, _context, data, this);
            } else {
                // TODO: this is not correct:
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
            MutableString taguri = GetTagUri(self);
            MutableString styleStr = ToYamlStyle(self);

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

        internal Node/*!*/ Map(object self, IDictionary/*!*/ map) {
            MutableString taguri = GetTagUri(self);
            object style = ToYamlStyle(self);

            return Map(
                taguri != null ? taguri.ConvertToString() : "",
                map,
                RubyOps.IsTrue(style)
            );
        }

        internal Node Sequence(object self, RubyArray seq) {
            MutableString taguri = GetTagUri(self);
            MutableString style = ToYamlStyle(self);

            return Sequence(
                taguri != null ? taguri.ConvertToString() : "",
                seq,
                style != null
            );
        }


        internal void AddYamlProperties(object self, Dictionary<MutableString, object>/*!*/ map) {
            AddYamlProperties(self, map, ToYamlProperties(self));
        }

        internal void AddYamlProperties(object self, Dictionary<MutableString, object>/*!*/ map, RubyArray/*!*/ props) {
            foreach (object prop in props) {
                string p = prop.ToString();
                map[MutableString.Create(p.Substring(1))] = KernelOps.InstanceVariableGet(_context, self, p);
            }
        }
    }
}