using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace Microsoft.Scripting.Metadata {
    public static class MetadataServices {
        // Stores metadata tables for each loaded non-dynamic assemblies.
        // The first module in the array is always a manifest module.
        private static Dictionary<Assembly, MetadataTables[]> _metadataCache;

        private static MetadataTables[] GetAsseblyMetadata(Assembly assembly) {
            if (_metadataCache == null) {
                _metadataCache = new Dictionary<Assembly, MetadataTables[]>();
            }

            lock (_metadataCache) {
                MetadataTables[] metadata;
                if (!_metadataCache.TryGetValue(assembly, out metadata)) {
                    var modules = assembly.GetModules(false);
                    metadata = new MetadataTables[modules.Length];
                    int i = 1;
                    foreach (var module in modules) {
                        var tables = MetadataTables.OpenModule(module);
                        if (tables.AssemblyDef.Record.IsNull) {
                            metadata[i++] = MetadataTables.OpenModule(module);
                        } else {
                            metadata[0] = tables;
                        }
                    }
                    
                    _metadataCache.Add(assembly, metadata);
                }
                return metadata;
            }
        }

        private static void GetName(CustomAttributeDef ca, out MetadataName name, out MetadataName @namespace) {
            var ctor = ca.Constructor;
            if (ctor.IsMemberRef) {
                var cls = ctor.MemberRef.Class;
                if (cls.IsTypeRef) {
                    name = cls.TypeRef.TypeName;
                    @namespace = cls.TypeRef.TypeNamespace;
                } else {
                    name = cls.TypeDef.Name;
                    @namespace = cls.TypeDef.Namespace;
                }
            } else {
                var ctorDef = ctor.MethodDef;
                TypeDef typeDef = ctorDef.FindDeclaringType();
                name = typeDef.Name;
                @namespace = typeDef.Namespace;
            }
        }

        private static readonly byte[] _ExtensionAttributeNameUtf8 = Encoding.UTF8.GetBytes("ExtensionAttribute");
        private static readonly byte[] _ExtensionAttributeNamespaceUtf8 = Encoding.UTF8.GetBytes("System.Runtime.CompilerServices");

        private static bool IsExtensionAttribute(CustomAttributeDef ca) {
            MetadataName name, ns;
            GetName(ca, out name, out ns);
            return name.Equals(_ExtensionAttributeNameUtf8, 0, _ExtensionAttributeNameUtf8.Length)
                && ns.Equals(_ExtensionAttributeNamespaceUtf8, 0, _ExtensionAttributeNamespaceUtf8.Length);
        }

        private static MetadataRecord GetExtensionAttributeCtor(MetadataTables tables) {
            AssemblyDef adef = tables.AssemblyDef;
            if (!adef.Record.IsNull) {
                foreach (CustomAttributeDef ca in adef.CustomAttributes) {
                    if (IsExtensionAttribute(ca)) {
                        return ca.Constructor;
                    }
                }
            }
            return default(MetadataRecord);
        }

        public static List<KeyValuePair<Module, int>> GetVisibleExtensionMethods(Assembly assembly) {
            if (assembly == null) {
                throw new ArgumentNullException("assembly");
            }

            MetadataTables manifest = GetAsseblyMetadata(assembly)[0];
            MetadataRecord eaCtor = GetExtensionAttributeCtor(manifest);
            var result = new List<KeyValuePair<Module, int>>();
            if (!eaCtor.IsNull) {
                foreach (CustomAttributeDef ca in manifest.CustomAttributes) {
                    if (ca.Constructor.Equals(eaCtor) && ca.Parent.IsMethodDef) {
                        MethodDef mdef = ca.Parent.MethodDef;
                        var mattrs = mdef.Attributes;
                        if ((mattrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public && (mattrs & MethodAttributes.Static) != 0) {
                            var declType = mdef.FindDeclaringType();
                            var tattrs = declType.Attributes;
                            if (((tattrs & TypeAttributes.VisibilityMask) == TypeAttributes.Public ||
                                (tattrs & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic) &&
                                (tattrs & TypeAttributes.Abstract) != 0 &&
                                (tattrs & TypeAttributes.Sealed) != 0) {
                                result.Add(new KeyValuePair<Module, int>(manifest.Module, mdef.Record.Token.Value));
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static List<MethodInfo> GetVisibleExtensionMethodInfos(Assembly assembly) {
            var tokens = GetVisibleExtensionMethods(assembly);
            
            List<MethodInfo> result = new List<MethodInfo>(tokens.Count);
            foreach (var moduleAndToken in tokens) {
                result.Add((MethodInfo)moduleAndToken.Key.ResolveMethod(moduleAndToken.Value));
            }
            return result;
        }
    }
}
