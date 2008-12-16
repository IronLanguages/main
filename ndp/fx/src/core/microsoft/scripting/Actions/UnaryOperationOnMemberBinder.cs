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
    /// A Binder that is responsible for runtime binding of operation:
    /// a.b (op)= c
    /// </summary>
    public abstract class UnaryOperationOnMemberBinder : DynamicMetaObjectBinder {
        private readonly ExpressionType _operation;
        private readonly string _name;
        private readonly bool _ignoreCase;

        /// <summary>
        /// Constructor of the OperationOnIndexBinder object, representing "a.b (op)= c" operation.
        /// </summary>
        /// <param name="operation">Binary operation to be performed.</param>
        /// <param name="name">Name of the member for the operation.</param>
        /// <param name="ignoreCase">Ignore case of the member.</param>
        protected UnaryOperationOnMemberBinder(ExpressionType operation, string name, bool ignoreCase) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.Requires(UnaryOperationBinder.OperationIsValid(operation), "operation");
            _operation = operation;
            _name = name;
            _ignoreCase = ignoreCase;
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
        /// Name of the member for the operation.
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// Ignore case of the member.
        /// </summary>
        public bool IgnoreCase {
            get {
                return _ignoreCase;
            }
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
        /// <returns>MetaObject representing the binding result.</returns>
        public DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target) {
            return FallbackUnaryOperation(target);
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
        /// <param name="errorSuggestion">The representaiton of the binding error that the target meta object recommends the language to use if the language cannot bind. This allows the target meta object to participate in the error handling process.</param>
        /// <returns>MetaObject representing the binding result.</returns>
        public abstract DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion);

        /// <summary>
        /// Implements a binding logic for the operation. This is called by the target when
        /// the target lets the executing language participate in the binding process.
        /// </summary>
        /// <param name="target">Target of the operation.</param>
        /// <returns>MetaObject representing the binding.</returns>
        public DynamicMetaObject FallbackUnaryOperationOnMember(DynamicMetaObject target) {
            return FallbackUnaryOperationOnMember(target, null);
        }

        /// <summary>
        /// Implements a binding logic for the operation. This is called by the target when
        /// the target lets the executing language participate in the binding process.
        /// </summary>
        /// <param name="target">Target of the operation.</param>
        /// <param name="errorSuggestion">The representation of the binding error that the target meta
        /// object recommends the language to use if the language cannot bind.
        /// This allows the target meta object to participate in the error handling process.</param>
        /// <returns>MetaObject representing the binding.</returns>
        public abstract DynamicMetaObject FallbackUnaryOperationOnMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion);

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, params DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.Requires(args == null || args.Length == 0, "args");

            return target.BindUnaryOperationOnMember(this);
        }

        /// <summary>
        /// Implements Equality operation for the OperationOnMemberBinder
        /// </summary>
        /// <param name="obj">Instance to comapre equal to.</param>
        /// <returns>true/false</returns>
        [Confined]
        public override bool Equals(object obj) {
            UnaryOperationOnMemberBinder gma = obj as UnaryOperationOnMemberBinder;
            return gma != null && gma._operation == _operation && gma._name == _name && gma._ignoreCase == _ignoreCase;
        }

        /// <summary>
        /// Calculates hash code for the OperationOnMemberBinder
        /// </summary>
        /// <returns>The hash code.</returns>
        [Confined]
        public override int GetHashCode() {
            return UnaryOperationOnMemberBinderHash ^ (int)_operation ^ _name.GetHashCode() ^ (_ignoreCase ? unchecked((int)0x80000000) : 0);
        }
    }
}
