// NoTypeInfo.h : Declaration of the CNoTypeInfo

#pragma once
#include "DlrComLibrary_i.h"
#include "resource.h"       // main symbols
#include <comsvcs.h>

// CNoTypeInfo

class ATL_NO_VTABLE CNoTypeInfo :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CNoTypeInfo, &CLSID_NoTypeInfo>,
	public IDispatchImpl<INoTypeInfo, &IID_INoTypeInfo, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CNoTypeInfo()
		: propertyValue(0)
	{
	}

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_NOTYPEINFO)

DECLARE_NOT_AGGREGATABLE(CNoTypeInfo)

BEGIN_COM_MAP(CNoTypeInfo)
	COM_INTERFACE_ENTRY(INoTypeInfo)
	COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()

// INoTypeInfo
public:

	STDMETHOD(GetTypeInfoCount)(UINT* pctinfo);

	STDMETHOD(get_SimpleProperty)(LONG* pVal);
	STDMETHOD(put_SimpleProperty)(LONG newVal);
	STDMETHOD(get_DefaultProperty)(LONG* pVal);
	STDMETHOD(put_DefaultProperty)(LONG newVal);

private:
	LONG propertyValue;
public:
	STDMETHOD(SimpleMethod)(LONG* retval);
};

OBJECT_ENTRY_AUTO(__uuidof(NoTypeInfo), CNoTypeInfo)
