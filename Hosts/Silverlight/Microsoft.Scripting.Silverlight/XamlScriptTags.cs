using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Xml;
using System.IO;
using System.Windows;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Silverlight {

    /// <summary>
    /// Manages the script-tags that are used for XAML
    /// </summary>
    public class XamlScriptTags {

        /// <summary>
        /// Finds the script-tag that corresponds with this Silverlight control,
        /// and download the src if need be. When the downloads complete (or
        /// if there is nothing to download), set the RootVisual to the xaml
        /// content.
        /// </summary>
        public static void Load() {

            // If there is no xamlid, then this control doesn't process XAML
            // script tags.
            if (!DynamicApplication.Current.InitParams.ContainsKey("xamlid"))
                return;
            
            // Look for the element with an id matching the value of xamlid,
            // and abort if it's not found.
            var id = DynamicApplication.Current.InitParams["xamlid"];
            if (id == null) return;
            var xamlScriptTag = (HtmlElement) HtmlPage.Document.GetElementById(id);
            if (xamlScriptTag == null) return;

            // Make sure it has the proper mime-type
            var type = (string) xamlScriptTag.GetAttribute("type");
            if (type != "application/xaml+xml" && type != "application/xml+xaml")
                return;

            // Fetch the external URI.
            var src = (string)xamlScriptTag.GetAttribute("src");

            // Should the loading of the XAML be deferred?
            bool defer = (bool)xamlScriptTag.GetProperty("defer");

            // Define XAML now so "onComplete" can close over it.
            string xaml = null;

            Action onComplete = () => {
                // Only load "non-deffered" script-tags. Being deffered just means
                // the XAML isn't loaded, but it is already downloaded (if it was
                // an external script-tag), so users can just get it from the virtual
                // file-system. Inline XAML which is deferred can only be accessed by
                // getting it from the DOM.
                if (defer)
                    return;

                // Fetch innerHTML if inline, otherwise get the contents from the download cache
                xaml = (src == null || src.Length == 0 ?
                         (string)xamlScriptTag.GetProperty("innerHTML") :
                         BrowserPAL.PAL.VirtualFilesystem.GetFileContents(DynamicApplication.MakeUri(src))
                       ).Trim();

                // set the RootVisual
                DynamicApplication.Current.LoadRootVisualFromString(AddNamespaces(StripCDATA(xaml)));
            };

            // If src is set, download the src and invoke onComplete when the
            // download is finished. Otherwise, just invoke onComplete.
            if (src == null || src.Length == 0) {
                onComplete.Invoke();
            } else {
                ((HttpVirtualFilesystem)HttpPAL.PAL.VirtualFilesystem).
                    DownloadAndCache(new List<Uri>(){ DynamicApplication.MakeUri((string)src) }, onComplete);
            }
        }

        private static string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        private static string XamlXNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        public static string AddNamespaces(string xml) {
            StringReader strReader = new StringReader(xml);
            XmlReader xmlReader = XmlReader.Create(strReader);
            StringBuilder outxml = new StringBuilder();
            XmlWriter xmlWriter = XmlWriter.Create(outxml);
            while (xmlReader.Read()) {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.IsStartElement()) {
                    string xmlns = xmlReader.LookupNamespace("");
                    if (xmlns == null || xmlns.Length == 0) xmlns = XamlNamespace;

                    xmlWriter.WriteStartElement(xmlReader.LocalName, xmlns);

                    xmlWriter.WriteAttributes(xmlReader, false);

                    string xmlnsx = xmlReader.LookupNamespace("x");
                    if (xmlnsx == null || xmlnsx.Length == 0) {
                        xmlWriter.WriteAttributeString("xmlns", "x", null, XamlXNamespace);
                        xmlWriter.LookupPrefix("x");
                    }

                    string xmlnsxaml = xmlReader.LookupNamespace("xaml");
                    if (xmlnsxaml == null || xmlnsx.Length == 0) {
                        xmlWriter.WriteAttributeString("xmlns", "xaml", null, XamlNamespace);
                    }

                    xmlWriter.WriteRaw(xmlReader.ReadInnerXml());

                    xmlWriter.WriteEndElement();
                    break;
                }
            }
            xmlReader.Close();
            xmlWriter.Close();
            return outxml.ToString();
        }

        #region Gestalt's AddNamespaces implementation (unused)
#if false
        private string _AddNamespaces(string content) {
            int space = content.IndexOf(" ");
            int bracket = content.IndexOf(">");
            if (bracket < space) space = bracket;

            string left = content.Substring(0, space) + " ";
            string right = " " + content.Substring(space, content.Length - space);

            string decl = "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"";
            if (!right.Contains(decl)) right = decl + right;

            decl = "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"";
            if (!right.Contains(decl)) right = decl + " " + right;

            decl = "xmlns:xaml=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"";
            if (!right.Contains(decl)) right = decl + " " + right;

            return left + right;
        }
#endif
        #endregion

        public static string StripCDATA(string xml) {
            var stringReader = new StringReader(xml);
            var startToken = "<![CDATA[";
            var endToken = "]]>";
            StringBuilder buffer = new StringBuilder();
            var inCDATA = false;
            char c;

            if (!xml.Contains(startToken) || !xml.Contains(endToken))
                return xml.Trim();

            while (true) {
                int i = stringReader.Read();
                c = (char)i;
                buffer.Append(c);
                if (!inCDATA && buffer.ToString().Contains(startToken)) {
                    inCDATA = true;
                    buffer.Remove(0, buffer.Length);
                }
                if (inCDATA && buffer.ToString().Contains(endToken)) {
                    buffer.Remove(buffer.Length - endToken.Length, endToken.Length);
                    inCDATA = false;
                    break;
                }
            }
            return buffer.ToString().Trim();
        }
    }
}
