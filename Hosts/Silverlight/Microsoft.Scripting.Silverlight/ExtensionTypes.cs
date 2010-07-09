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

using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Browser;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Silverlight;
using System;
using System.Dynamic;
using Microsoft.Scripting.Utils;

[assembly: ExtensionType(typeof(HtmlDocument), typeof(HtmlDocumentExtension))]
[assembly: ExtensionType(typeof(HtmlElement), typeof(HtmlElementExtension))]
[assembly: ExtensionType(typeof(HtmlObject), typeof(HtmlObjectExtension))]
[assembly: ExtensionType(typeof(ScriptObject), typeof(ScriptObjectExtension))]
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
            return (object)doc.GetElementById(name) ??
                HtmlElementExtension.GetBoundMember(doc.DocumentElement, name);
        }

        [SpecialName]
        public static void SetMember(HtmlDocument doc, string name, object value) {
            HtmlElementExtension.SetMember(doc.DocumentElement, name, value);
        }
    }

    /// <summary>
    /// Injects properties for getting/setting the attributes of HtmlWindow
    /// </summary>
    public static class HtmlWindowExtension {
        [SpecialName]
        public static object GetBoundMember(HtmlWindow win, string name) {
            return HtmlObjectExtension.GetBoundMember(win, name);
        }

        [SpecialName]
        public static void SetMember(HtmlWindow win, string name, object value) {
            HtmlObjectExtension.SetMember(win, name, value);
        }
    }

    /// <summary>
    /// Injects properties for getting/setting the attributes of HtmlElement
    /// </summary>
    public static class HtmlElementExtension {
        [SpecialName]
        public static object GetBoundMember(HtmlObject obj, string name) {
            return HtmlObjectExtension.GetBoundMember(obj, name);
        }

        [SpecialName]
        public static void SetMember(HtmlObject obj, string name, object value) {
            HtmlObjectExtension.SetMember(obj, name, value);
        }
    }

    public static class HtmlObjectExtension {
        [SpecialName]
        public static object GetBoundMember(HtmlObject obj, string name) {
            if (name == "events" || name == "Events") {
                return Events(obj);
            }
            return ScriptObjectExtension.GetBoundMember(obj, name);
        }

        [SpecialName]
        public static void SetMember(HtmlObject obj, string name, object value) {
            ScriptObjectExtension.SetMember(obj, name, value);
        }

        public static DynamicHtmlEvents Events(HtmlObject obj) {
            return new DynamicHtmlEvents(obj);
        }
    }

    public static class ScriptObjectExtension {
        [SpecialName]
        public static object GetBoundMember(ScriptObject obj, string name) {
            return obj.GetProperty(name);
        }

        [SpecialName]
        public static void SetMember(ScriptObject obj, string name, object value) {
            obj.SetProperty(name, value);
        }

        [SpecialName]
        public static object Invoke(ScriptObject obj, params object[] args) {
            return obj.InvokeSelf(args);
        }
    }

    public class DynamicHtmlEvents {

        private HtmlObject _obj;

        internal DynamicHtmlEvents(HtmlObject obj) {
            _obj = obj;
        }

        [SpecialName]
        public static object GetBoundMember(DynamicHtmlEvents events, string name) {
            return new DynamicHtmlEvent(events._obj, name);
        }

        [SpecialName]
        public static void SetMember(DynamicHtmlEvents events, string name, object value) {
            // no-op
        }
    }

    public class DynamicHtmlEvent {
        
        private readonly HtmlObject _object;
        private readonly string _name;

        internal DynamicHtmlEvent(HtmlObject obj, string name) {
            _object = obj;
            _name = name;
        }

        public static DynamicHtmlEvent operator +(DynamicHtmlEvent @event, object func) {
            var handler = DynamicApplication.Current.Engine.Engine.Operations.ConvertTo<EventHandler<HtmlEventArgs>>(func);
            @event.Add(handler);
            return @event;
        }

        public static DynamicHtmlEvent operator +(DynamicHtmlEvent @event, EventHandler handler) {
            @event.Add(handler);
            return @event;
        }

        public static DynamicHtmlEvent operator +(DynamicHtmlEvent @event, EventHandler<HtmlEventArgs> handler) {
            @event.Add(handler);
            return @event;
        }

        private void Add(EventHandler handler) {
            _object.AttachEvent(_name, handler);
        }

        private void Add(EventHandler<HtmlEventArgs> handler) {
            _object.AttachEvent(_name, handler);
        }
    }

    /// <summary>
    /// Injects child XAML objects as properties
    /// </summary>
    public static class FrameworkElementExtension {
        [SpecialName]
        public static object GetBoundMember(FrameworkElement element, string name) {
            return (object) element.FindName(name) ?? OperationFailed.Value;
        }
    }
    #endregion
}
