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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

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

        private readonly GeneratorExpression _generator;
        private readonly ParameterExpression _current;
        private readonly ParameterExpression _state;

        // The one return label, or more than one if we're in a finally
        private readonly Stack<LabelTarget> _returnLabels = new Stack<LabelTarget>();
        private ParameterExpression _gotoRouter;
        private bool _inTryWithFinally;

        private readonly List<YieldMarker> _yields = new List<YieldMarker>();

        private List<int> _debugCookies;

        private readonly Set<ParameterExpression> _vars = new Set<ParameterExpression>();

        // Possible optimization: reuse temps. Requires scoping them correctly,
        // and then storing them back in a free list
        private readonly List<ParameterExpression> _temps = new List<ParameterExpression>();

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
                cases[i] = Expression.SwitchCase(Expression.Goto(_yields[i].Label), Expression.Constant(_yields[i].State));
            }
            cases[count] = Expression.SwitchCase(Expression.Goto(_returnLabels.Peek()), Expression.Constant(Finished));

            Type generatorNextOfT = typeof(GeneratorNext<>).MakeGenericType(_generator.Target.Type);

            // Create the lambda for the GeneratorNext<T>, hoisting variables
            // into a scope outside the lambda
            var allVars = new List<ParameterExpression>(_vars);
            allVars.AddRange(_temps);

            body = Expression.Block(
                allVars,
                Expression.Lambda(
                    generatorNextOfT,
                    Expression.Block(
                        new ParameterExpression[] { _gotoRouter },
                        Expression.Switch(Expression.Assign(_gotoRouter, _state), cases),
                        body,
                        Expression.Assign(_state, Expression.Constant(Finished)),
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
                    debugCookies[i] = Expression.Constant(_debugCookies[i]);

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

        private BinaryExpression ToTemp(ref Expression e) {
            Debug.Assert(e != null);
            var temp = Expression.Variable(e.Type, "$temp$" + _temps.Count);
            _temps.Add(temp);
            var result = Expression.Assign(temp, e);
            e = temp;
            return result;
        }

        private BlockExpression ToTemp(ref ReadOnlyCollection<Expression> args) {
            int count = args.Count;
            var block = new Expression[count];
            var newArgs = new Expression[count];
            args.CopyTo(newArgs, 0);
            for (int i = 0; i < count; i++) {
                block[i] = ToTemp(ref newArgs[i]);
            }
            args = new ReadOnlyCollection<Expression>(newArgs);
            return Expression.Block(block);
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
                return Expression.MakeTry(@try, @finally, fault, handlers);
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
                @try = Expression.Block(MakeYieldRouter(startYields, tryYields, tryStart), @try);
            }

            // Transform catches with yield to deferred handlers
            if (catchYields != tryYields) {
                var block = new List<Expression>();

                block.Add(MakeYieldRouter(tryYields, catchYields, tryStart));
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
                        Expression.Void(Expression.Assign(deferredVar, exceptionVar)),
                        filter
                    );

                    // We need to rewrite rethrows into "throw deferredVar"
                    var catchBody = new RethrowRewriter { Exception = deferredVar }.Visit(c.Body);
                    
                    // if (deferredVar != null) {
                    //     ... catch body ...
                    // }
                    block.Add(
                        Expression.Condition(
                            Expression.NotEqual(deferredVar, Expression.Constant(null, deferredVar.Type)),
                            Expression.Void(catchBody),
                            Expression.Empty()
                        )
                    );
                }

                block[1] = Expression.MakeTry(@try, null, null, new ReadOnlyCollection<CatchBlock>(handlers));
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
                    @try = Expression.MakeTry(@try, null, null, handlers);
                    handlers = new CatchBlock[0];
                }

                // NOTE: the order of these routers is important
                // The first call changes the labels to all point at "tryEnd",
                // so the second router will jump to "tryEnd"
                var tryEnd = Expression.Label();
                Expression inFinallyRouter = MakeYieldRouter(catchYields, finallyYields, tryEnd);
                Expression inTryRouter = MakeYieldRouter(catchYields, finallyYields, tryStart);

                var all = Expression.Variable(typeof(Exception), "e");
                var saved = Expression.Variable(typeof(Exception), "$saved$" + _temps.Count);
                _temps.Add(saved);
                @try = Expression.Block(
                    Expression.TryCatchFinally(
                        Expression.Block(
                            inTryRouter,
                            @try,
                            Expression.Assign(saved, Expression.Constant(null, saved.Type)),
                            Expression.Label(tryEnd)
                        ),
                        Expression.Block(
                            MakeSkipFinallyBlock(finallyReturn),
                            inFinallyRouter,
                            @finally,
                            Expression.Condition(
                                Expression.NotEqual(saved, Expression.Constant(null, saved.Type)),
                                Expression.Throw(saved),
                                Expression.Empty()
                            ),
                            Expression.Label(finallyReturn)
                        ),
                        Expression.Catch(all, Expression.Void(Expression.Assign(saved, all)))
                    ),
                    Expression.Condition(
                        Expression.Equal(_gotoRouter, Expression.Constant(GotoRouterYielding)),
                        Expression.Goto(_returnLabels.Peek()),
                        Expression.Empty()
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
                @try = Expression.MakeTry(@try, @finally, fault, handlers);
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
                    Expression.Equal(_gotoRouter, Expression.Constant(GotoRouterYielding)),
                    Expression.NotEqual(_state, Expression.Constant(Finished))
                ),
                Expression.Goto(target),
                Expression.Empty()
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

        private SwitchExpression MakeYieldRouter(int start, int end, LabelTarget newTarget) {
            Debug.Assert(end > start);
            var cases = new SwitchCase[end - start];
            for (int i = start; i < end; i++) {
                YieldMarker y = _yields[i];
                cases[i - start] = Expression.SwitchCase(Expression.Goto(y.Label), Expression.Constant(y.State));
                // Any jumps from outer switch statements should go to the this
                // router, not the original label (which they cannot legally jump to)
                y.Label = newTarget;
            }
            return Expression.Switch(_gotoRouter, cases);
        }

        protected override Expression VisitExtension(Expression node) {
            var yield = node as YieldExpression;
            if (yield != null) {
                return VisitYield(yield);
            }

            // We need to reduce here, otherwise we can't guarentee proper
            // stack spilling of the resulting expression.
            // In effect, generators are one of the last rewrites that should
            // happen
            return Visit(node.ReduceExtensions());
        }

        private Expression VisitYield(YieldExpression node) {
            if (node.Target != _generator.Target) {
                throw new InvalidOperationException("yield and generator must have the same LabelTarget object");
            }

            var value = Visit(node.Value);

            var block = new List<Expression>();
            if (value == null) {
                // Yield break
                block.Add(Expression.Assign(_state, Expression.Constant(Finished)));
                if (_inTryWithFinally) {
                    block.Add(Expression.Assign(_gotoRouter, Expression.Constant(GotoRouterYielding)));
                }
                block.Add(Expression.Goto(_returnLabels.Peek()));
                return Expression.Block(block);
            }

            // Yield return
            block.Add(Expression.Assign(_current, value));
            YieldMarker marker = GetYieldMarker(node);
            block.Add(Expression.Assign(_state, Expression.Constant(marker.State)));
            if (_inTryWithFinally) {
                block.Add(Expression.Assign(_gotoRouter, Expression.Constant(GotoRouterYielding)));
            }
            block.Add(Expression.Goto(_returnLabels.Peek()));
            block.Add(Expression.Label(marker.Label));
            block.Add(Expression.Assign(_gotoRouter, Expression.Constant(GotoRouterNone)));
            block.Add(Expression.Empty());
            return Expression.Block(block);
        }

        protected override Expression VisitBlock(BlockExpression node) {
            int yields = _yields.Count;
            var b = Visit(node.Expressions);
            if (b == node.Expressions) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.Block(node.Variables, b);
            }

            // save the variables for later
            // (they'll be hoisted outside of the lambda)
            _vars.UnionWith(node.Variables);

            // Return a new block expression with the rewritten body except for that
            // all the variables are removed.
            return Expression.Block(null, b);
        }

        protected override Expression VisitLambda<T>(Expression<T> node) {
            // don't recurse into nested lambdas
            return node;
        }

        #region stack spilling (to permit yield in the middle of an expression)

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

            var block = new List<Expression>();

            // If the left hand side did not rewrite itself, we may still need
            // to rewrite to ensure proper evaluation order. Essentially, we
            // want all of the left side evaluated first, then the value, then
            // the assignment
            if (left == node.Left) {
                switch (left.NodeType) {
                    case ExpressionType.MemberAccess:
                        var member = (MemberExpression)node.Left;
                        Expression e = Visit(member.Expression);
                        block.Add(ToTemp(ref e));
                        left = Expression.MakeMemberAccess(e, member.Member);
                        break;
                    case ExpressionType.Index:
                        var index = (IndexExpression)node.Left;
                        Expression o = Visit(index.Object);
                        ReadOnlyCollection<Expression> a = Visit(index.Arguments);
                        if (o == index.Object && a == index.Arguments) {
                            return index;
                        }
                        block.Add(ToTemp(ref o));
                        block.Add(ToTemp(ref a));
                        left = Expression.MakeIndex(o, index.Indexer, a);
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
                left = leftBlock.Expressions[leftBlock.Expressions.Count - 1];
                block.AddRange(leftBlock.Expressions);
                block.RemoveAt(block.Count - 1);
            }

            if (right != node.Right) {
                block.Add(ToTemp(ref right));
            }

            block.Add(Expression.Assign(left, right));
            return Expression.Block(block);
        }

        protected override Expression VisitDynamic(DynamicExpression node) {
            int yields = _yields.Count;
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (a == node.Arguments) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.MakeDynamic(node.DelegateType, node.Binder, a);
            }
            return Expression.Block(
                ToTemp(ref a),
                Expression.MakeDynamic(node.DelegateType, node.Binder, a)
            );
        }

        protected override Expression VisitIndex(IndexExpression node) {
            int yields = _yields.Count;
            Expression o = Visit(node.Object);
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (o == node.Object && a == node.Arguments) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.MakeIndex(o, node.Indexer, a);
            }
            return Expression.Block(
                ToTemp(ref o),
                ToTemp(ref a),
                Expression.MakeIndex(o, node.Indexer, a)
            );
        }

        protected override Expression VisitInvocation(InvocationExpression node) {
            int yields = _yields.Count;
            Expression e = Visit(node.Expression);
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (e == node.Expression && a == node.Arguments) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.Invoke(e, a);
            }
            return Expression.Block(
                ToTemp(ref e),
                ToTemp(ref a),
                Expression.Invoke(e, a)
            );
        }

        protected override Expression VisitMethodCall(MethodCallExpression node) {
            int yields = _yields.Count;
            Expression o = Visit(node.Object);
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (o == node.Object && a == node.Arguments) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.Call(o, node.Method, a);
            }
            if (o == null) {
                return Expression.Block(
                    ToTemp(ref a),
                    Expression.Call(null, node.Method, a)
                );
            }
            return Expression.Block(
                ToTemp(ref o),
                ToTemp(ref a),
                Expression.Call(o, node.Method, a)
            );
        }

        protected override Expression VisitNew(NewExpression node) {
            int yields = _yields.Count;
            ReadOnlyCollection<Expression> a = Visit(node.Arguments);
            if (a == node.Arguments) {
                return node;
            }
            if (yields == _yields.Count) {
                return (node.Members != null)
                    ? Expression.New(node.Constructor, a, node.Members)
                    : Expression.New(node.Constructor, a);
            }
            return Expression.Block(
                ToTemp(ref a),
                (node.Members != null)
                    ? Expression.New(node.Constructor, a, node.Members)
                    : Expression.New(node.Constructor, a)
            );
        }

        protected override Expression VisitNewArray(NewArrayExpression node) {
            int yields = _yields.Count;
            ReadOnlyCollection<Expression> e = Visit(node.Expressions);
            if (e == node.Expressions) {
                return node;
            }
            if (yields == _yields.Count) {
                return (node.NodeType == ExpressionType.NewArrayInit)
                    ? Expression.NewArrayInit(node.Type.GetElementType(), e)
                    : Expression.NewArrayBounds(node.Type.GetElementType(), e);
            }
            return Expression.Block(
                ToTemp(ref e),
                (node.NodeType == ExpressionType.NewArrayInit)
                    ? Expression.NewArrayInit(node.Type.GetElementType(), e)
                    : Expression.NewArrayBounds(node.Type.GetElementType(), e)
            );
        }

        protected override Expression VisitMember(MemberExpression node) {
            int yields = _yields.Count;
            Expression e = Visit(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.MakeMemberAccess(e, node.Member);
            }
            return Expression.Block(
                ToTemp(ref e),
                Expression.MakeMemberAccess(e, node.Member)
            );
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

            int yields = _yields.Count;
            Expression left = Visit(node.Left);
            Expression right = Visit(node.Right);
            if (left == node.Left && right == node.Right) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method, node.Conversion);
            }

            return Expression.Block(
                ToTemp(ref left),
                ToTemp(ref right),
                Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method, node.Conversion)
            );
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node) {
            int yields = _yields.Count;
            Expression e = Visit(node.Expression);
            if (e == node.Expression) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.TypeIs(e, node.TypeOperand);
            }
            return Expression.Block(
                ToTemp(ref e),
                Expression.TypeIs(e, node.TypeOperand)
            );
        }

        protected override Expression VisitUnary(UnaryExpression node) {
            // For OpAssign nodes: if has a yield, we need to do the generator
            // transformation on the reduced value.
            if (node.CanReduce) {
                return Visit(node.Reduce());
            }

            int yields = _yields.Count;
            Expression o = Visit(node.Operand);
            if (o == node.Operand) {
                return node;
            }
            if (yields == _yields.Count) {
                return Expression.MakeUnary(node.NodeType, o, node.Type, node.Method);
            }
            return Expression.Block(
                ToTemp(ref o),
                Expression.MakeUnary(node.NodeType, o, node.Type, node.Method)
            );
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
