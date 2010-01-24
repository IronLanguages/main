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

#if !CLR2
using System.Linq.Expressions;
#endif

using System.Collections.Generic;
using System.Dynamic;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler {
    internal sealed class PythonDynamicExpression1 : LightDynamicExpression1 {
        private readonly CompilationMode/*!*/ _mode;

        public PythonDynamicExpression1(DynamicMetaObjectBinder/*!*/ binder, CompilationMode/*!*/ mode, Expression/*!*/ arg0) 
            : base(binder, arg0) {
            _mode = mode;
        }

        public override Expression/*!*/ Reduce() {
            return _mode.Dynamic((DynamicMetaObjectBinder)Binder, Type, Argument0);
        }
    }

    internal sealed class PythonDynamicExpression2 : LightDynamicExpression2 {
        private readonly CompilationMode/*!*/ _mode;

        public PythonDynamicExpression2(DynamicMetaObjectBinder/*!*/ binder, CompilationMode/*!*/ mode, Expression/*!*/ arg0, Expression/*!*/ arg1)
            : base(binder, arg0, arg1) {
            _mode = mode;
        }

        public override Expression/*!*/ Reduce() {
            return _mode.Dynamic((DynamicMetaObjectBinder)Binder, Type, Argument0, Argument1);
        }
    }

    internal sealed class PythonDynamicExpression3 : LightDynamicExpression3 {
        private readonly CompilationMode/*!*/ _mode;

        public PythonDynamicExpression3(DynamicMetaObjectBinder/*!*/ binder, CompilationMode/*!*/ mode, Expression/*!*/ arg0, Expression/*!*/ arg1, Expression/*!*/ arg2)
            : base(binder, arg0, arg1, arg2) {
            _mode = mode;
        }

        public override Expression/*!*/ Reduce() {
            return _mode.Dynamic((DynamicMetaObjectBinder)Binder, Type, Argument0, Argument1, Argument2);
        }
    }

    internal sealed class PythonDynamicExpression4 : LightDynamicExpression4 {
        private readonly CompilationMode/*!*/ _mode;

        public PythonDynamicExpression4(DynamicMetaObjectBinder/*!*/ binder, CompilationMode/*!*/ mode, Expression/*!*/ arg0, Expression/*!*/ arg1, Expression/*!*/ arg2, Expression/*!*/ arg3)
            : base(binder, arg0, arg1, arg2, arg3) {
            _mode = mode;
        }

        public override Expression/*!*/ Reduce() {
            return _mode.Dynamic((DynamicMetaObjectBinder)Binder, Type, Argument0, Argument1, Argument2, Argument3);
        }
    }

    internal sealed class PythonDynamicExpressionN : LightTypedDynamicExpressionN {
        private readonly CompilationMode/*!*/ _mode;

        public PythonDynamicExpressionN(DynamicMetaObjectBinder/*!*/ binder, CompilationMode/*!*/ mode, IList<Expression>/*!*/ args)
            : base(binder, typeof(object), args) {
            _mode = mode;
        }

        public override Expression/*!*/ Reduce() {
            return _mode.Dynamic((DynamicMetaObjectBinder)Binder, Type, Arguments);
        }
    }
}
