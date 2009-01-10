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
using System.Dynamic.Utils;
using System.Linq.Expressions;
using Microsoft.Contracts;

namespace System.Dynamic {

    /// <summary>
    /// Represents the dynamic get index operation at the call site, providing the binding semantic and the details about the operation.
    /// </summary>
    public abstract class GetIndexBinder : DynamicMetaObjectBinder {
        private readonly ReadOnlyCollection<ArgumentInfo> _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetIndexBinder" />.
        /// </summary>
        /// <param name="arguments">The signature of the arguments at the call site.</param>
        protected GetIndexBinder(params ArgumentInfo[] arguments)
            : this((IEnumerable<ArgumentInfo>)arguments) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetIndexBinder" />.
        /// </summary>
        /// <param name="arguments">The signature of the arguments at the call site.</param>
        protected GetIndexBinder(IEnumerable<ArgumentInfo> arguments) {
            _arguments = arguments.ToReadOnly();
        }

        /// <summary>
        /// Gets the signature of the arguments at the call site.
        /// </summary>
        public ReadOnlyCollection<ArgumentInfo> Arguments {
            get { return _arguments; }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object" /> is equal to the current object.
        /// </summary>
        /// <param name="obj">The <see cref="Object" /> to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise false.</returns>
        [Confined]
        public override bool Equals(object obj) {
            GetIndexBinder ia = obj as GetIndexBinder;
            return ia != null && ia._arguments.ListEquals(_arguments);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>An <see cref="Int32" /> containing the hash code for this instance.</returns>
        [Confined]
        public override int GetHashCode() {
            return GetIndexBinderHash ^ _arguments.ListHashCode();
        }

        /// <summary>
        /// Performs the binding of the dynamic get index operation.
        /// </summary>
        /// <param name="target">The target of the dynamic get index operation.</param>
        /// <param name="args">An array of arguments of the dynamic get index operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");

            return target.BindGetIndex(this, args);
        }

        /// <summary>
        /// Performs the binding of the dynamic get index operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic get index operation.</param>
        /// <param name="indexes">The arguments of the dynamic get index operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes) {
            return FallbackGetIndex(target, indexes, null);
        }

        /// <summary>
        /// When overridden in the derived class, performs the binding of the dynamic get index operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic get index operation.</param>
        /// <param name="indexes">The arguments of the dynamic get index operation.</param>
        /// <param name="errorSuggestion">The binding result to use if binding fails, or null.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public abstract DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion);
    }
}
