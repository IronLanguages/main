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

#if !SILVERLIGHT

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Dynamic;

namespace System.Dynamic {
    class ComInvokeAction : InvokeBinder {
        public override object CacheIdentity {
            get { return this; }
        }

        internal ComInvokeAction(params ArgumentInfo[] arguments)
            : base(arguments) {
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            return base.Equals(obj as ComInvokeAction);
        }

        public override MetaObject FallbackInvoke(MetaObject target, MetaObject[] args, MetaObject errorSuggestion) {
            return errorSuggestion ?? MetaObject.CreateThrow(target, args, typeof(NotSupportedException), "Cannot perform call");
        }
    }
}

#endif
