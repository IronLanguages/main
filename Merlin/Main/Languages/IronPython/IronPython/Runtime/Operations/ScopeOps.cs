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
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Represents functionality that is exposed on PythonModule's but not exposed on the common ScriptModule
    /// class.
    /// </summary>
    public static class ScopeOps {
        [StaticExtensionMethod]
        public static Scope/*!*/ __new__(CodeContext/*!*/ context, PythonType/*!*/ cls, params object[]/*!*/ args\u00F8) {
            Scope res;
            if (cls == TypeCache.Module) {
                res = new Scope(PythonDictionary.MakeSymbolDictionary());
            } else if (cls.IsSubclassOf(TypeCache.Module)) {
                res = (Scope)cls.CreateInstance(context);
            } else {
                throw PythonOps.TypeError("{0} is not a subtype of module", cls.Name);
            }

            PythonContext.GetContext(context).CreateModule(null, res, null, ModuleOptions.None);
            res.Clear();
            return res;
        }

        [StaticExtensionMethod]
        public static Scope/*!*/ __new__(CodeContext/*!*/ context, PythonType/*!*/ cls, [ParamDictionary]PythonDictionary kwDict\u00F8, params object[]/*!*/ args\u00F8) {
            return __new__(context, cls, args\u00F8);
        }

        public static void __init__(Scope/*!*/ scope, string name) {
            __init__(scope, name, null);
        }

        public static void __init__(Scope/*!*/ scope, string name, string documentation) {
            scope.SetName(Symbols.Name, name);

            if (documentation != null) {
                scope.SetName(Symbols.Doc, documentation);
            }
        }

        public static object __getattribute__(Scope/*!*/ self, string name) {
            SymbolId si = SymbolTable.StringToId(name);
            object res;
            if (self.TryGetName(si, out res)) {
                return res;
            }

            throw PythonOps.AttributeErrorForMissingAttribute("module", si);
        }

        public static void __setattr__(Scope/*!*/ self, string name, object value) {
            self.SetName(SymbolTable.StringToId(name), value);
        }

        public static void __delattr__(Scope/*!*/ self, string name) {
            SymbolId si = SymbolTable.StringToId(name);
            if (!self.TryRemoveName(si)) {
                throw PythonOps.AttributeErrorForMissingAttribute("module", si);
            }
        }

        public static string/*!*/ __repr__(Scope/*!*/ scope) {
            return __str__(scope);
        }

        public static string/*!*/ __str__(Scope/*!*/ scope) {
            PythonModule module = DefaultContext.DefaultPythonContext.EnsurePythonModule(scope);
            string file = module.GetFile() as string;
            string name = module.GetName() as string ?? "?";

            if (file == null) {
                return String.Format("<module '{0}' (built-in)>", name);
            }
            return String.Format("<module '{0}' from '{1}'>", name, file);
        }

        [SpecialName, PropertyMethod]
        public static IAttributesCollection/*!*/ Get__dict__(Scope/*!*/ scope) {
            return new PythonDictionary(new GlobalScopeDictionaryStorage(scope));
        }

        [SpecialName, PropertyMethod]
        public static IAttributesCollection Set__dict__(Scope/*!*/ scope, object value) {
            throw PythonOps.TypeError("readonly attribute");
        }

        [SpecialName, PropertyMethod]
        public static IAttributesCollection Delete__dict__(Scope/*!*/ scope) {
            throw PythonOps.TypeError("can't set attributes of built-in/extension type 'module'");
        }

        [SpecialName]
        public static IList<object>/*!*/ GetMemberNames(CodeContext/*!*/ context, Scope/*!*/ scope) {
            bool showCls = PythonOps.IsClsVisible(context);

            List<object> ret = new List<object>();
            foreach (KeyValuePair<object, object> kvp in scope.Dict) {
                if (showCls || kvp.Value != Uninitialized.Instance) {
                    if (kvp.Key is SymbolId) {
                        ret.Add(SymbolTable.IdToString((SymbolId)kvp.Key));
                    } else {
                        ret.Add(kvp.Key);
                    }
                }
            }
            return ret;
        }

        [SpecialName]
        public static object GetCustomMember(CodeContext/*!*/ context, Scope/*!*/ scope, string name) {
            object value;
            if (scope.TryGetName(SymbolTable.StringToId(name), out value)) {
                if (value != Uninitialized.Instance) {
                    return value;
                }
            }
            return OperationFailed.Value;
        }

        [SpecialName]
        public static void SetMember(CodeContext/*!*/ context, Scope/*!*/ scope, string name, object value) {
            if (name == "__dict__") {
                Set__dict__(scope, value);
            } else {
                PythonContext pc = (PythonContext)context.LanguageContext;
                PythonModule pm = pc.GetPythonModule(scope);
                if (pm != null) {
                    pm.OnModuleChange(new ModuleChangeEventArgs(SymbolTable.StringToId(name), ModuleChangeType.Set, value));
                }

                scope.SetName(SymbolTable.StringToId(name), value);
            }
        }

        [SpecialName]
        public static bool DeleteMember(CodeContext/*!*/ context, Scope/*!*/ scope, string name) {
            if (scope.TryRemoveName(SymbolTable.StringToId(name))) {
                PythonContext pc = (PythonContext)context.LanguageContext;
                PythonModule pm = pc.GetPythonModule(scope);
                if (pm != null) {
                    pm.OnModuleChange(new ModuleChangeEventArgs(SymbolTable.StringToId(name), ModuleChangeType.Delete));
                }

                return true;
            }

            return false;
        }
    }
}
