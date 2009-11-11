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
// Enables instruction counting and displaying stats at process exit.
// #define STATS

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Scripting.Interpreter {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    [DebuggerTypeProxy(typeof(InstructionArray.DebugView))]
    public struct InstructionArray {
        internal readonly Instruction[] Instructions;
        internal readonly object[] Objects;

        // list of (instruction index, cookie) sorted by instruction index:
        internal readonly List<KeyValuePair<int, object>> DebugCookies;

        internal InstructionArray(Instruction[] instructions, object[] objects, List<KeyValuePair<int, object>> debugCookies) {
            Instructions = instructions;
            DebugCookies = debugCookies;
            Objects = objects;
        }

        internal int Length {
            get { return Instructions.Length; }
        }

        #region Debug View

        internal sealed class DebugView {
            private readonly InstructionArray _array;

            public DebugView(InstructionArray array) {
                _array = array;

            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public InstructionList.DebugView.InstructionView[]/*!*/ A0 {
                get {
                    return InstructionList.DebugView.GetInstructionViews(_array.Instructions, _array.Objects, _array.DebugCookies);
                }
            }
        }

        #endregion
    }

    [DebuggerTypeProxy(typeof(InstructionList.DebugView))]
    public sealed class InstructionList {
        private readonly List<Instruction> _instructions = new List<Instruction>();
        private List<object> _objects;

        private int _currentStackDepth;
        private int _maxStackDepth;
        
        // list of (instruction index, cookie) sorted by instruction index:
        private List<KeyValuePair<int, object>> _debugCookies = null;

        #region Debug View

        internal sealed class DebugView {
            private readonly InstructionList _list;

            public DebugView(InstructionList list) {
                _list = list;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public InstructionView[]/*!*/ A0 {
                get {
                    return GetInstructionViews(_list._instructions, _list._objects, _list._debugCookies);
                }
            }

            internal static InstructionView[] GetInstructionViews(IList<Instruction> instructions, IList<object> objects,
                IList<KeyValuePair<int, object>> debugCookies) {

                var result = new List<InstructionView>();
                int index = 0;
                int stackDepth = 0;

                var cookieEnumerator = (debugCookies != null ? debugCookies : new KeyValuePair<int, object>[0]).GetEnumerator();
                var hasCookie = cookieEnumerator.MoveNext();

                for (int i = 0; i < instructions.Count; i++) {
                    object cookie = null;
                    while (hasCookie && cookieEnumerator.Current.Key == i) {
                        cookie = cookieEnumerator.Current.Value;
                        hasCookie = cookieEnumerator.MoveNext();
                    }

                    int stackDiff = instructions[i].StackBalance;
                    string name = instructions[i].ToDebugString(cookie, objects);
                    result.Add(new InstructionView(instructions[i], name, i, stackDepth));
                    
                    index++;
                    stackDepth += stackDiff;
                }
                return result.ToArray();
            }

            [DebuggerDisplay("{GetValue(),nq}", Name = "{GetName(),nq}", Type = "{GetDisplayType(), nq}")]
            internal struct InstructionView {
                private readonly int _index;
                private readonly int _stackDepth;
                private readonly string _name;
                private readonly Instruction _instruction;

                internal string GetName() {
                    return _index.ToString() + (_stackDepth == 0 ? "" : " D(" + _stackDepth.ToString() + ")");
                }

                internal string GetValue() {
                    return _name;
                }

                internal string GetDisplayType() {
                    return _instruction.StackBalance.ToString();
                }

                public InstructionView(Instruction instruction, string name, int index, int stackDepth) {
                    _instruction = instruction;
                    _name = name;
                    _index = index;
                    _stackDepth = stackDepth;
                }
            }
        }
                        
        #endregion

        #region Core Emit Ops

        public void Emit(Instruction instruction) {
            _instructions.Add(instruction);
            UpdateStackDepth(instruction.ConsumedStack, instruction.ProducedStack);
        }

        private void UpdateStackDepth(int consumed, int produced) {
            Debug.Assert(consumed >= 0 && produced >= 0);

            _currentStackDepth -= consumed;
            Debug.Assert(_currentStackDepth >= 0); // checks that there's enough room to pop
            _currentStackDepth += produced;
            if (_currentStackDepth > _maxStackDepth) {
                _maxStackDepth = _currentStackDepth;
            }
        }

        /// <summary>
        /// Attaches a cookie to the last emitted instruction.
        /// </summary>
        [Conditional("DEBUG")]
        public void SetDebugCookie(object cookie) {
#if DEBUG
            if (_debugCookies == null) {
                _debugCookies = new List<KeyValuePair<int, object>>();
            }

            Debug.Assert(Count > 0);
            _debugCookies.Add(new KeyValuePair<int, object>(Count - 1, cookie));
#endif
        }

        public int Count {
            get { return _instructions.Count; }
        }

        public int CurrentStackDepth {
            get { return _currentStackDepth; }
        }

        public int MaxStackDepth {
            get { return _maxStackDepth; }
        }

#if STATS
        private static Dictionary<string, int> _executedInstructions = new Dictionary<string, int>();
        private static Dictionary<string, Dictionary<object, bool>> _instances = new Dictionary<string, Dictionary<object, bool>>();

        static InstructionList() {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler((_, __) => {
                PerfTrack.DumpHistogram(_executedInstructions);
                Console.WriteLine("-- Total executed: {0}", _executedInstructions.Values.Aggregate(0, (sum, value) => sum + value));
                Console.WriteLine("-----");

                var referenced = new Dictionary<string, int>();
                int total = 0;
                foreach (var entry in _instances) {
                    referenced[entry.Key] = entry.Value.Count;
                    total += entry.Value.Count;
                }

                PerfTrack.DumpHistogram(referenced);
                Console.WriteLine("-- Total referenced: {0}", total);
                Console.WriteLine("-----");
            });
        }
#endif
        public InstructionArray ToArray() {
#if STATS
            lock (_executedInstructions) {
                _instructions.ForEach((instr) => {
                    int value = 0;
                    var name = instr.GetType().Name;
                    _executedInstructions.TryGetValue(name, out value);
                    _executedInstructions[name] = value + 1;

                    Dictionary<object, bool> dict;
                    if (!_instances.TryGetValue(name, out dict)) {
                        _instances[name] = dict = new Dictionary<object, bool>();
                    }
                    dict[instr] = true;
                });
            }
#endif
            return new InstructionArray(
                _instructions.ToArray(), 
                (_objects != null) ? _objects.ToArray() : null,
                _debugCookies
            );
        }

        #endregion

        #region Stack Operations

        private const int PushIntMinCachedValue = -100;
        private const int PushIntMaxCachedValue = 100;
        private const int CachedObjectCount = 256;

        private static Instruction _null;
        private static Instruction _true;
        private static Instruction _false;
        private static Instruction[] _ints;
        private static Instruction[] _loadObjectCached;

        public void EmitLoad(object value) {
            EmitLoad(value, null);
        }

        public void EmitLoad(bool value) {
            if ((bool)value) {
                Emit(_true ?? (_true = new LoadObjectInstruction(value)));
            } else {
                Emit(_false ?? (_false = new LoadObjectInstruction(value)));
            }
        }

        public void EmitLoad(object value, Type type) {
            if (value == null) {
                Emit(_null ?? (_null = new LoadObjectInstruction(null)));
                return;
            }

            if (type == null || type.IsValueType) {
                if (value is bool) {
                    EmitLoad((bool)value);
                    return;
                } 
                
                if (value is int) {
                    int i = (int)value;
                    if (i >= PushIntMinCachedValue && i <= PushIntMaxCachedValue) {
                        if (_ints == null) {
                            _ints = new Instruction[PushIntMaxCachedValue - PushIntMinCachedValue + 1];
                        }
                        i -= PushIntMinCachedValue;
                        Emit(_ints[i] ?? (_ints[i] = new LoadObjectInstruction(value)));
                        return;
                    }
                }
            }

            if (_objects == null) {
                _objects = new List<object>();
                if (_loadObjectCached == null) {
                    _loadObjectCached = new Instruction[CachedObjectCount];
                }
            }

            if (_objects.Count < _loadObjectCached.Length) {
                uint index = (uint)_objects.Count;
                _objects.Add(value);
                Emit(_loadObjectCached[index] ?? (_loadObjectCached[index] = new LoadCachedObjectInstruction(index)));
            } else {
                Emit(new LoadObjectInstruction(value));
            }
        }

        public void EmitDup() {
            Emit(DupInstruction.Instance);
        }

        public void EmitPop() {
            Emit(PopInstruction.Instance);
        }

        #endregion

        #region Locals

        internal void SwitchToBoxed(int index) {
            for (int i = 0; i < _instructions.Count; i++) {
                var instruction = _instructions[i] as IBoxableInstruction;

                if (instruction != null) {
                    var newInstruction = instruction.BoxIfIndexMatches(index);
                    if (newInstruction != null) {
                        _instructions[i] = newInstruction;
                    }
                }
            }
        }

        private const int LocalInstrCacheSize = 64;

        private static Instruction[] _loadLocal;
        private static Instruction[] _loadLocalBoxed;
        private static Instruction[] _loadLocalFromClosure;
        private static Instruction[] _loadLocalFromClosureBoxed;
        private static Instruction[] _assignLocal;
        private static Instruction[] _storeLocal;
        private static Instruction[] _assignLocalBoxed;
        private static Instruction[] _storeLocalBoxed;
        private static Instruction[] _assignLocalToClosure;
        private static Instruction[] _initReference;
        private static Instruction[] _initImmutableRefBox;

        public void EmitLoadLocal(int index) {
            if (_loadLocal == null) {
                _loadLocal = new Instruction[LocalInstrCacheSize];
            }

            if (index < _loadLocal.Length) {
                Emit(_loadLocal[index] ?? (_loadLocal[index] = new LoadLocalInstruction(index)));
            } else {
                Emit(new LoadLocalInstruction(index));
            }
        }

        public void EmitLoadLocalBoxed(int index) {
            Emit(GetBoxedLocal(index));
        }

        internal static Instruction GetBoxedLocal(int index) {
            if (_loadLocalBoxed == null) {
                _loadLocalBoxed = new Instruction[LocalInstrCacheSize];
            }

            if (index < _loadLocalBoxed.Length) {
                return _loadLocalBoxed[index] ?? (_loadLocalBoxed[index] = new GetBoxedLocalInstruction(index));
            } else {
                return new GetBoxedLocalInstruction(index);
            }
        }

        public void EmitLoadLocalFromClosure(int index) {
            if (_loadLocalFromClosure == null) {
                _loadLocalFromClosure = new Instruction[LocalInstrCacheSize];
            }

            if (index < _loadLocalFromClosure.Length) {
                Emit(_loadLocalFromClosure[index] ?? (_loadLocalFromClosure[index] = new GetClosureInstruction(index)));
            } else {
                Emit(new GetClosureInstruction(index));
            }
        }

        public void EmitLoadLocalFromClosureBoxed(int index) {
            if (_loadLocalFromClosureBoxed == null) {
                _loadLocalFromClosureBoxed = new Instruction[LocalInstrCacheSize];
            }

            if (index < _loadLocalFromClosureBoxed.Length) {
                Emit(_loadLocalFromClosureBoxed[index] ?? (_loadLocalFromClosureBoxed[index] = new LoadLocalFromClosureBoxedInstruction(index)));
            } else {
                Emit(new LoadLocalFromClosureBoxedInstruction(index));
            }
        }

        public void EmitAssignLocal(int index) {
            if (_assignLocal == null) {
                _assignLocal = new Instruction[LocalInstrCacheSize];
            }

            if (index < _assignLocal.Length) {
                Emit(_assignLocal[index] ?? (_assignLocal[index] = new AssignLocalInstruction(index)));
            } else {
                Emit(new AssignLocalInstruction(index));
            }
        }

        public void EmitStoreLocal(int index) {
            if (_storeLocal == null) {
                _storeLocal = new Instruction[LocalInstrCacheSize];
            }

            if (index < _storeLocal.Length) {
                Emit(_storeLocal[index] ?? (_storeLocal[index] = new StoreLocalInstruction(index)));
            } else {
                Emit(new StoreLocalInstruction(index));
            }
        }

        public void EmitAssignedLocalBoxed(int index) {
            Emit(AssignLocalBoxed(index));
        }

        internal static Instruction AssignLocalBoxed(int index) {
            if (_assignLocalBoxed == null) {
                _assignLocalBoxed = new Instruction[LocalInstrCacheSize];
            }

            if (index < _assignLocalBoxed.Length) {
                return _assignLocalBoxed[index] ?? (_assignLocalBoxed[index] = new AssignLocalBoxedInstruction(index));
            } else {
                return new AssignLocalBoxedInstruction(index);
            }
        }

        public void EmitStoreLocalBoxed(int index) {
            Emit(StoreLocalBoxed(index));
        }

        internal static Instruction StoreLocalBoxed(int index) {
            if (_storeLocalBoxed == null) {
                _storeLocalBoxed = new Instruction[LocalInstrCacheSize];
            }

            if (index < _storeLocalBoxed.Length) {
                return _storeLocalBoxed[index] ?? (_storeLocalBoxed[index] = new StoreLocalBoxedInstruction(index));
            } else {
                return new StoreLocalBoxedInstruction(index);
            }
        }

        public void EmitAssignLocalToClosure(int index) {
            if (_assignLocalToClosure == null) {
                _assignLocalToClosure = new Instruction[LocalInstrCacheSize];
            }

            if (index < _assignLocalToClosure.Length) {
                Emit(_assignLocalToClosure[index] ?? (_assignLocalToClosure[index] = new AssignLocalToClosureInstruction(index)));
            } else {
                Emit(new AssignLocalToClosureInstruction(index));
            }
        }

        public void EmitInitializeLocal(int index, Type type) {
            object value = LightCompiler.GetImmutableDefaultValue(type);
            if (value != null) {
                Emit(new InitializeLocalInstruction.ImmutableValue(index, value));
            } else if (type.IsValueType) {
                Emit(new InitializeLocalInstruction.MutableValue(index, type));
            } else {
                Emit(InitReference(index));
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

        public void EmitNewRuntimeVariables(int count) {
            Emit(new RuntimeVariablesInstruction(count));
        }

        #endregion

        #region Array Operations

        public void EmitGetArrayItem(Type arrayType) {
            Type elementType = arrayType.GetElementType();
            if (elementType.IsClass || elementType.IsInterface) {
                Emit(InstructionFactory<object>.Factory.GetArrayItem());
            } else {
                Emit(InstructionFactory.GetFactory(elementType).GetArrayItem());
            }
        }

        public void EmitSetArrayItem(Type arrayType) {
            Type elementType = arrayType.GetElementType();
            if (elementType.IsClass || elementType.IsInterface) {
                Emit(InstructionFactory<object>.Factory.SetArrayItem());
            } else {
                Emit(InstructionFactory.GetFactory(elementType).SetArrayItem());
            }
        }

        public void EmitNewArray(Type elementType) {
            Emit(InstructionFactory.GetFactory(elementType).NewArray());
        }

        public void EmitNewArrayBounds(Type elementType, int rank) {
            Emit(new NewArrayBoundsInstruction(elementType, rank));
        }

        public void EmitNewArrayInit(Type elementType, int elementCount) {
            Emit(InstructionFactory.GetFactory(elementType).NewArrayInit(elementCount));
        }

        #endregion

        #region Arithmetic Operations

        public void EmitAdd(Type type, bool @checked) {
            if (@checked) {
                Emit(AddOvfInstruction.Create(type));
            } else {
                Emit(AddInstruction.Create(type));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public void EmitSub(Type type, bool @checked) {
            throw new NotSupportedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public void EmitMul(Type type, bool @checked) {
            throw new NotSupportedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public void EmitDiv(Type type) {
            throw new NotSupportedException();
        }

        #endregion

        #region Comparisons

        public void EmitEqual(Type type) {
            Emit(EqualInstruction.Create(type));
        }

        public void EmitNotEqual(Type type) {
            Emit(NotEqualInstruction.Create(type));
        }

        public void EmitLessThan(Type type) {
            Emit(LessThanInstruction.Create(type));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public void EmitLessThanOrEqual(Type type) {
            throw new NotSupportedException();
        }

        public void EmitGreaterThan(Type type) {
            Emit(GreaterThanInstruction.Create(type));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public void EmitGreaterThanOrEqual(Type type) {
            throw new NotSupportedException();
        }

        #endregion

        #region Conversions

        public void EmitNumericConvertChecked(TypeCode from, TypeCode to) {
            Emit(new NumericConvertInstruction.Checked(from, to));
        }

        public void EmitNumericConvertUnchecked(TypeCode from, TypeCode to) {
            Emit(new NumericConvertInstruction.Unchecked(from, to));
        }

        #endregion

        #region Boolean Operators

        public void EmitNot() {
            Emit(NotInstruction.Instance);
        }

        #endregion

        #region Types

        public void EmitDefaultValue(Type type) {
            Emit(InstructionFactory.GetFactory(type).DefaultValue());
        }

        public void EmitNew(ConstructorInfo constructorInfo) {
            Emit(new NewInstruction(constructorInfo));
        }

        internal void EmitCreateDelegate(LightDelegateCreator creator) {
            Emit(new CreateDelegateInstruction(creator));
        }

        public void EmitTypeEquals() {
            Emit(TypeEqualsInstruction.Instance);
        }

        public void EmitTypeIs(Type type) {
            Emit(InstructionFactory.GetFactory(type).TypeIs());
        }

        #endregion

        #region Fields and Methods

        private static readonly Dictionary<FieldInfo, Instruction> _loadFields = new Dictionary<FieldInfo, Instruction>();

        public void EmitLoadField(FieldInfo field) {
            Emit(GetLoadField(field));
        }

        private Instruction GetLoadField(FieldInfo field) {
            lock (_loadFields) {
                Instruction instruction;
                if (!_loadFields.TryGetValue(field, out instruction)) {
                    if (field.IsStatic) {
                        instruction = new LoadStaticFieldInstruction(field);
                    } else {
                        instruction = new LoadFieldInstruction(field);
                    }
                    _loadFields.Add(field, instruction);
                }
                return instruction;
            }
        }
        
        public void EmitStoreField(FieldInfo field) {
            if (field.IsStatic) {
                Emit(new StoreStaticFieldInstruction(field));
            } else {
                Emit(new StoreFieldInstruction(field));
            }
        }

        public void EmitCall(MethodInfo method) {
            EmitCall(method, method.GetParameters());
        }

        public void EmitCall(MethodInfo method, ParameterInfo[] parameters) {
            Emit(CallInstruction.Create(method, parameters));
        }

        #endregion

        #region Dynamic

        public void EmitDynamic(Type type, CallSiteBinder binder) {
            Emit(CreateDynamicInstruction(type, binder));
        }

        private static Dictionary<Type, Func<CallSiteBinder, Instruction>> _factories =
            new Dictionary<Type, Func<CallSiteBinder, Instruction>>();

        internal static Instruction CreateDynamicInstruction(Type delegateType, CallSiteBinder binder) {
            Func<CallSiteBinder, Instruction> factory;
            lock (_factories) {
                if (!_factories.TryGetValue(delegateType, out factory)) {
                    if (delegateType.GetMethod("Invoke").ReturnType == typeof(void)) {
                        // TODO: We should generally support void returning binders but the only
                        // ones that exist are delete index/member who's perf isn't that critical.
                        return new DynamicInstructionN(delegateType, CallSite.Create(delegateType, binder), true);
                    }

                    Type instructionType = DynamicInstructionN.GetDynamicInstructionType(delegateType);
                    if (instructionType == null) {
                        return new DynamicInstructionN(delegateType, CallSite.Create(delegateType, binder));
                    }

                    factory = (Func<CallSiteBinder, Instruction>)Delegate.CreateDelegate(
                        typeof(Func<CallSiteBinder, Instruction>),
                        instructionType.GetMethod("Factory")
                    );

                    _factories[delegateType] = factory;
                }
            }
            return factory(binder);
        }

        #endregion

        #region Control Flow

        internal void FixupBranch(int branchIndex, int offset, int targetStackDepth) {
            _instructions[branchIndex] = ((OffsetInstruction)_instructions[branchIndex]).Fixup(offset, targetStackDepth);
        }

        public void EmitGoto(BranchLabel label, bool hasResult, bool hasValue) {
            Emit(new GotoInstruction(Count, hasResult, hasValue));
            label.AddBranch(Count - 1);
        }

        private void EmitBranch(OffsetInstruction instruction, BranchLabel label) {
            Emit(instruction);
            label.AddBranch(Count - 1);
        }

        public void EmitBranch(BranchLabel label) {
            EmitBranch(new BranchInstruction(), label);
        }

        public void EmitBranch(BranchLabel label, bool hasResult, bool hasValue) {
            EmitBranch(new BranchInstruction(hasResult, hasValue), label);
        }

        public void EmitCoalescingBranch(BranchLabel leftNotNull) {
            EmitBranch(new CoalescingBranchInstruction(), leftNotNull);
        }

        public void EmitBranchTrue(BranchLabel elseLabel) {
            EmitBranch(new BranchTrueInstruction(), elseLabel);
        }

        public void EmitBranchFalse(BranchLabel elseLabel) {
            EmitBranch(new BranchFalseInstruction(), elseLabel);
        }

        internal bool AddFinally(int gotoInstructionIndex, int tryStart, int finallyStackDepth, int finallyStart, int finallyEnd) {
            return ((GotoInstruction)_instructions[gotoInstructionIndex]).AddFinally(tryStart, finallyStackDepth, finallyStart, finallyEnd);
        }

        public void EmitThrow() {
            Emit(ThrowInstruction.Throw);
        }

        public void EmitThrowVoid() {
            Emit(ThrowInstruction.VoidThrow);
        }

        public void EmitEnterExceptionHandlerNonVoid() {
            Emit(EnterExceptionHandlerInstruction.NonVoid);
        }

        public void EmitEnterExceptionHandlerVoid() {
            Emit(EnterExceptionHandlerInstruction.Void);
        }

        public void EmitLeaveExceptionHandler(bool hasValue, BranchLabel startOfFinally) {
            EmitBranch(new LeaveExceptionHandlerInstruction(hasValue), startOfFinally);
        }

        public void EmitSwitch(Dictionary<int, int> cases) {
            Emit(new SwitchInstruction(cases));
        }

        #endregion
    }
}
