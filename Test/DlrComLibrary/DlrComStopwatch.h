// DlrComStopwatch.h : Declaration of the CDlrComStopwatch

#pragma once
#include "resource.h"       // main symbols

#include "DlrComLibrary_i.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CDlrComStopwatch

class ATL_NO_VTABLE CDlrComStopwatch :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CDlrComStopwatch, &CLSID_DlrComStopwatch>,
	public IDispatchImpl<IDlrComStopwatch, &IID_IDlrComStopwatch, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
	LARGE_INTEGER _startCount;

public:
	CDlrComStopwatch()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_DLRCOMSTOPWATCH)


BEGIN_COM_MAP(CDlrComStopwatch)
	COM_INTERFACE_ENTRY(IDlrComStopwatch)
	COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:

	STDMETHOD(Start)(void);
	STDMETHOD(get_ElapsedSeconds)(DOUBLE* pVal);
};

OBJECT_ENTRY_AUTO(__uuidof(DlrComStopwatch), CDlrComStopwatch)
