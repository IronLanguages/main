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
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace Microsoft.Scripting.Metadata {
    internal sealed class MetadataImport {
        private readonly MemoryBlock _image;
        private const int TableCount = (int)MetadataRecordType.GenericParamConstraint + 1;

        internal MetadataImport(MemoryBlock image) {
            _image = image;

            try {
                ReadPEFileLevelData();
                ReadCORModuleLevelData();
                ReadMetadataLevelData();
            } catch (ArgumentOutOfRangeException) {
                throw new BadImageFormatException();
            }
        }

        #region PE File

        private int _numberOfSections;
        private OptionalHeaderDirectoryEntries _optionalHeaderDirectoryEntries;
        private SectionHeader[] _sectionHeaders;
        // private MemoryBlock _win32ResourceBlock;

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
            if (memReader.RemainingBytes < _numberOfSections * PEFileConstants.SizeofSectionHeader) {
                throw new BadImageFormatException();
            }

            _sectionHeaders = new SectionHeader[_numberOfSections];
            SectionHeader[] sectionHeaderArray = _sectionHeaders;
            for (int i = 0; i < _numberOfSections; i++) {
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
            uint signature = memReader.ReadUInt32();
            if (signature != PEFileConstants.PESignature) {
                throw new BadImageFormatException();
            }

            //  Read the COFF Header
            _numberOfSections = memReader.Block.ReadUInt16(memReader.Position + sizeof(ushort));
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

            // _win32ResourceBlock = DirectoryToMemoryBlock(_optionalHeaderDirectoryEntries.ResourceTableDirectory);
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

        #endregion

        #region COR Module

        private COR20Header _cor20Header;
        private StorageHeader _storageHeader;
        private StreamHeader[] _streamHeaders;

        private MemoryBlock _stringStream;
        private MemoryBlock _blobStream;
        private MemoryBlock _guidStream;
        private MemoryBlock _userStringStream;

        private MetadataStreamKind _metadataStreamKind;
        private MemoryBlock _metadataTableStream;
        // private MemoryBlock _resourceMemoryBlock;
        // private MemoryBlock _strongNameSignatureBlock;

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

            _cor20Header.MetaDataDirectory.RelativeVirtualAddress = memReader.ReadUInt32();
            _cor20Header.MetaDataDirectory.Size = memReader.ReadUInt32();
            
            // COR20Header.COR20Flags = (COR20Flags)memReader.ReadUInt32();
            // COR20Header.EntryPointTokenOrRVA = memReader.ReadUInt32();
            memReader.SeekRelative(2 * sizeof(uint));

            _cor20Header.ResourcesDirectory.RelativeVirtualAddress = memReader.ReadUInt32();
            _cor20Header.ResourcesDirectory.Size = memReader.ReadUInt32();
            _cor20Header.StrongNameSignatureDirectory.RelativeVirtualAddress = memReader.ReadUInt32();
            _cor20Header.StrongNameSignatureDirectory.Size = memReader.ReadUInt32();

            // CodeManagerTableDirectory
            // VtableFixupsDirectory
            // ExportAddressTableJumpsDirectory
            // ManagedNativeHeaderDirectory
            memReader.SeekRelative(4 * 2 * sizeof(uint));
        }

        private void ReadMetadataHeader(MemoryReader memReader) {
            uint signature = memReader.ReadUInt32();
            if (signature != COR20Constants.COR20MetadataSignature) {
                throw new BadImageFormatException();
            }

            // MajorVersion = memReader.ReadUInt16();
            // MinorVersion = memReader.ReadUInt16();
            memReader.SeekRelative(2 * sizeof(ushort));
            
            uint reserved = memReader.ReadUInt32();
            if (reserved != 0) {
                throw new BadImageFormatException();
            }

            int versionStringSize = memReader.ReadInt32();
            memReader.SeekRelative(versionStringSize);
        }

        private void ReadStorageHeader(MemoryReader memReader) {
            _storageHeader.Flags = memReader.ReadUInt16();
            _storageHeader.NumberOfStreams = memReader.ReadUInt16();
        }

        private void ReadStreamHeaders(MemoryReader memReader) {
            int numberOfStreams = _storageHeader.NumberOfStreams;
            _streamHeaders = new StreamHeader[numberOfStreams];
            StreamHeader[] streamHeaders = _streamHeaders;
            for (int i = 0; i < numberOfStreams; i++) {
                if (memReader.RemainingBytes < COR20Constants.MinimumSizeofStreamHeader) {
	                throw new BadImageFormatException();
                }

                streamHeaders[i].Offset = memReader.ReadUInt32();
                streamHeaders[i].Size = memReader.ReadUInt32();
                streamHeaders[i].Name = memReader.ReadAscii(32);
                memReader.Align(4);
            }
        }

        private void ProcessAndCacheStreams(MemoryBlock metadataRoot) {
            _metadataStreamKind = MetadataStreamKind.Illegal;

            foreach (StreamHeader streamHeader in _streamHeaders) {
                if ((long)streamHeader.Offset + streamHeader.Size > metadataRoot.Length) {
                    throw new BadImageFormatException();
                }
                MemoryBlock block = metadataRoot.GetRange((int)streamHeader.Offset, (int)streamHeader.Size);

                switch (streamHeader.Name) {
                    case COR20Constants.StringStreamName:
                        if (_stringStream != null) {
                            throw new BadImageFormatException();
                        }
                        // the first and the last byte of the heap must be zero:
                        if (block.Length == 0 || block.ReadByte(0) != 0 || block.ReadByte(block.Length - 1) != 0) {
                            throw new BadImageFormatException();
                        }
                        _stringStream = block;
                        break;

                    case COR20Constants.BlobStreamName:
                        if (_blobStream != null) {
                            throw new BadImageFormatException();
                        }
                        _blobStream = block;
                        break;

                    case COR20Constants.GUIDStreamName:
                        if (_guidStream != null) {
                            throw new BadImageFormatException();
                        }
                        _guidStream = block;
                        break;

                    case COR20Constants.UserStringStreamName:
                        if (_userStringStream != null) {
                            throw new BadImageFormatException();
                        }
                        _userStringStream = block;
                        break;

                    case COR20Constants.CompressedMetadataTableStreamName:
                        if (_metadataStreamKind != MetadataStreamKind.Illegal) {
                            throw new BadImageFormatException();
                        }
                        _metadataStreamKind = MetadataStreamKind.Compressed;
                        _metadataTableStream = block;
                        break;

                    case COR20Constants.UncompressedMetadataTableStreamName:
                        if (_metadataStreamKind != MetadataStreamKind.Illegal) {
                            throw new BadImageFormatException();
                        }
                        _metadataStreamKind = MetadataStreamKind.UnCompressed;
                        _metadataTableStream = block;
                        break;

                    default:
		                throw new BadImageFormatException();
                }
            }

            // mandatory streams:
            if (_stringStream == null || _guidStream == null || _metadataStreamKind == MetadataStreamKind.Illegal) {
                throw new BadImageFormatException();
            }
        }

        private void ReadCORModuleLevelData() {
            ReadCOR20Header();

            MemoryBlock metadataRoot = DirectoryToMemoryBlock(_cor20Header.MetaDataDirectory);
            if (metadataRoot == null || metadataRoot.Length < _cor20Header.MetaDataDirectory.Size) {
                throw new BadImageFormatException();
            }

            MemoryReader memReader = new MemoryReader(metadataRoot);

            ReadMetadataHeader(memReader);
            ReadStorageHeader(memReader);
            ReadStreamHeaders(memReader);
            ProcessAndCacheStreams(metadataRoot);

            // _resourceMemoryBlock = DirectoryToMemoryBlock(_cor20Header.ResourcesDirectory);
            // _strongNameSignatureBlock = DirectoryToMemoryBlock(_cor20Header.StrongNameSignatureDirectory);
        }

        #endregion Methods [CORModule]

        #region Metadata

        private MetadataTableHeader _metadataTableHeader;
        private int[] _tableRowCounts;

        internal ModuleTable ModuleTable;
        internal TypeRefTable TypeRefTable;
        internal TypeDefTable TypeDefTable;
        internal FieldPtrTable FieldPtrTable;
        internal FieldTable FieldTable;
        internal MethodPtrTable MethodPtrTable;
        internal MethodTable MethodTable;
        internal ParamPtrTable ParamPtrTable;
        internal ParamTable ParamTable;
        internal InterfaceImplTable InterfaceImplTable;
        internal MemberRefTable MemberRefTable;
        internal ConstantTable ConstantTable;
        internal CustomAttributeTable CustomAttributeTable;
        internal FieldMarshalTable FieldMarshalTable;
        internal DeclSecurityTable DeclSecurityTable;
        internal ClassLayoutTable ClassLayoutTable;
        internal FieldLayoutTable FieldLayoutTable;
        internal StandAloneSigTable StandAloneSigTable;
        internal EventMapTable EventMapTable;
        internal EventPtrTable EventPtrTable;
        internal EventTable EventTable;
        internal PropertyMapTable PropertyMapTable;
        internal PropertyPtrTable PropertyPtrTable;
        internal PropertyTable PropertyTable;
        internal MethodSemanticsTable MethodSemanticsTable;
        internal MethodImplTable MethodImplTable;
        internal ModuleRefTable ModuleRefTable;
        internal TypeSpecTable TypeSpecTable;
        internal ImplMapTable ImplMapTable;
        internal FieldRVATable FieldRVATable;
        internal EnCLogTable EnCLogTable;
        internal EnCMapTable EnCMapTable;
        internal AssemblyTable AssemblyTable;
        internal AssemblyProcessorTable AssemblyProcessorTable;
        internal AssemblyOSTable AssemblyOSTable;
        internal AssemblyRefTable AssemblyRefTable;
        internal AssemblyRefProcessorTable AssemblyRefProcessorTable;
        internal AssemblyRefOSTable AssemblyRefOSTable;
        internal FileTable FileTable;
        internal ExportedTypeTable ExportedTypeTable;
        internal ManifestResourceTable ManifestResourceTable;
        internal NestedClassTable NestedClassTable;
        internal GenericParamTable GenericParamTable;
        internal MethodSpecTable MethodSpecTable;
        internal GenericParamConstraintTable GenericParamConstraintTable;

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

        private void ReadMetadataTableInformation(MemoryReader memReader) {
            if (memReader.RemainingBytes < MetadataStreamConstants.SizeOfMetadataTableHeader) {
                throw new BadImageFormatException();
            }

            // Reserved
            memReader.SeekRelative(sizeof(uint));

            _metadataTableHeader.MajorVersion = memReader.ReadByte();
            _metadataTableHeader.MinorVersion = memReader.ReadByte();
            _metadataTableHeader.HeapSizeFlags = (HeapSizeFlag)memReader.ReadByte();
            
            // Rid
            memReader.SeekRelative(sizeof(byte));

            _metadataTableHeader.ValidTables = (TableMask)memReader.ReadUInt64();
            _metadataTableHeader.SortedTables = (TableMask)memReader.ReadUInt64();
            ulong presentTables = (ulong)_metadataTableHeader.ValidTables;
            ulong validTablesForVersion = 0;

            int version = _metadataTableHeader.MajorVersion << 8 | _metadataTableHeader.MinorVersion;
            switch (version) {
                case 0x0100:
                    validTablesForVersion = (ulong)TableMask.V1_0_TablesMask;
                    break;

                case 0x0101:
                    validTablesForVersion = (ulong)TableMask.V1_1_TablesMask;
                    break;

                case 0x0200:
                    validTablesForVersion = (ulong)TableMask.V2_0_TablesMask;
                    break;

                default:
                    throw new BadImageFormatException();
            }

            if ((presentTables & ~validTablesForVersion) != 0) {
                throw new BadImageFormatException();
            }

            if (_metadataStreamKind == MetadataStreamKind.Compressed && (presentTables & (ulong)TableMask.CompressedStreamNotAllowedMask) != 0) {
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
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
        }

        private void ReadMetadataLevelData() {
            MemoryReader memReader = new MemoryReader(_metadataTableStream);

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
            _blobStream.Read(dataOffset, result);
            return result;
        }

        internal MemoryBlock GetBlobBlock(uint blob) {
            int size;
            int dataOffset = GetBlobDataOffset(blob, out size);
            return _blobStream.GetRange(dataOffset, size);
        }

        internal int GetBlobDataOffset(uint blob, out int size) {
            if (_blobStream == null || blob >= _blobStream.Length) {
                throw new BadImageFormatException();
            }

            int offset = (int)blob;
            int bytesRead;
            size = _blobStream.ReadCompressedInt32(offset, out bytesRead);
            if (offset > _blobStream.Length - bytesRead - size) {
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
                case ElementType.Boolean: return _blobStream.ReadByte(offset) != 0;
                case ElementType.Char: return (char)_blobStream.ReadUInt16(offset);
                case ElementType.Int8: return _blobStream.ReadSByte(offset);
                case ElementType.UInt8: return _blobStream.ReadByte(offset);
                case ElementType.Int16: return _blobStream.ReadInt16(offset);
                case ElementType.UInt16: return _blobStream.ReadUInt16(offset);
                case ElementType.Int32: return _blobStream.ReadInt32(offset);
                case ElementType.UInt32: return _blobStream.ReadUInt32(offset);
                case ElementType.Int64: return _blobStream.ReadInt64(offset);
                case ElementType.UInt64: return _blobStream.ReadUInt64(offset);
                case ElementType.Single: return _blobStream.ReadSingle(offset);
                case ElementType.Double: return _blobStream.ReadDouble(offset);

                case ElementType.String:
                    return _blobStream.ReadUtf16(offset, size);

                case ElementType.Class:
                    if (_blobStream.ReadUInt32(offset) != 0) {
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
            if (blob - 1 > _guidStream.Length - 16) {
                throw new BadImageFormatException();
            }

            if (blob == 0) {
                return Guid.Empty;
            }

            return _guidStream.ReadGuid((int)(blob - 1));
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
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
                case (int)MetadataTokenType.String >> 24: return token.Rid < _stringStream.Length;
                case (int)MetadataTokenType.Name >> 24: return _userStringStream != null && token.Rid < _userStringStream.Length;
            }
            return false;
        }

        internal int GetRowCount(int tableIndex) {
            return (int)_tableRowCounts[tableIndex];
        }

        internal MetadataName GetMetadataName(uint blob) {
            return _stringStream.ReadName(blob);
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

        #region Test Support

        internal MemoryBlock Image {
            get { return _image; }
        }

        #endregion

        #region Dump
#if DEBUG
        public unsafe void Dump(TextWriter output) {
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

            //output.WriteLine();
            //output.WriteLine("Win32Resources:");
            //output.WriteLine("  +{0:X8} {1}", _win32ResourceBlock.Pointer - _image.Pointer, _win32ResourceBlock.Length);

            output.WriteLine();
            output.WriteLine("COR20 Header:");
            output.WriteLine("  MetaDataDirectory                @{0:X8} {1}", _cor20Header.MetaDataDirectory.RelativeVirtualAddress, _cor20Header.MetaDataDirectory.Size);
            output.WriteLine("  ResourcesDirectory               @{0:X8} {1}", _cor20Header.ResourcesDirectory.RelativeVirtualAddress, _cor20Header.ResourcesDirectory.Size);
            output.WriteLine("  StrongNameSignatureDirectory     @{0:X8} {1}", _cor20Header.StrongNameSignatureDirectory.RelativeVirtualAddress, _cor20Header.StrongNameSignatureDirectory.Size);
            output.WriteLine();
            output.WriteLine("StorageHeader:");
            output.WriteLine("  Flags                            {0}", _storageHeader.Flags);
            output.WriteLine("  NumberOfStreams                  {0}", _storageHeader.NumberOfStreams);

            output.WriteLine();
            output.WriteLine("Streams:");
            foreach (var stream in _streamHeaders) {
                output.WriteLine("  {0,-10}             {1:X8} {2}", "'" + stream.Name + "'", stream.Offset, stream.Size);
            }

            output.WriteLine();
            output.WriteLine("StringStream:            +{0:X8}", _stringStream.Pointer - _image.Pointer);
            output.WriteLine("BlobStream:              +{0:X8}", _blobStream != null ? (_blobStream.Pointer - _image.Pointer) : 0);
            output.WriteLine("GUIDStream:              +{0:X8}", _guidStream.Pointer - _image.Pointer);
            output.WriteLine("UserStringStream:        +{0:X8}", _userStringStream != null ? (_userStringStream.Pointer - _image.Pointer) : 0);
            output.WriteLine("MetadataTableStream:     +{0:X8}", _metadataTableStream.Pointer - _image.Pointer);
            //output.WriteLine("ResourceMemoryReader:    +{0:X8}", _resourceMemoryBlock != null ? (_resourceMemoryBlock.Pointer - _image.Pointer) : 0);
            
            output.WriteLine();
            output.WriteLine("Misc:");
            output.WriteLine("  MetadataStreamKind     {0}", _metadataStreamKind);
        }

#endif
        #endregion
    }
}
