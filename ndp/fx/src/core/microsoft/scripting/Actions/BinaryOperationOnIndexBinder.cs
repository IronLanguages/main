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
    /// Represents the binary dynamic operation on index at the call site, providing the binding semantic and the details about the operation.
    /// </summary>
    public abstract class BinaryOperationOnIndexBinder : DynamicMetaObjectBinder {
        private ExpressionType _operation;
        private readonly ReadOnlyCollection<ArgumentInfo> _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperationOnIndexBinder"/> class.
        /// </summary>
        /// <param name="operation">The binary operation kind.</param>
        /// <param name="arguments">The signature of the arguments at the call site.</param>
        protected BinaryOperationOnIndexBinder(ExpressionType operation, params ArgumentInfo[] arguments)
            : this(operation, (IEnumerable<ArgumentInfo>)arguments) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperationOnIndexBinder"/> class.
        /// </summary>
        /// <param name="operation">The binary operation kind.</param>
        /// <param name="arguments">The signature of the arguments at the call site.</param>
        protected BinaryOperationOnIndexBinder(ExpressionType operation, IEnumerable<ArgumentInfo> arguments) {
            ContractUtils.Requires(BinaryOperationBinder.OperationIsValid(operation), "operation");
            _operation = operation;
            _arguments = arguments.ToReadOnly();
        }

        /// <summary>
        /// The binary operation kind.
        /// </summary>
        public ExpressionType Operation {
            get {
                return _operation;
            }
        }

        /// <summary>
        /// The signature of the arguments at the call site.
        /// </summary>
        public ReadOnlyCollection<ArgumentInfo> Arguments {
            get { return _arguments; }
        }

        /// <summary>
        /// Determines whether the specified System.Object is equal to the current <see cref="BinaryOperationOnIndexBinder"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="BinaryOperationOnIndexBinder"/>.</param>
        /// <returns>true if the specified System.Object is equal to the current <see cref="BinaryOperationOnIndexBinder"/>; otherwise, false.</returns>
        [Confined]
        public override bool Equals(object obj) {
            BinaryOperationOnIndexBinder ia = obj as BinaryOperationOnIndexBinder;
            return ia != null && ia._operation == _operation && ia._arguments.ListEquals(_arguments);
        }

        /// <summary>
        /// Returns the hash code for this instance. 
        /// </summary>
        /// <returns>An <see cref="Int32"/> containing the hash code for this instance.</returns>
        [Confined]
        public override int GetHashCode() {
            return BinaryOperationOnIndexBinderHash ^ (int)_operation ^ _arguments.ListHashCode();
        }

        /// <summary>
        /// Performs the binding of the dynamic binary operation.
        /// </summary>
        /// <param name="target">The target of the dynamic operation.</param>
        /// <param name="args">An array of arguments of the dynamic operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNull(args, "args");
            ContractUtils.Requires(args.Length >= 2, "args");

            DynamicMetaObject value = args[args.Length - 1];
            DynamicMetaObject[] indexes = args.RemoveLast();

            ContractUtils.RequiresNotNull(value, "args");
            ContractUtils.RequiresNotNullItems(indexes, "args");

            return target.BindBinaryOperationOnIndex(this, indexes, value);
        }

        /// <summary>
        /// Performs the binding of the binary dynamic operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic binary operation.</param>
        /// <param name="arg">The right hand side operand of the dynamic binary operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        /// <remarks> This method is called by the target when the target implements the binary operation on index
        /// as a sequence of get index, binary operation and set index, to let the <see cref="DynamicMetaObject"/>
        /// request the binding of the binary operation only.
        /// </remarks>
        public DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg) {
            return FallbackBinaryOperation(target, arg, null);
        }

        /// <summary>
        /// When overridden in the derived class, performs the binding of the binary dynamic operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic binary operation.</param>
        /// <param name="arg">The right hand side operand of the dynamic binary operation.</param>
        /// <param name="errorSuggestion">The binding result in case the binding fails, or null.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        /// <remarks> This method is called by the target when the target implements the binary operation on index
        /// as a sequence of get index, binary operation and set index, to let the <see cref="DynamicMetaObject"/>
        /// request the binding of the binary operation only.
        /// </remarks>
        public abstract DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion);

        /// <summary>
        /// Performs the binding of the dynamic binary operation on index if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic binary operation on index.</param>
        /// <param name="indexes">An array of <see cref="DynamicMetaObject"/> instances - indexes for the dynamic binary operation on index.</param>
        /// <param name="value">The right hand side operand of the dynamic binary operation on index.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public DynamicMetaObject FallbackBinaryOperationOnIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value) {
            return FallbackBinaryOperationOnIndex(target, indexes, value, null);
        }

        /// <summary>
        /// When overridden in the derived class, performs the binding of the dynamic binary operation on index if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic binary operation on index.</param>
        /// <param name="indexes">An array of <see cref="DynamicMetaObject"/> instances - indexes for the dynamic binary operation on index.</param>
        /// <param name="value">The right hand side operand of the dynamic binary operation on index.</param>
        /// <param name="errorSuggestion">The binding result in case the binding fails, or null.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public abstract DynamicMetaObject FallbackBinaryOperationOnIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion);
    }
}
