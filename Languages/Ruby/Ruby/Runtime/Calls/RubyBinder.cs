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
using System.Dynamic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Utils;
using System.Threading;
using System.Diagnostics;
using IronRuby.Runtime.Conversions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace IronRuby.Runtime.Calls {
    public sealed class RubyBinder : DefaultBinder {
        private readonly RubyContext/*!*/ _context;

        internal RubyBinder(RubyContext/*!*/ context){
            _context = context;
        }

        public override string GetTypeName(Type/*!*/ t) {
            return _context.GetTypeName(t, true);
        }

        public override string GetObjectTypeName(object arg) {
            return _context.GetClassDisplayName(arg);
        }

        public override bool PrivateBinding {
            get {
                return _context.DomainManager.Configuration.PrivateBinding;
            }
        }

        #region Conversions

        public override Expression ConvertExpression(Expression expr, Type toType, ConversionResultKind kind, OverloadResolverFactory context) {
            throw new InvalidOperationException("OBSOLETE");
        }

        public override bool CanConvertFrom(Type/*!*/ fromType, Type/*!*/ toType, bool toNotNullable, NarrowingLevel level) {
            return Converter.CanConvertFrom(null, fromType, toType, toNotNullable, level, false, false).IsConvertible;
        }

        public override Candidate PreferConvert(Type t1, Type t2) {
            return Converter.PreferConvert(t1, t2);
        }        

        #endregion

        #region MetaObjects

        // negative start reserves as many slots at the beginning of the new array:
        internal static object/*!*/[]/*!*/ ToValues(DynamicMetaObject/*!*/[]/*!*/ args, int start) {
            var result = new object[args.Length - start];
            for (int i = Math.Max(0, -start); i < result.Length; i++) {
                result[i] = args[start + i].Value;
            }
            return result;
        }

#if DEBUG && !SILVERLIGHT
        // ExpressionWriter might call ToString on a live object that might dynamically invoke a method.
        // We need to prevent recursion in such case.
        [ThreadStatic]
        internal static bool _DumpingExpression;

        private static int _precompiledRuleCounter;
        private static int _ruleCounter;
#if !CLR2
        private static MethodInfo _dumpViewMethod; 
#endif
#endif

        [Conditional("DEBUG")]
        internal static void DumpPrecompiledRule(CallSiteBinder/*!*/ binder, MemberDispatcher/*!*/ dispatcher) {
#if DEBUG && !SILVERLIGHT
            if (RubyOptions.ShowRules) {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Precompiled Rule #{0}: {1}", Interlocked.Increment(ref _precompiledRuleCounter), binder);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(dispatcher);
                Console.ForegroundColor = oldColor;
            }
#endif
        }

        [Conditional("DEBUG")]
        internal static void DumpRule(CallSiteBinder/*!*/ binder, BindingRestrictions/*!*/ restrictions, Expression/*!*/ expr) {
#if DEBUG && !SILVERLIGHT
            if (RubyOptions.ShowRules) {
                var oldColor = Console.ForegroundColor;
                try {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Rule #{0}: {1}", Interlocked.Increment(ref _ruleCounter), binder);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    if (!_DumpingExpression) {
                        var d = (restrictions != BindingRestrictions.Empty) ? Expression.IfThen(restrictions.ToExpression(), expr) : expr;
                        _DumpingExpression = true;
#if CLR2
                        d.DumpExpression(Console.Out);
#else
                        try {
                            if (_dumpViewMethod == null) {
                                _dumpViewMethod = typeof(Expression).GetMethod("get_DebugView", BindingFlags.NonPublic | BindingFlags.Instance);
                            }
                            Console.WriteLine(_dumpViewMethod.Invoke(d, ArrayUtils.EmptyObjects));
                            Console.WriteLine();
                        } catch {
                            // nop
                        }
#endif
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
