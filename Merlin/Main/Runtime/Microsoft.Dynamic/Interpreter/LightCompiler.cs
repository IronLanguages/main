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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Microsoft.Scripting.Runtime;
using System.Runtime.CompilerServices;

namespace Microsoft.Scripting.Interpreter {
    public sealed class ExceptionHandler {
        public readonly Type ExceptionType;
        public readonly int StartIndex;
        public readonly int EndIndex;
        public readonly int LabelIndex;
        public readonly int HandlerStartIndex;

        public bool IsFault { get { return ExceptionType == null; } }

        internal ExceptionHandler(int start, int end, int labelIndex, int handlerStartIndex, Type exceptionType) {
            StartIndex = start;
            EndIndex = end;
            LabelIndex = labelIndex;
            ExceptionType = exceptionType;
            HandlerStartIndex = handlerStartIndex;
        }

        public bool Matches(Type exceptionType, int index) {
            if (index >= StartIndex && index < EndIndex) {
                if (ExceptionType == null || ExceptionType.IsAssignableFrom(exceptionType)) {
                    return true;
                }
            }
            return false;
        }

        public bool IsBetterThan(ExceptionHandler other) {
            if (other == null) return true;

            if (StartIndex == other.StartIndex && EndIndex == other.EndIndex) {
                return HandlerStartIndex < other.HandlerStartIndex;
            }

            if (StartIndex > other.StartIndex) {
                Debug.Assert(EndIndex <= other.EndIndex);
                return true;
            } else if (EndIndex < other.EndIndex) {
                Debug.Assert(StartIndex == other.StartIndex);
                return true;
            } else {
                return false;
            }
        }

        internal bool IsInside(int index) {
            return index >= StartIndex && index < EndIndex;
        }

        public override string ToString() {
            return String.Format("{0} [{1}-{2}] [{3}->]",
                (IsFault ? "fault" : "catch(" + ExceptionType.Name + ")"),
                StartIndex, EndIndex,
                HandlerStartIndex
            );
        }
    }

    [Serializable]
    public class DebugInfo {
        // TODO: readonly

        public int StartLine, EndLine;
        public int Index;
        public string FileName;
        public bool IsClear;
        private static readonly DebugInfoComparer _debugComparer = new DebugInfoComparer();

        private class DebugInfoComparer : IComparer<DebugInfo> {
            //We allow comparison between int and DebugInfo here
            int IComparer<DebugInfo>.Compare(DebugInfo d1, DebugInfo d2) {
                if (d1.Index > d2.Index) return 1;
                else if (d1.Index == d2.Index) return 0;
                else return -1;
            }
        }
        
        public static DebugInfo GetMatchingDebugInfo(DebugInfo[] debugInfos, int index) {
            //Create a faked DebugInfo to do the search
            DebugInfo d = new DebugInfo { Index = index };

            //to find the closest debug info before the current index

            int i = Array.BinarySearch<DebugInfo>(debugInfos, d, _debugComparer);
            if (i < 0) {
                //~i is the index for the first bigger element
                //if there is no bigger element, ~i is the length of the array
                i = ~i;
                if (i == 0) {
                    return null;
                }
                //return the last one that is smaller
                i = i - 1;
            }

            return debugInfos[i];
        }

        public override string ToString() {
            if (IsClear) {
                return String.Format("{0}: clear", Index);
            } else {
                return String.Format("{0}: [{1}-{2}] '{3}'", Index, StartLine, EndLine, FileName);
            }
        }
    }

    // TODO:
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    [Serializable]
    public struct InterpretedFrameInfo {
        public readonly string MethodName;
        
        // TODO:
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public readonly DebugInfo DebugInfo;

        public InterpretedFrameInfo(string methodName, DebugInfo info) {
            MethodName = methodName;
            DebugInfo = info;
        }

        public override string ToString() {
            return MethodName + (DebugInfo != null ? ": " + DebugInfo.ToString() : null);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public sealed class LightCompiler {        
        private static readonly MethodInfo _RunMethod = typeof(Interpreter).GetMethod("Run");
        private static readonly MethodInfo _GetCurrentMethod = typeof(MethodBase).GetMethod("GetCurrentMethod");

#if DEBUG
        static LightCompiler() {
            Debug.Assert(_GetCurrentMethod != null && _RunMethod != null);
        }
#endif
        // zero: sync compilation
        private readonly int _compilationThreshold;

        private readonly InstructionList _instructions;
        private readonly LocalVariables _locals = new LocalVariables();

        private readonly List<ExceptionHandler> _handlers = new List<ExceptionHandler>();
        
        private readonly List<DebugInfo> _debugInfos = new List<DebugInfo>();
        private readonly List<UpdateStackTraceInstruction> _stackTraceUpdates = new List<UpdateStackTraceInstruction>();

        private readonly Dictionary<LabelTarget, BranchLabel> _treeLabels = new Dictionary<LabelTarget, BranchLabel>();

        private readonly Stack<ParameterExpression> _exceptionForRethrowStack = new Stack<ParameterExpression>();

        // Set to true to force compiliation of this lambda.
        // This disables the interpreter for this lambda. We still need to
        // walk it, however, to resolve variables closed over from the parent
        // lambdas (because they may be interpreted).
        private bool _forceCompile;

        private readonly LightCompiler _parent;

        internal LightCompiler(int compilationThreshold) {
            _instructions = new InstructionList();
            _compilationThreshold = compilationThreshold < 0 ? 32 : compilationThreshold;
        }

        private LightCompiler(LightCompiler parent)
            : this(parent._compilationThreshold) {
            _parent = parent;
        }

        public InstructionList Instructions {
            get { return _instructions; }
        }

        public LocalVariables Locals {
            get { return _locals; }
        }

        internal static Expression Unbox(Expression strongBoxExpression) {
            return Expression.Field(strongBoxExpression, typeof(StrongBox<object>).GetField("Value"));
        }

        internal LightDelegateCreator CompileTop(LambdaExpression node) {
            foreach (var p in node.Parameters) {
                _locals.DefineLocal(p);
            }

            Compile(node.Body);
            
            // pop the result of the last expression:
            if (node.Body.Type != typeof(void) && node.ReturnType == typeof(void)) {
                _instructions.EmitPop();
            }

            Debug.Assert(_instructions.CurrentStackDepth == (node.ReturnType != typeof(void) ? 1 : 0));

            return new LightDelegateCreator(MakeInterpreter(node), node);
        }

        private Interpreter MakeInterpreter(LambdaExpression lambda) {
            if (_forceCompile) {
                return null;
            }

            var handlers = _handlers.ToArray();
            var debugInfos = _debugInfos.ToArray();
            foreach (var stackTraceUpdate in _stackTraceUpdates) {
                stackTraceUpdate._debugInfos = debugInfos;
            }

            return new Interpreter(lambda, _locals, _treeLabels, _instructions.ToArray(), handlers, debugInfos, _compilationThreshold);
        }

        private BranchLabel MapLabel(LabelTarget target) {
            BranchLabel label;
            if (!_treeLabels.TryGetValue(target, out label)) {
                label = _instructions.MakeLabel();
                _treeLabels[target] = label;
            }
            return label;
        }

        private void CompileConstantExpression(Expression expr) {
            var node = (ConstantExpression)expr;
            _instructions.EmitLoad(node.Value, node.Type);
        }

        private void CompileDefaultExpression(Expression expr) {
            CompileDefaultExpression(expr.Type);
        }

        private void CompileDefaultExpression(Type type) {
            if (type != typeof(void)) {
                if (type.IsValueType) {
                    object value = ScriptingRuntimeHelpers.GetPrimitiveDefaultValue(type);
                    if (value != null) {
                        _instructions.EmitLoad(value);
                    } else {
                        _instructions.EmitDefaultValue(type);
                    }
                } else {
                    _instructions.EmitLoad(null);
                }
            }
        }

        private LocalVariable EnsureAvailableForClosure(ParameterExpression expr) {
            LocalVariable local;
            if (_locals.TryGetLocal(expr, out local)) {
                if (!local.InClosure && !local.IsBoxed) {
                    _locals.Box(expr);
                    _instructions.SwitchToBoxed(local.Index);
                }
                return local;
            } else if (_parent != null) {
                _parent.EnsureAvailableForClosure(expr);
                return _locals.AddClosureVariable(expr);
            } else {
                throw new InvalidOperationException("unbound variable: " + expr);
            }
        }

        public void EnsureVariable(ParameterExpression variable) {
            if (_locals.ContainsVariable(variable)) {
                EnsureAvailableForClosure(variable);
            }
        }

        public void CompileGetVariable(ParameterExpression variable) {
            LocalVariable local;
            if (!_locals.TryGetLocal(variable, out local)) {
                local = EnsureAvailableForClosure(variable);
            }

            if (local.InClosure) {
                _instructions.EmitLoadLocalFromClosure(local.Index);
            } else if (local.IsBoxed) {
                _instructions.EmitLoadLocalBoxed(local.Index);
            } else {
                _instructions.EmitLoadLocal(local.Index);
            }

            _instructions.SetDebugCookie(variable.Name);
        }

        public void CompileGetBoxedVariable(ParameterExpression variable) {
            LocalVariable local;
            if (!_locals.TryGetLocal(variable, out local)) {
                local = EnsureAvailableForClosure(variable);
            }

            if (local.InClosure) {
                _instructions.EmitLoadLocalFromClosureBoxed(local.Index);
            } else {
                Debug.Assert(local.IsBoxed);
                _instructions.EmitLoadLocal(local.Index);
            }

            _instructions.SetDebugCookie(variable.Name);
        }

        public void CompileSetVariable(ParameterExpression variable, bool isVoid) {
            LocalVariable local;
            if (!_locals.TryGetLocal(variable, out local)) {
                local = EnsureAvailableForClosure(variable);
            }

            if (local.InClosure) {
                if (isVoid) {
                    _instructions.EmitStoreLocalToClosure(local.Index);
                } else {
                    _instructions.EmitAssignLocalToClosure(local.Index);
                }
            } else if (local.IsBoxed) {
                if (isVoid) {
                    _instructions.EmitStoreLocalBoxed(local.Index);
                } else {
                    _instructions.EmitAssignLocalBoxed(local.Index);
                }
            } else {
                if (isVoid) {
                    _instructions.EmitStoreLocal(local.Index);
                } else {
                    _instructions.EmitAssignLocal(local.Index);
                }
            }

            _instructions.SetDebugCookie(variable.Name);
        }

        private void CompileParameterExpression(Expression expr) {
            var node = (ParameterExpression)expr;
            CompileGetVariable(node);
        }

        private void CompileBlockExpression(Expression expr, bool asVoid) {
            var node = (BlockExpression)expr;

            // TODO: pop these off a stack when exiting
            // TODO: basic flow analysis so we don't have to initialize all
            // variables.
            foreach (var variable in node.Variables) {
                LocalVariable local = _locals.DefineLocal(variable);
                _instructions.EmitInitializeLocal(local.Index, variable.Type);
                _instructions.SetDebugCookie(variable.Name);
            }

            for (int i = 0; i < node.Expressions.Count - 1; i++) {
                CompileAsVoid(node.Expressions[i]);
            }

            var lastExpression = node.Expressions[node.Expressions.Count - 1];
            if (asVoid) {
                CompileAsVoid(lastExpression);
            } else {
                Compile(lastExpression, asVoid);
            }
        }

        private void CompileIndexExpression(Expression expr) {
            var index = (IndexExpression)expr;

            // instance:
            if (index.Object != null) {
                Compile(index.Object);
            }

            // indexes, byref args not allowed.
            foreach (var arg in index.Arguments) {
                Compile(arg);
            }

            if (index.Indexer != null) {
                _instructions.EmitCall(index.Indexer.GetGetMethod(true));
            } else if (index.Arguments.Count != 1) {
                _instructions.EmitCall(index.Object.Type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance));
            } else {
                _instructions.EmitGetArrayItem(index.Object.Type);
            }
        }

        private void CompileIndexAssignment(BinaryExpression node, bool asVoid) {
            var index = (IndexExpression)node.Left;

            if (!asVoid) {
                throw new NotImplementedException();
            }

            // instance:
            if (index.Object != null) {
                Compile(index.Object);
            }

            // indexes, byref args not allowed.
            foreach (var arg in index.Arguments) {
                Compile(arg);
            }

            // value:
            Compile(node.Right);

            if (index.Indexer != null) {
                _instructions.EmitCall(index.Indexer.GetSetMethod(true));
            } else if (index.Arguments.Count != 1) {
                _instructions.EmitCall(index.Object.Type.GetMethod("Set", BindingFlags.Public | BindingFlags.Instance));
            } else {
                _instructions.EmitSetArrayItem(index.Object.Type);
            }
        }

        private void CompileMemberAssignment(BinaryExpression node, bool asVoid) {
            var member = (MemberExpression)node.Left;

            PropertyInfo pi = member.Member as PropertyInfo;
            if (pi != null) {
                var method = pi.GetSetMethod();
                Compile(member.Expression);
                Compile(node.Right);

                int index = 0;
                if (!asVoid) {
                    index = _locals.DefineLocal(Expression.Parameter(node.Right.Type)).Index;
                    _instructions.EmitAssignLocal(index);
                    // TODO: free the variable when it goes out of scope
                }

                _instructions.EmitCall(method);

                if (!asVoid) {
                    _instructions.EmitLoadLocal(index);
                }
                return;
            }

            FieldInfo fi = member.Member as FieldInfo;
            if (fi != null) {
                if (member.Expression != null) {
                    Compile(member.Expression);
                }
                Compile(node.Right);

                int index = 0;
                if (!asVoid) {
                    index = _locals.DefineLocal(Expression.Parameter(node.Right.Type)).Index;
                    _instructions.EmitAssignLocal(index);
                    // TODO: free the variable when it goes out of scope
                }

                _instructions.EmitStoreField(fi);

                if (!asVoid) {
                    _instructions.EmitLoadLocal(index);
                }
                return;
            }

            throw new NotImplementedException();
        }

        private void CompileVariableAssignment(BinaryExpression node, bool asVoid) {
            this.Compile(node.Right);

            var target = (ParameterExpression)node.Left;
            CompileSetVariable(target, asVoid);
        }

        private void CompileAssignBinaryExpression(Expression expr, bool asVoid) {
            var node = (BinaryExpression)expr;

            switch (node.Left.NodeType) {
                case ExpressionType.Index:
                    CompileIndexAssignment(node, asVoid); 
                    break;

                case ExpressionType.MemberAccess:
                    CompileMemberAssignment(node, asVoid); 
                    break;

                case ExpressionType.Parameter:
                case ExpressionType.Extension:
                    CompileVariableAssignment(node, asVoid); 
                    break;

                default:
                    throw new InvalidOperationException("Invalid lvalue for assignment: " + node.Left.NodeType);
            }
        }

        private void CompileBinaryExpression(Expression expr) {
            var node = (BinaryExpression)expr;

            if (node.Method != null) {
                Compile(node.Left);
                Compile(node.Right);
                _instructions.EmitCall(node.Method);
            } else {
                switch (node.NodeType) {
                    case ExpressionType.ArrayIndex:
                        Debug.Assert(node.Right.Type == typeof(int));
                        Compile(node.Left);
                        Compile(node.Right);
                        _instructions.EmitGetArrayItem(node.Left.Type);
                        return;

                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                        CompileArithmetic(node.NodeType, node.Left, node.Right);
                        return;

                    case ExpressionType.Equal:
                        CompileEqual(node.Left, node.Right);
                        return;

                    case ExpressionType.NotEqual:
                        CompileNotEqual(node.Left, node.Right);
                        return;

                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                        CompileComparison(node.NodeType, node.Left, node.Right);
                        return;

                    default:
                        throw new NotImplementedException(node.NodeType.ToString());
                }
            }
        }

        private void CompileEqual(Expression left, Expression right) {
            Debug.Assert(left.Type == right.Type || !left.Type.IsValueType && !right.Type.IsValueType);
            Compile(left);
            Compile(right);
            _instructions.EmitEqual(left.Type);
        }

        private void CompileNotEqual(Expression left, Expression right) {
            Debug.Assert(left.Type == right.Type || !left.Type.IsValueType && !right.Type.IsValueType);
            Compile(left);
            Compile(right);
            _instructions.EmitNotEqual(left.Type);
        }

        private void CompileComparison(ExpressionType nodeType, Expression left, Expression right) {
            Debug.Assert(left.Type == right.Type && TypeUtils.IsNumeric(left.Type));

            // TODO:
            // if (TypeUtils.IsNullableType(left.Type) && liftToNull) ...

            Compile(left);
            Compile(right);
            
            switch (nodeType) {
                case ExpressionType.LessThan: _instructions.EmitLessThan(left.Type); break;
                case ExpressionType.LessThanOrEqual: _instructions.EmitLessThanOrEqual(left.Type); break;
                case ExpressionType.GreaterThan: _instructions.EmitGreaterThan(left.Type); break;
                case ExpressionType.GreaterThanOrEqual: _instructions.EmitGreaterThanOrEqual(left.Type); break;
                default: throw Assert.Unreachable;
            }
        }

        private void CompileArithmetic(ExpressionType nodeType, Expression left, Expression right) {
            Debug.Assert(left.Type == right.Type && TypeUtils.IsArithmetic(left.Type));
            Compile(left);
            Compile(right);
            switch (nodeType) {
                case ExpressionType.Add: _instructions.EmitAdd(left.Type, false); break;
                case ExpressionType.AddChecked: _instructions.EmitAdd(left.Type, true); break;
                case ExpressionType.Subtract: _instructions.EmitSub(left.Type, false); break;
                case ExpressionType.SubtractChecked: _instructions.EmitSub(left.Type, true); break;
                case ExpressionType.Multiply: _instructions.EmitMul(left.Type, false); break;
                case ExpressionType.MultiplyChecked: _instructions.EmitMul(left.Type, true); break;
                case ExpressionType.Divide: _instructions.EmitDiv(left.Type); break;
                default: throw Assert.Unreachable;
            }
        }

        private void CompileConvertUnaryExpression(Expression expr) {
            var node = (UnaryExpression)expr;
            if (node.Method != null) {
                Compile(node.Operand);

                // We should be able to ignore Int32ToObject
                if (node.Method != Runtime.ScriptingRuntimeHelpers.Int32ToObjectMethod) {
                    _instructions.EmitCall(node.Method);
                }
            } else if (node.Type == typeof(void)) {
                CompileAsVoid(node.Operand);
            } else {
                Compile(node.Operand);
                CompileConvertToType(node.Operand.Type, node.Type, node.NodeType == ExpressionType.ConvertChecked);
            }
        }

        private void CompileConvertToType(Type typeFrom, Type typeTo, bool isChecked) {
            Debug.Assert(typeFrom != typeof(void) && typeTo != typeof(void));

            if (TypeUtils.AreEquivalent(typeTo, typeFrom)) {
                return;
            }

            TypeCode from = Type.GetTypeCode(typeFrom);
            TypeCode to = Type.GetTypeCode(typeTo);
            if (TypeUtils.IsNumeric(from) && TypeUtils.IsNumeric(to)) {
                if (isChecked) {
                    _instructions.EmitNumericConvertChecked(from, to);
                } else {
                    _instructions.EmitNumericConvertUnchecked(from, to);
                }
                return;
            }

            // TODO: Conversions to a super-class or implemented interfaces are no-op. 
            // A conversion to a non-implemented interface or an unrelated class, etc. should fail.
            return;
        }

        private void CompileNotExpression(UnaryExpression node) {
            if (node.Operand.Type == typeof(bool)) {
                Compile(node.Operand);
                _instructions.EmitNot();
            } else {
                throw new NotImplementedException();
            }
        }

        private void CompileUnaryExpression(Expression expr) {
            var node = (UnaryExpression)expr;
            
            if (node.Method != null) {
                Compile(node.Operand);
                _instructions.EmitCall(node.Method);
            } else {
                switch (node.NodeType) {
                    case ExpressionType.Not:
                        CompileNotExpression(node);
                        return;
                    default:
                        throw new NotImplementedException();
                }
            }
        }


        private void CompileAndAlsoBinaryExpression(Expression expr) {
            CompileLogicalBinaryExpression(expr, true);
        }

        private void CompileOrElseBinaryExpression(Expression expr) {
            CompileLogicalBinaryExpression(expr, false);
        }

        private void CompileLogicalBinaryExpression(Expression expr, bool andAlso) {
            var node = (BinaryExpression)expr;
            if (node.Method != null) {
                throw new NotImplementedException();
            }

            Debug.Assert(node.Left.Type == node.Right.Type);

            if (node.Left.Type == typeof(bool)) {
                var elseLabel = _instructions.MakeLabel();
                var endLabel = _instructions.MakeLabel();
                Compile(node.Left);
                if (andAlso) {
                    _instructions.EmitBranchFalse(elseLabel);
                } else {
                    _instructions.EmitBranchTrue(elseLabel);
                }
                Compile(node.Right);
                _instructions.EmitBranch(endLabel, false, true);
                _instructions.MarkLabel(elseLabel);
                _instructions.EmitLoad(!andAlso);
                _instructions.MarkLabel(endLabel);
                return;
            }

            Debug.Assert(node.Left.Type == typeof(bool?));
            throw new NotImplementedException();
        }

        private void CompileConditionalExpression(Expression expr, bool asVoid) {
            var node = (ConditionalExpression)expr;
            Compile(node.Test);

            if (node.IfTrue == AstUtils.Empty()) {
                var endOfFalse = _instructions.MakeLabel();
                _instructions.EmitBranchTrue(endOfFalse);
                Compile(node.IfFalse, asVoid);
                _instructions.MarkLabel(endOfFalse);
            } else {
                var endOfTrue = _instructions.MakeLabel();
                _instructions.EmitBranchFalse(endOfTrue);
                Compile(node.IfTrue, asVoid);

                if (node.IfFalse != AstUtils.Empty()) {
                    var endOfFalse = _instructions.MakeLabel();
                    _instructions.EmitBranch(endOfFalse, false, !asVoid);
                    _instructions.MarkLabel(endOfTrue);
                    Compile(node.IfFalse, asVoid);
                    _instructions.MarkLabel(endOfFalse);
                } else {
                    _instructions.MarkLabel(endOfTrue);
                }
            }
        }

        #region Loops

        private void CompileLoopExpression(Expression expr) {
            var node = (LoopExpression)expr;
            var enterLoop = new EnterLoopInstruction(node, _compilationThreshold, _instructions.Count);
            
            var continueLabel = node.ContinueLabel == null ? _instructions.MakeLabel() : MapLabel(node.ContinueLabel);
            _instructions.MarkLabel(continueLabel);

            // emit loop body:
            _instructions.Emit(enterLoop);
            CompileAsVoid(node.Body);

            // emit loop branch:
            _instructions.EmitBranch(continueLabel, expr.Type != typeof(void), false);
            if (node.BreakLabel != null) {
                _instructions.MarkLabel(MapLabel(node.BreakLabel));
            }

            enterLoop.FinishLoop(_instructions.Count);
        }

        #endregion

        private void CompileSwitchExpression(Expression expr) {
            var node = (SwitchExpression)expr;

            // Currently only supports int test values, with no method
            if (node.SwitchValue.Type != typeof(int) || node.Comparison != null) {
                throw new NotImplementedException();
            }

            // Test values must be constant
            if (!node.Cases.All(c => c.TestValues.All(t => t is ConstantExpression))) {
                throw new NotImplementedException();
            }

            BranchLabel end = _instructions.MakeLabel();
            bool hasValue = node.Type != typeof(void);

            Compile(node.SwitchValue);
            var caseDict = new Dictionary<int, int>();
            int switchIndex = _instructions.Count;
            _instructions.EmitSwitch(caseDict);

            if (node.DefaultBody != null) {
                Compile(node.DefaultBody);
            } else {
                Debug.Assert(!hasValue);
            }
            _instructions.EmitBranch(end, false, hasValue);

            for (int i = 0; i < node.Cases.Count; i++) {
                var switchCase = node.Cases[i];

                int caseOffset = _instructions.Count - switchIndex;
                foreach (ConstantExpression testValue in switchCase.TestValues) {
                    caseDict[(int)testValue.Value] = caseOffset;
                }

                Compile(switchCase.Body);

                if (i < node.Cases.Count - 1) {
                    _instructions.EmitBranch(end, false, hasValue);
                }
            }

            _instructions.MarkLabel(end);
        }

        private void CompileLabelExpression(Expression expr) {
            var node = (LabelExpression)expr;

            if (node.DefaultValue != null) {
                this.Compile(node.DefaultValue);
            }

            _instructions.MarkLabel(MapLabel(node.Target));
        }

        private void CompileGotoExpression(Expression expr) {
            var node = (GotoExpression)expr;

            if (node.Value != null) {
                Compile(node.Value);
            }

            var label = MapLabel(node.Target);
            _instructions.EmitGoto(label, node.Type != typeof(void), node.Value != null);
        }

        private void CompileThrowUnaryExpression(Expression expr, bool asVoid) {
            var node = (UnaryExpression)expr;

            if (node.Operand == null) {
                CompileGetVariable(_exceptionForRethrowStack.Peek());
            } else {
                Compile(node.Operand);
            }

            if (asVoid) {
                _instructions.EmitThrowVoid();
            } else {
                _instructions.EmitThrow();
            }
        }

        // TODO: remove (replace by true fault support)
        private bool EndsWithRethrow(Expression expr) {
            if (expr.NodeType == ExpressionType.Throw) {
                var node = (UnaryExpression)expr;
                return node.Operand == null;
            }

            BlockExpression block = expr as BlockExpression;
            if (block != null) {
                return EndsWithRethrow(block.Expressions[block.Expressions.Count - 1]);
            }
            return false;
        }


        // TODO: remove (replace by true fault support)
        private void CompileAsVoidRemoveRethrow(Expression expr) {
            int stackDepth = _instructions.CurrentStackDepth;

            if (expr.NodeType == ExpressionType.Throw) {
                Debug.Assert(((UnaryExpression)expr).Operand == null);
                return;
            }

            BlockExpression node = (BlockExpression)expr;
            foreach (var variable in node.Variables) {
                _locals.DefineLocal(variable);
            }

            for (int i = 0; i < node.Expressions.Count - 1; i++) {
                CompileAsVoid(node.Expressions[i]);
            }

            CompileAsVoidRemoveRethrow(node.Expressions[node.Expressions.Count - 1]);

            Debug.Assert(stackDepth == _instructions.CurrentStackDepth);
        }

        private void CompileTryExpression(Expression expr) {
            var node = (TryExpression)expr;

            BranchLabel end = _instructions.MakeLabel();
            BranchLabel gotoEnd = _instructions.MakeLabel();

            int tryStackDepth = _instructions.CurrentStackDepth;
            int tryStart = _instructions.Count;

            BranchLabel startOfFinally = null;
            if (node.Finally != null) {
                startOfFinally = _instructions.MakeLabel();
                _instructions.EmitEnterTryFinally(startOfFinally);
            }

            Compile(node.Body);

            bool hasValue = node.Body.Type != typeof(void);
            int tryEnd = _instructions.Count;

            // handlers jump here:
            _instructions.MarkLabel(gotoEnd);
            _instructions.EmitGoto(end, hasValue, hasValue);
            
            // keep the result on the stack:     
            if (node.Handlers.Count > 0) {
                int handlerContinuationDepth = _instructions.CurrentContinuationsDepth;

                // TODO: emulates faults (replace by true fault support)
                if (node.Finally == null && node.Handlers.Count == 1) {
                    var handler = node.Handlers[0];
                    if (handler.Filter == null && handler.Test == typeof(Exception) && handler.Variable == null) {
                        if (EndsWithRethrow(handler.Body)) {
                            if (hasValue) {
                                _instructions.EmitEnterExceptionHandlerNonVoid();
                            } else {
                                _instructions.EmitEnterExceptionHandlerVoid();
                            }

                            // at this point the stack balance is prepared for the hidden exception variable:
                            int handlerLabel = _instructions.MarkRuntimeLabel();
                            int handlerStart = _instructions.Count;

                            CompileAsVoidRemoveRethrow(handler.Body);
                            _instructions.EmitLeaveFault(hasValue);
                            _instructions.MarkLabel(end);

                            _handlers.Add(new ExceptionHandler(tryStart, tryEnd, handlerLabel, handlerStart, null));
                            return;
                        }
                    }
                }

                foreach (var handler in node.Handlers) {
                    if (handler.Filter != null) throw new NotImplementedException();
                    var parameter = handler.Variable ?? Expression.Parameter(handler.Test);
                    _locals.DefineLocal(parameter);
                    _exceptionForRethrowStack.Push(parameter);

                    // add a stack balancing nop instruction (exception handling pushes the current exception):
                    if (hasValue) {
                        _instructions.EmitEnterExceptionHandlerNonVoid();
                    } else {
                        _instructions.EmitEnterExceptionHandlerVoid();
                    }

                    // at this point the stack balance is prepared for the hidden exception variable:
                    int handlerLabel = _instructions.MarkRuntimeLabel();
                    int handlerStart = _instructions.Count;

                    CompileSetVariable(parameter, true);
                    Compile(handler.Body);

                    _exceptionForRethrowStack.Pop();

                    // keep the value of the body on the stack:
                    Debug.Assert(hasValue == (handler.Body.Type != typeof(void)));
                    _instructions.EmitLeaveExceptionHandler(hasValue, gotoEnd);

                    _handlers.Add(new ExceptionHandler(tryStart, tryEnd, handlerLabel, handlerStart, handler.Test));
                }

                if (node.Fault != null) {
                    throw new NotImplementedException();
                }
            }
            
            if (node.Finally != null) {
                _instructions.MarkLabel(startOfFinally);
                _instructions.EmitEnterFinally();
                CompileAsVoid(node.Finally);
                _instructions.EmitLeaveFinally();
            }

            _instructions.MarkLabel(end);
        }

        private void CompileDynamicExpression(Expression expr) {
            var node = (DynamicExpression)expr;

            foreach (var arg in node.Arguments) {
                Compile(arg);
            }

            _instructions.EmitDynamic(node.DelegateType, node.Binder);
        }

        private void CompileMethodCallExpression(Expression expr) {
            var node = (MethodCallExpression)expr;
            
            if (node.Method == _GetCurrentMethod && node.Object == null && node.Arguments.Count == 0) {
                // If we call GetCurrentMethod, it will expose details of the
                // interpreter's CallInstruction. Instead, we use
                // Interpreter.Run, which logically represents the running
                // method, and will appear in the stack trace of an exception.
                _instructions.EmitLoad(_RunMethod);
                return;
            }

            var parameters = node.Method.GetParameters();

            // TODO:
            // Support pass by reference.
            // Note that LoopCompiler needs to be updated too.

            // force compilation for now:
            if (!CollectionUtils.TrueForAll(parameters, (p) => !p.ParameterType.IsByRef)) {
                _forceCompile = true;
            }

            if (!node.Method.IsStatic) {
                Compile(node.Object);
            }

            foreach (var arg in node.Arguments) {
                Compile(arg);
            }

            _instructions.EmitCall(node.Method, parameters);
        }

        private void CompileNewExpression(Expression expr) {
            var node = (NewExpression)expr;

            if (node.Constructor != null) {
                foreach (var arg in node.Arguments) {
                    this.Compile(arg);
                }
                _instructions.EmitNew(node.Constructor);
            } else {
                Debug.Assert(expr.Type.IsValueType);
                _instructions.EmitDefaultValue(node.Type);
            }
        }

        private void CompileMemberExpression(Expression expr) {
            var node = (MemberExpression)expr;

            var member = node.Member;
            FieldInfo fi = member as FieldInfo;
            if (fi != null) {
                if (fi.IsLiteral) {
                    _instructions.EmitLoad(fi.GetRawConstantValue(), fi.FieldType);
                } else if (fi.IsStatic) {
                    if (fi.IsInitOnly) {
                        _instructions.EmitLoad(fi.GetValue(null), fi.FieldType);
                    } else {
                        _instructions.EmitLoadField(fi);
                    }
                } else {
                    Compile(node.Expression);
                    _instructions.EmitLoadField(fi);
                }
                return;
            }

            PropertyInfo pi = member as PropertyInfo;
            if (pi != null) {
                var method = pi.GetGetMethod();
                if (node.Expression != null) {
                    Compile(node.Expression);
                }
                _instructions.EmitCall(method);
                return;
            }


            throw new System.NotImplementedException();
        }

        private void CompileNewArrayExpression(Expression expr) {
            var node = (NewArrayExpression)expr;

            foreach (var arg in node.Expressions) {
                Compile(arg);
            }

            Type elementType = node.Type.GetElementType();
            int rank = node.Expressions.Count;

            if (node.NodeType == ExpressionType.NewArrayInit) {
                _instructions.EmitNewArrayInit(elementType, rank);
            } else if (node.NodeType == ExpressionType.NewArrayBounds) {
                if (rank == 1) {
                    _instructions.EmitNewArray(elementType);
                } else {
                    _instructions.EmitNewArrayBounds(elementType, rank);
                }
            } else {
                throw new System.NotImplementedException();
            }
        }

        class ParameterVisitor : ExpressionVisitor {
            private readonly LightCompiler _compiler;

            /// <summary>
            /// A stack of variables that are defined in nested scopes. We search
            /// this first when resolving a variable in case a nested scope shadows
            /// one of our variable instances.
            /// </summary>
            private readonly Stack<HashSet<ParameterExpression>> _shadowedVars = new Stack<HashSet<ParameterExpression>>();

            public ParameterVisitor(LightCompiler compiler) {
                _compiler = compiler;
            }

            protected override Expression VisitLambda<T>(Expression<T> node) {
                _shadowedVars.Push(new HashSet<ParameterExpression>(node.Parameters));
                try {
                    Visit(node.Body);
                    return node;
                } finally {
                    _shadowedVars.Pop();
                }
            }

            protected override Expression VisitBlock(BlockExpression node) {
                if (node.Variables.Count > 0) {
                    _shadowedVars.Push(new HashSet<ParameterExpression>(node.Variables));
                }
                try {
                    Visit(node.Expressions);
                    return node;
                } finally {
                    if (node.Variables.Count > 0) {
                        _shadowedVars.Pop();
                    }
                }
           }

            protected override CatchBlock VisitCatchBlock(CatchBlock node) {
                if (node.Variable != null) {
                    _shadowedVars.Push(new HashSet<ParameterExpression>(new[] { node.Variable }));
                }
                try {
                    Visit(node.Filter);
                    Visit(node.Body);
                    return node;
                } finally {
                    if (node.Variable != null) {
                        _shadowedVars.Pop();
                    }
                }
            }

            protected override Expression VisitParameter(ParameterExpression node) {
                // Skip variables that are shadowed by a nested scope/lambda
                foreach (HashSet<ParameterExpression> hidden in _shadowedVars) {
                    if (hidden.Contains(node)) {
                        return node;
                    }
                }

                // If we didn't find it, it must exist at a higher level scope
                _compiler.EnsureVariable(node);
                return node;
            }

        }

        private void CompileExtensionExpression(Expression expr) {
            var instructionProvider = expr as IInstructionProvider;
            if (instructionProvider != null) {
                instructionProvider.AddInstructions(this);
                return;
            }

            var skip = expr as Ast.SkipInterpretExpression;
            if (skip != null) {
                new ParameterVisitor(this).Visit(skip);
                return;
            }

            var node = expr as Microsoft.Scripting.Ast.SymbolConstantExpression;
            if (node != null) {
                _instructions.EmitLoad(node.Value);
                return;
            }

            var updateStack = expr as LastFaultingLineExpression;
            if (updateStack != null) {
                var updateStackInstr = new UpdateStackTraceInstruction();
                _instructions.Emit(updateStackInstr);
                _stackTraceUpdates.Add(updateStackInstr);
                return;
            }

            if (expr.CanReduce) {
                Compile(expr.Reduce());
            } else {
                throw new System.NotImplementedException();
            }
        }


        private void CompileDebugInfoExpression(Expression expr) {
            var node = (DebugInfoExpression)expr;
            int start = _instructions.Count;
            var info = new DebugInfo()
            {
                Index = start,
                FileName = node.Document.FileName,
                StartLine = node.StartLine,
                EndLine = node.EndLine,
                IsClear = node.IsClear
            };
            _debugInfos.Add(info);
        }

        private void CompileRuntimeVariablesExpression(Expression expr) {
            // Generates IRuntimeVariables for all requested variables
            var node = (RuntimeVariablesExpression)expr;
            foreach (var variable in node.Variables) {
                EnsureAvailableForClosure(variable);
                CompileGetBoxedVariable(variable);
            }

            _instructions.EmitNewRuntimeVariables(node.Variables.Count);
        }


        private void CompileLambdaExpression(Expression expr) {
            var node = (LambdaExpression)expr;
            var creator = new LightCompiler(this).CompileTop(node);

            foreach (ParameterExpression variable in creator.Interpreter.Locals.GetClosureVariables()) {
                CompileGetBoxedVariable(variable);
            }
            _instructions.EmitCreateDelegate(creator);
        }

        private void CompileCoalesceBinaryExpression(Expression expr) {
            var node = (BinaryExpression)expr;

            if (TypeUtils.IsNullableType(node.Left.Type)) {
                throw new NotImplementedException();
            } else if (node.Conversion != null) {
                throw new NotImplementedException();
            } else {
                var leftNotNull = _instructions.MakeLabel();
                Compile(node.Left);
                _instructions.EmitCoalescingBranch(leftNotNull);
                _instructions.EmitPop();
                Compile(node.Right);
                _instructions.MarkLabel(leftNotNull);
            }
        }

        private void CompileInvocationExpression(Expression expr) {
            var node = (InvocationExpression)expr;

            // TODO: LambdaOperand optimization (see compiler)
            if (typeof(LambdaExpression).IsAssignableFrom(node.Expression.Type)) {
                throw new System.NotImplementedException();
            }

            // TODO: do not create a new Call Expression
            if (PlatformAdaptationLayer.IsCompactFramework) {
                // Workaround for a bug in Compact Framework
                Compile(
                    AstUtils.Convert(
                        Expression.Call(
                            node.Expression,
                            node.Expression.Type.GetMethod("DynamicInvoke"),
                            Expression.NewArrayInit(typeof(object), node.Arguments.Map((e) => AstUtils.Convert(e, typeof(object))))
                        ),
                        node.Type
                    )
                );
            } else {
                CompileMethodCallExpression(Expression.Call(node.Expression, node.Expression.Type.GetMethod("Invoke"), node.Arguments));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "expr")]
        private void CompileListInitExpression(Expression expr) {
            throw new System.NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "expr")]
        private void CompileMemberInitExpression(Expression expr) {
            throw new System.NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "expr")]
        private void CompileQuoteUnaryExpression(Expression expr) {
            throw new System.NotImplementedException();
        }

        private void CompileUnboxUnaryExpression(Expression expr) {
            var node = (UnaryExpression)expr;
            // unboxing is a nop:
            Compile(node.Operand);
        }

        private void CompileTypeEqualExpression(Expression expr) {
            Debug.Assert(expr.NodeType == ExpressionType.TypeEqual);
            var node = (TypeBinaryExpression)expr;

            Compile(node.Expression);
            _instructions.EmitLoad(node.TypeOperand);
            _instructions.EmitTypeEquals();
        }

        private void CompileTypeIsExpression(Expression expr) {
            Debug.Assert(expr.NodeType == ExpressionType.TypeIs);
            var node = (TypeBinaryExpression)expr;

            Compile(node.Expression);

            // use TypeEqual for sealed types:
            if (node.TypeOperand.IsSealed) {
                _instructions.EmitLoad(node.TypeOperand);
                _instructions.EmitTypeEquals();
            } else {
                _instructions.EmitTypeIs(node.TypeOperand);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "expr")]
        private void CompileReducibleExpression(Expression expr) {
            throw new System.NotImplementedException();
        }

        internal void Compile(Expression expr, bool asVoid) {
            if (asVoid) {
                CompileAsVoid(expr);
            } else {
                Compile(expr);
            }
        }

        internal void CompileAsVoid(Expression expr) {
            int startingStackDepth = _instructions.CurrentStackDepth;
            switch (expr.NodeType) {
                case ExpressionType.Assign:
                    CompileAssignBinaryExpression(expr, true);
                    break;

                case ExpressionType.Block:
                    CompileBlockExpression(expr, true);
                    break;

                case ExpressionType.Throw:
                    CompileThrowUnaryExpression(expr, true);
                    break;

                case ExpressionType.Constant:
                case ExpressionType.Default:
                case ExpressionType.Parameter:
                    // no-op
                    break;

                default:
                    Compile(expr);
                    if (expr.Type != typeof(void)) {
                        _instructions.EmitPop();
                    }
                    break;
            }
            Debug.Assert(_instructions.CurrentStackDepth == startingStackDepth);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void Compile(Expression expr) {
            int startingStackDepth = _instructions.CurrentStackDepth;
            switch (expr.NodeType) {
                case ExpressionType.Add: CompileBinaryExpression(expr); break;
                case ExpressionType.AddChecked: CompileBinaryExpression(expr); break;
                case ExpressionType.And: CompileBinaryExpression(expr); break;
                case ExpressionType.AndAlso: CompileAndAlsoBinaryExpression(expr); break;
                case ExpressionType.ArrayLength: CompileUnaryExpression(expr); break;
                case ExpressionType.ArrayIndex: CompileBinaryExpression(expr); break;
                case ExpressionType.Call: CompileMethodCallExpression(expr); break;
                case ExpressionType.Coalesce: CompileCoalesceBinaryExpression(expr); break;
                case ExpressionType.Conditional: CompileConditionalExpression(expr, expr.Type == typeof(void)); break;
                case ExpressionType.Constant: CompileConstantExpression(expr); break;
                case ExpressionType.Convert: CompileConvertUnaryExpression(expr); break;
                case ExpressionType.ConvertChecked: CompileConvertUnaryExpression(expr); break;
                case ExpressionType.Divide: CompileBinaryExpression(expr); break;
                case ExpressionType.Equal: CompileBinaryExpression(expr); break;
                case ExpressionType.ExclusiveOr: CompileBinaryExpression(expr); break;
                case ExpressionType.GreaterThan: CompileBinaryExpression(expr); break;
                case ExpressionType.GreaterThanOrEqual: CompileBinaryExpression(expr); break;
                case ExpressionType.Invoke: CompileInvocationExpression(expr); break;
                case ExpressionType.Lambda: CompileLambdaExpression(expr); break;
                case ExpressionType.LeftShift: CompileBinaryExpression(expr); break;
                case ExpressionType.LessThan: CompileBinaryExpression(expr); break;
                case ExpressionType.LessThanOrEqual: CompileBinaryExpression(expr); break;
                case ExpressionType.ListInit: CompileListInitExpression(expr); break;
                case ExpressionType.MemberAccess: CompileMemberExpression(expr); break;
                case ExpressionType.MemberInit: CompileMemberInitExpression(expr); break;
                case ExpressionType.Modulo: CompileBinaryExpression(expr); break;
                case ExpressionType.Multiply: CompileBinaryExpression(expr); break;
                case ExpressionType.MultiplyChecked: CompileBinaryExpression(expr); break;
                case ExpressionType.Negate: CompileUnaryExpression(expr); break;
                case ExpressionType.UnaryPlus: CompileUnaryExpression(expr); break;
                case ExpressionType.NegateChecked: CompileUnaryExpression(expr); break;
                case ExpressionType.New: CompileNewExpression(expr); break;
                case ExpressionType.NewArrayInit: CompileNewArrayExpression(expr); break;
                case ExpressionType.NewArrayBounds: CompileNewArrayExpression(expr); break;
                case ExpressionType.Not: CompileUnaryExpression(expr); break;
                case ExpressionType.NotEqual: CompileBinaryExpression(expr); break;
                case ExpressionType.Or: CompileBinaryExpression(expr); break;
                case ExpressionType.OrElse: CompileOrElseBinaryExpression(expr); break;
                case ExpressionType.Parameter: CompileParameterExpression(expr); break;
                case ExpressionType.Power: CompileBinaryExpression(expr); break;
                case ExpressionType.Quote: CompileQuoteUnaryExpression(expr); break;
                case ExpressionType.RightShift: CompileBinaryExpression(expr); break;
                case ExpressionType.Subtract: CompileBinaryExpression(expr); break;
                case ExpressionType.SubtractChecked: CompileBinaryExpression(expr); break;
                case ExpressionType.TypeAs: CompileUnaryExpression(expr); break;
                case ExpressionType.TypeIs: CompileTypeIsExpression(expr); break;
                case ExpressionType.Assign: CompileAssignBinaryExpression(expr, expr.Type == typeof(void)); break;
                case ExpressionType.Block: CompileBlockExpression(expr, expr.Type == typeof(void)); break;
                case ExpressionType.DebugInfo: CompileDebugInfoExpression(expr); break;
                case ExpressionType.Decrement: CompileUnaryExpression(expr); break;
                case ExpressionType.Dynamic: CompileDynamicExpression(expr); break;
                case ExpressionType.Default: CompileDefaultExpression(expr); break;
                case ExpressionType.Extension: CompileExtensionExpression(expr); break;
                case ExpressionType.Goto: CompileGotoExpression(expr); break;
                case ExpressionType.Increment: CompileUnaryExpression(expr); break;
                case ExpressionType.Index: CompileIndexExpression(expr); break;
                case ExpressionType.Label: CompileLabelExpression(expr); break;
                case ExpressionType.RuntimeVariables: CompileRuntimeVariablesExpression(expr); break;
                case ExpressionType.Loop: CompileLoopExpression(expr); break;
                case ExpressionType.Switch: CompileSwitchExpression(expr); break;
                case ExpressionType.Throw: CompileThrowUnaryExpression(expr, expr.Type == typeof(void)); break;
                case ExpressionType.Try: CompileTryExpression(expr); break;
                case ExpressionType.Unbox: CompileUnboxUnaryExpression(expr); break;
                case ExpressionType.TypeEqual: CompileTypeEqualExpression(expr); break;
                case ExpressionType.OnesComplement: CompileUnaryExpression(expr); break;
                case ExpressionType.IsTrue: CompileUnaryExpression(expr); break;
                case ExpressionType.IsFalse: CompileUnaryExpression(expr); break;
                case ExpressionType.AddAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.PostDecrementAssign:
                    CompileReducibleExpression(expr); break;
                default: throw Assert.Unreachable;
            };
            Debug.Assert(_instructions.CurrentStackDepth == startingStackDepth + (expr.Type == typeof(void) ? 0 : 1));
        }
    }
}
