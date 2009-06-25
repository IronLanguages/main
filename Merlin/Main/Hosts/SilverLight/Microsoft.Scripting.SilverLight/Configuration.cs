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

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Silverlight {
    public class Configuration {

        private static string _languagesConfigFile = "languages.config";

        public static ScriptRuntimeSetup LoadFromAssemblies(IEnumerable<Assembly> assemblies) {
            var setup = new ScriptRuntimeSetup();
            foreach (var assembly in assemblies) {
                foreach (DynamicLanguageProviderAttribute attribute in assembly.GetCustomAttributes(typeof(DynamicLanguageProviderAttribute), false)) {
                    setup.LanguageSetups.Add(new LanguageSetup(
                        attribute.LanguageContextType.AssemblyQualifiedName,
                        attribute.DisplayName,
                        attribute.Names,
                        attribute.FileExtensions
                    ));
                }
            }
            return setup;
        }

        public static ScriptRuntimeSetup TryParseFile() {
            Stream configFile = Package.GetFile(_languagesConfigFile);
            if (configFile == null) {
                return null;
            }

            var result = new ScriptRuntimeSetup();
            try {
                XmlReader reader = XmlReader.Create(configFile);
                reader.MoveToContent();
                if (!reader.IsStartElement("Languages")) {
                    throw new ConfigFileException("expected 'Configuration' root element", _languagesConfigFile);
                }

                while (reader.Read()) {
                    if (reader.NodeType != XmlNodeType.Element || reader.Name != "Language") {
                        continue;
                    }
                    string context = null, assembly = null, exts = null;
                    while (reader.MoveToNextAttribute()) {
                        switch (reader.Name) {
                            case "languageContext":
                                context = reader.Value;
                                break;
                            case "assembly":
                                assembly = reader.Value;
                                break;
                            case "extensions":
                                exts = reader.Value;
                                break;
                        }
                    }

                    if (context == null || assembly == null || exts == null) {
                        throw new ConfigFileException("expected 'Language' element to have attributes 'languageContext', 'assembly', 'extensions'", _languagesConfigFile);
                    }

                    string[] extensions = exts.Split(',');
                    result.LanguageSetups.Add(new LanguageSetup(context + ", " + assembly, String.Empty, extensions, extensions));
                }
            } catch (ConfigFileException cfe) {
                throw cfe;
            } catch (Exception ex) {
                throw new ConfigFileException(ex.Message, _languagesConfigFile, ex);
            }

            return result;
        }

        // an exception parsing the host configuration file
        public class ConfigFileException : Exception {
            public ConfigFileException(string msg, string configFile)
                : this(msg, configFile, null) {
            }
            public ConfigFileException(string msg, string configFile, Exception inner)
                : base("Invalid configuration file " + configFile + ": " + msg, inner) {
            }

        }

    }
}
