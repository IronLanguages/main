// HiddenMembers.h : Declaration of the CHiddenMembers

#pragma once
#include "DlrComLibrary_i.h"
#include "resource.h"       // main symbols
#include <comsvcs.h>



// CHiddenMembers

class ATL_NO_VTABLE CHiddenMembers :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CHiddenMembers, &CLSID_HiddenMembers>,
	public IDispatchImpl<IHiddenMembers, &IID_IHiddenMembers, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CHiddenMembers()
	{
		backingField=0;
	}

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_HIDDENMEMBERS)

DECLARE_NOT_AGGREGATABLE(CHiddenMembers)

BEGIN_COM_MAP(CHiddenMembers)
	COM_INTERFACE_ENTRY(IHiddenMembers)
	COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()


private:
	LONG backingField;

// IHiddenMembers
public:
	STDMETHOD(SimpleMethod)(LONG* retval);
	STDMETHOD(get_SimpleProperty)(LONG* pVal);
	STDMETHOD(put_SimpleProperty)(LONG newVal);
	STDMETHOD(HiddenMethod)(LONG* retval);
	STDMETHOD(get_HiddenProperty)(LONG* pVal);
	STDMETHOD(put_HiddenProperty)(LONG newVal);
	STDMETHOD(RestrictedMethod)(LONG* retval);
	STDMETHOD(get_RestrictedProperty)(LONG* pVal);
	STDMETHOD(put_RestrictedProperty)(LONG newVal);
};

OBJECT_ENTRY_AUTO(__uuidof(HiddenMembers), CHiddenMembers)
