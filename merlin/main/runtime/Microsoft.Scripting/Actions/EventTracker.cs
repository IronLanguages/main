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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Actions {
    public class EventTracker : MemberTracker {
        private EventInfo _event;

        internal EventTracker(EventInfo eventInfo) {
            _event = eventInfo;
        }

        public override Type DeclaringType {
            get { return _event.DeclaringType; }
        }

        public override TrackerTypes MemberType {
            get { return TrackerTypes.Event; }
        }

        public override string Name {
            get { return _event.Name; }
        }

        public EventInfo Event {
            get {
                return _event;
            }
        }

        /// <summary>
        /// Doesn't need to check PrivateBinding setting: no method that is part of the event is public the entire event is private. 
        /// If the code has already a reference to the event tracker instance for a private event its "static-ness" is not influenced 
        /// by private-binding setting.
        /// </summary>
        public bool IsStatic {
            get {
                MethodInfo mi = Event.GetAddMethod(false) ??
                    Event.GetRemoveMethod(false) ??
                    Event.GetRaiseMethod(false) ??
                    Event.GetAddMethod(true) ??
                    Event.GetRemoveMethod(true) ??
                    Event.GetRaiseMethod(true);

                MethodInfo m;
                Debug.Assert(
                    ((m = Event.GetAddMethod(true)) == null || m.IsStatic == mi.IsStatic) &&
                    ((m = Event.GetRaiseMethod(true)) == null || m.IsStatic == mi.IsStatic) &&
                    ((m = Event.GetRaiseMethod(true)) == null || m.IsStatic == mi.IsStatic),
                    "Methods are either all static or all instance."
                );
                
                return mi.IsStatic;
            }
        }

        protected internal override Expression GetBoundValue(Expression context, ActionBinder binder, Type type, Expression instance) {
            return binder.ReturnMemberTracker(type, new BoundMemberTracker(this, instance));
        }

        public override MemberTracker BindToInstance(Expression instance) {
            if (IsStatic) {
                return this;
            }

            return new BoundMemberTracker(this, instance);
        }

        [Confined]
        public override string ToString() {
            return _event.ToString();
        }
    }
}
