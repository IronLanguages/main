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

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Binding;

#if !CLR2
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Compiler.Ast {
    using Ast = MSAst.Expression;

    public class TryStatement : Statement {
        private SourceLocation _header;

        /// <summary>
        /// The statements under the try-block.
        /// </summary>
        private Statement _body;

        /// <summary>
        /// Array of except (catch) blocks associated with this try. NULL if there are no except blocks.
        /// </summary>
        private readonly TryStatementHandler[] _handlers;

        /// <summary>
        /// The body of the optional Else block for this try. NULL if there is no Else block.
        /// </summary>
        private Statement _else;

        /// <summary>
        /// The body of the optional finally associated with this try. NULL if there is no finally block.
        /// </summary>
        private Statement _finally;

        public TryStatement(Statement body, TryStatementHandler[] handlers, Statement else_, Statement finally_) {
            _body = body;
            _handlers = handlers;
            _else = else_;
            _finally = finally_;
        }

        public SourceLocation Header {
            set { _header = value; }
        }

        public Statement Body {
            get { return _body; }
        }

        public Statement Else {
            get { return _else; }
        }

        public Statement Finally {
            get { return _finally; }
        }

        public IList<TryStatementHandler> Handlers {
            get { return _handlers; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            // allocated all variables here so they won't be shared w/ other 
            // locals allocated during the body or except blocks.
            MSAst.ParameterExpression noNestedException = null;
            if (_finally != null) {
                noNestedException = ag.GetTemporary("$noException", typeof(bool));
            }

            MSAst.ParameterExpression lineUpdated = null;
            MSAst.ParameterExpression runElse = null;

            if (_else != null || (_handlers != null && _handlers.Length > 0)) {
                lineUpdated = ag.GetTemporary("$lineUpdated", typeof(bool));
                if (_else != null) {
                    runElse = ag.GetTemporary("run_else", typeof(bool));
                }
            }

            // don't allocate locals below here...
            MSAst.Expression body = ag.Transform(_body);
            MSAst.Expression @else = ag.Transform(_else);

            if (body == null) {
                return null;
            }

            if (_handlers != null) {
                foreach (var handler in _handlers) {
                    ag.HandlerLocations[handler.Span.Start.Line] = false;
                }
            }

            MSAst.ParameterExpression exception;

            MSAst.Expression @catch = TransformHandlers(ag, out exception);
            MSAst.Expression result;

            // We have else clause, must generate guard around it
            if (@else != null) {
                Debug.Assert(@catch != null);

                //  run_else = true;
                //  try {
                //      try_body
                //  } catch ( ... ) {
                //      run_else = false;
                //      catch_body
                //  }
                //  if (run_else) {
                //      else_body
                //  }
                result =
                    Ast.Block(
                        Ast.Assign(runElse, AstUtils.Constant(true)),
                        // save existing line updated, we could choose to do this only for nested exception handlers.
                        ag.PushLineUpdated(false, lineUpdated),
                        AstUtils.Try(
                            ag.AddDebugInfo(AstUtils.Empty(), new SourceSpan(Span.Start, _header)),
                            body
                        ).Catch(exception,
                            Ast.Assign(runElse, AstUtils.Constant(false)),
                            @catch,
                            // restore existing line updated after exception handler completes
                            ag.PopLineUpdated(lineUpdated),
                            AstUtils.Default(body.Type)
                        ),
                        AstUtils.IfThen(runElse,
                            @else
                        ),
                        AstUtils.Empty()
                    );

            } else if (@catch != null) {        // no "else" clause
                //  try {
                //      <try body>
                //  } catch (Exception e) {
                //      ... catch handling ...
                //  }
                //
                result = AstUtils.Try(
                        ag.AddDebugInfo(AstUtils.Empty(), new SourceSpan(Span.Start, _header)),
                        // save existing line updated
                        ag.PushLineUpdated(false, lineUpdated),
                        body
                    ).Catch(exception,
                        @catch,
                        // restore existing line updated after exception handler completes
                        ag.PopLineUpdated(lineUpdated),
                        Ast.Call(AstGenerator.GetHelperMethod("ExceptionHandled"), ag.LocalContext),
                        AstUtils.Default(body.Type)
                    );
            } else {
                result = body;
            }

            try {
                return AddFinally(ag, result, noNestedException);
            } finally {
                // free all locals here after the children nodes have been generated
                if (lineUpdated != null) {
                    ag.FreeTemp(lineUpdated);
                }
                if (runElse != null) {
                    ag.FreeTemp(@runElse);
                }
            }
        }

        private MSAst.Expression AddFinally(AstGenerator/*!*/ ag, MSAst.Expression/*!*/ body, MSAst.ParameterExpression nestedException) {
            if (_finally != null) {
                bool isEmitting = ag._isEmittingFinally;
                ag._isEmittingFinally = true;
                int loopId = ++ag._loopOrFinallyId;
                ag.LoopOrFinallyIds.Add(loopId, true);
                try {
                    Debug.Assert(nestedException != null);

                    MSAst.ParameterExpression nestedFrames = ag.GetTemporary("$nestedFrames", typeof(List<DynamicStackFrame>));

                    bool inFinally = ag.InFinally;
                    ag.InFinally = true;
                    MSAst.Expression @finally = ag.Transform(_finally);
                    ag.InFinally = inFinally;
                    if (@finally == null) {
                        // error reported during compilation
                        return null;
                    }

                    // lots is going on here.  We need to consider:
                    //      1. Exceptions propagating out of try/except/finally.  Here we need to save the line #
                    //          from the exception block and not save the # from the finally block later.
                    //      2. Exceptions propagating out of the finally block.  Here we need to report the line number
                    //          from the finally block and leave the existing stack traces cleared.
                    //      3. Returning from the try block: Here we need to run the finally block and not update the
                    //          line numbers.
                    body = AstUtils.Try( // we use a fault to know when we have an exception and when control leaves normally (via
                        // either a return or the body completing successfully).
                        AstUtils.Try(
                            ag.AddDebugInfo(AstUtils.Empty(), new SourceSpan(Span.Start, _header)),
                            Ast.Assign(nestedException, AstUtils.Constant(false)),
                            body
                        ).Fault(
                        // fault
                            Ast.Assign(nestedException, AstUtils.Constant(true))
                        )
                    ).FinallyWithJumps(
                        // if we had an exception save the line # that was last executing during the try
                        AstUtils.If(
                            nestedException,
                            ag.GetSaveLineNumberExpression(false)
                        ),

                        // clear the frames incase thae finally throws, and allow line number
                        // updates to proceed
                        ag.UpdateLineUpdated(false),
                        Ast.Assign(
                            nestedFrames,
                            Ast.Call(AstGenerator.GetHelperMethod("GetAndClearDynamicStackFrames"))
                        ),

                        // run the finally code
                        @finally,

                        // if the finally exits normally restore any previous exception info
                        Ast.Call(
                            AstGenerator.GetHelperMethod("SetDynamicStackFrames"),
                            nestedFrames
                        ),

                        // if we took an exception in the try block we have saved the line number.  Otherwise
                        // we have no line number saved and will need to continue saving them if
                        // other exceptions are thrown.
                        AstUtils.If(                            
                            nestedException,
                            ag.UpdateLineUpdated(true)
                        )
                    );
                    ag.FreeTemp(nestedFrames);
                    ag.FreeTemp(nestedException);
                } finally {
                    ag._isEmittingFinally = isEmitting;
                    ag.LoopOrFinallyIds.Remove(loopId);
                }
            }
            return body;
        }


        /// <summary>
        /// Transform multiple python except handlers for a try block into a single catch body.
        /// </summary>
        /// <param name="ag"></param>
        /// <param name="variable">The variable for the exception in the catch block.</param>
        /// <returns>Null if there are no except handlers. Else the statement to go inside the catch handler</returns>
        private MSAst.Expression TransformHandlers(AstGenerator ag, out MSAst.ParameterExpression variable) {
            if (_handlers == null || _handlers.Length == 0) {
                variable = null;
                return null;
            }
            bool emittingFinally = ag._isEmittingFinally;
            ag._isEmittingFinally = false;
            try {
                MSAst.ParameterExpression exception = ag.GetTemporary("exception", typeof(Exception));
                MSAst.ParameterExpression extracted = ag.GetTemporary("extracted", typeof(object));

                // The variable where the runtime will store the exception.
                variable = exception;

                var tests = new List<Microsoft.Scripting.Ast.IfStatementTest>(_handlers.Length);
                MSAst.ParameterExpression converted = null;
                MSAst.Expression catchAll = null;

                for (int index = 0; index < _handlers.Length; index++) {
                    TryStatementHandler tsh = _handlers[index];

                    if (tsh.Test != null) {
                        Microsoft.Scripting.Ast.IfStatementTest ist;

                        //  translating:
                        //      except Test ...
                        //
                        //  generate following AST for the Test (common part):
                        //      CheckException(exception, Test)
                        MSAst.Expression test =
                            Ast.Call(
                                AstGenerator.GetHelperMethod("CheckException"),
                                ag.LocalContext,
                                extracted,
                                ag.TransformAsObject(tsh.Test)
                            );

                        if (tsh.Target != null) {
                            //  translating:
                            //      except Test, Target:
                            //          <body>
                            //  into:
                            //      if ((converted = CheckException(exception, Test)) != null) {
                            //          Target = converted;
                            //          traceback-header
                            //          <body>
                            //      }

                            if (converted == null) {
                                converted = ag.GetTemporary("converted");
                            }

                            ist = AstUtils.IfCondition(
                                Ast.NotEqual(
                                    Ast.Assign(converted, test),
                                    AstUtils.Constant(null)
                                ),
                                Ast.Block(
                                    tsh.Target.TransformSet(ag, SourceSpan.None, converted, PythonOperationKind.None),
                                    ag.AddDebugInfo(
                                        GetTracebackHeader(
                                            ag,
                                            exception,
                                            ag.Transform(tsh.Body)
                                        ),
                                        new SourceSpan(tsh.Start, tsh.Header)
                                    ),
                                    AstUtils.Empty()
                                )
                            );
                        } else {
                            //  translating:
                            //      except Test:
                            //          <body>
                            //  into:
                            //      if (CheckException(exception, Test) != null) {
                            //          traceback-header
                            //          <body>
                            //      }
                            ist = AstUtils.IfCondition(
                                Ast.NotEqual(
                                    test,
                                    AstUtils.Constant(null)
                                ),
                                ag.AddDebugInfo(
                                    GetTracebackHeader(
                                        ag,
                                        exception,
                                        ag.Transform(tsh.Body)
                                    ),
                                    new SourceSpan(tsh.Start, tsh.Header)
                                )
                            );
                        }

                        // Add the test to the if statement test cascade
                        tests.Add(ist);
                    } else {
                        Debug.Assert(index == _handlers.Length - 1);
                        Debug.Assert(catchAll == null);

                        //  translating:
                        //      except:
                        //          <body>
                        //  into:
                        //  {
                        //          traceback-header
                        //          <body>
                        //  }

                        catchAll = ag.AddDebugInfo(
                            GetTracebackHeader(ag, exception, ag.Transform(tsh.Body)),
                            new SourceSpan(tsh.Start, tsh.Header)
                        );
                    }
                }

                MSAst.Expression body = null;

                if (tests.Count > 0) {
                    // rethrow the exception if we have no catch-all block
                    if (catchAll == null) {
                        catchAll = Ast.Block(
                            ag.GetSaveLineNumberExpression(true),
                            Ast.Throw(
                                Ast.Call(
                                    typeof(ExceptionHelpers).GetMethod("UpdateForRethrow"),
                                    exception
                                )
                            )
                        );
                    }

                    body = AstUtils.If(
                        tests.ToArray(),                        
                        catchAll
                    );
                } else {
                    Debug.Assert(catchAll != null);
                    body = catchAll;
                }

                if (converted != null) {
                    ag.FreeTemp(converted);
                }
                ag.FreeTemp(exception);
                ag.FreeTemp(extracted);

                // Codegen becomes:
                //     extracted = PythonOps.SetCurrentException(exception)
                //      < dynamic exception analysis >
                return Ast.Block(
                    Ast.Assign(
                        extracted,
                        Ast.Call(
                            AstGenerator.GetHelperMethod("SetCurrentException"),
                            ag.LocalContext,
                            exception
                        )
                    ),
                    body,
                    AstUtils.Empty()
                );
            } finally {
                ag._isEmittingFinally = emittingFinally;
            }
        }

        /// <summary>
        /// Surrounds the body of an except block w/ the appropriate code for maintaining the traceback.
        /// </summary>
        internal static MSAst.Expression GetTracebackHeader(AstGenerator ag, MSAst.ParameterExpression exception, MSAst.Expression body) {
            // we are about to enter a except block.  We need to emit the line number update so we track
            // the line that the exception was thrown from.  We then need to build exc_info() so that
            // it's available.  Finally we clear the list of dynamic stack frames because they've all
            // been associated with this exception.
            return Ast.Block(
                // pass false so if we take another exception we'll add it to the frame list
                ag.GetSaveLineNumberExpression(false),
                Ast.Call(
                    AstGenerator.GetHelperMethod("BuildExceptionInfo"),
                    ag.LocalContext,
                    exception
                ),
                body,
                AstUtils.Empty()
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_body != null) {
                    _body.Walk(walker);
                }
                if (_handlers != null) {
                    foreach (TryStatementHandler handler in _handlers) {
                        handler.Walk(walker);
                    }
                }
                if (_else != null) {
                    _else.Walk(walker);
                }
                if (_finally != null) {
                    _finally.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        internal override bool CanThrow {
            get {
                return false;
            }
        }
    }

    // A handler corresponds to the except block.
    public class TryStatementHandler : Node {
        private SourceLocation _header;
        private readonly Expression _test, _target;
        private readonly Statement _body;

        public TryStatementHandler(Expression test, Expression target, Statement body) {
            _test = test;
            _target = target;
            _body = body;
        }

        public SourceLocation Header {
            get { return _header; }
            set { _header = value; }
        }

        public Expression Test {
            get { return _test; }
        }

        public Expression Target {
            get { return _target; }
        }

        public Statement Body {
            get { return _body; }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_test != null) {
                    _test.Walk(walker);
                }
                if (_target != null) {
                    _target.Walk(walker);
                }
                if (_body != null) {
                    _body.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
