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

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Represents a logical member of a type.  The member could either be real concrete member on a type or
    /// an extension member.
    /// 
    /// This seperates the "physical" members that .NET knows exist on types from the members that
    /// logically exist on a type.  It also provides other abstractions above the level of .NET reflection
    /// such as MemberGroups and NamespaceTracker's.
    /// 
    /// It also provides a wrapper around the reflection APIs which cannot be extended from partial trust.
    /// </summary>
    public abstract class MemberTracker {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly MemberTracker[] EmptyTrackers = new MemberTracker[0];

        private static readonly Dictionary<MemberKey, MemberTracker> _trackers = new Dictionary<MemberKey, MemberTracker>();

        /// <summary>
        /// We ensure we only produce one MemberTracker for each member which logically lives on the declaring type.  So 
        /// for example if you get a member from a derived class which is declared on the base class it should be the same 
        /// as getting the member from the base class.  That’s easy enough until you get into extension members – here there
        /// might be one extension member which is being applied to multiple types.  Therefore we need to take into account the 
        /// extension type when ensuring that we only have 1 MemberTracker ever created.
        /// </summary>
        class MemberKey {
            private readonly MemberInfo Member;
            private readonly Type Extending;

            public MemberKey(MemberInfo member, Type extending) {
                Member = member;
                Extending = extending;
            }

            public override int GetHashCode() {
                int res = Member.GetHashCode();
                if (Extending != null) {
                    res ^= Extending.GetHashCode();
                }
                return res;
            }

            public override bool Equals(object obj) {
                MemberKey other = obj as MemberKey;
                if (other == null) return false;

                return other.Member == Member &&
                    other.Extending == Extending;
            }
        }

        internal MemberTracker() {
        }

        /// <summary>
        /// The type of member tracker.
        /// </summary>
        public abstract TrackerTypes MemberType {
            get;
        }

        /// <summary>
        /// The logical declaring type of the member.
        /// </summary>
        public abstract Type DeclaringType {
            get;
        }

        /// <summary>
        /// The name of the member.
        /// </summary>
        public abstract string Name {
            get;
        }

        public static MemberTracker FromMemberInfo(MemberInfo member) {
            return FromMemberInfo(member, null);
        }

        public static MemberTracker FromMemberInfo(MemberInfo member, Type extending) {
            ContractUtils.RequiresNotNull(member, "member");

            lock (_trackers) {
                MemberTracker res;
                MemberKey key = new MemberKey(member, extending);
                if (_trackers.TryGetValue(key, out res)) return res;

                switch (member.MemberType) {
                    case MemberTypes.Constructor: res = new ConstructorTracker((ConstructorInfo)member); break;
                    case MemberTypes.Event: res = new EventTracker((EventInfo)member); break;
                    case MemberTypes.Field: res = new FieldTracker((FieldInfo)member); break;
                    case MemberTypes.Method:
                        MethodInfo mi = (MethodInfo)member;
                        if (extending != null) {
                            res = new ExtensionMethodTracker(mi, member.IsDefined(typeof(StaticExtensionMethodAttribute), false), extending);
                        } else {
                            res = new MethodTracker(mi);
                        }
                        break;
                    case MemberTypes.TypeInfo:
                    case MemberTypes.NestedType: res = new NestedTypeTracker((Type)member); break;
                    case MemberTypes.Property: res = new ReflectedPropertyTracker((PropertyInfo)member); break;
                    default: throw Error.UnknownMemberType(member.MemberType);
                }

                _trackers[key] = res;
                return res;
            }
        }

        #region Public expression builders

        /// <summary>
        /// Gets the expression that creates the value.  
        /// 
        /// Returns null if it's an error to get the value.  The caller can then call GetErrorForGet to get 
        /// the correct error Expression (or null if they should provide a default).
        /// </summary>
        public virtual DynamicMetaObject GetValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type) {
            return binder.ReturnMemberTracker(type, this);
        }

        /// <summary>
        /// Gets an expression that assigns a value to the left hand side.
        /// 
        /// Returns null if it's an error to assign to.  The caller can then call GetErrorForSet to
        /// get the correct error Expression (or null if a default error should be provided).
        /// </summary>
        public virtual DynamicMetaObject SetValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type, DynamicMetaObject value) {
            return null;
        }

        /// <summary>
        /// Gets an expression that assigns a value to the left hand side.
        /// 
        /// Returns null if it's an error to assign to.  The caller can then call GetErrorForSet to
        /// get the correct error Expression (or null if a default error should be provided).
        /// </summary>
        public virtual DynamicMetaObject SetValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
            return SetValue(resolverFactory, binder, type, value);
        }

        /// <summary>
        /// Gets an expression that performs a call on the object using the specified arguments.
        /// 
        /// Returns null if it's an error to perform the specific operation.  The caller can then call 
        /// GetErrorsForDoCall to get the correct error Expression (or null if a default error should be provided).
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Call")] // TODO: fix
        internal virtual DynamicMetaObject Call(OverloadResolverFactory resolverFactory, ActionBinder binder, params DynamicMetaObject[] arguments) {
            return null;
        }

        #endregion

        #region Public error expression builders

        /// <summary>
        /// Returns the error associated with getting the value.  
        /// 
        /// A null return value indicates that the default error message should be provided by the caller.
        /// </summary>
        public virtual ErrorInfo GetError(ActionBinder binder) {
            return null;
        }

        /// <summary>
        /// Returns the error associated with accessing this member via a bound instance.
        /// 
        /// A null return value indicates that the default error message should be provided by the caller.
        /// </summary>
        public virtual ErrorInfo GetBoundError(ActionBinder binder, DynamicMetaObject instance) {
            return null;
        }

        /// <summary>
        /// Helper for getting values that have been bound.  Called from BoundMemberTracker.  Custom member
        /// trackers can override this to provide their own behaviors when bound to an instance.
        /// </summary>
        protected internal virtual DynamicMetaObject GetBoundValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type, DynamicMetaObject instance) {
            return GetValue(resolverFactory, binder, type);
        }

        /// <summary>
        /// Helper for setting values that have been bound.  Called from BoundMemberTracker.  Custom member
        /// trackers can override this to provide their own behaviors when bound to an instance.
        /// </summary>
        protected internal virtual DynamicMetaObject SetBoundValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type, DynamicMetaObject value, DynamicMetaObject instance) {
            return SetValue(resolverFactory, binder, type, instance);
        }

        /// <summary>
        /// Helper for setting values that have been bound.  Called from BoundMemberTracker.  Custom member
        /// trackers can override this to provide their own behaviors when bound to an instance.
        /// </summary>
        protected internal virtual DynamicMetaObject SetBoundValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type, DynamicMetaObject value, DynamicMetaObject instance, DynamicMetaObject errorSuggestion) {
            return SetValue(resolverFactory, binder, type, instance, errorSuggestion);
        }


        /// <summary>
        /// Binds the member tracker to the specified instance rturning a new member tracker if binding 
        /// is possible.  If binding is not possible the existing member tracker will be returned.  For example
        /// binding to a static field results in returning the original MemberTracker.  Binding to an instance
        /// field results in a new BoundMemberTracker which will get GetBoundValue/SetBoundValue to pass the
        /// instance through.
        /// </summary>
        public virtual MemberTracker BindToInstance(DynamicMetaObject instance) {
            return this;
        }

        #endregion
    }
}
