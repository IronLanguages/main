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

namespace Microsoft.Scripting.Metadata {
    internal enum PEMagic : ushort {
        PEMagic32 = 0x010B,
        PEMagic64 = 0x020B,
    }

    internal enum MetadataStreamKind {
        Illegal,
        Compressed,
        UnCompressed,
    }

    internal enum MetadataTokenType {
        Module = 0x00000000,
        TypeRef = 0x01000000,
        TypeDef = 0x02000000,
        FieldDef = 0x04000000,
        MethodDef = 0x06000000,
        ParamDef = 0x08000000,
        InterfaceImpl = 0x09000000,
        MemberRef = 0x0a000000,
        CustomAttribute = 0x0c000000,
        Permission = 0x0e000000,
        Signature = 0x11000000,
        Event = 0x14000000,
        Property = 0x17000000,
        ModuleRef = 0x1a000000,
        TypeSpec = 0x1b000000,
        Assembly = 0x20000000,
        AssemblyRef = 0x23000000,
        File = 0x26000000,
        ExportedType = 0x27000000,
        ManifestResource = 0x28000000,
        NestedClass = 0x29000000,
        GenericPar = 0x2a000000,
        MethodSpec = 0x2b000000,
        GenericParamConstraint = 0x2c000000,

        String = 0x70000000,
        Name = 0x71000000,
        BaseType = 0x72000000,
        Invalid = 0x7FFFFFFF,
    }

    internal enum TableMask : ulong {
        Module = 0x0000000000000001UL << 0x00,
        TypeRef = 0x0000000000000001UL << 0x01,
        TypeDef = 0x0000000000000001UL << 0x02,
        FieldPtr = 0x0000000000000001UL << 0x03,
        Field = 0x0000000000000001UL << 0x04,
        MethodPtr = 0x0000000000000001UL << 0x05,
        Method = 0x0000000000000001UL << 0x06,
        ParamPtr = 0x0000000000000001UL << 0x07,
        Param = 0x0000000000000001UL << 0x08,
        InterfaceImpl = 0x0000000000000001UL << 0x09,
        MemberRef = 0x0000000000000001UL << 0x0A,
        Constant = 0x0000000000000001UL << 0x0B,
        CustomAttribute = 0x0000000000000001UL << 0x0C,
        FieldMarshal = 0x0000000000000001UL << 0x0D,
        DeclSecurity = 0x0000000000000001UL << 0x0E,
        ClassLayout = 0x0000000000000001UL << 0x0F,
        FieldLayout = 0x0000000000000001UL << 0x10,
        StandAloneSig = 0x0000000000000001UL << 0x11,
        EventMap = 0x0000000000000001UL << 0x12,
        EventPtr = 0x0000000000000001UL << 0x13,
        Event = 0x0000000000000001UL << 0x14,
        PropertyMap = 0x0000000000000001UL << 0x15,
        PropertyPtr = 0x0000000000000001UL << 0x16,
        Property = 0x0000000000000001UL << 0x17,
        MethodSemantics = 0x0000000000000001UL << 0x18,
        MethodImpl = 0x0000000000000001UL << 0x19,
        ModuleRef = 0x0000000000000001UL << 0x1A,
        TypeSpec = 0x0000000000000001UL << 0x1B,
        ImplMap = 0x0000000000000001UL << 0x1C,
        FieldRva = 0x0000000000000001UL << 0x1D,
        EnCLog = 0x0000000000000001UL << 0x1E,
        EnCMap = 0x0000000000000001UL << 0x1F,
        Assembly = 0x0000000000000001UL << 0x20,
        AssemblyProcessor = 0x0000000000000001UL << 0x21,
        AssemblyOS = 0x0000000000000001UL << 0x22,
        AssemblyRef = 0x0000000000000001UL << 0x23,
        AssemblyRefProcessor = 0x0000000000000001UL << 0x24,
        AssemblyRefOS = 0x0000000000000001UL << 0x25,
        File = 0x0000000000000001UL << 0x26,
        ExportedType = 0x0000000000000001UL << 0x27,
        ManifestResource = 0x0000000000000001UL << 0x28,
        NestedClass = 0x0000000000000001UL << 0x29,
        GenericParam = 0x0000000000000001UL << 0x2A,
        MethodSpec = 0x0000000000000001UL << 0x2B,
        GenericParamConstraint = 0x0000000000000001UL << 0x2C,

        SortedTablesMask =
          TableMask.ClassLayout
          | TableMask.Constant
          | TableMask.CustomAttribute
          | TableMask.DeclSecurity
          | TableMask.FieldLayout
          | TableMask.FieldMarshal
          | TableMask.FieldRva
          | TableMask.GenericParam
          | TableMask.GenericParamConstraint
          | TableMask.ImplMap
          | TableMask.InterfaceImpl
          | TableMask.MethodImpl
          | TableMask.MethodSemantics
          | TableMask.NestedClass,
        CompressedStreamNotAllowedMask =
          TableMask.FieldPtr
          | TableMask.MethodPtr
          | TableMask.ParamPtr
          | TableMask.EventPtr
          | TableMask.PropertyPtr
          | TableMask.EnCLog
          | TableMask.EnCMap,
        V1_0_TablesMask =
          TableMask.Module
          | TableMask.TypeRef
          | TableMask.TypeDef
          | TableMask.FieldPtr
          | TableMask.Field
          | TableMask.MethodPtr
          | TableMask.Method
          | TableMask.ParamPtr
          | TableMask.Param
          | TableMask.InterfaceImpl
          | TableMask.MemberRef
          | TableMask.Constant
          | TableMask.CustomAttribute
          | TableMask.FieldMarshal
          | TableMask.DeclSecurity
          | TableMask.ClassLayout
          | TableMask.FieldLayout
          | TableMask.StandAloneSig
          | TableMask.EventMap
          | TableMask.EventPtr
          | TableMask.Event
          | TableMask.PropertyMap
          | TableMask.PropertyPtr
          | TableMask.Property
          | TableMask.MethodSemantics
          | TableMask.MethodImpl
          | TableMask.ModuleRef
          | TableMask.TypeSpec
          | TableMask.ImplMap
          | TableMask.FieldRva
          | TableMask.EnCLog
          | TableMask.EnCMap
          | TableMask.Assembly
          | TableMask.AssemblyRef
          | TableMask.File
          | TableMask.ExportedType
          | TableMask.ManifestResource
          | TableMask.NestedClass,
        V1_1_TablesMask =
          TableMask.Module
          | TableMask.TypeRef
          | TableMask.TypeDef
          | TableMask.FieldPtr
          | TableMask.Field
          | TableMask.MethodPtr
          | TableMask.Method
          | TableMask.ParamPtr
          | TableMask.Param
          | TableMask.InterfaceImpl
          | TableMask.MemberRef
          | TableMask.Constant
          | TableMask.CustomAttribute
          | TableMask.FieldMarshal
          | TableMask.DeclSecurity
          | TableMask.ClassLayout
          | TableMask.FieldLayout
          | TableMask.StandAloneSig
          | TableMask.EventMap
          | TableMask.EventPtr
          | TableMask.Event
          | TableMask.PropertyMap
          | TableMask.PropertyPtr
          | TableMask.Property
          | TableMask.MethodSemantics
          | TableMask.MethodImpl
          | TableMask.ModuleRef
          | TableMask.TypeSpec
          | TableMask.ImplMap
          | TableMask.FieldRva
          | TableMask.EnCLog
          | TableMask.EnCMap
          | TableMask.Assembly
          | TableMask.AssemblyRef
          | TableMask.File
          | TableMask.ExportedType
          | TableMask.ManifestResource
          | TableMask.NestedClass,
        V2_0_TablesMask =
          TableMask.Module
          | TableMask.TypeRef
          | TableMask.TypeDef
          | TableMask.FieldPtr
          | TableMask.Field
          | TableMask.MethodPtr
          | TableMask.Method
          | TableMask.ParamPtr
          | TableMask.Param
          | TableMask.InterfaceImpl
          | TableMask.MemberRef
          | TableMask.Constant
          | TableMask.CustomAttribute
          | TableMask.FieldMarshal
          | TableMask.DeclSecurity
          | TableMask.ClassLayout
          | TableMask.FieldLayout
          | TableMask.StandAloneSig
          | TableMask.EventMap
          | TableMask.EventPtr
          | TableMask.Event
          | TableMask.PropertyMap
          | TableMask.PropertyPtr
          | TableMask.Property
          | TableMask.MethodSemantics
          | TableMask.MethodImpl
          | TableMask.ModuleRef
          | TableMask.TypeSpec
          | TableMask.ImplMap
          | TableMask.FieldRva
          | TableMask.EnCLog
          | TableMask.EnCMap
          | TableMask.Assembly
          | TableMask.AssemblyRef
          | TableMask.File
          | TableMask.ExportedType
          | TableMask.ManifestResource
          | TableMask.NestedClass
          | TableMask.GenericParam
          | TableMask.MethodSpec
          | TableMask.GenericParamConstraint,
    }

    internal enum HeapSizeFlag : byte {
        StringHeapLarge = 0x01, //  4 byte uint indexes used for string heap offsets
        GUIDHeapLarge = 0x02,   //  4 byte uint indexes used for GUID heap offsets
        BlobHeapLarge = 0x04,   //  4 byte uint indexes used for Blob heap offsets
        EnCDeltas = 0x20,       //  Indicates only EnC Deltas are present
        DeletedMarks = 0x80,    //  Indicates metadata might contain items marked deleted
    }

    internal enum MethodSemanticsFlags : ushort {
        Setter = 0x0001,
        Getter = 0x0002,
        Other = 0x0004,
        AddOn = 0x0008,
        RemoveOn = 0x0010,
        Fire = 0x0020,
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum ElementType : byte {
        End = 0x00,
        Void = 0x01,
        Boolean = 0x02,
        Char = 0x03,
        Int8 = 0x04,
        UInt8 = 0x05,
        Int16 = 0x06,
        UInt16 = 0x07,
        Int32 = 0x08,
        UInt32 = 0x09,
        Int64 = 0x0a,
        UInt64 = 0x0b,
        Single = 0x0c,
        Double = 0x0d,
        String = 0x0e,
        
        Pointer = 0x0f,
        ByReference = 0x10,
        
        ValueType = 0x11,
        Class = 0x12,
        GenericTypeParameter = 0x13,
        Array = 0x14,
        GenericTypeInstance = 0x15,
        TypedReference = 0x16,
        
        IntPtr = 0x18,
        UIntPtr = 0x19,
        FunctionPointer = 0x1b,
        Object = 0x1c,
        Vector = 0x1d,
        
        GenericMethodParameter = 0x1e,
        
        RequiredModifier = 0x1f,
        OptionalModifier = 0x20,
        
        Internal = 0x21,
        
        Max = 0x22,
        
        Modifier = 0x40,
        Sentinel = 0x41,
        Pinned = 0x45,
        // SingleHFA = 0x54, //  What is this?
        // DoubleHFA = 0x55, //  What is this?
    }

    // TODO: merge with MetadataSignature
    public static class SignatureHeader {
        public const byte DefaultCall = 0x00;
        public const byte CCall = 0x01;
        public const byte StdCall = 0x02;
        public const byte ThisCall = 0x03;
        public const byte FastCall = 0x04;
        public const byte VarArgCall = 0x05;
        public const byte Field = 0x06;
        public const byte LocalVar = 0x07;
        public const byte Property = 0x08;
        //public const byte UnManaged = 0x09;  //  Not used as of now in CLR
        public const byte GenericInstance = 0x0A;
        //public const byte NativeVarArg = 0x0B;  //  Not used as of now in CLR
        public const byte Max = 0x0C;
        public const byte CallingConventionMask = 0x0F;

        public const byte HasThis = 0x20;
        public const byte ExplicitThis = 0x40;
        public const byte Generic = 0x10;

        public static bool IsMethodSignature(byte signatureHeader) {
            return (signatureHeader & SignatureHeader.CallingConventionMask) <= SignatureHeader.VarArgCall;
        }

        public static bool IsVarArgCallSignature(byte signatureHeader) {
            return (signatureHeader & SignatureHeader.CallingConventionMask) == SignatureHeader.VarArgCall;
        }

        public static bool IsFieldSignature(byte signatureHeader) {
            return (signatureHeader & SignatureHeader.CallingConventionMask) == SignatureHeader.Field;
        }

        public static bool IsLocalVarSignature(byte signatureHeader) {
            return (signatureHeader & SignatureHeader.CallingConventionMask) == SignatureHeader.LocalVar;
        }

        public static bool IsPropertySignature(byte signatureHeader) {
            return (signatureHeader & SignatureHeader.CallingConventionMask) == SignatureHeader.Property;
        }

        public static bool IsGenericInstanceSignature(byte signatureHeader) {
            return (signatureHeader & SignatureHeader.CallingConventionMask) == SignatureHeader.GenericInstance;
        }

        public static bool IsExplicitThis(byte signatureHeader) {
            return (signatureHeader & SignatureHeader.ExplicitThis) == SignatureHeader.ExplicitThis;
        }

        public static bool IsGeneric(byte signatureHeader) {
            return (signatureHeader & SignatureHeader.Generic) == SignatureHeader.Generic;
        }
    }
    
    #region PEFile specific data

    internal static class PEFileConstants {
        internal const ushort DosSignature = 0x5A4D;     // MZ
        internal const int PESignatureOffsetLocation = 0x3C;
        internal const uint PESignature = 0x00004550;    // PE00
        internal const int BasicPEHeaderSize = PEFileConstants.PESignatureOffsetLocation;
        internal const int SizeofCOFFFileHeader = 20;
        internal const int SizeofOptionalHeaderStandardFields32 = 28;
        internal const int SizeofOptionalHeaderStandardFields64 = 24;
        internal const int SizeofOptionalHeaderNTAdditionalFields32 = 68;
        internal const int SizeofOptionalHeaderNTAdditionalFields64 = 88;
        internal const int NumberofOptionalHeaderDirectoryEntries = 16;
        internal const int SizeofOptionalHeaderDirectoriesEntries = 64;
        internal const int SizeofSectionHeader = 40;
        internal const int SizeofSectionName = 8;
        internal const int SizeofResourceDirectory = 16;
        internal const int SizeofResourceDirectoryEntry = 8;
    }

    internal struct DirectoryEntry {
        internal uint RelativeVirtualAddress;
        internal uint Size;
    }

    internal struct OptionalHeaderDirectoryEntries {
        // commented fields not needed:
        // internal DirectoryEntry ExportTableDirectory;
        // internal DirectoryEntry ImportTableDirectory;
        internal DirectoryEntry ResourceTableDirectory;
        // internal DirectoryEntry ExceptionTableDirectory;
        // internal DirectoryEntry CertificateTableDirectory;
        // internal DirectoryEntry BaseRelocationTableDirectory;
        // internal DirectoryEntry DebugTableDirectory;
        // internal DirectoryEntry CopyrightTableDirectory;
        // internal DirectoryEntry GlobalPointerTableDirectory;
        // internal DirectoryEntry ThreadLocalStorageTableDirectory;
        // internal DirectoryEntry LoadConfigTableDirectory;
        // internal DirectoryEntry BoundImportTableDirectory;
        // internal DirectoryEntry ImportAddressTableDirectory;
        // internal DirectoryEntry DelayImportTableDirectory;
        internal DirectoryEntry COR20HeaderTableDirectory;
        // internal DirectoryEntry ReservedDirectory;
    }

    internal struct SectionHeader {
        // commented fields not needed:
        // internal string Name;
        internal uint VirtualSize;
        internal uint VirtualAddress;
        internal uint SizeOfRawData;
        internal uint OffsetToRawData;
        // internal uint RVAToRelocations;
        // internal uint PointerToLineNumbers;
        // internal ushort NumberOfRelocations;
        // internal ushort NumberOfLineNumbers;
        // internal SectionCharacteristics SectionCharacteristics;
    }

    #endregion PEFile specific data

    #region CLR Header Specific data

    internal static class COR20Constants {
        internal const int SizeOfCOR20Header = 72;
        internal const uint COR20MetadataSignature = 0x424A5342;
        internal const int MinimumSizeofMetadataHeader = 16;
        internal const int SizeofStorageHeader = 4;
        internal const int MinimumSizeofStreamHeader = 8;
        internal const string StringStreamName = "#Strings";
        internal const string BlobStreamName = "#Blob";
        internal const string GUIDStreamName = "#GUID";
        internal const string UserStringStreamName = "#US";
        internal const string CompressedMetadataTableStreamName = "#~";
        internal const string UncompressedMetadataTableStreamName = "#-";
        internal const int LargeStreamHeapSize = 0x0001000;
    }

    internal struct COR20Header {
        // internal int CountBytes;
        // internal ushort MajorRuntimeVersion;
        // internal ushort MinorRuntimeVersion;
        internal DirectoryEntry MetaDataDirectory;
        // internal COR20Flags COR20Flags;
        // internal uint EntryPointTokenOrRVA;
        internal DirectoryEntry ResourcesDirectory;
        internal DirectoryEntry StrongNameSignatureDirectory;
        // internal DirectoryEntry CodeManagerTableDirectory;
        // internal DirectoryEntry VtableFixupsDirectory;
        // internal DirectoryEntry ExportAddressTableJumpsDirectory;
        // internal DirectoryEntry ManagedNativeHeaderDirectory;
    }

    internal struct StorageHeader {
        internal ushort Flags;
        internal ushort NumberOfStreams;
    }

    internal struct StreamHeader {
        internal uint Offset;
        internal uint Size;
        internal string Name;
    }

    #endregion CLR Header Specific data

    #region Metadata Stream Specific data

    internal static class MetadataStreamConstants {
        internal const int SizeOfMetadataTableHeader = 24;
        internal const int LargeTableRowCount = 0x00010000;
    }

    internal struct MetadataTableHeader {
        // internal uint Reserved;
        internal byte MajorVersion;
        internal byte MinorVersion;
        internal HeapSizeFlag HeapSizeFlags;
        // internal byte RowId;
        internal TableMask ValidTables;
        internal TableMask SortedTables;
        internal int[] CompressedMetadataTableRowCount;

        //  Helper methods
        internal int GetNumberOfTablesPresent() {
            const ulong MASK_01010101010101010101010101010101 = 0x5555555555555555UL;
            const ulong MASK_00110011001100110011001100110011 = 0x3333333333333333UL;
            const ulong MASK_00001111000011110000111100001111 = 0x0F0F0F0F0F0F0F0FUL;
            const ulong MASK_00000000111111110000000011111111 = 0x00FF00FF00FF00FFUL;
            const ulong MASK_00000000000000001111111111111111 = 0x0000FFFF0000FFFFUL;
            const ulong MASK_11111111111111111111111111111111 = 0x00000000FFFFFFFFUL;

            ulong count = (ulong)this.ValidTables;
            count = (count & MASK_01010101010101010101010101010101) + ((count >> 1) & MASK_01010101010101010101010101010101);
            count = (count & MASK_00110011001100110011001100110011) + ((count >> 2) & MASK_00110011001100110011001100110011);
            count = (count & MASK_00001111000011110000111100001111) + ((count >> 4) & MASK_00001111000011110000111100001111);
            count = (count & MASK_00000000111111110000000011111111) + ((count >> 8) & MASK_00000000111111110000000011111111);
            count = (count & MASK_00000000000000001111111111111111) + ((count >> 16) & MASK_00000000000000001111111111111111);
            count = (count & MASK_11111111111111111111111111111111) + ((count >> 32) & MASK_11111111111111111111111111111111);
            return (int)count;
        }
    }

    internal static class TypeDefOrRefTag {
        internal const int NumberOfBits = 2;
        internal const uint TypeDef = 0x00000000;
        internal const uint TypeRef = 0x00000001;
        internal const uint TypeSpec = 0x00000002;
        internal const uint TagMask = 0x00000003;
        internal const int LargeRowSize = 0x00000001 << (16 - NumberOfBits);
        internal const TableMask TablesReferenced = TableMask.TypeDef | TableMask.TypeRef | TableMask.TypeSpec;

        internal static MetadataToken ConvertToToken(uint typeDefOrRefTag) {
            MetadataTokenType tokenType;
            switch (typeDefOrRefTag & TagMask) {
                case TypeDef: tokenType = MetadataTokenType.TypeDef; break;
                case TypeRef: tokenType = MetadataTokenType.TypeRef; break;
                case TypeSpec: tokenType = MetadataTokenType.TypeSpec; break;
                default: throw new BadImageFormatException();
            }
            return new MetadataToken(tokenType, typeDefOrRefTag >> NumberOfBits);
        }
    }

    internal static class HasConstantTag {
        internal const int NumberOfBits = 2;
        internal const int LargeRowSize = 0x00000001 << (16 - HasConstantTag.NumberOfBits);
        internal const uint Field = 0x00000000;
        internal const uint Param = 0x00000001;
        internal const uint Property = 0x00000002;
        internal const uint TagMask = 0x00000003;
        internal const TableMask TablesReferenced = TableMask.Field | TableMask.Param | TableMask.Property;

        internal static MetadataToken ConvertToToken(uint hasConstant) {
            MetadataTokenType tokenType;
            switch (hasConstant & TagMask) {
                case Field: tokenType = MetadataTokenType.FieldDef; break;
                case Param: tokenType = MetadataTokenType.ParamDef; break;
                case Property: tokenType = MetadataTokenType.Property; break;
                default: throw new BadImageFormatException();
            }
            return new MetadataToken(tokenType, hasConstant >> NumberOfBits);
        }

        internal static uint ConvertToTag(MetadataToken token) {
            uint rowId = (uint)token.Rid;
            switch (token.TokenType) {
                case MetadataTokenType.FieldDef: return (rowId << NumberOfBits) | Field;
                case MetadataTokenType.ParamDef: return (rowId << NumberOfBits) | Param;
                case MetadataTokenType.Property: return (rowId << NumberOfBits) | Property;
            }
            return 0;
        }
    }

    internal static class HasCustomAttributeTag {
        internal const int NumberOfBits = 5;
        internal const int LargeRowSize = 0x00000001 << (16 - HasCustomAttributeTag.NumberOfBits);
        internal const uint Method = 0x00000000;
        internal const uint Field = 0x00000001;
        internal const uint TypeRef = 0x00000002;
        internal const uint TypeDef = 0x00000003;
        internal const uint Param = 0x00000004;
        internal const uint InterfaceImpl = 0x00000005;
        internal const uint MemberRef = 0x00000006;
        internal const uint Module = 0x00000007;
        internal const uint DeclSecurity = 0x00000008;
        internal const uint Property = 0x00000009;
        internal const uint Event = 0x0000000A;
        internal const uint StandAloneSig = 0x0000000B;
        internal const uint ModuleRef = 0x0000000C;
        internal const uint TypeSpec = 0x0000000D;
        internal const uint Assembly = 0x0000000E;
        internal const uint AssemblyRef = 0x0000000F;
        internal const uint File = 0x00000010;
        internal const uint ExportedType = 0x00000011;
        internal const uint ManifestResource = 0x00000012;
        internal const uint GenericParameter = 0x00000013;
        internal const uint TagMask = 0x0000001F;

        internal const TableMask TablesReferenced =
            TableMask.Method
          | TableMask.Field
          | TableMask.TypeRef
          | TableMask.TypeDef
          | TableMask.Param
          | TableMask.InterfaceImpl
          | TableMask.MemberRef
          | TableMask.Module
          | TableMask.DeclSecurity
          | TableMask.Property
          | TableMask.Event
          | TableMask.StandAloneSig
          | TableMask.ModuleRef
          | TableMask.TypeSpec
          | TableMask.Assembly
          | TableMask.AssemblyRef
          | TableMask.File
          | TableMask.ExportedType
          | TableMask.ManifestResource
          | TableMask.GenericParam;

        internal static MetadataToken ConvertToToken(uint hasCustomAttribute) {
            MetadataTokenType tokenType;
            switch (hasCustomAttribute & TagMask) {
                case HasCustomAttributeTag.Method: tokenType = MetadataTokenType.MethodDef; break;
                case HasCustomAttributeTag.Field: tokenType = MetadataTokenType.FieldDef; break;
                case HasCustomAttributeTag.TypeRef: tokenType = MetadataTokenType.TypeRef; break;
                case HasCustomAttributeTag.TypeDef: tokenType = MetadataTokenType.TypeDef; break;
                case HasCustomAttributeTag.Param: tokenType = MetadataTokenType.ParamDef; break;
                case HasCustomAttributeTag.InterfaceImpl: tokenType = MetadataTokenType.InterfaceImpl; break;
                case HasCustomAttributeTag.MemberRef: tokenType = MetadataTokenType.MemberRef; break;
                case HasCustomAttributeTag.Module: tokenType = MetadataTokenType.Module; break;
                case HasCustomAttributeTag.DeclSecurity: tokenType = MetadataTokenType.Permission; break;
                case HasCustomAttributeTag.Property: tokenType = MetadataTokenType.Property; break;
                case HasCustomAttributeTag.Event: tokenType = MetadataTokenType.Event; break;
                case HasCustomAttributeTag.StandAloneSig: tokenType = MetadataTokenType.Signature; break;
                case HasCustomAttributeTag.ModuleRef: tokenType = MetadataTokenType.ModuleRef; break;
                case HasCustomAttributeTag.TypeSpec: tokenType = MetadataTokenType.TypeSpec; break;
                case HasCustomAttributeTag.Assembly: tokenType = MetadataTokenType.Assembly; break;
                case HasCustomAttributeTag.AssemblyRef: tokenType = MetadataTokenType.AssemblyRef; break;
                case HasCustomAttributeTag.File: tokenType = MetadataTokenType.File; break;
                case HasCustomAttributeTag.ExportedType: tokenType = MetadataTokenType.ExportedType; break;
                case HasCustomAttributeTag.ManifestResource: tokenType = MetadataTokenType.ManifestResource; break;
                case HasCustomAttributeTag.GenericParameter: tokenType = MetadataTokenType.GenericPar; break;
                default: throw new BadImageFormatException();
            }
            return new MetadataToken(tokenType, hasCustomAttribute >> HasCustomAttributeTag.NumberOfBits);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static uint ConvertToTag(MetadataToken token) {
            uint rowId = (uint)token.Rid;
            switch (token.TokenType) {
                case MetadataTokenType.MethodDef:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.Method;
                case MetadataTokenType.FieldDef:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.Field;
                case MetadataTokenType.TypeRef:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.TypeRef;
                case MetadataTokenType.TypeDef:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.TypeDef;
                case MetadataTokenType.ParamDef:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.Param;
                case MetadataTokenType.InterfaceImpl:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.InterfaceImpl;
                case MetadataTokenType.MemberRef:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.MemberRef;
                case MetadataTokenType.Module:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.Module;
                case MetadataTokenType.Permission:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.DeclSecurity;
                case MetadataTokenType.Property:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.Property;
                case MetadataTokenType.Event:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.Event;
                case MetadataTokenType.Signature:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.StandAloneSig;
                case MetadataTokenType.ModuleRef:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.ModuleRef;
                case MetadataTokenType.TypeSpec:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.TypeSpec;
                case MetadataTokenType.Assembly:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.Assembly;
                case MetadataTokenType.AssemblyRef:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.AssemblyRef;
                case MetadataTokenType.File:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.File;
                case MetadataTokenType.ExportedType:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.ExportedType;
                case MetadataTokenType.ManifestResource:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.ManifestResource;
                case MetadataTokenType.GenericPar:
                    return rowId << HasCustomAttributeTag.NumberOfBits | HasCustomAttributeTag.GenericParameter;
            }
            return 0;
        }
    }

    internal static class HasFieldMarshalTag {
        internal const int NumberOfBits = 1;
        internal const int LargeRowSize = 0x00000001 << (16 - NumberOfBits);
        internal const uint Field = 0x00000000;
        internal const uint Param = 0x00000001;
        internal const uint TagMask = 0x00000001;
        internal const TableMask TablesReferenced = TableMask.Field | TableMask.Param;

        internal static MetadataToken ConvertToToken(uint hasFieldMarshal) {
            MetadataTokenType tokenType = (hasFieldMarshal & TagMask) == Field ? MetadataTokenType.FieldDef : MetadataTokenType.ParamDef;
            return new MetadataToken(tokenType, hasFieldMarshal >> NumberOfBits);
        }

        internal static uint ConvertToTag(MetadataToken token) {
            uint rowId = (uint)token.Rid;
            switch (token.TokenType) {
                case MetadataTokenType.FieldDef: return rowId << NumberOfBits | Field;
                case MetadataTokenType.ParamDef: return rowId << NumberOfBits | Param;
            }
            return 0;
        }
    }

    internal static class HasDeclSecurityTag {
        internal const int NumberOfBits = 2;
        internal const int LargeRowSize = 0x00000001 << (16 - NumberOfBits);
        internal const uint TypeDef = 0x00000000;
        internal const uint Method = 0x00000001;
        internal const uint Assembly = 0x00000002;
        internal const uint TagMask = 0x00000003;

        internal const TableMask TablesReferenced = TableMask.TypeDef | TableMask.Method | TableMask.Assembly;

        internal static MetadataToken ConvertToToken(uint hasDeclSecurity) {
            MetadataTokenType tokenType;
            switch (hasDeclSecurity & TagMask) {
                case TypeDef: tokenType = MetadataTokenType.TypeDef; break;
                case Method: tokenType = MetadataTokenType.MethodDef; break;
                case Assembly: tokenType = MetadataTokenType.Assembly; break;
                default: throw new BadImageFormatException();
            }
            return new MetadataToken(tokenType, hasDeclSecurity >> NumberOfBits);
        }

        internal static uint ConvertToTag(MetadataToken token) {
            uint rowId = (uint)token.Rid;
            switch (token.TokenType) {
                case MetadataTokenType.TypeDef:  return rowId << NumberOfBits | TypeDef;
                case MetadataTokenType.MethodDef:return rowId << NumberOfBits | Method;
                case MetadataTokenType.Assembly: return rowId << NumberOfBits | Assembly;
            }
            return 0;
        }
    }

    internal static class MemberRefParentTag {
        internal const int NumberOfBits = 3;
        internal const int LargeRowSize = 0x00000001 << (16 - NumberOfBits);
        internal const uint TypeDef = 0x00000000;
        internal const uint TypeRef = 0x00000001;
        internal const uint ModuleRef = 0x00000002;
        internal const uint Method = 0x00000003;
        internal const uint TypeSpec = 0x00000004;
        internal const uint TagMask = 0x00000007;

        internal const TableMask TablesReferenced = TableMask.TypeDef | TableMask.TypeRef | TableMask.ModuleRef | TableMask.Method | TableMask.TypeSpec;

        internal static MetadataToken ConvertToToken(uint memberRef) {
            MetadataTokenType tokenType;
            switch (memberRef & TagMask) {
                case TypeDef: tokenType = MetadataTokenType.TypeDef; break;
                case TypeRef: tokenType = MetadataTokenType.TypeRef; break;
                case ModuleRef: tokenType = MetadataTokenType.ModuleRef; break;
                case Method: tokenType = MetadataTokenType.MethodDef; break;
                case TypeSpec: tokenType = MetadataTokenType.TypeSpec; break;
                default: throw new BadImageFormatException();
            }
            return new MetadataToken(tokenType, memberRef >> NumberOfBits);
        }
    }

    internal static class HasSemanticsTag {
        internal const int NumberOfBits = 1;
        internal const int LargeRowSize = 0x00000001 << (16 - HasSemanticsTag.NumberOfBits);
        internal const uint Event = 0x00000000;
        internal const uint Property = 0x00000001;
        internal const uint TagMask = 0x00000001;
        internal const TableMask TablesReferenced = TableMask.Event | TableMask.Property;

        internal static MetadataToken ConvertToToken(uint hasSemantic) {
            MetadataTokenType tokenType = (hasSemantic & TagMask) == Event ? MetadataTokenType.Event : MetadataTokenType.Property;
            return new MetadataToken(tokenType, hasSemantic >> NumberOfBits);
        }

        internal static uint ConvertEventRowIdToTag(int eventRowId) {
            return (uint)eventRowId << NumberOfBits | Event;
        }

        internal static uint ConvertPropertyRowIdToTag(int propertyRowId) {
            return (uint)propertyRowId << NumberOfBits | Property;
        }
    }

    internal static class MethodDefOrRefTag {
        internal const int NumberOfBits = 1;
        internal const int LargeRowSize = 0x00000001 << (16 - NumberOfBits);
        internal const uint TagMask = 0x00000001;

        internal const TableMask TablesReferenced = TableMask.Method | TableMask.MemberRef;

        internal static MetadataToken ConvertToToken(uint methodDefOrRef) {
            var tokenType = (methodDefOrRef & TagMask) == 0 ? MetadataTokenType.MethodDef : MetadataTokenType.MemberRef;
            return new MetadataToken(tokenType, methodDefOrRef >> NumberOfBits);
        }
    }

    internal static class MemberForwardedTag {
        internal const int NumberOfBits = 1;
        internal const int LargeRowSize = 0x00000001 << (16 - NumberOfBits);
        internal const uint Field = 0x00000000;
        internal const uint Method = 0x00000001;
        internal const uint TagMask = 0x00000001;
        internal const TableMask TablesReferenced = TableMask.Field | TableMask.Method;

        internal static MetadataToken ConvertToToken(uint memberForwarded) {
            var tokenType = (memberForwarded & TagMask) == 0 ? MetadataTokenType.FieldDef : MetadataTokenType.MethodDef;
            return new MetadataToken(tokenType, memberForwarded >> NumberOfBits);
        }

        internal static uint ConvertMethodDefRowIdToTag(int methodDefRowId) {
            return (uint)methodDefRowId << MemberForwardedTag.NumberOfBits | MemberForwardedTag.Method;
        }
    }

    internal static class ImplementationTag {
        internal const int NumberOfBits = 2;
        internal const int LargeRowSize = 0x00000001 << (16 - NumberOfBits);
        internal const uint File = 0x00000000;
        internal const uint AssemblyRef = 0x00000001;
        internal const uint ExportedType = 0x00000002;
        internal const uint TagMask = 0x00000003;
        
        internal const TableMask TablesReferenced = TableMask.File | TableMask.AssemblyRef | TableMask.ExportedType;

        internal static MetadataToken ConvertToToken(uint implementation) {
            if (implementation == 0) {
                return default(MetadataToken);
            }

            MetadataTokenType tokenType;
            switch (implementation & TagMask) {
                case File: tokenType = MetadataTokenType.File; break;
                case AssemblyRef: tokenType = MetadataTokenType.AssemblyRef; break;
                case ExportedType: tokenType = MetadataTokenType.ExportedType; break;
                default: throw new BadImageFormatException();
            }
            return new MetadataToken(tokenType, implementation >> NumberOfBits);
        }
    }

    internal static class CustomAttributeTypeTag {
        internal const int NumberOfBits = 3;
        internal const int LargeRowSize = 0x00000001 << (16 - CustomAttributeTypeTag.NumberOfBits);
        internal const uint Method = 0x00000002;
        internal const uint MemberRef = 0x00000003;
        internal const uint TagMask = 0x0000007;
        internal const TableMask TablesReferenced = TableMask.Method | TableMask.MemberRef;

        internal static MetadataToken ConvertToToken(uint customAttributeType) {
            MetadataTokenType tokenType;
            switch (customAttributeType & TagMask) {
                case Method: tokenType = MetadataTokenType.MethodDef; break;
                case MemberRef: tokenType = MetadataTokenType.MemberRef; break;
                default: throw new BadImageFormatException();
            }
            return new MetadataToken(tokenType, customAttributeType >> NumberOfBits);
        }
    }

    internal static class ResolutionScopeTag {
        internal const int NumberOfBits = 2;
        internal const int LargeRowSize = 0x00000001 << (16 - NumberOfBits);
        internal const uint Module = 0x00000000;
        internal const uint ModuleRef = 0x00000001;
        internal const uint AssemblyRef = 0x00000002;
        internal const uint TypeRef = 0x00000003;
        internal const uint TagMask = 0x00000003;
        internal const TableMask TablesReferenced = TableMask.Module | TableMask.ModuleRef | TableMask.AssemblyRef | TableMask.TypeRef;

        internal static MetadataToken ConvertToToken(uint resolutionScope) {
            MetadataTokenType tokenType;
            switch (resolutionScope & TagMask) {
                case Module: tokenType = MetadataTokenType.Module; break;
                case ModuleRef: tokenType = MetadataTokenType.ModuleRef; break;
                case AssemblyRef: tokenType = MetadataTokenType.AssemblyRef; break;
                case TypeRef: tokenType = MetadataTokenType.TypeRef; break;
                default: throw new BadImageFormatException();
            }
            return new MetadataToken(tokenType, resolutionScope >> NumberOfBits);
        }
    }

    internal static class TypeOrMethodDefTag {
        internal const int NumberOfBits = 1;
        internal const int LargeRowSize = 0x00000001 << (16 - TypeOrMethodDefTag.NumberOfBits);
        internal const uint TypeDef = 0x00000000;
        internal const uint MethodDef = 0x00000001;
        internal const uint TagMask = 0x0000001;

        internal const TableMask TablesReferenced = TableMask.TypeDef | TableMask.Method;

        internal static MetadataToken ConvertToToken(uint typeOrMethodDef) {
            var tokenType = (typeOrMethodDef & TagMask) == TypeDef ? MetadataTokenType.TypeDef : MetadataTokenType.MethodDef;
            return new MetadataToken(tokenType, typeOrMethodDef >> NumberOfBits);
        }

        internal static uint ConvertTypeDefRowIdToTag(int typeDefRowId) {
            return (uint)typeDefRowId << NumberOfBits | TypeDef;
        }

        internal static uint ConvertMethodDefRowIdToTag(int methodDefRowId) {
            return (uint)methodDefRowId << NumberOfBits | MethodDef;
        }
    }

    #endregion 
}
