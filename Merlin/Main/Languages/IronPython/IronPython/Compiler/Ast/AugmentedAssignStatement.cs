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

using System.Diagnostics;
using MSAst = System.Linq.Expressions;
using Operators = Microsoft.Scripting.Runtime.Operators;

namespace IronPython.Compiler.Ast {
    public class AugmentedAssignStatement : Statement {
        private readonly PythonOperator _op;
        private readonly Expression _left;
        private readonly Expression _right;

        public AugmentedAssignStatement(PythonOperator op, Expression left, Expression right) {
            _op = op;
            _left = left; 
            _right = right;
        }

        public PythonOperator Operator {
            get { return _op; }
        }

        public Expression Left {
            get { return _left; }
        }

        public Expression Right {
            get { return _right; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            return _left.TransformSet(ag, Span, ag.Transform(_right), PythonOperatorToAction(_op));
        }

        private static Operators PythonOperatorToAction(PythonOperator op) {
            switch (op) {
                // Binary
                case PythonOperator.Add:
                    return Operators.InPlaceAdd;
                case PythonOperator.Subtract:
                    return Operators.InPlaceSubtract;
                case PythonOperator.Multiply:
                    return Operators.InPlaceMultiply;
                case PythonOperator.Divide:
                    return Operators.InPlaceDivide;
                case PythonOperator.TrueDivide:
                    return Operators.InPlaceTrueDivide;
                case PythonOperator.Mod:
                    return Operators.InPlaceMod;
                case PythonOperator.BitwiseAnd:
                    return Operators.InPlaceBitwiseAnd;
                case PythonOperator.BitwiseOr:
                    return Operators.InPlaceBitwiseOr;
                case PythonOperator.Xor:
                    return Operators.InPlaceExclusiveOr;
                case PythonOperator.LeftShift:
                    return Operators.InPlaceLeftShift;
                case PythonOperator.RightShift:
                    return Operators.InPlaceRightShift;
                case PythonOperator.Power:
                    return Operators.InPlacePower;
                case PythonOperator.FloorDivide:
                    return Operators.InPlaceFloorDivide;
                default:
                    Debug.Assert(false, "Unexpected PythonOperator: " + op.ToString());
                    return Operators.None;
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_left != null) {
                    _left.Walk(walker);
                }
                if (_right != null) {
                    _right.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
