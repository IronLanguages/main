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

            // Fetch the external URI.
            var src = (string)xamlScriptTag.GetProperty("src");

            // Define XAML now so "onComplete" can close over it.
            string xaml = null;

            Action onComplete = () => {
                // Fetch innerHTML if inline, or the downloaded file otherwise
                xaml = src == string.Empty ? 
                    (string) xamlScriptTag.GetProperty("innerHTML") :
                    BrowserPAL.PAL.VirtualFilesystem.GetFileContents(new Uri((string)src, UriKind.RelativeOrAbsolute));

                // Rewrite the XAML to remove CDATA or <?xml line
                var r = new StringReader(xaml);
                var w = new StringWriter();
                string line;
                while ((line = r.ReadLine()) != null) {
                    if (line.Trim() == "<![CDATA[" || line.Trim() == "]]>" || line.Contains("<?xml")) continue;
                    w.WriteLine(line);
                }
                xaml = w.ToString();

                // Find the root XAML node, create the corresponding Control 
                // object, and set the RootVisual with the XAML string.
                XmlReader reader = XmlReader.Create(new StringReader(xaml));
                reader.MoveToContent();
                if (reader.NodeType == XmlNodeType.Element) {
                    var aqn = typeof(System.Windows.Controls.UserControl).AssemblyQualifiedName;
                    var type = Type.GetType(
                        string.Format("System.Windows.Controls.{0}, {1}", reader.Name, aqn.Substring(aqn.IndexOf(',') + 1))
                    );
                    if(type != null) {
                        var root = (UIElement) type.GetConstructor(new Type[] { }).Invoke(new object[] { });
                        DynamicApplication.Current.LoadRootVisual(root, xaml, false);
                    }
                }
            };

            // If src is set, download the src and invoke onComplete when the
            // download is finished. Otherwise, just invoke onComplete.
            if (src == string.Empty) {                
                onComplete.Invoke();
            } else {
                ((HttpVirtualFilesystem)HttpPAL.PAL.VirtualFilesystem).
                    DownloadAndCache(new List<Uri>(){ new Uri((string)src, UriKind.RelativeOrAbsolute) }, onComplete);
            }
        }
    }
}
