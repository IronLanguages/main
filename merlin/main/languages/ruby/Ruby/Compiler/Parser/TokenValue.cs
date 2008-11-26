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
using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Compiler.Ast;
using System.Diagnostics;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler {

    public enum TokenValueType {
        None = 0,
        String, 
        Integer,
        BigInteger,
        Double,
        RegexOptions,
        StringTokenizer,
    }

    // TODO: 
    // [StructLayout(LayoutKind.Explicit)]
    public partial struct TokenValue {
        // TODO: debug only
        public TokenValueType Type { get { return _type; } }
        private TokenValueType _type;

        // [FieldOffset(0)]
        private int _integer1;

        // [FieldOffset(0)]
        private double _double;

        // [FieldOffset(8)]
        public object obj;

        // [FieldOffset(12)]
        public Node node;

        public TokenValue(Arguments arguments, Block block) {
            _integer1 = 0;
            _double = 0.0;
            node = arguments;
            obj = block;
            _type = TokenValueType.None;
        }

        public TokenValue(List<Expression> comparisons, Expression comparisonArray) {
            _integer1 = 0;
            _double = 0.0;
            node = comparisonArray;
            obj = comparisons;
            _type = TokenValueType.None;
        }

        public int Integer { get { return _integer1; } set { _integer1 = value; } }
        public double Double { get { return _double; } set { _double = value; } }

        // Tokens: StringContent
        internal StringLiteralEncoding/*!*/ StringLiteralEncoding { get { return (StringLiteralEncoding)_integer1; } }
        
        // Tokens: StringBegin, SymbolBegin, RegexBegin, ShellStringBegin
        internal StringTokenizer/*!*/ StringTokenizer { get { return (StringTokenizer)obj; } set { obj = value; } }

        internal int VariableFactory { get { return _integer1; } set { _integer1 = value; } }

        public RubyRegexOptions RegExOptions { get { return (RubyRegexOptions)Integer; } set { Integer = (int)value; } }
        public CallExpression CallExpression { get { return (CallExpression)node; } set { node = value; } }
        public ElseIfClause ElseIfClause { get { return (ElseIfClause)node; } set { node = value; } }
        public RescueClause RescueClause { get { return (RescueClause)node; } set { node = value; } }
        public WhenClause WhenClause { get { return (WhenClause)node; } set { node = value; } }
        
        public Arguments Arguments { get { return (Arguments)node; } set { node = value; } }
        public Block Block { get { return (Block)obj; } set { obj = value; } }

        public Expression Expression { get { return (Expression)node; } set { node = value; } }
        public List<Expression>/*!*/ Expressions { get { return (List<Expression>)obj; } set { obj = value; } }

        public BlockReference BlockReference { get { return (BlockReference)node; } set { node = value; } }
        public BlockDefinition BlockDefinition { get { return (BlockDefinition)node; } set { node = value; } }
        public ConstantVariable ConstantVariable { get { return (ConstantVariable)node; } set { node = value; } }
        public Maplet Maplet { get { return (Maplet)node; } set { node = value; } }
        public BigInteger/*!*/ BigInteger { get { return (BigInteger)obj; } set { obj = value; } }
        public String/*!*/ String { get { return (String)obj; } set { obj = value; } }
        public Parameters Parameters { get { return (Parameters)node; } set { node = value; } }
        public LocalVariable LocalVariable { get { return (LocalVariable)node; } set { node = value; } }
        public SimpleAssignmentExpression SimpleAssignmentExpression { get { return (SimpleAssignmentExpression)node; } set { node = value; } }
        public LeftValue LeftValue { get { return (LeftValue)node; } set { node = value; } }
        public CompoundLeftValue CompoundLeftValue { get { return (CompoundLeftValue)node; } set { node = value; } }
        public Body Body { get { return (Body)node; } set { node = value; } }
        public CompoundRightValue CompoundRightValue { get { return (CompoundRightValue)obj; } set { obj = value; } }
        public JumpStatement JumpStatement { get { return (JumpStatement)obj; } set { obj = value; } }
        public RegexMatchReference RegexMatchReference { get { return (RegexMatchReference)obj; } set { obj = value; } }

        public List<LeftValue> LeftValues { get { return (List<LeftValue>)obj; } set { obj = value; } }
        public List<Identifier>/*!*/ Identifiers { get { return (List<Identifier>)obj; } set { obj = value; } }
        public List<ElseIfClause>/*!*/ ElseIfClauses { get { return (List<ElseIfClause>)obj; } set { obj = value; } }
        public List<WhenClause>/*!*/ WhenClauses { get { return (List<WhenClause>)obj; } set { obj = value; } }
        public List<LeftValue>/*!*/ LeftValueList { get { return (List<LeftValue>)obj; } set { obj = value; } }
        public List<Expression>/*!*/ Statements { get { return (List<Expression>)obj; } set { obj = value; } }
        public List<RescueClause> RescueClauses { get { return (List<RescueClause>)obj; } set { obj = value; } }
        public List<Maplet> Maplets { get { return (List<Maplet>)obj; } set { obj = value; } }
        public List<LocalVariable>/*!*/ LocalVariables { get { return (List<LocalVariable>)obj; } set { obj = value; } }
        public List<SimpleAssignmentExpression>/*!*/ SimpleAssignmentExpressions { get { return (List<SimpleAssignmentExpression>)obj; } set { obj = value; } }
        
        internal void SetInteger(int value) {
            Integer = value;
            _type = TokenValueType.Integer;
        }

        internal void SetBigInteger(BigInteger value) {
            BigInteger = value;
            _type = TokenValueType.BigInteger;
        }

        internal void SetDouble(double value) {
            Double = value;
            _type = TokenValueType.Double;
        }

        internal void SetString(string/*!*/ value, StringLiteralEncoding encoding) {
            Assert.NotNull(value);
            String = value;
            _integer1 = (int)encoding;
            _type = TokenValueType.String;
        }

        internal void SetString(string/*!*/ value, bool hasUnicodeEscape) {
            SetString(value, 
                hasUnicodeEscape ? StringLiteralEncoding.UTF8 : 
                value.IsAscii() ? StringLiteralEncoding.Ascii : // TODO: tokenizer already knows IsAscii
                StringLiteralEncoding.Default
            );
        }

        internal void SetSymbol(string/*!*/ value) {
            SetString(value, StringLiteralEncoding.Ascii);
        }

        internal void SetStringTokenizer(StringTokenizer value) {
            StringTokenizer = value;
            _type = TokenValueType.StringTokenizer;
        }

        internal void SetRegexOptions(RubyRegexOptions value) {
            Integer = (int)value;
            _type = TokenValueType.RegexOptions;
        }

#if DEBUG
        public override string ToString() {
            string str;
            switch (_type) {
                case TokenValueType.Double:
                    str = Double.ToString();
                    break;

                case TokenValueType.Integer:
                    str = Integer.ToString();
                    break;

                case TokenValueType.BigInteger:
                    str = BigInteger.ToString();
                    break;

                case TokenValueType.RegexOptions:
                    str = ((RubyRegexOptions)Integer).ToString();
                    break;

                case TokenValueType.String:
                    str = "\"" + Parser.EscapeString(String) + "\"";
                    break;

                case TokenValueType.StringTokenizer:
                    str = (StringTokenizer != null) ? StringTokenizer.ToString() : "";
                    break;

                default:
                    str = "";
                    break;
            }
            return String.Format("{0}: {1}", _type, str);
        }
#endif
    }
}
