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

        public static string GetFileContents(string relativePath) {
            return GetFileContents(new Uri(NormalizePath(relativePath), UriKind.Relative));
        }

        public static string GetFileContents(Uri relativeUri) {
            Stream stream = GetFile(relativeUri);
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
            return GetFile(new Uri(NormalizePath(relativePath), UriKind.Relative));
        }

        public static Stream GetFile(Uri relativeUri) {
            StreamResourceInfo sri = Application.GetResourceStream(relativeUri);
            return (sri != null) ? sri.Stream : null;
        }

        public static string NormalizePath(string path) {
            // files are stored in the XAP using forward slashes
            return path.Replace(Path.DirectorySeparatorChar, '/');
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
    }
}
