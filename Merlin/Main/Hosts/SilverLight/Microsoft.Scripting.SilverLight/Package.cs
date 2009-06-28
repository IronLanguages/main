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
using System.IO;
using System.Reflection;
using System.Windows.Resources;
using System.Windows;

namespace Microsoft.Scripting.Silverlight {
    public class Package {

        private const string _defaultEntryPoint = "app";

        private static List<string> _extensionUris = new List<string>() {
            "http://go.microsoft.com/fwlink/?LinkID=146361", // Microsoft.Scripting.slvx
            "http://go.microsoft.com/fwlink/?LinkID=146359", // IronRuby.slvx
            "http://go.microsoft.com/fwlink/?LinkID=146360", // IronPython.slvx
        };

        private static List<string> _languageAssemblyNames = new List<string>() {
            "IronRuby",
            "IronRuby.Libraries",
            "IronPython",
            "IronPython.Modules"
        };

        private static List<string> _dlrAssemblyNames = new List<String>()
        {
            "Microsoft.Scripting.Core",
            "Microsoft.Scripting",
            "Microsoft.Scripting.ExtensionAttribute",
            "Microsoft.Scripting.Silverlight"
        };

        private static List<StreamResourceInfo> _languageAssemblies = new List<StreamResourceInfo>();

        public static List<string> DLRAssemblyNames { get { return _dlrAssemblyNames; } }
        public static List<string> LanguageAssemblyNames { get { return _languageAssemblyNames; }}
        public static List<string> LanguageExtensionUris { get { return _extensionUris; }}
        public static List<StreamResourceInfo> LanguageAssemblies { get { return _languageAssemblies; } }

        public static string GetFileContents(string relativePath) {
            return GetFileContents(DynamicApplication.XapFile, relativePath);
        }

        public static string GetFileContents(Uri relativeUri) {
            return GetFileContents(DynamicApplication.XapFile, relativeUri);
        }

        public static string GetFileContents(StreamResourceInfo xap, string relativePath) {
            return GetFileContents(xap, new Uri(NormalizePath(relativePath), UriKind.Relative));
        }

        public static string GetFileContents(StreamResourceInfo xap, Uri relativeUri) {
            Stream stream = GetFile(xap, relativeUri);
            if (stream == null) {
                return null;
            }

            string result;
            using (StreamReader sr = new StreamReader(stream)) {
                result = sr.ReadToEnd();
            }
            return result;
        }

        public static Stream GetFile(string relativePath) {
            return GetFileInternal(DynamicApplication.XapFile, relativePath);
        }

        public static Stream GetFile(Uri relativeUri) {
            return GetFileInternal(DynamicApplication.XapFile, relativeUri);
        }
        
        public static Stream GetFile(StreamResourceInfo xap, string relativePath) {
            return GetFileInternal(xap, relativePath);
        }

        public static Stream GetFile(StreamResourceInfo xap, Uri relativeUri) {
            return GetFileInternal(xap, relativeUri);
        }

        private static Stream GetFileInternal(StreamResourceInfo xap, string relativePath) {
            return GetFileInternal(xap, new Uri(NormalizePath(relativePath), UriKind.Relative));
        }

        private static Stream GetFileInternal(StreamResourceInfo xap, Uri relativeUri) {
            StreamResourceInfo sri = null;
            if (xap == null) {
                sri = Application.GetResourceStream(relativeUri);
            } else {
                sri = Application.GetResourceStream(xap, relativeUri);
            }
            return (sri != null) ? sri.Stream : null;
        }

        public static string NormalizePath(string path) {
            // files are stored in the XAP using forward slashes
            string normPath = path.Replace(Path.DirectorySeparatorChar, '/');

            // Application.GetResource doesn't like paths that start with ./ 
            // TODO try to get this fixed in SL
            if (normPath.StartsWith("./")) {
                normPath = normPath.Substring(2);
            }

            return normPath;
        }

        public static IEnumerable<Assembly> GetManifestAssemblies() {
            var result = new List<Assembly>();
            foreach (var part in Deployment.Current.Parts) {
                try {
                    result.Add(BrowserPAL.PAL.LoadAssembly(Path.GetFileNameWithoutExtension(part.Source)));
                } catch (Exception) {
                    // skip
                }
            }
            return result;
        }

        public static IEnumerable<Assembly> GetExtensionAssemblies() {
            var result = new List<Assembly>();
            foreach (var sri in LanguageAssemblies) {
                try {
                    result.Add(new AssemblyPart().Load(sri.Stream));
                } catch (Exception) {
                    // skip
                }
            }
            return result;
        }

        public static string GetEntryPointContents() {
            string code = null;

            if (DynamicApplication.Current.EntryPoint == null) {
                // try default entry point name w/ all extensions

                foreach (var language in DynamicApplication.Current.Runtime.Setup.LanguageSetups) {
                    foreach (var ext in language.FileExtensions) {
                        string file = _defaultEntryPoint + ext;
                        string contents = GetFileContents(file);
                        if (contents != null) {
                            if (DynamicApplication.Current.EntryPoint != null) {
                                throw new ApplicationException(string.Format("Application can only have one entry point, but found two: {0}, {1}", DynamicApplication.Current.EntryPoint, file));
                            }
                            DynamicApplication.Current.EntryPoint = file;
                            code = contents;
                        }
                    }
                }

                if (code == null) {
                    throw new ApplicationException(string.Format("Application must have an entry point called {0}.*, where * is the language's extension", _defaultEntryPoint));
                }
                return code;
            }

            // if name was supplied just download it
            code = GetFileContents(DynamicApplication.Current.EntryPoint);
            if (code == null) {
                throw new ApplicationException(string.Format("Could not find the entry point file {0} in the XAP", DynamicApplication.Current.EntryPoint));
            }
            return code;
        }

        public static bool ContainsDLRAssemblies(AssemblyPartCollection assemblyPartCollection) {
            var ret = assemblyPartCollection.Count != 0;
            
            foreach (var name in _dlrAssemblyNames) {
                bool found = false;
                foreach (AssemblyPart ap in assemblyPartCollection) {
                    if (string.Format("{0}.dll", name) == ap.Source) {
                        found = true;
                        break;
                    }
                }
                ret = ret && found;
            }

            bool langFound = false;
            foreach (AssemblyPart ap in assemblyPartCollection) {
                var rb = string.Format("{0}.dll", _languageAssemblyNames[0]);
                var py = string.Format("{0}.dll", _languageAssemblyNames[2]);
                if(ap.Source == rb || ap.Source == py) {
                    langFound = true;
                    break;
                }
            }

            return ret && langFound;
        }
    }
}
