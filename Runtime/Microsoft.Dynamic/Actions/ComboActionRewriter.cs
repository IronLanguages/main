/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// A tree rewriter which will find dynamic sites which consume dynamic sites and
    /// turn them into a single combo dynamic site.  The combo dynamic site will then run the
    /// individual meta binders and produce the resulting code in a single dynamic site.
    /// </summary>
    public class ComboActionRewriter : ExpressionVisitor {
        /// <summary>
        /// A reducible node which we use to generate the combo dynamic sites.  Each time we encounter
        /// a dynamic site we replace it with a ComboDynamicSiteExpression.  When a child of a dynamic site
        /// turns out to be a ComboDynamicSiteExpression we will then merge the child with the parent updating
        /// the binding mapping info.  If any of the inputs cause side effects then we'll stop the combination.
        /// </summary>
        class ComboDynamicSiteExpression : Expression {
            private readonly Expression[] _inputs;
            private readonly List<BinderMappingInfo> _binders;
            private readonly Type _type;

            public ComboDynamicSiteExpression(Type type, List<BinderMappingInfo> binders, Expression[] inputs) {

                _binders = binders;
                _inputs = inputs;
                _type = type;
            }

            public override bool CanReduce {
                get { return true; }
            }

            public sealed override Type Type {
                get { return _type; }
            }

            public sealed override ExpressionType NodeType {
                get { return ExpressionType.Extension; }
            }

            public Expression[] Inputs {
                get {
                    return _inputs;
                }
            }

            public List<BinderMappingInfo> Binders {
                get {
                    return _binders;
                }
            }

            public override Expression Reduce() {
                // we just reduce to a simple DynamicExpression
                return Expression.Dynamic(
                    new ComboBinder(_binders),
                    Type,
                    _inputs
                );
            }
        }

        protected override Expression VisitDynamic(DynamicExpression node) {
            DynamicMetaObjectBinder metaBinder = node.Binder as DynamicMetaObjectBinder;

            if (metaBinder == null) {
                // don't rewrite non meta-binder nodes, we can't compose them
                return node;
            }

            // gather the real arguments for the new dynamic site node
            var args = node.Arguments;
            bool foundSideEffectingArgs = false;
            List<Expression> inputs = new List<Expression>();

            // parameter mapping is 1 List<ComboParameterMappingInfo> for each meta binder, the inner list
            // contains the mapping info for each particular binder

            List<BinderMappingInfo> binders = new List<BinderMappingInfo>();
            List<ParameterMappingInfo> myInfo = new List<ParameterMappingInfo>();

            int actionCount = 0;
            for (int i = 0; i < args.Count; i++) {
                Expression e = args[i];

                if (!foundSideEffectingArgs) {
                    // attempt to combine the arguments...
                    Expression rewritten = Visit(e);

                    ComboDynamicSiteExpression combo = rewritten as ComboDynamicSiteExpression;
                    ConstantExpression ce;
                    if (combo != null) {
                        // an action expression we can combine with our own expression

                        // remember how many actions we have so far - if any of our children consume
                        // actions their offset is bumped up
                        int baseActionCount = actionCount;

                        foreach (BinderMappingInfo comboInfo in combo.Binders) {
                            List<ParameterMappingInfo> newInfo = new List<ParameterMappingInfo>();

                            foreach (ParameterMappingInfo info in comboInfo.MappingInfo) {
                                if (info.IsParameter) {
                                    // all of the inputs from the child now become ours
                                    newInfo.Add(ParameterMappingInfo.Parameter(inputs.Count));
                                    inputs.Add(combo.Inputs[info.ParameterIndex]);
                                } else if (info.IsAction) {
                                    newInfo.Add(ParameterMappingInfo.Action(info.ActionIndex + baseActionCount));
                                    actionCount++;
                                } else {
                                    Debug.Assert(info.Constant != null);

                                    // constants can just flow through
                                    newInfo.Add(info);
                                }
                            }

                            binders.Add(new BinderMappingInfo(comboInfo.Binder, newInfo));
                        }

                        myInfo.Add(ParameterMappingInfo.Action(actionCount++));
                    } else if ((ce = rewritten as ConstantExpression) != null) {
                        // we can hoist the constant into the combo
                        myInfo.Add(ParameterMappingInfo.Fixed(ce));
                    } else if (IsSideEffectFree(rewritten)) {
                        // we can treat this as an input parameter
                        myInfo.Add(ParameterMappingInfo.Parameter(inputs.Count));
                        inputs.Add(rewritten);
                    } else {
                        // this argument is doing something we don't understand - we have to leave
                        // it as is (an input we consume) and all the remaining arguments need to be 
                        // evaluated normally as this could have side effects on them.
                        foundSideEffectingArgs = true;
                        myInfo.Add(ParameterMappingInfo.Parameter(inputs.Count));
                        inputs.Add(e);
                    }
                } else {
                    // we've already seen an argument which may have side effects, don't do
                    // any more combinations.
                    myInfo.Add(ParameterMappingInfo.Parameter(inputs.Count));
                    inputs.Add(e);
                }
            }
            binders.Add(new BinderMappingInfo(metaBinder, myInfo));
            // TODO: Remove any duplicate inputs (e.g. locals being fed in multiple times)
            return new ComboDynamicSiteExpression(node.Type, binders, inputs.ToArray());
        }

        private bool IsSideEffectFree(Expression rewritten) {
            if (rewritten is ParameterExpression) {
                return true;
            }

            if (rewritten.NodeType == ExpressionType.TypeIs) {
                return IsSideEffectFree(((UnaryExpression)rewritten).Operand);
            }

            BinaryExpression be = rewritten as BinaryExpression;
            if (be != null) {
                if (be.Method == null && IsSideEffectFree(be.Left) && IsSideEffectFree(be.Right)) {
                    return true;
                }
            }

            MethodCallExpression mc = rewritten as MethodCallExpression;
            if (mc != null && mc.Method != null) {
                return mc.Method.IsDefined(typeof(NoSideEffectsAttribute), false);
            }

            ConditionalExpression ce = rewritten as ConditionalExpression;
            if (ce != null) {
                return IsSideEffectFree(ce.Test) && IsSideEffectFree(ce.IfTrue) && IsSideEffectFree(ce.IfFalse);
            }

            MemberExpression me = rewritten as MemberExpression;
            if (me != null && me.Member is System.Reflection.FieldInfo) {
                return false;
            }

            return false;
        }
    }
}
