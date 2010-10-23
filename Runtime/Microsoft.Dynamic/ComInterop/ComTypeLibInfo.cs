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
using System.Dynamic;

namespace Microsoft.Scripting.ComInterop {

    public sealed class ComTypeLibInfo : IDynamicMetaObjectProvider  {
        private readonly ComTypeLibDesc _typeLibDesc;

        internal ComTypeLibInfo(ComTypeLibDesc typeLibDesc) {
            _typeLibDesc = typeLibDesc;
        }

        public string Name {
            get { return _typeLibDesc.Name; }
        }

        public Guid Guid {
            get { return _typeLibDesc.Guid; }
        }

        public short VersionMajor {
            get { return _typeLibDesc.VersionMajor; }
        }

        public short VersionMinor {
            get { return _typeLibDesc.VersionMinor; }
        }

        public ComTypeLibDesc TypeLibDesc {
            get { return _typeLibDesc; }
        }

        // TODO: internal
        public string[] GetMemberNames() {
            return new string[] { this.Name, "Guid", "Name", "VersionMajor", "VersionMinor" };
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
            return new TypeLibInfoMetaObject(parameter, this);
        }
    }
}

#endif
