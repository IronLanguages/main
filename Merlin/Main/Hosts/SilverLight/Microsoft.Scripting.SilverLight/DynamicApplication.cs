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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Net;

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

        internal static bool InUIThread {
            get { return _UIThreadId == Thread.CurrentThread.ManagedThreadId; }
        }
        private static int _UIThreadId;
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
        #endregion

        #region Public API
        // these are instance methods so you can do Application.Current.TheMethod(...)

        /// <summary>
        /// Loads a XAML file, represented by a Uri, into a UIElement, and sets
        /// the UIElement as the RootVisual of the application.
        /// </summary>
        /// <param name="root">UIElement to load the XAML into</param>
        /// <param name="uri">Uri to a XAML file</param>
        /// <returns></returns>
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
        /// <returns></returns>
        public DependencyObject LoadRootVisual(UIElement root, string uri) {
            return LoadRootVisual(root, MakeUri(uri));
        }

        /// <summary>
        /// Loads a XAML file, represented by a string, into any object.
        /// </summary>
        /// <param name="component">The object to load the XAML into</param>
        /// <param name="uri">string representing the relative Uri of the XAML file</param>
        public object LoadComponent(object component, string uri) {
            return LoadComponent(component, MakeUri(uri));
        }

        /// <summary>
        /// Loads a XAML file, represented by a string, into any object.
        /// </summary>
        /// <param name="component">The object to load the XAML into</param>
        /// <param name="uri">relative Uri of the XAML file</param>
        public new object LoadComponent(object component, Uri relativeUri) {
            Application.LoadComponent(component, relativeUri);
            return component;
        }

        /// <summary>
        /// Makes a Uri object that is relative to the location of the "start" source file.
        /// </summary>
        /// <param name="relativeUri">Any Uri</param>
        /// <returns>A Uri relative to the "start" source file</returns>
        public Uri MakeUri(string relativeUri) {
            // Get the source file location so we can make the URI relative to the executing source file
            string baseUri = Path.GetDirectoryName(Settings.EntryPoint);
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

            Startup += new StartupEventHandler(DynamicApplication_Startup);
        }

        void DynamicApplication_Startup(object sender, StartupEventArgs e) {
            Settings.Parse(InitParams = NormalizeInitParams(e.InitParams));
            Engine = new DynamicEngine();
            AppManifest = new DynamicAppManifest();
            AppManifest.LoadAssemblies(() => Engine.Start());
        }

        private IDictionary<string, string> NormalizeInitParams(IDictionary<string, string> initParams) {
            // normalize initParams because otherwise it preserves whitespace, which is not very useful
            var result = new Dictionary<string, string>(initParams);
            foreach (KeyValuePair<string, string> pair in initParams) {
                result[pair.Key.Trim()] = pair.Value.Trim();
            }
            return result;
        }

        internal void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs args) {
            args.Handled = true;
            ErrorFormatter.DisplayError(Settings.ErrorTargetID, args.ExceptionObject);
        }
        #endregion
    }
}
