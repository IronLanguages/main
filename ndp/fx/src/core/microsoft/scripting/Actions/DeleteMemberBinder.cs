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
    public abstract class DeleteMemberBinder : MetaObjectBinder {
        private readonly string _name;
        private readonly bool _ignoreCase;

        protected DeleteMemberBinder(string name, bool ignoreCase) {
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

        public MetaObject FallbackDeleteMember(MetaObject target) {
            return FallbackDeleteMember(target, null);
        }

        public abstract MetaObject FallbackDeleteMember(MetaObject target, MetaObject errorSuggestion);

        public sealed override MetaObject Bind(MetaObject target, MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.Requires(args.Length == 0);

            return target.BindDeleteMember(this);
        }

        public override int GetHashCode() {
            return DeleteMemberBinderHash ^ _name.GetHashCode() ^ (_ignoreCase ? 0x8000000 : 0); ;
        }

        public override bool Equals(object obj) {
            DeleteMemberBinder dma = obj as DeleteMemberBinder;
            return dma != null && dma._name == _name && dma._ignoreCase == _ignoreCase;
        }
    }
}
