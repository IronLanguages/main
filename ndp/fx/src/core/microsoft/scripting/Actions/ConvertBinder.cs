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
using Microsoft.Contracts;

namespace System.Dynamic {
    public abstract class ConvertBinder : MetaObjectBinder {
        private readonly Type _type;
        private readonly bool _explicit;

        protected ConvertBinder(Type type, bool @explicit) {
            _type = type;
            _explicit = @explicit;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type {
            get {
                return _type;
            }
        }

        public bool Explicit {
            get {
                return _explicit;
            }
        }

        public MetaObject FallbackConvert(MetaObject target) {
            return FallbackConvert(target, null);
        }

        public abstract MetaObject FallbackConvert(MetaObject target, MetaObject errorSuggestion);

        public sealed override MetaObject Bind(MetaObject target, MetaObject[] args) {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.Requires(args.Length == 0);

            return target.BindConvert(this);
        }

        [Confined]
        public override bool Equals(object obj) {
            ConvertBinder ca = obj as ConvertBinder;
            return ca != null && ca._type == _type && ca._explicit == _explicit;
        }

        [Confined]
        public override int GetHashCode() {
            return ConvertBinderHash ^ _type.GetHashCode() ^ (_explicit ? 0x8000000 : 0);
        }
    }
}
