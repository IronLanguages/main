// ReturnValues.h : Declaration of the CReturnValues

#pragma once
#include "resource.h"       // main symbols

#include "DlrComLibrary_i.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CReturnValues

class ATL_NO_VTABLE CReturnValues :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CReturnValues, &CLSID_ReturnValues>,
	public IDispatchImpl<IReturnValues, &IID_IReturnValues, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CReturnValues()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_RETURNVALUES)


BEGIN_COM_MAP(CReturnValues)
	COM_INTERFACE_ENTRY(IReturnValues)
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
	STDMETHOD_(void,mNoRetVal)();
	STDMETHOD_(int,mIntRetVal)();
	STDMETHOD_(int,mTwoRetVals)(int* a);
	STDMETHOD(mNullRefException)();
	STDMETHOD(mGenericCOMException)();

	static BOOL AllDestructed() { return s_cConstructed == s_cReleased; }

private:
    static int s_cConstructed;
    static int s_cReleased;

};

OBJECT_ENTRY_AUTO(__uuidof(ReturnValues), CReturnValues)
