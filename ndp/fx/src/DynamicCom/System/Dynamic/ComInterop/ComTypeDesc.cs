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

#if !SILVERLIGHT // ComObject

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace System.Dynamic.ComInterop {

    internal class ComTypeDesc {
        private string _typeName;
        private string _documentation;
        private Guid _guid;
        private Dictionary<string, ComMethodDesc> _funcs;
        private Dictionary<string, ComMethodDesc> _puts;
        private Dictionary<string, ComMethodDesc> _putRefs;
        private Dictionary<string, ComEventDesc> _events;
        private ComMethodDesc _getItem;
        private ComMethodDesc _setItem;
        private static readonly Dictionary<string, ComEventDesc> _EmptyEventsDict = new Dictionary<string, ComEventDesc>();

        internal ComTypeDesc(ITypeInfo typeInfo) {
            if (typeInfo != null) {
                ComRuntimeHelpers.GetInfoFromType(typeInfo, out _typeName, out _documentation);
            }
        }

        internal static ComTypeDesc FromITypeInfo(ComTypes.ITypeInfo typeInfo) {
            ComTypes.TYPEATTR typeAttr;
            typeAttr = ComRuntimeHelpers.GetTypeAttrForTypeInfo(typeInfo);
            if (typeAttr.typekind == ComTypes.TYPEKIND.TKIND_COCLASS) {
                return new ComTypeClassDesc(typeInfo);
            } else if (typeAttr.typekind == ComTypes.TYPEKIND.TKIND_ENUM) {
                return new ComTypeEnumDesc(typeInfo);
            } else if ((typeAttr.typekind == ComTypes.TYPEKIND.TKIND_DISPATCH) ||
                  (typeAttr.typekind == ComTypes.TYPEKIND.TKIND_INTERFACE)) {
                ComTypeDesc typeDesc = new ComTypeDesc(typeInfo);
                return typeDesc;
            } else {
                throw Error.UnsupportedEnumType();
            }
        }

        internal static ComTypeDesc CreateEmptyTypeDesc() {
            ComTypeDesc typeDesc = new ComTypeDesc(null);
            typeDesc._funcs = new Dictionary<string, ComMethodDesc>();
            typeDesc._events = _EmptyEventsDict;

            return typeDesc;
        }

        internal static Dictionary<string, ComEventDesc> EmptyEvents {
            get { return _EmptyEventsDict; }
        }

        internal Dictionary<string, ComMethodDesc> Funcs {
            get { return _funcs; }
            set { _funcs = value; }
        }

        internal Dictionary<string, ComMethodDesc> Puts {
            get { return _puts; }
            set { _puts = value; }
        }
        internal Dictionary<string, ComMethodDesc> PutRefs {
            get { return _putRefs; }
            set { _putRefs = value; }
        }

        internal Dictionary<string, ComEventDesc> Events {
            get { return _events; }
            set { _events = value; }
        }

        public string TypeName {
            get { return _typeName; }
        }

        internal Guid Guid {
            get { return _guid; }
            set { _guid = value; }
        }

        internal ComMethodDesc GetItem {
            get { return _getItem; }
        }
        internal void EnsureGetItem(ComMethodDesc candidate){
            Interlocked.CompareExchange(ref _getItem, candidate, null);
        }


        internal ComMethodDesc SetItem {
            get { return _setItem; }
        }
        internal void EnsureSetItem(ComMethodDesc candidate) {
            Interlocked.CompareExchange(ref _setItem, candidate, null);
        }

    }
}

#endif
