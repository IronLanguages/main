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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using Microsoft.Contracts;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// TODO: Alternatively, it should be sufficient to remember indices for this, list, dict and block.
    /// </summary>
    public struct Argument : IEquatable<Argument> {
        private readonly ArgumentType _kind;
        private readonly string _name;

        public static readonly Argument Simple = new Argument(ArgumentType.Simple, null);

        public ArgumentType Kind { get { return _kind; } }
        public string Name { get { return _name; } }

        public Argument(string name) {
            _kind = ArgumentType.Named;
            _name = name;
        }

        public Argument(ArgumentType kind) {
            _kind = kind;
            _name = null;
        }

        public Argument(ArgumentType kind, string name) {
            ContractUtils.Requires((kind == ArgumentType.Named) ^ (name == null), "kind");
            _kind = kind;
            _name = name;
        }

        public override bool Equals(object obj) {
            return obj is Argument && Equals((Argument)obj);
        }

        [StateIndependent]
        public bool Equals(Argument other) {
            return _kind == other._kind && _name == other._name;
        }

        public static bool operator ==(Argument left, Argument right) {
            return left.Equals(right);
        }

        public static bool operator !=(Argument left, Argument right) {
            return !left.Equals(right);
        }

        public override int GetHashCode() {
            return (_name != null) ? _name.GetHashCode() ^ (int)_kind : (int)_kind;
        }

        public bool IsSimple {
            get {
                return Equals(Simple);
            }
        }

        public override string ToString() {
            return _name == null ? _kind.ToString() : _kind.ToString() + ":" + _name;
        }

        internal Expression CreateExpression() {
            return Expression.New(
                typeof(Argument).GetConstructor(new Type[] { typeof(ArgumentType), typeof(string) }),
                AstUtils.Constant(_kind),
                AstUtils.Constant(_name, typeof(string))
            );
        }
    }
}


