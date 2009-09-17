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
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Resources;
using System.Xml;
using Microsoft.Scripting.Hosting;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Markup;
using System.Windows.Browser;

namespace Microsoft.Scripting.Silverlight {

    /// <summary>
    /// The entry point for dynamic language applications
    /// It is a static class that exists to bootstrap the DLR, and start running the application
    /// Also contains helper APIs. These can be accessed by using:
    /// 
    ///   System.Windows.Application.Current
    ///   
    /// ... which returns the global instance of DynamicApplication
    /// </summary>
    public class DynamicApplication : Application {

        #region Properties
        /// <summary>
        /// Returns the "initParams" argument passed to the Silverlight control
        /// (otherwise would be inaccessible because the DLR host consumes them)
        /// </summary>
        public IDictionary<string, string> InitParams { get; private set; }

        /// <summary>
        /// Returns the instance of the DynamicApplication.
        /// Importantly, this method works if not on the UI thread, unlike
        /// Application.Current
        /// </summary>
        public static new DynamicApplication Current { get { return _Current; } }
        private static volatile DynamicApplication _Current;

        /// <summary>
        /// DynamicEngine responcible for interfacing with the DLR and
        /// running all dynamic language code.
        /// </summary>
        public DynamicEngine Engine { get; private set; }

        /// <summary>
        /// Application Manifest abstraction to handle loading assemblies.
        /// </summary>
        public DynamicAppManifest AppManifest { get; private set; }

        /// <summary>
        /// The script tags defined on the HTML page
        /// </summary>
        internal DynamicScriptTags ScriptTags { get; private set; }

        /// <summary>
        /// Avaliable languages
        /// </summary>
        internal DynamicLanguageConfig LanguagesConfig { get; private set; }
        #endregion

        #region Depricated Properties
        [Obsolete("Use DynamicApplication.Current.Engine.Runtime instead")]
        public ScriptRuntime Runtime { get { return Engine.Runtime; } }

        [Obsolete("Use Settings.EntryPoint instead")]
        public string EntryPoint { get { return Settings.EntryPoint; } }

        [Obsolete("Use Settings.Debug instead")]
        public bool Debug { get { return Settings.Debug; } }

        [Obsolete("Use Settings.ErrorTargetID instead")]
        public string ErrorTargetID { get { return Settings.ErrorTargetID; } }

        [Obsolete("Use Settings.ReportUnhandledErrors instead")]
        public bool ReportUnhandledErrors { get { return Settings.ReportUnhandledErrors; } }

        [Obsolete("Use DynamicEngine.CreateRuntimeSetup() instead")]
        public static ScriptRuntimeSetup CreateRuntimeSetup() {
            return DynamicEngine.CreateRuntimeSetup();
        }

        // not really used anymore ...
        internal static bool InUIThread {
            get { return _UIThreadId == Thread.CurrentThread.ManagedThreadId; }
        }
        private static int _UIThreadId;
        #endregion

        #region Public API
        // these are instance methods so you can do Application.Current.TheMethod(...)

        /// <summary>
        /// Loads a XAML file, represented by a Uri, into a UIElement, and sets
        /// the UIElement as the RootVisual of the application.
        /// </summary>
        /// <param name="root">UIElement to load the XAML into</param>
        /// <param name="uri">Uri to a XAML file</param>
        /// <returns>the RootVisual</returns>
        public DependencyObject LoadRootVisual(UIElement root, Uri uri) {
            root = (UIElement) LoadComponent(root, uri);
            RootVisual = root;
            return root;
        }

        /// <summary>
        /// Loads a XAML file, represented by a string, into a UIElement, and sets
        /// the UIElement as the RootVisual of the application.
        /// </summary>
        /// <param name="root">UIElement to load the XAML into</param>
        /// <param name="uri">string representing the relative Uri of the XAML file</param>
        /// <returns>the RootVisual</returns>
        public DependencyObject LoadRootVisual(UIElement root, string xamlUri) {
            return LoadRootVisual(root, xamlUri, true);
        }

        /// <summary>
        /// Either loads a XAML file URI, or the XAML itself, represented by a string,
        /// and sets the RootVisual of the application.
        /// </summary>
        /// <param name="root">element to load XAML into</param>
        /// <param name="xamlOrUri">string representing a URI of a XAML file, or the XAML content itself</param>
        /// <param name="uri">set this to "true" if "xamlOrUri" is a URI, and "false" if it is XAML content</param>
        /// <returns>the RootVisual</returns>
        public DependencyObject LoadRootVisual(UIElement root, string xamlOrUri, bool uri) {
            if (uri) {
                return LoadRootVisual(root, MakeUri(xamlOrUri));
            } else {
                root = (UIElement)LoadComponent(root, xamlOrUri, false);
                RootVisual = root;
                return root;
            }
        }

        /// <summary>
        /// Loads a XAML file, represented by a string, into any object.
        /// </summary>
        /// <param name="component">The object to load the XAML into</param>
        /// <param name="uri">string representing the relative Uri of the XAML file</param>
        public object LoadComponent(object component, string xamlUri) {
            return LoadComponent(component, xamlUri, true);
        }

        /// <summary>
        /// Loads a XAML file or XAML content into an object.
        /// </summary>
        /// <param name="component">The object to load XAML into</param>
        /// <param name="xamlOrUri">either the URI of a XAML file, or the actual XAML content</param>
        /// <param name="uri">set this to "true" if "xamlOrUri" is a URI, and "false" if it is XAML content</param>
        /// <returns>The loaded XAML</returns>
        public object LoadComponent(object component, string xamlOrUri, bool uri) {
            if (uri) {
                return LoadComponent(component, MakeUri(xamlOrUri));
            } else {
                component = XamlReader.Load(Regex.Replace(xamlOrUri, "x:Class=\".*?\"", ""));
                return component;
            }
        }

        /// <summary>
        /// Loads a XAML file, represented by a string, into any object.
        /// </summary>
        /// <param name="component">The object to load the XAML into</param>
        /// <param name="uri">relative Uri of the XAML file</param>
        public new object LoadComponent(object component, Uri relativeUri) {
            if (Application.GetResourceStream(relativeUri) == null) {
                var xamlStream = HttpPAL.PAL.VirtualFilesystem.GetFile(relativeUri);
                if (xamlStream != null) {
                    string xaml;
                    using (StreamReader sr = new StreamReader(xamlStream)) {
                        xaml = sr.ReadToEnd();
                    }
                    return LoadComponent(component, xaml, false);
                }
            } else {
                Application.LoadComponent(component, relativeUri);
                return component;
            }
            return null;

        }

        /// <summary>
        /// Makes a Uri object that is relative to the location of the "start" source file.
        /// </summary>
        /// <param name="relativeUri">Any relative uri</param>
        /// <returns>A Uri relative to the "start" source file</returns>
        public Uri MakeUri(string relativeUri) {
            string baseUri = ScriptTags.HasScriptTags && Settings.EntryPoint != null ?
                Path.GetDirectoryName(Settings.EntryPoint) :
                "";
            if (baseUri != "") baseUri += "/";
            return new Uri(baseUri + relativeUri, UriKind.Relative);
        }
        #endregion

        #region Implementation

        /// <summary>
        /// Called by Silverlight host when it instantiates our application
        /// </summary>
        public DynamicApplication() {
            if (_Current != null) {
                throw new Exception("Only one instance of DynamicApplication can be created");
            }

            _Current = this;
            _UIThreadId = Thread.CurrentThread.ManagedThreadId;

            Settings.ReportUnhandledErrors = true;

            AppManifest = new DynamicAppManifest();
            LanguagesConfig = DynamicLanguageConfig.Create(AppManifest.Assemblies);

            Startup += new StartupEventHandler(DynamicApplication_Startup);
        }

        /// <summary>
        /// Starts the dynamic application
        /// </summary>
        void DynamicApplication_Startup(object sender, StartupEventArgs e) {
            Settings.Parse(InitParams = NormalizeInitParams(e.InitParams));
            ScriptTags = new DynamicScriptTags(LanguagesConfig);
            XamlScriptTags.Load();
            LanguagesConfig.DownloadLanguages(AppManifest, () => {
                ScriptTags.DownloadExternalCode(() => {
                    Engine = new DynamicEngine();
                    if (Settings.ConsoleEnabled)
                        Repl.Show();
                    ScriptTags.Run(Engine);
                    Engine.Run(Settings.EntryPoint);
                });
            });
        }

        /// <summary>
        /// normalize initParams because otherwise it preserves whitespace, which is not very useful
        /// </summary>
        /// <param name="initParams">InitParams from Silverlight</param>
        /// <returns>Normalized InitParams</returns>
        private IDictionary<string, string> NormalizeInitParams(IDictionary<string, string> initParams) {
            var result = new Dictionary<string, string>(initParams);
            foreach (KeyValuePair<string, string> pair in initParams) {
                result[pair.Key.Trim()] = pair.Value.Trim();
            }
            return result;
        }

        /// <summary>
        /// Any unhandled exceptions in the Silverlight application will be
        /// handled here, which displays the error to the HTML page hosting the
        /// Silverlight control.
        /// </summary>
        internal void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs args) {
            args.Handled = true;
            ErrorFormatter.DisplayError(Settings.ErrorTargetID, args.ExceptionObject);
        }
        #endregion
    }
}
