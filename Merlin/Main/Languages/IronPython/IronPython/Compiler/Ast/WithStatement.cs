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
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Binding;

using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public class WithStatement : Statement {
        private SourceLocation _header;
        private readonly Expression _contextManager;
        private readonly Expression _var;
        private Statement _body;

        public WithStatement(Expression contextManager, Expression var, Statement body) {
            _contextManager = contextManager;
            _var = var;
            _body = body;
        }

        public SourceLocation Header {
            set { _header = value; }
        }

        public Expression Variable {
            get { return _var; }
        }

        public Expression ContextManager {
            get { return _contextManager; }
        }

        public Statement Body {
            get { return _body; }
        }

        /// <summary>
        /// WithStatement is translated to the DLR AST equivalent to
        /// the following Python code snippet (from with statement spec):
        /// 
        /// mgr = (EXPR)
        /// exit = mgr.__exit__  # Not calling it yet
        /// value = mgr.__enter__()
        /// exc = True
        /// try:
        ///     VAR = value  # Only if "as VAR" is present
        ///     BLOCK
        /// except:
        ///     # The exceptional case is handled here
        ///     exc = False
        ///     if not exit(*sys.exc_info()):
        ///         raise
        ///     # The exception is swallowed if exit() returns true
        /// finally:
        ///     # The normal and non-local-goto cases are handled here
        ///     if exc:
        ///         exit(None, None, None)
        /// 
        /// </summary>
        internal override MSAst.Expression Transform(AstGenerator ag) {            
            MSAst.ParameterExpression lineUpdated = ag.GetTemporary("$lineUpdated_with", typeof(bool));
            // Five statements in the result...
            ReadOnlyCollectionBuilder<MSAst.Expression> statements = new ReadOnlyCollectionBuilder<MSAst.Expression>(6);
            

            //******************************************************************
            // 1. mgr = (EXPR)
            //******************************************************************
            MSAst.ParameterExpression manager = ag.GetTemporary("with_manager");
            statements.Add(
                ag.MakeAssignment(
                    manager,
                    ag.Transform(_contextManager),
                    new SourceSpan(Start, _header)
                )
            );

            //******************************************************************
            // 2. exit = mgr.__exit__  # Not calling it yet
            //******************************************************************
            MSAst.ParameterExpression exit = ag.GetTemporary("with_exit");
            statements.Add(
                ag.MakeAssignment(
                    exit,
                    ag.Get(
                        typeof(object),
                        "__exit__",
                        manager
                    )
                )
            );

            //******************************************************************
            // 3. value = mgr.__enter__()
            //******************************************************************
            MSAst.ParameterExpression value = ag.GetTemporary("with_value");
            statements.Add(
                ag.AddDebugInfoAndVoid(
                    ag.MakeAssignment(
                        value,
                        ag.Invoke(
                            typeof(object),
                            new CallSignature(0),
                            ag.Get(
                                typeof(object),
                                "__enter__",
                                manager
                            )
                        )
                    ),
                    new SourceSpan(Start, _header)
                )
            );

            //******************************************************************
            // 4. exc = True
            //******************************************************************
            MSAst.ParameterExpression exc = ag.GetTemporary("with_exc", typeof(bool));
            statements.Add(
                ag.MakeAssignment(
                    exc,
                    AstUtils.Constant(true)
                )
            );

            //******************************************************************
            //  5. The final try statement:
            //
            //  try:
            //      VAR = value  # Only if "as VAR" is present
            //      BLOCK
            //  except:
            //      # The exceptional case is handled here
            //      exc = False
            //      if not exit(*sys.exc_info()):
            //          raise
            //      # The exception is swallowed if exit() returns true
            //  finally:
            //      # The normal and non-local-goto cases are handled here
            //      if exc:
            //          exit(None, None, None)
            //******************************************************************

            MSAst.ParameterExpression exception = ag.GetTemporary("exception", typeof(Exception));
            MSAst.ParameterExpression nestedFrames = ag.GetTemporary("$nestedFrames", typeof(List<DynamicStackFrame>));
            statements.Add(
                // try:
                AstUtils.Try(
                    AstUtils.Try(// try statement body
                        ag.PushLineUpdated(false, lineUpdated),
                        _var != null ?
                            Ast.Block(
                                // VAR = value
                                _var.TransformSet(ag, SourceSpan.None, value, PythonOperationKind.None),
                                // BLOCK
                                ag.Transform(_body),
                                AstUtils.Empty()
                            ) :
                            // BLOCK
                            ag.Transform(_body) // except:, // try statement location
                    ).Catch(exception,
                    // Python specific exception handling code
                        TryStatement.GetTracebackHeader(                        
                            ag,
                            exception,
                            ag.AddDebugInfoAndVoid(
                                Ast.Block(
                                    // exc = False
                                    ag.MakeAssignment(
                                        exc,
                                        AstUtils.Constant(false)
                                    ),
                                    Ast.Assign(
                                        nestedFrames,
                                        Ast.Call(AstGenerator.GetHelperMethod("GetAndClearDynamicStackFrames"))
                                    ),
                                    //  if not exit(*sys.exc_info()):
                                    //      raise
                                    AstUtils.IfThen(
                                        ag.Convert(
                                            typeof(bool),
                                            ConversionResultKind.ExplicitCast,
                                            ag.Operation(
                                                typeof(bool),
                                                PythonOperationKind.Not,
                                                MakeExitCall(ag, exit, exception)
                                            )
                                        ),
                                        ag.UpdateLineUpdated(true),
                                        Ast.Call(
                                            AstGenerator.GetHelperMethod("SetDynamicStackFrames"),
                                            nestedFrames
                                        ),
                                        Ast.Throw(
                                            Ast.Call(
                                                AstGenerator.GetHelperMethod("MakeRethrowExceptionWorker"),
                                                exception
                                            )
                                        )
                                    )
                                ),
                                _body.Span
                            )
                        ),
                        Ast.Call(
                            AstGenerator.GetHelperMethod("SetDynamicStackFrames"),
                            nestedFrames
                        ),
                        ag.PopLineUpdated(lineUpdated),
                        Ast.Empty()
                    )
                // finally:                    
                ).Finally(
                //  if exc:
                //      exit(None, None, None)
                    AstUtils.IfThen(
                        exc,
                        ag.AddDebugInfoAndVoid(
                            Ast.Block(
                                Ast.Dynamic(
                                    ag.PyContext.Invoke(
                                        new CallSignature(3)        // signature doesn't include function
                                    ),
                                    typeof(object),
                                    new MSAst.Expression[] {
                                        ag.LocalContext,
                                        exit,
                                        AstUtils.Constant(null),
                                        AstUtils.Constant(null),
                                        AstUtils.Constant(null)
                                    }
                                ),                                    
                                Ast.Empty()
                            ),
                            _contextManager.Span
                        )
                    )
                )
            );

            statements.Add(AstUtils.Empty());
            return Ast.Block(statements.ToReadOnlyCollection());
        }

        private MSAst.Expression MakeExitCall(AstGenerator ag, MSAst.ParameterExpression exit, MSAst.Expression exception) {
            // The 'with' statement's exceptional clause explicitly does not set the thread's current exception information.
            // So while the pseudo code says:
            //    exit(*sys.exc_info())
            // we'll actually do:
            //    exit(*PythonOps.GetExceptionInfoLocal($exception))
            return ag.Convert(
                typeof(bool),
                ConversionResultKind.ExplicitCast,
                ag.Invoke(
                    typeof(object),
                    new CallSignature(ArgumentType.List),
                    exit,
                    Ast.Call(
                        AstGenerator.GetHelperMethod("GetExceptionInfoLocal"),
                        ag.LocalContext,
                        exception
                    )
                )
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_contextManager != null) {
                    _contextManager.Walk(walker);
                }
                if (_var != null) {
                    _var.Walk(walker);
                }
                if (_body != null) {
                    _body.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
