// ProvidesClassInfo.h : Declaration of the CProvidesClassInfo

#pragma once
#include "DlrComLibrary_i.h"
#include "resource.h"       // main symbols
#include <comsvcs.h>
#include "_IDispEventsEvents_CP.H"



// CProvidesClassInfo

class ATL_NO_VTABLE CProvidesClassInfo :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CProvidesClassInfo, &CLSID_ProvidesClassInfo>,
	public IConnectionPointContainerImpl<CProvidesClassInfo>,
	public CProxy_IDispEventsEvents<CProvidesClassInfo>,
	public IDispatchImpl<IProvidesClassInfo, &IID_IProvidesClassInfo, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IProvideClassInfo2Impl<&CLSID_ProvidesClassInfo, &IID_IProvidesClassInfo, &LIBID_DlrComLibraryLib>
{
public:
	CProvidesClassInfo()
		: m_neg_scenario(false)
		, m_expected_hresult(0)
	{
	}

	DECLARE_REGISTRY_RESOURCEID(IDR_PROVIDESCLASSINFO)

	DECLARE_NOT_AGGREGATABLE(CProvidesClassInfo)

	BEGIN_COM_MAP(CProvidesClassInfo)
		COM_INTERFACE_ENTRY(IProvidesClassInfo)
		COM_INTERFACE_ENTRY(IDispatch)
		COM_INTERFACE_ENTRY(IProvideClassInfo)
		COM_INTERFACE_ENTRY(IProvideClassInfo2)
		COM_INTERFACE_ENTRY(IConnectionPointContainer)
	END_COM_MAP()

	BEGIN_CONNECTION_POINT_MAP(CProvidesClassInfo)
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

OBJECT_ENTRY_AUTO(__uuidof(ProvidesClassInfo), CProvidesClassInfo)
