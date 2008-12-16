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

namespace System.Dynamic {
    public abstract class CreateInstanceBinder : DynamicMetaObjectBinder {
        private readonly ReadOnlyCollection<ArgumentInfo> _arguments;

        protected CreateInstanceBinder(IEnumerable<ArgumentInfo> arguments) {
            _arguments = arguments.ToReadOnly();
        }

        protected CreateInstanceBinder(params ArgumentInfo[] arguments)
            : this((IEnumerable<ArgumentInfo>)arguments) {
        }

        public ReadOnlyCollection<ArgumentInfo> Arguments {
            get {
                return _arguments;
            }
        }

        public DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args) {
            return FallbackCreateInstance(target, args, null);
        }

        public abstract DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion);

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");

            return target.BindCreateInstance(this, args);
        }

        [Confined]
        public override bool Equals(object obj) {
            CreateInstanceBinder ca = obj as CreateInstanceBinder;
            return ca != null && ca._arguments.ListEquals(_arguments);
        }

        [Confined]
        public override int GetHashCode() {
            return CreateInstanceBinderHash ^ _arguments.ListHashCode();
        }
    }
}
