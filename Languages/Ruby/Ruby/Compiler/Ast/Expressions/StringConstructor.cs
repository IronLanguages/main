/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using System.Diagnostics;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using MSAst = Microsoft.Scripting.Ast;
    
    public enum StringKind {
        Mutable,
        Symbol,
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
                    return TransformConcatentation(gen, _parts, StringFactory.Instance);

                case StringKind.Symbol:
                    return TransformConcatentation(gen, _parts, SymbolFactory.Instance);

                case StringKind.Command:
                    return CallSiteBuilder.InvokeMethod(gen.Context, "`", new RubyCallSignature(1, RubyCallFlags.HasScope | RubyCallFlags.HasImplicitSelf),
                        gen.CurrentScopeVariable,
                        gen.CurrentSelfVariable,
                        TransformConcatentation(gen, _parts, StringFactory.Instance)
                    );
            }

            throw Assert.Unreachable;
        }
        
        internal static MSA.Expression/*!*/ MakeConversion(AstGenerator/*!*/ gen, Expression/*!*/ expression) {
            return AstUtils.LightDynamic(ConvertToSAction.Make(gen.Context), typeof(MutableString), expression.TransformRead(gen));
        }

        #region Factories

        // TODO:

        internal interface IFactory {
            MSA.Expression/*!*/ CreateExpression(AstGenerator/*!*/ gen, string/*!*/ literal, RubyEncoding/*!*/ encoding);
            MSA.Expression/*!*/ CreateExpression(AstGenerator/*!*/ gen, byte[]/*!*/ literal, RubyEncoding/*!*/ encoding);
            MSA.Expression/*!*/ CreateExpressionN(AstGenerator/*!*/ gen, IEnumerable<MSA.Expression>/*!*/ args);
            MSA.Expression/*!*/ CreateExpressionM(AstGenerator/*!*/ gen, MSAst.ExpressionCollectionBuilder/*!*/ args);
        }

        private sealed class StringFactory : IFactory {
            public static readonly StringFactory Instance = new StringFactory();        
            
            public MSA.Expression/*!*/ CreateExpression(AstGenerator/*!*/ gen, string/*!*/ literal, RubyEncoding/*!*/ encoding) {
                return Methods.CreateMutableStringL.OpCall(Ast.Constant(literal), encoding.Expression);
            }

            public MSA.Expression/*!*/ CreateExpression(AstGenerator/*!*/ gen, byte[]/*!*/ literal, RubyEncoding/*!*/ encoding) {
                return Methods.CreateMutableStringB.OpCall(Ast.Constant(literal), encoding.Expression);
            }

            public MSA.Expression/*!*/ CreateExpressionN(AstGenerator/*!*/ gen, IEnumerable<MSA.Expression>/*!*/ args) {
                return Methods.CreateMutableString("N").OpCall(Ast.NewArrayInit(typeof(MutableString), args));
            }

            public MSA.Expression/*!*/ CreateExpressionM(AstGenerator/*!*/ gen, MSAst.ExpressionCollectionBuilder/*!*/ args) {
                string suffix = new String('M', args.Count);
                args.Add(gen.Encoding.Expression);
                return Methods.CreateMutableString(suffix).OpCall(args);
            }
        }

        internal sealed class SymbolFactory : IFactory {
            public static readonly SymbolFactory Instance = new SymbolFactory();

            public MSA.Expression/*!*/ CreateExpression(AstGenerator/*!*/ gen, string/*!*/ literal, RubyEncoding/*!*/ encoding) {
                return Ast.Constant(gen.Context.CreateSymbol(literal, encoding));
            }

            public MSA.Expression/*!*/ CreateExpression(AstGenerator/*!*/ gen, byte[]/*!*/ literal, RubyEncoding/*!*/ encoding) {
                return Ast.Constant(gen.Context.CreateSymbol(literal, encoding));
            }

            public MSA.Expression/*!*/ CreateExpressionN(AstGenerator/*!*/ gen, IEnumerable<MSA.Expression>/*!*/ args) {
                return Methods.CreateSymbol("N").OpCall(Ast.NewArrayInit(typeof(MutableString), args), gen.CurrentScopeVariable);
            }

            public MSA.Expression/*!*/ CreateExpressionM(AstGenerator/*!*/ gen, MSAst.ExpressionCollectionBuilder/*!*/ args) {
                string suffix = new String('M', args.Count);
                args.Add(gen.Encoding.Expression);
                args.Add(gen.CurrentScopeVariable);
                return Methods.CreateSymbol(suffix).OpCall(args);
            }
        }

        #endregion

        #region Literal Concatenation

        private sealed class LiteralConcatenation : List<object> {
            private RubyEncoding/*!*/ _encoding;
            private int _length;
            private bool _isBinary;

            private void ObjectInvariant() {
                ContractUtils.Invariant(_isBinary || CollectionUtils.TrueForAll(this, (item) => item is string));
            }

            public LiteralConcatenation(RubyEncoding/*!*/ sourceEncoding) {
                Assert.NotNull(sourceEncoding);
                _encoding = sourceEncoding;
            }

            public bool IsBinary {
                get { return _isBinary; } 
            }

            public RubyEncoding/*!*/ Encoding {
                get { return _encoding; }
            }

            public new void Clear() {
                _length = 0;
                base.Clear();
            }

            // Returns false if the literal can't be concatenated due to incompatible encodings.
            public bool Add(StringLiteral/*!*/ literal) {
                var concatEncoding = MutableString.GetCompatibleEncoding(_encoding, literal.Encoding);
                if (concatEncoding == null) {
                    return false;
                }

                var str = literal.Value as string;
                if (str != null) {
                    if (_isBinary) {
                        _length += literal.Encoding.Encoding.GetByteCount(str);
                    } else {
                        _length += str.Length;
                    }
                } else {
                    var bytes = (byte[])literal.Value;
                    if (!_isBinary) {
                        _length = 0;
                        foreach (object item in this) { 
                            Debug.Assert(item is string);
                            _length += _encoding.Encoding.GetByteCount((string)item);
                        }
                        _isBinary = true;
                    }

                    _length += bytes.Length;
                }

                _encoding = concatEncoding;
                base.Add(literal.Value);
                return true;
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
                            offset += _encoding.Encoding.GetBytes(str, 0, str.Length, result, offset);
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

        internal static MSA.Expression/*!*/ TransformConcatentation(AstGenerator/*!*/ gen, List<Expression>/*!*/ parts, IFactory/*!*/ factory) {

            // fast path for a single element:
            if (parts.Count == 1) {
                var literal = parts[0] as StringLiteral;
                if (literal != null) {
                    var str = literal.Value as string;
                    if (str != null) {
                        return factory.CreateExpression(gen, str, literal.Encoding);
                    } else {
                        return factory.CreateExpression(gen, (byte[])literal.Value, literal.Encoding);
                    }
                } else {
                    return factory.CreateExpressionM(gen, new MSAst.ExpressionCollectionBuilder { MakeConversion(gen, parts[0]) });
                }
            }

            var merged = new MSAst.ExpressionCollectionBuilder();
            var concat = new LiteralConcatenation(gen.Encoding);

            if (!ConcatLiteralsAndTransformRecursive(gen, parts, concat, merged)) {
                // TODO: we should emit Append calls directly, and not create an array first
                // TODO: MRI reports a syntax error
                // we can't concatenate due to encoding incompatibilities, report error at runtime:
                return factory.CreateExpressionN(gen, CollectionUtils.ConvertAll(parts, (e) => e.Transform(gen)));
            }

            // finish trailing literals:
            if (concat.Count > 0) {
                merged.Add(StringLiteral.Transform(concat.GetValue(), concat.Encoding));
            }

            if (merged.Count <= RubyOps.MakeStringParamCount) {
                if (merged.Count == 0) {
                    return factory.CreateExpression(gen, String.Empty, gen.Encoding);
                }

                return factory.CreateExpressionM(gen, merged);
            } else {
                // TODO: we should emit Append calls directly, and not create an array first
                return factory.CreateExpressionN(gen, merged);
            } 
        }

        //
        // Traverses expressions in "parts" and concats all contiguous literal strings/bytes.
        // Notes: 
        //  - We place the string/byte[] values that can be concatenated so far in to "concat" list that keeps track of their total length. 
        //    If we reach a non-literal expression and we have some literals ready in "concat" list we perform concat and clear the list.
        //  - "result" contains argument expressions to be passed to a CreateMutableString* overload.
        //  - "opName" contains the name of the operation. This method appends either "non-literal" suffix (for each expression) 
        //    or "encoding" suffix (for each concatenated literal).
        //
        // Returns false if the parts can't be concatenated due to encoding incompatibilities.
        // 
        private static bool ConcatLiteralsAndTransformRecursive(AstGenerator/*!*/ gen, List<Expression>/*!*/ parts,
            LiteralConcatenation/*!*/ concat, MSAst.ExpressionCollectionBuilder/*!*/ result) {

            for (int i = 0; i < parts.Count; i++) {
                Expression part = parts[i];
                StringLiteral literal;
                StringConstructor ctor;

                if ((literal = part as StringLiteral) != null) {
                    if (!concat.Add(literal)) {
                        return false;
                    }
                } else if ((ctor = part as StringConstructor) != null) {
                    if (!ConcatLiteralsAndTransformRecursive(gen, ctor.Parts, concat, result)) {
                        return false;
                    }
                } else {
                    if (concat.Count > 0) {
                        result.Add(StringLiteral.Transform(concat.GetValue(), concat.Encoding));
                        concat.Clear();
                    }

                    result.Add(MakeConversion(gen, part));
                }
            }

            return true;
        }

        #endregion
    }
}
