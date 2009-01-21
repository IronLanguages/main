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
using System.Text;
using System.Configuration;
using System.Xml;
using System.IO;

namespace Chiron {
    /// <summary>
    /// Generates AppManifest.xaml from the template in Chiron.exe.config
    /// </summary>
    class AppManifestTemplate {
        string _template;

        internal AppManifestTemplate(string template) {
            _template = template;
        }

        internal XmlDocument Generate(IEnumerable<Uri> assemblySources) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(_template);
            XmlElement target = (XmlElement)doc.GetElementsByTagName("Deployment.Parts")[0];

            foreach (Uri source in assemblySources) {
                XmlElement ap = doc.CreateElement("AssemblyPart", target.NamespaceURI);
                string src = source.ToString();
                ap.SetAttribute("Source", src);
                target.AppendChild(ap);
            }
            return doc;
        }
    }

    public class AppManifestSection : IConfigurationSectionHandler {
        public object Create(object parent, object configContext, XmlNode section) {
            if (((XmlElement)section).GetElementsByTagName("Deployment.Parts").Count != 1) {
                throw new ConfigurationErrorsException("appManifestTemplate section requires exactly one Deployment.Parts element");
            }
            return new AppManifestTemplate(section.InnerXml);
        }
    }
}
