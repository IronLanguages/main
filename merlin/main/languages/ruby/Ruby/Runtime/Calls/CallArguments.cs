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
using System.Dynamic.Binders;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Compiler;

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
        private readonly MetaObject/*!*/ _context;

        // _args[0] might be target, if so _target is null:
        private MetaObject _target;

        // Arguments must be readonly if _copyOnWrite is true. 
        private MetaObject[]/*!*/ _args;
        private bool _copyArgsOnWrite;
        
        private Expression _scopeExpression;
        private Expression _contextExpression;
        
        private RubyCallSignature _signature;

        public RubyCallSignature/*!*/ Signature {
            get { return _signature; }
        }

        public int CallSiteArgumentCount {
            get {
                // context, target, arguments:
                return 1 + ExplicitArgumentCount; 
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

        public Expression/*!*/ ScopeExpression {
            get {
                if (_scopeExpression == null) {
                    if (_signature.HasScope) {
                        _scopeExpression = AstUtils.Convert(MetaContext.Expression, typeof(RubyScope));
                    } else {
                        _scopeExpression = Methods.GetEmptyScope.OpCall(AstUtils.Convert(MetaContext.Expression, typeof(RubyContext)));
                    }
                }
                return _scopeExpression;
            }
        }

        public Expression/*!*/ ContextExpression {
            get {
                if (_contextExpression == null) {
                    if (_signature.HasScope) {
                        _contextExpression = Methods.GetContextFromScope.OpCall(AstUtils.Convert(MetaContext.Expression, typeof(RubyScope)));
                    } else {
                        _contextExpression = AstUtils.Convert(MetaContext.Expression, typeof(RubyContext));
                    }
                }
                return _contextExpression;
            }
        }

        public RubyScope/*!*/ Scope {
            get { return (RubyScope)MetaContext.Value; }
        }

        public RubyContext/*!*/ RubyContext {
            get { return _signature.HasScope ? Scope.RubyContext : (RubyContext)MetaContext.Value; }
        }

        // RubyScope or RubyContext
        public MetaObject/*!*/ MetaContext {
            get { return _context; }
        }

        public MetaObject/*!*/ MetaTarget {
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

        public Expression GetSplattedArgumentExpression() {
            return _signature.HasSplattedArgument ? _args[GetSplattedArgumentIndex()].Expression : null;
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
            result[0] = MetaContext.Expression;
            result[1] = targetExpression;

            int i = 2, j = FirstArgumentIndex;
            for (; j < _args.Length; i++, j++) {
                result[i] = _args[j].Expression;
            }

            Debug.Assert(i == result.Length && j == _args.Length);

            return result;
        }

        private int GetSimpleArgumentsIndex(int i) {
            return FirstArgumentIndex + (_signature.HasBlock ? 1 : 0) + i;
        }

        internal object GetSimpleArgument(int i) {
            return _args[GetSimpleArgumentsIndex(i)].Value;
        }

        internal Expression/*!*/ GetSimpleArgumentExpression(int i) {
            return _args[GetSimpleArgumentsIndex(i)].Expression;
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

        internal CallArguments(MetaObject/*!*/ context, MetaObject/*!*/ target, MetaObject/*!*/[]/*!*/ args, RubyCallSignature signature) {
            Assert.NotNull(target, context);
            Assert.NotNullItems(args);

            Debug.Assert(signature.HasScope == context.Value is RubyScope);
            Debug.Assert(!signature.HasScope == context.Value is RubyContext);

            _target = target;
            _context = context;
            _args = args;
            _copyArgsOnWrite = true;
            _signature = signature;
        }

        internal CallArguments(MetaObject/*!*/ context, MetaObject/*!*/[]/*!*/ args, RubyCallSignature signature) {
            Assert.NotNull(context);
            Assert.NotNullItems(args);
            Assert.NotEmpty(args);

            Debug.Assert(signature.HasScope == context.Value is RubyScope);
            Debug.Assert(!signature.HasScope == context.Value is RubyContext);
            
            _target = null;
            _context = context;
            _args = args;
            _copyArgsOnWrite = true;
            _signature = signature;
        }

        public void InsertSimple(int index, MetaObject/*!*/ arg) {
            index = GetSimpleArgumentsIndex(index);

            _args = ArrayUtils.InsertAt(_args, index, arg);
            _signature = new RubyCallSignature(_signature.ArgumentCount + 1, _signature.Flags);
        }

        public void SetSimpleArgument(int index, MetaObject/*!*/ arg) {
            SetArgument(GetSimpleArgumentsIndex(index), arg);
        }

        private void SetArgument(int index, MetaObject/*!*/ arg) {
            if (_copyArgsOnWrite) {
                _args = ArrayUtils.Copy(_args);
                _copyArgsOnWrite = false;
            }

            _args[index] = arg;
        }

        public void SetTarget(Expression/*!*/ expression, object value) {
            Assert.NotNull(expression);

            var metaTarget = new MetaObject(expression, Restrictions.Empty, value);

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
