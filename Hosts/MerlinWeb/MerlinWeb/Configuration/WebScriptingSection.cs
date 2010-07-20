#if UNUSED

using System;
using System.Configuration;
using Microsoft.Web.Scripting.UI;

namespace Microsoft.Web.Scripting.Configuration {
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

        [ConfigurationProperty("pageBaseType", DefaultValue = "Microsoft.Web.Scripting.UI.ScriptPage")]
        public string PageBaseType {
            get {
                return (string)base[_propPageBaseType];
            }
            set {
                base[_propPageBaseType] = value;
            }
        }

        [ConfigurationProperty("userControlBaseType", DefaultValue = "Microsoft.Web.Scripting.UI.ScriptUserControl")]
        public string UserControlBaseType {
            get {
                return (string)base[_propUserControlBaseType];
            }
            set {
                base[_propUserControlBaseType] = value;
            }
        }

        [ConfigurationProperty("masterPageBaseType", DefaultValue = "Microsoft.Web.Scripting.UI.ScriptMaster")]
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