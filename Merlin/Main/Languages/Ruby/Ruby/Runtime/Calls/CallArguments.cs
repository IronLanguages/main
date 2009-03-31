/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Compiler;
using Microsoft.Scripting;

namespace IronRuby.Runtime.Calls {

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

        public object Target {
            get { return MetaTarget.Value; }
        }

        public Proc GetBlock() {
            return (Proc)_args[GetBlockIndex()].Value;
        }

        public object GetSplattedArgument() {
            return _args[GetSplattedArgumentIndex()].Value;
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


        public Expression[]/*!*/ GetSimpleArgumentExpressions() {
            var result = new Expression[SimpleArgumentCount];
            for (int i = 0, j = GetSimpleArgumentsIndex(0); i < result.Length; j++, i++) {
                result[i] = _args[j].Expression;
            }
            return result;
        }

        internal Expression[]/*!*/ GetCallSiteArguments(Expression/*!*/ targetExpression) {
            // context, target, arguments:
            var result = new Expression[CallSiteArgumentCount];

            int i = 0;
            if (_hasScopeOrContextArg) {
                result[i++] = _signature.HasScope ? MetaScope.Expression : MetaContext.Expression;
            }
            result[i++] = targetExpression;

            for (int j = FirstArgumentIndex; j < _args.Length; j++) {
                result[i++] = _args[j].Expression;
            }

            Debug.Assert(i == result.Length);

            return result;
        }

        private int GetSimpleArgumentsIndex(int i) {
            return FirstArgumentIndex + (_signature.HasBlock ? 1 : 0) + i;
        }

        internal object GetSimpleArgument(int i) {
            return GetSimpleMetaArgument(i).Value;
        }

        internal Expression/*!*/ GetSimpleArgumentExpression(int i) {
            return GetSimpleMetaArgument(i).Expression;
        }

        internal DynamicMetaObject/*!*/ GetSimpleMetaArgument(int i) {
            return _args[GetSimpleArgumentsIndex(i)];
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
        internal CallArguments(RubyContext context, DynamicMetaObject/*!*/ scopeOrContextOrTarget, DynamicMetaObject/*!*/[]/*!*/ args, RubyCallSignature signature) {
            Assert.NotNull(scopeOrContextOrTarget);
            Assert.NotNullItems(args);

            Debug.Assert(signature.HasScope == scopeOrContextOrTarget.Value is RubyScope);
            Debug.Assert((context == null && !signature.HasScope) == scopeOrContextOrTarget.Value is RubyContext);

            if (context != null) {
                // bound site:
                _context = new DynamicMetaObject(AstUtils.Constant(context), BindingRestrictions.Empty, context);
                if (signature.HasScope) {
                    _scope = scopeOrContextOrTarget;
                    _hasScopeOrContextArg = true;
                } else {
                    _target = scopeOrContextOrTarget;
                }
            } else if (signature.HasScope) {
                // unbound site with scope:
                _context = new DynamicMetaObject(
                    Methods.GetContextFromScope.OpCall(scopeOrContextOrTarget.Expression), BindingRestrictions.Empty, 
                    ((RubyScope)scopeOrContextOrTarget.Value).RubyContext
                );
                _scope = scopeOrContextOrTarget;
                _hasScopeOrContextArg = true;
                _target = null;
            } else {
                // unbound site with context:
                _context = scopeOrContextOrTarget;
                _hasScopeOrContextArg = true;
                _target = null;
            }

            Debug.Assert(_target != null || args.Length > 0);

            _args = args;
            _copyArgsOnWrite = true;
            _signature = signature;
        }

        // interop binders: the target is a Ruby meta-object closed over the context
        internal CallArguments(DynamicMetaObject/*!*/ context, DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, RubyCallSignature signature) {
            Assert.NotNull(target, context);
            Assert.NotNullItems(args);

            Debug.Assert(!signature.HasScope);

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
                RubyCallSignature.Simple(callInfo.ArgumentCount)
            ) {
        }

        public void InsertSimple(int index, DynamicMetaObject/*!*/ arg) {
            index = GetSimpleArgumentsIndex(index);

            _args = ArrayUtils.InsertAt(_args, index, arg);
            _signature = new RubyCallSignature(_signature.ArgumentCount + 1, _signature.Flags);
        }

        internal void InsertMethodName(string/*!*/ methodName) {
            // insert the method name argument into the args
            object symbol = SymbolTable.StringToId(methodName);
            InsertSimple(0, new DynamicMetaObject(AstUtils.Constant(symbol), BindingRestrictions.Empty, symbol));
        }

        public void SetSimpleArgument(int index, DynamicMetaObject/*!*/ arg) {
            SetArgument(GetSimpleArgumentsIndex(index), arg);
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
        }
    }
}
