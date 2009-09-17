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
    /// <summary>
    /// Provides an interned representation of a string which supports both case sensitive and case insensitive
    /// lookups.
    /// 
    /// By default all lookups are case sensitive.  Case insensitive lookups can be performed by first creating
    /// a normal SymbolId for a given string and then accessing the CaseInsensitiveIdentifier property.  Using
    /// the case insensitive identifier during a lookup will cause the lookup to be case insensitive.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes"), Serializable]
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
            get { return new SymbolId(_id & ~SymbolTable.CaseVersionMask); }
        }

        public int CaseInsensitiveId {
            get { return _id & ~SymbolTable.CaseVersionMask; }
        }

        public bool IsCaseInsensitive {
            get {
                return (_id & SymbolTable.CaseVersionMask) == 0;
            }
        }

        [Confined]
        public override bool Equals(object obj) {
            if (!(obj is SymbolId)) return false;
            return Equals((SymbolId)obj);
        }

        [StateIndependent]
        public bool Equals(SymbolId other) {

            if (_id == other._id) {
                return true;
            } else if (IsCaseInsensitive || other.IsCaseInsensitive) {
                return (_id & ~SymbolTable.CaseVersionMask) == (other._id & ~SymbolTable.CaseVersionMask);
            }
            return false;
        }

        [Confined]
        public override int GetHashCode() {
            return _id & ~SymbolTable.CaseVersionMask;
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
            return a.Equals(b);
        }

        public static bool operator !=(SymbolId a, SymbolId b) {
            return !a.Equals(b);
        }

        public static bool operator <(SymbolId a, SymbolId b) {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >(SymbolId a, SymbolId b) {
            return a.CompareTo(b) > 0;
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
