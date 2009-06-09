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
using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic;
using Microsoft.Contracts;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// Rule Builder
    /// 
    /// A rule is the mechanism that LanguageBinders use to specify both what code to execute (the Target)
    /// for a particular action on a particular set of objects, but also a Test that guards the Target.
    /// Whenver the Test returns true, it is assumed that the Target will be the correct action to
    /// take on the arguments.
    /// 
    /// In the current design, a RuleBuilder is also used to provide a mini binding scope for the
    /// parameters and temporary variables that might be needed by the Test and Target.  This will
    /// probably change in the future as we unify around the notion of Lambdas.
    /// 
    /// TODO: remove once everyone is converted over to MetaObjects
    /// </summary>
    public sealed class RuleBuilder {
        internal Expression _test;                  // the test that determines if the rule is applicable for its parameters
        internal Expression _target;                // the target that executes if the rule is true
        internal readonly Expression _context;               // CodeContext, if any.
        private bool _error;                        // true if the rule represents an error
        internal List<ParameterExpression> _temps;  // temporaries allocated by the rule

        // the parameters which the rule is processing
        internal readonly IList<Expression> _parameters;
        
        // the return label of the rule
        internal readonly LabelTarget _return;

        /// <summary>
        /// Completed rule
        /// </summary>
        private Expression _binding;

        public RuleBuilder(ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {

            if (parameters.Count > 0 && typeof(CodeContext).IsAssignableFrom(parameters[0].Type)) {
                _context = parameters[0];
                var p = ArrayUtils.RemoveAt(parameters, 0);
                _parameters = p;
            } else {
                // TODO: remove the copy when we have covariant IEnumerable<T>
                _parameters = parameters.ToArray();
            }
            _return = returnLabel;
        }

        public void Clear() {
            _test = null;
            _target = null;
            _error = false;
            _temps = null;
        }

        /// <summary>
        /// An expression that should return true if and only if Target should be executed
        /// </summary>
        public Expression Test {
            get { return _test; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                ContractUtils.Requires(TypeUtils.IsBool(value.Type), "value", Strings.TypeOfTestMustBeBool);
                _test = value;
            }
        }

        /// <summary>
        /// The code to execute if the Test is true.
        /// </summary>
        public Expression Target {
            get { return _target; }
            set { _target = value; }
        }

        public Expression Context {
            get {
                return _context;
            }
        }

        /// <summary>
        /// Gets the logical parameters to the dynamic site in the form of Expressions.
        /// </summary>
        public IList<Expression> Parameters {
            get {
                return _parameters;
            }
        }

        public LabelTarget ReturnLabel {
            get { return _return; }
        }

        /// <summary>
        /// Allocates a temporary variable for use during the rule.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public ParameterExpression GetTemporary(Type type, string name) {
            ParameterExpression t = Expression.Variable(type, name);
            AddTemporary(t);
            return t;
        }

        public void AddTemporary(ParameterExpression variable) {
            ContractUtils.RequiresNotNull(variable, "variable");
            if (_temps == null) {
                _temps = new List<ParameterExpression>();
            }
            _temps.Add(variable);
        }

        public Expression MakeReturn(ActionBinder binder, Expression expr) {
            // we create a temporary here so that ConvertExpression doesn't need to (because it has no way to declare locals).
            if (expr.Type != typeof(void)) {
                ParameterExpression variable = GetTemporary(expr.Type, "$retVal");
                Expression conv = binder.ConvertExpression(variable, ReturnType, ConversionResultKind.ExplicitCast, Context);
                if (conv == variable) return MakeReturn(expr);

                return MakeReturn(Ast.Block(Ast.Assign(variable, expr), conv));
            }
            return MakeReturn(binder.ConvertExpression(expr, ReturnType, ConversionResultKind.ExplicitCast, Context));
        }

        private Expression MakeReturn(Expression expression) {
            return Ast.Return(_return, AstUtils.Convert(expression, _return.Type));
        }

        public Expression MakeError(Expression expr) {
            if (expr != null) {
                // TODO: Change to ConvertHelper
                if (!TypeUtils.CanAssign(typeof(Exception), expr.Type)) {
                    expr = Ast.Convert(expr, typeof(Exception));
                }
            }

            _error = true;
            return Ast.Throw(expr);
        }

        public bool IsError {
            get {
                return _error;
            }
            set {
                _error = value;
            }
        }

        public void AddTest(Expression expression) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.Requires(TypeUtils.IsBool(expression.Type), "expression", Strings.TypeOfExpressionMustBeBool);

            if (_test == null) {
                _test = expression;
            } else {
                _test = Ast.AndAlso(_test, expression);
            }
        }

        public void MakeTest(params Type[] types) {
            _test = MakeTestForTypes(types, 0);
        }

        public static Expression MakeTypeTestExpression(Type t, Expression expr) {
            // we must always check for non-sealed types explicitly - otherwise we end up
            // doing fast-path behavior on a subtype which overrides behavior that wasn't
            // present for the base type.
            //TODO there's a question about nulls here
            if (CompilerHelpers.IsSealed(t) && t == expr.Type) {
                if (t.IsValueType) {
                    return AstUtils.Constant(true);
                }
                return Ast.NotEqual(expr, AstUtils.Constant(null));
            }

            return Ast.AndAlso(
                Ast.NotEqual(
                    expr,
                    AstUtils.Constant(null)),
                Ast.Equal(
                    Ast.Call(
                        AstUtils.Convert(expr, typeof(object)),
                        typeof(object).GetMethod("GetType")
                    ),
                    AstUtils.Constant(t)
                )
            );
        }

        public Expression MakeTestForTypes(Type[] types, int index) {
            Expression test = MakeTypeTest(types[index], index);
            if (index < types.Length - 1) {
                Expression nextTests = MakeTestForTypes(types, index + 1);
                if (ConstantCheck.Check(test, true)) {
                    return nextTests;
                } else if (ConstantCheck.Check(nextTests, true)) {
                    return test;
                } else {
                    return Ast.AndAlso(test, nextTests);
                }
            } else {
                return test;
            }
        }

        public Expression MakeTypeTest(Type type, int index) {
            return MakeTypeTest(type, Parameters[index]);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public Expression MakeTypeTest(Type type, Expression tested) {
            if (type == null || type == typeof(DynamicNull)) {
                return Ast.Equal(tested, AstUtils.Constant(null));
            }

            return MakeTypeTestExpression(type, tested);
        }

        /// <summary>
        /// Gets the number of logical parameters the dynamic site is provided with.
        /// </summary>
        public int ParameterCount {
            get {
                return _parameters.Count;
            }
        }

        public Expression MakeTypeTestExpression(Type t, int param) {
            return MakeTypeTestExpression(t, Parameters[param]);
        }

        [Confined]
        public override string ToString() {
            return string.Format("RuleBuilder({0})", _target);
        }

        public Type ReturnType {
            get { return _return.Type; }
        }

        public Expression CreateRule() {
            if (_binding == null) {
                if (_test == null) {
                    throw Error.MissingTest();
                }
                if (_target == null) {
                    throw Error.MissingTarget();
                }

                _binding = Expression.Block(
                    _temps != null ? _temps.ToArray() : new ParameterExpression[0],
                    Expression.Condition(
                        _test,
                        AstUtils.Convert(_target, typeof(void)),
                        AstUtils.Empty()
                    )
                );
            }

            return _binding;
        }
    }
}
