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
using System.Dynamic;
using System.Reflection;

using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;

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

        public override DynamicMetaObject GetValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type) {
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
                return binder.MakeCallExpression(resolverFactory, getter);
            }

            // private binding is just a call to the getter method...
            return MemberTracker.FromMemberInfo(getter).Call(resolverFactory, binder);
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

        protected internal override DynamicMetaObject GetBoundValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type, DynamicMetaObject instance) {
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

            var defaultBinder = (DefaultBinder)binder;
            if (binder.PrivateBinding || CompilerHelpers.IsVisible(getter)) {
                return defaultBinder.MakeCallExpression(resolverFactory, getter, instance);
            }

            // private binding is just a call to the getter method...
            return DefaultBinder.MakeError(defaultBinder.MakeNonPublicMemberGetError(resolverFactory, this, type, instance), BindingRestrictions.Empty, typeof(object));
        }

        public override ErrorInfo GetBoundError(ActionBinder binder, DynamicMetaObject instance) {
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

        public override MemberTracker BindToInstance(DynamicMetaObject instance) {
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
