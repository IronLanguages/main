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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Dynamic;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;

using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Runtime.Binding;

namespace IronPython.Runtime {
    
    /// <summary>
    /// Importer class - used for importing modules.  Used by Ops and __builtin__
    /// Singleton living on Python engine.
    /// </summary>
    public static class Importer {
        internal const string ModuleReloadMethod = "PerformModuleReload";

        #region Internal API Surface

        /// <summary>
        /// Gateway into importing ... called from Ops.  Performs the initial import of
        /// a module and returns the module.
        /// </summary>
        public static object Import(CodeContext/*!*/ context, string fullName, PythonTuple from, int level) {
            Exception exLast = PythonOps.SaveCurrentException();
            try {
                PythonContext pc = PythonContext.GetContext(context);

                if (level == -1) {
                    // no specific level provided, call the 4 param version so legacy code continues to work
                    return pc.OldImportSite.Target(
                        pc.OldImportSite,
                        context,
                        FindImportFunction(context), 
                        fullName, 
                        Builtin.globals(context), 
                        context.Scope.Dict, 
                        from
                    );
                }

                // relative import or absolute import, in other words:
                //
                // from . import xyz
                // or 
                // from __future__ import absolute_import
            
                return pc.ImportSite.Target(
                    pc.ImportSite,
                    context,
                    FindImportFunction(context), 
                    fullName, 
                    Builtin.globals(context), 
                    context.Scope.Dict, 
                    from, 
                    level
                );
            } finally {
                PythonOps.RestoreCurrentException(exLast);
            }

        }

        /// <summary>
        /// Gateway into importing ... called from Ops.  This is called after
        /// importing the module and is used to return individual items from
        /// the module.  The outer modules dictionary is then updated with the
        /// result.
        /// </summary>
        public static object ImportFrom(CodeContext/*!*/ context, object from, string name) {
            Exception exLast = PythonOps.SaveCurrentException();
            try {
                Scope scope = from as Scope;
                PythonType pt;
                NamespaceTracker nt;
                if (scope != null) {
                    object ret;
                    if (scope.TryGetName(SymbolTable.StringToId(name), out ret)) {
                        return ret;
                    }

                    object path;
                    List listPath;
                    if (scope.TryGetName(Symbols.Path, out path) && (listPath = path as List) != null) {
                        return ImportNestedModule(context, scope, name, listPath);
                    }
                } else if ((pt = from as PythonType) != null) {
                    PythonTypeSlot pts;
                    object res;
                    if (pt.TryResolveSlot(context, SymbolTable.StringToId(name), out pts) &&
                        pts.TryGetValue(context, null, pt, out res)) {
                        return res;
                    }
                } else if ((nt = from as NamespaceTracker) != null) {
                    object res = NamespaceTrackerOps.GetCustomMember(context, nt, name);
                    if (res != OperationFailed.Value) {
                        return res;
                    }
                } else {
                    // This is too lax, for example it allows from module.class import member
                    object ret;
                    if (PythonOps.TryGetBoundAttr(context, from, SymbolTable.StringToId(name), out ret)) {
                        return ret;
                    }
                }
            } finally {
                PythonOps.RestoreCurrentException(exLast);
            }
            throw PythonOps.ImportError("Cannot import name {0}", name);
        }

        private static object ImportModuleFrom(CodeContext/*!*/ context, object from, string name) {
            Scope scope = from as Scope;
            if (scope != null) {
                object path;
                List listPath;
                if (scope.TryGetName(Symbols.Path, out path) && (listPath = path as List) != null) {
                    return ImportNestedModule(context, scope, name, listPath);
                }
            }

            NamespaceTracker ns = from as NamespaceTracker;
            if (ns != null) {
                object val;
                if (ns.TryGetValue(SymbolTable.StringToId(name), out val)) {
                    return MemberTrackerToPython(context, val);
                }
            }

            throw PythonOps.ImportError("No module named {0}", name);
        }

        /// <summary>
        /// Called by the __builtin__.__import__ functions (general importing) and ScriptEngine (for site.py)
        /// 
        /// level indiciates whether to perform absolute or relative imports.
        ///     -1 indicates both should be performed
        ///     0 indicates only absolute imports should be performed
        ///     Positive numbers indicate the # of parent directories to search relative to the calling module
        /// </summary>        
        public static object ImportModule(CodeContext/*!*/ context, object globals, string/*!*/ modName, bool bottom, int level) {
            if (modName.IndexOf(Path.DirectorySeparatorChar) != -1) {
                throw PythonOps.ImportError("Import by filename is not supported.", modName);
            }

            string package = null;
            object attribute;
            if (TryGetGlobalValue(globals, Symbols.Package, out attribute)) {
                package = attribute as string;
                if (package == null && attribute != null) {
                    throw PythonOps.ValueError("__package__ set to non-string");
                }
            } else {
                package = null;
                if (level > 0) {
                    // explicit relative import, calculate and store __package__
                    object pathAttr, nameAttr;
                    if (TryGetGlobalValue(globals, Symbols.Name, out nameAttr) && nameAttr is string) {
                        if (TryGetGlobalValue(globals, Symbols.Path, out pathAttr)) {
                            ((IAttributesCollection)globals)[Symbols.Package] = nameAttr;
                        } else {
                            ((IAttributesCollection)globals)[Symbols.Package] = ((string)nameAttr).rpartition(".")[0];
                        }
                    }
                }
            }

            object newmod = null;
            string[] parts = modName.Split('.');
            string finalName = null;

            if (level != 0) {
                // try a relative import
            
                // if importing a.b.c, import "a" first and then import b.c from a
                string name;    // name of the module we are to import in relation to the current module
                Scope parentScope;
                List path;      // path to search
                if (TryGetNameAndPath(context, globals, parts[0], level, package, out name, out path, out parentScope)) {
                    finalName = name;
                    // import relative
                    if (!TryGetExistingOrMetaPathModule(context, name, path, out newmod)) {
                        newmod = ImportTopRelative(context, parts[0], name, path);
                        if (newmod != null && parentScope != null) {
                            parentScope.SetName(SymbolTable.StringToId(modName), newmod);
                        }
                    } else if (parts.Length == 1) {
                        // if we imported before having the assembly
                        // loaded and then loaded the assembly we want
                        // to make the assembly available now.

                        if (newmod is NamespaceTracker) {
                            PythonContext.EnsureModule(context).ShowCls = true;
                        }
                    }
                }
            }
            
            if (level <= 0) {
                // try an absolute import
                if (newmod == null) {
                    object parentPkg;
                    if (package != null && !PythonContext.GetContext(context).SystemStateModules.TryGetValue(package, out parentPkg)) {
                        Scope warnScope = new Scope();
                        warnScope.SetName(Symbols.File, package);
                        warnScope.SetName(Symbols.Name, package);
                        PythonOps.Warn(
                            new CodeContext(warnScope, context.LanguageContext), 
                            PythonExceptions.RuntimeWarning, 
                            "Parent module '{0}' not found while handling absolute import", 
                            package);
                    }

                    newmod = ImportTopAbsolute(context, parts[0]);
                    finalName = parts[0];
                    if (newmod == null) {
                        return null;
                    }
                }
            }
            
            // now import the a.b.c etc.  a needs to be included here
            // because the process of importing could have modified
            // sys.modules.
            object next = newmod;
            string curName = null;
            for (int i = 0; i < parts.Length; i++) {
                curName = i == 0 ? finalName : curName + "." + parts[i];
                object tmpNext;
                if (TryGetExistingModule(context, curName, out tmpNext)) {
                    next = tmpNext;
                    if (i == 0) {
                        // need to update newmod if we pulled it out of sys.modules
                        // just in case we're in bottom mode.
                        newmod = next;
                    }
                } else if(i != 0) {
                    // child module isn't loaded yet, import it.
                    next = ImportModuleFrom(context, next, parts[i]);
                } else {
                    // top-level module doesn't exist in sys.modules, probably
                    // came from some weird meta path hook.
                    newmod = next;
                }
            }

            return bottom ? next : newmod;
        }

        private static object ImportTopRelative(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ full, List/*!*/ path) {
            object importedScope = ImportFromPath(context, name, full, path);
            if (importedScope != null) {
                context.Scope.SetName(SymbolTable.StringToId(name), importedScope);
            }
            return importedScope;
        }
        
        /// <summary>
        /// Interrogates the importing module for __name__ and __path__, which determine
        /// whether the imported module (whose name is 'name') is being imported as nested
        /// module (__path__ is present) or as sibling.
        /// 
        /// For sibling import, the full name of the imported module is parent.sibling
        /// For nested import, the full name of the imported module is parent.module.nested
        /// where parent.module is the mod.__name__
        /// </summary>
        /// <param name="context"></param>
        /// <param name="globals">the globals dictionary</param>
        /// <param name="name">Name of the module to be imported</param>
        /// <param name="full">Output - full name of the module being imported</param>
        /// <param name="path">Path to use to search for "full"</param>
        /// <param name="level">the import level for relaive imports</param>
        /// <param name="parentScope">the parent scope</param>
        /// <param name="package">the global __package__ value</param>
        /// <returns></returns>
         private static bool TryGetNameAndPath(CodeContext/*!*/ context, object globals, string name, int level, string package, out string full, out List path, out Scope parentScope) {
           Debug.Assert(level != 0);   // shouldn't be here for absolute imports

            // Unless we can find enough information to perform relative import,
            // we are going to import the module whose name we got
            full = name;
            path = null;
            parentScope = null;

            // We need to get __name__ to find the name of the imported module.
            // If absent, fall back to absolute import
            object attribute;

            if (!TryGetGlobalValue(globals, Symbols.Name, out attribute)) {
                return false;
            }

            // And the __name__ needs to be string
            string modName = attribute as string;
            if (modName == null) {
                return false;
            }
           
            // If the module has __path__ (and __path__ is list), nested module is being imported
            // otherwise, importing sibling to the importing module
            if (package == null && TryGetGlobalValue(globals, Symbols.Path, out attribute) && (path = attribute as List) != null) {
                // found __path__, importing nested module. The actual name of the nested module
                // is the name of the mod plus the name of the imported module
                if (level == -1) {
                    // absolute import of some module
                    full = modName + "." + name;
                } else if (name == String.Empty) {
                    // relative import of ancestor
                    full = (StringOps.rsplit(modName, ".", level - 1)[0] as string);
                } else {
                    // relative import of some ancestors child
                    full = (StringOps.rsplit(modName, ".", level - 1)[0] as string) + "." + name;
                }
                return true;
            }
             
            // importing sibling. The name of the imported module replaces
            // the last element in the importing module name
            string[] names = modName.Split('.');
            if (names.Length == 1) {
                // name doesn't include dot, only absolute import possible
                if (level > 0) {
                    throw PythonOps.ValueError("Attempted relative import in non-package");
                }

                return false;
            }

            string pn;
            if (package == null) {
                pn = GetParentPackageName(level, names);
            } else {
                // __package__ doesn't include module name, so level is - 1.
                pn = GetParentPackageName(level - 1, package.Split('.'));
            }

            path = GetParentPathAndScope(context, pn, out parentScope);
            if (path != null) {
                if (String.IsNullOrEmpty(name)) {
                    full = pn;
                } else {
                    full = pn + "." + name;
                }
                return true;
            }

            if (level > 0) {
                throw PythonOps.SystemError("Parent module '{0}' not loaded, cannot perform relative import", pn);
            }
            // not enough information - absolute import
            return false;
        }

         private static string GetParentPackageName(int level, string[] names) {
             StringBuilder parentName = new StringBuilder(names[0]);

             if (level < 0) level = 1;
             for (int i = 1; i < names.Length - level; i++) {
                 parentName.Append('.');
                 parentName.Append(names[i]);
             }
             return parentName.ToString();
         }

        private static bool TryGetGlobalValue(object globals, SymbolId symbol, out object attribute) {
            IAttributesCollection attrGlobals = globals as IAttributesCollection;
            if (attrGlobals != null) {
                if (!attrGlobals.TryGetValue(symbol, out attribute)) {
                    return false;
                }
            } else {
                // Python doesn't allow imports from arbitrary user mappings.
                attribute = null;
                return false;
            }
            return true;
        }

        public static object ReloadModule(CodeContext/*!*/ context, Scope/*!*/ scope) {
            PythonContext pc = PythonContext.GetContext(context);

            PythonModule module = pc.GetReloadableModule(scope);

            // We created the module and it only contains Python code. If the user changes
            // __file__ we'll reload from that file. 
            string fileName = module.GetFile() as string;

            // built-in module:
            if (fileName == null) {
                ReloadBuiltinModule(context, module);
                return scope;
            }

            string name = module.GetName() as string;
            if (name != null) {
                List path = null;
                // find the parent module and get it's __path__ property
                int dotIndex = name.LastIndexOf('.');
                if (dotIndex != -1) {
                    Scope parentScope;
                    path = GetParentPathAndScope(context, name.Substring(0, dotIndex), out parentScope);
                }

                object reloaded;
                if (TryLoadMetaPathModule(context, module.GetName() as string, path, out reloaded) && reloaded != null) {
                    return scope;
                }

                List sysPath;
                if (PythonContext.GetContext(context).TryGetSystemPath(out sysPath)) {
                    object ret = ImportFromPathHook(context, name, name, sysPath, null);
                    if (ret != null) {
                        return ret;
                    }
                }
            }

            if (!pc.DomainManager.Platform.FileExists(fileName)) {
                throw PythonOps.SystemError("module source file not found");
            }

            SourceUnit sourceUnit = pc.CreateFileUnit(fileName, pc.DefaultEncoding, SourceCodeKind.File);
            pc.GetScriptCode(sourceUnit, name, ModuleOptions.None).Run(scope);
            return scope;
        }

        /// <summary>
        /// Given the parent module name looks up the __path__ property.
        /// </summary>
        private static List GetParentPathAndScope(CodeContext/*!*/ context, string/*!*/ parentModuleName, out Scope parentScope) {
            List path = null;
            object parentModule;
            parentScope = null;
            
            // Try lookup parent module in the sys.modules
            if (PythonContext.GetContext(context).SystemStateModules.TryGetValue(parentModuleName, out parentModule)) {
                // see if it's a module
                parentScope = parentModule as Scope;
                if (parentScope != null) {
                    object objPath;
                    // get its path as a List if it's there
                    if (parentScope.TryGetName(Symbols.Path, out objPath)) {
                        path = objPath as List;
                    }
                }
            }
            return path;
        }

        private static void ReloadBuiltinModule(CodeContext/*!*/ context, PythonModule/*!*/ module) {
            Assert.NotNull(module);
            Debug.Assert(module.GetName() is string, "Module is reloadable only if its name is a non-null string");
            Type type;

            string name = (string)module.GetName();
            PythonContext pc = PythonContext.GetContext(context);

            if (!pc.Builtins.TryGetValue(name, out type)) {
                throw new NotImplementedException();
            }

            // should be a built-in module which we can reload.
            Debug.Assert(module.Scope.Dict is PythonDictionary);
            Debug.Assert(((PythonDictionary)module.Scope.Dict)._storage is ModuleDictionaryStorage);

            ((ModuleDictionaryStorage)((PythonDictionary)module.Scope.Dict)._storage).Reload();
        }

        /// <summary>
        /// Trys to get an existing module and if that fails fall backs to searching 
        /// </summary>
        private static bool TryGetExistingOrMetaPathModule(CodeContext/*!*/ context, string fullName, List path, out object ret) {
            if (TryGetExistingModule(context, fullName, out ret)) {
                return true;
            }

            return TryLoadMetaPathModule(context, fullName, path, out ret);
        }

        /// <summary>
        /// Attempts to load a module from sys.meta_path as defined in PEP 302.
        /// 
        /// The meta_path provides a list of importer objects which can be used to load modules before
        /// searching sys.path but after searching built-in modules.
        /// </summary>
        private static bool TryLoadMetaPathModule(CodeContext/*!*/ context, string fullName, List path, out object ret) {
            List metaPath = PythonContext.GetContext(context).GetSystemStateValue("meta_path") as List;
            if (metaPath != null) {
                foreach (object importer in (IEnumerable)metaPath) {
                    if (FindAndLoadModuleFromImporter(context, importer, fullName, path, out ret)) {
                        return true;
                    }
                }
            }

            ret = null;
            return false;
        }

        /// <summary>
        /// Given a user defined importer object as defined in PEP 302 tries to load a module.
        /// 
        /// First the find_module(fullName, path) is invoked to get a loader, then load_module(fullName) is invoked
        /// </summary>
        private static bool FindAndLoadModuleFromImporter(CodeContext/*!*/ context, object importer, string fullName, List path, out object ret) {
            object find_module = PythonOps.GetBoundAttr(context, importer, SymbolTable.StringToId("find_module"));

            PythonContext pycontext = PythonContext.GetContext(context);
            object loader = pycontext.Call(context, find_module, fullName, path);
            if (loader != null) {
                object findMod = PythonOps.GetBoundAttr(context, loader, SymbolTable.StringToId("load_module"));
                ret = pycontext.Call(context, findMod, fullName);
                return ret != null;
            }

            ret = null;
            return false;
        }

        internal static bool TryGetExistingModule(CodeContext/*!*/ context, string/*!*/ fullName, out object ret) {
            // Python uses None/null as a key here to indicate a missing module
            if (PythonContext.GetContext(context).SystemStateModules.TryGetValue(fullName, out ret)) {
                return ret != null;
            }
            return false;
        }

        #endregion

        #region Private Implementation Details

        private static object ImportTopAbsolute(CodeContext/*!*/ context, string/*!*/ name) {
            object ret;
            if (TryGetExistingModule(context, name, out ret)) {
                if (IsReflected(ret)) {
                    // Even though we found something in sys.modules, we need to check if a
                    // clr.AddReference has invalidated it. So try ImportReflected again.
                    ret = ImportReflected(context, name) ?? ret;
                }

                NamespaceTracker rp = ret as NamespaceTracker;
                if (rp != null || ret == PythonContext.GetContext(context).ClrModule) {
                    PythonContext.EnsureModule(context).ShowCls = true;
                }

                return ret;
            }

            if (TryLoadMetaPathModule(context, name, null, out ret)) {
                return ret;
            }

            ret = ImportBuiltin(context, name);
            if (ret != null) return ret;

            List path;
            if (PythonContext.GetContext(context).TryGetSystemPath(out path)) {
                ret = ImportFromPath(context, name, name, path);
                if (ret != null) return ret;
            }

            ret = ImportReflected(context, name);
            if (ret != null) return ret;

            return null;
        }

        private static bool TryGetNestedModule(CodeContext/*!*/ context, Scope/*!*/ scope, string/*!*/ name, out object nested) {
            Assert.NotNull(context, scope, name);

            if (scope.TryGetName(SymbolTable.StringToId(name), out nested)) {
                if (nested is Scope) return true;

                // This allows from System.Math import *
                PythonType dt = nested as PythonType;
                if (dt != null && dt.IsSystemType) {
                    return true;
                }
            }
            return false;
        }

        private static object ImportNestedModule(CodeContext/*!*/ context, Scope/*!*/ scope, string name, List/*!*/ path) {
            object ret;

            PythonModule module = PythonContext.GetContext(context).EnsurePythonModule(scope);

            string fullName = CreateFullName(module.GetName() as string, name);

            if (TryGetExistingOrMetaPathModule(context, fullName, path, out ret)) {
                module.Scope.SetName(SymbolTable.StringToId(name), ret);
                return ret;
            }

            if (TryGetNestedModule(context, scope, name, out ret)) { return ret; }

            object importedScope = ImportFromPath(context, name, fullName, path);
            if (importedScope != null) {
                module.Scope.SetName(SymbolTable.StringToId(name), importedScope);
                return importedScope;
            }

            throw PythonOps.ImportError("cannot import {0} from {1}", name, module.GetName());
        }

        private static object FindImportFunction(CodeContext/*!*/ context) {
            object builtin, import;
            if (!context.GlobalScope.TryGetName(Symbols.Builtins, out builtin)) {
                builtin = PythonContext.GetContext(context).BuiltinModuleInstance;
            }

            Scope scope = builtin as Scope;
            if (scope != null && scope.TryGetName(Symbols.Import, out import)) {
                return import;
            }

            IAttributesCollection dict = builtin as IAttributesCollection;
            if (dict != null && dict.TryGetValue(Symbols.Import, out import)) {
                return import;
            }

            throw PythonOps.ImportError("cannot find __import__");
        }

        internal static object ImportBuiltin(CodeContext/*!*/ context, string/*!*/ name) {
            Assert.NotNull(context, name);

            PythonContext pc = PythonContext.GetContext(context);
            if (name == "sys") {
                return pc.SystemState;
            } else if (name == "clr") {
                PythonContext.EnsureModule(context).ShowCls = true;
                pc.SystemStateModules["clr"] = pc.ClrModule;
                return pc.ClrModule;
            }

            PythonModule mod = pc.CreateBuiltinModule(name);
            if (mod != null) {
                pc.PublishModule(name, mod);
                return mod.Scope;
            }

            return null;
        }

        private static object ImportReflected(CodeContext/*!*/ context, string/*!*/ name) {
            object ret;
            PythonContext pc = PythonContext.GetContext(context);
            if (!pc.DomainManager.Globals.TryGetName(SymbolTable.StringToId(name), out ret)) {
                ret = TryImportSourceFile(pc, name);
            }

            ret = MemberTrackerToPython(context, ret);
            if (ret != null) {
                PythonContext.GetContext(context).SystemStateModules[name] = ret;
            }
            return ret;
        }

        private static object MemberTrackerToPython(CodeContext/*!*/ context, object ret) {
            MemberTracker res = ret as MemberTracker;
            if (res != null) {
                PythonContext.EnsureModule(context).ShowCls = true;
                object realRes = res;

                switch (res.MemberType) {
                    case TrackerTypes.Type: realRes = DynamicHelpers.GetPythonTypeFromType(((TypeTracker)res).Type); break;
                    case TrackerTypes.Field: realRes = PythonTypeOps.GetReflectedField(((FieldTracker)res).Field); break;
                    case TrackerTypes.Event: realRes = PythonTypeOps.GetReflectedEvent((EventTracker)res); break;
                    case TrackerTypes.Method:
                        MethodTracker mt = res as MethodTracker;
                        realRes = PythonTypeOps.GetBuiltinFunction(mt.DeclaringType, mt.Name, new MemberInfo[] { mt.Method });
                        break;
                }

                ret = realRes;
            }
            return ret;
        }

        internal static Scope TryImportSourceFile(PythonContext/*!*/ context, string/*!*/ name) {
            var sourceUnit = TryFindSourceFile(context, name);
            if (sourceUnit == null || 
                GetFullPathAndValidateCase(context, Path.Combine(Path.GetDirectoryName(sourceUnit.Path), name + Path.GetExtension(sourceUnit.Path)), false) == null) {
                return null;
            }

            var scope = ExecuteSourceUnit(sourceUnit);
            if (sourceUnit.LanguageContext != context) {
                // foreign language, we should publish in sys.modules too
                context.SystemStateModules[name] = scope;
            }
            sourceUnit.LanguageContext.DomainManager.Globals.SetName(SymbolTable.StringToId(name), scope);
            return scope;
        }

        internal static Scope ExecuteSourceUnit(SourceUnit/*!*/ sourceUnit) {
            ScriptCode compiledCode = sourceUnit.Compile();
            Scope scope = compiledCode.CreateScope();
            compiledCode.Run(scope);
            return scope;
        }

        internal static SourceUnit TryFindSourceFile(PythonContext/*!*/ context, string/*!*/ name) {
            List paths;
            if (!context.TryGetSystemPath(out paths)) {
                return null;
            }

            foreach (object dirObj in paths) {
                string directory = dirObj as string;
                if (directory == null) continue;  // skip invalid entries

                string candidatePath = null;
                LanguageContext candidateLanguage = null;
                foreach (string extension in context.DomainManager.Configuration.GetFileExtensions()) {
                    string fullPath;

                    try {
                        fullPath = Path.Combine(directory, name + extension);
                    } catch (ArgumentException) {
                        // skip invalid paths
                        continue;
                    }

                    if (context.DomainManager.Platform.FileExists(fullPath)) {
                        if (candidatePath != null) {
                            throw PythonOps.ImportError(String.Format("Found multiple modules of the same name '{0}': '{1}' and '{2}'",
                                name, candidatePath, fullPath));
                        }

                        candidatePath = fullPath;
                        candidateLanguage = context.DomainManager.GetLanguageByExtension(extension);
                    }
                }

                if (candidatePath != null) {
                    return candidateLanguage.CreateFileUnit(candidatePath);
                }
            }

            return null;
        }

        private static bool IsReflected(object module) {
            // corresponds to the list of types that can be returned by ImportReflected
            return module is MemberTracker
                || module is PythonType
                || module is ReflectedEvent
                || module is ReflectedField
                || module is BuiltinFunction;
        }

        private static string CreateFullName(string/*!*/ baseName, string name) {
            if (baseName == null || baseName.Length == 0 || baseName == "__main__") {
                return name;
            }
            return baseName + "." + name;
        }

        #endregion

        private static object ImportFromPath(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ fullName, List/*!*/ path) {
            return ImportFromPathHook(context, name, fullName, path, LoadFromDisk);
        }

        private static object ImportFromPathHook(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ fullName, List/*!*/ path, Func<CodeContext, string, string, string, object> defaultLoader) {
            Assert.NotNull(context, name, fullName, path);

            IDictionary<object, object> importCache = PythonContext.GetContext(context).GetSystemStateValue("path_importer_cache") as IDictionary<object, object>;

            if (importCache == null) {
                return null;
            }

            foreach (object dirname in (IEnumerable)path) {
                string str = dirname as string;

                if (str != null || (Converter.TryConvertToString(dirname, out str) && str != null)) {  // ignore non-string
                    object importer;
                    if (!importCache.TryGetValue(str, out importer)) {
                        importCache[str] = importer = FindImporterForPath(context, str);
                    }

                    if (importer != null) {
                        // user defined importer object, get the loader and use it.
                        object ret;
                        if (FindAndLoadModuleFromImporter(context, importer, fullName, null, out ret)) {
                            return ret;
                        }
                    } else if(defaultLoader != null) {
                        object res = defaultLoader(context, name, fullName, str);
                        if (res != null) {
                            return res;
                        }
                    }
                }
            }
            
            return null;
        }

        private static object LoadFromDisk(CodeContext context, string name, string fullName, string str) {
            // default behavior
            PythonModule module;
            string pathname = Path.Combine(str, name);

            module = LoadPackageFromSource(context, fullName, pathname);
            if (module != null) {
                return module.Scope;
            }

            string filename = pathname + ".py";
            module = LoadModuleFromSource(context, fullName, filename);
            if (module != null) {
                return module.Scope;
            }
            return null;
        }

        /// <summary>
        /// Finds a user defined importer for the given path or returns null if no importer
        /// handles this path.
        /// </summary>
        private static object FindImporterForPath(CodeContext/*!*/ context, string dirname) {
            List pathHooks = PythonContext.GetContext(context).GetSystemStateValue("path_hooks") as List;

            foreach (object hook in (IEnumerable)pathHooks) {
                try {
                    object handler = PythonCalls.Call(context, hook, dirname);

                    if (handler != null) {
                        return handler;
                    }
                } catch (ImportException) {
                    // we can't handle the path
                }
            }

#if !SILVERLIGHT    // DirectoryExists isn't implemented on Silverlight
            if (!context.LanguageContext.DomainManager.Platform.DirectoryExists(dirname)) {
                return new NullImporter(dirname);
            }
#endif

            return null;
        }

        private static PythonModule LoadModuleFromSource(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ path) {
            Assert.NotNull(context, name, path);

            PythonContext pc = PythonContext.GetContext(context);

            string fullPath = GetFullPathAndValidateCase(pc, path, false);
            if (fullPath == null || !pc.DomainManager.Platform.FileExists(fullPath)) {
                return null;
            }

            SourceUnit sourceUnit = pc.CreateFileUnit(fullPath, pc.DefaultEncoding, SourceCodeKind.File);
            return LoadFromSourceUnit(context, sourceUnit, name, sourceUnit.Path);
        }

        private static string GetFullPathAndValidateCase(LanguageContext/*!*/ context, string path, bool isDir) {
#if !SILVERLIGHT
            // check for a match in the case of the filename, unfortunately we can't do this
            // in Silverlight becauase there's no way to get the original filename.

            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            string dir = Path.GetDirectoryName(path);
            if (!pal.DirectoryExists(dir)) {
                return null;
            }

            try {
                string file = Path.GetFileName(path);
                string[] files = isDir ? pal.GetDirectories(dir, file) : pal.GetFiles(dir, file);                

                if (files.Length != 1 || Path.GetFileName(files[0]) != file) {
                    return null;
                }

                return Path.GetFullPath(files[0]);
            } catch (IOException) {
                return null;
            }
#else
            return path;
#endif
        }

        internal static PythonModule LoadPackageFromSource(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ path) {
            Assert.NotNull(context, name, path);

            path = GetFullPathAndValidateCase(PythonContext.GetContext(context), path, true);
            if (path == null) {
                return null;
            }

            return LoadModuleFromSource(context, name, Path.Combine(path, "__init__.py"));
        }

        private static PythonModule/*!*/ LoadFromSourceUnit(CodeContext/*!*/ context, SourceUnit/*!*/ sourceCode, string/*!*/ name, string/*!*/ path) {
            Assert.NotNull(sourceCode, name, path);
            return PythonContext.GetContext(context).CompileModule(path, name, sourceCode, ModuleOptions.Initialize | ModuleOptions.Optimized);
        }
    }
}
