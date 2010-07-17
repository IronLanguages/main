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

namespace Microsoft.Scripting.Metadata {
    internal sealed class ModuleTable {
        internal const int TableIndex = 0;
        internal readonly int NumberOfRows;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly bool IsGUIDHeapRefSizeSmall;
        private readonly int GenerationOffset;
        private readonly int NameOffset;
        private readonly int MVIdOffset;
        private readonly int EnCIdOffset;
        private readonly int EnCBaseIdOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal ModuleTable(int numberOfRows, int stringHeapRefSize, int guidHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            IsGUIDHeapRefSizeSmall = guidHeapRefSize == 2;
            GenerationOffset = 0;
            NameOffset = GenerationOffset + sizeof(UInt16);
            MVIdOffset = NameOffset + stringHeapRefSize;
            EnCIdOffset = MVIdOffset + guidHeapRefSize;
            EnCBaseIdOffset = EnCIdOffset + guidHeapRefSize;
            RowSize = EnCBaseIdOffset + guidHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }

        internal uint GetMVId(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + MVIdOffset, IsGUIDHeapRefSizeSmall);
        }
    }

    internal sealed class TypeRefTable {
        internal const int TableIndex = 1;
        internal readonly int NumberOfRows;
        private readonly bool IsResolutionScopeRefSizeSmall;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly int ResolutionScopeOffset;
        private readonly int NameOffset;
        private readonly int NamespaceOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal TypeRefTable(int numberOfRows, int resolutionScopeRefSize, int stringHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsResolutionScopeRefSizeSmall = resolutionScopeRefSize == 2;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            ResolutionScopeOffset = 0;
            NameOffset = ResolutionScopeOffset + resolutionScopeRefSize;
            NamespaceOffset = NameOffset + stringHeapRefSize;
            RowSize = NamespaceOffset + stringHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }

        internal uint GetNamespace(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NamespaceOffset, IsStringHeapRefSizeSmall);
        }

        internal MetadataToken GetResolutionScope(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return ResolutionScopeTag.ConvertToToken(Table.ReadReference(rowOffset + ResolutionScopeOffset, IsResolutionScopeRefSizeSmall));
        }
    }

    internal sealed class TypeDefTable {
        internal const int TableIndex = 2;
        internal readonly int NumberOfRows;
        private readonly bool IsFieldRefSizeSmall;
        private readonly bool IsMethodRefSizeSmall;
        private readonly bool IsTypeDefOrRefRefSizeSmall;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly int FlagsOffset;
        private readonly int NameOffset;
        private readonly int NamespaceOffset;
        private readonly int ExtendsOffset;
        private readonly int FieldListOffset;
        private readonly int MethodListOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal TypeDefTable(int numberOfRows, int fieldRefSize, int methodRefSize, int typeDefOrRefRefSize, int stringHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsFieldRefSizeSmall = fieldRefSize == 2;
            IsMethodRefSizeSmall = methodRefSize == 2;
            IsTypeDefOrRefRefSizeSmall = typeDefOrRefRefSize == 2;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            FlagsOffset = 0;
            NameOffset = FlagsOffset + sizeof(UInt32);
            NamespaceOffset = NameOffset + stringHeapRefSize;
            ExtendsOffset = NamespaceOffset + stringHeapRefSize;
            FieldListOffset = ExtendsOffset + typeDefOrRefRefSize;
            MethodListOffset = FieldListOffset + fieldRefSize;
            RowSize = MethodListOffset + methodRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal TypeAttributes GetFlags(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (TypeAttributes)Table.ReadUInt32(rowOffset + FlagsOffset);
        }

        internal uint GetNamespace(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NamespaceOffset, IsStringHeapRefSizeSmall);
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }

        internal MetadataToken GetExtends(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return TypeDefOrRefTag.ConvertToToken(Table.ReadReference(rowOffset + ExtendsOffset, IsTypeDefOrRefRefSizeSmall));
        }

        internal uint GetFirstFieldRid(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + FieldListOffset, IsFieldRefSizeSmall);
        }

        internal uint GetFirstMethodRid(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + MethodListOffset, IsMethodRefSizeSmall);
        }

        internal int FindTypeContainingMethod(int methodDefOrPtrRowId, int methodTableRowCount) {
            int numOfRows = NumberOfRows;

            int rid = 1 + Table.BinarySearchForSlot(numOfRows, methodTableRowCount, RowSize, MethodListOffset, (uint)methodDefOrPtrRowId, IsMethodRefSizeSmall);
            if (rid == 0) {
                throw new BadImageFormatException();
            }
            return rid;
        }

        internal int FindTypeContainingField(int fieldDefOrPtrRowId, int fieldTableRowCount) {
            int numOfRows = NumberOfRows;

            int rid = 1 + Table.BinarySearchForSlot(numOfRows, fieldTableRowCount, RowSize, FieldListOffset, (uint)fieldDefOrPtrRowId, IsFieldRefSizeSmall);
            if (rid == 0) {
                throw new BadImageFormatException();
            }

            return rid;
        }
    }

    internal sealed class FieldPtrTable {
        internal const int TableIndex = 3;
        internal readonly int NumberOfRows;
        private readonly bool IsFieldTableRowRefSizeSmall;
        private readonly int FieldOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal FieldPtrTable(int numberOfRows, int fieldTableRowRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsFieldTableRowRefSizeSmall = fieldTableRowRefSize == 2;
            FieldOffset = 0;
            RowSize = FieldOffset + fieldTableRowRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal MetadataToken GetFieldFor(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            uint rid = Table.ReadReference(rowOffset + FieldOffset, IsFieldTableRowRefSizeSmall);
            return new MetadataToken(MetadataTokenType.FieldDef, rid);
        }
    }

    internal sealed class FieldTable {
        internal const int TableIndex = 4;
        internal readonly int NumberOfRows;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly bool IsBlobHeapRefSizeSmall;
        private readonly int FlagsOffset;
        private readonly int NameOffset;
        private readonly int SignatureOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal FieldTable(int numberOfRows, int stringHeapRefSize, int blobHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
            FlagsOffset = 0;
            NameOffset = FlagsOffset + sizeof(UInt16);
            SignatureOffset = NameOffset + stringHeapRefSize;
            RowSize = SignatureOffset + blobHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal FieldAttributes GetFlags(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (FieldAttributes)Table.ReadUInt16(rowOffset + FlagsOffset);
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }

        internal uint GetSignature(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + SignatureOffset, IsBlobHeapRefSizeSmall);
        }
    }

    internal sealed class MethodPtrTable {
        internal const int TableIndex = 5;
        internal readonly int NumberOfRows;
        private readonly bool IsMethodTableRowRefSizeSmall;
        private readonly int MethodOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal MethodPtrTable(int numberOfRows, int methodTableRowRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsMethodTableRowRefSizeSmall = methodTableRowRefSize == 2;
            MethodOffset = 0;
            RowSize = MethodOffset + methodTableRowRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal MetadataToken GetMethodFor(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            uint rid = Table.ReadReference(rowOffset + MethodOffset, IsMethodTableRowRefSizeSmall);
            return new MetadataToken(MetadataTokenType.MethodDef, rid);
        }
    }

    internal sealed class MethodTable {
        internal const int TableIndex = 6;
        internal readonly int NumberOfRows;
        private readonly bool IsParamRefSizeSmall;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly bool IsBlobHeapRefSizeSmall;
        private readonly int RVAOffset;
        private readonly int ImplFlagsOffset;
        private readonly int FlagsOffset;
        private readonly int NameOffset;
        private readonly int SignatureOffset;
        private readonly int ParamListOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal MethodTable(int numberOfRows, int paramRefSize, int stringHeapRefSize, int blobHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsParamRefSizeSmall = paramRefSize == 2;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
            RVAOffset = 0;
            ImplFlagsOffset = RVAOffset + sizeof(UInt32);
            FlagsOffset = ImplFlagsOffset + sizeof(UInt16);
            NameOffset = FlagsOffset + sizeof(UInt16);
            SignatureOffset = NameOffset + stringHeapRefSize;
            ParamListOffset = SignatureOffset + blobHeapRefSize;
            RowSize = ParamListOffset + paramRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal uint GetFirstParamRid(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + ParamListOffset, IsParamRefSizeSmall);
        }

        internal uint GetSignature(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + SignatureOffset, IsBlobHeapRefSizeSmall);
        }

        internal uint GetRVA(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadUInt32(rowOffset + RVAOffset);
        }

        internal MethodAttributes GetFlags(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (MethodAttributes)Table.ReadUInt16(rowOffset + FlagsOffset);
        }

        internal MethodImplAttributes GetImplFlags(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (MethodImplAttributes)Table.ReadUInt16(rowOffset + ImplFlagsOffset);
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }

        internal int GetNextRVA(int rva) {
            int nextRVA = Int32.MaxValue;
            int endOffset = NumberOfRows * RowSize;
            for (int iterOffset = RVAOffset; iterOffset < endOffset; iterOffset += RowSize) {
                int currentRVA = Table.ReadInt32(iterOffset);
                if (currentRVA > rva && currentRVA < nextRVA) {
                    nextRVA = currentRVA;
                }
            }
            return nextRVA == Int32.MaxValue ? -1 : nextRVA;
        }

        internal int FindMethodContainingParam(int paramDefOrPtrRowId, int paramTableRowCount) {
            int numOfRows = NumberOfRows;

            int rid = 1 + Table.BinarySearchForSlot(numOfRows, paramTableRowCount, RowSize, ParamListOffset, (uint)paramDefOrPtrRowId, IsParamRefSizeSmall);
            if (rid == 0) {
                throw new BadImageFormatException();
            }
            return rid;
        }
    }

    internal sealed class ParamPtrTable {
        internal const int TableIndex = 7;
        internal readonly int NumberOfRows;
        private readonly bool IsParamTableRowRefSizeSmall;
        private readonly int ParamOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal ParamPtrTable(int numberOfRows, int paramTableRowRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsParamTableRowRefSizeSmall = paramTableRowRefSize == 2;
            ParamOffset = 0;
            RowSize = ParamOffset + paramTableRowRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal MetadataToken GetParamFor(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            uint rid = Table.ReadReference(rowOffset + ParamOffset, IsParamTableRowRefSizeSmall);
            return new MetadataToken(MetadataTokenType.ParamDef, rid);
        }
    }

    internal sealed class ParamTable {
        internal const int TableIndex = 8;
        internal readonly int NumberOfRows;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly int FlagsOffset;
        private readonly int SequenceOffset;
        private readonly int NameOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal ParamTable(int numberOfRows, int stringHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            FlagsOffset = 0;
            SequenceOffset = FlagsOffset + sizeof(UInt16);
            NameOffset = SequenceOffset + sizeof(UInt16);
            RowSize = NameOffset + stringHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal ParameterAttributes GetFlags(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (ParameterAttributes)Table.ReadUInt16(rowOffset + FlagsOffset);
        }

        internal ushort GetSequence(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadUInt16(rowOffset + SequenceOffset);
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }
    }

    internal sealed class InterfaceImplTable {
        internal const int TableIndex = 9;
        internal readonly int NumberOfRows;
        private readonly bool IsTypeDefTableRowRefSizeSmall;
        private readonly bool IsTypeDefOrRefRefSizeSmall;
        private readonly int ClassOffset;
        private readonly int InterfaceOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal InterfaceImplTable(int numberOfRows, int typeDefTableRowRefSize, int typeDefOrRefRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsTypeDefTableRowRefSizeSmall = typeDefTableRowRefSize == 2;
            IsTypeDefOrRefRefSizeSmall = typeDefOrRefRefSize == 2;
            ClassOffset = 0;
            InterfaceOffset = ClassOffset + typeDefTableRowRefSize;
            RowSize = InterfaceOffset + typeDefOrRefRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal int FindInterfaceImplForType(int typeDefRowId, out int interfaceCount) {
            int foundRowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                ClassOffset,
                (uint)typeDefRowId,
                IsTypeDefOrRefRefSizeSmall
            );

            if (foundRowNumber == -1) {
                interfaceCount = 0;
                return 0;
            }

            int startRowNumber = foundRowNumber;
            while (
                startRowNumber > 0 && 
                Table.ReadReference((startRowNumber - 1) * RowSize + ClassOffset, IsTypeDefOrRefRefSizeSmall) == typeDefRowId
            ) {
                startRowNumber--;
            }

            int endRowNumber = foundRowNumber;
            while (
                endRowNumber + 1 < NumberOfRows &&
                Table.ReadReference((endRowNumber + 1) * RowSize + ClassOffset, IsTypeDefOrRefRefSizeSmall) == typeDefRowId
            ) {
                endRowNumber++;
            }

            interfaceCount = endRowNumber - startRowNumber + 1;
            return startRowNumber + 1;
        }

        internal uint GetClass(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + ClassOffset, IsTypeDefTableRowRefSizeSmall);
        }

        internal MetadataToken GetInterface(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return TypeDefOrRefTag.ConvertToToken(Table.ReadReference(rowOffset + InterfaceOffset, IsTypeDefOrRefRefSizeSmall));
        }
    }

    internal sealed class MemberRefTable {
        internal const int TableIndex = 0x0a;
        internal readonly int NumberOfRows;
        private readonly bool IsMemberRefParentRefSizeSmall;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly bool IsBlobHeapRefSizeSmall;
        private readonly int ClassOffset;
        private readonly int NameOffset;
        private readonly int SignatureOffset;
        private int RowSize;
        internal MemoryBlock Table;

        internal MemberRefTable(int numberOfRows, int memberRefParentRefSize, int stringHeapRefSize, int blobHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsMemberRefParentRefSizeSmall = memberRefParentRefSize == 2;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
            ClassOffset = 0;
            NameOffset = ClassOffset + memberRefParentRefSize;
            SignatureOffset = NameOffset + stringHeapRefSize;
            RowSize = SignatureOffset + blobHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal MetadataToken GetClass(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return MemberRefParentTag.ConvertToToken(Table.ReadReference(rowOffset + ClassOffset, IsMemberRefParentRefSizeSmall));
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }

        internal uint GetSignature(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + SignatureOffset, IsBlobHeapRefSizeSmall);
        }
    }

    internal sealed class ConstantTable {
        internal const int TableIndex = 0x0b;
        internal readonly int NumberOfRows;
        private readonly bool IsHasConstantRefSizeSmall;
        private readonly bool IsBlobHeapRefSizeSmall;
        private readonly int TypeOffset;
        private readonly int ParentOffset;
        private readonly int ValueOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal ConstantTable(int numberOfRows, int hasConstantRefSize, int blobHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsHasConstantRefSizeSmall = hasConstantRefSize == 2;
            IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
            TypeOffset = 0;
            ParentOffset = TypeOffset + sizeof(Byte) + 1; //  Alignment here (+1)...
            ValueOffset = ParentOffset + hasConstantRefSize;
            RowSize = ValueOffset + blobHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal int GetConstantRowId(MetadataToken parentToken) {
            int foundRowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                ParentOffset,
                HasConstantTag.ConvertToTag(parentToken),
                IsHasConstantRefSizeSmall
            );
            return foundRowNumber + 1;
        }

        internal uint GetValue(int rowId, out ElementType type) {
            int rowOffset = (rowId - 1) * RowSize;
            type = (ElementType)Table.ReadByte(rowOffset + TypeOffset);
            return Table.ReadReference(rowOffset + ValueOffset, IsBlobHeapRefSizeSmall);
        }

    }

    internal sealed class CustomAttributeTable {
        internal const int TableIndex = 0x0c;
        internal readonly int NumberOfRows;
        private readonly bool IsHasCustomAttributeRefSizeSmall;
        private readonly bool IsCustomAttriubuteTypeRefSizeSmall;
        private readonly bool IsBlobHeapRefSizeSmall;
        private readonly int ParentOffset;
        private readonly int TypeOffset;
        private readonly int ValueOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal CustomAttributeTable(int numberOfRows, int hasCustomAttributeRefSize, int customAttributeTypeRefSize, int blobHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsHasCustomAttributeRefSizeSmall = hasCustomAttributeRefSize == 2;
            IsCustomAttriubuteTypeRefSizeSmall = customAttributeTypeRefSize == 2;
            IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
            ParentOffset = 0;
            TypeOffset = ParentOffset + hasCustomAttributeRefSize;
            ValueOffset = TypeOffset + customAttributeTypeRefSize;
            RowSize = ValueOffset + blobHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal MetadataToken GetParent(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return HasCustomAttributeTag.ConvertToToken(Table.ReadReference(rowOffset + ParentOffset, IsHasCustomAttributeRefSizeSmall));
        }

        internal MetadataToken GetConstructor(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return CustomAttributeTypeTag.ConvertToToken(Table.ReadReference(rowOffset + TypeOffset, IsCustomAttriubuteTypeRefSizeSmall));
        }

        internal uint GetValue(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + ValueOffset, IsBlobHeapRefSizeSmall);
        }

        internal int FindCustomAttributesForToken(MetadataToken token, out int customAttributeCount) {
            return BinarySearchTag(HasCustomAttributeTag.ConvertToTag(token), out customAttributeCount);
        }

        // returns RowId
        private int BinarySearchTag(uint searchCodedTag, out int customAttributeCount) {
            int foundRowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                ParentOffset,
                searchCodedTag,
                IsHasCustomAttributeRefSizeSmall
            );

            if (foundRowNumber == -1) {
                customAttributeCount = 0;
                return 0;
            }

            int startRowNumber = foundRowNumber;
            while (
                startRowNumber > 0 && 
                Table.ReadReference((startRowNumber - 1) * RowSize + ParentOffset, IsHasCustomAttributeRefSizeSmall) == searchCodedTag
            ) {
                startRowNumber--;
            }

            int endRowNumber = foundRowNumber;
            while (
                endRowNumber + 1 < NumberOfRows &&
                Table.ReadReference((endRowNumber + 1) * RowSize + ParentOffset, IsHasCustomAttributeRefSizeSmall) == searchCodedTag
            ) {
                endRowNumber++;
            }

            customAttributeCount = endRowNumber - startRowNumber + 1;
            return startRowNumber + 1;
        }
    }

    internal sealed class FieldMarshalTable {
        internal const int TableIndex = 0x0d;
        internal readonly int NumberOfRows;
        // private readonly bool IsHasFieldMarshalRefSizeSmall;
        // private readonly bool IsBlobHeapRefSizeSmall;
        private readonly int ParentOffset;
        private readonly int NativeTypeOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal FieldMarshalTable(int numberOfRows, int hasFieldMarshalRefSize, int blobHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            // IsHasFieldMarshalRefSizeSmall = hasFieldMarshalRefSize == 2;
            // IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
            ParentOffset = 0;
            NativeTypeOffset = ParentOffset + hasFieldMarshalRefSize;
            RowSize = NativeTypeOffset + blobHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }
#if TODO
        internal int GetFieldMarshalRowId(MetadataToken token) {
            int foundRowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                ParentOffset,
                HasFieldMarshalTag.ConvertToTag(token),
                IsHasFieldMarshalRefSizeSmall
            );
            return foundRowNumber + 1;
        }
#endif
    }

    internal sealed class DeclSecurityTable {
        internal const int TableIndex = 0x0e;
        internal readonly int NumberOfRows;
        // private readonly bool IsHasDeclSecurityRefSizeSmall;
        // private readonly bool IsBlobHeapRefSizeSmall;
        private readonly int ActionOffset;
        private readonly int ParentOffset;
        private readonly int PermissionSetOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal DeclSecurityTable(int numberOfRows, int hasDeclSecurityRefSize, int blobHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            // IsHasDeclSecurityRefSizeSmall = hasDeclSecurityRefSize == 2;
            // IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
            ActionOffset = 0;
            ParentOffset = ActionOffset + sizeof(UInt16);
            PermissionSetOffset = ParentOffset + hasDeclSecurityRefSize;
            RowSize = PermissionSetOffset + blobHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }
#if TODO
        internal int FindSecurityAttributesForToken(MetadataToken token, out int securityAttributeCount) {
            uint searchCodedTag = HasDeclSecurityTag.ConvertToTag(token);
            return BinarySearchTag(searchCodedTag, out securityAttributeCount);
        }

        private int BinarySearchTag(uint searchCodedTag, out int securityAttributeCount) {
            int foundRowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                ParentOffset,
                searchCodedTag,
                IsHasDeclSecurityRefSizeSmall
            );

            if (foundRowNumber == -1) {
                securityAttributeCount = 0;
                return 0;
            }

            int startRowNumber = foundRowNumber;
            while (
                startRowNumber > 0 && 
                Table.ReadReference((startRowNumber - 1) * RowSize + ParentOffset, IsHasDeclSecurityRefSizeSmall) == searchCodedTag
            ) {
                startRowNumber--;
            }

            int endRowNumber = foundRowNumber;
            while (
                endRowNumber + 1 < NumberOfRows && 
                Table.ReadReference((endRowNumber + 1) * RowSize + ParentOffset, IsHasDeclSecurityRefSizeSmall) == searchCodedTag
            ) {
                endRowNumber++;
            }
            
            securityAttributeCount = endRowNumber - startRowNumber + 1;
            return startRowNumber + 1;
        }
#endif
    }

    internal sealed class ClassLayoutTable {
        internal const int TableIndex = 0x0f;
        internal int NumberOfRows;
        // private readonly bool IsTypeDefTableRowRefSizeSmall;
        private readonly int PackagingSizeOffset;
        private readonly int ClassSizeOffset;
        private readonly int ParentOffset;
        private int RowSize;
        internal MemoryBlock Table;

        internal ClassLayoutTable(int numberOfRows, int typeDefTableRowRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            // IsTypeDefTableRowRefSizeSmall = typeDefTableRowRefSize == 2;
            PackagingSizeOffset = 0;
            ClassSizeOffset = PackagingSizeOffset + sizeof(UInt16);
            ParentOffset = ClassSizeOffset + sizeof(UInt32);
            RowSize = ParentOffset + typeDefTableRowRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }
#if TODO
        internal ushort GetPackingSize(int typeRowId) {
            int foundRowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                ParentOffset,
                (uint)typeRowId,
                IsTypeDefTableRowRefSizeSmall
            );

            if (foundRowNumber == -1) {
                return 0;
            }

            int rowOffset = foundRowNumber * RowSize;
            return Table.ReadUInt16(rowOffset + PackagingSizeOffset);
        }

        internal uint GetClassSize(int typeRowId) {
            int foundRowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                ParentOffset,
                (uint)typeRowId,
                IsTypeDefTableRowRefSizeSmall
            );

            if (foundRowNumber == -1) {
                return 0;
            }

            int rowOffset = foundRowNumber * RowSize;
            return Table.ReadUInt32(rowOffset + ClassSizeOffset);
        }
#endif
    }

    internal sealed class FieldLayoutTable {
        internal const int TableIndex = 0x10;
        internal readonly int NumberOfRows;
        // private readonly bool IsFieldTableRowRefSizeSmall;
        private readonly int OffsetOffset;
        private readonly int FieldOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal FieldLayoutTable(int numberOfRows, int fieldTableRowRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            // IsFieldTableRowRefSizeSmall = fieldTableRowRefSize == 2;
            OffsetOffset = 0;
            FieldOffset = OffsetOffset + sizeof(UInt32);
            RowSize = FieldOffset + fieldTableRowRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }
#if TODO
        internal uint GetOffset(int rowId) {
            int foundRowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                FieldOffset,
                (uint)rowId,
                IsFieldTableRowRefSizeSmall
            );

            if (foundRowNumber == -1) {
                return 0;
            }

            int rowOffset = foundRowNumber * RowSize;
            return Table.ReadUInt32(rowOffset + OffsetOffset);
        }
#endif
    }

    internal sealed class StandAloneSigTable {
        internal const int TableIndex = 0x11;
        internal readonly int NumberOfRows;
        private readonly bool IsBlobHeapRefSizeSmall;
        private readonly int SignatureOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal StandAloneSigTable(int numberOfRows, int blobHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
            SignatureOffset = 0;
            RowSize = SignatureOffset + blobHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal uint GetSignature(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + SignatureOffset, IsBlobHeapRefSizeSmall);
        }
    }

    internal sealed class EventMapTable {
        internal const int TableIndex = 0x12;
        internal readonly int NumberOfRows;
        private readonly bool IsTypeDefTableRowRefSizeSmall;
        private readonly bool IsEventRefSizeSmall;
        private readonly int ParentOffset;
        private readonly int EventListOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal EventMapTable(int numberOfRows, int typeDefTableRowRefSize, int eventRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsTypeDefTableRowRefSizeSmall = typeDefTableRowRefSize == 2;
            IsEventRefSizeSmall = eventRefSize == 2;
            ParentOffset = 0;
            EventListOffset = ParentOffset + typeDefTableRowRefSize;
            RowSize = EventListOffset + eventRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal int FindEventMapRowIdFor(int typeDefRowId) {
            // TODO: we can sort this if not sorted already
            //  We do a linear scan here because we dont have these tables sorted
            int rowNumber = Table.LinearSearchReference(
                RowSize,
                ParentOffset,
                (uint)typeDefRowId,
                IsTypeDefTableRowRefSizeSmall
            );
            return rowNumber + 1;
        }

        internal uint GetEventListStartFor(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + EventListOffset, IsEventRefSizeSmall);
        }

        internal uint GetParent(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + ParentOffset, IsTypeDefTableRowRefSizeSmall);
        }

        internal uint FindTypeContainingEvent(int eventDefOrPtrRowId, int eventTableRowCount) {
            int numOfRows = NumberOfRows;

            int rid = 1 + Table.BinarySearchForSlot(numOfRows, eventTableRowCount, RowSize, EventListOffset, (uint)eventDefOrPtrRowId, IsEventRefSizeSmall);
            if (rid == 0) {
                throw new BadImageFormatException();
            }
            return GetParent(rid);
        }
    }

    internal sealed class EventPtrTable {
        internal const int TableIndex = 0x13;
        internal readonly int NumberOfRows;
        private readonly bool IsEventTableRowRefSizeSmall;
        private readonly int EventOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal EventPtrTable(int numberOfRows, int eventTableRowRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsEventTableRowRefSizeSmall = eventTableRowRefSize == 2;
            EventOffset = 0;
            RowSize = EventOffset + eventTableRowRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal MetadataToken GetEventFor(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            uint rid = Table.ReadReference(rowOffset + EventOffset, IsEventTableRowRefSizeSmall);
            return new MetadataToken(MetadataTokenType.Event, rid);
        }
    }

    internal sealed class EventTable {
        internal const int TableIndex = 0x14;
        internal int NumberOfRows;
        private readonly bool IsTypeDefOrRefRefSizeSmall;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly int FlagsOffset;
        private readonly int NameOffset;
        private readonly int EventTypeOffset;
        private int RowSize;
        internal MemoryBlock Table;

        internal EventTable(int numberOfRows, int typeDefOrRefRefSize, int stringHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsTypeDefOrRefRefSizeSmall = typeDefOrRefRefSize == 2;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            FlagsOffset = 0;
            NameOffset = FlagsOffset + sizeof(UInt16);
            EventTypeOffset = NameOffset + stringHeapRefSize;
            RowSize = EventTypeOffset + typeDefOrRefRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal MetadataToken GetEventType(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return TypeDefOrRefTag.ConvertToToken(Table.ReadReference(rowOffset + EventTypeOffset, IsTypeDefOrRefRefSizeSmall));
        }

        internal EventAttributes GetFlags(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (EventAttributes)Table.ReadUInt16(rowOffset + FlagsOffset);
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }
    }

    internal sealed class PropertyMapTable {
        internal const int TableIndex = 0x15;
        internal readonly int NumberOfRows;
        private readonly bool IsTypeDefTableRowRefSizeSmall;
        private readonly bool IsPropertyRefSizeSmall;
        private readonly int ParentOffset;
        private readonly int PropertyListOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal PropertyMapTable(int numberOfRows, int typeDefTableRowRefSize, int propertyRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsTypeDefTableRowRefSizeSmall = typeDefTableRowRefSize == 2;
            IsPropertyRefSizeSmall = propertyRefSize == 2;
            ParentOffset = 0;
            PropertyListOffset = ParentOffset + typeDefTableRowRefSize;
            RowSize = PropertyListOffset + propertyRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal int FindPropertyMapRowIdFor(int typeDefRowId) {
            // TODO: we can sort this if not sorted already
            //  We do a linear scan here because we don't have these tables sorted
            int rowNumber = Table.LinearSearchReference(
                RowSize,
                ParentOffset,
                (uint)typeDefRowId,
                IsTypeDefTableRowRefSizeSmall
            );
            return rowNumber + 1;
        }

        internal uint GetFirstPropertyRid(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + PropertyListOffset, IsPropertyRefSizeSmall);
        }

        internal uint GetParent(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + ParentOffset, IsTypeDefTableRowRefSizeSmall);
        }

        internal uint FindTypeContainingProperty(int propertyDefOrPtrRowId, int propertyTableRowCount) {
            int numOfRows = NumberOfRows;

            int rid = 1 + Table.BinarySearchForSlot(numOfRows, propertyTableRowCount, RowSize, PropertyListOffset, (uint)propertyDefOrPtrRowId, IsPropertyRefSizeSmall);
            if (rid == 0) {
                throw new BadImageFormatException();
            }

            return GetParent(rid);
        }
    }

    internal sealed class PropertyPtrTable {
        internal const int TableIndex = 0x16;
        internal readonly int NumberOfRows;
        private readonly bool IsPropertyTableRowRefSizeSmall;
        private readonly int PropertyOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal PropertyPtrTable(int numberOfRows, int propertyTableRowRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsPropertyTableRowRefSizeSmall = propertyTableRowRefSize == 2;
            PropertyOffset = 0;
            RowSize = PropertyOffset + propertyTableRowRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal MetadataToken GetPropertyFor(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            uint rid = Table.ReadReference(rowOffset + PropertyOffset, IsPropertyTableRowRefSizeSmall);
            return new MetadataToken(MetadataTokenType.Property, rid);
        }
    }

    internal sealed class PropertyTable {
        internal const int TableIndex = 0x17;
        internal readonly int NumberOfRows;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly bool IsBlobHeapRefSizeSmall;
        private readonly int FlagsOffset;
        private readonly int NameOffset;
        private readonly int SignatureOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal PropertyTable(int numberOfRows, int stringHeapRefSize, int blobHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
            FlagsOffset = 0;
            NameOffset = FlagsOffset + sizeof(UInt16);
            SignatureOffset = NameOffset + stringHeapRefSize;
            RowSize = SignatureOffset + blobHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal uint GetSignature(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + SignatureOffset, IsBlobHeapRefSizeSmall);
        }

        internal PropertyAttributes GetFlags(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (PropertyAttributes)Table.ReadUInt16(rowOffset + FlagsOffset);
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }
    }

    internal sealed class MethodSemanticsTable {
        internal const int TableIndex = 0x18;
        internal readonly int NumberOfRows;
        private readonly bool IsMethodTableRowRefSizeSmall;
        private readonly bool IsHasSemanticRefSizeSmall;
        private readonly int SemanticsFlagOffset;
        private readonly int MethodOffset;
        private readonly int AssociationOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal MethodSemanticsTable(int numberOfRows, int methodTableRowRefSize, int hasSemanticRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsMethodTableRowRefSizeSmall = methodTableRowRefSize == 2;
            IsHasSemanticRefSizeSmall = hasSemanticRefSize == 2;
            SemanticsFlagOffset = 0;
            MethodOffset = SemanticsFlagOffset + sizeof(UInt16);
            AssociationOffset = MethodOffset + methodTableRowRefSize;
            RowSize = AssociationOffset + hasSemanticRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal MethodSemanticsFlags GetFlags(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (MethodSemanticsFlags)Table.ReadUInt16(rowOffset + SemanticsFlagOffset);
        }

        internal uint GetMethodRid(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + MethodOffset, IsMethodTableRowRefSizeSmall);
        }

        internal MetadataToken GetAssociation(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return HasSemanticsTag.ConvertToToken(Table.ReadReference(rowOffset + AssociationOffset, IsHasSemanticRefSizeSmall));
        }

        //  returns rowID
        internal int FindSemanticMethodsForEvent(int eventRowId, out int methodCount) {
            uint searchCodedTag = HasSemanticsTag.ConvertEventRowIdToTag(eventRowId);
            return BinarySearchTag(searchCodedTag, out methodCount);
        }

        internal int FindSemanticMethodsForProperty(int propertyRowId, out int methodCount) {
            uint searchCodedTag = HasSemanticsTag.ConvertPropertyRowIdToTag(propertyRowId);
            return BinarySearchTag(searchCodedTag, out methodCount);
        }

        private int BinarySearchTag(uint searchCodedTag, out int methodCount) {
            int foundRowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                AssociationOffset,
                searchCodedTag,
                IsHasSemanticRefSizeSmall
            );

            if (foundRowNumber == -1) {
                methodCount = 0;
                return 0;
            }

            int startRowNumber = foundRowNumber;
            while (
                startRowNumber > 0 && 
                Table.ReadReference((startRowNumber - 1) * RowSize + AssociationOffset, IsHasSemanticRefSizeSmall) == searchCodedTag
            ) {
                startRowNumber--;
            }

            int endRowNumber = foundRowNumber;
            while (
                endRowNumber + 1 < NumberOfRows &&
                Table.ReadReference((endRowNumber + 1) * RowSize + AssociationOffset, IsHasSemanticRefSizeSmall) == searchCodedTag
            ) {
                endRowNumber++;
            }

            methodCount = (ushort)(endRowNumber - startRowNumber + 1);
            return startRowNumber + 1;
        }
    }

    internal sealed class MethodImplTable {
        internal const int TableIndex = 0x19;
        internal readonly int NumberOfRows;
        private readonly bool IsTypeDefTableRowRefSizeSmall;
        // private readonly bool IsMethodDefOrRefRefSizeSmall;
        private readonly int ClassOffset;
        private readonly int MethodBodyOffset;
        private readonly int MethodDeclarationOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal MethodImplTable(int numberOfRows, int typeDefTableRowRefSize, int methodDefOrRefRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsTypeDefTableRowRefSizeSmall = typeDefTableRowRefSize == 2;
            // IsMethodDefOrRefRefSizeSmall = methodDefOrRefRefSize == 2;
            ClassOffset = 0;
            MethodBodyOffset = ClassOffset + typeDefTableRowRefSize;
            MethodDeclarationOffset = MethodBodyOffset + methodDefOrRefRefSize;
            RowSize = MethodDeclarationOffset + methodDefOrRefRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal int FindMethodsImplForClass(int typeDefRowId, out ushort methodImplCount) {
            methodImplCount = 0;
            int foundRowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                ClassOffset,
                (uint)typeDefRowId,
                IsTypeDefTableRowRefSizeSmall
            );

            if (foundRowNumber == -1) {
                return 0;
            }

            int startRowNumber = foundRowNumber;
            while (
                startRowNumber > 0 && 
                Table.ReadReference((startRowNumber - 1) * RowSize + ClassOffset, IsTypeDefTableRowRefSizeSmall) == typeDefRowId
            ) {
                startRowNumber--;
            }

            int endRowNumber = foundRowNumber;
            while (
                endRowNumber + 1 < NumberOfRows && 
                Table.ReadReference((endRowNumber + 1) * RowSize + ClassOffset, IsTypeDefTableRowRefSizeSmall) == typeDefRowId
            ) {
                endRowNumber++;
            }

            methodImplCount = (ushort)(endRowNumber - startRowNumber + 1);
            return startRowNumber + 1;
        }
    }

    internal sealed class ModuleRefTable {
        internal const int TableIndex = 0x1a;
        internal readonly int NumberOfRows;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly int NameOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal ModuleRefTable(int numberOfRows, int stringHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            NameOffset = 0;
            RowSize = NameOffset + stringHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }
    }

    internal sealed class TypeSpecTable {
        internal const int TableIndex = 0x1b;
        internal readonly int NumberOfRows;
        private readonly bool IsBlobHeapRefSizeSmall;
        private readonly int SignatureOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal TypeSpecTable(int numberOfRows, int blobHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
            SignatureOffset = 0;
            RowSize = SignatureOffset + blobHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal uint GetSignature(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + SignatureOffset, IsBlobHeapRefSizeSmall);
        }
    }

    internal sealed class ImplMapTable {
        internal const int TableIndex = 0x1c;
        internal readonly int NumberOfRows;
        // private readonly bool IsModuleRefTableRowRefSizeSmall;
        private readonly bool IsMemberForwardRowRefSizeSmall;
        // private readonly bool IsStringHeapRefSizeSmall;
        private readonly int FlagsOffset;
        private readonly int MemberForwardedOffset;
        private readonly int ImportNameOffset;
        private readonly int ImportScopeOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal ImplMapTable(int numberOfRows, int moduleRefTableRowRefSize, int memberForwardedRefSize, int stringHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            // IsModuleRefTableRowRefSizeSmall = moduleRefTableRowRefSize == 2;
            IsMemberForwardRowRefSizeSmall = memberForwardedRefSize == 2;
            // IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            FlagsOffset = 0;
            MemberForwardedOffset = FlagsOffset + sizeof(UInt16);
            ImportNameOffset = MemberForwardedOffset + memberForwardedRefSize;
            ImportScopeOffset = ImportNameOffset + stringHeapRefSize;
            RowSize = ImportScopeOffset + moduleRefTableRowRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal int FindImplForMethod(int methodRowId) {
            return BinarySearchTag(MemberForwardedTag.ConvertMethodDefRowIdToTag(methodRowId));
        }

        private int BinarySearchTag(uint searchCodedTag) {
            int foundRowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                MemberForwardedOffset,
                searchCodedTag,
                IsMemberForwardRowRefSizeSmall
            );
            return foundRowNumber + 1;
        }
    }

    internal sealed class FieldRVATable {
        internal const int TableIndex = 0x1d;
        internal readonly int NumberOfRows;
        private readonly bool IsFieldTableRowRefSizeSmall;
        private readonly int RVAOffset;
        private readonly int FieldOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal FieldRVATable(int numberOfRows, int fieldTableRowRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsFieldTableRowRefSizeSmall = fieldTableRowRefSize == 2;
            RVAOffset = 0;
            FieldOffset = RVAOffset + sizeof(UInt32);
            RowSize = FieldOffset + fieldTableRowRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal uint GetFieldRVA(int fieldDefRowId) {
            int foundRowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                FieldOffset,
                (uint)fieldDefRowId,
                IsFieldTableRowRefSizeSmall
            );

            if (foundRowNumber == -1) {
                return 0;
            }

            int rowOffset = foundRowNumber * RowSize;
            return Table.ReadUInt32(rowOffset + RVAOffset);
        }
    }

    internal sealed class EnCLogTable {
        internal const int TableIndex = 0x1e;
        internal readonly int NumberOfRows;
        private readonly int TokenOffset;
        private readonly int FuncCodeOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal EnCLogTable(int numberOfRows, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            TokenOffset = 0;
            FuncCodeOffset = TokenOffset + sizeof(UInt32);
            RowSize = FuncCodeOffset + sizeof(UInt32);
            Table = block.GetRange(start, RowSize * numberOfRows);
        }
    }

    internal sealed class EnCMapTable {
        internal const int TableIndex = 0x1f;
        internal readonly int NumberOfRows;
        private readonly int TokenOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal EnCMapTable(int numberOfRows, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            TokenOffset = 0;
            RowSize = TokenOffset + sizeof(UInt32);
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

    }

    internal sealed class AssemblyTable {
        internal const int TableIndex = 0x20;
        internal readonly int NumberOfRows;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly bool IsBlobHeapRefSizeSmall;
        private readonly int HashAlgIdOffset;
        private readonly int MajorVersionOffset;
        private readonly int MinorVersionOffset;
        private readonly int BuildNumberOffset;
        private readonly int RevisionNumberOffset;
        private readonly int FlagsOffset;
        private readonly int PublicKeyOffset;
        private readonly int NameOffset;
        private readonly int CultureOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal AssemblyTable(int numberOfRows, int stringHeapRefSize, int blobHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
            HashAlgIdOffset = 0;
            MajorVersionOffset = HashAlgIdOffset + sizeof(UInt32);
            MinorVersionOffset = MajorVersionOffset + sizeof(UInt16);
            BuildNumberOffset = MinorVersionOffset + sizeof(UInt16);
            RevisionNumberOffset = BuildNumberOffset + sizeof(UInt16);
            FlagsOffset = RevisionNumberOffset + sizeof(UInt16);
            PublicKeyOffset = FlagsOffset + sizeof(UInt32);
            NameOffset = PublicKeyOffset + blobHeapRefSize;
            CultureOffset = NameOffset + stringHeapRefSize;
            RowSize = CultureOffset + stringHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal AssemblyHashAlgorithm GetHashAlgorithm(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (AssemblyHashAlgorithm)Table.ReadUInt32(rowOffset + HashAlgIdOffset);
        }

        internal AssemblyNameFlags GetFlags(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (AssemblyNameFlags)Table.ReadUInt32(rowOffset + FlagsOffset);
        }

        internal Version GetVersion(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return new Version(
                Table.ReadUInt16(rowOffset + MajorVersionOffset),
                Table.ReadUInt16(rowOffset + MinorVersionOffset),
                Table.ReadUInt16(rowOffset + BuildNumberOffset),
                Table.ReadUInt16(rowOffset + RevisionNumberOffset)
            );
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }

        internal uint GetCulture(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + CultureOffset, IsStringHeapRefSizeSmall);
        }

        internal uint GetPublicKey(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + PublicKeyOffset, IsBlobHeapRefSizeSmall);
        }
    }

    internal sealed class AssemblyProcessorTable {
        internal const int TableIndex = 0x21;
        internal readonly int NumberOfRows;
        private readonly int ProcessorOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal AssemblyProcessorTable(int numberOfRows, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            ProcessorOffset = 0;
            RowSize = ProcessorOffset + sizeof(UInt32);
            Table = block.GetRange(start, RowSize * numberOfRows);
        }
    }

    internal sealed class AssemblyOSTable {
        internal const int TableIndex = 0x22;
        internal readonly int NumberOfRows;
        private readonly int OSPlatformIdOffset;
        private readonly int OSMajorVersionIdOffset;
        private readonly int OSMinorVersionIdOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal AssemblyOSTable(int numberOfRows, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            OSPlatformIdOffset = 0;
            OSMajorVersionIdOffset = OSPlatformIdOffset + sizeof(UInt32);
            OSMinorVersionIdOffset = OSMajorVersionIdOffset + sizeof(UInt32);
            RowSize = OSMinorVersionIdOffset + sizeof(UInt32);
            Table = block.GetRange(start, RowSize * numberOfRows);
        }
    }

    internal sealed class AssemblyRefTable {
        internal const int TableIndex = 0x23;
        internal readonly int NumberOfRows;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly bool IsBlobHeapRefSizeSmall;
        private readonly int MajorVersionOffset;
        private readonly int MinorVersionOffset;
        private readonly int BuildNumberOffset;
        private readonly int RevisionNumberOffset;
        private readonly int FlagsOffset;
        private readonly int PublicKeyOrTokenOffset;
        private readonly int NameOffset;
        private readonly int CultureOffset;
        private readonly int HashValueOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal AssemblyRefTable(int numberOfRows, int stringHeapRefSize, int blobHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
            MajorVersionOffset = 0;
            MinorVersionOffset = MajorVersionOffset + sizeof(UInt16);
            BuildNumberOffset = MinorVersionOffset + sizeof(UInt16);
            RevisionNumberOffset = BuildNumberOffset + sizeof(UInt16);
            FlagsOffset = RevisionNumberOffset + sizeof(UInt16);
            PublicKeyOrTokenOffset = FlagsOffset + sizeof(UInt32);
            NameOffset = PublicKeyOrTokenOffset + blobHeapRefSize;
            CultureOffset = NameOffset + stringHeapRefSize;
            HashValueOffset = CultureOffset + stringHeapRefSize;
            RowSize = HashValueOffset + blobHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal Version GetVersion(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return new Version(
                Table.ReadUInt16(rowOffset + MajorVersionOffset),
                Table.ReadUInt16(rowOffset + MinorVersionOffset),
                Table.ReadUInt16(rowOffset + BuildNumberOffset),
                Table.ReadUInt16(rowOffset + RevisionNumberOffset)
            );
        }

        internal AssemblyNameFlags GetFlags(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (AssemblyNameFlags)Table.ReadUInt32(rowOffset + FlagsOffset);
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }

        internal uint GetCulture(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + CultureOffset, IsStringHeapRefSizeSmall);
        }

        internal uint GetPublicKeyOrToken(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + PublicKeyOrTokenOffset, IsBlobHeapRefSizeSmall);
        }

        internal uint GetHashValue(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + HashValueOffset, IsBlobHeapRefSizeSmall);
        }
    }

    internal sealed class AssemblyRefProcessorTable {
        internal const int TableIndex = 0x24;
        internal readonly int NumberOfRows;
        // private readonly bool IsAssemblyRefTableRowSizeSmall;
        private readonly int ProcessorOffset;
        private readonly int AssemblyRefOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal AssemblyRefProcessorTable(int numberOfRows, int assembyRefTableRowRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            // IsAssemblyRefTableRowSizeSmall = assembyRefTableRowRefSize == 2;
            ProcessorOffset = 0;
            AssemblyRefOffset = ProcessorOffset + sizeof(UInt32);
            RowSize = AssemblyRefOffset + assembyRefTableRowRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }
    }

    internal sealed class AssemblyRefOSTable {
        internal const int TableIndex = 0x25;
        internal readonly int NumberOfRows;
        // private readonly bool IsAssemblyRefTableRowRefSizeSmall;
        private readonly int OSPlatformIdOffset;
        private readonly int OSMajorVersionIdOffset;
        private readonly int OSMinorVersionIdOffset;
        private readonly int AssemblyRefOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal AssemblyRefOSTable(int numberOfRows, int assembyRefTableRowRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            // IsAssemblyRefTableRowRefSizeSmall = assembyRefTableRowRefSize == 2;
            OSPlatformIdOffset = 0;
            OSMajorVersionIdOffset = OSPlatformIdOffset + sizeof(UInt32);
            OSMinorVersionIdOffset = OSMajorVersionIdOffset + sizeof(UInt32);
            AssemblyRefOffset = OSMinorVersionIdOffset + sizeof(UInt32);
            RowSize = AssemblyRefOffset + assembyRefTableRowRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }
    }

    internal sealed class FileTable {
        internal const int TableIndex = 0x26;
        internal readonly int NumberOfRows;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly bool IsBlobHeapRefSizeSmall;
        private readonly int FlagsOffset;
        private readonly int NameOffset;
        private readonly int HashValueOffset;
        private readonly int RowSize;
        public readonly MemoryBlock Table;

        internal FileTable(int numberOfRows, int stringHeapRefSize, int blobHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
            FlagsOffset = 0;
            NameOffset = FlagsOffset + sizeof(UInt32);
            HashValueOffset = NameOffset + stringHeapRefSize;
            RowSize = HashValueOffset + blobHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal uint GetHashValue(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + HashValueOffset, IsBlobHeapRefSizeSmall);
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }

        internal AssemblyFileAttributes GetFlags(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (AssemblyFileAttributes)Table.ReadUInt32(rowOffset + FlagsOffset);
        }
    }

    internal sealed class ExportedTypeTable {
        internal const int TableIndex = 0x27;
        internal readonly int NumberOfRows;
        private readonly bool IsImplementationRefSizeSmall;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly int FlagsOffset;
        private readonly int TypeDefIdOffset;
        private readonly int TypeNameOffset;
        private readonly int TypeNamespaceOffset;
        private readonly int ImplementationOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal ExportedTypeTable(int numberOfRows, int implementationRefSize, int stringHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsImplementationRefSizeSmall = implementationRefSize == 2;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            FlagsOffset = 0;
            TypeDefIdOffset = FlagsOffset + sizeof(UInt32);
            TypeNameOffset = TypeDefIdOffset + sizeof(UInt32);
            TypeNamespaceOffset = TypeNameOffset + stringHeapRefSize;
            ImplementationOffset = TypeNamespaceOffset + stringHeapRefSize;
            RowSize = ImplementationOffset + implementationRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal uint GetNamespace(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + TypeNamespaceOffset, IsStringHeapRefSizeSmall);
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + TypeNameOffset, IsStringHeapRefSizeSmall);
        }

        internal TypeAttributes GetFlags(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (TypeAttributes)Table.ReadUInt32(rowOffset + FlagsOffset);
        }

        internal MetadataToken GetImplementation(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return ImplementationTag.ConvertToToken(Table.ReadReference(rowOffset + ImplementationOffset, IsImplementationRefSizeSmall));
        }
    }

    internal sealed class ManifestResourceTable {
        internal const int TableIndex = 0x28;
        internal readonly int NumberOfRows;
        private readonly bool IsImplementationRefSizeSmall;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly int OffsetOffset;
        private readonly int FlagsOffset;
        private readonly int NameOffset;
        private readonly int ImplementationOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal ManifestResourceTable(int numberOfRows, int implementationRefSize, int stringHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsImplementationRefSizeSmall = implementationRefSize == 2;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            OffsetOffset = 0;
            FlagsOffset = OffsetOffset + sizeof(UInt32);
            NameOffset = FlagsOffset + sizeof(UInt32);
            ImplementationOffset = NameOffset + stringHeapRefSize;
            RowSize = ImplementationOffset + implementationRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal MetadataToken GetImplementation(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return ImplementationTag.ConvertToToken(Table.ReadReference(rowOffset + ImplementationOffset, IsImplementationRefSizeSmall));
        }

        internal ManifestResourceAttributes GetFlags(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (ManifestResourceAttributes)Table.ReadUInt32(rowOffset + FlagsOffset);
        }

        internal uint GetOffset(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadUInt32(rowOffset + OffsetOffset);
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return this.Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }
    }

    internal sealed class NestedClassTable {
        internal const int TableIndex = 0x29;
        internal readonly int NumberOfRows;
        private readonly bool IsTypeDefTableRowRefSizeSmall;
        private readonly int NestedClassOffset;
        private readonly int EnclosingClassOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal NestedClassTable(int numberOfRows, int typeDefTableRowRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsTypeDefTableRowRefSizeSmall = typeDefTableRowRefSize == 2;
            NestedClassOffset = 0;
            EnclosingClassOffset = NestedClassOffset + typeDefTableRowRefSize;
            RowSize = EnclosingClassOffset + typeDefTableRowRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal MetadataToken GetNestedType(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            uint rid = Table.ReadReference(rowOffset + NestedClassOffset, IsTypeDefTableRowRefSizeSmall);
            return new MetadataToken(MetadataTokenType.TypeDef, rid);
        }

        internal MetadataToken GetEnclosingType(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            uint rid = Table.ReadReference(rowOffset + EnclosingClassOffset, IsTypeDefTableRowRefSizeSmall);
            return new MetadataToken(MetadataTokenType.TypeDef, rid);
        }

        internal uint FindParentTypeDefRowId(int nestedTypeRowId) {
            int rowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                NestedClassOffset,
                (uint)nestedTypeRowId,
                IsTypeDefTableRowRefSizeSmall
            );

            if (rowNumber == -1) {
                return 0;
            }

            return Table.ReadReference(rowNumber * RowSize + EnclosingClassOffset, IsTypeDefTableRowRefSizeSmall);
        }
    }

    internal sealed class GenericParamTable {
        internal const int TableIndex = 0x2a;
        internal readonly int NumberOfRows;
        private readonly bool IsTypeOrMethodDefRefSizeSmall;
        private readonly bool IsStringHeapRefSizeSmall;
        private readonly int NumberOffset;
        private readonly int FlagsOffset;
        private readonly int OwnerOffset;
        private readonly int NameOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal GenericParamTable(int numberOfRows, int typeOrMethodDefRefSize, int stringHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsTypeOrMethodDefRefSizeSmall = typeOrMethodDefRefSize == 2;
            IsStringHeapRefSizeSmall = stringHeapRefSize == 2;
            NumberOffset = 0;
            FlagsOffset = NumberOffset + sizeof(UInt16);
            OwnerOffset = FlagsOffset + sizeof(UInt16);
            NameOffset = OwnerOffset + typeOrMethodDefRefSize;
            RowSize = NameOffset + stringHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal int GetIndex(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadUInt16(rowOffset + NumberOffset);
        }

        internal GenericParameterAttributes GetFlags(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return (GenericParameterAttributes)Table.ReadUInt16(rowOffset + FlagsOffset);
        }

        internal MetadataToken GetOwner(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return TypeOrMethodDefTag.ConvertToToken(Table.ReadReference(rowOffset + OwnerOffset, IsTypeOrMethodDefRefSizeSmall));
        }

        internal uint GetName(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + NameOffset, IsStringHeapRefSizeSmall);
        }

        //  returns rowID
        internal int FindGenericParametersForType(int typeDefRowId, out int genericParamCount) {
            uint searchCodedTag = TypeOrMethodDefTag.ConvertTypeDefRowIdToTag(typeDefRowId);
            return BinarySearchTag(searchCodedTag, out genericParamCount);
        }

        internal int FindGenericParametersForMethod(int typeDefRowId, out int genericParamCount) {
            uint searchCodedTag = TypeOrMethodDefTag.ConvertMethodDefRowIdToTag(typeDefRowId);
            return BinarySearchTag(searchCodedTag, out genericParamCount);
        }

        private int BinarySearchTag(uint searchCodedTag, out int genericParamCount) {
            int foundRowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                OwnerOffset,
                searchCodedTag,
                IsTypeOrMethodDefRefSizeSmall
            );

            if (foundRowNumber == -1) {
                genericParamCount = 0;
                return 0;
            }

            int startRowNumber = foundRowNumber;
            while (
                startRowNumber > 0 && 
                Table.ReadReference((startRowNumber - 1) * RowSize + OwnerOffset, IsTypeOrMethodDefRefSizeSmall) == searchCodedTag
            ) {
                startRowNumber--;
            }

            int endRowNumber = foundRowNumber;
            while (
                endRowNumber + 1 < NumberOfRows && 
                Table.ReadReference((endRowNumber + 1) * RowSize + OwnerOffset, IsTypeOrMethodDefRefSizeSmall) == searchCodedTag
            ) {
                endRowNumber++;
            }

            genericParamCount = endRowNumber - startRowNumber + 1;
            return startRowNumber + 1;
        }
    }

    internal sealed class MethodSpecTable {
        internal const int TableIndex = 0x2b;
        internal readonly int NumberOfRows;
        private readonly bool IsMethodDefOrRefRefSizeSmall;
        private readonly bool IsBlobHeapRefSizeSmall;
        private readonly int MethodOffset;
        private readonly int InstantiationOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal MethodSpecTable(int numberOfRows, int methodDefOrRefRefSize, int blobHeapRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsMethodDefOrRefRefSizeSmall = methodDefOrRefRefSize == 2;
            IsBlobHeapRefSizeSmall = blobHeapRefSize == 2;
            MethodOffset = 0;
            InstantiationOffset = MethodOffset + methodDefOrRefRefSize;
            RowSize = InstantiationOffset + blobHeapRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal MetadataToken GetGenericMethod(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return MethodDefOrRefTag.ConvertToToken(Table.ReadReference(rowOffset + MethodOffset, IsMethodDefOrRefRefSizeSmall));
        }

        internal uint GetSignature(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return Table.ReadReference(rowOffset + InstantiationOffset, IsBlobHeapRefSizeSmall);
        }
    }

    internal sealed class GenericParamConstraintTable {
        internal const int TableIndex = 0x2c;
        internal readonly int NumberOfRows;
        private readonly bool IsGenericParamTableRowRefSizeSmall;
        private readonly bool IsTypeDefOrRefRefSizeSmall;
        private readonly int OwnerOffset;
        private readonly int ConstraintOffset;
        private readonly int RowSize;
        internal readonly MemoryBlock Table;

        internal GenericParamConstraintTable(int numberOfRows, int genericParamTableRowRefSize, int typeDefOrRefRefSize, int start, MemoryBlock block) {
            NumberOfRows = numberOfRows;
            IsGenericParamTableRowRefSizeSmall = genericParamTableRowRefSize == 2;
            IsTypeDefOrRefRefSizeSmall = typeDefOrRefRefSize == 2;
            OwnerOffset = 0;
            ConstraintOffset = OwnerOffset + genericParamTableRowRefSize;
            RowSize = ConstraintOffset + typeDefOrRefRefSize;
            Table = block.GetRange(start, RowSize * numberOfRows);
        }

        internal MetadataToken GetConstraint(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            return TypeDefOrRefTag.ConvertToToken(Table.ReadReference(rowOffset + ConstraintOffset, IsTypeDefOrRefRefSizeSmall));
        }

        internal MetadataToken GetOwner(int rowId) {
            int rowOffset = (rowId - 1) * RowSize;
            uint rid = Table.ReadReference(rowOffset + OwnerOffset, IsGenericParamTableRowRefSizeSmall);
            return new MetadataToken(MetadataTokenType.GenericPar, rid);
        }

        internal int FindConstraintForGenericParam(int genericParamRowId, out int genericParamConstraintCount) {
            int foundRowNumber = Table.BinarySearchReference(
                NumberOfRows,
                RowSize,
                OwnerOffset,
                (uint)genericParamRowId,
                IsGenericParamTableRowRefSizeSmall
            );

            if (foundRowNumber == -1) {
                genericParamConstraintCount = 0;
                return 0;
            }

            int startRowNumber = foundRowNumber;
            while (
                startRowNumber > 0 && 
                Table.ReadReference((startRowNumber - 1) * RowSize + OwnerOffset, IsGenericParamTableRowRefSizeSmall) == genericParamRowId
            ) {
                startRowNumber--;
            }

            int endRowNumber = foundRowNumber;
            while (
                endRowNumber + 1 < NumberOfRows && 
                Table.ReadReference((endRowNumber + 1) * RowSize + OwnerOffset, IsGenericParamTableRowRefSizeSmall) == genericParamRowId
            ) {
                endRowNumber++;
            }

            genericParamConstraintCount = endRowNumber - startRowNumber + 1;
            return startRowNumber + 1;
        }
    }
}
