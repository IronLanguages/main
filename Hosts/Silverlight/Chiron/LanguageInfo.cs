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
using System.Configuration;
using System.Xml;
using System.Reflection;
using System.Text;

namespace Chiron {
    class LanguageInfo {
        internal readonly string[] Extensions;
        internal readonly string[] Assemblies;
        internal readonly string LanguageContext;
        internal readonly string[] Names;
        internal readonly string External;

        public LanguageInfo(string[] extensions, string[] assemblies, string languageContext, string[] names, string external) {
            Extensions = extensions;
            Assemblies = assemblies;
            LanguageContext = languageContext;
            Names = names;
            External = external;
        }

        public string GetContextAssemblyName() {
            return AssemblyName.GetAssemblyName(Chiron.TryGetAssemblyPath(Assemblies[0])).FullName;
        }

        public string GetExtensionsString() {
            StringBuilder str = new StringBuilder();
            foreach (string ext in Extensions) {
                if (str.Length > 0) str.Append(",");
                if (ext.StartsWith(".")) str.Append(ext);
                else str.Append(ext + ",." + ext);
            }
            return str.ToString();
        }

        public string GetNames() {
            StringBuilder str = new StringBuilder();
            foreach (string n in Names) {
                if (str.Length > 0) str.Append(",");
                str.Append(n);
            }
            return str.ToString();
        }

        public string GetAssemblyNames() {
            StringBuilder str = new StringBuilder();
            foreach (string asm in Assemblies) {
                if (str.Length > 0) str.Append(";");
                str.Append(asm);
            }
            return str.ToString();
        }
    }

    public class LanguageSection : IConfigurationSectionHandler {
        public object Create(object parent, object configContext, XmlNode section) {
            Dictionary<string, LanguageInfo> languages = new Dictionary<string, LanguageInfo>();
            char[] splitChars = new char[] { ' ', '\t', ',', ';', '\r', '\n' };

            foreach (XmlElement elem in ((XmlElement)section).GetElementsByTagName("Language")) {
                var external = elem.GetAttribute("external");
                if (Chiron.UrlPrefix != null) {
                    external = string.Format("{0}{1}", Chiron.UrlPrefix, external);
                }

                LanguageInfo info = new LanguageInfo(
                    elem.GetAttribute("extensions").Split(splitChars, StringSplitOptions.RemoveEmptyEntries),
                    elem.GetAttribute("assemblies").Split(splitChars, StringSplitOptions.RemoveEmptyEntries),
                    elem.GetAttribute("languageContext"),
                    elem.GetAttribute("names").Split(splitChars, StringSplitOptions.RemoveEmptyEntries),
                    external
                );

                foreach (string ext in info.Extensions) {
                    var _ext = ext;
                    if(!_ext.StartsWith(".")) _ext = "." + _ext.ToLower();
                    languages[_ext] = info;
                }


            }

            return languages;
        }
    }
}
