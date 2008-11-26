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

namespace System.Linq.Expressions {
    public enum ArgumentKind {
        Positional,
        Named
    }

    public abstract class ArgumentInfo {
        internal ArgumentInfo() {
        }

        public ArgumentKind ArgumentType {
            get { return GetArgumentType(); }
        }

        internal abstract ArgumentKind GetArgumentType();
    }

    public sealed class PositionalArgumentInfo : ArgumentInfo {
        private readonly int _position;

        internal PositionalArgumentInfo(int position) {
            _position = position;
        }

        public int Position {
            get { return _position; }
        }

        internal override ArgumentKind GetArgumentType() {
            return ArgumentKind.Positional;
        }

        [Confined]
        public override bool Equals(object obj) {
            PositionalArgumentInfo arg = obj as PositionalArgumentInfo;
            return arg != null && arg._position == _position;
        }

        [Confined]
        public override int GetHashCode() {
            return _position;
        }
    }

    public sealed class NamedArgumentInfo : ArgumentInfo {
        private readonly string _name;

        internal NamedArgumentInfo(string name) {
            _name = name;
        }

        public string Name {
            get { return _name; }
        }

        internal override ArgumentKind GetArgumentType() {
            return ArgumentKind.Named;
        }

        [Confined]
        public override bool Equals(object obj) {
            NamedArgumentInfo arg = obj as NamedArgumentInfo;
            return arg != null && arg._name == _name;
        }

        [Confined]
        public override int GetHashCode() {
            return _name.GetHashCode();
        }
    }

    public partial class Expression {
        public static PositionalArgumentInfo PositionalArg(int position) {
            ContractUtils.Requires(position >= 0, "position", Strings.MustBePositive);
            return new PositionalArgumentInfo(position);
        }
        public static NamedArgumentInfo NamedArg(string name) {
            // TODO: should we allow the empty string?
            ContractUtils.Requires(!string.IsNullOrEmpty(name), "name");
            return new NamedArgumentInfo(name);
        }
    }
}
