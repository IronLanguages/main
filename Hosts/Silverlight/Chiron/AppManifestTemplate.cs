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

        internal XmlDocument Generate(IEnumerable<Uri> assemblySources, IEnumerable<Uri> externalSources) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(_template);

            XmlElement partsTarget = (XmlElement)doc.GetElementsByTagName("Deployment.Parts")[0];
            foreach (Uri source in assemblySources) {
                XmlElement ap = doc.CreateElement("AssemblyPart", partsTarget.NamespaceURI);
                string src = source.ToString();
                ap.SetAttribute("Source", src);
                partsTarget.AppendChild(ap);
            }

            XmlNodeList externalParts = doc.GetElementsByTagName("Deployment.ExternalParts");
            if(externalParts.Count > 0) {
                XmlElement externalsTarget = (XmlElement)externalParts[0];
                foreach (Uri source in externalSources) {
                    XmlElement ap = doc.CreateElement("ExtensionPart", externalsTarget.NamespaceURI);
                    string src = source.ToString();
                    ap.SetAttribute("Source", src);
                    externalsTarget.AppendChild(ap);
                }
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
