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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Dynamic;

namespace IronPython.Compiler {
    /// <summary>
    /// Provides a wrapper around "dynamic" expressions which we've opened coded (for optimized code generation).
    /// 
    /// This lets us recognize both normal Dynamic and our own Dynamic expressions and apply the combo binder on them.
    /// </summary>
    class ReducableDynamicExpression  : Expression {
        private readonly Expression/*!*/ _reduction;
        private readonly DynamicMetaObjectBinder/*!*/ _binder;
        private readonly Expression/*!*/[] _args;

        public ReducableDynamicExpression(Expression/*!*/ reduction, DynamicMetaObjectBinder/*!*/ binder, Expression/*!*/[]/*!*/ args) {
            _reduction = reduction;
            _binder = binder;
            _args = args;
        }

        public DynamicMetaObjectBinder/*!*/ Binder {
            get {
                return _binder;
            }
        }

        public Expression/*!*/[]/*!*/ Args {
            get {
                return _args;
            }
        }

        public override bool CanReduce {
            get {
                return true;
            }
        }

        protected override ExpressionType NodeTypeImpl() {
            return ExpressionType.Extension;
        }

        protected override Type TypeImpl() {
            return _reduction.Type;
        }

        public override Expression Reduce() {
            return _reduction;
        }
    }
}
