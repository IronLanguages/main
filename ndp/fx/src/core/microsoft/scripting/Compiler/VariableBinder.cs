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

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic.Utils;

namespace System.Linq.Expressions.Compiler {
    /// <summary>
    /// Determines if variables are closed over in nested lambdas and need to
    /// be hoisted.
    /// </summary>
    internal sealed class VariableBinder : ExpressionVisitor {
        private readonly AnalyzedTree _tree = new AnalyzedTree();
        private readonly Stack<CompilerScope> _scopes = new Stack<CompilerScope>();
        private readonly Stack<BoundConstants> _constants = new Stack<BoundConstants>();
        private bool _inQuote;

        internal static AnalyzedTree Bind(LambdaExpression lambda) {
            var binder = new VariableBinder();
            binder.Visit(lambda);
            return binder._tree;
        }

        private VariableBinder() {
        }

        protected internal override Expression VisitConstant(ConstantExpression node) {
            // If we're in Quote, we can ignore constants completely
            if (_inQuote) {
                return node;
            }
            
            // Constants that can be emitted into IL don't need to be stored on
            // the delegate
            if (ILGen.CanEmitConstant(node.Value, node.Type)) {
                return node;
            }

            _constants.Peek().AddReference(node.Value, node.Type);
            return node;
        }

        protected internal override Expression VisitUnary(UnaryExpression node) {
            if (node.NodeType == ExpressionType.Quote) {
                bool savedInQuote = _inQuote;
                _inQuote = true;
                Visit(node.Operand);
                _inQuote = savedInQuote;
            } else {
                Visit(node.Operand);
            }
            return node;
        }

        protected internal override Expression VisitLambda<T>(Expression<T> node) {
            _scopes.Push(_tree.Scopes[node] = new CompilerScope(node));
            _constants.Push(_tree.Constants[node] = new BoundConstants());
            Visit(MergeScopes(node));
            _constants.Pop();
            _scopes.Pop();
            return node;
        }

        protected internal override Expression VisitBlock(BlockExpression node) {
            if (node.Variables.Count == 0) {
                Visit(node.Expressions);
                return node;
            }
            _scopes.Push(_tree.Scopes[node] = new CompilerScope(node));
            Visit(MergeScopes(node));
            _scopes.Pop();
            return node;
        }

        // If the immediate child is another scope, merge it into this one
        // This is an optimization to save environment allocations and
        // array accesses.
        private ReadOnlyCollection<Expression> MergeScopes(Expression node) {
            ReadOnlyCollection<Expression> body;
            var lambda = node as LambdaExpression;
            if (lambda != null) {
                body = new ReadOnlyCollection<Expression>(new[] { lambda.Body });
            }  else {
                body = ((BlockExpression)node).Expressions;
            }

            var currentScope = _scopes.Peek();
            while (IsMergeable(body)) {
                var block = (BlockExpression)body[0];

                if (block.Variables.Count > 0) {
                    if (currentScope.MergedScopes == null) {
                        currentScope.MergedScopes = new Set<BlockExpression>();
                    }
                    currentScope.MergedScopes.Add(block);
                    foreach (var v in block.Variables) {
                        currentScope.Definitions.Add(v, VariableStorageKind.Local);
                    }
                }
                node = block;
                body = block.Expressions;
            }
            return body;
        }

        //A block body is mergeable if the body only contains one single block node containing variables,
        //and the child block has the same type as the parent block.
        private static bool IsMergeable(ReadOnlyCollection<Expression> body) {
            return body.Count == 1 && body[0].NodeType == ExpressionType.Block;
        }


        protected internal override Expression VisitParameter(ParameterExpression node) {
            var scope = _scopes.Peek();

            if (scope.Definitions.ContainsKey(node)) {
                return node;
            }

            if (scope.ReferenceCount == null) {
                scope.ReferenceCount = new Dictionary<ParameterExpression, int>();
            }

            Helpers.IncrementCount(node, _scopes.Peek().ReferenceCount);
            Reference(node, VariableStorageKind.Local);
            return node;
        }

        protected internal override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
            foreach (var v in node.Variables) {
                // Force hoisting of these variables
                Reference(v, VariableStorageKind.Hoisted);
            }
            return node;
        }

        private void Reference(ParameterExpression node, VariableStorageKind storage) {
            CompilerScope definition = null;
            foreach (CompilerScope scope in _scopes) {
                if (scope.Definitions.ContainsKey(node)) {
                    definition = scope;
                    break;
                }
                scope.NeedsClosure = true;
                if (scope.Node.NodeType == ExpressionType.Lambda) {
                    storage = VariableStorageKind.Hoisted;
                }
            }
            if (definition == null) {
                throw Error.UndefinedVariable(node.Name, node.Type, CurrentLambdaName);
            }
            if (storage == VariableStorageKind.Hoisted) {
                if (node.IsByRef) {
                    throw Error.CannotCloseOverByRef(node.Name, CurrentLambdaName);
                }
                definition.Definitions[node] = VariableStorageKind.Hoisted;
            }
        }

        private CompilerScope LambdaScope {
            get {
                foreach (var scope in _scopes) {
                    if (scope.Node.NodeType == ExpressionType.Lambda) {
                        return scope;
                    }
                }
                throw ContractUtils.Unreachable;
            }
        }

        private string CurrentLambdaName {
            get {
                return ((LambdaExpression)LambdaScope.Node).Name;
            }
        }
    }
}
