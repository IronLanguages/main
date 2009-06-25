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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;
using System.Reflection;
using System.IO;

namespace Microsoft.Scripting.Silverlight {
    public class Window {

        #region Window template
        private const string _windowContainerId = "silverlightDlrWindowContainer";
        private const string _windowId          = "silverlightDlrWindow";
        private const string _windowMenuId      = "silverlightDlrWindowMenu";
        private const string _windowLinkId      = "silverlightDlrWindowLink";

        // 0 - window id/class
        // 1 - window menu id/class
        // 2 - window link id/class
        private static string _htmlTemplate = @"
<!-- window -->
<div class=""{0}"" id=""{0}"">
  <!-- menu -->
  <div class=""{1}"" id=""{1}"">
    <a id=""{2}"" href=""javascript:void(0);"" onclick=""sdlrw.hideAllPanels(this)"">&dArr; Minimize</a>
  </div>
</div> <!-- silverlightDlrWindow -->";

        // 1 - menu text
        // 0 - menu entry id
        private static string _menuEntryTemplate = @"
<a id=""{0}Link"" href=""javascript:void(0);"" onclick=""sdlrw.showPanel('{0}')"">&uArr; {1}</a>";
        #endregion

        #region Private fields
        private HtmlElement _windowLocationDiv;
        private static Window _current;
        #endregion

        #region Properties
        public static Window Current { get { return _current; } }
        public HtmlElement Contents {
            get {
                return HtmlPage.Document.GetElementById(_windowId);
            }
        }
        public HtmlElement Menu {
            get {
                return HtmlPage.Document.GetElementById(_windowMenuId);
            }
        }
        #endregion

        #region Public API
        public static void Show() {
            Show(true);
        }

        public static void Show(string windowLocationId) {
            Show(windowLocationId, true);
        }

        public static void Show(HtmlElement windowLocationDiv) {
            Show(windowLocationDiv, true);
        }

        public static void Show(bool inject) {
            Show((HtmlElement)null, inject);
        }

        public static void Show(string windowLocationId, bool inject) {
            Show(HtmlPage.Document.GetElementById(windowLocationId), inject);
        }

        public static void Show(HtmlElement windowLocationDiv, bool inject) {
            if (_current == null) {
                _current = new Window(windowLocationDiv, inject);
            }
        }

        public void AddPanel(string title, HtmlElement panel) {
            panel.CssClass += " silverlightDlrPanel";
            _current.Contents.AppendChild(panel);
            AddMenuItem(title, panel.Id);
        }

        public void Initialize() {
            HtmlPage.Window.Eval("sdlrw.initialize()");
        }

        public void ShowPanel(string id) {
            HtmlPage.Window.Eval(string.Format(@"sdlrw.showPanel(""{0}"")", id));
        }

        private void AddMenuItem(string title, string id) {
            Menu.SetProperty("innerHTML",
                string.Format(_menuEntryTemplate, id, title) +
                Menu.GetProperty("innerHTML")
            );
        }
        #endregion

        #region Implementation
        private Window(HtmlElement windowLocationDiv, bool injectStyleAndScript) {
            if (_current == null) {
                _windowLocationDiv = windowLocationDiv;
                if (_windowLocationDiv == null) {
                    _windowLocationDiv = HtmlPage.Document.CreateElement("div");
                    _windowLocationDiv.Id = _windowContainerId;
                }
                if (HtmlPage.Document.GetElementById(_windowLocationDiv.Id) == null) {
                    HtmlPage.Document.Body.AppendChild(_windowLocationDiv);
                }
                _windowLocationDiv.SetProperty("innerHTML", WindowHtml());

                if (injectStyleAndScript) {
                    InjectScriptBlock();
                    InjectStyleBlock();
                    injectStyleAndScript = false;
                }
            }
        }

        private void InjectScriptBlock() {
            HtmlHead().AppendChild(EmbedResourceInTag("script", "text/javascript", "agdlr.js"));
        }

        private void InjectStyleBlock() {
            HtmlHead().AppendChild(EmbedResourceInTag("style", "text/css", "agdlr.css"));
        }

        private HtmlElement EmbedResourceInTag(string tagName, string mimeType, string filename) {
            var block = HtmlPage.Document.CreateElement(tagName);
            block.SetAttribute("type", mimeType);
            
            string scriptOrStyle = GetResource(filename);
            
            var ieScriptOrStyleSet = false;

            if (HtmlPage.BrowserInformation.UserAgent.Contains("MSIE")) {
                if (tagName == "script") {
                    block.SetProperty("text", scriptOrStyle);
                    ieScriptOrStyleSet = true;
                } else if (tagName == "style") {
                    (block.GetProperty("styleSheet") as ScriptObject).SetProperty("cssText", scriptOrStyle);
                    ieScriptOrStyleSet = true;
                }
            }
            
            if(!ieScriptOrStyleSet) {
                var textNode = HtmlPage.Document.Invoke("createTextNode", new string[] { scriptOrStyle });
                (block as ScriptObject).Invoke("appendChild", new object[] { textNode });
            }
            return block;
        }

        private HtmlElement HtmlHead() {
            return HtmlPage.Document.GetElementsByTagName("head")[0] as HtmlElement;
        }

        private string GetResource(string filename) {
            var assembly = Assembly.GetExecutingAssembly();
            var textStreamReader = new StreamReader(assembly.GetManifestResourceStream("Microsoft.Scripting.Silverlight." + filename));
            var result = textStreamReader.ReadToEnd();
            textStreamReader.Close();
            return result;
        }

        private string WindowHtml() {
            return string.Format(_htmlTemplate, _windowId, _windowMenuId, _windowLinkId);
        }
        #endregion
    }
}
