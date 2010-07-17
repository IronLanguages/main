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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using System.Security;

//
// Tables not exposed directly:
// - Constant             (0x0B) - via GetDefaultValue on ParamDef, FieldDef and PropertyDef
// - MethodSemantics      (0x18) - via GetAccessors on PropertyDef/EventDef
// - PropertyMap          (0x15) - via PropertyDef
// - EventMap             (0x12) - via EventDef
// - Param                (0x08) - via Parameters on MethodDef
// - GenericParam         (0x2A) - via GenericParameters on TypeDef, MethodDef
// - GenericParamConst.   (0x2C) - via Constraint on GenericParamDef
//
// Tables not exposed at all:
// - DeclSecurity         (0x0E) - Security system; if we need these we should add SecurityAttributes view on TypeDef, MethodDef, and AssemblyDef.
// - FieldMarshal         (0x0D) - P/Invokes
// - ImplMap              (0x1C) - P/Invokes
// - ClassLayout          (0x0F) - type loader
// - FieldLayout          (0x10) - type loader
// - AssemblyProcessor    (0x21)
// - AssemblyOS           (0x22)
// - AssemblyRefProcessor (0x24)
// - AssemblyRefOS        (0x25)
//

#if CCI
namespace Microsoft.Scripting.Metadata {
#else
namespace System.Reflection {
    using Microsoft.Scripting.Metadata;
#endif
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
    public enum AssemblyFileAttributes {
        ContainsMetadata = 0x00000000,
        ContainsNoMetadata = 0x00000001,
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2217:DoNotMarkEnumsWithFlags")]
    [Flags]
    public enum ManifestResourceAttributes {
        PublicVisibility = 0x00000001,
        PrivateVisibility = 0x00000002,
        VisibilityMask = 0x00000007,

        InExternalFile = 0x00000010,
    }

    [Serializable, DebuggerDisplay("{DebugView}")]
    public partial struct MetadataToken : IEquatable<MetadataToken> {
        internal readonly int m_value;

        /// <summary>
        /// We need to be able to construct tokens out of byte-code.
        /// </summary>
        public MetadataToken(int value) {
            m_value = value;
        }

        internal MetadataToken(MetadataTokenType type, int rowId) {
            m_value = (int)type | (rowId & 0x00ffffff);
        }

        internal MetadataToken(MetadataTokenType type, uint rowId) {
            m_value = (int)type | (int)(rowId & 0x00ffffff);
        }

        public MetadataToken(MetadataRecordType type, int rowId) {
            m_value = ((int)type << 24) | rowId;
        }

        // SECURITY: Nothing unsafe here.
        [SecuritySafeCritical]
        public override bool Equals(object obj) {
            return obj is MetadataToken && Equals((MetadataToken)obj);
        }

        // SECURITY: Nothing unsafe here.
        [SecuritySafeCritical]
        public bool Equals(MetadataToken other) {
            return m_value == other.m_value;
        }

        public static bool operator ==(MetadataToken self, MetadataToken other) {
            return self.Equals(other);
        }

        public static bool operator !=(MetadataToken self, MetadataToken other) {
            return self.Equals(other);
        }

        // SECURITY: Nothing unsafe here.
        [SecuritySafeCritical]
        public override int GetHashCode() {
            return m_value;
        }

        public bool IsNull {
            get { return Rid == 0; }
        }

        public int Rid {
            get { return m_value & 0x00FFFFFF; }
        }

        public int Value {
            get { return m_value; }
        }

        internal MetadataTokenType TokenType { get { return (MetadataTokenType)(m_value & 0xFF000000); } }
        public MetadataRecordType RecordType { get { return (MetadataRecordType)(m_value >> 24); } }

        internal string DebugView {
            get { return String.Format(CultureInfo.InvariantCulture, "0x{0:x8}", m_value); }
        }

        public bool IsTypeRef { get { return TokenType == MetadataTokenType.TypeRef; } }
        public bool IsTypeDef { get { return TokenType == MetadataTokenType.TypeDef; } }
        public bool IsFieldDef { get { return TokenType == MetadataTokenType.FieldDef; } }
        public bool IsMethodDef { get { return TokenType == MetadataTokenType.MethodDef; } }
        public bool IsMemberRef { get { return TokenType == MetadataTokenType.MemberRef; } }
        public bool IsEvent { get { return TokenType == MetadataTokenType.Event; } }
        public bool IsProperty { get { return TokenType == MetadataTokenType.Property; } }
        public bool IsParamDef { get { return TokenType == MetadataTokenType.ParamDef; } }
        public bool IsTypeSpec { get { return TokenType == MetadataTokenType.TypeSpec; } }
        public bool IsMethodSpec { get { return TokenType == MetadataTokenType.MethodSpec; } }
        public bool IsString { get { return TokenType == MetadataTokenType.String; } }
        public bool IsSignature { get { return TokenType == MetadataTokenType.Signature; } }
        public bool IsCustomAttribute { get { return TokenType == MetadataTokenType.CustomAttribute; } }
        public bool IsGenericParam { get { return TokenType == MetadataTokenType.GenericPar; } }
        public bool IsGenericParamContraint { get { return TokenType == MetadataTokenType.GenericParamConstraint; } }
    }
}

namespace Microsoft.Scripting.Metadata {
    [DebuggerDisplay("{DebugView}")]
    public partial struct MetadataRecord : IEquatable<MetadataRecord> {
        internal readonly MetadataToken m_token;
        internal readonly MetadataTables m_tables;
        
        internal MetadataRecord(MetadataToken token, MetadataTables tables) {
            Contract.Assert(tables != null);
            m_token = token;
            m_tables = tables;
        }

        public MetadataTables Tables {
            get { return m_tables; }
        }

        public MetadataToken Token {
            get { return m_token; }
        }

        // SECURITY: Nothing unsafe here.
        [SecuritySafeCritical]
        public override bool Equals(object obj) {
            return obj is MetadataRecord && Equals((MetadataRecord)obj);
        }

        // SECURITY: Nothing unsafe here.
        [SecuritySafeCritical]
        public bool Equals(MetadataRecord other) {
            return m_token.Equals(other.m_token) && ReferenceEquals(m_tables, other.m_tables);
        }

        public static bool operator ==(MetadataRecord self, MetadataRecord other) {
            return self.Equals(other);
        }

        public static bool operator !=(MetadataRecord self, MetadataRecord other) {
            return self.Equals(other);
        }

        // SECURITY: Nothing unsafe here.
        [SecuritySafeCritical]
        public override int GetHashCode() {
            return m_token.GetHashCode() ^ m_tables.GetHashCode();
        }

        public bool IsNull {
            get { return m_token.IsNull; }
        }

        /// <summary>
        /// Token is null or represents a row in a metadata table.
        /// </summary>
        public bool IsValid {
            get { return m_tables.IsValidToken(m_token); }
        }

        internal static MetadataRecord Null(MetadataTables tables) {
            return new MetadataRecord(new MetadataToken(0), tables);
        }

        internal string DebugView {
            get { return m_token.DebugView; }
        }

        public MetadataRecordType Type { get { return m_token.RecordType; } }

        public bool IsAssemblyDef { get { return Type == MetadataRecordType.AssemblyDef; } }
        public bool IsAssemblyRef { get { return Type == MetadataRecordType.AssemblyRef; } }
        public bool IsModuleDef { get { return Type == MetadataRecordType.ModuleDef; } }
        public bool IsModuleRef { get { return Type == MetadataRecordType.ModuleRef; } }
        public bool IsFileDef { get { return Type == MetadataRecordType.FileDef; } }
        public bool IsManifestResourceDef { get { return Type == MetadataRecordType.ManifestResourceDef; } }
        public bool IsTypeRef { get { return Type == MetadataRecordType.TypeRef; } }
        public bool IsTypeDef { get { return Type == MetadataRecordType.TypeDef; } }
        public bool IsTypeSpec { get { return Type == MetadataRecordType.TypeSpec; } }
        public bool IsTypeExport { get { return Type == MetadataRecordType.TypeExport; } }
        public bool IsTypeNesting { get { return Type == MetadataRecordType.TypeNesting; } }
        public bool IsMemberRef { get { return Type == MetadataRecordType.MemberRef; } }
        public bool IsFieldDef { get { return Type == MetadataRecordType.FieldDef; } }
        public bool IsMethodDef { get { return Type == MetadataRecordType.MethodDef; } }
        public bool IsMethodSpec { get { return Type == MetadataRecordType.MethodSpec; } }
        public bool IsInterfaceImpl { get { return Type == MetadataRecordType.InterfaceImpl; } }
        public bool IsEvent { get { return Type == MetadataRecordType.EventDef; } }
        public bool IsProperty { get { return Type == MetadataRecordType.PropertyDef; } }
        public bool IsParamDef { get { return Type == MetadataRecordType.ParamDef; } }
        public bool IsGenericParamDef { get { return Type == MetadataRecordType.GenericParamDef; } }
        public bool IsGenericParamConstraint { get { return Type == MetadataRecordType.GenericParamConstraint; } }
        public bool IsSignatureDef { get { return Type == MetadataRecordType.SignatureDef; } }
        public bool IsCustomAttributeDef { get { return Type == MetadataRecordType.CustomAttributeDef; } }

        public AssemblyDef AssemblyDef { get { return new AssemblyDef(this); } }
        public AssemblyRef AssemblyRef { get { return new AssemblyRef(this); } }
        public FileDef FileDef { get { return new FileDef(this); } }
        public ManifestResourceDef ResourceDef { get { return new ManifestResourceDef(this); } }
        public ModuleDef ModuleDef { get { return new ModuleDef(this); } }
        public ModuleRef ModuleRef { get { return new ModuleRef(this); } }
        public TypeDef TypeDef { get { return new TypeDef(this); } }
        public TypeRef TypeRef { get { return new TypeRef(this); } }
        public TypeSpec TypeSpec { get { return new TypeSpec(this); } }
        public TypeNesting TypeNesting { get { return new TypeNesting(this); } }
        public TypeExport TypeExport { get { return new TypeExport(this); } }
        public InterfaceImpl InterfaceImpl { get { return new InterfaceImpl(this); } }
        public FieldDef FieldDef { get { return new FieldDef(this); } }
        public MethodDef MethodDef { get { return new MethodDef(this); } }
        public MethodSpec MethodSpec { get { return new MethodSpec(this); } }
        public ParamDef ParamDef { get { return new ParamDef(this); } }
        public MemberRef MemberRef { get { return new MemberRef(this); } }
        public EventDef EventDef { get { return new EventDef(this); } }
        public PropertyDef PropertyDef { get { return new PropertyDef(this); } }
        public GenericParamDef GenericParamDef { get { return new GenericParamDef(this); } }
        public GenericParamConstraint GenericParamConstraint { get { return new GenericParamConstraint(this); } }
        public CustomAttributeDef CustomAttributeDef { get { return new CustomAttributeDef(this); } }
        public SignatureDef SignatureDef { get { return new SignatureDef(this); } }
    }

    public sealed partial class MetadataTables {
        internal readonly MetadataImport m_import;

        // path of an unloaded module, null for loaded modules and in-memory modules:
        internal readonly string m_path;

        /// <summary>
        /// Gets the path of the module whose metadata tables this instance represents.
        /// Null for in-memory modules that are not backed by a file.
        /// </summary>
        /// <exception cref="SecurityException">The path is not accessible in partial trust.</exception>
        public string Path {
            get { return (Module != null) ? Module.Assembly.Location : m_path; }
        }

        public ModuleDef ModuleDef {
            get { return new MetadataRecord(new MetadataToken(MetadataTokenType.Module, 1), this).ModuleDef; }
        }

        /// <summary>
        /// Returns AssemblyDef for manifest modules, null token otherwise.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public AssemblyDef AssemblyDef {
            get {
                switch (GetRowCount((int)MetadataTokenType.Assembly >> 24)) {
                    case 0: return MetadataRecord.Null(this).AssemblyDef;
                    case 1: return new MetadataRecord(new MetadataToken(MetadataTokenType.Assembly, 1), this).AssemblyDef;
                    default: throw new BadImageFormatException();
                }
            }
        }

        public MetadataTableView AssemblyRefs {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.AssemblyRef); }
        }

        public MetadataTableView ModuleRefs {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.ModuleRef); }
        }

        public MetadataTableView Files {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.File); }
        }

        public MetadataTableView ManifestResources {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.ManifestResource); }
        }

        public MetadataTableView TypeDefs {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.TypeDef); }
        }

        public MetadataTableView TypeSpecs {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.TypeSpec); }
        }

        public MetadataTableView TypeRefs {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.TypeRef); }
        }

        public MetadataTableView TypeNestings {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.NestedClass); }
        }

        public MetadataTableView TypeExports {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.ExportedType); }
        }

        public MetadataTableView MethodDefs {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.MethodDef); }
        }

        public MetadataTableView MethodSpecs {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.MethodSpec); }
        }

        public MetadataTableView FieldDefs {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.FieldDef); }
        }

        public MetadataTableView MemberRefs {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.MemberRef); }
        }

        public MetadataTableView Signatures {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.Signature); }
        }

        public MetadataTableView CustomAttributes {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.CustomAttribute); }
        }

        public MetadataTableView InterfacesImpls {
            get { return new MetadataTableView(MetadataRecord.Null(this), MetadataTokenType.InterfaceImpl); }
        }

        public MetadataRecord GetRecord(MetadataToken token) {
            return new MetadataRecord(token, this);
        }
    }

    public partial struct MetadataTableView {
        private readonly MetadataTokenType m_type;
        private readonly MetadataRecord m_parent;

        internal MetadataTableView(MetadataRecord parent, MetadataTokenType type) {
            m_type = type;
            m_parent = parent;
        }
    }

    /// <summary>
    /// Module table entry (0x00 tokens).
    /// </summary>
    public partial struct ModuleDef {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(ModuleDef moduleDef) {
            return moduleDef.m_record;
        }

        public static explicit operator ModuleDef(MetadataRecord record) {
            return record.ModuleDef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }

        public MetadataTableView CustomAttributes {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.CustomAttribute);
            }
        }
    }

    /// <summary>
    /// TypeRef table entry (0x01 tokens).
    /// </summary>
    public partial struct TypeRef {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(TypeRef typeRef) {
            return typeRef.m_record;
        }

        public static explicit operator TypeRef(MetadataRecord record) {
            return record.TypeRef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }
    }

    /// <summary>
    /// TypeDef table entry (0x02 tokens).
    /// </summary>
    public partial struct TypeDef {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(TypeDef typeDef) {
            return typeDef.m_record;
        }

        public static explicit operator TypeDef(MetadataRecord record) {
            return record.TypeDef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }

        /// <summary>
        /// This typedef represents a container of global functions and fields (manufactured &lt;Module&gt; type).
        /// </summary>
        public bool IsGlobal {
            get { return m_record.m_token.Rid == 1; }
        }

        public MetadataTableView ImplementedInterfaces {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.InterfaceImpl);
            }
        }

        public MetadataTableView GenericParameters {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.GenericPar);
            }
        }

        public MetadataTableView Fields {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.FieldDef);
            }
        }

        public MetadataTableView Methods {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.MethodDef);
            }
        }

        public MetadataTableView Properties {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.Property);
            }
        }

        public MetadataTableView Events {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.Event);
            }
        }

        public MetadataTableView CustomAttributes {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.CustomAttribute);
            }
        }

        // TODO: layout
    }

    /// <summary>
    /// Combines Field (0x04 tokens), FieldRVA (0x1d tokens) and Constant (0x0B) table entries.
    /// </summary>
    public partial struct FieldDef {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(FieldDef fieldDef) {
            return fieldDef.m_record;
        }

        public static explicit operator FieldDef(MetadataRecord record) {
            return record.FieldDef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }

        public MetadataTableView CustomAttributes {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.CustomAttribute);
            }
        }
    }

    /// <summary>
    /// MethodDef table entry (0x06 tokens).
    /// </summary>
    public partial struct MethodDef {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(MethodDef methodDef) {
            return methodDef.m_record;
        }

        public static explicit operator MethodDef(MetadataRecord record) {
            return record.MethodDef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }

        public MetadataTableView Parameters {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.ParamDef);
            }
        }

        public MetadataTableView GenericParameters {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.GenericPar);
            }
        }

        public MetadataTableView CustomAttributes {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.CustomAttribute);
            }
        }

        // TODO: FindAssociate: event/property
    }

    /// <summary>
    /// Param table entry (0x08 tokens).
    /// </summary>
    public partial struct ParamDef {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(ParamDef paramDef) {
            return paramDef.m_record;
        }

        public static explicit operator ParamDef(MetadataRecord record) {
            return record.ParamDef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }

        public MetadataTableView CustomAttributes {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.CustomAttribute);
            }
        }
    }

    /// <summary>
    /// InterfaceImpl table entry (0x09 tokens).
    /// TODO: we might not need this - TypeDef.ImplementedInterfaces might be a special enumerator that directly returns InterfaceType tokens.
    /// </summary>
    public partial struct InterfaceImpl {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(InterfaceImpl paramDef) {
            return paramDef.m_record;
        }

        public static explicit operator InterfaceImpl(MetadataRecord record) {
            return record.InterfaceImpl;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }
    }

    /// <summary>
    /// MemberRef table entry (0x0A tokens).
    /// Stores MethodRefs and FieldRefs.
    /// </summary>
    public partial struct MemberRef {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(MemberRef memberRef) {
            return memberRef.m_record;
        }

        public static explicit operator MemberRef(MetadataRecord record) {
            return record.MemberRef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }
    }

    // Constant (0x0B) - Param, Field, Property 

    /// <summary>
    /// CustomAttribute table entry (0x0C tokens).
    /// </summary>
    public partial struct CustomAttributeDef {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(CustomAttributeDef customAttributeDef) {
            return customAttributeDef.m_record;
        }

        public static explicit operator CustomAttributeDef(MetadataRecord record) {
            return record.CustomAttributeDef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }

        // TODO: return value?, can be null
        // public CustomAttributeValue Value { get { return null; } }
    }

    // FieldMarshal 0x0D
    // DeclSecurity 0x0E      // SecurityAttributes view on TypeDef, MethodDef, and AssemblyDef
    // FieldLayout  0x10

    /// <summary>
    /// StandAloneSig table entry (0x11 token).
    /// </summary>
    public partial struct SignatureDef {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(SignatureDef signatureDef) {
            return signatureDef.m_record;
        }

        public static explicit operator SignatureDef(MetadataRecord record) {
            return record.SignatureDef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }
    }

    /// <summary>
    /// Combines information from PropertyMap (0x15), MethodSemantics (0x18) and Property (0x17) tables.
    /// </summary>
    public partial struct PropertyDef {
        // index into Property table
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(PropertyDef propertyDef) {
            return propertyDef.m_record;
        }

        public static explicit operator PropertyDef(MetadataRecord record) {
            return record.PropertyDef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }

        public MetadataTableView CustomAttributes {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.CustomAttribute);
            }
        }
    }

    public partial struct PropertyAccessors {
        private readonly PropertyDef m_property;

        // method-defs or null tokens:
        private readonly MetadataToken m_getter, m_setter;

        internal PropertyAccessors(PropertyDef propertyDef, MetadataToken getter, MetadataToken setter) {
            Contract.Assert(getter.IsMethodDef);
            Contract.Assert(setter.IsMethodDef);
            m_getter = getter;
            m_setter = setter;
            m_property = propertyDef;
        }

        public PropertyDef DeclaringProperty {
            get { return m_property; }
        }

        public bool HasGetter {
            get { return !m_getter.IsNull; }
        }

        public bool HasSetter {
            get { return !m_setter.IsNull; }
        }

        public MethodDef Getter { get { return new MetadataRecord(m_getter, m_property.Record.Tables).MethodDef; } }
        public MethodDef Setter { get { return new MetadataRecord(m_setter, m_property.Record.Tables).MethodDef; } }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public IEnumerable<MethodDef> Others {
            get {
                // TODO: 
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// Combines information from EventMap (0x15), MethodSemantics (0x18) and Event (0x17) tables.
    /// </summary>
    public partial struct EventDef {
        // index into Event table
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(EventDef eventDef) {
            return eventDef.m_record;
        }

        public static explicit operator EventDef(MetadataRecord record) {
            return record.EventDef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }

        public MetadataTableView CustomAttributes {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.CustomAttribute);
            }
        }
    }

    public partial struct EventAccessors {
        private readonly EventDef m_event;

        // method-defs or null tokens:
        private readonly MetadataToken m_add, m_remove, m_fire;

        internal EventAccessors(EventDef eventDef, MetadataToken add, MetadataToken remove, MetadataToken fire) {
            Contract.Assert(add.IsMethodDef);
            Contract.Assert(remove.IsMethodDef);
            Contract.Assert(fire.IsMethodDef);
            m_add = add;
            m_remove = remove;
            m_fire = fire;
            m_event = eventDef;
        }

        public EventDef DeclaringEvent {
            get { return m_event; }
        }

        public bool HasAdd { get { return !m_add.IsNull; } }
        public bool HasRemove { get { return !m_remove.IsNull; } }
        public bool HasFire { get { return !m_fire.IsNull; } }

        public MethodDef Add { get { return new MetadataRecord(m_add, m_event.Record.Tables).MethodDef; } }
        public MethodDef Remove { get { return new MetadataRecord(m_remove, m_event.Record.Tables).MethodDef; } }
        public MethodDef Fire { get { return new MetadataRecord(m_fire, m_event.Record.Tables).MethodDef; } }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public IEnumerable<MethodDef> Others {
            get {
                // TODO: 
                throw new NotImplementedException();
            }
        }
    }

    // MethodImpl (0x19)

    /// <summary>
    /// ModuleRef table entry (0x1A tokens).
    /// </summary>
    public partial struct ModuleRef {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(ModuleRef moduleDef) {
            return moduleDef.m_record;
        }

        public static explicit operator ModuleRef(MetadataRecord record) {
            return record.ModuleRef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }
    }

    /// <summary>
    /// TypeSpec table entry (0x1B tokens).
    /// </summary>
    public partial struct TypeSpec {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(TypeSpec typeSpec) {
            return typeSpec.m_record;
        }

        public static explicit operator TypeSpec(MetadataRecord record) {
            return record.TypeSpec;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }
    }

    // ImplMap      0x1C  (P/Invokes, not needed)

    /// <summary>
    /// Assembly table entry (0x20 tokens).
    /// </summary>
    public partial struct AssemblyDef {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(AssemblyDef assemblyDef) {
            return assemblyDef.m_record;
        }

        public static explicit operator AssemblyDef(MetadataRecord record) {
            return record.AssemblyDef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }

        public MetadataTableView CustomAttributes {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.CustomAttribute);
            }
        }
    }

    /// <summary>
    /// Assembly table entry (0x23 tokens).
    /// </summary>
    public partial struct AssemblyRef {   // TODO: AssemblyRef name is already an internal class 
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(AssemblyRef assemblyRef) {
            return assemblyRef.m_record;
        }

        public static explicit operator AssemblyRef(MetadataRecord record) {
            return record.AssemblyRef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }
    }

    /// <summary>
    /// File table entry (0x26 tokens).
    /// </summary>
    public partial struct FileDef {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(FileDef fileDef) {
            return fileDef.m_record;
        }

        public static explicit operator FileDef(MetadataRecord record) {
            return record.FileDef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }
    }

    /// <summary>
    /// ExportedType table entry (0x27 tokens).
    /// </summary>
    public partial struct TypeExport {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(TypeExport typeExport) {
            return typeExport.m_record;
        }

        public static explicit operator TypeExport(MetadataRecord record) {
            return record.TypeExport;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }
    }

    /// <summary>
    /// ManifestResource table entry (0x28 tokens).
    /// </summary>
    public partial struct ManifestResourceDef {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(ManifestResourceDef resourceDef) {
            return resourceDef.m_record;
        }

        public static explicit operator ManifestResourceDef(MetadataRecord record) {
            return record.ResourceDef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }
    }

    /// <summary>
    /// NestedClass table entry (0x29 tokens).
    /// TODO: Don't need if we exposed nested types enumeration on type-def directly and build TypeNesting mapping lazily.
    /// </summary>
    public partial struct TypeNesting {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(TypeNesting nestedClassDef) {
            return nestedClassDef.m_record;
        }

        public static explicit operator TypeNesting(MetadataRecord record) {
            return record.TypeNesting;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }
    }

    /// <summary>
    /// GenericParam table entry (0x2A tokens).
    /// </summary>
    public partial struct GenericParamDef {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(GenericParamDef genericParamDef) {
            return genericParamDef.m_record;
        }

        public static explicit operator GenericParamDef(MetadataRecord record) {
            return record.GenericParamDef;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }

        public MetadataTableView Constraints {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.GenericParamConstraint);
            }
        }

        public MetadataTableView CustomAttributes {
            get {
                return new MetadataTableView(m_record, MetadataTokenType.CustomAttribute);
            }
        }
    }

    /// <summary>
    /// GenericParamConstraint table entry (0x2C tokens).
    /// </summary>
    public partial struct GenericParamConstraint {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(GenericParamConstraint constraint) {
            return constraint.m_record;
        }

        public static explicit operator GenericParamConstraint(MetadataRecord record) {
            return record.GenericParamConstraint;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }
    }

    /// <summary>
    /// MethodSpec table entry (0x2B tokens).
    /// Used when decoding IL instructions.
    /// </summary>
    public partial struct MethodSpec {
        private readonly MetadataRecord m_record;

        public static implicit operator MetadataRecord(MethodSpec methodSpec) {
            return methodSpec.m_record;
        }

        public static explicit operator MethodSpec(MetadataRecord record) {
            return record.MethodSpec;
        }

        public MetadataRecord Record {
            get { return m_record; }
        }
    }
}
