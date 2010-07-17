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
using System.Configuration.Assemblies;
using System.Reflection;
using System.Security;
using System.IO;
using System.Diagnostics.Contracts;

namespace Microsoft.Scripting.Metadata {
    [Serializable]
    public enum MetadataRecordType {
        ModuleDef = ModuleTable.TableIndex,
        TypeRef = TypeRefTable.TableIndex,
        TypeDef = TypeDefTable.TableIndex,
        FieldDef = FieldTable.TableIndex,
        MethodDef = MethodTable.TableIndex,
        ParamDef = ParamTable.TableIndex,
        InterfaceImpl = InterfaceImplTable.TableIndex,
        MemberRef = MemberRefTable.TableIndex,
        CustomAttributeDef = CustomAttributeTable.TableIndex,
        Permission = DeclSecurityTable.TableIndex,
        SignatureDef = StandAloneSigTable.TableIndex,
        EventDef = EventTable.TableIndex,
        PropertyDef = PropertyTable.TableIndex,
        ModuleRef = ModuleRefTable.TableIndex,
        TypeSpec = TypeSpecTable.TableIndex,
        AssemblyDef = AssemblyTable.TableIndex,
        AssemblyRef = AssemblyRefTable.TableIndex,
        FileDef = FileTable.TableIndex,
        TypeExport = ExportedTypeTable.TableIndex,
        ManifestResourceDef = ManifestResourceTable.TableIndex,
        TypeNesting = NestedClassTable.TableIndex,
        GenericParamDef = GenericParamTable.TableIndex,
        MethodSpec = MethodSpecTable.TableIndex,
        GenericParamConstraint = GenericParamConstraintTable.TableIndex,
    }

    public partial class MetadataTables {
        private readonly Module m_module;

        internal MetadataTables(MetadataImport import, string path, Module module) {
            m_import = import;
            m_path = path;
            m_module = module;
        }

        /// <summary>
        /// Gets the module whose metadata tables this instance represents.
        /// Null if the tables reflect unloaded module file.
        /// </summary>
        public Module Module {
            get {
                return m_module;
            }
        }

        public bool IsValidToken(MetadataToken token) {
            return m_import.IsValidToken(token);
        }

        internal MetadataName ToMetadataName(uint blob) {
            return m_import.GetMetadataName(blob);
        }

        public static MetadataTables OpenFile(string path) {
            if (path == null) {
                throw new ArgumentNullException("path");
            }
            
            return new MetadataTables(CreateImport(path), path, null);
        }

        public static MetadataTables OpenModule(Module module) {
            if (module == null) {
                throw new ArgumentNullException("module");
            }

            return new MetadataTables(CreateImport(module.FullyQualifiedName), null, module);
        }

        private static MetadataImport CreateImport(string path) {
            var file = MemoryMapping.Create(path);
            return new MetadataImport(file.GetRange(0, (int)System.Math.Min(file.Capacity, Int32.MaxValue)));
        }

#if DEBUG
        public void Dump(TextWriter output) {
            m_import.Dump(output);
        }

        public int GetRowCount(int tableIndex) {
            return m_import.GetRowCount(tableIndex);
        }
#else
        public int GetRowCount(int tableIndex) {
            return m_import.GetRowCount(tableIndex);
        }
#endif
        internal int GetRowCount(MetadataRecordType tableIndex) {
            return m_import.GetRowCount((int)tableIndex);
        }
    }

    public partial struct MetadataRecord {
        internal int Rid {
            get { return m_token.Rid; }
        }

        internal MetadataImport Import {
            get { return m_tables.m_import; }
        }
    }

    public partial struct MetadataTableView {
        /// <summary>
        /// Gets the number of records in the view.
        /// If the view is over an entire table this operation is O(1), 
        /// otherwise it might take up to O(log(#records in the table)).
        /// </summary>
        public int GetCount() {
            if (m_parent.IsNull) {
                return m_parent.Tables.GetRowCount((int)m_type >> 24);
            }

            int start, count;
            m_parent.Import.GetEnumeratorRange(m_type, m_parent.Token, out start, out count);
            return count;
        }

        public MetadataTableEnumerator GetEnumerator() {
            return new MetadataTableEnumerator(m_parent, m_type);
        }
    }

    public partial struct ModuleDef {
        internal ModuleDef(MetadataRecord record) {
            Contract.Requires(record.IsModuleDef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.ModuleTable.GetName(m_record.Rid));
            }
        }

        public Guid Mvid {
            get {
                return m_record.Import.GetGuid(m_record.Import.ModuleTable.GetMVId(m_record.Rid));
            }
        }
    }

    public partial struct TypeRef {
        internal TypeRef(MetadataRecord record) {
            Contract.Requires(record.IsTypeRef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        /// <summary>
        /// AssemblyRef:
        ///   If the target type is defined in a different Assembly from the current module.
        /// TypeRef:
        ///   Target type is nested in TypeRef.
        /// ModuleRef:
        ///   Target type is defined in another module within the same Assembly as this one.
        /// ModuleDef:
        ///   If the target type is defined in the current module (this should not occur in a CLI "compressed metadata" module).
        /// Null token:
        ///   There shall be a row in the ExportedType table for this Type - its Implementation field shall contain 
        ///   a File token or an AssemblyRef token that says where the type is defined.
        /// </summary>
        public MetadataRecord ResolutionScope {
            get {
                return new MetadataRecord(m_record.Import.TypeRefTable.GetResolutionScope(m_record.Rid), m_record.Tables);
            }
        }

        public MetadataName TypeName {
            get {
                return m_record.m_tables.ToMetadataName(m_record.Import.TypeRefTable.GetName(m_record.Rid));
            }
        }

        public MetadataName TypeNamespace {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.TypeRefTable.GetNamespace(m_record.Rid));
            }
        }
    }

    public partial struct TypeDef {
        internal TypeDef(MetadataRecord record) {
            Contract.Requires(record.IsTypeDef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }
        
        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.TypeDefTable.GetName(m_record.Rid));
            }
        }

        public MetadataName Namespace {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.TypeDefTable.GetNamespace(m_record.Rid));
            }
        }

        /// <summary>
        /// Flags field in TypeDef table.
        /// </summary>
        public TypeAttributes Attributes {
            get {
                return m_record.Import.TypeDefTable.GetFlags(m_record.Rid);
            }
        }

        public MetadataRecord BaseType {
            get {
                return new MetadataRecord(m_record.Import.TypeDefTable.GetExtends(m_record.Rid), m_record.Tables);
            }
        }

        /// <summary>
        /// Finds a nesting type-def. The search time is logarithmic in the number of nested types defined in the owning module.
        /// Returns a null token if this is not a nested type-def.
        /// </summary>
        public TypeDef FindDeclaringType() {
            return new MetadataRecord(
                new MetadataToken(MetadataTokenType.TypeDef, m_record.Import.NestedClassTable.FindParentTypeDefRowId(m_record.Rid)), 
                m_record.Tables
            ).TypeDef;
        }

        /// <summary>
        /// O(log(#generic parameters in module))
        /// </summary>
        public int GetGenericParameterCount() {
            int count;
            m_record.Import.GenericParamTable.FindGenericParametersForType(m_record.Rid, out count);
            return count;
        }
    }
    
    public partial struct FieldDef {
        internal FieldDef(MetadataRecord record) {
            Contract.Requires(record.IsFieldDef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }
        
        /// <summary>
        /// Flags field in the Field table.
        /// </summary>
        public FieldAttributes Attributes {
            get {
                return m_record.Import.FieldTable.GetFlags(m_record.Rid);
            }
        }

        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.FieldTable.GetName(m_record.Rid));
            }
        }

        public MemoryBlock Signature {
            get {
                return m_record.Import.GetBlobBlock(m_record.Import.FieldTable.GetSignature(m_record.Rid));
            }
        }

        /// <summary>
        /// O(log(#fields, parameters and properties with default value)).
        /// Returns <see cref="Missing.Value"/> if the field doesn't have a default value.
        /// </summary>
        public object GetDefaultValue() {
            return m_record.Import.GetDefaultValue(m_record.Token);
        }

        /// <summary>
        /// Returns null reference iff the field has no RVA.
        /// If size is 0 the memory block will span over the rest of the data section.
        /// O(log(#fields with RVAs)).
        /// </summary>
        public MemoryBlock GetData(int size) {
            if (size < 0) {
                throw new ArgumentOutOfRangeException("size");
            }
            uint rva = m_record.Import.FieldRVATable.GetFieldRVA(m_record.Rid);
            return rva != 0 ? m_record.Import.RvaToMemoryBlock(rva, (uint)size) : null;
        }

        /// <summary>
        /// Finds type-def that declares this field. The search time is logarithmic in the number of types defined in the owning module.
        /// </summary>
        public TypeDef FindDeclaringType() {
            return new MetadataRecord(
                new MetadataToken(MetadataTokenType.TypeDef, m_record.Import.TypeDefTable.FindTypeContainingField(m_record.Rid, m_record.Import.FieldTable.NumberOfRows)), 
                m_record.Tables
            ).TypeDef;
        }
    }

    public partial struct MethodDef {
        internal MethodDef(MetadataRecord record) {
            Contract.Requires(record.IsMethodDef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        /// <summary>
        /// ImplFlags field in the MethodDef table.
        /// </summary>
        public MethodImplAttributes ImplAttributes {
            get {
                return m_record.Import.MethodTable.GetImplFlags(m_record.Rid);
            }
        }

        /// <summary>
        /// Flags field in the MethodDef table.
        /// </summary>
        public MethodAttributes Attributes {
            get {
                return m_record.Import.MethodTable.GetFlags(m_record.Rid);
            }
        }

        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.MethodTable.GetName(m_record.Rid));
            }
        }

        public MemoryBlock Signature {
            get {
                return m_record.Import.GetBlobBlock(m_record.Import.MethodTable.GetSignature(m_record.Rid));
            }
        }

        /// <summary>
        /// Returns a null reference iff the method has no body.
        /// If size is 0 the memory block will span over the rest of the data section.
        /// </summary>
        public MemoryBlock GetBody() {
            // TODO: calculate size, decode method header and return MetadataMethodBody.
            uint rva = m_record.Import.MethodTable.GetRVA(m_record.Rid);
            return rva != 0 ? m_record.Import.RvaToMemoryBlock(rva, 0) : null;
        }

        /// <summary>
        /// Finds type-def that declares this method. The search time is logarithmic in the number of types defined in the owning module.
        /// </summary>
        public TypeDef FindDeclaringType() {
            return new MetadataRecord(
                new MetadataToken(MetadataTokenType.TypeDef, m_record.Import.TypeDefTable.FindTypeContainingMethod(m_record.Rid, m_record.Import.MethodTable.NumberOfRows)), 
                m_record.Tables
            ).TypeDef;
        }

        /// <summary>
        /// O(log(#generic parameters in module))
        /// </summary>
        public int GetGenericParameterCount() {
            int count;
            m_record.Import.GenericParamTable.FindGenericParametersForMethod(m_record.Rid, out count);
            return count;
        }

        // TODO: FindAssociate: event/property
    }

    public partial struct ParamDef {
        internal ParamDef(MetadataRecord record) {
            Contract.Requires(record.IsParamDef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }
        
        public ParameterAttributes Attributes {
            get {
                return m_record.Import.ParamTable.GetFlags(m_record.Rid);
            }
        }

        /// <summary>
        /// Value greater or equal to zero and less than or equal to the number of parameters in owner method. 
        /// A value of 0 refers to the owner method's return type; its parameters are then numbered from 1 onwards.
        /// Not all parameters need to have a corresponding ParamDef entry.
        /// </summary>
        public int Index {
            get {
                return m_record.Import.ParamTable.GetSequence(m_record.Rid);
            }
        }

        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.ParamTable.GetName(m_record.Rid));
            }
        }

        /// <summary>
        /// O(log(#fields, parameters and properties with default value)).
        /// Returns <see cref="Missing.Value"/> if the field doesn't have a default value.
        /// </summary>
        public object GetDefaultValue() {
            return m_record.Import.GetDefaultValue(m_record.Token);
        }

        /// <summary>
        /// Binary searches MethodDef table for a method that declares this parameter.
        /// </summary>
        public MethodDef FindDeclaringMethod() {
            return new MetadataRecord(
                new MetadataToken(MetadataTokenType.MethodDef, m_record.Import.MethodTable.FindMethodContainingParam(m_record.Rid, m_record.Import.ParamTable.NumberOfRows)),
                m_record.Tables
            ).MethodDef;
        }
    }

    public partial struct InterfaceImpl {
        internal InterfaceImpl(MetadataRecord record) {
            Contract.Requires(record.IsInterfaceImpl && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }
        
        /// <summary>
        /// Could be a null token in EnC scenarios.
        /// </summary>
        public TypeDef ImplementingType {
            get {
                return new MetadataRecord(
                    new MetadataToken(MetadataTokenType.TypeDef, m_record.Import.InterfaceImplTable.GetClass(m_record.Rid)), 
                    m_record.Tables
                ).TypeDef;
            }
        }

        /// <summary>
        /// TypeDef, TypeRef, or TypeSpec.
        /// </summary>
        public MetadataRecord InterfaceType {
            get {
                return new MetadataRecord(m_record.Import.InterfaceImplTable.GetInterface(m_record.Rid), m_record.Tables);
            }
        }
    }

    public partial struct MemberRef {
        internal MemberRef(MetadataRecord record) {
            Contract.Requires(record.IsMemberRef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        /// <summary>
        /// TypeRef or TypeDef:
        ///   If the class that defines the member is defined in another module. 
        ///   Note that it is unusual, but valid, to use a TypeRef token when the member is defined in this same module, 
        ///   in which case, its TypeDef token can be used instead.
        /// ModuleRef:
        ///   If the member is defined, in another module of the same assembly, as a global function or variable.
        /// MethodDef: 
        ///   When used to supply a call-site signature for a vararg method that is defined in this module. 
        ///   The Name shall match the Name in the corresponding MethodDef row. 
        ///   The Signature shall match the Signature in the target method definition
        /// TypeSpec:
        ///   If the member is a member of a generic type
        /// </summary>
        public MetadataRecord Class {
            get {
                return new MetadataRecord(m_record.Import.MemberRefTable.GetClass(m_record.Rid), m_record.Tables);
            }
        }

        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.MemberRefTable.GetName(m_record.Rid));
            }
        }

        public MemoryBlock Signature {
            get {
                return m_record.Import.GetBlobBlock(m_record.Import.MemberRefTable.GetSignature(m_record.Rid));
            }
        }
    }

    public partial struct CustomAttributeDef {
        internal CustomAttributeDef(MetadataRecord record) {
            Contract.Requires(record.IsCustomAttributeDef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }
        
        /// <summary>
        /// Any token except the CustomAttribute.
        /// </summary>
        public MetadataRecord Parent {
            get {
                return new MetadataRecord(m_record.Import.CustomAttributeTable.GetParent(m_record.Rid), m_record.Tables);
            }
        }

        /// <summary>
        /// Returns the value of Type column in the CustomAttribute table.
        /// MethodDef or MemberRef.
        /// </summary>
        public MetadataRecord Constructor {
            get {
                return new MetadataRecord(m_record.Import.CustomAttributeTable.GetConstructor(m_record.Rid), m_record.Tables);
            }
        }

        /// <summary>
        /// Value blob.
        /// </summary>
        public MemoryBlock Value { 
            get {
                return m_record.Import.GetBlobBlock(m_record.Import.CustomAttributeTable.GetValue(m_record.Rid));
            } 
        }
    }

    public partial struct SignatureDef {
        internal SignatureDef(MetadataRecord record) {
            Contract.Requires(record.IsSignatureDef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        public MemoryBlock Signature {
            get {
                return m_record.Import.GetBlobBlock(m_record.Import.StandAloneSigTable.GetSignature(m_record.Rid));
            }
        }
    }

    public partial struct PropertyDef {
        internal PropertyDef(MetadataRecord record) {
            Contract.Requires(record.IsProperty && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }
        
        public PropertyAttributes Attributes {
            get {
                return m_record.Import.PropertyTable.GetFlags(m_record.Rid);
            }
        }

        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.PropertyTable.GetName(m_record.Rid));
            }
        }

        public MemoryBlock Signature {
            get {
                return m_record.Import.GetBlobBlock(m_record.Import.PropertyTable.GetSignature(m_record.Rid));
            }
        }

        public PropertyAccessors GetAccessors() {
            int methodCount;
            var import = m_record.Import;
            int semanticsRow = import.MethodSemanticsTable.FindSemanticMethodsForProperty(m_record.Rid, out methodCount);
            
            uint getter = 0, setter = 0;
            for (ushort i = 0; i < methodCount; i++) {
                switch (import.MethodSemanticsTable.GetFlags(semanticsRow)) {
                    case MethodSemanticsFlags.Getter: getter = import.MethodSemanticsTable.GetMethodRid(semanticsRow); break;
                    case MethodSemanticsFlags.Setter: setter = import.MethodSemanticsTable.GetMethodRid(semanticsRow); break;
                }
                semanticsRow++;
            }

            return new PropertyAccessors(this, new MetadataToken(MetadataTokenType.MethodDef, getter), new MetadataToken(MetadataTokenType.MethodDef, setter));
        }

        /// <summary>
        /// O(log(#fields, parameters and properties with default value)).
        /// Returns <see cref="Missing.Value"/> if the field doesn't have a default value.
        /// </summary>
        public object GetDefaultValue() {
            return m_record.Import.GetDefaultValue(m_record.Token);
        }

        /// <summary>
        /// Finds type-def that declares this property. The search time is logarithmic in the number of types with properties defined in the owning module.
        /// </summary>
        public TypeDef FindDeclaringType() {
            return new MetadataRecord(
                new MetadataToken(MetadataTokenType.TypeDef, m_record.Import.PropertyMapTable.FindTypeContainingProperty(m_record.Rid, m_record.Import.PropertyTable.NumberOfRows)),
                m_record.Tables
            ).TypeDef;
        }
    }

    public partial struct EventDef {
        internal EventDef(MetadataRecord record) {
            Contract.Requires(record.IsEvent && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        public EventAttributes Attributes {
            get {
                return m_record.Import.EventTable.GetFlags(m_record.Rid);
            }
        }

        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.EventTable.GetName(m_record.Rid));
            }
        }

        public MetadataRecord EventType {
            get {
                return new MetadataRecord(m_record.Import.EventTable.GetEventType(m_record.Rid), m_record.Tables);
            }
        }

        public EventAccessors GetAccessors() {
            int methodCount;
            var import = m_record.Import;
            int semanticsRow = import.MethodSemanticsTable.FindSemanticMethodsForEvent(m_record.Rid, out methodCount);

            uint add = 0, remove = 0, fire = 0;
            for (ushort i = 0; i < methodCount; i++) {
                switch (import.MethodSemanticsTable.GetFlags(semanticsRow)) {
                    case MethodSemanticsFlags.AddOn: add = import.MethodSemanticsTable.GetMethodRid(semanticsRow); break;
                    case MethodSemanticsFlags.RemoveOn: remove = import.MethodSemanticsTable.GetMethodRid(semanticsRow); break;
                    case MethodSemanticsFlags.Fire: fire = import.MethodSemanticsTable.GetMethodRid(semanticsRow); break;
                }
                semanticsRow++;
            }

            return new EventAccessors(this, 
                new MetadataToken(MetadataTokenType.MethodDef, add),
                new MetadataToken(MetadataTokenType.MethodDef, remove),
                new MetadataToken(MetadataTokenType.MethodDef, fire)
            );
        }

        /// <summary>
        /// Finds type-def that declares this event. The search time is logarithmic in the number of types with events defined in the owning module.
        /// </summary>
        public TypeDef FindDeclaringType() {
            return new MetadataRecord(
               new MetadataToken(MetadataTokenType.TypeDef, m_record.Import.EventMapTable.FindTypeContainingEvent(m_record.Rid, m_record.Import.EventTable.NumberOfRows)),
               m_record.Tables
           ).TypeDef;
        }
    }

    public partial struct ModuleRef {
        internal ModuleRef(MetadataRecord record) {
            Contract.Requires(record.IsModuleRef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.ModuleRefTable.GetName(m_record.Rid));
            }
        }
    }

    public partial struct TypeSpec {
        internal TypeSpec(MetadataRecord record) {
            Contract.Requires(record.IsTypeSpec && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        public MemoryBlock Signature {
            get {
                return m_record.Import.GetBlobBlock(m_record.Import.TypeSpecTable.GetSignature(m_record.Rid));
            }
        }
    }

    public partial struct AssemblyDef {
        internal AssemblyDef(MetadataRecord record) {
            Contract.Requires(record.IsAssemblyDef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        public AssemblyHashAlgorithm HashAlgorithm {
            get {
                return m_record.Import.AssemblyTable.GetHashAlgorithm(m_record.Rid);
            }
        }

        public Version Version {
            get {
                return m_record.Import.AssemblyTable.GetVersion(m_record.Rid);
            }
        }

        public AssemblyNameFlags NameFlags {
            get {
                return m_record.Import.AssemblyTable.GetFlags(m_record.Rid);
            }
        }

        public byte[] GetPublicKey() {
            var import = m_record.Import;
            return import.GetBlob(import.AssemblyTable.GetPublicKey(m_record.Rid));
        }

        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.AssemblyTable.GetName(m_record.Rid));
            }
        }

        public MetadataName Culture {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.AssemblyTable.GetCulture(m_record.Rid));
            }
        }
    }

    public partial struct AssemblyRef {
        internal AssemblyRef(MetadataRecord record) {
            Contract.Requires(record.IsAssemblyRef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        public byte[] GetHashValue() {
            var import = m_record.Import;
            return import.GetBlob(import.AssemblyRefTable.GetHashValue(m_record.Rid));
        }

        public Version Version {
            get {
                return m_record.Import.AssemblyRefTable.GetVersion(m_record.Rid);
            }
        }

        public AssemblyNameFlags NameFlags {
            get {
                return m_record.Import.AssemblyRefTable.GetFlags(m_record.Rid);
            }
        }

        public byte[] GetPublicKeyOrToken() {
            var import = m_record.Import;
            return import.GetBlob(import.AssemblyRefTable.GetPublicKeyOrToken(m_record.Rid));
        }

        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.AssemblyRefTable.GetName(m_record.Rid));
            }
        }

        public MetadataName Culture {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.AssemblyRefTable.GetCulture(m_record.Rid));
            }
        }
    }

    public partial struct FileDef {
        internal FileDef(MetadataRecord record) {
            Contract.Requires(record.IsFileDef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        public AssemblyFileAttributes Attributes {
            get {
                return m_record.Import.FileTable.GetFlags(m_record.Rid);
            }
        }

        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.FileTable.GetName(m_record.Rid));
            }
        }

        public byte[] GetHashValue() {
            var import = m_record.Import;
            return import.GetBlob(import.FileTable.GetHashValue(m_record.Rid));
        }
    }

    public partial struct TypeExport {
        internal TypeExport(MetadataRecord record) {
            Contract.Requires(record.IsTypeExport && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        public TypeAttributes Attributes {
            get {
                return m_record.Import.ExportedTypeTable.GetFlags(m_record.Rid);
            }
        }

        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.ExportedTypeTable.GetName(m_record.Rid));
            }
        }

        public MetadataName Namespace {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.ExportedTypeTable.GetNamespace(m_record.Rid));
            }
        }

        /// <summary>
        /// Forwarded type: AssemblyRef
        /// Nested types: ExportedType
        /// Type in another module of this assembly: FileDef
        /// </summary>
        public MetadataRecord Implementation {
            get {
                return new MetadataRecord(m_record.Import.ExportedTypeTable.GetImplementation(m_record.Rid), m_record.Tables);
            }
        }
    }

    public partial struct ManifestResourceDef {
        internal ManifestResourceDef(MetadataRecord record) {
            Contract.Requires(record.IsManifestResourceDef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        // TODO: MemoryBlock
        [CLSCompliant(false)]
        public uint Offset {
            get {
                return m_record.Import.ManifestResourceTable.GetOffset(m_record.Rid);
            }
        }

        public ManifestResourceAttributes Attributes {
            get { 
                return m_record.Import.ManifestResourceTable.GetFlags(m_record.Rid);
            }
        }

        public MetadataRecord Implementation {
            get {
                return new MetadataRecord(m_record.Import.ManifestResourceTable.GetImplementation(m_record.Rid), m_record.Tables);
            }
        }

        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.ManifestResourceTable.GetName(m_record.Rid));
            }
        }
    }

    public partial struct TypeNesting {
        internal TypeNesting(MetadataRecord record) {
            Contract.Requires(record.IsTypeNesting && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        public TypeDef NestedType {
            get {
                return new MetadataRecord(m_record.Import.NestedClassTable.GetNestedType(m_record.Rid), m_record.Tables).TypeDef;
            }
        }

        public TypeDef EnclosingType {
            get {
                return new MetadataRecord(m_record.Import.NestedClassTable.GetEnclosingType(m_record.Rid), m_record.Tables).TypeDef;
            }
        }
    }

    public partial struct GenericParamDef {
        internal GenericParamDef(MetadataRecord record) {
            Contract.Requires(record.IsGenericParamDef && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        public GenericParameterAttributes Attributes {
            get {
                return m_record.Import.GenericParamTable.GetFlags(m_record.Rid);
            }
        }

        /// <summary>
        /// Value greater or equal to zero and less than or equal to the number of parameters in owner method/type. 
        /// All generic parameters are listed in the table.
        /// </summary>
        public int Index {
            get {
                return m_record.Import.GenericParamTable.GetIndex(m_record.Rid);
            }
        }

        public MetadataName Name {
            get {
                return m_record.Tables.ToMetadataName(m_record.Import.GenericParamTable.GetName(m_record.Rid));
            }
        }

        /// <summary>
        /// TypeDef or MethodDef.
        /// </summary>
        public MetadataRecord Owner {
            get {
                return new MetadataRecord(m_record.Import.GenericParamTable.GetOwner(m_record.Rid), m_record.Tables);
            }
        }
    }

    public partial struct GenericParamConstraint {
        internal GenericParamConstraint(MetadataRecord record) {
            Contract.Requires(record.IsGenericParamConstraint && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        public GenericParamDef Owner {
            get {
                return new MetadataRecord(m_record.Import.GenericParamConstraintTable.GetOwner(m_record.Rid), m_record.Tables).GenericParamDef;
            }
        }

        /// <summary>
        /// TypeDef, TypeRef, or TypeSpec.
        /// </summary>
        public MetadataRecord Constraint {
            get {
                return new MetadataRecord(m_record.Import.GenericParamConstraintTable.GetConstraint(m_record.Rid), m_record.Tables);
            }
        }
    }

    public partial struct MethodSpec {
        internal MethodSpec(MetadataRecord record) {
            Contract.Requires(record.IsMethodSpec && record.Tables.IsValidToken(record.Token));
            m_record = record;
        }

        /// <summary>
        /// MethodDef or MethodRef.
        /// </summary>
        public MetadataRecord GenericMethod {
            get {
                return new MetadataRecord(m_record.Import.MethodSpecTable.GetGenericMethod(m_record.Rid), m_record.Tables);
            }
        }

        public MemoryBlock Signature {
            get {
                return m_record.Import.GetBlobBlock(m_record.Import.MethodSpecTable.GetSignature(m_record.Rid));
            }
        }
    }
}
