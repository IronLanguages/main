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

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Scripting.Utils;
using System;

namespace Microsoft.Scripting {
    /// <summary>
    /// Provides a common table of all SymbolId's in the system.
    /// 
    /// Implementation details:
    /// 
    /// The case insensitive lookups are implemented by using the top 8 bits for
    /// storing information about multiple casings.  These bits are zero for a case insensitive
    /// identifier or specify the casing version for case sensitive lookups.  Because of this
    /// there can be at most 255 variations of casing for each identifier.
    /// 
    /// Two dictionaries are used to track both the case sensitive and case insensitive versions.
    /// 
    /// For the case insensitive versions this is just a normal dictionary keyed from string to
    /// the ID for that specific version.  For the case sensitive version a case insensitive
    /// dictionary is used.  The value in this case is the last case insensitive version that
    /// we handed out.
    /// 
    /// When we hand out an ID we first do a lookup in the normal dictionary.  If this succeeds
    /// then we have the ID and we're done.  If this fails we then need to consult the case
    /// insensitive dictionary.  If the entry exists there then we just need to bump the invariant
    /// version, store that back into the invariant dictionary, and then update the normal dictionary
    /// with the newly produced version.  If teh entry wasn't in the case insensitive dictionary
    /// then we need to create a new entry in both tables.
    /// </summary>
    public static class SymbolTable {
        private static readonly object _lockObj = new object();

        private static readonly Dictionary<string, int> _idDict = new Dictionary<string, int>(InitialTableSize);
        private static readonly Dictionary<string, int> _invariantDict = new Dictionary<string, int>(InitialTableSize, StringComparer.OrdinalIgnoreCase);

        private const int InitialTableSize = 256;
        private static readonly Dictionary<int, string> _fieldDict = CreateFieldDictionary();

        private static int _nextCaseInsensitiveId = 1;

        internal const int CaseVersionMask = unchecked((int)0xFF000000);
        internal const int CaseVersionIncrement = 0x01000000;

        private static Dictionary<int, string>  CreateFieldDictionary() {
            Dictionary<int, string> result = new Dictionary<int, string>(InitialTableSize);
            result[0] = null;   // initialize the null string
            return result;
        }

        public static SymbolId StringToId(string value) {
            ContractUtils.RequiresNotNull(value, "value");

            int res;
            lock (_lockObj) {
                // First, look up the identifier case-sensitively.
                if (!_idDict.TryGetValue(value, out res)) {
                    // OK, didn't find it, so let's look up the case-insensitive
                    // identifier.
                    if (!_invariantDict.TryGetValue(value, out res)) {
                        // This is a whole new identifier.
                        if (_nextCaseInsensitiveId == ~CaseVersionMask) {
                            throw Error.CantAddIdentifier(value);
                        }

                        // allocate new ID at case version 1.
                        res = _nextCaseInsensitiveId++ | CaseVersionIncrement;
                    } else {
                        // OK, this is a new casing of an existing identifier.
                        // Throw if we've exhausted the number of casings.
                        if (unchecked(((uint)res & CaseVersionMask) == CaseVersionMask)) {
                            throw Error.CantAddCasing(value);
                        }

                        // bump the case version
                        res += CaseVersionIncrement;
                    }

                    // update the tables with the IDs
                    _invariantDict[value] = res;
                    _idDict[value] = res;
                    _fieldDict[res] = value;
                }
            }
            return new SymbolId(res);
        }

        public static SymbolId StringToCaseInsensitiveId(string value) {
            return StringToId(value).CaseInsensitiveIdentifier;
        }

        public static SymbolId[] QualifiedStringToIds(string values) {
            if (values != null) {
                string[] strings = values.Split('.');
                SymbolId[] identifiers = new SymbolId[strings.Length];

                for (int i = 0; i < strings.Length; i++) {
                    identifiers[i] = StringToId(strings[i]);
                }

                return identifiers;
            }

            return null;
        }

        public static string IdToString(SymbolId id) {
            lock (_fieldDict) {
                if (id.IsCaseInsensitive) {
                    return _fieldDict[id.Id | CaseVersionIncrement];
                }
                return _fieldDict[id.Id];
            }
        }

        // Tries to lookup the SymbolId to see if it is valid
        public static bool ContainsId(SymbolId id) {
            lock (_fieldDict) {
                if (id.IsCaseInsensitive) {
                    return _fieldDict.ContainsKey(id.Id | CaseVersionIncrement);
                }
                return _fieldDict.ContainsKey(id.Id);
            }
        }

        public static string[] IdsToStrings(IList<SymbolId> ids) {
            string[] ret = new string[ids.Count];
            for (int i = 0; i < ids.Count; i++) {
                if (ids[i] == SymbolId.Empty) ret[i] = null;
                else ret[i] = IdToString(ids[i]);
            }
            return ret;
        }

        public static SymbolId[] StringsToIds(IList<string> values) {
            SymbolId[] ret = new SymbolId[values.Count];
            for (int i = 0; i < values.Count; i++) {
                if (values[i] == null) ret[i] = SymbolId.Empty;
                else ret[i] = StringToId(values[i]);
            }
            return ret;
        }

        public static bool StringHasId(string symbol) {
            ContractUtils.RequiresNotNull(symbol, "symbol");

            lock (_lockObj) {
                return _idDict.ContainsKey(symbol);
            }
        }

        public static SymbolId StringToIdOrEmpty(string value) {
            if (value == null) {
                return SymbolId.Empty;
            }
            return StringToId(value);
        }
    }
}
