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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Dynamic;
using System.Text;
using System.Threading;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Compiler.Generation;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using MSA = System.Linq.Expressions;

    internal class AstGenerator {
        private static int _UniqueId;

        private readonly RubyBinder/*!*/ _binder;
        private readonly RubyCompilerOptions/*!*/ _compilerOptions;
        private readonly SourceUnit/*!*/ _sourceUnit;
        private readonly MSA.SymbolDocumentInfo _document;
        private readonly Encoding/*!*/ _encoding;
        private readonly Profiler _profiler;
        private readonly bool _debugCompiler;
        private readonly bool _debugMode;
        private readonly bool _traceEnabled;
        private readonly bool _savingToDisk;

        internal AstGenerator(RubyBinder/*!*/ binder, RubyCompilerOptions/*!*/ options, SourceUnit/*!*/ sourceUnit, Encoding/*!*/ encoding,
            bool debugCompiler, bool debugMode, bool traceEnabled, bool profilerEnabled, bool savingToDisk) {

            Assert.NotNull(binder, options, encoding);
            _binder = binder;
            _compilerOptions = options;
            _debugCompiler = debugCompiler;
            _debugMode = debugMode;
            _traceEnabled = traceEnabled;
            _sourceUnit = sourceUnit;
            _document = sourceUnit.Document;
            _encoding = encoding;
            _profiler = profilerEnabled ? Profiler.Instance : null;
            _savingToDisk = savingToDisk;
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

        public SourceUnit/*!*/ SourceUnit {
            get { return _sourceUnit; }
        }

        public Encoding Encoding {
            get { return _encoding; }
        }

        internal ActionBinder Binder {
            get {
                return _binder;
            }
        }

        #region Lexical Scopes

        public abstract class LexicalScope {
            public LexicalScope Parent;
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
            private readonly MSA.LabelTarget/*!*/ _breakLabel;
            private readonly MSA.LabelTarget/*!*/ _continueLabel;

            public RescueScope _parentRescue;

            public MSA.Expression/*!*/ RetryingVariable {
                get { return _retryingVariable; }
            }

            public RescueScope ParentRescue {
                get { return _parentRescue; }
                set { _parentRescue = value; }
            }

            public MSA.LabelTarget/*!*/ BreakLabel {
                get { return _breakLabel; }
            }

            public MSA.LabelTarget/*!*/ ContinueLabel {
                get { return _continueLabel; }
            }

            public RescueScope(MSA.Expression/*!*/ retryingVariable, MSA.LabelTarget/*!*/ breakLabel, MSA.LabelTarget/*!*/ continueLabel) {
                Assert.NotNull(retryingVariable, breakLabel, continueLabel);
                _retryingVariable = retryingVariable;
                _breakLabel = breakLabel;
                _continueLabel = continueLabel;
            }
        }

        public class VariableScope : LexicalScope {
            private readonly ScopeBuilder/*!*/ _builder;
            private readonly MSA.Expression/*!*/ _selfVariable;
            private readonly MSA.Expression/*!*/ _runtimeScopeVariable;
            private VariableScope _parentVariableScope;

            public ScopeBuilder/*!*/ Builder {
                get { return _builder; }
            }

            public MSA.Expression/*!*/ SelfVariable {
                get { return _selfVariable; }
            }

            public MSA.Expression/*!*/ RuntimeScopeVariable {
                get { return _runtimeScopeVariable; }
            }

            public VariableScope ParentVariableScope {
                get { return _parentVariableScope; }
                set { _parentVariableScope = value; }
            }

            public VariableScope(ScopeBuilder/*!*/ locals, MSA.Expression/*!*/ selfVariable, MSA.Expression/*!*/ runtimeScopeVariable) {
                Assert.NotNull(selfVariable, runtimeScopeVariable);
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

            public FrameScope(ScopeBuilder/*!*/ builder, MSA.Expression/*!*/ selfVariable, MSA.Expression/*!*/ runtimeScopeVariable)
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
                    expression = AstFactory.Box(expression);
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

            public MSA.Expression/*!*/ BfcVariable {
                get { return _bfcVariable; }
            }

            public MSA.LabelTarget/*!*/ RedoLabel {
                get { return _redoLabel; }
            }

            public BlockScope(ScopeBuilder/*!*/ builder, MSA.Expression/*!*/ selfVariable, MSA.Expression/*!*/ runtimeScopeVariable,
                MSA.Expression/*!*/ bfcVariable, MSA.LabelTarget/*!*/ redoLabel)
                : base(builder, selfVariable, runtimeScopeVariable) {
                Assert.NotNull(bfcVariable, redoLabel);
                _bfcVariable = bfcVariable;
                _redoLabel = redoLabel;
            }
        }

        public sealed class MethodScope : FrameScope {
            private readonly MSA.Expression _blockVariable;
            private readonly MSA.Expression/*!*/ _rfcVariable;
            private readonly MSA.Expression _currentMethodVariable;
            private readonly string _methodName;
            private readonly Parameters _parameters;
            private MethodScope _parentMethod;

            public MSA.Expression BlockVariable {
                get { return _blockVariable; }
            }

            public MSA.Expression/*!*/ RfcVariable {
                get { return _rfcVariable; }
            }

            public MethodScope ParentMethod {
                get { return _parentMethod; }
                set { _parentMethod = value; }
            }

            // TODO: super call
            // non-null if !IsTopLevelCode
            public string MethodName {
                get { return _methodName; }
            }

            // TODO: super call
            // non-null if !IsTopLevelCode
            public Parameters Parameters {
                get { return _parameters; }
            }

            // non-null if !IsTopLevelCode
            public MSA.Expression CurrentMethodVariable {
                get { return _currentMethodVariable; }
            }

            public bool IsTopLevelCode {
                get { return _parentMethod == null; }
            }

            public MethodScope(
                ScopeBuilder/*!*/ builder, 
                MSA.Expression/*!*/ selfVariable, 
                MSA.Expression/*!*/ runtimeScopeVariable,
                MSA.Expression blockVariable, 
                MSA.Expression/*!*/ rfcVariable,
                MSA.Expression currentMethodVariable, 
                string methodName, 
                Parameters parameters)
                : base(builder, selfVariable, runtimeScopeVariable) {

                Assert.NotNull(rfcVariable);
                _blockVariable = blockVariable;
                _rfcVariable = rfcVariable;
                _methodName = methodName;
                _parameters = parameters;
                _currentMethodVariable = currentMethodVariable;
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

            public ModuleScope(ScopeBuilder/*!*/ builder, MSA.Expression/*!*/ selfVariable, MSA.Expression/*!*/ runtimeScopeVariable, bool isSingleton)
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
        private ModuleScope _currentModule;

        // inner-most module (not reset by method definition):
        public ModuleScope CurrentModule {
            get { return _currentModule; }
        }

        // inner-most method frame:
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

        // RFC variable of the current method frame:
        public MSA.Expression/*!*/ CurrentRfcVariable {
            get { return CurrentMethod.RfcVariable; }
        }

        // "self" variable of the current variable scope:
        public MSA.Expression/*!*/ CurrentSelfVariable {
            get { return _currentVariableScope.SelfVariable; }
        }

        // runtime scope variable of the current variable scope:
        public MSA.Expression/*!*/ CurrentScopeVariable {
            get { return _currentVariableScope.RuntimeScopeVariable; }
        }

        // inner-most scope builder:
        public ScopeBuilder/*!*/ CurrentScope {
            get { return _currentVariableScope.Builder; }
        }

        public ModuleScope GetCurrentNonSingletonModule() {
            ModuleScope scope = CurrentModule;
            while (scope != null && scope.IsSingleton) {
                scope = scope.ParentModule;
            }
            return scope;
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

        public void EnterRescueClause(MSA.Expression/*!*/ retryingVariable, MSA.LabelTarget/*!*/ breakLabel, MSA.LabelTarget/*!*/ continueLabel) {
            Assert.NotNull(retryingVariable, breakLabel, continueLabel);

            RescueScope body = new RescueScope(retryingVariable, breakLabel, continueLabel);

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
            MSA.Expression/*!*/ runtimeScopeVariable, 
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
            MSA.Expression/*!*/ runtimeScopeVariable,
            MSA.Expression blockParameter,
            MSA.Expression/*!*/ rfcVariable,
            MSA.Expression currentMethodVariable,
            string/*!*/ methodName,
            Parameters parameters) {
            Assert.NotNull(locals, selfParameter, runtimeScopeVariable, rfcVariable);

            MethodScope method = new MethodScope(
                locals,
                selfParameter, 
                runtimeScopeVariable, 
                blockParameter, 
                rfcVariable,
                currentMethodVariable, 
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
            MSA.Expression/*!*/ runtimeScopeVariable, 
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

        public void EnterSourceUnit(
            ScopeBuilder/*!*/ locals,
            MSA.Expression/*!*/ selfParameter,
            MSA.Expression/*!*/ runtimeScopeVariable,
            MSA.Expression blockParameter,
            MSA.Expression/*!*/ rfcVariable,
            MSA.Expression currentMethodVariable,
            string methodName,
            Parameters parameters) {
            Assert.NotNull(locals, selfParameter, runtimeScopeVariable, rfcVariable);

            Debug.Assert(_currentElement == null && _currentLoop == null && _currentRescue == null &&
                _currentVariableScope == null && _currentModule == null && _currentBlock == null && _currentMethod == null);

            EnterMethodDefinition(
                locals,
                selfParameter,
                runtimeScopeVariable,
                blockParameter,
                rfcVariable,
                currentMethodVariable,
                methodName,
                parameters);
        }

        public void LeaveSourceUnit() {
            Debug.Assert(_currentElement == _currentMethod && _currentVariableScope == _currentMethod);
            Debug.Assert(_currentLoop == null && _currentRescue == null);
            Debug.Assert(_currentBlock == null);

            _currentElement = null;
            _currentMethod = null;
            _currentVariableScope = null;
        }

        #endregion

        /// <summary>
        /// Makes a read of the current method's block parameter. 
        /// Returns Null constant in top-level code.
        /// </summary>
        internal MSA.Expression/*!*/ MakeMethodBlockParameterRead() {
            Debug.Assert(CurrentMethod != null);

            if (CurrentMethod.BlockVariable != null) {
                return CurrentMethod.BlockVariable;
            } else {
                return Ast.Constant(null, typeof(Proc));
            }
        }

        internal List<MSA.Expression>/*!*/ TranformExpressions(List<Expression>/*!*/ arguments) {
            Assert.NotNull(arguments);
            return TranformExpressions(arguments, new List<MSA.Expression>(arguments.Count));
        }

        internal List<MSA.Expression>/*!*/ TranformExpressions(List<Expression>/*!*/ arguments, List<MSA.Expression>/*!*/ result) {
            Assert.NotNullItems(arguments);
            Assert.NotNull(result);

            foreach (Expression arg in arguments) {
                result.Add(arg.TransformRead(this));
            }

            return result;
        }

        internal MSA.Expression/*!*/ TransformStatements(List<Expression>/*!*/ statements, ResultOperation resultOperation) {
            return TransformStatements(null, statements, null, resultOperation);
        }

        internal MSA.Expression/*!*/ TransformStatements(MSA.Expression prologue, List<Expression>/*!*/ statements, ResultOperation resultOperation) {
            return TransformStatements(prologue, statements, null, resultOperation);
        }

        internal MSA.Expression/*!*/ TransformStatements(MSA.Expression prologue, List<Expression>/*!*/ statements, MSA.Expression epilogue, 
            ResultOperation resultOperation) {

            Assert.NotNullItems(statements);

            int count = statements.Count + (prologue != null ? 1 : 0) + (epilogue != null ? 1 : 0);

            if (count == 0) {

                if (resultOperation.IsIgnore) {
                    return Ast.Empty();
                } else if (resultOperation.Variable != null) {
                    return Ast.Assign(resultOperation.Variable, Ast.Constant(null, resultOperation.Variable.Type));
                } else {
                    return Ast.Return(CurrentFrame.ReturnLabel, Ast.Constant(null));
                }

            } else if (count == 1) {
                if (prologue != null) {
                    return prologue;
                }

                if (epilogue != null) {
                    return epilogue;
                }

                if (resultOperation.IsIgnore) {
                    return statements[0].Transform(this);
                } else {
                    return statements[0].TransformResult(this, resultOperation);
                }

            } else {
                var result = new MSA.Expression[count + 1];
                int resultIndex = 0;

                if (prologue != null) {
                    result[resultIndex++] = prologue;
                }

                // transform all but the last statement if it is an expression stmt:
                for (int i = 0; i < statements.Count - 1; i++) {
                    result[resultIndex++] = statements[i].Transform(this);
                }

                if (statements.Count > 0) {
                    if (resultOperation.IsIgnore) {
                        result[resultIndex++] = statements[statements.Count - 1].Transform(this);
                    } else {
                        result[resultIndex++] = statements[statements.Count - 1].TransformResult(this, resultOperation);
                    }
                }

                if (epilogue != null) {
                    result[resultIndex++] = epilogue;
                }

                result[resultIndex++] = MSA.Expression.Empty();
                Debug.Assert(resultIndex == result.Length);

                return Ast.Block(new ReadOnlyCollection<MSA.Expression>(result));
            }
        }

        internal MSA.Expression/*!*/ TransformStatementsToExpression(List<Expression> statements) {
            if (statements == null || statements.Count == 0) {
                return Ast.Constant(null);
            }

            if (statements.Count == 1) {
                return statements[0].TransformRead(this);
            }

            var result = new MSA.Expression[statements.Count];
            for (int i = 0; i < result.Length - 1; i++) {
                result[i] = statements[i].Transform(this);
            }
            result[result.Length - 1] = statements[statements.Count - 1].TransformRead(this);

            return Ast.Block(new ReadOnlyCollection<MSA.Expression>(result));
        }

        internal List<MSA.Expression>/*!*/ TransformMapletsToExpressions(IList<Maplet>/*!*/ maplets) {
            Assert.NotNullItems(maplets);
            return TransformMapletsToExpressions(maplets, new List<MSA.Expression>(maplets.Count * 2));
        }

        internal List<MSA.Expression>/*!*/ TransformMapletsToExpressions(IList<Maplet>/*!*/ maplets, List<MSA.Expression>/*!*/ result) {
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

        internal MSA.Expression/*!*/ MakeHashOpCall(List<MSA.Expression>/*!*/ expressions) {
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
                expression = Ast.Block(expression, Ast.Constant(null, typeof(object)));
            } else if (returnLabel.Type != expression.Type) {
                if (!CanAssign(returnLabel.Type, expression.Type)) {
                    // Add conversion step to the AST
                    expression = Ast.Convert(expression, returnLabel.Type);
                }
            }
            return Ast.Return(returnLabel, expression);
        }

        internal MSA.Expression/*!*/ AddDebugInfo(MSA.Expression/*!*/ expression, SourceSpan location) {
            if (_document == null || !location.IsValid) {
                return expression;
            }
            return MSA.Expression.DebugInfo(expression, _document,
                location.Start.Line, location.Start.Column, location.End.Line, location.End.Column);
        }

        internal MSA.Expression/*!*/ DebugMarker(string/*!*/ marker) {
            return _debugCompiler ? Methods.X.OpCall(Ast.Constant(marker)) : (MSA.Expression)Ast.Empty();
        }

        internal MSA.Expression/*!*/ DebugMark(MSA.Expression/*!*/ expression, string/*!*/ marker) {
            return _debugCompiler ? AstFactory.Block(Methods.X.OpCall(Ast.Constant(marker)), expression) : expression;
        }

        internal virtual void TraceCallSite(Expression/*!*/ expression, MSA.DynamicExpression/*!*/ callSite) {
        }

        internal string/*!*/ EncodeMethodName(string/*!*/ name, SourceSpan location) {
            string encoded = name;

            // TODO: hack
            // encodes line number, file name into the method name
#if !SILVERLIGHT && !DEBUG
            if (!_debugMode)
#endif
            {
                string fileName = _sourceUnit.HasPath ? Path.GetFileName(_sourceUnit.Path) : String.Empty;
                encoded = String.Format("{0};{1};{2}", encoded, fileName, location.Start.Line);
            }

            return String.IsNullOrEmpty(encoded) ? RubyExceptionData.TopLevelMethodName : encoded;
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
