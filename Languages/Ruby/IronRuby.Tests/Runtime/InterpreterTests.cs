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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Reflection;

namespace IronRuby.Tests {
    using Ast = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public partial class Tests {
        #region Tracing Helpers

        [ThreadStatic]
        public static List<object> _trace = new List<object>();

        public static void Trace(object value) {
            _trace.Add(value);
        }

        private Expression TraceCall(Expression arg) {
            return Ast.Call(null, typeof(Tests).GetMethod("Trace"), Ast.Convert(arg, typeof(object)));
        }

        private void RunTraceTest(Expression<Action> lambda, Action<Action> invoker, out object[] compiled, out object[] interpreted, out object[] sync) {
            if (invoker == null) {
                invoker = (a) => a();
            }

            _trace.Clear();
            
            invoker(lambda.Compile());
            compiled = _trace.ToArray();
            _trace.Clear();

            // force synchronous compilation:
            invoker(lambda.LightCompile(0));
            sync = _trace.ToArray();
            _trace.Clear();

            // force interpretation:
            invoker(lambda.LightCompile(Int32.MaxValue));
            interpreted = _trace.ToArray();
            _trace.Clear();
        }

        private void TraceTestLambda(Expression<Action> lambda, Type expectedException) {
            TraceTestLambda(lambda, (a) => {
                try {
                    a();
                    Assert(false, "Expected exception " + expectedException);
                } catch (Exception e) {
                    Assert(e.GetType() == expectedException, "Expected exception " + expectedException);
                }
            });
        }

        private void TraceTestLambda(Expression<Action> lambda) {
            TraceTestLambda(lambda, (Action<Action>)null);
        }

        private void TraceTestLambda(Expression<Action> lambda, Action<Action> invoker) {
            object[] compiled, interpreted, sync;
            RunTraceTest(lambda, invoker, out compiled, out interpreted, out sync);
            Assert(compiled.ValueEquals(interpreted));
            Assert(compiled.ValueEquals(sync));
        }

        private void XTraceTestLambda(Expression<Action> lambda) {
            object[] compiled, interpreted, sync;
            RunTraceTest(lambda, null, out compiled, out interpreted, out sync);

            Console.WriteLine("-- compiled --");

            foreach (var obj in compiled) {
                Console.WriteLine(obj);
            }

            Console.WriteLine("-- interpreted --");

            foreach (var obj in interpreted) {
                Console.WriteLine(obj);
            }

            Console.WriteLine("-- sync --");

            foreach (var obj in sync) {
                Console.WriteLine(obj);
            }
        }

        #endregion

        [Options(NoRuntime = true)]
        public void Interpreter1A() {
            var m_AddValue = new Action<StrongBox<int>, int>(Interpreter1_AddValue).Method;
            var m_ThrowNSE = new Action(Interpreter1_ThrowNSE).Method;
            var m_f = new Func<int, int, int, int, int>(Interpreter1_f).Method;
            var m_g = new Func<int, int, int, int>(Interpreter1_g).Method;


            var value = new StrongBox<int>(0);
            LabelTarget label;
            int rc, ri;

            // value of try-catch:
            var l0 = Ast.Lambda<Func<int>>(
                Ast.TryCatch(Ast.Constant(1, typeof(int)), Ast.Catch(typeof(Exception), Ast.Constant(2, typeof(int))))
            );

            rc = l0.Compile()();
            ri = l0.LightCompile()();
            Assert(rc == ri);

            // cross-block goto in try-catch-finally:
            label = Ast.Label(typeof(int));
            var l1 = Ast.Lambda<Func<int>>(
                Ast.Label(label, Ast.TryCatchFinally(
                    Ast.Goto(label, Ast.Constant(1), typeof(int)),
                    Ast.Call(null, m_AddValue, Ast.Constant(value), Ast.Constant(1)),
                    Ast.Catch(typeof(Exception), Ast.Constant(2, typeof(int)))
                )));

            value.Value = 0;
            rc = l1.Compile()();
            ri = l1.LightCompile()();

            Assert(value.Value == 11);
            Assert(rc == ri);

            // cross-block goto in try-catch-finally with an exception thrown and caught in finally:
            label = Ast.Label(typeof(int));
            var l2 = Ast.Lambda<Func<int>>(
                Ast.Label(label, Ast.TryCatchFinally(
                    Ast.Goto(label, Ast.Constant(1), typeof(int)),
                    Ast.Block(
                        Ast.TryCatch(
                            Ast.Call(null, m_ThrowNSE),
                            Ast.Catch(typeof(NotSupportedException),
                                Ast.Call(null, m_AddValue, Ast.Constant(value), Ast.Constant(1))
                            )
                        ),

                        Ast.Call(null, m_AddValue, Ast.Constant(value), Ast.Constant(2))
                    ),
                    Ast.Catch(typeof(Exception), Ast.Constant(2, typeof(int)))
                )));

            value.Value = 0;
            rc = l2.Compile()();
            ri = l2.LightCompile()();

            Assert(value.Value == 1212);
            Assert(rc == ri);

            // executing fault and finally blocks for an exception coming from a method call:
            label = Ast.Label(typeof(int));
            var a = Ast.Lambda<Action>(
                Ast.TryCatch(
                    Ast.TryFinally(// TODO: faults not supported yet: Ast.TryFault(
                        Ast.TryFinally(
                            Ast.Call(null, m_ThrowNSE),
                            Ast.Call(null, m_AddValue, Ast.Constant(value), Ast.Constant(1))
                        ),
                        Ast.Call(null, m_AddValue, Ast.Constant(value), Ast.Constant(2))
                    ),
                    Ast.Catch(typeof(NotSupportedException),
                        Ast.Call(null, m_AddValue, Ast.Constant(value), Ast.Constant(3))
                    )
                )
            );

            value.Value = 0;
            a.Compile()();
            a.LightCompile()();

            Assert(value.Value == 123123);


            // try-catch with non-empty stack:
            var l3 = Ast.Lambda<Func<int>>(
                Ast.Call(null, m_f,
                    Ast.Constant(1),
                    Ast.Constant(2),
                    Ast.TryCatch(
                        Ast.Call(null, m_g,
                            Ast.Constant(3),
                            Ast.Constant(4),
                            Ast.Throw(Ast.Constant(new Exception("!!!")), typeof(int))
                        ),
                        Ast.Catch(
                            typeof(Exception),
                            Ast.Constant(5)
                        )
                    ),
                    Ast.Constant(7)
                )
            );

            rc = l3.Compile()();
            ri = l3.LightCompile()();
            Assert(rc == ri);


            // goto carrying a value needs to pop the value from the stack before executing finally
            // clauses to prevent stack overflow (the finally clause doesn't expect the value on the stack):
            var label3 = Ast.Label(typeof(int));
            var l4 = Ast.Lambda<Func<int>>(
                Ast.Label(label3, 
                    Ast.Block(
                        Ast.TryFinally(
                            Ast.Goto(label3, Ast.Constant(1), typeof(void)),
                            Ast.Call(null, m_g, Ast.Constant(2), Ast.Constant(3), Ast.Constant(4))
                        ),
                        Ast.Constant(3)
                    )
                )
            );

            rc = l4.Compile()();
            ri = l4.LightCompile()();
            Assert(rc == ri);

            
            // goto needs to pop unused values from the stack before it executes finally:
            var label4 = Ast.Label(typeof(int));
            var l5 = Ast.Lambda<Func<int>>(
                Ast.Label(label4,
                    Ast.Block(
                        Ast.TryFinally(
                            Ast.Call(null, m_g, Ast.Constant(9), Ast.Constant(8), Ast.Goto(label4, Ast.Constant(1), typeof(int))),
                            Ast.Call(null, m_f, Ast.Constant(2), Ast.Constant(3), Ast.Constant(4), Ast.Constant(5))
                        ),
                        Ast.Constant(3)
                    )
                )
            );

            rc = l5.Compile()();
            ri = l5.LightCompile()();
            Assert(rc == ri);
        }

        public static void Interpreter1_AddValue(StrongBox<int> value, int d) {
            value.Value = value.Value * 10 + d;
        }

        public static void Interpreter1_ThrowNSE() {
            throw new NotSupportedException();
        }

        public static int Interpreter1_f(int a, int b, int c, int d) {
            return a * 1000 + b * 100 + c * 10 + d;
        }

        public static int Interpreter1_g(int a, int b, int c) {
            return 20;
        }

        [Options(NoRuntime = true)]
        public void Interpreter1B() {
            LabelTarget label = Ast.Label();

            // throw in a finally that is executed while jumping to a label cancels the jump:
            var l0 = Ast.Lambda<Action>(
                Ast.TryFinally(                                                         
                    Ast.TryCatch(
                        Ast.Block(
                            Ast.TryFinally(                                             
                                Ast.TryFinally(                                         
                                    Ast.Block(
                                        TraceCall(Ast.Constant(0)),
                                        Ast.Goto(label)                                 
                                    ),
                                    // F2:
                                    Ast.Block(                                          
                                        TraceCall(Ast.Constant(1)),
                                        Ast.Throw(Ast.Constant(new Exception("foo")))   
                                    )                                                   
                                ),
                                // F1:
                                TraceCall(Ast.Constant(2))                              
                            ),
                            // LABEL:
                            Ast.Label(label),
                            TraceCall(Ast.Constant(3))
                        ),
                        // CATCH:
                        Ast.Catch(typeof(Exception),
                            TraceCall(Ast.Constant(4))
                        )
                    ),
                    // F0:
                    TraceCall(Ast.Constant(5))
                )
            );

            TraceTestLambda(l0);

            // throw in a finally that is executed while jumping to a label cancels the jump:
            var l1 = Ast.Lambda<Action>(
                Ast.Block(
                    Ast.TryFinally(                                                         
                        Ast.TryCatch(
                            Ast.Block(
                                Ast.TryFinally(                                             
                                    Ast.TryFinally(                                         
                                        Ast.Block(
                                            TraceCall(Ast.Constant(0)),
                                            Ast.Goto(label)                                 
                                        ),
                                        // F2:                                              
                                        Ast.Block(                                          
                                            TraceCall(Ast.Constant(1)),
                                            Ast.Throw(Ast.Constant(new Exception("foo")))   
                                        )                                                   
                                    ),
                                    // F1:
                                    TraceCall(Ast.Constant(2))
                                ),
                                
                                TraceCall(Ast.Constant(3))
                            ),
                            // CATCH:
                            Ast.Catch(typeof(Exception),
                                TraceCall(Ast.Constant(4))
                            )
                        ),
                        // F0:
                        TraceCall(Ast.Constant(5))
                    ),
                    Ast.Block(
                        TraceCall(Ast.Constant(6)),
                        // LABEL:
                        Ast.Label(label),
                        TraceCall(Ast.Constant(7))
                    )
                )
            );

            TraceTestLambda(l1);

            // throw caught a try-catch in a finally that is executed while jumping to a label doesn't cancel the jump
            var l2 = Ast.Lambda<Action>(
                Ast.TryCatch(
                    Ast.Block(
                        Ast.TryFinally(
                            Ast.TryFinally(
                                Ast.Block(
                                    TraceCall(Ast.Constant(0)),
                                    Ast.Goto(label)
                                ),
                                Ast.Block(
                                    TraceCall(Ast.Constant(1)),
                                    Ast.TryCatch(
                                        Ast.Throw(Ast.Constant(new Exception("foo"))),
                                        Ast.Catch(typeof(Exception),
                                            TraceCall(Ast.Constant(2))
                                        )
                                    )
                                )
                            ),
                            TraceCall(Ast.Constant(3))
                        ),
                        TraceCall(Ast.Constant(4)),
                        Ast.Label(label),
                        TraceCall(Ast.Constant(5))
                    ),
                    Ast.Catch(typeof(Exception),
                        TraceCall(Ast.Constant(6))
                    )
                )
            );

            TraceTestLambda(l2);
        }

        /// <summary>
        /// Faults.
        /// </summary>
        [Options(NoRuntime = true)]
        public void Interpreter1C() {
            var inner =
                Ast.TryFinally(
                    Ast.TryCatch(
                        Ast.TryFinally(
                            Ast.Block(
                                TraceCall(Ast.Constant(0)),
                                Ast.Throw(Ast.Constant(new NotSupportedException("ex1")))
                            ),
                            TraceCall(Ast.Constant(1))
                        ),
                        Ast.Catch(typeof(Exception),
                            Ast.Block(
                                TraceCall(Ast.Constant(2)),
                                Ast.Rethrow()
                            )
                        )
                    ),
                    TraceCall(Ast.Constant(3))
                );

            var l0 = Ast.Lambda<Action>(
                Ast.TryCatch(
                    inner,
                    Ast.Catch(typeof(Exception), 
                        TraceCall(Ast.Constant(4))
                    )
                )
            );

            TraceTestLambda(l0);

            var l1 = Ast.Lambda<Action>(
                Ast.TryCatch(
                    inner,
                    Ast.Catch(typeof(InvalidOperationException),
                        TraceCall(Ast.Constant(4))
                    )
                )
            );

            TraceTestLambda(l1, typeof(NotSupportedException));

            var l2 = Ast.Lambda<Action>(
                inner
            );

            TraceTestLambda(l2, typeof(NotSupportedException));
        }

        /// <summary>
        /// Faults.
        /// </summary>
        [Options(NoRuntime = true)]
        public void Interpreter1D() {
            var l1 = Ast.Lambda<Action>(
                TraceCall(
                    Ast.TryCatch(
                        Ast.Call(null, new Func<int>(ThrowNSEReturnInt).Method),
                        Ast.Catch(typeof(Exception),
                            Ast.Block(
                                Ast.Constant(2),
                                Ast.Rethrow(typeof(int))
                            )
                        )
                    )
                )
            );

            l1.LightCompile();
        }

        public static int ThrowNSEReturnInt() {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Jump out of finally.
        /// </summary>
        [Options(NoRuntime = true)]
        public void Interpreter_JumpFromFinally1() {
            var label1 = Ast.Label();
            var label2 = Ast.Label();

            var l0 = Ast.Lambda<Action>(
                Ast.Block(
                    Ast.TryFinally(                                                         
                        Ast.Block(
                            AstUtils.FinallyFlowControl(
                                Ast.TryFinally(                                             
                                    Ast.TryFinally(                                         
                                        Ast.Block(
                                            TraceCall(Ast.Constant(0)),
                                            Ast.Goto(label1)                                 
                                        ),
                                        // F2:                                              
                                        Ast.Block(                                          
                                            TraceCall(Ast.Constant(1)),
                                            Ast.Goto(label2)
                                        )                                                   
                                    ),
                                    // F1:
                                    TraceCall(Ast.Constant(2))
                                )
                            ),
                            TraceCall(Ast.Constant(3)),

                            // LABEL2:
                            Ast.Label(label2),
                            TraceCall(Ast.Constant(4))
                        ),
                        // F0:
                        TraceCall(Ast.Constant(5))
                    ),
                    Ast.Block(
                        TraceCall(Ast.Constant(6)),
                        // LABEL1:
                        Ast.Label(label1),
                        TraceCall(Ast.Constant(7))
                    )
                )
            );

            TraceTestLambda(l0);
        }

        /// <summary>
        /// Pending continuation override: jump from finally should cancel any pending jumps (gotos or throws) from try-block.
        /// </summary>
        [Options(NoRuntime = true)]
        public void Interpreter_JumpFromFinally2() {
            var label = Ast.Label();

            var l0 = Ast.Lambda<Action>(
                AstUtils.FinallyFlowControl(
                    Ast.Block(
                        Ast.TryFinally(
                            Ast.Throw(Ast.Constant(new Exception("ex"))),
                            Ast.Block(
                                TraceCall(Ast.Constant(0)),
                                Ast.Goto(label)
                            )
                        ),
                        Ast.Label(label),
                        TraceCall(Ast.Constant(1))
                    )
                )
            );

            TraceTestLambda(l0);
        }

        [Options(NoRuntime = true)]
        public void Interpreter2() {
            Interpreter2_Test<Func<int>>(
                (var) => Ast.Lambda<Func<int>>(var),
                (c, i) => c() == i()
            );

            Interpreter2_Test<IRuntimeVariables>(
                (var) => Ast.RuntimeVariables(var),
                (c, i) => (int)c[0] == (int)i[0]
            );
        }

        private void Interpreter2_Test<TClosure>(Func<ParameterExpression, Expression> closure, Func<TClosure, TClosure, bool> comparer) {
            // value of try-catch:
            var closureVar = Ast.Parameter(typeof(int), "x");
            var indexVar = Ast.Parameter(typeof(int), "i");
            var limitVar = Ast.Parameter(typeof(int), "limit");
            var closuresVar = Ast.Parameter(typeof(TClosure[]), "closures");
            var returnLabel = Ast.Label(typeof(void));

            var l0 = Ast.Lambda<Action<int, int, TClosure[]>>(
                Ast.Label(returnLabel,
                    Ast.Loop(
                        Ast.IfThenElse(
                            Ast.NotEqual(indexVar, limitVar),
                            Ast.Block(new[] { closureVar },
                                Ast.Assign(closureVar, indexVar),
                                Ast.Assign(Ast.ArrayAccess(closuresVar, indexVar), closure(closureVar)),
                                Ast.Assign(indexVar, Ast.Add(indexVar, Ast.Constant(1)))
                            ),
                            Ast.Return(returnLabel)
                        )
                    )
               ),
               new[] { indexVar, limitVar, closuresVar }
            );

            var fsc = new TClosure[2];
            l0.Compile()(0, 2, fsc);

            var fsi = new TClosure[2];
            l0.LightCompile()(0, 2, fsi);

            Assert(comparer(fsc[0], fsi[0]));
            Assert(comparer(fsc[1], fsi[1]));
        }

        /// <summary>
        /// ThreadAbortException handling.
        /// </summary>
        [Options(NoRuntime = true)]
        public void Interpreter3() {
            if (_driver.PartialTrust) return;

            var label = Ast.Label(typeof(void));
            foreach (var gotoLabel in new Expression[] { Ast.Goto(label), Ast.Empty() }) {
                var var_tracker = Ast.Parameter(typeof(List<object>));
                var var_abort1 = Ast.Parameter(typeof(ThreadAbortException));
                var var_e = Ast.Parameter(typeof(Exception));
                var var_abort2 = Ast.Parameter(typeof(ThreadAbortException));

                var m_Interpreter3_abort = new Action(Interpreter3_abort).Method;
                var m_Interpreter3_catchAbortThrowNSE = new Action<List<object>, ThreadAbortException>(Interpreter3_catchAbortThrowNSE).Method;
                var m_Interpreter3_catchE = new Action<List<object>, Exception>(Interpreter3_catchE).Method;
                var m_Interpreter3_unreachable = new Action<List<object>>(Interpreter3_unreachable).Method;
                var m_Interpreter3_catchAbort = new Action<List<object>, ThreadAbortException>(Interpreter3_catchAbort).Method;

                var l = Ast.Lambda<Action<List<object>>>(
                    AstUtils.Try(
                        AstUtils.Try(
                            AstUtils.Try(
                                Ast.Call(null, m_Interpreter3_abort)
                            ).Catch(var_abort1,
                                Ast.Call(null, m_Interpreter3_catchAbortThrowNSE, var_tracker, var_abort1)
                            )
                        ).Catch(var_e,
                            Ast.Call(null, m_Interpreter3_catchE, var_tracker, var_e),
                            gotoLabel
                        ),
                        Ast.Call(null, m_Interpreter3_unreachable, var_tracker),
                        Ast.Label(label, Ast.Call(null, m_Interpreter3_unreachable, var_tracker))
                    ).Catch(var_abort2,
                        Ast.Call(null, m_Interpreter3_catchAbort, var_tracker, var_abort2)
                    ),
                    new[] { var_tracker }
                );

                var ctracker = new List<object>();
                var itracker = new List<object>();
                try {
                    l.Compile()(ctracker);
                    l.LightCompile()(itracker);
                } catch (ThreadAbortException) {
                    Thread.ResetAbort();
                    Assert(false);
                    return;
                }

                foreach (var t in new[] { ctracker, itracker }) {
                    Assert(t.Count == 10);
                    Assert(t[0] as string == "1");
                    Assert(t[1] is ThreadAbortException);
                    Assert(t[2] as string == "stateInfo");
                    Assert(t[3] as string == "2");
                    Assert(t[4] is NotSupportedException);
                    Assert((ThreadState)t[5] == ThreadState.AbortRequested);
                    Assert(t[6] as string == "3");
                    Assert(t[7] is ThreadAbortException);
                    Assert(ReferenceEquals(t[8], t[2]));
                    Assert((ThreadState)t[9] == ThreadState.AbortRequested);
                }
            }
        }

        public static void Interpreter3_abort() {
            Thread.CurrentThread.Abort("stateInfo");
        }

        public static void Interpreter3_catchAbortThrowNSE(List<object> tracker, ThreadAbortException e) {
            tracker.Add("1");
            tracker.Add(e);
            tracker.Add(e.ExceptionState);
            throw new NotSupportedException();
        }

        public static void Interpreter3_catchE(List<object> tracker, Exception e) {
            tracker.Add("2");
            tracker.Add(e);
            tracker.Add(Thread.CurrentThread.ThreadState);
        }

        public static void Interpreter3_unreachable(List<object> tracker) {
            tracker.Add("UNREACHABLE");
        }

        public static void Interpreter3_catchAbort(List<object> tracker, ThreadAbortException e) {
            tracker.Add("3");
            tracker.Add(e);
            tracker.Add(e.ExceptionState);
            tracker.Add(Thread.CurrentThread.ThreadState);
            Thread.ResetAbort();
        }

        public static void Interpreter4() {
            // TODO: figure out if this is specified behavior of thread abort:

            //var var_tracker = Ast.Parameter(typeof(List<object>));
            //var var_abort1 = Ast.Parameter(typeof(ThreadAbortException));
            //var var_abort2 = Ast.Parameter(typeof(ThreadAbortException));
            //var var_abort3 = Ast.Parameter(typeof(ThreadAbortException));
            //var var_abort4 = Ast.Parameter(typeof(ThreadAbortException));

            //var m_Interpreter4_abort = new Action(Interpreter4_abort).Method;
            //var m_Interpreter4_traceE = new Action<List<object>, ThreadAbortException>(Interpreter4_traceE).Method;
            //var m_Interpreter4_traceS = new Action<List<object>, string>(Interpreter4_traceS).Method;
            //var m_Interpreter4_reset = new Action<List<object>, ThreadAbortException>(Interpreter4_reset).Method;

            //var l = Ast.Lambda<Action<List<object>>>(
            //    AstUtils.Try(
            //        AstUtils.Try(
            //            Ast.Call(null, m_Interpreter4_abort, Ast.Constant("r1"))
            //        ).Catch(var_abort1,
            //            Ast.Call(null, m_Interpreter4_traceE, var_tracker, var_abort1),
            //            AstUtils.Try(
            //                AstUtils.Try(
            //                    Ast.Call(null, m_Interpreter4_abort, Ast.Constant("r2"))
            //                ).Catch(var_abort2,
            //                    Ast.Call(null, m_Interpreter4_traceE, var_tracker, var_abort2)
            //                ),
            //                Ast.Call(null, m_Interpreter4_traceS, var_tracker, Ast.Constant("1"))
            //            ).Catch(var_abort3,
            //                Ast.Call(null, m_Interpreter4_traceE, var_tracker, var_abort3)
            //            ),
            //            Ast.Call(null, m_Interpreter4_traceS, var_tracker, Ast.Constant("2"))
            //        ),
            //        Ast.Call(null, m_Interpreter4_traceS, var_tracker, Ast.Constant("2"))
            //    ).Catch(var_abort4,
            //        Ast.Call(null, m_Interpreter4_reset, var_tracker, var_abort4)
            //    ),
            //    new[] { var_tracker }
            //);

            //var ctracker = new List<object>();
            //var itracker = new List<object>();
            //try {
            //    l.Compile()(ctracker);
            //    //l.LightCompile()(itracker);
            //} catch (ThreadAbortException) {
            //    Thread.ResetAbort();
            //    Assert(false);
            //    return;
            //}
        }

        public static void Interpreter4_abort(string value) {
            Thread.CurrentThread.Abort(value);
        }

        public static void Interpreter4_traceE(List<object> tracker, ThreadAbortException e) {
            tracker.Add(e.ExceptionState);
        }

        public static void Interpreter4_traceS(List<object> tracker, string s) {
            tracker.Add(s);
        }

        public static void Interpreter4_reset(List<object> tracker, ThreadAbortException e) {
            tracker.Add(e.ExceptionState);
            Thread.ResetAbort();
            tracker.Add(e.ExceptionState);
        }

        [Options(NoRuntime = true)]
        public void Interpreter5() {
            var strArray1 = new string[] { "foo" };
            var strArray2 = new string[1, 1];
            var strArray3 = new string[1, 1, 1];
            var strArray8 = new string[1, 1, 1, 1, 1, 1, 1, 1];
            strArray2[0, 0] = "foo";
            strArray3[0, 0, 0] = "foo";
            strArray8[0, 0, 0, 0, 0, 0, 0, 0] = "foo";

           // T[]::Get
            Assert("foo" == Ast.Lambda<Func<string>>(
                Ast.Call(Ast.Constant(strArray1), typeof(string[]).GetMethod("Get"), Ast.Constant(0))
            ).LightCompile()());

            // T[]::Set
            Ast.Lambda<Action>(
                Ast.Call(Ast.Constant(strArray1), typeof(string[]).GetMethod("Set"), Ast.Constant(0), Ast.Constant("bar"))
            ).LightCompile()();
            Assert(strArray1[0] == "bar");

            // T[,]::Get
            Assert("foo" == Ast.Lambda<Func<string>>(
                Ast.Call(Ast.Constant(strArray2), typeof(string[,]).GetMethod("Get"), Ast.Constant(0), Ast.Constant(0))
            ).LightCompile()());

            // T[,]::Set
            Ast.Lambda<Action>(
                Ast.Call(Ast.Constant(strArray2), typeof(string[,]).GetMethod("Set"), Ast.Constant(0), Ast.Constant(0), Ast.Constant("bar"))
            ).LightCompile()();
            Assert(strArray2[0, 0] == "bar");

            // T[,,]::Get
            Assert("foo" == Ast.Lambda<Func<string>>(
                Ast.Call(Ast.Constant(strArray3), typeof(string[, ,]).GetMethod("Get"), Ast.Constant(0), Ast.Constant(0), Ast.Constant(0))
            ).LightCompile()());

            // T[,,]::Set
            Ast.Lambda<Action>(
                Ast.Call(Ast.Constant(strArray3), typeof(string[, ,]).GetMethod("Set"), Ast.Constant(0), Ast.Constant(0), Ast.Constant(0), Ast.Constant("bar"))
            ).LightCompile()();
            Assert(strArray3[0, 0, 0] == "bar");

            // T[*]::Get
            Assert("foo" == Ast.Lambda<Func<string>>(
                Ast.Call(Ast.Constant(strArray8), typeof(string[, , , , , , ,]).GetMethod("Get"),
                        Ast.Constant(0), Ast.Constant(0), Ast.Constant(0), Ast.Constant(0),
                        Ast.Constant(0), Ast.Constant(0), Ast.Constant(0), Ast.Constant(0)
                )
            ).LightCompile()());

            // T[*]::Set
            Ast.Lambda<Action>(
                Ast.Call(Ast.Constant(strArray8), typeof(string[, , , , , , ,]).GetMethod("Set"),
                        Ast.Constant(0), Ast.Constant(0), Ast.Constant(0), Ast.Constant(0),
                        Ast.Constant(0), Ast.Constant(0), Ast.Constant(0), Ast.Constant(0),
                        Ast.Constant("bar")
                )
            ).LightCompile()();
            Assert(strArray8[0, 0, 0, 0, 0, 0, 0, 0] == "bar");
        }

        /// <summary>
        /// Variable shadowing.
        /// </summary>
        [Options(NoRuntime = true)]
        public void Interpreter6() {
            var i_var = Ast.Parameter(typeof(int), "i");

            var l1 = Ast.Lambda<Action>(
                Ast.Block(new[] { i_var },
                    Ast.Assign(i_var, Ast.Constant(1)),
                    TraceCall(i_var),

                    Ast.Block(new[] { i_var },
                        Ast.Assign(i_var, Ast.Constant(2)),
                        TraceCall(i_var)
                    ),

                    Ast.Block(new[] { i_var },
                        Ast.Assign(i_var, Ast.Constant(3)),
                        TraceCall(i_var)
                    ),

                    TraceCall(i_var)
                )
            );
            
            //XTraceTestLambda(l1);

            var e_var = Ast.Parameter(typeof(Exception), "e");
            var ex1 = new Exception("ex1");
            var ex2 = new Exception("ex2");

            // each catch block actually defines e variable again shadowing the block's e variable.
            // Thus assignments within the catch block don't affect block's e variable.
            var l2 = Ast.Lambda<Action>(
                Ast.Block(new[] { e_var },
                    Ast.TryCatch(Ast.Throw(Ast.Constant(ex1)), Ast.Catch(e_var, TraceCall(Ast.Property(e_var, "Message")))),
                    TraceCall(e_var),
                    Ast.TryCatch(Ast.Throw(Ast.Constant(ex2)), Ast.Catch(e_var, TraceCall(Ast.Property(e_var, "Message")))),
                    TraceCall(e_var)
                )
            );

            l2.Compile()();

            //XTraceTestLambda(l2);

            var l3 = Ast.Lambda<Action>(
                Ast.Block(new[] { i_var },
                    TraceCall(
                        Ast.Invoke(
                            Ast.Lambda<Func<int, int>>(Ast.Add(i_var, Ast.Constant(1)), new[] { i_var }),
                            Ast.Constant(1)
                        )
                    )
                )
            );

            l3.Compile()();

            //XTraceTestLambda(l2);

            // TODO: add tests for loop compiler
        }

        [Options(NoRuntime = true)]
        public void InterpreterNumeric1() {
            Assert(Expression.Lambda<Func<short>>(
                Expression.Add(Expression.Constant((short)1), Expression.Constant((short)2))
            ).LightCompile()() == 3);

            Assert(Expression.Lambda<Func<int>>(
                Expression.Add(Expression.Constant((int)1), Expression.Constant((int)2))
            ).LightCompile()() == 3);

            Assert(Expression.Lambda<Func<short>>(
                Expression.AddChecked(Expression.Constant((short)1), Expression.Constant((short)2))
            ).LightCompile()() == 3);

            Assert(Expression.Lambda<Func<bool>>(
                Expression.LessThan(Expression.Constant((byte)1), Expression.Constant((byte)2))
            ).LightCompile()() == true);

            Assert(Expression.Lambda<Func<bool>>(
                Expression.Equal(Expression.Constant(true), Expression.Constant(false))
            ).LightCompile()() == false);

            object obj1 = 1;
            object obj2 = 1;
            Assert(Expression.Lambda<Func<bool>>(
                Expression.Equal(Expression.Constant(obj1, typeof(object)), Expression.Constant(obj2, typeof(object)))
            ).LightCompile()() == false);

            Assert(Expression.Lambda<Func<bool>>(
                Expression.Equal(Expression.Constant(1), Expression.Constant(1))
            ).LightCompile()() == true);
        }

        public class ClassWithMethods2 {
            private readonly string Str = "<this>";

            public static void SF0() { TestValues.Add("0"); }
            public static void SF1(string a) { TestValues.Add(a); }
            public static void SF2(string a, string b) { TestValues.Add(a + b); }
            public static void SF3(string a, string b, string c) { TestValues.Add(a + b + c); }
            public static void SF4(string a, string b, string c, string d) { TestValues.Add(a + b + c + d); }
            public static void SF5(string a, string b, string c, string d, string e) { TestValues.Add(a + b + c + d + e); }
            public static string SG0() { TestValues.Add("0"); return "G0"; }
            public static string SG1(string a) { TestValues.Add(a); return "G1"; }
            public static string SG2(string a, string b) { TestValues.Add(a + b); return "G2"; }
            public static string SG3(string a, string b, string c) { TestValues.Add(a + b + c); return "G3"; }
            public static string SG4(string a, string b, string c, string d) { TestValues.Add(a + b + c + d); return "G4"; }
            public static string SG5(string a, string b, string c, string d, string e) { TestValues.Add(a + b + c + d + e); return "G5"; }

            public void F0() { TestValues.Add(Str + "0"); }
            public void F1(string a) { TestValues.Add(Str + a); }
            public void F2(string a, string b) { TestValues.Add(Str + a + b); }
            public void F3(string a, string b, string c) { TestValues.Add(Str + a + b + c); }
            public void F4(string a, string b, string c, string d) { TestValues.Add(Str + a + b + c + d); }
            public void F5(string a, string b, string c, string d, string e) { TestValues.Add(Str + a + b + c + d + e); }
            public string G0() { TestValues.Add(Str + "0"); return "G0"; }
            public string G1(string a) { TestValues.Add(Str + a); return "G1"; }
            public string G2(string a, string b) { TestValues.Add(Str + a + b); return "G2"; }
            public string G3(string a, string b, string c) { TestValues.Add(Str + a + b + c); return "G3"; }
            public string G4(string a, string b, string c, string d) { TestValues.Add(Str + a + b + c + d); return "G4"; }
            public string G5(string a, string b, string c, string d, string e) { TestValues.Add(Str + a + b + c + d + e); return "G5"; }
        }

        private static MethodInfo GM2(string name) {
            return typeof(ClassWithMethods2).GetMethod(name);
        }

        [ThreadStatic]
        private static List<string> TestValues = new List<string>();

        [Options(NoRuntime = true)]
        public void InterpreterMethodCalls1() {
            var sf = Expression.Lambda<Action>(Ast.Block(
                Ast.Call(null, GM2("SF0")),
                Ast.Call(null, GM2("SF1"), Ast.Constant("1")),
                Ast.Call(null, GM2("SF2"), Ast.Constant("1"), Ast.Constant("2")),
                Ast.Call(null, GM2("SF3"), Ast.Constant("1"), Ast.Constant("2"), Ast.Constant("3")),
                Ast.Call(null, GM2("SF4"), Ast.Constant("1"), Ast.Constant("2"), Ast.Constant("3"), Ast.Constant("4")),
                Ast.Call(null, GM2("SF5"), Ast.Constant("1"), Ast.Constant("2"), Ast.Constant("3"), Ast.Constant("4"), Ast.Constant("5"))
            ));

            var sg = Expression.Lambda<Func<string[]>>(Ast.NewArrayInit(typeof(string),
                Ast.Call(null, GM2("SG0")),
                Ast.Call(null, GM2("SG1"), Ast.Constant("1")),
                Ast.Call(null, GM2("SG2"), Ast.Constant("1"), Ast.Constant("2")),
                Ast.Call(null, GM2("SG3"), Ast.Constant("1"), Ast.Constant("2"), Ast.Constant("3")),
                Ast.Call(null, GM2("SG4"), Ast.Constant("1"), Ast.Constant("2"), Ast.Constant("3"), Ast.Constant("4")),
                Ast.Call(null, GM2("SG5"), Ast.Constant("1"), Ast.Constant("2"), Ast.Constant("3"), Ast.Constant("4"), Ast.Constant("5"))
            ));

            var i = Expression.Constant(new ClassWithMethods2());

            var f = Expression.Lambda<Action>(Ast.Block(
                Ast.Call(i, GM2("F0")),
                Ast.Call(i, GM2("F1"), Ast.Constant("1")),
                Ast.Call(i, GM2("F2"), Ast.Constant("1"), Ast.Constant("2")),
                Ast.Call(i, GM2("F3"), Ast.Constant("1"), Ast.Constant("2"), Ast.Constant("3")),
                Ast.Call(i, GM2("F4"), Ast.Constant("1"), Ast.Constant("2"), Ast.Constant("3"), Ast.Constant("4")),
                Ast.Call(i, GM2("F5"), Ast.Constant("1"), Ast.Constant("2"), Ast.Constant("3"), Ast.Constant("4"), Ast.Constant("5"))
            ));

            var g = Expression.Lambda<Func<string[]>>(Ast.NewArrayInit(typeof(string),
                Ast.Call(i, GM2("G0")),
                Ast.Call(i, GM2("G1"), Ast.Constant("1")),
                Ast.Call(i, GM2("G2"), Ast.Constant("1"), Ast.Constant("2")),
                Ast.Call(i, GM2("G3"), Ast.Constant("1"), Ast.Constant("2"), Ast.Constant("3")),
                Ast.Call(i, GM2("G4"), Ast.Constant("1"), Ast.Constant("2"), Ast.Constant("3"), Ast.Constant("4")),
                Ast.Call(i, GM2("G5"), Ast.Constant("1"), Ast.Constant("2"), Ast.Constant("3"), Ast.Constant("4"), Ast.Constant("5"))
            ));

            sf.Compile()();
            var c_sg_result = sg.Compile()();
            f.Compile()();
            var c_g_result = g.Compile()();
            string[] c_list = TestValues.ToArray();
            TestValues.Clear();

            sf.LightCompile()();
            var i_sg_result = sg.LightCompile()();
            f.LightCompile()();
            var i_g_result = g.LightCompile()();
            string[] i_list = TestValues.ToArray();
            TestValues.Clear();

            Assert(ArrayUtils.ValueEquals(c_sg_result, i_sg_result));
            Assert(ArrayUtils.ValueEquals(c_g_result, i_g_result));
            Assert(ArrayUtils.ValueEquals(c_list, i_list));
        }

        [Options(NoRuntime = true)]
        public void InterpreterLoops1() {
            ParameterExpression i_var = Ast.Parameter(typeof(int), "i");
            ParameterExpression s_var = Ast.Parameter(typeof(string), "s");
            ParameterExpression s2_var = Ast.Parameter(typeof(string), "s2");
            LabelTarget break2_label = Ast.Label();
            LabelTarget break1_label = Ast.Label();
            LabelTarget label1 = Ast.Label();

            var l = Expression.Lambda<Action>(
                Ast.Block(new[] { i_var },
                    Ast.Assign(i_var, Ast.Constant(0)),
                    Ast.Loop(
                        Ast.Block(new[] { s2_var },
                            Ast.Assign(s2_var, Ast.Constant("z")),
                            Ast.Loop(
                                Ast.Block(
                                    new[] { s_var },
                                    Ast.Assign(s_var, Ast.Constant("a")),
                                    Ast.IfThen(Ast.Equal(Expression.Assign(i_var, Ast.Add(i_var, Ast.Constant(1))), Ast.Constant(3)), Ast.Break(break2_label)),
                                    Ast.IfThen(Ast.Equal(i_var, Ast.Constant(5)), Ast.Break(label1)),
                                    TraceCall(s_var),
                                    TraceCall(s2_var)
                                ),
                                break2_label
                            ),
                            TraceCall(Ast.Constant("d"))
                        )
                    ),
                    TraceCall(Ast.Constant("b")),
                    Ast.Label(label1),
                    TraceCall(Ast.Constant("c"))
                )
            );

            TraceTestLambda(l);
        }

        public static void MethodWithRef(ref int foo) {
            foo++;
        }

        [Options(NoRuntime = true)]
        public void InterpreterLoops2() {
            ParameterExpression i_var = Ast.Parameter(typeof(int), "i");
            ParameterExpression k_var = Ast.Parameter(typeof(int), "k");
            ParameterExpression j_var = Ast.Parameter(typeof(int), "j");
            ParameterExpression o_var = Ast.Parameter(typeof(object), "o");
            ParameterExpression d_var = Ast.Parameter(typeof(Func<int>), "d");
            LabelTarget label1 = Ast.Label();
            LabelTarget label2 = Ast.Label();

            var innerLambda = Ast.Lambda<Func<int>>(
                Ast.Block(new[] { k_var }, 
                    Ast.Assign(k_var, Ast.Constant(0)),
                    Ast.Loop(
                        Ast.Block(
                            Ast.IfThen(Ast.Equal(Ast.Assign(k_var, Ast.Add(k_var, Ast.Constant(1))), Ast.Constant(5)), Ast.Goto(label2)),
                            Ast.Assign(j_var, Ast.Add(j_var, Ast.Constant(1)))
                        )
                    ),
                    Ast.Label(label2),
                    j_var                    
                )
            );

            var l = Expression.Lambda<Action>(
                Ast.Block(new[] { i_var, j_var, d_var },
                    Ast.Assign(i_var, Ast.Constant(0)),
                    Ast.Assign(j_var, Ast.Constant(-1)),

                    // close over j:
                    Ast.Assign(d_var, innerLambda),
                            
                    Ast.Loop(
                        Ast.Block(
                            Ast.IfThen(Ast.Equal(Ast.Assign(i_var, Ast.Add(i_var, Ast.Constant(1))), Ast.Constant(5)), Ast.Return(label1)), 
                            
                            // this assignment must be to the closure Value not to the local j_var:
                            Ast.Assign(j_var, i_var),

                            TraceCall(Ast.Convert(Ast.Invoke(d_var), typeof(Object)))
                        )
                    ),
                    Ast.Label(label1)
                )
            );

            TraceTestLambda(l);            
        }

        /// <summary>
        /// Gotos with values jumping from the loop.
        /// </summary>
        [Options(NoRuntime = true)]
        public void InterpreterLoops3() {
            ParameterExpression i_var = Ast.Parameter(typeof(int), "i");
            LabelTarget label1 = Ast.Label(typeof(int));

            var l = Ast.Lambda<Action>(
                Ast.Block(new[] { i_var },
                    Ast.Assign(i_var,     
                        Ast.Block(
                            Ast.Loop( 
                                Ast.Goto(label1, Ast.Constant(123))
                            ),
                            Ast.Label(label1, Ast.Constant(5))
                        )
                    ),
                    TraceCall(i_var)
                )
            );

            TraceTestLambda(l);
        }
        
        [Options(NoRuntime = true)]
        public void InterpreterLoops4() {
            ParameterExpression i_var = Ast.Parameter(typeof(int), "i");
            ParameterExpression j_var = Ast.Parameter(typeof(int), "j");
            ParameterExpression k_var = Ast.Parameter(typeof(int), "k");
            LabelTarget label1 = Ast.Label();

            var l = Ast.Lambda<Action>(
                Ast.Block(new[] { i_var },
                    Ast.Loop(
                        Ast.Block(new[] { j_var },
                            Ast.IfThen(Ast.Equal(Ast.Assign(i_var, Ast.Add(i_var, Ast.Constant(1))), Ast.Constant(5)), Ast.Break(label1)),

                            Ast.Assign(
                                j_var,
                                Ast.Invoke(
                                    Ast.Lambda<Func<int, int>>(Ast.Add(k_var, i_var), new[] { k_var }),
                                    Ast.Constant(1)
                                )
                            ),

                            TraceCall(j_var)
                        ),
                        label1
                    )
                )
            );

            TraceTestLambda(l);
        }

        public struct S1 {
            public int X;
            public int Y;

            public S1(int x, int y) {
                X = x;
                Y = y;
            }

            public void SetX() {
                X = 1;
            }
        }

        [Options(NoRuntime = true)]
        public void InterpreterValueTypes1() {
            var arg = Ast.Parameter(typeof(object));
            var l = Ast.Lambda<Action<object>>(
                Ast.Call(
                    Ast.MakeUnary(ExpressionType.Unbox, arg, typeof(S1)),
                    typeof(S1).GetMethod("SetX")
                ),
                new[] { arg } 
            );

            object s = new S1(3, 4);

            var f = l.LightCompile();
            f(s);

            S1 unboxed = (S1)s;
            Assert(unboxed.X == 1);
            Assert(unboxed.Y == 4);
        }
    }
}
