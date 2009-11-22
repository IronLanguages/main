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
                    BrowserPAL.PAL.VirtualFilesystem.GetFileContents(DynamicApplication.MakeUri(src));

                // Rewrite the XAML to remove CDATA or <?xml line
                var r = new StringReader(xaml);
                var w = new StringWriter();
                string line;
                while ((line = r.ReadLine()) != null) {
                    if (line.Trim() == "<![CDATA[" || line.Trim() == "]]>" || line.Contains("<?xml")) continue;
                    w.WriteLine(line);
                }
                xaml = w.ToString();

                // set the RootVisual
                DynamicApplication.Current.LoadRootVisualFromString(xaml);
            };

            // If src is set, download the src and invoke onComplete when the
            // download is finished. Otherwise, just invoke onComplete.
            if (src == string.Empty) {                
                onComplete.Invoke();
            } else {
                ((HttpVirtualFilesystem)HttpPAL.PAL.VirtualFilesystem).
                    DownloadAndCache(new List<Uri>(){ DynamicApplication.MakeUri((string)src) }, onComplete);
            }
        }
    }
}
