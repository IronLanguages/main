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

using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Utils;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Scripting.ComInterop {

    internal static class ComRuntimeHelpers {

        internal static void GetInfoFromType(ComTypes.ITypeInfo typeInfo, out string name, out string documentation) {
            int dwHelpContext;
            string strHelpFile;

            typeInfo.GetDocumentation(-1, out name, out documentation, out dwHelpContext, out strHelpFile);
        }

        internal static string GetNameOfMethod(ComTypes.ITypeInfo typeInfo, int memid) {
            int cNames;
            string[] rgNames = new string[1];
            typeInfo.GetNames(memid, rgNames, 1, out cNames);
            return rgNames[0];
        }

        internal static string GetNameOfLib(ComTypes.ITypeLib typeLib) {
            string name;
            string strDocString;
            int dwHelpContext;
            string strHelpFile;

            typeLib.GetDocumentation(-1, out name, out strDocString, out dwHelpContext, out strHelpFile);
            return name;
        }

        internal static string GetNameOfType(ComTypes.ITypeInfo typeInfo) {
            string name;
            string documentation;
            GetInfoFromType(typeInfo, out name, out documentation);

            return name;
        }

        /// <summary>
        /// Look for typeinfo using IDispatch.GetTypeInfo
        /// </summary>
        /// <param name="dispatch"></param>
        /// <param name="throwIfMissingExpectedTypeInfo">
        /// Some COM objects just dont expose typeinfo. In these cases, this method will return null.
        /// Some COM objects do intend to expose typeinfo, but may not be able to do so if the type-library is not properly 
        /// registered. This will be considered as acceptable or as an error condition depending on throwIfMissingExpectedTypeInfo</param>
        /// <returns></returns>
        internal static ComTypes.ITypeInfo GetITypeInfoFromIDispatch(IDispatch dispatch, bool throwIfMissingExpectedTypeInfo) {
            uint typeCount;
            int hresult = dispatch.TryGetTypeInfoCount(out typeCount);
            Marshal.ThrowExceptionForHR(hresult);
            Debug.Assert(typeCount <= 1);
            if (typeCount == 0) {
                return null;
            }

            IntPtr typeInfoPtr = IntPtr.Zero;

            hresult = dispatch.TryGetTypeInfo(0, 0, out typeInfoPtr);
            if (!ComHresults.IsSuccess(hresult)) {
                CheckIfMissingTypeInfoIsExpected(hresult, throwIfMissingExpectedTypeInfo);
                return null;
            }
            if (typeInfoPtr == IntPtr.Zero) { // be defensive against components that return IntPtr.Zero
                if (throwIfMissingExpectedTypeInfo) {
                    Marshal.ThrowExceptionForHR(ComHresults.E_FAIL);
                }
                return null;
            }

            ComTypes.ITypeInfo typeInfo = null;
            try {
                typeInfo = Marshal.GetObjectForIUnknown(typeInfoPtr) as ComTypes.ITypeInfo;
            } finally {
                Marshal.Release(typeInfoPtr);
            }

            return typeInfo;
        }

        /// <summary>
        /// This method should be called when typeinfo is not available for an object. The function
        /// will check if the typeinfo is expected to be missing. This can include error cases where
        /// the same error is guaranteed to happen all the time, on all machines, under all circumstances.
        /// In such cases, we just have to operate without the typeinfo.
        /// 
        /// However, if accessing the typeinfo is failing in a transient way, we might want to throw
        /// an exception so that we will eagerly predictably indicate the problem.
        /// </summary>
        private static void CheckIfMissingTypeInfoIsExpected(int hresult, bool throwIfMissingExpectedTypeInfo) {
            Debug.Assert(!ComHresults.IsSuccess(hresult));

            // Word.Basic always returns this because of an incorrect implementation of IDispatch.GetTypeInfo
            // Any implementation that returns E_NOINTERFACE is likely to do so in all environments
            if (hresult == ComHresults.E_NOINTERFACE) {
                return;
            }

            // This assert is potentially over-restrictive since COM components can behave in quite unexpected ways.
            // However, asserting the common expected cases ensures that we find out about the unexpected scenarios, and
            // can investigate the scenarios to ensure that there is no bug in our own code.
            Debug.Assert(hresult == ComHresults.TYPE_E_LIBNOTREGISTERED);

            if (throwIfMissingExpectedTypeInfo) {
                Marshal.ThrowExceptionForHR(hresult);
            }
        }

        // TODO: we should be throwing a different exception here
        // COMException sounds right but it has a specific meaning to the
        // runtime
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        internal static ComTypes.TYPEATTR GetTypeAttrForTypeInfo(ComTypes.ITypeInfo typeInfo) {
            IntPtr pAttrs = IntPtr.Zero;
            typeInfo.GetTypeAttr(out pAttrs);

            // GetTypeAttr should never return null, this is just to be safe
            if (pAttrs == IntPtr.Zero) {
                throw new COMException("Cannot retrieve type information");
            }

            try {
                return (ComTypes.TYPEATTR)Marshal.PtrToStructure(pAttrs, typeof(ComTypes.TYPEATTR));
            } finally {
                typeInfo.ReleaseTypeAttr(pAttrs);
            }
        }

        // TODO: we should be throwing a different exception here
        // COMException sounds right but it has a specific meaning to the
        // runtime
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        internal static ComTypes.TYPELIBATTR GetTypeAttrForTypeLib(ComTypes.ITypeLib typeLib) {
            IntPtr pAttrs = IntPtr.Zero;
            typeLib.GetLibAttr(out pAttrs);

            // GetTypeAttr should never return null, this is just to be safe
            if (pAttrs == IntPtr.Zero) {
                throw new COMException("Cannot retrieve type information");
            }

            try {
                return (ComTypes.TYPELIBATTR)Marshal.PtrToStructure(pAttrs, typeof(ComTypes.TYPELIBATTR));
            } finally {
                typeLib.ReleaseTLibAttr(pAttrs);
            }
        }
    }

    public static class UnsafeNativeMethods {
        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.None)]
        [System.Runtime.Versioning.ResourceConsumption(System.Runtime.Versioning.ResourceScope.Process, System.Runtime.Versioning.ResourceScope.Process)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")] // TODO: fix
        [DllImport("oleaut32.dll", PreserveSig = false)]
        internal static extern void VariantClear(IntPtr variant);

        [System.Runtime.Versioning.ResourceExposure(System.Runtime.Versioning.ResourceScope.Machine)]
        [System.Runtime.Versioning.ResourceConsumption(System.Runtime.Versioning.ResourceScope.Machine, System.Runtime.Versioning.ResourceScope.Machine)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")] // TODO: fix
        [DllImport("oleaut32.dll", PreserveSig = false)]
        internal static extern ComTypes.ITypeLib LoadRegTypeLib(ref Guid clsid, short majorVersion, short minorVersion, int lcid);
    }
}

#endif
