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
using System.Dynamic;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Tests {
    using Ast = Expression;
    using BlockCallTarget0 = Func<BlockParam, object, object>;

    public partial class Tests {

        private static DynamicMetaObject/*!*/ MO(object value) {
            return new DynamicMetaObject(Ast.Constant(value), BindingRestrictions.Empty, value);
        }

        private static OverloadInfo/*!*/[]/*!*/ GetStaticMethods(Type/*!*/ type, string/*!*/ name) {
            return Array.ConvertAll(
                type.GetMember(name, BindingFlags.Public | BindingFlags.Static),
                (mi) => new ReflectionOverloadInfo((MethodBase)mi)
            );
        }

        private static OverloadInfo/*!*/[]/*!*/ GetInstanceMethods(Type/*!*/ type, string/*!*/ name) {
            return Array.ConvertAll(
                type.GetMember(name, BindingFlags.Public | BindingFlags.Instance), 
                (mi) => new ReflectionOverloadInfo((MethodBase)mi)
            );
        }

        #region Block

        public class OverloadsWithBlock {
            public static object Times1(BlockParam block, int self) {
                return 1;
            }

            public static object Times2(int self) {
                return 2;
            }

            public static object Times3([NotNull]BlockParam/*!*/ block, int self) {
                return 3;
            }

            public static object Times4(RubyContext/*!*/ context, BlockParam block, object self) {
                return 4;
            }
        }

        public void OverloadResolution_Block1() {
            var scope = Context.EmptyScope;
            var proc = new Proc(ProcKind.Proc, null, scope, new BlockDispatcher0(BlockSignatureAttributes.None, null, 0).
                SetMethod(new BlockCallTarget0((x, y) => null)));

            var arguments = new[] {
                // 1.times
                new CallArguments(Context, MO(scope), new[] { MO(1) }, RubyCallSignature.WithScope(0)),
                // 1.times &nil             
                new CallArguments(Context, MO(scope), new[] {  MO(1), MO(null) }, RubyCallSignature.WithScopeAndBlock(0)),
                // 1.times &p                            
                new CallArguments(Context, MO(1), new[] {  MO(proc) }, RubyCallSignature.WithBlock(0)),
                // obj.times &p                          
                new CallArguments(Context, MO("foo"), new[] {  MO(proc) }, RubyCallSignature.WithBlock(0)),
            };

            var results = new[] {
                "Times2",
                "Times1",
                "Times3",
                "Times4",
            };

            var metaBuilder = new MetaObjectBuilder(null);
            for (int i = 0; i < arguments.Length; i++) {
                RubyOverloadResolver resolver;
                var bindingTarget = RubyMethodGroupInfo.ResolveOverload(
                    metaBuilder, arguments[i], "times", GetStaticMethods(typeof(OverloadsWithBlock), "Times*"), SelfCallConvention.SelfIsParameter,
                    false, out resolver
                );

                Assert(bindingTarget.Success);
                Assert(bindingTarget.Overload.Name == results[i]);
            }
        }

        #endregion

        #region Misc

        public class Overloads1 {
            public class X {
            }

            public enum E {
                A = 1, B = 2
            }

            public void F1(int a) { }
            public void F2(BigInteger a) { }
            public void F3(double a) { }

            public void G1([DefaultProtocol]int a) { }
            public void G2(BigInteger a) { }
            public void G3(double a) { }

            public void I1(int a) { }
            public void I2([NotNull]MutableString a) { }
            public void I3([DefaultProtocol]Union<int, MutableString> a) { }

            public void J1(int a) { }
            public void J2([NotNull]BigInteger a) { }
            public void J3([DefaultProtocol]IntegerValue a) { }

            // This is rather useless overload combination:
            // If DP parameter behaved exaclty like object it would be ambiguous.
            // Since we try implicit conversions first, all types but numerics assignable to integer prefer K1.
            // Therefore [DP] is irrelevant here.
            public void K1(object a) { }
            public void K2([DefaultProtocol]int a) { }

            // level 0: no overload applicable
            // level 1: {L1, L3} applicable, MutableString <-/-> RubySymbol
            public void L1([NotNull]RubySymbol a, [DefaultProtocol, NotNull]string b) { }
            public void L2([NotNull]string a, [DefaultProtocol, NotNull]string b) { }
            public void L3([DefaultProtocol]MutableString a, [DefaultProtocol, NotNull]string b) { }

            public void M1(int a) { }
            public void M2(E e) { }

            public void N1(string a) { }
            public void N2(char a) { }
        }

        public void OverloadResolution_Numeric1() {
            var metaBuilder = new MetaObjectBuilder(null);
            Context.ObjectClass.SetConstant("X", Context.GetClass(typeof(Overloads1.X)));

            object c = Engine.Execute(@"class C < X; new; end");
            var sym = Context.CreateAsciiSymbol("x");
            var ms = MutableString.CreateAscii("x");

            var cases = new[] {
                // F
                new { Args = new[] { MO(1) }, Overloads = "F*", Result = "F1" },
                new { Args = new[] { MO((byte)1) }, Overloads = "F*", Result = "F1" },
                new { Args = new[] { MO(1L) }, Overloads = "F*", Result = "F2" },
                new { Args = new[] { MO(1.2F) }, Overloads = "F*", Result = "F3" },

                // G
                new { Args = new[] { MO(1) }, Overloads = "G*", Result = "G1" },
                new { Args = new[] { MO((byte)1) }, Overloads = "G*", Result = "G1" },
                new { Args = new[] { MO(1L) }, Overloads = "G*", Result = "G2" },
                new { Args = new[] { MO(1.2F) }, Overloads = "G*", Result = "G3" },
                new { Args = new[] { MO(c) }, Overloads = "G*", Result = "G1" },

                // I
                new { Args = new[] { MO(c) }, Overloads = "I*", Result = "I3" },

                // J
                new { Args = new[] { MO(1) }, Overloads = "J*", Result = "J1" },
                new { Args = new[] { MO((BigInteger)1000) }, Overloads = "J*", Result = "J2" },
                new { Args = new[] { MO((byte)12) }, Overloads = "J*", Result = "J1" },
                new { Args = new[] { MO(c) }, Overloads = "J*", Result = "J3" },
                new { Args = new[] { MO(1.0) }, Overloads = "J*", Result = "J3" },
      
                // K
                new { Args = new[] { MO(1) }, Overloads = "K*", Result = "K2" },
                new { Args = new[] { MO(c) }, Overloads = "K*", Result = "K1" },
                new { Args = new[] { MO("x") }, Overloads = "K*", Result = "K1" },

                // L
                new { Args = new[] { MO(sym), MO(sym) }, Overloads = "L*", Result = "L1" },
                new { Args = new[] { MO("x"), MO(sym) }, Overloads = "L*", Result = "L2" },
                new { Args = new[] { MO(ms), MO(sym) }, Overloads = "L*", Result = "L3" },
                new { Args = new[] { MO(null), MO(sym) }, Overloads = "L*", Result = "L3" },
                new { Args = new[] { MO(c), MO(sym) }, Overloads = "L*", Result = "L3" },

                // M
                new { Args = new[] { MO(1) }, Overloads = "M*", Result = "M1" },
                new { Args = new[] { MO(Overloads1.E.A) }, Overloads = "M*", Result = "M2" },

                // N
                new { Args = new[] { MO(MutableString.CreateAscii("x")) }, Overloads = "N*", Result = "N1" },
            };

            for (int i = 0; i < cases.Length; i++) {
                var args = new CallArguments(Context, MO(new Overloads1()), cases[i].Args, RubyCallSignature.Simple(cases[i].Args.Length));
                var resolver = new RubyOverloadResolver(metaBuilder, args, SelfCallConvention.SelfIsInstance, false);
                var overloads = GetInstanceMethods(typeof(Overloads1), cases[i].Overloads);
                var result = resolver.ResolveOverload(i.ToString(), overloads, NarrowingLevel.None, NarrowingLevel.All);

                Assert(result.Success && result.Overload.Name == cases[i].Result);
            }
        }

        #endregion

        #region Param Arrays

        public class MethodsWithParamArrays {
            public static KeyValuePair<int, Array> F1(int p1, params int[] ps) {
                return new KeyValuePair<int, Array>(2, new object[] { p1, ps });
            }

            public static KeyValuePair<int, Array> F2(int p1) {
                return new KeyValuePair<int, Array>(2, new object[] { p1 });
            }

            public static KeyValuePair<int, Array> F3(int p1, int p2) {
                return new KeyValuePair<int, Array>(3, new object[] { p1, p2 });
            }

            public static KeyValuePair<int, Array> F4(double p1, int p2, params int[] ps) {
                return new KeyValuePair<int, Array>(4, new object[] { p1, p2, ps });
            }

            public static KeyValuePair<int, Array> F5(bool p1, int p2, int p3, params int[] ps) {
                return new KeyValuePair<int, Array>(5, new object[] { p1, p2, p3, ps });
            }

            internal static List<OverloadInfo>/*!*/ GetMethods() {
                var methods = new List<OverloadInfo>();
                methods.AddRange(GetStaticMethods(typeof(MethodsWithParamArrays), "F*"));
                methods.Add(CreateParamsArrayMethod("F0", new[] { typeof(int), typeof(int[]), typeof(string), typeof(int) }, 1, 0));
                return methods;
            }
        }

        public void OverloadResolution_ParamArrays1() {
        }

        #endregion

        #region Failures

        public class AmbiguousOverloads {
            public static int F(object[] p) {
                return 1;
            }
            
            public static int F(object p) {
                return 1;
            }

            public static int F(int p) {
                return 2;
            }

            public static int F(MutableString p) {
                return 3;
            }
        }

        public void AmbiguousMatch1() {
            Runtime.Globals.SetVariable("C", Context.GetClass(typeof(AmbiguousOverloads)));
            TestOutput(@"
[1, nil, 'foo'].each do |x| 
  puts C.f(x) rescue p $!.class
end 
", @"
2
System::Reflection::AmbiguousMatchException
3
");
        }

        #endregion
    }
}
