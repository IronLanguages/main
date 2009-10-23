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
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpreter {

    public interface IInstructionProvider {
        void AddInstructions(LightCompiler compiler);
    }

    public abstract partial class Instruction {
        public virtual int ConsumedStack { get { return 0; } }
        public virtual int ProducedStack { get { return 0; } }

        public abstract int Run(InterpretedFrame frame);

        public virtual string InstructionName {
            get { return GetType().Name.Replace("Instruction", ""); }
        }

        public override string ToString() {
            return InstructionName + "()";
        }

        public virtual string ToString(LightCompiler compiler) {
            return ToString();
        }

        public static Instruction DefaultValue(Type type) {
            return InstructionFactory.GetFactory(type).DefaultValue();
        }
    }

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

    public sealed class CoalescingBranchInstruction : OffsetInstruction {
        public CoalescingBranchInstruction() {
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            Debug.Assert(_offset != Unknown);

            if (frame.Peek() != null) {
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly EnterExceptionHandlerInstruction Void = new EnterExceptionHandlerInstruction(false);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly ThrowInstruction Throw = new ThrowInstruction(true);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
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

    #region operations, i.e. arithmetic, comparisons, etc.

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

    public class NewInstruction : Instruction {
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

    public class DefaultValueInstruction<T> : Instruction {
        internal DefaultValueInstruction() { }

        public override int ConsumedStack { get { return 0; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            frame.Push(default(T));
            return +1;
        }

        public override string ToString() {
            return "New " + typeof(T);
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

    public abstract class NumericConvertInstruction : Instruction {
        internal readonly TypeCode _from, _to;

        protected NumericConvertInstruction(TypeCode from, TypeCode to) {
            _from = from;
            _to = to;
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        public override string ToString() {
            return InstructionName + "(" + _from + "->" + _to + ")";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
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

    public class TypeIsInstruction<T> : Instruction {
        internal TypeIsInstruction() { }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            // unfortunately Type.IsInstanceOfType() is 35-times slower than "is T" so we use generic code:
            frame.Push(ScriptingRuntimeHelpers.BooleanToObject(frame.Pop() is T));
            return +1;
        }

        public override string ToString() {
            return "TypeIs " + typeof(T).Name; 
        }
    }

    public class NotInstruction : Instruction {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Instruction Instance = new NotInstruction();

        private NotInstruction() { }
        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            frame.Push((bool)frame.Pop() ? ScriptingRuntimeHelpers.False : ScriptingRuntimeHelpers.True);
            return +1;
        }
    }

    public class AddIntInstruction : Instruction {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
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
                SByte right = (SByte)frame.Pop();
                frame.Push(((SByte)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanInt16 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Int16 right = (Int16)frame.Pop();
                frame.Push(((Int16)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanChar : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Char right = (Char)frame.Pop();
                frame.Push(((Char)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanInt32 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Int32 right = (Int32)frame.Pop();
                frame.Push(((Int32)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanInt64 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Int64 right = (Int64)frame.Pop();
                frame.Push(((Int64)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanByte : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Byte right = (Byte)frame.Pop();
                frame.Push(((Byte)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanUInt16 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                UInt16 right = (UInt16)frame.Pop();
                frame.Push(((UInt16)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanUInt32 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                UInt32 right = (UInt32)frame.Pop();
                frame.Push(((UInt32)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanUInt64 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                UInt64 right = (UInt64)frame.Pop();
                frame.Push(((UInt64)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanSingle : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Single right = (Single)frame.Pop();
                frame.Push(((Single)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanDouble : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Double right = (Double)frame.Pop();
                frame.Push(((Double)frame.Pop()) < right);
                return +1;
            }
        }

        public static Instruction Instance(Type type) {
            Debug.Assert(!type.IsEnum);
            switch (Type.GetTypeCode(type)) {
                case TypeCode.SByte: return _SByte ?? (_SByte = new LessThanSByte());
                case TypeCode.Byte: return _Byte ?? (_Byte = new LessThanByte());
                case TypeCode.Char: return _Char ?? (_Char = new LessThanChar());
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new LessThanInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new LessThanInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new LessThanInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new LessThanUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new LessThanUInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new LessThanUInt64());
                case TypeCode.Single: return _Single ?? (_Single = new LessThanSingle());
                case TypeCode.Double: return _Double ?? (_Double = new LessThanDouble());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "LessThan()";
        }
    }

    public abstract class GreaterThanInstruction : Instruction {
        private static Instruction _SByte, _Int16, _Char, _Int32, _Int64, _Byte, _UInt16, _UInt32, _UInt64, _Single, _Double;

        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }

        private GreaterThanInstruction() {
        }

        internal sealed class GreaterThanSByte : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                SByte right = (SByte)frame.Pop();
                frame.Push(((SByte)frame.Pop()) > right);
                return +1;
            }
        }

        internal sealed class GreaterThanInt16 : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Int16 right = (Int16)frame.Pop();
                frame.Push(((Int16)frame.Pop()) > right);
                return +1;
            }
        }

        internal sealed class GreaterThanChar : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Char right = (Char)frame.Pop();
                frame.Push(((Char)frame.Pop()) > right);
                return +1;
            }
        }

        internal sealed class GreaterThanInt32 : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Int32 right = (Int32)frame.Pop();
                frame.Push(((Int32)frame.Pop()) > right);
                return +1;
            }
        }

        internal sealed class GreaterThanInt64 : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Int64 right = (Int64)frame.Pop();
                frame.Push(((Int64)frame.Pop()) > right);
                return +1;
            }
        }

        internal sealed class GreaterThanByte : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Byte right = (Byte)frame.Pop();
                frame.Push(((Byte)frame.Pop()) > right);
                return +1;
            }
        }

        internal sealed class GreaterThanUInt16 : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                UInt16 right = (UInt16)frame.Pop();
                frame.Push(((UInt16)frame.Pop()) > right);
                return +1;
            }
        }

        internal sealed class GreaterThanUInt32 : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                UInt32 right = (UInt32)frame.Pop();
                frame.Push(((UInt32)frame.Pop()) > right);
                return +1;
            }
        }

        internal sealed class GreaterThanUInt64 : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                UInt64 right = (UInt64)frame.Pop();
                frame.Push(((UInt64)frame.Pop()) > right);
                return +1;
            }
        }

        internal sealed class GreaterThanSingle : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Single right = (Single)frame.Pop();
                frame.Push(((Single)frame.Pop()) > right);
                return +1;
            }
        }

        internal sealed class GreaterThanDouble : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Double right = (Double)frame.Pop();
                frame.Push(((Double)frame.Pop()) > right);
                return +1;
            }
        }

        public static Instruction Instance(Type type) {
            Debug.Assert(!type.IsEnum);
            switch (Type.GetTypeCode(type)) {
                case TypeCode.SByte: return _SByte ?? (_SByte = new GreaterThanSByte());
                case TypeCode.Byte: return _Byte ?? (_Byte = new GreaterThanByte());
                case TypeCode.Char: return _Char ?? (_Char = new GreaterThanChar());
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new GreaterThanInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new GreaterThanInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new GreaterThanInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new GreaterThanUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new GreaterThanUInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new GreaterThanUInt64());
                case TypeCode.Single: return _Single ?? (_Single = new GreaterThanSingle());
                case TypeCode.Double: return _Double ?? (_Double = new GreaterThanDouble());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "GreaterThan()";
        }
    }

    public class TypeEqualsInstruction : Instruction {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
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
