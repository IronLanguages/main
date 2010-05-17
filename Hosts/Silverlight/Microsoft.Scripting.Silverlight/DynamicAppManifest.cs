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

#if CLR2
using Microsoft.Scripting.Utils;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace Microsoft.Scripting.Silverlight {
    public class DynamicAppManifest {

#if SILVERLIGHT_3
        /// <summary>
        /// List of valid assemblies to be inside Microsoft.Scripting.slvx
        /// </summary>
        private static List<string> _assemblyNames = new List<string>() {
            "Microsoft.Scripting.ExtensionAttribute",
            "Microsoft.Scripting.Core",
            "Microsoft.Scripting",
            "Microsoft.Dynamic",
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
        /// List of all dynamic language extensions found in the AppManifest.
        /// </summary>
        public List<Uri> Extensions { get; private set; }

        /// <summary>
        /// "Initializes "Assemblies" with any AssemblyParts, and "Extensions"
        /// with any ExtensionParts
        /// </summary>
        public DynamicAppManifest() {
            Assemblies = AssemblyParts();
#if SILVERLIGHT_3
            Extensions = DLRExtensionParts();
#else
            Extensions = new List<Uri>();
#endif
        }

        /// <summary>
        /// Get all AssemblyParts from the AppManifest, as loaded Assembly instances.
        /// </summary>
        public List<Assembly> AssemblyParts() {
            var result = new List<Assembly>();
            foreach (var part in Deployment.Current.Parts) {
                try {
                    result.Add(XapPAL.PAL.LoadAssembly(Path.GetFileNameWithoutExtension(part.Source)));
                } catch (Exception) {
                    // skip
                    // TODO this shouldn't catch all exceptions ... just any 
                    // exceptions that can be thrown the try block ...
                }
            }
            return result;
        }

#if SILVERLIGHT_3
        /// <summary>
        /// Get all DLR/language-specific ExtensionParts, which is just the
        /// extension containing the Microsoft.* assemblies.
        /// </summary>
        /// <returns></returns>
        private List<Uri> DLRExtensionParts() {
            var dlrExtensions = new List<Uri>();
            if (Type.GetType("System.Windows.ExtensionPart, System.Windows, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e") != null) {
                foreach (var externalPart in Deployment.Current.ExternalParts) {
                    var extensionPart = (ExtensionPart)externalPart;
                    if (Regex.IsMatch(extensionPart.Source.ToString(), "Microsoft.Scripting.slvx")) {
                        dlrExtensions.Add(extensionPart.Source);
                    }
                }
            }
            return dlrExtensions;
        }
#endif
    }
}
