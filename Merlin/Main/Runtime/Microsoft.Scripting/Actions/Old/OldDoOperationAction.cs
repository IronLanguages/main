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

using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Microsoft.Contracts;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {

    public class OldDoOperationAction : OldDynamicAction, IExpressionSerializable {
        private readonly ActionBinder _binder;
        private readonly Operators _operation;

        public static OldDoOperationAction Make(ActionBinder binder, Operators operation) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return new OldDoOperationAction(binder, operation);
        }

        private OldDoOperationAction(ActionBinder binder, Operators operation) {
            _binder = binder;
            _operation = operation;
        }

        public Operators Operation {
            get { return _operation; }
        }

        public override DynamicActionKind Kind {
            get { return DynamicActionKind.DoOperation; }
        }

        public ActionBinder Binder {
            get { return _binder; }
        }

        public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
            return Binder.Bind(this, args, parameters, returnLabel);
        }

        [Confined]
        public override bool Equals(object obj) {
            OldDoOperationAction other = obj as OldDoOperationAction;
            if (other == null) return false;
            return _operation == other._operation && (object)_binder == (object)other._binder;
        }

        [Confined]
        public override int GetHashCode() {
            return ((int)Kind << 28 ^ ((int)_operation)) ^ System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(_binder);
        }

        //??? Do these belong here or mone Operators enum
        public bool IsComparision {
            get {
                return CompilerHelpers.IsComparisonOperator(OperatorInfo.OperatorToExpressionType(_operation).Value);
            }
        }

        public bool IsUnary {
            get {
                switch (_operation) {
                    case Operators.OnesComplement:
                    case Operators.Negate:
                    case Operators.Positive:
                    case Operators.AbsoluteValue:
                    case Operators.Not:

                    // Added for COM support...
                    case Operators.Documentation:
                        return true;
                }
                return false;
            }
        }

        public bool IsInPlace {
            get {
                return CompilerHelpers.InPlaceOperatorToOperator(_operation) != Operators.None;
            }
        }

        public Operators DirectOperation {
            get {
                Operators res = CompilerHelpers.InPlaceOperatorToOperator(_operation);
                if (res != Operators.None) return res;

                throw new InvalidOperationException();
            }
        }

        [Confined]
        public override string ToString() {
            return base.ToString() + " " + _operation.ToString();
        }

        public Expression CreateExpression() {
            return Expression.Call(
               typeof(OldDoOperationAction).GetMethod("Make", new Type[] { typeof(ActionBinder), typeof(Operators) }),
               CreateActionBinderReadExpression(),
               AstUtils.Constant(_operation)
            );
        }

    }
}
