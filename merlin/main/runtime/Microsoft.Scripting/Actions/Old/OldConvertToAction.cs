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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {

    public class OldConvertToAction : OldDynamicAction, IEquatable<OldConvertToAction>, IExpressionSerializable {
        private readonly ActionBinder _binder;
        private readonly Type _type;
        private readonly ConversionResultKind _resultKind;

        public static OldConvertToAction Make(ActionBinder binder, Type type) {
            return Make(binder, type, ConversionResultKind.ImplicitCast);
        }

        public static OldConvertToAction Make(ActionBinder binder, Type type, ConversionResultKind resultKind) {
            ContractUtils.RequiresNotNull(binder, "binder");
            ContractUtils.RequiresNotNull(type, "type");
            return new OldConvertToAction(binder, type, resultKind);
        }

        private OldConvertToAction(ActionBinder binder, Type type, ConversionResultKind resultKind) {
            _binder = binder;
            _type = type;
            _resultKind = resultKind;
        }

        public Type ToType { get { return _type; } }
        public ConversionResultKind ResultKind { get { return _resultKind; } }
        public override DynamicActionKind Kind { get { return DynamicActionKind.ConvertTo; } }

        public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
            return _binder.Bind(this, args, parameters, returnLabel);
        }

        [Confined]
        public override bool Equals(object obj) {
            return Equals(obj as OldConvertToAction);
        }

        [Confined]
        public override int GetHashCode() {
            return (int)Kind << 28 ^ (int)ResultKind ^ _type.GetHashCode() ^ System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(_binder);
        }

        [Confined]
        public override string ToString() {
            return base.ToString() + " to " + _type.ToString();
        }

        public Expression CreateExpression() {
            return Expression.Call(
                typeof(OldConvertToAction).GetMethod("Make", new Type[] { typeof(ActionBinder), typeof(Type), typeof(ConversionResultKind) }),
                CreateActionBinderReadExpression(),
                Expression.Constant(_type),
                Expression.Constant(_resultKind)
            );
        }

        #region IEquatable<OldConvertToAction> Members

        [StateIndependent]
        public bool Equals(OldConvertToAction other) {
            if (other == null) return false;
            if ((object)_binder != (object)other._binder) return false;
            return _type == other._type && _resultKind == other._resultKind;
        }

        #endregion
    }

    /// <summary>
    /// Determines the result of a conversion action.  The result can either result in an exception, a value that
    /// has been successfully converted or default(T), or a true/false result indicating if the value can be converted.
    /// </summary>
    public enum ConversionResultKind {
        /// <summary>
        /// Attempts to perform available implicit conversions and throws if there are no available conversions.
        /// </summary>
        ImplicitCast,
        /// <summary>
        /// Attempst to perform available implicit and explicit conversions and throws if there are no available conversions.
        /// </summary>
        ExplicitCast,
        /// <summary>
        /// Attempts to perform available implicit conversions and returns default(ReturnType) if no conversions can be performed.
        /// 
        /// If the return type of the rule is a value type then the return value will be zero-initialized.  If the return type
        /// of the rule is object or another class then the return type will be null (even if the conversion is to a value type).
        /// This enables ImplicitTry to be used to do TryConvertTo even if the type is value type (and the difference between
        /// null and a real value can be distinguished).
        /// </summary>
        ImplicitTry,
        /// <summary>
        /// Attempts to perform available implicit and explicit conversions and returns default(ReturnType) if no conversions 
        /// can be performed.
        /// 
        /// If the return type of the rule is a value type then the return value will be zero-initialized.  If the return type
        /// of the rule is object or another class then the return type will be null (even if the conversion is to a value type).
        /// This enables ExplicitTry to be used to do TryConvertTo even if the type is value type (and the difference between
        /// null and a real value can be distinguished).
        /// </summary>
        ExplicitTry
    }
}
