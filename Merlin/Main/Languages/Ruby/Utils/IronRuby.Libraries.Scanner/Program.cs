/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using IronRuby;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using IronRuby.Builtins;

namespace IronRuby.Library.Scanner {
    static class ExtensionMethods {
        public static IEnumerable<T> SelectCustomAttributes<T>(this Type type) where T : Attribute {
            return type.GetCustomAttributes(typeof(T), false).Cast<T>();
        }

        public static T SelectAttribute<T>(this Type type) where T : Attribute {
            return type.GetCustomAttributes(typeof(T), false).Cast<T>().First();
        }

        public static bool IsClassOrModule(this Type type) {
            if (type.IsDefined(typeof(RubyClassAttribute), false)) {

                RubyClassAttribute classAttr = type.SelectAttribute<RubyClassAttribute>();

                // A RubyClass that is explicitly named
                if (!String.IsNullOrEmpty(classAttr.Name))
                    return true;

                // A RubyClass that defines a self-contained class
                if (classAttr.Extends == null)
                    return true;

                // A RubyClass that includes a module eg ArrayOps
                if (type.IsDefined(typeof(IncludesAttribute), false))
                    return true;

                // Filters out RubyClasses that extend a type that isn't a RubyClass eg TypeTrackerOps
                return classAttr.Extends.IsClassOrModule();

            } else if (type.IsDefined(typeof(RubyModuleAttribute), false)) {

                // Filter out well-known RubyClass names
                RubyModuleAttribute moduleAttr = type.SelectAttribute<RubyModuleAttribute>();

                if (   moduleAttr.Name == RubyClass.ClassSingletonName 
                    || moduleAttr.Name == RubyClass.ClassSingletonSingletonName 
                    || moduleAttr.Name == RubyClass.MainSingletonName)
                    return false;
                
                // Filters out extension modules like ArrayOps
                return moduleAttr.Extends == null;
            }
            return false;
        }

        public static IEnumerable<T> SelectCustomAttributes<T>(this MethodInfo method) where T : Attribute {
            return method.GetCustomAttributes(typeof(T), false).Cast<T>();
        }
    }

    class RubyClassInfo {
        public Type ClrType { get; set; }

        public delegate void Block(IEnumerable<RubyMethodAttribute> methods);

        public string Name {
            get {
                RubyModuleAttribute attr = ClrType.SelectAttribute<RubyModuleAttribute>();
                if (String.IsNullOrEmpty(attr.Name)) {
                    if (attr.Extends == null) {
                        return attr.Name ?? ClrType.Name;
                    } else {
                        if (attr.Extends.IsDefined(typeof(RubyClassAttribute), false)) {
                            return attr.Extends.SelectAttribute<RubyClassAttribute>().Name;
                        } else {
                            object x = attr.Extends;
                            return "<unknown>"; // String.Empty;
                        }
                    }
                } else {
                    return attr.Name;
                }
            }
        }

        private Type LookupExtensionModuleType(Type includeAttrType) {
            Type includedType;
            Program.ExtensionModules.TryGetValue(includeAttrType, out includedType);
            return includedType ?? includeAttrType;
        }

        private void GetMethodNames(Type t, Block accumulate) {
            var methods = (from m in t.GetMethods()
                           where m.IsDefined(typeof(RubyMethodAttribute), false)
                           select m.SelectCustomAttributes<RubyMethodAttribute>());

            // TODO: is there a LINQ-esque way of flattening these arrays?
            var flattened = new List<RubyMethodAttribute>();
            foreach (var methodBlock in methods) {
                foreach (var method in methodBlock) {
                    flattened.Add(method);
                }
            }

            accumulate(flattened);

            foreach (IncludesAttribute attr in t.SelectCustomAttributes<IncludesAttribute>())
                foreach (Type includeType in attr.Types)
                    GetMethodNames(LookupExtensionModuleType(includeType), accumulate);
        }

        private IEnumerable<string> GetMethodNames(RubyMethodAttributes methodType) {
            var result = new List<string>();
            GetMethodNames(ClrType, methods => 
                result.AddRange((from m in methods
                                 where m.MethodAttributes == methodType 
                                 select m.Name).Distinct()));
            result.Sort();
            return result;
        }

        public IEnumerable<string> InstanceMethods {
            get { return GetMethodNames(RubyMethodAttributes.PublicInstance); }
        }

        public IEnumerable<string> SingletonMethods {
            get { return GetMethodNames(RubyMethodAttributes.PublicSingleton); }
        }
    }

    class Program {
        static IEnumerable<RubyClassInfo> GetRubyTypes(Assembly a) {
            return from rci in
                        (from t in a.GetTypes()
                         where t.IsClassOrModule() 
                         select new RubyClassInfo { ClrType = t })
                   orderby rci.Name
                   select rci;
        }

        static Dictionary<Type, Type> GetExtensionModules(Assembly a) {
            var modules = from t in a.GetTypes()
                          where t.IsDefined(typeof(RubyModuleAttribute), false)
                          select new { Type = t, Attribute = t.SelectCustomAttributes<RubyModuleAttribute>().First() };

            var result = new Dictionary<Type, Type>();
            foreach(var m in modules)
                if (m.Attribute.Extends != null)
                    result[m.Attribute.Extends] = m.Type;
            return result;
        }

#if SIGNED
        const string RubyAssembly = @"IronRuby.Libraries, Version=0.4.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
#else
        const string RubyAssembly = @"IronRuby.Libraries, Version=0.4.0.0, Culture=neutral";
#endif
        internal static Dictionary<Type, Type> ExtensionModules;

        static List<string> ImplementedMethods = new List<string>();

        delegate IEnumerable<string> GetMethodsBlock(RubyClassInfo rci);

        static void DumpMethods(IEnumerable<RubyClassInfo> types, GetMethodsBlock getMethods) {
            foreach (RubyClassInfo rci in types) {
                if (!String.IsNullOrEmpty(rci.Name))
                    foreach (string methodName in getMethods(rci))
                        ImplementedMethods.Add(String.Format("{0}#{1}", rci.Name, methodName));
            }
        }

        static void Main(string[] args) {
            var name = new AssemblyName(RubyAssembly);
            var a = Assembly.Load(name);

            ExtensionModules = GetExtensionModules(a);
            var types = GetRubyTypes(a);

            DumpMethods(types, t => t.InstanceMethods);
            DumpMethods(types, t => t.SingletonMethods);

            ImplementedMethods.Sort();
            string current = null;
            foreach (string method in ImplementedMethods) {
                if (method != current) {
                    Console.WriteLine(method);
                    current = method;
                }
            }
        }
    }
}
