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
#if !SILVERLIGHT

using System;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace Microsoft.Scripting.Metadata {
    internal sealed class MetadataImport {
        private readonly MemoryBlock _image;
        private const int TableCount = (int)MetadataRecordType.GenericParamConstraint + 1;

        #region Constructors and Utils

        internal MetadataImport(MemoryBlock image) {
            _image = image;

            try {
                ReadPEFileLevelData();
                ReadCORModuleLevelData();
                ReadMetadataLevelData();
            } catch (OverflowException) {
                throw new BadImageFormatException();
            } catch (ArgumentOutOfRangeException) {
                throw new BadImageFormatException();
            }
        }

        #endregion Constructors and Utils

        #region Fields and Properties [PEFile]

        private int _numberOfSections;
        private OptionalHeaderDirectoryEntries _optionalHeaderDirectoryEntries;
        private SectionHeader[] _sectionHeaders;
        private MemoryBlock _win32ResourceBlock;

        #endregion Fields and Properties [PEFile]

        #region Methods [PEFile]

        private void ReadOptionalHeaderDirectoryEntries(MemoryReader memReader) {
            // ExportTableDirectory
            // ImportTableDirectory
            memReader.SeekRelative(2 * 2 * sizeof(uint));

            _optionalHeaderDirectoryEntries.ResourceTableDirectory.RelativeVirtualAddress = memReader.ReadUInt32();
            _optionalHeaderDirectoryEntries.ResourceTableDirectory.Size = memReader.ReadUInt32();

            // ExceptionTableDirectory
            // CertificateTableDirectory
            // BaseRelocationTableDirectory
            // DebugTableDirectory
            // CopyrightTableDirectory
            // GlobalPointerTableDirectory
            // ThreadLocalStorageTableDirectory
            // LoadConfigTableDirectory
            // BoundImportTableDirectory
            // ImportAddressTableDirectory
            // DelayImportTableDirectory
            memReader.SeekRelative(11 * 2 * sizeof(uint));

            _optionalHeaderDirectoryEntries.COR20HeaderTableDirectory.RelativeVirtualAddress = memReader.ReadUInt32();
            _optionalHeaderDirectoryEntries.COR20HeaderTableDirectory.Size = memReader.ReadUInt32();

            // ReservedDirectory
            memReader.SeekRelative(1 * 2 * sizeof(uint));
        }

        private void ReadSectionHeaders(MemoryReader memReader) {
            int numberOfSections = _numberOfSections;
            if (memReader.RemainingBytes < numberOfSections * PEFileConstants.SizeofSectionHeader) {
                throw new BadImageFormatException();
            }

            _sectionHeaders = new SectionHeader[numberOfSections];
            SectionHeader[] sectionHeaderArray = _sectionHeaders;
            for (int i = 0; i < numberOfSections; ++i) {
                memReader.SeekRelative(PEFileConstants.SizeofSectionName);
                sectionHeaderArray[i].VirtualSize = memReader.ReadUInt32();
                sectionHeaderArray[i].VirtualAddress = memReader.ReadUInt32();
                sectionHeaderArray[i].SizeOfRawData = memReader.ReadUInt32();
                sectionHeaderArray[i].OffsetToRawData = memReader.ReadUInt32();

                //sectionHeaderArray[i].RVAToRelocations = memReader.ReadInt32();
                //sectionHeaderArray[i].PointerToLineNumbers = memReader.ReadInt32();
                //sectionHeaderArray[i].NumberOfRelocations = memReader.ReadUInt16();
                //sectionHeaderArray[i].NumberOfLineNumbers = memReader.ReadUInt16();
                //sectionHeaderArray[i].SectionCharacteristics = (SectionCharacteristics)memReader.ReadUInt32();
                memReader.SeekRelative(2 * sizeof(int) + 2 * sizeof(ushort) + sizeof(uint));
            }
        }

        private void ReadPEFileLevelData() {
            if (_image.Length < PEFileConstants.BasicPEHeaderSize) {
                throw new BadImageFormatException();
            }

            MemoryReader memReader = new MemoryReader(_image);

            //  Look for DOS Signature "MZ"
            ushort dosSig = _image.ReadUInt16(0);
            if (dosSig != PEFileConstants.DosSignature) {
                throw new BadImageFormatException();
            }

            //  Skip the DOS Header
            int ntHeaderOffset = _image.ReadInt32(PEFileConstants.PESignatureOffsetLocation);
            memReader.Seek(ntHeaderOffset);

            //  Look for PESignature "PE\0\0"
            uint NTSignature = memReader.ReadUInt32();
            if (NTSignature != PEFileConstants.PESignature) {
                throw new BadImageFormatException();
            }

            //  Read the COFF Header
            _numberOfSections = memReader.Block.ReadInt16(memReader.Position + sizeof(ushort));
            memReader.SeekRelative(PEFileConstants.SizeofCOFFFileHeader);

            //  Read the magic to determine if its PE or PE+
            switch ((PEMagic)memReader.ReadUInt16()) {
                case PEMagic.PEMagic32:
                    memReader.SeekRelative(PEFileConstants.SizeofOptionalHeaderStandardFields32 - sizeof(ushort));
                    memReader.SeekRelative(PEFileConstants.SizeofOptionalHeaderNTAdditionalFields32);
                    break;

                case PEMagic.PEMagic64:
                    memReader.SeekRelative(PEFileConstants.SizeofOptionalHeaderStandardFields64 - sizeof(ushort));
                    memReader.SeekRelative(PEFileConstants.SizeofOptionalHeaderNTAdditionalFields64);
                    break;

                default:
                    throw new BadImageFormatException();
            }

            ReadOptionalHeaderDirectoryEntries(memReader);
            ReadSectionHeaders(memReader);

            _win32ResourceBlock = DirectoryToMemoryBlock(_optionalHeaderDirectoryEntries.ResourceTableDirectory);
        }

        internal MemoryBlock RvaToMemoryBlock(uint rva, uint size) {
            foreach (SectionHeader section in _sectionHeaders) {
                uint relativeOffset;
                if (rva >= section.VirtualAddress && (relativeOffset = rva - section.VirtualAddress) < section.VirtualSize) {
                    uint maxSize;
                    if (size > (maxSize = section.VirtualSize - relativeOffset)) {
                        throw new BadImageFormatException();
                    }

                    return _image.GetRange(
                        unchecked((int)(section.OffsetToRawData + relativeOffset)),
                        unchecked((int)(size == 0 ? maxSize : size))
                    );
                }
            }
            throw new BadImageFormatException();
        }

        private MemoryBlock DirectoryToMemoryBlock(DirectoryEntry directory) {
            if (directory.RelativeVirtualAddress == 0 || directory.Size == 0) {
                return null;
            }
            return RvaToMemoryBlock(directory.RelativeVirtualAddress, directory.Size);
        }

        #endregion Methods [PEFile]

        #region Fields and Properties [CORModule]

        private COR20Header COR20Header;
        private MetadataHeader MetadataHeader;
        private StorageHeader StorageHeader;
        private StreamHeader[] StreamHeaders;

        private MemoryBlock StringStream;
        private MemoryBlock BlobStream;
        private MemoryBlock GUIDStream;
        private MemoryBlock UserStringStream;

        private MetadataStreamKind MetadataStreamKind;
        private MemoryBlock MetadataTableStream;
        private MemoryBlock ResourceMemoryBlock;
        private MemoryBlock StrongNameSignatureBlock;

        #endregion Fields and Properties [CORModule]

        #region Methods [CORModule]

        private void ReadCOR20Header() {
            MemoryBlock memBlock = DirectoryToMemoryBlock(_optionalHeaderDirectoryEntries.COR20HeaderTableDirectory);
            if (memBlock == null || memBlock.Length < _optionalHeaderDirectoryEntries.COR20HeaderTableDirectory.Size) {
                throw new BadImageFormatException();
            }

            MemoryReader memReader = new MemoryReader(memBlock);
            // CountBytes = memReader.ReadInt32();
            // MajorRuntimeVersion = memReader.ReadUInt16();
            // MinorRuntimeVersion = memReader.ReadUInt16();
            memReader.SeekRelative(sizeof(int) + 2 * sizeof(short));

            COR20Header.MetaDataDirectory.RelativeVirtualAddress = memReader.ReadUInt32();
            COR20Header.MetaDataDirectory.Size = memReader.ReadUInt32();
            
            // COR20Header.COR20Flags = (COR20Flags)memReader.ReadUInt32();
            // COR20Header.EntryPointTokenOrRVA = memReader.ReadUInt32();
            memReader.SeekRelative(2 * sizeof(uint));

            COR20Header.ResourcesDirectory.RelativeVirtualAddress = memReader.ReadUInt32();
            COR20Header.ResourcesDirectory.Size = memReader.ReadUInt32();
            COR20Header.StrongNameSignatureDirectory.RelativeVirtualAddress = memReader.ReadUInt32();
            COR20Header.StrongNameSignatureDirectory.Size = memReader.ReadUInt32();

            // CodeManagerTableDirectory
            // VtableFixupsDirectory
            // ExportAddressTableJumpsDirectory
            // ManagedNativeHeaderDirectory
            memReader.SeekRelative(4 * 2 * sizeof(uint));
        }

        private void ReadMetadataHeader(MemoryReader memReader) {
            MetadataHeader.Signature = memReader.ReadUInt32();
            if (MetadataHeader.Signature != COR20Constants.COR20MetadataSignature) {
                throw new BadImageFormatException();
            }

            MetadataHeader.MajorVersion = memReader.ReadUInt16();
            MetadataHeader.MinorVersion = memReader.ReadUInt16();
            MetadataHeader.ExtraData = memReader.ReadUInt32();
            MetadataHeader.VersionStringSize = memReader.ReadInt32();
            if (memReader.RemainingBytes < MetadataHeader.VersionStringSize) {
                throw new BadImageFormatException();
            }

            int bytesRead;
            MetadataHeader.VersionString = memReader.Block.ReadUtf8(memReader.Position, out bytesRead);
            memReader.SeekRelative(MetadataHeader.VersionStringSize);
        }

        private void ReadStorageHeader(MemoryReader memReader) {
            StorageHeader.Flags = memReader.ReadUInt16();
            StorageHeader.NumberOfStreams = memReader.ReadInt16();
        }

        private void ReadStreamHeaders(MemoryReader memReader) {
            int numberOfStreams = StorageHeader.NumberOfStreams;
            StreamHeaders = new StreamHeader[numberOfStreams];
            StreamHeader[] streamHeaders = StreamHeaders;
            for (int i = 0; i < numberOfStreams; ++i) {
                if (memReader.RemainingBytes < COR20Constants.MinimumSizeofStreamHeader) {
	                throw new BadImageFormatException();
                }

                streamHeaders[i].Offset = memReader.ReadUInt32();
                streamHeaders[i].Size = memReader.ReadUInt32();
                //  Review: Oh well there is no way i can test if we will read correctly. However we can check it after reading and aligning...
                streamHeaders[i].Name = memReader.ReadAscii();
                memReader.Align(4);
                if (memReader.RemainingBytes < 0) {
	                throw new BadImageFormatException();
                }
            }
        }

        private void ProcessAndCacheStreams(MemoryBlock metadataRoot) {
            foreach (StreamHeader streamHeader in StreamHeaders) {
                if ((long)streamHeader.Offset + streamHeader.Size > metadataRoot.Length) {
                    throw new BadImageFormatException();
                }
                MemoryBlock block = metadataRoot.GetRange((int)streamHeader.Offset, (int)streamHeader.Size);

                switch (streamHeader.Name) {
                    case COR20Constants.StringStreamName:
                        StringStream = block;
                        break;

                    case COR20Constants.BlobStreamName:
                        BlobStream = block;
                        break;

                    case COR20Constants.GUIDStreamName:
                        GUIDStream = block;
                        break;

                    case COR20Constants.UserStringStreamName:
                        UserStringStream = block;
                        break;

                    case COR20Constants.CompressedMetadataTableStreamName:
                        MetadataStreamKind = MetadataStreamKind.Compressed;
                        MetadataTableStream = block;
                        break;

                    case COR20Constants.UncompressedMetadataTableStreamName:
                        MetadataStreamKind = MetadataStreamKind.UnCompressed;
                        MetadataTableStream = block;
                        break;

                    default:
		                throw new BadImageFormatException();
                }
            }
        }

        private void ReadCORModuleLevelData() {
            ReadCOR20Header();

            MemoryBlock metadataRoot = DirectoryToMemoryBlock(COR20Header.MetaDataDirectory);
            if (metadataRoot.Length < COR20Header.MetaDataDirectory.Size) {
                throw new BadImageFormatException();
            }

            MemoryReader memReader = new MemoryReader(metadataRoot);

            ReadMetadataHeader(memReader);
            ReadStorageHeader(memReader);
            ReadStreamHeaders(memReader);
            ProcessAndCacheStreams(metadataRoot);

            ResourceMemoryBlock = DirectoryToMemoryBlock(COR20Header.ResourcesDirectory);
            StrongNameSignatureBlock = DirectoryToMemoryBlock(COR20Header.StrongNameSignatureDirectory);
        }

        #endregion Methods [CORModule]

        #region Fields and Properties [Metadata]

        private MetadataTableHeader _metadataTableHeader;
        private int[] _tableRowCounts;

        public ModuleTable ModuleTable;
        public TypeRefTable TypeRefTable;
        public TypeDefTable TypeDefTable;
        public FieldPtrTable FieldPtrTable;
        public FieldTable FieldTable;
        public MethodPtrTable MethodPtrTable;
        public MethodTable MethodTable;
        public ParamPtrTable ParamPtrTable;
        public ParamTable ParamTable;
        public InterfaceImplTable InterfaceImplTable;
        public MemberRefTable MemberRefTable;
        public ConstantTable ConstantTable;
        public CustomAttributeTable CustomAttributeTable;
        public FieldMarshalTable FieldMarshalTable;
        public DeclSecurityTable DeclSecurityTable;
        public ClassLayoutTable ClassLayoutTable;
        public FieldLayoutTable FieldLayoutTable;
        public StandAloneSigTable StandAloneSigTable;
        public EventMapTable EventMapTable;
        public EventPtrTable EventPtrTable;
        public EventTable EventTable;
        public PropertyMapTable PropertyMapTable;
        public PropertyPtrTable PropertyPtrTable;
        public PropertyTable PropertyTable;
        public MethodSemanticsTable MethodSemanticsTable;
        public MethodImplTable MethodImplTable;
        public ModuleRefTable ModuleRefTable;
        public TypeSpecTable TypeSpecTable;
        public ImplMapTable ImplMapTable;
        public FieldRVATable FieldRVATable;
        public EnCLogTable EnCLogTable;
        public EnCMapTable EnCMapTable;
        public AssemblyTable AssemblyTable;
        public AssemblyProcessorTable AssemblyProcessorTable;
        public AssemblyOSTable AssemblyOSTable;
        public AssemblyRefTable AssemblyRefTable;
        public AssemblyRefProcessorTable AssemblyRefProcessorTable;
        public AssemblyRefOSTable AssemblyRefOSTable;
        public FileTable FileTable;
        public ExportedTypeTable ExportedTypeTable;
        public ManifestResourceTable ManifestResourceTable;
        public NestedClassTable NestedClassTable;
        public GenericParamTable GenericParamTable;
        public MethodSpecTable MethodSpecTable;
        public GenericParamConstraintTable GenericParamConstraintTable;

        internal bool IsManifestModule {
            get { return AssemblyTable.NumberOfRows == 1; }
        }

        internal bool UseFieldPtrTable {
            get { return FieldPtrTable.NumberOfRows > 0; }
        }

        internal bool UseMethodPtrTable {
            get { return MethodPtrTable.NumberOfRows > 0; }
        }

        internal bool UseParamPtrTable {
            get { return ParamPtrTable.NumberOfRows > 0; }
        }

        internal bool UseEventPtrTable { 
            get { return EventPtrTable.NumberOfRows > 0; }
        }

        internal bool UsePropertyPtrTable {
            get { return PropertyPtrTable.NumberOfRows > 0; }
        }

        #endregion Fields and Properties [Metadata]

        #region Methods [Metadata]

        private void ReadMetadataTableInformation(MemoryReader memReader) {
            if (memReader.RemainingBytes < MetadataStreamConstants.SizeOfMetadataTableHeader) {
                throw new BadImageFormatException();
            }

            _metadataTableHeader.Reserved = memReader.ReadUInt32();
            _metadataTableHeader.MajorVersion = memReader.ReadByte();
            _metadataTableHeader.MinorVersion = memReader.ReadByte();
            _metadataTableHeader.HeapSizeFlags = (HeapSizeFlag)memReader.ReadByte();
            _metadataTableHeader.RowId = memReader.ReadByte();
            _metadataTableHeader.ValidTables = (TableMask)memReader.ReadUInt64();
            _metadataTableHeader.SortedTables = (TableMask)memReader.ReadUInt64();
            ulong presentTables = (ulong)_metadataTableHeader.ValidTables;
            ulong validTablesForVersion = 0;

            int version = _metadataTableHeader.MajorVersion << 16 | _metadataTableHeader.MinorVersion;
            switch (version) {
                case 0x00010000:
                    validTablesForVersion = (ulong)TableMask.V1_0_TablesMask;
                    break;

                case 0x00010001:
                    validTablesForVersion = (ulong)TableMask.V1_1_TablesMask;
                    break;

                case 0x00020000:
                    validTablesForVersion = (ulong)TableMask.V2_0_TablesMask;
                    break;

                default:
                    throw new BadImageFormatException();
            }

            if ((presentTables & ~validTablesForVersion) != 0) {
                throw new BadImageFormatException();
            }

            if (MetadataStreamKind == MetadataStreamKind.Compressed && (presentTables & (ulong)TableMask.CompressedStreamNotAllowedMask) != 0) {
                throw new BadImageFormatException();
            }

            ulong requiredSortedTables = presentTables & validTablesForVersion & (ulong)TableMask.SortedTablesMask;
            if ((requiredSortedTables & (ulong)_metadataTableHeader.SortedTables) != requiredSortedTables) {
                throw new BadImageFormatException();
            }

            int numberOfTables = _metadataTableHeader.GetNumberOfTablesPresent();
            if (memReader.RemainingBytes < numberOfTables * sizeof(Int32)) {
                throw new BadImageFormatException();
            }

            int[] metadataTableRowCount = _metadataTableHeader.CompressedMetadataTableRowCount = new int[numberOfTables];
            for (int i = 0; i < numberOfTables; i++) {
                uint rowCount = memReader.ReadUInt32();
                if (rowCount > 0x00ffffff) {
                    throw new BadImageFormatException();
                }
                metadataTableRowCount[i] = (int)rowCount;
            }
        }

        private static int ComputeCodedTokenSize(int largeRowSize, int[] rowCountArray, TableMask tablesReferenced) {
            bool isAllReferencedTablesSmall = true;
            ulong tablesReferencedMask = (ulong)tablesReferenced;
            for (int tableIndex = 0; tableIndex < TableCount; tableIndex++) {
                if ((tablesReferencedMask & 0x0000000000000001UL) != 0) {
                    isAllReferencedTablesSmall &= (rowCountArray[tableIndex] < largeRowSize);
                }
                tablesReferencedMask >>= 1;
            }
            return isAllReferencedTablesSmall ? 2 : 4;
        }

        private void ProcessAndCacheMetadataTableBlocks(MemoryBlock metadataTablesMemoryBlock) {

            int[] rowCountArray = _tableRowCounts = new int[TableCount];
            int[] rowRefSizeArray = new int[TableCount];
            int[] rowCountCompressedArray = _metadataTableHeader.CompressedMetadataTableRowCount;
            ulong validTables = (ulong)_metadataTableHeader.ValidTables;

            //  Fill in the row count and table reference sizes...
            for (int tableIndex = 0, arrayIndex = 0; tableIndex < rowRefSizeArray.Length; tableIndex++) {
                if ((validTables & 0x0000000000000001UL) != 0) {
                    int rowCount = rowCountCompressedArray[arrayIndex++];
                    rowCountArray[tableIndex] = rowCount;
                    rowRefSizeArray[tableIndex] = rowCount < MetadataStreamConstants.LargeTableRowCount ? 2 : 4;
                } else {
                    rowRefSizeArray[tableIndex] = 2;
                }
                validTables >>= 1;
            }

            //  Compute ref sizes for tables that can have pointer tables for it
            int fieldRefSize = rowRefSizeArray[FieldPtrTable.TableIndex] > 2 ? 4 : rowRefSizeArray[FieldPtrTable.TableIndex];
            int methodRefSize = rowRefSizeArray[MethodPtrTable.TableIndex] > 2 ? 4 : rowRefSizeArray[MethodPtrTable.TableIndex];
            int paramRefSize = rowRefSizeArray[ParamPtrTable.TableIndex] > 2 ? 4 : rowRefSizeArray[ParamPtrTable.TableIndex];
            int eventRefSize = rowRefSizeArray[EventPtrTable.TableIndex] > 2 ? 4 : rowRefSizeArray[EventPtrTable.TableIndex];
            int propertyRefSize = rowRefSizeArray[PropertyPtrTable.TableIndex] > 2 ? 4 : rowRefSizeArray[PropertyPtrTable.TableIndex];
            //  Compute the coded token ref sizes
            int typeDefOrRefRefSize = ComputeCodedTokenSize(TypeDefOrRefTag.LargeRowSize, rowCountArray, TypeDefOrRefTag.TablesReferenced);
            int hasConstantRefSize = ComputeCodedTokenSize(HasConstantTag.LargeRowSize, rowCountArray, HasConstantTag.TablesReferenced);
            int hasCustomAttributeRefSize = ComputeCodedTokenSize(HasCustomAttributeTag.LargeRowSize, rowCountArray, HasCustomAttributeTag.TablesReferenced);
            int hasFieldMarshalRefSize = ComputeCodedTokenSize(HasFieldMarshalTag.LargeRowSize, rowCountArray, HasFieldMarshalTag.TablesReferenced);
            int hasDeclSecurityRefSize = ComputeCodedTokenSize(HasDeclSecurityTag.LargeRowSize, rowCountArray, HasDeclSecurityTag.TablesReferenced);
            int memberRefParentRefSize = ComputeCodedTokenSize(MemberRefParentTag.LargeRowSize, rowCountArray, MemberRefParentTag.TablesReferenced);
            int hasSemanticsRefSize = ComputeCodedTokenSize(HasSemanticsTag.LargeRowSize, rowCountArray, HasSemanticsTag.TablesReferenced);
            int methodDefOrRefRefSize = ComputeCodedTokenSize(MethodDefOrRefTag.LargeRowSize, rowCountArray, MethodDefOrRefTag.TablesReferenced);
            int memberForwardedRefSize = ComputeCodedTokenSize(MemberForwardedTag.LargeRowSize, rowCountArray, MemberForwardedTag.TablesReferenced);
            int implementationRefSize = ComputeCodedTokenSize(ImplementationTag.LargeRowSize, rowCountArray, ImplementationTag.TablesReferenced);
            int customAttributeTypeRefSize = ComputeCodedTokenSize(CustomAttributeTypeTag.LargeRowSize, rowCountArray, CustomAttributeTypeTag.TablesReferenced);
            int resolutionScopeRefSize = ComputeCodedTokenSize(ResolutionScopeTag.LargeRowSize, rowCountArray, ResolutionScopeTag.TablesReferenced);
            int typeOrMethodDefRefSize = ComputeCodedTokenSize(TypeOrMethodDefTag.LargeRowSize, rowCountArray, TypeOrMethodDefTag.TablesReferenced);
            //  Compute HeapRef Sizes
            int stringHeapRefSize = (_metadataTableHeader.HeapSizeFlags & HeapSizeFlag.StringHeapLarge) == HeapSizeFlag.StringHeapLarge ? 4 : 2;
            int guidHeapRefSize = (_metadataTableHeader.HeapSizeFlags & HeapSizeFlag.GUIDHeapLarge) == HeapSizeFlag.GUIDHeapLarge ? 4 : 2;
            int blobHeapRefSize = (_metadataTableHeader.HeapSizeFlags & HeapSizeFlag.BlobHeapLarge) == HeapSizeFlag.BlobHeapLarge ? 4 : 2;
            
            //  Populate the Table blocks
            int totalRequiredSize = 0;
            int currentTableSize = 0;
            int currentPointer = 0;

            ModuleTable = new ModuleTable(rowCountArray[ModuleTable.TableIndex], stringHeapRefSize, guidHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = ModuleTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            TypeRefTable = new TypeRefTable(rowCountArray[TypeRefTable.TableIndex], resolutionScopeRefSize, stringHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = TypeRefTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            TypeDefTable = new TypeDefTable(rowCountArray[TypeDefTable.TableIndex], fieldRefSize, methodRefSize, typeDefOrRefRefSize, stringHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = TypeDefTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            FieldPtrTable = new FieldPtrTable(rowCountArray[FieldPtrTable.TableIndex], rowRefSizeArray[FieldPtrTable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = FieldPtrTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            FieldTable = new FieldTable(rowCountArray[FieldTable.TableIndex], stringHeapRefSize, blobHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = FieldTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            MethodPtrTable = new MethodPtrTable(rowCountArray[MethodPtrTable.TableIndex], rowRefSizeArray[MethodPtrTable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = MethodPtrTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            MethodTable = new MethodTable(rowCountArray[MethodTable.TableIndex], paramRefSize, stringHeapRefSize, blobHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = MethodTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            ParamPtrTable = new ParamPtrTable(rowCountArray[ParamPtrTable.TableIndex], rowRefSizeArray[ParamPtrTable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = ParamPtrTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            ParamTable = new ParamTable(rowCountArray[ParamTable.TableIndex], stringHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = ParamTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            InterfaceImplTable = new InterfaceImplTable(rowCountArray[InterfaceImplTable.TableIndex], rowRefSizeArray[InterfaceImplTable.TableIndex], typeDefOrRefRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = InterfaceImplTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            MemberRefTable = new MemberRefTable(rowCountArray[MemberRefTable.TableIndex], memberRefParentRefSize, stringHeapRefSize, blobHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = MemberRefTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            ConstantTable = new ConstantTable(rowCountArray[ConstantTable.TableIndex], hasConstantRefSize, blobHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = ConstantTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            CustomAttributeTable = new CustomAttributeTable(rowCountArray[CustomAttributeTable.TableIndex], hasCustomAttributeRefSize, customAttributeTypeRefSize, blobHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = CustomAttributeTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            FieldMarshalTable = new FieldMarshalTable(rowCountArray[FieldMarshalTable.TableIndex], hasFieldMarshalRefSize, blobHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = FieldMarshalTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            DeclSecurityTable = new DeclSecurityTable(rowCountArray[DeclSecurityTable.TableIndex], hasDeclSecurityRefSize, blobHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = DeclSecurityTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            ClassLayoutTable = new ClassLayoutTable(rowCountArray[ClassLayoutTable.TableIndex], rowRefSizeArray[ClassLayoutTable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = ClassLayoutTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            FieldLayoutTable = new FieldLayoutTable(rowCountArray[FieldLayoutTable.TableIndex], rowRefSizeArray[FieldLayoutTable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = FieldLayoutTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            StandAloneSigTable = new StandAloneSigTable(rowCountArray[StandAloneSigTable.TableIndex], blobHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = StandAloneSigTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            EventMapTable = new EventMapTable(rowCountArray[EventMapTable.TableIndex], rowRefSizeArray[EventMapTable.TableIndex], eventRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = EventMapTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            EventPtrTable = new EventPtrTable(rowCountArray[EventPtrTable.TableIndex], rowRefSizeArray[EventPtrTable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = EventPtrTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            EventTable = new EventTable(rowCountArray[EventTable.TableIndex], typeDefOrRefRefSize, stringHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = EventTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            PropertyMapTable = new PropertyMapTable(rowCountArray[PropertyMapTable.TableIndex], rowRefSizeArray[PropertyMapTable.TableIndex], propertyRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = PropertyMapTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            PropertyPtrTable = new PropertyPtrTable(rowCountArray[PropertyPtrTable.TableIndex], rowRefSizeArray[PropertyPtrTable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = PropertyPtrTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            PropertyTable = new PropertyTable(rowCountArray[PropertyTable.TableIndex], stringHeapRefSize, blobHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = PropertyTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            MethodSemanticsTable = new MethodSemanticsTable(rowCountArray[MethodSemanticsTable.TableIndex], rowRefSizeArray[MethodSemanticsTable.TableIndex], hasSemanticsRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = MethodSemanticsTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            MethodImplTable = new MethodImplTable(rowCountArray[MethodImplTable.TableIndex], rowRefSizeArray[MethodImplTable.TableIndex], methodDefOrRefRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = MethodImplTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            ModuleRefTable = new ModuleRefTable(rowCountArray[ModuleRefTable.TableIndex], stringHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = ModuleRefTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            TypeSpecTable = new TypeSpecTable(rowCountArray[TypeSpecTable.TableIndex], blobHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = TypeSpecTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            ImplMapTable = new ImplMapTable(rowCountArray[ImplMapTable.TableIndex], rowRefSizeArray[ImplMapTable.TableIndex], memberForwardedRefSize, stringHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = ImplMapTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            FieldRVATable = new FieldRVATable(rowCountArray[FieldRVATable.TableIndex], rowRefSizeArray[FieldRVATable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = FieldRVATable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            EnCLogTable = new EnCLogTable(rowCountArray[EnCLogTable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = EnCLogTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            EnCMapTable = new EnCMapTable(rowCountArray[EnCMapTable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = EnCMapTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            AssemblyTable = new AssemblyTable(rowCountArray[AssemblyTable.TableIndex], stringHeapRefSize, blobHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = AssemblyTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            AssemblyProcessorTable = new AssemblyProcessorTable(rowCountArray[AssemblyProcessorTable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = AssemblyProcessorTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            AssemblyOSTable = new AssemblyOSTable(rowCountArray[AssemblyOSTable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = AssemblyOSTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            AssemblyRefTable = new AssemblyRefTable(rowCountArray[AssemblyRefTable.TableIndex], stringHeapRefSize, blobHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = AssemblyRefTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            AssemblyRefProcessorTable = new AssemblyRefProcessorTable(rowCountArray[AssemblyRefProcessorTable.TableIndex], rowRefSizeArray[AssemblyRefProcessorTable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = AssemblyRefProcessorTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            AssemblyRefOSTable = new AssemblyRefOSTable(rowCountArray[AssemblyRefOSTable.TableIndex], rowRefSizeArray[AssemblyRefOSTable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = AssemblyRefOSTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            FileTable = new FileTable(rowCountArray[FileTable.TableIndex], stringHeapRefSize, blobHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = FileTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            ExportedTypeTable = new ExportedTypeTable(rowCountArray[ExportedTypeTable.TableIndex], implementationRefSize, stringHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = ExportedTypeTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            ManifestResourceTable = new ManifestResourceTable(rowCountArray[ManifestResourceTable.TableIndex], implementationRefSize, stringHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = ManifestResourceTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            NestedClassTable = new NestedClassTable(rowCountArray[NestedClassTable.TableIndex], rowRefSizeArray[NestedClassTable.TableIndex], currentPointer, metadataTablesMemoryBlock);
            currentTableSize = NestedClassTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            GenericParamTable = new GenericParamTable(rowCountArray[GenericParamTable.TableIndex], typeOrMethodDefRefSize, stringHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = GenericParamTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            MethodSpecTable = new MethodSpecTable(rowCountArray[MethodSpecTable.TableIndex], methodDefOrRefRefSize, blobHeapRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = MethodSpecTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            GenericParamConstraintTable = new GenericParamConstraintTable(rowCountArray[GenericParamConstraintTable.TableIndex], rowRefSizeArray[GenericParamConstraintTable.TableIndex], typeDefOrRefRefSize, currentPointer, metadataTablesMemoryBlock);
            currentTableSize = GenericParamConstraintTable.Table.Length;
            totalRequiredSize += currentTableSize;
            currentPointer += currentTableSize;
            if (totalRequiredSize > metadataTablesMemoryBlock.Length) {
                throw new BadImageFormatException();
            }
        }

        private void ReadMetadataLevelData() {
            MemoryReader memReader = new MemoryReader(MetadataTableStream);

            ReadMetadataTableInformation(memReader);
            ProcessAndCacheMetadataTableBlocks(memReader.GetRemainingBlock());

            if (ModuleTable.NumberOfRows != 1) {
                throw new BadImageFormatException();
            }
        }

        #endregion

        #region Blobs and User Strings

        internal byte[] GetBlob(uint blob) {
            int size;
            int dataOffset = GetBlobDataOffset(blob, out size);
            var result = new byte[size];
            BlobStream.Read(dataOffset, result);
            return result;
        }

        internal MemoryBlock GetBlobBlock(uint blob) {
            int size;
            int dataOffset = GetBlobDataOffset(blob, out size);
            return BlobStream.GetRange(dataOffset, size);
        }

        internal int GetBlobDataOffset(uint blob, out int size) {
            if (blob >= BlobStream.Length) {
                throw new BadImageFormatException();
            }

            int offset = (int)blob;
            int bytesRead;
            size = BlobStream.ReadCompressedInt32(offset, out bytesRead);
            if (offset > BlobStream.Length - bytesRead - size) {
                throw new BadImageFormatException();
            }

            return offset + bytesRead;
        }

        internal object GetBlobValue(uint blob, ElementType type) {
            int size;
            int offset = GetBlobDataOffset(blob, out size);
            if (size < GetMinTypeSize(type)) {
                throw new BadImageFormatException();
            }

            switch (type) {
                case ElementType.Boolean: return BlobStream.ReadByte(offset) != 0;
                case ElementType.Char: return (char)BlobStream.ReadUInt16(offset);
                case ElementType.Int8: return BlobStream.ReadSByte(offset);
                case ElementType.UInt8: return BlobStream.ReadByte(offset);
                case ElementType.Int16: return BlobStream.ReadInt16(offset);
                case ElementType.UInt16: return BlobStream.ReadUInt16(offset);
                case ElementType.Int32: return BlobStream.ReadInt32(offset);
                case ElementType.UInt32: return BlobStream.ReadUInt32(offset);
                case ElementType.Int64: return BlobStream.ReadInt64(offset);
                case ElementType.UInt64: return BlobStream.ReadUInt64(offset);
                case ElementType.Single: return BlobStream.ReadSingle(offset);
                case ElementType.Double: return BlobStream.ReadDouble(offset);

                case ElementType.String:
                    return BlobStream.ReadUtf16(offset, size);

                case ElementType.Class:
                    if (BlobStream.ReadUInt32(offset) != 0) {
                        throw new BadImageFormatException();
                    }
                    return null;

                case ElementType.Void:
                    return DBNull.Value;

                default:
                    throw new BadImageFormatException();
            }
        }

        private static int GetMinTypeSize(ElementType type) {
            switch (type) {
                case ElementType.Boolean:
                case ElementType.Int8:
                case ElementType.UInt8:
                    return 1;

                case ElementType.Char:
                case ElementType.Int16:
                case ElementType.UInt16:
                    return 2;

                case ElementType.Int32:
                case ElementType.UInt32:
                case ElementType.Single:
                case ElementType.Class:
                    return 2;

                case ElementType.Int64:
                case ElementType.UInt64:
                case ElementType.Double:
                    return 8;

                case ElementType.String:
                case ElementType.Void:
                    return 0;
            }
            return Int32.MaxValue;
        }

        internal Guid GetGuid(uint blob) {
            if (blob - 1 > GUIDStream.Length - 16) {
                throw new BadImageFormatException();
            }

            if (blob == 0) {
                return Guid.Empty;
            }

            return GUIDStream.ReadGuid((int)(blob - 1));
        }

        #endregion

        #region Table Enumeration

        internal int GetFieldRange(int typeDefRid, out int count) {
            Debug.Assert(typeDefRid <= TypeDefTable.NumberOfRows);

            int numberOfFieldRows = UseFieldPtrTable ? FieldPtrTable.NumberOfRows : FieldTable.NumberOfRows;
            uint start = TypeDefTable.GetFirstFieldRid(typeDefRid);
            uint nextStart = (typeDefRid == TypeDefTable.NumberOfRows) ? (uint)numberOfFieldRows + 1 : TypeDefTable.GetFirstFieldRid(typeDefRid + 1);

            count = GetRangeCount(numberOfFieldRows, start, nextStart);
            return (int)start;
        }

        internal int GetMethodRange(int typeDefRid, out int count) {
            Debug.Assert(typeDefRid <= TypeDefTable.NumberOfRows);

            int numberOfMethodRows = UseMethodPtrTable ? MethodPtrTable.NumberOfRows : MethodTable.NumberOfRows;

            uint start = TypeDefTable.GetFirstMethodRid(typeDefRid);
            uint nextStart = (typeDefRid == TypeDefTable.NumberOfRows) ? (uint)numberOfMethodRows + 1 : TypeDefTable.GetFirstMethodRid(typeDefRid + 1);

            count = GetRangeCount(numberOfMethodRows, start, nextStart);
            return (int)start;
        }

        internal int GetEventRange(int typeDefRid, out int count) {
            Debug.Assert(typeDefRid <= TypeDefTable.NumberOfRows);

            int eventMapRid = EventMapTable.FindEventMapRowIdFor(typeDefRid);
            if (eventMapRid == 0) {
                count = 0;
                return 0;
            }

            int numberOfEventRows = UseEventPtrTable ? EventPtrTable.NumberOfRows : EventTable.NumberOfRows;
            uint start = EventMapTable.GetEventListStartFor(eventMapRid);
            uint nextStart = (eventMapRid == EventMapTable.NumberOfRows) ? (uint)numberOfEventRows + 1 : EventMapTable.GetEventListStartFor(eventMapRid + 1);

            count = GetRangeCount(numberOfEventRows, start, nextStart);
            return (int)start;
        }

        internal int GetPropertyRange(int typeDefRid, out int count) {
            Debug.Assert(typeDefRid <= TypeDefTable.NumberOfRows);

            int propertyMapRid = PropertyMapTable.FindPropertyMapRowIdFor(typeDefRid);
            if (propertyMapRid == 0) {
                count = 0;
                return 0;
            }

            int numberOfPropertyRows = UsePropertyPtrTable ? PropertyPtrTable.NumberOfRows : PropertyTable.NumberOfRows;
            uint start = PropertyMapTable.GetFirstPropertyRid(propertyMapRid);
            uint nextStart = (propertyMapRid == PropertyMapTable.NumberOfRows) ? (uint)numberOfPropertyRows + 1 : PropertyMapTable.GetFirstPropertyRid(propertyMapRid + 1);

            count = GetRangeCount(numberOfPropertyRows, start, nextStart);
            return (int)start;
        }

        private int GetRangeCount(int rowCount, uint start, uint nextStart) {
            if (start == 0) {
                return 0;
            }

            if (start > rowCount + 1 || nextStart > rowCount + 1 || start > nextStart) {
                throw new BadImageFormatException();
            }
            return (int)(nextStart - start);
        }

        internal int GetParamRange(int methodDefRid, out int count) {
            Debug.Assert(methodDefRid <= MethodTable.NumberOfRows);

            int numberOfParamRows = UseParamPtrTable ? ParamPtrTable.NumberOfRows : ParamTable.NumberOfRows;
            uint start = MethodTable.GetFirstParamRid(methodDefRid);
            uint nextStart = (methodDefRid == MethodTable.NumberOfRows) ? (uint)numberOfParamRows + 1 : MethodTable.GetFirstParamRid(methodDefRid + 1);

            count = GetRangeCount(numberOfParamRows, start, nextStart);
            return (int)start;
        }

        internal EnumerationIndirection GetEnumeratorRange(MetadataTokenType type, MetadataToken parent, out int startRid, out int count) {
            Debug.Assert(IsValidToken(parent));

            switch (type) {
                case MetadataTokenType.MethodDef:
                    if (parent.IsNull) {
                        count = MethodTable.NumberOfRows;
                        startRid = 1;
                    } else {
                        Debug.Assert(parent.IsTypeDef);
                        startRid = GetMethodRange(parent.Rid, out count);
                    }
                    return UseParamPtrTable ? EnumerationIndirection.Method : EnumerationIndirection.None;

                case MetadataTokenType.Property:
                    if (parent.IsNull) {
                        count = PropertyTable.NumberOfRows;
                        startRid = 1;
                    } else {
                        Debug.Assert(parent.IsTypeDef);
                        startRid = GetPropertyRange(parent.Rid, out count);
                    }
                    return UsePropertyPtrTable ? EnumerationIndirection.Property : EnumerationIndirection.None;

                case MetadataTokenType.Event:
                    if (parent.IsNull) {
                        count = EventTable.NumberOfRows;
                        startRid = 1;
                    } else {
                        Debug.Assert(parent.IsTypeDef);
                        startRid = GetEventRange(parent.Rid, out count);
                    }
                    return UseEventPtrTable ? EnumerationIndirection.Event : EnumerationIndirection.None;

                case MetadataTokenType.FieldDef:
                    if (parent.IsNull) {
                        count = FieldTable.NumberOfRows;
                        startRid = 1;
                    } else {
                        Debug.Assert(parent.IsTypeDef);
                        startRid = GetFieldRange(parent.Rid, out count);
                    }
                    return UseFieldPtrTable ? EnumerationIndirection.Field : EnumerationIndirection.None;

                case MetadataTokenType.ParamDef:
                    if (parent.IsNull) {
                        count = ParamTable.NumberOfRows;
                        startRid = 1;
                    } else {
                        Debug.Assert(parent.IsMethodDef);
                        startRid = GetParamRange(parent.Rid, out count);
                    }
                    return UseParamPtrTable ? EnumerationIndirection.Param : EnumerationIndirection.None;

                case MetadataTokenType.CustomAttribute:
                    if (parent.IsNull) {
                        count = CustomAttributeTable.NumberOfRows;
                        startRid = 1;
                    } else {
                        startRid = CustomAttributeTable.FindCustomAttributesForToken(parent, out count);
                    }
                    return EnumerationIndirection.None;

                case MetadataTokenType.InterfaceImpl:
                    if (parent.IsNull) {
                        count = InterfaceImplTable.NumberOfRows;
                        startRid = 1;
                    } else {
                        Debug.Assert(parent.IsTypeDef);
                        startRid = InterfaceImplTable.FindInterfaceImplForType(parent.Rid, out count);
                    }
                    return EnumerationIndirection.None;

                case MetadataTokenType.GenericPar:
                    if (parent.IsNull) {
                        count = GenericParamTable.NumberOfRows;
                        startRid = 1;
                    } else if (parent.IsTypeDef) {
                        startRid = GenericParamTable.FindGenericParametersForType(parent.Rid, out count);
                    } else {
                        Debug.Assert(parent.IsMethodDef);
                        startRid = GenericParamTable.FindGenericParametersForMethod(parent.Rid, out count);
                    }
                    return EnumerationIndirection.None;

                case MetadataTokenType.GenericParamConstraint:
                    if (parent.IsNull) {
                        count = GenericParamConstraintTable.NumberOfRows;
                        startRid = 1;
                    } else {
                        Debug.Assert(parent.IsGenericParam);
                        startRid = GenericParamConstraintTable.FindConstraintForGenericParam(parent.Rid, out count);
                    }
                    return EnumerationIndirection.None;

                case MetadataTokenType.AssemblyRef:
                case MetadataTokenType.ModuleRef:
                case MetadataTokenType.File:
                case MetadataTokenType.TypeDef:
                case MetadataTokenType.TypeSpec:
                case MetadataTokenType.TypeRef:
                case MetadataTokenType.NestedClass:
                case MetadataTokenType.ExportedType:
                case MetadataTokenType.MethodSpec:
                case MetadataTokenType.MemberRef:
                case MetadataTokenType.Signature:
                case MetadataTokenType.ManifestResource:
                    Debug.Assert(parent.IsNull);
                    count = _tableRowCounts[(int)type >> 24];
                    startRid = 1;
                    return EnumerationIndirection.None;

                default:
                    Debug.Assert(false);
                    throw new InvalidOperationException();
            }
        }

        #endregion

        internal bool IsValidToken(MetadataToken token) {
            int tableIndex = (int)token.RecordType;
            if (tableIndex < _tableRowCounts.Length) {
                return token.Rid <= _tableRowCounts[tableIndex];
            }
            switch (tableIndex) {
                case (int)MetadataTokenType.String >> 24: return token.Rid < StringStream.Length;
                case (int)MetadataTokenType.Name >> 24: return token.Rid < UserStringStream.Length;
            }
            return false;
        }

        internal int GetRowCount(int tableIndex) {
            return (int)_tableRowCounts[tableIndex];
        }

        internal MetadataName GetMetadataName(uint blob) {
            return StringStream.ReadName(blob);
        }

        internal object GetDefaultValue(MetadataToken token) {
            ElementType type;
            int constantRid = ConstantTable.GetConstantRowId(token);
            if (constantRid == 0) {
                return Missing.Value;
            }
            uint blob = ConstantTable.GetValue(constantRid, out type);
            return GetBlobValue(blob, type);
        }

        #region Dump
#if DEBUG
        public unsafe void Dump(TextWriter output) {
            IntPtr imageStart = (IntPtr)_image.Pointer;
            output.WriteLine("Image:");
            output.WriteLine("  {0}", _image.Length);
            output.WriteLine();
            output.WriteLine("COFF header:");
            output.WriteLine("  NumberOfSections            {0}", _numberOfSections);
            output.WriteLine();

            output.WriteLine("Directories");
            output.WriteLine("  ResourceTableDirectory      +{0:X8} {1}", _optionalHeaderDirectoryEntries.ResourceTableDirectory.RelativeVirtualAddress, _optionalHeaderDirectoryEntries.ResourceTableDirectory.Size);
            output.WriteLine("  COR20HeaderTableDirectory   +{0:X8} {1}", _optionalHeaderDirectoryEntries.COR20HeaderTableDirectory.RelativeVirtualAddress, _optionalHeaderDirectoryEntries.COR20HeaderTableDirectory.Size);

            output.WriteLine();
            foreach (var section in _sectionHeaders) {
                output.WriteLine("Section");
                output.WriteLine("  VirtualAddress                   {0}", section.VirtualAddress);
                output.WriteLine("  VirtualSize                      {0}", section.VirtualSize);
                output.WriteLine("  SizeOfRawData                    {0}", section.SizeOfRawData);
                output.WriteLine("  OffsetToRawData                  {0}", section.OffsetToRawData);
            }

            output.WriteLine();
            output.WriteLine("Win32Resources:");
            output.WriteLine("  +{0:X8} {1}", _win32ResourceBlock.Pointer - _image.Pointer, _win32ResourceBlock.Length);

            output.WriteLine();
            output.WriteLine("COR20 Header:");
            output.WriteLine("  MetaDataDirectory                @{0:X8} {1}", COR20Header.MetaDataDirectory.RelativeVirtualAddress, COR20Header.MetaDataDirectory.Size);
            output.WriteLine("  ResourcesDirectory               @{0:X8} {1}", COR20Header.ResourcesDirectory.RelativeVirtualAddress, COR20Header.ResourcesDirectory.Size);
            output.WriteLine("  StrongNameSignatureDirectory     @{0:X8} {1}", COR20Header.StrongNameSignatureDirectory.RelativeVirtualAddress, COR20Header.StrongNameSignatureDirectory.Size);
            output.WriteLine();
            output.WriteLine("MetadataHeader:");
            output.WriteLine("  Signature                        {0}", MetadataHeader.Signature);
            output.WriteLine("  MajorVersion                     {0}", MetadataHeader.MajorVersion);
            output.WriteLine("  MinorVersion                     {0}", MetadataHeader.MinorVersion);
            output.WriteLine("  ExtraData                        {0}", MetadataHeader.ExtraData);
            output.WriteLine("  VersionStringSize                {0}", MetadataHeader.VersionStringSize);
            output.WriteLine("  VersionString                    '{0}'", MetadataHeader.VersionString);
            output.WriteLine();
            output.WriteLine("StorageHeader:");
            output.WriteLine("  Flags                            {0}", StorageHeader.Flags);
            output.WriteLine("  NumberOfStreams                  {0}", StorageHeader.NumberOfStreams);

            output.WriteLine();
            output.WriteLine("Streams:");
            foreach (var stream in StreamHeaders) {
                output.WriteLine("  {0,-10}             {1:X8} {2}", "'" + stream.Name + "'", stream.Offset, stream.Size);
            }

            output.WriteLine();
            output.WriteLine("StringStream:            +{0:X8}", StringStream.Pointer - _image.Pointer);
            output.WriteLine("BlobStream:              +{0:X8}", BlobStream.Pointer - _image.Pointer);
            output.WriteLine("GUIDStream:              +{0:X8}", GUIDStream.Pointer - _image.Pointer);
            output.WriteLine("UserStringStream:        +{0:X8}", UserStringStream.Pointer - _image.Pointer);
            output.WriteLine("MetadataTableStream:     +{0:X8}", MetadataTableStream.Pointer - _image.Pointer);
            output.WriteLine("ResourceMemoryReader:    +{0:X8}", ResourceMemoryBlock.Pointer - _image.Pointer);
            
            output.WriteLine();
            output.WriteLine("Misc:");
            output.WriteLine("  MetadataStreamKind     {0}", MetadataStreamKind);
        }

#endif
        #endregion
    }
}

#endif