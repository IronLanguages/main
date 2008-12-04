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

        public bool IsByRef {
            get {
                return GetIsByRef();
            }
        }

        public ArgumentKind ArgumentType {
            get { return GetArgumentType(); }
        }

        internal abstract ArgumentKind GetArgumentType();
        internal abstract bool GetIsByRef();
    }

    public class PositionalArgumentInfo : ArgumentInfo {
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

        internal override bool GetIsByRef() {
            return false;
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

    internal sealed class ByRefPositionalArgumentInfo : PositionalArgumentInfo {
        internal ByRefPositionalArgumentInfo(int position)
            : base(position) {
        }

        internal override bool GetIsByRef() {
            return true;
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ unchecked((int)0x80000000);
        }

        public override bool Equals(object obj) {
            ByRefPositionalArgumentInfo arg = obj as ByRefPositionalArgumentInfo;
            return arg != null && arg.Position == Position;
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

        internal override bool GetIsByRef() {
            return false;
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
        /// <summary>
        /// Returns a new PositionalArgumentInfo that represents a positional argument in the dynamic binding process.
        /// </summary>
        /// <param name="position">A position of the argument in the call signature.</param>
        /// <returns>The new PositionalArgumentInfo.</returns>
        public static PositionalArgumentInfo PositionalArg(int position) {
            ContractUtils.Requires(position >= 0, "position", Strings.MustBePositive);
            return new PositionalArgumentInfo(position);
        }

        /// <summary>
        /// Returns a new NamedArgumentInfo that represents a named argument in the dynamic binding process.
        /// </summary>
        /// <param name="name">The name of the argument at the call site.</param>
        /// <returns>The new NamedArgumentInfo.</returns>
        public static NamedArgumentInfo NamedArg(string name) {
            ContractUtils.Requires(!string.IsNullOrEmpty(name), "name");
            return new NamedArgumentInfo(name);
        }

        /// <summary>
        /// Returns a new PositionalArgumentInfo that represents a positional by ref argument in the dynamic binding process.
        /// </summary>
        /// <param name="position">A position of the argument in the call signature.</param>
        /// <returns>The new PositionalArgumentInfo.</returns>
        public static PositionalArgumentInfo ByRefArgument(int position) {
            ContractUtils.Requires(position >= 0, "position", Strings.MustBePositive);
            return new ByRefPositionalArgumentInfo(position);
        }
    }
}
