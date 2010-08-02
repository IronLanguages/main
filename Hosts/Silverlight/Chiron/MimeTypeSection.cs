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

using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace Chiron {
    // Processes the MimeMap section of Chiron.exe.config
    // MIME type entries look like this:
    // <mimeMap fileExtension=".xaml" mimeType="application/xaml+xml" />
    public class MimeTypeSection : IConfigurationSectionHandler {
        public object Create(object parent, object configContext, XmlNode section) {

            Dictionary<string, string> mimeMap = new Dictionary<string, string>();

            foreach (XmlElement mimeEntry in ((XmlElement)section).GetElementsByTagName("mimeMap")) {
                string ext = mimeEntry.GetAttribute("fileExtension");
                string type = mimeEntry.GetAttribute("mimeType");
                if (string.IsNullOrEmpty(ext) || string.IsNullOrEmpty(type)) {
                    throw new ConfigurationErrorsException("mimeMap element requires the fileExtension and mimeType attributes");
                }
                ext = ext.ToLowerInvariant();
                if (mimeMap.ContainsKey(ext)) {
                    throw new ConfigurationErrorsException("duplicate mimeMap fileExtension: " + ext);
                }
                mimeMap.Add(ext, type);
            }

            return mimeMap;
        }
    }
}
