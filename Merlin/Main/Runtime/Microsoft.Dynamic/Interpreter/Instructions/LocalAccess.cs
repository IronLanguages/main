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
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Interpreter {
    public interface IBoxableInstruction {
        Instruction BoxIfIndexMatches(int index);
    }

    public abstract class LocalAccessInstruction : Instruction {
        internal readonly int _index;

        protected LocalAccessInstruction(int index) {
            _index = index;
        }

        public override string ToString() {
            return InstructionName + "(" + _index + ")";
        }

        public override string ToString(LightCompiler compiler) {
            return InstructionName + "(" + GetExpression(compiler).Name + ": " + _index + ")";
        }

        internal abstract ParameterExpression GetExpression(LightCompiler compiler);
    }

    #region Get

    public sealed class GetLocalInstruction : LocalAccessInstruction, IBoxableInstruction {
        internal GetLocalInstruction(int index)
            : base(index) {
        }

        public override int ProducedStack { get { return 1; } }
        
        public override int Run(InterpretedFrame frame) {
            frame.Data[frame.StackIndex++] = frame.Data[_index];
            //frame.Push(frame.Data[_index]);
            return +1;
        }

        internal override ParameterExpression GetExpression(LightCompiler compiler) {
            return compiler.Locals[_index];
        }
        
        public Instruction BoxIfIndexMatches(int index) {
            return (index == _index) ? Instruction.GetBoxedLocal(index) : null;
        }
    }

    public sealed class GetBoxedLocalInstruction : LocalAccessInstruction {
        internal GetBoxedLocalInstruction(int index)
            : base(index) {
        }

        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            var box = (StrongBox<object>)frame.Data[_index];
            frame.Data[frame.StackIndex++] = box.Value;
            return +1;
        }

        internal override ParameterExpression GetExpression(LightCompiler compiler) {
            return compiler.Locals[_index];
        }
    }

    public sealed class GetClosureInstruction : LocalAccessInstruction {
        internal GetClosureInstruction(int index)
            : base(index) {
        }

        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            var box = frame.Closure[_index];
            frame.Data[frame.StackIndex++] = box.Value;
            return +1;
        }

        internal override ParameterExpression GetExpression(LightCompiler compiler) {
            return compiler.ClosureVariables[_index];
        }
    }

    public sealed class GetBoxedClosureInstruction : LocalAccessInstruction {
        internal GetBoxedClosureInstruction(int index)
            : base(index) {
        }

        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            var box = frame.Closure[_index];
            frame.Data[frame.StackIndex++] = box;
            return +1;
        }

        internal override ParameterExpression GetExpression(LightCompiler compiler) {
            return compiler.ClosureVariables[_index];
        }
    }

    #endregion

    #region Set

    public sealed class SetLocalInstruction : LocalAccessInstruction, IBoxableInstruction {
        internal SetLocalInstruction(int index)
            : base(index) {
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            frame.Data[_index] = frame.Peek();
            return +1;
        }

        public Instruction BoxIfIndexMatches(int index) {
            return (index == _index) ? Instruction.SetBoxedLocal(index) : null;
        }

        internal override ParameterExpression GetExpression(LightCompiler compiler) {
            return compiler.Locals[_index];
        }
    }

    public sealed class SetLocalVoidInstruction : LocalAccessInstruction, IBoxableInstruction {
        internal SetLocalVoidInstruction(int index)
            : base(index) {
        }

        public override int ConsumedStack { get { return 1; } }
        public override int Run(InterpretedFrame frame) {
            frame.Data[_index] = frame.Data[--frame.StackIndex];
            //frame.Data[_index] = frame.Pop();
            return +1;
        }

        public Instruction BoxIfIndexMatches(int index) {
            return (index == _index) ? Instruction.SetBoxedLocalVoid(index) : null;
        }

        internal override ParameterExpression GetExpression(LightCompiler compiler) {
            return compiler.Locals[_index];
        }
    }

    public sealed class SetBoxedLocalInstruction : LocalAccessInstruction {
        internal SetBoxedLocalInstruction(int index)
            : base(index) {
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            var box = (StrongBox<object>)frame.Data[_index];
            box.Value = frame.Peek();
            return +1;
        }

        internal override ParameterExpression GetExpression(LightCompiler compiler) {
            return compiler.Locals[_index];
        }
    }

    public sealed class SetBoxedLocalVoidInstruction : LocalAccessInstruction {
        internal SetBoxedLocalVoidInstruction(int index)
            : base(index) {
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 0; } }

        public override int Run(InterpretedFrame frame) {
            var box = (StrongBox<object>)frame.Data[_index];
            box.Value = frame.Data[--frame.StackIndex];
            return +1;
        }

        internal override ParameterExpression GetExpression(LightCompiler compiler) {
            return compiler.Locals[_index];
        }
    }

    public sealed class SetClosureInstruction : LocalAccessInstruction {
        internal SetClosureInstruction(int index)
            : base(index) {
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        public override int Run(InterpretedFrame frame) {
            var box = frame.Closure[_index];
            box.Value = frame.Peek();
            return +1;
        }

        internal override ParameterExpression GetExpression(LightCompiler compiler) {
            return compiler.ClosureVariables[_index];
        }
    }

    #endregion

    #region Initialize

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1012:AbstractTypesShouldNotHaveConstructors")]
    public abstract class InitializeLocalInstruction : LocalAccessInstruction {
        internal InitializeLocalInstruction(int index)
            : base(index) {
        }

        internal sealed class Reference : InitializeLocalInstruction, IBoxableInstruction {
            internal Reference(int index)
                : base(index) {
            }

            public override int Run(InterpretedFrame frame) {
                // nop
                return 1;
            }

            public Instruction BoxIfIndexMatches(int index) {
                return (index == _index) ? InitImmutableRefBox(index) : null;
            }

            public override string InstructionName {
                get { return "InitRef"; }
            }
        }

        internal sealed class ImmutableValue : InitializeLocalInstruction, IBoxableInstruction {
            private readonly object _defaultValue;

            internal ImmutableValue(int index, object defaultValue)
                : base(index) {
                _defaultValue = defaultValue;
            }

            public override int Run(InterpretedFrame frame) {
                frame.Data[_index] = _defaultValue;
                return 1;
            }

            public Instruction BoxIfIndexMatches(int index) {
                return (index == _index) ? new ImmutableBox(index, _defaultValue) : null;
            }

            public override string InstructionName {
                get { return "InitImmutableValue"; }
            }
        }

        internal sealed class ImmutableBox : InitializeLocalInstruction {
            // immutable value:
            private readonly object _defaultValue;

            internal ImmutableBox(int index, object defaultValue)
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

        internal sealed class MutableValue : InitializeLocalInstruction, IBoxableInstruction {
            private readonly Type _type;

            internal MutableValue(int index, Type type)
                : base(index) {
                _type = type;
            }

            public override int Run(InterpretedFrame frame) {
                try {
                    frame.Data[_index] = Activator.CreateInstance(_type);
                } catch (TargetInvocationException e) {
                    ExceptionHelpers.UpdateForRethrow(e.InnerException);
                    throw e.InnerException;
                }

                return 1;
            }

            public Instruction BoxIfIndexMatches(int index) {
                return (index == _index) ? new MutableBox(index, _type) : null;
            }

            public override string InstructionName {
                get { return "InitMutableValue"; }
            }
        }

        internal sealed class MutableBox : InitializeLocalInstruction {
            private readonly Type _type;

            internal MutableBox(int index, Type type)
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

        internal override ParameterExpression GetExpression(LightCompiler compiler) {
            return compiler.Locals[_index];
        }
    }

    #endregion

    #region Factories

    public partial class Instruction {
        private const int LocalInstrCacheSize = 32;

        private static Instruction[] _getLocal;
        private static Instruction[] _getBoxedLocal;
        private static Instruction[] _getClosure;
        private static Instruction[] _getBoxedClosure;
        private static Instruction[] _setLocal;
        private static Instruction[] _setLocalVoid;
        private static Instruction[] _setBoxedLocal;
        private static Instruction[] _setBoxedLocalVoid;
        private static Instruction[] _setClosure;
        private static Instruction[] _initReference;
        private static Instruction[] _initImmutableRefBox;

        public static Instruction GetLocal(int index) {
            if (_getLocal == null) {
                _getLocal = new Instruction[LocalInstrCacheSize];
            }

            if (index < _getLocal.Length) {
                return _getLocal[index] ?? (_getLocal[index] = new GetLocalInstruction(index));
            } 

            return new GetLocalInstruction(index);
        }

        public static Instruction GetBoxedLocal(int index) {
            if (_getBoxedLocal == null) {
                _getBoxedLocal = new Instruction[LocalInstrCacheSize];
            }

            if (index < _getBoxedLocal.Length) {
                return _getBoxedLocal[index] ?? (_getBoxedLocal[index] = new GetBoxedLocalInstruction(index));
            } 

            return new GetBoxedLocalInstruction(index);
        }

        public static Instruction GetClosure(int index) {
            if (_getClosure == null) {
                _getClosure = new Instruction[LocalInstrCacheSize];
            }

            if (index < _getClosure.Length) {
                return _getClosure[index] ?? (_getClosure[index] = new GetClosureInstruction(index));
            }

            return new GetClosureInstruction(index);
        }

        public static Instruction GetBoxedClosure(int index) {
            if (_getBoxedClosure == null) {
                _getBoxedClosure = new Instruction[LocalInstrCacheSize];
            }

            if (index < _getBoxedClosure.Length) {
                return _getBoxedClosure[index] ?? (_getBoxedClosure[index] = new GetBoxedClosureInstruction(index));
            }

            return new GetBoxedClosureInstruction(index);
        }

        public static Instruction SetLocal(int index) {
            if (_setLocal == null) {
                _setLocal = new Instruction[LocalInstrCacheSize];
            }

            if (index < _setLocal.Length) {
                return _setLocal[index] ?? (_setLocal[index] = new SetLocalInstruction(index));
            } 

            return new SetLocalInstruction(index);
        }

        public static Instruction SetLocalVoid(int index) {
            if (_setLocalVoid == null) {
                _setLocalVoid = new Instruction[LocalInstrCacheSize];
            }

            if (index < _setLocalVoid.Length) {
                return _setLocalVoid[index] ?? (_setLocalVoid[index] = new SetLocalVoidInstruction(index));
            }

            return new SetLocalVoidInstruction(index);
        }

        public static Instruction SetBoxedLocal(int index) {
            if (_setBoxedLocal == null) {
                _setBoxedLocal = new Instruction[LocalInstrCacheSize];
            }

            if (index < _setBoxedLocal.Length) {
                return _setBoxedLocal[index] ?? (_setBoxedLocal[index] = new SetBoxedLocalInstruction(index));
            } 

            return new SetBoxedLocalInstruction(index);
        }

        public static Instruction SetBoxedLocalVoid(int index) {
            if (_setBoxedLocalVoid == null) {
                _setBoxedLocalVoid = new Instruction[LocalInstrCacheSize];
            }

            if (index < _setBoxedLocalVoid.Length) {
                return _setBoxedLocalVoid[index] ?? (_setBoxedLocalVoid[index] = new SetBoxedLocalVoidInstruction(index));
            }

            return new SetBoxedLocalVoidInstruction(index);
        }

        public static Instruction SetClosure(int index) {
            if (_setClosure == null) {
                _setClosure = new Instruction[LocalInstrCacheSize];
            }

            if (index < _setClosure.Length) {
                return _setClosure[index] ?? (_setClosure[index] = new SetClosureInstruction(index));
            }

            return new SetClosureInstruction(index);
        }

        public static Instruction InitializeLocal(int index, Type type) {
            object value = LightCompiler.GetImmutableDefaultValue(type);
            if (value != null) {
                return new InitializeLocalInstruction.ImmutableValue(index, value);
            } else if (type.IsValueType) {
                return new InitializeLocalInstruction.MutableValue(index, type);
            } else {
                return InitReference(index);
            }
        }

        private static Instruction InitReference(int index) {
            if (_initReference == null) {
                _initReference = new Instruction[LocalInstrCacheSize];
            }

            if (index < _initReference.Length) {
                return _initReference[index] ?? (_initReference[index] = new InitializeLocalInstruction.Reference(index));
            }

            return new InitializeLocalInstruction.Reference(index);
        }

        internal static Instruction InitImmutableRefBox(int index) {
            if (_initImmutableRefBox == null) {
                _initImmutableRefBox = new Instruction[LocalInstrCacheSize];
            }

            if (index < _initImmutableRefBox.Length) {
                return _initImmutableRefBox[index] ?? (_initImmutableRefBox[index] = new InitializeLocalInstruction.ImmutableBox(index, null));
            }

            return new InitializeLocalInstruction.ImmutableBox(index, null);
        }
    }

    #endregion
}
