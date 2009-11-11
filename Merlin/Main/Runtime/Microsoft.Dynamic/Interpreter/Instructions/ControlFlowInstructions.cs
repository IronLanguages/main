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
    internal abstract class OffsetInstruction : Instruction {
        internal const int Unknown = Int32.MinValue;
        internal const int CacheSize = 32;

        // the offset to jump to (relative to this instruction):
        protected int _offset = Unknown;

        public int Offset { get { return _offset; } }
        public abstract Instruction[] Cache { get; }

        public virtual Instruction Fixup(int offset, int targetStackDepth) {
            Debug.Assert(_offset == Unknown && offset != Unknown);
            _offset = offset;

            var cache = Cache;
            if (cache != null && offset >= 0 && offset < cache.Length) {
                return cache[offset] ?? (cache[offset] = this);
            }

            return this;
        }

        public override string ToString() {
            return InstructionName + (_offset == Unknown ? "(?)" : "(" + _offset + ")");
        }
    }

    internal sealed class BranchFalseInstruction : OffsetInstruction {
        private static Instruction[] _cache;

        public override Instruction[] Cache { 
            get {
                if (_cache == null) {
                    _cache = new Instruction[CacheSize];
                }
                return _cache;
            } 
        }

        internal BranchFalseInstruction() {
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

    internal sealed class BranchTrueInstruction : OffsetInstruction {
        private static Instruction[] _cache;

        public override Instruction[] Cache {
            get {
                if (_cache == null) {
                    _cache = new Instruction[CacheSize];
                }
                return _cache;
            }
        }

        internal BranchTrueInstruction() {
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

    internal sealed class CoalescingBranchInstruction : OffsetInstruction {
        private static Instruction[] _cache;

        public override Instruction[] Cache {
            get {
                if (_cache == null) {
                    _cache = new Instruction[CacheSize];
                }
                return _cache;
            }
        }

        internal CoalescingBranchInstruction() {
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

    internal class BranchInstruction : OffsetInstruction {
        private static Instruction[][][] _caches;

        public override Instruction[] Cache {
            get {
                if (_caches == null) {
                    _caches = new Instruction[2][][] { new Instruction[2][], new Instruction[2][] };
                }
                return _caches[ConsumedStack][ProducedStack] ?? (_caches[ConsumedStack][ProducedStack] = new Instruction[CacheSize]);
            }
        }

        internal readonly bool _hasResult;
        internal readonly bool _hasValue;

        internal BranchInstruction() 
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
    internal sealed class GotoInstruction : BranchInstruction {
        internal struct FinallyBlock {
            public int Start, End, StackDepth;

            public FinallyBlock(int start, int end, int stackDepth) {
                Start = start;
                End = end;
                StackDepth = stackDepth;
            }
        }

        // the target label stack depth
        internal int _targetStackDepth;

        // index of this instruction in instruction array (used only during compilation):
        internal int _instructionIndex; 

        // A list of finally blocks that need to be executed as we jump to the target label.
        internal List<FinallyBlock> _finallyBlocks;

        public GotoInstruction(int instructionIndex, bool hasResult, bool hasValue) 
            : base(hasResult, hasValue) {
            Debug.Assert(instructionIndex >= 0);
            _instructionIndex = instructionIndex;
        }

        public override Instruction[] Cache {
            get { return null; }
        }

        public override Instruction Fixup(int offset, int targetStackDepth) {
            _targetStackDepth = targetStackDepth;
            return base.Fixup(offset, targetStackDepth);
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
    internal sealed class EnterExceptionHandlerInstruction : Instruction {
        internal static readonly EnterExceptionHandlerInstruction Void = new EnterExceptionHandlerInstruction(false);
        internal static readonly EnterExceptionHandlerInstruction NonVoid = new EnterExceptionHandlerInstruction(true);

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

    internal sealed class LeaveExceptionHandlerInstruction : BranchInstruction {
        private static Instruction[] _cache;

        public override Instruction[] Cache {
            get {
                if (_cache == null) {
                    _cache = new Instruction[CacheSize];
                }
                return _cache;
            }
        }
        
        public LeaveExceptionHandlerInstruction(bool hasValue) 
            : base(hasValue, hasValue) {
        }

        public override int Run(InterpretedFrame frame) {
            // CLR rethrows ThreadAbortException when leaving catch handler if abort is requested on the current thread.
            Interpreter.AbortThreadIfRequested(frame, _offset);
            return _offset;
        }
    }

    internal sealed class SwitchInstruction : Instruction {
        private readonly Dictionary<int, int> _cases;

        internal SwitchInstruction(Dictionary<int, int> cases) {
            Assert.NotNull(cases);
            _cases = cases;
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 0; } }

        public override int Run(InterpretedFrame frame) {
            int target;
            return _cases.TryGetValue((int)frame.Pop(), out target) ? target : 1;
        }
    }

    internal sealed class ThrowInstruction : Instruction {
        internal static readonly ThrowInstruction Throw = new ThrowInstruction(true);
        internal static readonly ThrowInstruction VoidThrow = new ThrowInstruction(false);

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
}
