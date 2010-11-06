// NonDispatch.h : Declaration of the CNonDispatch

#pragma once
#include "DlrComLibrary_i.h"
#include "resource.h"       // main symbols
#include <comsvcs.h>



// CNonDispatch

class ATL_NO_VTABLE CNonDispatch :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CNonDispatch, &CLSID_NonDispatch>,
	public INonDispatch
{
public:
	CNonDispatch()
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

DECLARE_REGISTRY_RESOURCEID(IDR_NONDISPATCH)

DECLARE_NOT_AGGREGATABLE(CNonDispatch)

BEGIN_COM_MAP(CNonDispatch)
	COM_INTERFACE_ENTRY(INonDispatch)
END_COM_MAP()




// INonDispatch
public:
};

OBJECT_ENTRY_AUTO(__uuidof(NonDispatch), CNonDispatch)
