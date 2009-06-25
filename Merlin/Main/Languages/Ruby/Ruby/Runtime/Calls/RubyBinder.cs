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
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Utils;
using System.Threading;
using System.Diagnostics;

namespace IronRuby.Runtime.Calls {
    public sealed class RubyBinder : DefaultBinder {
        private readonly RubyContext/*!*/ _context;

        internal RubyBinder(RubyContext/*!*/ context)
            : base(context.DomainManager) {
            _context = context;
        }

        public override string GetTypeName(Type/*!*/ t) {
            return _context.GetTypeName(t, true);
        }

        public override string GetObjectTypeName(object arg) {
            return _context.GetClassDisplayName(arg);
        }

        #region Conversions

        public override Expression ConvertExpression(Expression expr, Type toType, ConversionResultKind kind, Expression context) {
            throw new InvalidOperationException("OBSOLETE");
        }

        public override bool CanConvertFrom(Type/*!*/ fromType, Type/*!*/ toType, bool toNotNullable, NarrowingLevel level) {
            return Converter.CanConvertFrom(fromType, toType, level, true);
        }

        public override Candidate PreferConvert(Type t1, Type t2) {
            return Converter.PreferConvert(t1, t2);
        }        

        #endregion

        #region MetaObjects

        // negative start reserves as many slots at the beginning of the new array:
        internal static Expression/*!*/[]/*!*/ ToExpressions(DynamicMetaObject/*!*/[]/*!*/ args, int start) {
            var result = new Expression[args.Length - start];
            for (int i = Math.Max(0, -start); i < result.Length; i++) {
                result[i] = args[start + i].Expression;
            }
            return result;
        }

        // negative start reserves as many slots at the beginning of the new array:
        internal static object/*!*/[]/*!*/ ToValues(DynamicMetaObject/*!*/[]/*!*/ args, int start) {
            var result = new object[args.Length - start];
            for (int i = Math.Max(0, -start); i < result.Length; i++) {
                result[i] = args[start + i].Value;
            }
            return result;
        }

#if DEBUG && !SILVERLIGHT && !SYSTEM_CORE
        // ExpressionWriter might call ToString on a live object that might dynamically invoke a method.
        // We need to prevent recursion in such case.
        [ThreadStatic]
        internal static bool _DumpingExpression;

        private static int _precompiledRuleCounter;
        private static int _ruleCounter;
#endif

        [Conditional("DEBUG")]
        internal static void DumpPrecompiledRule(DynamicMetaObjectBinder/*!*/ action, MethodDispatcher/*!*/ dispatcher) {
#if DEBUG && !SILVERLIGHT && !SYSTEM_CORE
            if (RubyOptions.ShowRules) {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Precompiled Rule #{0}: {1}", Interlocked.Increment(ref _precompiledRuleCounter), action);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(dispatcher);
                Console.ForegroundColor = oldColor;
            }
#endif
        }

        [Conditional("DEBUG")]
        internal static void DumpRule(DynamicMetaObjectBinder/*!*/ action, BindingRestrictions/*!*/ restrictions, Expression/*!*/ expr) {
#if DEBUG && !SILVERLIGHT && !SYSTEM_CORE
            if (RubyOptions.ShowRules) {
                var oldColor = Console.ForegroundColor;
                try {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Rule #{0}: {1}", Interlocked.Increment(ref _ruleCounter), action);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    if (!_DumpingExpression) {
                        var d = (restrictions != BindingRestrictions.Empty) ? Expression.IfThen(restrictions.ToExpression(), expr) : expr;
                        _DumpingExpression = true;
                        d.DumpExpression(Console.Out);
                        Console.WriteLine();
                    }
                } finally {
                    _DumpingExpression = false;
                    Console.ForegroundColor = oldColor;
                }
            }
#endif
        }

        #endregion
    }
}
