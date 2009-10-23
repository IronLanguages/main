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

namespace Microsoft.Scripting.Interpreter {
    public sealed class NewArrayInitInstruction<TElement> : Instruction {
        private readonly int _elementCount;

        internal NewArrayInitInstruction(int elementCount) {
            _elementCount = elementCount;
        }

        public override int ConsumedStack { get { return _elementCount; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            TElement[] array = new TElement[_elementCount];
            for (int i = _elementCount - 1; i >= 0; i--) {
                array[i] = (TElement)frame.Pop();
            }
            frame.Push(array);
            return +1;
        }
    }

    public sealed class NewArrayInstruction<TElement> : Instruction {
        internal NewArrayInstruction() { }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            int length = (int)frame.Pop();
            frame.Push(new TElement[length]);
            return +1;
        }
    }

    public sealed class NewArrayBoundsInstruction : Instruction {
        private readonly Type _elementType;
        private readonly int _rank;

        internal NewArrayBoundsInstruction(Type elementType, int rank) {
            _elementType = elementType;
            _rank = rank;
        }

        public override int ConsumedStack { get { return _rank; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            var lengths = new int[_rank];
            for (int i = _rank - 1; i >= 0; i--) {
                lengths[i] = (int)frame.Pop();
            }
            var array = Array.CreateInstance(_elementType, lengths);
            frame.Push(array);
            return +1;
        }
    }

    public sealed class GetArrayItemInstruction<TElement> : Instruction {
        internal GetArrayItemInstruction() { }

        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            int index = (int)frame.Pop();
            TElement[] array = (TElement[])frame.Pop();
            frame.Push(array[index]);
            return +1;
        }

        public override string InstructionName {
            get { return "GetArrayItem"; }
        }
    }

    public sealed class SetArrayItemInstruction<TElement> : Instruction {
        internal SetArrayItemInstruction() { }

        public override int ConsumedStack { get { return 3; } }
        public override int ProducedStack { get { return 0; } }

        public override int Run(InterpretedFrame frame) {
            TElement value = (TElement)frame.Pop();
            int index = (int)frame.Pop();
            TElement[] array = (TElement[])frame.Pop();
            array[index] = value;
            return +1;
        }

        public override string InstructionName {
            get { return "SetArrayItem"; }
        }
    }

    #region Factories

    public partial class Instruction {
        public static Instruction GetArrayItem(Type arrayType) {
            Type elementType = arrayType.GetElementType();
            if (elementType.IsClass || elementType.IsInterface) {
                return InstructionFactory<object>.Factory.GetArrayItem();
            } else {
                return InstructionFactory.GetFactory(elementType).GetArrayItem();
            }
        }

        public static Instruction SetArrayItem(Type arrayType) {
            Type elementType = arrayType.GetElementType();
            if (elementType.IsClass || elementType.IsInterface) {
                return InstructionFactory<object>.Factory.SetArrayItem();
            } else {
                return InstructionFactory.GetFactory(elementType).SetArrayItem();
            }
        }

        public static Instruction NewArray(Type elementType) {
            return InstructionFactory.GetFactory(elementType).NewArray();
        }

        public static Instruction NewArrayBounds(Type elementType, int rank) {
            return new NewArrayBoundsInstruction(elementType, rank);
        }

        public static Instruction NewArrayInit(Type elementType, int elementCount) {
            return InstructionFactory.GetFactory(elementType).NewArrayInit(elementCount);
        }
    }

    #endregion
}
