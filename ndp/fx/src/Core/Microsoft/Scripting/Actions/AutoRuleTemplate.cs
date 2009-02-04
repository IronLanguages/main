/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.Generic;
using System.Dynamic.Utils;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace System.Dynamic {
    /// <summary>
    /// Handles auto-templating of rules.  There are three important actions this performs:
    ///     1. Detects if templating is possible between two rules
    ///     2. Re-writes a non-templated rule into templated form
    ///     3. Extracts the constants from a non-templated rule which is compatible with a 
    ///         templated rule so that they can be used by the existing generated code.
    ///         
    /// Auto-templating is currently only used for serially monomorphic call sites where we
    /// can easily avoid code gen.  It is not used for polymorphic call sites although such
    /// a feature could be enabled in the future.
    /// </summary>
    internal static class AutoRuleTemplate {
        /// <summary>
        /// The entry point into auto-rule tempating.  This consumes the monomorphic rule which is currently
        /// stored in the cache as well as the rule that was just produced by the binder.  
        /// </summary>
        /// <param name="from">The original rule that is currently stored in the cache.  This rule may
        /// or may not be a templated rule.</param>
        /// <param name="to">The new rule produced by a binder.</param>
        internal static CallSiteRule<T> CopyOrCreateTemplatedRule<T>(CallSiteRule<T> from, CallSiteRule<T> to) where T : class {
            List<KeyValuePair<ConstantExpression, int>> replacementList;

            TreeCompareResult tc = TreeComparer.CompareTrees(to.Binding, from.Binding, from.TemplatedConsts, out replacementList);

            if (tc == TreeCompareResult.Incompatible) {
                return to;
            }

            // We have 2 rules which are compatible.  We should create a new template or 
            // re-use the existing one.

            TemplateData<T> template;
            object[] values = GetConstantValues(replacementList);

            if (tc == TreeCompareResult.TooSpecific || from.Template == null) {
                // create a new one - either we are going from a non-templated rule to using a templated rule, 
                // or we are further generalizing an existing rule.  We need to re-write the incoming tree 
                // to be templated over the necessary constants and return the new rule bound to the template.

                Expression<Func<Object[], T>> templateExpr = TemplateRuleRewriter.MakeTemplate<T>(to.RuleSet.Stitch(), replacementList);

                Func<Object[], T> templateFunction = templateExpr.Compile();
                Set<int> consts = new Set<int>(replacementList.Select(pair => pair.Value));
                template = new TemplateData<T>(templateFunction, consts);
            } else {
                template = from.Template;
            }

            T newBody = template.TemplateFunction(values);
            return new CallSiteRule<T>(to.Binding, newBody, template);
        }

        private static object[] GetConstantValues(List<KeyValuePair<ConstantExpression,int>> newConstants) {
            object[] res = new object[newConstants.Count];
            int index = 0;
            foreach (KeyValuePair<ConstantExpression,int> ce in newConstants) {
                res[index++] = ce.Key.Value;
            }
            return res;
        }


        internal class TemplateRuleRewriter : ExpressionVisitor {
            private Dictionary<int, ParameterExpression> _map;
            private int _curConstNum;

            private TemplateRuleRewriter(Dictionary<int, ParameterExpression> map) {
                _map = map;
                _curConstNum = -1;
            }

            /// <summary>
            /// Creates a higher order factory expression that can produce
            /// instances of original expresion bound to given set of constant values.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="origTree">Original expresion.</param>
            /// <param name="constToTemplate">Which constants should be parameterised.</param>
            /// <returns>Factory expression.</returns>
            internal static Expression<Func<Object[], T>> MakeTemplate<T>(
                    Expression<T> origTree, 
                    List<KeyValuePair<ConstantExpression,int>> constToTemplate) {


                // The goal is to produce a nested lambda 
                // which is the same as original, but with specified constants re-bound to variables
                // that we will initialize to given values.

                ParameterExpression constsArg = Expression.Parameter(typeof(object[]), null);

                // these are the variables that would act as constant replacements. 
                // they will be hoisted into a closure so every produced rule will get its own version.
                List<ParameterExpression> locals = new List<ParameterExpression>();

                // maping of constants to locals to tell rewriter what to do.
                Dictionary<int, ParameterExpression> map = new Dictionary<int, ParameterExpression>();

                List<Expression> statements = new List<Expression>();

                int i = 0;
                foreach(var cur in constToTemplate) {
                    Type curConstType = TypeUtils.GetConstantType(cur.Key.Type);
                    var local = Expression.Parameter(curConstType, null);
                    locals.Add(local);
                    statements.Add(
                        Expression.Assign(
                            local,
                            Helpers.Convert(
                                Expression.ArrayAccess(constsArg, Expression.Constant(i)),
                                curConstType
                            )
                        )
                    );
                    map.Add(cur.Value, local);
                    i++;
                }

                // remap original lambda
                TemplateRuleRewriter rewriter = new TemplateRuleRewriter(map);
                Expression<T> templatedTree = (Expression<T>)rewriter.Visit(origTree);

                statements.Add(templatedTree);

                
                //  T template(object[] constArg){
                //      local0 = constArg[0];
                //      local1 = constArg[1];
                //      ...
                //      localN = constArg[N];
                //
                //      return {original lambda, but with selected consts bound to locals}      
                //  }

                Expression<Func<Object[], T>> template = Expression.Lambda<Func<Object[], T>>(
                    Expression.Block(
                        locals,
                        statements
                    ),
                    constsArg
                );

                // Need to compile with forceDynamic because T could be invisible,
                // or one of the argument types could be invisible
                return template;
            }

            protected internal override Expression VisitConstant(ConstantExpression node) {
                _curConstNum++;

                if (_map.ContainsKey(_curConstNum)) {
                    return _map[_curConstNum];
                }

                return base.VisitConstant(node);
            }

            // Extensions may contain constants too. 
            // We visit them in TreeCompare and so we do here.
            protected internal override Expression VisitExtension(Expression node) {
                return Visit(node.ReduceExtensions());
            }

        }
    }
}

