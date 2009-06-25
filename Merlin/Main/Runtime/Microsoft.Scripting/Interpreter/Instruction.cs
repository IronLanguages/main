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
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;

namespace Microsoft.Scripting.Interpreter {

    public interface IInstructionProvider {
        void AddInstructions(LightCompiler compiler);
    }

    public abstract class Instruction {
        public virtual int ConsumedStack { get { return 0; } }
        public virtual int ProducedStack { get { return 0; } }

        public abstract int Run(InterpretedFrame frame);

        public virtual string InstructionName {
            get { return GetType().Name.Replace("Instruction", ""); }
        }

        public override string ToString() {
            return InstructionName + "()";
        }
    }

    #region Basic Stack Operations

    public class PushInstruction : Instruction {
        private object _value;
        public PushInstruction(object value) {
            this._value = value;
        }

        public override int ProducedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex++] = _value;
            return +1;
        }

        public override string ToString() {
            return "Push(" + (_value ?? "null") + ")";
        }
    }

    public class PopInstruction : Instruction {
        public static PopInstruction Instance = new PopInstruction();

        private PopInstruction() { }

        public override int ConsumedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            frame.Pop();
            return +1;
        }

        public override string ToString() {
            return "Pop()";
        }
    }

    public class DupInstruction : Instruction {
        public static DupInstruction Instance = new DupInstruction();

        private DupInstruction() { }

        public override int ConsumedStack { get { return 0; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex++] = frame.Peek();
            return +1;
        }

        public override string ToString() {
            return "Dup()";
        }
    }

    #endregion

    #region Local Variables

    public interface IBoxableInstruction {
        Instruction BoxIfIndexMatches(int index);
    }

    public abstract class LocalAccessInstruction : Instruction {
        internal readonly int _index;

        public LocalAccessInstruction(int index) {
            _index = index;
        }

#if DEBUG
        public override string ToString() {
            return InstructionName + "(" + _name + ": " + _index + ")";
        }

        private string _name;
#endif

        [Conditional("DEBUG")]
        public void SetName(string name) {
#if DEBUG
            _name = name;
#endif
        }

        [Conditional("DEBUG")]
        public void SetName(LocalAccessInstruction other) {
#if DEBUG
            _name = other._name;
#endif
        }
    }

    public sealed class GetLocalInstruction : LocalAccessInstruction, IBoxableInstruction {
        public GetLocalInstruction(int index) 
            : base(index) {
        }

        public override int ProducedStack { get { return 1; } }
        
        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex++] = frame.Data[_index];
            //frame.Push(frame.Data[_index]);
            return +1;
        }

        public Instruction BoxIfIndexMatches(int index) {
            if (index == _index) {
                var result = new GetBoxedLocalInstruction(index);
                result.SetName(this);
                return result;
            } else {
                return null;
            }
        }
    }

    public sealed class GetBoxedLocalInstruction : LocalAccessInstruction {
        public GetBoxedLocalInstruction(int index)
            : base(index) {
        }

        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            var box = (StrongBox<object>)frame.Data[_index];
            frame.Data[frame.StackIndex++] = box.Value;
            return +1;
        }
    }

    public sealed class GetClosureInstruction : LocalAccessInstruction {
        public GetClosureInstruction(int index) 
            : base(index) {
        }

        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            var box = frame.Closure[_index];
            frame.Data[frame.StackIndex++] = box.Value;
            return +1;
        }
    }

    public sealed class GetBoxedClosureInstruction : LocalAccessInstruction {
        public GetBoxedClosureInstruction(int index) 
            : base(index) {
        }

        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            var box = frame.Closure[_index];
            frame.Data[frame.StackIndex++] = box;
            return +1;
        }
    }

    public sealed class SetLocalInstruction : LocalAccessInstruction, IBoxableInstruction {
        public SetLocalInstruction(int index)
            : base(index) {
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[_index] = frame.Peek();
            return +1;
        }

        public Instruction BoxIfIndexMatches(int index) {
            if (index == _index) {
                var result = new SetBoxedLocalInstruction(index);
                result.SetName(this);
                return result;
            } else {
                return null;
            }
        }
    }

    public sealed class SetBoxedLocalInstruction : LocalAccessInstruction {
        public SetBoxedLocalInstruction(int index) 
            : base(index) {
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            var box = (StrongBox<object>)frame.Data[_index];
            box.Value = frame.Peek();
            return +1;
        }
    }

    public sealed class SetBoxedLocalVoidInstruction : LocalAccessInstruction {
        public SetBoxedLocalVoidInstruction(int index)
            : base(index) {
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 0; } }

        public override int Run(InterpretedFrame frame) {
            var box = (StrongBox<object>)frame.Data[_index];
            box.Value = frame.Data[--frame.StackIndex];
            return +1;
        }
    }

    public sealed class SetClosureInstruction : LocalAccessInstruction {
        public SetClosureInstruction(int index)
            : base(index) {
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            var box = frame.Closure[_index];
            box.Value = frame.Peek();
            return +1;
        }
    }

    public sealed class SetLocalVoidInstruction : LocalAccessInstruction, IBoxableInstruction {
        public SetLocalVoidInstruction(int index) 
            : base(index) {
        }

        public override int ConsumedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            frame.Data[_index] = frame.Data[--frame.StackIndex];
            //frame.Data[_index] = frame.Pop();
            return +1;
        }

        public Instruction BoxIfIndexMatches(int index) {
            if (index == _index) {
                var result = new SetBoxedLocalVoidInstruction(index);
                result.SetName(this);
                return result;
            } else {
                return null;
            }
        }
    }

    public abstract class InitializeLocalInstruction : LocalAccessInstruction {
        public InitializeLocalInstruction(int index)
            : base(index) {
        }

        private sealed class Reference : InitializeLocalInstruction, IBoxableInstruction {
            public Reference(int index) 
                : base(index) {
            }

            public override int Run(InterpretedFrame frame) {
                // nop
                return 1;
            }

            public Instruction BoxIfIndexMatches(int index) {
                if (index == _index) {
                    var result = new ImmutableBox(index, null);
                    result.SetName(this);
                    return result;
                } else {
                    return null;
                }
            }

            public override string InstructionName {
                get { return "InitRef"; }
            }
        }

        private sealed class ImmutableValue : InitializeLocalInstruction, IBoxableInstruction {
            private readonly object _defaultValue;

            public ImmutableValue(int index, object defaultValue) 
                : base(index) {
                _defaultValue = defaultValue;
            }

            public override int Run(InterpretedFrame frame) {
                frame.Data[_index] = _defaultValue;
                return 1;
            }

            public Instruction BoxIfIndexMatches(int index) {
                if (index == _index) {
                    var result = new ImmutableBox(index, _defaultValue);
                    result.SetName(this);
                    return result;
                } else {
                    return null;
                }
            }

            public override string InstructionName {
                get { return "InitImmutableValue"; }
            }
        }

        private sealed class ImmutableBox : InitializeLocalInstruction {
            // immutable value:
            private readonly object _defaultValue;

            public ImmutableBox(int index, object defaultValue)
                : base(index) {
                _defaultValue = defaultValue;
            }

            public override int Run(InterpretedFrame frame) {
                frame.Data[_index] = new StrongBox<object>() { Value = _defaultValue };
                return 1;
            }

            public override string InstructionName {
                get { return "InitImmutableBox"; }
            }
        }

        private sealed class MutableValue : InitializeLocalInstruction, IBoxableInstruction {
            private readonly Type _type;

            public MutableValue(int index, Type type)
                : base(index) {
                _type = type;
            }

            public override int Run(InterpretedFrame frame) {
                frame.Data[_index] = Activator.CreateInstance(_type);
                return 1;
            }

            public Instruction BoxIfIndexMatches(int index) {
                if (index == _index) {
                    var result = new MutableBox(index, _type);
                    result.SetName(this);
                    return result;
                } else {
                    return null;
                }
            }

            public override string InstructionName {
                get { return "InitMutableValue"; }
            }
        }

        private sealed class MutableBox : InitializeLocalInstruction {
            private readonly Type _type;

            public MutableBox(int index, Type type)
                : base(index) {
                _type = type;
            }

            public override int Run(InterpretedFrame frame) {
                frame.Data[_index] = new StrongBox<object>() { Value = Activator.CreateInstance(_type) };
                return 1;
            }

            public override string InstructionName {
                get { return "InitMutableBox"; }
            }
        }

        public static Instruction Create(int index, ParameterExpression local) {
            var result = CreateInstance(index, local);
            result.SetName(local.Name);
            return result;
        }

        private static LocalAccessInstruction CreateInstance(int index, ParameterExpression local) {
            switch (Type.GetTypeCode(local.Type)) {
                case TypeCode.Boolean: return new ImmutableValue(index, ScriptingRuntimeHelpers.False);
                case TypeCode.SByte: return new ImmutableValue(index, default(SByte));
                case TypeCode.Byte: return new ImmutableValue(index, default(Byte));
                case TypeCode.Char: return new ImmutableValue(index, default(Char));
                case TypeCode.Int16: return new ImmutableValue(index, default(Int16));
                case TypeCode.Int32: return new ImmutableValue(index, ScriptingRuntimeHelpers.Int32ToObject(0));
                case TypeCode.Int64: return new ImmutableValue(index, default(Int64));
                case TypeCode.UInt16: return new ImmutableValue(index, default(UInt16));
                case TypeCode.UInt32: return new ImmutableValue(index, default(UInt32));
                case TypeCode.UInt64: return new ImmutableValue(index, default(UInt64));
                case TypeCode.Single: return new ImmutableValue(index, default(Single));
                case TypeCode.Double: return new ImmutableValue(index, default(Double));
                case TypeCode.DBNull: return new ImmutableValue(index, default(DBNull));
                case TypeCode.DateTime: return new ImmutableValue(index, default(DateTime));
                case TypeCode.Decimal: return new ImmutableValue(index, default(Decimal));
                
                case TypeCode.String:
                case TypeCode.Object: 
                    if (local.Type.IsValueType) {
                        return new MutableValue(index, local.Type);
                    } else {
                        return new Reference(index);
                    }

                default:
                    throw Assert.Unreachable;
            }

        }
    }

    #endregion

    #region Branches

    public abstract class OffsetInstruction : Instruction {
        internal const int Unknown = Int32.MinValue;

        // the offset to jump to (relative to this instruction):
        [CLSCompliant(false)]
        protected int _offset = Unknown;

        public virtual void Fixup(int offset, int targetStackDepth) {
            Debug.Assert(_offset == Unknown && offset != Unknown);
            _offset = offset;
        }

        public override string ToString() {
            return InstructionName + (_offset == Unknown ? "(?)" : "(" + _offset + ")");
        }
    }

    public sealed class BranchFalseInstruction : OffsetInstruction {
        public BranchFalseInstruction() {
        }

        public override int ConsumedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            Debug.Assert(_offset != Unknown);

            if (!(bool)frame.Pop()) {
                return _offset;
            }

            return +1;
        }
    }

    public sealed class BranchTrueInstruction : OffsetInstruction {
        public BranchTrueInstruction() {
        }

        public override int ConsumedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            Debug.Assert(_offset != Unknown);

            if ((bool)frame.Pop()) {
                return _offset;
            }

            return +1;
        }
    }

    public class BranchInstruction : OffsetInstruction {
        internal readonly bool _hasResult;
        internal readonly bool _hasValue;

        public BranchInstruction() 
            : this(false, false) {
        }

        public BranchInstruction(bool hasResult, bool hasValue) {
            _hasResult = hasResult;
            _hasValue = hasValue;
        }

        public override int ConsumedStack {
            get { return _hasValue ? 1 : 0; }
        }

        public override int ProducedStack {
            get { return _hasResult ? 1 : 0; }
        }

        public override int Run(InterpretedFrame frame) {
            Debug.Assert(_offset != Unknown);

            return _offset;
        }
    }

    /// <summary>
    /// This instruction implements a goto expression that can jump out of any expression. 
    /// It pops values (arguments) from the evaluation stack that the expression tree nodes in between 
    /// the goto expression and the target label node pushed and not consumed yet. 
    /// A goto expression can jump into a node that evaluates arguments only if it carries 
    /// a value and jumps right after the first argument (the carried value will be used as the first argument). 
    /// Goto can jump into an arbitrary child of a BlockExpression since the block doesn’t accumulate values 
    /// on evaluation stack as its child expressions are being evaluated.
    /// 
    /// Goto needs to execute any finally blocks on the way to the target label.
    /// <example>
    /// { 
    ///     f(1, 2, try { g(3, 4, try { goto L } finally { ... }, 6) } finally { ... }, 7, 8)
    ///     L: ... 
    /// }
    /// </example>
    /// The goto expression here jumps to label L while having 4 items on evaluation stack (1, 2, 3 and 4). 
    /// The jump needs to execute both finally blocks, the first one on stack level 4 the 
    /// second one on stack level 2. So, it needs to jump the first finally block, pop 2 items from the stack, 
    /// run second finally block and pop another 2 items from the stack and set instruction pointer to label L.
    /// 
    /// Goto also needs to rethrow ThreadAbortException iff it jumps out of a catch handler and 
    /// the current thread is in "abort requested" state.
    /// </summary>
    public sealed class GotoInstruction : BranchInstruction {
        private struct FinallyBlock {
            public int Start, End, StackDepth;

            public FinallyBlock(int start, int end, int stackDepth) {
                Start = start;
                End = end;
                StackDepth = stackDepth;
            }
        }

        // the target label stack depth
        private int _targetStackDepth;

        // index of this instruction in instruction array (used only during compilation):
        private int _instructionIndex; 

        // A list of finally blocks that need to be executed as we jump to the target label.
        private List<FinallyBlock> _finallyBlocks;

        public GotoInstruction(int instructionIndex, bool hasResult, bool hasValue) 
            : base(hasResult, hasValue) {
            Debug.Assert(instructionIndex >= 0);
            _instructionIndex = instructionIndex;
        }

        public override void Fixup(int offset, int targetStackDepth) {
            base.Fixup(offset, targetStackDepth);
            _targetStackDepth = targetStackDepth;
        }

        internal bool AddFinally(int tryStart, int finallyStackDepth, int finallyStart, int finallyEnd) {
            if (!JumpsOutOfRange(tryStart, finallyStart)) {
                return false;
            }

            if (_finallyBlocks == null) {
                _finallyBlocks = new List<FinallyBlock>();
            }

            _finallyBlocks.Add(new FinallyBlock(finallyStart, finallyEnd, finallyStackDepth));
            return true;
        }

        private bool JumpsOutOfRange(int start, int end) {
            // we haven't visited target label for a forward jump => it is out of range
            if (_offset != Unknown) {
                int targetIndex = _instructionIndex + _offset;
                if (targetIndex >= start && targetIndex < end) {
                    // target is within try body or catch handlers:
                    return false;
                }
            }
            return true;
        }
        
        public override int Run(InterpretedFrame frame) {
            Debug.Assert(_offset != Unknown);

            Interpreter.AbortThreadIfRequested(frame, _offset);

            object value = _hasValue ? frame.Pop() : null;
                
            // run finally blocks:
            if (_finallyBlocks != null) {
                int oldIndex = frame.InstructionIndex;
                for (int i = 0; i < _finallyBlocks.Count; i++) {
                    var finallyBlock = _finallyBlocks[i];

                    frame.SetStackDepth(finallyBlock.StackDepth);
                    frame.InstructionIndex = finallyBlock.Start;

                    // If an exception is thrown and caught in finally the we go on.
                    // If an exception is thrown but not handled within finally block it is propagated.
                    frame.Interpreter.RunBlock(frame, finallyBlock.End);
                }
                frame.InstructionIndex = oldIndex;
            }

            frame.SetStackDepth(_targetStackDepth);
            if (_hasValue) {
                frame.Data[frame.StackIndex - 1] = value;
            }

            // keep the return value on the stack 
            return _offset;
        }
    }

    // no-op: we need this just to balance the stack depth.
    public sealed class EnterExceptionHandlerInstruction : Instruction {
        public static readonly EnterExceptionHandlerInstruction Void = new EnterExceptionHandlerInstruction(false);
        public static readonly EnterExceptionHandlerInstruction NonVoid = new EnterExceptionHandlerInstruction(true);

        // True if try-expression is non-void.
        private readonly bool _hasValue;

        private EnterExceptionHandlerInstruction(bool hasValue) {
            _hasValue = hasValue;
        }

        // Try body and catch handlers leave a value of try-expression on the stack.
        // Each handler "consumes" the value possibly pushed in try body.
        public override int ConsumedStack { get { return _hasValue ? 1 : 0; } }
        
        // A variable storing the current exception is pushed to the stack by exception handling.
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            // nop (the exception value is pushed by the interpreter in HandleCatch)
            return 1;
        }
    }

    public sealed class LeaveExceptionHandlerInstruction : BranchInstruction {
        public LeaveExceptionHandlerInstruction(bool hasValue) 
            : base(hasValue, hasValue) {
        }

        public override int Run(InterpretedFrame frame) {
            // CLR rethrows ThreadAbortException when leaving catch handler if abort is requested on the current thread.
            Interpreter.AbortThreadIfRequested(frame, _offset);
            return _offset;
        }
    }

    public sealed class SwitchInstruction : Instruction {
        //TODO this is probably much more efficient as an int[] for simple cases
        private Dictionary<int, int> _cases = new Dictionary<int, int>();
        private int _defaultOffset;

        public SwitchInstruction() { }
        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 0; } } //???
        public override int Run(InterpretedFrame frame) {
            var test = frame.Pop();
            if (test == null) return _defaultOffset;

            int offset;
            if (_cases.TryGetValue((int)test, out offset)) {
                return offset;
            } else {
                return _defaultOffset;
            }
        }

        internal void AddCase(int test, int offset) {
            // First one wins if keys are duplicated
            if (!_cases.ContainsKey(test)) {
                _cases.Add(test, offset);
            }
        }
        internal void AddDefault(int offset) {
            _defaultOffset = offset;
        }

    }

    public sealed class ThrowInstruction : Instruction {
        public static readonly ThrowInstruction Throw = new ThrowInstruction(true);
        public static readonly ThrowInstruction VoidThrow = new ThrowInstruction(false);

        private readonly bool _hasResult;

        private ThrowInstruction(bool hasResult) {
            _hasResult = hasResult;
        }

        public override int ProducedStack {
            get { return _hasResult ? 1 : 0; }
        }

        public override int ConsumedStack {
            get { return 1; }
        }

        public override int Run(InterpretedFrame frame) {
            throw (Exception)frame.Pop();
        }
    }

    #endregion

    #region operations, i.e. calls, arithmetic, comparisons, etc.

    //TODO generate the Func and Action equivalent overloads for better performance
    public class CallInstruction : Instruction {
        private ReflectedCaller _target;
        private MethodInfo _methodInfo;
        private bool _isVoid;
        private int _argCount;

        public CallInstruction(MethodInfo target) {
            _methodInfo = target;
            _isVoid = target.ReturnType == typeof(void);
            _argCount = target.GetParameters().Length;
            if (!target.IsStatic) _argCount += 1;

            _target = ReflectedCaller.Create(target);
        }

        public override int ProducedStack { get { return _isVoid ? 0 : 1; } }
        public override int ConsumedStack { get { return _argCount; } }

        public override int Run(InterpretedFrame frame) {
            object[] args = new object[_argCount];
            for (int i = _argCount - 1; i >= 0; i--) {
                args[i] = frame.Pop();
            }

            object ret = _target.Invoke(args);
            if (!_isVoid) frame.Push(ret);

            return +1;
        }

        public override string ToString() {
            return "Call(" + _methodInfo + ")";
        }
    }


    public class CreateDelegateInstruction : Instruction {
        private readonly LightDelegateCreator _creator;

        internal CreateDelegateInstruction(LightDelegateCreator delegateCreator) {
            this._creator = delegateCreator;
        }

        public override int ConsumedStack { get { return _creator.ClosureVariables.Count; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            StrongBox<object>[] closure;
            if (ConsumedStack > 0) {
                closure = new StrongBox<object>[ConsumedStack];
                for (int i = closure.Length - 1; i >= 0; i--) {
                    closure[i] = (StrongBox<object>)frame.Pop();
                }
            } else {
                closure = null;
            }

            Delegate d = _creator.CreateDelegate(closure);

            frame.Push(d);
            return +1;
        }
    }

    public class NewArrayInitInstruction : Instruction {
        private Type _elementType;
        private int _elementCount;
        public NewArrayInitInstruction(Type elementType, int elementCount) {
            this._elementType = elementType;
            this._elementCount = elementCount;
        }

        public override int ConsumedStack { get { return _elementCount; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            var array = Array.CreateInstance(_elementType, _elementCount);
            for (int i = _elementCount - 1; i >= 0; i--) {
                array.SetValue(frame.Pop(), i);
            }
            frame.Push(array);
            return +1;
        }
    }

    public class NewArrayBoundsInstruction1 : Instruction {
        private Type _elementType;

        public NewArrayBoundsInstruction1(Type elementType) {
            this._elementType = elementType;
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            int length = (int)frame.Pop();
            var array = Array.CreateInstance(_elementType, length);
            frame.Push(array);
            return +1;
        }
    }

    public class NewArrayBoundsInstructionN : Instruction {
        private Type _elementType;
        private int _boundsCount;
        public NewArrayBoundsInstructionN(Type elementType, int boundsCount) {
            this._elementType = elementType;
            this._boundsCount = boundsCount;
        }

        public override int ConsumedStack { get { return _boundsCount; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            var bounds = new int[_boundsCount];
            for (int i = _boundsCount - 1; i >= 0; i--) {
                bounds[i] = (int)frame.Pop();
            }
            var array = Array.CreateInstance(_elementType, bounds);
            frame.Push(array);
            return +1;
        }
    }

    public sealed class NewInstruction : Instruction {
        private readonly ConstructorInfo _constructor;
        private readonly int _argCount;

        public NewInstruction(ConstructorInfo constructor) {
            _constructor = constructor;
            _argCount = constructor.GetParameters().Length;

        }
        public override int ConsumedStack { get { return _argCount; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            object[] args = new object[_argCount];
            for (int i = _argCount - 1; i >= 0; i--) {
                args[i] = frame.Pop();
            }

            object ret;
            try {
                ret = _constructor.Invoke(args);
            } catch (TargetInvocationException e) {
                ExceptionHelpers.UpdateForRethrow(e.InnerException);
                throw e.InnerException;
            }
            frame.Push(ret);
            return +1;
        }

        public override string ToString() {
            return "New " + _constructor.DeclaringType.Name + "(" + _constructor + ")";
        }
    }

    public class StaticFieldAccessInstruction : Instruction {
        private readonly FieldInfo _field;

        public StaticFieldAccessInstruction(FieldInfo field) {
            Debug.Assert(field.IsStatic);
            _field = field;
        }

        public override int ProducedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            frame.Push(_field.GetValue(null));
            return +1;
        }
    }

    public class FieldAccessInstruction : Instruction {
        private readonly FieldInfo _field;

        public FieldAccessInstruction(FieldInfo field) {
            Assert.NotNull(field);
            _field = field;
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            frame.Push(_field.GetValue(frame.Pop()));
            return +1;
        }
    }

    public class FieldAssignInstruction : Instruction {
        private readonly FieldInfo _field;

        public FieldAssignInstruction(FieldInfo field) {
            Assert.NotNull(field);
            _field = field;
        }

        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 0; } }

        public override int Run(InterpretedFrame frame) {
            object value = frame.Pop();
            object self = frame.Pop();
            _field.SetValue(self, value);
            return +1;
        }
    }

    public class GetArrayItemInstruction<T> : Instruction {
        public static readonly Instruction Instance = new GetArrayItemInstruction<T>();

        private GetArrayItemInstruction() { }

        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            int index = (int)frame.Pop();
            T[] array = (T[])frame.Pop();
            frame.Push(array[index]);
            return +1;
        }

        public override string InstructionName {
            get { return "GetArrayItem"; }
        }
    }

    public class SetArrayItemInstruction<T> : Instruction {
        public static readonly Instruction Instance = new SetArrayItemInstruction<T>();

        private SetArrayItemInstruction() { }
        public override int ConsumedStack { get { return 3; } }
        public override int ProducedStack { get { return 0; } }

        public override int Run(InterpretedFrame frame) {
            int index = (int)frame.Pop();
            T[] array = (T[])frame.Pop();
            T value = (T)frame.Pop();
            array[index] = value;
            return +1;
        }

        public override string InstructionName {
            get { return "SetArrayItem"; }
        }
    }

    public abstract class NumericConvertInstruction : Instruction {
        internal readonly TypeCode _from, _to;

        public NumericConvertInstruction(TypeCode from, TypeCode to) {
            _from = from;
            _to = to;
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        public override string ToString() {
            return InstructionName + "(" + _from + "->" + _to + ")";
        }
        
        public sealed class Unchecked : NumericConvertInstruction {
            public override string InstructionName { get { return "UncheckedConvert"; } }

            public Unchecked(TypeCode from, TypeCode to) 
                : base(from, to) { 
            }

            public override int Run(InterpretedFrame frame) {
                frame.Push(Convert(frame.Pop()));
                return +1;
            }

            private object Convert(object obj) {
                switch (_from) {
                    case TypeCode.Byte: return ConvertInt32((Byte)obj);
                    case TypeCode.SByte: return ConvertInt32((SByte)obj);
                    case TypeCode.Int16: return ConvertInt32((Int16)obj);
                    case TypeCode.Char: return ConvertInt32((Char)obj);
                    case TypeCode.Int32: return ConvertInt32((Int32)obj);
                    case TypeCode.Int64: return ConvertInt64((Int64)obj);
                    case TypeCode.UInt16: return ConvertInt32((UInt16)obj);
                    case TypeCode.UInt32: return ConvertInt64((UInt32)obj);
                    case TypeCode.UInt64: return ConvertUInt64((UInt64)obj);
                    case TypeCode.Single: return ConvertDouble((Single)obj);
                    case TypeCode.Double: return ConvertDouble((Double)obj);
                    default: throw Assert.Unreachable;
                }
            }

            private object ConvertInt32(int obj) {
                unchecked {
                    switch (_to) {
                        case TypeCode.Byte: return (Byte)obj;
                        case TypeCode.SByte: return (SByte)obj;
                        case TypeCode.Int16: return (Int16)obj;
                        case TypeCode.Char: return (Char)obj;
                        case TypeCode.Int32: return (Int32)obj;
                        case TypeCode.Int64: return (Int64)obj;
                        case TypeCode.UInt16: return (UInt16)obj;
                        case TypeCode.UInt32: return (UInt32)obj;
                        case TypeCode.UInt64: return (UInt64)obj;
                        case TypeCode.Single: return (Single)obj;
                        case TypeCode.Double: return (Double)obj;
                        default: throw Assert.Unreachable;
                    }
                }
            }

            private object ConvertInt64(Int64 obj) {
                unchecked {
                    switch (_to) {
                        case TypeCode.Byte: return (Byte)obj;
                        case TypeCode.SByte: return (SByte)obj;
                        case TypeCode.Int16: return (Int16)obj;
                        case TypeCode.Char: return (Char)obj;
                        case TypeCode.Int32: return (Int32)obj;
                        case TypeCode.Int64: return (Int64)obj;
                        case TypeCode.UInt16: return (UInt16)obj;
                        case TypeCode.UInt32: return (UInt32)obj;
                        case TypeCode.UInt64: return (UInt64)obj;
                        case TypeCode.Single: return (Single)obj;
                        case TypeCode.Double: return (Double)obj;
                        default: throw Assert.Unreachable;
                    }
                }
            }
                                
            private object ConvertUInt64(UInt64 obj) {
                unchecked {
                    switch (_to) {
                        case TypeCode.Byte: return (Byte)obj;
                        case TypeCode.SByte: return (SByte)obj;
                        case TypeCode.Int16: return (Int16)obj;
                        case TypeCode.Char: return (Char)obj;
                        case TypeCode.Int32: return (Int32)obj;
                        case TypeCode.Int64: return (Int64)obj;
                        case TypeCode.UInt16: return (UInt16)obj;
                        case TypeCode.UInt32: return (UInt32)obj;
                        case TypeCode.UInt64: return (UInt64)obj;
                        case TypeCode.Single: return (Single)obj;
                        case TypeCode.Double: return (Double)obj;
                        default: throw Assert.Unreachable;
                    }
                }
            }

            private object ConvertDouble(Double obj) {
                unchecked {
                    switch (_to) {
                        case TypeCode.Byte: return (Byte)obj;
                        case TypeCode.SByte: return (SByte)obj;
                        case TypeCode.Int16: return (Int16)obj;
                        case TypeCode.Char: return (Char)obj;
                        case TypeCode.Int32: return (Int32)obj;
                        case TypeCode.Int64: return (Int64)obj;
                        case TypeCode.UInt16: return (UInt16)obj;
                        case TypeCode.UInt32: return (UInt32)obj;
                        case TypeCode.UInt64: return (UInt64)obj;
                        case TypeCode.Single: return (Single)obj;
                        case TypeCode.Double: return (Double)obj;
                        default: throw Assert.Unreachable;
                    }
                }
            }
        }

        public sealed class Checked : NumericConvertInstruction {
            public override string InstructionName { get { return "CheckedConvert"; } }

            public Checked(TypeCode from, TypeCode to) 
                : base(from, to) { 
            }

            public override int Run(InterpretedFrame frame) {
                frame.Push(Convert(frame.Pop()));
                return +1;
            }

            private object Convert(object obj) {
                switch (_from) {
                    case TypeCode.Byte: return ConvertInt32((Byte)obj);
                    case TypeCode.SByte: return ConvertInt32((SByte)obj);
                    case TypeCode.Int16: return ConvertInt32((Int16)obj);
                    case TypeCode.Char: return ConvertInt32((Char)obj);
                    case TypeCode.Int32: return ConvertInt32((Int32)obj);
                    case TypeCode.Int64: return ConvertInt64((Int64)obj);
                    case TypeCode.UInt16: return ConvertInt32((UInt16)obj);
                    case TypeCode.UInt32: return ConvertInt64((UInt32)obj);
                    case TypeCode.UInt64: return ConvertUInt64((UInt64)obj);
                    case TypeCode.Single: return ConvertDouble((Single)obj);
                    case TypeCode.Double: return ConvertDouble((Double)obj);
                    default: throw Assert.Unreachable;
                }
            }

            private object ConvertInt32(int obj) {
                checked {
                    switch (_to) {
                        case TypeCode.Byte: return (Byte)obj;
                        case TypeCode.SByte: return (SByte)obj;
                        case TypeCode.Int16: return (Int16)obj;
                        case TypeCode.Char: return (Char)obj;
                        case TypeCode.Int32: return (Int32)obj;
                        case TypeCode.Int64: return (Int64)obj;
                        case TypeCode.UInt16: return (UInt16)obj;
                        case TypeCode.UInt32: return (UInt32)obj;
                        case TypeCode.UInt64: return (UInt64)obj;
                        case TypeCode.Single: return (Single)obj;
                        case TypeCode.Double: return (Double)obj;
                        default: throw Assert.Unreachable;
                    }
                }
            }

            private object ConvertInt64(Int64 obj) {
                checked {
                    switch (_to) {
                        case TypeCode.Byte: return (Byte)obj;
                        case TypeCode.SByte: return (SByte)obj;
                        case TypeCode.Int16: return (Int16)obj;
                        case TypeCode.Char: return (Char)obj;
                        case TypeCode.Int32: return (Int32)obj;
                        case TypeCode.Int64: return (Int64)obj;
                        case TypeCode.UInt16: return (UInt16)obj;
                        case TypeCode.UInt32: return (UInt32)obj;
                        case TypeCode.UInt64: return (UInt64)obj;
                        case TypeCode.Single: return (Single)obj;
                        case TypeCode.Double: return (Double)obj;
                        default: throw Assert.Unreachable;
                    }
                }
            }
                                
            private object ConvertUInt64(UInt64 obj) {
                checked {
                    switch (_to) {
                        case TypeCode.Byte: return (Byte)obj;
                        case TypeCode.SByte: return (SByte)obj;
                        case TypeCode.Int16: return (Int16)obj;
                        case TypeCode.Char: return (Char)obj;
                        case TypeCode.Int32: return (Int32)obj;
                        case TypeCode.Int64: return (Int64)obj;
                        case TypeCode.UInt16: return (UInt16)obj;
                        case TypeCode.UInt32: return (UInt32)obj;
                        case TypeCode.UInt64: return (UInt64)obj;
                        case TypeCode.Single: return (Single)obj;
                        case TypeCode.Double: return (Double)obj;
                        default: throw Assert.Unreachable;
                    }
                }
            }

            private object ConvertDouble(Double obj) {
                checked {
                    switch (_to) {
                        case TypeCode.Byte: return (Byte)obj;
                        case TypeCode.SByte: return (SByte)obj;
                        case TypeCode.Int16: return (Int16)obj;
                        case TypeCode.Char: return (Char)obj;
                        case TypeCode.Int32: return (Int32)obj;
                        case TypeCode.Int64: return (Int64)obj;
                        case TypeCode.UInt16: return (UInt16)obj;
                        case TypeCode.UInt32: return (UInt32)obj;
                        case TypeCode.UInt64: return (UInt64)obj;
                        case TypeCode.Single: return (Single)obj;
                        case TypeCode.Double: return (Double)obj;
                        default: throw Assert.Unreachable;
                    }
                }
            }
        }
    }

    public class NotInstruction : Instruction {
        public static readonly Instruction Instance = new NotInstruction();

        private NotInstruction() { }
        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            frame.Push(!(bool)frame.Pop());
            return +1;
        }
    }

    public class AddIntInstruction : Instruction {
        public static readonly Instruction Instance = new AddIntInstruction();

        private AddIntInstruction() { }

        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            frame.Push((int)frame.Pop() + (int)frame.Pop());
            return +1;
        }
    }

    public abstract class EqualInstruction : Instruction {
        // Perf: EqualityComparer<T> but is 3/2 to 2 times slower.
        private static Instruction _Reference, _Boolean, _SByte, _Int16, _Char, _Int32, _Int64, _Byte, _UInt16, _UInt32, _UInt64, _Single, _Double;

        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }

        private EqualInstruction() {
        }

        internal sealed class EqualBoolean : EqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Boolean)frame.Pop()) == ((Boolean)frame.Pop()));
                return +1;
            }
        }

        internal sealed class EqualSByte : EqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((SByte)frame.Pop()) == ((SByte)frame.Pop()));
                return +1;
            }
        }

        internal sealed class EqualInt16 : EqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Int16)frame.Pop()) == ((Int16)frame.Pop()));
                return +1;
            }
        }

        internal sealed class EqualChar : EqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Char)frame.Pop()) == ((Char)frame.Pop()));
                return +1;
            }
        }

        internal sealed class EqualInt32 : EqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Int32)frame.Pop()) == ((Int32)frame.Pop()));
                return +1;
            }
        }

        internal sealed class EqualInt64 : EqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Int64)frame.Pop()) == ((Int64)frame.Pop()));
                return +1;
            }
        }

        internal sealed class EqualByte : EqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Byte)frame.Pop()) == ((Byte)frame.Pop()));
                return +1;
            }
        }

        internal sealed class EqualUInt16 : EqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((UInt16)frame.Pop()) == ((UInt16)frame.Pop()));
                return +1;
            }
        }

        internal sealed class EqualUInt32 : EqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((UInt32)frame.Pop()) == ((UInt32)frame.Pop()));
                return +1;
            }
        }

        internal sealed class EqualUInt64 : EqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((UInt64)frame.Pop()) == ((UInt64)frame.Pop()));
                return +1;
            }
        }

        internal sealed class EqualSingle : EqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Single)frame.Pop()) == ((Single)frame.Pop()));
                return +1;
            }
        }

        internal sealed class EqualDouble : EqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Double)frame.Pop()) == ((Double)frame.Pop()));
                return +1;
            }
        }

        internal sealed class EqualReference : EqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(frame.Pop() == frame.Pop());
                return +1;
            }
        }

        public static Instruction Create(Type type) {
            // Boxed enums can be unboxed as their underlying types:
            switch (Type.GetTypeCode(type.IsEnum ? Enum.GetUnderlyingType(type) : type)) {
                case TypeCode.Boolean: return _Boolean ?? (_Boolean = new EqualBoolean());
                case TypeCode.SByte: return _SByte ?? (_SByte = new EqualSByte());
                case TypeCode.Byte: return _Byte ?? (_Byte = new EqualByte());
                case TypeCode.Char: return _Char ?? (_Char = new EqualChar());
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new EqualInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new EqualInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new EqualInt64());

                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new EqualInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new EqualInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new EqualInt64());

                case TypeCode.Single: return _Single ?? (_Single = new EqualSingle());
                case TypeCode.Double: return _Double ?? (_Double = new EqualDouble());

                case TypeCode.Object:
                    if (!type.IsValueType) {
                        return _Reference ?? (_Reference = new EqualReference());
                    }
                    // TODO: Nullable<T>
                    throw new NotImplementedException();

                default:
                    throw new NotImplementedException();
            }
        }

        public override string ToString() {
            return "Equal()";
        }
    }

    public abstract class NotEqualInstruction : Instruction {
        // Perf: EqualityComparer<T> but is 3/2 to 2 times slower.
        private static Instruction _Reference, _Boolean, _SByte, _Int16, _Char, _Int32, _Int64, _Byte, _UInt16, _UInt32, _UInt64, _Single, _Double;
            
        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }

        private NotEqualInstruction() {
        }

        internal sealed class NotEqualBoolean : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Boolean)frame.Pop()) != ((Boolean)frame.Pop()));
                return +1;
            }
        }

        internal sealed class NotEqualSByte : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((SByte)frame.Pop()) != ((SByte)frame.Pop()));
                return +1;
            }
        }

        internal sealed class NotEqualInt16 : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Int16)frame.Pop()) != ((Int16)frame.Pop()));
                return +1;
            }
        }

        internal sealed class NotEqualChar : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Char)frame.Pop()) != ((Char)frame.Pop()));
                return +1;
            }
        }

        internal sealed class NotEqualInt32 : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Int32)frame.Pop()) != ((Int32)frame.Pop()));
                return +1;
            }
        }

        internal sealed class NotEqualInt64 : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Int64)frame.Pop()) != ((Int64)frame.Pop()));
                return +1;
            }
        }

        internal sealed class NotEqualByte : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Byte)frame.Pop()) != ((Byte)frame.Pop()));
                return +1;
            }
        }

        internal sealed class NotEqualUInt16 : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((UInt16)frame.Pop()) != ((UInt16)frame.Pop()));
                return +1;
            }
        }

        internal sealed class NotEqualUInt32 : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((UInt32)frame.Pop()) != ((UInt32)frame.Pop()));
                return +1;
            }
        }

        internal sealed class NotEqualUInt64 : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((UInt64)frame.Pop()) != ((UInt64)frame.Pop()));
                return +1;
            }
        }

        internal sealed class NotEqualSingle : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Single)frame.Pop()) != ((Single)frame.Pop()));
                return +1;
            }
        }

        internal sealed class NotEqualDouble : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Double)frame.Pop()) != ((Double)frame.Pop()));
                return +1;
            }
        }

        internal sealed class NotEqualReference : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(frame.Pop() != frame.Pop());
                return +1;
            }
        }

        public static Instruction Instance(Type type) {
            // Boxed enums can be unboxed as their underlying types:
            switch (Type.GetTypeCode(type.IsEnum ? Enum.GetUnderlyingType(type) : type)) {
                case TypeCode.Boolean: return _Boolean ?? (_Boolean = new NotEqualBoolean());
                case TypeCode.SByte: return _SByte ?? (_SByte = new NotEqualSByte());
                case TypeCode.Byte: return _Byte ?? (_Byte = new NotEqualByte());
                case TypeCode.Char: return _Char ?? (_Char = new NotEqualChar());
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new NotEqualInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new NotEqualInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new NotEqualInt64());

                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new NotEqualInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new NotEqualInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new NotEqualInt64());

                case TypeCode.Single: return _Single ?? (_Single = new NotEqualSingle());
                case TypeCode.Double: return _Double ?? (_Double = new NotEqualDouble());

                case TypeCode.Object:
                    if (!type.IsValueType) {
                        return _Reference ?? (_Reference = new NotEqualReference());
                    }
                    // TODO: Nullable<T>
                    throw new NotImplementedException();

                default:
                    throw new NotImplementedException();
            }
        }

        public override string ToString() {
            return "NotEqual()";
        }
    }

    public abstract class LessThanInstruction : Instruction {
        private static Instruction _SByte, _Int16, _Char, _Int32, _Int64, _Byte, _UInt16, _UInt32, _UInt64, _Single, _Double;

        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }

        private LessThanInstruction() {
        }

        internal sealed class LessThanSByte : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((SByte)frame.Pop()) < ((SByte)frame.Pop()));
                return +1;
            }
        }

        internal sealed class LessThanInt16 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Int16)frame.Pop()) < ((Int16)frame.Pop()));
                return +1;
            }
        }

        internal sealed class LessThanChar : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Char)frame.Pop()) < ((Char)frame.Pop()));
                return +1;
            }
        }

        internal sealed class LessThanInt32 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Int32)frame.Pop()) < ((Int32)frame.Pop()));
                return +1;
            }
        }

        internal sealed class LessThanInt64 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Int64)frame.Pop()) < ((Int64)frame.Pop()));
                return +1;
            }
        }

        internal sealed class LessThanByte : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Byte)frame.Pop()) < ((Byte)frame.Pop()));
                return +1;
            }
        }

        internal sealed class LessThanUInt16 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((UInt16)frame.Pop()) < ((UInt16)frame.Pop()));
                return +1;
            }
        }

        internal sealed class LessThanUInt32 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((UInt32)frame.Pop()) < ((UInt32)frame.Pop()));
                return +1;
            }
        }

        internal sealed class LessThanUInt64 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((UInt64)frame.Pop()) < ((UInt64)frame.Pop()));
                return +1;
            }
        }

        internal sealed class LessThanSingle : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Single)frame.Pop()) < ((Single)frame.Pop()));
                return +1;
            }
        }

        internal sealed class LessThanDouble : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(((Double)frame.Pop()) < ((Double)frame.Pop()));
                return +1;
            }
        }

        public static Instruction Instance(Type type) {
            Debug.Assert(!type.IsEnum);
            switch (Type.GetTypeCode(type)) {
                case TypeCode.SByte: return _SByte ?? (_SByte = new LessThanSByte());
                case TypeCode.Byte: return _Byte ?? (_Byte = new LessThanSByte());
                case TypeCode.Char: return _Char ?? (_Char = new LessThanSByte());
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new LessThanSByte());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new LessThanSByte());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new LessThanSByte());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new LessThanSByte());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new LessThanSByte());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new LessThanSByte());
                case TypeCode.Single: return _Single ?? (_Single = new LessThanSByte());
                case TypeCode.Double: return _Double ?? (_Double = new LessThanSByte());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "LessThan()";
        }
    }

    public sealed class TypeEqualsInstruction : Instruction {
        public static readonly TypeEqualsInstruction Instance = new TypeEqualsInstruction();

        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }

        private TypeEqualsInstruction() {
        }

        public override int Run(InterpretedFrame frame) {
            Type type = (Type)frame.Pop();
            object obj = frame.Pop();
            frame.Push(ScriptingRuntimeHelpers.BooleanToObject(obj != null && obj.GetType() == type));
            return +1;
        }

        public override string InstructionName {
            get { return "TypeEquals()"; }
        }
    }

    #endregion

    public class RuntimeVariablesInstruction : Instruction {
        private readonly int _count;

        public RuntimeVariablesInstruction(int count) {
            _count = count;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return _count; } }

        public override int Run(InterpretedFrame frame) {
            var ret = new IStrongBox[_count];
            for (int i = ret.Length - 1; i >= 0; i--) {
                ret[i] = (IStrongBox)frame.Pop();
            }
            frame.Push(RuntimeVariables.Create(ret));
            return +1;
        }

        public override string ToString() {
            return "GetRuntimeVariables()";
        }
    }
}
