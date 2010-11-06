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
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using Microsoft.Scripting.Metadata;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;

namespace Metadata {
    public static class Program {
        public static int Counter_ExtensionMethods;
        public static int Counter_Fields;
        public static int Counter_Methods;
        public static int Counter_Properties;
        public static int Counter_Events;
        public static int Counter_Types;
        public static int Counter_MaxMapTablesSize = 0, Counter_TotalMapTablesSize = 0;
        public static int Counter_Files;
        public static TextWriter _output = Console.Out;
        public static bool Detailed = false;

        #region Extension Methods

        private static void DumpExtensionMethods(MetadataTables tables) {
            _output.WriteLine("Extension methods:");
            foreach (MethodDef em in tables.GetVisibleExtensionMethods()) {
                TypeDef td = em.FindDeclaringType();
                _output.WriteLine("{0}.{1}::{2}", td.Namespace, td.Name, em.Name);
            }
        }

        private static void EnumerateExtensionMethods(Module module) {
            foreach (MethodInfo em in module.GetVisibleExtensionMethods()) {
                Counter_ExtensionMethods++;
            }
        }

        private static void EnumerateExtensionMethods(MetadataTables tables) {
            foreach (MethodDef em in tables.GetVisibleExtensionMethods()) {
                if (tables.Module != null) {
                    var mb = tables.Module.ResolveMethod(em.Record.Token.Value);
                }
                Counter_ExtensionMethods++;
            }
        }

        #endregion

        #region Nested Types

        private static void DumpNestedTypes(MetadataTables tables) {
            _output.WriteLine("Type nesting: ");

            TypeNestings nesting = new TypeNestings(tables);

            _output.WriteLine("Total pairs: {0}",
                (from e in nesting.GetEnclosingTypes()
                 from n in nesting.GetNestedTypes(e)
                 select 1).Sum()
            );

            DumpNestedTypes(nesting, 0,
                // top-level:
                from TypeDef typeDef in nesting.GetEnclosingTypes()
                where !typeDef.Attributes.IsNested()
                select typeDef
            );
        }

        private static void DumpNestedTypes(TypeNestings nesting, int level, IEnumerable<TypeDef> enclosing) {
            string indent = new String('\t', level);
            foreach (var enc in enclosing) {
                int count;
                var nested = nesting.GetNestedTypes(enc, out count);
                if (count > 0) {
                    _output.WriteLine("{0}{1}: {2}", indent, enc.Name, count);
                    DumpNestedTypes(nesting, level + 1, nested);
                } else {
                    _output.WriteLine("{0}{1}", indent, enc.Name);
                }
            }
        }

        #endregion

        #region Namespace Tree

        private static void DumpNamespaceTree(NamespaceTreeNode node) {
            DumpNamespaceTree(0, node);
        }

        private static void DumpNamespaceTree(int level, NamespaceTreeNode node) {
            string indent = new String(' ', level * 2);
            _output.WriteLine("{0}{1}: {2}/{3}", 
                indent, 
                node.Name, 
                node.GetTypeDefs().Count(),
                (from def in node.GetTypeDefs() where (def.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public select def).Count()
            );

            foreach (var typeDef in node.GetTypeDefs()) {
                if ((typeDef.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public) {
                    _output.WriteLine("{0}  {1}", indent, typeDef.Name);
                }
            }

            foreach (var ns in node.GetNamespaces()) {
                DumpNamespaceTree(level + 1, ns);
            }
        }

        private static void Statistics(NamespaceTree tree, bool dump) {
            var x =
                from ns in tree.GetAllNamespaces()
                let count = ns.GetTypeDefs().Count()
                group ns by count into g
                let gCount = g.Count()
                orderby g.Key descending
                select new { Size = g.Key, Count = gCount };

            foreach (var p in x) {
                if (dump) {
                    _output.WriteLine("{0}\t{1}", p.Size, p.Count);
                }
            }

            Counter_Types = x.Sum((a) => a.Size);
            if (dump) {
                _output.WriteLine("Namespace size: total {0}, avg {1}", Counter_Types, x.Average((a) => a.Size));
            }
        }

        private static void Statistics(RNamespaceTree tree, bool dump) {
            var x =
                from ns in tree.GetAllNamespaces()
                let count = ns.GetTypeDefs().Count()
                group ns by count into g
                let gCount = g.Count()
                orderby g.Key descending
                select new { Size = g.Key, Count = gCount };

            foreach (var p in x) {
                if (dump) {
                    _output.WriteLine("{0}\t{1}", p.Size, p.Count);
                }
            }

            Counter_Types = x.Sum((a) => a.Size);
            if (dump) {
                _output.WriteLine("Namespace size: total {0}, avg {1}", Counter_Types, x.Average((a) => a.Size));
            }
        }

        #endregion

        #region Members

        private static void EnumerateMembers(MetadataTables tables) {
            foreach (TypeDef typeDef in tables.TypeDefs) {
                foreach (FieldDef fieldDef in typeDef.Fields) {
                    Counter_Fields++;
                }

                foreach (MethodDef methodDef in typeDef.Methods) {
                    Counter_Methods++;
                }

                foreach (PropertyDef propertyDef in typeDef.Properties) {
                    Counter_Properties++;
                }

                foreach (EventDef eventDef in typeDef.Events) {
                    Counter_Events++;
                }
            }
        }

        private static string TypeToString(MetadataRecord record) {
            if (record.IsNull) {
                return "<none>";
            }

            switch (record.Type) {
                case MetadataRecordType.TypeDef:
                    var typeDef = record.TypeDef;
                    return String.Format("def({0}.{1})", typeDef.Namespace, typeDef.Name);

                case MetadataRecordType.TypeRef:
                    var typeRef = record.TypeRef;
                    return String.Format("ref({0}.{1})", typeRef.TypeNamespace, typeRef.TypeName);

                case MetadataRecordType.TypeSpec:
                    var typeSpec = record.TypeSpec;
                    return String.Format("spec({0})", SignatureToString(typeSpec.Signature));
            }

            throw new InvalidOperationException("Unknown type token");
        }

        private static string MethodToString(MetadataRecord record) {
            if (record.IsNull) {
                return "<none>";
            }

            switch (record.Type) {
                case MetadataRecordType.MethodDef:
                    var methodDef = record.MethodDef;
                    var parent = methodDef.FindDeclaringType();
                    return String.Format("def({0}.{1}::{2})", parent.Namespace, parent.Name, methodDef.Name);

                case MetadataRecordType.MemberRef:
                    var memberRef = record.MemberRef;
                    return String.Format("ref({0}::{1}|{2})", TypeToString(memberRef.Class), memberRef.Name, SignatureToString(memberRef.Signature));

                case MetadataRecordType.MethodSpec:
                    var methodSpec = record.MethodSpec;
                    return String.Format("spec({0})", SignatureToString(methodSpec.Signature));
            }

            throw new InvalidOperationException("Unknown method token");
        }

        private static string DefaultValueToString(object value) {
            if (value == null) {
                return "null";
            }
            string str = value as string;
            if (str != null) {
                return "\"" + str + "\"";
            } else {
                return "(" + value.GetType().Name + ") " + value.ToString();
            }
        }

        private static string SignatureToString(MemoryBlock signature) {
            return MemoryToString(signature);
        }

        private static void DumpCustomAttributes(MetadataTableView attributes, string indent) {
            foreach (CustomAttributeDef ca in attributes) {
                _output.WriteLine("{0}custom attribute {1}({2})", indent, MethodToString(ca.Constructor), SignatureToString(ca.Value));
            }
        }

        private static void DumpGenericParameters(MetadataTableView genericParams, MetadataRecord owner) {
            foreach (GenericParamDef gp in genericParams) {
                _output.WriteLine("  generic parameter #{0}: {1}", gp.Index, gp.Name, gp.Attributes);
                Debug.Assert(gp.Owner.Equals(owner));
                foreach (GenericParamConstraint gpc in gp.Constraints) {
                    _output.WriteLine("    constraint {0}", TypeToString(gpc.Constraint));
                    Debug.Assert(gpc.Owner.Record.Equals(gp.Record));
                }
                if (Detailed) {
                    DumpCustomAttributes(gp.CustomAttributes, "    ");
                }
            }
        }

        private static string MemoryToString(MemoryBlock block, int max = 0) {
            byte[] bytes = new byte[Math.Min(max, block.Length)];
            block.Read(0, bytes);
            return BitConverter.ToString(bytes);
        }

        private static void DumpMembers(TypeDef typeDef) {
            int gpCount = typeDef.GetGenericParameterCount();

            _output.WriteLine("{0} {1}.{2}{3} : {4:X8}",
                ((typeDef.Attributes & TypeAttributes.Interface) != 0) ? "interface" : "type",
                typeDef.Namespace,
                typeDef.Name,
                gpCount > 0 ? "<#" + gpCount + ">" : null,
                TypeToString(typeDef.BaseType)
            );

            DumpGenericParameters(typeDef.GenericParameters, typeDef.Record);

            foreach (InterfaceImpl ifaces in typeDef.ImplementedInterfaces) {
                Debug.Assert(ifaces.ImplementingType.Record.Equals(typeDef.Record));
                _output.WriteLine("  implements {0}", TypeToString(ifaces.InterfaceType));
            }

            foreach (FieldDef fieldDef in typeDef.Fields) {
                var attrs = fieldDef.Attributes;
                _output.Write("  field      {0}", fieldDef.Name);
                if ((attrs & FieldAttributes.HasDefault) != 0) {
                    _output.Write(" = {0}", DefaultValueToString(fieldDef.GetDefaultValue()));
                }
                _output.WriteLine();

                if (Detailed) {
                    _output.WriteLine("    signature: {0}", SignatureToString(fieldDef.Signature));
                    _output.WriteLine("    attributes: {0}", fieldDef.Attributes);
                    DumpCustomAttributes(fieldDef.CustomAttributes, "    ");
                    MemoryBlock data = fieldDef.GetData(0);
                    if (data != null) {
                        // we would need to parse signature to get the size of the field's type:
                        _output.WriteLine("    data: {0}-...", MemoryToString(data, 5));
                    }
                }
                Debug.Assert(fieldDef.FindDeclaringType().Record.Equals(typeDef.Record));
            }

            foreach (MethodDef methodDef in typeDef.Methods) {
                _output.WriteLine("  method     {0}", methodDef.Name); 
                
                if (Detailed) {
                    _output.WriteLine("    signature: {0}", SignatureToString(methodDef.Signature));
                    _output.WriteLine("    attributes: {0} {1}", methodDef.Attributes, methodDef.ImplAttributes);
                    DumpCustomAttributes(methodDef.CustomAttributes, "    ");
                    DumpGenericParameters(methodDef.GenericParameters, methodDef.Record);
                    
                    foreach (ParamDef p in methodDef.Parameters) {
                        _output.WriteLine("    parameter #{0}: {1}", p.Index, p.Name);
                        Debug.Assert(p.FindDeclaringMethod().Record.Equals(methodDef));
                        DumpCustomAttributes(p.CustomAttributes, "    ");
                    }

                    MemoryBlock body = methodDef.GetBody();
                    if (body != null) {
                        _output.WriteLine("    body: {0}-...", MemoryToString(body, 5));
                    }
                }
                Debug.Assert(methodDef.FindDeclaringType().Record.Equals(typeDef.Record));
            }

            foreach (PropertyDef propertyDef in typeDef.Properties) {
                var attrs = propertyDef.Attributes;
                _output.Write("  property   {0} {{ ", propertyDef.Name);
                var accessors = propertyDef.GetAccessors();
                if (accessors.HasGetter) {
                    _output.Write("get: {0}; ", accessors.Getter.Name);
                }
                if (accessors.HasSetter) {
                    _output.Write("set: {0}; ", accessors.Setter.Name);
                }
                if ((attrs & PropertyAttributes.HasDefault) != 0) {
                    _output.Write("default: {0}; ", DefaultValueToString(propertyDef.GetDefaultValue()));
                }

                _output.WriteLine("}");

                if (Detailed) {
                    DumpCustomAttributes(propertyDef.CustomAttributes, "    ");
                }
                Debug.Assert(propertyDef.FindDeclaringType().Record.Equals(typeDef.Record));
            }

            foreach (EventDef eventDef in typeDef.Events) {
                _output.Write("  event      {0} {{ ", eventDef.Name);
                var accessors = eventDef.GetAccessors();
                if (accessors.HasAdd) {
                    _output.Write("add: {0}; ", accessors.Add.Name);
                }
                if (accessors.HasRemove) {
                    _output.Write("remove: {0}; ", accessors.Remove.Name);
                }
                if (accessors.HasFire) {
                    _output.Write("fire: {0}; ", accessors.Fire.Name);
                }
                _output.WriteLine("}");

                if (Detailed) {
                    DumpCustomAttributes(eventDef.CustomAttributes, "    ");
                }
                Debug.Assert(eventDef.FindDeclaringType().Record.Equals(typeDef));
            }

            _output.WriteLine();
        }

        private static void DumpMembers(MetadataTables tables) {
            foreach (TypeDef typeDef in tables.TypeDefs) {
                DumpMembers(typeDef);
            }
        }

        private static void DumpMembers(Type type) {
            DumpMembers(type.Module.GetMetadataTables().GetRecord(new MetadataToken(type.MetadataToken)).TypeDef);
        }

        #endregion

        #region PE, Modules, Files, Assemblies

        private static string ImplementationToString(MetadataRecord impl) {
            if (impl.IsFileDef) {
                return "file(" + impl.FileDef.Name + ")";
            } else if (impl.IsAssemblyRef) {
                return "assembly(" + impl.AssemblyRef.Name + ")";
            } else {
                Debug.Assert(impl.IsNull);
                return "<no-impl>";
            }
        }

        public static void DumpModule(MetadataTables tables) {
            ModuleDef md = tables.ModuleDef;
            _output.WriteLine("Module:");
            _output.WriteLine("  {0} {1}", md.Name, md.Mvid);
            
            AssemblyDef adef = tables.AssemblyDef;
            if (!adef.Record.IsNull) {
                _output.WriteLine("AssemblyDef:");
                _output.WriteLine("  {0} {1} {2} {3} {4} {5}",
                    adef.Name, adef.Version, adef.Culture, adef.NameFlags, adef.HashAlgorithm, BitConverter.ToString(adef.GetPublicKey())
                );
            }

            if (tables.AssemblyRefs.GetCount() > 0) {
                _output.WriteLine("AssemblyRefs:");
                foreach (AssemblyRef a in tables.AssemblyRefs) {
                    _output.WriteLine("  {0} {1} {2} {3} {5}",
                        a.Name, a.Version, a.Culture, a.NameFlags, BitConverter.ToString(a.GetHashValue()), BitConverter.ToString(a.GetPublicKeyOrToken())
                    );
                }
            }

            if (tables.Files.GetCount() > 0) {
                _output.WriteLine("Files:");
                foreach (FileDef fd in tables.Files) {
                    _output.WriteLine("  {0} {1} {2}",
                        fd.Name, fd.Attributes, BitConverter.ToString(fd.GetHashValue())
                    );
                }
            }

            if (tables.ManifestResources.GetCount() > 0) {
                _output.WriteLine("ManifestResources:");
                foreach (ManifestResourceDef resource in tables.ManifestResources) {
                    _output.WriteLine("  {0} {1} {2} 0x{3:X8}",
                        resource.Name, resource.Attributes, ImplementationToString(resource.Implementation), resource.Offset
                    );
                }
            }

            _output.WriteLine(new String('-', 50));
        }

        #endregion

        #region Dump

        private static void DumpMemMappedAssemblies() {
            var tree = new NamespaceTree();
            foreach (var file in Directory.GetFiles(Path.GetDirectoryName(typeof(object).Assembly.Location), "*.dll")) {
                try {
                    tree.Add(MetadataTables.OpenFile(file));
                    _output.WriteLine(file);
                } catch {
                }
            }
            DumpNamespaceTree(tree.Root);
            Statistics(tree, true);
        }

        private static string MdTablesToString(MetadataTables tables) {
            return tables.Module != null ? tables.Module.Assembly.ToString() + ":" + tables.Module.ToString() : tables.Path;
        }

        private static void DumpTables(
            IEnumerable<MetadataTables> allTables,
            bool dumpPE,
            bool dumpModule,
            bool dumpStatistics,
            bool dumpNamespaces,
            bool dumpExtensionMethods,
            bool dumpNestedTypes,
            bool dumpMembers,
            bool dumpMemberDetails) {

            var tree = new NamespaceTree();
            foreach (MetadataTables tables in allTables) {
#if DEBUG && CCI
                if (dumpPE && !tables.AssemblyDef.Record.IsNull) {
                    tables.Dump(_output);
                }
#endif
                if (dumpModule) {
                    DumpModule(tables);
                }
                if (dumpNestedTypes) {
                    _output.WriteLine("Nested types in {0}", MdTablesToString(tables));
                    DumpNestedTypes(tables);
                }
                if (dumpExtensionMethods) {
                    _output.WriteLine("Extension methods in {0}", MdTablesToString(tables));
                    DumpExtensionMethods(tables);
                }
                if (dumpMembers) {
                    _output.WriteLine("Members");
                    Detailed = dumpMemberDetails;
                    DumpMembers(tables);
                }
                if (dumpNamespaces) {
                    tree.Add(tables);
                }
            }

            if (dumpStatistics) {
                Statistics(tree, true);
            }

            if (dumpNamespaces) {
                DumpNamespaceTree(tree.Root);
            }
        }

        #endregion

        #region Enumeration

        private static void ReflectionEnumerate(IEnumerable<Assembly> assemblies) {
            var tree = new RNamespaceTree();
            foreach (var assembly in assemblies) {
                foreach (var module in assembly.GetModules(false)) {
                    EnumerateExtensionMethods(module);
                    tree.Add(module);
                    ModuleEnumerated(null);
                }
            }

            Counter_Types +=
                (from ns in tree.GetAllNamespaces()
                 from t in ns.GetTypeDefs()
                 select 1).Count();
        }

        private static void Enumerate(IEnumerable<MetadataTables> tables) {
            var tree = new NamespaceTree();
            foreach (var moduleTables in tables) {
                EnumerateExtensionMethods(moduleTables);
                tree.Add(moduleTables);
                ModuleEnumerated(moduleTables);
            }
            
            Counter_Types +=
                (from ns in tree.GetAllNamespaces()
                 from t in ns.GetTypeDefs()
                 select 1).Count();
        }

        private static void ModuleEnumerated(MetadataTables tables) {
#if CCI
            if (tables != null) {
                int mapSize = tables.GetRowCount(0x12) + tables.GetRowCount(0x15);
                Counter_MaxMapTablesSize = Math.Max(Counter_MaxMapTablesSize, mapSize);
                Counter_TotalMapTablesSize += mapSize;
            }
#endif
            Counter_Files++;
        }

        private static void EnumerateProperties(IEnumerable<MetadataTables> tables) {
            foreach (var moduleTables in tables) {
                Stopwatch swAssociates = new Stopwatch();
                Stopwatch swMethodsAndFields = new Stopwatch();
                
                int typesWithAssociates = 0;
                int methodCount = 0, fieldCount = 0;
                int propertyCount = 0, eventCount = 0;
                foreach (TypeDef type in moduleTables.TypeDefs) {
                    int typePropertyCount = 0, typeEventCount = 0;

                    swAssociates.Start();
    
                    foreach (PropertyDef prop in type.Properties) {
                        typePropertyCount++;
                    }

                    foreach (EventDef evnt in type.Events) {
                        typeEventCount++;
                    }

                    propertyCount += typePropertyCount;
                    eventCount += typeEventCount;
                    if (typePropertyCount > 0 || typeEventCount > 0) {
                        typesWithAssociates++;
                    }

                    swAssociates.Stop();
                    swMethodsAndFields.Start();

                    foreach (MethodDef method in type.Methods) {
                        methodCount++;
                    }

                    foreach (FieldDef field in type.Fields) {
                        fieldCount++;
                    }
                    swMethodsAndFields.Stop();
                }

                _output.WriteLine("{0,-50} {1,3}/{2,3}ms  p:{3,4} e:{4,4} m:{5,5} f:{6,5}  ta:{7,4}",
                    Path.GetFileName(moduleTables.Path),
                    swAssociates.ElapsedMilliseconds,
                    swMethodsAndFields.ElapsedMilliseconds,
                    propertyCount, 
                    eventCount,
                    methodCount,
                    fieldCount,
                    typesWithAssociates
                );
            }
        }

        #endregion

        #region Fuzzing

        // offsets in PE file and the number of 32-bit words to fuzz (based on 1.exe test file structure)
        private static int[] _Offsets = new[] {
            0x00000000, 1,
            0x00000080, 1,
            0x00000084, 1,
            0x00000098, 1, 
            0x00000108, 2,
            0x00000168, 2,
            0x00000180, 4,
            0x000001A8, 4,
            0x000001D0, 4,
            0x00000210, 2,
            0x00000220, 4,
            0x00005F00, 100,
        };

        private unsafe static void FuzzTables(IEnumerable<MetadataTables> tables) {
            foreach (var moduleTables in tables) {
                MemoryBlock image = moduleTables.m_import.Image;
                byte[] memory = new byte[image.Length];
                image.Read(0, memory);
                _output = TextWriter.Null;
                Random random = new Random();

                fixed (byte* ptr = memory) {
                    var newImage = new MemoryBlock(memory, ptr, memory.Length);
                    TestCorruptedHeaders(memory, ptr, newImage);
                    TestShortened(memory, ptr);
                }
            }
        }

        private unsafe static void TestCorruptedHeaders(byte[] memory, byte* ptr, MemoryBlock newImage) {
            foreach (uint data in new uint[] { 0, 0x0000ffff, 0xffff0000, 0x00ff00ff, 0xff0000ff, 0xff00ff00, 0xffffffff, 0x0100ffff, 0x01010101, 
                (uint)PEMagic.PEMagic64, (uint)PEMagic.PEMagic32}) {

                for (int i = 0; i < _Offsets.Length; i += 2) {
                    int pos = _Offsets[i];
                    int wordCount = _Offsets[i + 1];

                    uint* p = (uint*)(ptr + pos);
                    for (int w = 0; w < wordCount; w++) {
                        uint old = *p;
                        *p = data;

                        bool success = false;
                        try {
                            try {
                                new MetadataImport(newImage);
                                success = true;
                            } catch (BadImageFormatException) {
                                // ok
                                success = true;
                            }
                        } finally {
                            if (!success) {
                                Console.WriteLine("pos: {0}, count: {1}", pos, wordCount);
                            }
                            *p = old;
                        }
                        p++;
                    }

                    Console.Write('.');
                }
            }
        }

        private unsafe static void TestShortened(byte[] memory, byte* ptr) {
            for (int i = 0; i < _Offsets.Length; i += 2) {
                int pos = _Offsets[i];
                int wordCount = _Offsets[i + 1];

                for (int j = 0; j < wordCount * 4; j++) {
                    bool success = false;
                    try {
                        try {
                            new MetadataImport(new MemoryBlock(memory, ptr, pos + j));
                            success = true;
                        } catch (BadImageFormatException) {
                            success = true;
                        }
                    } finally {
                        if (!success) {
                            Console.WriteLine("Size: {0}", i);
                        }
                    }
                    Console.Write('.');
                }
            }
        }

        private static void Test(IEnumerable<MetadataTables> tables) {
            _output = TextWriter.Null;
            try {
                DumpTables(tables, true, true, true, true, true, true, true, true);
            } catch (BadImageFormatException e) {
                // ok, reader throws this
                Console.Error.WriteLine(e.Message);
            } catch (ArgumentOutOfRangeException e) {
                // ok, MemoryBlock throws this if the offset is out of range
                Console.Error.WriteLine(e.Message);
            }
        }

        #endregion

        private static List<Assembly> LoadAssemblies(IEnumerable<string> assemblyFiles) {
            _output.WriteLine("Loading assemblies.");
            Stopwatch loadTime = new Stopwatch();
            loadTime.Start();

            List<Assembly> result = new List<Assembly>(150);
            foreach (var file in assemblyFiles) {
                Assembly assembly = null;
                try {
                    assembly = Assembly.LoadFrom(file);
                } catch {
                    _output.WriteLine("{0} is not a valid assembly", file);
                }

                if (assembly != null) {
                    result.Add(assembly);
                }
            }

            loadTime.Stop();
            _output.WriteLine("Loaded {0} assemblies in {1} ms.", result.Count, loadTime.ElapsedMilliseconds);
            return result;
        }

        private static void Measure(Action f) {
            var initialWS = Process.GetCurrentProcess().PeakWorkingSet64;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            f();

            stopwatch.Stop();
            _output.WriteLine("Time: {0}ms, WS: {1}", stopwatch.ElapsedMilliseconds, (Process.GetCurrentProcess().PeakWorkingSet64 - initialWS) / (1024 * 1024));
        }

        private static IEnumerable<string> FwAssemblyFiles() {
            return Directory.GetFiles(Path.GetDirectoryName(typeof(object).Assembly.Location), "*.dll");
        }
        
        static void Main(string[] args) {
            //Detailed = true;
            //DumpMembers(typeof(Tests.Class1<,,,,>));
            //DumpMembers(typeof(System.Security.Policy.NetCodeGroup), true);
            
            List<string> argList = new List<string>(args);
            if (argList.Remove("/?")) {
                Console.WriteLine("Usage: Metadata.exe [options] [assembly list|*]");
                Console.WriteLine();
                Console.WriteLine("options:");
                Console.WriteLine("  /ref             use reflection; implies /load");
                Console.WriteLine("  /load            load assemblies");
                Console.WriteLine("  /ep              compares enumerating properties and events vs. methods and fields");
                Console.WriteLine("  /t               test all APIs, no output, catch any expected exception");
                Console.WriteLine("  /f               simple fuzzing test");
                Console.WriteLine("  /u               unit tests");
                Console.WriteLine("  /d [options]     dump");
                Console.WriteLine("                     n ... namespace tree (default)");
                Console.WriteLine("                     p ... PE file headers");
                Console.WriteLine("                     o ... module metadata");
                Console.WriteLine("                     s ... statistics");
                Console.WriteLine("                     e ... extension methods");
                Console.WriteLine("                     t ... nested types");
                Console.WriteLine("                     m ... members");
                Console.WriteLine("                     d ... member details");
                Console.WriteLine("  /expect <path>   compares /d output with a content of a file");
                Console.WriteLine();
                Console.WriteLine("assembly list");
                Console.WriteLine("  -empty-          current mscorlib.dll, System.Core.dll, System.dll");
                Console.WriteLine("  *                about 100 assemblies from the current .NET Framework");
                Console.WriteLine("  file1 file2 ...  specified files");
                return;
            }

            // options:
            bool test = argList.Remove("/t");
            bool fuzz = argList.Remove("/f");
            bool unitTests = argList.Remove("/u");
            bool useReflection = argList.Remove("/ref");
            bool loadAssemblies = argList.Remove("/load");
            bool enumProperties = argList.Remove("/ep");

            if (fuzz && (test || enumProperties || useReflection)) {
                Console.Error.WriteLine("Can't use /f with /t, /ref or /ep");
                return;
            }

            if (test && useReflection) {
                Console.Error.WriteLine("Can't use /ref with /t");
                return;
            }

            if (enumProperties && useReflection) {
                Console.Error.WriteLine("Can't use /ref with /ep");
                return;
            }

            if (useReflection) {
                loadAssemblies = true;
            }

            if (unitTests) {
                UnitTests.Run();
            }

            string dumpOptions;
            int dumpIdx = argList.IndexOf("/d");
            if (dumpIdx >= 0) {
                if (dumpIdx + 1 < argList.Count) {
                    dumpOptions = argList[dumpIdx + 1];
                    argList.RemoveAt(dumpIdx + 1);
                } else {
                    dumpOptions = "n";
                }
                argList.RemoveAt(dumpIdx);
            } else {
                dumpOptions = null;
            }

            string expectedOutputFile;
            int expectedIdx = argList.IndexOf("/expect");
            if (expectedIdx >= 0) {
                if (expectedIdx + 1 < argList.Count) {
                    expectedOutputFile = argList[expectedIdx + 1];
                    argList.RemoveAt(expectedIdx + 1);
                } else {
                    Console.Error.WriteLine("/expected requires a file");
                    return;
                }
                argList.RemoveAt(expectedIdx);
            } else {
                expectedOutputFile = null;
            }

            bool allFwAssemblies = argList.Remove("*");

            // assemblies:
            IEnumerable<string> assemblyFiles = argList;
            if (allFwAssemblies) {
                string fwDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
                assemblyFiles = assemblyFiles.Concat(
                    from fileName in AssemblyList.FwAssemblies
                    select Path.Combine(fwDir, fileName)
                );
            } else if (argList.Count == 0) {
                argList.Add(typeof(object).Assembly.Location);
                argList.Add(typeof(Expression).Assembly.Location);
                argList.Add(typeof(Regex).Assembly.Location);
            }

            // tables: 
            List<MetadataTables> tables;
            IEnumerable<Assembly> assemblies;
            if (loadAssemblies) {
                assemblies = LoadAssemblies(assemblyFiles);
                if (useReflection) {
                    tables = null;
                } else {
                    Stopwatch swTableLoad = new Stopwatch();
                    swTableLoad.Start();
                    tables = new List<MetadataTables>(from assembly in assemblies
                                                      from module in assembly.GetModules(false)
                                                      select module.GetMetadataTables());
                    swTableLoad.Stop();
                    _output.WriteLine("{0} modules loaded in {1}ms", tables.Count, swTableLoad.ElapsedMilliseconds);
                }
            } else {
                assemblies = null;
                Stopwatch swTableLoad = new Stopwatch();
                tables = new List<MetadataTables>();
                foreach (var file in assemblyFiles) {
                    try {
                        swTableLoad.Start();
                        tables.Add(MetadataTables.OpenFile(file));
                        swTableLoad.Stop();
                    } catch (FileNotFoundException) {
                        _output.WriteLine("File {0} doesn't exist.", file);
                    } catch (BadImageFormatException) {
                        _output.WriteLine("{0} is not a valid PE file", file);
                    }
                }

                if (dumpOptions == null) {
                    _output.WriteLine("Metadata tables ({0}) loaded in {1}ms", tables.Count, swTableLoad.ElapsedMilliseconds);
                }
            }

            if (fuzz) {
                FuzzTables(tables);
                return;
            }

            if (test) {
                Test(tables);
                return;
            }

            if (dumpOptions != null) {
                string tempDumpFile = "dump.txt";
                bool success = false;
                if (expectedOutputFile != null) {
                    _output = new StreamWriter(tempDumpFile, false, new UTF8Encoding(true, false), 0x400);
                }
                try {
                    try {
                        DumpTables(
                            tables,
                            dumpPE: dumpOptions.IndexOf('p') >= 0,
                            dumpModule: dumpOptions.IndexOf('o') >= 0,
                            dumpStatistics: dumpOptions.IndexOf('s') >= 0,
                            dumpNamespaces: dumpOptions.IndexOf('n') >= 0,
                            dumpExtensionMethods: dumpOptions.IndexOf('e') >= 0,
                            dumpNestedTypes: dumpOptions.IndexOf('t') >= 0,
                            dumpMembers: dumpOptions.IndexOf('m') >= 0,
                            dumpMemberDetails: dumpOptions.IndexOf('d') >= 0
                        );
                    } finally {
                        if (expectedOutputFile != null) {
                            _output.Close();
                        }
                    }

                    if (expectedOutputFile != null) {
                        // TODO: extract zip and compare
                    }
                    success = true;
                } finally {
                    if (success && _output != null) {
                        File.Delete(tempDumpFile);
                    }
                }
                
                return;
            }

            if (enumProperties) {
                Measure(() => EnumerateProperties(tables));
                return;
            }

            if (assemblies != null) {
                if (useReflection) {
                    Measure(() => ReflectionEnumerate(assemblies));
                } else {
                    Measure(() => Enumerate(tables));
                }
            } else {
                Measure(() => Enumerate(tables));
            }

            if (Counter_Files > 0) {
                _output.WriteLine("Enumerated {0} extension methods and {1} types.", Counter_ExtensionMethods, Counter_Types);
                _output.WriteLine("PropertyMap and EventMap sizes: max {0}, avg {1} per file.", Counter_MaxMapTablesSize, Counter_TotalMapTablesSize / Counter_Files);
            }
        }
    }
}
