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

#if !SILVERLIGHT

using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Dynamic.Utils;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Dynamic")]
namespace System.Dynamic {

    /// <summary>
    /// Provides helper methods to bind COM objects dynamically.
    /// </summary>
    public static class ComBinder {

        /// <summary>
        /// Determines if an object is a COM object.
        /// </summary>
        /// <param name="value">The object to test.</param>
        /// <returns>true if the object is a COM object, false otherwise.</returns>
        public static bool IsComObject(object value) {
            return ComObject.IsComObject(value);
        }

        /// <summary>
        /// Tries to perform binding of the dynamic get member operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="GetMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="instance">The target of the dynamic operation. </param>
        /// <param name="result">The new <see cref="DynamicMetaObject"/> representing the result of the binding.</param>
        /// <param name="delayInvocation">true if member evaluation may be delayed.</param>
        /// <returns>true if operation was bound successfully; otherwise, false.</returns>
        public static bool TryBindGetMember(GetMemberBinder binder, DynamicMetaObject instance, out DynamicMetaObject result, bool delayInvocation) {
            if (delayInvocation == false) {
                return TryBindGetMember(binder, instance, out result);
            }
            if (TryGetMetaObject(ref instance)) {
                result = instance.BindGetMember(binder);
                return true;
            } else {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to perform binding of the dynamic get member operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="GetMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="instance">The target of the dynamic operation. </param>
        /// <param name="result">The new <see cref="DynamicMetaObject"/> representing the result of the binding.</param>
        /// <returns>true if operation was bound successfully; otherwise, false.</returns>
        public static bool TryBindGetMember(GetMemberBinder binder, DynamicMetaObject instance, out DynamicMetaObject result) {
            if (TryGetMetaObject(ref instance)) {
                // in COM x.foo is equivalent to x.foo()
                var adaptor = new ComGetMemberAsInvokeBinder(binder);
                result = instance.BindInvokeMember(adaptor, new DynamicMetaObject[0]);
                return true;
            } else {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to perform binding of the dynamic set member operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="SetMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="instance">The target of the dynamic operation.</param>
        /// <param name="value">The <see cref="DynamicMetaObject"/> representing the value for the set member operation.</param>
        /// <param name="result">The new <see cref="DynamicMetaObject"/> representing the result of the binding.</param>
        /// <returns>true if operation was bound successfully; otherwise, false.</returns>
        public static bool TryBindSetMember(SetMemberBinder binder, DynamicMetaObject instance, DynamicMetaObject value, out DynamicMetaObject result) {
            if (TryGetMetaObject(ref instance)) {
                result = instance.BindSetMember(binder, value);
                return true;
            } else {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to perform binding of the dynamic invoke operation.
        /// </summary>    
        /// <param name="binder">An instance of the <see cref="GetMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="instance">The target of the dynamic operation. </param>
        /// <param name="args">An array of <see cref="DynamicMetaObject"/> instances - arguments to the invoke member operation.</param>
        /// <param name="result">The new <see cref="DynamicMetaObject"/> representing the result of the binding.</param>
        /// <returns>true if operation was bound successfully; otherwise, false.</returns>
        public static bool TryBindInvoke(InvokeBinder binder, DynamicMetaObject instance, DynamicMetaObject[] args, out DynamicMetaObject result) {
            if (TryGetMetaObject(ref instance)) {
                result = instance.BindInvoke(binder, args);
                return true;
            } else {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to perform binding of the dynamic invoke member operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="InvokeMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="instance">The target of the dynamic operation. </param>
        /// <param name="args">An array of <see cref="DynamicMetaObject"/> instances - arguments to the invoke member operation.</param>
        /// <param name="result">The new <see cref="DynamicMetaObject"/> representing the result of the binding.</param>
        /// <returns>true if operation was bound successfully; otherwise, false.</returns>
        public static bool TryBindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject instance, DynamicMetaObject[] args, out DynamicMetaObject result) {
            if (TryGetMetaObject(ref instance)) {
                result = instance.BindInvokeMember(binder, args);
                return true;
            } else {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to perform binding of the dynamic get index operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="GetMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="instance">The target of the dynamic operation. </param>
        /// <param name="args">An array of <see cref="DynamicMetaObject"/> instances - arguments to the invoke member operation.</param>
        /// <param name="result">The new <see cref="DynamicMetaObject"/> representing the result of the binding.</param>
        /// <returns>true if operation was bound successfully; otherwise, false.</returns>
        public static bool TryBindGetIndex(GetIndexBinder binder, DynamicMetaObject instance, DynamicMetaObject[] args, out DynamicMetaObject result) {
            if (TryGetMetaObject(ref instance)) {
                result = instance.BindGetIndex(binder, args);
                return true;
            } else {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to perform binding of the dynamic set index operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="GetMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="instance">The target of the dynamic operation. </param>
        /// <param name="args">An array of <see cref="DynamicMetaObject"/> instances - arguments to the invoke member operation.</param>
        /// <param name="value">The <see cref="DynamicMetaObject"/> representing the value for the set index operation.</param>
        /// <param name="result">The new <see cref="DynamicMetaObject"/> representing the result of the binding.</param>
        /// <returns>true if operation was bound successfully; otherwise, false.</returns>
        public static bool TryBindSetIndex(SetIndexBinder binder, DynamicMetaObject instance, DynamicMetaObject[] args, DynamicMetaObject value, out DynamicMetaObject result) {
            if (TryGetMetaObject(ref instance)) {
                result = instance.BindSetIndex(binder, args, value);
                return true;
            } else {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Gets the member names associated with the object.
        /// This function can operate only with objects for which <see cref="IsComObject"/> returns true.
        /// </summary>
        /// <param name="value">The object for which member names are requested.</param>
        /// <returns>The collection of member names.</returns>
        public static IEnumerable<string> GetDynamicMemberNames(object value) {
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.Requires(IsComObject(value), "value", Strings.ComObjectExpected);

            return ComObject.ObjectToComObject(value).MemberNames;
        }

        /// <summary>
        /// Gets the data-like members and associated data for an object.
        /// This function can operate only with objects for which <see cref="IsComObject"/> returns true.
        /// </summary>
        /// <param name="value">The object for which data members are requested.</param>
        /// <returns>The collection of pairs that represent data member's names and their data.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IEnumerable<KeyValuePair<string, object>> GetDynamicDataMembers(object value) {
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.Requires(IsComObject(value), "value", Strings.ComObjectExpected);

            return ComObject.ObjectToComObject(value).DataMembers;
        }

        private static bool TryGetMetaObject(ref DynamicMetaObject instance) {
            // If we're already a COM MO don't make a new one
            // (we do this to prevent recursion if we call Fallback from COM)
            if (instance is ComUnwrappedMetaObject) {
                return false;
            }

            if (IsComObject(instance.Value)) {
                instance = new ComMetaObject(instance.Expression, instance.Restrictions, instance.Value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adaptor class that allows transforming GetMember into InvokeMember when member evaluation is forced.
        /// </summary>
        private class ComGetMemberAsInvokeBinder : InvokeMemberBinder {
            private readonly GetMemberBinder _originalBinder;
            internal ComGetMemberAsInvokeBinder(GetMemberBinder originalBinder) :
                base(originalBinder.Name, originalBinder.IgnoreCase, new ArgumentInfo[0]) {
                _originalBinder = originalBinder;
            }
            public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
                return _originalBinder.FallbackGetMember(target);
            }
            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
                throw Assert.Unreachable;
            }
        }
    }
}

#endif
