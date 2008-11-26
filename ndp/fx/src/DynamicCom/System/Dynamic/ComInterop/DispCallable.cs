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

using System.Linq.Expressions;
using System.Dynamic.Binders;
using Microsoft.Contracts;
using System.Globalization;

namespace System.Dynamic.ComInterop {
    /// <summary>
    /// This represents a bound dispmethod on a IDispatch object.
    /// </summary>
    internal sealed class DispCallable : IDynamicObject {

        private readonly IDispatch _dispatch;
        private readonly ComMethodDesc _methodDesc;

        internal DispCallable(IDispatch dispatch, ComMethodDesc methodDesc) {
            _dispatch = dispatch;
            _methodDesc = methodDesc;
        }

        public override string ToString() {
            return String.Format(CultureInfo.CurrentCulture, "<bound dispmethod {0}>", _methodDesc.Name);
        }

        public IDispatch DispatchObject {
            get { return _dispatch; }
        }

        public ComMethodDesc ComMethodDesc {
            get { return _methodDesc; }
        }

        public MetaObject GetMetaObject(Expression parameter) {
            return new DispCallableMetaObject(parameter, this);
        }

        public override bool Equals(object obj) {
            var other = obj as DispCallable;
            return other != null && other._dispatch == _dispatch && other._methodDesc == _methodDesc;
        }

        public override int GetHashCode() {
            return _dispatch.GetHashCode() ^ _methodDesc.GetHashCode();
        }
    }
}

#endif
