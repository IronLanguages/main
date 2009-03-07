/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using System.Collections.Generic;
using System.Dynamic;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

using Ast = System.Linq.Expressions.Expression;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Calls {
    public sealed class MetaObjectBuilder {
        private Expression _condition;
        private Expression _restriction;
        private Expression _result;
        private List<ParameterExpression> _temps;
        private bool _error;
        private bool _treatRestrictionsAsConditions;

        internal MetaObjectBuilder() {
        }

        public bool Error {
            get { return _error; }
        }

        public Expression Result {
            get { return _result; }
            set { _result = value; }
        }

        public ParameterExpression BfcVariable { get; set; }

        /// <summary>
        /// A rule builder sets this up if the resulting rule is required to be wrapped in a non-local control flow handler.
        /// This delegate must be called exactly once (<see cref="BuildControlFlow"/> method).
        /// </summary>
        public Action<MetaObjectBuilder, CallArguments> ControlFlowBuilder { get; set; }

        public bool TreatRestrictionsAsConditions {
            get { return _treatRestrictionsAsConditions; }
            set { _treatRestrictionsAsConditions = value; }
        }

        internal DynamicMetaObject/*!*/ CreateMetaObject(DynamicMetaObjectBinder/*!*/ action) {
            Debug.Assert(ControlFlowBuilder == null, "Control flow required but not built");

            var expr = _error ? Ast.Throw(_result) : _result;

            if (_condition != null) {
                var deferral = action.GetUpdateExpression(typeof(object));
                expr = Ast.Condition(_condition, AstUtils.Convert(expr, typeof(object)), deferral);
            }

            if (_temps != null) {
                expr = Ast.Block(_temps, expr);
            }

            BindingRestrictions restrictions;
            if (_restriction != null) {
                restrictions = BindingRestrictions.GetExpressionRestriction(_restriction);
            } else {
                restrictions = BindingRestrictions.Empty;
            }

            return new DynamicMetaObject(expr, restrictions);
        }

        public void SetError(Expression/*!*/ expression) {
            Assert.NotNull(expression);
            Debug.Assert(!_error, "Error already set");

            _result = expression;
            _error = true;
        }

        public void SetWrongNumberOfArgumentsError(int actual, int expected) {
            SetError(Methods.MakeWrongNumberOfArgumentsError.OpCall(AstUtils.Constant(actual), AstUtils.Constant(expected)));
        }

        public void AddCondition(Expression/*!*/ condition) {
            Assert.NotNull(condition);
            _condition = (_condition != null) ? Ast.AndAlso(_condition, condition) : condition;
        }

        public void AddRestriction(Expression/*!*/ restriction) {
            Assert.NotNull(restriction);
            if (_treatRestrictionsAsConditions) {
                AddCondition(restriction);
            } else {
                _restriction = (_restriction != null) ? Ast.AndAlso(_restriction, restriction) : restriction;
            }
        }

        public static Expression/*!*/ GetObjectTypeTestExpression(object value, Expression/*!*/ expression) {
            if (value == null) {
                return Ast.Equal(expression, AstUtils.Constant(null));
            } else {
                return RuleBuilder.MakeTypeTestExpression(value.GetType(), expression);
            }
        }

        public void AddTypeRestriction(Type/*!*/ type, Expression/*!*/ expression) {
            // TODO: assertion failure in DLR:
            // AddRestriction(Ast.TypeEqual(expression, type));
            AddRestriction(RuleBuilder.MakeTypeTestExpression(type, expression));
        }

        public void AddObjectTypeRestriction(object value, Expression/*!*/ expression) {
            if (value == null) {
                AddRestriction(Ast.Equal(expression, AstUtils.Constant(null)));
            } else {
                AddTypeRestriction(value.GetType(), expression);
            }
        }

        public void AddObjectTypeCondition(object value, Expression/*!*/ expression) {
            AddCondition(GetObjectTypeTestExpression(value, expression));
        }

        // TODO: do not test runtime for runtime bound sites
        public void AddTargetTypeTest(object target, RubyClass/*!*/ targetClass, Expression/*!*/ targetParameter, 
            RubyContext/*!*/ context, Expression/*!*/ contextExpression) {

            // no changes to the module's class hierarchy while building the test:
            targetClass.Context.RequiresClassHierarchyLock();

            // initialization changes the version number, so ensure that the module is initialized:
            targetClass.InitializeMethodsNoLock(); 
            
            // singleton nil:
            if (target == null) {
                AddRestriction(Ast.Equal(targetParameter, AstUtils.Constant(null)));
                AddFullVersionTest(context.NilClass, context, contextExpression);
                return;
            }

            // singletons true, false:
            if (target is bool) {
                AddRestriction(Ast.AndAlso(
                    Ast.TypeIs(targetParameter, typeof(bool)),
                    Ast.Equal(Ast.Convert(targetParameter, typeof(bool)), AstUtils.Constant(target))
                ));

                if ((bool)target) {
                    AddFullVersionTest(context.TrueClass, context, contextExpression);
                } else {
                    AddFullVersionTest(context.FalseClass, context, contextExpression);
                }
                return;

            }

            // user defined instance singletons, modules, classes:
            if (targetClass.IsSingletonClass) {
                AddRestriction(
                    Ast.Equal(
                        Ast.Convert(targetParameter, typeof(object)),
                        Ast.Convert(AstUtils.Constant(target), typeof(object))
                    )
                );

                // we need to check for a runtime (e.g. "foo" .NET string instance could be shared accross runtimes):
                AddFullVersionTest(targetClass, context, contextExpression);
                return;
            }

            Type type = target.GetType();
            AddTypeRestriction(type, targetParameter);
            
            if (typeof(IRubyObject).IsAssignableFrom(type)) {
                // Ruby objects (get the method directly to prevent interface dispatch):
                MethodInfo classGetter = type.GetMethod("get_" + RubyObject.ClassPropertyName, BindingFlags.Public | BindingFlags.Instance);
                if (classGetter != null && classGetter.ReturnType == typeof(RubyClass)) {
                    AddCondition(
                        // (#{type})target.Class.Version.Value == #{immediateClass.Version}
                        Ast.Equal(
                            Ast.Field(
                                Ast.Field(
                                    Ast.Call(Ast.Convert(targetParameter, type), classGetter), 
                                    Fields.RubyClass_Version
                                ), 
                                Fields.StrongBox_Of_Int_Value
                            ),
                            AstUtils.Constant(targetClass.Version.Value)
                        )
                    );
                    return;
                }

                // TODO: explicit iface-implementation
                throw new NotSupportedException("Type implementing IRubyObject should have RubyClass getter");
            } else {
                // CLR objects:
                AddFullVersionTest(targetClass, context, contextExpression);
            }
        }

        private void AddFullVersionTest(RubyClass/*!*/ cls, RubyContext/*!*/ context, Expression/*!*/ contextExpression) {
            Assert.NotNull(cls, context, contextExpression);
            cls.Context.RequiresClassHierarchyLock();

            // check for runtime (note that the module's runtime could be different from the call-site runtime):
            AddRestriction(Ast.Equal(contextExpression, AstUtils.Constant(context)));

            AddVersionTest(cls);
        }

        internal void AddVersionTest(RubyClass/*!*/ cls) {
            cls.Context.RequiresClassHierarchyLock();

            // check for module version (do not burn a module reference to the rule):
            AddCondition(Ast.Equal(Ast.Field(AstUtils.Constant(cls.Version), Fields.StrongBox_Of_Int_Value), AstUtils.Constant(cls.Version.Value)));
        }

        internal bool AddSplattedArgumentTest(object value, Expression/*!*/ expression, out int listLength, out ParameterExpression/*!*/ listVariable) {
            if (value == null) {
                AddRestriction(Ast.Equal(expression, AstUtils.Constant(null)));
            } else {
                // test exact type:
                AddTypeRestriction(value.GetType(), expression);

                List<object> list = value as List<object>;
                if (list != null) {
                    Type type = typeof(List<object>);
                    listLength = list.Count;
                    listVariable = GetTemporary(type, "#list");
                    AddCondition(Ast.Equal(
                        Ast.Property(Ast.Assign(listVariable, Ast.Convert(expression, type)), type.GetProperty("Count")),
                        AstUtils.Constant(list.Count))
                    );
                    return true;
                }
            }

            listLength = -1;
            listVariable = null;
            return false;
        }

        public ParameterExpression/*!*/ GetTemporary(Type/*!*/ type, string/*!*/ name) {
            if (_temps == null) {
                _temps = new List<ParameterExpression>();
            }

            var variable = Ast.Variable(type, name);
            _temps.Add(variable);
            return variable;
        }

        public void BuildControlFlow(CallArguments/*!*/ args) {
            if (ControlFlowBuilder != null) {
                ControlFlowBuilder(this, args);
                ControlFlowBuilder = null;
            }
        }
    }
}
