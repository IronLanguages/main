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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Dynamic;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types {

    /// <summary>
    /// The unbound representation of an event property
    /// </summary>
    [PythonType("event#")]
    public sealed class ReflectedEvent : PythonTypeSlot, ICodeFormattable {
        private readonly bool _clsOnly;
        private readonly EventInfo/*!*/ _eventInfo;
        private WeakDictionary<object, NormalHandlerList/*!*/> _handlerLists;
        private static readonly object _staticTarget = new object();

        internal ReflectedEvent(EventInfo/*!*/ eventInfo, bool clsOnly) {
            Assert.NotNull(eventInfo);

            _clsOnly = clsOnly;
            _eventInfo = eventInfo;
        }

        #region Internal APIs

        internal override bool TryGetValue(CodeContext/*!*/ context, object instance, PythonType owner, out object value) {
            Assert.NotNull(context, owner);

            value = new BoundEvent(this, instance, owner);
            return true;
        }

        internal override bool GetAlwaysSucceeds {
            get {
                return true;
            }
        }

        internal override bool TrySetValue(CodeContext/*!*/ context, object instance, PythonType owner, object value) {
            Assert.NotNull(context);
            BoundEvent et = value as BoundEvent;

            if (et == null || EventInfosDiffer(et)) {
                BadEventChange bea = value as BadEventChange;

                if (bea != null) {
                    PythonType dt = bea.Owner as PythonType;
                    if (dt != null) {
                        if (bea.Instance == null) {
                            throw new MissingMemberException(String.Format("attribute '{1}' of '{0}' object is read-only", dt.Name, SymbolTable.StringToId(_eventInfo.Name)));
                        } else {
                            throw new MissingMemberException(String.Format("'{0}' object has no attribute '{1}'", dt.Name, SymbolTable.StringToId(_eventInfo.Name)));
                        }
                    }
                }

                throw ReadOnlyException(DynamicHelpers.GetPythonTypeFromType(Info.DeclaringType));
            }

            return true;
        }

        private bool EventInfosDiffer(BoundEvent et) {
            // if they're the same object they're the same...
            if (et.Event.Info == this.Info) {
                return false;
            }

            // otherwise compare based upon type & metadata token (they
            // differ by ReflectedType)
            if (et.Event.Info.DeclaringType != Info.DeclaringType ||
                et.Event.Info.MetadataToken != Info.MetadataToken) {
                return true;
            }

            return false;
        }

        internal override bool IsSetDescriptor(CodeContext/*!*/ context, PythonType owner) {
            return true;
        }

        internal override bool TryDeleteValue(CodeContext/*!*/ context, object instance, PythonType owner) {
            Assert.NotNull(context, owner);
            throw ReadOnlyException(DynamicHelpers.GetPythonTypeFromType(Info.DeclaringType));
        }

        internal override bool IsAlwaysVisible {
            get {
                return !_clsOnly;
            }
        }

        private HandlerList/*!*/ GetStubList(object instance) {
#if !SILVERLIGHT
            if (instance != null && ComOps.IsComObject(instance)) {
                return GetComStubList(instance);
            }
#endif

            if (_handlerLists == null) {
                System.Threading.Interlocked.CompareExchange(ref _handlerLists, new WeakDictionary<object, NormalHandlerList>(), null);
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
        private HandlerList/*!*/ GetComStubList(object/*!*/ instance) {
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

        #endregion

        #region Public Python APIs

        public EventInfo/*!*/ Info {
            [PythonHidden]
            get {
                return _eventInfo;
            }
        }

        /// <summary>
        /// BoundEvent is the object that gets returned when the user gets an event object.  An
        /// BoundEvent tracks where the event was received from and is used to verify we get
        /// a proper add when dealing w/ statics events.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")] // TODO: fix
        public class BoundEvent {
            private readonly ReflectedEvent/*!*/ _event;
            private readonly PythonType/*!*/ _ownerType;
            private readonly object _instance;

            public ReflectedEvent/*!*/ Event {
                get {
                    return _event;
                }
            }

            public BoundEvent(ReflectedEvent/*!*/ reflectedEvent, object instance, PythonType/*!*/ ownerType) {
                Assert.NotNull(reflectedEvent, ownerType);

                _event = reflectedEvent;
                _instance = instance;
                _ownerType = ownerType;
            }

            // this one's correct, InPlaceAdd is wrong but we still have some dependencies on the wrong name.
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")] // TODO: fix
            [SpecialName]
            public object op_AdditionAssignment(CodeContext/*!*/ context, object func) {
                return InPlaceAdd(context, func);
            }

            [SpecialName]
            public object InPlaceAdd(CodeContext/*!*/ context, object func) {
                if (func == null) {
                    throw PythonOps.TypeError("event addition expected callable object, got None");
                }

                MethodInfo add = _event.Info.GetAddMethod(true);
                if (add.IsStatic) {
                    if (_ownerType != DynamicHelpers.GetPythonTypeFromType(_event.Info.DeclaringType)) {
                        // mutating static event, only allow this from the type we're mutating, not sub-types
                        return new BadEventChange(_ownerType, _instance);
                    }
                }

                Delegate handler;
                HandlerList stubs;

                // we can add event directly (signature does match):
                if (_event.Info.EventHandlerType.IsAssignableFrom(func.GetType())) {
                    handler = (Delegate)func;
                    stubs = null;
                } else {
                    // create signature converting stub:
                    handler = BinderOps.GetDelegate(context.LanguageContext, func, _event.Info.EventHandlerType);
                    stubs = _event.GetStubList(_instance);
                }

                bool privateBinding = context.LanguageContext.DomainManager.Configuration.PrivateBinding;

                // wire the handler up:
                if (!add.DeclaringType.IsPublic) {
                    add = CompilerHelpers.GetCallableMethod(add, privateBinding);
                }

                if ((add.IsPublic && add.DeclaringType.IsPublic) || privateBinding) {
                    add.Invoke(_instance, new object[] { handler });
                } else {
                    throw new ArgumentTypeException("cannot add to private event");
                }

                if (stubs != null) {
                    // remember the stub so that we could search for it on removal:
                    stubs.AddHandler(func, handler);
                }

                return this;
            }

            [SpecialName]
            public object InPlaceSubtract(CodeContext/*!*/ context, object func) {
                Assert.NotNull(context);
                if (func == null) {
                    throw PythonOps.TypeError("event subtraction expected callable object, got None");
                }

                MethodInfo remove = _event.Info.GetRemoveMethod(true);
                if (remove.IsStatic) {
                    if (_ownerType != DynamicHelpers.GetPythonTypeFromType(_event.Info.DeclaringType)) {
                        // mutating static event, only allow this from the type we're mutating, not sub-types
                        return new BadEventChange(_ownerType, _instance);
                    }
                }

                bool privateBinding = context.LanguageContext.DomainManager.Configuration.PrivateBinding;

                if (!remove.DeclaringType.IsPublic) {
                    remove = CompilerHelpers.GetCallableMethod(remove, privateBinding);
                }

                bool isRemovePublic = remove.IsPublic && remove.DeclaringType.IsPublic;
                if (isRemovePublic || privateBinding) {

                    Delegate handler;

                    if (_event.Info.EventHandlerType.IsAssignableFrom(func.GetType())) {
                        handler = (Delegate)func;
                    } else {
                        handler = _event.GetStubList(_instance).RemoveHandler(context, func);
                    }

                    if (handler != null) {
                        remove.Invoke(_instance, new object[] { handler });
                    }
                } else {
                    throw new ArgumentTypeException("cannot subtract from private event");
                }

                return this;
            }
        }

        public void __set__(CodeContext context, object instance, object value) {
            TrySetValue(context, instance, DynamicHelpers.GetPythonType(instance), value);
        }

        public new void __delete__(CodeContext context, object instance) {
            TryDeleteValue(context, instance, DynamicHelpers.GetPythonType(instance));
        }

        #endregion

        #region Private Helpers

        private class BadEventChange {
            private readonly PythonType/*!*/ _ownerType;
            private readonly object _instance;

            public BadEventChange(PythonType/*!*/ ownerType, object instance) {
                _ownerType = ownerType;
                _instance = instance;
            }

            public PythonType Owner {
                get {
                    return _ownerType;
                }
            }

            public object Instance {
                get {
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Holds on a list of delegates hooked to the event. 
        /// We need the list because we cannot enumerate the delegates hooked to CLR event and we need to do so in 
        /// handler removal (we need to do custom delegate comparison there). If BCL enables the enumeration we could remove this.
        /// </summary>
        private abstract class HandlerList {
            public abstract void AddHandler(object callableObject, Delegate/*!*/ handler);
            public abstract Delegate RemoveHandler(CodeContext/*!*/ context, object callableObject);
        }

#if !SILVERLIGHT
        private sealed class ComHandlerList : HandlerList {
            /// <summary>
            /// Storage for the handlers - a key value pair of the callable object and the delegate handler.
            /// 
            /// The delegate handler is closed over the callable object.  Therefore as long as the object is alive the
            /// delegate will stay alive and so will the callable object.  That means it's fine to have a weak reference
            /// to both of these objects.
            /// </summary>
            private readonly CopyOnWriteList<KeyValuePair<object, object>> _handlers = new CopyOnWriteList<KeyValuePair<object, object>>();

            public override void AddHandler(object callableObject, Delegate/*!*/ handler) {
                Assert.NotNull(handler);
                _handlers.Add(new KeyValuePair<object, object>(callableObject, handler));
            }

            public override Delegate RemoveHandler(CodeContext context, object callableObject) {
                Assert.NotNull(context);

                List<KeyValuePair<object, object>> copyOfHandlers = _handlers.GetCopyForRead();
                for (int i = copyOfHandlers.Count - 1; i >= 0; i--) {
                    object key = copyOfHandlers[i].Key;
                    object value = copyOfHandlers[i].Value;

                    if (PythonOps.EqualRetBool(context, key, callableObject)) {
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
            private readonly CopyOnWriteList<KeyValuePair<WeakReference/*!*/, WeakReference/*!*/>> _handlers = new CopyOnWriteList<KeyValuePair<WeakReference/*!*/, WeakReference/*!*/>>();

            public NormalHandlerList() {
            }

            public override void AddHandler(object callableObject, Delegate/*!*/ handler) {
                Assert.NotNull(handler);
                _handlers.Add(new KeyValuePair<WeakReference/*!*/, WeakReference/*!*/>(new WeakReference(callableObject), new WeakReference(handler)));
            }

            public override Delegate RemoveHandler(CodeContext/*!*/ context, object callableObject) {
                Assert.NotNull(context);

                List<KeyValuePair<WeakReference, WeakReference>> copyOfHandlers = _handlers.GetCopyForRead();
                for (int i = copyOfHandlers.Count - 1; i >= 0; i--) {
                    object key = copyOfHandlers[i].Key.Target;
                    object value = copyOfHandlers[i].Value.Target;

                    if (key != null && value != null && PythonOps.EqualRetBool(context, key, callableObject)) {
                        Delegate handler = (Delegate)value;
                        _handlers.RemoveAt(i);
                        return handler;
                    }
                }

                return null;
            }
        }

        private MissingMemberException/*!*/ ReadOnlyException(PythonType/*!*/ dt) {
            Assert.NotNull(dt);
            return new MissingMemberException(String.Format("attribute '{1}' of '{0}' object is read-only", dt.Name, SymbolTable.StringToId(_eventInfo.Name)));
        }

        #endregion

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            return string.Format("<event# {0} on {1}>", Info.Name, Info.DeclaringType.Name);
        }

        #endregion
    }
}
