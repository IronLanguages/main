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
using System.Runtime.CompilerServices;
using System.Dynamic;
using System.Dynamic.Utils;

namespace System.Linq.Expressions {

    internal enum TreeCompareResult {
        Incompatible,
        Compatible,
        TooSpecific,
    }

    internal class TreeComparer {
        #region Tree Walker

        /// <summary>
        /// Walks all of the nodes of a tree and puts all of the expressions into
        /// a list.
        /// </summary>
        private class FlatTreeWalker : ExpressionVisitor {
            internal List<Expression> Expressions = new List<Expression>();

            protected internal override Expression VisitDynamic(DynamicExpression node) {
                Expressions.Add(node);
                return base.VisitDynamic(node);
            }

            protected internal override Expression VisitBinary(BinaryExpression node) {
                Expressions.Add(node);
                return base.VisitBinary(node);
            }

            protected internal override Expression VisitBlock(BlockExpression node) {
                Expressions.Add(node);
                return base.VisitBlock(node);
            }

            protected internal override Expression VisitGoto(GotoExpression node) {
                Expressions.Add(node);
                return base.VisitGoto(node);
            }

            protected internal override Expression VisitConditional(ConditionalExpression node) {
                Expressions.Add(node);
                return base.VisitConditional(node);
            }

            protected internal override Expression VisitConstant(ConstantExpression node) {
                Expressions.Add(node);
                return base.VisitConstant(node);
            }

            protected internal override Expression VisitDefault(DefaultExpression node) {
                Expressions.Add(node);
                return base.VisitDefault(node);
            }

            protected internal override Expression VisitInvocation(InvocationExpression node) {
                Expressions.Add(node);
                return base.VisitInvocation(node);
            }

            protected internal override Expression VisitLabel(LabelExpression node) {
                Expressions.Add(node);
                return base.VisitLabel(node);
            }

            protected internal override Expression VisitLambda<T>(Expression<T> node) {
                Expressions.Add(node);
                return base.VisitLambda(node);
            }

            protected internal override Expression VisitLoop(LoopExpression node) {
                Expressions.Add(node);
                return base.VisitLoop(node);
            }

            protected internal override Expression VisitMember(MemberExpression node) {
                Expressions.Add(node);
                return base.VisitMember(node);
            }

            protected internal override Expression VisitMethodCall(MethodCallExpression node) {
                Expressions.Add(node);
                return base.VisitMethodCall(node);
            }

            protected internal override Expression VisitNewArray(NewArrayExpression node) {
                Expressions.Add(node);
                return base.VisitNewArray(node);
            }

            protected internal override Expression VisitNew(NewExpression node) {
                Expressions.Add(node);
                return base.VisitNew(node);
            }

            protected internal override Expression VisitParameter(ParameterExpression node) {
                Expressions.Add(node);
                return base.VisitParameter(node);
            }

            protected internal override Expression VisitSwitch(SwitchExpression node) {
                Expressions.Add(node);
                return base.VisitSwitch(node);
            }

            protected internal override Expression VisitTry(TryExpression node) {
                Expressions.Add(node);
                return base.VisitTry(node);
            }

            protected internal override Expression VisitTypeBinary(TypeBinaryExpression node) {
                Expressions.Add(node);
                return base.VisitTypeBinary(node);
            }

            protected internal override Expression VisitUnary(UnaryExpression node) {
                Expressions.Add(node);
                return base.VisitUnary(node);
            }

            protected internal override Expression VisitExtension(Expression node) {
                if (!node.CanReduce) {
                    Expressions.Add(node);
                } else {
                    return Visit(node.ReduceExtensions());
                }
                return node;
            }
        }

        #endregion

        private class VariableInfo {
            private Dictionary<ParameterExpression, int> _left = new Dictionary<ParameterExpression, int>();
            private Dictionary<ParameterExpression, int> _right = new Dictionary<ParameterExpression, int>();
            private int _curLeft, _curRight;

            internal int GetLeftVariable(ParameterExpression ve) {
                if (ve == null) {
                    return -1;
                }

                int res;
                if (!_left.TryGetValue(ve, out res)) {
                    _left[ve] = res = _curLeft++;
                }

                return res;
            }

            internal int GetRightVariable(ParameterExpression ve) {
                if (ve == null) {
                    return -1;
                }

                int res;
                if (!_right.TryGetValue(ve, out res)) {
                    _right[ve] = res = _curRight++;
                }

                return res;
            }
        }

        private VariableInfo _varInfo;
        private int _curConstNum;

        /// <summary>
        /// Constants that were templated in original tree.
        /// </summary>
        private System.Linq.Expressions.Compiler.Set<int> _templated;

        /// <summary>
        /// New tree requires more general template.
        /// </summary>
        private bool _tooSpecific;
        
        /// <summary>
        /// New tree is sufficiently similar to the old one.
        /// </summary>
        private bool _compatible;

        /// <summary>
        /// Constants that require parameterisation and their position 
        /// The numbering is assumed as in traversal by ExpressionVisitor.
        /// </summary>
        private List<KeyValuePair<ConstantExpression, int>> _replacementList;

        private TreeComparer(System.Linq.Expressions.Compiler.Set<int> templated) {
            _templated = templated;
        }

        internal static TreeCompareResult CompareTrees(
                Expression left, 
                Expression right, 
                System.Linq.Expressions.Compiler.Set<int> templated,
                out List<KeyValuePair<ConstantExpression, int>> ReplacementList){


            TreeComparer comparer = new TreeComparer(templated);
            comparer.Compare(left, right);

            if (!comparer._compatible) {
                ReplacementList = null;
                return TreeCompareResult.Incompatible;
            }

            ReplacementList = comparer._replacementList;
            if (comparer._tooSpecific) {
                return TreeCompareResult.TooSpecific;
            } else {
                return TreeCompareResult.Compatible;
            }
        }

        /// <summary>
        /// Compares two trees.
        /// If trees differ only in constants, produces list of constants that should be parameterised.
        /// Also verifies if existing template is sufficient and could be reused.
        /// </summary>
        private void Compare(Expression left, Expression right) {
            FlatTreeWalker walkLeft = new FlatTreeWalker();
            FlatTreeWalker walkRight = new FlatTreeWalker();
            walkLeft.Visit(left);
            walkRight.Visit(right);

            // false untill proven compatible.
            _compatible = false;

            // check the length first to see if the trees are obviously different            
            if (walkLeft.Expressions.Count != walkRight.Expressions.Count) {
                return;
            }

            _varInfo = new VariableInfo();
            _curConstNum = -1;
            _replacementList = new List<KeyValuePair<ConstantExpression, int>>();           

            // then see if they differ by just constants which we could replace
            for (int i = 0; i < walkLeft.Expressions.Count; i++) {
                Expression currentLeft = walkLeft.Expressions[i], currentRight = walkRight.Expressions[i];

                if (currentLeft.NodeType != currentRight.NodeType) {
                    // different node types, they can't possibly be equal
                    return;
                } else if (currentLeft.Type != currentRight.Type) {
                    // they can't possibly be a match
                    return;
                }

                if (!CompareTwoNodes(currentLeft, currentRight)) {
                    return;
                }
            }

            _compatible = true;
            return;
        }

        private bool IsTemplatedConstant(int constantNum) {
            return _templated != null && _templated.Contains(constantNum);
        }

        private void AddToReplacementList(ConstantExpression ce) {
            _replacementList.Add(new KeyValuePair<ConstantExpression, int>(ce, _curConstNum));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private bool CompareTwoNodes(Expression currentLeft, Expression currentRight) {
            switch (currentLeft.NodeType) {
                case ExpressionType.Dynamic:
                    var dynLeft = (DynamicExpression)currentLeft;
                    var dynRight = (DynamicExpression)currentRight;

                    if (!dynRight.Binder.CacheIdentity.Equals(dynLeft.Binder.CacheIdentity)) {
                        return false;
                    }
                    break;
                case ExpressionType.Constant:
                    _curConstNum++;

                    // check constant value                        
                    ConstantExpression ceLeft = (ConstantExpression)currentLeft;
                    ConstantExpression ceRight = (ConstantExpression)currentRight;

                    object leftValue = ceLeft.Value;
                    object rightValue = ceRight.Value;

                    // See if they're both sites
                    CallSite leftSite = leftValue as CallSite;
                    CallSite rightSite = rightValue as CallSite;
                    if (leftSite != null) {
                        if (rightSite == null) {
                            return false;
                        }

                        if (!leftSite.Binder.CacheIdentity.Equals(rightSite.Binder.CacheIdentity)) {
                            return false;
                        }

                        return true;
                    } else if (rightSite != null) {
                        return false;
                    }

                    if (IsTemplatedConstant(_curConstNum)) {
                        // always add already templated values
                        AddToReplacementList(ceLeft);
                    } else {
                        // different constants should become parameters in the template.
                        if (leftValue == null) {
                            if (rightValue != null) {
                                //new templated const
                                _tooSpecific = true;
                                AddToReplacementList(ceLeft);
                            }
                        } else {
                            if (!leftValue.Equals(rightValue)){
                                //new templated const
                                _tooSpecific = true;
                                AddToReplacementList(ceLeft);
                            }
                        }
                    }

                    break;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    if (!CompareEquality((BinaryExpression)currentLeft, (BinaryExpression)currentRight)) {
                        return false;
                    }
                    break;
                case ExpressionType.Add:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Divide:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.AddAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.DivideAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ExclusiveOrAssign:
                    if (!Compare((BinaryExpression)currentLeft, (BinaryExpression)currentRight)) {
                        return false;
                    }
                    break;
                case ExpressionType.Call:
                    if (!Compare((MethodCallExpression)currentLeft, (MethodCallExpression)currentRight)) {
                        return false;
                    }
                    break;
                case ExpressionType.New:
                    // chcek ConstructorInfo and BindingInfo
                    if (!Compare((NewExpression)currentLeft, (NewExpression)currentRight)) {
                        return false;
                    }
                    break;
                case ExpressionType.TypeIs:
                case ExpressionType.TypeEqual:
                    // check type
                    if (!Compare((TypeBinaryExpression)currentLeft, (TypeBinaryExpression)currentRight)) {
                        return false;
                    }
                    break;
                case ExpressionType.Block:
                    // compare factory method
                    if (!Compare(_varInfo, (BlockExpression)currentLeft, (BlockExpression)currentRight)) {
                        return false;
                    }
                    break;
                case ExpressionType.MemberAccess:
                    // compare member
                    if (!Compare((MemberExpression)currentLeft, (MemberExpression)currentRight)) {
                        return false;
                    }
                    break;
                case ExpressionType.Try:
                    // compare catch finally blocks and their handler types
                    if (!Compare(_varInfo, (TryExpression)currentLeft, (TryExpression)currentRight)) {
                        return false;
                    }
                    break;
                case ExpressionType.Parameter:
                    if (!Compare(_varInfo, (ParameterExpression)currentLeft, (ParameterExpression)currentRight)) {
                        return false;
                    }
                    break;
                case ExpressionType.Lambda:
                case ExpressionType.Assign:
                case ExpressionType.Goto:
                case ExpressionType.Throw:
                case ExpressionType.Loop:
                case ExpressionType.Default:
                case ExpressionType.Convert:
                case ExpressionType.TypeAs:
                case ExpressionType.Unbox:
                case ExpressionType.Negate:
                case ExpressionType.Not:
                case ExpressionType.IsFalse:
                case ExpressionType.IsTrue:
                case ExpressionType.OnesComplement:
                case ExpressionType.Conditional:
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                case ExpressionType.Invoke:
                    // these nodes children and types completely
                    // define the node
                    break;
                case ExpressionType.Label:
                case ExpressionType.Switch:
                    // we could improve the compare to compare labels & switch,
                    // but these are rarely used in rules.
                    return false;
                case ExpressionType.Extension:
                    // we should have been reduced, but error on the side of being different.
                    return false;
                default:
                    throw ContractUtils.Unreachable;
            }
            return true;
        }

        private static bool CompareEquality(BinaryExpression left, BinaryExpression right) {
            if (left.Left.Type == typeof(object) && left.Right.Type == typeof(object)) {
                // could be comparing object to runtime constant w/ identity semantics.
                return CompareBinaryForEquality(GetConstantExpression(left.Left), GetConstantExpression(right.Left)) &&
                       CompareBinaryForEquality(GetConstantExpression(left.Right), GetConstantExpression(right.Right));
            }

            return true;
        }

        private static ConstantExpression GetConstantExpression(Expression expression) {
            if (expression.NodeType == ExpressionType.Convert) {
                return GetConstantExpression(((UnaryExpression)expression).Operand);
            }

            return expression as ConstantExpression;
        }

        private static bool CompareBinaryForEquality(ConstantExpression left, ConstantExpression right) {
            if (left == null || right == null) {
                return true;
            }

            return left.Value == right.Value;
        }

        private static bool Compare(BinaryExpression left, BinaryExpression right) {
            if (left.Method != right.Method) {
                return false;
            }

            return true;
        }

        private static bool Compare(MethodCallExpression left, MethodCallExpression right) {
            if (left.Method != right.Method) {
                return false;
            }

            return true;
        }

        private static bool Compare(NewExpression left, NewExpression right) {
            if (left.Constructor != right.Constructor) {
                return false;
            }

            return true;
        }


        private static bool Compare(TypeBinaryExpression left, TypeBinaryExpression right) {
            if (left.TypeOperand != right.TypeOperand) {
                return false;
            }

            return true;
        }

        private static bool Compare(VariableInfo varInfo, BlockExpression left, BlockExpression right) {
            if (left.Variables.Count != right.Variables.Count) {
                return false;
            }

            for (int i = 0; i < left.Variables.Count; i++) {
                Compare(varInfo, left.Variables[i], right.Variables[i]);
            }
            return true;
        }

        private static bool Compare(MemberExpression left, MemberExpression right) {
            if (left.Member != right.Member) {
                return false;
            }

            return true;
        }

        private static bool Compare(VariableInfo varInfo, TryExpression left, TryExpression right) {
            if ((left.Finally == null && right.Finally != null) ||
                (left.Finally != null && right.Finally == null)) {
                return false;
            }

            if (left.Handlers.Count != right.Handlers.Count) {
                return false;
            }

            for (int i = 0; i < left.Handlers.Count; i++) {
                if (left.Handlers[i].Test != right.Handlers[i].Test) {
                    return false;
                }

                if (varInfo.GetLeftVariable(left.Handlers[i].Variable) != varInfo.GetRightVariable(right.Handlers[i].Variable)) {
                    return false;
                }
            }

            return true;
        }

        private static bool Compare(VariableInfo varInfo, ParameterExpression left, ParameterExpression right) {
            if (varInfo.GetLeftVariable(left) != varInfo.GetRightVariable(right)) {
                return false;
            }

            return true;
        }
    }
}
