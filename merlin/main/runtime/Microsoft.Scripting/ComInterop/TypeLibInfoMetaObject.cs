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

#if !SILVERLIGHT

using System.Collections.Generic;
using System.Dynamic.Binders;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.ComInterop {

    internal sealed class TypeLibInfoMetaObject : MetaObject {
        private readonly ComTypeLibInfo _info;

        internal TypeLibInfoMetaObject(Expression expression, ComTypeLibInfo info)
            : base(expression, Restrictions.Empty, info) {
            _info = info;
        }

        public override MetaObject BindGetMember(GetMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            string name = binder.Name;

            if (name == _info.Name) {
                name = "TypeLibDesc";
            } else if (name != "Guid" &&
                name != "Name" &&
                name != "VersionMajor" &&
                name != "VersionMinor") {

                return binder.FallbackGetMember(this);
            }

            return new MetaObject(
                Expression.Property(
                    AstUtils.Convert(Expression, typeof(ComTypeLibInfo)),
                    typeof(ComTypeLibInfo).GetProperty(name)
                ),
                ComTypeLibInfoRestrictions(this)
            );
        }

        public override IEnumerable<string> GetDynamicMemberNames() {
            return _info.GetMemberNames();
        }

        private Restrictions ComTypeLibInfoRestrictions(params MetaObject[] args) {
            return Restrictions.Combine(args).Merge(Restrictions.GetTypeRestriction(Expression, typeof(ComTypeLibInfo)));
        }

        private MetaObject RestrictThisToType() {
            return new MetaObject(
                AstUtils.Convert(
                    Expression,
                    typeof(ComTypeLibInfo)
                ),
                Restrictions.GetTypeRestriction(Expression, typeof(ComTypeLibInfo))
            );
        }
    }
}

#endif
