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

using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Browser;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Silverlight;
using System;
using System.Dynamic;

[assembly: ExtensionType(typeof(HtmlDocument), typeof(HtmlDocumentExtension))]
[assembly: ExtensionType(typeof(HtmlElement), typeof(HtmlElementExtension))]
[assembly: ExtensionType(typeof(FrameworkElement), typeof(FrameworkElementExtension))]

namespace Microsoft.Scripting.Silverlight {

    #region Run code to load language-specific type extensions
    public static class LanguageTypeExtensions {
        public static void Load(DynamicLanguageConfig langConfig) {
            foreach (var pair in langConfig.LanguagesUsed) {
                if (pair.Value) {
                    var lang = langConfig.GetLanguageByName(pair.Key);
                    LanguageTypeExtensions.LoadByExtension(lang.Extensions[0]);
                }
            }
        }

        public static void LoadByExtension(string fileExtension) {
            var path = string.Format("init{0}", fileExtension);
            var code = DynamicApplication.GetResource(path);
            if (code == null) return;
            var dyneng = DynamicApplication.Current.Engine;
            dyneng.Engine = dyneng.Runtime.GetEngineByFileExtension(fileExtension);
            dyneng.Engine.CreateScriptSourceFromString(code, path).Execute(dyneng.CreateScope());
        }
    }
    #endregion

    #region Extension classes that support Microsoft.Scripting.Runtime.ExtensionTypeAttribute
    /// <summary>
    /// Injects properties into the HtmlDocument object for each element ID
    /// </summary>
    public static class HtmlDocumentExtension {
        [SpecialName]
        public static object GetBoundMember(HtmlDocument doc, string name) {
            HtmlElement result = doc.GetElementById(name);
            if (result == null) {
                return OperationFailed.Value;
            }
            return result;
        }
    }

    /// <summary>
    /// Injects properties for getting/setting the attributes of HtmlElement
    /// </summary>
    public static class HtmlElementExtension {
        [SpecialName]
        public static object GetBoundMember(HtmlElement element, string name) {
            return element.GetProperty(name);
        }

        // TODO: should this be SetMemberAfter?
        [SpecialName]
        public static void SetMember(HtmlElement element, string name, string value) {
            element.SetProperty(name, value);
        }
    }

    /// <summary>
    /// Injects child XAML objects as properties
    /// </summary>
    public static class FrameworkElementExtension {
        [SpecialName]
        public static object GetBoundMember(FrameworkElement element, string name) {
            object result = element.FindName(name);
            if (result == null) {
                return OperationFailed.Value;
            }
            return result;
        }
    }
    #endregion
}
