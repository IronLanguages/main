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

namespace System.Dynamic.Binders {
    public abstract class SetMemberBinder : MetaObjectBinder {
        private readonly string _name;
        private readonly bool _ignoreCase;

        protected SetMemberBinder(string name, bool ignoreCase) {
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

        public sealed override MetaObject Bind(MetaObject target, MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");
            ContractUtils.Requires(args.Length == 1);

            return target.BindSetMember(this, args[0]);
        }

        public MetaObject FallbackSetMember(MetaObject target, MetaObject value) {
            return FallbackSetMember(target, value, null);
        }

        public abstract MetaObject FallbackSetMember(MetaObject target, MetaObject value, MetaObject errorSuggestion);

        public override int GetHashCode() {
            return SetMemberBinderHash ^ _name.GetHashCode() ^ (_ignoreCase ? 0x8000000 : 0);
        }

        public override bool Equals(object obj) {
            SetMemberBinder sa = obj as SetMemberBinder;
            return sa != null && sa._name == _name && sa._ignoreCase == _ignoreCase;
        }
    }
}
