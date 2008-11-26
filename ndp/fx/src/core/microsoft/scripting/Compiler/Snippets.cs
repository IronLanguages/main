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

using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Dynamic.Utils;
using System.Threading;
using System.Dynamic;
using System.Globalization;
using System.Collections.Generic;

namespace System.Linq.Expressions.Compiler {

    // TODO: This should be a static class
    // TODO: simplify initialization logic & state
    internal sealed class Snippets {
        internal static readonly Snippets Shared = new Snippets();

        private Snippets() { }

        private int _methodNameIndex;

        private AssemblyGen _assembly;
        private AssemblyGen _unsafeAssembly;
        private AssemblyGen _debugAssembly;
        private AssemblyGen _unsafeDebugAssembly;

        // TODO: options should be internal
        private string _snippetsDirectory = null;
        private bool _saveSnippets = false;

        internal bool SaveSnippets {
            get { return _saveSnippets; }
        }

        /// <summary>
        /// Directory where snippet assembly will be saved if SaveSnippets is set.
        /// </summary>
        internal string SnippetsDirectory {
            get { return _snippetsDirectory; }
        }

        internal AssemblyGen GetAssembly(bool emitSymbols, bool isUnsafe) {
            // If snippets are not to be saved, we can merge unsafe and safe IL.
            if (isUnsafe && _saveSnippets) {
                return (emitSymbols) ?
                    GetOrCreateAssembly(emitSymbols, isUnsafe, ref _unsafeDebugAssembly) :
                    GetOrCreateAssembly(emitSymbols, isUnsafe, ref _unsafeAssembly);
            } else {
                return (emitSymbols) ?
                    GetOrCreateAssembly(emitSymbols, isUnsafe, ref _debugAssembly) :
                    GetOrCreateAssembly(emitSymbols, isUnsafe, ref _assembly);
            }
        }

        private AssemblyGen GetOrCreateAssembly(bool emitSymbols, bool isUnsafe, ref AssemblyGen assembly) {
            if (assembly == null) {
                string suffix = (emitSymbols) ? ".debug" : "" + (isUnsafe ? ".unsafe" : "");
                Interlocked.CompareExchange(ref assembly, CreateNewAssembly(suffix, emitSymbols, isUnsafe), null);
            }
            return assembly;
        }

        private AssemblyGen CreateNewAssembly(string nameSuffix, bool emitSymbols, bool isUnsafe) {
            string dir;

            if (_saveSnippets) {
                dir = _snippetsDirectory ?? Directory.GetCurrentDirectory();
            } else {
                dir = null;
            }

            string name = "Snippets" + nameSuffix;

            return new AssemblyGen(new AssemblyName(name), dir, ".dll", emitSymbols, isUnsafe);
        }

        internal string GetMethodILDumpFile(MethodBase method) {
            string fullName = ((method.DeclaringType != null) ? method.DeclaringType.Name + "." : "") + method.Name;

            if (fullName.Length > 100) {
                fullName = fullName.Substring(0, 100);
            }

            string filename = String.Format(
                CultureInfo.CurrentCulture, 
                "{0}_{1}.il", 
                Helpers.ToValidFileName(fullName), 
                Interlocked.Increment(ref _methodNameIndex)
            );

            string dir = _snippetsDirectory ?? Path.Combine(Path.GetTempPath(), "__DLRIL");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, filename);
        }

#if MICROSOFT_SCRIPTING_CORE
        // NOTE: this method is called through reflection from Microsoft.Scripting
        internal static void SetSaveAssemblies(string directory) {
            Shared.ConfigureSaveAssemblies(directory);
        }

        private void ConfigureSaveAssemblies(string directory) {
            _saveSnippets = true;
            _snippetsDirectory = directory;
        }
        
        // NOTE: this method is called through reflection from Microsoft.Scripting
        internal static string[] SaveAssemblies() {
            return Shared.DumpAssemblies();
        }

        //return the assembly locations that need to be verified
        private string[] DumpAssemblies() {
            if (!_saveSnippets) {
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

            if (_unsafeAssembly != null) {
                _unsafeAssembly.SaveAssembly();
            }

            if (_unsafeDebugAssembly != null) {
                _unsafeDebugAssembly.SaveAssembly();
            }

            _unsafeDebugAssembly = null;

            return assemlyLocations.ToArray();
        }
#endif
        internal DynamicILGen CreateDynamicMethod(string methodName, Type returnType, Type[] parameterTypes,
            bool isDebuggable) {

            ContractUtils.RequiresNotEmpty(methodName, "methodName");
            ContractUtils.RequiresNotNull(returnType, "returnType");
            ContractUtils.RequiresNotNullItems(parameterTypes, "parameterTypes");

            if (_saveSnippets) {
                AssemblyGen assembly = GetAssembly(isDebuggable, false);
                TypeBuilder tb = assembly.DefinePublicType(methodName, typeof(object), false);
                MethodBuilder mb = tb.DefineMethod(methodName, TypeUtils.PublicStatic, returnType, parameterTypes);
                return new DynamicILGenType(tb, mb, mb.GetILGenerator());
            } else {
                DynamicMethod dm = Helpers.CreateDynamicMethod(methodName, returnType, parameterTypes);
                return new DynamicILGenMethod(dm, dm.GetILGenerator());
            }
        }

        internal TypeBuilder DefinePublicType(string name, Type parent) {
            return GetAssembly(false, false).DefinePublicType(name, parent, false);
        }

        internal TypeBuilder DefineUnsafeType(string name, Type parent) {
            return GetAssembly(false, true).DefinePublicType(name, parent, false);
        }

        internal TypeBuilder DefineType(string name, Type parent, bool preserveName, bool isUnsafe, bool emitDebugSymbols) {
            AssemblyGen ag = GetAssembly(emitDebugSymbols, isUnsafe);
            return ag.DefinePublicType(name, parent, preserveName);
        }

        internal DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes) {
            string uniqueName = name + "##" + Interlocked.Increment(ref _methodNameIndex);
            return Helpers.CreateDynamicMethod(uniqueName, returnType, parameterTypes);
        }

        internal TypeBuilder DefineDelegateType(string name) {
            AssemblyGen assembly = GetAssembly(false, false);
            return assembly.DefineType(
                name,
                typeof(MulticastDelegate),
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
                false
            );
        }
    }
}
