/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    using MSA = System.Linq.Expressions;
    using System.Diagnostics;

    public enum StringKind {
        Mutable,
        Immutable,
        Command
    }

    /// <summary>
    /// Sequence of string literals and/or string embedded expressions.
    /// </summary>
    public partial class StringConstructor : Expression {
        private readonly StringKind _kind;
        private readonly List<Expression>/*!*/ _parts;

        public StringKind Kind {
            get { return _kind; }
        }

        public List<Expression>/*!*/ Parts {
            get { return _parts; }
        }

        public StringConstructor(List<Expression>/*!*/ parts, StringKind kind, SourceSpan location) 
            : base(location) {
            ContractUtils.RequiresNotNullItems(parts, "parts");

            _parts = parts;
            _kind = kind;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            switch (_kind) {
                case StringKind.Mutable:
                    return TransformConcatentation(gen, _parts, Methods.CreateMutableString);

                case StringKind.Immutable:
                    return TransformConcatentation(gen, _parts, Methods.CreateSymbol);

                case StringKind.Command:
                    return Ast.Dynamic(RubyCallAction.Make("`", new RubyCallSignature(1, RubyCallFlags.HasScope | RubyCallFlags.HasImplicitSelf)), typeof(object),
                        gen.CurrentScopeVariable,
                        gen.CurrentSelfVariable,
                        TransformConcatentation(gen, _parts, Methods.CreateMutableString)
                    );
            }

            throw Assert.Unreachable;
        }

        internal static MSA.Expression/*!*/ MakeConstant(object/*!*/ value) {
            // TODO: readonly byte[] ?
            return AstUtils.Constant(value);
        }
        
        internal static MSA.Expression/*!*/ MakeConversion(AstGenerator/*!*/ gen, Expression/*!*/ expression) {
            return Ast.Dynamic(
                ConvertToSAction.Instance,
                typeof(MutableString),
                Methods.GetContextFromScope.OpCall(gen.CurrentScopeVariable), expression.TransformRead(gen)
            );
        }

        #region Literal Concatenation

        private sealed class LiteralConcatenation : List<object> {
            private readonly Encoding/*!*/ _encoding;
            private int _length;
            private bool _isBinary;

            private void ObjectInvariant() {
                ContractUtils.Invariant(_isBinary || CollectionUtils.TrueForAll(this, (item) => item is string));
            }

            public LiteralConcatenation(Encoding/*!*/ encoding) {
                Assert.NotNull(encoding);
                _encoding = encoding;
            }

            public bool IsBinary {
                get { return _isBinary; } 
            }

            public new void Clear() {
                _length = 0;
                base.Clear();
            }

            public void Add(StringLiteral/*!*/ literal) {
                var str = literal.Value as string;
                if (str != null) {
                    if (_isBinary) {
                        _length += _encoding.GetByteCount(str);
                    } else {
                        _length += str.Length;
                    }
                } else {
                    var bytes = (byte[])literal.Value;
                    if (!_isBinary) {
                        _length = 0;
                        foreach (object item in this) { 
                            Debug.Assert(item is string);
                            _length += _encoding.GetByteCount((string)item);
                        }
                        _isBinary = true;
                    }

                    _length += bytes.Length;
                }

                base.Add(literal.Value);
            }

            public object/*!*/ GetValue() {
                ContractUtils.Ensures(ContractUtils.Result<object>() is byte[] || ContractUtils.Result<object>() is string);

                if (Count == 1) {
                    return this[0];
                }

                if (_isBinary) {
                    int offset = 0;
                    var result = new byte[_length];

                    foreach (object item in this) {
                        byte[] bytes = item as byte[];
                        if (bytes != null) {
                            Buffer.BlockCopy(bytes, 0, result, offset, bytes.Length);
                            offset += bytes.Length;
                        } else {
                            string str = (string)item;
                            offset += _encoding.GetBytes(str, 0, str.Length, result, offset);
                        }
                    }

                    Debug.Assert(offset == result.Length);
                    return result;
                } else {
                    var result = new StringBuilder(_length);
                    foreach (string item in this) {
                        result.Append(item);
                    }
                    return result.ToString();
                }
            }
        }

        internal static MSA.Expression/*!*/ TransformConcatentation(AstGenerator/*!*/ gen, List<Expression>/*!*/ parts, Func<string, MethodInfo>/*!*/ opFactory) {
            return TransformConcatentation(gen, parts, opFactory, null, null);
        }

        internal static MSA.Expression/*!*/ TransformConcatentation(AstGenerator/*!*/ gen, List<Expression>/*!*/ parts,
            Func<string, MethodInfo>/*!*/ opFactory, MSA.Expression regexOptions, MSA.Expression regexCache) {

            ContractUtils.Requires((regexOptions == null) == (regexCache == null));

            // fast path for a single element:
            if (parts.Count == 1) {
                var literal = parts[0] as StringLiteral;
                if (literal != null) {
                    var str = literal.Value as string;
                    if (str != null) {
                        return MakeFactoryCall(gen, opFactory("L"), Ast.Constant(str), regexOptions, regexCache);
                    }
                } else {
                    return MakeFactoryCall(gen, opFactory("M"), MakeConversion(gen, parts[0]), regexOptions, regexCache);
                }
            }

            var opSuffix = new StringBuilder(Math.Min(parts.Count, 4));

            bool anyBinary;
            List<MSA.Expression> merged = ConcatLiteralsAndTransform(gen, parts, opSuffix, out anyBinary);

            // TODO (opt): We don't to optimize for binary strings, we can if it is needed.
            if (!anyBinary && merged.Count <= RubyOps.MakeStringParamCount) {
                if (merged.Count == 0) {
                    merged.Add(AstUtils.Constant(String.Empty));
                    opSuffix.Append(RubyOps.SuffixLiteral);
                }

                merged.Add(AstUtils.Constant(gen.Encoding));

                if (regexOptions != null) {
                    merged.Add(regexOptions);
                    merged.Add(regexCache);
                }

                return opFactory(opSuffix.ToString()).OpCall(merged);
            } else {
                return MakeFactoryCall(gen, opFactory("N"), Ast.NewArrayInit(typeof(object), merged), regexOptions, regexCache);
            } 
        }

        private static MSA.Expression/*!*/ MakeFactoryCall(AstGenerator/*!*/ gen, MethodInfo/*!*/ factory, MSA.Expression/*!*/ arg, 
            MSA.Expression regexOptions, MSA.Expression regexCache) {

            if (regexOptions == null) {
                return factory.OpCall(arg, AstUtils.Constant(gen.Encoding));
            } else {
                return factory.OpCall(arg, AstUtils.Constant(gen.Encoding), regexOptions, regexCache);
            }
        }

        private static List<MSA.Expression>/*!*/ ConcatLiteralsAndTransform(AstGenerator/*!*/ gen, List<Expression>/*!*/ parts, 
            StringBuilder/*!*/ opName, out bool anyBinary) {

            var result = new List<MSA.Expression>();
            var concat = new LiteralConcatenation(gen.Encoding.Encoding);
            anyBinary = false;
            ConcatLiteralsAndTransformRecursive(gen, parts, concat, result, opName, ref anyBinary);

            // finish trailing literals:
            if (concat.Count > 0) {
                result.Add(MakeConstant(concat.GetValue()));
                opName.Append(RubyOps.SuffixLiteral);
                anyBinary |= concat.IsBinary;
            }

            return result;
        }

        //
        // Traverses expressions in "parts" and concats all contiguous literal strings/bytes.
        // Notes: 
        //  - We place the string/byte[] values that can be concatenated so far in to "concat" list thats keep track of their total length. 
        //    If we reach a non-literal expression and we have some literals ready in "concat" list we perform concat and clear the list.
        //  - "result" contains argument expressions to be passed to a CreateMutableString* overload.
        //  - "opName" contains the name of the operation. This method appends either "non-literal" suffix (for each expression) 
        //    or "encoding" suffix (for each concatenated literal).
        //  - "anyBinary" keeps track of whether any iteral visited so far is binary (byte[]).
        //
        private static void ConcatLiteralsAndTransformRecursive(AstGenerator/*!*/ gen, List<Expression>/*!*/ parts,
            LiteralConcatenation/*!*/ concat, List<MSA.Expression>/*!*/ result, StringBuilder/*!*/ opName, ref bool anyBinary) {

            for (int i = 0; i < parts.Count; i++) {
                Expression part = parts[i];
                StringLiteral literal;
                StringConstructor ctor;

                if ((literal = part as StringLiteral) != null) {
                    concat.Add(literal);                    
                } else if ((ctor = part as StringConstructor) != null) {
                    ConcatLiteralsAndTransformRecursive(gen, ctor.Parts, concat, result, opName, ref anyBinary);
                } else {
                    if (concat.Count > 0) {
                        result.Add(MakeConstant(concat.GetValue()));
                        opName.Append(RubyOps.SuffixLiteral);
                        anyBinary |= concat.IsBinary;
                        concat.Clear();
                    }

                    result.Add(MakeConversion(gen, part));
                    opName.Append(RubyOps.SuffixMutable);
                }
            }
        }

        #endregion
    }
}
