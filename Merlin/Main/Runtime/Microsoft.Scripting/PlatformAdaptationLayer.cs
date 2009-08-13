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
using System.Linq.Expressions;
using System.Reflection;
using System.Security;

namespace Microsoft.Scripting {

#if SILVERLIGHT
    public class ExitProcessException : Exception {

        public int ExitCode { get { return exitCode; } }
        int exitCode;

        public ExitProcessException(int exitCode) {
            this.exitCode = exitCode;
        }
    }
#endif

    /// <summary>
    /// Abstracts system operations that are used by DLR and could potentially be platform specific.
    /// The host can implement its PAL to adapt DLR to the platform it is running on.
    /// For example, the Silverlight host adapts some file operations to work against files on the server.
    /// </summary>
    [Serializable]
    public class PlatformAdaptationLayer {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly PlatformAdaptationLayer Default = new PlatformAdaptationLayer();

#if SILVERLIGHT
        // this dictionary is readonly after initialization:
        private Dictionary<string, string> _assemblyFullNames = new Dictionary<string, string>();

        public PlatformAdaptationLayer() {
            LoadSilverlightAssemblyNameMapping();
        }

        // TODO: remove the need for this
        private void LoadSilverlightAssemblyNameMapping() {
            // non-trasparent assemblies
            AssemblyName platformKeyVer = new AssemblyName(typeof(object).Assembly.FullName);
            AddAssemblyMappings(platformKeyVer,
                "mscorlib",
                "System",
                "System.Core",
                "System.Net",
                "System.Runtime.Serialization",
                "System.ServiceModel.Web",
                "System.Windows",
                "System.Windows.Browser",
                "System.Xml",
                "Microsoft.VisualBasic"
            );

            // DLR + language assemblies
            AssemblyName languageKeyVer = new AssemblyName(typeof(PlatformAdaptationLayer).Assembly.FullName);
            AddAssemblyMappings(languageKeyVer, 
                "Microsoft.Scripting",
                "Microsoft.Scripting.ExtensionAttribute",
                "Microsoft.Scripting.Core",
                "Microsoft.Scripting.Silverlight",
                "IronPython",
                "IronPython.Modules",
                "IronRuby",
                "IronRuby.Libraries",
                "Microsoft.JScript.Compiler",
                "Microsoft.JScript.Runtime"
            );

            // transparent assemblies => same version as mscorlib but uses transparent key (same as languages)
            AssemblyName transparentKeyVer = new AssemblyName(typeof(object).Assembly.FullName);
            transparentKeyVer.SetPublicKeyToken(languageKeyVer.GetPublicKeyToken());
            AddAssemblyMappings(transparentKeyVer,
                "System.ServiceModel",
                "System.ServiceModel.Syndication",
                "System.Windows.Controls",
                "System.Windows.Controls.Data",
                "System.Windows.Controls.Data.Design",
                "System.Windows.Controls.Design",
                "System.Windows.Controls.Extended",
                "System.Windows.Controls.Extended.Design",
                "System.Xml.Linq",
                "System.Xml.Serialization"
            );
        }

        private void AddAssemblyMappings(AssemblyName keyVersion, params string[] names) {
            foreach (string asm in names) {
                keyVersion.Name = asm;
                _assemblyFullNames.Add(asm.ToLower(), keyVersion.FullName);
            }
        }

        protected string LookupFullName(string name) {
            AssemblyName asm = new AssemblyName(name);
            if (asm.Version != null || asm.GetPublicKeyToken() != null || asm.GetPublicKey() != null) {
                return name;
            }
            return _assemblyFullNames.ContainsKey(name.ToLower()) ? _assemblyFullNames[name.ToLower()] : name;
        }
#endif
        #region Assembly Loading

        public virtual Assembly LoadAssembly(string name) {
#if !SILVERLIGHT
            return Assembly.Load(name);
#else
            return Assembly.Load(LookupFullName(name));
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile")]
        public virtual Assembly LoadAssemblyFromPath(string path) {
#if !SILVERLIGHT
            return Assembly.LoadFile(path);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual void TerminateScriptExecution(int exitCode) {
#if !SILVERLIGHT
            System.Environment.Exit(exitCode);
#else
            throw new ExitProcessException(exitCode);
#endif
        }

        #endregion

        #region Virtual File System

        public virtual StringComparer PathComparer {
            get {
                return StringComparer.Ordinal;
            }
        }

        public virtual bool FileExists(string path) {
#if !SILVERLIGHT
            return File.Exists(path);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual bool DirectoryExists(string path) {
#if !SILVERLIGHT
            return Directory.Exists(path);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share) {
#if !SILVERLIGHT
            return new FileStream(path, mode, access, share);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) {
#if !SILVERLIGHT
            return new FileStream(path, mode, access, share, bufferSize);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual Stream OpenInputFileStream(string path) {
#if !SILVERLIGHT
            return new FileStream(path, FileMode.Open, FileAccess.Read);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual Stream OpenOutputFileStream(string path) {
#if !SILVERLIGHT
            return new FileStream(path, FileMode.Create, FileAccess.Write);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual string[] GetFiles(string path, string searchPattern) {
#if !SILVERLIGHT
            return Directory.GetFiles(path, searchPattern);            
#else
            throw new NotImplementedException();
#endif
        }

        public virtual string[] GetDirectories(string path, string searchPattern) {
#if !SILVERLIGHT
            return Directory.GetDirectories(path, searchPattern);
#else
            throw new NotImplementedException();
#endif
        }

        /// <exception cref="ArgumentException">Invalid path.</exception>
        public virtual string GetFullPath(string path) {
#if !SILVERLIGHT
            try {
                return Path.GetFullPath(path);
            } catch (Exception) {
                throw Error.InvalidPath();
            }
#else
            throw new NotImplementedException();
#endif
        }

        public virtual string GetFileName(string file) {
            return Path.GetFileName(file);
        }

        /// <exception cref="ArgumentException">Invalid path.</exception>
        public virtual bool IsAbsolutePath(string path) {
#if !SILVERLIGHT
            // GetPathRoot returns either :
            // "" -> relative to the current dir
            // "\" -> relative to the drive of the current dir
            // "X:" -> relative to the current dir, possibly on a different drive
            // "X:\" -> absolute
            return
                Environment.OSVersion.Platform != PlatformID.Unix && Path.GetPathRoot(path).EndsWith(@":\") ||
                Environment.OSVersion.Platform == PlatformID.Unix && Path.IsPathRooted(path);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual string CurrentDirectory {
            get {
#if !SILVERLIGHT
                return Environment.CurrentDirectory;
#else
                throw new NotImplementedException();
#endif
            }
        }

        #endregion

        #region Environmental Variables

        public virtual string GetEnvironmentVariable(string key) {
#if !SILVERLIGHT
            return Environment.GetEnvironmentVariable(key);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual void SetEnvironmentVariable(string key, string value) {
#if !SILVERLIGHT
            Environment.SetEnvironmentVariable(key, value);
#else
            throw new NotImplementedException();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual System.Collections.IDictionary GetEnvironmentVariables() {
#if !SILVERLIGHT
            return Environment.GetEnvironmentVariables();
#else
            throw new NotImplementedException();
#endif
        }

        #endregion
    }
}
