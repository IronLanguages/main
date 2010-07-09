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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Reflection;

namespace Chiron {
    /// <summary>
    /// XAP file builder for dynamic language applications
    /// Needs to know how to insert assemblies for the language
    /// </summary>
    static class XapBuilder {

        public static byte[] XapToMemory(string dir) {
            MemoryStream ms = new MemoryStream();
            XapFiles(new ZipArchive(ms, FileAccess.Write), dir);
            return ms.ToArray();
        }

        public static void XapToDisk(string dir, string xapfile) {
            XapFiles(new ZipArchive(xapfile, FileAccess.Write), dir);
        }

        static void XapFiles(ZipArchive zip, string dir) {
            ICollection<LanguageInfo> langs;
            IList<Uri> assemblies, externals;

            string manifestPath = Path.Combine(dir, "AppManifest.xaml");
            if (File.Exists(manifestPath)) {
                langs = FindSourceLanguages(dir);
                GetManifestParts(manifestPath, out assemblies, out externals);
            } else {
                using (Stream appManifest = zip.Create("AppManifest.xaml")) {
                    GenerateManifest(dir, out langs, out assemblies, out externals).Save(appManifest);
                }
            }

            AddAssemblies(zip, dir, assemblies);

            GenerateLanguagesConfig(zip, langs);

            AddPathDirectories(zip);

            // add files on disk last so they always overwrite generated files
            zip.CopyFromDirectory(dir, "");
            
            zip.Close();
        }

        // add directories that are on Chiron's path
        internal static void AddPathDirectories(ZipArchive zip) {
            if (Chiron.LocalPath != null) {
                foreach (var path in Chiron.LocalPath) {
                    string[] splitPath = path.Split(Path.DirectorySeparatorChar);
                    zip.CopyFromDirectory(path, splitPath[splitPath.Length - 1]);
                }
            }
        }

        // Get the URIs of the parts from the AppManifest.xaml file
        private static void GetManifestParts(string manifestPath, out IList<Uri> assemblies, out IList<Uri> externals) {
            assemblies = new List<Uri>();
            externals = new List<Uri>();
            
            XmlDocument doc = new XmlDocument();
            doc.Load(manifestPath);

            Action<XmlElement, IList<Uri>> processPart = (part, parts) => {
                string src = part.GetAttribute("Source");
                if (!string.IsNullOrEmpty(src)) {
                    parts.Add(new Uri(src, UriKind.RelativeOrAbsolute));
                }
            };

            foreach (XmlElement ap in doc.GetElementsByTagName("AssemblyPart"))
                processPart(ap, assemblies);

            foreach (XmlElement ap in doc.GetElementsByTagName("ExternalPart"))
                processPart(ap, externals);
        }

        internal static XmlDocument GenerateManifest(string dir) {
            ICollection<LanguageInfo> langs;
            IList<Uri> assemblies, externals;
            return GenerateManifest(dir, out langs, out assemblies, out externals);
        }

        private static XmlDocument GenerateManifest(string dir, out ICollection<LanguageInfo> langs, out IList<Uri> assemblies, out IList<Uri> externals) {
            langs = FindSourceLanguages(dir);
            if (Chiron.UseExtensions) {
                externals = GetLanguageExternals(langs);
                assemblies = new List<Uri>();
#if DEBUG
                // Put assembly in the XAP for easy debugging
                assemblies.Add(GetAssemblyUri("Microsoft.Scripting.Silverlight.dll"));
#endif
            } else {
                externals = new List<Uri>();
                assemblies = GetLanguageAssemblies(langs);
            }
            return Chiron.ManifestTemplate.Generate(assemblies, externals);
        }

        // Gets the list of DLR+language assemblies that should be added to the XAP
        private static IList<Uri> GetLanguageAssemblies(IEnumerable<LanguageInfo> langs) {
            IList<Uri> assemblies = new List<Uri>();
            assemblies.Add(GetAssemblyUri("Microsoft.Scripting.Silverlight.dll"));
#if CLR2
            assemblies.Add(GetAssemblyUri("Microsoft.Scripting.Core.dll"));
#else
            assemblies.Add(GetAssemblyUri("System.Numerics.dll"));
#endif
            assemblies.Add(GetAssemblyUri("Microsoft.Scripting.dll"));
            assemblies.Add(GetAssemblyUri("Microsoft.Dynamic.dll"));

            if (Chiron.DetectLanguage) {
                foreach (LanguageInfo lang in langs) {
                    foreach (string asm in lang.Assemblies) {
                        assemblies.Add(GetAssemblyUri(asm));
                    }
                }
            }
            return assemblies;
        }

        // Gets the list of extensions that will be automatically added to the XAP
        private static IList<Uri> GetLanguageExternals(IEnumerable<LanguageInfo> langs) {
            IList<Uri> extensions = new List<Uri>();
            extensions.Add(GetExternalUri("Microsoft.Scripting.slvx"));
            if (Chiron.DetectLanguage) {
                foreach (LanguageInfo lang in langs) {
                    foreach (string ext in lang.Extensions) {
                        extensions.Add(GetExternalUri(ext));
                    }
                }
            }
            return extensions;
        }

        private static Uri GetAssemblyUri(string path) {
            return GetBuildUri(path, false);
        }

        private static Uri GetExternalUri(string path) {
            return GetBuildUri(path, true);
        }

        private static Uri GetBuildUri(string path, bool prependUri) {
            Uri uri = new Uri(path, UriKind.RelativeOrAbsolute);
            if (prependUri) {
                string prefix = Chiron.UrlPrefix;
                if (prefix != "" && !IsPathRooted(uri))
                    uri = new Uri(prefix + path, UriKind.RelativeOrAbsolute);
            }
            return uri;
        }

        // returns true if the uri is absolute or starts with '/'
        // (i.e. absolute uri or absolute path)
        private static bool IsPathRooted(Uri uri) {
            return uri.IsAbsoluteUri || uri.OriginalString.StartsWith("/");
        }

        // Adds assemblies with relative paths into the XAP file
        private static void AddAssemblies(ZipArchive zip, string dir, IList<Uri> assemblyLocations) {
            foreach (Uri uri in assemblyLocations) {
                if (IsPathRooted(uri)) {
                    continue;
                }

                string targetPath = uri.OriginalString;
                string localPath = Path.Combine(dir, targetPath);

                if (!File.Exists(localPath)) {
                    localPath = Chiron.TryGetAssemblyPath(targetPath);

                    if (localPath == null) {
                        throw new ApplicationException("Could not find assembly: " + uri);
                    }
                }

                zip.CopyFromFile(localPath, targetPath);

                // Copy PDBs if available
                string pdbPath = Path.ChangeExtension(localPath, ".pdb");
                string pdbTarget = Path.ChangeExtension(targetPath, ".pdb");
                if (File.Exists(pdbPath)) {
                    zip.CopyFromFile(pdbPath, pdbTarget);
                }
            }
        }

        // Generates languages.config file
        // this is needed by the DLR to load arbitrary DLR-based languages implementations
        private static void GenerateLanguagesConfig(ZipArchive zip, ICollection<LanguageInfo> langs) {
            bool needLangConfig = false;
            foreach (LanguageInfo lang in langs) {
                if (lang.LanguageContext != "") {
                    needLangConfig = true;
                    break;
                }
            }

            // Only need language configuration file for non-builtin languages
            if (needLangConfig) {
                Stream outStream = zip.Create("languages.config");
                StreamWriter writer = new StreamWriter(outStream);
                writer.WriteLine("<Languages>");

                foreach (LanguageInfo lang in langs) {
                    writer.WriteLine("  <Language");
                    writer.WriteLine("            names=\"{0}\"", lang.GetNames());
                    writer.WriteLine("            languageContext=\"{0}\"", lang.LanguageContext);
                    writer.WriteLine("            extensions=\"{0}\"", lang.GetExtensionsString());
                    writer.WriteLine("            assemblies=\"{0}\"", lang.GetAssemblyNames());
                    writer.WriteLine("            external=\"{0}\"", lang.External);
                    writer.WriteLine("  />");
                    
                }

                writer.WriteLine("</Languages>");
                writer.Close();
            }
        }

        // Scans the application's directory to find all files whose extension 
        // matches one of Chiron's known languages
        internal static ICollection<LanguageInfo> FindSourceLanguages(string dir) {
            Dictionary<LanguageInfo, bool> result = new Dictionary<LanguageInfo, bool>();

            if (Chiron.DetectLanguage) {
                foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories)) {
                    string ext = Path.GetExtension(file);
                    LanguageInfo lang;
                    if (Chiron.Languages.TryGetValue(ext.ToLower(), out lang)) {
                        result[lang] = true;
                    }
                }
            } else {
                return Chiron.Languages.Values;
            }

            return result.Keys;
        }
    }
}
