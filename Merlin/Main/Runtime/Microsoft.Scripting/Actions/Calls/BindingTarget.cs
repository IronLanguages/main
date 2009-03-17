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
using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions.Calls {
    /// <summary>
    /// Encapsulates the result of an attempt to bind to one or methods using the MethodBinder.
    /// 
    /// Users should first check the Result property to see if the binding was successful or
    /// to determine the specific type of failure that occured.  If the binding was successful
    /// MakeExpression can then be called to create an expression which calls the method.
    /// If the binding was a failure callers can then create a custom error message based upon
    /// the reason the call failed.
    /// </summary>
    public sealed class BindingTarget {
        private readonly BindingResult _result;                                           // the result of the binding
        private readonly string _name;                                                    // the name of the method being bound to
        private readonly MethodTarget _target;                                            // the MethodTarget if the binding was successful 
        private readonly Type[] _argTests;                                                // Deprecated: if successful tests needed to disambiguate between overloads, MetaObject binding is preferred
        private readonly RestrictionInfo _restrictedArgs;                                    // the arguments after they've been restricted to their known types
        private readonly NarrowingLevel _level;                                           // the NarrowingLevel at which the target succeeds on conversion
        private readonly CallFailure[] _callFailures;                                     // if failed on conversion the various conversion failures for all overloads
        private readonly MethodTarget[] _ambiguousMatches;                                // list of methods which are ambiguous to bind to.
        private readonly int[] _expectedArgs;                                             // gets the acceptable number of parameters which can be passed to the method.
        private readonly int _actualArgs;                                                 // gets the actual number of arguments provided

        /// <summary>
        /// Creates a new BindingTarget when the method binding has succeeded.
        /// 
        /// OBSOLETE
        /// </summary>
        internal BindingTarget(string name, int actualArgumentCount, MethodTarget target, NarrowingLevel level, Type[] argTests) {
            _name = name;
            _target = target;
            _argTests = argTests;
            _level = level;
            _actualArgs = actualArgumentCount;
        }

        /// <summary>
        /// Creates a new BindingTarget when the method binding has succeeded.
        /// </summary>
        internal BindingTarget(string name, int actualArgumentCount, MethodTarget target, NarrowingLevel level, RestrictionInfo restrictedArgs) {
            _name = name;
            _target = target;
            _restrictedArgs = restrictedArgs;
            _level = level;
            _actualArgs = actualArgumentCount;
        }

        /// <summary>
        /// Creates a new BindingTarget when the method binding has failed due to an incorrect argument count
        /// </summary>
        internal BindingTarget(string name, int actualArgumentCount, int[] expectedArgCount) {
            _name = name;
            _result = BindingResult.IncorrectArgumentCount;
            _expectedArgs = expectedArgCount;
            _actualArgs = actualArgumentCount;
        }

        /// <summary>
        /// Creates a new BindingTarget when the method binding has failued due to 
        /// one or more parameters which could not be converted.
        /// </summary>
        internal BindingTarget(string name, int actualArgumentCount, CallFailure[] failures) {
            _name = name;
            _result = BindingResult.CallFailure;
            _callFailures = failures;
            _actualArgs = actualArgumentCount;
        }

        /// <summary>
        /// Creates a new BindingTarget when the match was ambiguous
        /// </summary>
        internal BindingTarget(string name, int actualArgumentCount, MethodTarget[] ambiguousMatches) {
            _name = name;
            _result = BindingResult.AmbiguousMatch;
            _ambiguousMatches = ambiguousMatches;
            _actualArgs = actualArgumentCount;
        }

        /// <summary>
        /// Gets the result of the attempt to bind.
        /// </summary>
        public BindingResult Result {
            get {
                return _result;
            }
        }

        /// <summary>
        /// Gets an Expression which calls the binding target if the method binding succeeded.
        /// 
        /// Throws InvalidOperationException if the binding failed.
        /// 
        /// OBSOLETE
        /// </summary>
        public Expression MakeExpression(RuleBuilder rule, IList<Expression> parameters) {
            ContractUtils.RequiresNotNull(rule, "rule");

            if (_target == null) {
                throw new InvalidOperationException("An expression cannot be produced because the method binding was unsuccessful.");
            } 
            
            return MakeExpression(new ParameterBinderWithCodeContext(_target.Binder._binder, rule.Context), parameters);
        }

        /// <summary>
        /// Gets an Expression which calls the binding target if the method binding succeeded.
        /// 
        /// Throws InvalidOperationException if the binding failed.
        /// 
        /// OBSOLETE
        /// </summary>
        public Expression MakeExpression(ParameterBinder parameterBinder, IList<Expression> parameters) {
            ContractUtils.RequiresNotNull(parameterBinder, "parameterBinder");
            ContractUtils.RequiresNotNull(parameters, "parameters");

            if (_target == null) {
                throw new InvalidOperationException("An expression cannot be produced because the method binding was unsuccessful.");
            }

            return _target.MakeExpression(parameterBinder, parameters, ArgumentTests);
        }

        /// <summary>
        /// Gets an Expression which calls the binding target if the method binding succeeded.
        /// 
        /// Throws InvalidOperationException if the binding failed.
        /// </summary>
        public Expression MakeExpression() {
            if (_target == null) {
                throw new InvalidOperationException("An expression cannot be produced because the method binding was unsuccessful.");
            }

            return MakeExpression(new ParameterBinder(_target.Binder._binder));
        }

        /// <summary>
        /// Gets an Expression which calls the binding target if the method binding succeeded.
        /// 
        /// Throws InvalidOperationException if the binding failed.
        /// </summary>
        public Expression MakeExpression(ParameterBinder parameterBinder) {
            ContractUtils.RequiresNotNull(parameterBinder, "parameterBinder");

            if (_target == null) {
                throw new InvalidOperationException("An expression cannot be produced because the method binding was unsuccessful.");
            } else if (_restrictedArgs == null) {
                throw new InvalidOperationException("An expression cannot be produced because the method binding was done with Expressions, not MetaObject's");
            }

            Expression[] exprs = new Expression[_restrictedArgs.Objects.Length];
            for (int i = 0; i < exprs.Length; i++) {
                exprs[i] = _restrictedArgs.Objects[i].Expression;
            }

            return _target.MakeExpression(parameterBinder, exprs);
        }

        public OptimizingCallDelegate MakeDelegate(ParameterBinder parameterBinder) {
            ContractUtils.RequiresNotNull(parameterBinder, "parameterBinder");

            if (_target == null) {
                throw new InvalidOperationException("An expression cannot be produced because the method binding was unsuccessful.");
            } else if (_restrictedArgs == null) {
                throw new InvalidOperationException("An expression cannot be produced because the method binding was done with Expressions, not MetaObject's");
            }

            return _target.MakeDelegate(parameterBinder, _restrictedArgs);
        }

        /// <summary>
        /// Returns the method if the binding succeeded, or null if no method was applicable.
        /// </summary>
        public MethodBase Method {
            get {
                if (_target != null) {
                    return _target.Method;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the name of the method as supplied to the MethodBinder.
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// Returns the MethodTarget if the binding succeeded, or null if no method was applicable.
        /// </summary>
        public MethodTarget MethodTarget {
            get {
                return _target;
            }
        }

        /// <summary>
        /// Returns the methods which don't have any matches or null if Result == BindingResult.AmbiguousMatch
        /// </summary>
        public IEnumerable<MethodTarget> AmbiguousMatches {
            get {
                return _ambiguousMatches;
            }
        }

        /// <summary>
        /// Returns the methods and their associated conversion failures if Result == BindingResult.CallFailure.
        /// </summary>
        public ICollection<CallFailure> CallFailures {
            get {
                return _callFailures;
            }
        }

        /// <summary>
        /// Returns the acceptable number of arguments which can be passed to the method if Result == BindingResult.IncorrectArgumentCount.
        /// </summary>
        public IList<int> ExpectedArgumentCount {
            get {
                return _expectedArgs;
            }
        }

        /// <summary>
        /// Returns the number of arguments provided to the call.  0 if the call succeeded or failed for a reason other
        /// than argument count mismatch.
        /// </summary>
        public int ActualArgumentCount {
            get {
                return _actualArgs;
            }
        }

        /// <summary>
        /// Gets the type tests that need to be performed to ensure that a call is
        /// not applicable for an overload.
        /// 
        /// The members of the array correspond to each of the arguments.  An element is 
        /// null if no test is necessary.
        /// </summary>
        public IList<Type> ArgumentTests {
            get {
                return _argTests;
            }
        }

        /// <summary>
        /// Gets the MetaObjects which we originally did binding against in their restricted form.
        /// 
        /// The members of the array correspond to each of the arguments.  All members of the array
        /// have a value.
        /// </summary>
        public RestrictionInfo RestrictedArguments {
            get {
                return _restrictedArgs;
            }
        }

        /// <summary>
        /// Returns the return type of the binding, or null if no method was applicable.
        /// </summary>
        public Type ReturnType {
            get {
                if (_target != null) {
                    return _target.ReturnType;
                }

                return null;
            }
        }

        /// <summary>
        /// Returns the NarrowingLevel of the method if the call succeeded.  If the call
        /// failed returns NarrowingLevel.None.
        /// </summary>
        public NarrowingLevel NarrowingLevel {
            get {
                return _level;
            }
        }

        /// <summary>
        /// Returns true if the binding was succesful, false if it failed.
        /// 
        /// This is an alias for BindingTarget.Result == BindingResult.Success.
        /// </summary>
        public bool Success {
            get {
                return _result == BindingResult.Success;
            }
        }

        internal MethodTarget GetTargetThrowing() {
            if (_target == null) throw new InvalidOperationException();

            return _target;
        }
    }
}
