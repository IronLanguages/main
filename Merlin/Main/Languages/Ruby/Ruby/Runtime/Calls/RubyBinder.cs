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

        #endregion
    }
}
