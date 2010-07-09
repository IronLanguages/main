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

using System.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting.Configuration {
    // <language names="IronPython;Python;py" extensions=".py" type="AQTN" displayName="IronPython v2">
    //    <option name="foo" value="bar" />
    // </language>
    public class LanguageElement : ConfigurationElement {
        private const string _Names = "names";
        private const string _Extensions = "extensions";
        private const string _Type = "type";
        private const string _DisplayName = "displayName";

        private static ConfigurationPropertyCollection _Properties = new ConfigurationPropertyCollection {
            new ConfigurationProperty(_Names, typeof(string), null),
            new ConfigurationProperty(_Extensions, typeof(string), null),
            new ConfigurationProperty(_Type, typeof(string), null, ConfigurationPropertyOptions.IsRequired),
            new ConfigurationProperty(_DisplayName, typeof(string), null)
        };

        protected override ConfigurationPropertyCollection Properties {
            get { return _Properties; }
        }

        public string Names {
            get { return (string)this[_Names]; }
            set { this[_Names] = value; }
        }

        public string Extensions {
            get { return (string)this[_Extensions]; }
            set { this[_Extensions] = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public string Type {
            get { return (string)this[_Type]; }
            set { this[_Type] = value; }
        }

        public string DisplayName {
            get { return (string)this[_DisplayName]; }
            set { this[_DisplayName] = value; }
        }

        public string[] GetNamesArray() {
            return Split(Names);
        }

        public string[] GetExtensionsArray() {
            return Split(Extensions);
        }

        private static string[] Split(string str) {
            return (str != null) ? str.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries) : ArrayUtils.EmptyStrings;
        }
    }
}

#endif