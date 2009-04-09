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
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Compiler;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    public sealed class MetaObjectBuilder {
        // RubyContext the site binder is bound to or null if it is unbound.
        private readonly RubyContext _siteContext;

        private Expression _condition;
        private BindingRestrictions/*!*/ _restrictions;
        private Expression _result;
        private List<ParameterExpression> _temps;
        private bool _error;
        private bool _treatRestrictionsAsConditions;

        internal MetaObjectBuilder(RubyMetaBinder/*!*/ rubyBinder, DynamicMetaObject/*!*/[]/*!*/ arguments) 
            : this((DynamicMetaObject)null, arguments) {
            _siteContext = rubyBinder.Context;
        }

        internal MetaObjectBuilder(DynamicMetaObject target, params DynamicMetaObject/*!*/[]/*!*/ arguments) {
            var restrictions = BindingRestrictions.Combine(arguments);
            if (target != null) {
                restrictions = target.Restrictions.Merge(restrictions);
            }

            _restrictions = restrictions;
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

#if DEBUG && !SILVERLIGHT && !SYSTEM_CORE
        private static int _ruleCounter;
#endif

        internal DynamicMetaObject/*!*/ CreateMetaObject(RubyMetaBinder/*!*/ action) {
            return CreateMetaObject(action, action.ResultType);
        }

        internal DynamicMetaObject/*!*/ CreateMetaObject(ConvertBinder/*!*/ action) {
            return CreateMetaObject(action, action.Type);
        }

        internal DynamicMetaObject/*!*/ CreateMetaObject(DynamicMetaObjectBinder/*!*/ action) {
            return CreateMetaObject(action, typeof(object));
        }

        private DynamicMetaObject/*!*/ CreateMetaObject(DynamicMetaObjectBinder/*!*/ action, Type/*!*/ returnType) {
            Debug.Assert(ControlFlowBuilder == null, "Control flow required but not built");

            var expr = _error ? Ast.Throw(_result, returnType) : AstUtils.Convert(_result, returnType);

            if (_condition != null) {
                var deferral = action.GetUpdateExpression(returnType);
                expr = Ast.Condition(_condition, expr, deferral);
            }

            if (_temps != null) {
                expr = Ast.Block(_temps, expr);
            }

#if DEBUG && !SILVERLIGHT && !SYSTEM_CORE
            if (RubyOptions.ShowRules) {
                var oldColor = Console.ForegroundColor;
                try {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Rule #{0}: {1}", Interlocked.Increment(ref _ruleCounter), action);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    var d = (_restrictions != BindingRestrictions.Empty) ? Ast.IfThen(_restrictions.ToExpression(), expr) : expr;
                    d.DumpExpression(null, Console.Out);
                } finally {
                    Console.ForegroundColor = oldColor;
                }
            }
#endif

            return new DynamicMetaObject(expr, _restrictions);
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

        #region Result

        public void SetError(Expression/*!*/ expression) {
            Assert.NotNull(expression);
            Debug.Assert(!_error, "Error already set");

            _result = expression;
            _error = true;
        }

        public void SetWrongNumberOfArgumentsError(int actual, int expected) {
            SetError(Methods.MakeWrongNumberOfArgumentsError.OpCall(AstUtils.Constant(actual), AstUtils.Constant(expected)));
        }

        public void SetMetaResult(DynamicMetaObject/*!*/ metaResult, CallArguments/*!*/ args) {
            // TODO: 
            // Should NormalizeArguments return a struct that provides us an information whether to treat particular argument's restrictions as conditions?
            // The splatted array is stored in a local. Therefore we cannot apply restrictions on it.
            SetMetaResult(metaResult, args.SimpleArgumentCount == 0 && args.Signature.HasSplattedArgument);
        }

        public void SetMetaResult(DynamicMetaObject/*!*/ metaResult, bool treatRestrictionsAsConditions) {
            _result = metaResult.Expression;
            if (treatRestrictionsAsConditions || _treatRestrictionsAsConditions) {
                AddCondition(metaResult.Restrictions.ToExpression());
            } else {
                Add(metaResult.Restrictions);
            }
        }

        #endregion

        #region Restrictions

        public void AddObjectTypeRestriction(object value, Expression/*!*/ expression) {
            if (value == null) {
                AddRestriction(Ast.Equal(expression, AstUtils.Constant(null)));
            } else {
                AddTypeRestriction(value.GetType(), expression);
            }
        }

        public void AddTypeRestriction(Type/*!*/ type, Expression/*!*/ expression) {
            if (_treatRestrictionsAsConditions) {
                AddCondition(Ast.TypeEqual(expression, type));
            } else if (expression.Type != type || !type.IsSealed) {
                Add(BindingRestrictions.GetTypeRestriction(expression, type));
            }
        }

        public void AddRestriction(Expression/*!*/ restriction) {
            if (_treatRestrictionsAsConditions) {
                AddCondition(restriction);
            } else {
                Add(BindingRestrictions.GetExpressionRestriction(restriction));
            }
        }

        private void Add(BindingRestrictions/*!*/ restriction) {
            Debug.Assert(!_treatRestrictionsAsConditions);
            _restrictions = _restrictions.Merge(restriction);
        }

        #endregion

        #region Conditions

        public void AddCondition(Expression/*!*/ condition) {
            Assert.NotNull(condition);
            _condition = (_condition != null) ? Ast.AndAlso(_condition, condition) : condition;
        }

        public static Expression/*!*/ GetObjectTypeTestExpression(object value, Expression/*!*/ expression) {
            if (value == null) {
                return Ast.Equal(expression, AstUtils.Constant(null));
            } else {
                return RuleBuilder.MakeTypeTestExpression(value.GetType(), expression);
            }
        }

        public void AddObjectTypeCondition(object value, Expression/*!*/ expression) {
            AddCondition(GetObjectTypeTestExpression(value, expression));
        }

        #endregion

        #region Tests

        public void AddTargetTypeTest(object target, RubyClass/*!*/ targetClass, Expression/*!*/ targetParameter, DynamicMetaObject/*!*/ metaContext) {

            // no changes to the module's class hierarchy while building the test:
            targetClass.Context.RequiresClassHierarchyLock();

            // initialization changes the version number, so ensure that the module is initialized:
            targetClass.InitializeMethodsNoLock();

            var context = (RubyContext)metaContext.Value;

            // singleton nil:
            if (target == null) {
                AddRestriction(Ast.Equal(targetParameter, AstUtils.Constant(null)));
                AddFullVersionTest(context.NilClass, metaContext);
                return;
            }

            // singletons true, false:
            if (target is bool) {
                AddRestriction(Ast.AndAlso(
                    Ast.TypeIs(targetParameter, typeof(bool)),
                    Ast.Equal(Ast.Convert(targetParameter, typeof(bool)), AstUtils.Constant(target))
                ));

                if ((bool)target) {
                    AddFullVersionTest(context.TrueClass, metaContext);
                } else {
                    AddFullVersionTest(context.FalseClass, metaContext);
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
                AddFullVersionTest(targetClass, metaContext);
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
                AddFullVersionTest(targetClass, metaContext);
            }
        }

        private void AddFullVersionTest(RubyClass/*!*/ cls, DynamicMetaObject/*!*/ metaContext) {
            Assert.NotNull(cls, metaContext);
            cls.Context.RequiresClassHierarchyLock();

            // check for runtime (note that the module's runtime could be different from the call-site runtime):
            if (_siteContext == null) {
                // TODO: use holder
                AddRestriction(Ast.Equal(metaContext.Expression, AstUtils.Constant(metaContext.Value)));
            } else if (_siteContext != metaContext.Value) {
                throw new InvalidOperationException("Runtime-bound site called from a different runtime");
            }
             
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

        #endregion
    }
}
