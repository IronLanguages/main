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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Dynamic.Utils;
using Microsoft.Contracts;

namespace System.Dynamic {
    public abstract class InvokeMemberBinder : MetaObjectBinder {
        private readonly string _name;
        private readonly bool _ignoreCase;
        private readonly ReadOnlyCollection<ArgumentInfo> _arguments;

        protected InvokeMemberBinder(string name, bool ignoreCase, IEnumerable<ArgumentInfo> arguments) {
            _name = name;
            _ignoreCase = ignoreCase;
            _arguments = arguments.ToReadOnly();
        }

        protected InvokeMemberBinder(string name, bool ignoreCase, params ArgumentInfo[] arguments)
            : this(name, ignoreCase, (IEnumerable<ArgumentInfo>)arguments) {
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

        public ReadOnlyCollection<ArgumentInfo> Arguments {
            get {
                return _arguments;
            }
        }

        public sealed override MetaObject Bind(MetaObject target, MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems(args, "args");

            return target.BindInvokeMember(this, args);
        }

        public MetaObject FallbackInvokeMember(MetaObject target, MetaObject[] args) {
            return FallbackInvokeMember(target, args, null);
        }

        public abstract MetaObject FallbackInvokeMember(MetaObject target, MetaObject[] args, MetaObject errorSuggestion);
        public abstract MetaObject FallbackInvoke(MetaObject target, MetaObject[] args, MetaObject errorSuggestion);

        [Confined]
        public override bool Equals(object obj) {
            InvokeMemberBinder ca = obj as InvokeMemberBinder;
            return ca != null && ca._name == _name && ca._ignoreCase == _ignoreCase && ca._arguments.ListEquals(_arguments);
        }

        [Confined]
        public override int GetHashCode() {
            return InvokeMemberBinderHash ^ _name.GetHashCode() ^ (_ignoreCase ? 0x8000000 : 0) ^ _arguments.ListHashCode();
        }
    }
}
