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

#if !SILVERLIGHT // ComObject

using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Dynamic.Utils;
using System.Globalization;
using System.Linq.Expressions;
using System.Linq.Expressions.Compiler;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace System.Dynamic.ComInterop {

    /// <summary>
    /// An object that implements IDispatch
    /// 
    /// This currently has the following issues:
    /// 1. If we prefer ComObjectWithTypeInfo over IDispatchComObject, then we will often not
    ///    IDispatchComObject since implementations of IDispatch often rely on a registered type library. 
    ///    If we prefer IDispatchComObject over ComObjectWithTypeInfo, users get a non-ideal experience.
    /// 2. IDispatch cannot distinguish between properties and methods with 0 arguments (and non-0 
    ///    default arguments?). So obj.foo() is ambiguous as it could mean invoking method foo, 
    ///    or it could mean invoking the function pointer returned by property foo.
    ///    We are attempting to find whether we need to call a method or a property by examining
    ///    the ITypeInfo associated with the IDispatch. ITypeInfo tell's use what parameters the method
    ///    expects, is it a method or a property, what is the default property of the object, how to 
    ///    create an enumerator for collections etc.
    /// 3. IronPython processes the signature and converts ref arguments into return values. 
    ///    However, since the signature of a DispMethod is not available beforehand, this conversion 
    ///    is not possible. There could be other signature conversions that may be affected. How does 
    ///    VB6 deal with ref arguments and IDispatch?
    ///    
    /// We also support events for IDispatch objects:
    /// Background:
    /// COM objects support events through a mechanism known as Connect Points.
    /// Connection Points are separate objects created off the actual COM 
    /// object (this is to prevent circular references between event sink
    /// and event source). When clients want to sink events generated  by 
    /// COM object they would implement callback interfaces (aka source 
    /// interfaces) and hand it over (advise) to the Connection Point. 
    /// 
    /// Implementation details:
    /// When IDispatchComObject.TryGetMember request is received we first check
    /// whether the requested member is a property or a method. If this check
    /// fails we will try to determine whether an event is requested. To do 
    /// so we will do the following set of steps:
    /// 1. Verify the COM object implements IConnectionPointContainer
    /// 2. Attempt to find COM object's coclass's description
    ///    a. Query the object for IProvideClassInfo interface. Go to 3, if found
    ///    b. From object's IDispatch retrieve primary interface description
    ///    c. Scan coclasses declared in object's type library.
    ///    d. Find coclass implementing this particular primary interface 
    /// 3. Scan coclass for all its source interfaces.
    /// 4. Check whether to any of the methods on the source interfaces matches 
    /// the request name
    /// 
    /// Once we determine that TryGetMember requests an event we will return
    /// an instance of BoundDispEvent class. This class has InPlaceAdd and
    /// InPlaceSubtract operators defined. Calling InPlaceAdd operator will:
    /// 1. An instance of ComEventSinksContainer class is created (unless 
    /// RCW already had one). This instance is hanged off the RCW in attempt
    /// to bind the lifetime of event sinks to the lifetime of the RCW itself,
    /// meaning event sink will be collected once the RCW is collected (this
    /// is the same way event sinks lifetime is controlled by PIAs).
    /// Notice: ComEventSinksContainer contains a Finalizer which will go and
    /// unadvise all event sinks.
    /// Notice: ComEventSinksContainer is a list of ComEventSink objects. 
    /// 2. Unless we have already created a ComEventSink for the required 
    /// source interface, we will create and advise a new ComEventSink. Each
    /// ComEventSink implements a single source interface that COM object 
    /// supports. 
    /// 3. ComEventSink contains a map between method DISPIDs to  the 
    /// multicast delegate that will be invoked when the event is raised.
    /// 4. ComEventSink implements IReflect interface which is exposed as
    /// custom IDispatch to COM consumers. This allows us to intercept calls
    /// to IDispatch.Invoke and apply custom logic - in particular we will
    /// just find and invoke the multicast delegate corresponding to the invoked
    /// dispid.
    ///  </summary>

    internal sealed class IDispatchComObject : ComObject, IDynamicObject {

        private readonly IDispatch _dispatchObject;
        private ComTypeDesc _comTypeDesc;
        private static Dictionary<Guid, ComTypeDesc> _CacheComTypeDesc = new Dictionary<Guid, ComTypeDesc>();

        internal IDispatchComObject(IDispatch rcw)
            : base(rcw) {
            _dispatchObject = rcw;
        }

        public override string ToString() {
            EnsureScanDefinedMethods();

            string typeName = this._comTypeDesc.TypeName;
            if (String.IsNullOrEmpty(typeName))
                typeName = "IDispatch";

            return String.Format(CultureInfo.CurrentCulture, "{0} ({1})", RuntimeCallableWrapper.ToString(), typeName);
        }

        public ComTypeDesc ComTypeDesc {
            get {
                EnsureScanDefinedMethods();
                return _comTypeDesc;
            }
        }

        public IDispatch DispatchObject {
            get {
                return _dispatchObject;
            }
        }

        private static int GetIDsOfNames(IDispatch dispatch, string name, out int dispId) {
            int[] dispIds = new int[1];
            Guid emtpyRiid = Guid.Empty;
            int hresult = dispatch.TryGetIDsOfNames(
                ref emtpyRiid,
                new string[] { name },
                1,
                0,
                dispIds);

            dispId = dispIds[0];
            return hresult;
        }

        static int Invoke(IDispatch dispatch, int memberDispId, out object result) {
            Guid emtpyRiid = Guid.Empty;
            ComTypes.DISPPARAMS dispParams = new ComTypes.DISPPARAMS();
            ComTypes.EXCEPINFO excepInfo = new ComTypes.EXCEPINFO();
            uint argErr;
            int hresult = dispatch.TryInvoke(
                memberDispId,
                ref emtpyRiid,
                0,
                ComTypes.INVOKEKIND.INVOKE_PROPERTYGET,
                ref dispParams,
                out result,
                out excepInfo,
                out argErr);

            return hresult;
        }

        public bool TryGetGetItem(out ComMethodDesc value) {
            ComMethodDesc methodDesc = _comTypeDesc.GetItem;
            if (methodDesc != null) {
                value = methodDesc;
                return true;
            }

            return SlowTryGetGetItem(out value);
        }

        private bool SlowTryGetGetItem(out ComMethodDesc value) {
            EnsureScanDefinedMethods();

            ComMethodDesc methodDesc = _comTypeDesc.GetItem;

            // The following attempts to get a method corresponding to "[PROPERTYGET, DISPID(0)] HRESULT Item(...)".
            // However, without type information, we really don't know whether or not we have a property getter.
            // All we can do is verify that the found dispId is DISPID_VALUE.  So, if we find a dispId of DISPID_VALUE,
            // we happily package it up as a property getter; otherwise, it's a no go...
            if (methodDesc == null) {
                int dispId;
                string name = "Item";
                int hresult = GetIDsOfNames(_dispatchObject, name, out dispId);
                if (hresult == ComHresults.DISP_E_UNKNOWNNAME) {
                    value = null;
                    return false;
                } else if (hresult != ComHresults.S_OK) {
                    throw Error.CouldNotGetDispId(name, string.Format(CultureInfo.InvariantCulture, "0x{1:X})", hresult));
                } else if (dispId != ComDispIds.DISPID_VALUE) {
                    value = null;
                    return false;
                }

                _comTypeDesc.EnsureGetItem(new ComMethodDesc(name, dispId, ComTypes.INVOKEKIND.INVOKE_PROPERTYGET));
                methodDesc = _comTypeDesc.GetItem;
            }

            value = methodDesc;
            return true;
        }

        public bool TryGetSetItem(out ComMethodDesc value) {
            ComMethodDesc methodDesc = _comTypeDesc.SetItem;
            if (methodDesc != null) {
                value = methodDesc;
                return true;
            }

            return SlowTryGetSetItem(out value);
        }

        public bool SlowTryGetSetItem(out ComMethodDesc value) {
            EnsureScanDefinedMethods();

            ComMethodDesc methodDesc = _comTypeDesc.SetItem;

            // The following attempts to get a method corresponding to "[PROPERTYPUT, DISPID(0)] HRESULT Item(...)".
            // However, without type information, we really don't know whether or not we have a property setter.
            // All we can do is verify that the found dispId is DISPID_VALUE.  So, if we find a dispId of DISPID_VALUE,
            // we happily package it up as a property setter; otherwise, it's a no go...
            if (methodDesc == null) {
                int dispId;
                string name = "Item";
                int hresult = GetIDsOfNames(_dispatchObject, name, out dispId);
                if (hresult == ComHresults.DISP_E_UNKNOWNNAME) {
                    value = null;
                    return false;
                } else if (hresult != ComHresults.S_OK) {
                    throw Error.CouldNotGetDispId(name, string.Format(CultureInfo.InvariantCulture, "0x{1:X})", hresult));
                } else if (dispId != ComDispIds.DISPID_VALUE) {
                    value = null;
                    return false;
                }

                _comTypeDesc.EnsureSetItem(new ComMethodDesc(name, dispId, ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT));
                methodDesc = _comTypeDesc.SetItem;
            }

            value = methodDesc;
            return true;
        }

        internal bool TryGetIDOfName(string name) {
            int dispId;
            return GetIDsOfNames(_dispatchObject, name, out dispId) == ComHresults.S_OK;
        }

        internal bool TryGetMemberMethod(string name, out ComMethodDesc method) {
            EnsureScanDefinedMethods();
            return _comTypeDesc.Funcs.TryGetValue(name, out method);
        }

        internal bool TryGetMemberEvent(string name, out ComEventDesc @event) {
            EnsureScanDefinedEvents();
            return _comTypeDesc.Events.TryGetValue(name, out @event);
        }

        internal bool TryGetMemberMethodExplicit(string name, out ComMethodDesc method) {
            EnsureScanDefinedMethods();

            // TODO: We have a thread-safety issue here right now
            // TODO: since we are mutating _funcs array
            // TODO: The workaround is to use Hashtable (which is thread-safe
            // TODO: on read operations) to fetch the value out.
            int dispId;
            int hresult = GetIDsOfNames(_dispatchObject, name, out dispId);

            if (hresult == ComHresults.S_OK) {
                ComMethodDesc cmd = new ComMethodDesc(name, dispId);
                _comTypeDesc.Funcs.Add(name, cmd);
                method = cmd;
                return true;
            } else if (hresult == ComHresults.DISP_E_UNKNOWNNAME) {
                method = null;
                return false;
            } else {
                throw Error.CouldNotGetDispId(name, string.Format(CultureInfo.InvariantCulture, "0x{1:X})", hresult));
            }
        }

        internal bool TryGetPropertySetterExplicit(string name, out ComMethodDesc method, Type limitType) {
            EnsureScanDefinedMethods();

            // TODO: We have a thread-safety issue here right now
            // TODO: since we are mutating _funcs array
            // TODO: The workaround is to use Hashtable (which is thread-safe
            // TODO: on read operations) to fetch the value out.
            int dispId;
            int hresult = GetIDsOfNames(_dispatchObject, name, out dispId);

            if (hresult == ComHresults.S_OK) {
                // we do not know whether we have put or putref here
                // and we will not guess and pretend we found both.
                ComMethodDesc put = new ComMethodDesc(name, dispId, ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT);
                _comTypeDesc.Puts.Add(name, put);

                ComMethodDesc putref = new ComMethodDesc(name, dispId, ComTypes.INVOKEKIND.INVOKE_PROPERTYPUTREF);
                _comTypeDesc.PutRefs.Add(name, putref);

                if (ComBinderHelpers.PreferPut(limitType)) {
                    method = put;
                } else {
                    method = putref;
                }
                return true;
            } else if (hresult == ComHresults.DISP_E_UNKNOWNNAME) {
                method = null;
                return false;
            } else {
                throw Error.CouldNotGetDispId(name, string.Format(CultureInfo.InvariantCulture, "0x{1:X})", hresult));
            }
        }

        internal override IEnumerable<string> MemberNames {
            get {
                EnsureScanDefinedMethods();
                EnsureScanDefinedEvents();

                var names = new List<string>();

                foreach (string name in _comTypeDesc.Funcs.Keys) {
                    names.Add(name);
                }

                if (_comTypeDesc.Events != null && _comTypeDesc.Events.Count > 0) {
                    foreach (string name in _comTypeDesc.Events.Keys) {
                        names.Add(name);
                    }
                }

                return names.ToArray();
            }
        }

        internal override IEnumerable<KeyValuePair<string, object>> DataMembers {
            get {
                EnsureScanDefinedMethods();

                Type comType = RuntimeCallableWrapper.GetType();
                var members = new List<KeyValuePair<string, object>>();
                foreach (ComMethodDesc method in _comTypeDesc.Funcs.Values) {
                    if (method.IsDataMember) {
                        object value = comType.InvokeMember(method.Name, BindingFlags.GetProperty, null, RuntimeCallableWrapper, EmptyArray<object>.Instance, CultureInfo.InvariantCulture);
                        members.Add(new KeyValuePair<string, object>(method.Name, value));
                    }
                }

                return members.ToArray();
            }
        }

        MetaObject IDynamicObject.GetMetaObject(Expression parameter) {
            EnsureScanDefinedMethods();
            return new IDispatchMetaObject(parameter, this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        internal static void GetFuncDescForDescIndex(ComTypes.ITypeInfo typeInfo, int funcIndex, out ComTypes.FUNCDESC funcDesc, out IntPtr funcDescHandle) {
            IntPtr pFuncDesc = IntPtr.Zero;
            typeInfo.GetFuncDesc(funcIndex, out pFuncDesc);

            // GetFuncDesc should never return null, this is just to be safe
            if (pFuncDesc == IntPtr.Zero) {
                throw Error.CannotRetrieveTypeInformation();
            }

            funcDesc = (ComTypes.FUNCDESC)Marshal.PtrToStructure(pFuncDesc, typeof(ComTypes.FUNCDESC));
            funcDescHandle = pFuncDesc;
        }

        private void EnsureScanDefinedEvents() {

            // _comTypeDesc.Events is null if we have not yet attempted
            // to scan the object for events.
            if (_comTypeDesc != null && _comTypeDesc.Events != null) {
                return;
            }

            // check type info in the type descriptions cache
            ComTypes.ITypeInfo typeInfo = ComRuntimeHelpers.GetITypeInfoFromIDispatch(_dispatchObject, true);
            if (typeInfo == null) {
                _comTypeDesc = ComTypeDesc.CreateEmptyTypeDesc();
                return;
            }

            ComTypes.TYPEATTR typeAttr = ComRuntimeHelpers.GetTypeAttrForTypeInfo(typeInfo);

            if (_comTypeDesc == null) {
                lock (_CacheComTypeDesc) {
                    if (_CacheComTypeDesc.TryGetValue(typeAttr.guid, out _comTypeDesc) == true &&
                        _comTypeDesc.Events != null) {
                        return;
                    }
                }
            }

            ComTypeDesc typeDesc = ComTypeDesc.FromITypeInfo(typeInfo);

            ComTypes.ITypeInfo classTypeInfo = null;
            Dictionary<string, ComEventDesc> events = null;

            var cpc = RuntimeCallableWrapper as ComTypes.IConnectionPointContainer;
            if (cpc == null) {
                // No ICPC - this object does not support events
                events = ComTypeDesc.EmptyEvents;
            } else if ((classTypeInfo = GetCoClassTypeInfo(this.RuntimeCallableWrapper, typeInfo)) == null) {
                // no class info found - this object may support events
                // but we could not discover those
                events = ComTypeDesc.EmptyEvents;
            } else {
                events = new Dictionary<string, ComEventDesc>();

                ComTypes.TYPEATTR classTypeAttr = ComRuntimeHelpers.GetTypeAttrForTypeInfo(classTypeInfo);
                for (int i = 0; i < classTypeAttr.cImplTypes; i++) {
                    int hRefType;
                    classTypeInfo.GetRefTypeOfImplType(i, out hRefType);

                    ComTypes.ITypeInfo interfaceTypeInfo;
                    classTypeInfo.GetRefTypeInfo(hRefType, out interfaceTypeInfo);

                    ComTypes.IMPLTYPEFLAGS flags;
                    classTypeInfo.GetImplTypeFlags(i, out flags);
                    if ((flags & ComTypes.IMPLTYPEFLAGS.IMPLTYPEFLAG_FSOURCE) != 0) {
                        ScanSourceInterface(interfaceTypeInfo, ref events);
                    }
                }

                if (events.Count == 0) {
                    events = ComTypeDesc.EmptyEvents;
                }
            }

            lock (_CacheComTypeDesc) {
                ComTypeDesc cachedTypeDesc;
                if (_CacheComTypeDesc.TryGetValue(typeAttr.guid, out cachedTypeDesc)) {
                    _comTypeDesc = cachedTypeDesc;
                } else {
                    _comTypeDesc = typeDesc;
                    _CacheComTypeDesc.Add(typeAttr.guid, _comTypeDesc);
                }
                _comTypeDesc.Events = events;
            }

        }

        private static void ScanSourceInterface(ComTypes.ITypeInfo sourceTypeInfo, ref Dictionary<string, ComEventDesc> events) {
            ComTypes.TYPEATTR sourceTypeAttribute = ComRuntimeHelpers.GetTypeAttrForTypeInfo(sourceTypeInfo);

            for (int index = 0; index < sourceTypeAttribute.cFuncs; index++) {
                IntPtr funcDescHandleToRelease = IntPtr.Zero;

                try {
                    ComTypes.FUNCDESC funcDesc;
                    GetFuncDescForDescIndex(sourceTypeInfo, index, out funcDesc, out funcDescHandleToRelease);

                    // we are not interested in hidden or restricted functions for now.
                    if ((funcDesc.wFuncFlags & (int)ComTypes.FUNCFLAGS.FUNCFLAG_FHIDDEN) != 0) {
                        continue;
                    }
                    if ((funcDesc.wFuncFlags & (int)ComTypes.FUNCFLAGS.FUNCFLAG_FRESTRICTED) != 0) {
                        continue;
                    }

                    string name = ComRuntimeHelpers.GetNameOfMethod(sourceTypeInfo, funcDesc.memid);

                    // Sometimes coclass has multiple source interfaces. Usually this is caused by
                    // adding new events and putting them on new interfaces while keeping the
                    // old interfaces around. This may cause name collisioning which we are
                    // resolving by keeping only the first event with the same name.
                    if (events.ContainsKey(name) == false) {
                        ComEventDesc eventDesc = new ComEventDesc();
                        eventDesc.dispid = funcDesc.memid;
                        eventDesc.sourceIID = sourceTypeAttribute.guid;
                        events.Add(name, eventDesc);
                    }
                } finally {
                    if (funcDescHandleToRelease != IntPtr.Zero) {
                        sourceTypeInfo.ReleaseFuncDesc(funcDescHandleToRelease);
                    }
                }
            }
        }

        private static ComTypes.ITypeInfo GetCoClassTypeInfo(object rcw, ComTypes.ITypeInfo typeInfo) {
            Debug.Assert(typeInfo != null);

            IProvideClassInfo provideClassInfo = rcw as IProvideClassInfo;
            if (provideClassInfo != null) {
                IntPtr typeInfoPtr = IntPtr.Zero;
                try {
                    provideClassInfo.GetClassInfo(out typeInfoPtr);
                    if (typeInfoPtr != IntPtr.Zero) {
                        return Marshal.GetObjectForIUnknown(typeInfoPtr) as ComTypes.ITypeInfo;
                    }
                } finally {
                    if (typeInfoPtr != IntPtr.Zero) {
                        Marshal.Release(typeInfoPtr);
                    }
                }
            }

            // retrieving class information through IPCI has failed - 
            // we can try scanning the typelib to find the coclass

            ComTypes.ITypeLib typeLib;
            int typeInfoIndex;
            typeInfo.GetContainingTypeLib(out typeLib, out typeInfoIndex);
            string typeName = ComRuntimeHelpers.GetNameOfType(typeInfo);

            ComTypeLibDesc typeLibDesc = ComTypeLibDesc.GetFromTypeLib(typeLib);
            ComTypeClassDesc coclassDesc = typeLibDesc.GetCoClassForInterface(typeName);
            if (coclassDesc == null) {
                return null;
            }

            ComTypes.ITypeInfo typeInfoCoClass;
            Guid coclassGuid = coclassDesc.Guid;
            typeLib.GetTypeInfoOfGuid(ref coclassGuid, out typeInfoCoClass);
            return typeInfoCoClass;
        }

        private void EnsureScanDefinedMethods() {
            if (_comTypeDesc != null && _comTypeDesc.Funcs != null)
                return;

            ComTypes.ITypeInfo typeInfo = ComRuntimeHelpers.GetITypeInfoFromIDispatch(_dispatchObject, true);
            if (typeInfo == null) {
                _comTypeDesc = ComTypeDesc.CreateEmptyTypeDesc();
                return;
            }

            ComTypes.TYPEATTR typeAttr = ComRuntimeHelpers.GetTypeAttrForTypeInfo(typeInfo);

            if (_comTypeDesc == null) {
                lock (_CacheComTypeDesc) {
                    if (_CacheComTypeDesc.TryGetValue(typeAttr.guid, out _comTypeDesc) == true &&
                        _comTypeDesc.Funcs != null) {
                        return;
                    }
                }
            }

            ComTypeDesc typeDesc = ComTypeDesc.FromITypeInfo(typeInfo);

            ComMethodDesc getItem = null;
            ComMethodDesc setItem = null;
            Dictionary<string, ComMethodDesc> funcs = new Dictionary<string, ComMethodDesc>(typeAttr.cFuncs);
            Dictionary<string, ComMethodDesc> puts = new Dictionary<string, ComMethodDesc>();
            Dictionary<string, ComMethodDesc> putrefs = new Dictionary<string, ComMethodDesc>();
            Set<int> usedDispIds = new Set<int>();

            for (int definedFuncIndex = 0; definedFuncIndex < typeAttr.cFuncs; definedFuncIndex++) {
                IntPtr funcDescHandleToRelease = IntPtr.Zero;

                try {
                    ComTypes.FUNCDESC funcDesc;
                    GetFuncDescForDescIndex(typeInfo, definedFuncIndex, out funcDesc, out funcDescHandleToRelease);

                    if ((funcDesc.wFuncFlags & (int)ComTypes.FUNCFLAGS.FUNCFLAG_FRESTRICTED) != 0) {
                        // This function is not meant for the script user to use.
                        continue;
                    }

                    ComMethodDesc method = new ComMethodDesc(typeInfo, funcDesc);

                    if ((funcDesc.invkind & ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT) != 0) {
                        puts.Add(method.Name, method);
                        continue;
                    }
                    if ((funcDesc.invkind & ComTypes.INVOKEKIND.INVOKE_PROPERTYPUTREF) != 0) {
                        putrefs.Add(method.Name, method);
                        continue;
                    }

                    usedDispIds.Add(funcDesc.memid);

                    if (funcDesc.memid == ComDispIds.DISPID_NEWENUM) {
                        funcs.Add("GetEnumerator", method);
                        continue;
                    }

                    funcs.Add(method.Name, method);

                    // for the special dispId == 0, we need to store the method descriptor 
                    // for the Do(GetItem) binder. 
                    if (funcDesc.memid == ComDispIds.DISPID_VALUE) {
                        getItem = method;
                    }
                } finally {
                    if (funcDescHandleToRelease != IntPtr.Zero) {
                        typeInfo.ReleaseFuncDesc(funcDescHandleToRelease);
                    }
                }
            }

            ProcessPut(funcs, puts, usedDispIds, ref setItem);
            ProcessPut(funcs, putrefs, usedDispIds, ref setItem);

            lock (_CacheComTypeDesc) {
                ComTypeDesc cachedTypeDesc;
                if (_CacheComTypeDesc.TryGetValue(typeAttr.guid, out cachedTypeDesc)) {
                    _comTypeDesc = cachedTypeDesc;
                } else {
                    _comTypeDesc = typeDesc;
                    _CacheComTypeDesc.Add(typeAttr.guid, _comTypeDesc);
                }
                _comTypeDesc.Funcs = funcs;
                _comTypeDesc.Puts = puts;
                _comTypeDesc.PutRefs = putrefs;
                _comTypeDesc.EnsureGetItem(getItem);
                _comTypeDesc.EnsureSetItem(setItem);
            }
        }

        private static void ProcessPut(Dictionary<string, ComMethodDesc> funcs, Dictionary<string, ComMethodDesc> methods, Set<int> usedDispIds, ref ComMethodDesc setItem) {
            foreach (ComMethodDesc method in methods.Values) {
                if (!usedDispIds.Contains(method.DispId)) {
                    funcs.Add(method.Name, method);
                    usedDispIds.Add(method.DispId);
                }

                // for the special dispId == 0, we need to store
                // the method descriptor for the Do(SetItem) binder. 
                if (method.DispId == ComDispIds.DISPID_VALUE && setItem == null) {
                    setItem = method;
                }
            }
        }

        internal bool TryGetPropertySetter(string name, out ComMethodDesc method, Type limitType) {
            EnsureScanDefinedMethods();

            if (ComBinderHelpers.PreferPut(limitType)) {
                return _comTypeDesc.Puts.TryGetValue(name, out method) ||
                    _comTypeDesc.PutRefs.TryGetValue(name, out method);
            } else {
                return _comTypeDesc.PutRefs.TryGetValue(name, out method) ||
                    _comTypeDesc.Puts.TryGetValue(name, out method);
            }
        }

        internal bool TryGetEventHandler(string name, out ComEventDesc @event) {
            EnsureScanDefinedEvents();
            return _comTypeDesc.Events.TryGetValue(name, out @event);
        }
    }
}

#endif
