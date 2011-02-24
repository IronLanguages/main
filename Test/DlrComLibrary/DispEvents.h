// DispEvents.h : Declaration of the CDispEvents

#pragma once
#include "resource.h"       // main symbols

#include "DlrComLibrary_i.h"
#include "_IDispEventsEvents_CP.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CDispEvents

class ATL_NO_VTABLE CDispEvents :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CDispEvents, &CLSID_DispEvents>,
	public IConnectionPointContainerImpl<CDispEvents>,
	public CProxy_IDispEventsEvents<CDispEvents>,
	public IDispatchImpl<IDispEvents, &IID_IDispEvents, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CDispEvents()
		: m_neg_scenario(false)
		, m_expected_hresult(0)
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_DISPEVENTS)


BEGIN_COM_MAP(CDispEvents)
	COM_INTERFACE_ENTRY(IDispEvents)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(IConnectionPointContainer)
END_COM_MAP()

BEGIN_CONNECTION_POINT_MAP(CDispEvents)
	CONNECTION_POINT_ENTRY(__uuidof(_IDispEventsEvents))
END_CONNECTION_POINT_MAP()


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

	STDMETHOD(get_neg_scenario)(VARIANT_BOOL* pVal);
	STDMETHOD(put_neg_scenario)(VARIANT_BOOL newVal);
	STDMETHOD(get_expected_hresult)(ULONG* pVal);
	STDMETHOD(put_expected_hresult)(ULONG newVal);
	STDMETHOD(triggerNull)(void);
	STDMETHOD(triggerInOutretBool)(VARIANT_BOOL inval, VARIANT_BOOL* ret);
	STDMETHOD(triggerInOutBstr)(BSTR inval, BSTR* outval);
	STDMETHOD(triggerUShort)(USHORT inval);
	STDMETHOD(triggerNullShort)(void);

	static BOOL AllDestructed() { return s_cConstructed == s_cReleased; }

private:
	bool m_neg_scenario;
	unsigned long m_expected_hresult;

	static int s_cConstructed;
    static int s_cReleased;
};

OBJECT_ENTRY_AUTO(__uuidof(DispEvents), CDispEvents)
