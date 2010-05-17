

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 7.00.0500 */
/* at Fri Jan 29 09:30:21 2010
 */
/* Compiler settings for .\COMTest.idl:
    Oicf, W1, Zp8, env=Win32 (32b run)
    protocol : dce , ms_ext, c_ext, robust
    error checks: stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
//@@MIDL_FILE_HEADING(  )

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __COMTest_i_h__
#define __COMTest_i_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __ISimpleComObject_FWD_DEFINED__
#define __ISimpleComObject_FWD_DEFINED__
typedef interface ISimpleComObject ISimpleComObject;
#endif 	/* __ISimpleComObject_FWD_DEFINED__ */


#ifndef ___ISimpleComObjectEvents_FWD_DEFINED__
#define ___ISimpleComObjectEvents_FWD_DEFINED__
typedef interface _ISimpleComObjectEvents _ISimpleComObjectEvents;
#endif 	/* ___ISimpleComObjectEvents_FWD_DEFINED__ */


#ifndef __SimpleComObject_FWD_DEFINED__
#define __SimpleComObject_FWD_DEFINED__

#ifdef __cplusplus
typedef class SimpleComObject SimpleComObject;
#else
typedef struct SimpleComObject SimpleComObject;
#endif /* __cplusplus */

#endif 	/* __SimpleComObject_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"

#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __ISimpleComObject_INTERFACE_DEFINED__
#define __ISimpleComObject_INTERFACE_DEFINED__

/* interface ISimpleComObject */
/* [unique][helpstring][nonextensible][dual][uuid][object] */ 


EXTERN_C const IID IID_ISimpleComObject;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("C819EB8A-97B9-4DF4-9557-FBEE6BD88EF0")
    ISimpleComObject : public IDispatch
    {
    public:
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_FloatProperty( 
            /* [retval][out] */ FLOAT *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_FloatProperty( 
            /* [in] */ FLOAT newVal) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE HelloWorld( 
            /* [retval][out] */ BSTR *pRet) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetProcessThreadID( 
            /* [out] */ LONG *pdwProcessId,
            /* [out] */ LONG *pdwThreadId) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISimpleComObjectVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISimpleComObject * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ 
            __RPC__deref_out  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISimpleComObject * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISimpleComObject * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            ISimpleComObject * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            ISimpleComObject * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            ISimpleComObject * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [range][in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            ISimpleComObject * This,
            /* [in] */ DISPID dispIdMember,
            /* [in] */ REFIID riid,
            /* [in] */ LCID lcid,
            /* [in] */ WORD wFlags,
            /* [out][in] */ DISPPARAMS *pDispParams,
            /* [out] */ VARIANT *pVarResult,
            /* [out] */ EXCEPINFO *pExcepInfo,
            /* [out] */ UINT *puArgErr);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_FloatProperty )( 
            ISimpleComObject * This,
            /* [retval][out] */ FLOAT *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_FloatProperty )( 
            ISimpleComObject * This,
            /* [in] */ FLOAT newVal);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *HelloWorld )( 
            ISimpleComObject * This,
            /* [retval][out] */ BSTR *pRet);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *GetProcessThreadID )( 
            ISimpleComObject * This,
            /* [out] */ LONG *pdwProcessId,
            /* [out] */ LONG *pdwThreadId);
        
        END_INTERFACE
    } ISimpleComObjectVtbl;

    interface ISimpleComObject
    {
        CONST_VTBL struct ISimpleComObjectVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISimpleComObject_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define ISimpleComObject_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define ISimpleComObject_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define ISimpleComObject_GetTypeInfoCount(This,pctinfo)	\
    ( (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo) ) 

#define ISimpleComObject_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    ( (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo) ) 

#define ISimpleComObject_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    ( (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId) ) 

#define ISimpleComObject_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    ( (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr) ) 


#define ISimpleComObject_get_FloatProperty(This,pVal)	\
    ( (This)->lpVtbl -> get_FloatProperty(This,pVal) ) 

#define ISimpleComObject_put_FloatProperty(This,newVal)	\
    ( (This)->lpVtbl -> put_FloatProperty(This,newVal) ) 

#define ISimpleComObject_HelloWorld(This,pRet)	\
    ( (This)->lpVtbl -> HelloWorld(This,pRet) ) 

#define ISimpleComObject_GetProcessThreadID(This,pdwProcessId,pdwThreadId)	\
    ( (This)->lpVtbl -> GetProcessThreadID(This,pdwProcessId,pdwThreadId) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __ISimpleComObject_INTERFACE_DEFINED__ */



#ifndef __COMTestLib_LIBRARY_DEFINED__
#define __COMTestLib_LIBRARY_DEFINED__

/* library COMTestLib */
/* [helpstring][version][uuid] */ 


EXTERN_C const IID LIBID_COMTestLib;

#ifndef ___ISimpleComObjectEvents_DISPINTERFACE_DEFINED__
#define ___ISimpleComObjectEvents_DISPINTERFACE_DEFINED__

/* dispinterface _ISimpleComObjectEvents */
/* [helpstring][uuid] */ 


EXTERN_C const IID DIID__ISimpleComObjectEvents;

#if defined(__cplusplus) && !defined(CINTERFACE)

    MIDL_INTERFACE("D293233D-AB87-464B-B438-28AB8CABAC03")
    _ISimpleComObjectEvents : public IDispatch
    {
    };
    
#else 	/* C style interface */

    typedef struct _ISimpleComObjectEventsVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            _ISimpleComObjectEvents * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ 
            __RPC__deref_out  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            _ISimpleComObjectEvents * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            _ISimpleComObjectEvents * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            _ISimpleComObjectEvents * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            _ISimpleComObjectEvents * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            _ISimpleComObjectEvents * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [range][in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            _ISimpleComObjectEvents * This,
            /* [in] */ DISPID dispIdMember,
            /* [in] */ REFIID riid,
            /* [in] */ LCID lcid,
            /* [in] */ WORD wFlags,
            /* [out][in] */ DISPPARAMS *pDispParams,
            /* [out] */ VARIANT *pVarResult,
            /* [out] */ EXCEPINFO *pExcepInfo,
            /* [out] */ UINT *puArgErr);
        
        END_INTERFACE
    } _ISimpleComObjectEventsVtbl;

    interface _ISimpleComObjectEvents
    {
        CONST_VTBL struct _ISimpleComObjectEventsVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define _ISimpleComObjectEvents_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define _ISimpleComObjectEvents_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define _ISimpleComObjectEvents_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define _ISimpleComObjectEvents_GetTypeInfoCount(This,pctinfo)	\
    ( (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo) ) 

#define _ISimpleComObjectEvents_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    ( (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo) ) 

#define _ISimpleComObjectEvents_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    ( (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId) ) 

#define _ISimpleComObjectEvents_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    ( (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */


#endif 	/* ___ISimpleComObjectEvents_DISPINTERFACE_DEFINED__ */


EXTERN_C const CLSID CLSID_SimpleComObject;

#ifdef __cplusplus

class DECLSPEC_UUID("78A03F3F-C7D0-43A4-B21E-C82CE9BF3267")
SimpleComObject;
#endif
#endif /* __COMTestLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

unsigned long             __RPC_USER  BSTR_UserSize(     unsigned long *, unsigned long            , BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserMarshal(  unsigned long *, unsigned char *, BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserUnmarshal(unsigned long *, unsigned char *, BSTR * ); 
void                      __RPC_USER  BSTR_UserFree(     unsigned long *, BSTR * ); 

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


