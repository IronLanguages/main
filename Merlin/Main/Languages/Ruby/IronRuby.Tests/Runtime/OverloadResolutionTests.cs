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
using System.Text;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime.Calls;
using System.Dynamic;

using MSA = System.Linq.Expressions;
using Ast = System.Linq.Expressions.Expression;
using IronRuby.Builtins;

namespace IronRuby.Tests {
    public partial class Tests {

        #region Block

        public void OverloadResolution_Block() {
            var t = GetType();

            var gse = new RubyGlobalScope(Context, new Scope(), new object(), true);
            var scope = new RubyTopLevelScope(gse, null, new SymbolDictionary());
            var proc = new Proc(ProcKind.Proc, null, scope, new BlockDispatcher0((x, y) => null, BlockSignatureAttributes.None));

            var scopeArg = new DynamicMetaObject(Ast.Constant(proc.LocalScope), BindingRestrictions.Empty, proc.LocalScope);
            var contextArg = new DynamicMetaObject(Ast.Constant(Context), BindingRestrictions.Empty, Context);
            var instanceInt = new DynamicMetaObject(Ast.Constant(1), BindingRestrictions.Empty, 1);
            var str = "foo";
            var instanceStr = new DynamicMetaObject(Ast.Constant(str), BindingRestrictions.Empty, str);
            var procArg = new DynamicMetaObject(Ast.Constant(proc), BindingRestrictions.Empty, proc);
            var nullArg = new DynamicMetaObject(Ast.Constant(Ast.Constant(null)), BindingRestrictions.Empty, null);

            var arguments = new[] {
                // 1.times
                new CallArguments(scopeArg, instanceInt, new DynamicMetaObject[0], RubyCallSignature.WithScope(0)),
                // 1.times &nil             
                new CallArguments(scopeArg, instanceInt, new[] {  nullArg }, RubyCallSignature.WithScopeAndBlock(0)),
                // 1.times &p                            
                new CallArguments(contextArg, instanceInt, new[] {  procArg }, RubyCallSignature.WithBlock(0)),
                // obj.times &p                          
                new CallArguments(contextArg, instanceStr, new[] {  procArg }, RubyCallSignature.WithBlock(0)),
            };

            var results = new[] {
                "Times2",
                "Times1",
                "Times3",
                "Times4",
            };

            for (int i = 0; i < arguments.Length; i++) {
                var bindingTarget = RubyMethodGroupInfo.ResolveOverload("times", new[] {
                    t.GetMethod("Times1"),
                    t.GetMethod("Times2"),
                    t.GetMethod("Times3"),
                    t.GetMethod("Times4"),
                }, arguments[i], true, false);

                Assert(bindingTarget.Success);
                Assert(bindingTarget.Method.Name == results[i]);
            }
        }

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

        #endregion

    }
}
