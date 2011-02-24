// DlrUniversalObj.h : Declaration of the CDlrUniversalObj

#pragma once
#include "resource.h"       // main symbols

#include "DlrComLibrary_i.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CDlrUniversalObj

class ATL_NO_VTABLE CDlrUniversalObj :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CDlrUniversalObj, &CLSID_DlrUniversalObj>,
	public IDispatchImpl<IDlrUniversalObj, &IID_IDlrUniversalObj, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CDlrUniversalObj()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_DLRUNIVERSALOBJ)


BEGIN_COM_MAP(CDlrUniversalObj)
	COM_INTERFACE_ENTRY(IDlrUniversalObj)
	COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()



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

	STDMETHOD(m0)(void);
	STDMETHOD(m2)(BSTR arg1, BSTR arg2);
	STDMETHOD(m1kw1)(VARIANT arg1, VARIANT arg2);

	static BOOL AllDestructed() { return s_cConstructed == s_cReleased; }

private:
    static int s_cConstructed;
    static int s_cReleased;

};

OBJECT_ENTRY_AUTO(__uuidof(DlrUniversalObj), CDlrUniversalObj)
