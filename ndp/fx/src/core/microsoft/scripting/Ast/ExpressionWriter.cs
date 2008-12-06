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
#if MICROSOFT_SCRIPTING_CORE

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Dynamic;
using System.Dynamic.Utils;

namespace System.Linq.Expressions {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal sealed class ExpressionWriter : ExpressionVisitor {
        [Flags]
        private enum Flow {
            None,
            Space,
            NewLine,

            Break = 0x8000      // newline if column > MaxColumn
        };

        private struct LambdaId {
            private readonly LambdaExpression _lambda;
            private readonly int _id;

            internal LambdaId(LambdaExpression lambda, int id) {
                _lambda = lambda;
                _id = id;
            }

            internal LambdaExpression Lambda {
                get { return _lambda; }
            }
            internal int Id {
                get { return _id; }
            }
        }

        private const int Tab = 4;
        private const int MaxColumn = 120;

        private TextWriter _out;
        private int _column;

        private Queue<LambdaId> _lambdaIds;
        private int _blockid;
        private Stack<int> _stack = new Stack<int>();
        private int _delta;
        private Flow _flow;

        private ExpressionWriter(TextWriter file) {
            _out = file;
        }

        private int Base {
            get {
                return _stack.Count > 0 ? _stack.Peek() : 0;
            }
        }

        private int Delta {
            get { return _delta; }
        }

        private int Depth {
            get { return Base + Delta; }
        }

        private void Indent() {
            _delta += Tab;
        }
        private void Dedent() {
            _delta -= Tab;
        }

        private void NewLine() {
            _flow = Flow.NewLine;
        }

        /// <summary>
        /// Write out the given AST
        /// </summary>
        internal static void Dump(Expression node, string descr, TextWriter writer) {
            Debug.Assert(node != null);
            Debug.Assert(writer != null);

            ExpressionWriter dv = new ExpressionWriter(writer);
            dv.DoDump(node, descr);
        }

        private void DoDump(Expression node, string description) {
            if (description != null) {
                WritePrologue(description);
            }

            Visit(node);
            WriteLine();

            WriteLambdas();
            WriteLine();
        }

        private void WritePrologue(string name) {
            WriteLine("//");
            WriteLine("// AST: {0}", name);
            WriteLine("//");
            WriteLine();
        }

        private void WriteLambdas() {
            Debug.Assert(_stack.Count == 0);

            while (_lambdaIds != null && _lambdaIds.Count > 0) {
                LambdaId b = _lambdaIds.Dequeue();
                WriteLine();
                WriteLine("//");
                WriteLine("// LAMBDA: {0}({1})", b.Lambda.Name, b.Id);
                WriteLine("//");
                DumpLambda(b.Lambda);
                WriteLine();

                Debug.Assert(_stack.Count == 0);
            }
        }

        private int Enqueue(LambdaExpression lambda) {
            if (_lambdaIds == null) {
                _lambdaIds = new Queue<LambdaId>();
            }
            _lambdaIds.Enqueue(new LambdaId(lambda, ++_blockid));
            return _blockid;
        }

        #region The printing code

        private void Out(string s) {
            Out(Flow.None, s, Flow.None);
        }

        private void Out(Flow before, string s) {
            Out(before, s, Flow.None);
        }

        private void Out(string s, Flow after) {
            Out(Flow.None, s, after);
        }

        private void Out(Flow before, string s, Flow after) {
            switch (GetFlow(before)) {
                case Flow.None:
                    break;
                case Flow.Space:
                    Write(" ");
                    break;
                case Flow.NewLine:
                    WriteLine();
                    Write(new String(' ', Depth));
                    break;
            }
            Write(s);
            _flow = after;
        }

        private void WriteLine() {
            _out.WriteLine();
            _column = 0;
        }
        private void WriteLine(string s) {
            _out.WriteLine(s);
            _column = 0;
        }
        private void WriteLine(string format, object arg0) {
            string s = String.Format(CultureInfo.CurrentCulture, format, arg0);
            WriteLine(s);
        }
        private void WriteLine(string format, object arg0, object arg1) {
            string s = String.Format(CultureInfo.CurrentCulture, format, arg0, arg1);
            WriteLine(s);
        }
        private void Write(string s) {
            _out.Write(s);
            _column += s.Length;
        }

        private Flow GetFlow(Flow flow) {
            Flow last;

            last = CheckBreak(_flow);
            flow = CheckBreak(flow);

            // Get the biggest flow that is requested None < Space < NewLine
            return (Flow)System.Math.Max((int)last, (int)flow);
        }

        private Flow CheckBreak(Flow flow) {
            if ((flow & Flow.Break) != 0) {
                if (_column > (MaxColumn + Depth)) {
                    flow = Flow.NewLine;
                } else {
                    flow &= ~Flow.Break;
                }
            }
            return flow;
        }

        #endregion

        #region The AST Output

        // More proper would be to make this a virtual method on Action
        private static string FormatBinder(CallSiteBinder binder) {
            ConvertBinder convert;
            GetMemberBinder getMember;
            SetMemberBinder setMember;
            DeleteMemberBinder deleteMember;
            GetIndexBinder getIndex;
            SetIndexBinder setIndex;
            DeleteIndexBinder deleteIndex;
            InvokeMemberBinder call;
            InvokeBinder invoke;
            CreateInstanceBinder create;
            UnaryOperationBinder unary;
            BinaryOperationBinder binary;

            if ((convert = binder as ConvertBinder) != null) {
                return "Convert " + convert.Type;
            } else if ((getMember = binder as GetMemberBinder) != null) {
                return "GetMember " + getMember.Name;
            } else if ((setMember = binder as SetMemberBinder) != null) {
                return "SetMember " + setMember.Name;
            } else if ((deleteMember = binder as DeleteMemberBinder) != null) {
                return "DeleteMember " + deleteMember.Name;
            } else if ((getIndex = binder as GetIndexBinder) != null) {
                return "GetIndex";
            } else if ((setIndex = binder as SetIndexBinder) != null) {
                return "SetIndex";
            } else if ((deleteIndex = binder as DeleteIndexBinder) != null) {
                return "DeleteIndex";
            } else if ((call = binder as InvokeMemberBinder) != null) {
                return "Call " + call.Name;
            } else if ((invoke = binder as InvokeBinder) != null) {
                return "Invoke";
            } else if ((create = binder as CreateInstanceBinder) != null) {
                return "Create ";
            } else if ((unary = binder as UnaryOperationBinder) != null) {
                return "UnaryOperation " + unary.Operation;
            } else if ((binary = binder as BinaryOperationBinder) != null) {
                return "BinaryOperation " + binary.Operation;
            } else {
                return "CallSiteBinder(" + binder.ToString() + ") ";
            }
        }

        private void VisitExpressions<T>(char open, IList<T> expressions) where T : Expression {
            VisitExpressions<T>(open, expressions, false);
        }

        private void VisitExpressions<T>(char open, IList<T> expressions, bool forceMultiline) where T : Expression {
            VisitExpressions(open, expressions, forceMultiline, (e) => Visit(e));
        }

        private void VisitDeclarations(char open, IList<ParameterExpression> expressions, bool forceMultiline) {
            VisitExpressions(open, expressions, forceMultiline, (variable) =>
            {
                Out(variable.Type.ToString());
                if (variable.IsByRef) {
                    Out("&");
                }
                Out(" ");
                Out(variable.Name ?? ".anonymous");
            });
        }

        private void VisitExpressions<T>(char open, IList<T> expressions, bool forceMultiline, Action<T> visit) {

            bool multiline = expressions != null && (forceMultiline || expressions.Count > 1);

            Out(open.ToString());
            if (expressions != null) {
                Indent();
                bool isFirst = true;
                foreach (T e in expressions) {
                    if (isFirst) {
                        if (multiline) {
                            NewLine();
                        }
                        isFirst = false;
                    } else {
                        Out(",", Flow.NewLine);
                    }
                    visit(e);
                }
                Dedent();
            }

            string close;
            switch (open) {
                case '(': close = ")"; break;
                case '{': close = "}"; break;
                case '[': close = "]"; break;
                case '<': close = ">"; break;
                default: throw Assert.Unreachable;
            }
            if (multiline) {
                Out(Flow.NewLine, close, Flow.Break);
            } else {
                Out(close, Flow.Break);
            }
        }

        protected internal override Expression VisitDynamic(DynamicExpression node) {
            Out(".site", Flow.Space);

            Out("(");
            Out(node.Type.Name);
            Out(")", Flow.Space);

            Out(FormatBinder(node.Binder));
            VisitExpressions('(', node.Arguments);
            return node;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected internal override Expression VisitBinary(BinaryExpression node) {
            if (node.NodeType == ExpressionType.ArrayIndex) {
                ParenthesizedVisit(node, node.Left);
                Out("[");
                Visit(node.Right);
                Out("]");
            } else {
                bool parenthesizeLeft = NeedsParentheses(node, node.Left);
                bool parenthesizeRight = NeedsParentheses(node, node.Right);

                string op;
                bool isChecked = false;
                Flow beforeOp = Flow.Space;
                switch (node.NodeType) {
                    case ExpressionType.Assign: op = "="; break;
                    case ExpressionType.Equal: op = "=="; break;
                    case ExpressionType.NotEqual: op = "!="; break;
                    case ExpressionType.AndAlso: op = "&&"; beforeOp = Flow.NewLine; break;
                    case ExpressionType.OrElse: op = "||"; beforeOp = Flow.NewLine; break;
                    case ExpressionType.GreaterThan: op = ">"; break;
                    case ExpressionType.LessThan: op = "<"; break;
                    case ExpressionType.GreaterThanOrEqual: op = ">="; break;
                    case ExpressionType.LessThanOrEqual: op = "<="; break;
                    case ExpressionType.Add: op = "+"; break;
                    case ExpressionType.AddAssign: op = "+="; break;
                    case ExpressionType.AddAssignChecked: op = "+="; isChecked = true; break;
                    case ExpressionType.AddChecked: op = "+"; isChecked = true; break;
                    case ExpressionType.Subtract: op = "-"; break;
                    case ExpressionType.SubtractAssign: op = "-="; break;
                    case ExpressionType.SubtractAssignChecked: op = "-="; isChecked = true; break;
                    case ExpressionType.SubtractChecked: op = "-"; isChecked = true; break;
                    case ExpressionType.Divide: op = "/"; break;
                    case ExpressionType.DivideAssign: op = "/="; break;
                    case ExpressionType.Modulo: op = "%"; break;
                    case ExpressionType.ModuloAssign: op = "%="; break;
                    case ExpressionType.Multiply: op = "*"; break;
                    case ExpressionType.MultiplyAssign: op = "*="; break;
                    case ExpressionType.MultiplyAssignChecked: op = "*="; isChecked = true; break;
                    case ExpressionType.MultiplyChecked: op = "*"; isChecked = true; break;
                    case ExpressionType.LeftShift: op = "<<"; break;
                    case ExpressionType.LeftShiftAssign: op = "<<="; break;
                    case ExpressionType.RightShift: op = ">>"; break;
                    case ExpressionType.RightShiftAssign: op = ">>="; break;
                    case ExpressionType.And: op = "&"; break;
                    case ExpressionType.AndAssign: op = "&="; break;
                    case ExpressionType.Or: op = "|"; break;
                    case ExpressionType.OrAssign: op = "|="; break;
                    case ExpressionType.ExclusiveOr: op = "^"; break;
                    case ExpressionType.ExclusiveOrAssign: op = "^="; break;
                    case ExpressionType.Power: op = "**"; break;
                    case ExpressionType.PowerAssign: op = "**="; break;
                    //TODO: need to handle conversion lambda
                    case ExpressionType.Coalesce: op = "??"; break;

                    default:
                        throw new InvalidOperationException();
                }
                if (isChecked) {
                    Out(Flow.Break, "checked(", Flow.None);
                }
                
                
                if (parenthesizeLeft) {
                    Out("(", Flow.None);
                }
                Visit(node.Left);
                if (parenthesizeLeft) {
                    Out(Flow.None, ")", Flow.Break);
                }                

                Out(beforeOp, op, Flow.Space | Flow.Break);
                
                if (parenthesizeRight) {
                    Out("(", Flow.None);
                }
                Visit(node.Right);
                if (parenthesizeRight) {
                    Out(Flow.None, ")", Flow.Break);
                }
            }
            return node;
        }

        protected internal override Expression VisitParameter(ParameterExpression node) {
            Out("$" + node.Name);
            return node;
        }

        protected internal override Expression VisitLambda<T>(Expression<T> node) {
            int id = Enqueue(node);
            Out(
                String.Format(CultureInfo.CurrentCulture,
                    "{0} ({1} {2} #{3})",
                    ".lambda",
                    node.Name,
                    node.Type,
                    id
                )
            );
            return node;
        }

        // TODO: calculate tree depth?
        private static bool IsSimpleExpression(Expression node) {
            var binary = node as BinaryExpression;
            if (binary != null) {
                return !(binary.Left is BinaryExpression || binary.Right is BinaryExpression);
            }

            return false;
        }

        protected internal override Expression VisitConditional(ConditionalExpression node) {
            if (IsSimpleExpression(node.Test)) {
                Out(".if (");
                Visit(node.Test);
                Out(") {", Flow.NewLine);
            } else {
                Out(".if (", Flow.NewLine);
                Indent();
                Visit(node.Test);
                Dedent();
                Out(Flow.NewLine, ") {", Flow.NewLine);
            }
            Indent();
            Visit(node.IfTrue);
            Dedent();
            Out(Flow.NewLine, "} .else {", Flow.NewLine);
            Indent();
            Visit(node.IfFalse);
            Dedent();
            Out(Flow.NewLine, "}", Flow.NewLine);
            return node;
        }

        private static string Constant(object value) {
            if (value == null) {
                return ".null";
            }

            ITemplatedValue itv = value as ITemplatedValue;
            if (itv != null) {
                return ".template" + itv.Index.ToString(CultureInfo.CurrentCulture) + " (" + itv.ObjectValue.ToString() + ")";
            }

            string s;
            if ((s = value as string) != null) {
                return "\"" + s + "\"";
            }
            if (value is int || value is double) {
                return String.Format(CultureInfo.CurrentCulture, "{0:G}", value);
            }
            if (value is bool) {
                return value.ToString();
            }
            return ".const<" + value.GetType().Name + ">(" + value.ToString() + ")";
        }

        protected internal override Expression VisitConstant(ConstantExpression node) {
            Out(Constant(node.Value));
            return node;
        }

        protected internal override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
            Out(".localScope");
            VisitExpressions('(', node.Variables);
            return node;
        }

        // Prints ".instanceField" or "declaringType.staticField"
        private void OutMember(Expression node, Expression instance, MemberInfo member) {
            if (instance != null) {
                ParenthesizedVisit(node, instance);
                Out("." + member.Name);
            } else {
                // For static members, include the type name
                Out(member.DeclaringType.Name + "." + member.Name);
            }
        }

        protected internal override Expression VisitMember(MemberExpression node) {
            OutMember(node, node.Expression, node.Member);
            return node;
        }

        protected internal override Expression VisitInvocation(InvocationExpression node) {
            ParenthesizedVisit(node, node.Expression);
            Out(".Invoke");
            VisitExpressions('(', node.Arguments);
            return node;
        }

        private static bool NeedsParentheses(Expression parent, Expression child) {
            return GetOperatorPrecedence(child) < GetOperatorPrecedence(parent);
        }
        
        // the greater the higher
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static int GetOperatorPrecedence(Expression node) {
            switch (node.NodeType) {
                case ExpressionType.Assign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.AddAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                    return 0;

                case ExpressionType.Coalesce:
                    return 1;

                case ExpressionType.OrElse:
                    return 2;

                case ExpressionType.AndAlso:
                    return 3;

                case ExpressionType.Or:
                    return 4;

                case ExpressionType.ExclusiveOr:
                    return 5;

                case ExpressionType.And:
                    return 6;

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    return 7;    

                case ExpressionType.GreaterThan:
                case ExpressionType.LessThan:                
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThanOrEqual:
                    return 8;

                case ExpressionType.LeftShift:
                case ExpressionType.RightShift:
                    return 9;

                case ExpressionType.Add:
                case ExpressionType.Subtract:
                    return 10;

                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                    return 11;

                case ExpressionType.Power:
                    return 12;

                case ExpressionType.Negate:
                case ExpressionType.UnaryPlus:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.OnesComplement:
                    return 13;

                case ExpressionType.PostIncrementAssign:
                case ExpressionType.PostDecrementAssign:
                case ExpressionType.NegateChecked:
                case ExpressionType.ConvertChecked:
                case ExpressionType.AddChecked:
                case ExpressionType.SubtractChecked:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.AddAssignChecked:
                    return 14;

                case ExpressionType.Call:
                case ExpressionType.Parameter:
                case ExpressionType.Constant:
                default:
                    return 15;
            }
        }

        private void ParenthesizedVisit(Expression parent, Expression nodeToVisit) {
            if (NeedsParentheses(parent, nodeToVisit)) {
                Out("(");
                Visit(nodeToVisit);
                Out(")");
            } else {
                Visit(nodeToVisit);
            }
        }
        
        protected internal override Expression VisitMethodCall(MethodCallExpression node) {
            if (node.Object != null) {
                ParenthesizedVisit(node, node.Object);
                Out(".");
            }
            if (node.Method.ReflectedType != null) {
                Out("'" + node.Method.ReflectedType.Name + "." + node.Method.Name + "'");
            } else {
                Out("'" + node.Method.Name + "'");
            }
            VisitExpressions('(', node.Arguments);
            return node;
        }

        protected internal override Expression VisitNewArray(NewArrayExpression node) {
            if (node.NodeType == ExpressionType.NewArrayBounds) {
                // .new MyType[expr1, expr2]
                Out(".new " + node.Type.GetElementType().Name, Flow.Space);
                VisitExpressions('[', node.Expressions);
            } else {
                // .new MyType {expr1, expr2}
                Out(".new " + node.Type.Name, Flow.Space);
                VisitExpressions('{', node.Expressions);
            }
            return node;
        }

        protected internal override Expression VisitNew(NewExpression node) {
            Out(".new " + node.Type.Name);
            VisitExpressions('(', node.Arguments);
            return node;
        }

        protected internal override Expression VisitTypeBinary(TypeBinaryExpression node) {
            Visit(node.Expression);
            switch (node.NodeType) {
                case ExpressionType.TypeIs:
                    Out(Flow.Space, ".is", Flow.Space);
                    break;
                case ExpressionType.TypeEqual:
                    Out(Flow.Space, ".TypeEqual", Flow.Space);
                    break;
            }
            Out(node.TypeOperand.Name);
            return node;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected internal override Expression VisitUnary(UnaryExpression node) {
            switch (node.NodeType) {
                case ExpressionType.Convert:
                    Out("(" + node.Type.Name + ")");
                    break;
                case ExpressionType.ConvertChecked:
                    Out("checked((" + node.Type.Name + ")");
                    break;
                case ExpressionType.TypeAs:
                    break;
                case ExpressionType.Not:
                    Out(node.Type == typeof(bool) ? "!" : "~");
                    break;
                case ExpressionType.OnesComplement:
                    Out("~");
                    break;
                case ExpressionType.Negate:
                    Out("-");
                    break;
                case ExpressionType.NegateChecked:
                    Out("checked(-");
                    break;
                case ExpressionType.UnaryPlus:
                    Out("+");
                    break;
                case ExpressionType.ArrayLength:
                    break;
                case ExpressionType.Quote:
                    Out("'");
                    break;
                case ExpressionType.Throw:
                    Out(".throw ");
                    break;
            }

            bool parenthesize = NeedsParentheses(node, node.Operand);
            if (parenthesize) {
                Out("(");
            }

            Visit(node.Operand);

            if (parenthesize) {
                Out(")");
            }

            switch (node.NodeType) {
                case ExpressionType.Convert:
                case ExpressionType.Throw:
                    break;

                case ExpressionType.ConvertChecked:
                case ExpressionType.NegateChecked:
                    Out(")");
                    break;
                
                case ExpressionType.TypeAs:
                    Out(Flow.Space, "as", Flow.Space | Flow.Break);
                    Out(node.Type.Name);
                    break;

                case ExpressionType.ArrayLength:
                    Out(".Length");
                    break;
            }
            return node;
        }

        protected internal override Expression VisitBlock(BlockExpression node) {
            Out(node.Type == typeof(void) ? ".block " : ".comma ");

            if (node.Variables.Count > 0) {
                VisitDeclarations('(', node.Variables, true);
            }

            Out(" ");

            VisitExpressions('{', node.Expressions, true);

            return node;
        }

        protected internal override Expression VisitDefault(DefaultExpression node) {
            if (node.Type == typeof(void)) {
                Out("/*empty*/");
            } else {
                Out(".default(" + node.Type + ")");
            }
            return node;
        }

        protected internal override Expression VisitLabel(LabelExpression node) {
            DumpLabel(node.Target);
            Out(":", Flow.NewLine);
            Visit(node.DefaultValue);
            return node;
        }

        protected internal override Expression VisitGoto(GotoExpression node) {
            Out("." + node.Kind.ToString().ToLower(CultureInfo.CurrentCulture), Flow.Space);
            DumpLabel(node.Target);
            ParenthesizedVisit(node, node.Value);
            Out("", Flow.Space);
            return node;
        }

        protected internal override Expression VisitLoop(LoopExpression node) {
            Out(".loop", Flow.Space);
            if (node.BreakLabel != null) {
                Out("break:");
                DumpLabel(node.BreakLabel);
                Out(Flow.Space, "");
            }
            if (node.ContinueLabel != null) {
                Out("continue:");
                DumpLabel(node.ContinueLabel);
                Out(Flow.Space, "");
            }
            Out(" {", Flow.NewLine);
            Indent();
            Visit(node.Body);
            Dedent();
            Out(Flow.NewLine, "}"); return node;
        }

        protected override SwitchCase VisitSwitchCase(SwitchCase node) {
            if (node.IsDefault) {
                Out(".default");
            } else {
                Out(".case " + node.Value);
            }
            Out(":", Flow.NewLine);
            Indent(); Indent();
            Visit(node.Body);
            Dedent(); Dedent();
            NewLine();
            return node;
        }

        protected internal override Expression VisitSwitch(SwitchExpression node) {
            Out(".switch ");
            if (node.BreakLabel != null) {
                DumpLabel(node.BreakLabel);
                Out(" ");
            }
            Out("(");
            Visit(node.Test);
            Out(") {", Flow.NewLine);
            Visit(node.SwitchCases, VisitSwitchCase);
            Out("}", Flow.NewLine);
            return node;
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node) {
            Out(Flow.NewLine, "} .catch (" + node.Test.Name);
            if (node.Variable != null) {
                Out(Flow.Space, node.Variable.Name ?? "");
            }
            if (node.Filter != null) {
                Out(") if (", Flow.Break);
                Visit(node.Filter);
            }
            Out(") {", Flow.NewLine);
            Indent();
            Visit(node.Body);
            Dedent();
            return node;
        }

        protected internal override Expression VisitTry(TryExpression node) {
            Out(".try {", Flow.NewLine);
            Indent();
            Visit(node.Body);
            Dedent();
            Visit(node.Handlers, VisitCatchBlock);
            if (node.Finally != null) {
                Out(Flow.NewLine, "} .finally {", Flow.NewLine);
                Indent();
                Visit(node.Finally);
                Dedent();
            } else if (node.Fault != null) {
                Out(Flow.NewLine, "} .fault {", Flow.NewLine);
                Indent();
                Visit(node.Fault);
                Dedent();
            }

            Out(Flow.NewLine, "}", Flow.NewLine);
            return node;
        }

        protected internal override Expression VisitIndex(IndexExpression node) {
            if (node.Indexer != null) {
                OutMember(node, node.Object, node.Indexer);
            } else {
                Visit(node.Object);
                Out(".");
            }

            VisitExpressions('[', node.Arguments);
            return node;
        }

        protected internal override Expression VisitExtension(Expression node) {
            Out(".extension", Flow.Space);

            Out(node.GetType().Name, Flow.Space);
            Out("(", Flow.Space);
            // walk it
            base.VisitExtension(node);
            Out(")", Flow.NewLine);
            return node;
        }

        private static string GetLambdaInfo(LambdaExpression lambda) {
            return String.Format(CultureInfo.CurrentCulture, ".lambda {0} {1} ()", lambda.ReturnType, lambda.Name);
        }

        private void DumpLabel(LabelTarget target) {
            if (string.IsNullOrEmpty(target.Name)) {
                Out(String.Format(CultureInfo.CurrentCulture, ".label 0x{0:x8}", target.GetHashCode()));
            } else {
                Out(String.Format(CultureInfo.CurrentCulture, ".label '{0}'", target.Name));
            }
        }

        private void DumpLambda(LambdaExpression node) {
            Out(GetLambdaInfo(node));

            VisitDeclarations('(', node.Parameters, true);

            Out(Flow.Space, "{", Flow.NewLine);
            Indent();
            Visit(node.Body);
            Dedent();
            Out(Flow.NewLine, "}");
        }

        #endregion
    }
}

#endif