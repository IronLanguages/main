/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
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
using Microsoft.Scripting.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace IronRuby.StandardLibrary.Yaml {
    public sealed class RubyRepresenter : Representer {
        private readonly YamlCallSiteStorage/*!*/ _siteStorage;

        internal RubyContext/*!*/ Context {
            get { return _siteStorage.Context; }
        }

        internal RubyRepresenter(YamlCallSiteStorage/*!*/ siteStorage) {
            _siteStorage = siteStorage;
        }

        #region TagUri, ToYamlStyle

        internal string GetTagUri(object obj) {
            return GetTagUri(obj, null, null);
        }

        internal string GetTagUri(object obj, string defaultTag, Type defaultTagType) {
            var site = _siteStorage.TagUri;
            return ToTag(site.Target(site, obj), obj, defaultTag, defaultTagType);
        }

        internal string ToTag(object tagUri) {
            return ToTag(tagUri, null, null, null);
        }

        private string ToTag(object tagUri, object obj, string defaultTag, Type defaultTagType) {
            if (tagUri == null) {
                return String.Empty;
            }

            // TODO: any conversions?
            var mstr = tagUri as MutableString;
            if (mstr == null) {
                throw RubyExceptions.CreateUnexpectedTypeError(Context, tagUri, "String");
            }

            // TODO: MRI seems to save binary representation of the returned value, 
            // yet there is no encoding information saved along. So the reader would read meaningless binary data.
            if (!mstr.IsAscii()) {
                throw new NotSupportedException("Non-ASCII tags not supported");
            }
            
            string tag = mstr.ToString();
            return (tag != defaultTag || obj.GetType() != defaultTagType) ? tag : null;
        }

        internal ScalarQuotingStyle GetYamlStyle(object obj) {
            var site = _siteStorage.ToYamlStyle;
            return RubyYaml.ToYamlStyle(Context, site.Target(site, obj));
        }

        #endregion

        #region Node Construction

        protected override bool HasIdentity(object data) {
            return RubyUtils.IsRubyValueType(data) || base.HasIdentity(data);
        }

        protected override Node/*!*/ CreateNode(object data) {
            RubyMemberInfo method = Context.GetImmediateClassOf(data).ResolveMethodForSite("to_yaml", VisibilityContext.AllVisible).Info;
            if (method == _siteStorage.ObjectToYamlMethod) {
                var site = _siteStorage.ToYamlNode;
                return ToNode(site.Target(site, data, this));
            } else {
                var site = _siteStorage.ToYaml;
                return ToNode(site.Target(site, data, this));
            }
        }

        internal Node/*!*/ ToNode(object obj) {
            Node node = obj as Node;
            if (node == null) {
                throw RubyExceptions.CreateUnexpectedTypeError(Context, obj, "YAML node");
            }
            return node;
        }

        internal Node/*!*/ Map(object obj, IDictionary/*!*/ map) {
            return Map(
                GetTagUri(obj, Tags.Map, typeof(Hash)), 
                map, 
                GetYamlStyle(obj)
            );
        }

        internal Node/*!*/ Map(string tag, IDictionary/*!*/ map, ScalarQuotingStyle style) {
            return Map(tag, map, style != ScalarQuotingStyle.None ? FlowStyle.Inline : FlowStyle.Block);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Dynamically calls to_yaml_properties on a given object. The method should return a list of 
        /// instance variable names (Symbols or Strings) of the object that should be serialized into the output stream.
        /// </summary>
        internal IList/*!*/ ToYamlProperties(object obj) {
            var site = _siteStorage.ToYamlProperties;
            var props = site.Target(site, obj) as IList;
            if (props == null) {
                throw RubyExceptions.CreateTypeError("to_yaml_properties must return an array");
            }
            return props;
        }

        internal void AddYamlProperties(Dictionary<object, object>/*!*/ propertyMap, object obj, bool plainNames) {
            AddYamlProperties(propertyMap, obj, ToYamlProperties(obj), plainNames);
        }

        internal void AddYamlProperties(Dictionary<object, object>/*!*/ propertyMap, object obj, IList/*!*/ instanceVariableNames, bool plainNames) {
            foreach (object name in instanceVariableNames) {
                RubySymbol symbol;
                MutableString mstr;

                // MRI doesn't use a dynamic conversion:
                if ((symbol = name as RubySymbol) != null) {
                    propertyMap[plainNames ? (object)symbol.GetSlice(1) : symbol] = KernelOps.InstanceVariableGet(Context, obj, symbol.ToString());
                } else if ((mstr = name as MutableString) != null) {
                    propertyMap[plainNames ? mstr.GetSlice(1) : mstr] = KernelOps.InstanceVariableGet(Context, obj, mstr.ToString());
                } else {
                    throw RubyExceptions.CreateTypeError("unexpected type {0}, expected Symbol or String", Context.GetClassDisplayName(name));
                }
            }
        }

        internal static string/*!*/ ConvertToFieldName(RubyContext/*!*/ context, object name) {
            // MRI doesn't use a dynamic conversion:
            if (name is RubySymbol || name is MutableString) {
                return name.ToString();
            } else {
                throw RubyExceptions.CreateTypeError("unexpected type {0}, expected Symbol or String", context.GetClassDisplayName(name));
            }
        }

        #endregion
    }
}