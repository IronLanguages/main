/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/


#if !CLR2
using System.Linq.Expressions;
using Microsoft.Scripting.Ast;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;
using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Provides support for light exceptions.  These exceptions are propagated by
    /// returning an instance of a private wrapper class containing the exception.  Code
    /// which is aware of light exceptions will branch to apporiate exception handling
    /// blocks when in a try and otherwise return the value up the stack.  This avoids 
    /// using the underlying CLR exception mechanism with overhead such as creating stack 
    /// traces.
    /// 
    /// When a light exception reaches the boundary of code which is not light exception
    /// aware the caller must check to see if a light exception is being thrown and if
    /// so raise a .NET exception.
    /// 
    /// This class provides methods for re-writing expression trees to support light exceptions,
    /// methods to create light throw objects, check if an object is a light
    /// throw object, and turn such an object back into a .NET Exception which can be thrown.
    /// 
    /// Light exceptions also don't build up stack traces or interoperate with filter blocks
    /// via 2-pass exception handling.
    /// </summary>
    public static class LightExceptions {
        internal static MethodInfo _checkAndThrow = new Func<object, object>(LightExceptions.CheckAndThrow).Method;

        /// <summary>
        /// Rewrites the provided expression to support light exceptions.  
        /// 
        /// Calls to the returned expression, if not from other light-weight aware calls,
        /// need to call GetLightException on return to see if an exception was thrown
        /// and if so throw it.
        /// </summary>
        public static Expression Rewrite(Expression expression) {
            ContractUtils.RequiresNotNull(expression, "expression");
            
            return new LightExceptionRewriter().Rewrite(expression);
        }

        /// <summary>
        /// Returns a new expression which will lazily reduce to a light
        /// expression re-written version of the same expression.
        /// </summary>
        public static Expression RewriteLazy(Expression expression) {
            ContractUtils.RequiresNotNull(expression, "expression");

            return new LightExceptionConvertingExpression(expression, false);
        }

        /// <summary>
        /// Returns a new expression which is re-written for light exceptions
        /// but will throw an exception if it escapes the expression.  If this
        /// expression is part of a larger experssion which is later re-written 
        /// for light exceptions then it will propagate the light exception up.
        /// </summary>
        public static Expression RewriteExternal(Expression expression) {
            ContractUtils.RequiresNotNull(expression, "expression");

            return CheckAndThrow(new LightExceptionConvertingExpression(expression, true));
        }

        /// <summary>
        /// Returns an object which represents a light exception.
        /// </summary>
        public static object Throw(Exception exceptionValue) {
            ContractUtils.RequiresNotNull(exceptionValue, "exceptionValue");

            return new LightException(exceptionValue);
        }

        /// <summary>
        /// Returns an object which represents a light exception.
        /// </summary>
        public static Expression Throw(Expression exceptionValue) {
            ContractUtils.RequiresNotNull(exceptionValue, "exceptionValue");

            return new LightThrowExpression(exceptionValue);
        }

        /// <summary>
        /// Returns an object which represents a light exception.
        /// </summary>
        public static Expression Throw(Expression exceptionValue, Type retType) {
            ContractUtils.RequiresNotNull(exceptionValue, "exceptionValue");
            ContractUtils.RequiresNotNull(retType, "type");

            return Expression.Convert(new LightThrowExpression(exceptionValue), retType);
        }

        /// <summary>
        /// If the binder supports light exceptions then a light exception throwing expression is returned.
        /// 
        /// Otherwise a normal throwing expression is returned.
        /// </summary>
        public static Expression Throw(this DynamicMetaObjectBinder binder, Expression exceptionValue) {
            ContractUtils.RequiresNotNull(binder, "binder");
            ContractUtils.RequiresNotNull(exceptionValue, "exceptionValue");

            if (binder.SupportsLightThrow()) {
                return Throw(exceptionValue);
            }

            return Expression.Throw(exceptionValue);
        }

        /// <summary>
        /// If the binder supports light exceptions then a light exception throwing expression is returned.
        /// 
        /// Otherwise a normal throwing expression is returned.
        /// </summary>
        public static Expression Throw(this DynamicMetaObjectBinder binder, Expression exceptionValue, Type retType) {
            ContractUtils.RequiresNotNull(binder, "binder");
            ContractUtils.RequiresNotNull(exceptionValue, "exceptionValue");
            ContractUtils.RequiresNotNull(retType, "type");

            if (binder.SupportsLightThrow()) {
                return Throw(exceptionValue, retType);
            }

            return Expression.Throw(exceptionValue, retType);
        }

        /// <summary>
        /// Throws the exception if the value represents a light exception
        /// </summary>
        public static object CheckAndThrow(object value) {
            LightException lightEx = value as LightException;
            if (lightEx != null) {
                ThrowException(lightEx);
            }
            return value;
        }

        private static void ThrowException(LightException lightEx) {
            throw lightEx.Exception;
        }

        /// <summary>
        /// Wraps the expression in a check and rethrow.
        /// </summary>
        public static Expression CheckAndThrow(Expression expr) {
            ContractUtils.RequiresNotNull(expr, "expr");
            ContractUtils.Requires(expr.Type == typeof(object), "checked expression must be type of object");

            return new LightCheckAndThrowExpression(expr);
        }

        /// <summary>
        /// Checks to see if the provided value is a light exception.
        /// </summary>
        public static bool IsLightException(object value) {
            return value is LightException;
        }

        /// <summary>
        /// Gets the light exception from an object which may contain a light
        /// exception.  Returns null if the object is not a light exception.
        /// 
        /// Used for throwing the exception at non-light exception boundaries.  
        /// </summary>
        public static Exception GetLightException(object exceptionValue) {
            LightException lightEx = exceptionValue as LightException;
            if (lightEx != null) {
                return lightEx.Exception;
            }

            return null;
        }

        /// <summary>
        /// Returns true if the call site binder is a light exception binder and supports
        /// light throws.  Returns false otherwise.
        /// </summary>
        /// <param name="binder"></param>
        /// <returns></returns>
        public static bool SupportsLightThrow(this CallSiteBinder binder) {
            ILightExceptionBinder lightBinder = binder as ILightExceptionBinder;
            if (lightBinder != null) {  
                return lightBinder.SupportsLightThrow;
            }
            return false;
        }

        /// <summary>
        /// Sealed wrapper class to indicate something is a light exception.
        /// </summary>
        private sealed class LightException {
            public readonly Exception Exception;

            public LightException(Exception exception) {
                Debug.Assert(exception != null);

                Exception = exception;
            }
        }
        
        private static ReadOnlyCollection<Expression> ToReadOnly(Expression[] args) {
            return new ReadOnlyCollectionBuilder<Expression>(args).ToReadOnlyCollection();
        }
    }
}
