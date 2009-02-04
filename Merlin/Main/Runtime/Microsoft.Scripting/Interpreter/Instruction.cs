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

namespace Microsoft.Scripting.Interpreter {

    public interface IInstructionProvider {
        Instruction GetInstruction(LightCompiler compiler);
    }

    public abstract class Instruction {
        public virtual int ConsumedStack { get { return 0; } }
        public virtual int ProducedStack { get { return 0; } }

        public abstract int Run(StackFrame frame);
    }

    #region Basic Stack Operations
    public class PushInstruction : Instruction {
        private object _value;
        public PushInstruction(object value) {
            this._value = value;
        }

        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            frame.Data[frame.StackIndex++] = _value;
            return +1;
        }

        public override string ToString() {
            return "Push(" + _value + ")";
        }
    }

    public class PopInstruction : Instruction {
        public static PopInstruction Instance = new PopInstruction();

        private PopInstruction() { }

        public override int ConsumedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            frame.Pop();
            return +1;
        }
    }
    #endregion

    #region variable gets and sets

    public interface IBoxableInstruction {
        Instruction BoxIfIndexMatches(int index);
    }

    public class GetLocalInstruction : Instruction, IBoxableInstruction {
        private int _index;
        public GetLocalInstruction(int index) {
            this._index = index;
        }

        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            frame.Data[frame.StackIndex++] = frame.Data[_index];
            //frame.Push(frame.Data[_index]);
            return +1;
        }

        public override string ToString() {
            return "GetLocal(" + _index + ")";
        }

        public Instruction BoxIfIndexMatches(int index) {
            if (index == _index) return new GetBoxedLocalInstruction(index);
            else return null;
        }
    }

    public class GetBoxedLocalInstruction : Instruction {
        private int _index;
        public GetBoxedLocalInstruction(int index) {
            this._index = index;
        }

        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            var box = (StrongBox<object>)frame.Data[_index];
            frame.Data[frame.StackIndex++] = box.Value;
            return +1;
        }

        public override string ToString() {
            return "GetBoxedLocal(" + _index + ")";
        }
    }

    public class GetClosureInstruction : Instruction {
        private int _index;
        public GetClosureInstruction(int index) {
            this._index = index;
        }

        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            var box = frame.Closure[_index];
            frame.Data[frame.StackIndex++] = box.Value;
            return +1;
        }

        public override string ToString() {
            return "GetClosure(" + _index + ")";
        }
    }

    public class GetBoxedClosureInstruction : Instruction {
        private int _index;
        public GetBoxedClosureInstruction(int index) {
            this._index = index;
        }

        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            var box = frame.Closure[_index];
            frame.Data[frame.StackIndex++] = box;
            return +1;
        }

        public override string ToString() {
            return "GetBoxedClosure(" + _index + ")";
        }
    }

    public class SetLocalInstruction : Instruction, IBoxableInstruction {
        private int _index;
        public SetLocalInstruction(int index) {
            this._index = index;
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            frame.Data[_index] = frame.Peek();
            return +1;
        }

        public override string ToString() {
            return "SetLocal(" + _index + ")";
        }

        public Instruction BoxIfIndexMatches(int index) {
            if (index == _index) return new SetBoxedLocalInstruction(index);
            else return null;
        }
    }

    public class SetBoxedLocalInstruction : Instruction {
        private int _index;
        public SetBoxedLocalInstruction(int index) {
            this._index = index;
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            var box = (StrongBox<object>)frame.Data[_index];
            box.Value = frame.Peek();
            return +1;
        }

        public override string ToString() {
            return "SetBoxedLocal(" + _index + ")";
        }
    }

    public class SetBoxedLocalVoidInstruction : Instruction {
        private int _index;
        public SetBoxedLocalVoidInstruction(int index) {
            this._index = index;
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 0; } }
        public override int Run(StackFrame frame) {
            var box = (StrongBox<object>)frame.Data[_index];
            box.Value = frame.Data[--frame.StackIndex];
            return +1;
        }

        public override string ToString() {
            return "SetBoxedLocal(" + _index + ")";
        }
    }

    public class SetClosureInstruction : Instruction {
        private int _index;
        public SetClosureInstruction(int index) {
            this._index = index;
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            var box = frame.Closure[_index];
            box.Value = frame.Peek();
            return +1;
        }

        public override string ToString() {
            return "SetClosure(" + _index + ")";
        }
    }

    public class SetLocalVoidInstruction : Instruction, IBoxableInstruction {
        private int _index;
        public SetLocalVoidInstruction(int index) {
            this._index = index;
        }

        public override int ConsumedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            frame.Data[_index] = frame.Data[--frame.StackIndex];
            //frame.Data[_index] = frame.Pop();
            return +1;
        }

        public override string ToString() {
            return "SetLocalVoid(" + _index + ")";
        }

        public Instruction BoxIfIndexMatches(int index) {
            if (index == _index) return new SetBoxedLocalVoidInstruction(index);
            else return null;
        }
    }

    public class GetGlobalInstruction : Instruction {
        private ModuleGlobalWrapper _global;
        public GetGlobalInstruction(ModuleGlobalWrapper global) {
            this._global = global;
        }

        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            frame.Data[frame.StackIndex++] = _global.CurrentValue;
            return +1;
        }

        public override string ToString() {
            return "GetGlobal(" + _global + ")";
        }
    }

    public class SetGlobalInstruction : Instruction {
        private ModuleGlobalWrapper _global;
        public SetGlobalInstruction(ModuleGlobalWrapper global) {
            this._global = global;
        }

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            _global.CurrentValue = frame.Peek();
            return +1;
        }

        public override string ToString() {
            return "SetGlobal(" + _global + ")";
        }
    }

    public class LookupNameInstruction : Instruction {
        internal SymbolId _symbol;
        public LookupNameInstruction(SymbolId symbol) {
            this._symbol = symbol;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            var context = (CodeContext)frame.Pop();
            frame.Push(LookupName(context));
            return +1;
        }

        protected virtual object LookupName(CodeContext context) {
            return context.LanguageContext.LookupName(context.Scope, _symbol);
        }

        public override string ToString() {
            return "LookupName(" + _symbol + ")";
        }
    }

    public class LookupGlobalNameInstruction : LookupNameInstruction {
        public LookupGlobalNameInstruction(SymbolId symbol)
            : base(symbol) {
        }

        protected override object LookupName(CodeContext context) {
            return context.LanguageContext.LookupName(context.GlobalScope, _symbol);
        }

        public override string ToString() {
            return "LookupGlobalName(" + _symbol + ")";
        }
    }

    public class SetNameInstruction : Instruction {
        internal SymbolId _symbol;
        public SetNameInstruction(SymbolId symbol) {
            this._symbol = symbol;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return 2; } }
        public override int Run(StackFrame frame) {
            var value = frame.Pop();
            var context = (CodeContext)frame.Pop();
            frame.Push(value);
            SetName(context, value);
            return +1;
        }

        protected virtual void SetName(CodeContext context, object value) {
            context.LanguageContext.SetName(context.Scope, _symbol, value);
        }

        public override string ToString() {
            return "SetName(" + _symbol + ")";
        }
    }

    public class SetGlobalNameInstruction : SetNameInstruction {
        public SetGlobalNameInstruction(SymbolId symbol)
            : base(symbol) {
        }

        protected override void SetName(CodeContext context, object value) {
            context.LanguageContext.SetName(context.GlobalScope, _symbol, value);
        }

        public override string ToString() {
            return "SetGlobalName(" + _symbol + ")";
        }
    }
    #endregion

    #region Branches
    public abstract class OffsetInstruction : Instruction {
        protected int offset;
        public void SetOffset(int offset) {
            this.offset = offset;
        }

        public override string ToString() {
            return this.GetType().Name + "(" + offset + ")";
        }
    }

    public class BranchFalseInstruction : OffsetInstruction {
        public BranchFalseInstruction() { }

        public override int ConsumedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            if (!(bool)frame.Pop()) {
                return offset;
            }
            return +1;
        }
    }

    public class BranchTrueInstruction : OffsetInstruction {
        public BranchTrueInstruction() { }

        public override int ConsumedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            if ((bool)frame.Pop()) {
                return offset;
            }
            return +1;
        }
    }

    public class BranchInstruction : OffsetInstruction {
        public BranchInstruction() { }

        public override int Run(StackFrame frame) {
            return offset;
        }
    }

    public class SwitchInstruction : Instruction {
        //TODO this is probably much more efficient as an int[] for simple cases
        private Dictionary<int, int> _cases = new Dictionary<int, int>();
        private int _defaultOffset;

        public SwitchInstruction() { }
        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 0; } } //???
        public override int Run(StackFrame frame) {
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

    public class ThrowInstruction : Instruction {
        public static ThrowInstruction Instance = new ThrowInstruction();

        private ThrowInstruction() { }

        public override int ConsumedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            var exception = (Exception)frame.Pop();
            throw exception;
        }
    }

    public class RethrowInstruction : Instruction {
        public static RethrowInstruction Instance = new RethrowInstruction();

        private RethrowInstruction() { }

        public override int ConsumedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            var exception = (Exception)frame.Pop();
            ExceptionHelpers.UpdateForRethrow(exception);
            throw exception;
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

        public override int Run(StackFrame frame) {
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
        public override int Run(StackFrame frame) {
            StrongBox<object>[] closure = new StrongBox<object>[ConsumedStack];
            for (int i = closure.Length - 1; i >= 0; i-- ) {
                closure[i] = (StrongBox<object>)frame.Pop();
            }

            Delegate d = _creator.CreateDelegate(closure);

            frame.Push(d);
            return +1;
        }
    }

    public class NewArrayInstruction : Instruction {
        private Type _elementType;
        private int _elementCount;
        public NewArrayInstruction(Type elementType, int elementCount) {
            this._elementType = elementType;
            this._elementCount = elementCount;
        }

        public override int ConsumedStack { get { return _elementCount; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            var array = Array.CreateInstance(_elementType, _elementCount);
            for (int i = _elementCount - 1; i >= 0; i--) {
                array.SetValue(frame.Pop(), i);
            }
            frame.Push(array);
            return +1;
        }
    }

    public class NewInstruction : Instruction {
        private ConstructorInfo _constructor;
        private int _argCount;

        public NewInstruction(ConstructorInfo constructor) {
            this._constructor = constructor;
            this._argCount = constructor.GetParameters().Length;

        }
        public override int ConsumedStack { get { return _argCount; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            object[] args = new object[_argCount];
            for (int i = _argCount - 1; i >= 0; i--) {
                args[i] = frame.Pop();
            }

            object ret = _constructor.Invoke(args);
            frame.Push(ret);
            return +1;
        }
    }

    public class ArrayIndexInstruction<T> : Instruction {
        public static readonly Instruction Instance = new ArrayIndexInstruction<T>();

        private ArrayIndexInstruction() { }
        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            int index = (int)frame.Pop();
            T[] array = (T[])frame.Pop();
            frame.Push(array[index]);
            return +1;
        }
    }

    public class NotInstruction : Instruction {
        public static readonly Instruction Instance = new NotInstruction();

        private NotInstruction() { }
        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            frame.Push(!(bool)frame.Pop());
            return +1;
        }
    }

    public class EqualBoolInstruction : Instruction {
        public static readonly Instruction Instance = new EqualBoolInstruction();

        private EqualBoolInstruction() { }
        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            frame.Push(((bool)frame.Pop()) == ((bool)frame.Pop()));
            return +1;
        }
    }

    public class EqualIntInstruction : Instruction {
        public static readonly Instruction Instance = new EqualIntInstruction();

        private EqualIntInstruction() { }
        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            frame.Push(((int)frame.Pop()) == ((int)frame.Pop()));
            return +1;
        }
    }

    public class NotEqualIntInstruction : Instruction {
        public static readonly Instruction Instance = new NotEqualIntInstruction();

        private NotEqualIntInstruction() { }
        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            frame.Push(((int)frame.Pop()) != ((int)frame.Pop()));
            return +1;
        }
    }

    public class NotEqualObjectInstruction : Instruction {
        public static readonly Instruction Instance = new NotEqualObjectInstruction();

        private NotEqualObjectInstruction() { }
        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }
        public override int Run(StackFrame frame) {
            frame.Push(frame.Pop() != frame.Pop());
            return +1;
        }
    }
    #endregion

    public class RuntimeVariablesInstruction : Instruction {
        private int _count;
        public RuntimeVariablesInstruction(int count) {
            this._count = count;
        }

        public override int ProducedStack { get { return 1; } }
        public override int ConsumedStack { get { return _count; } }
        public override int Run(StackFrame frame) {
            var ret = new IStrongBox[_count];
            for (int i = ret.Length - 1; i >= 0; i--) {
                ret[i] = (IStrongBox)frame.Pop();
            }
            frame.Push(ret);
            return +1;
        }
    }
}
