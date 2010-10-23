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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
    
namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using AstBlock = Microsoft.Scripting.Ast.BlockBuilder;
    using AstExpressions = ReadOnlyCollectionBuilder<MSA.Expression>;
    
    internal sealed class AstGenerator {
        private static int _UniqueId;

        private readonly RubyContext/*!*/ _context;
        private readonly RubyCompilerOptions/*!*/ _compilerOptions;
        private readonly MSA.SymbolDocumentInfo _document;
        private readonly MSA.Expression _sequencePointClearance;
        private readonly RubyEncoding/*!*/ _encoding;
        private readonly Profiler _profiler;
        private readonly bool _printInteractiveResult;
        private readonly bool _debugCompiler;
        private readonly bool _debugMode;
        private readonly bool _traceEnabled;
        private readonly bool _savingToDisk;

        private IList<MSA.Expression> _fileInitializers; // lazy
        private MSA.Expression _sourcePathConstant; // lazy

        internal AstGenerator(RubyContext/*!*/ context, RubyCompilerOptions/*!*/ options, MSA.SymbolDocumentInfo document, RubyEncoding/*!*/ encoding,
            bool printInteractiveResult) {

            Assert.NotNull(context, options, encoding);
            _context = context;
            _compilerOptions = options;
            _debugMode = context.DomainManager.Configuration.DebugMode;
            _traceEnabled = context.RubyOptions.EnableTracing;
            _document = document;
            _sequencePointClearance = (document != null) ? Ast.ClearDebugInfo(document) : null;
            _encoding = encoding;
            _profiler = context.RubyOptions.Profile ? Profiler.Instance : null;
            _savingToDisk = context.RubyOptions.SavePath != null;
            _printInteractiveResult = printInteractiveResult;
#if SILVERLIGHT
            _debugCompiler = false;
#else
            _debugCompiler = Snippets.Shared.SaveSnippets;
#endif
        }

        public RubyCompilerOptions/*!*/ CompilerOptions { 
            get { return _compilerOptions; } 
        }

        public bool DebugMode { 
            get { return _debugMode; }
        }

        public Profiler Profiler {
            get { return _profiler; }
        }

        public bool DebugCompiler {
            get { return _debugCompiler; }
        }

        public bool TraceEnabled {
            get { return _traceEnabled; }
        }

        public bool SavingToDisk {
            get { return _savingToDisk; }
        }

        public string SourcePath {
            get { return _document != null ? _document.FileName : "(eval)"; }
        }

        public MSA.SymbolDocumentInfo Document {
            get { return _document; }
        }

        public bool PrintInteractiveResult {
            get { return _printInteractiveResult; }
        }

        public MSA.Expression/*!*/ SourcePathConstant {
            get {
                if (_sourcePathConstant == null) {
                    _sourcePathConstant = Ast.Constant(SourcePath, typeof(string));
                }
                return _sourcePathConstant;
            }
        }

        public RubyEncoding/*!*/ Encoding {
            get { return _encoding; }
        }

        internal RubyContext/*!*/ Context {
            get { return _context; }
        }

        internal void AddFileInitializer(MSA.Expression/*!*/ expression) {
            if (_fileInitializers == null) {
                _fileInitializers = new List<MSA.Expression>();
            }
            _fileInitializers.Add(expression);
        }

        internal IList<MSA.Expression>/*!*/ FileInitializers {
            get {
                return (IList<MSA.Expression>)_fileInitializers ?? AstFactory.EmptyExpressions;
            }
        }

        #region Lexical Scopes

        public abstract class LexicalScope {
            public LexicalScope Parent;
            public virtual bool IsLambda { get { return false; } }
        }

        public sealed class LoopScope : LexicalScope {
            private readonly MSA.Expression/*!*/ _redoVariable;
            private readonly MSA.Expression/*!*/ _resultVariable;
            private readonly MSA.LabelTarget/*!*/ _breakLabel;
            private readonly MSA.LabelTarget/*!*/ _continueLabel;
            public LoopScope _parentLoop;

            public MSA.Expression/*!*/ RedoVariable {
                get { return _redoVariable; }
            }

            public MSA.Expression/*!*/ ResultVariable {
                get { return _resultVariable; }
            }

            public MSA.LabelTarget/*!*/ BreakLabel {
                get { return _breakLabel; }
            }

            public MSA.LabelTarget/*!*/ ContinueLabel {
                get { return _continueLabel; }
            }

            public LoopScope ParentLoop {
                get { return _parentLoop; }
                set { _parentLoop = value; }
            }

            public LoopScope(MSA.Expression/*!*/ redoVariable, MSA.Expression/*!*/ resultVariable, MSA.LabelTarget/*!*/ breakLabel, MSA.LabelTarget/*!*/ continueLabel) {
                Assert.NotNull(redoVariable, resultVariable, breakLabel, continueLabel);
                _redoVariable = redoVariable;
                _resultVariable = resultVariable;
                _breakLabel = breakLabel;
                _continueLabel = continueLabel;
            }
        }

        public sealed class RescueScope : LexicalScope {
            private readonly MSA.Expression/*!*/ _retryingVariable;
            private readonly MSA.LabelTarget/*!*/ _retryLabel;

            public RescueScope _parentRescue;

            public MSA.Expression/*!*/ RetryingVariable {
                get { return _retryingVariable; }
            }

            public RescueScope ParentRescue {
                get { return _parentRescue; }
                set { _parentRescue = value; }
            }

            public MSA.LabelTarget/*!*/ RetryLabel {
                get { return _retryLabel; }
            }

            public RescueScope(MSA.Expression/*!*/ retryingVariable, MSA.LabelTarget/*!*/ retryLabel) {
                Assert.NotNull(retryingVariable, retryLabel);
                _retryingVariable = retryingVariable;
                _retryLabel = retryLabel;
            }
        }

        public class VariableScope : LexicalScope {
            private readonly ScopeBuilder/*!*/ _builder;
            private readonly MSA.Expression/*!*/ _selfVariable;
            private readonly MSA.ParameterExpression/*!*/ _runtimeScopeVariable;
            private VariableScope _parentVariableScope;

            public ScopeBuilder/*!*/ Builder {
                get { return _builder; }
            }

            public MSA.Expression/*!*/ SelfVariable {
                get { return _selfVariable; }
            }

            public MSA.ParameterExpression/*!*/ RuntimeScopeVariable {
                get { return _runtimeScopeVariable; }
            }

            public VariableScope ParentVariableScope {
                get { return _parentVariableScope; }
                set { _parentVariableScope = value; }
            }

            public VariableScope(ScopeBuilder/*!*/ locals, MSA.Expression/*!*/ selfVariable, MSA.ParameterExpression/*!*/ runtimeScopeVariable) {
                Assert.NotNull(locals, selfVariable, runtimeScopeVariable);
                _builder = locals;
                _runtimeScopeVariable = runtimeScopeVariable;
                _selfVariable = selfVariable;
            }
        }

        public abstract class FrameScope : VariableScope {
            // A unique id of the scope per app-domain. Used for caching calls in dynamic sites.
            private readonly int _uniqueId;

            private BlockScope _parentBlock;
            private RescueScope _parentRescue;
            private LoopScope _parentLoop;
            private MSA.LabelTarget _returnLabel;

            public int UniqueId {
                get { return _uniqueId; }
            }
            
            public BlockScope ParentBlock { get { return _parentBlock; } set { _parentBlock = value; } }
            public RescueScope ParentRescue { get { return _parentRescue; } set { _parentRescue = value; } }
            public LoopScope ParentLoop { get { return _parentLoop; } set { _parentLoop = value; } }

            public FrameScope(ScopeBuilder/*!*/ builder, MSA.Expression/*!*/ selfVariable, MSA.ParameterExpression/*!*/ runtimeScopeVariable)
                : base(builder, selfVariable, runtimeScopeVariable) {
                _uniqueId = Interlocked.Increment(ref _UniqueId);
            }

            internal MSA.LabelTarget/*!*/ ReturnLabel {
                get {
                    if (_returnLabel == null) {
                        _returnLabel = MSA.Expression.Label(typeof(object));
                    }
                    return _returnLabel;
                }
            }

            internal MSA.Expression/*!*/ AddReturnTarget(MSA.Expression/*!*/ expression) {
                if (expression.Type != typeof(object)) {
                    expression = AstUtils.Convert(expression, typeof(object));
                }
                if (_returnLabel != null) {
                    expression = Ast.Label(_returnLabel, expression);
                    _returnLabel = null;
                }
                return expression;
            }
        }

        public sealed class BlockScope : FrameScope {
            private readonly MSA.Expression/*!*/ _bfcVariable;
            private readonly MSA.LabelTarget/*!*/ _redoLabel;

            public override bool IsLambda {
                get { return true; }
            }

            public MSA.Expression/*!*/ BfcVariable {
                get { return _bfcVariable; }
            }

            public MSA.LabelTarget/*!*/ RedoLabel {
                get { return _redoLabel; }
            }

            public BlockScope(ScopeBuilder/*!*/ builder, MSA.Expression/*!*/ selfVariable, MSA.ParameterExpression/*!*/ runtimeScopeVariable,
                MSA.Expression/*!*/ bfcVariable, MSA.LabelTarget/*!*/ redoLabel)
                : base(builder, selfVariable, runtimeScopeVariable) {
                Assert.NotNull(bfcVariable, redoLabel);
                _bfcVariable = bfcVariable;
                _redoLabel = redoLabel;
            }
        }

        public sealed class MethodScope : FrameScope {
            private readonly MSA.Expression _blockVariable;
            private readonly string _methodName;
            private readonly Parameters _parameters;
            private MethodScope _parentMethod;

            public override bool IsLambda {
                get { return true; }
            }

            // use MakeMethodBlockParameterRead to access method's block parameter:
            public MSA.Expression BlockVariable {
                get { return _blockVariable; }
            }

            public MethodScope ParentMethod {
                get { return _parentMethod; }
                set { _parentMethod = value; }
            }

            // null for top-level code, used by block definition (to encode method name into stack frame)
            public string MethodName {
                get { return _methodName; }
            }

            // null for code that is evaluated outside a method scope, used by super-call
            public Parameters Parameters {
                get { return _parameters; }
            }

            public MethodScope(
                ScopeBuilder/*!*/ builder,
                MSA.Expression/*!*/ selfVariable,
                MSA.ParameterExpression/*!*/ runtimeScopeVariable,
                MSA.Expression blockVariable, 
                string methodName, 
                Parameters parameters)
                : base(builder, selfVariable, runtimeScopeVariable) {

                _blockVariable = blockVariable;
                _methodName = methodName;
                _parameters = parameters;
            }
        }

        public sealed class ModuleScope : VariableScope {
            private readonly bool _isSingleton;
            private ModuleScope _parentModule;

            public ModuleScope ParentModule {
                get { return _parentModule; }
                set { _parentModule = value; }
            }

            public bool IsSingleton {
                get { return _isSingleton; }
            }

            public ModuleScope(ScopeBuilder/*!*/ builder, MSA.Expression/*!*/ selfVariable, MSA.ParameterExpression/*!*/ runtimeScopeVariable, bool isSingleton)
                : base(builder, selfVariable, runtimeScopeVariable) {
                _isSingleton = isSingleton;
            }
        }

        private LexicalScope/*!*/ _currentElement;
        private MethodScope/*!*/ _currentMethod;
        private BlockScope _currentBlock;
        private LoopScope _currentLoop;
        private RescueScope _currentRescue;
        private VariableScope _currentVariableScope;
        private MethodScope _topLevelScope;

        // inner-most module (available only if we enter a module declaration in the current AST, not available in eval'd code or in a method):
        private ModuleScope _currentModule;

        // inner-most method or top-level frame:
        public MethodScope/*!*/ CurrentMethod {
            get {
                Debug.Assert(_currentMethod != null);
                return _currentMethod;
            }
        }

        // inner-most frame scope (block or method):
        public FrameScope/*!*/ CurrentFrame {
            get { return (FrameScope)CurrentBlock ?? CurrentMethod; }
        }

        // inner-most block scope within the current method frame:
        public BlockScope CurrentBlock {
            get { return _currentBlock; }
        }

        // inner-most loop within the current frame (block or method):
        public LoopScope CurrentLoop {
            get { return _currentLoop; }
        }

        // inner-most rescue within the current frame (block or method):
        public RescueScope CurrentRescue {
            get { return _currentRescue; }
        }

        // "self" variable of the current variable scope:
        public MSA.Expression/*!*/ CurrentSelfVariable {
            get { return _currentVariableScope.SelfVariable; }
        }

        // runtime scope variable of the current variable scope:
        public MSA.ParameterExpression/*!*/ CurrentScopeVariable {
            get { return _currentVariableScope.RuntimeScopeVariable; }
        }

        // inner-most scope builder:
        public ScopeBuilder/*!*/ CurrentScope {
            get { return _currentVariableScope.Builder; }
        }

        // top-level (source unit tree) method scope:
        public MethodScope TopLevelScope {
            get { return _topLevelScope; }
        }

        #endregion

        #region Entering and Leaving Lexical Scopes

        public void EnterLoop(MSA.Expression/*!*/ redoVariable, MSA.Expression/*!*/ resultVariable, MSA.LabelTarget/*!*/ breakLabel, MSA.LabelTarget/*!*/ continueLabel) {
            Assert.NotNull(redoVariable, resultVariable, breakLabel, continueLabel);

            LoopScope loop = new LoopScope(redoVariable, resultVariable, breakLabel, continueLabel);

            loop.Parent = _currentElement;
            loop.ParentLoop = _currentLoop;

            _currentElement = _currentLoop = loop;
        }

        public void LeaveLoop() {
            Debug.Assert(_currentElement == _currentLoop);
            _currentElement = _currentLoop.Parent;
            _currentLoop = _currentLoop.ParentLoop;
        }

        public void EnterRescueClause(MSA.Expression/*!*/ retryingVariable, MSA.LabelTarget/*!*/ retryLabel) {
            Assert.NotNull(retryingVariable, retryLabel);

            RescueScope body = new RescueScope(retryingVariable, retryLabel);

            body.Parent = _currentElement;
            body.ParentRescue = _currentRescue;

            _currentElement = _currentRescue = body;
        }

        public void LeaveRescueClause() {
            Debug.Assert(_currentElement == _currentRescue);
            _currentElement = _currentRescue.Parent;
            _currentRescue = _currentRescue.ParentRescue;
        }

        public void EnterBlockDefinition(
            ScopeBuilder/*!*/ locals,
            MSA.Expression/*!*/ bfcVariable,
            MSA.Expression/*!*/ selfVariable,
            MSA.ParameterExpression/*!*/ runtimeScopeVariable, 
            MSA.LabelTarget/*!*/ redoLabel) {
            Assert.NotNull(locals, bfcVariable, selfVariable);
            Assert.NotNull(redoLabel);

            BlockScope block = new BlockScope(locals, selfVariable, runtimeScopeVariable, bfcVariable, redoLabel);
            block.Parent = _currentElement;
            block.ParentRescue = _currentRescue;
            block.ParentLoop = _currentLoop;
            block.ParentBlock = _currentBlock;
            block.ParentVariableScope = _currentVariableScope;
            
            _currentElement = block;
            _currentRescue = null;
            _currentLoop = null;
            _currentBlock = block;
            _currentVariableScope = block;
        }

        public void LeaveBlockDefinition() {
            Debug.Assert(_currentElement == _currentBlock);
            BlockScope oldBlock = _currentBlock;

            _currentElement = oldBlock.Parent;
            _currentRescue = oldBlock.ParentRescue;
            _currentLoop = oldBlock.ParentLoop;
            _currentVariableScope = oldBlock.ParentVariableScope;
            _currentBlock = oldBlock.ParentBlock;
        }

        public void EnterMethodDefinition(
            ScopeBuilder/*!*/ locals, 
            MSA.Expression/*!*/ selfParameter,
            MSA.ParameterExpression/*!*/ runtimeScopeVariable,
            MSA.Expression blockParameter,
            string/*!*/ methodName,
            Parameters parameters) {
            Assert.NotNull(locals, selfParameter, runtimeScopeVariable);

            MethodScope method = new MethodScope(
                locals,
                selfParameter, 
                runtimeScopeVariable, 
                blockParameter, 
                methodName, 
                parameters
            );

            method.Parent = _currentElement;
            method.ParentRescue = _currentRescue;
            method.ParentLoop = _currentLoop;
            method.ParentBlock = _currentBlock;
            method.ParentVariableScope = _currentVariableScope;
            method.ParentMethod = _currentMethod;

            _currentElement = method;
            _currentRescue = null;
            _currentLoop = null;
            _currentBlock = null;
            _currentVariableScope = method;
            _currentMethod = method;
        }

        public void LeaveMethodDefinition() {
            Debug.Assert(_currentElement == _currentMethod);
            MethodScope oldMethod = _currentMethod;

            _currentElement = oldMethod.Parent;
            _currentRescue = oldMethod.ParentRescue;
            _currentLoop = oldMethod.ParentLoop;
            _currentBlock = oldMethod.ParentBlock;
            _currentVariableScope = oldMethod.ParentVariableScope;
            _currentMethod = oldMethod.ParentMethod;
        }

        public void EnterModuleDefinition(
            ScopeBuilder/*!*/ locals,
            MSA.Expression/*!*/ selfVariable,
            MSA.ParameterExpression/*!*/ runtimeScopeVariable, 
            bool isSingleton) {
            Assert.NotNull(locals, selfVariable, runtimeScopeVariable);

            ModuleScope module = new ModuleScope(locals, selfVariable, runtimeScopeVariable, isSingleton);

            module.Parent = _currentElement;
            module.ParentVariableScope = _currentVariableScope;
            module.ParentModule = _currentModule;

            _currentElement = module;
            _currentVariableScope = module;
            _currentModule = module;
        }

        public void LeaveModuleDefinition() {
            Debug.Assert(_currentElement == _currentModule);
            ModuleScope oldModule = _currentModule;

            _currentElement = oldModule.Parent;
            _currentVariableScope = oldModule.ParentVariableScope;
            _currentModule = oldModule.ParentModule;
        }

        public void EnterFileInitializer(
            ScopeBuilder/*!*/ locals,
            MSA.Expression/*!*/ selfVariable,
            MSA.ParameterExpression/*!*/ runtimeScopeVariable) {

            VariableScope scope = new VariableScope(locals, selfVariable, runtimeScopeVariable);

            scope.Parent = _currentElement;
            scope.ParentVariableScope = _currentVariableScope;

            _currentElement = scope;
            _currentVariableScope = scope;
        }

        public void LeaveFileInitializer() {
            Debug.Assert(_currentElement == _currentVariableScope);
            VariableScope oldScope = _currentVariableScope;

            _currentElement = oldScope.Parent;
            _currentVariableScope = oldScope.ParentVariableScope;
        }

        public void EnterSourceUnit(
            ScopeBuilder/*!*/ locals,
            MSA.Expression/*!*/ selfParameter,
            MSA.ParameterExpression/*!*/ runtimeScopeVariable,
            MSA.Expression blockParameter,
            string methodName,
            Parameters parameters) {
            Assert.NotNull(locals, selfParameter, runtimeScopeVariable);

            Debug.Assert(_currentElement == null && _currentLoop == null && _currentRescue == null &&
                _currentVariableScope == null && _currentModule == null && _currentBlock == null && _currentMethod == null);

            EnterMethodDefinition(
                locals,
                selfParameter,
                runtimeScopeVariable,
                blockParameter,
                methodName,
                parameters);

            _topLevelScope = _currentMethod;
        }

        public void LeaveSourceUnit() {
            Debug.Assert(_currentElement == _currentMethod && _currentVariableScope == _currentMethod);
            Debug.Assert(_currentLoop == null && _currentRescue == null);
            Debug.Assert(_currentBlock == null);

            _currentElement = null;
            _currentMethod = null;
            _currentVariableScope = null;
            _topLevelScope = null;
        }

        #endregion

        /// <summary>
        /// Gets the inner most scope that compiles to a lambda expression.
        /// </summary>
        private VariableScope/*!*/ GetCurrentLambdaScope() {
            LexicalScope scope = _currentVariableScope;
            while (!scope.IsLambda) {
                scope = scope.Parent;
            }
            return (VariableScope)scope;
        }

        /// <summary>
        /// Makes a read of the current method's block parameter. 
        /// </summary>
        internal MSA.Expression/*!*/ MakeMethodBlockParameterRead() {
            VariableScope lambdaScope = GetCurrentLambdaScope();
            if (lambdaScope == CurrentMethod && CurrentMethod.BlockVariable != null) {
                return CurrentMethod.BlockVariable;
            } else {
                // TODO: we can optimize and inline for 1..n levels of nesting:
                return Methods.GetMethodBlockParameter.OpCall(CurrentScopeVariable);
            }
        }

        /// <summary>
        /// Makes a read of the Self property of the current method's block parameter. 
        /// Returns Null constant in top-level code.
        /// </summary>
        internal MSA.Expression/*!*/ MakeMethodBlockParameterSelfRead() {
            VariableScope lambdaScope = GetCurrentLambdaScope();
            if (lambdaScope == CurrentMethod && CurrentMethod.BlockVariable != null) {
                return Methods.GetProcSelf.OpCall(CurrentMethod.BlockVariable);
            } else {
                // TODO: we can optimize and inline for 1..n levels of nesting:
                return Methods.GetMethodBlockParameterSelf.OpCall(CurrentScopeVariable);
            }
        }

        internal AstExpressions/*!*/ TransformExpressions(IList<Expression>/*!*/ arguments) {
            Assert.NotNull(arguments);
            return TranformExpressions(arguments, new AstExpressions(arguments.Count));
        }

        internal AstExpressions/*!*/ TranformExpressions(IList<Expression>/*!*/ arguments, AstExpressions/*!*/ result) {
            Assert.NotNullItems(arguments);
            Assert.NotNull(result);

            foreach (Expression arg in arguments) {
                result.Add(arg.TransformRead(this));
            }

            return result;
        }

        internal MSA.Expression/*!*/ TransformStatements(Statements/*!*/ statements, ResultOperation resultOperation) {
            return TransformStatements(null, statements, null, resultOperation);
        }

        internal MSA.Expression/*!*/ TransformStatements(MSA.Expression prologue, Statements/*!*/ statements, ResultOperation resultOperation) {
            return TransformStatements(prologue, statements, null, resultOperation);
        }

        internal MSA.Expression/*!*/ TransformStatements(MSA.Expression prologue, Statements/*!*/ statements, MSA.Expression epilogue, 
            ResultOperation resultOperation) {

            Assert.NotNull(statements);

            int count = statements.Count + (prologue != null ? 1 : 0) + (epilogue != null ? 1 : 0);

            if (count == 0) {

                if (resultOperation.IsIgnore) {
                    return AstUtils.Empty();
                } else if (resultOperation.Variable != null) {
                    return Ast.Assign(resultOperation.Variable, AstUtils.Constant(null, resultOperation.Variable.Type));
                } else {
                    return Ast.Return(CurrentFrame.ReturnLabel, AstUtils.Constant(null));
                }

            } else if (count == 1) {
                if (prologue != null) {
                    return prologue;
                }

                if (epilogue != null) {
                    return epilogue;
                }

                if (resultOperation.IsIgnore) {
                    return statements.First.Transform(this);
                } else {
                    return statements.First.TransformResult(this, resultOperation);
                }

            } else {
                var result = new AstBlock();

                if (prologue != null) {
                    result.Add(prologue);
                }

                // transform all but the last statement if it is an expression stmt:
                foreach (var statement in statements.AllButLast) {
                    result.Add(statement.Transform(this));
                }

                if (statements.Count > 0) {
                    if (resultOperation.IsIgnore) {
                        result.Add(statements.Last.Transform(this));
                    } else {
                        result.Add(statements.Last.TransformResult(this, resultOperation));
                    }
                }

                if (epilogue != null) {
                    result.Add(epilogue);
                }

                result.Add(AstUtils.Empty());
                return result;
            }
        }

        internal MSA.Expression/*!*/ TransformStatementsToBooleanExpression(Statements statements, bool positive) {
            return TransformStatementsToExpression(statements, true, positive);
        }

        internal MSA.Expression/*!*/ TransformStatementsToExpression(Statements statements) {
            return TransformStatementsToExpression(statements, false, false);
        }

        private MSA.Expression/*!*/ TransformStatementsToExpression(Statements statements, bool toBoolean, bool positive) {

            if (statements == null || statements.Count == 0) {
                return toBoolean ? AstUtils.Constant(!positive) : AstUtils.Constant(null);
            }

            var last = toBoolean ? statements.Last.TransformCondition(this, positive) : statements.Last.TransformReadStep(this);
            if (statements.Count == 1) {
                return last;
            }

            var result = new AstBlock();
            foreach (var statement in statements.AllButLast) {
                result.Add(statement.Transform(this));
            }
            result.Add(last);

            return result;
        }

        internal AstExpressions/*!*/ TransformMapletsToExpressions(IList<Maplet>/*!*/ maplets) {
            Assert.NotNullItems(maplets);
            return TransformMapletsToExpressions(maplets, new AstExpressions(maplets.Count * 2));
        }

        internal AstExpressions/*!*/ TransformMapletsToExpressions(IList<Maplet>/*!*/ maplets, AstExpressions/*!*/ result) {
            Assert.NotNullItems(maplets);
            Assert.NotNull(result);

            foreach (Maplet entry in maplets) {
                result.Add(entry.Key.TransformRead(this));
                result.Add(entry.Value.TransformRead(this));
            }

            return result;
        }

        public MSA.Expression/*!*/ TransformToHashConstructor(IList<Maplet>/*!*/ maplets) {
            return MakeHashOpCall(TransformMapletsToExpressions(maplets));
        }

        internal MSA.Expression/*!*/ MakeHashOpCall(IEnumerable<MSA.Expression>/*!*/ expressions) {
            return Methods.MakeHash.OpCall(CurrentScopeVariable, AstUtils.NewArrayHelper(typeof(object), expressions));
        }

        internal static bool CanAssign(Type/*!*/ to, Type/*!*/ from) {
            return to.IsAssignableFrom(from) && (to.IsValueType == from.IsValueType);
        }

        internal MSA.Expression/*!*/ AddReturnTarget(MSA.Expression/*!*/ expression) {
            return CurrentFrame.AddReturnTarget(expression);
        }

        internal MSA.LabelTarget/*!*/ ReturnLabel {
            get { return CurrentFrame.ReturnLabel; }
        }

        internal MSA.Expression/*!*/ Return(MSA.Expression/*!*/ expression) {
            MSA.LabelTarget returnLabel = ReturnLabel;
            if (returnLabel.Type != typeof(void) && expression.Type == typeof(void)) {
                expression = Ast.Block(expression, AstUtils.Constant(null, typeof(object)));
            } else if (returnLabel.Type != expression.Type) {
                if (!CanAssign(returnLabel.Type, expression.Type)) {
                    // Add conversion step to the AST
                    expression = Ast.Convert(expression, returnLabel.Type);
                }
            }
            return Ast.Return(returnLabel, expression);
        }

        internal MSA.Expression ClearDebugInfo() {
            return _sequencePointClearance;
        }

        internal MSA.Expression/*!*/ AddDebugInfo(MSA.Expression/*!*/ expression, SourceSpan location) {
            if (_document != null) {
                // TODO: should we add clearance for non-goto expressions?
                // return AstUtils.AddDebugInfo(expression, _document, location.Start, location.End);
                var sequencePoint = Ast.DebugInfo(_document, location.Start.Line, location.Start.Column, location.End.Line, location.End.Column);
                return Ast.Block(sequencePoint, expression);
            } else {
                return expression;
            }
        }

        internal MSA.Expression DebugMarker(string/*!*/ marker) {
            return _debugCompiler ? Methods.X.OpCall(AstUtils.Constant(marker)) : null;
        }

        internal MSA.Expression/*!*/ DebugMark(MSA.Expression/*!*/ expression, string/*!*/ marker) {
            return _debugCompiler ? Ast.Block(Methods.X.OpCall(AstUtils.Constant(marker)), expression) : expression;
        }

        internal MSA.Expression/*!*/ TryCatchAny(MSA.Expression/*!*/ tryBody, MSA.Expression/*!*/ catchBody) {
            var variable = CurrentScope.DefineHiddenVariable("#value", tryBody.Type);

            return
                Ast.Block(
                    Ast.TryCatch(
                        Ast.Assign(variable, tryBody),
                        Ast.Catch(typeof(Exception), 
                            Ast.Assign(variable, catchBody)
                        )
                    ),
                    variable
                );
        }
    }
}
