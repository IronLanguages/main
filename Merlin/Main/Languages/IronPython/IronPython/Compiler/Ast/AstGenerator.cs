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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;

using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    internal class AstGenerator {
        private readonly CompilerContext/*!*/ _context;                 // compiler context (source unit, etc...) that we are compiling against
        private readonly bool _print;                                   // true if we should print expression statements
        private readonly LabelTarget _generatorLabel;                   // the label, if we're transforming for a generator function
        private readonly string _name;                                  // the name of the method, module, etc...
        private int? _curLine;                                          // tracks what the current line we've emitted at code-gen time
        private MSAst.ParameterExpression _lineNoVar, _lineNoUpdated;   // the variable used for storing current line # and if we need to store to it
        private List<MSAst.ParameterExpression/*!*/> _temps;            // temporary variables allocated against the lambda so we can re-use them
        private readonly BinderState/*!*/ _binderState;                 // the state stored for the binder
        private bool _inFinally;                                        // true if we are currently in a finally (coordinated with our loop state)
        private LabelTarget _breakLabel;                                // the current label for break, if we're in a loop
        private LabelTarget _continueLabel;                             // the current label for continue, if we're in a loop
        private LabelTarget _returnLabel;                               // the label for the end of the current method, if "return" was used
        private readonly MSAst.SymbolDocumentInfo _document;            // if set, used to wrap expressions with debug information
        private readonly GlobalAllocator/*!*/ _globals;                 // helper class for generating globals code gen
        private readonly List<ParameterExpression> _locals;             // local variables allocated during the transformation of the code
        private readonly List<ParameterExpression> _params;             // parameters allocated during the transformation of the code
        private List<ClosureInfo> _liftedVars;                         // list of all variables and which ones are closed over.
        private MSAst.ParameterExpression _localCodeContext;            // the current context if it's different from the global context.
        private readonly Profiler _profiler;                            // captures timing data if profiling
        private readonly AstGenerator/*!*/ _parent;                     // the parent generator
        private readonly string/*!*/ _profilerName;                     // a human-friendly name to be used as the output form the profiler
        private Dictionary<PythonVariable, MSAst.Expression> _localLifted; // expressions for how we refer to lifted variables locally
        internal bool _isEmittingFinally;                                // true if we're emitting a finally (used for proper handling of exception tracking during rethrow)

        private static readonly Dictionary<string, MethodInfo> _HelperMethods = new Dictionary<string, MethodInfo>(); // cache of helper methods
        private static readonly MethodInfo _UpdateStackTrace = typeof(ExceptionHelpers).GetMethod("UpdateStackTrace");
        private static readonly MethodInfo _GetCurrentMethod = typeof(MethodBase).GetMethod("GetCurrentMethod");
        internal static readonly MSAst.Expression[] EmptyExpression = new MSAst.Expression[0];
        internal static readonly MSAst.BlockExpression EmptyBlock = Ast.Block(AstUtils.Empty());
        private const string NameForExec = "module: <exec>";

        private AstGenerator(string name, bool generator, string profilerName, bool print) {
            _print = print;
            _generatorLabel = generator ? Ast.Label(typeof(object), "generatorLabel") : null;

            _name = name;
            _locals = new List<ParameterExpression>();
            _params = new List<ParameterExpression>();

            if (profilerName == null) {
                if (name.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0) {
                    _profilerName = "module " + name;
                } else {
                    _profilerName = "module " + System.IO.Path.GetFileNameWithoutExtension(name);
                }
            } else {
                _profilerName = profilerName;
            }
        }

        internal AstGenerator(AstGenerator/*!*/ parent, string name, bool generator, string profilerName)
            : this(name, generator, profilerName, false) {
            Assert.NotNull(parent);
            _context = parent.Context;
            _binderState = parent.BinderState;
            _parent = parent;
            _document = _context.SourceUnit.Document ?? Ast.SymbolDocument(name, PythonContext.LanguageGuid, PythonContext.VendorGuid);
            _profiler = parent._profiler;

            _globals = parent._globals;
        }

        internal AstGenerator(CompilationMode mode, CompilerContext/*!*/ context, SourceSpan span, string name, bool generator, bool print)
            : this(name, generator, null, print) {
            Assert.NotNull(context);
            _context = context;
            _binderState = new BinderState(Binder);
            _document = _context.SourceUnit.Document ?? Ast.SymbolDocument(name, PythonContext.LanguageGuid, PythonContext.VendorGuid);

            LanguageContext pc = context.SourceUnit.LanguageContext;
            switch (mode) {
                case CompilationMode.Collectable: _globals = new ArrayGlobalAllocator(pc); break;
                case CompilationMode.Lookup: _globals = new DictionaryGlobalAllocator(); break;
                case CompilationMode.ToDisk: _globals = new SavableGlobalAllocator(pc); break;
                case CompilationMode.Uncollectable: _globals = new StaticGlobalAllocator(pc, name); break;
            }

            PythonOptions po = (pc.Options as PythonOptions);
            Assert.NotNull(po);
            if (po.EnableProfiler && mode != CompilationMode.ToDisk) {
                _profiler = Profiler.GetProfiler(PythonContext);
                if (mode == CompilationMode.Lookup) {
                    _profilerName = NameForExec;
                }
            }
        }

        public bool Optimize {
            get { return PythonContext.PythonOptions.Optimize; }
        }

        public bool StripDocStrings {
            get { return PythonContext.PythonOptions.StripDocStrings; }
        }

        public bool DebugMode {
            get { return _context.SourceUnit.LanguageContext.DomainManager.Configuration.DebugMode; }
        }

        public bool IsGlobal {
            get {
                return _parent == null;
            }
        }

        public MSAst.Expression/*!*/ LocalContext {
            get {
                return _localCodeContext ?? Globals.GlobalContext;
            }
        }

        public GlobalAllocator/*!*/ Globals {
            get {
                return _globals;
            }
        }

        public PythonDivisionOptions DivisionOptions {
            get {
                return PythonContext.PythonOptions.DivisionOptions;
            }
        }

        private PythonContext/*!*/ PythonContext {
            get {
                return ((PythonContext)_context.SourceUnit.LanguageContext);
            }
        }

        public CompilerContext/*!*/ Context {
            get { return _context; }
        }

        public PythonBinder/*!*/ Binder {
            get { return (PythonBinder)_context.SourceUnit.LanguageContext.Binder; }
        }

        public BinderState/*!*/ BinderState {
            get { return _binderState; }
        }

        public bool PrintExpressions {
            get { return _print; }
        }

        internal bool IsGenerator {
            get { return _generatorLabel != null; }
        }

        internal LabelTarget GeneratorLabel {
            get { return _generatorLabel; }
        }

        public bool InLoop {
            get { return _breakLabel != null; }
        }

        public bool InFinally {
            get {
                return _inFinally;
            }
            set {
                _inFinally = value;
            }
        }

        public MSAst.LabelTarget BreakLabel {
            get { return _breakLabel; }
        }

        public MSAst.LabelTarget ContinueLabel {
            get { return _continueLabel; }
        }

        public MSAst.LabelTarget ReturnLabel {
            get {
                if (_returnLabel == null) {
                    _returnLabel = MSAst.Expression.Label(typeof(object));
                }
                return _returnLabel;
            }
        }

        public Dictionary<PythonVariable, MSAst.Expression> LocalLifted {
            get {
                return _localLifted;
            }
        }

        public void AddError(string message, SourceSpan span) {
            // TODO: error code
            _context.Errors.Add(_context.SourceUnit, message, span, -1, Severity.Error);
        }

        public MSAst.ParameterExpression/*!*/ GetTemporary(string name) {
            return GetTemporary(name, typeof(object));
        }

        public MSAst.ParameterExpression/*!*/ GetTemporary(string name, Type type) {
            if (_temps != null) {
                foreach (MSAst.ParameterExpression temp in _temps) {
                    if (temp.Type == type) {
                        _temps.Remove(temp);
                        return temp;
                    }
                }
            }
            return HiddenVariable(type, name);
        }

        internal void AddHiddenVariable(ParameterExpression tmp) {
            _locals.Add(tmp);
        }

        internal MSAst.ParameterExpression/*!*/ HiddenVariable(Type/*!*/ type, string/*!*/ name) {
            MSAst.ParameterExpression var = Ast.Parameter(type, name);
            _locals.Add(var);
            return var;
        }

        internal MSAst.ParameterExpression/*!*/ Variable(Type/*!*/ type, string/*!*/ name) {
            ParameterExpression result = Ast.Variable(type, name);
            _locals.Add(result);
            return result;
        }

        internal ClosureExpression/*!*/ LiftedVariable(PythonVariable/*!*/ variable, string/*!*/ name, bool accessInNestedScope) {
            ParameterExpression result = HiddenVariable(typeof(ClosureCell), name);

            ClosureExpression closureVar = new ClosureExpression(variable, result, null);
            EnsureLiftedVars();
            _liftedVars.Add(new DefinitionClosureInfo(closureVar, true));
            return closureVar;
        }

        internal ClosureExpression/*!*/ LiftedParameter(PythonVariable variable, string name) {
            ParameterExpression result = Ast.Variable(typeof(object), name);
            _params.Add(result);

            ClosureExpression closureVar = new ClosureExpression(variable, HiddenVariable(typeof(ClosureCell), name), result);
            EnsureLiftedVars();

            _liftedVars.Add(new DefinitionClosureInfo(closureVar, true));
            return closureVar;
        }

        internal void ReferenceVariable(PythonVariable variable, int index, MSAst.Expression localTuple, bool accessedInThisScope) {
            EnsureLiftedVars();

            _liftedVars.Add(new ReferenceClosureInfo(variable, index, localTuple, accessedInThisScope));
        }

        internal MSAst.Expression SetLocalLiftedVariable(PythonVariable/*!*/ variable, MSAst.Expression/*!*/ expr) {
            if (_localLifted == null) {
                _localLifted = new Dictionary<PythonVariable, MSAst.Expression>();
            }

            return _localLifted[variable] = expr;
        }

        private void EnsureLiftedVars() {
            if (_liftedVars == null) {
                _liftedVars = new List<ClosureInfo>();
            }
        }

        public int TupleIndex(PythonVariable var) {
            Debug.Assert(_parent._liftedVars != null);

            var vars = _parent._liftedVars;
            for (int i = 0; i < vars.Count; i++) {
                if (vars[i].PythonVariable == var) {
                    return i;
                }
            }
            
            throw new InvalidOperationException();
        }

        public Type GetParentTupleType() {
            Debug.Assert(_parent != null);
            Debug.Assert(_parent._liftedVars != null);

            return Microsoft.Scripting.Tuple.MakeTupleType(ArrayUtils.ConvertAll<ClosureInfo, Type>(_parent._liftedVars.ToArray(), x => typeof(ClosureCell)));
        }

        internal MSAst.ParameterExpression/*!*/ Parameter(Type/*!*/ type, string name) {
            ParameterExpression result = Ast.Variable(type, name);
            return Parameter(result);
        }

        internal MSAst.ParameterExpression Parameter(ParameterExpression/*!*/ result) {
            _params.Add(result);
            return result;
        }

        internal void CreateNestedContext() {
            _localCodeContext = Ast.Parameter(typeof(CodeContext), "$localContext");
        }

        internal MSAst.Expression/*!*/ MakeBody(MSAst.Expression/*!*/ parentContext, MSAst.Expression[] init, MSAst.Expression/*!*/ body, bool isVisible) {
            // wrap a CodeContext scope if needed
            Debug.Assert(!IsGlobal);

            if (_localCodeContext != null) {
                body = Ast.Block(
                    new[] { _localCodeContext },
                    init.Length == 0 ? (MSAst.Expression)MSAst.Expression.Empty() : (MSAst.Expression)Ast.Block(init),
                    Ast.Assign(
                        _localCodeContext,
                        CreateLocalContext(parentContext, isVisible)
                    ),
                    body
                );
            } else {
                body = Ast.Block(ArrayUtils.Append(init, body));
            }

            // wrap a scope if needed
            if (_locals != null && _locals.Count > 0) {
                body = Ast.Block(new ReadOnlyCollection<ParameterExpression>(_locals.ToArray()), body);
            }

            return body;
        }

        private MSAst.Expression/*!*/ CreateLocalContext(MSAst.Expression/*!*/ parentContext, bool isVisible) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("CreateLocalContext"),
                parentContext,
                _liftedVars != null ?
                    Microsoft.Scripting.Tuple.Create(ArrayUtils.ConvertAll<ClosureInfo, MSAst.Expression>(_liftedVars.ToArray(), x => x.GetClosureCellExpression())) :
                    Microsoft.Scripting.Tuple.Create(),
                _liftedVars != null ?
                Ast.Constant(ArrayUtils.ConvertAll<ClosureInfo, SymbolId>(_liftedVars.ToArray(), x => (x.IsClosedOver ? x.Name : SymbolId.Empty))) :
                    Ast.Constant(new SymbolId[0]),
                Ast.Constant(isVisible)
            );
        }

        internal MSAst.ParameterExpression/*!*/ CreateHiddenParameter(Type/*!*/ type, string name) {
            ParameterExpression result = Ast.Variable(type, name);
            _params.Add(result);
            return result;
        }

        public void FreeTemp(MSAst.ParameterExpression/*!*/ temp) {
            if (IsGenerator) {
                return;
            }

            if (_temps == null) {
                _temps = new List<MSAst.ParameterExpression/*!*/>();
            }
            _temps.Add(temp);
        }

        internal MSAst.Expression/*!*/ MakeAssignment(MSAst.ParameterExpression/*!*/ variable, MSAst.Expression/*!*/ right) {
            return Ast.Assign(variable, AstUtils.Convert(right, variable.Type));
        }

        internal MSAst.Expression/*!*/ MakeAssignment(MSAst.ParameterExpression/*!*/ variable, MSAst.Expression/*!*/ right, SourceSpan span) {
            return AddDebugInfo(MakeAssignment(variable, right), span);
        }

        internal static MSAst.Expression/*!*/ ConvertIfNeeded(MSAst.Expression/*!*/ expression, Type/*!*/ type) {
            Debug.Assert(expression != null);
            // Do we need conversion?
            if (!CanAssign(type, expression.Type)) {
                // Add conversion step to the AST
                expression = AstUtils.Convert(expression, type);
            }
            return expression;
        }

        internal MSAst.Expression/*!*/ DynamicConvertIfNeeded(MSAst.Expression/*!*/ expression, Type/*!*/ type) {
            Debug.Assert(expression != null);
            // Do we need conversion?
            if (!CanAssign(type, expression.Type)) {
                // Add conversion step to the AST
                DynamicExpression ae = expression as DynamicExpression;
                ReducableDynamicExpression rde = expression as ReducableDynamicExpression;

                if ((ae != null && ae.Binder is PythonBinaryOperationBinder) ||
                    (rde != null && rde.Binder is PythonBinaryOperationBinder)) {
                    // create a combo site which does the conversion
                    PythonBinaryOperationBinder binder;
                    MSAst.Expression[] args;
                    if (ae != null) {
                        binder = (PythonBinaryOperationBinder)ae.Binder;
                        args = ArrayUtils.ToArray(ae.Arguments);
                    } else {
                        binder = (PythonBinaryOperationBinder)rde.Binder;
                        args = rde.Args;
                    }

                    ParameterMappingInfo[] infos = new ParameterMappingInfo[args.Length];
                    for (int i = 0; i<infos.Length; i++) {
                        infos[i] = ParameterMappingInfo.Parameter(i);
                    }

                    expression = Globals.Dynamic(
                        BinderState.BinaryOperationRetType(
                            binder,
                            BinderState.Convert(
                                type,
                                ConversionResultKind.ExplicitCast
                            )
                        ),
                        type,
                        args
                    );
                } else {
                    expression = Convert(
                        type,
                        ConversionResultKind.ExplicitCast,
                        expression
                    );
                }
            }
            return expression;
        }

        internal static bool CanAssign(Type/*!*/ to, Type/*!*/ from) {
            return to.IsAssignableFrom(from) && (to.IsValueType == from.IsValueType);
        }

        public string GetDocumentation(Statement/*!*/ stmt) {
            if (StripDocStrings) {
                return null;
            }

            return stmt.Documentation;
        }

        internal MSAst.Expression/*!*/ AddDebugInfo(MSAst.Expression/*!*/ expression, SourceLocation start, SourceLocation end) {
            return Utils.AddDebugInfo(expression, _document, start, end);
        }

        internal MSAst.Expression/*!*/ AddDebugInfo(MSAst.Expression/*!*/ expression, SourceSpan location) {
            return Utils.AddDebugInfo(expression, _document, location.Start, location.End);
        }

        internal MSAst.Expression/*!*/ AddDebugInfoAndVoid(MSAst.Expression/*!*/ expression, SourceSpan location) {
            if (expression.Type != typeof(void)) {
                expression = AstUtils.Void(expression);
            }
            return AddDebugInfo(expression, location);
        }

        internal MSAst.Expression/*!*/ AddReturnTarget(MSAst.Expression/*!*/ expression) {
            return AddReturnTarget(expression, typeof(object));
        }

        internal MSAst.Expression/*!*/ AddReturnTarget(MSAst.Expression/*!*/ expression, Type type) {
            if (_returnLabel != null) {
                expression = Ast.Label(_returnLabel, AstUtils.Convert(expression, type));
                _returnLabel = null;
            }
            return expression;
        }

        #region Dynamic stack trace support

        /// <summary>
        /// A temporary variable to track the current line number
        /// </summary>
        private MSAst.ParameterExpression/*!*/ LineNumberExpression {
            get {
                if (_lineNoVar == null) {
                    _lineNoVar = HiddenVariable(typeof(int), "$lineNo");
                }

                return _lineNoVar;
            }
        }

        /// <summary>
        /// A temporary variable to track if the current line number has been emitted via the fault update block.
        /// 
        /// For example consider:
        /// 
        /// try:
        ///     raise Exception()
        /// except Exception, e:
        ///     # do something here
        ///     raise
        ///     
        /// At "do something here" we need to have already emitted the line number, when we re-raise we shouldn't add it 
        /// again.  If we handled the exception then we should have set the bool back to false.
        /// 
        /// We also sometimes directly check _lineNoUpdated to avoid creating this unless we have nested exceptions.
        /// </summary>
        private MSAst.ParameterExpression/*!*/ LineNumberUpdated {
            get {
                if (_lineNoUpdated == null) {
                    _lineNoUpdated = HiddenVariable(typeof(bool), "$lineUpdated");
                }

                return _lineNoUpdated;
            }
        }

        /// <summary>
        /// Wraps the body of a statement which should result in a frame being available during
        /// exception handling.  This ensures the line number is updated as the stack is unwound.
        /// </summary>
        internal MSAst.Expression/*!*/ WrapScopeStatements(MSAst.Expression/*!*/ body) {
            if (_lineNoVar == null) {
                // we have nothing that can throw, so don't emit the fault block at all
                return body;
            }

            return AstUtils.Try(
                body
            ).Fault(
                GetUpdateTrackbackExpression()
            );
        }

        /// <summary>
        /// Gets the expression for updating the dynamic stack trace at runtime when an
        /// exception is thrown.
        /// </summary>
        internal MSAst.Expression GetUpdateTrackbackExpression() {
            if (_lineNoUpdated == null) {
                return Ast.Call(
                    _UpdateStackTrace,
                    LocalContext,
                    Ast.Call(_GetCurrentMethod),
                    AstUtils.Constant(Name),
                    AstUtils.Constant(Context.SourceUnit.Path ?? "<string>"),
                    new LastFaultingLineExpression(LineNumberExpression)
                );
            }

            return GetLineNumberUpdateExpression(true);
        }

        /// <summary>
        /// Gets the expression for the actual updating of the line number for stack traces to be available
        /// </summary>
        internal MSAst.Expression GetLineNumberUpdateExpression(bool preventAdditionalAdds) {
            return Ast.Block(
                AstUtils.If(
                    Ast.Not(
                        LineNumberUpdated
                    ),                
                    Ast.Call(
                        typeof(ExceptionHelpers).GetMethod("UpdateStackTrace"),
                        LocalContext,
                        Ast.Call(typeof(MethodBase).GetMethod("GetCurrentMethod")),
                        AstUtils.Constant(Name),
                        AstUtils.Constant(Context.SourceUnit.Path ?? "<string>"),
                        new LastFaultingLineExpression(LineNumberExpression)
                    )
                ),
                Ast.Assign(
                    LineNumberUpdated,
                    AstUtils.Constant(preventAdditionalAdds)
                ),
                AstUtils.Empty()
            );            
        }

        #endregion

        #region Utility methods

        public MSAst.Expression Transform(Expression from) {
            return Transform(from, typeof(object));
        }

        public MSAst.Expression Transform(Expression from, Type/*!*/ type) {
            if (from != null) {
                return from.Transform(this, type);
            }
            return null;
        }

        public MSAst.Expression TransformAsObject(Expression from) {
            return TransformAndConvert(from, typeof(object));
        }

        public MSAst.Expression TransformAndConvert(Expression from, Type/*!*/ type) {
            if (from != null) {
                MSAst.Expression transformed = from.Transform(this, type);
                transformed = ConvertIfNeeded(transformed, type);
                return transformed;
            }
            return null;
        }

        internal MSAst.Expression TransformOrConstantNull(Expression expression, Type/*!*/ type) {
            if (expression == null) {
                return AstUtils.Constant(null, type);
            } else {
                return ConvertIfNeeded(expression.Transform(this, type), type);
            }
        }

        public MSAst.Expression TransformAndDynamicConvert(Expression from, Type/*!*/ type) {
            if (from != null) {
                MSAst.Expression transformed = from.Transform(this, typeof(object));
                transformed = DynamicConvertIfNeeded(transformed, type);
                return transformed;
            }
            return null;
        }

        public MSAst.Expression Transform(Statement from) {
            if (from == null) {
                return null;
            } else {
                return TransformWithLineNumberUpdate(from);                
            }
        }

        internal MSAst.Expression[] Transform(Expression[] expressions) {
            return Transform(expressions, typeof(object));
        }

        internal MSAst.Expression[] Transform(Expression[] expressions, Type/*!*/ type) {
            Debug.Assert(expressions != null);
            MSAst.Expression[] to = new MSAst.Expression[expressions.Length];
            for (int i = 0; i < expressions.Length; i++) {
                Debug.Assert(expressions[i] != null);
                to[i] = Transform(expressions[i], type);
            }
            return to;
        }

        internal MSAst.Expression[] TransformAndConvert(Expression[] expressions, Type/*!*/ type) {
            Debug.Assert(expressions != null);
            MSAst.Expression[] to = new MSAst.Expression[expressions.Length];
            for (int i = 0; i < expressions.Length; i++) {
                Debug.Assert(expressions[i] != null);
                to[i] = TransformAndConvert(expressions[i], type);
            }
            return to;
        }

        internal MSAst.Expression[] Transform(Statement/*!*/[]/*!*/ from) {
            Debug.Assert(from != null);
            MSAst.Expression[] to = new MSAst.Expression[from.Length];

            for (int i = 0; i < from.Length; i++) {
                Debug.Assert(from[i] != null);

                to[i] = TransformWithLineNumberUpdate(from[i]);
            }
            return to;
        }

        private MSAst.Expression TransformWithLineNumberUpdate(Statement/*!*/ fromStmt) {
            // add line number tracking when the line changes...  First we see if the
            // line number changes and then we transform the body.  This prevents the body
            // from updating the line info first.
            bool updateLine = false;

            if (fromStmt.CanThrow &&        // don't need to update line tracking for statements that can't throw
                ((_curLine.HasValue && fromStmt.Start.IsValid && _curLine.Value != fromStmt.Start.Line) ||  // don't need to update unless line has changed
                (!_curLine.HasValue && fromStmt.Start.IsValid))) {  // do need to update if we don't yet have a valid line

                _curLine = fromStmt.Start.Line;
                updateLine = true;
            }

            MSAst.Expression toExpr = fromStmt.Transform(this);

            if (toExpr != null && updateLine) {
                toExpr = Ast.Block(
                    UpdateLineNumber(fromStmt.Start.Line),
                    toExpr
                );
            }

            return toExpr;
        }

        internal MSAst.Expression PushLineUpdated(bool updated, ParameterExpression saveCurrent) {
            return MSAst.Expression.Block(
                    Ast.Assign(saveCurrent, LineNumberUpdated),
                    Ast.Assign(LineNumberUpdated, AstUtils.Constant(updated))
                );
        }

        internal MSAst.Expression PopLineUpdated(ParameterExpression saveCurrent) {
            return Ast.Assign(LineNumberUpdated, saveCurrent);
        }

        internal MSAst.Expression UpdateLineUpdated(bool updated) {
            return Ast.Assign(LineNumberUpdated, AstUtils.Constant(updated));
        }

        internal MSAst.Expression UpdateLineNumber(int line) {
            return AstUtils.SkipInterpret(
                Ast.Assign(LineNumberExpression, AstUtils.Constant(line))
            );
        }

        internal MSAst.Expression TransformLoopBody(Statement body, out LabelTarget breakLabel, out LabelTarget continueLabel) {
            // Save state
            bool savedInFinally = _inFinally;
            LabelTarget savedBreakLabel = _breakLabel;
            LabelTarget savedContinueLabel = _continueLabel;

            _inFinally = false;
            breakLabel = _breakLabel = Ast.Label();
            continueLabel = _continueLabel = Ast.Label();
            MSAst.Expression result;
            try {
                result = Transform(body);
            } finally {
                _inFinally = savedInFinally;
                _breakLabel = savedBreakLabel;
                _continueLabel = savedContinueLabel;
            }
            return result;
        }

        #endregion

        /// <summary>
        /// Returns MethodInfo of the Python helper method given its name.
        /// </summary>
        /// <param name="name">Method name to find.</param>
        /// <returns></returns>
        internal static MethodInfo GetHelperMethod(string/*!*/ name) {
            MethodInfo mi;
            lock (_HelperMethods) {
                if (!_HelperMethods.TryGetValue(name, out mi)) {
                    _HelperMethods[name] = mi = typeof(PythonOps).GetMethod(name);
                }
            }
            Debug.Assert(mi != null, "Missing Python helper: " + name);
            return mi;
        }

        /// <summary>
        /// Returns MethodInfo of the Python helper method given its name and signature.
        /// </summary>
        /// <param name="name">Name of the method to return</param>
        /// <param name="types">Parameter types</param>
        /// <returns></returns>
        internal static MethodInfo GetHelperMethod(string/*!*/ name, params Type/*!*/[]/*!*/ types) {
            MethodInfo mi = typeof(PythonOps).GetMethod(name, types);
#if DEBUG
            if (mi == null) {
                System.Text.StringBuilder sb = new System.Text.StringBuilder("(");
                for (int i = 0; i < types.Length; i++) {
                    if (i > 0) sb.Append(", ");
                    sb.Append(types[i].Name);
                }
                sb.Append(")");
                Debug.Assert(mi != null, "Missing Python helper: " + name + sb.ToString());
            }
#endif
            return mi;
        }

        internal MSAst.Expression AddDecorators(MSAst.Expression ret, IList<Expression> decorators) {
            // add decorators
            if (decorators != null) {
                for (int i = decorators.Count - 1; i >= 0; i--) {
                    Expression decorator = decorators[i];
                    ret = Invoke(
                        typeof(object),
                        new CallSignature(1),
                        Transform(decorator),
                        ret
                    );
                }
            }
            return ret;
        }

        internal static MSAst.Expression/*!*/ AddDefaultReturn(MSAst.Expression/*!*/ body, Type returnType) {
            if (body.Type == typeof(void) && returnType != typeof(void)) {
                body = Ast.Block(body, Ast.Default(returnType));
            }
            return body;
        }

        public IList<MSAst.ParameterExpression> Parameters {
            get {
                return _params;
            }
        }

        public string Name {
            get {
                return _name;
            }
        }

        internal string ProfilerName {
            get {
                if (_parent != null) {
                    return _parent.ProfilerName + ": " + _profilerName;
                } else {
                    return _profilerName;
                }
            }
        }

        internal MSAst.Expression AddProfiling(MSAst.Expression/*!*/ body) {
            if (_profiler != null) {
                MSAst.ParameterExpression tick = GetTemporary("$tick", typeof(long));
                bool unique = (_profilerName == NameForExec);
                body = _profiler.AddProfiling(body, tick, ProfilerName, unique);
            }
            return body;
        }

        internal bool ShouldInterpret {
            get {
                if (_globals is DictionaryGlobalAllocator) {
                    return false;
                }
                return ((PythonContext)_context.SourceUnit.LanguageContext).ShouldInterpret((PythonCompilerOptions)_context.Options, _context.SourceUnit);
            }
        }

        internal bool EmitDebugSymbols {
            get {
                return _context.SourceUnit.EmitDebugSymbols;
            }
        }

        internal ScriptCode MakeScriptCode(MSAst.Expression/*!*/ body, CompilerContext/*!*/ context, PythonAst/*!*/ ast) {
            return Globals.MakeScriptCode(Ast.Block(_locals, body), context, ast);
        }

        #region Binder Factories

        public MSAst.Expression/*!*/ Invoke(Type/*!*/ resultType, CallSignature signature, params MSAst.Expression/*!*/[]/*!*/ args) {
            PythonInvokeBinder invoke = BinderState.Invoke(signature);
            switch (args.Length) {
                case 0: return Globals.Dynamic(invoke, resultType, LocalContext);
                case 1: return Globals.Dynamic(invoke, resultType, LocalContext, args[0]);
                case 2: return Globals.Dynamic(invoke, resultType, LocalContext, args[0], args[1]);
                case 3: return Globals.Dynamic(invoke, resultType, LocalContext, args[0], args[1], args[2]);
                default:
                    return Globals.Dynamic(
                        invoke,
                        resultType,
                        ArrayUtils.Insert(LocalContext, args)
                    );
            }

        }

        public MSAst.Expression/*!*/ Convert(Type/*!*/ type, ConversionResultKind resultKind, MSAst.Expression/*!*/ target) {
            return Globals.Dynamic(
                BinderState.Convert(
                    type,
                    resultKind
                ),
                type,
                target
            );
        }

        public MSAst.Expression/*!*/ Operation(Type/*!*/ resultType, PythonOperationKind operation, MSAst.Expression arg0) {
            return Globals.Dynamic(
                Binders.UnaryOperationBinder(
                    BinderState,
                    operation
                ),
                resultType,
                arg0
            );
        }

        public MSAst.Expression/*!*/ Operation(Type/*!*/ resultType, PythonOperationKind operation, MSAst.Expression arg0, MSAst.Expression arg1) {
            return Globals.Dynamic(
                Binders.BinaryOperationBinder(
                    BinderState,
                    operation
                ),
                resultType,
                arg0,
                arg1
            );
        }

        public MSAst.Expression/*!*/ Set(Type/*!*/ resultType, string/*!*/ name, MSAst.Expression/*!*/ target, MSAst.Expression/*!*/ value) {
            return Globals.Dynamic(
                BinderState.SetMember(
                    name
                ),
                resultType,
                target,
                value
            );
        }

        public MSAst.Expression/*!*/ Get(Type/*!*/ resultType, string/*!*/ name, MSAst.Expression/*!*/ target) {
            return Binders.Get(LocalContext, BinderState, resultType, name, target);
        }

        public MSAst.Expression/*!*/ TryGet(Type/*!*/ resultType, string/*!*/ name, MSAst.Expression/*!*/ target) {
            return Binders.TryGet(LocalContext, BinderState, resultType, name, target);
        }

        public MSAst.Expression/*!*/ Delete(Type/*!*/ resultType, string/*!*/ name, MSAst.Expression/*!*/ target) {
            return Globals.Dynamic(
                BinderState.DeleteMember(
                    name
                ),
                resultType,
                target
            );
        }

        internal MSAst.Expression/*!*/ GetIndex(Type/*!*/ type, MSAst.Expression/*!*/[]/*!*/ expression) {
            return Globals.Dynamic(
                BinderState.GetIndex(
                    expression.Length
                ),
                type,
                expression
            );
        }

        internal MSAst.Expression/*!*/ GetSlice(Type/*!*/ type, MSAst.Expression/*!*/[]/*!*/ expression) {
            return Globals.Dynamic(
                BinderState.GetSlice,
                type,
                expression
            );
        }

        internal MSAst.Expression/*!*/ SetIndex(Type/*!*/ type, MSAst.Expression/*!*/[]/*!*/ expression) {
            return Globals.Dynamic(
                BinderState.SetIndex(
                    expression.Length - 1
                ),
                type,
                expression
            );
        }

        internal MSAst.Expression/*!*/ SetSlice(Type/*!*/ type, MSAst.Expression/*!*/[]/*!*/ expression) {
            return Globals.Dynamic(
                BinderState.SetSlice,
                type,
                expression
            );
        }

        internal MSAst.Expression/*!*/ DeleteIndex(Type/*!*/ type, MSAst.Expression/*!*/[]/*!*/ expression) {
            return Globals.Dynamic(
                BinderState.DeleteIndex(
                    expression.Length
                ),
                type,
                expression
            );
        }

        internal MSAst.Expression/*!*/ DeleteSlice(Type/*!*/ type, MSAst.Expression/*!*/[]/*!*/ expression) {
            return Globals.Dynamic(
                BinderState.DeleteSlice,
                type,
                expression
            );
        }

        #endregion

    }    
}
