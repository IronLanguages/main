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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {

    public class OldSetMemberAction : OldMemberAction, IExpressionSerializable {
        public static OldSetMemberAction Make(ActionBinder binder, string name) {
            return Make(binder, SymbolTable.StringToId(name));
        }

        public static OldSetMemberAction Make(ActionBinder binder, SymbolId name) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return new OldSetMemberAction(binder, name);
        }

        private OldSetMemberAction(ActionBinder binder, SymbolId name)
            : base(binder, name) {
        }

        public override DynamicActionKind Kind {
            get { return DynamicActionKind.SetMember; }
        }

        public Expression CreateExpression() {
            return Expression.Call(
                typeof(OldSetMemberAction).GetMethod("Make", new Type[] { typeof(ActionBinder), typeof(SymbolId) }),
                CreateActionBinderReadExpression(),
                AstUtils.Constant(Name)
            );
        }

    }
}
