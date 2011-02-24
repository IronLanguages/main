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
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Scripting.Metadata;
using Microsoft.Scripting.Utils;

namespace Metadata {
    public sealed class TypeNestings {
        private readonly MetadataTables _tables;
        private readonly Dictionary<MetadataToken, List<MetadataToken>> _mapping;
        private static readonly TypeDef[] _EmptyTypeDefs = new TypeDef[0];

        public TypeNestings(MetadataTables tables) {
            ContractUtils.Requires(tables != null);
            _tables = tables;
            _mapping = new Dictionary<MetadataToken, List<MetadataToken>>();
            Populate();
        }

        private void Populate() {
            foreach (TypeNesting nesting in _tables.TypeNestings) {
                var enclosing = nesting.EnclosingType.Record.Token;
                List<MetadataToken> nested;
                if (!_mapping.TryGetValue(enclosing, out nested)) {
                    _mapping.Add(enclosing, nested = new List<MetadataToken>());
                }
                nested.Add(nesting.NestedType.Record.Token);
            }
        }

        public IEnumerable<TypeDef> GetEnclosingTypes() {
            return from enclosing in _mapping.Keys select _tables.GetRecord(enclosing).TypeDef;
        }

        public IEnumerable<TypeDef> GetNestedTypes(TypeDef typeDef) {
            int count;
            return GetNestedTypes(typeDef, out count);
        }

        public IEnumerable<TypeDef> GetNestedTypes(TypeDef typeDef, out int count) {
            ContractUtils.Requires(((MetadataRecord)typeDef).Tables.Equals(_tables));

            List<MetadataToken> nestedList;
            if (_mapping.TryGetValue(typeDef.Record.Token, out nestedList)) {
                count = nestedList.Count;
                return from nested in nestedList select _tables.GetRecord(nested).TypeDef;
            }

            count = 0;
            return _EmptyTypeDefs;
        }
    }
}
