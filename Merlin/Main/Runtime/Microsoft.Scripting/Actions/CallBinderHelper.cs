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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// Creates rules for performing method calls.  Currently supports calling built-in functions, built-in method descriptors (w/o 
    /// a bound value) and bound built-in method descriptors (w/ a bound value), delegates, types defining a "Call" method marked
    /// with SpecialName.
    /// </summary>
    /// <typeparam name="TAction">The specific type of CallAction</typeparam>
    public class CallBinderHelper<TAction> : BinderHelper<TAction>
        where TAction : OldCallAction {

        private object[] _args;                                     // the arguments the binder is binding to - args[0] is the target, args[1..n] are args to the target
        private Expression _instance;                               // the instance or null if this is a non-instance call
        private Type _instanceType;                                 // the type of _instance, to override _instance.Type when doing private binding.
        private Expression _test;                                   // the test expression, built up and assigned at the end
        private readonly RuleBuilder _rule;                         // the rule we end up producing
        private readonly bool _reversedOperator;                    // if we're producing a binary operator or a reversed operator (should go away, Python specific).
        private readonly MethodBase[] _targets;
        private readonly NarrowingLevel _maxLevel;                  // the maximum narrowing level allowed

        public CallBinderHelper(CodeContext context, TAction action, object[] args, RuleBuilder rule)
            : base(context, action) {
            ContractUtils.RequiresNotEmpty(args, "args");

            _maxLevel = NarrowingLevel.All;
            _args = RemoveExplicitInstanceArgument(action, args);
            _rule = rule;
            _test = _rule.MakeTypeTest(CompilerHelpers.GetType(Callable), 0);
        }

        public CallBinderHelper(CodeContext context, TAction action, object[] args, RuleBuilder rule, IList<MethodBase> targets)
            : this(context, action, args, rule) {
            _targets = ArrayUtils.ToArray(targets);
            _maxLevel = NarrowingLevel.All;
        }

        public CallBinderHelper(CodeContext context, TAction action, object[] args, RuleBuilder rule, IList<MethodBase> targets, NarrowingLevel maxLevel, bool isReversedOperator)
            : this(context, action, args, rule) {
            _targets = ArrayUtils.ToArray(targets);
            _reversedOperator = isReversedOperator;
            _maxLevel = maxLevel;
        }

        public virtual void MakeRule() {
            Type t = CompilerHelpers.GetType(Callable);

            MethodBase[] targets = GetTargetMethods();
            if (targets != null && targets.Length > 0) {
                // we're calling a well-known MethodBase
                MakeMethodBaseRule(targets);
            } else {
                // we can't call this object
                MakeCannotCallRule(t);
            }

            // if we produced an ActionOnCall rule we don't replace the test w/ our own.
            if (_rule.Test == null) {
                _rule.Test = _test;
            }
        }

        #region Method Call Rule

        private void MakeMethodBaseRule(MethodBase[] targets) {
            Type[] argTypes; // will not include implicit instance argument (if any)
            string[] argNames; // will include ArgumentKind.Dictionary keyword names


            GetArgumentNamesAndTypes(out argNames, out argTypes);

            Type[] bindingArgs = argTypes; // will include instance argument (if any)
            CallTypes callType = CallTypes.None;
            if (_instance != null) {
                bindingArgs = ArrayUtils.Insert(InstanceType, argTypes);
                callType = CallTypes.ImplicitInstance;
            }

            if (_reversedOperator && bindingArgs.Length >= 2) {
                // we swap the arguments before binding, and swap back before calling.
                ArrayUtils.SwapLastTwo(bindingArgs);
                if (argNames.Length >= 2) {
                    ArrayUtils.SwapLastTwo(argNames);
                }
            }

            // attempt to bind to an individual method
            MethodBinder binder = MethodBinder.MakeBinder(Binder, GetTargetName(targets), targets, argNames, NarrowingLevel.None, _maxLevel);
            BindingTarget bt = binder.MakeBindingTarget(callType, bindingArgs);

            if (bt.Success) {
                // if we succeed make the target for the rule
                MethodBase target = bt.Method;
                MethodInfo targetMethod = target as MethodInfo;

                if (targetMethod != null) {
                    target = CompilerHelpers.GetCallableMethod(targetMethod, Binder.PrivateBinding);
                }

                Expression[] exprargs = FinishTestForCandidate(bt.ArgumentTests, argTypes);

                _rule.Target = _rule.MakeReturn(
                    Binder,
                    bt.MakeExpression(_rule, exprargs));
            } else {
                // make an error rule
                MakeInvalidParametersRule(bt);
            }
        }

        private static object[] RemoveExplicitInstanceArgument(TAction action, object[] args) {
            //If an instance is explicitly passed in as an argument, ignore it.
            //Calls that need an instance will pick it up from the bound objects 
            //passed in or the rule. CallType can differentiate between the type 
            //of call during method binding.
            int instanceIndex = action.Signature.IndexOf(ArgumentType.Instance);
            if (instanceIndex > -1) {
                args = ArrayUtils.RemoveAt(args, instanceIndex + 1);
            }
            return args;
        }

        private static string GetTargetName(MethodBase[] targets) {
            return targets[0].IsConstructor ? targets[0].DeclaringType.Name : targets[0].Name;
        }

        protected Expression[] FinishTestForCandidate(IList<Type> testTypes, Type[] explicitArgTypes) {
            Expression[] exprArgs = MakeArgumentExpressions();
            Debug.Assert(exprArgs.Length == (explicitArgTypes.Length + ((_instance == null) ? 0 : 1)));
            Debug.Assert(testTypes == null || exprArgs.Length == testTypes.Count);

            MakeSplatTests();

            if (_reversedOperator) {
                ArrayUtils.SwapLastTwo(exprArgs);
            }

            if (explicitArgTypes.Length > 0 && testTypes != null) {
                // We've already tested the instance, no need to test it again. So remove it before adding 
                // rules for the arguments
                Expression[] exprArgsWithoutInstance = exprArgs;
                List<Type> testTypesWithoutInstance = new List<Type>(testTypes);
                for (int i = 0; i < exprArgs.Length; i++) {
                    if (exprArgs[i] == _instance) {
                        // We found the instance, so remove it
                        exprArgsWithoutInstance = ArrayUtils.RemoveAt(exprArgs, i);
                        testTypesWithoutInstance.RemoveAt(i);
                        break;
                    }
                }

                _test = Ast.AndAlso(_test, MakeNecessaryTests(_rule, testTypesWithoutInstance.ToArray(), exprArgsWithoutInstance));
            }

            return exprArgs;
        }

        /// <summary>
        /// Gets expressions to access all the arguments. This includes the instance argument. Splat arguments are
        /// unpacked in the output. The resulting array is similar to Rule.Parameters (but also different in some ways)
        /// </summary>
        protected Expression[] MakeArgumentExpressions() {
            List<Expression> exprargs = new List<Expression>();
            if (_instance != null) {
                exprargs.Add(_instance);
            }

            for (int i = 0; i < Action.Signature.ArgumentCount; i++) { // ArgumentCount(Action, _rule)
                switch (Action.Signature.GetArgumentKind(i)) {
                    case ArgumentType.Simple:
                    case ArgumentType.Named:
                        exprargs.Add(_rule.Parameters[i + 1]);
                        break;

                    case ArgumentType.List:
                        IList<object> list = (IList<object>)_args[i + 1];
                        for (int j = 0; j < list.Count; j++) {
                            exprargs.Add(
                                Ast.Call(
                                    Ast.Convert(
                                        _rule.Parameters[i + 1],
                                        typeof(IList<object>)
                                    ),
                                    typeof(IList<object>).GetMethod("get_Item"),
                                    Ast.Constant(j)
                                )
                            );
                        }
                        break;

                    case ArgumentType.Dictionary:
                        IDictionary dict = (IDictionary)_args[i + 1];

                        IDictionaryEnumerator dictEnum = dict.GetEnumerator();
                        while (dictEnum.MoveNext()) {
                            DictionaryEntry de = dictEnum.Entry;

                            string strKey = de.Key as string;
                            if (strKey == null) continue;

                            Expression dictExpr = _rule.Parameters[_rule.Parameters.Count - 1];
                            exprargs.Add(
                                Ast.Call(
                                    AstUtils.Convert(dictExpr, typeof(IDictionary)),
                                    typeof(IDictionary).GetMethod("get_Item"),
                                    Ast.Constant(strKey)
                                )
                            );
                        }
                        break;
                }
            }
            return exprargs.ToArray();
        }

        #endregion

        #region Target acquisition

        protected virtual MethodBase[] GetTargetMethods() {
            if (_targets != null) return _targets;

            object target = Callable;
            MethodBase[] targets;
            Delegate d;
            MemberGroup mg;
            MethodGroup mthgrp;
            BoundMemberTracker bmt;

            if ((d = target as Delegate) != null) {
                targets = GetDelegateTargets(d);
            } else if ((mg = target as MemberGroup) != null) {
                List<MethodInfo> foundTargets = new List<MethodInfo>();
                foreach (MemberTracker mt in mg) {
                    if (mt.MemberType == TrackerTypes.Method) {
                        foundTargets.Add(((MethodTracker)mt).Method);
                    }
                }
                targets = foundTargets.ToArray();
            } else if ((mthgrp = target as MethodGroup) != null) {
                _test = Ast.AndAlso(_test, Ast.Equal(AstUtils.Convert(Rule.Parameters[0], typeof(object)), Ast.Constant(target)));

                List<MethodBase> foundTargets = new List<MethodBase>();
                foreach (MethodTracker mt in mthgrp.Methods) {
                    foundTargets.Add(mt.Method);
                }

                targets = foundTargets.ToArray();
            } else if ((bmt = target as BoundMemberTracker) != null) {
                targets = GetBoundMemberTargets(bmt);
            } else {
                targets = GetOperatorTargets(target);
            }

            return targets;
        }

        private MethodBase[] GetBoundMemberTargets(BoundMemberTracker bmt) {
            Debug.Assert(bmt.Instance == null); // should be null for trackers that leak to user code

            MethodBase[] targets;
            _instance = AstUtils.Convert(
                Ast.Property(
                    Ast.Convert(Rule.Parameters[0], typeof(BoundMemberTracker)),
                    typeof(BoundMemberTracker).GetProperty("ObjectInstance")
                ),
                bmt.BoundTo.DeclaringType
            );
            _test = Ast.AndAlso(
                _test,
                Ast.Equal(
                    Ast.Property(
                        Ast.Convert(Rule.Parameters[0], typeof(BoundMemberTracker)),
                        typeof(BoundMemberTracker).GetProperty("BoundTo")
                    ),
                    Ast.Constant(bmt.BoundTo)
                )
            );
            _test = Ast.AndAlso(
                _test,
                Rule.MakeTypeTest(
                    CompilerHelpers.GetType(bmt.ObjectInstance),
                    Ast.Property(
                        Ast.Convert(Rule.Parameters[0], typeof(BoundMemberTracker)),
                        typeof(BoundMemberTracker).GetProperty("ObjectInstance")
                    )
                )
            );
            switch (bmt.BoundTo.MemberType) {
                case TrackerTypes.MethodGroup:
                    targets = ((MethodGroup)bmt.BoundTo).GetMethodBases();
                    break;
                case TrackerTypes.Method:
                    targets = new MethodBase[] { ((MethodTracker)bmt.BoundTo).Method };
                    break;
                default:
                    throw new InvalidOperationException(); // nothing else binds yet
            }
            return targets;
        }

        private MethodBase[] GetDelegateTargets(Delegate d) {
            _instance = AstUtils.Convert(_rule.Parameters[0], d.GetType());
            return new MethodBase[] { d.GetType().GetMethod("Invoke") };
        }

        private MethodBase[] GetOperatorTargets(object target) {
            MethodBase[] targets = null;

            // see if the type defines a well known Call method
            Type targetType = CompilerHelpers.GetType(target);



            MemberGroup callMembers = Binder.GetMember(Action, targetType, "Call");
            List<MethodBase> callTargets = new List<MethodBase>();
            foreach (MemberTracker mi in callMembers) {
                if (mi.MemberType == TrackerTypes.Method) {
                    MethodInfo method = ((MethodTracker)mi).Method;
                    if (method.IsSpecialName) {
                        callTargets.Add(method);
                    }
                }
            }
            if (callTargets.Count > 0) {
                targets = callTargets.ToArray();
                _instance = Ast.Convert(_rule.Parameters[0], CompilerHelpers.GetType(Callable));
            }

            return targets;
        }

        #endregion

        #region Test support

        /// <summary>
        /// Makes test for param arrays and param dictionary parameters.
        /// </summary>
        protected void MakeSplatTests() {
            if (Action.Signature.HasListArgument()) {
                MakeParamsArrayTest();
            }

            if (Action.Signature.HasDictionaryArgument()) {
                MakeParamsDictionaryTest();
            }
        }

        private void MakeParamsArrayTest() {
            int listIndex = Action.Signature.IndexOf(ArgumentType.List);
            Debug.Assert(listIndex != -1);
            _test = Ast.AndAlso(_test, MakeParamsTest(_args[listIndex + 1], _rule.Parameters[listIndex + 1]));
        }

        private void MakeParamsDictionaryTest() {
            IDictionary dict = (IDictionary)_args[_args.Length - 1];
            IDictionaryEnumerator dictEnum = dict.GetEnumerator();

            // verify the dictionary has the same count and arguments.

            string[] names = new string[dict.Count];
            int index = 0;
            while (dictEnum.MoveNext()) {
                string name = dictEnum.Entry.Key as string;
                if (name == null) {
                    throw new ArgumentTypeException(String.Format("expected string for dictionary argument got {0}", dictEnum.Entry.Key));
                }
                names[index++] = name;
            }

            _test = Ast.AndAlso(
                _test,
                Ast.AndAlso(
                    Ast.TypeIs(_rule.Parameters[_rule.Parameters.Count - 1], typeof(IDictionary)),
                    Ast.Call(
                        typeof(ScriptingRuntimeHelpers).GetMethod("CheckDictionaryMembers"),
                        Ast.Convert(_rule.Parameters[_rule.Parameters.Count - 1], typeof(IDictionary)),
                        Ast.Constant(names)
                    )
                )
            );
        }

        #endregion

        #region Error support

        protected virtual void MakeCannotCallRule(Type type) {
            _rule.Target =
                _rule.MakeError(
                    Ast.New(
                        typeof(ArgumentTypeException).GetConstructor(new Type[] { typeof(string) }),
                        Ast.Constant(type.Name + " is not callable")
                    )
                );
        }

        private void MakeInvalidParametersRule(BindingTarget bt) {
            MakeSplatTests();

            if (_args.Length > 1) {
                // we do an exact type check on all of the arguments types for a failed call.
                Expression[] argExpr = MakeArgumentExpressions();
                string[] names;
                Type[] vals;
                GetArgumentNamesAndTypes(out names, out vals);
                if (_instance != null) {
                    // target type was added to test already
                    argExpr = ArrayUtils.RemoveFirst(argExpr);
                }

                _test = Ast.AndAlso(_test, MakeNecessaryTests(_rule, vals, argExpr));
            }

            _rule.Target = Binder.MakeInvalidParametersError(bt).MakeErrorForRule(_rule, Binder);
        }

        #endregion

        #region Misc. Helpers

        /// <summary>
        /// Gets all of the argument names and types. The instance argument is not included
        /// </summary>
        /// <param name="argNames">The names correspond to the end of argTypes.
        /// ArgumentKind.Dictionary is unpacked in the return value.
        /// This is set to an array of size 0 if there are no keyword arguments</param>
        /// <param name="argTypes">Non named arguments are returned at the beginning.
        /// ArgumentKind.List is unpacked in the return value. </param>
        protected void GetArgumentNamesAndTypes(out string[] argNames, out Type[] argTypes) {
            // Get names of named arguments
            argNames = Action.Signature.GetArgumentNames();

            argTypes = GetArgumentTypes(Action, _args);

            if (Action.Signature.HasDictionaryArgument()) {
                // need to get names from dictionary argument...
                GetDictionaryNamesAndTypes(ref argNames, ref argTypes);
            }
        }

        private void GetDictionaryNamesAndTypes(ref string[] argNames, ref Type[] argTypes) {
            Debug.Assert(Action.Signature.GetArgumentKind(Action.Signature.ArgumentCount - 1) == ArgumentType.Dictionary);

            List<string> names = new List<string>(argNames);
            List<Type> types = new List<Type>(argTypes);

            IDictionary dict = (IDictionary)_args[_args.Length - 1];
            IDictionaryEnumerator dictEnum = dict.GetEnumerator();
            while (dictEnum.MoveNext()) {
                DictionaryEntry de = dictEnum.Entry;

                if (de.Key is string) {
                    names.Add((string)de.Key);
                    types.Add(CompilerHelpers.GetType(de.Value));
                }
            }

            argNames = names.ToArray();
            argTypes = types.ToArray();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")] // TODO: fix
        protected object[] Arguments {
            get {
                return _args;
            }
        }

        protected object[] GetExplicitArguments() {
            return ArrayUtils.RemoveFirst(_args);
        }

        protected object Callable {
            get {
                return _args[0];
            }
        }

        public RuleBuilder Rule {
            get {
                return _rule;
            }
        }

        /// <summary>
        /// The instance for the target method, or null if this is a non-instance call.
        /// 
        /// If it is set, it will typically be set to extract the instance from the Callable.
        /// </summary>
        public Expression Instance {
            get {
                return _instance;
            }
            set {
                Debug.Assert(!Action.Signature.HasInstanceArgument());
                _instance = value;
            }
        }

        public Type InstanceType {
            get {
                if (_instanceType != null) {
                    return _instanceType;
                }
                if (_instance != null) {
                    return _instance.Type;
                }
                return null;
            }
            set {
                _instanceType = value;
            }
        }

        protected Expression Test {
            get {
                return _test;
            }
            set {
                _test = value;
            }
        }
        #endregion
    }
}
