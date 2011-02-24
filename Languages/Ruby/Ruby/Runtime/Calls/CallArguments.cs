/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
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

using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;
    using AstExpressions = ReadOnlyCollectionBuilder<Expression>;

    /// <summary>
    /// Wraps the arguments of a dynamic call site
    /// Includes the actual arguments, the expressions that produced those arguments,
    /// and the call signature.
    /// 
    /// These three things are grouped together to ensure that they are all in sync
    /// when we want to shift the arguments around during the method binding process.
    /// </summary>
    public sealed class CallArguments {
        private readonly bool _hasScopeOrContextArg;
        private readonly DynamicMetaObject/*!*/ _context;
        private DynamicMetaObject _scope;

        // _args[0] might be target, if so _target is null:
        private DynamicMetaObject _target;
        private RubyClass _targetClass;

        // Arguments must be readonly if _copyOnWrite is true. 
        private DynamicMetaObject[]/*!*/ _args;
        private bool _copyArgsOnWrite;
        
        private RubyCallSignature _signature;

        public RubyCallSignature/*!*/ Signature {
            get { return _signature; }
        }

        public int CallSiteArgumentCount {
            get {
                // (scope|context)?, target, arguments:
                return (_hasScopeOrContextArg ? 1 : 0) + ExplicitArgumentCount; 
            }
        }

        public int ExplicitArgumentCount {
            get {
                // target, arguments:
                return (_target != null ? 1 : 0) + _args.Length;
            }
        }

        // Index of the first argument in _args array.
        private int FirstArgumentIndex {
            get { return (_target == null) ? 1 : 0; }
        }

        public int SimpleArgumentCount {
            get {
                return _args.Length - FirstArgumentIndex - (_signature.HasBlock ? 1 : 0) - 
                    (_signature.HasSplattedArgument ? 1 : 0) - (_signature.HasRhsArgument ? 1 : 0); 
            }
        }

        public DynamicMetaObject/*!*/ MetaScope {
            get {
                if (_scope == null) {
                    Debug.Assert(!_signature.HasScope);
                    // we can burn the scope as a constant since we'll restrict the context arg to the current context:
                    var emptyScope = ((RubyContext)_context.Value).EmptyScope;
                    _scope = new DynamicMetaObject(Ast.Constant(emptyScope, typeof(RubyScope)), BindingRestrictions.Empty, emptyScope);
                }
                return _scope;
            }
        }

        public DynamicMetaObject/*!*/ MetaContext {
            get { return _context; }
        }

        public RubyScope/*!*/ Scope {
            get { return (RubyScope)MetaScope.Value; }
        }

        public RubyContext/*!*/ RubyContext {
            get { return (RubyContext)MetaContext.Value; }
        }

        public DynamicMetaObject/*!*/ MetaTarget {
            get { return _target ?? _args[0]; }            
        }

        public Expression/*!*/ TargetExpression {
            get { return MetaTarget.Expression; }
        }

        public RubyClass/*!*/ TargetClass {
            get {
                if (_targetClass == null) {
                    _targetClass = RubyContext.GetImmediateClassOf(Target);
                }
                return _targetClass;
            }
        }

        public object Target {
            get { return MetaTarget.Value; }
        }

        public Proc GetBlock() {
            return (Proc)_args[GetBlockIndex()].Value;
        }

        public IList/*!*/ GetSplattedArgument() {
            return (IList)_args[GetSplattedArgumentIndex()].Value;
        }

        public object GetRhsArgument() {
            return _args[GetRhsArgumentIndex()].Value;
        }

        public Expression GetBlockExpression() {
            return _signature.HasBlock ? _args[GetBlockIndex()].Expression : null;
        }

        public DynamicMetaObject GetMetaBlock() {
            return _signature.HasBlock ? _args[GetBlockIndex()] : null;
        }

        public DynamicMetaObject GetSplattedMetaArgument() {
            return _signature.HasSplattedArgument ? _args[GetSplattedArgumentIndex()] : null;
        }

        public Expression GetSplattedArgumentExpression() {
            return _signature.HasSplattedArgument ? _args[GetSplattedArgumentIndex()].Expression : null;
        }

        public DynamicMetaObject GetRhsMetaArgument() {
            return _signature.HasRhsArgument ? _args[GetRhsArgumentIndex()] : null;
        }

        public Expression GetRhsArgumentExpression() {
            return _signature.HasRhsArgument ? _args[GetRhsArgumentIndex()].Expression : null;
        }


        public AstExpressions/*!*/ GetSimpleArgumentExpressions() {
            int count = SimpleArgumentCount;
            var result = new AstExpressions(count);
            for (int i = 0, j = GetSimpleArgumentIndex(0); i < count; j++, i++) {
                result.Add(_args[j].Expression);
            }
            return result;
        }

        internal ReadOnlyCollection<Expression>/*!*/ GetCallSiteArguments(Expression/*!*/ targetExpression) {
            // context, target, arguments:
            var result = new AstExpressions(CallSiteArgumentCount);

            if (_hasScopeOrContextArg) {
                result.Add(_signature.HasScope ? MetaScope.Expression : MetaContext.Expression);
            }
            result.Add(targetExpression);

            for (int j = FirstArgumentIndex; j < _args.Length; j++) {
                result.Add(_args[j].Expression);
            }

            return result.ToReadOnlyCollection();
        }

        private int GetSimpleArgumentIndex(int i) {
            return FirstArgumentIndex + (_signature.HasBlock ? 1 : 0) + i;
        }

        internal object GetSimpleArgument(int i) {
            return GetSimpleMetaArgument(i).Value;
        }

        internal Expression/*!*/ GetSimpleArgumentExpression(int i) {
            return GetSimpleMetaArgument(i).Expression;
        }

        internal DynamicMetaObject/*!*/ GetSimpleMetaArgument(int i) {
            return _args[GetSimpleArgumentIndex(i)];
        }
        
        internal int GetBlockIndex() {
            Debug.Assert(_signature.HasBlock);
            return FirstArgumentIndex;
        }

        internal int GetSplattedArgumentIndex() {
            Debug.Assert(_signature.HasSplattedArgument);
            return _args.Length - (_signature.HasRhsArgument ? 2 : 1);
        }

        internal int GetRhsArgumentIndex() {
            Debug.Assert(_signature.HasRhsArgument);
            return _args.Length - 1;
        }

        // Ruby binders: 
        internal CallArguments(RubyContext context, DynamicMetaObject/*!*/ scopeOrContextOrTargetOrArgArray, DynamicMetaObject/*!*/[]/*!*/ args, RubyCallSignature signature) {
            Assert.NotNull(scopeOrContextOrTargetOrArgArray);
            Assert.NotNullItems(args);

            ArgumentArray argArray = scopeOrContextOrTargetOrArgArray.Value as ArgumentArray;
            if (argArray != null) {
                Debug.Assert(args.Length == 0 && argArray.Count >= 1);

                // build meta-objects for arguments wrapped in the array:
                args = new DynamicMetaObject[argArray.Count - 1];
                for (int i = 0; i < args.Length; i++) {
                    args[i] = argArray.GetMetaObject(scopeOrContextOrTargetOrArgArray.Expression, 1 + i);
                }
                scopeOrContextOrTargetOrArgArray = argArray.GetMetaObject(scopeOrContextOrTargetOrArgArray.Expression, 0);
            }

            Debug.Assert(signature.HasScope == scopeOrContextOrTargetOrArgArray.Value is RubyScope);
            Debug.Assert((context == null && !signature.HasScope) == scopeOrContextOrTargetOrArgArray.Value is RubyContext);

            if (context != null) {
                // bound site:
                _context = new DynamicMetaObject(AstUtils.Constant(context), BindingRestrictions.Empty, context);
                if (signature.HasScope) {
                    _scope = scopeOrContextOrTargetOrArgArray;
                    _hasScopeOrContextArg = true;
                } else {
                    _target = scopeOrContextOrTargetOrArgArray;
                }
            } else if (signature.HasScope) {
                // unbound site with scope:
                _context = new DynamicMetaObject(
                    Methods.GetContextFromScope.OpCall(scopeOrContextOrTargetOrArgArray.Expression), BindingRestrictions.Empty, 
                    ((RubyScope)scopeOrContextOrTargetOrArgArray.Value).RubyContext
                );
                _scope = scopeOrContextOrTargetOrArgArray;
                _hasScopeOrContextArg = true;
                _target = null;
            } else {
                // unbound site with context:
                _context = scopeOrContextOrTargetOrArgArray;
                _hasScopeOrContextArg = true;
                _target = null;
            }

            Debug.Assert(_target != null || args.Length > 0);

            _args = args;
            _copyArgsOnWrite = true;
            _signature = signature;

            Debug.Assert(!signature.HasSplattedArgument || GetSplattedArgument() != null);
        }

        // interop binders: the target is a Ruby meta-object closed over the context
        internal CallArguments(DynamicMetaObject/*!*/ context, DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, RubyCallSignature signature) {
            Assert.NotNull(target, context);
            Assert.NotNullItems(args);

            Debug.Assert(!signature.HasScope && !signature.HasSplattedArgument);

            _target = target;
            _context = context;
            _args = args;
            _copyArgsOnWrite = true;
            _signature = signature;
        }

        // interop binders: the target is a foreign meta-object, the binder is context-bound:
        internal CallArguments(RubyContext/*!*/ context, DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, CallInfo/*!*/ callInfo) 
            : this (
                new DynamicMetaObject(AstUtils.Constant(context), BindingRestrictions.Empty, context),
                target,
                args,
                RubyCallSignature.Interop(callInfo.ArgumentCount)
            ) {
        }

        public void InsertSimple(int index, DynamicMetaObject/*!*/ arg) {
            index = GetSimpleArgumentIndex(index);

            _args = ArrayUtils.InsertAt(_args, index, arg);
            _signature = new RubyCallSignature(_signature.ArgumentCount + 1, _signature.Flags);
        }

        internal void InsertMethodName(string/*!*/ methodName) {
            // insert the method name argument into the args
            object symbol = RubyContext.EncodeIdentifier(methodName);
            InsertSimple(0, new DynamicMetaObject(AstUtils.Constant(symbol), BindingRestrictions.Empty, symbol));
        }

        public void SetSimpleArgument(int index, DynamicMetaObject/*!*/ arg) {
            SetArgument(GetSimpleArgumentIndex(index), arg);
        }

        private void SetArgument(int index, DynamicMetaObject/*!*/ arg) {
            if (_copyArgsOnWrite) {
                _args = ArrayUtils.Copy(_args);
                _copyArgsOnWrite = false;
            }

            _args[index] = arg;
        }

        public void SetTarget(Expression/*!*/ expression, object value) {
            Assert.NotNull(expression);

            var metaTarget = new DynamicMetaObject(expression, BindingRestrictions.Empty, value);

            if (_target == null) {
                if (_copyArgsOnWrite) {
                    _args = ArrayUtils.RemoveFirst(_args);
                    _copyArgsOnWrite = false;
                    _target = metaTarget;
                } else {
                    _args[0] = metaTarget;
                }
            } else {
                _target = metaTarget;
            }

            _targetClass = null;
        }
    }
}
