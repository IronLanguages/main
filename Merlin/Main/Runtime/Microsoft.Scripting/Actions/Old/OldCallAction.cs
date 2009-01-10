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
    // TODO: rename to match InvocationExpression
    public class OldCallAction : OldDynamicAction, IEquatable<OldCallAction>, IExpressionSerializable {
        private readonly ActionBinder _binder;
        private readonly CallSignature _signature;

        protected OldCallAction(ActionBinder binder, CallSignature callSignature) {
            _binder = binder;
            _signature = callSignature;
        }

        public static OldCallAction Make(ActionBinder binder, CallSignature signature) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return new OldCallAction(binder, signature);
        }

        public static OldCallAction Make(ActionBinder binder, int argumentCount) {
            ContractUtils.Requires(argumentCount >= 0, "argumentCount");
            ContractUtils.RequiresNotNull(binder, "binder");
            return new OldCallAction(binder, new CallSignature(argumentCount));
        }

        public ActionBinder Binder {
            get {
                return _binder;
            }
        }

        public CallSignature Signature {
            get { return _signature; }
        }

        public override DynamicActionKind Kind {
            get { return DynamicActionKind.Call; }
        }

        public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
            return Binder.Bind(this, args, parameters, returnLabel);
        }

        [StateIndependent]
        public bool Equals(OldCallAction other) {
            if (other == null || other.GetType() != GetType()) return false;
            if ((object)_binder != (object)other._binder) return false;
            return _signature.Equals(other._signature);
        }

        [Confined]
        public override bool Equals(object obj) {
            return Equals(obj as OldCallAction);
        }

        [Confined]
        public override int GetHashCode() {
            return ((int)Kind << 28 ^ _signature.GetHashCode()) ^ System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(_binder);
        }

        [Confined]
        public override string ToString() {
            return base.ToString() + _signature.ToString();
        }

        public virtual Expression CreateExpression() {
            return Expression.Call(
                typeof(OldCallAction).GetMethod("Make", new Type[] { typeof(ActionBinder), typeof(CallSignature) }),
                CreateActionBinderReadExpression(),
                Signature.CreateExpression()
            );
        }
    }
}


