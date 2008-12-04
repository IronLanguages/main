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

using Microsoft.Contracts;
using System.Dynamic.Utils;

namespace System.Dynamic {
    public abstract class GetMemberBinder : MetaObjectBinder {
        private readonly string _name;
        private readonly bool _ignoreCase;

        protected GetMemberBinder(string name, bool ignoreCase) {
            ContractUtils.RequiresNotNull(name, "name");

            _name = name;
            _ignoreCase = ignoreCase;
        }

        public string Name {
            get {
                return _name;
            }
        }

        public bool IgnoreCase {
            get {
                return _ignoreCase;
            }
        }

        public MetaObject FallbackGetMember(MetaObject target) {
            return FallbackGetMember(target, null);
        }

        public abstract MetaObject FallbackGetMember(MetaObject target, MetaObject errorSuggestion);

        public sealed override MetaObject Bind(MetaObject target, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.Requires(args.Length == 0);

            return target.BindGetMember(this);
        }

        [Confined]
        public override bool Equals(object obj) {
            GetMemberBinder gma = obj as GetMemberBinder;
            return gma != null && gma._name == _name && gma._ignoreCase == _ignoreCase;
        }

        [Confined]
        public override int GetHashCode() {
            return GetMemberBinderHash ^ _name.GetHashCode() ^ (_ignoreCase ? unchecked((int)0x80000000) : 0);
        }
    }
}
