// MultipleParams.h : Declaration of the CMultipleParams

#pragma once
#include "resource.h"       // main symbols

#include "DlrComLibrary_i.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CMultipleParams

class ATL_NO_VTABLE CMultipleParams :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CMultipleParams, &CLSID_MultipleParams>,
	public IDispatchImpl<IMultipleParams, &IID_IMultipleParams, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CMultipleParams()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_MULTIPLEPARAMS)


BEGIN_COM_MAP(CMultipleParams)
	COM_INTERFACE_ENTRY(IMultipleParams)
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

	STDMETHOD(mZeroParams)(void);
	STDMETHOD(mOneParamNoRetval)(BSTR a);
	STDMETHOD(mOneParam)(BSTR a, BSTR* b);
	STDMETHOD(mTwoParams)(BYTE a, BYTE b, BYTE* c);
	STDMETHOD(mThreeParams)(DOUBLE a, DOUBLE b, DOUBLE c, DOUBLE* d);
	STDMETHOD(mFourParams)(VARIANT_BOOL a, ULONG b, BSTR c, ULONG d, ULONG* e);
	STDMETHOD(mFiveParams)(BSTR a, FLOAT b, BSTR c, FLOAT d, FLOAT e, FLOAT* f);

	static BOOL AllDestructed() { return s_cConstructed == s_cReleased; }

private:
    static int s_cConstructed;
    static int s_cReleased;
};

OBJECT_ENTRY_AUTO(__uuidof(MultipleParams), CMultipleParams)
