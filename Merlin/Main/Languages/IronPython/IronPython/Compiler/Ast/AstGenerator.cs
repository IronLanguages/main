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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
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
        private readonly LambdaBuilder/*!*/ _block;                     // the DLR lambda that we are building
        private readonly CompilerContext/*!*/ _context;                 // compiler context (source unit, etc...) that we are compiling against
        private readonly bool _print;                                   // true if we should print expression statements
        private readonly LabelTarget _generatorLabel;                   // the label, if we're transforming for a generator function
        private int? _curLine;                                          // tracks what the current line we've emitted at code-gen time
        private MSAst.ParameterExpression _lineNoVar, _lineNoUpdated;   // the variable used for storing current line # and if we need to store to it
        private List<MSAst.ParameterExpression/*!*/> _temps;            // temporary variables allocated against the lambda so we can re-use them
        private MSAst.ParameterExpression _generatorParameter;          // the extra parameter receiving the instance of PythonGenerator
        private readonly BinderState/*!*/ _binderState;                 // the state stored for the binder
        private bool _inFinally;                                        // true if we are currently in a finally (coordinated with our loop state)
        private bool _disableInterpreter;                               // true if we generated loops, functions, etc... that shouldn't be interpreted        
        private LabelTarget _breakLabel;                                // the current label for break, if we're in a loop
        private LabelTarget _continueLabel;                             // the current label for continue, if we're in a loop
        private LabelTarget _returnLabel;                               // the label for the end of the current method, if "return" was used
        private readonly MSAst.SymbolDocumentInfo _document;            // if set, used to wrap expressions with debug information

        private static readonly Dictionary<string, MethodInfo> _HelperMethods = new Dictionary<string, MethodInfo>(); // cache of helper methods
        private static readonly MethodInfo _UpdateStackTrace = typeof(ExceptionHelpers).GetMethod("UpdateStackTrace");
        private static readonly MethodInfo _GetCurrentMethod = typeof(MethodBase).GetMethod("GetCurrentMethod");
        internal static readonly MSAst.Expression[] EmptyExpression = new MSAst.Expression[0];
        internal static readonly MSAst.BlockExpression EmptyBlock = Ast.Block(Ast.Empty());

        private AstGenerator(string name, bool generator, bool print) {
            _print = print;
            _generatorLabel = generator ? Ast.Label(typeof(object)) : null;

            _block = AstUtils.Lambda(typeof(object), name);
        }

        internal AstGenerator(AstGenerator/*!*/ parent, string name, bool generator, bool print)
            : this(name, generator, false) {
            Assert.NotNull(parent);
            _context = parent.Context;
            _binderState = parent.BinderState;
            _document = _context.SourceUnit.Document;
        }

        internal AstGenerator(CompilerContext/*!*/ context, SourceSpan span, string name, bool generator, bool print)
            : this(name, generator, print) {
            Assert.NotNull(context);
            _context = context;
            _binderState = new BinderState(Binder);
            _document = _context.SourceUnit.Document;
        }

        // We don't need to insert code to track lines in adaptive mode as the interpreter does that for us.
        public bool TrackLines {
            get { return !PythonContext.PythonOptions.AdaptiveCompilation; }
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

        public PythonDivisionOptions DivisionOptions {
            get {
                return PythonContext.PythonOptions.DivisionOptions;
            }
        }

        internal bool DisableInterpreter {
            get {
                return _disableInterpreter;
            }
            set {
                _disableInterpreter = value;
            }
        }

        private PythonContext/*!*/ PythonContext {
            get {
                return ((PythonContext)_context.SourceUnit.LanguageContext);
            }
        }

        public LambdaBuilder/*!*/ Block {
            get { return _block; }
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

        internal MSAst.ParameterExpression GeneratorParameter {
            get { return _generatorParameter; }
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
            return _block.HiddenVariable(type, name);
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
            return AstUtils.Assign(variable, AstUtils.Convert(right, variable.Type));
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
                /*ActionExpression ae = expression as ActionExpression;
                if (ae != null) {
                    // create a combo site which does the conversion
                    ParameterMappingInfo[] infos = new ParameterMappingInfo[ae.Arguments.Count];
                    for (int i = 0; i<infos.Length; i++) {
                        infos[i] = ParameterMappingInfo.Parameter(i);
                    }

                    expression = Ast.Dynamic(
                        new ComboBinder(
                            new BinderMappingInfo(
                                (MetaAction)ae.BindingInfo,
                                infos
                            ),
                            new BinderMappingInfo(
                                new ConversionBinder(
                                    BinderState,
                                    type,
                                    ConversionResultKind.ExplicitCast
                                ),
                                ParameterMappingInfo.Action(0)
                            )
                        ),
                        type,
                        ae.Arguments
                    );
                } else*/ {
                    expression = Binders.Convert(
                        BinderState,
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
            if (_returnLabel != null) {
                expression = Ast.Label(_returnLabel, AstUtils.Convert(expression, typeof(object)));
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
                    _lineNoVar = _block.HiddenVariable(typeof(int), "$lineNo");
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
                    _lineNoUpdated = _block.HiddenVariable(typeof(bool), "$lineUpdated");
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
                    AstUtils.CodeContext(),
                    Ast.Call(_GetCurrentMethod),
                    Ast.Constant(_block.Name),
                    Ast.Constant(Context.SourceUnit.Path ?? "<string>"),
                    LineNumberExpression
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
                        AstUtils.CodeContext(),
                        Ast.Call(typeof(MethodBase).GetMethod("GetCurrentMethod")),
                        Ast.Constant(_block.Name),
                        Ast.Constant(Context.SourceUnit.Path ?? "<string>"),
                        LineNumberExpression
                    )
                ),
                AstUtils.Assign(
                    LineNumberUpdated,
                    Ast.Constant(preventAdditionalAdds)
                ),
                Ast.Empty()
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
                return Ast.Constant(null, type);
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

            if (TrackLines && fromStmt.CanThrow &&        // don't need to update line tracking for statements that can't throw
                ((_curLine.HasValue && fromStmt.Start.IsValid && _curLine.Value != fromStmt.Start.Line) ||  // don't need to update unless line has changed
                (!_curLine.HasValue && fromStmt.Start.IsValid))) {  // do need to update if we don't yet have a valid line

                _curLine = fromStmt.Start.Line;
                updateLine = true;
            }

            MSAst.Expression toExpr = fromStmt.Transform(this);

            if (toExpr != null && updateLine) {
                toExpr = Ast.Block(
                    UpdateLineNumber(fromStmt.Start.Line),
                    toExpr,
                    Ast.Empty()
                );
            }

            return toExpr;
        }

        internal MSAst.Expression PushLineUpdated(bool updated, ParameterExpression saveCurrent) {
            if (TrackLines) {
                return MSAst.Expression.Block(
                        Ast.Assign(saveCurrent, LineNumberUpdated),
                        Ast.Assign(LineNumberUpdated, Ast.Constant(updated))
                    );
            } else {
                return MSAst.Expression.Empty();
            }
        }

        internal MSAst.Expression PopLineUpdated(ParameterExpression saveCurrent) {
            if (TrackLines) {
                return Ast.Assign(LineNumberUpdated, saveCurrent);
            } else {
                return MSAst.Expression.Empty();
            }
        }

        internal MSAst.Expression UpdateLineUpdated(bool updated) {
            if (TrackLines) {
                return Ast.Assign(LineNumberUpdated, Ast.Constant(updated));
            } else {
                return MSAst.Expression.Empty();
            }
        }

        internal MSAst.Expression UpdateLineNumber(int line) {
            if (TrackLines) {
                return Ast.Assign(LineNumberExpression, Ast.Constant(line));
            } else {
                return MSAst.Expression.Empty();
            }
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

        internal void CreateGeneratorParameter() {
            Debug.Assert(IsGenerator);
            Debug.Assert(_generatorParameter == null);
            _generatorParameter = Block.CreateHiddenParameter("$generator", typeof(PythonGenerator));
        }

        internal MSAst.Expression AddDecorators(MSAst.Expression ret, IList<Expression> decorators) {
            // add decorators
            if (decorators != null) {
                for (int i = decorators.Count - 1; i >= 0; i--) {
                    Expression decorator = decorators[i];
                    ret = Binders.Invoke(
                        BinderState,
                        typeof(object),
                        new CallSignature(1),
                        Transform(decorator),
                        ret
                    );
                }
            }
            return ret;
        }

    }
}
