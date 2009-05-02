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
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {

    /// <summary>
    /// Represents a logical Property as a member of a Type.  This Property can either be a real 
    /// concrete Property on a type (implemented with a ReflectedPropertyTracker) or an extension
    /// property (implemented with an ExtensionPropertyTracker).
    /// </summary>
    public abstract class PropertyTracker : MemberTracker {
        public override TrackerTypes MemberType {
            get { return TrackerTypes.Property; }
        }

        public abstract MethodInfo GetGetMethod();
        public abstract MethodInfo GetSetMethod();
        public abstract MethodInfo GetGetMethod(bool privateMembers);
        public abstract MethodInfo GetSetMethod(bool privateMembers);

        public virtual MethodInfo GetDeleteMethod() {
            return null;
        }

        public virtual MethodInfo GetDeleteMethod(bool privateMembers) {
            return null;
        }

        public abstract ParameterInfo[] GetIndexParameters();

        public abstract bool IsStatic {
            get;
        }

        public abstract Type PropertyType {
            get;
        }

        #region Public expression builders

        public override Expression GetValue(Expression context, ActionBinder binder, Type type) {
            if (!IsStatic || GetIndexParameters().Length > 0) {
                // need to bind to a value or parameters to get the value.
                return binder.ReturnMemberTracker(type, this);
            }

            MethodInfo getter = ResolveGetter(binder.PrivateBinding);
            if (getter == null || getter.ContainsGenericParameters) {
                // no usable getter
                return null;
            }

            if (getter.IsPublic && getter.DeclaringType.IsPublic) {
                return AstUtils.Convert(binder.MakeCallExpression(context, getter), typeof(object));
            }

            // private binding is just a call to the getter method...
            return MemberTracker.FromMemberInfo(getter).Call(context, binder);
        }

        public override ErrorInfo GetError(ActionBinder binder) {
            MethodInfo getter = ResolveGetter(binder.PrivateBinding);

            if (getter == null) {
                return binder.MakeMissingMemberErrorInfo(DeclaringType, Name);
            }

            if (getter.ContainsGenericParameters) {
                return binder.MakeGenericAccessError(this);
            }

            throw new InvalidOperationException();
        }

        #endregion

        #region Internal expression builders

        protected internal override Expression GetBoundValue(Expression context, ActionBinder binder, Type type, Expression instance) {
            if (instance != null && IsStatic) {
                return null;
            }

            if (GetIndexParameters().Length > 0) {
                // need to bind to a value or parameters to get the value.
                return binder.ReturnMemberTracker(type, BindToInstance(instance));
            }

            MethodInfo getter = GetGetMethod(true);
            if (getter == null || getter.ContainsGenericParameters) {
                // no usable getter
                return null;
            }

            getter = CompilerHelpers.TryGetCallableMethod(getter);

            if (binder.PrivateBinding || CompilerHelpers.IsVisible(getter)) {
                return AstUtils.Convert(binder.MakeCallExpression(context, getter, instance), typeof(object));
            }

            // private binding is just a call to the getter method...
            return DefaultBinder.MakeError(((DefaultBinder)binder).MakeNonPublicMemberGetError(context, this, type, instance), typeof(object));
        }

        public override ErrorInfo GetBoundError(ActionBinder binder, Expression instance) {
            MethodInfo getter = ResolveGetter(binder.PrivateBinding);

            if (getter == null) {
                return binder.MakeMissingMemberErrorInfo(DeclaringType, Name);
            }

            if (getter.ContainsGenericParameters) {
                return binder.MakeGenericAccessError(this);
            }

            if (IsStatic) {
                return binder.MakeStaticPropertyInstanceAccessError(this, false, instance);
            }

            throw new InvalidOperationException();
        }

        public override MemberTracker BindToInstance(Expression instance) {
            return new BoundMemberTracker(this, instance);
        }

        #endregion

        #region Private expression builder helpers

        private MethodInfo ResolveGetter(bool privateBinding) {
            MethodInfo getter = GetGetMethod(true);
            if (getter != null) {
                getter = CompilerHelpers.TryGetCallableMethod(getter);
                if (privateBinding || CompilerHelpers.IsVisible(getter)) {
                    return getter;
                }
            }
            return null;
        }

        #endregion
    }
}
