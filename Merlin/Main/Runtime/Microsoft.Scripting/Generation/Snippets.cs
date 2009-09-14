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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Microsoft.Scripting.Utils;
using System.Collections.Generic;

namespace Microsoft.Scripting.Generation {

    // TODO: This should be a static class
    // TODO: simplify initialization logic & state
    public sealed class Snippets {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Snippets Shared = new Snippets();

        private Snippets() { }
        
        private int _methodNameIndex;

        private AssemblyGen _assembly;
        private AssemblyGen _debugAssembly;

        // TODO: options should be internal
        private string _snippetsDirectory;
        private bool _saveSnippets;

        /// <summary>
        /// Directory where snippet assembly will be saved if SaveSnippets is set.
        /// </summary>
        public string SnippetsDirectory {
            get { return _snippetsDirectory; }
        }

        /// <summary>
        /// Save snippets to an assembly (see also SnippetsDirectory, SnippetsFileName).
        /// </summary>
        public bool SaveSnippets {
            get { return _saveSnippets; }
        }

        private AssemblyGen GetAssembly(bool emitSymbols) {
            return (emitSymbols) ?
                GetOrCreateAssembly(emitSymbols,  ref _debugAssembly) :
                GetOrCreateAssembly(emitSymbols, ref _assembly);
        }

        private AssemblyGen GetOrCreateAssembly(bool emitSymbols, ref AssemblyGen assembly) {
            if (assembly == null) {
                string suffix = (emitSymbols) ? ".debug" : "";
                suffix += ".scripting";
                Interlocked.CompareExchange(ref assembly, CreateNewAssembly(suffix, emitSymbols), null);
            }
            return assembly;
        }

        private AssemblyGen CreateNewAssembly(string nameSuffix, bool emitSymbols) {
            string dir;

            if (_saveSnippets) {
                dir = _snippetsDirectory ?? Directory.GetCurrentDirectory();
            } else {
                dir = null;
            }

            string name = "Snippets" + nameSuffix;

            return new AssemblyGen(new AssemblyName(name), dir, ".dll", emitSymbols);
        }

        internal string GetMethodILDumpFile(MethodBase method) {
            string fullName = ((method.DeclaringType != null) ? method.DeclaringType.Name + "." : "") + method.Name;

            if (fullName.Length > 100) {
                fullName = fullName.Substring(0, 100);
            }

            string filename = String.Format("{0}_{1}.il", IOUtils.ToValidFileName(fullName), Interlocked.Increment(ref _methodNameIndex));

            string dir = _snippetsDirectory ?? Path.Combine(Path.GetTempPath(), "__DLRIL");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, filename);
        }

        public static void SetSaveAssemblies(bool enable, string directory) {
            //Set SaveAssemblies on for inner ring by calling SetSaveAssemblies via Reflection.
            Assembly core = typeof(Expression).Assembly;
            Type assemblyGen = core.GetType(typeof(Expression).Namespace + ".Compiler.AssemblyGen");
            //The type may not exist.
            if (assemblyGen != null) {
                MethodInfo configSaveAssemblies = assemblyGen.GetMethod("SetSaveAssemblies", BindingFlags.NonPublic | BindingFlags.Static);
                //The method may not exist.
                if (configSaveAssemblies != null) {
                    string[] coreAssemblyLocations = (string[])configSaveAssemblies.Invoke(null, new object[] { enable, directory });
                }
            }
            Shared.ConfigureSaveAssemblies(enable, directory);
        }

        private void ConfigureSaveAssemblies(bool enable, string directory) {
            _saveSnippets = enable;
            _snippetsDirectory = directory;
        }

        public static void SaveAndVerifyAssemblies() {
            if (!Shared.SaveSnippets) {
                return;
            }
            // Invoke the core AssemblyGen.SaveAssembliesToDisk via reflection to get the locations of assemlies
            // to be verified. Verify them using PEVerify.exe.
            // Do this before verifying outer ring assemblies because they will depend on
            // the core ones.
            // The order needs to be
            // 1) Save inner ring assemblies.
            // 2) Save outer ring assemblies. This has to happen before verifying inner ring assemblies because
            //    inner ring assemblies have dependency on outer ring assemlies via generated IL.
            // 3) Verify inner ring assemblies.
            // 4) Verify outer ring assemblies.
            Assembly core = typeof(Expression).Assembly;
            Type assemblyGen = core.GetType(typeof(Expression).Namespace + ".Compiler.AssemblyGen");
            //The type may not exist.
            string[] coreAssemblyLocations = null;
            if (assemblyGen != null) {
                MethodInfo saveAssemblies = assemblyGen.GetMethod("SaveAssembliesToDisk", BindingFlags.NonPublic | BindingFlags.Static);
                //The method may not exist.
                if (saveAssemblies != null) {
                    coreAssemblyLocations = (string[])saveAssemblies.Invoke(null, null);
                }
            }

            string[] outerAssemblyLocations = Shared.SaveAssemblies();

            if (coreAssemblyLocations != null) {
                foreach (var file in coreAssemblyLocations) {
                    AssemblyGen.PeVerifyAssemblyFile(file);
                }
            }
            //verify outer ring assemblies
            foreach (var file in outerAssemblyLocations) {
                AssemblyGen.PeVerifyAssemblyFile(file);
            }
        }

        // Return the assembly locations that need to be verified
        private string[] SaveAssemblies() {
            if (!SaveSnippets) {
                return new string[0];
            }

            List<string> assemlyLocations = new List<string>();

            // first save all assemblies to disk:
            if (_assembly != null) {
                string assemblyLocation = _assembly.SaveAssembly();
                if (assemblyLocation != null) {
                    assemlyLocations.Add(assemblyLocation);
                }
                _assembly = null;
            }

            if (_debugAssembly != null) {
                string debugAssemblyLocation = _debugAssembly.SaveAssembly();
                if (debugAssemblyLocation != null) {
                    assemlyLocations.Add(debugAssemblyLocation);
                }
                _debugAssembly = null;
            }

            return assemlyLocations.ToArray();
        }

        public DynamicILGen CreateDynamicMethod(string methodName, Type returnType, Type[] parameterTypes, bool isDebuggable) {

            ContractUtils.RequiresNotEmpty(methodName, "methodName");
            ContractUtils.RequiresNotNull(returnType, "returnType");
            ContractUtils.RequiresNotNullItems(parameterTypes, "parameterTypes");

            if (Snippets.Shared.SaveSnippets) {
                AssemblyGen assembly = GetAssembly(isDebuggable);
                TypeBuilder tb = assembly.DefinePublicType(methodName, typeof(object), false);
                MethodBuilder mb = tb.DefineMethod(methodName, CompilerHelpers.PublicStatic, returnType, parameterTypes);
                return new DynamicILGenType(tb, mb, mb.GetILGenerator());
            } else {
                DynamicMethod dm = RawCreateDynamicMethod(methodName, returnType, parameterTypes);
                return new DynamicILGenMethod(dm, dm.GetILGenerator());
            }
        }

        public TypeBuilder DefinePublicType(string name, Type parent) {
            return GetAssembly(false).DefinePublicType(name, parent, false);
        }

        public TypeGen DefineType(string name, Type parent, bool preserveName, bool emitDebugSymbols) {
            AssemblyGen ag = GetAssembly(emitDebugSymbols);
            TypeBuilder tb = ag.DefinePublicType(name, parent, preserveName);
            return new TypeGen(ag, tb);
        }

        internal DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes) {
            string uniqueName = name + "##" + Interlocked.Increment(ref _methodNameIndex);
            return RawCreateDynamicMethod(uniqueName, returnType, parameterTypes);
        }

        public TypeBuilder DefineDelegateType(string name) {
            AssemblyGen assembly = GetAssembly(false);
            return assembly.DefineType(
                name,
                typeof(MulticastDelegate),
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
                false
            );
        }

        private static readonly Type[] _DelegateCtorSignature = new Type[] { typeof(object), typeof(IntPtr) };

        public Type DefineDelegate(string name, Type returnType, params Type[] argTypes) {
            TypeBuilder tb = DefineDelegateType(name);
            tb.DefineConstructor(
                MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, 
                CallingConventions.Standard, _DelegateCtorSignature).SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
            tb.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, returnType, argTypes).SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
            return tb.CreateType();

        }

        internal bool IsSnippetsAssembly(Assembly asm) {
            return (_assembly != null && asm == _assembly.AssemblyBuilder) ||
                   (_debugAssembly != null && asm == _debugAssembly.AssemblyBuilder);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework")]
        private static DynamicMethod RawCreateDynamicMethod(string name, Type returnType, Type[] parameterTypes) {
#if SILVERLIGHT // Module-hosted DynamicMethod is not available in SILVERLIGHT
            return new DynamicMethod(name, returnType, parameterTypes);
#else
            //
            // WARNING: we set restrictedSkipVisibility == true  (last parameter)
            //          setting this bit will allow accessing nonpublic members
            //          for more information see http://msdn.microsoft.com/en-us/library/bb348332.aspx
            //
            return new DynamicMethod(name, returnType, parameterTypes, true);
#endif
        }
    }
}
