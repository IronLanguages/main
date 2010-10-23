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

#if UNUSED

using System;
using System.Configuration;
using Microsoft.Scripting.AspNet.UI;

namespace Microsoft.Scripting.AspNet.Configuration {
    class WebScriptingSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        private static readonly ConfigurationProperty _propPageBaseType =
            new ConfigurationProperty("pageBaseType",
                                        typeof(String),
                                        typeof(ScriptPage).FullName);
        private static readonly ConfigurationProperty _propUserControlBaseType =
            new ConfigurationProperty("userControlBaseType",
                                        typeof(String),
                                        typeof(ScriptUserControl).FullName);
        private static readonly ConfigurationProperty _propMasterPageBaseType =
            new ConfigurationProperty("masterPageBaseType",
                                        typeof(String),
                                        typeof(ScriptMaster).FullName);

        static WebScriptingSection() {
            _properties.Add(_propPageBaseType);
            _properties.Add(_propUserControlBaseType);
            _properties.Add(_propMasterPageBaseType);
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("pageBaseType", DefaultValue = "Microsoft.Scripting.AspNet.UI.ScriptPage")]
        public string PageBaseType {
            get {
                return (string)base[_propPageBaseType];
            }
            set {
                base[_propPageBaseType] = value;
            }
        }

        [ConfigurationProperty("userControlBaseType", DefaultValue = "Microsoft.Scripting.AspNet.UI.ScriptUserControl")]
        public string UserControlBaseType {
            get {
                return (string)base[_propUserControlBaseType];
            }
            set {
                base[_propUserControlBaseType] = value;
            }
        }

        [ConfigurationProperty("masterPageBaseType", DefaultValue = "Microsoft.Scripting.AspNet.UI.ScriptMaster")]
        public string MasterPageBaseType {
            get {
                return (string)base[_propMasterPageBaseType];
            }
            set {
                base[_propMasterPageBaseType] = value;
            }
        }
    }
}
#endif