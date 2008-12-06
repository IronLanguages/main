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
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Contracts;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting {
    [Serializable]
    public struct SymbolId : ISerializable, IComparable, IComparable<SymbolId>, IEquatable<SymbolId> {
        private readonly int _id;

        public SymbolId(int value) {
            _id = value;
        }

        public SymbolId(SymbolId value) {
            _id = value._id;
        }

        public int Id {
            get { return _id; }
        }

        public SymbolId CaseInsensitiveIdentifier {
            get { return new SymbolId(_id & 0x00FFFFFF); }
        }

        public int CaseInsensitiveId {
            get { return _id & 0x00FFFFFF; }
        }

        [Confined]
        public override bool Equals(object obj) {
            if (!(obj is SymbolId)) return false;
            SymbolId other = (SymbolId)obj;
            return _id == other._id;
        }

        [StateIndependent]
        public bool Equals(SymbolId other) {
            return _id == other._id;
        }

        [Confined]
        public bool CaseInsensitiveEquals(SymbolId other) {
            return (_id & 0x00FFFFFF) == (other._id & 0x00FFFFFF);
        }

        [Confined]
        public override int GetHashCode() {
            return _id;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [Confined]
        public int GetCaseInsensitiveHashCode() {
            return (_id & 0x00FFFFFF);
        }

        /// <summary>
        /// Override of ToString.
        /// DO NOT USE THIS METHOD TO RETRIEVE STRING THAT THE SYMBOL REPRESENTS
        /// Use SymbolTable.IdToString(SymbolId) instead.
        /// </summary>
        [Confined]
        public override string ToString() {
            return SymbolTable.IdToString(this);
        }

        public static explicit operator SymbolId(string s) {
            return SymbolTable.StringToId(s);
        }

        public static bool operator ==(SymbolId a, SymbolId b) {
            return a._id == b._id;
        }

        public static bool operator !=(SymbolId a, SymbolId b) {
            return a._id != b._id;
        }

        public static bool operator <(SymbolId a, SymbolId b) {
            return a._id < b._id;
        }

        public static bool operator >(SymbolId a, SymbolId b) {
            return a._id > b._id;
        }

        #region IComparable Members

        public int CompareTo(object obj) {
            if (!(obj is SymbolId)) {
                return -1;
            }

            return CompareTo((SymbolId)obj);
        }

        public int CompareTo(SymbolId other) {
            // Note that we could just compare _id which will result in a faster comparison. However, that will
            // mean that sorting will depend on the order in which the symbols were interned. This will often
            // not be expected. Hence, we just compare the symbol strings

            string thisString = SymbolTable.IdToString(this);
            string otherString = SymbolTable.IdToString(other);
            return thisString.CompareTo(otherString);
        }

        #endregion

        #region Cross-Domain/Process Serialization Support

#if !SILVERLIGHT // Security, SerializationInfo, StreamingContext
        // When leaving a context we serialize out our ID as a name
        // rather than a raw ID.  When we enter a new context we 
        // consult it's FieldTable to get the ID of the symbol name in
        // the new context.

        private SymbolId(SerializationInfo info, StreamingContext context) {
            ContractUtils.RequiresNotNull(info, "info");

            _id = SymbolTable.StringToId(info.GetString("symbolName"))._id;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            ContractUtils.RequiresNotNull(info, "info");

            info.AddValue("symbolName", SymbolTable.IdToString(this));
        }
#endif

        #endregion

        public const int EmptyId = 0;
        /// <summary>SymbolId for null string</summary>
        public static readonly SymbolId Empty = new SymbolId(EmptyId);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly SymbolId[] EmptySymbols = new SymbolId[0];

        public bool IsEmpty {
            get { return _id == EmptyId; }
        }

        public const int InvalidId = -1;
        /// <summary>SymbolId to represent invalid value</summary>
        public static readonly SymbolId Invalid = new SymbolId(InvalidId);

        public bool IsInvalid {
            get { return _id == InvalidId; }
        }
    }
}
