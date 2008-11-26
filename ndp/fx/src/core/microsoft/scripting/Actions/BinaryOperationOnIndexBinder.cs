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

namespace System.Dynamic.Binders {
    /// <summary>
    /// A Binder that is responsible for runtime binding of operation:
    /// a[b] (op)= c
    /// </summary>
    public abstract class BinaryOperationOnIndexBinder : MetaObjectBinder {
        private ExpressionType _operation;
        private readonly ReadOnlyCollection<ArgumentInfo> _arguments;

        /// <summary>
        /// Constructor of the OperationOnIndexBinder object, representing "a[b] (op)= c" operation.
        /// </summary>
        /// <param name="operation">Binary operation to be performed.</param>
        /// <param name="arguments">Description of the indexes (named, positional)</param>
        protected BinaryOperationOnIndexBinder(ExpressionType operation, params ArgumentInfo[] arguments)
            : this(operation, (IEnumerable<ArgumentInfo>)arguments) {
        }

        protected BinaryOperationOnIndexBinder(ExpressionType operation, IEnumerable<ArgumentInfo> arguments) {
            ContractUtils.Requires(BinaryOperationBinder.OperationIsValid(operation), "operation");
            _operation = operation;
            _arguments = arguments.ToReadOnly();
        }

        /// <summary>
        /// The operation to be performed.
        /// </summary>
        public ExpressionType Operation {
            get {
                return _operation;
            }
        }

        /// <summary>
        /// Descriptions of arguments to the indexer. This allows for named and positional arguments.
        /// </summary>
        public ReadOnlyCollection<ArgumentInfo> Arguments {
            get { return _arguments; }
        }

        /// <summary>
        /// Implements Equality operation for the OperationOnIndexBinder
        /// </summary>
        /// <param name="obj">Instance to comapre equal to.</param>
        /// <returns>true/false</returns>
        [Confined]
        public override bool Equals(object obj) {
            BinaryOperationOnIndexBinder ia = obj as BinaryOperationOnIndexBinder;
            return ia != null && ia._operation == _operation && ia._arguments.ListEquals(_arguments);
        }

        /// <summary>
        /// Calculates hash code for the OperationOnIndexBinder
        /// </summary>
        /// <returns>The hash code.</returns>
        [Confined]
        public override int GetHashCode() {
            return BinaryOperationOnIndexBinderHash ^ (int)_operation ^ _arguments.ListHashCode();
        }

        /// <summary>
        /// Performs binding of the operation on the target (represented as meta object) and
        /// list of arguments (indexes and right-hand value) represented as meta objects
        /// </summary>
        /// <param name="target">Target of the operation.</param>
        /// <param name="args">List of indexes and right-hand value</param>
        /// <returns>MetaObject representing the binding.</returns>
        public sealed override MetaObject Bind(MetaObject target, MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNull(args, "args");
            ContractUtils.Requires(args.Length >= 2, "args");

            MetaObject value = args[args.Length - 1];
            MetaObject[] indexes = args.RemoveLast();

            ContractUtils.RequiresNotNull(value, "args");
            ContractUtils.RequiresNotNullItems(indexes, "args");

            return target.BindBinaryOperationOnIndex(this, indexes, value);
        }

        /// <summary>
        /// Implements a binding logic for the binary operation part of the binding.
        /// This is called by the target when the target implements the whole operation:
        ///    a[b] += c
        /// as:
        ///    a[b] = a[b] + c
        /// to let the language participate in the binding of the binary operation only.
        /// </summary>
        /// <param name="target">Target of the operation.</param>
        /// <param name="arg">Right-hand operator value</param>
        /// <returns>MetaObject representing the binding result.</returns>
        public MetaObject FallbackBinaryOperation(MetaObject target, MetaObject arg) {
            return FallbackBinaryOperation(target, arg, null);
        }

        /// <summary>
        /// Implements a binding logic for the binary operation part of the binding.
        /// This is called by the target when the target implements the whole operation:
        ///    a[b] += c
        /// as:
        ///    a[b] = a[b] + c
        /// to let the language participate in the binding of the binary operation only.
        /// </summary>
        /// <param name="target">Target of the operation.</param>
        /// <param name="arg">Right-hand operator value</param>
        /// <param name="errorSuggestion">The representaiton of the binding error that the target meta object recommends the language to use if the language cannot bind. This allows the target meta object to participate in the error handling process.</param>
        /// <returns>MetaObject representing the binding result.</returns>
        public abstract MetaObject FallbackBinaryOperation(MetaObject target, MetaObject arg, MetaObject errorSuggestion);

        /// <summary>
        /// Implements a binding logic for the operation. This is called by the target when
        /// the target lets the executing language participate in the binding process.
        /// </summary>
        /// <param name="target">Target of the operation.</param>
        /// <param name="indexes">List of indexes</param>
        /// <param name="value">Right-hand value</param>
        /// <returns>MetaObject representing the binding.</returns>
        public MetaObject FallbackBinaryOperationOnIndex(MetaObject target, MetaObject[] indexes, MetaObject value) {
            return FallbackBinaryOperationOnIndex(target, indexes, value, null);
        }

        /// <summary>
        /// Implements a binding logic for the operation. This is called by the target when
        /// the target lets the executing language participate in the binding process.
        /// </summary>
        /// <param name="target">Target of the operation.</param>
        /// <param name="indexes">List of indexes</param>
        /// <param name="value">Right-hand value</param>
        /// <param name="errorSuggestion">The representaiton of the binding error that the target meta object recommends the language to use if the language cannot bind. This allows the target meta object to participate in the error handling process.</param>
        /// <returns>MetaObject representing the binding.</returns>
        public abstract MetaObject FallbackBinaryOperationOnIndex(MetaObject target, MetaObject[] indexes, MetaObject value, MetaObject errorSuggestion);
    }
}
