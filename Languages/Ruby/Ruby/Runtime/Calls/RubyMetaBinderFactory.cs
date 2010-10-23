/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if CLR2
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif

using System;
using System.Collections.Generic;
using System.Threading;
using System.Dynamic;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Runtime.Calls {
    internal sealed class RubyMetaBinderFactory {
        private static RubyMetaBinderFactory _Shared;

        /// <summary>
        /// Sites shared across runtimes.
        /// </summary>
        internal static RubyMetaBinderFactory Shared {
            get {
                if (_Shared == null) {
                    Interlocked.CompareExchange(ref _Shared, new RubyMetaBinderFactory(null), null);
                }
                return _Shared;
            }
        }

        private readonly RubyContext _context;

        // (name, signature) => binder:
        private readonly Dictionary<Key<string/*!*/, RubyCallSignature>, RubyCallAction>/*!*/ _callActions;

        // (scope id, signature) => binder:
        private readonly Dictionary<Key<int, RubyCallSignature>, SuperCallAction>/*!*/ _superCallActions;

        // (typeof(action)) => binder:
        private readonly Dictionary<Type, RubyConversionAction>/*!*/ _conversionActions;

        // (CompositeConversion) => binder:
        private readonly Dictionary<CompositeConversion, CompositeConversionAction>/*!*/ _compositeConversionActions;

        // (type) => binder:
        private readonly Dictionary<Type, GenericConversionAction>/*!*/ _genericConversionActions;

        // interop binders:
        private Dictionary<CallInfo, InteropBinder.CreateInstance> _interopCreateInstance;
        private Dictionary<CallInfo, InteropBinder.Return> _interopReturn;
        private Dictionary<CallInfo, InteropBinder.Invoke> _interopInvoke;
        private Dictionary<Key<string, CallInfo>, InteropBinder.InvokeMember> _interopInvokeMember;
        private Dictionary<string, InteropBinder.GetMember> _interopGetMember;
        private Dictionary<string, InteropBinder.TryGetMemberExact> _interopTryGetMemberExact;
        private Dictionary<string, InteropBinder.SetMember> _interopSetMember;
        private Dictionary<string, InteropBinder.SetMemberExact> _interopSetMemberExact;
        private Dictionary<CallInfo, InteropBinder.GetIndex> _interopGetIndex;
        private Dictionary<CallInfo, InteropBinder.SetIndex> _interopSetIndex;
        private Dictionary<Key<string, CallInfo>, InteropBinder.SetIndexedProperty> _interopSetIndexedProperty;
        private Dictionary<ExpressionType, DynamicMetaObjectBinder> _interopOperation;
        private Dictionary<Key<Type, bool>, InteropBinder.Convert> _interopConvert;
        private InteropBinder.Splat _interopSplat;

        internal RubyMetaBinderFactory(RubyContext context) {
            _context = context;
            _callActions = new Dictionary<Key<string, RubyCallSignature>, RubyCallAction>();
            _superCallActions = new Dictionary<Key<int, RubyCallSignature>, SuperCallAction>();
            _conversionActions = new Dictionary<Type, RubyConversionAction>();
            _compositeConversionActions = new Dictionary<CompositeConversion, CompositeConversionAction>();
            _genericConversionActions = new Dictionary<Type, GenericConversionAction>();
        }

        public RubyCallAction/*!*/ Call(string/*!*/ methodName, RubyCallSignature signature) {
            var key = Key.Create(methodName, signature);

            lock (_callActions) {
                RubyCallAction result;
                if (!_callActions.TryGetValue(key, out result)) {
                    _callActions.Add(key, result = new RubyCallAction(_context, methodName, signature));
                }
                return result;
            }
        }

        public SuperCallAction/*!*/ SuperCall(int/*!*/ lexicalScopeId, RubyCallSignature signature) {
            var key = Key.Create(lexicalScopeId, signature);

            lock (_superCallActions) {
                SuperCallAction result;
                if (!_superCallActions.TryGetValue(key, out result)) {
                    _superCallActions.Add(key, result = new SuperCallAction(_context, signature, lexicalScopeId));
                }
                return result;
            }
        }

        public TAction/*!*/ Conversion<TAction>() where TAction : RubyConversionAction, new() {
            var key = typeof(TAction);

            lock (_conversionActions) {
                RubyConversionAction result;
                if (!_conversionActions.TryGetValue(key, out result)) {
                    _conversionActions.Add(key, result = new TAction() { Context = _context });
                }
                return (TAction)result;
            }
        }

        public CompositeConversionAction/*!*/ CompositeConversion(CompositeConversion conversion) {
            var key = conversion;

            lock (_conversionActions) {
                CompositeConversionAction result;
                if (!_compositeConversionActions.TryGetValue(key, out result)) {
                    _compositeConversionActions.Add(key, result = CompositeConversionAction.Make(_context, conversion));
                }
                return result;
            }
        }

        public GenericConversionAction/*!*/ GenericConversionAction(Type/*!*/ type) {
            lock (_conversionActions) {
                GenericConversionAction result;
                if (!_genericConversionActions.TryGetValue(type, out result)) {
                    _genericConversionActions.Add(type, result = new GenericConversionAction(_context, type));
                }
                return result;
            }
        }

        public InteropBinder.CreateInstance/*!*/ InteropCreateInstance(CallInfo/*!*/ callInfo) {
            if (_interopCreateInstance == null) {
                Interlocked.CompareExchange(ref _interopCreateInstance, new Dictionary<CallInfo, InteropBinder.CreateInstance>(), null);
            }

            lock (_interopCreateInstance) {
                InteropBinder.CreateInstance result;
                if (!_interopCreateInstance.TryGetValue(callInfo, out result)) {
                    _interopCreateInstance.Add(callInfo, result = new InteropBinder.CreateInstance(_context, callInfo));
                }
                return result;
            }
        }

        public InteropBinder.Return/*!*/ InteropReturn(CallInfo/*!*/ callInfo) {
            if (_interopReturn == null) {
                Interlocked.CompareExchange(ref _interopReturn, new Dictionary<CallInfo, InteropBinder.Return>(), null);
            }

            lock (_interopReturn) {
                InteropBinder.Return result;
                if (!_interopReturn.TryGetValue(callInfo, out result)) {
                    _interopReturn.Add(callInfo, result = new InteropBinder.Return(_context, callInfo));
                }
                return result;
            }
        }

        public InteropBinder.Invoke/*!*/ InteropInvoke(CallInfo/*!*/ callInfo) {
            if (_interopInvoke == null) {
                Interlocked.CompareExchange(ref _interopInvoke, new Dictionary<CallInfo, InteropBinder.Invoke>(), null);
            }

            lock (_interopInvoke) {
                InteropBinder.Invoke result;
                if (!_interopInvoke.TryGetValue(callInfo, out result)) {
                    _interopInvoke.Add(callInfo, result = new InteropBinder.Invoke(_context, callInfo));
                }
                return result;
            }
        }

        public InteropBinder.InvokeMember/*!*/ InteropInvokeMember(string/*!*/ name, CallInfo/*!*/ callInfo) {
            if (_interopInvokeMember == null) {
                Interlocked.CompareExchange(ref _interopInvokeMember, new Dictionary<Key<string, CallInfo>, InteropBinder.InvokeMember>(), null);
            }

            var key = Key.Create(name, callInfo);

            lock (_interopInvokeMember) {
                InteropBinder.InvokeMember result;
                if (!_interopInvokeMember.TryGetValue(key, out result)) {
                    _interopInvokeMember.Add(key, result = new InteropBinder.InvokeMember(_context, name, callInfo, null));
                }
                return result;
            }
        }

        public InteropBinder.GetMember/*!*/ InteropGetMember(string/*!*/ name) {
            if (_interopGetMember == null) {
                Interlocked.CompareExchange(ref _interopGetMember, new Dictionary<string, InteropBinder.GetMember>(), null);
            }

            lock (_interopGetMember) {
                InteropBinder.GetMember result;
                if (!_interopGetMember.TryGetValue(name, out result)) {
                    _interopGetMember.Add(name, result = new InteropBinder.GetMember(_context, name, null));
                }
                return result;
            }
        }

        public InteropBinder.TryGetMemberExact/*!*/ InteropTryGetMemberExact(string/*!*/ name) {
            if (_interopTryGetMemberExact == null) {
                Interlocked.CompareExchange(ref _interopTryGetMemberExact, new Dictionary<string, InteropBinder.TryGetMemberExact>(), null);
            }

            lock (_interopTryGetMemberExact) {
                InteropBinder.TryGetMemberExact result;
                if (!_interopTryGetMemberExact.TryGetValue(name, out result)) {
                    _interopTryGetMemberExact.Add(name, result = new InteropBinder.TryGetMemberExact(_context, name));
                }
                return result;
            }
        }

        public InteropBinder.SetMember/*!*/ InteropSetMember(string/*!*/ name) {
            if (_interopSetMember == null) {
                Interlocked.CompareExchange(ref _interopSetMember, new Dictionary<string, InteropBinder.SetMember>(), null);
            }

            lock (_interopSetMember) {
                InteropBinder.SetMember result;
                if (!_interopSetMember.TryGetValue(name, out result)) {
                    _interopSetMember.Add(name, result = new InteropBinder.SetMember(_context, name));
                }
                return result;
            }
        }

        public InteropBinder.SetMemberExact/*!*/ InteropSetMemberExact(string/*!*/ name) {
            if (_interopSetMemberExact == null) {
                Interlocked.CompareExchange(ref _interopSetMemberExact, new Dictionary<string, InteropBinder.SetMemberExact>(), null);
            }

            lock (_interopSetMemberExact) {
                InteropBinder.SetMemberExact result;
                if (!_interopSetMemberExact.TryGetValue(name, out result)) {
                    _interopSetMemberExact.Add(name, result = new InteropBinder.SetMemberExact(_context, name));
                }
                return result;
            }
        }

        public InteropBinder.GetIndex/*!*/ InteropGetIndex(CallInfo/*!*/ callInfo) {
            if (_interopGetIndex == null) {
                Interlocked.CompareExchange(ref _interopGetIndex, new Dictionary<CallInfo, InteropBinder.GetIndex>(), null);
            }

            lock (_interopGetIndex) {
                InteropBinder.GetIndex result;
                if (!_interopGetIndex.TryGetValue(callInfo, out result)) {
                    _interopGetIndex.Add(callInfo, result = new InteropBinder.GetIndex(_context, callInfo));
                }
                return result;
            }
        }

        public InteropBinder.SetIndex/*!*/ InteropSetIndex(CallInfo/*!*/ callInfo) {
            if (_interopSetIndex == null) {
                Interlocked.CompareExchange(ref _interopSetIndex, new Dictionary<CallInfo, InteropBinder.SetIndex>(), null);
            }

            lock (_interopSetIndex) {
                InteropBinder.SetIndex result;
                if (!_interopSetIndex.TryGetValue(callInfo, out result)) {
                    _interopSetIndex.Add(callInfo, result = new InteropBinder.SetIndex(_context, callInfo));
                }
                return result;
            }
        }

        public InteropBinder.SetIndexedProperty/*!*/ InteropSetIndexedProperty(string/*!*/ name, CallInfo/*!*/ callInfo) {
            if (_interopSetIndexedProperty == null) {
                Interlocked.CompareExchange(ref _interopSetIndexedProperty, new Dictionary<Key<string, CallInfo>, InteropBinder.SetIndexedProperty>(), null);
            }

            var key = Key.Create(name, callInfo);

            lock (_interopSetIndexedProperty) {
                InteropBinder.SetIndexedProperty result;
                if (!_interopSetIndexedProperty.TryGetValue(key, out result)) {
                    _interopSetIndexedProperty.Add(key, result = new InteropBinder.SetIndexedProperty(_context, name, callInfo));
                }
                return result;
            }
        }

        public InteropBinder.BinaryOperation/*!*/ InteropBinaryOperation(ExpressionType op) {
            if (_interopOperation == null) {
                Interlocked.CompareExchange(ref _interopOperation, new Dictionary<ExpressionType, DynamicMetaObjectBinder>(), null);
            }

            lock (_interopOperation) {
                DynamicMetaObjectBinder result;
                if (!_interopOperation.TryGetValue(op, out result)) {
                    _interopOperation.Add(op, result = new InteropBinder.BinaryOperation(_context, op));
                }
                return (InteropBinder.BinaryOperation)result;
            }
        }

        public InteropBinder.UnaryOperation/*!*/ InteropUnaryOperation(ExpressionType op) {
            if (_interopOperation == null) {
               Interlocked.CompareExchange(ref _interopOperation, new Dictionary<ExpressionType, DynamicMetaObjectBinder>(), null);
            }

            lock (_interopOperation) {
                DynamicMetaObjectBinder result;
                if (!_interopOperation.TryGetValue(op, out result)) {
                    _interopOperation.Add(op, result = new InteropBinder.UnaryOperation(_context, op));
                }
                return (InteropBinder.UnaryOperation)result;
            }
        }

        public InteropBinder.Convert/*!*/ InteropConvert(Type/*!*/ type, bool isExplicit) {
            if (_interopConvert == null) {
                Interlocked.CompareExchange(ref _interopConvert, new Dictionary<Key<Type, bool>, InteropBinder.Convert>(), null);
            }

            var key = Key.Create(type, isExplicit);

            lock (_interopConvert) {
                InteropBinder.Convert result;
                if (!_interopConvert.TryGetValue(key, out result)) {
                    _interopConvert.Add(key, result = new InteropBinder.Convert(_context, type, isExplicit));
                }
                return result;
            }
        }

        public InteropBinder.Splat/*!*/ InteropSplat() {
            if (_interopSplat == null) {
                _interopSplat = new InteropBinder.Splat(_context);
            }
            return _interopSplat;
        }
    }
}
 