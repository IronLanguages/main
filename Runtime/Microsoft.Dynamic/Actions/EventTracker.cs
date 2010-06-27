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
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using Microsoft.Contracts;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    public class EventTracker : MemberTracker {
        // For each instance of the class that declares the event there is a list of pairs in a table 
        // (like if we added an instance field for each instance event into its declaring class). 
        // We use _staticTarget for a static event (it is bound to its declaring type).
        // Each pair in the list holds on the stub handler that was added to the event delegate chain and the callable 
        // object that is passed to +=/-= operators. 
        private WeakDictionary<object, NormalHandlerList> _handlerLists;
        private static readonly object _staticTarget = new object();

        private readonly EventInfo _eventInfo;
        private MethodInfo _addMethod;
        private MethodInfo _removeMethod;

        internal EventTracker(EventInfo eventInfo) {
            Assert.NotNull(eventInfo);
            _eventInfo = eventInfo;
        }

        public override Type DeclaringType {
            get { return _eventInfo.DeclaringType; }
        }

        public override TrackerTypes MemberType {
            get { return TrackerTypes.Event; }
        }

        public override string Name {
            get { return _eventInfo.Name; }
        }

        public EventInfo Event {
            get {
                return _eventInfo;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public MethodInfo GetCallableAddMethod() {
            if (_addMethod == null) {
                _addMethod = CompilerHelpers.TryGetCallableMethod(_eventInfo.GetAddMethod(true));
            }
            return _addMethod;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public MethodInfo GetCallableRemoveMethod() {
            if (_removeMethod == null) {
                _removeMethod = CompilerHelpers.TryGetCallableMethod(_eventInfo.GetRemoveMethod(true));
            }
            return _removeMethod;
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

        protected internal override DynamicMetaObject GetBoundValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type, DynamicMetaObject instance) {
            return binder.ReturnMemberTracker(type, new BoundMemberTracker(this, instance));
        }

        public override MemberTracker BindToInstance(DynamicMetaObject instance) {
            if (IsStatic) {
                return this;
            }

            return new BoundMemberTracker(this, instance);
        }

        [Confined]
        public override string ToString() {
            return _eventInfo.ToString();
        }

        public void AddHandler(object target, object handler, DynamicDelegateCreator delegateCreator) {
            ContractUtils.RequiresNotNull(handler, "handler");
            ContractUtils.RequiresNotNull(delegateCreator, "delegateCreator");

            Delegate delegateHandler;
            HandlerList stubs;

            // we can add event directly (signature does match):
            if (_eventInfo.EventHandlerType.IsAssignableFrom(handler.GetType())) {
                delegateHandler = (Delegate)handler;
                stubs = null;
            } else {
                // create signature converting stub:
                delegateHandler = delegateCreator.GetDelegate(handler, _eventInfo.EventHandlerType);
                stubs = GetHandlerList(target);
            }

            GetCallableAddMethod().Invoke(target, new object[] { delegateHandler });

            if (stubs != null) {
                // remember the stub so that we could search for it on removal:
                stubs.AddHandler(handler, delegateHandler);
            }
        }

        public void RemoveHandler(object target, object handler, IEqualityComparer<object> objectComparer) {
            ContractUtils.RequiresNotNull(handler, "handler");
            ContractUtils.RequiresNotNull(objectComparer, "objectComparer");

            Delegate delegateHandler;
            if (_eventInfo.EventHandlerType.IsAssignableFrom(handler.GetType())) {
                delegateHandler = (Delegate)handler;
            } else {
                delegateHandler = GetHandlerList(target).RemoveHandler(handler, objectComparer);
            }

            if (delegateHandler != null) {
                GetCallableRemoveMethod().Invoke(target, new object[] { delegateHandler });
            }
        }

        #region Private Implementation Details

        private HandlerList GetHandlerList(object instance) {
#if !SILVERLIGHT
            if (TypeUtils.IsComObject(instance)) {
                return GetComHandlerList(instance);
            }
#endif

            if (_handlerLists == null) {
                Interlocked.CompareExchange(ref _handlerLists, new WeakDictionary<object, NormalHandlerList>(), null);
            }

            if (instance == null) {
                // targetting a static method, we'll use a random object
                // as our place holder here...
                instance = _staticTarget;
            }

            lock (_handlerLists) {
                NormalHandlerList result;
                if (_handlerLists.TryGetValue(instance, out result)) {
                    return result;
                }

                result = new NormalHandlerList();
                _handlerLists[instance] = result;
                return result;
            }
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the stub list for a COM Object.  For COM objects we store the stub list
        /// directly on the object using the Marshal APIs.  This allows us to not have
        /// any circular references to deal with via weak references which are challenging
        /// in the face of COM.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        private HandlerList GetComHandlerList(object instance) {
            HandlerList hl = (HandlerList)Marshal.GetComObjectData(instance, this);
            if (hl == null) {
                lock (_staticTarget) {
                    hl = (HandlerList)Marshal.GetComObjectData(instance, this);
                    if (hl == null) {
                        hl = new ComHandlerList();
                        if (!Marshal.SetComObjectData(instance, this, hl)) {
                            throw new COMException("Failed to set COM Object Data");
                        }
                    }
                }
            }

            return hl;
        }
#endif

        /// <summary>
        /// Holds on a list of delegates hooked to the event. 
        /// We need the list because we cannot enumerate the delegates hooked to CLR event and we need to do so in 
        /// handler removal (we need to do custom delegate comparison there). If BCL enables the enumeration we could remove this.
        /// </summary>
        private abstract class HandlerList {
            public abstract void AddHandler(object callableObject, Delegate handler);
            public abstract Delegate RemoveHandler(object callableObject, IEqualityComparer<object> comparer);
        }

#if !SILVERLIGHT
        private sealed class ComHandlerList : HandlerList {
            /// <summary>
            /// Storage for the handlers - a key value pair of the callable object and the delegate handler.
            /// </summary>
            private readonly CopyOnWriteList<KeyValuePair<object, object>> _handlers = new CopyOnWriteList<KeyValuePair<object, object>>();

            public override void AddHandler(object callableObject, Delegate handler) {
                Assert.NotNull(handler);
                _handlers.Add(new KeyValuePair<object, object>(callableObject, handler));
            }

            public override Delegate RemoveHandler(object callableObject, IEqualityComparer<object> comparer) {
                List<KeyValuePair<object, object>> copyOfHandlers = _handlers.GetCopyForRead();
                for (int i = copyOfHandlers.Count - 1; i >= 0; i--) {
                    object key = copyOfHandlers[i].Key;
                    object value = copyOfHandlers[i].Value;

                    if (comparer.Equals(key, callableObject)) {
                        Delegate handler = (Delegate)value;
                        _handlers.RemoveAt(i);
                        return handler;
                    }
                }

                return null;
            }
        }
#endif

        private sealed class NormalHandlerList : HandlerList {
            /// <summary>
            /// Storage for the handlers - a key value pair of the callable object and the delegate handler.
            /// 
            /// The delegate handler is closed over the callable object.  Therefore as long as the object is alive the
            /// delegate will stay alive and so will the callable object.  That means it's fine to have a weak reference
            /// to both of these objects.
            /// </summary>
            private readonly CopyOnWriteList<KeyValuePair<WeakReference, WeakReference>> _handlers = new CopyOnWriteList<KeyValuePair<WeakReference, WeakReference>>();

            public NormalHandlerList() {
            }

            public override void AddHandler(object callableObject, Delegate handler) {
                Assert.NotNull(handler);
                _handlers.Add(new KeyValuePair<WeakReference, WeakReference>(new WeakReference(callableObject), new WeakReference(handler)));
            }

            public override Delegate RemoveHandler(object callableObject, IEqualityComparer<object> comparer) {
                List<KeyValuePair<WeakReference, WeakReference>> copyOfHandlers = _handlers.GetCopyForRead();
                for (int i = copyOfHandlers.Count - 1; i >= 0; i--) {
                    object key = copyOfHandlers[i].Key.Target;
                    object value = copyOfHandlers[i].Value.Target;

                    if (key != null && value != null && comparer.Equals(key, callableObject)) {
                        Delegate handler = (Delegate)value;
                        _handlers.RemoveAt(i);
                        return handler;
                    }
                }

                return null;
            }
        }

        #endregion
    }
}
