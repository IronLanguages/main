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

namespace Microsoft.Scripting.Interpreter {
    public sealed class PushInstruction : Instruction {
        private readonly object _value;

        internal PushInstruction(object value) {
            _value = value;
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

    public sealed class PopInstruction : Instruction {
        internal static readonly PopInstruction Instance = new PopInstruction();

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

    public sealed class DupInstruction : Instruction {
        internal readonly static DupInstruction Instance = new DupInstruction();

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

    #region Factories

    public partial class Instruction {
        private const int PushIntMinCachedValue = -100;
        private const int PushIntMaxCachedValue = 100;

        private static Instruction _null;
        private static Instruction _true;
        private static Instruction _false;
        private static Instruction[] _ints;

        public static Instruction Push(object value) {
            if (value == null) {
                return _null ?? (_null = new PushInstruction(null));
            }

            if (value is bool) {
                if ((bool)value) {
                    return _true ?? (_true = new PushInstruction(value));
                } else {
                    return _false ?? (_false = new PushInstruction(value));
                }
            }

            if (value is int) {
                int i = (int)value;
                if (i >= PushIntMinCachedValue && i <= PushIntMaxCachedValue) {
                    if (_ints == null) {
                        _ints = new Instruction[PushIntMaxCachedValue - PushIntMinCachedValue + 1];
                    }
                    i -= PushIntMinCachedValue;
                    return _ints[i] ?? (_ints[i] = new PushInstruction(value));
                }
            }

            return new PushInstruction(value);
        }

        public static Instruction Dup() {
            return DupInstruction.Instance;
        }

        public static Instruction Pop() {
            return PopInstruction.Instance;
        }
    }

    #endregion
}
