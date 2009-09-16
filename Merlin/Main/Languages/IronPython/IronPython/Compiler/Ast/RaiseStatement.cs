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
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Compiler.Ast {
    using Ast = MSAst.Expression;

    public class RaiseStatement : Statement {
        private readonly Expression _type, _value, _traceback;

        public RaiseStatement(Expression exceptionType, Expression exceptionValue, Expression traceBack) {
            _type = exceptionType;
            _value = exceptionValue;
            _traceback = traceBack;
        }

        public Expression Type {
            get { return _type; }
        }

        public Expression Value {
            get { return _value; }
        }

        public Expression Traceback {
            get { return _traceback; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            MSAst.Expression raiseExpression;
            if (_type == null && _value == null && _traceback == null) {
                raiseExpression = Ast.Call(
                    AstGenerator.GetHelperMethod("MakeRethrownException"),
                    ag.LocalContext
                );

                if (!ag._isEmittingFinally) {
                    raiseExpression = Ast.Block(
                        ag.UpdateLineUpdated(true),
                        raiseExpression
                    );
                }
            } else {
                raiseExpression = Ast.Call(
                    AstGenerator.GetHelperMethod("MakeException"),
                    ag.LocalContext,
                    ag.TransformOrConstantNull(_type, typeof(object)),
                    ag.TransformOrConstantNull(_value, typeof(object)),
                    ag.TransformOrConstantNull(_traceback, typeof(object))
                );
            }
            return ag.AddDebugInfo(
                Ast.Throw(raiseExpression),
                Span
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_type != null) {
                    _type.Walk(walker);
                }
                if (_value != null) {
                    _value.Walk(walker);
                }
                if (_traceback != null) {
                    _traceback.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
