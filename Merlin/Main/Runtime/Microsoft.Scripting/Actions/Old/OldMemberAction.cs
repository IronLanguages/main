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

namespace Microsoft.Scripting.Actions {
    public abstract class OldMemberAction : OldDynamicAction, IEquatable<OldMemberAction> {
        private readonly ActionBinder _binder;
        private readonly SymbolId _name;

        public SymbolId Name {
            get { return _name; }
        }

        public ActionBinder Binder {
            get { return _binder; }
        }

        protected OldMemberAction(ActionBinder binder, SymbolId name) {
            _binder = binder;
            _name = name;
        }

        public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
            return Binder.Bind(this, args, parameters, returnLabel);
        }

        [Confined]
        public override bool Equals(object obj) {
            return Equals(obj as OldMemberAction);
        }

        [Confined]
        public override int GetHashCode() {
            return (int)Kind << 28 ^ _name.GetHashCode() ^ System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(_binder);
        }

        [Confined]
        public override string ToString() {
            return base.ToString() + " " + SymbolTable.IdToString(_name);
        }

        #region IEquatable<OldMemberAction> Members

        [StateIndependent]
        public bool Equals(OldMemberAction other) {
            if (other == null) return false;
            if ((object)_binder != (object)other._binder) return false;
            return _name == other._name && Kind == other.Kind;
        }

        #endregion
    }
}
