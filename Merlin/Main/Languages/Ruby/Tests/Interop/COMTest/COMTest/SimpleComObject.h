// SimpleComObject.h : Declaration of the CSimpleComObject

#pragma once
#include "resource.h"       // main symbols

#include "COMTest_i.h"
#include "_ISimpleComObjectEvents_CP.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CSimpleComObject

class ATL_NO_VTABLE CSimpleComObject :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CSimpleComObject, &CLSID_SimpleComObject>,
	public IConnectionPointContainerImpl<CSimpleComObject>,
	public CProxy_ISimpleComObjectEvents<CSimpleComObject>,
	public IDispatchImpl<ISimpleComObject, &IID_ISimpleComObject, &LIBID_COMTestLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CSimpleComObject()
		: m_fField(0)
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_SIMPLECOMOBJECT)


BEGIN_COM_MAP(CSimpleComObject)
	COM_INTERFACE_ENTRY(ISimpleComObject)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(IConnectionPointContainer)
END_COM_MAP()

BEGIN_CONNECTION_POINT_MAP(CSimpleComObject)
	CONNECTION_POINT_ENTRY(__uuidof(_ISimpleComObjectEvents))
END_CONNECTION_POINT_MAP()


	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:
	STDMETHOD(get_FloatProperty)(FLOAT* pVal);
	STDMETHOD(put_FloatProperty)(FLOAT newVal);
protected:
	// used by FloatProperty
	float m_fField;
public:
	STDMETHOD(HelloWorld)(BSTR* pRet);
	STDMETHOD(GetProcessThreadID)(LONG* pdwProcessId, LONG* pdwThreadId);
};

OBJECT_ENTRY_AUTO(__uuidof(SimpleComObject), CSimpleComObject)
