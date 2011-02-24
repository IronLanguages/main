// DlrComServer.h : Declaration of the CDlrComServer

#pragma once
#include "resource.h"       // main symbols

#include "DlrComLibrary_i.h"
#include "_IDlrComServerEvents_CP.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CDlrComServer

typedef CComEnum<IEnumVARIANT, &IID_IEnumVARIANT, VARIANT,
                              _Copy<VARIANT> > VariantComEnum;

#define NUMELEMENTS 3

class ATL_NO_VTABLE CDlrComServer :
    public CComObjectRootEx<CComSingleThreadModel>,
    public CComCoClass<CDlrComServer, &CLSID_DlrComServer>,
    public ISupportErrorInfo,
    public IConnectionPointContainerImpl<CDlrComServer>,
    public CProxy_IDlrComServerEvents<CDlrComServer>,
    public IDispatchImpl<IDlrComServer, &IID_IDlrComServer, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
    CDlrComServer()
    {
		//Initialize the Variant array.
		for(int i =0; i < NUMELEMENTS; i++)
			VariantInit(&m_arr[i]);

		m_arr[0].vt = VT_I4;
		m_arr[0].lVal = 42;
		m_arr[1].vt = VT_BOOL;
		m_arr[1].boolVal = true;
		m_arr[2].vt = VT_BSTR;
		m_arr[2].bstrVal = SysAllocString(_T("DLR"));
    }

DECLARE_REGISTRY_RESOURCEID(IDR_DLRCOMSERVER)


BEGIN_COM_MAP(CDlrComServer)
    COM_INTERFACE_ENTRY(IDlrComServer)
    COM_INTERFACE_ENTRY2(IDispatch, IDlrComServer)
    COM_INTERFACE_ENTRY(ISupportErrorInfo)
    COM_INTERFACE_ENTRY(IConnectionPointContainer)
END_COM_MAP()

BEGIN_CONNECTION_POINT_MAP(CDlrComServer)
    CONNECTION_POINT_ENTRY(__uuidof(_IDlrComServerEvents))
END_CONNECTION_POINT_MAP()
// ISupportsErrorInfo
    STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);


    DECLARE_PROTECT_FINAL_CONSTRUCT()

    HRESULT FinalConstruct()
    {
        s_cConstructed++;

        return S_OK;
    }

    void FinalRelease()
    {
        s_cReleased++;
    }	

public:
    STDMETHOD(SimpleMethod)(void);
    STDMETHOD(IntArguments)(LONG arg1, LONG arg2);
    STDMETHOD(StringArguments)(BSTR arg1, BSTR arg2);
    STDMETHOD(ObjectArguments)(IUnknown* arg1, IUnknown* arg2);
    STDMETHOD(TestErrorInfo)(void);
    STDMETHOD(GetByteArray)(SAFEARRAY** ppsaRetVal);
    STDMETHOD(GetIntArray)(SAFEARRAY** ppsaRetVal);
    STDMETHOD(GetObjArray)(SAFEARRAY** ppsaRetVal);
	STDMETHOD(SumArgs)(LONG a1, LONG a2, LONG a3, LONG a4, LONG a5, LONG* result);
	STDMETHOD(get__NewEnum)(IUnknown** ppUnk);	

    static BOOL AllDestructed() { return s_cConstructed == s_cReleased; }

private:
    static int s_cConstructed;
    static int s_cReleased;
	VARIANT m_arr[NUMELEMENTS];
};

OBJECT_ENTRY_AUTO(__uuidof(DlrComServer), CDlrComServer)
