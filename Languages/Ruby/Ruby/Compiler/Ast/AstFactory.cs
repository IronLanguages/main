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
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Conversions;

using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstExpressions = ReadOnlyCollectionBuilder<MSA.Expression>;

    public static class AstFactory {

        internal static readonly MSA.Expression[] EmptyExpressions = new MSA.Expression[0];
        internal static readonly MSA.ParameterExpression[] EmptyParameters = new MSA.ParameterExpression[0];
        internal static readonly MSA.Expression NullOfMutableString = Ast.Constant(null, typeof(MutableString));
        internal static readonly MSA.Expression NullOfProc = Ast.Constant(null, typeof(Proc));
        internal static readonly MSA.Expression True = Ast.Constant(ScriptingRuntimeHelpers.True);
        internal static readonly MSA.Expression False = Ast.Constant(ScriptingRuntimeHelpers.False);
        internal static readonly MSA.Expression BlockReturnReasonBreak = AstUtils.Constant(BlockReturnReason.Break);

        public static MSA.Expression/*!*/ Infinite(MSA.LabelTarget @break, MSA.LabelTarget @continue, params MSA.Expression[]/*!*/ body) {
            return AstUtils.Infinite(Ast.Block(body), @break, @continue);
        }

        public static MSA.Expression/*!*/ IsTrue(MSA.Expression/*!*/ expression) {
            if (expression.Type == typeof(bool)) {
                return expression;
            } else {
                return Methods.IsTrue.OpCall(AstUtils.Box(expression));
            }
        }

        public static MSA.Expression/*!*/ IsFalse(MSA.Expression/*!*/ expression) {
            if (expression.Type == typeof(bool)) {
                return Ast.Not(expression);
            } else {
                return Methods.IsFalse.OpCall(AstUtils.Box(expression));
            }
        }

        public static MSA.Expression/*!*/ Logical(MSA.Expression/*!*/ left, MSA.Expression/*!*/ right, bool isConjunction) {
            if (isConjunction) {
                return Ast.AndAlso(left, right);
            } else {
                return Ast.OrElse(left, right);
            }
        }

        internal static TryStatementBuilder/*!*/ FinallyIf(this TryStatementBuilder/*!*/ builder, bool ifdef, params MSA.Expression[]/*!*/ body) {
            return ifdef ? builder.Finally(body) : builder;
        }

        internal static TryStatementBuilder/*!*/ FilterIf(this TryStatementBuilder/*!*/ builder, bool ifdef,
            MSA.ParameterExpression/*!*/ holder, MSA.Expression/*!*/ condition, params MSA.Expression[]/*!*/ body) {
            return ifdef ? builder.Filter(holder, condition, body) : builder;
        }

        public static MSA.Expression/*!*/ Condition(MSA.Expression/*!*/ test, MSA.Expression/*!*/ ifTrue, MSA.Expression/*!*/ ifFalse) {
            Assert.NotNull(test, ifTrue, ifFalse);
            Debug.Assert(test.Type == typeof(bool));

            if (ifTrue.Type != ifFalse.Type) {
                if (ifTrue.Type.IsAssignableFrom(ifFalse.Type)) {
                    ifFalse = Ast.Convert(ifFalse, ifTrue.Type);
                } else if (ifFalse.Type.IsAssignableFrom(ifTrue.Type)) {
                    ifTrue = Ast.Convert(ifTrue, ifFalse.Type);
                } else {
                    ifTrue = AstUtils.Box(ifTrue);
                    ifFalse = AstUtils.Box(ifFalse);
                }
            }

            return Ast.Condition(test, ifTrue, ifFalse);
        }

        internal static MSA.Expression/*!*/ CallDelegate(Delegate/*!*/ method, MSA.Expression[]/*!*/ arguments) {
            // We prefer to peek inside the delegate and call the target method directly. However, we need to
            // exclude DynamicMethods since Delegate.Method returns a dummy MethodInfo, and we cannot emit a call to it.
            if (method.Method.DeclaringType == null || !method.Method.DeclaringType.IsPublic || !method.Method.IsPublic) {
                // do not inline:
                return Ast.Call(AstUtils.Constant(method), method.GetType().GetMethod("Invoke"), arguments);
            } 
            
            if (method.Target != null) {
                // inline a closed static delegate:
                if (method.Method.IsStatic) {
                    return Ast.Call(null, method.Method, ArrayUtils.Insert(AstUtils.Constant(method.Target), arguments));
                } 

                // inline a closed instance delegate:
                return Ast.Call(AstUtils.Constant(method.Target), method.Method, arguments);
            }

            // inline an open static delegate:
            if (method.Method.IsStatic) {
                return Ast.Call(null, method.Method, arguments);
            } 
         
            // inline an open instance delegate:
            return Ast.Call(arguments[0], method.Method, ArrayUtils.RemoveFirst(arguments));
        }

        internal static MSA.Expression/*!*/ YieldExpression(
            RubyContext/*!*/ context,
            ICollection<MSA.Expression>/*!*/ arguments, 
            MSA.Expression splattedArgument,
            MSA.Expression rhsArgument,
            MSA.Expression blockArgument,
            MSA.Expression/*!*/ bfcVariable,
            MSA.Expression/*!*/ selfArgument) {

            Assert.NotNull(arguments, bfcVariable, selfArgument);

            bool hasArgumentArray;
            var opMethod = Methods.Yield(arguments.Count, splattedArgument != null, rhsArgument != null, out hasArgumentArray);

            var args = new AstExpressions();

            foreach (var arg in arguments) {
                args.Add(AstUtils.Box(arg));
            }

            if (hasArgumentArray) {
                args = new AstExpressions { Ast.NewArrayInit(typeof(object), args) };
            }

            if (splattedArgument != null) {
                args.Add(AstUtils.LightDynamic(ExplicitSplatAction.Make(context), typeof(IList), splattedArgument));
            }

            if (rhsArgument != null) {
                args.Add(AstUtils.Box(rhsArgument));
            }

            args.Add(blockArgument != null ? AstUtils.Convert(blockArgument, typeof(Proc)) : AstFactory.NullOfProc);

            args.Add(AstUtils.Box(selfArgument));
            args.Add(bfcVariable);

            return Ast.Call(opMethod, args);
        }
    }
}
