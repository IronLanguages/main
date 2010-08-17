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

using System;
using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    /// <summary>
    /// lhs = rhs
    /// lhs op= rhs
    /// </summary>
    public partial class SimpleAssignmentExpression : AssignmentExpression {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static new SimpleAssignmentExpression[] EmptyArray = new SimpleAssignmentExpression[0];

        private readonly LeftValue/*!*/ _left;
        private readonly Expression/*!*/ _right;

        public LeftValue/*!*/ Left {
            get { return _left; }
        }

        public Expression/*!*/ Right {
            get { return _right; }
        }

        public SimpleAssignmentExpression(LeftValue/*!*/ left, Expression/*!*/ right, string operation, SourceSpan location)
            : base(operation, location) {
            Assert.NotNull(left, right);
            Debug.Assert(!(left is CompoundLeftValue));

            _left = left;
            _right = right;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            // TODO: 
            // {target}[{arguments}] op= rhs
            // we need to evaluate {arguments} once: http://ironruby.codeplex.com/workitem/4525

            // first, read target into a temp:
            MSA.Expression transformedLeftTarget = _left.TransformTargetRead(gen);
            MSA.Expression leftTargetTemp;

            if (transformedLeftTarget != null) {
                leftTargetTemp = gen.CurrentScope.DefineHiddenVariable(String.Empty, transformedLeftTarget.Type);
            } else {
                leftTargetTemp = null;
            }

            MSA.Expression transformedRight = _right.TransformRead(gen);

            // lhs &&= rhs  -->  lhs && (lhs = rhs)
            // lhs ||= rhs  -->  lhs || (lhs = rhs)
            if (Operation == Symbols.And || Operation == Symbols.Or) {
                MSA.Expression transformedLeftRead = _left.TransformRead(gen,
                    (transformedLeftTarget != null) ? Ast.Assign(leftTargetTemp, transformedLeftTarget) : null,
                    true // tryRead
                );

                MSA.Expression transformedWrite = _left.TransformWrite(gen, leftTargetTemp, transformedRight);

                if (Operation == Symbols.And) {
                    return AndExpression.TransformRead(gen, transformedLeftRead, transformedWrite);
                } else {
                    return OrExpression.TransformRead(gen, transformedLeftRead, transformedWrite);
                }
            } else {
                // lhs op= rhs  -->  lhs = lhs op rhs
                if (Operation != null) {
                    MSA.Expression transformedLeftRead = _left.TransformRead(gen, leftTargetTemp, false);
                    transformedRight = MethodCall.TransformRead(this, gen, false, Operation, transformedLeftRead, null, null, transformedRight, null);
                }

                // transform lhs write assigning lhs-target temp:
                return _left.TransformWrite(gen,
                    (transformedLeftTarget != null) ? Ast.Assign(leftTargetTemp, transformedLeftTarget) : null,
                    transformedRight
                );
            }
        }

        internal override string GetNodeName(AstGenerator gen) {
            if (Operation == Symbols.And || Operation == Symbols.Or) {
                return "expression";
            } else {
                return base.GetNodeName(gen);
            }
        }
    }
}
