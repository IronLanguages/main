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

#if !CLR2
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Dynamic;

namespace Microsoft.Scripting.Runtime {
    using Ast = MSAst.Expression;

    /// <summary>
    /// Wraps a an IDictionary[object, object] and exposes it as an IDynamicMetaObjectProvider so that
    /// users can access string attributes using member accesses.
    /// </summary>
    public sealed class ObjectDictionaryExpando : IDynamicMetaObjectProvider {
        private readonly IDictionary<object, object> _data;

        public ObjectDictionaryExpando(IDictionary<object, object> dictionary) {
            _data = dictionary;
        }

        public IDictionary<object, object> Dictionary {
            get {
                return _data;
            }
        }


        private static object TryGetMember(object adapter, string name) {
            object result;
            if (((ObjectDictionaryExpando)adapter)._data.TryGetValue(name, out result)) {
                return result;
            }
            return StringDictionaryExpando._getFailed;
        }

        private static void TrySetMember(object adapter, string name, object value) {
            ((ObjectDictionaryExpando)adapter)._data[name] = value;
        }

        private static bool TryDeleteMember(object adapter, string name) {
            return ((ObjectDictionaryExpando)adapter)._data.Remove(name);
        }

        #region IDynamicMetaObjectProvider Members

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(MSAst.Expression parameter) {
            return new DictionaryExpandoMetaObject(parameter, this, _data.Keys, TryGetMember, TrySetMember, TryDeleteMember);
        }

        #endregion
    }
}
