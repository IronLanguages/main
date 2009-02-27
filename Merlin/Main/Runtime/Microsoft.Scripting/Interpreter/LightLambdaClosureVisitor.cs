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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using System.Reflection;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Interpreter {

    /// <summary>
    /// Visits a LambdaExpression, replacing the constants with direct accesses
    /// to their StrongBox fields. This is very similar to what
    /// ExpressionQuoter does for LambdaCompiler.
    /// 
    /// Also inserts debug information tracking similar to what the interpreter
    /// would do.
    /// </summary>
    internal sealed class LightLambdaClosureVisitor : ExpressionVisitor {
        /// <summary>
        /// Indexes of variables into the closure array
        /// </summary>
        private readonly Dictionary<ParameterExpression, int> _closureVars;

        /// <summary>
        /// The variable that holds onto the StrongBox{object}[] closure from
        /// the interpreter
        /// </summary>
        private readonly ParameterExpression _closureArray;

        /// <summary>
        /// A stack of variables that are defined in nested scopes. We search
        /// this first when resolving a variable in case a nested scope shadows
        /// one of our variable instances.
        /// </summary>
        private readonly Stack<Set<ParameterExpression>> _shadowedVars = new Stack<Set<ParameterExpression>>();

        // variables for tracking exception information:
        private ParameterExpression _fileName;
        private ParameterExpression _lineNumber;
        private SymbolDocumentInfo _lastDocument;
        private int _lastLineNumber;

        private LightLambdaClosureVisitor(IList<ParameterExpression> closureVars, ParameterExpression closureArray) {
            _closureArray = closureArray;
            _closureVars = new Dictionary<ParameterExpression, int>(closureVars.Count);
            for (int i = 0, n = closureVars.Count; i < n; i++) {
                _closureVars.Add(closureVars[i], i);
            }
        }

        /// <summary>
        /// Walks the lambda and produces a higher order function, which can be
        /// used to bind the lambda to a closure array from the interpreter.
        /// </summary>
        /// <param name="lambda">The lambda to bind.</param>
        /// <param name="closureVars">The variables that are closed over from an outer scope.</param>
        /// <param name="delegateTypeMatch">true if the delegate type is the same; false if it was changed to Func/Action.</param>
        /// <returns>A delegate that can be called to produce a delegate bound to the passed in closure array.</returns>
        internal static Func<StrongBox<object>[], Delegate> BindLambda(LambdaExpression lambda, IList<ParameterExpression> closureVars, out bool delegateTypeMatch) {
            var closure = Expression.Parameter(typeof(StrongBox<object>[]), "closure");
            var visitor = new LightLambdaClosureVisitor(closureVars, closure);
            LambdaExpression rewritten = visitor.VisitTopLambda(lambda);
            delegateTypeMatch = (rewritten.Type == lambda.Type);

            // Create a higher-order function which fills in the parameters
            var result = Expression.Lambda<Func<StrongBox<object>[], Delegate>>(rewritten, closure);
            return result.Compile();
        }

        private LambdaExpression VisitTopLambda(LambdaExpression lambda) {
            // 1. Rewrite the the tree
            lambda = (LambdaExpression)Visit(lambda);

            // 2. Fix the lambda's delegate type: it must be Func<...> or
            // Action<...> to be called from the generated Run methods.
            Type delegateType = GetDelegateType(lambda);

            // 3. Add top level exception handling
            Expression body = AddExceptionHandling(lambda.Body, lambda.Name);

            // 4. Return the lambda with the handling and the (possibly new) delegate type
            return Expression.Lambda(delegateType, body, lambda.Name, lambda.Parameters);
        }

        private static Type GetDelegateType(LambdaExpression lambda) {
            Type delegateType = lambda.Type;
            if (lambda.ReturnType == typeof(void) && lambda.Parameters.Count == 2 &&
                lambda.Parameters[0].IsByRef && lambda.Parameters[1].IsByRef) {
                delegateType = typeof(ActionRef<,>).MakeGenericType(lambda.Parameters.Map(p => p.Type));
            } else {
                Type[] types = lambda.Parameters.Map(p => p.IsByRef ? p.Type.MakeByRefType() : p.Type);
                delegateType = Expression.GetDelegateType(types.AddLast(lambda.ReturnType));
            }
            return delegateType;
        }

        #region debugging

        private Expression AddExceptionHandling(Expression body, string methodName) {
            if (_fileName == null) {
                return body;
            }

            var e = Expression.Variable(typeof(Exception), "e");
            return Expression.Block(
                new[] { _fileName, _lineNumber },
                Expression.TryCatch(
                    body,
                    Expression.Catch(
                        e,
                        Expression.Block(
                            Expression.Call(
                                typeof(ExceptionHelpers),
                                "UpdateStackTraceForRethrow",
                                null, 
                                e,
                                Expression.Call(typeof(MethodBase), "GetCurrentMethod", null),
                                AstUtils.Constant(methodName),
                                _fileName,
                                _lineNumber
                            ),
                            Expression.Rethrow(body.Type)
                        )
                    )
                )
            );
        }

        protected override Expression VisitDebugInfo(DebugInfoExpression node) {
            // We're not going to clear the debug info, as that requires a
            // try-finally for every debug info. Also we'll just use the
            // start line.
            // TODO: under the new design, we may be able to do the clearance correctly.
            if (node.IsClear) {
                return node;
            }

            if (_fileName == null) {
                _fileName = Expression.Variable(typeof(string), "file");
                _lineNumber = Expression.Variable(typeof(int), "line");
            }

            if (node.Document == _lastDocument) {
                if (node.StartLine == _lastLineNumber) {
                    // Just return the node so we don't have to rewrite.
                    // The compiler will ignore it if compiling into a
                    // DynamicMethod.
                    return base.VisitDebugInfo(node);
                }

                _lastLineNumber = node.StartLine;
                return Expression.Block(
                    Expression.Assign(_lineNumber, AstUtils.Constant(_lastLineNumber)),
                    AstUtils.Empty()
                );
            }

            _lastDocument = node.Document;
            _lastLineNumber = node.StartLine;
            return Expression.Block(
                Expression.Assign(_fileName, AstUtils.Constant(_lastDocument.FileName)),
                Expression.Assign(_lineNumber, AstUtils.Constant(_lastLineNumber)),
                AstUtils.Empty()
            );
        }

        #endregion

        #region closures

        protected override Expression VisitLambda<T>(Expression<T> node) {
            _shadowedVars.Push(new Set<ParameterExpression>(node.Parameters));
            Expression b = Visit(node.Body);
            _shadowedVars.Pop();
            if (b == node.Body) {
                return node;
            }
            return Expression.Lambda<T>(b, node.Name, node.Parameters);
        }

        protected override Expression VisitBlock(BlockExpression node) {
            if (node.Variables.Count > 0) {
                _shadowedVars.Push(new Set<ParameterExpression>(node.Variables));
            }
            var b = Visit(node.Expressions);
            if (node.Variables.Count > 0) {
                _shadowedVars.Pop();
            }
            if (b == node.Expressions) {
                return node;
            }
            return Expression.Block(node.Variables, b);
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node) {
            if (node.Variable != null) {
                _shadowedVars.Push(new Set<ParameterExpression>(new[] { node.Variable }));
            }
            Expression b = Visit(node.Body);
            Expression f = Visit(node.Filter);
            if (node.Variable != null) {
                _shadowedVars.Pop();
            }
            if (b == node.Body && f == node.Filter) {
                return node;
            }
            return Expression.MakeCatchBlock(node.Test, node.Variable, b, f);
        }

        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
            int count = node.Variables.Count;
            var boxes = new List<Expression>();
            var vars = new List<ParameterExpression>();
            var indexes = new int[count];
            for (int i = 0; i < count; i++) {
                Expression box = GetBox(node.Variables[i]);
                if (box == null) {
                    indexes[i] = vars.Count;
                    vars.Add(node.Variables[i]);
                } else {
                    indexes[i] = -1 - boxes.Count;
                    boxes.Add(box);
                }
            }

            // No variables were rewritten. Just return the original node.
            if (boxes.Count == 0) {
                return node;
            }

            var boxesArray = Expression.NewArrayInit(typeof(IStrongBox), boxes);

            // All of them were rewritten. Just return the array, wrapped in a
            // read-only collection.
            if (vars.Count == 0) {
                return Expression.New(
                    typeof(ReadOnlyCollection<IStrongBox>).GetConstructor(new[] { typeof(IList<IStrongBox>) }),
                    boxesArray
                );
            }

            // Otherwise, we need to return an object that merges them
            Func<IList<IStrongBox>, IList<IStrongBox>, int[], IList<IStrongBox>> helper = MergedRuntimeVariables.Create;
            return Expression.Invoke(AstUtils.Constant(helper), Expression.RuntimeVariables(vars), boxesArray, AstUtils.Constant(indexes));
        }

        protected override Expression VisitParameter(ParameterExpression node) {
            Expression box = GetBox(node);
            if (box == null) {
                return node;
            }
            // Convert can go away if we switch to strongly typed StrongBox
            return Ast.Utils.Convert(Expression.Field(box, "Value"), node.Type);
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            if (node.NodeType == ExpressionType.Assign &&
                node.Left.NodeType == ExpressionType.Parameter) {

                var variable = (ParameterExpression)node.Left;
                Expression box = GetBox(variable);
                if (box != null) {
                    // We need to convert to object to store the value in the box.
                    return Expression.Block(
                        new[] { variable },
                        Expression.Assign(variable, Visit(node.Right)), 
                        Expression.Assign(Expression.Field(box, "Value"), Ast.Utils.Convert(variable, typeof(object))),
                        variable
                    );
                }
            }
            return base.VisitBinary(node);
        }

        private IndexExpression GetBox(ParameterExpression variable) {
            // Skip variables that are shadowed by a nested scope/lambda
            foreach (Set<ParameterExpression> hidden in _shadowedVars) {
                if (hidden.Contains(variable)) {
                    return null;
                }
            }

            int index;
            if (_closureVars.TryGetValue(variable, out index)) {
                return Expression.ArrayAccess(_closureArray, AstUtils.Constant(index));
            }

            throw new InvalidOperationException("unbound variable: " + variable);
        }

        protected override Expression VisitExtension(Expression node) {
            // Reduce extensions now so we can find embedded variables
            return Visit(node.ReduceExtensions());
        }

        #region MergedRuntimeVariables

        /// <summary>
        /// Provides a list of variables, supporing read/write of the values
        /// </summary>
        private sealed class MergedRuntimeVariables : IList<IStrongBox> {
            private readonly IList<IStrongBox> _first;
            private readonly IList<IStrongBox> _second;

            // For reach item, the index into the first or second list
            // Positive values mean the first array, negative means the second
            private readonly int[] _indexes;

            private MergedRuntimeVariables(IList<IStrongBox> first, IList<IStrongBox> second, int[] indexes) {
                _first = first;
                _second = second;
                _indexes = indexes;
            }

            internal static IList<IStrongBox> Create(IList<IStrongBox> first, IList<IStrongBox> second, int[] indexes) {
                return new MergedRuntimeVariables(first, second, indexes);
            }

            public int Count {
                get { return _indexes.Length; }
            }

            public IStrongBox this[int index] {
                get {
                    index = _indexes[index];
                    return (index >= 0) ? _first[index] : _second[-1 - index];
                }
                set {
                    throw new NotSupportedException("Collection is read-only.");
                }
            }

            public int IndexOf(IStrongBox item) {
                for (int i = 0, n = _indexes.Length; i < n; i++) {
                    if (this[i] == item) {
                        return i;
                    }
                }
                return -1;
            }

            public bool Contains(IStrongBox item) {
                return IndexOf(item) >= 0;
            }

            public void CopyTo(IStrongBox[] array, int arrayIndex) {
                ContractUtils.RequiresNotNull(array, "array");
                int count = _indexes.Length;
                if (arrayIndex < 0 || arrayIndex + count > array.Length) {
                    throw new ArgumentOutOfRangeException("arrayIndex");
                }
                for (int i = 0; i < count; i++) {
                    array[arrayIndex++] = this[i];
                }
            }

            bool ICollection<IStrongBox>.IsReadOnly {
                get { return true; }
            }

            public IEnumerator<IStrongBox> GetEnumerator() {
                for (int i = 0, n = _indexes.Length; i < n; i++) {
                    yield return this[i];
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            void IList<IStrongBox>.Insert(int index, IStrongBox item) {
                throw new NotSupportedException("Collection is read-only.");
            }

            void IList<IStrongBox>.RemoveAt(int index) {
                throw new NotSupportedException("Collection is read-only.");
            }

            void ICollection<IStrongBox>.Add(IStrongBox item) {
                throw new NotSupportedException("Collection is read-only.");
            }

            void ICollection<IStrongBox>.Clear() {
                throw new NotSupportedException("Collection is read-only.");
            }

            bool ICollection<IStrongBox>.Remove(IStrongBox item) {
                throw new NotSupportedException("Collection is read-only.");
            }
        }
        #endregion

        #endregion

    }
}
