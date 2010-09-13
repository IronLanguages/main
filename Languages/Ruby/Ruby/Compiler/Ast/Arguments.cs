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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using IronRuby.Runtime.Conversions;
using AstUtils = Microsoft.Scripting.Ast.Utils;
	
namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstExpressions = ReadOnlyCollectionBuilder<MSA.Expression>;
    using ExpressionsBuilder = Microsoft.Scripting.Ast.ExpressionCollectionBuilder<MSA.Expression>;

    // TODO: remove this class
    public class Arguments {
        internal static readonly Arguments Empty = new Arguments();

        private readonly Expression/*!*/[]/*!*/ _expressions;
        
        public bool IsEmpty {
            get { return _expressions.Length == 0; }
        }

        public Expression/*!*/[]/*!*/ Expressions { get { return _expressions; } }
        
        public Arguments() {
            _expressions = Expression.EmptyArray;
        }

        public Arguments(Expression/*!*/ arg) {
            ContractUtils.RequiresNotNull(arg, "arg");
            _expressions = new Expression[] { arg };
        }

        public Arguments(Expression/*!*/[] expressions) {
            if (expressions != null) {
                Assert.NotNullItems(expressions);
            }

            _expressions = expressions ?? Expression.EmptyArray;
        }

        #region Transform to Call/Yield Expression

        internal void TransformToCall(AstGenerator/*!*/ gen, CallSiteBuilder/*!*/ siteBuilder) {
            siteBuilder.SplattedArgument = TransformToCallInternal(gen, siteBuilder);
        }

        internal MSA.Expression/*!*/ TransformToYield(AstGenerator/*!*/ gen, MSA.Expression/*!*/ bfcVariable, MSA.Expression/*!*/ selfExpression) {
            var args = new List<MSA.Expression>();
            var splatted = TransformToCallInternal(gen, args);
            return AstFactory.YieldExpression(gen.Context, args, splatted, null, null, bfcVariable, selfExpression);
        }

        /// <summary>
        /// Adds arguments to the given collection (result) and returns a transformed splatted argument.
        /// </summary>
        private MSA.Expression TransformToCallInternal(AstGenerator/*!*/ gen, ICollection<MSA.Expression>/*!*/ result) {
            int splattedCount;
            int firstSplatted = IndexOfSplatted(out splattedCount);

            for (int i = 0; i < (firstSplatted != -1 ? firstSplatted : _expressions.Length); i++) {
                result.Add(_expressions[i].TransformRead(gen));
            }

            // TODO: 1.9 allows multiple splats at call-site, e.g. foo(a,b,*c,*d,e,*f).
            // Currently our call-sites only implement single splatting so we convert the arguments to such form, 
            // e.g. foo(a,b,*[*c,*d,e,*f]).
            if (splattedCount == 1) {
                return _expressions[firstSplatted].TransformRead(gen);
            } else if (splattedCount > 1) {
                return UnsplatArguments(gen, firstSplatted);
            } else {
                return null;
            }
        }

        internal int IndexOfSplatted(out int splattedCount) {
            splattedCount = 0;
            int result = -1;
            for (int i = 0; i < _expressions.Length; i++) {
                if (_expressions[i] is SplattedArgument) {
                    splattedCount++;
                    if (result == -1) {
                        result = i;
                    }
                }
            }

            return result;
        }
        
        internal MSA.Expression UnsplatArguments(AstGenerator/*!*/ gen, int start) {
            // [*array] == array, [*item] = [item]
            if (start == _expressions.Length - 1) {
                return Methods.Unsplat.OpCall(AstUtils.Box(_expressions[start].TransformRead(gen)));
            }
            
            MSA.Expression array = Methods.MakeArray0.OpCall();
            for (int i = start; i < _expressions.Length; i++) {
                if (_expressions[i] is SplattedArgument) {
                    array = Methods.AddRange.OpCall(array, _expressions[i].TransformRead(gen));
                } else {
                    array = Methods.AddItem.OpCall(array, AstUtils.Box(_expressions[i].TransformRead(gen)));
                }
                
            }
            return array;
        }

        #endregion

        #region Transform To Return Value or Array Initializer

        internal MSA.Expression/*!*/ TransformToArray(AstGenerator/*!*/ gen) {
            int splattedCount;
            int splatted = IndexOfSplatted(out splattedCount);
            if (splatted >= 0) {
                return UnsplatArguments(gen, 0);
            }

            // TODO: optimize big arrays
            return Methods.MakeArrayOpCall(gen.TransformExpressions(_expressions));
        }

        internal MSA.Expression/*!*/ TransformToReturnValue(AstGenerator/*!*/ gen) {
            if (_expressions.Length == 1) {
                return _expressions[0].TransformRead(gen);
            }

            return TransformToArray(gen);
        }

        #endregion
    }
}
