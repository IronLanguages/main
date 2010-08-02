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
using System.Diagnostics.Contracts;
using System.Security;
using System.Text;

namespace Microsoft.Scripting.Metadata {
    /// <summary>
    /// Zero terminated, UTF8 encoded sequence of bytes representing a name in metadata (a type name, a member name, etc).
    /// The name is bound to the module it was retrieved from. The module is kept alive until all its metadata names are collected.
    /// Doesn't cache hashcode, byte or character count.
    /// </summary>
    public unsafe struct MetadataName : IEquatable<MetadataName>, IEquatable<MetadataNamePart> {
        internal readonly byte* m_data;
        internal readonly object m_keepAlive;

        public static readonly MetadataName Empty = default(MetadataName);

        internal MetadataName(byte* data, object keepAlive) {
            m_data = data;
            m_keepAlive = keepAlive;
        }

        public bool IsEmpty {
            get {
                bool result = m_data == null || *m_data == 0;
                GC.KeepAlive(m_keepAlive);
                return result;
            }
        }

        // SECURITY: The method is actually not safe. We must make sure that this object is not leaked to partially-trusted code.
        [SecuritySafeCritical]
        public override bool Equals(object obj) {
            return obj is MetadataName && Equals((MetadataName)obj)
                || obj is MetadataNamePart && Equals((MetadataNamePart)obj);
        }

        // SECURITY: The method is actually not safe. We must make sure that this object is not leaked to partially-trusted code.
        [SecuritySafeCritical]
        public bool Equals(MetadataName other) {
            bool result = Equals(m_data, other.m_data);
            GC.KeepAlive(m_keepAlive);
            GC.KeepAlive(other.m_keepAlive);
            return result;
        }

        // SECURITY: The method is actually not safe. We must make sure that this object is not leaked to partially-trusted code.
        [SecuritySafeCritical]
        public bool Equals(MetadataNamePart other) {
            return other.Equals(this);
        }

        public static bool operator ==(MetadataName self, MetadataNamePart other) {
            return self.Equals(other);
        }

        public static bool operator ==(MetadataName self, MetadataName other) {
            return self.Equals(other);
        }

        public static bool operator !=(MetadataName self, MetadataNamePart other) {
            return self.Equals(other);
        }

        public static bool operator !=(MetadataName self, MetadataName other) {
            return self.Equals(other);
        }

        // safe
        public bool Equals(byte[] bytes, int start, int count) {
            if (bytes == null) {
                throw new ArgumentNullException("bytes");
            }
            if (start < 0) {
                throw new ArgumentOutOfRangeException("start");
            }
            if (count < 0 || count > bytes.Length - start) {
                throw new ArgumentOutOfRangeException("count");
            }

            bool result;
            fixed (byte* ptr = &bytes[start]) {
                result = Equals(m_data, ptr, count);
            }
            GC.KeepAlive(m_keepAlive);
            return result;
        }

        // SECURITY: The method is actually not safe. We must make sure that this object is not leaked to partially-trusted code.
        [SecuritySafeCritical]
        public override string ToString() {
            if (m_data == null) {
                return string.Empty;
            }

            int byteCount = GetLength();
            int charCount = Encoding.UTF8.GetCharCount(m_data, byteCount, null);
#if CCI
            string result = new string('\0', charCount);
#else
            string result = String.FastAllocateString(charCount);
#endif
            fixed (char* ptr = result) {
                Encoding.UTF8.GetChars(m_data, byteCount, ptr, charCount, null);
            }
            GC.KeepAlive(m_keepAlive);
            return result;
        }

        // safe
        internal string ToString(int byteCount) {
            if (m_data == null) {
                Contract.Assert(byteCount == 0);
                return string.Empty;
            }

            // TODO: use FastAllocateString (if we know character length we can optimize):
            string result = new string((sbyte*)m_data, 0, byteCount, Encoding.UTF8);
            GC.KeepAlive(m_keepAlive);
            return result;
        }

        // SECURITY: The method is actually not safe. We must make sure that this object is not leaked to partially-trusted code.
        [SecuritySafeCritical]
        public override int GetHashCode() {
            int result = GetByteHashCode(m_data);
            GC.KeepAlive(m_keepAlive);
            return result;
        }

        // safe
        internal int GetHashCode(int start, int count) {
            int result = GetByteHashCode((m_data != null) ? m_data + start : null, count);
            GC.KeepAlive(m_keepAlive);
            return result;
        }

        // safe
        public int GetLength() {
            int result = GetLength(m_data);
            GC.KeepAlive(m_keepAlive);
            return result;
        }

        public MetadataNamePart GetExtent() {
            return new MetadataNamePart(this, GetLength());
        }

        // safe
        internal MetadataName GetSuffix(int start) {
            return (m_data != null) ? new MetadataName(m_data + start, m_keepAlive) : Empty;
        }

        // safe
        internal int IndexOf(byte b) {
            int result = IndexOf(m_data, b);
            GC.KeepAlive(m_keepAlive);
            return result;
        }

        // safe
        internal int IndexOf(byte b, int start, int count) {
            int result = IndexOf(m_data, b, start, count);
            GC.KeepAlive(m_keepAlive);
            return result;
        }

        // safe
        internal int LastIndexOf(byte b, int start, int count) {
            if (m_data == null) return -1;
            byte* ptr = FindPrevious(m_data + start, m_data + start - count + 1, b);
            GC.KeepAlive(m_keepAlive);
            return ptr != null ? (int)(ptr - m_data) : -1;
        }

        internal static int GetLength(byte* bytes) {
            if (bytes == null) {
                return 0;
            }
            byte* ptr = bytes;
            while (*ptr != 0) {
                ptr++;
            }
            return (int)(ptr - bytes);
        }

        internal static int IndexOf(byte* bytes, byte b) {
            if (bytes == null) {
                return -1;
            }
            byte* ptr = bytes;
            while (*ptr != 0 && *ptr != b) {
                ptr++;
            }
            if (*ptr == 0) {
                return -1;
            }
            return (int)(ptr - bytes);
        }

        internal static int IndexOf(byte* bytes, byte b, int start, int count) {
            if (bytes == null) {
                return -1;
            }

            // TODO: return Buffer.IndexOfByte(m_data, b, start, count);
            byte* ptr = bytes + start;
            byte* endPtr = bytes + start + count;
            while (ptr < endPtr) {
                if (*ptr == b) {
                    return (int)(ptr - bytes);
                }
                ptr++;
            }
            return -1;
        }
        
        internal static byte* FindPrevious(byte* start, byte* last, byte b) {
            Contract.Assert(start != null && last != null);
            byte* ptr = start - 1;
            while (true) {
                if (ptr < last) {
                    return null;
                }
                if (*ptr == b) {
                    return ptr;
                }
                ptr--;
            }
        }

        internal static int GetByteHashCode(byte* bytes) {
            int hash1 = 5381;
            int hash2 = hash1;

            if (bytes != null) {
                int c;
                byte* s = bytes;
                while ((c = s[0]) != 0) {
                    hash1 = ((hash1 << 5) + hash1) ^ c;
                    c = s[1];
                    if (c == 0) {
                        break;
                    }
                    hash2 = ((hash2 << 5) + hash2) ^ c;
                    s += 2;
                }
            }
            return hash1 + (hash2 * 1566083941);
        }

        internal static int GetByteHashCode(byte* bytes, int count) {
            Contract.Assert(bytes != null || count == 0);

            int hash1 = 5381;
            int hash2 = hash1;

            if (bytes != null) {
                byte* last = bytes + count - 1;
                byte* s = bytes;
                while (s < last) {
                    hash1 = ((hash1 << 5) + hash1) ^ (int)s[0];
                    hash2 = ((hash2 << 5) + hash2) ^ (int)s[1];
                    s += 2;
                }
                if (s < bytes + count) {
                    hash1 = ((hash1 << 5) + hash1) ^ (int)s[0];
                }
            }
            return hash1 + (hash2 * 1566083941);
        }

        internal static bool Equals(byte* p, byte* q) {
            if (p == q) {
                return true;
            }
            if (p == null) {
                return *q == 0;
            }
            if (q == null) {
                return *p == 0;
            }

            while (true) {
                if (*p != *q) {
                    return false;
                }
                if (*p == 0) {
                    return true;
                }
                p++;
                q++;
            }
        }

        internal static bool Equals(byte* p, byte* q, int qCount) {
            if (p == null) {
                return qCount == 0;
            }

            byte* e = q + qCount;
            while (true) {
                if (*p == 0) {
                    return q == e;
                }
                if (q == e || *p != *q) {
                    return false;
                }
                p++;
                q++;
            }
        }

        internal static bool Equals(byte* p, int pCount, byte* q, int qCount) {
            if (pCount != qCount) {
                return false;
            }
            if (p == q || pCount == 0) {
                return true;
            }

            byte* e = p + pCount;
            while (p < e) {
                if (*p != *q) {
                    return false;
                }
                p++;
                q++;
            }
            return true;
        }
    }

    public struct MetadataNamePart : IEquatable<MetadataNamePart>, IEquatable<MetadataName> {
        private readonly MetadataName m_name;
        private readonly int m_byteCount;

        public static readonly MetadataNamePart Empty = default(MetadataNamePart);

        internal MetadataNamePart(MetadataName name, int byteCount) {
            m_name = name;
            m_byteCount = byteCount;
        }

        public int Length {
            get { return m_byteCount; }
        }

        public int IndexOf(byte b) {
            return m_name.IndexOf(b, 0, m_byteCount);
        }

        public int IndexOf(byte b, int start, int count) {
            if (start < 0) {
                throw new ArgumentOutOfRangeException("start");
            }
            if (count < 0 || count > Length - start) {
                throw new ArgumentOutOfRangeException("count");
            }
            return m_name.IndexOf(b, start, count);
        }

        public int LastIndexOf(byte b, int start, int count) {
            if (start < 0 || start > Length) {
                throw new ArgumentOutOfRangeException("start");
            }
            if (count < 0 || start < count - 1) {
                throw new ArgumentOutOfRangeException("count");
            }
            return m_name.LastIndexOf(b, start, count);
        }

        public unsafe MetadataNamePart GetPart(int start) {
            if (start < 0 || start > Length) {
                throw new ArgumentOutOfRangeException("start");
            }
            return new MetadataNamePart(m_name.GetSuffix(start), m_byteCount - start);
        }

        public unsafe MetadataNamePart GetPart(int start, int count) {
            if (start < 0) {
                throw new ArgumentOutOfRangeException("start");
            }
            if (count < 0 || count > Length - start) {
                throw new ArgumentOutOfRangeException("count");
            }
            return new MetadataNamePart(m_name.GetSuffix(start), count);
        }

        // SECURITY: The method is actually not safe. We must make sure that this object is not leaked to partially-trusted code.
        [SecuritySafeCritical]
        public override string ToString() {
            return m_name.ToString(m_byteCount);
        }

        // SECURITY: The method is actually not safe. We must make sure that this object is not leaked to partially-trusted code.
        [SecuritySafeCritical]
        public override int GetHashCode() {
            return m_name.GetHashCode(0, m_byteCount);
        }

        // SECURITY: The method is actually not safe. We must make sure that this object is not leaked to partially-trusted code.
        [SecuritySafeCritical]
        public override bool Equals(object obj) {
            return obj is MetadataNamePart && Equals((MetadataNamePart)obj)
                || obj is MetadataName && Equals((MetadataName)obj);
        }

        // SECURITY: The method is actually not safe. We must make sure that this object is not leaked to partially-trusted code.
        [SecuritySafeCritical]
        public unsafe bool Equals(MetadataNamePart other) {
            bool result = MetadataName.Equals(m_name.m_data, m_byteCount, other.m_name.m_data, other.m_byteCount);
            GC.KeepAlive(m_name.m_keepAlive);
            GC.KeepAlive(other.m_name.m_keepAlive);
            return result;
        }

        // SECURITY: The method is actually not safe. We must make sure that this object is not leaked to partially-trusted code.
        [SecuritySafeCritical]
        public unsafe bool Equals(MetadataName other) {
            bool result = MetadataName.Equals(other.m_data, m_name.m_data, m_byteCount);
            GC.KeepAlive(m_name.m_keepAlive);
            GC.KeepAlive(other.m_keepAlive);
            return result;
        }

        public static bool operator ==(MetadataNamePart self, MetadataNamePart other) {
            return self.Equals(other);
        }

        public static bool operator ==(MetadataNamePart self, MetadataName other) {
            return self.Equals(other);
        }

        public static bool operator !=(MetadataNamePart self, MetadataNamePart other) {
            return self.Equals(other);
        }

        public static bool operator !=(MetadataNamePart self, MetadataName other) {
            return self.Equals(other);
        }
    }
}
