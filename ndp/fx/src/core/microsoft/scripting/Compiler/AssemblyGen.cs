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

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Dynamic.Utils;
using System.Security;
using System.Text;
using System.Threading;

namespace System.Linq.Expressions.Compiler {
    internal sealed class AssemblyGen {
        private readonly AssemblyBuilder _myAssembly;
        private readonly ModuleBuilder _myModule;
        private readonly bool _isDebuggable;

#if !SILVERLIGHT
        private readonly string _outFileName;       // can be null iff !SaveAndReloadAssemblies
        private readonly string _outDir;            // null means the current directory
#endif

        private int _index;

        internal bool IsDebuggable {
            get {
#if !SILVERLIGHT
                Debug.Assert(_isDebuggable == (_myModule.GetSymWriter() != null));
#endif
                return _isDebuggable;
            }
        }

        internal AssemblyGen(AssemblyName name, string outDir, string outFileExtension, bool isDebuggable, bool isUnsafe) {
            ContractUtils.RequiresNotNull(name, "name");

#if SILVERLIGHT  // AssemblyBuilderAccess.RunAndSave, Environment.CurrentDirectory
            _myAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            _myModule = _myAssembly.DefineDynamicModule(name.Name, isDebuggable);
#else
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
            CustomAttributeBuilder[] attributes;

            if (isUnsafe) {
                attributes = new CustomAttributeBuilder[0];
            } else {
                attributes = new CustomAttributeBuilder[] { 
                    new CustomAttributeBuilder(typeof(SecurityTransparentAttribute).GetConstructor(Type.EmptyTypes), new object[0])
                };
            }

            if (outDir != null) {
                _myAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave, outDir,
                    null, null, null, null, false, attributes);

                _myModule = _myAssembly.DefineDynamicModule(name.Name, _outFileName, isDebuggable);
            } else {                
                _myAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run, attributes);
                _myModule = _myAssembly.DefineDynamicModule(name.Name, isDebuggable);                
            }

            if (isUnsafe) {
                _myModule.SetCustomAttribute(
                    new CustomAttributeBuilder(
                        typeof(UnverifiableCodeAttribute).GetConstructor(Type.EmptyTypes), 
                        new object[0]
                    )
                );
            }

            _myAssembly.DefineVersionInfoResource();
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

            _myAssembly.SetCustomAttribute(new CustomAttributeBuilder(
               typeof(DebuggableAttribute).GetConstructor(argTypes), argValues)
            );

            _myModule.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(DebuggableAttribute).GetConstructor(argTypes), argValues)
            );
        }

#if !SILVERLIGHT // IResourceWriter
        internal void AddResourceFile(string name, string file, ResourceAttributes attribute) {
            IResourceWriter rw = _myModule.DefineResource(Path.GetFileName(file), name, attribute);

            string ext = Path.GetExtension(file);
            if (String.Equals(ext, ".resources", StringComparison.OrdinalIgnoreCase)) {
                ResourceReader rr = new ResourceReader(file);
                using (rr) {
                    System.Collections.IDictionaryEnumerator de = rr.GetEnumerator();

                    while (de.MoveNext()) {
                        string key = de.Key as string;
                        rw.AddResource(key, de.Value);
                    }
                }
            } else {
                rw.AddResource(name, File.ReadAllBytes(file));
            }
        }
#endif

        #region Save the assembly

        //Return the location of the saved assembly file.
        //The file location is used by PE verification in Microsoft.Scripting.
        internal string SaveAssembly() {
#if !SILVERLIGHT // AssemblyBuilder.Save
            _myAssembly.Save(_outFileName, PortableExecutableKinds.ILOnly, ImageFileMachine.I386);
            return Path.Combine(_outDir, _outFileName);
#else
            return null;
#endif
        }
        #endregion

        internal TypeBuilder DefinePublicType(string name, Type parent, bool preserveName) {
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

#if !SILVERLIGHT
        internal void SetEntryPoint(MethodInfo mi, PEFileKinds kind) {
            _myAssembly.SetEntryPoint(mi, kind);
        }
#endif

        internal AssemblyBuilder AssemblyBuilder {
            get { return _myAssembly; }
        }

        internal ModuleBuilder ModuleBuilder {
            get { return _myModule; }
        }
    }

    internal static class SymbolGuids {
        internal static readonly Guid LanguageType_ILAssembly =
            new Guid(-1358664493, -12063, 0x11d2, 0x97, 0x7c, 0, 160, 0xc9, 180, 0xd5, 12);

        internal static readonly Guid DocumentType_Text =
            new Guid(0x5a869d0b, 0x6611, 0x11d3, 0xbd, 0x2a, 0, 0, 0xf8, 8, 0x49, 0xbd);

        internal static readonly Guid LanguageVendor_Microsoft =
            new Guid(-1723120188, -6423, 0x11d2, 0x90, 0x3f, 0, 0xc0, 0x4f, 0xa3, 2, 0xa1);
    }
}

