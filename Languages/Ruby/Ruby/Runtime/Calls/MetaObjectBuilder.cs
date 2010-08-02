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
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;
    using AstExpressions = ReadOnlyCollectionBuilder<Expression>;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public sealed class MetaObjectBuilder {
        // RubyContext the site binder is bound to or null if it is unbound.
        private readonly RubyContext _siteContext;

        private Expression _condition;
        private BindingRestrictions/*!*/ _restrictions;
        private Expression _result;
        private AstExpressions _initializations;
        private List<ParameterExpression> _temps;
        private bool _error;
        private bool _treatRestrictionsAsConditions;

        internal MetaObjectBuilder(RubyMetaBinder/*!*/ binder, DynamicMetaObject/*!*/[]/*!*/ arguments)
            : this(binder.Context, (DynamicMetaObject)null, arguments) {
        }

        internal MetaObjectBuilder(IInteropBinder/*!*/ binder, DynamicMetaObject target, params DynamicMetaObject/*!*/[]/*!*/ arguments)
            : this(binder.Context, target, arguments) {
        }

        internal MetaObjectBuilder(DynamicMetaObject target, params DynamicMetaObject/*!*/[]/*!*/ arguments)
            : this((RubyContext)null, target, arguments) {
        }

        private MetaObjectBuilder(RubyContext siteContext, DynamicMetaObject target, DynamicMetaObject/*!*/[]/*!*/ arguments) {
            var restrictions = BindingRestrictions.Combine(arguments);
            if (target != null) {
                restrictions = target.Restrictions.Merge(restrictions);
            }

            _restrictions = restrictions;
            _siteContext = siteContext;
        }

        private void Clear() {
            _condition = null;
            _restrictions = BindingRestrictions.Empty;
            _result = null;
            _initializations = null;
            _error = false;
            _treatRestrictionsAsConditions = false;
        }

        public bool Error {
            get { return _error; }
        }

        public Expression Result {
            get { return _result; }
            set { 
                _result = value;
                _error = false;
            }
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
            return CreateMetaObject(action, action.ReturnType);
        }

        internal DynamicMetaObject/*!*/ CreateMetaObject(DynamicMetaObjectBinder/*!*/ binder, Type/*!*/ returnType) {
            Debug.Assert(ControlFlowBuilder == null, "Control flow required but not built");

            var restrictions = _restrictions;
            var expr = _error ? Ast.Throw(_result, returnType) : AstUtils.Convert(_result, returnType);

            if (_condition != null) {
                var deferral = binder.GetUpdateExpression(returnType);
                expr = Ast.Condition(_condition, expr, deferral);
            }

            if (_temps != null || _initializations != null) {
                AddInitialization(expr);
                if (_temps != null) {
                    expr = Ast.Block(_temps, _initializations);
                } else {
                    expr = Ast.Block(_initializations);
                }
            }

            Clear();
            RubyBinder.DumpRule(binder, restrictions, expr);
            return new DynamicMetaObject(expr, restrictions);
        }

        public ParameterExpression/*!*/ GetTemporary(Type/*!*/ type, string/*!*/ name) {
            return AddTemporary(Ast.Variable(type, name));
        }

        private ParameterExpression/*!*/ AddTemporary(ParameterExpression/*!*/ variable) {
            if (_temps == null) {
                _temps = new List<ParameterExpression>();
            }

            _temps.Add(variable);
            return variable;
        }

        internal void AddInitialization(Expression/*!*/ expression) {
            if (_initializations == null) {
                _initializations = new AstExpressions();
            }

            _initializations.Add(expression);
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
            SetMetaResult(metaResult, args.Signature.HasSplattedArgument);
        }

        public void SetMetaResult(DynamicMetaObject/*!*/ metaResult, bool treatRestrictionsAsConditions) {
            Result = metaResult.Expression;
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

        public void AddRestriction(BindingRestrictions/*!*/ restriction) {
            if (_treatRestrictionsAsConditions) {
                AddCondition(restriction.ToExpression());
            } else {
                Add(restriction);
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

        #endregion

        #region Tests

        public void AddTargetTypeTest(object target, RubyClass/*!*/ targetClass, Expression/*!*/ targetParameter, DynamicMetaObject/*!*/ metaContext,
            IEnumerable<string>/*!*/ resolvedNames) {

            // no changes to the module's class hierarchy while building the test:
            targetClass.Context.RequiresClassHierarchyLock();

            // initialization changes the version number, so ensure that the module is initialized:
            targetClass.InitializeMethodsNoLock();

            var context = (RubyContext)metaContext.Value;

            if (target is IRubyObject) {
                Type type = target.GetType();
                AddTypeRestriction(type, targetParameter);
            
                // Ruby objects (get the method directly to prevent interface dispatch):
                MethodInfo classGetter = type.GetMethod(Methods.IRubyObject_get_ImmediateClass.Name, BindingFlags.Public | BindingFlags.Instance);
                if (type.IsVisible && classGetter != null && classGetter.ReturnType == typeof(RubyClass)) {
                    AddCondition(
                        // (#{type})target.ImmediateClass.Version.Method == #{immediateClass.Version.Method}
                        Ast.Equal(
                            Ast.Field(
                                Ast.Field(
                                    Ast.Call(Ast.Convert(targetParameter, type), classGetter), 
                                    Fields.RubyModule_Version
                                ),
                                Fields.VersionHandle_Method
                            ),
                            AstUtils.Constant(targetClass.Version.Method)
                        )
                    );
                    return;
                }

                // TODO: explicit iface-implementation
                throw new NotSupportedException("Type implementing IRubyObject should be visible and have ImmediateClass getter");
            }

            AddRuntimeTest(metaContext);

            // singleton nil:
            if (target == null) {
                AddRestriction(Ast.Equal(targetParameter, AstUtils.Constant(null)));
                AddVersionTest(context.NilClass);
                return;
            }

            // singletons true, false:
            if (target is bool) {
                AddRestriction(Ast.AndAlso(
                    Ast.TypeIs(targetParameter, typeof(bool)),
                    Ast.Equal(Ast.Convert(targetParameter, typeof(bool)), AstUtils.Constant(target))
                ));

                AddVersionTest((bool)target ? context.TrueClass : context.FalseClass);
                return;
            }

            var nominalClass = targetClass.NominalClass;

            Debug.Assert(!nominalClass.IsSingletonClass);
            Debug.Assert(!nominalClass.IsRubyClass);

            // Do we need a singleton check?
            if (nominalClass.ClrSingletonMethods == null ||
                CollectionUtils.TrueForAll(resolvedNames, (methodName) => !nominalClass.ClrSingletonMethods.ContainsKey(methodName))) {

                // no: there is no singleton subclass of target class that defines any method being called:
                AddTypeRestriction(target.GetType(), targetParameter);
                AddVersionTest(targetClass);

            } else if (targetClass.IsSingletonClass) {

                // yes: check whether the incoming object is a singleton and the singleton has the right version:
                AddTypeRestriction(target.GetType(), targetParameter);
                AddCondition(Methods.IsClrSingletonRuleValid.OpCall(
                    metaContext.Expression,
                    targetParameter,
                    AstUtils.Constant(targetClass.Version.Method)
                ));

            } else {

                // yes: check whether the incoming object is NOT a singleton and the class has the right version:
                AddTypeRestriction(target.GetType(), targetParameter);
                AddCondition(Methods.IsClrNonSingletonRuleValid.OpCall(
                    metaContext.Expression, 
                    targetParameter,
                    Ast.Constant(targetClass.Version),
                    AstUtils.Constant(targetClass.Version.Method)
                ));
            }
        }

        private void AddRuntimeTest(DynamicMetaObject/*!*/ metaContext) {
            Assert.NotNull(metaContext);

            // check for runtime (note that the module's runtime could be different from the call-site runtime):
            if (_siteContext == null) {
                // TODO: use holder
                AddRestriction(Ast.Equal(metaContext.Expression, AstUtils.Constant(metaContext.Value)));
            } else if (_siteContext != metaContext.Value) {
                throw new InvalidOperationException("Runtime-bound site called from a different runtime");
            }
        }

        internal void AddVersionTest(RubyClass/*!*/ cls) {
            cls.Context.RequiresClassHierarchyLock();

            // check for module version (do not burn a module reference to the rule):
            AddCondition(Ast.Equal(Ast.Field(AstUtils.Constant(cls.Version), Fields.VersionHandle_Method), AstUtils.Constant(cls.Version.Method)));
        }

        internal void AddSplattedArgumentTest(IList/*!*/ value, Expression/*!*/ expression, out int listLength, out ParameterExpression/*!*/ listVariable) {
            Expression assignment;
            listVariable = expression as ParameterExpression;
            if (listVariable != null && typeof(IList).IsAssignableFrom(expression.Type)) {
                assignment = expression;
            } else {
                listVariable = GetTemporary(typeof(IList), "#list");
                assignment = Ast.Assign(listVariable, AstUtils.Convert(expression, typeof(IList)));
            }
            
            listLength = value.Count;
            AddCondition(Ast.Equal(Ast.Property(assignment, typeof(ICollection).GetProperty("Count")), AstUtils.Constant(value.Count)));
        }

        #endregion
    }
}
