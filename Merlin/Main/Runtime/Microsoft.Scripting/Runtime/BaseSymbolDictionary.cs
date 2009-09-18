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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;

namespace Microsoft.Scripting.Runtime {

    /// <summary>
    /// Base class for SymbolId dictionaries.  
    /// 
    /// SymbolId dictionaries are fast dictionaries used for looking up members of classes, 
    /// function environments, function locals, and other places which are typically indexed by 
    /// string names.  
    /// 
    /// SymbolId dictionaries support both keying by SymbolId (the common case) and object keys 
    /// (supporting late bound access to the dictionary as a normal Dictionary&lt;object, object&gt; 
    /// when exposed directly to user code).  When indexed by objects null is a valid value for the
    /// key.
    /// </summary>
    abstract class BaseSymbolDictionary {
        private static readonly object _nullObject = new object();
        private const int ObjectKeysId = -2;
        internal static readonly SymbolId ObjectKeys = new SymbolId(ObjectKeysId);

        /// <summary>
        /// Creates a new SymbolIdDictBase from the specified creating context which will be
        /// used for comparisons.
        /// </summary>
        protected BaseSymbolDictionary() {
        }

        public static object NullToObj(object o) {
            if (o == null) return _nullObject;
            return o;
        }

        public static object ObjToNull(object o) {
            if (o == _nullObject) return null;
            return o;
        }

        public static bool IsNullObject(object o) {
            return o == _nullObject;
        }
    }
}