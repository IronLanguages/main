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
using System.Dynamic;
using System.Linq.Expressions;
using System.Threading;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    public class BinderState : IExpressionSerializable {
        private readonly PythonBinder/*!*/ _binder;
        private CodeContext _context;
        private PythonInvokeBinder _invokeNoArgs, _invokeOneArg;
        private Dictionary<CallSignature, PythonInvokeBinder/*!*/> _invokeBinders;
        private Dictionary<string/*!*/, PythonGetMemberBinder/*!*/> _getMemberBinders;
        private Dictionary<string/*!*/, PythonGetMemberBinder/*!*/> _tryGetMemberBinders;
        private Dictionary<string/*!*/, PythonSetMemberBinder/*!*/> _setMemberBinders;
        private Dictionary<string/*!*/, PythonDeleteMemberBinder/*!*/> _deleteMemberBinders;
        private Dictionary<string/*!*/, CompatibilityGetMember/*!*/> _compatGetMember;
        private Dictionary<PythonOperationKind, PythonOperationBinder/*!*/> _operationBinders;
        private Dictionary<ExpressionType, PythonUnaryOperationBinder/*!*/> _unaryBinders;
        private Dictionary<ExpressionType, PythonBinaryOperationBinder/*!*/> _binaryBinders;
        private Dictionary<BinaryOperationRetTypeKey, OperationRetBoolBinder/*!*/> _binaryRetTypeBinders;
        private Dictionary<Type/*!*/, PythonConversionBinder/*!*/>[] _conversionBinders;
        private Dictionary<Type/*!*/, ConvertBinder/*!*/> _explicitCompatConvertBinders;
        private Dictionary<Type/*!*/, ConvertBinder/*!*/> _implicitCompatConvertBinders;
        private Dictionary<Type/*!*/, DynamicMetaObjectBinder/*!*/>[] _convertRetObjectBinders;
        private Dictionary<CallSignature, CreateFallback/*!*/> _createBinders;
        private Dictionary<CallSignature, CompatibilityInvokeBinder/*!*/> _compatInvokeBinders;
        private PythonGetSliceBinder _getSlice;
        private PythonSetSliceBinder _setSlice;
        private PythonDeleteSliceBinder _deleteSlice;
        private PythonGetIndexBinder[] _getIndexBinders;
        private PythonSetIndexBinder[] _setIndexBinders;
        private PythonDeleteIndexBinder[] _deleteIndexBinders;


        public BinderState(PythonBinder/*!*/ binder) {
            Debug.Assert(binder != null);

            _binder = binder;
        }

        public BinderState(PythonBinder/*!*/ binder, CodeContext context) {
            Debug.Assert(binder != null);

            _binder = binder;
            _context = context;
        }

        public CodeContext Context {
            get {
                return _context;
            }
            set {
                _context = value;
            }
        }

        public PythonBinder/*!*/ Binder {
            get {
                return _binder;
            }
        }

        public static BinderState/*!*/ GetBinderState(DynamicMetaObjectBinder/*!*/ action) {
            IPythonSite pySite = action as IPythonSite;
            if (pySite != null) {
                return pySite.Binder;
            }

            return DefaultContext.DefaultPythonContext.DefaultBinderState;
        }

        public static Expression/*!*/ GetCodeContext(DynamicMetaObjectBinder/*!*/ action) {
            return Microsoft.Scripting.Ast.Utils.Constant(BinderState.GetBinderState(action).Context);
        }

        #region Binder Factories

        internal CompatibilityInvokeBinder/*!*/ CompatInvoke(CallInfo /*!*/ callInfo) {
            if (_compatInvokeBinders == null) {
                Interlocked.CompareExchange(
                    ref _compatInvokeBinders,
                    new Dictionary<CallSignature, CompatibilityInvokeBinder>(),
                    null
                );
            }

            lock (_compatInvokeBinders) {
                CallSignature sig = BindingHelpers.CallInfoToSignature(callInfo);
                CompatibilityInvokeBinder res;
                if (!_compatInvokeBinders.TryGetValue(sig, out res)) {
                    _compatInvokeBinders[sig] = res = new CompatibilityInvokeBinder(this, callInfo);
                }

                return res;
            }
        }

       
        internal PythonConversionBinder/*!*/ Convert(Type/*!*/ type, ConversionResultKind resultKind) {
            if (_conversionBinders == null) {
                Interlocked.CompareExchange(
                    ref _conversionBinders,
                    new Dictionary<Type, PythonConversionBinder>[(int)ConversionResultKind.ExplicitTry + 1], // max conversion result kind
                    null
                );
            }

            if (_conversionBinders[(int)resultKind] == null) {
                Interlocked.CompareExchange(
                    ref _conversionBinders[(int)resultKind],
                    new Dictionary<Type, PythonConversionBinder>(),
                    null
                );
            }

            Dictionary<Type, PythonConversionBinder> dict = _conversionBinders[(int)resultKind];
            lock (dict) {
                PythonConversionBinder res;
                if (!dict.TryGetValue(type, out res)) {
                    dict[type] = res = new PythonConversionBinder(this, type, resultKind);
                }

                return res;
            }
        }

        internal ConvertBinder/*!*/ CompatConvert(Type/*!*/ toType, bool isExplicit) {
            Dictionary<Type, ConvertBinder> binders;
            if (isExplicit) {
                if (_explicitCompatConvertBinders == null) {
                    Interlocked.CompareExchange(
                        ref _explicitCompatConvertBinders,
                        new Dictionary<Type, ConvertBinder>(), 
                        null
                    );
                }

                binders = _explicitCompatConvertBinders;
            } else {
                if (_implicitCompatConvertBinders == null) {
                    Interlocked.CompareExchange(
                        ref _implicitCompatConvertBinders,
                        new Dictionary<Type, ConvertBinder>(), 
                        null
                    );
                }

                binders = _implicitCompatConvertBinders;
            }

            ConvertBinder res;
            lock (binders) {
                if (!binders.TryGetValue(toType, out res)) {
                    binders[toType] = res = new CompatConversionBinder(this, toType, isExplicit);
                }
            }

            return res;
        }

        internal DynamicMetaObjectBinder/*!*/ ConvertRetObject(Type/*!*/ type, ConversionResultKind resultKind) {
            if (_convertRetObjectBinders == null) {
                Interlocked.CompareExchange(
                    ref _convertRetObjectBinders,
                    new Dictionary<Type, DynamicMetaObjectBinder>[(int)ConversionResultKind.ExplicitTry + 1], // max conversion result kind
                    null
                );
            }

            if (_convertRetObjectBinders[(int)resultKind] == null) {
                Interlocked.CompareExchange(
                    ref _convertRetObjectBinders[(int)resultKind],
                    new Dictionary<Type, DynamicMetaObjectBinder>(),
                    null
                );
            }

            Dictionary<Type, DynamicMetaObjectBinder> dict = _convertRetObjectBinders[(int)resultKind];
            lock (dict) {
                DynamicMetaObjectBinder res;
                if (!dict.TryGetValue(type, out res)) {
                    dict[type] = res = new PythonConversionBinder(this, type, resultKind, true);
                }

                return res;
            }
        }

        internal CreateFallback/*!*/ Create(CompatibilityInvokeBinder/*!*/ realFallback, CallInfo /*!*/ callInfo) {
            if (_createBinders == null) {
                Interlocked.CompareExchange(
                    ref _createBinders,
                    new Dictionary<CallSignature, CreateFallback>(),
                    null
                );
            }

            lock (_createBinders) {
                CallSignature sig = BindingHelpers.CallInfoToSignature(callInfo);
                CreateFallback res;
                if (!_createBinders.TryGetValue(sig, out res)) {
                    _createBinders[sig] = res = new CreateFallback(realFallback, callInfo);
                }

                return res;
            }
        }

        internal PythonGetMemberBinder/*!*/ GetMember(string/*!*/ name) {
            return GetMember(name, false);
        }

        internal PythonGetMemberBinder/*!*/ GetMember(string/*!*/ name, bool isNoThrow) {
            Dictionary<string, PythonGetMemberBinder> dict;
            if (isNoThrow) {
                if (_tryGetMemberBinders == null) {
                    Interlocked.CompareExchange(
                        ref _tryGetMemberBinders,
                        new Dictionary<string, PythonGetMemberBinder>(),
                        null
                    );
                }

                dict = _tryGetMemberBinders;
            } else {
                if (_getMemberBinders == null) {
                    Interlocked.CompareExchange(
                        ref _getMemberBinders,
                        new Dictionary<string, PythonGetMemberBinder>(),
                        null
                    );
                }

                dict = _getMemberBinders;
            }

            lock (dict) {
                PythonGetMemberBinder res;
                if (!dict.TryGetValue(name, out res)) {
                    dict[name] = res = new PythonGetMemberBinder(this, name, isNoThrow);
                }

                return res;
            }
        }

        internal CompatibilityGetMember/*!*/ CompatGetMember(string/*!*/ name) {
            if (_compatGetMember == null) {
                Interlocked.CompareExchange(
                    ref _compatGetMember,
                    new Dictionary<string, CompatibilityGetMember>(),
                    null
                );
            }

            lock (_compatGetMember) {
                CompatibilityGetMember res;
                if (!_compatGetMember.TryGetValue(name, out res)) {
                    _compatGetMember[name] = res = new CompatibilityGetMember(this, name);
                }

                return res;
            }
        }

        internal PythonSetMemberBinder/*!*/ SetMember(string/*!*/ name) {
            if (_setMemberBinders == null) {
                Interlocked.CompareExchange(
                    ref _setMemberBinders,
                    new Dictionary<string, PythonSetMemberBinder>(),
                    null
                );
            }

            lock (_setMemberBinders) {
                PythonSetMemberBinder res;
                if (!_setMemberBinders.TryGetValue(name, out res)) {
                    _setMemberBinders[name] = res = new PythonSetMemberBinder(this, name);
                }

                return res;
            }
        }

        internal PythonDeleteMemberBinder/*!*/ DeleteMember(string/*!*/ name) {
            if (_deleteMemberBinders == null) {
                Interlocked.CompareExchange(
                    ref _deleteMemberBinders,
                    new Dictionary<string, PythonDeleteMemberBinder>(),
                    null
                );
            }

            lock (_deleteMemberBinders) {
                PythonDeleteMemberBinder res;
                if (!_deleteMemberBinders.TryGetValue(name, out res)) {
                    _deleteMemberBinders[name] = res = new PythonDeleteMemberBinder(this, name);
                }

                return res;
            }
        }

        internal PythonInvokeBinder/*!*/ Invoke(CallSignature signature) {
            if (_invokeBinders == null) {
                Interlocked.CompareExchange(
                    ref _invokeBinders,
                    new Dictionary<CallSignature, PythonInvokeBinder>(),
                    null
                );
            }

            lock (_invokeBinders) {
                PythonInvokeBinder res;
                if (!_invokeBinders.TryGetValue(signature, out res)) {
                    _invokeBinders[signature] = res = new PythonInvokeBinder(this, signature);
                }

                return res;
            }
        }

        internal PythonInvokeBinder/*!*/ InvokeNone {
            get {
                if (_invokeNoArgs == null) {
                    _invokeNoArgs = Invoke(new CallSignature(0));
                }

                return _invokeNoArgs;
            }
        }

        internal PythonInvokeBinder/*!*/ InvokeOne {
            get {
                if (_invokeOneArg == null) {
                    _invokeOneArg = Invoke(new CallSignature(1));
                }

                return _invokeOneArg;
            }
        }

        internal PythonOperationBinder/*!*/ Operation(PythonOperationKind operation) {
            if (_operationBinders == null) {
                Interlocked.CompareExchange(
                    ref _operationBinders,
                    new Dictionary<PythonOperationKind, PythonOperationBinder>(),
                    null
                );
            }

            lock (_operationBinders) {
                PythonOperationBinder res;
                if (!_operationBinders.TryGetValue(operation, out res)) {
                    _operationBinders[operation] = res = new PythonOperationBinder(this, operation);
                }

                return res;
            }
        }

        internal PythonUnaryOperationBinder/*!*/ UnaryOperation(ExpressionType operation) {
            if (_unaryBinders == null) {
                Interlocked.CompareExchange(
                    ref _unaryBinders,
                    new Dictionary<ExpressionType, PythonUnaryOperationBinder>(),
                    null
                );
            }

            lock (_unaryBinders) {
                PythonUnaryOperationBinder res;
                if (!_unaryBinders.TryGetValue(operation, out res)) {
                    _unaryBinders[operation] = res = new PythonUnaryOperationBinder(this, operation);
                }

                return res;
            }

        }

        internal PythonBinaryOperationBinder/*!*/ BinaryOperation(ExpressionType operation) {
            if (_binaryBinders == null) {
                Interlocked.CompareExchange(
                    ref _binaryBinders,
                    new Dictionary<ExpressionType, PythonBinaryOperationBinder>(),
                    null
                );
            }

            lock (_binaryBinders) {
                PythonBinaryOperationBinder res;
                if (!_binaryBinders.TryGetValue(operation, out res)) {
                    _binaryBinders[operation] = res = new PythonBinaryOperationBinder(this, operation);
                }

                return res;
            }
        }

        internal OperationRetBoolBinder/*!*/ BinaryOperationRetType(PythonBinaryOperationBinder opBinder, PythonConversionBinder convBinder) {
            if (_binaryRetTypeBinders == null) {
                Interlocked.CompareExchange(
                    ref _binaryRetTypeBinders,
                    new Dictionary<BinaryOperationRetTypeKey, OperationRetBoolBinder>(),
                    null
                );
            }

            lock (_binaryRetTypeBinders) {
                OperationRetBoolBinder res;
                BinaryOperationRetTypeKey key = new BinaryOperationRetTypeKey(convBinder.Type, opBinder.Operation);
                if (!_binaryRetTypeBinders.TryGetValue(key, out res)) {
                    _binaryRetTypeBinders[key] = res = new OperationRetBoolBinder(opBinder, convBinder);
                }

                return res;
            }
        }

        internal PythonGetIndexBinder/*!*/ GetIndex(int argCount) {
            if (_getIndexBinders == null) {
                Interlocked.CompareExchange(ref _getIndexBinders, new PythonGetIndexBinder[argCount + 1], null);
            }

            lock (this) {
                if (_getIndexBinders.Length <= argCount) {
                    Array.Resize(ref _getIndexBinders, argCount + 1);
                }

                if (_getIndexBinders[argCount] == null) {
                    _getIndexBinders[argCount] = new PythonGetIndexBinder(this, argCount);
                }

                return _getIndexBinders[argCount];
            }
        }

        internal PythonSetIndexBinder/*!*/ SetIndex(int argCount) {
            if (_setIndexBinders == null) {
                Interlocked.CompareExchange(ref _setIndexBinders, new PythonSetIndexBinder[argCount + 1], null);
            }

            lock (this) {
                if (_setIndexBinders.Length <= argCount) {
                    Array.Resize(ref _setIndexBinders, argCount + 1);
                }

                if (_setIndexBinders[argCount] == null) {
                    _setIndexBinders[argCount] = new PythonSetIndexBinder(this, argCount);
                }

                return _setIndexBinders[argCount];
            }
        }

        internal PythonDeleteIndexBinder/*!*/ DeleteIndex(int argCount) {
            if (_deleteIndexBinders == null) {
                Interlocked.CompareExchange(ref _deleteIndexBinders, new PythonDeleteIndexBinder[argCount + 1], null);
            }

            lock (this) {
                if (_deleteIndexBinders.Length <= argCount) {
                    Array.Resize(ref _deleteIndexBinders, argCount + 1);
                }

                if (_deleteIndexBinders[argCount] == null) {
                    _deleteIndexBinders[argCount] = new PythonDeleteIndexBinder(this, argCount);
                }

                return _deleteIndexBinders[argCount];
            }
        }

        internal PythonGetSliceBinder/*!*/ GetSlice {
            get {
                if (_getSlice == null) {
                    Interlocked.CompareExchange(ref _getSlice, new PythonGetSliceBinder(this), null);
                }

                return _getSlice;
            }
        }

        internal PythonSetSliceBinder/*!*/ SetSlice {
            get {
                if (_setSlice == null) {
                    Interlocked.CompareExchange(ref _setSlice, new PythonSetSliceBinder(this), null);
                }

                return _setSlice;
            }
        }

        internal PythonDeleteSliceBinder/*!*/ DeleteSlice {
            get {
                if (_deleteSlice == null) {
                    Interlocked.CompareExchange(ref _deleteSlice, new PythonDeleteSliceBinder(this), null);
                }

                return _deleteSlice;
            }
        }

        #endregion

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Expression.Call(
                typeof(PythonOps).GetMethod("GetInitialBinderState"),
                Compiler.Ast.ArrayGlobalAllocator._globalContext
            );
        }

        #endregion

        class BinaryOperationRetTypeKey : IEquatable<BinaryOperationRetTypeKey> {
            public readonly Type ReturnType;
            public readonly ExpressionType Operation;

            public BinaryOperationRetTypeKey(Type retType, ExpressionType operation) {
                ReturnType = retType;
                Operation = operation;
            }

            #region IEquatable<BinaryOperationRetTypeKey> Members

            public bool Equals(BinaryOperationRetTypeKey other) {
                return other.ReturnType == ReturnType &&
                    other.Operation == Operation;
            }

            #endregion

            public override int GetHashCode() {
                return ReturnType.GetHashCode() ^ Operation.GetHashCode();
            }

            public override bool Equals(object obj) {
                BinaryOperationRetTypeKey other = obj as BinaryOperationRetTypeKey;
                if (other != null) {
                    return Equals(other);
                }

                return false;
            }
        }
    }
}
