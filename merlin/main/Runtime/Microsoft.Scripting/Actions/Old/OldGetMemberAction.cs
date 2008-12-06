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
using System.Linq.Expressions;
using Microsoft.Contracts;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    [Flags]
    public enum GetMemberBindingFlags {
        /// <summary>
        /// No member binding flags
        /// </summary>
        None,
        /// <summary>
        /// The result of the get should produce a value that is bound to the instance it was extracted from, if possible.
        /// </summary>
        Bound = 0x01,
        /// <summary>
        /// Instead of throwing the binder will return OperationFailed.Value if the member does not exist or is write-only.
        /// </summary>
        NoThrow,
    }

    public class OldGetMemberAction : OldMemberAction, IEquatable<OldGetMemberAction>, IExpressionSerializable {
        private readonly GetMemberBindingFlags _flags;

        public static OldGetMemberAction Make(ActionBinder binder, string name) {
            return Make(binder, SymbolTable.StringToId(name), GetMemberBindingFlags.Bound);
        }

        public static OldGetMemberAction Make(ActionBinder binder, SymbolId name) {
            return Make(binder, name, GetMemberBindingFlags.Bound);
        }

        public static OldGetMemberAction Make(ActionBinder binder, string name, GetMemberBindingFlags bindingFlags) {
            return Make(binder, SymbolTable.StringToId(name), bindingFlags);
        }

        public static OldGetMemberAction Make(ActionBinder binder, SymbolId name, GetMemberBindingFlags bindingFlags) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return new OldGetMemberAction(binder, name, bindingFlags);
        }

        private OldGetMemberAction(ActionBinder binder, SymbolId name, GetMemberBindingFlags bindingFlags)
            : base(binder, name) {
            _flags = bindingFlags;
        }

        public override DynamicActionKind Kind {
            get { return DynamicActionKind.GetMember; }
        }

        [Confined]
        public override bool Equals(object obj) {
            return Equals(obj as OldGetMemberAction);
        }

        [Confined]
        public override int GetHashCode() {
            return base.GetHashCode() ^ _flags.GetHashCode();
        }

        public bool IsBound {
            get {
                return (_flags & GetMemberBindingFlags.Bound) != 0;
            }
        }

        public bool IsNoThrow {
            get {
                return (_flags & GetMemberBindingFlags.NoThrow) != 0;
            }
        }

        public Expression CreateExpression() {
            return Expression.Call(
                typeof(OldGetMemberAction).GetMethod("Make", new Type[] { typeof(ActionBinder), typeof(SymbolId), typeof(GetMemberBindingFlags) }),
                CreateActionBinderReadExpression(),
                AstUtils.Constant(Name),
                Expression.Constant(_flags)
            );
        }

        #region IEquatable<OldGetMemberAction> Members

        [StateIndependent]
        public bool Equals(OldGetMemberAction other) {
            if (other == null) return false;
            return other.Name == Name && other._flags == _flags && base.Equals(other);
        }

        #endregion
    }
}
