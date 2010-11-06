// OptionalParams.h : Declaration of the COptionalParams

#pragma once
#include "resource.h"       // main symbols

#include "DlrComLibrary_i.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// COptionalParams

class ATL_NO_VTABLE COptionalParams :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<COptionalParams, &CLSID_OptionalParams>,
	public IDispatchImpl<IOptionalParams, &IID_IOptionalParams, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	COptionalParams()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_OPTIONALPARAMS)


BEGIN_COM_MAP(COptionalParams)
	COM_INTERFACE_ENTRY(IOptionalParams)
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

	STDMETHOD(mSingleOptionalParam)(VARIANT a);
	STDMETHOD(mOneOptionalParam)(VARIANT a, VARIANT b);
	STDMETHOD(mTwoOptionalParams)(VARIANT a, VARIANT b, VARIANT c);
	STDMETHOD(mOptionalParamWithDefaultValue)(VARIANT a, VARIANT b, VARIANT* c);
	STDMETHOD(mOptionalOutParam)(VARIANT a, VARIANT* b);
	STDMETHOD(mOptionalStringParam)(BSTR a, BSTR *b);
	STDMETHOD(mOptionalIntParam)(int a, int *b);
	static BOOL AllDestructed() { return s_cConstructed == s_cReleased; }

private:
    static int s_cConstructed;
    static int s_cReleased;
};

OBJECT_ENTRY_AUTO(__uuidof(OptionalParams), COptionalParams)
