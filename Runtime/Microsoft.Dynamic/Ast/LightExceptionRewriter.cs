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


#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting.Utils;
using System.Threading;
using Microsoft.Scripting.Runtime;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Internal re-writer class which creates code which is light exception aware.
    /// </summary>
    class LightExceptionRewriter : ExpressionVisitor {
        private LabelTarget _currentHandler;    // current label we should branch to when an exception is found
        private ParameterExpression _rethrow;   // current exception variable that we will rethrow

        private static readonly ParameterExpression _lastValue = Expression.Parameter(typeof(object), "$lastValue");
        private static readonly ReadOnlyCollection<ParameterExpression> _lastValueParamArray = new ReadOnlyCollectionBuilder<ParameterExpression>(1) { _lastValue }.ToReadOnlyCollection();
        private static readonly Expression _isLightExExpr = Expression.Call(new Func<Exception, bool>(LightExceptions.IsLightException).Method, _lastValue);
        private static readonly Expression _lastException = Expression.Call(new Func<object, Exception>(LightExceptions.GetLightException).Method, _lastValue);
        private readonly LabelTarget _returnLabel = Expression.Label(typeof(object), GetEhLabelName("ehUnwind"));
#if DEBUG
        private static int _curLabel;
#endif

        internal Expression Rewrite(Expression expr) {
            var lambda = expr as LambdaExpression;
            if (lambda != null) {
                // just re-write the body, no need for an outer return label
                return Expression.Lambda(
                    Rewrite(lambda.Body),
                    lambda.Name,
                    lambda.TailCall,
                    lambda.Parameters
                );
            }

            // add a label so we can branch when we're propagating out the exception value
            expr = Visit(expr);
            if (expr.Type == typeof(void)) {
                expr = Expression.Block(expr, Utils.Constant(null));
            }

            return new LightExceptionRewrittenCode(_returnLabel, expr);
        }

        /// <summary>
        /// Class used to be avoid overhead of creating expression trees when we're usually 
        /// </summary>
        class LightExceptionRewrittenCode : Expression, IInstructionProvider {
            private readonly LabelTarget _returnLabel;
            private readonly Expression _body;

            public LightExceptionRewrittenCode(LabelTarget target, Expression body) {
                _returnLabel = target;
                _body = body;
            }

            public override Expression Reduce() {
                return Expression.Block(typeof(object), _lastValueParamArray, new ReadOnlyCollectionBuilder<Expression> {
                    Expression.Label(_returnLabel, _body)
                });
            }

            public override bool CanReduce {
                get { return true; }
            }

            public override ExpressionType NodeType {
                get { return ExpressionType.Extension; }
            }

            public override Type Type {
                get { return typeof(object); }
            }

            #region IInstructionProvider Members

            public void AddInstructions(LightCompiler compiler) {
                compiler.PushLabelBlock(LabelScopeKind.Block);

                var local = compiler.Locals.DefineLocal(_lastValue, compiler.Instructions.Count);
                var label = compiler.DefineLabel(_returnLabel);
                compiler.Compile(_body);
                compiler.Instructions.MarkLabel(label.GetLabel(compiler));
                compiler.Locals.UndefineLocal(local, compiler.Instructions.Count);

                compiler.PopLabelBlock(LabelScopeKind.Block);
            }

            #endregion
        }

        protected override Expression VisitLambda<T>(Expression<T> node) {
            // new top-level code, don't rewrite inner methods.
            return node;
        }

        protected override Expression VisitExtension(Expression node) {
            // check if the extension is light exception aware.
            ILightExceptionAwareExpression lightAware = node as ILightExceptionAwareExpression;
            if (lightAware != null) {
                var newNode = lightAware.ReduceForLightExceptions();
                if (newNode != node) {
                    return CheckExpression(Visit(newNode), node.Type);
                }                
            }

            return base.VisitExtension(node);
        }

        protected override Expression VisitDynamic(DynamicExpression node) {
            // check if we have a light exception aware binder, and if so upgrade
            // to light exceptions.
            var lightBinder = node.Binder as ILightExceptionBinder;
            if (lightBinder != null) {
                var newBinder = lightBinder.GetLightExceptionBinder();
                if (newBinder != node.Binder) {
                    return CheckExpression(
                        Expression.Dynamic(
                            newBinder,
                            node.Type,
                            base.Visit(node.Arguments)
                        ),
                        node.Type
                    );
                }
            }

            return base.VisitDynamic(node);
        }

        protected override Expression VisitTry(TryExpression node) {
            if (node.Fault != null) {
                throw new NotSupportedException();
            } else if (node.Handlers != null && node.Handlers.Count > 0) {
                return RewriteTryCatch(node);
            } else {
                // we don't yet support re-writing finally bodies for light exceptions
                var body = Visit(node.Body);
                if (body != node.Body) {
                    return RewriteTryFinally(body, node.Finally);
                }
                return node;
            }
        }

        private Expression RewriteTryFinally(Expression tryBody, Expression finallyBody) {
            // TODO: Make this work
            //var finallyRan = Expression.Parameter(typeof(bool), "finallyRan");
            //return Expression.Block(
            //    new[] { finallyRan },
            //    Expression.TryFinally(
            //        Expression.Block(
            //            tryBody,
            //            Expression.Assign(finallyRan, Expression.Constant(true)),
            //            Visit(finallyBody)
            //        ),
            //        Expression.Condition(
            //             Expression.Not(finallyRan),
            //             tryBody,
            //             Expression.Default(finallyBody.Type)
            //        )
            //    )
            //);
                        
            return Expression.TryFinally(tryBody, finallyBody);
        }

        private static string GetEhLabelName(string baseName) {
#if DEBUG
            return baseName + _curLabel++;
#else
            return baseName;
#endif
        }

        protected override Expression VisitUnary(UnaryExpression node) {
            if (node.NodeType == ExpressionType.Throw) {
                Expression exception = node.Operand ?? _rethrow;

                return Expression.Block(
                    Expression.Assign(_lastValue, LightExceptions.Throw(Visit(exception))),
                    PropagateException(node.Type)
                );
            }

            return base.VisitUnary(node);
        }
        
        private Expression RewriteTryBody(TryExpression node, LabelTarget ehLabel) {
            Expression body;
            LabelTarget prevHandler = _currentHandler;
            _currentHandler = ehLabel;
            try {
                body = Visit(node.Body);
            } finally {
                _currentHandler = prevHandler;
            }

            return body;
        }

        private CatchBlock[] VisitHandlers(TryExpression node, bool realCatch) {
            CatchBlock[] handlers = new CatchBlock[node.Handlers.Count];
            for (int i = 0; i < node.Handlers.Count; i++) {
                var handler = node.Handlers[i];

                ParameterExpression oldRethrow = _rethrow;
                try {
                    if (handler.Variable == null) {
                        ParameterExpression rethrow = _rethrow = Expression.Parameter(handler.Test, "$exception");
                        handlers[i] = Expression.Catch(
                            rethrow,
                            TrackCatch(Visit(handler.Body), rethrow, realCatch),
                            Visit(handler.Filter)
                        );
                    } else {
                        ParameterExpression rethrow = _rethrow = Expression.Parameter(typeof(Exception), "$exception");
                        handlers[i] = Expression.Catch(
                            handler.Variable,
                            Expression.Block(
                                new[] { rethrow },
                                Expression.Assign(rethrow, handler.Variable),
                                TrackCatch(Visit(handler.Body), rethrow, realCatch)
                            ),
                            Visit(handler.Filter)
                        );
                    }
                } finally {
                    _rethrow = oldRethrow;
                }
            }            

            return handlers;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "realCatch"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "exception")]
        private Expression TrackCatch(Expression expression, Expression exception, bool realCatch) {
#if DEBUG
            if (realCatch) {
                return Expression.Block(
                    Expression.Call(
                        typeof(ScriptingRuntimeHelpers).GetMethod("NoteException"),
                        exception
                    ),
                    expression
                );
            }
#endif
            return expression;
        }

        private Expression RewriteTryCatch(TryExpression node) {
            // we inline the catch handlers after the catch blocks and use labels
            // to branch around and propagate the result out.
            // 
            // goto tryDone(
            //    try {
            //      if (LightExceptions.IsLightException(_lastValue = someCall)) {
            //          goto ehLabel;
            //      } else {
            //          _lastValue
            //      }
            //    } catch(Exception e) {
            //        handler;
            //    }
            // )
            //
            // ehLabel:
            //  if ((e = GetLightException(_lastValue) as Exception)) != null) {
            //      handler;
            //  } else {
            //      // unhandled exception, propagate up, either:
            //      goto _returnValue(_lastValue);
            //      // if we weren't in a nested exception handler or if we were:
            //      goto _ehLabelOuter;
            //  }
            // tryDone:
            //  // yields either the value of the try block or the real catch block
            //  // from the goto tryDone or it gets it's default value from ehLabel
            //  // which is branched to when an exception is detected.
            //

            var ehLabel = Expression.Label(typeof(void), GetEhLabelName("lightEh"));
            var tryDoneLabel = Expression.Label(node.Body.Type, GetEhLabelName("tryDone"));

            Expression body = Expression.Block(
                Expression.Goto(
                    tryDoneLabel,
                    Expression.TryCatch(RewriteTryBody(node, ehLabel), VisitHandlers(node, true))
                ),
                Expression.Label(
                    tryDoneLabel,
                    Expression.Block(
                        Expression.Label(ehLabel),
                        Utils.Convert(
                            LightCatch(VisitHandlers(node, false)),
                            node.Body.Type
                        )
                    )
                )
            );

            // if we have a finally wrap the whole thing up now.
            if (node.Finally != null) {
                body = RewriteTryFinally(body, node.Finally);
            }

            return body;
        }

        private Expression LightCatch(CatchBlock[] handlers) {
            // start off with the rethrow - just a return of the value.  Then
            // walk backwards and add each handler as a test in a cascading
            // conditional.  If all tests fail, we make it back to the return value.
            Expression rethrow = PropagateException(typeof(object));
            
            for (int i = handlers.Length - 1; i >= 0; i--) {
                var curHandler = handlers[i];

                Expression test = Expression.NotEqual(
                    Expression.Assign(
                        curHandler.Variable,
                        Expression.TypeAs(_lastException, curHandler.Test)
                    ),
                    Expression.Constant(null)
                );

                if (handlers[i].Filter != null) {
                    throw new NotSupportedException("filters for light exceptions");
                    // we could do this but the emulation wouldn't be perfect when filters
                    // run in relation to finallys
                    //test = Expression.AndAlso(test, handlers[i].Filter);
                }

                rethrow = Expression.Block(
                    new[] { curHandler.Variable },
                    Expression.Condition(
                        test,
                        Utils.Convert(handlers[i].Body, typeof(object)),
                        rethrow
                    )
                );
            }
            
            return rethrow;
        }

       
        /// <summary>
        /// Adds light exception handling to the provided expression which
        /// is light exception aware.
        /// </summary>
        private Expression CheckExpression(Expression expr, Type retType) {
            if (expr.Type == typeof(object)) {
                // if we're not object then we can't be a light exception
                expr = new LightExceptionCheckExpression(
                    expr, 
                    retType, 
                    _currentHandler ?? _returnLabel, 
                    _currentHandler == null ? _lastValue : null
                );
            }
           
            return expr;
        }

        class LightExceptionCheckExpression : Expression, IInstructionProvider {
            private readonly Type _retType;
            private readonly LabelTarget _target;
            private readonly Expression _lastValue, _expr;

            public LightExceptionCheckExpression(Expression expr, Type retType, LabelTarget currentHandler, ParameterExpression lastValue) {
                _expr = expr;
                _retType = retType;
                _target = currentHandler;
                _lastValue = lastValue;
            }

            public override Expression Reduce() {
                return Expression.Condition(
                    Expression.Block(
                        Expression.Assign(LightExceptionRewriter._lastValue, _expr),
                        IsLightExceptionExpression.Instance
                    ),
                    Expression.Goto(_target, _lastValue, _retType),
                    Utils.Convert(LightExceptionRewriter._lastValue, _retType)
                );
            }

            public override bool CanReduce {
                get { return true; }
            }

            public override ExpressionType NodeType {
                get { return ExpressionType.Extension; }
            }

            public override Type Type {
                get { return _retType; }
            }

            #region IInstructionProvider Members

            public void AddInstructions(LightCompiler compiler) {
                var endOfTrue = compiler.Instructions.MakeLabel();
                var endOfFalse = compiler.Instructions.MakeLabel();

                // condition
                compiler.Compile(_expr);
                compiler.CompileSetVariable(LightExceptionRewriter._lastValue, false);
                compiler.Instructions.Emit(IsLightExceptionInstruction.Instance);

                // true - an exception occured
                compiler.Instructions.EmitBranchFalse(endOfTrue);
                
                if (_lastValue != null) {
                    compiler.CompileParameterExpression(_lastValue);
                }

                compiler.Instructions.EmitGoto(
                    compiler.GetBranchLabel(_target),
                    _retType != typeof(void),
                    _lastValue != null && _lastValue.Type != typeof(void)
                );
                compiler.Instructions.EmitBranch(endOfFalse, false, true);

                // false - no exception                
                compiler.Instructions.MarkLabel(endOfTrue);
                compiler.CompileParameterExpression(LightExceptionRewriter._lastValue);
                compiler.Instructions.MarkLabel(endOfFalse);
            }

            #endregion
        }

        private Expression PropagateException(Type retType) {
            if (_currentHandler == null) {
                // no eh blocks, we propagate the value up
                return Expression.Goto(_returnLabel, _lastValue, retType);
            }

            // branch to current exception handler
            return Expression.Goto(_currentHandler, retType);
        }

        class IsLightExceptionExpression : Expression, IInstructionProvider {
            public static IsLightExceptionExpression Instance = new IsLightExceptionExpression();

            private IsLightExceptionExpression() { }

            public override Expression Reduce() {
                return _isLightExExpr;
            }

            public override Type Type {
                get {
                    return typeof(bool);
                }
            }

            public override ExpressionType NodeType {
                get {
                    return ExpressionType.Extension;
                }
            }

            public override bool CanReduce {
                get {
                    return true;
                }
            }

            #region IInstructionProvider Members

            public void AddInstructions(LightCompiler compiler) {
                compiler.Compile(_lastValue);
                compiler.Instructions.Emit(IsLightExceptionInstruction.Instance);
            }

            #endregion
        }

        class IsLightExceptionInstruction : Instruction {
            public static IsLightExceptionInstruction Instance = new IsLightExceptionInstruction();

            private IsLightExceptionInstruction() { }

            public override int ConsumedStack {
                get { return 1; }
            }

            public override int ProducedStack {
                get { return 1; }
            }

            public override int Run(InterpretedFrame frame) {
                frame.Push(ScriptingRuntimeHelpers.BooleanToObject(LightExceptions.IsLightException(frame.Pop())));
                return +1;
            }
        }

    }
}
