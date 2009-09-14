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
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Dynamic;

using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions.Calls;

namespace IronPython.Runtime.Binding {
    class WarningInfo {
        private readonly string/*!*/ _message;
        private readonly PythonType/*!*/ _type;
        private readonly Expression _condition;
        private readonly Func<bool> _conditionDelegate;

        public WarningInfo(PythonType/*!*/ type, string/*!*/ message) {
            _message = message;
            _type = type;
        }

        public WarningInfo(PythonType/*!*/ type, string/*!*/ message, Expression condition, Func<bool> conditionDelegate) {
            _message = message;
            _type = type;
            _condition = condition;
            _conditionDelegate = conditionDelegate;
        }

        public DynamicMetaObject/*!*/ AddWarning(Expression/*!*/ codeContext, DynamicMetaObject/*!*/ result) {
            Expression warn = Expression.Call(
                typeof(PythonOps).GetMethod("Warn"),
                codeContext,
                AstUtils.Constant(_type),
                AstUtils.Constant(_message),
                AstUtils.Constant(ArrayUtils.EmptyObjects)
            );

            if (_condition != null) {
                warn = Expression.Condition(_condition, warn, AstUtils.Empty());
            }

            return new DynamicMetaObject(
                    Expression.Block(
                    warn,
                    result.Expression
                ),
                result.Restrictions
            );
        }

        public OptimizingCallDelegate/*!*/ AddWarning(OptimizingCallDelegate/*!*/ result) {
            if(_conditionDelegate != null) {
                return delegate(object[] callArgs, out bool shouldOptimize) { 
                    if (_conditionDelegate()) {
                        PythonOps.Warn((CodeContext)callArgs[0], _type, _message, ArrayUtils.EmptyObjects);
                    }
                    return result(callArgs, out shouldOptimize);
                };
            }

            return delegate(object[] callArgs, out bool shouldOptimize) { 
                PythonOps.Warn((CodeContext)callArgs[0], _type, _message, ArrayUtils.EmptyObjects);
                return result(callArgs, out shouldOptimize);
            };
        }
    }
}
