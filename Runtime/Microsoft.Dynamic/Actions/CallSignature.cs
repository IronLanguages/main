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
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Scripting.Utils;
using Microsoft.Contracts;
using Microsoft.Scripting.Generation;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Richly represents the signature of a callsite.
    /// </summary>
    public struct CallSignature : IEquatable<CallSignature> {
        // TODO: invariant _infos != null ==> _argumentCount == _infos.Length
        
        /// <summary>
        /// Array of additional meta information about the arguments, such as named arguments.
        /// Null for a simple signature that's just an expression list. eg: foo(a*b,c,d)
        /// </summary>
        private readonly Argument[] _infos;

        /// <summary>
        /// Number of arguments in the signature.
        /// </summary>
        private readonly int _argumentCount;

        /// <summary>
        /// All arguments are unnamed and matched by position. 
        /// </summary>
        public bool IsSimple {
            get { return _infos == null; }
        }

        public int ArgumentCount {
            get {
                Debug.Assert(_infos == null || _infos.Length == _argumentCount);
                return _argumentCount; 
            }
        }

        #region Construction

        public CallSignature(CallSignature signature) {
            _infos = signature.GetArgumentInfos();
            _argumentCount = signature._argumentCount;
        }
        
        public CallSignature(int argumentCount) {
            ContractUtils.Requires(argumentCount >= 0, "argumentCount");
            _argumentCount = argumentCount;
            _infos = null;
        }

        public CallSignature(params Argument[] infos) {
            bool simple = true;

            if (infos != null) {
                _argumentCount = infos.Length;
                for (int i = 0; i < infos.Length; i++) {
                    if (infos[i].Kind != ArgumentType.Simple) {
                        simple = false;
                        break;
                    }
                }
            } else {
                _argumentCount = 0;
            }

            _infos = (!simple) ? infos : null;
        }

        public CallSignature(params ArgumentType[] kinds) {
            bool simple = true;

            if (kinds != null) {
                _argumentCount = kinds.Length;
                for (int i = 0; i < kinds.Length; i++) {
                    if (kinds[i] != ArgumentType.Simple) {
                        simple = false;
                        break;
                    }
                }
            } else {
                _argumentCount = 0;
            }

            if (!simple) {
                _infos = new Argument[kinds.Length];
                for (int i = 0; i < kinds.Length; i++) {
                    _infos[i] = new Argument(kinds[i]);
                }
            } else {
                _infos = null;
            }
        }

        #endregion

        #region IEquatable<CallSignature> Members

        [StateIndependent]
        public bool Equals(CallSignature other) {
            if (_infos == null) {
                return other._infos == null && other._argumentCount == _argumentCount;
            } else if (other._infos == null) {
                return false;
            }

            if (_infos.Length != other._infos.Length) return false;

            for (int i = 0; i < _infos.Length; i++) {
                if (!_infos[i].Equals(other._infos[i])) return false;
            }

            return true;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj) {
            return obj is CallSignature && Equals((CallSignature)obj);
        }

        public static bool operator ==(CallSignature left, CallSignature right) {
            return left.Equals(right);
        }

        public static bool operator !=(CallSignature left, CallSignature right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            if (_infos == null) {
                return "Simple";
            }
            
            StringBuilder sb = new StringBuilder("(");
            for (int i = 0; i < _infos.Length; i++) {
                if (i > 0) {
                    sb.Append(", ");
                }
                sb.Append(_infos[i].ToString());
            }
            sb.Append(")");
            return sb.ToString();
        }

        public override int GetHashCode() {
            int h = 6551;
            if (_infos != null) {
                foreach (Argument info in _infos) {
                    h ^= (h << 5) ^ info.GetHashCode();
                }
            }
            return h;
        }

        #endregion

        #region Helpers

        public Argument[] GetArgumentInfos() {
            return (_infos != null) ? ArrayUtils.Copy(_infos) : CompilerHelpers.MakeRepeatedArray(Argument.Simple, _argumentCount);
        }

        public CallSignature InsertArgument(Argument info) {
            return InsertArgumentAt(0, info);
        }

        public CallSignature InsertArgumentAt(int index, Argument info) {
            if (this.IsSimple) {
                if (info.IsSimple) {
                    return new CallSignature(_argumentCount + 1);
                }
                
                return new CallSignature(ArrayUtils.InsertAt(GetArgumentInfos(), index, info));
            }

            return new CallSignature(ArrayUtils.InsertAt(_infos, index, info));
        }

        public CallSignature RemoveFirstArgument() {
            return RemoveArgumentAt(0);
        }

        public CallSignature RemoveArgumentAt(int index) {
            if (_argumentCount == 0) {
                throw new InvalidOperationException();
            }

            if (IsSimple) {
                return new CallSignature(_argumentCount - 1);
            }

            return new CallSignature(ArrayUtils.RemoveAt(_infos, index));
        }

        public int IndexOf(ArgumentType kind) {
            if (_infos == null) {
                return (kind == ArgumentType.Simple && _argumentCount > 0) ? 0 : -1;
            }

            for (int i = 0; i < _infos.Length; i++) {
                if (_infos[i].Kind == kind) {
                    return i;
                }
            }
            return -1;
        }

        public bool HasDictionaryArgument() {
            return IndexOf(ArgumentType.Dictionary) > -1;
        }

        public bool HasInstanceArgument() {
            return IndexOf(ArgumentType.Instance) > -1;
        }

        public bool HasListArgument() {
            return IndexOf(ArgumentType.List) > -1;
        }

        internal bool HasNamedArgument() {
            return IndexOf(ArgumentType.Named) > -1;
        }

        /// <summary>
        /// True if the OldCallAction includes an ArgumentInfo of ArgumentKind.Dictionary or ArgumentKind.Named.
        /// </summary>
        public bool HasKeywordArgument() {
            if (_infos != null) {
                foreach (Argument info in _infos) {
                    if (info.Kind == ArgumentType.Dictionary || info.Kind == ArgumentType.Named) {
                        return true;
                    }
                }
            }
            return false;
        }
        
        public ArgumentType GetArgumentKind(int index) {
            // TODO: Contract.Requires(index >= 0 && index < _argumentCount, "index");
            return _infos != null ? _infos[index].Kind : ArgumentType.Simple;
        }

        public string GetArgumentName(int index) {
            ContractUtils.Requires(index >= 0 && index < _argumentCount);
            return _infos != null ? _infos[index].Name : null;
        }

        /// <summary>
        /// Gets the number of positional arguments the user provided at the call site.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetProvidedPositionalArgumentCount() {
            int result = _argumentCount;

            if (_infos != null) {
                for (int i = 0; i < _infos.Length; i++) {
                    ArgumentType kind = _infos[i].Kind;

                    if (kind == ArgumentType.Dictionary || kind == ArgumentType.List || kind == ArgumentType.Named) {
                        result--;
                    }
                }
            }

            return result;
        }

        public string[] GetArgumentNames() {
            if (_infos == null) {
                return ArrayUtils.EmptyStrings;
            }

            List<string> result = new List<string>();
            foreach (Argument info in _infos) {
                if (info.Name != null) {
                    result.Add(info.Name);
                }
            }

            return result.ToArray();
        }

        public Expression CreateExpression() {            
            if (_infos == null) {
                return Expression.New(
                    typeof(CallSignature).GetConstructor(new Type[] { typeof(int) }),
                    AstUtils.Constant(ArgumentCount)
                );
            } else {
                Expression[] args = new Expression[_infos.Length];
                for (int i = 0; i < args.Length; i++) {
                    args[i] = _infos[i].CreateExpression();
                }
                return Expression.New(
                    typeof(CallSignature).GetConstructor(new Type[] { typeof(Argument[]) }), 
                    Expression.NewArrayInit(typeof(Argument), args)
                );
            }
        }

        #endregion
    }
}
