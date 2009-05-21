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

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;
using Ast = System.Linq.Expressions.Expression;

namespace IronRuby.Tests {
    public partial class Tests {

        private static DynamicMetaObject/*!*/ MO(object value) {
            return new DynamicMetaObject(Ast.Constant(value), BindingRestrictions.Empty, value);
        }

        private static MethodInfo/*!*/[]/*!*/ GetStaticMethods(Type/*!*/ type, string/*!*/ name) {
            return Array.ConvertAll(type.GetMember(name, BindingFlags.Public | BindingFlags.Static), (mi) => (MethodInfo)mi);
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
            var proc = new Proc(ProcKind.Proc, null, scope, new BlockDispatcher0((x, y) => null, BlockSignatureAttributes.None));

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
                RubyOverloadResolver parameterBinder;
                var bindingTarget = RubyMethodGroupInfo.ResolveOverload(
                    metaBuilder, arguments[i], "times", GetStaticMethods(typeof(OverloadsWithBlock), "Times*"), SelfCallConvention.SelfIsParameter,
                    false, out parameterBinder
                );

                Assert(bindingTarget.Success);
                Assert(bindingTarget.Method.Name == results[i]);
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

            internal static List<MethodInfo>/*!*/ GetMethods() {
                var methods = new List<MethodInfo>();
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
            public static int F(params object[] p) {
                return 1;
            }
            
            public static int F(object p) {
                return 1;
            }

            public static int F(int p) {
                return 2;
            }

            public static int F(string p) {
                return 3;
            }
        }

        public void AmbiguousMatch() {
            Context.SetGlobalConstant("C", Context.GetClass(typeof(AmbiguousOverloads)));
            AssertOutput(() => CompilerTest(@"
[1, nil, 'foo'].each do |x| 
  puts C.f(x) rescue p $!.class
end 
"), @"
2
System::Reflection::AmbiguousMatchException
3
");
        }

        #endregion
    }
}
