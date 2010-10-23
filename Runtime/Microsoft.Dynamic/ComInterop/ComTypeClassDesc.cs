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

#if !SILVERLIGHT // ComObject
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Dynamic;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.ComInterop {

    public class ComTypeClassDesc : ComTypeDesc, IDynamicMetaObjectProvider {
        private LinkedList<string> _itfs; // implemented interfaces
        private LinkedList<string> _sourceItfs; // source interfaces supported by this coclass
        private Type _typeObj;
        
        public object CreateInstance() {
            if (_typeObj == null) {
                _typeObj = System.Type.GetTypeFromCLSID(Guid);
            }
            return System.Activator.CreateInstance(System.Type.GetTypeFromCLSID(Guid));
        }

        internal ComTypeClassDesc(ComTypes.ITypeInfo typeInfo, ComTypeLibDesc typeLibDesc) :
            base(typeInfo, ComType.Class, typeLibDesc) {
            ComTypes.TYPEATTR typeAttr = ComRuntimeHelpers.GetTypeAttrForTypeInfo(typeInfo);
            Guid = typeAttr.guid;

            for (int i = 0; i < typeAttr.cImplTypes; i++) {
                int hRefType;
                typeInfo.GetRefTypeOfImplType(i, out hRefType);
                ComTypes.ITypeInfo currentTypeInfo;
                typeInfo.GetRefTypeInfo(hRefType, out currentTypeInfo);

                ComTypes.IMPLTYPEFLAGS implTypeFlags;
                typeInfo.GetImplTypeFlags(i, out implTypeFlags);

                bool isSourceItf = (implTypeFlags & ComTypes.IMPLTYPEFLAGS.IMPLTYPEFLAG_FSOURCE) != 0;
                AddInterface(currentTypeInfo, isSourceItf);
            }
        }

        private void AddInterface(ComTypes.ITypeInfo itfTypeInfo, bool isSourceItf) {
            string itfName = ComRuntimeHelpers.GetNameOfType(itfTypeInfo);

            if (isSourceItf) {
                if (_sourceItfs == null) {
                    _sourceItfs = new LinkedList<string>();
                }
                _sourceItfs.AddLast(itfName);
            } else {
                if (_itfs == null) {
                    _itfs = new LinkedList<string>();
                }
                _itfs.AddLast(itfName);
            }
        }

        internal bool Implements(string itfName, bool isSourceItf) {
            if (isSourceItf)
                return _sourceItfs.Contains(itfName);
            else
                return _itfs.Contains(itfName);
        }

        #region IDynamicMetaObjectProvider Members

        public DynamicMetaObject GetMetaObject(Expression parameter) {
            return new ComClassMetaObject(parameter, this);
        }

        #endregion
    }
}

#endif
