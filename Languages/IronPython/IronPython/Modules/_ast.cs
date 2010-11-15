/* ****************************************************************************
 *
 * Copyright (c) Jeff Hardy 2010. 
 * Copyright (c) Dan Eloff 2008-2009. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using PyOperator = IronPython.Compiler.PythonOperator;
using PythonList = IronPython.Runtime.List;
using System.Runtime.InteropServices;

#if CLR2
using Microsoft.Scripting.Math;
#else
using System.Numerics;
#endif

[assembly: PythonModule("_ast", typeof(IronPython.Modules._ast))]
namespace IronPython.Modules
{
    public static class _ast
    {
        public const string __version__ = "62047";
        public const int PyCF_ONLY_AST = 0x400;

        private static PythonContext _context;

        [SpecialName]
        public static void PerformModuleReload(PythonContext/*!*/ context, PythonDictionary/*!*/ dict) {
            _context = context;
        }

        private class ThrowingErrorSink : ErrorSink
        {
            public static new readonly ThrowingErrorSink/*!*/ Default = new ThrowingErrorSink();

            private ThrowingErrorSink() {
            }

            public override void Add(SourceUnit sourceUnit, string message, SourceSpan span, int errorCode, Severity severity) {
                if (severity == Severity.Warning) {
                    PythonOps.SyntaxWarning(message, sourceUnit, span, errorCode);
                } else {
                    throw PythonOps.SyntaxError(message, sourceUnit, span, errorCode);
                }
            }
        }

        internal static AST BuildAst(CodeContext context, SourceUnit sourceUnit, PythonCompilerOptions opts, string mode) {
            Parser parser = Parser.CreateParser(
                new CompilerContext(sourceUnit, opts, ThrowingErrorSink.Default),
                (PythonOptions)context.LanguageContext.Options);

            PythonAst ast = parser.ParseFile(true);
            return ConvertToAST(ast, mode);
        }

        private static mod ConvertToAST(PythonAst pythonAst, string kind) {
            ContractUtils.RequiresNotNull(pythonAst, "pythonAst");
            ContractUtils.RequiresNotNull(kind, "kind");
            return ConvertToAST((SuiteStatement)pythonAst.Body, kind);
        }

        private static mod ConvertToAST(SuiteStatement suite, string kind) {
            ContractUtils.RequiresNotNull(suite, "suite");
            ContractUtils.RequiresNotNull(kind, "kind");
            switch (kind) {
                case "exec":
                    return new Module(suite);
                case "eval":
                    return new Expression(suite);
                case "single":
                    return new Interactive(suite);
                default:
                    throw new ArgumentException("kind must be 'exec' or 'eval' or 'single'");
            }
        }

        private static stmt ConvertToAST(Statement stmt) {
            ContractUtils.RequiresNotNull(stmt, "stmt");
            return AST.Convert(stmt);
        }

        private static expr ConvertToAST(Compiler.Ast.Expression expr) {
            ContractUtils.RequiresNotNull(expr, "expr");
            return AST.Convert(expr);
        }

        [PythonType]
        public abstract class AST
        {
            private PythonTuple __fields = new PythonTuple();   // Genshi assumes _fields in not None
            private int _lineno;
            private int _col_offset;

            public PythonTuple _fields {
                get { return __fields; }
                protected set { __fields = value; }
            }

            public int lineno {
                get { return _lineno; }
                set { _lineno = value; }
            }

            public int col_offset {
                get { return _col_offset; }
                set { _col_offset = value; }
            }

            protected void GetSourceLocation(Node node) {
                _lineno = node.Start.Line;

                // IronPython counts from 1; CPython counts from 0
                _col_offset = node.Start.Column - 1;
            }

            internal static PythonList ConvertStatements(Statement stmt) {
                return ConvertStatements(stmt, false);
            }

            internal static PythonList ConvertStatements(Statement stmt, bool allowNull) {
                if (stmt == null)
                    if (allowNull)
                        return PythonOps.MakeEmptyList(0);
                    else
                        throw new ArgumentNullException("stmt");

                if (stmt is SuiteStatement) {
                    SuiteStatement suite = (SuiteStatement)stmt;
                    PythonList l = PythonOps.MakeEmptyList(suite.Statements.Count);
                    foreach (Statement s in suite.Statements)
                        l.Add(Convert(s));

                    return l;
                }

                return PythonOps.MakeListNoCopy(Convert(stmt));
            }

            internal static stmt Convert(Statement stmt) {
                stmt ast;

                if (stmt is FunctionDefinition)
                    ast = new FunctionDef((FunctionDefinition)stmt);
                else if (stmt is ReturnStatement)
                    ast = new Return((ReturnStatement)stmt);
                else if (stmt is AssignmentStatement)
                    ast = new Assign((AssignmentStatement)stmt);
                else if (stmt is AugmentedAssignStatement)
                    ast = new AugAssign((AugmentedAssignStatement)stmt);
                else if (stmt is DelStatement)
                    ast = new Delete((DelStatement)stmt);
                else if (stmt is PrintStatement)
                    ast = new Print((PrintStatement)stmt);
                else if (stmt is ExpressionStatement)
                    ast = new Expr((ExpressionStatement)stmt);
                else if (stmt is ForStatement)
                    ast = new For((ForStatement)stmt);
                else if (stmt is WhileStatement)
                    ast = new While((WhileStatement)stmt);
                else if (stmt is IfStatement)
                    ast = new If((IfStatement)stmt);
                else if (stmt is WithStatement)
                    ast = new With((WithStatement)stmt);
                else if (stmt is RaiseStatement)
                    ast = new Raise((RaiseStatement)stmt);
                else if (stmt is TryStatement)
                    ast = Convert((TryStatement)stmt);
                else if (stmt is AssertStatement)
                    ast = new Assert((AssertStatement)stmt);
                else if (stmt is ImportStatement)
                    ast = new Import((ImportStatement)stmt);
                else if (stmt is FromImportStatement)
                    ast = new ImportFrom((FromImportStatement)stmt);
                else if (stmt is ExecStatement)
                    ast = new Exec((ExecStatement)stmt);
                else if (stmt is GlobalStatement)
                    ast = new Global((GlobalStatement)stmt);
                else if (stmt is ClassDefinition)
                    ast = new ClassDef((ClassDefinition)stmt);
                else if (stmt is BreakStatement)
                    ast = Break.Instance;
                else if (stmt is ContinueStatement)
                    ast = Continue.Instance;
                else if (stmt is EmptyStatement)
                    ast = Pass.Instance;
                else
                    throw new ArgumentTypeException("Unexpected statement type: " + stmt.GetType());

                ast.GetSourceLocation(stmt);
                return ast;
            }

            internal static stmt Convert(TryStatement stmt) {
                if (stmt.Finally != null) {
                    PythonList body;
                    if (stmt.Handlers != null && stmt.Handlers.Count != 0)
                        body = PythonOps.MakeListNoCopy(new TryExcept(stmt));
                    else
                        body = ConvertStatements(stmt.Body);

                    return new TryFinally(body, ConvertStatements(stmt.Finally));
                }

                return new TryExcept(stmt);
            }

            internal static PythonList ConvertAliases(IList<DottedName> names, IList<string> asnames) {
                PythonList l = PythonOps.MakeEmptyList(names.Count);

                if (names == FromImportStatement.Star)
                    l.Add(new alias("*", null));
                else
                    for (int i = 0; i < names.Count; i++)
                        l.Add(new alias(names[i].MakeString(), asnames[i]));

                return l;
            }

            internal static PythonList ConvertAliases(IList<string> names, IList<string> asnames) {
                PythonList l = PythonOps.MakeEmptyList(names.Count);

                if (names == FromImportStatement.Star)
                    l.Add(new alias("*", null));
                else
                    for (int i = 0; i < names.Count; i++)
                        l.Add(new alias(names[i], asnames[i]));

                return l;
            }

            internal static expr Convert(Compiler.Ast.Expression expr) {
                return Convert(expr, Load.Instance);
            }

            internal static expr Convert(Compiler.Ast.Expression expr, expr_context ctx) {
                expr ast;

                if (expr is ConstantExpression)
                    ast = Convert((ConstantExpression)expr);
                else if (expr is NameExpression)
                    ast = new Name((NameExpression)expr, ctx);
                else if (expr is UnaryExpression)
                    ast = new UnaryOp((UnaryExpression)expr);
                else if (expr is BinaryExpression)
                    ast = Convert((BinaryExpression)expr);
                else if (expr is AndExpression)
                    ast = new BoolOp((AndExpression)expr);
                else if (expr is OrExpression)
                    ast = new BoolOp((OrExpression)expr);
                else if (expr is CallExpression)
                    ast = new Call((CallExpression)expr);
                else if (expr is ParenthesisExpression)
                    return Convert(((ParenthesisExpression)expr).Expression);
                else if (expr is LambdaExpression)
                    ast = new Lambda((LambdaExpression)expr);
                else if (expr is ListExpression)
                    ast = new List((ListExpression)expr, ctx);
                else if (expr is TupleExpression)
                    ast = new Tuple((TupleExpression)expr, ctx);
                else if (expr is DictionaryExpression)
                    ast = new Dict((DictionaryExpression)expr);
                else if (expr is ListComprehension)
                    ast = new ListComp((ListComprehension)expr);
                else if (expr is GeneratorExpression)
                    ast = new GeneratorExp((GeneratorExpression)expr);
                else if (expr is MemberExpression)
                    ast = new Attribute((MemberExpression)expr, ctx);
                else if (expr is YieldExpression)
                    ast = new Yield((YieldExpression)expr);
                else if (expr is ConditionalExpression)
                    ast = new IfExp((ConditionalExpression)expr);
                else if (expr is IndexExpression)
                    ast = new Subscript((IndexExpression)expr, ctx);
                else if (expr is SliceExpression)
                    ast = new Slice((SliceExpression)expr);
                else if (expr is BackQuoteExpression)
                    ast = new Repr((BackQuoteExpression)expr);
                else
                    throw new ArgumentTypeException("Unexpected expression type: " + expr.GetType());

                ast.GetSourceLocation(expr);
                return ast;
            }

            internal static expr Convert(ConstantExpression expr) {
                expr ast;

                if (expr.Value == null)
                    return new Name("None", Load.Instance);

                if (expr.Value is int || expr.Value is double || expr.Value is Int64 || expr.Value is BigInteger || expr.Value is Complex)
                    ast = new Num(expr.Value);
                else if (expr.Value is string)
                    ast = new Str((string)expr.Value);
                else if (expr.Value is IronPython.Runtime.Bytes)
                    ast = new Str(Converter.ConvertToString(expr.Value));
                else if (expr.Value == PythonOps.Ellipsis)
                    ast = Ellipsis.Instance;
                else
                    throw new ArgumentTypeException("Unexpected constant type: " + expr.Value.GetType());

                return ast;
            }

            internal static expr Convert(BinaryExpression expr) {
                AST op = Convert(expr.Operator);
                if (op is @operator)
                    return new BinOp(expr, (@operator)op);
                if (op is cmpop)
                    return new Compare(expr, (cmpop)op);

                throw new ArgumentTypeException("Unexpected operator type: " + op.GetType());
            }

            internal static AST Convert(Node node) {
                AST ast;

                if (node is TryStatementHandler)
                    ast = new excepthandler((TryStatementHandler)node);
                else
                    throw new ArgumentTypeException("Unexpected node type: " + node.GetType());

                ast.GetSourceLocation(node);
                return ast;
            }

            internal static PythonList Convert(IList<ComprehensionIterator> iters) {
                ComprehensionIterator[] iters2 = new ComprehensionIterator[iters.Count];
                iters.CopyTo(iters2, 0);
                return Convert(iters2);
            }

            internal static PythonList Convert(ComprehensionIterator[] iters) {
                PythonList comps = new PythonList();
                int start = 1;
                for (int i = 0; i < iters.Length; i++) {
                    if (i == 0 || iters[i] is ComprehensionIf)
                        if (i == iters.Length - 1)
                            i++;
                        else
                            continue;

                    ComprehensionIf[] ifs = new ComprehensionIf[i - start];
                    Array.Copy(iters, start, ifs, 0, ifs.Length);
                    comps.Add(new comprehension((ComprehensionFor)iters[start - 1], ifs));
                    start = i + 1;
                }
                return comps;
            }

            internal static AST Convert(PyOperator op) {
                // We treat operator classes as singletons here to keep overhead down
                // But we cannot fully make them singletons if we wish to keep compatibility wity CPython
                switch (op) {
                    case PyOperator.Add:
                        return Add.Instance;
                    case PyOperator.BitwiseAnd:
                        return BitAnd.Instance;
                    case PyOperator.BitwiseOr:
                        return BitOr.Instance;
                    case PyOperator.Divide:
                        return Div.Instance;
                    case PyOperator.Equal:
                        return Eq.Instance;
                    case PyOperator.FloorDivide:
                        return FloorDiv.Instance;
                    case PyOperator.GreaterThan:
                        return Gt.Instance;
                    case PyOperator.GreaterThanOrEqual:
                        return GtE.Instance;
                    case PyOperator.In:
                        return In.Instance;
                    case PyOperator.Invert:
                        return Invert.Instance;
                    case PyOperator.Is:
                        return Is.Instance;
                    case PyOperator.IsNot:
                        return IsNot.Instance;
                    case PyOperator.LeftShift:
                        return LShift.Instance;
                    case PyOperator.LessThan:
                        return Lt.Instance;
                    case PyOperator.LessThanOrEqual:
                        return LtE.Instance;
                    case PyOperator.Mod:
                        return Mod.Instance;
                    case PyOperator.Multiply:
                        return Mult.Instance;
                    case PyOperator.Negate:
                        return USub.Instance;
                    case PyOperator.Not:
                        return Not.Instance;
                    case PyOperator.NotEqual:
                        return NotEq.Instance;
                    case PyOperator.NotIn:
                        return NotIn.Instance;
                    case PyOperator.Pos:
                        return UAdd.Instance;
                    case PyOperator.Power:
                        return Pow.Instance;
                    case PyOperator.RightShift:
                        return RShift.Instance;
                    case PyOperator.Subtract:
                        return Sub.Instance;
                    case PyOperator.Xor:
                        return BitXor.Instance;
                    default:
                        throw new ArgumentException("Unexpected PyOperator: " + op, "op");
                }
            }
        }

        [PythonType]
        public class alias : AST
        {
            private string _name;
            private string _asname;

            public alias() {
                _fields = new PythonTuple(new[] { "name", "asname" });
            }

            internal alias(string name, string asname)
                : this() {
                _name = name;
                _asname = asname;
            }

            public string name {
                get { return _name; }
                set { _name = value; }
            }

            public string asname {
                get { return _asname; }
                set { _asname = value; }
            }
        }

        [PythonType]
        public class arguments : AST
        {
            private PythonList _args;
            private string _vararg;
            private string _kwarg;
            private PythonList _defaults;

            public arguments() {
                _fields = new PythonTuple(new[] { "args", "vararg", "kwarg", "defaults" });
            }


            internal arguments(IList<Parameter> parameters)
                : this() {
                _args = PythonOps.MakeEmptyList(parameters.Count);
                _defaults = PythonOps.MakeEmptyList(parameters.Count);
                foreach (Parameter param in parameters) {
                    if (param.IsList)
                        _vararg = param.Name;
                    else if (param.IsDictionary)
                        _kwarg = param.Name;
                    else {
                        args.Add(new Name(param.Name, Param.Instance));
                        if (param.DefaultValue != null)
                            defaults.Add(Convert(param.DefaultValue));
                    }
                }
            }


            internal arguments(Parameter[] parameters)
                : this(parameters as IList<Parameter>) {
            }

            public PythonList args {
                get { return _args; }
                set { _args = value; }
            }

            public string vararg {
                get { return _vararg; }
                set { _vararg = value; }
            }

            public string kwarg {
                get { return _kwarg; }
                set { _kwarg = value; }
            }

            public PythonList defaults {
                get { return _defaults; }
                set { _defaults = value; }
            }
        }

        [PythonType]
        public abstract class boolop : AST
        {
        }

        [PythonType]
        public abstract class cmpop : AST
        {
        }

        [PythonType]
        public class comprehension : AST
        {
            private expr _target;
            private expr _iter;
            private PythonList _ifs;

            public comprehension() {
                _fields = new PythonTuple(new[] { "target", "iter", "ifs" });
            }

            internal comprehension(ComprehensionFor listFor, ComprehensionIf[] listIfs)
                : this() {
                _target = Convert(listFor.Left, Store.Instance);
                _iter = Convert(listFor.List);
                _ifs = PythonOps.MakeEmptyList(listIfs.Length);
                foreach (ComprehensionIf listIf in listIfs)
                    _ifs.Add(Convert(listIf.Test));
            }

            public expr target {
                get { return _target; }
                set { _target = value; }
            }

            public expr iter {
                get { return _iter; }
                set { _iter = value; }
            }

            public PythonList ifs {
                get { return _ifs; }
                set { _ifs = value; }
            }
        }

        [PythonType]
        public class excepthandler : AST
        {
            private expr _type;
            private expr _name;
            private PythonList _body;

            public excepthandler() {
                _fields = new PythonTuple(new[] { "type", "name", "body", "lineno", "col_offset" });
            }

            internal excepthandler(TryStatementHandler stmt)
                : this() {
                if (stmt.Test != null)
                    _type = Convert(stmt.Test);
                if (stmt.Target != null)
                    _name = Convert(stmt.Target, Store.Instance);

                _body = ConvertStatements(stmt.Body);
            }

            public expr type {
                get { return _type; }
                set { _type = value; }
            }

            public expr name {
                get { return _name; }
                set { _name = value; }
            }

            public PythonList body {
                get { return _body; }
                set { _body = value; }
            }
        }

        [PythonType]
        public abstract class expr : AST
        {
        }

        [PythonType]
        public abstract class expr_context : AST
        {
        }

        [PythonType]
        public class keyword : AST
        {
            private string _arg;
            private expr _value;

            public keyword() {
                _fields = new PythonTuple(new[] { "arg", "value" });
            }

            internal keyword(IronPython.Compiler.Ast.Arg arg)
                : this() {
                _arg = arg.Name;
                _value = Convert(arg.Expression);
            }

            public string arg {
                get { return _arg; }
                set { _arg = value; }
            }

            public expr value {
                get { return _value; }
                set { _value = value; }
            }
        }

        [PythonType]
        public abstract class mod : AST
        {
            internal abstract PythonList GetStatements();
        }

        [PythonType]
        public abstract class @operator : AST
        {
        }

        [PythonType]
        public abstract class slice : expr // This is the only departure we make from the CPython _ast inheritence tree.
        {
        }

        [PythonType]
        public abstract class stmt : AST
        {
        }

        [PythonType]
        public abstract class unaryop : AST
        {
        }

        [PythonType]
        public class Add : @operator
        {
            internal static Add Instance = new Add();
        }

        [PythonType]
        public class And : boolop
        {
            internal static And Instance = new And();
        }

        [PythonType]
        public class Assert : stmt
        {
            private expr _test;
            private expr _msg; // Optional

            public Assert() {
                _fields = new PythonTuple(new[] { "test", "msg" });
            }

            internal Assert(AssertStatement stmt)
                : this() {
                _test = Convert(stmt.Test);
                if (stmt.Message != null)
                    _msg = Convert(stmt.Message);
            }

            public expr test {
                get { return _test; }
                set { _test = value; }
            }

            public expr msg {
                get { return _msg; }
                set { _msg = value; }
            }
        }

        [PythonType]
        public class Assign : stmt
        {
            private PythonList _targets;
            private expr _value;

            public Assign() {
                _fields = new PythonTuple(new[] { "targets", "value" });
            }

            internal Assign(AssignmentStatement stmt)
                : this() {
                _targets = PythonOps.MakeEmptyList(stmt.Left.Count);
                foreach (Compiler.Ast.Expression expr in stmt.Left)
                    _targets.Add(Convert(expr, Store.Instance));

                _value = Convert(stmt.Right);
            }

            public PythonList targets {
                get { return _targets; }
                set { _targets = value; }
            }

            public expr value {
                get { return _value; }
                set { _value = value; }
            }
        }

        [PythonType]
        public class Attribute : expr
        {
            private expr _value;
            private string _attr;
            private expr_context _ctx;

            public Attribute() {
                _fields = new PythonTuple(new[] { "value", "attr", "ctx" });
            }

            internal Attribute(MemberExpression attr, expr_context ctx)
                : this() {
                _value = Convert(attr.Target);
                _attr = attr.Name;
                _ctx = ctx;
            }

            public expr value {
                get { return _value; }
                set { _value = value; }
            }

            public string attr {
                get { return _attr; }
                set { _attr = value; }
            }

            public expr_context ctx {
                get { return _ctx; }
                set { _ctx = value; }
            }
        }

        [PythonType]
        public class AugAssign : stmt
        {
            private expr _target;
            private @operator _op;
            private expr _value;

            public AugAssign() {
                _fields = new PythonTuple(new[] { "target", "op", "value" });
            }

            internal AugAssign(AugmentedAssignStatement stmt)
                : this() {
                _target = Convert(stmt.Left, Store.Instance);
                _value = Convert(stmt.Right);
                _op = (@operator)Convert(stmt.Operator);
            }

            public expr target {
                get { return _target; }
                set { _target = value; }
            }

            public @operator op {
                get { return _op; }
                set { _op = value; }
            }

            public expr value {
                get { return _value; }
                set { _value = value; }
            }
        }

        /// <summary>
        /// Not used.
        /// </summary>
        [PythonType]
        public class AugLoad : expr_context
        {
        }

        /// <summary>
        /// Not used.
        /// </summary>
        [PythonType]
        public class AugStore : expr_context
        {
        }

        [PythonType]
        public class BinOp : expr
        {
            private expr _left;
            private expr _right;
            private @operator _op;

            public BinOp() {
                _fields = new PythonTuple(new[] { "left", "op", "right" });
            }

            public BinOp(expr left, @operator op, expr right, [Optional]int? lineno)
                : this() {
                _left = left;
                _op = op;
                _right = right;

                if (lineno != null)
                    this.lineno = lineno.Value;
            }

            internal BinOp(BinaryExpression expr, @operator op)
                : this() {
                _left = Convert(expr.Left);
                _right = Convert(expr.Right);
                _op = op;
            }

            public expr left {
                get { return _left; }
                set { _left = value; }
            }

            public expr right {
                get { return _right; }
                set { _right = value; }
            }

            public @operator op {
                get { return _op; }
                set { _op = value; }
            }
        }

        [PythonType]
        public class BitAnd : @operator
        {
            internal static BitAnd Instance = new BitAnd();
        }

        [PythonType]
        public class BitOr : @operator
        {
            internal static BitOr Instance = new BitOr();
        }

        [PythonType]
        public class BitXor : @operator
        {
            internal static BitXor Instance = new BitXor();
        }

        [PythonType]
        public class BoolOp : expr
        {
            private boolop _op;
            private PythonList _values;

            public BoolOp() {
                _fields = new PythonTuple(new[] { "op", "values" });
            }

            internal BoolOp(AndExpression and)
                : this() {
                _values = PythonOps.MakeListNoCopy(Convert(and.Left), Convert(and.Right));
                _op = And.Instance;
            }

            internal BoolOp(OrExpression or)
                : this() {
                _values = PythonOps.MakeListNoCopy(Convert(or.Left), Convert(or.Right));
                _op = Or.Instance;
            }

            public boolop op {
                get { return _op; }
                set { _op = value; }
            }

            public PythonList values {
                get { return _values; }
                set { _values = value; }
            }
        }

        [PythonType]
        public class Break : stmt
        {
            internal static Break Instance = new Break();
        }

        [PythonType]
        public class Call : expr
        {
            private expr _func;
            private PythonList _args;
            private PythonList _keywords;
            private expr _starargs; // Optional
            private expr _kwargs; // Optional

            public Call() {
                _fields = new PythonTuple(new[] { "func", "args", "keywords", "starargs", "kwargs" });
            }

            internal Call(CallExpression call)
                : this() {
                _args = PythonOps.MakeEmptyList(call.Args.Count);
                _keywords = new PythonList();
                _func = Convert(call.Target);
                foreach (IronPython.Compiler.Ast.Arg arg in call.Args) {

                    if (arg.Name == null)
                        _args.Add(Convert(arg.Expression));
                    else if (arg.Name == "*")
                        _starargs = Convert(arg.Expression);
                    else if (arg.Name == "**")
                        _kwargs = Convert(arg.Expression);
                    else
                        _keywords.Add(new keyword(arg));
                }
            }

            public expr func {
                get { return _func; }
                set { _func = value; }
            }

            public PythonList args {
                get { return _args; }
                set { _args = value; }
            }

            public PythonList keywords {
                get { return _keywords; }
                set { _keywords = value; }
            }

            public expr starargs {
                get { return _starargs; }
                set { _starargs = value; }
            }

            public expr kwargs {
                get { return _kwargs; }
                set { _kwargs = value; }
            }
        }

        [PythonType]
        public class ClassDef : stmt
        {
            private string _name;
            private PythonList _bases;
            private PythonList _body;

            public ClassDef() {
                _fields = new PythonTuple(new[] { "name", "bases", "body" });
            }

            internal ClassDef(ClassDefinition def)
                : this() {
                _name = def.Name;
                _bases = PythonOps.MakeEmptyList(def.Bases.Count);
                foreach (Compiler.Ast.Expression expr in def.Bases)
                    _bases.Add(Convert(expr));
                _body = ConvertStatements(def.Body);
            }

            public string name {
                get { return _name; }
                set { _name = value; }
            }

            public PythonList bases {
                get { return _bases; }
                set { _bases = value; }
            }

            public PythonList body {
                get { return _body; }
                set { _body = value; }
            }
        }

        [PythonType]
        public class Compare : expr
        {
            private expr _left;
            private PythonList _ops;
            private PythonList _comparators;

            public Compare() {
                _fields = new PythonTuple(new[] { "left", "ops", "comparators" });
            }

            internal Compare(BinaryExpression expr, cmpop op)
                : this() {
                _left = Convert(expr.Left);
                _ops = PythonOps.MakeListNoCopy(op);
                _comparators = PythonOps.MakeListNoCopy(Convert(expr.Right));
            }

            public expr left {
                get { return _left; }
                set { _left = value; }
            }

            public PythonList ops {
                get { return _ops; }
                set { _ops = value; }
            }

            public PythonList comparators {
                get { return _comparators; }
                set { _comparators = value; }
            }
        }

        [PythonType]
        public class Continue : stmt
        {
            internal static Continue Instance = new Continue();
        }

        [PythonType]
        public class Del : expr_context
        {
            internal static Del Instance = new Del();
        }

        [PythonType]
        public class Delete : stmt
        {
            private PythonList _targets;

            public Delete() {
                _fields = new PythonTuple(new[] { "targets", });
            }

            internal Delete(DelStatement stmt)
                : this() {
                _targets = PythonOps.MakeEmptyList(stmt.Expressions.Count);
                foreach (Compiler.Ast.Expression expr in stmt.Expressions)
                    _targets.Add(Convert(expr, Del.Instance));
            }

            public PythonList targets {
                get { return _targets; }
                set { _targets = value; }
            }
        }

        [PythonType]
        public class Dict : expr
        {
            private PythonList _keys;
            private PythonList _values;

            public Dict() {
                _fields = new PythonTuple(new[] { "keys", "values" });
            }

            internal Dict(DictionaryExpression expr)
                : this() {
                _keys = PythonOps.MakeEmptyList(expr.Items.Count);
                _values = PythonOps.MakeEmptyList(expr.Items.Count);
                foreach (SliceExpression item in expr.Items) {
                    _keys.Add(Convert(item.SliceStart));
                    _values.Add(Convert(item.SliceStop));
                }
            }

            public PythonList keys {
                get { return _keys; }
                set { _keys = value; }
            }

            public PythonList values {
                get { return _values; }
                set { _values = value; }
            }
        }

        [PythonType]
        public class Div : @operator
        {
            internal static Div Instance = new Div();
        }

        [PythonType]
        public class Ellipsis : slice
        {
            internal static Ellipsis Instance = new Ellipsis();
        }

        [PythonType]
        public class Eq : cmpop
        {
            internal static Eq Instance = new Eq();
        }

        [PythonType]
        public class Exec : stmt
        {
            private expr _body;
            private expr _globals; // Optional
            private expr _locals; // Optional

            public Exec() {
                _fields = new PythonTuple(new[] { "body", "globals", "locals" });
            }

            public Exec(ExecStatement stmt)
                : this() {
                _body = Convert(stmt.Code);
                if (stmt.Globals != null)
                    _globals = Convert(stmt.Globals);
                if (stmt.Locals != null)
                    _locals = Convert(stmt.Locals);
            }

            public expr body {
                get { return _body; }
                set { _body = value; }
            }

            public expr globals {
                get { return _globals; }
                set { _globals = value; }
            }

            public expr locals {
                get { return _locals; }
                set { _locals = value; }
            }
        }

        [PythonType]
        public class Expr : stmt
        {
            private expr _value;

            public Expr() {
                _fields = new PythonTuple(new[] { "value", });
            }

            internal Expr(ExpressionStatement stmt)
                : this() {
                _value = Convert(stmt.Expression);
            }

            public expr value {
                get { return _value; }
                set { _value = value; }
            }
        }

        [PythonType]
        public class Expression : mod
        {
            private expr _body;

            public Expression() {
                _fields = new PythonTuple(new[] { "body", });
            }

            internal Expression(SuiteStatement suite)
                : this() {
                _body = Convert(((ExpressionStatement)suite.Statements[0]).Expression);
            }

            public expr body {
                get { return _body; }
                set { _body = value; }
            }

            internal override PythonList GetStatements() {
                return PythonOps.MakeListNoCopy(_body);
            }
        }

        [PythonType]
        public class ExtSlice : slice
        {
            private PythonList _dims;

            public ExtSlice() {
                _fields = new PythonTuple(new[] { "dims", });
            }

            internal ExtSlice(PythonList dims)
                : this() {
                _dims = dims;
            }

            public PythonList dims {
                get { return _dims; }
                set { _dims = value; }
            }
        }

        [PythonType]
        public class FloorDiv : @operator
        {
            internal static FloorDiv Instance = new FloorDiv();
        }

        [PythonType]
        public class For : stmt
        {
            private expr _target;
            private expr _iter;
            private PythonList _body;
            private PythonList _orelse; // Optional, default []

            public For() {
                _fields = new PythonTuple(new[] { "target", "iter", "body", "orelse" });
            }

            internal For(ForStatement stmt)
                : this() {
                _target = Convert(stmt.Left, Store.Instance);
                _iter = Convert(stmt.List);
                _body = ConvertStatements(stmt.Body);
                _orelse = ConvertStatements(stmt.Else, true);
            }

            public expr target {
                get { return _target; }
                set { _target = value; }
            }

            public expr iter {
                get { return _iter; }
                set { _iter = value; }
            }

            public PythonList body {
                get { return _body; }
                set { _body = value; }
            }

            public PythonList orelse {
                get { return _orelse; }
                set { _orelse = value; }
            }
        }

        [PythonType]
        public class FunctionDef : stmt
        {
            private string _name;
            private arguments _args;
            private PythonList _body;
            private PythonList _decorators;

            public FunctionDef() {
                _fields = new PythonTuple(new[] { "name", "args", "body", "decorators" });
            }

            internal FunctionDef(FunctionDefinition def)
                : this() {
                _name = def.Name;
                _args = new arguments(def.Parameters);
                _body = ConvertStatements(def.Body);

                if (def.Decorators != null) {
                    _decorators = PythonOps.MakeEmptyList(def.Decorators.Count);
                    foreach (Compiler.Ast.Expression expr in def.Decorators)
                        _decorators.Add(Convert(expr));
                } else
                    _decorators = PythonOps.MakeEmptyList(0);
            }

            public string name {
                get { return _name; }
                set { _name = value; }
            }

            public arguments args {
                get { return _args; }
                set { _args = value; }
            }

            public PythonList body {
                get { return _body; }
                set { _body = value; }
            }

            public PythonList decorators {
                get { return _decorators; }
                set { _decorators = value; }
            }
        }

        [PythonType]
        public class GeneratorExp : expr
        {
            private expr _elt;
            private PythonList _generators;

            public GeneratorExp() {
                _fields = new PythonTuple(new[] { "elt", "generators" });
            }

            internal GeneratorExp(GeneratorExpression expr)
                : this() {
                ExtractListComprehensionIterators walker = new ExtractListComprehensionIterators();
                expr.Function.Body.Walk(walker);
                ComprehensionIterator[] iters = walker.Iterators;
                Debug.Assert(iters.Length != 0, "A generator expression cannot have zero iterators.");
                iters[0] = new ComprehensionFor(((ComprehensionFor)iters[0]).Left, expr.Iterable);
                _elt = Convert(walker.Yield.Expression);
                _generators = Convert(iters);
            }

            public expr elt {
                get { return _elt; }
                set { _elt = value; }
            }

            public PythonList generators {
                get { return _generators; }
                set { _generators = value; }
            }


            internal class ExtractListComprehensionIterators : PythonWalker
            {
                private readonly List<ComprehensionIterator> _iterators = new List<ComprehensionIterator>();
                public YieldExpression Yield;

                public ComprehensionIterator[] Iterators {
                    get { return _iterators.ToArray(); }
                }

                public override bool Walk(ForStatement node) {
                    _iterators.Add(new ComprehensionFor(node.Left, node.List));
                    node.Body.Walk(this);
                    return false;
                }

                public override bool Walk(IfStatement node) {
                    _iterators.Add(new ComprehensionIf(node.Tests[0].Test));
                    node.Tests[0].Body.Walk(this);
                    return false;
                }

                public override bool Walk(YieldExpression node) {
                    Yield = node;
                    return false;
                }
            }
        }

        [PythonType]
        public class Global : stmt
        {
            private PythonList _names;

            public Global() {
                _fields = new PythonTuple(new[] { "names", });
            }

            internal Global(GlobalStatement stmt)
                : this() {
                _names = new PythonList(stmt.Names);
            }

            public PythonList names {
                get { return _names; }
                set { _names = value; }
            }
        }

        [PythonType]
        public class Gt : cmpop
        {
            internal static Gt Instance = new Gt();
        }

        [PythonType]
        public class GtE : cmpop
        {
            internal static GtE Instance = new GtE();
        }

        [PythonType]
        public class If : stmt
        {
            private expr _test;
            private PythonList _body;
            private PythonList _orelse; // Optional, default []

            public If() {
                _fields = new PythonTuple(new[] { "test", "body", "orelse" });
            }

            internal If(IfStatement stmt)
                : this() {
                If current = this;
                If parent = null;
                foreach (IfStatementTest ifTest in stmt.Tests) {
                    if (parent != null) {
                        current = new If();
                        parent._orelse = PythonOps.MakeListNoCopy(current);
                    }

                    current.Initialize(ifTest);
                    parent = current;
                }

                current._orelse = ConvertStatements(stmt.ElseStatement, true);
            }

            internal void Initialize(IfStatementTest ifTest) {
                _test = Convert(ifTest.Test);
                _body = ConvertStatements(ifTest.Body);
            }

            public expr test {
                get { return _test; }
                set { _test = value; }
            }

            public PythonList body {
                get { return _body; }
                set { _body = value; }
            }

            public PythonList orelse {
                get { return _orelse; }
                set { _orelse = value; }
            }
        }

        [PythonType]
        public class IfExp : expr
        {
            private expr _test;
            private expr _body;
            private expr _orelse;

            public IfExp() {
                _fields = new PythonTuple(new[] { "test", "body", "orelse" });
            }

            internal IfExp(ConditionalExpression cond)
                : this() {
                _test = Convert(cond.Test);
                _body = Convert(cond.TrueExpression);
                _orelse = Convert(cond.FalseExpression);
            }

            public expr test {
                get { return _test; }
                set { _test = value; }
            }

            public expr body {
                get { return _body; }
                set { _body = value; }
            }

            public expr orelse {
                get { return _orelse; }
                set { _orelse = value; }
            }
        }

        [PythonType]
        public class Import : stmt
        {
            private PythonList _names;

            public Import() {
                _fields = new PythonTuple(new[] { "names", });
            }

            internal Import(ImportStatement stmt)
                : this() {
                _names = ConvertAliases(stmt.Names, stmt.AsNames);
            }

            public PythonList names {
                get { return _names; }
                set { _names = value; }
            }
        }

        [PythonType]
        public class ImportFrom : stmt
        {
            private string _module;
            private PythonList _names;
            private int _level; // Optional, default 0

            public ImportFrom() {
                _fields = new PythonTuple(new[] { "module", "names", "level" });
            }

            public ImportFrom(FromImportStatement stmt)
                : this() {
                _module = stmt.Root.MakeString();
                _names = ConvertAliases(stmt.Names, stmt.AsNames);
                if (stmt.Root is RelativeModuleName)
                    _level = ((RelativeModuleName)stmt.Root).DotCount;
            }

            public string module {
                get { return _module; }
                set { _module = value; }
            }

            public PythonList names {
                get { return _names; }
                set { _names = value; }
            }

            public int level {
                get { return _level; }
                set { _level = value; }
            }
        }

        [PythonType]
        public class In : cmpop
        {
            internal static In Instance = new In();
        }

        [PythonType]
        public class Index : slice
        {
            private expr _value;

            public Index() {
                _fields = new PythonTuple(new[] { "value", });
            }

            internal Index(expr expr)
                : this() {
                _value = expr;
            }

            public expr value {
                get { return _value; }
                set { _value = value; }
            }
        }

        [PythonType]
        public class Interactive : mod
        {
            private PythonList _body;

            public Interactive() {
                _fields = new PythonTuple(new[] { "body", });
            }

            internal Interactive(SuiteStatement suite)
                : this() {
                _body = ConvertStatements(suite);
            }

            public PythonList body {
                get { return _body; }
                set { _body = value; }
            }

            internal override PythonList GetStatements() {
                return _body;
            }
        }

        [PythonType]
        public class Invert : unaryop
        {
            internal static Invert Instance = new Invert();
        }

        [PythonType]
        public class Is : cmpop
        {
            internal static Is Instance = new Is();
        }

        [PythonType]
        public class IsNot : cmpop
        {
            internal static IsNot Instance = new IsNot();
        }

        [PythonType]
        public class Lambda : expr
        {
            private arguments _args;
            private expr _body;

            public Lambda() {
                _fields = new PythonTuple(new[] { "args", "body" });
            }

            internal Lambda(LambdaExpression lambda)
                : this() {
                FunctionDef def = (FunctionDef)Convert(lambda.Function);
                _args = def.args;
                Debug.Assert(def.body.Count == 1, "LambdaExpression body should be one Return statement.");
                _body = ((Return)def.body[0]).value;
            }

            public arguments args {
                get { return _args; }
                set { _args = value; }
            }

            public expr body {
                get { return _body; }
                set { _body = value; }
            }
        }

        [PythonType]
        public class List : expr
        {
            private PythonList _elts;
            private expr_context _ctx;

            public List() {
                _fields = new PythonTuple(new[] { "elts", "ctx" });
            }

            internal List(ListExpression list, expr_context ctx)
                : this() {
                _elts = PythonOps.MakeEmptyList(list.Items.Count);
                foreach (Compiler.Ast.Expression expr in list.Items)
                    _elts.Add(Convert(expr, ctx));

                _ctx = ctx;
            }

            public PythonList elts {
                get { return _elts; }
                set { _elts = value; }
            }

            public expr_context ctx {
                get { return _ctx; }
                set { _ctx = value; }
            }
        }

        [PythonType]
        public class ListComp : expr
        {
            private expr _elt;
            private PythonList _generators;

            public ListComp() {
                _fields = new PythonTuple(new[] { "elt", "generators" });
            }

            internal ListComp(ListComprehension comp)
                : this() {
                _elt = Convert(comp.Item);
                _generators = Convert(comp.Iterators);
            }

            public expr elt {
                get { return _elt; }
                set { _elt = value; }
            }

            public PythonList generators {
                get { return _generators; }
                set { _generators = value; }
            }
        }

        [PythonType]
        public class Load : expr_context
        {
            internal static Load Instance = new Load();
        }

        [PythonType]
        public class Lt : cmpop
        {
            internal static Lt Instance = new Lt();
        }

        [PythonType]
        public class LtE : cmpop
        {
            internal static LtE Instance = new LtE();
        }

        [PythonType]
        public class LShift : @operator
        {
            internal static LShift Instance = new LShift();
        }

        [PythonType]
        public class Mod : @operator
        {
            internal static Mod Instance = new Mod();
        }

        [PythonType]
        public class Module : mod
        {
            private PythonList _body;

            public Module() {
                _fields = new PythonTuple(new[] { "body", });
            }

            internal Module(SuiteStatement suite)
                : this() {
                _body = ConvertStatements(suite);
            }

            public PythonList body {
                get { return _body; }
                set { _body = value; }
            }

            internal override PythonList GetStatements() {
                return _body;
            }
        }

        [PythonType]
        public class Mult : @operator
        {
            internal static Mult Instance = new Mult();
        }

        [PythonType]
        public class Name : expr
        {
            private string _id;
            private expr_context _ctx;

            public Name() {
                _fields = new PythonTuple(new[] { "id", "ctx" });
            }

            internal Name(NameExpression expr, expr_context ctx)
                : this(expr.Name, ctx) {
            }

            internal Name(string id, expr_context ctx)
                : this() {
                _id = id;
                _ctx = ctx;
            }

            public expr_context ctx {
                get { return _ctx; }
                set { _ctx = value; }
            }

            public string id {
                get { return _id; }
                set { _id = value; }
            }
        }

        [PythonType]
        public class Not : unaryop
        {
            internal static Not Instance = new Not();
        }

        [PythonType]
        public class NotEq : cmpop
        {
            internal static NotEq Instance = new NotEq();
        }

        [PythonType]
        public class NotIn : cmpop
        {
            internal static NotIn Instance = new NotIn();
        }

        [PythonType]
        public class Num : expr
        {
            private object _n;

            public Num() {
                _fields = new PythonTuple(new[] { "n", });
            }

            internal Num(object n)
                : this() {
                _n = n;
            }

            public object n {
                get { return _n; }
                set { _n = value; }
            }
        }

        [PythonType]
        public class Or : boolop
        {
            internal static Or Instance = new Or();
        }

        [PythonType]
        public class Param : expr_context
        {
            internal static Param Instance = new Param();
        }

        [PythonType]
        public class Pass : stmt
        {
            internal static Pass Instance = new Pass();
        }

        [PythonType]
        public class Pow : @operator
        {
            internal static Pow Instance = new Pow();
        }

        [PythonType]
        public class Print : stmt
        {
            private expr _dest;
            private PythonList _values;
            private bool _nl;

            public Print() {
                _fields = new PythonTuple(new[] { "dest", "values", "nl" });
            }

            internal Print(PrintStatement stmt)
                : this() {
                if (stmt.Destination != null)
                    _dest = Convert(stmt.Destination);

                _values = PythonOps.MakeEmptyList(stmt.Expressions.Count);
                foreach (Compiler.Ast.Expression expr in stmt.Expressions)
                    _values.Add(Convert(expr));

                _nl = !stmt.TrailingComma;
            }

            public expr dest {
                get { return _dest; }
                set { _dest = value; }
            }

            public PythonList values {
                get { return _values; }
                set { _values = value; }
            }

            public bool nl {
                get { return _nl; }
                set { _nl = value; }
            }
        }

        [PythonType]
        public class Raise : stmt
        {
            private expr _type; // Optional
            private expr _inst; // Optional
            private expr _tback; // Optional

            public Raise() {
                _fields = new PythonTuple(new[] { "type", "inst", "tback" });
            }

            internal Raise(RaiseStatement stmt)
                : this() {
                if (stmt.ExceptType != null)
                    _type = Convert(stmt.ExceptType);
                if (stmt.Value != null)
                    _inst = Convert(stmt.Value);
                if (stmt.Traceback != null)
                    _tback = Convert(stmt.Traceback);
            }

            public expr type {
                get { return _type; }
                set { _type = value; }
            }

            public expr inst {
                get { return _inst; }
                set { _inst = value; }
            }

            public expr tback {
                get { return _tback; }
                set { _tback = value; }
            }
        }

        [PythonType]
        public class Repr : expr
        {
            private expr _value;

            public Repr() {
                _fields = new PythonTuple(new[] { "value", });
            }

            internal Repr(BackQuoteExpression expr)
                : this() {
                _value = Convert(expr.Expression);
            }

            public expr value {
                get { return _value; }
                set { _value = value; }
            }
        }

        [PythonType]
        public class Return : stmt
        {
            private expr _value; // Optional

            public Return() {
                _fields = new PythonTuple(new[] { "value", });
            }

            public Return(ReturnStatement statement)
                : this() {
                // statement.Expression is never null
                //or is it?
                if (statement.Expression == null)
                    _value = null;
                else
                    _value = Convert(statement.Expression);
            }

            public expr value {
                get { return _value; }
                set { _value = value; }
            }
        }

        [PythonType]
        public class RShift : @operator
        {
            internal static RShift Instance = new RShift();
        }

        [PythonType]
        public class Slice : slice
        {
            private expr _lower; // Optional
            private expr _upper; // Optional
            private expr _step; // Optional

            public Slice() {
                _fields = new PythonTuple(new[] { "lower", "upper", "step" });
            }

            public Slice(SliceExpression expr)
                : this() {
                if (expr.SliceStart != null)
                    _lower = Convert(expr.SliceStart);
                if (expr.SliceStop != null)
                    _upper = Convert(expr.SliceStop);
                if (expr.StepProvided)
                    _step = Convert(expr.SliceStep);
            }

            public expr lower {
                get { return _lower; }
                set { _lower = value; }
            }

            public expr upper {
                get { return _upper; }
                set { _upper = value; }
            }

            public expr step {
                get { return _step; }
                set { _step = value; }
            }
        }

        [PythonType]
        public class Store : expr_context
        {
            internal static Store Instance = new Store();
        }

        [PythonType]
        public class Str : expr
        {
            private string _s;

            public Str() {
                _fields = new PythonTuple(new[] { "s", });
            }

            internal Str(string s)
                : this() {
                _s = s;
            }

            public string s {
                get { return _s; }
                set { _s = value; }
            }
        }

        [PythonType]
        public class Sub : @operator
        {
            internal static Sub Instance = new Sub();
        }

        [PythonType]
        public class Subscript : expr
        {
            private expr _value;
            private slice _slice;
            private expr_context _ctx;

            public Subscript() {
                _fields = new PythonTuple(new[] { "value", "slice", "ctx" });
            }

            internal Subscript(IndexExpression expr, expr_context ctx)
                : this() {
                _value = Convert(expr.Target);
                AST index = Convert(expr.Index);
                if (index is expr)
                    if (index is Tuple && ((TupleExpression)expr.Index).IsExpandable)
                        _slice = new ExtSlice(((Tuple)index).elts);
                    else
                        _slice = new Index((expr)index);
                else if (index is slice) // includes Ellipsis
                    _slice = (slice)index;
                else
                    throw new ArgumentTypeException("Unexpected index expression: " + expr.Index.GetType());

                _ctx = ctx;
            }

            public expr value {
                get { return _value; }
                set { _value = value; }
            }

            public slice slice {
                get { return _slice; }
                set { _slice = value; }
            }

            public expr_context ctx {
                get { return _ctx; }
                set { _ctx = value; }
            }
        }

        /// <summary>
        /// Not an actual node. We don't create this, but it's here for compatibility.
        /// </summary>
        [PythonType]
        public class Suite : mod
        {
            private PythonList _body;

            public Suite() {
                _fields = new PythonTuple(new[] { "body", });
            }

            public PythonList body {
                get { return _body; }
                set { _body = value; }
            }

            internal override PythonList GetStatements() {
                return _body;
            }
        }

        [PythonType]
        public class TryExcept : stmt
        {
            private PythonList _body;
            private PythonList _handlers;
            private PythonList _orelse; // Optional, default []

            public TryExcept() {
                _fields = new PythonTuple(new[] { "body", "handlers", "orelse" });
            }

            internal TryExcept(TryStatement stmt)
                : this() {
                _body = ConvertStatements(stmt.Body);

                _handlers = PythonOps.MakeEmptyList(stmt.Handlers.Count);
                foreach (TryStatementHandler tryStmt in stmt.Handlers)
                    _handlers.Add(Convert(tryStmt));

                _orelse = ConvertStatements(stmt.Else, true);
            }

            public PythonList body {
                get { return _body; }
                set { _body = value; }
            }

            public PythonList handlers {
                get { return _handlers; }
                set { _handlers = value; }
            }

            public PythonList orelse {
                get { return _orelse; }
                set { _orelse = value; }
            }
        }

        [PythonType]
        public class TryFinally : stmt
        {
            private PythonList _body;
            private PythonList _finalbody;

            public TryFinally() {
                _fields = new PythonTuple(new[] { "body", "finalbody" });
            }

            internal TryFinally(PythonList body, PythonList finalbody)
                : this() {
                _body = body;
                _finalbody = finalbody;
            }

            public PythonList body {
                get { return _body; }
                set { _body = value; }
            }

            public PythonList finalbody {
                get { return _finalbody; }
                set { _finalbody = value; }
            }
        }

        [PythonType]
        public class Tuple : expr
        {
            private PythonList _elts;
            private expr_context _ctx;

            public Tuple() {
                _fields = new PythonTuple(new[] { "elts", "ctx" });
            }

            internal Tuple(TupleExpression list, expr_context ctx)
                : this() {
                _elts = PythonOps.MakeEmptyList(list.Items.Count);
                foreach (Compiler.Ast.Expression expr in list.Items)
                    _elts.Add(Convert(expr, ctx));

                _ctx = ctx;
            }

            public PythonList elts {
                get { return _elts; }
                set { _elts = value; }
            }

            public expr_context ctx {
                get { return _ctx; }
                set { _ctx = value; }
            }
        }

        [PythonType]
        public class UnaryOp : expr
        {
            private unaryop _op;
            private expr _operand;

            public UnaryOp() {
                _fields = new PythonTuple(new[] { "op", "operand" });
            }

            internal UnaryOp(UnaryExpression expression)
                : this() {
                _op = (unaryop)Convert(expression.Op);
                _operand = Convert(expression.Expression);
            }

            public unaryop op {
                get { return _op; }
                set { _op = value; }
            }

            public expr operand {
                get { return _operand; }
                set { _operand = value; }
            }
        }

        [PythonType]
        public class UAdd : unaryop
        {
            internal static UAdd Instance = new UAdd();
        }

        [PythonType]
        public class USub : unaryop
        {
            internal static USub Instance = new USub();
        }

        [PythonType]
        public class While : stmt
        {
            private expr _test;
            private PythonList _body;
            private PythonList _orelse; // Optional, default []

            public While() {
                _fields = new PythonTuple(new[] { "test", "body", "orelse" });
            }

            internal While(WhileStatement stmt)
                : this() {
                _test = Convert(stmt.Test);
                _body = ConvertStatements(stmt.Body);
                _orelse = ConvertStatements(stmt.ElseStatement, true);
            }

            public expr test {
                get { return _test; }
                set { _test = value; }
            }

            public PythonList body {
                get { return _body; }
                set { _body = value; }
            }

            public PythonList orelse {
                get { return _orelse; }
                set { _orelse = value; }
            }
        }

        [PythonType]
        public class With : stmt
        {
            private expr _context_expr;
            private expr _optional_vars; // Optional
            private PythonList _body;

            public With() {
                _fields = new PythonTuple(new[] { "context_expr", "optional_vars", "body" });
            }

            internal With(WithStatement with)
                : this() {
                _context_expr = Convert(with.ContextManager);
                if (with.Variable != null)
                    _optional_vars = Convert(with.Variable);

                _body = ConvertStatements(with.Body);
            }

            public expr context_expr {
                get { return _context_expr; }
                set { _context_expr = value; }
            }

            public expr optional_vars {
                get { return _optional_vars; }
                set { _optional_vars = value; }
            }

            public PythonList body {
                get { return _body; }
                set { _body = value; }
            }
        }

        [PythonType]
        public class Yield : expr
        {
            private expr _value; // Optional

            public Yield() {
                _fields = new PythonTuple(new[] { "value", });
            }

            internal Yield(YieldExpression expr)
                : this() {
                // expr.Expression is never null
                _value = Convert(expr.Expression);
            }

            public expr value {
                get { return _value; }
                set { _value = value; }
            }
        }
    }
}
