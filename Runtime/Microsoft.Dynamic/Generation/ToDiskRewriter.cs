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
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Ast;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Generation {

    /// <summary>
    /// Serializes constants and dynamic sites so the code can be saved to disk
    /// </summary>
    internal sealed class ToDiskRewriter : ExpressionVisitor {
        private static int _uniqueNameId;
        private List<Expression> _constants;
        private Dictionary<object, Expression> _constantCache;
        private ParameterExpression _constantPool;
        private Dictionary<Type, Type> _delegateTypes;
        private int _depth;
        private readonly TypeGen _typeGen;
        
        internal ToDiskRewriter(TypeGen typeGen) {            
            _typeGen = typeGen;
        }

        public LambdaExpression RewriteLambda(LambdaExpression lambda) {
            return (LambdaExpression)Visit(lambda);
        }

        protected override Expression VisitLambda<T>(Expression<T> node) {
            _depth++;
            try {

                // Visit the lambda first, so we walk the tree and find any
                // constants we need to rewrite.
                node = (Expression<T>)base.VisitLambda(node);

                if (_depth != 1) {
                    return node;
                }

                var body = node.Body;

                if (_constants != null) {
                    // Rewrite the constants, they can contain embedded
                    // CodeContextExpressions
                    for (int i = 0; i < _constants.Count; i++) {
                        _constants[i] = Visit(_constants[i]);
                    }

                    // Add the consant pool variable to the top lambda
                    // We first create the array and then assign into it so that we can refer to the
                    // array and read values out that have already been created.
                    ReadOnlyCollectionBuilder<Expression> assigns = new ReadOnlyCollectionBuilder<Expression>(_constants.Count + 2);
                    assigns.Add(Expression.Assign(
                        _constantPool,
                        Expression.NewArrayBounds(typeof(object), Expression.Constant(_constants.Count))
                    ));

                    // emit inner most constants first so they're available for outer most constants to consume
                    for (int i = _constants.Count - 1; i >= 0 ; i--) {
                        assigns.Add(
                            Expression.Assign(
                                Expression.ArrayAccess(_constantPool, Expression.Constant(i)),
                                _constants[i]
                            )
                        );
                    }
                    assigns.Add(body);

                    body = Expression.Block(new[] { _constantPool }, assigns);
                }

                // Rewrite the lambda
                return Expression.Lambda<T>(
                    body,
                    node.Name + "$" + Interlocked.Increment(ref _uniqueNameId),
                    node.TailCall,
                    node.Parameters
                );

            } finally {
                _depth--;
            }
        }

        protected override Expression VisitExtension(Expression node) {
            if (node.NodeType == ExpressionType.Dynamic) {
                // the node was dynamic, the dynamic nodes were removed,
                // we now need to rewrite any call sites.
                return VisitDynamic((DynamicExpression)node);
            }

            return Visit(node.Reduce());
        }

        protected override Expression VisitConstant(ConstantExpression node) {
            var site = node.Value as CallSite;
            if (site != null) {
                return RewriteCallSite(site);
            }

            var exprSerializable = node.Value as IExpressionSerializable;
            if (exprSerializable != null) {
                EnsureConstantPool();
                Expression res;

                if (!_constantCache.TryGetValue(node.Value, out res)) {
                    Expression serialized = exprSerializable.CreateExpression();
                    _constants.Add(serialized);

                    _constantCache[node.Value] = res = AstUtils.Convert(
                        Expression.ArrayAccess(_constantPool, AstUtils.Constant(_constants.Count - 1)),
                        serialized.Type
                    );
                }

                return res;
            }

            var strings = node.Value as string[];
            if (strings != null) {
                if (strings.Length == 0) {
                    return Expression.Field(null, typeof(ArrayUtils).GetField("EmptyStrings"));
                }

                _constants.Add(
                    Expression.NewArrayInit(
                         typeof(string),
                         new ReadOnlyCollection<Expression>(
                             strings.Map(s => Expression.Constant(s, typeof(string)))
                         )
                     )
                 );

                return AstUtils.Convert(
                    Expression.ArrayAccess(_constantPool, AstUtils.Constant(_constants.Count - 1)),
                    typeof(string[])
                );
            }

            return base.VisitConstant(node);
        }

        // If the DynamicExpression uses a transient (in-memory) type for its
        // delegate, we need to replace it with a new delegate type that can be
        // saved to disk
        protected override Expression VisitDynamic(DynamicExpression node) {
            Type delegateType;
            if (RewriteDelegate(node.DelegateType, out delegateType)) {
                node = Expression.MakeDynamic(delegateType, node.Binder, node.Arguments);
            }

            // Reduce dynamic expression so that the lambda can be emitted as a non-dynamic method.
            return Visit(CompilerHelpers.Reduce(node));
        }

        private bool RewriteDelegate(Type delegateType, out Type newDelegateType) {
            if (!ShouldRewriteDelegate(delegateType)) {
                newDelegateType = null;
                return false;
            }

            if (_delegateTypes == null) {
                _delegateTypes = new Dictionary<Type, Type>();
            }

            // TODO: should caching move to AssemblyGen?
            if (!_delegateTypes.TryGetValue(delegateType, out newDelegateType)) {
                MethodInfo invoke = delegateType.GetMethod("Invoke");

                newDelegateType = _typeGen.AssemblyGen.MakeDelegateType(
                    delegateType.Name,
                    invoke.GetParameters().Map(p => p.ParameterType),
                    invoke.ReturnType
                );

                _delegateTypes[delegateType] = newDelegateType;
            }

            return true;
        }

        private bool ShouldRewriteDelegate(Type delegateType) {
            // We need to replace a transient delegateType with one stored in
            // the assembly we're saving to disk.
            //
            // One complication:
            // SaveAssemblies mode prevents us from detecting the module as
            // transient. If that option is turned on, always replace delegates
            // that live in another AssemblyBuilder

            var module = delegateType.Module as ModuleBuilder;
            if (module == null) {
                return false;
            }

            if (module.IsTransient()) {
                return true;
            }

            if (Snippets.Shared.SaveSnippets && module.Assembly != _typeGen.AssemblyGen.AssemblyBuilder) {
                return true;
            }

            return false;
        }

        private Expression RewriteCallSite(CallSite site) {
            IExpressionSerializable serializer = site.Binder as IExpressionSerializable;
            if (serializer == null) {
                throw Error.GenNonSerializableBinder();
            }

            EnsureConstantPool();

            Type siteType = site.GetType();

            _constants.Add(Expression.Call(siteType.GetMethod("Create"), serializer.CreateExpression()));

            // rewrite the node...
            return Visit(
                AstUtils.Convert(
                    Expression.ArrayAccess(_constantPool, AstUtils.Constant(_constants.Count - 1)),
                    siteType
                )
            );
        }

        private void EnsureConstantPool() {
            // add the initialization code that we'll generate later into the outermost
            // lambda and then return an index into the array we'll be creating.
            if (_constantPool == null) {
                _constantPool = Expression.Variable(typeof(object[]), "$constantPool");
                _constants = new List<Expression>();
                _constantCache = new Dictionary<object, Expression>();
            }
        }
    }
}
