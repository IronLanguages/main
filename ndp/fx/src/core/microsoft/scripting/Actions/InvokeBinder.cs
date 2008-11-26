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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Dynamic.Utils;
using Microsoft.Contracts;

namespace System.Dynamic.Binders {
    public abstract class InvokeBinder : MetaObjectBinder {
        private readonly ReadOnlyCollection<ArgumentInfo> _arguments;

        protected InvokeBinder(params ArgumentInfo[] arguments)
            : this((IEnumerable<ArgumentInfo>)arguments) {
        }

        protected InvokeBinder(IEnumerable<ArgumentInfo> arguments) {
            _arguments = arguments.ToReadOnly();
        }

        public ReadOnlyCollection<ArgumentInfo> Arguments {
            get { return _arguments; }
        }

        public MetaObject FallbackInvoke(MetaObject target, MetaObject[] args) {
            return FallbackInvoke(target, args, null);
        }

        public abstract MetaObject FallbackInvoke(MetaObject target, MetaObject[] args, MetaObject errorSuggestion);

        public sealed override MetaObject Bind(MetaObject target, MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");

            return target.BindInvoke(this, args);
        }

        [Confined]
        public override bool Equals(object obj) {
            InvokeBinder ia = obj as InvokeBinder;
            return ia != null && ia._arguments.ListEquals(_arguments);
        }

        [Confined]
        public override int GetHashCode() {
            return InvokeBinderHash ^ _arguments.ListHashCode();
        }
    }
}
