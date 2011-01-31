using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;

namespace SymplSample {

    internal static class ETGen {

        public static Expression AnalyzeExpr(SymplExpr expr, AnalysisScope scope) {
            if (expr is SymplImportExpr) {
                return AnalyzeImportExpr((SymplImportExpr)expr, scope);
            } else if (expr is SymplFunCallExpr) {
                return AnalyzeFunCallExpr((SymplFunCallExpr)expr, scope);
            } else if (expr is SymplDefunExpr) {
                return AnalyzeDefunExpr((SymplDefunExpr)expr, scope);
            } else if (expr is SymplLambdaExpr) {
                return AnalyzeLambdaExpr((SymplLambdaExpr)expr, scope);
            } else if (expr is SymplIdExpr) {
                return AnalyzeIdExpr((SymplIdExpr)expr, scope);
            } else if (expr is SymplQuoteExpr) {
                return AnalyzeQuoteExpr((SymplQuoteExpr)expr, scope);
            } else if (expr is SymplLiteralExpr) {
                return Expression.Constant(((SymplLiteralExpr)expr).Value);
            } else if (expr is SymplAssignExpr) {
                return AnalyzeAssignExpr((SymplAssignExpr)expr, scope);
            } else if (expr is SymplLetStarExpr) {
                return AnalyzeLetStarExpr((SymplLetStarExpr)expr, scope);
            } else if (expr is SymplBlockExpr) {
                return AnalyzeBlockExpr((SymplBlockExpr)expr, scope);
            } else if (expr is SymplEqExpr) {
                return AnalyzeEqExpr((SymplEqExpr)expr, scope);
            } else if (expr is SymplConsExpr) {
                return AnalyzeConsExpr((SymplConsExpr)expr, scope);
            } else if (expr is SymplListCallExpr) {
                return AnalyzeListCallExpr((SymplListCallExpr)expr, scope);
            } else if (expr is SymplIfExpr) {
                return AnalyzeIfExpr((SymplIfExpr)expr, scope);
            } else if (expr is SymplDottedExpr) {
                return AnalyzeDottedExpr((SymplDottedExpr)expr, scope);
            } else if (expr is SymplNewExpr) {
                return AnalyzeNewExpr((SymplNewExpr)expr, scope);
            } else if (expr is SymplLoopExpr) {
                return AnalyzeLoopExpr((SymplLoopExpr)expr, scope);
            } else if (expr is SymplBreakExpr) {
                return AnalyzeBreakExpr((SymplBreakExpr)expr, scope);
            } else if (expr is SymplEltExpr) {
                return AnalyzeEltExpr((SymplEltExpr)expr, scope);
            } else if (expr is SymplBinaryExpr) {
                return AnalyzeBinaryExpr((SymplBinaryExpr)expr, scope);
            } else if (expr is SymplUnaryExpr) {
                return AnalyzeUnaryExpr((SymplUnaryExpr)expr, scope);
            } else {
                throw new InvalidOperationException(
                    "Internal: no expression to analyze.");
            }
        }

        public static Expression AnalyzeImportExpr(SymplImportExpr expr,
                                                    AnalysisScope scope) {
            if (!scope.IsModule) {
                throw new InvalidOperationException(
                    "Import expression must be a top level expression.");
            }
            return Expression.Call(
                typeof(RuntimeHelpers).GetMethod("SymplImport"),
                scope.RuntimeExpr,
                scope.ModuleExpr,
                Expression.Constant(expr.NamespaceExpr.Select(id => id.Name)
                                                      .ToArray()),
                Expression.Constant(expr.MemberNames.Select(id => id.Name)
                                                      .ToArray()),
                Expression.Constant(expr.Renames.Select(id => id.Name)
                                                      .ToArray()));
        }

        public static DynamicExpression AnalyzeDefunExpr(SymplDefunExpr expr,
                                                          AnalysisScope scope) {
            if (!scope.IsModule) {
                throw new InvalidOperationException(
                    "Use Defmethod or Lambda when not defining top-level function.");
            }
            return Expression.Dynamic(
                       scope.GetRuntime().GetSetMemberBinder(expr.Name),
                       typeof(object),
                       scope.ModuleExpr,
                       AnalyzeLambdaDef(expr.Params, expr.Body, scope,
                                        "defun " + expr.Name));
        }

        public static LambdaExpression AnalyzeLambdaExpr(SymplLambdaExpr expr,
                                                         AnalysisScope scope) {
            return (LambdaExpression)AnalyzeLambdaDef(expr.Params, expr.Body,
                                                      scope, "lambda");
        }

        private static Expression AnalyzeLambdaDef
                (IdOrKeywordToken[] parms, SymplExpr[] body,
                 AnalysisScope scope, string description) {
            var funscope = new AnalysisScope(scope, description);
            funscope.IsLambda = true;  // needed for return support.
            var paramsInOrder = new List<ParameterExpression>();
            foreach (var p in parms) {
                var pe = Expression.Parameter(typeof(object), p.Name);
                paramsInOrder.Add(pe);
                funscope.Names[p.Name.ToLower()] = pe;
            }
            // No need to add fun name to module scope since recursive call just looks
            // up global name late bound.  For lambdas,to get the effect of flet to
            // support recursion, bind a variable to nil and then set it to a lambda.
            // Then the lambda's body can refer to the let bound var in its def.
            var bodyexprs = new List<Expression>();
            foreach (var e in body) {
                bodyexprs.Add(AnalyzeExpr(e, funscope));
            }
            // Set up the Type arg array for the delegate type.  Must include
            // the return type as the last Type, which is object for Sympl defs.
            var funcTypeArgs = new List<Type>();
            for (int i = 0; i < parms.Length + 1; i++) {
                funcTypeArgs.Add(typeof(object));
            }
            return Expression.Lambda(
                       Expression.GetFuncType(funcTypeArgs.ToArray()),
                       Expression.Block(bodyexprs),
                       paramsInOrder);
        }


		// Returns a dynamic InvokeMember or Invoke expression, depending on the
		// Function expression.
		//
        public static DynamicExpression AnalyzeFunCallExpr(
                SymplFunCallExpr expr, AnalysisScope scope) {
            if (expr.Function is SymplDottedExpr) {
                SymplDottedExpr dottedExpr = (SymplDottedExpr)expr.Function;
                Expression objExpr;
                int length = dottedExpr.Exprs.Length;
                if (length > 1) {
                    objExpr = AnalyzeDottedExpr(
                        // create a new dot expression for the object that doesn't
                        // include the last part
                        new SymplDottedExpr(
                               dottedExpr.ObjectExpr, 
                               RuntimeHelpers.RemoveLast(dottedExpr.Exprs)),
                        scope
                    );
                } else {
                    objExpr = AnalyzeExpr(dottedExpr.ObjectExpr, scope);
                }
                List<Expression> args = new List<Expression>();
                args.Add(objExpr);
                args.AddRange(expr.Arguments.Select(a => AnalyzeExpr(a, scope)));

                // last expr must be an id
                var lastExpr = (SymplIdExpr)(dottedExpr.Exprs.Last());
                return Expression.Dynamic(
                    scope.GetRuntime().GetInvokeMemberBinder(
                        new InvokeMemberBinderKey(
                            lastExpr.IdToken.Name,
                            new CallInfo(expr.Arguments.Length))),
                    typeof(object),
                    args
                );
            } else {
                var fun = AnalyzeExpr(expr.Function, scope);
                List<Expression> args = new List<Expression>();
                args.Add(fun);
                args.AddRange(expr.Arguments.Select(a => AnalyzeExpr(a, scope)));
                // Use DynExpr so that I don't always have to have a delegate to call,
                // such as what happens with IPy interop.
                return Expression.Dynamic(
                    scope.GetRuntime()
                         .GetInvokeBinder(new CallInfo(expr.Arguments.Length)),
                    typeof(object),
                    args
                );
            }
        }

		// Returns a chain of GetMember and InvokeMember dynamic expressions for
		// the dotted expr.
		//
        public static Expression AnalyzeDottedExpr(SymplDottedExpr expr,
                                                    AnalysisScope scope) {
            var curExpr = AnalyzeExpr(expr.ObjectExpr, scope);
            foreach (var e in expr.Exprs) {
                if (e is SymplIdExpr) {
                    curExpr = Expression.Dynamic(
                        scope.GetRuntime()
                             .GetGetMemberBinder(((SymplIdExpr)e).IdToken.Name),
                        typeof(object),
                        curExpr
                    );
                } else if (e is SymplFunCallExpr) {
                    var call = (SymplFunCallExpr)e;
                    List<Expression> args = new List<Expression>();
                    args.Add(curExpr);
                    args.AddRange(call.Arguments.Select(a => AnalyzeExpr(a, scope)));

                    curExpr = Expression.Dynamic(
                        // Dotted exprs must be simple invoke members, a.b.(c ...) 
                        scope.GetRuntime().GetInvokeMemberBinder(
                            new InvokeMemberBinderKey(
                                ((SymplIdExpr)call.Function).IdToken.Name,
                                new CallInfo(call.Arguments.Length))),
                        typeof(object),
                        args
                    );
                } else {
                    throw new InvalidOperationException(
                        "Internal: dotted must be IDs or Funs.");
                }
            }
            return curExpr;
        }

		// AnalyzeAssignExpr handles IDs, indexing, and member sets.  IDs are either
		// lexical or dynamic exprs on the module scope.  Everything
		// else is dynamic.
		//
        public static Expression AnalyzeAssignExpr(SymplAssignExpr expr,
                                                    AnalysisScope scope) {
            if (expr.Location is SymplIdExpr) {
                var idExpr = (SymplIdExpr)(expr.Location);
                var lhs = AnalyzeExpr(expr.Location, scope);
                var val = AnalyzeExpr(expr.Value, scope);
                var param = FindIdDef(idExpr.IdToken.Name, scope);
                if (param != null) {
                    // Assign returns value stored.
                    return Expression.Assign(
                               lhs,
                               Expression.Convert(val, param.Type));
                } else {
                    var tmp = Expression.Parameter(typeof(object),
                                                   "assignTmpForRes");
                    // Ensure stored value is returned.  Had some erroneous
                    // MOs come through here and left the code for example.
                    return Expression.Block(
                       new[] { tmp },
                       Expression.Assign(
                           tmp,
                           Expression.Convert(val, typeof(object))),
                       Expression.Dynamic(
                           scope.GetRuntime()
                                .GetSetMemberBinder(idExpr.IdToken.Name),
                           typeof(object),
                           scope.GetModuleExpr(),
                           tmp),
                       tmp);
                }
            } else if (expr.Location is SymplEltExpr) {
                var eltExpr = (SymplEltExpr)(expr.Location);
                var args = new List<Expression>();
                args.Add(AnalyzeExpr(eltExpr.ObjectExpr, scope));
                args.AddRange(eltExpr.Indexes.Select(e => AnalyzeExpr(e, scope)));
                args.Add(AnalyzeExpr(expr.Value, scope));
                // Trusting MO convention to return stored values.
                return Expression.Dynamic(
                           scope.GetRuntime().GetSetIndexBinder(
                                   new CallInfo(eltExpr.Indexes.Length)), 
                           typeof(object),
                           args);
            } else if (expr.Location is SymplDottedExpr) {
                // For now, one dot only.  Later, pick oflast dotted member
                // access (like AnalyzeFunctionCall), and use a temp and block.
                var dottedExpr = (SymplDottedExpr)(expr.Location);
                if (dottedExpr.Exprs.Length > 1) {
                    throw new InvalidOperationException(
                        "Don't support assigning with more than simple dotted " +
                        "expression, o.foo.");
                }
                if (!(dottedExpr.Exprs[0] is SymplIdExpr)) {
                    throw new InvalidOperationException(
                        "Only support unindexed field or property when assigning " +
                        "dotted expression location.");
                }
                var id = (SymplIdExpr)(dottedExpr.Exprs[0]);
                // Trusting MOs convention to return stored values.
                return Expression.Dynamic(
                           scope.GetRuntime().GetSetMemberBinder(id.IdToken.Name),
                           typeof(object),
                           AnalyzeExpr(dottedExpr.ObjectExpr, scope),
                           AnalyzeExpr(expr.Value, scope)
                );
            }

            throw new InvalidOperationException("Invalid left hand side type.");
        }

		// Return an Expression for referencing the ID.  If we find the name in the
		// scope chain, then we just return the stored ParamExpr.  Otherwise, the
		// reference is a dynamic member lookup on the root scope, a module object.
		//
        public static Expression AnalyzeIdExpr(SymplIdExpr expr,
                                                AnalysisScope scope) {
            if (expr.IdToken.IsKeywordToken) {
                if (expr.IdToken == KeywordToken.Nil)
                    return Expression.Constant(null, typeof(object));
                else if (expr.IdToken == KeywordToken.True)
                    return Expression.Constant(true);
                else if (expr.IdToken == KeywordToken.False)
                    return Expression.Constant(false);
                else
                    throw new InvalidOperationException(
                        "Internal: unrecognized keyword literal constant.");
            } else {
                var param = FindIdDef(expr.IdToken.Name, scope);
                if (param != null) {
                    return param;
                } else {
                    return Expression.Dynamic(
                       scope.GetRuntime().GetGetMemberBinder(expr.IdToken.Name),
                       typeof(object),
                       scope.GetModuleExpr()
                    );
                }
            }
        }

        // _findIdDef returns the ParameterExpr for the name by searching the scopes,
        // or it returns None.
        //
        private static Expression FindIdDef(string name, AnalysisScope scope) {
            var curscope = scope;
            name = name.ToLower();
            ParameterExpression res;
            while (curscope != null && !curscope.IsModule) {
                if (curscope.Names.TryGetValue(name, out res)) {
                    return res;
                } else {
                    curscope = curscope.Parent;
                }
            }

            if (scope == null) {
                throw new InvalidOperationException(
                    "Got bad AnalysisScope chain with no module at end.");
            }

            return null;
        }

		// AnalyzeLetStar returns a Block with vars, each initialized in the order
		// they appear.  Each var's init expr can refer to vars initialized before it.
		// The Block's body is the Let*'s body.
		//
        public static Expression AnalyzeLetStarExpr(SymplLetStarExpr expr,
                                                      AnalysisScope scope) {
            var letscope = new AnalysisScope(scope, "let*");
            // Analyze bindings.
            List<Expression> inits = new List<Expression>();
            List<ParameterExpression> varsInOrder = new List<ParameterExpression>();
            foreach (var b in expr.Bindings) {
                // Need richer logic for mvbind
                var v = Expression.Parameter(typeof(object), b.Variable.Name);
                varsInOrder.Add(v);
                inits.Add(
                    Expression.Assign(
                        v,
                        Expression.Convert(AnalyzeExpr(b.Value, letscope), v.Type))
                );
                // Add var to scope after analyzing init value so that init value
                // references to the same ID do not bind to his uninitialized var.
                letscope.Names[b.Variable.Name.ToLower()] = v;
            }
            List<Expression> body = new List<Expression>();
            foreach (var e in expr.Body) {
                body.Add(AnalyzeExpr(e, letscope));
            }
            // Order of vars to BlockExpr don't matter semantically, but may as well
            // keep them in the order the programmer specified in case they look at the
            // Expr Trees in the debugger or for meta-programming.
            inits.AddRange(body);
            return Expression.Block(typeof(object), varsInOrder.ToArray(), inits);
        }

        // AnalyzeBlockExpr returns a Block with the body exprs.
        //
        public static Expression AnalyzeBlockExpr(SymplBlockExpr expr,
                                                    AnalysisScope scope) {
            List<Expression> body = new List<Expression>();
            foreach (var e in expr.Body) {
                body.Add(AnalyzeExpr(e, scope));
            }
            return Expression.Block(typeof(object), body);
        }

        // AnalyzeQuoteExpr converts a list, literal, or id expr to a runtime quoted
        // literal and returns the Constant expression for it.
        //
        public static Expression AnalyzeQuoteExpr(SymplQuoteExpr expr,
                                                    AnalysisScope scope) {
            return Expression.Constant(MakeQuoteConstant(
                                           expr.Expr, scope.GetRuntime()));
        }

        private static object MakeQuoteConstant(object expr, Sympl symplRuntime) {
            if (expr is SymplListExpr) {
                SymplListExpr listexpr = (SymplListExpr)expr;
                int len = listexpr.Elements.Length;
                var exprs = new object[len];
                for (int i = 0; i < len; i++) {
                    exprs[i] = MakeQuoteConstant(listexpr.Elements[i], symplRuntime);
                }
                return Cons._List(exprs);
            } else if (expr is IdOrKeywordToken) {
                return symplRuntime.MakeSymbol(((IdOrKeywordToken)expr).Name);
            } else if (expr is LiteralToken) {
                return ((LiteralToken)expr).Value;
            } else {
                throw new InvalidOperationException(
                    "Internal: quoted list has -- " + expr.ToString());
            }
        }

        public static Expression AnalyzeEqExpr (SymplEqExpr expr,
                                                AnalysisScope scope) {
            var mi = typeof(RuntimeHelpers).GetMethod("SymplEq");
            return Expression.Call(mi, Expression.Convert(
                                           AnalyzeExpr(expr.Left, scope),
                                           typeof(object)),
                                   Expression.Convert(
                                       AnalyzeExpr(expr.Right, scope),
                                       typeof(object)));
        }
            
        public static Expression AnalyzeConsExpr (SymplConsExpr expr,
                                                  AnalysisScope scope) {
            var mi = typeof(RuntimeHelpers).GetMethod("MakeCons");
            return Expression.Call(mi, Expression.Convert(
                                           AnalyzeExpr(expr.Left, scope),
                                           typeof(object)),
                                   Expression.Convert(
                                       AnalyzeExpr(expr.Right, scope),
                                       typeof(object)));
        }

        public static Expression AnalyzeListCallExpr (SymplListCallExpr expr,
                                                      AnalysisScope scope) {
            var mi = typeof(Cons).GetMethod("_List");
            int len = expr.Elements.Length;
            var args = new Expression[len];
            for (int i = 0; i < len; i++) {
                args[i] = Expression.Convert(AnalyzeExpr(expr.Elements[i], scope),
                                             typeof(object));
            }
            return Expression.Call(mi, Expression
                                           .NewArrayInit(typeof(object), args));
        }

        public static Expression AnalyzeIfExpr (SymplIfExpr expr,
                                                AnalysisScope scope) {
            Expression alt = null;
            if (expr.Alternative != null) {
                alt = AnalyzeExpr(expr.Alternative, scope);
            } else {
                alt = Expression.Constant(false);
            }
            return Expression.Condition(
                       WrapBooleanTest(AnalyzeExpr(expr.Test, scope)),
                       Expression.Convert(AnalyzeExpr(expr.Consequent, scope),
                                             typeof(object)),
                       Expression.Convert(alt, typeof(object)));
        }
        
        private static Expression WrapBooleanTest (Expression expr) {
            var tmp = Expression.Parameter(typeof(object), "testtmp");
            return Expression.Block(
                new ParameterExpression[] { tmp },
                new Expression[] 
                        {Expression.Assign(tmp, Expression
                                                  .Convert(expr, typeof(object))),
                         Expression.Condition(
                             Expression.TypeIs(tmp, typeof(bool)), 
                             Expression.Convert(tmp, typeof(bool)),
                             Expression.NotEqual(
                                tmp, 
                                Expression.Constant(null, typeof(object))))});
        }

        public static Expression AnalyzeLoopExpr (SymplLoopExpr expr,
                                                  AnalysisScope scope) {
            var loopscope = new AnalysisScope(scope, "loop ");
            loopscope.IsLoop = true; // needed for break and continue
            loopscope.LoopBreak = Expression.Label(typeof(object), "loop break");
            int len = expr.Body.Length;
            var body = new Expression[len];
            for (int i = 0; i < len; i++) {
                body[i] = AnalyzeExpr(expr.Body[i], loopscope);
            }
            return Expression.Loop(Expression.Block(typeof(object), body), 
                                   loopscope.LoopBreak);
        }

        public static Expression AnalyzeBreakExpr (SymplBreakExpr expr,
                                        AnalysisScope scope) {
            var loopscope = _findFirstLoop(scope);
            if (loopscope == null)
                throw new InvalidOperationException(
                               "Call to Break not inside loop.");
            Expression value;
            if (expr.Value == null)
                value = Expression.Constant(null, typeof(object));
            else
                // Ok if value jumps to break label.
                value = AnalyzeExpr(expr.Value, loopscope);
            // Need final type=object arg because the Goto is in a value returning
            // position, and the Break factory doesn't set the GotoExpr.Type property
            // to the type of the LoopBreak label target's type.  For example, removing
            // this would cause the Convert to object for an If branch to throw because
            // the Goto is void without this last arg.
            return Expression.Break(loopscope.LoopBreak, value, typeof(object));
        }

        // _findFirstLoop returns the first loop AnalysisScope or None.
        //
        private static AnalysisScope _findFirstLoop (AnalysisScope scope) {
            var curscope = scope;
            while (curscope != null) {
                if (curscope.IsLoop)
                    return curscope;
                else
                    curscope = curscope.Parent;
            }
            return null;
        }

    
        public static Expression AnalyzeNewExpr(SymplNewExpr expr,
                                                AnalysisScope scope) {

            List<Expression> args = new List<Expression>();
            args.Add(AnalyzeExpr(expr.Type, scope));
            args.AddRange(expr.Arguments.Select(a => AnalyzeExpr(a, scope)));

            return Expression.Dynamic(
                scope.GetRuntime().GetCreateInstanceBinder(
                                     new CallInfo(expr.Arguments.Length)),
                typeof(object),
                args
            );
        }

        public static Expression AnalyzeBinaryExpr(SymplBinaryExpr expr,
                                                   AnalysisScope scope) {

            // The language has the following special logic to handle And and Or
            // x And y == if x then y
            // x Or y == if x then x else (if y then y)
            if (expr.Operation == ExpressionType.And) {
                return AnalyzeIfExpr(
                    new SymplIfExpr(
                        expr.Left, expr.Right, null),
                    scope); 
            } else if (expr.Operation == ExpressionType.Or) {
                // Use (LetStar (tmp expr) (if tmp tmp)) to represent (if expr expr)
                // to remore duplicate evaluation.
                // So x Or y is translated into
                // (Let* (tmp1 x) 
                //    (If tmp1 tmp1  
                //       (Let* (tmp2 y) (If tmp2 tmp2))))
                //           

                IdOrKeywordToken tmp2 = new IdOrKeywordToken(
                    // Real implementation needs to ensure unique ID in scope chain.
                    "__tmpLetVariable2");
                var tmpExpr2 = new SymplIdExpr(tmp2);
                var binding2 = new LetBinding(tmp2, expr.Right); ;
                var ifExpr2 = new SymplIfExpr(
                    tmpExpr2, tmpExpr2, null);
                var letExpr2 = new SymplLetStarExpr(
                        new[] { binding2 },
                        new[] { ifExpr2 });

                IdOrKeywordToken tmp1 = new IdOrKeywordToken(
                    // Real implementation needs to ensure unique ID in scope chain.
                    "__tmpLetVariable1");
                var tmpExpr1 = new SymplIdExpr(tmp1);
                LetBinding binding1 = new LetBinding(tmp1, expr.Left); ;
                SymplExpr ifExpr1 = new SymplIfExpr(
                    tmpExpr1, tmpExpr1, letExpr2);
                return AnalyzeLetStarExpr(
                    new SymplLetStarExpr(
                        new[] { binding1 },
                        new[] { ifExpr1 }
                    ),
                    scope
                );
            }

            return Expression.Dynamic(
                scope.GetRuntime().GetBinaryOperationBinder(expr.Operation),
                typeof(object),
                AnalyzeExpr(expr.Left, scope),
                AnalyzeExpr(expr.Right, scope)
            );
        }

        public static Expression AnalyzeUnaryExpr(SymplUnaryExpr expr,
                                                  AnalysisScope scope) {
            
            if (expr.Operation == ExpressionType.Not) {
                return Expression.Not(WrapBooleanTest(AnalyzeExpr(expr.Operand,
                                                                   scope)));
            }
            // Example purposes only, we should never get here since we only have Not.
            return Expression.Dynamic(
                scope.GetRuntime().GetUnaryOperationBinder(expr.Operation),
                typeof(object),
                AnalyzeExpr(expr.Operand, scope)
            );
        }

        // AnalyzeEltExpr returns and Expression for accessing an element of an
        // aggregate structure.  This also works for .NET objs with indexer Item
        // properties.  We handle analyzing Elt for assignment in AnalyzeAssignExpr.
        //
        public static Expression AnalyzeEltExpr(SymplEltExpr expr,
                                                AnalysisScope scope) {

            List<Expression> args = new List<Expression>();
            args.Add(AnalyzeExpr(expr.ObjectExpr, scope));
            args.AddRange(expr.Indexes.Select(e => AnalyzeExpr(e, scope)));

            return Expression.Dynamic(
                scope.GetRuntime().GetGetIndexBinder(
                                      new CallInfo(expr.Indexes.Length)),
                typeof(object),
                args
            );
        }

    } // ETGen


    // AnalysisScope holds identifier information so that we can do name binding
    // during analysis.  It manages a map from names to ParameterExprs so ET
    // definition locations and reference locations can alias the same variable.
    //
    // These chain from inner most BlockExprs, through LambdaExprs, to the root
    // which models a file or top-level expression.  The root has non-None
    // ModuleExpr and RuntimeExpr, which are ParameterExprs.
    //
    internal class AnalysisScope {

        private AnalysisScope _parent;
        private string _name;
        // Need runtime for interning Symbol constants at code gen time.
        private Sympl _runtime;
        private ParameterExpression _runtimeParam;
        private ParameterExpression _moduleParam;
        // Need IsLambda when support return to find tightest closing fun.
        private bool _isLambda = false;
        private bool _isLoop = false;
        private LabelTarget _loopBreak = null;
        //private LabelTarget _continueBreak = null;
        private Dictionary<string, ParameterExpression> _names;

        public AnalysisScope(AnalysisScope parent, string name)
            : this(parent, name, null, null, null) { }

        public AnalysisScope(AnalysisScope parent,
							  string name,
							  Sympl runtime,
							  ParameterExpression runtimeParam,
							  ParameterExpression moduleParam) {
            _parent = parent;
            _name = name;
            _runtime = runtime;
            _runtimeParam = runtimeParam;
            _moduleParam = moduleParam;

            _names = new Dictionary<string, ParameterExpression>();
            _isLambda = false;
        }

        public AnalysisScope Parent { get { return _parent; } }

        public ParameterExpression ModuleExpr { get { return _moduleParam; } }

        public ParameterExpression RuntimeExpr { get { return _runtimeParam; } }

        public Sympl Runtime { get { return _runtime; } }

        public bool IsModule { get { return _moduleParam != null; } }

        public bool IsLambda { 
            get { return _isLambda; } 
            set { _isLambda = value; }
        }

        public bool IsLoop { 
            get { return _isLoop; } 
            set { _isLoop = value; }
        }
        
        public LabelTarget LoopBreak { 
            get { return _loopBreak; } 
            set { _loopBreak = value; }
        }
        
        public LabelTarget LoopContinue { 
            get { return _loopBreak; } 
            set { _loopBreak = value; }
        }

        public Dictionary<string, ParameterExpression> Names { 
            get { return _names; } 
            set { _names = value; }
        }

        public ParameterExpression GetModuleExpr() {
            var curScope = this;
            while (!curScope.IsModule) {
                curScope = curScope.Parent;
            }
            return curScope.ModuleExpr;
        }

        public Sympl GetRuntime() {
            var curScope = this;
            while (curScope.Runtime == null) {
                curScope = curScope.Parent;
            }
            return curScope.Runtime;
        }
    } //AnalysisScope

} // Namespace
