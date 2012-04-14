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
#if FEATURE_REFEMIT

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Security;
using System.Text;
using System.Threading;
using System.Linq;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {
    public sealed class AssemblyGen {
        private readonly AssemblyBuilder _myAssembly;
        private readonly ModuleBuilder _myModule;
        private readonly bool _isDebuggable;

#if FEATURE_FILESYSTEM
        private readonly string _outFileName;       // can be null iff !SaveAndReloadAssemblies
        private readonly string _outDir;            // null means the current directory
        private const string peverify_exe = "peverify.exe";
#endif

        private int _index;

        internal bool IsDebuggable {
            get {
#if FEATURE_PDBEMIT && !SILVERLIGHT
                Debug.Assert(_isDebuggable == (_myModule.GetSymWriter() != null));
#endif
                return _isDebuggable;
            }
        }

        public AssemblyGen(AssemblyName name, string outDir, string outFileExtension, bool isDebuggable) {
            ContractUtils.RequiresNotNull(name, "name");

#if FEATURE_FILESYSTEM
            if (outFileExtension == null) {
                outFileExtension = ".dll";
            }

            if (outDir != null) {
                try {
                    outDir = Path.GetFullPath(outDir);
                } catch (Exception) {
                    throw Error.InvalidOutputDir();
                }
                try {
                    Path.Combine(outDir, name.Name + outFileExtension);
                } catch (ArgumentException) {
                    throw Error.InvalidAsmNameOrExtension();
                }

                _outFileName = name.Name + outFileExtension;
                _outDir = outDir;
            }

            // mark the assembly transparent so that it works in partial trust:
            CustomAttributeBuilder[] attributes = new CustomAttributeBuilder[] { 
                new CustomAttributeBuilder(typeof(SecurityTransparentAttribute).GetConstructor(ReflectionUtils.EmptyTypes), new object[0]),
#if !CLR2 && !ANDROID
                new CustomAttributeBuilder(typeof(SecurityRulesAttribute).GetConstructor(new[] { typeof(SecurityRuleSet) }), new object[] { SecurityRuleSet.Level1 }),
#endif
            };

            if (outDir != null) {
#if !CLR2 && !ANDROID
                _myAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave, outDir, false, attributes);
#else
                //The API DefineDynamicAssembly is obsolete in Dev10.
                _myAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave, outDir, 
                    null, null, null, null, false, attributes);
#endif
                _myModule = _myAssembly.DefineDynamicModule(name.Name, _outFileName, isDebuggable);
            } else {
                _myAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run, attributes);
                _myModule = _myAssembly.DefineDynamicModule(name.Name, isDebuggable);
            }

            _myAssembly.DefineVersionInfoResource();
#else
            _myAssembly = ReflectionUtils.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            _myModule = _myAssembly.DefineDynamicModule(name.Name, isDebuggable);
#endif
            _isDebuggable = isDebuggable;

            if (isDebuggable) {
                SetDebuggableAttributes();
            }
        }

        internal void SetDebuggableAttributes() {
            DebuggableAttribute.DebuggingModes attrs =
                DebuggableAttribute.DebuggingModes.Default |
                DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints |
                DebuggableAttribute.DebuggingModes.DisableOptimizations;

            Type[] argTypes = new Type[] { typeof(DebuggableAttribute.DebuggingModes) };
            Object[] argValues = new Object[] { attrs };

            var debuggableCtor = typeof(DebuggableAttribute).GetConstructor(argTypes);

            _myAssembly.SetCustomAttribute(new CustomAttributeBuilder(debuggableCtor, argValues));
            _myModule.SetCustomAttribute(new CustomAttributeBuilder(debuggableCtor, argValues));
        }

        #region Dump and Verify

        public string SaveAssembly() {
#if FEATURE_FILESYSTEM
            _myAssembly.Save(_outFileName, PortableExecutableKinds.ILOnly, ImageFileMachine.I386);
            return Path.Combine(_outDir, _outFileName);
#else
            return null;
#endif
        }

        internal void Verify() {
#if FEATURE_FILESYSTEM
            PeVerifyThis();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static void PeVerifyAssemblyFile(string fileLocation) {
#if FEATURE_FILESYSTEM
            Debug.WriteLine("Verifying generated IL: " + fileLocation);
            string outDir = Path.GetDirectoryName(fileLocation);
            string outFileName = Path.GetFileName(fileLocation);
            string peverifyPath = FindPeverify();
            if (peverifyPath == null) {
                Debug.WriteLine("PEVerify not available");
                return;
            }

            int exitCode = 0;
            string strOut = null;
            string verifyFile = null;

            try {
                string pythonPath = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;

                string assemblyFile = Path.Combine(outDir, outFileName).ToLower(CultureInfo.InvariantCulture);
                string assemblyName = Path.GetFileNameWithoutExtension(outFileName);
                string assemblyExtension = Path.GetExtension(outFileName);
                Random rnd = new System.Random();

                for (int i = 0; ; i++) {
                    string verifyName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}{3}", assemblyName, i, rnd.Next(1, 100), assemblyExtension);
                    verifyName = Path.Combine(Path.GetTempPath(), verifyName);

                    try {
                        File.Copy(assemblyFile, verifyName);
                        verifyFile = verifyName;
                        break;
                    } catch (IOException) {
                    }
                }

                // copy any DLLs or EXEs created by the process during the run...
                CopyFilesCreatedSinceStart(Path.GetTempPath(), Environment.CurrentDirectory, outFileName);
                CopyDirectory(Path.GetTempPath(), pythonPath);
                if (Snippets.Shared.SnippetsDirectory != null && Snippets.Shared.SnippetsDirectory != Path.GetTempPath()) {
                    CopyFilesCreatedSinceStart(Path.GetTempPath(), Snippets.Shared.SnippetsDirectory, outFileName);
                }

                // /IGNORE=80070002 ignores errors related to files we can't find, this happens when we generate assemblies
                // and then peverify the result.  Note if we can't resolve a token thats in an external file we still
                // generate an error.
                ProcessStartInfo psi = new ProcessStartInfo(peverifyPath, "/IGNORE=80070002 \"" + verifyFile + "\"");
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                Process proc = Process.Start(psi);
                Thread thread = new Thread(
                    new ThreadStart(
                        delegate {
                            using (StreamReader sr = proc.StandardOutput) {
                                strOut = sr.ReadToEnd();
                            }
                        }
                        ));

                thread.Start();
                proc.WaitForExit();
                thread.Join();
                exitCode = proc.ExitCode;
                proc.Close();
            } catch (Exception e) {
                strOut = "Unexpected exception: " + e.ToString();
                exitCode = 1;
            }

            if (exitCode != 0) {
                Console.WriteLine("Verification failed w/ exit code {0}: {1}", exitCode, strOut);
                throw Error.VerificationException(
                    outFileName,
                    verifyFile,
                    strOut ?? "");
            }

            if (verifyFile != null) {
                File.Delete(verifyFile);
            }
#endif
        }

#if FEATURE_FILESYSTEM
        internal static string FindPeverify() {
            string path = System.Environment.GetEnvironmentVariable("PATH");
            string[] dirs = path.Split(';');
            foreach (string dir in dirs) {
                string file = Path.Combine(dir, peverify_exe);
                if (File.Exists(file)) {
                    return file;
                }
            }
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void PeVerifyThis() {
            string fileLocation = Path.Combine(_outDir, _outFileName);
            PeVerifyAssemblyFile(fileLocation);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void CopyFilesCreatedSinceStart(string pythonPath, string dir, string outFileName) {
            DateTime start = Process.GetCurrentProcess().StartTime;
            foreach (string filename in Directory.GetFiles(dir)) {
                FileInfo fi = new FileInfo(filename);
                if (fi.Name != outFileName) {
                    if (fi.LastWriteTime - start >= TimeSpan.Zero) {
                        try {
                            File.Copy(filename, Path.Combine(pythonPath, fi.Name), true);
                        } catch (Exception e) {
                            Console.WriteLine("Error copying {0}: {1}", filename, e.Message);
                        }
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void CopyDirectory(string to, string from) {
            foreach (string filename in Directory.GetFiles(from)) {
                FileInfo fi = new FileInfo(filename);
                string toFile = Path.Combine(to, fi.Name);
                FileInfo toInfo = new FileInfo(toFile);

                if (fi.Extension.ToLowerInvariant() == ".dll" || fi.Extension.ToLowerInvariant() == ".exe") {
                    if (!File.Exists(toFile) || toInfo.CreationTime != fi.CreationTime) {
                        try {
                            File.Copy(filename, toFile, true);
                        } catch (Exception e) {
                            Console.WriteLine("Error copying {0}: {1}", filename, e.Message);
                        }
                    }
                }
            }
        }
#endif
        #endregion

        public TypeBuilder DefinePublicType(string name, Type parent, bool preserveName) {
            return DefineType(name, parent, TypeAttributes.Public, preserveName);
        }

        internal TypeBuilder DefineType(string name, Type parent, TypeAttributes attr, bool preserveName) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(parent, "parent");

            StringBuilder sb = new StringBuilder(name);
            if (!preserveName) {
                int index = Interlocked.Increment(ref _index);
                sb.Append("$");
                sb.Append(index);
            }

            // There is a bug in Reflection.Emit that leads to 
            // Unhandled Exception: System.Runtime.InteropServices.COMException (0x80131130): Record not found on lookup.
            // if there is any of the characters []*&+,\ in the type name and a method defined on the type is called.
            sb.Replace('+', '_').Replace('[', '_').Replace(']', '_').Replace('*', '_').Replace('&', '_').Replace(',', '_').Replace('\\', '_');

            name = sb.ToString();

            return _myModule.DefineType(name, attr, parent);
        }

#if FEATURE_FILESYSTEM
        internal void SetEntryPoint(MethodInfo mi, PEFileKinds kind) {
            _myAssembly.SetEntryPoint(mi, kind);
        }
#endif

        public AssemblyBuilder AssemblyBuilder {
            get { return _myAssembly; }
        }

        public ModuleBuilder ModuleBuilder {
            get { return _myModule; }
        }

        private const MethodAttributes CtorAttributes = MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public;
        private const MethodImplAttributes ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed;
        private const MethodAttributes InvokeAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
        private const TypeAttributes DelegateAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass;
        private static readonly Type[] _DelegateCtorSignature = new Type[] { typeof(object), typeof(IntPtr) };

        public Type MakeDelegateType(string name, Type[] parameters, Type returnType) {
            TypeBuilder builder = DefineType(name, typeof(MulticastDelegate), DelegateAttributes, false);
            builder.DefineConstructor(CtorAttributes, CallingConventions.Standard, _DelegateCtorSignature).SetImplementationFlags(ImplAttributes);
            builder.DefineMethod("Invoke", InvokeAttributes, returnType, parameters).SetImplementationFlags(ImplAttributes);
            return builder.CreateType();
        }
    }
}

#endif