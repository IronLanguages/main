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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// When finding a yield return or yield break, this rewriter flattens out
    /// containing blocks, scopes, and expressions with stack state. All
    /// scopes encountered have their variables promoted to the generator's
    /// closure, so they survive yields.
    /// </summary>
    internal sealed class GeneratorRewriter : ExpressionVisitor {
        // These two constants are used internally. They should not conflict
        // with valid yield states.
        private const int GotoRouterYielding = 0;
        private const int GotoRouterNone = -1;
        // The state of the generator before it starts and when it's done
        internal const int NotStarted = -1;
        internal const int Finished = 0;

        private sealed class YieldMarker {
            // Note: Label can be mutated as we generate try blocks
            internal LabelTarget Label = Expression.Label();
            internal readonly int State;

            internal YieldMarker(int state) {
                State = state;
            }
        }

        private sealed class LabelInfo {
            internal readonly LabelTarget NewLabel;
            internal readonly ParameterExpression Temp;

            internal LabelInfo(LabelTarget old) {
                NewLabel = Expression.Label(old.Name);
                Temp = Expression.Parameter(old.Type, old.Name);
            }
        }

        private readonly GeneratorExpression _generator;
        private readonly ParameterExpression _current;
        private readonly ParameterExpression _state;

        // The one return label, or more than one if we're in a finally
        private readonly Stack<LabelTarget> _returnLabels = new Stack<LabelTarget>();
        private ParameterExpression _gotoRouter;
        private bool _inTryWithFinally;

        private readonly List<YieldMarker> _yields = new List<YieldMarker>();

        private List<int> _debugCookies;

        private readonly HashSet<ParameterExpression> _vars = new HashSet<ParameterExpression>();

        // Possible optimization: reuse temps. Requires scoping them correctly,
        // and then storing them back in a free list
        private readonly List<ParameterExpression> _temps = new List<ParameterExpression>();

        // Variables used to support goto-with-value
        private Dictionary<LabelTarget, LabelInfo> _labelTemps;

        internal GeneratorRewriter(GeneratorExpression generator) {
            _generator = generator;
            _state = Expression.Parameter(typeof(int).MakeByRefType(), "state");
            _current = Expression.Parameter(_generator.Target.Type.MakeByRefType(), "current");
            _returnLabels.Push(Expression.Label());
            _gotoRouter = Expression.Variable(typeof(int), "$gotoRouter");
        }

        internal Expression Reduce() {
            // Visit body
            Expression body = Visit(_generator.Body);
            Debug.Assert(_returnLabels.Count == 1);

            // Add the switch statement to the body
            int count = _yields.Count;
            var cases = new SwitchCase[count + 1];
            for (int i = 0; i < count; i++) {
                cases[i] = Expression.SwitchCase(Expression.Goto(_yields[i].Label), AstUtils.Constant(_yields[i].State));
            }
            cases[count] = Expression.SwitchCase(Expression.Goto(_returnLabels.Peek()), AstUtils.Constant(Finished));

            Type generatorNextOfT = typeof(GeneratorNext<>).MakeGenericType(_generator.Target.Type);

            // Create the lambda for the GeneratorNext<T>, hoisting variables
            // into a scope outside the lambda
            var allVars = new List<ParameterExpression>(_vars);
            allVars.AddRange(_temps);

            // Collect temps that don't have to be closed over
            var innerTemps = new ReadOnlyCollectionBuilder<ParameterExpression>(1 + (_labelTemps != null ? _labelTemps.Count : 0));
            innerTemps.Add(_gotoRouter);
            if (_labelTemps != null) {
                foreach (LabelInfo info in _labelTemps.Values) {
                    innerTemps.Add(info.Temp);
                }
            }

            body = Expression.Block(
                allVars,
                Expression.Lambda(
                    generatorNextOfT,
                    Expression.Block(
                        innerTemps,
                        Expression.Switch(Expression.Assign(_gotoRouter, _state), cases),
                        body,
                        Expression.Assign(_state, AstUtils.Constant(Finished)),
                        Expression.Label(_returnLabels.Peek())
                    ),
                    _generator.Name,
                    new ParameterExpression[] { _state, _current }
                )
            );

            // Enumerable factory takes Func<GeneratorNext<T>> instead of GeneratorNext<T>
            if (_generator.IsEnumerable) {
                body = Expression.Lambda(body);
            }

            // We can't create a ConstantExpression of _debugCookies array here because we walk the tree
            // after constants have already been rewritten.  Instead we create a NewArrayExpression node
            // which initializes the array with contents from _debugCookies
            Expression debugCookiesArray = null;
            if (_debugCookies != null) {
                Expression[] debugCookies = new Expression[_debugCookies.Count];
                for(int i=0; i < _debugCookies.Count; i++)
                    debugCookies[i] = AstUtils.Constant(_debugCookies[i]);

                debugCookiesArray = Expression.NewArrayInit(
                    typeof(int),
                    debugCookies);
            }

            // Generate a call to ScriptingRuntimeHelpers.MakeGenerator<T>(args)
            return Expression.Call(
                typeof(ScriptingRuntimeHelpers),
                "MakeGenerator",
                new[] { _generator.Target.Type },
                (debugCookiesArray != null)
                    ? new[] { body, debugCookiesArray }
                    : new[] { body }
            );
        }

        private YieldMarker GetYieldMarker(YieldExpression node) {
            YieldMarker result = new YieldMarker(_yields.Count + 1);
            _yields.Add(result);
            if (node.YieldMarker != -1) {
                if (_debugCookies == null) {
                    _debugCookies = new List<int>(1);
                    _debugCookies.Add(Int32.MaxValue);
                }
                _debugCookies.Insert(result.State, node.YieldMarker);
            } else if (_debugCookies != null) {
                _debugCookies.Insert(result.State, Int32.MaxValue);
            }
            return result;
        }

        /// <summary>
        /// Makes an assignment to this variable. Pushes the assignment as far
        /// into the right side as possible, to allow jumps into it.
        /// </summary>
        private Expression MakeAssign(ParameterExpression variable, Expression value) {
            // TODO: this is not complete.
            // It may end up generating a bad tree if any of these nodes
            // contain yield and return a value: Switch, Loop, Goto, or Label.
            // Those are not supported, but we can't throw here because we may
            // end up disallowing valid uses (if some other expression contains
            // yield, but not this one).
            switch (value.NodeType) {
                case ExpressionType.Block:
                    return MakeAssignBlock(variable, value);
                case ExpressionType.Conditional:
                    return MakeAssignConditional(variable, value);
                case ExpressionType.Label:
                    return MakeAssignLabel(variable, (LabelExpression)value);
            }
            return Expression.Assign(variable, value);
        }

        struct GotoRewriteInfo {
            public readonly ParameterExpression Variable;
            public readonly LabelTarget VoidTarget;

            public GotoRewriteInfo(ParameterExpression variable, LabelTarget voidTarget) {
                Variable = variable;
                VoidTarget = voidTarget;
            }
        }

        private Expression MakeAssignLabel(ParameterExpression variable, LabelExpression value) {
            GotoRewriteInfo curVariable = new GotoRewriteInfo(variable, Expression.Label(value.Target.Name + "_voided"));

            var defaultValue = new GotoRewriter(this, curVariable, value.Target).Visit(value.DefaultValue);

            return MakeAssignLabel(variable, curVariable, defaultValue);
        }

        private Expression MakeAssignLabel(ParameterExpression variable, GotoRewriteInfo curVariable, Expression defaultValue) {
            return Expression.Label(
                curVariable.VoidTarget,
                MakeAssign(variable, defaultValue)
            );
        }

        class GotoRewriter : ExpressionVisitor {
            private readonly GotoRewriteInfo _gotoInfo;
            private readonly LabelTarget _target;
            private readonly GeneratorRewriter _rewriter;

            public GotoRewriter(GeneratorRewriter rewriter, GotoRewriteInfo gotoInfo, LabelTarget target) {
                _gotoInfo = gotoInfo;
                _target = target;
                _rewriter = rewriter;
            }

            protected override Expression VisitGoto(GotoExpression node) {
                if (node.Target == _target) {
                    return Expression.Goto(
                        _gotoInfo.VoidTarget,
                        Expression.Block(
                            _rewriter.MakeAssign(_gotoInfo.Variable, node.Value),
                            Expression.Default(typeof(void))
                        ),
                        node.Type
                    );
                }
                return base.VisitGoto(node);
            }
        }

        private Expression MakeAssignBlock(ParameterExpression variable, Expression value) {
            var node = (BlockExpression)value;
            var newBlock = new ReadOnlyCollectionBuilder<Expression>(node.Expressions);

            Expression blockRhs = newBlock[newBlock.Count - 1];
            if (blockRhs.NodeType == ExpressionType.Label) {
                var label = (LabelExpression)blockRhs;
                GotoRewriteInfo curVariable = new GotoRewriteInfo(variable, Expression.Label(label.Target.Name + "_voided"));

                var rewriter = new GotoRewriter(this, curVariable, label.Target);
                for (int i = 0; i < newBlock.Count - 1; i++) {
                    newBlock[i] = rewriter.Visit(newBlock[i]);
                }

                newBlock[newBlock.Count - 1] = MakeAssignLabel(variable, curVariable, rewriter.Visit(label.DefaultValue));
            } else {
                newBlock[newBlock.Count - 1] = MakeAssign(variable, newBlock[newBlock.Count - 1]);
            }
            
            return Expression.Block(node.Variables, newBlock);
        }

        private Expression MakeAssignConditional(ParameterExpression variable, Expression value) {
            var node = (ConditionalExpression)value;
            return Expression.Condition(node.Test, MakeAssign(variable, node.IfTrue), MakeAssign(variable, node.IfFalse));
        }

        #region VisitTry

        protected override Expression VisitTry(TryExpression node) {
            int startYields = _yields.Count;

            bool savedInTryWithFinally = _inTryWithFinally;
            if (node.Finally != null || node.Fault != null) {
                _inTryWithFinally = true;
            }
            Expression @try = Visit(node.Body);
            int tryYields = _yields.Count;

            IList<CatchBlock> handlers = Visit(node.Handlers, VisitCatchBlock);
            int catchYields = _yields.Count;

            // push a new return label in case the finally block yields
            _returnLabels.Push(Expression.Label());
            // only one of these can be non-null
            Expression @finally = Visit(node.Finally);
            Expression fault = Visit(node.Fault);
            LabelTarget finallyReturn = _returnLabels.Pop();
            int finallyYields = _yields.Count;

            _inTryWithFinally = savedInTryWithFinally;

            if (@try == node.Body &&
                handlers == node.Handlers &&
                @finally == node.Finally &&
                fault == node.Fault) {
                return node;
            }

            // No yields, just return
            if (startYields == _yields.Count) {
                return Expression.MakeTry(null, @try, @finally, fault, handlers);
            }

            if (fault != null && finallyYields != catchYields) {
                // No one needs this yet, and it's not clear how we should get back to
                // the fault
                throw new NotSupportedException("yield in fault block is not supported");
            }

            // If try has yields, we need to build a new try body that
            // dispatches to the yield labels
            var tryStart = Expression.Label();
            if (tryYields != startYields) {
                @try = Expression.Block(MakeYieldRouter(node.Body.Type, startYields, tryYields, tryStart), @try);
            }

            // Transform catches with yield to deferred handlers
            if (catchYields != tryYields) {
                var block = new List<Expression>();

                block.Add(MakeYieldRouter(node.Body.Type, tryYields, catchYields, tryStart));
                block.Add(null); // empty slot to fill in later

                for (int i = 0, n = handlers.Count; i < n; i++) {
                    CatchBlock c = handlers[i];

                    if (c == node.Handlers[i]) {
                        continue;
                    }

                    if (handlers.IsReadOnly) {
                        handlers = handlers.ToArray();
                    }

                    // the variable that will be scoped to the catch block
                    var exceptionVar = Expression.Variable(c.Test, null);

                    // the variable that the catch block body will use to
                    // access the exception. We reuse the original variable if
                    // the catch block had one. It needs to be hoisted because
                    // the catch might contain yields.
                    var deferredVar = c.Variable ?? Expression.Variable(c.Test, null);
                    _vars.Add(deferredVar);

                    // We need to ensure that filters can access the exception
                    // variable
                    Expression filter = c.Filter;
                    if (filter != null && c.Variable != null) {
                        filter = Expression.Block(new[] { c.Variable }, Expression.Assign(c.Variable, exceptionVar), filter);
                    }

                    // catch (ExceptionType exceptionVar) {
                    //     deferredVar = exceptionVar;
                    // }
                    handlers[i] = Expression.Catch(
                        exceptionVar,
                        Expression.Block(
                            Expression.Assign(deferredVar, exceptionVar), 
                            Expression.Default(node.Body.Type)
                        ),
                        filter
                    );

                    // We need to rewrite rethrows into "throw deferredVar"
                    var catchBody = new RethrowRewriter { Exception = deferredVar }.Visit(c.Body);
                    
                    // if (deferredVar != null) {
                    //     ... catch body ...
                    // }
                    block.Add(
                        Expression.Condition(
                            Expression.NotEqual(deferredVar, AstUtils.Constant(null, deferredVar.Type)),
                            catchBody,
                            Expression.Default(node.Body.Type)
                        )
                    );
                }

                block[1] = Expression.MakeTry(null, @try, null, null, new ReadOnlyCollection<CatchBlock>(handlers));
                @try = Expression.Block(block);
                handlers = new CatchBlock[0]; // so we don't reuse these
            }

            if (finallyYields != catchYields) {
                // We need to add a catch block to save the exception, so we
                // can rethrow in case there is a yield in the finally. Also,
                // add logic for returning. It looks like this:
                //
                // try { ... } catch (Exception all) { saved = all; }
                // finally {
                //  if (_finallyReturnVar) goto finallyReturn;
                //   ...
                //   if (saved != null) throw saved;
                //   finallyReturn:
                // }
                // if (_finallyReturnVar) goto _return;

                // We need to add a catch(Exception), so if we have catches,
                // wrap them in a try
                if (handlers.Count > 0) {
                    @try = Expression.MakeTry(null, @try, null, null, handlers);
                    handlers = new CatchBlock[0];
                }

                // NOTE: the order of these routers is important
                // The first call changes the labels to all point at "tryEnd",
                // so the second router will jump to "tryEnd"
                var tryEnd = Expression.Label();
                Expression inFinallyRouter = MakeYieldRouter(node.Body.Type, catchYields, finallyYields, tryEnd);
                Expression inTryRouter = MakeYieldRouter(node.Body.Type, catchYields, finallyYields, tryStart);

                var all = Expression.Variable(typeof(Exception), "e");
                var saved = Expression.Variable(typeof(Exception), "$saved$" + _temps.Count);
                _temps.Add(saved);
                @try = Expression.Block(
                    Expression.TryCatchFinally(
                        Expression.Block(
                            inTryRouter,
                            @try,
                            Expression.Assign(saved, AstUtils.Constant(null, saved.Type)),
                            Expression.Label(tryEnd)
                        ),
                        Expression.Block(
                            MakeSkipFinallyBlock(finallyReturn),
                            inFinallyRouter,
                            @finally,
                            Expression.Condition(
                                Expression.NotEqual(saved, AstUtils.Constant(null, saved.Type)),
                                Expression.Throw(saved),
                                Utils.Empty()
                            ),
                            Expression.Label(finallyReturn)
                        ),
                        Expression.Catch(all, Utils.Void(Expression.Assign(saved, all)))
                    ),
                    Expression.Condition(
                        Expression.Equal(_gotoRouter, AstUtils.Constant(GotoRouterYielding)),
                        Expression.Goto(_returnLabels.Peek()),
                        Utils.Empty()
                    )
                );

                @finally = null;
            } else if (@finally != null) {
                // try or catch had a yield, modify finally so we can skip over it
                @finally = Expression.Block(
                    MakeSkipFinallyBlock(finallyReturn),
                    @finally,
                    Expression.Label(finallyReturn)
                );
            }

            // Make the outer try, if needed
            if (handlers.Count > 0 || @finally != null || fault != null) {
                @try = Expression.MakeTry(null, @try, @finally, fault, handlers);
            }

            return Expression.Block(Expression.Label(tryStart), @try);
        }

        private class RethrowRewriter : ExpressionVisitor {
            internal ParameterExpression Exception;

            protected override Expression VisitUnary(UnaryExpression node) {
                if (node.NodeType == ExpressionType.Throw && node.Operand == null) {
                    return Expression.Throw(Exception, node.Type);
                }
                return base.VisitUnary(node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node) {
                return node; // don't recurse into lambdas 
            }

            protected override Expression VisitTry(TryExpression node) {
                return node; // don't recurse into other try's
            }
        }

        // Skip the finally block if we are yielding, but not if we're doing a
        // yield break
        private Expression MakeSkipFinallyBlock(LabelTarget target) {
            return Expression.Condition(
                Expression.AndAlso(
                    Expression.Equal(_gotoRouter, AstUtils.Constant(GotoRouterYielding)),
                    Expression.NotEqual(_state, AstUtils.Constant(Finished))
                ),
                Expression.Goto(target),
                Utils.Empty()
            );
        }

        // This is copied from the base implementation. 
        // Just want to make sure we disallow yield in filters
        protected override CatchBlock VisitCatchBlock(CatchBlock node) {
            ParameterExpression v = VisitAndConvert(node.Variable, "VisitCatchBlock");
            int yields = _yields.Count;
            Expression f = Visit(node.Filter);
            if (yields != _yields.Count) {
                // No one needs this yet, and it's not clear what it should even do
                throw new NotSupportedException("yield in filter is not allowed");
            }
            Expression b = Visit(node.Body);
            if (v == node.Variable && b == node.Body && f == node.Filter) {
                return node;
            }
            return Expression.MakeCatchBlock(node.Test, v, b, f);
        }

        #endregion

        private SwitchExpression MakeYieldRouter(Type type, int start, int end, LabelTarget newTarget) {
            Debug.Assert(end > start);
            var cases = new SwitchCase[end - start];
            for (int i = start; i < end; i++) {
                YieldMarker y = _yields[i];
                cases[i - start] = Expression.SwitchCase(Expression.Goto(y.Label, type), AstUtils.Constant(y.State));
                // Any jumps from outer switch statements should go to the this
                // router, not the original label (which they cannot legally jump to)
                y.Label = newTarget;
            }
            return Expression.Switch(_gotoRouter, Expression.Default(type), cases);
        }

        protected override Expression VisitExtension(Expression node) {
            var yield = node as YieldExpression;
            if (yield != null) {
                return VisitYield(yield);
            }

            var ffc = node as FinallyFlowControlExpression;
            if (ffc != null) {
                return Visit(node.ReduceExtensions());
            }

            // We have to reduce to ensure proper stack spilling.
            return Visit(node.ReduceExtensions());
        }

        private Expression VisitYield(YieldExpression node) {
            if (node.Target != _generator.Target) {
                throw new InvalidOperationException("yield and generator must have the same LabelTarget object");
            }

            var value = Visit(node.Value);

            var block = new ReadOnlyCollectionBuilder<Expression>();
            if (value == null) {
                // Yield break
                block.Add(Expression.Assign(_state, AstUtils.Constant(Finished)));
                if (_inTryWithFinally) {
                    block.Add(Expression.Assign(_gotoRouter, AstUtils.Constant(GotoRouterYielding)));
                }
                block.Add(Expression.Goto(_returnLabels.Peek()));
                return Expression.Block(block);
            }

            // Yield return
            block.Add(MakeAssign(_current, value));
            YieldMarker marker = GetYieldMarker(node);
            block.Add(Expression.Assign(_state, AstUtils.Constant(marker.State)));
            if (_inTryWithFinally) {
                block.Add(Expression.Assign(_gotoRouter, AstUtils.Constant(GotoRouterYielding)));
            }
            block.Add(Expression.Goto(_returnLabels.Peek()));
            block.Add(Expression.Label(marker.Label));
            block.Add(Expression.Assign(_gotoRouter, AstUtils.Constant(GotoRouterNone)));
            block.Add(Utils.Empty());
            return Expression.Block(block);
        }

        protected override Expression VisitBlock(BlockExpression node) {
            int yields = _yields.Count;
            var b = Visit(node.Expressions);
            if (b == node.Expressions) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.Block(node.Type, node.Variables, b);
            }

            // save the variables for later
            // (they'll be hoisted outside of the lambda)
            _vars.UnionWith(node.Variables);

            // Return a new block expression with the rewritten body except for that
            // all the variables are removed.
            return Expression.Block(node.Type, b);
        }

        protected override Expression VisitLambda<T>(Expression<T> node) {
            // don't recurse into nested lambdas
            return node;
        }

        #region goto with value support

        //
        // The rewriter assigns expressions into temporaries. If the expression is a label with a value the resulting tree would be illegal
        // since we cannot jump into RHS of an assignment. Hence we need to eliminate labels and gotos with value. We just need to rewrite 
        // those that are used in MakeAssign but it is easier to rewrite all. 
        //
        // var = label[L](value1)
        // ...
        // goto[L](value2)
        //
        // ->
        //
        // { tmp = value1; label[L]: var = tmp }
        // ...
        // { tmp = value2; goto[L] }
        //

        protected override Expression VisitLabel(LabelExpression node) {
            if (node.Target.Type == typeof(void)) {
                return base.VisitLabel(node);
            }

            LabelInfo info = GetLabelInfo(node.Target);
            return Expression.Block(
                MakeAssign(info.Temp, Visit(node.DefaultValue)),
                Expression.Label(info.NewLabel),
                info.Temp
            );
        }

        protected override Expression VisitGoto(GotoExpression node) {
            if (node.Target.Type == typeof(void)) {
                return base.VisitGoto(node);
            }

            LabelInfo info = GetLabelInfo(node.Target);
            return Expression.Block(
                MakeAssign(info.Temp, Visit(node.Value)),
                Expression.MakeGoto(node.Kind, info.NewLabel, null, node.Type)
            );
        }

        private LabelInfo GetLabelInfo(LabelTarget label) {
            if (_labelTemps == null) {
                _labelTemps = new Dictionary<LabelTarget, LabelInfo>();
            }

            LabelInfo temp;
            if (!_labelTemps.TryGetValue(label, out temp)) {
                _labelTemps[label] = temp = new LabelInfo(label);
            }

            return temp;
        }

        #endregion

        #region stack spilling (to permit yield in the middle of an expression)

        /// <summary>
        /// Returns true if the expression remains constant no matter when it is evaluated.
        /// </summary>
        private static bool IsConstant(Expression e) {
            return e is ConstantExpression;
        }

        private Expression ToTemp(ReadOnlyCollectionBuilder<Expression> block, Expression e) {
            Debug.Assert(e != null);
            if (IsConstant(e)) {
                return e;
            }

            var temp = Expression.Variable(e.Type, "generatorTemp" + _temps.Count);
            _temps.Add(temp);
            block.Add(MakeAssign(temp, e));
            return temp;
        }

        private ReadOnlyCollection<Expression> ToTemp(ReadOnlyCollectionBuilder<Expression> block, ICollection<Expression> args) {
            var spilledArgs = new ReadOnlyCollectionBuilder<Expression>(args.Count);
            foreach (var arg in args) {
                spilledArgs.Add(ToTemp(block, arg));
            }
            return spilledArgs.ToReadOnlyCollection();
        }

        private Expression Rewrite(Expression node, ReadOnlyCollection<Expression> arguments, 
            Func<ReadOnlyCollection<Expression>, Expression> factory) {
            return Rewrite(node, null, arguments, (e, args) => factory(args));
        }

        private Expression Rewrite(Expression node, Expression expr, ReadOnlyCollection<Expression> arguments,
            Func<Expression, ReadOnlyCollection<Expression>, Expression> factory) {

            int yields = _yields.Count;
            Expression newExpr = expr != null ? Visit(expr) : null;

            // TODO(opt): If we tracked the last argument that contains yield we wouldn't need to spill the rest of the arguments into locals.
            ReadOnlyCollection<Expression> newArgs = Visit(arguments);

            if (newExpr == expr && newArgs == arguments) {
                return node;
            }

            if (yields == _yields.Count) {
                return factory(newExpr, newArgs);
            }

            var block = new ReadOnlyCollectionBuilder<Expression>(newArgs.Count + 1);

            if (newExpr != null) {
                newExpr = ToTemp(block, newExpr);
            }

            var spilledArgs = ToTemp(block, newArgs);
            block.Add(factory(newExpr, spilledArgs));

            return Expression.Block(block);
        }

        // We need to rewrite unary expressions as well since ETs don't support jumping into unary expressions. 
        private Expression Rewrite(Expression node, Expression expr, Func<Expression, Expression> factory) {
            int yields = _yields.Count;
            Expression newExpr = Visit(expr);
            if (newExpr == expr) {
                return node;
            }

            if (yields == _yields.Count || IsConstant(newExpr)) {
                return factory(newExpr);
            }

            var block = new ReadOnlyCollectionBuilder<Expression>(2);
            newExpr = ToTemp(block, newExpr);
            block.Add(factory(newExpr));
            return Expression.Block(block);
        }

        private Expression Rewrite(Expression node, Expression expr1, Expression expr2, Func<Expression, Expression, Expression> factory) {
            int yields = _yields.Count;
            Expression newExpr1 = Visit(expr1);
            int yields1 = _yields.Count;
            Expression newExpr2 = Visit(expr2);
            if (newExpr1 == expr1 && newExpr2 == expr2) {
                return node;
            }

            // f({expr}, {expr})
            if (yields == _yields.Count) {
                return factory(newExpr1, newExpr2);
            }

            var block = new ReadOnlyCollectionBuilder<Expression>(3);

            // f({yield}, {expr}) -> { t = {yield}; f(t, {expr}) }
            // f({const}, yield) -> { t = {yield}; f({const}, t) }
            // f({expr|yield}, {yield}) -> { t1 = {expr|yeild}, t2 = {yield}; f(t1, t2) }

            newExpr1 = ToTemp(block, newExpr1);
                
            if (yields1 != _yields.Count) {
                newExpr2 = ToTemp(block, newExpr2);
            }

            block.Add(factory(newExpr1, newExpr2));
            return Expression.Block(block);
        }

        private Expression VisitAssign(BinaryExpression node) {
            int yields = _yields.Count;
            Expression left = Visit(node.Left);
            Expression right = Visit(node.Right);
            if (left == node.Left && right == node.Right) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.Assign(left, right);
            }

            var block = new ReadOnlyCollectionBuilder<Expression>();

            if (_generator.RewriteAssignments) {
                // We need to make sure that LHS is evaluated before RHS. For example,
                //
                // {expr0}[{expr1},..,{exprN}] = {rhs} 
                // ->
                // { l0 = {expr0}; l1 = {expr1}; ..; lN = {exprN}; r = {rhs}; l0[l1,..,lN] = r } 
                //
                if (left == node.Left) {
                    switch (left.NodeType) {
                        case ExpressionType.MemberAccess:
                            var member = (MemberExpression)node.Left;
                            if (member.Expression != null) {
                                left = member.Update(ToTemp(block, member.Expression));
                            }
                            break;

                        case ExpressionType.Index:
                            var index = (IndexExpression)node.Left;
                            left = index.Update((index.Object != null ? ToTemp(block, index.Object) : null), ToTemp(block, index.Arguments));
                            break;

                        case ExpressionType.Parameter:
                            // no action needed
                            break;

                        default:
                            // Extension should've been reduced by Visit above,
                            // and returned a different node
                            throw Assert.Unreachable;
                    }
                } else {
                    // Get the last expression of the rewritten left side
                    var leftBlock = (BlockExpression)left;
                    block.AddRange(leftBlock.Expressions);
                    block.RemoveAt(block.Count - 1);
                    left = leftBlock.Expressions[leftBlock.Expressions.Count - 1];
                }
            }

            if (right != node.Right) {
                right = ToTemp(block, right);
            }

            block.Add(Expression.Assign(left, right));
            return Expression.Block(block);
        }

        protected override Expression VisitDynamic(DynamicExpression node) {
            return Rewrite(node, node.Arguments, node.Update);
        }

        protected override Expression VisitIndex(IndexExpression node) {
            return Rewrite(node, node.Object, node.Arguments, node.Update);
        }

        protected override Expression VisitInvocation(InvocationExpression node) {
            return Rewrite(node, node.Expression, node.Arguments, node.Update);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node) {
            return Rewrite(node, node.Object, node.Arguments, node.Update);
        }

        protected override Expression VisitNew(NewExpression node) {
            return Rewrite(node, node.Arguments, node.Update);
        }

        protected override Expression VisitNewArray(NewArrayExpression node) {
            return Rewrite(node, node.Expressions, node.Update);
        }

        protected override Expression VisitMember(MemberExpression node) {
            return Rewrite(node, node.Expression, node.Update);
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            if (node.NodeType == ExpressionType.Assign) {
                return VisitAssign(node);
            }
            // For OpAssign nodes: if has a yield, we need to do the generator
            // transformation on the reduced value.
            if (node.CanReduce) {
                return Visit(node.Reduce());
            }

            return Rewrite(node, node.Left, node.Right, node.Update);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node) {
            return Rewrite(node, node.Expression, node.Update);
        }

        protected override Expression VisitUnary(UnaryExpression node) {
            // For OpAssign nodes: if has a yield, we need to do the generator
            // transformation on the reduced value.
            if (node.CanReduce) {
                return Visit(node.Reduce());
            }

            return Rewrite(node, node.Operand, node.Update);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node) {
            // See if anything changed
            int yields = _yields.Count;
            Expression e = base.VisitMemberInit(node);
            if (yields == _yields.Count) {
                return e;
            }
            // It has a yield. Reduce to basic nodes so we can jump in
            return e.Reduce();
        }

        protected override Expression VisitListInit(ListInitExpression node) {
            // See if anything changed
            int yields = _yields.Count;
            Expression e = base.VisitListInit(node);
            if (yields == _yields.Count) {
                return e;
            }
            // It has a yield. Reduce to basic nodes so we can jump in
            return e.Reduce();
        }

        #endregion
    }
}
