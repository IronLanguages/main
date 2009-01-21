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
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Generation {

    /// <summary>
    /// Rewrites known extension nodes into primitive ones:
    ///   * GlobalVariableExpression
    ///   * CodeContextExpression
    ///   * CodeContextScopeExpression
    ///   
    /// TODO: remove all of the functionality related to CodeContext, once
    /// Python and JS fix their scope implementations to and the CodeContext*
    /// nodes go away. All that should be left is global support, and even that
    /// can go once OptimizedModules moves into Python.
    /// </summary>
    public abstract class GlobalRewriter : ExpressionVisitor {
        private Expression _context;

        // Rewrite entry points
        public Expression<DlrMainCallTarget> RewriteLambda(LambdaExpression lambda) {
            return RewriteLambda(lambda, lambda.Name);
        }

        public Expression<DlrMainCallTarget> RewriteLambda(LambdaExpression lambda, string name) {
            Debug.Assert(_context == null);
            Debug.Assert(lambda.Parameters.Count == 0);

            // Fix up the top-level lambda to have a scope and language parameters
            ParameterExpression scopeParameter = Expression.Parameter(typeof(Scope), "$scope");
            ParameterExpression languageParameter = Expression.Parameter(typeof(LanguageContext), "$language");
            ParameterExpression contextVariable = Expression.Variable(typeof(CodeContext), "$globalContext");

            _context = contextVariable;
            lambda = (LambdaExpression)Visit(lambda);

            return Expression.Lambda<DlrMainCallTarget>(
                AstUtils.AddScopedVariable(
                    lambda.Body,
                    contextVariable,
                    Expression.Call(typeof(ScriptingRuntimeHelpers).GetMethod("CreateTopLevelCodeContext"), scopeParameter, languageParameter)
                ),
                name,
                new[] { scopeParameter, languageParameter }
            );
        }

        protected abstract Expression RewriteGet(GlobalVariableExpression node);
        protected abstract Expression RewriteSet(AssignmentExtensionExpression node);

        #region rewriter overrides

        // Reduce dynamic expression so that the lambda can be emitted as a non-dynamic method.
        protected override Expression VisitDynamic(DynamicExpression node) {
            return Visit(CompilerHelpers.Reduce(node));
        }

        protected override Expression VisitExtension(Expression node) {
            if (node is YieldExpression ||
                node is GeneratorExpression ||
                node is FinallyFlowControlExpression) {
                // These should be rewritten last, when doing finaly compilation
                // for now, just walk them so we can find other nodes
                return base.VisitExtension(node);
            }

            GlobalVariableExpression global = node as GlobalVariableExpression;
            if (global != null) {
                return RewriteGet(global);
            }

            CodeContextExpression cc = node as CodeContextExpression;
            if (cc != null) {
                return _context;
            }

            CodeContextScopeExpression ccs = node as CodeContextScopeExpression;
            if (ccs != null) {
                return Rewrite(ccs);
            }

            AssignmentExtensionExpression aee = node as AssignmentExtensionExpression;
            if (aee != null) {
                return Rewrite(aee);
            }

            // Must remove extension nodes because they could contain
            // one of the above node types. See, e.g. DeleteUnboundExpression
            return Visit(node.ReduceExtensions());
        }

        private Expression Rewrite(AssignmentExtensionExpression node) {
            Expression lvalue = node.Expression;

            GlobalVariableExpression global = lvalue as GlobalVariableExpression;
            if (global != null) {
                return RewriteSet(node);
            }

            return node;
        }

        #endregion

        #region CodeContext support

        protected Expression Context {
            get { return _context; }
            set { _context = value; }
        }

        private Expression Rewrite(CodeContextScopeExpression ccs) {
            Expression saved = _context;
            ParameterExpression nested = Expression.Variable(typeof(CodeContext), "$frame");

            // rewrite body with nested context
            _context = nested;
            Expression body = Visit(ccs.Body);
            _context = saved;

            // wrap the body in a scope that initializes the nested context
            return AstUtils.AddScopedVariable(body, nested, Visit(ccs.NewContext));
        }

        #endregion

        #region helpers

        protected static void EnsureUniqueName(IDictionary<string, GlobalVariableExpression> varsByName, GlobalVariableExpression node) {
            GlobalVariableExpression n2;
            if (varsByName.TryGetValue(node.Name, out n2)) {
                if (node == n2) {
                    return;
                }
                throw Error.GlobalsMustBeUnique();
            }

            varsByName.Add(node.Name, node);
        }

        #endregion
    }
}

namespace Microsoft.Scripting.Runtime {
    public static partial class ScriptingRuntimeHelpers {
        // emitted by GlobalRewriter
        // TODO: Python and JScript should do this
        public static CodeContext CreateTopLevelCodeContext(Scope scope, LanguageContext context) {
            context.EnsureScopeExtension(scope.ModuleScope);
            return new CodeContext(scope, context);
        }
    }
}
