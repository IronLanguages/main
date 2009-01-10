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
using System.Dynamic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public static partial class Utils {

        [Obsolete("use Expression.Dynamic instead of old-style action factories")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "span")]
        public static DynamicExpression Operator(SourceSpan span, ActionBinder binder, Operators op, Type result, params Expression[] arguments) {
            return Operator(binder, op, result, arguments);
        }

        [Obsolete("use Expression.Dynamic instead of old-style action factories")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "span")]
        public static DynamicExpression DeleteMember(SourceSpan span, ActionBinder binder, string name, params Expression[] arguments) {
            return DeleteMember(binder, name, arguments);
        }

        /// <summary>
        /// Creates DynamicExpression representing OldDoOperationAction.
        /// </summary>
        /// <param name="binder">The binder responsible for binding the dynamic operation.</param>
        /// <param name="op">The operation to perform</param>
        /// <param name="resultType">Type of the result desired (The DynamicExpression is strongly typed)</param>
        /// <param name="arguments">Array of arguments for the action expression</param>
        /// <returns>New instance of the DynamicExpression</returns>
        [Obsolete("use Expression.Dynamic instead of old-style action factories")]
        public static DynamicExpression Operator(ActionBinder binder, Operators op, Type resultType, params Expression[] arguments) {
            return Expression.Dynamic(OldDoOperationAction.Make(binder, op), resultType, arguments);
        }

        [Obsolete("use Expression.Dynamic instead of old-style action factories")]
        public static DynamicExpression GetMember(ActionBinder binder, string name, Type result, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length > 0, "arguments");

            return Expression.Dynamic(OldGetMemberAction.Make(binder, name), result, arguments);
        }

        [Obsolete("use Expression.Dynamic instead of old-style action factories")]
        public static DynamicExpression GetMember(ActionBinder binder, string name, GetMemberBindingFlags getMemberFlags, Type result, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length > 0, "arguments");

            return Expression.Dynamic(OldGetMemberAction.Make(binder, name, getMemberFlags), result, arguments);
        }

        [Obsolete("use Expression.Dynamic instead of old-style action factories")]
        private static DynamicExpression SetMember(ActionBinder binder, string name, Type result, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length >= 2, "arguments");

            return Expression.Dynamic(OldSetMemberAction.Make(binder, name), result, arguments);
        }

        [Obsolete("use Expression.Dynamic instead of old-style action factories")]
        public static DynamicExpression DeleteMember(ActionBinder binder, string name, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length >= 1, "arguments");

            return Expression.Dynamic(OldDeleteMemberAction.Make(binder, name), typeof(object), arguments);
        }

        // TODO: This helper should go. It does too much number magic.
        [Obsolete("use Expression.Dynamic instead of old-style action factories")]
        public static DynamicExpression Call(ActionBinder binder, Type result, params Expression[] arguments) {
            return Call(OldCallAction.Make(binder, arguments.Length - 2), result, arguments);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
        public static DynamicExpression Call(OldCallAction action, Type result, params Expression[] arguments) {
            return Expression.Dynamic(action, result, arguments);
        }       

        /// <summary>
        /// Creates DynamicExpression representing a CreateInstance action.
        /// </summary>
        /// <param name="action">The create instance action to perform.</param>
        /// <param name="result">Type of the result desired (The DynamicExpression is strongly typed)</param>
        /// <param name="arguments">Array of arguments for the action expression</param>
        /// <returns>New instance of the DynamicExpression</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
        [Obsolete("use Expression.Dynamic instead of old-style action factories")]
        public static DynamicExpression Create(OldCreateInstanceAction action, Type result, params Expression[] arguments) {
            return Expression.Dynamic(action, result, arguments);
        }

        // TODO: This helper should go. It does too much number magic.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
        [Obsolete("use Expression.Dynamic instead of old-style action factories")]
        public static DynamicExpression Create(ActionBinder binder, Type result, params Expression[] arguments) {
            return Create(OldCreateInstanceAction.Make(binder, arguments.Length - 2), result, arguments);
        }

        [Obsolete("use Expression.Dynamic instead of old-style action factories")]
        public static DynamicExpression ConvertTo(OldConvertToAction action, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length > 0, "arguments");

            return Expression.Dynamic(action, action.ToType, arguments);
        }

        [Obsolete("use Expression.Dynamic instead of old-style action factories")]
        public static DynamicExpression ConvertTo(ActionBinder binder, Type toType, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(toType, "toType");
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length > 0, "arguments");

            return Expression.Dynamic(OldConvertToAction.Make(binder, toType), toType, arguments);
        }

        /// <summary>
        /// Creates a new DynamicExpression which performs the specified conversion to the type.  The ActionExpress
        /// is strongly typed to the converted type.
        /// </summary>
        [Obsolete("use Expression.Dynamic instead of old-style action factories")]
        public static DynamicExpression ConvertTo(ActionBinder binder, Type toType, ConversionResultKind kind, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(toType, "toType");
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length > 0, "arguments");

            return Expression.Dynamic(OldConvertToAction.Make(binder, toType, kind), toType, arguments);
        }

        [Obsolete("use Expression.Dynamic instead of old-style action factories")]
        public static DynamicExpression ConvertTo(ActionBinder binder, Type toType, ConversionResultKind kind, Type actionExpressionType, params Expression[] arguments) {
            ContractUtils.RequiresNotNull(toType, "toType");
            ContractUtils.RequiresNotNull(arguments, "arguments");
            ContractUtils.Requires(arguments.Length > 0, "arguments");

            return Expression.Dynamic(OldConvertToAction.Make(binder, toType, kind), actionExpressionType, arguments);
        }
    }
}
