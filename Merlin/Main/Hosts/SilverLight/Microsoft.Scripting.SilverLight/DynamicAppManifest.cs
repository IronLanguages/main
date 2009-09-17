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
using System.Net;
using System.Windows.Resources;
using System.Windows;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Silverlight {
    public class DynamicAppManifest {

#if SILVERLIGHT_3
        // List of valid external packages
        // Needed so only language external packages are looked at
        private static List<string> _externalNames = new List<string>() {
            "Microsoft.Scripting.slvx",
            "IronRuby.slvx",
            "IronPython.slvx"
        };

        // List of valid assemblies to be inside the above external packages
        // Needed since there is no way to enumerate files in a XAP
        private static List<string> _assemblyNames = new List<string>() {
            "IronRuby",
            "IronRuby.Libraries",
            "IronPython",
            "IronPython.Modules",
            "Microsoft.Scripting.Core",
            "Microsoft.Scripting",
            "Microsoft.Dynamic",
            "Microsoft.Scripting.ExtensionAttribute",
            "Microsoft.Scripting.Silverlight"
        };
#endif

        /// <summary>
        /// List of all dynamic language assemblies loaded. Contains assemblies
        /// from AssemblyParts after construction. After LoadAssemblies is 
        /// called, any dynamic language assemblies that were loaded as part of
        /// ExternalParts is now part of this list.
        /// </summary>
        public List<Assembly> Assemblies { get; private set; }

        /// <summary>
        /// "Initializes "Assemblies" with any AssemblyParts
        /// </summary>
        public DynamicAppManifest() {
            Assemblies = AssemblyParts();
        }

        /// <summary>
        /// Populates the "Assemblies" list with any ExternalParts referenced.
        /// 
        /// ExternalParts are downloaded (though since Silverlight already downloaded
        /// then, this is just a browser-cache-hit) and all containing assemblies
        /// loaded. Invokes the onComplete action after loading.
        /// 
        /// If no ExternalParts are referenced, or the build is against SL2,
        /// onComplete is invoked immediately.
        /// </summary>
        /// <param name="onComplete">Executed when all assemblies are loaded</param>
        public void LoadAssemblies(Action onComplete) {
#if SILVERLIGHT_3
            if (UsesDLRExternals()) {
                // Silverlight BUG: Though Silverlight downloads each ExtensionPart and
                // loads each assembly inside, Silverlight doesn't expose the extension's stream.
                // The DLR needs to load the assembly itself to pick up language configurations
                // and load helper libraries. To do this, all DLR-based extensions are re-downloaded
                // (though it's only a browser-cache hit) and assemblies added to the "Assemblies"
                // list. 
                DownloadAndLoadExternalAssemblies(onComplete);
            } else {
#endif
                onComplete.Invoke();
#if SILVERLIGHT_3
            }
#endif
        }

#if SILVERLIGHT_3
        private static readonly object _lock = new object();

        // Downloads all ExtensionParts, loads all containing assemblies,
        // adds the assembly to the "Assemblies" list, and invokes "onComplete" 
        // when all ExtensionParts are downloaded and processed.
        void DownloadAndLoadExternalAssemblies(Action onComplete) {
            var xapvfs = (XapVirtualFilesystem) XapPAL.PAL.VirtualFilesystem;
	        var downloadQueue = new List<Uri>();
            foreach(var externalPart in DLRExtensionParts()) {
                downloadQueue.Add(externalPart.Source);
            }
            foreach (var uri in downloadQueue) {
                WebClient wc = new WebClient();
                wc.OpenReadCompleted += (sender, e) => {
                    // Make sure two handlers never step on eachother (could this even happen?)
                    lock (_lock) {
                        var sri = new StreamResourceInfo(e.Result, null);
                        foreach (var assembly in _assemblyNames) {
                            if (xapvfs.GetFile(sri, string.Format("{0}.dll", assembly)) != null) {
                                xapvfs.UsingStorageUnit(sri, () => Assemblies.Add(XapPAL.PAL.LoadAssembly(assembly)));
                            }
                        }
                        downloadQueue.Remove((Uri) e.UserState);
                        if (downloadQueue.Count == 0) {
                            onComplete.Invoke();
                        }
                    }
                };
                wc.OpenReadAsync(uri, uri);
            }
        }
#endif

        // An application uses DLR ExternalParts when just one DLR/Language
        // external part is referenced.
        bool UsesDLRExternals() {
#if SILVERLIGHT_3
            foreach (var extensionPart in ExtensionParts()) {
                foreach (var name in _externalNames) {
                    if (Regex.IsMatch(extensionPart.Source.ToString(), name)) {
                        return true;
                    }
                }
            }
#endif
            return false;
        }

        // Get all AssemblyParts from the AppManifest, as loaded Assembly instances.
        public List<Assembly> AssemblyParts() {
            var result = new List<Assembly>();
            foreach (var part in Deployment.Current.Parts) {
                try {
                    result.Add(XapPAL.PAL.LoadAssembly(Path.GetFileNameWithoutExtension(part.Source)));
                } catch (Exception) {
                    // skip
                }
            }
            return result;
        }

#if SILVERLIGHT_3

        // ForEach ExtensionPart in the AppManifest (needed because
        // ExternalPart is a useless type and needs to be cast to ExtensionPart).
        IEnumerable<ExtensionPart> ExtensionParts() {
            foreach (var externalPart in Deployment.Current.ExternalParts) {
                yield return (ExtensionPart)externalPart;
            }
        }

        // Get all DLR/language-specific ExtensionParts.
        List<ExtensionPart> DLRExtensionParts() {
            var dlrExtensions = new List<ExtensionPart>();
            foreach (var extensionPart in ExtensionParts()) {
                foreach (var name in _externalNames) {
                    if (Regex.IsMatch(extensionPart.Source.ToString(), name)) {
                        dlrExtensions.Add(extensionPart);
                    }
                }
            }
            return dlrExtensions;
        }
#endif
    }
}
