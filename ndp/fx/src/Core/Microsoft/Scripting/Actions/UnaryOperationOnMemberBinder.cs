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

using System.Dynamic.Utils;
using System.Linq.Expressions;
using Microsoft.Contracts;

namespace System.Dynamic {
    /// <summary>
    /// Represents the unary dynamic operation on member at the call site, providing the binding semantic and the details about the operation.
    /// </summary>
    public abstract class UnaryOperationOnMemberBinder : DynamicMetaObjectBinder {
        private readonly ExpressionType _operation;
        private readonly string _name;
        private readonly bool _ignoreCase;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnaryOperationOnMemberBinder"/> class.
        /// </summary>
        /// <param name="operation">The unary operation kind.</param>
        /// <param name="name">The name of the member for the operation.</param>
        /// <param name="ignoreCase">The value indicating whether to ignore the case of the member name.</param>
        protected UnaryOperationOnMemberBinder(ExpressionType operation, string name, bool ignoreCase) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.Requires(UnaryOperationBinder.OperationIsValid(operation), "operation");
            _operation = operation;
            _name = name;
            _ignoreCase = ignoreCase;
        }

        /// <summary>
        /// The unary operation kind.
        /// </summary>
        public ExpressionType Operation {
            get {
                return _operation;
            }
        }

        /// <summary>
        /// The name of the member for the operation.
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// Gets the value indicating whether to ignore the case of the member name.
        /// </summary>
        public bool IgnoreCase {
            get {
                return _ignoreCase;
            }
        }

        /// <summary>
        /// Performs the binding of the unary dynamic operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic unary operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        /// <remarks> This method is called by the target when the target implements the unary operation on member
        /// as a sequence of get member, unary operation and set member, to let the <see cref="DynamicMetaObject"/>
        /// request the binding of the unary operation only.
        /// </remarks>
        public DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target) {
            return FallbackUnaryOperation(target);
        }

        /// <summary>
        /// When overridden in the derived class, performs the binding of the unary dynamic operation if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic unary operation.</param>
        /// <param name="errorSuggestion">The binding result in case the binding fails, or null.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        /// <remarks> This method is called by the target when the target implements the unary operation on member
        /// as a sequence of get member, unary operation and set member, to let the <see cref="DynamicMetaObject"/>
        /// request the binding of the unary operation only.
        /// </remarks>
        public abstract DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion);

        /// <summary>
        /// Performs the binding of the dynamic unary operation on member if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic unary operation on member.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public DynamicMetaObject FallbackUnaryOperationOnMember(DynamicMetaObject target) {
            return FallbackUnaryOperationOnMember(target, null);
        }

        /// <summary>
        /// When overridden in the derived class, performs the binding of the dynamic unary operation on member if the target dynamic object cannot bind.
        /// </summary>
        /// <param name="target">The target of the dynamic unary operation on member.</param>
        /// <param name="errorSuggestion">The binding result in case the binding fails, or null.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public abstract DynamicMetaObject FallbackUnaryOperationOnMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion);

        /// <summary>
        /// Performs the binding of the dynamic unary operation.
        /// </summary>
        /// <param name="target">The target of the dynamic operation.</param>
        /// <param name="args">An array of arguments of the dynamic operation.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, params DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.Requires(args == null || args.Length == 0, "args");

            return target.BindUnaryOperationOnMember(this);
        }

        /// <summary>
        /// Determines whether the specified System.Object is equal to the current <see cref="UnaryOperationOnMemberBinder"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="UnaryOperationOnMemberBinder"/>.</param>
        /// <returns>true if the specified System.Object is equal to the current <see cref="UnaryOperationOnMemberBinder"/>; otherwise, false.</returns>
        [Confined]
        public override bool Equals(object obj) {
            UnaryOperationOnMemberBinder gma = obj as UnaryOperationOnMemberBinder;
            return gma != null && gma._operation == _operation && gma._name == _name && gma._ignoreCase == _ignoreCase;
        }

        /// <summary>
        /// Returns the hash code for this instance. 
        /// </summary>
        /// <returns>An <see cref="Int32"/> containing the hash code for this instance.</returns>
        [Confined]
        public override int GetHashCode() {
            return UnaryOperationOnMemberBinderHash ^ (int)_operation ^ _name.GetHashCode() ^ (_ignoreCase ? unchecked((int)0x80000000) : 0);
        }
    }
}
