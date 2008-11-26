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
    public class OldDeleteMemberAction : OldMemberAction, IEquatable<OldDeleteMemberAction>, IExpressionSerializable {
        private OldDeleteMemberAction(ActionBinder binder, SymbolId name)
            : base(binder, name) {
        }

        public static OldDeleteMemberAction Make(ActionBinder binder, string name) {
            return Make(binder, SymbolTable.StringToId(name));
        }

        public static OldDeleteMemberAction Make(ActionBinder binder, SymbolId name) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return new OldDeleteMemberAction(binder, name);
        }

        public override DynamicActionKind Kind {
            get { return DynamicActionKind.DeleteMember; }
        }

        public Expression CreateExpression() {
            return Expression.Call(
                typeof(OldDeleteMemberAction).GetMethod("Make", new Type[] { typeof(ActionBinder), typeof(SymbolId) }),
                CreateActionBinderReadExpression(),
                AstUtils.Constant(Name)
            );
        }

        #region IEquatable<OldDeleteMemberAction> Members

        [StateIndependent]
        public bool Equals(OldDeleteMemberAction other) {
            if (other == null) return false;

            return base.Equals(other);
        }

        #endregion

    }
}
