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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using System.Runtime.CompilerServices;
using System.Threading;

namespace IronRuby.Tests {
    using Ast = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public partial class Tests {
        [Options(NoRuntime = true)]
        public void Interpreter1() {
            var m_AddValue = new Action<StrongBox<int>, int>(Interpreter1_AddValue).Method;
            var m_ThrowNSE = new Action(Interpreter1_ThrowNSE).Method;
            var m_f = new Func<int, int, int, int, int>(Interpreter1_f).Method;
            var m_g = new Func<int, int, int, int>(Interpreter1_g).Method;

            var value = new StrongBox<int>();
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
            l1 = Ast.Lambda<Func<int>>(
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
            rc = l1.Compile()();
            ri = l1.LightCompile()();

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
            var l2 = Ast.Lambda<Func<int>>(
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

            rc = l2.Compile()();
            ri = l2.LightCompile()();
            Assert(rc == ri);


            // goto carrying a value needs to pop the value from the stack before executing finally
            // clauses to prevent stack overflow (the finally clause doesn't expect the value on the stack):
            var label3 = Ast.Label(typeof(int));
            var l3 = Ast.Lambda<Func<int>>(
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

            rc = l3.Compile()();
            ri = l3.LightCompile()();
            Assert(rc == ri);

            
            // goto needs to pop unused values from the stack before it executes finally:
            var label4 = Ast.Label(typeof(int));
            var l4 = Ast.Lambda<Func<int>>(
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

            rc = l4.Compile()();
            ri = l4.LightCompile()();
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
    }
}
