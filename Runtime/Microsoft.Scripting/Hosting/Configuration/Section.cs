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

#if !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting.Configuration {

    //
    // <configSections>
    //   <section name="microsoft.scripting" type="Microsoft.Scripting.Hosting.Configuration.Section, Microsoft.Scripting" />
    // </configSections>
    //
    // <microsoft.scripting [debugMode="{bool}"]? [privateBinding="{bool}"]?>
    //   <languages>  <!-- BasicMap with key (type): inherits language nodes, overwrites previous nodes (last wins) -->
    //     <language names="{(semi)colon-separated}" extensions="{(semi)colon-separated-with-optional-dot}" type="{AQTN}" [displayName="{string}"]? />
    //   </languages>
    //
    //   <options>    <!-- AddRemoveClearMap with key (option, [language]?): inherits language nodes, overwrites previous nodes (last wins) -->
    //     <set option="{string}" value="{string}" [language="{language-name}"]? />
    //     <clear />
    //     <remove option="{string}" [language="{language-name}"]? />
    //   </options>
    //
    // </microsoft.scripting>
    //
    public class Section : ConfigurationSection {
        public static readonly string SectionName = "microsoft.scripting";

        private const string _DebugMode = "debugMode";
        private const string _PrivateBinding = "privateBinding";
        private const string _Languages = "languages";
        private const string _Options = "options";

        private static ConfigurationPropertyCollection _Properties = new ConfigurationPropertyCollection() {
            new ConfigurationProperty(_DebugMode, typeof(bool?), null), 
            new ConfigurationProperty(_PrivateBinding, typeof(bool?), null), 
            new ConfigurationProperty(_Languages, typeof(LanguageElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection), 
            new ConfigurationProperty(_Options, typeof(OptionElementCollection), null), 
        };

        protected override ConfigurationPropertyCollection Properties {
            get { return _Properties; }
        }

        public bool? DebugMode {
            get { return (bool?)base[_DebugMode]; }
            set { base[_DebugMode] = value; }
        }

        public bool? PrivateBinding {
            get { return (bool?)base[_PrivateBinding]; }
            set { base[_PrivateBinding] = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IEnumerable<LanguageElement> GetLanguages() {
            var languages = this[_Languages] as LanguageElementCollection;
            if (languages == null) {
                yield break;
            }

            foreach (var languageConfig in languages) {
                yield return (LanguageElement)languageConfig;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IEnumerable<OptionElement> GetOptions() {
            var options = this[_Options] as OptionElementCollection;
            if (options == null) {
                yield break;
            }

            foreach (var option in options) {
                yield return (OptionElement)option;
            }
        }

        private static Section LoadFromFile(Stream configFileStream) {
            var result = new Section();
            using (var reader = XmlReader.Create(configFileStream)) {
                if (reader.ReadToDescendant("configuration") && reader.ReadToDescendant(SectionName)) {
                    result.DeserializeElement(reader, false);
                } else {
                    return null;
                }
            }
            return result;
        }

        internal static void LoadRuntimeSetup(ScriptRuntimeSetup setup, Stream configFileStream) {
            Section config;
            if (configFileStream != null) {
                config = LoadFromFile(configFileStream);
            } else {
                config = System.Configuration.ConfigurationManager.GetSection(Section.SectionName) as Section;
            }

            if (config == null) {
                return;
            }

            if (config.DebugMode.HasValue) {
                setup.DebugMode = config.DebugMode.Value;
            }
            if (config.PrivateBinding.HasValue) {
                setup.PrivateBinding = config.PrivateBinding.Value;
            }

            foreach (var languageConfig in config.GetLanguages()) {
                var provider = languageConfig.Type;
                var names = languageConfig.GetNamesArray();
                var extensions = languageConfig.GetExtensionsArray();
                var displayName = languageConfig.DisplayName ?? ((names.Length > 0) ? names[0] : languageConfig.Type);

                // Honor the latest-wins behavior of the <languages> tag for options that were already included in the setup object;
                // Keep the options though.
                bool found = false;
                foreach (var language in setup.LanguageSetups) {
                    if (language.TypeName == provider) {
                        language.Names.Clear();
                        foreach (string name in names) {
                            language.Names.Add(name);
                        }
                        language.FileExtensions.Clear();
                        foreach (string extension in extensions) {
                            language.FileExtensions.Add(extension);
                        }
                        language.DisplayName = displayName;
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    setup.LanguageSetups.Add(new LanguageSetup(provider, displayName, names, extensions));
                }
            }

            foreach (var option in config.GetOptions()) {
                if (String.IsNullOrEmpty(option.Language)) {
                    // common option:
                    setup.Options[option.Name] = option.Value;
                } else {
                    // language specific option:
                    bool found = false;
                    foreach (var language in setup.LanguageSetups) {
                        if (language.Names.Any(s => DlrConfiguration.LanguageNameComparer.Equals(s, option.Language))) {
                            language.Options[option.Name] = option.Value;
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        throw new ConfigurationErrorsException(string.Format("Unknown language name: '{0}'", option.Language));
                    }
                }
            }
        }
    }
}

#endif
