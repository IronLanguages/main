// Properties.h : Declaration of the CProperties

#pragma once
#include "resource.h"       // main symbols

#include "DlrComLibrary_i.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif


// CProperties

class ATL_NO_VTABLE CProperties :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CProperties, &CLSID_Properties>,
	public IDispatchImpl<IProperties, &IID_IProperties, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CProperties()
	{
		m_pBstr = NULL;
        VariantInit(&m_variantVal);
		m_dispVal = NULL;
		m_dblVal = 0;
		m_propertyWithParam = 0;
		m_outParamVal = NULL;
		m_dateVal = 0;
		m_longVal = 0;
		m_twoParamsVal = 0;
		m_defaultVal = TRUE;
	}

DECLARE_REGISTRY_RESOURCEID(IDR_PROPERTIES)


BEGIN_COM_MAP(CProperties)
	COM_INTERFACE_ENTRY(IProperties)
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

private:
	BSTR m_pBstr;
	VARIANT m_variantVal;
	DATE m_dateVal;
	LONG m_longVal;
	IDispatch* m_dispVal;
	DOUBLE m_dblVal;
	DOUBLE m_propertyWithParam;
	BSTR m_outParamVal;
	DOUBLE m_twoParamsVal;
	VARIANT_BOOL m_defaultVal;
	
	static int s_cConstructed;
    static int s_cReleased;

public:

	STDMETHOD(get_pBstr)(BSTR* pVal);
	STDMETHOD(put_pBstr)(BSTR newVal);
	STDMETHOD(get_pVariant)(VARIANT* pVal);
	STDMETHOD(put_pVariant)(VARIANT newVal);
	STDMETHOD(putref_pVariant)(VARIANT* newVal);
	STDMETHOD(get_pDate)(DATE* pVal);
	STDMETHOD(put_pDate)(DATE newVal);
	STDMETHOD(get_pLong)(LONG* pVal);
	STDMETHOD(put_pLong)(LONG newVal);
	STDMETHOD(get_RefProperty)(IDispatch** pVal);
	STDMETHOD(putref_RefProperty)(IDispatch* newVal);
	STDMETHOD(get_PutAndPutRefProperty)(DOUBLE* pVal);
	STDMETHOD(put_PutAndPutRefProperty)(DOUBLE newVal);
	STDMETHOD(putref_PutAndPutRefProperty)(DOUBLE* newVal);
	STDMETHOD(get_ReadOnlyProperty)(CHAR* pVal);
	STDMETHOD(put_WriteOnlyProperty)(DATE newVal);
	STDMETHOD(get_PropertyWithParam)(DOUBLE a, DOUBLE* pVal);
	STDMETHOD(put_PropertyWithParam)(DOUBLE a, DOUBLE newVal);		
	STDMETHOD(get_PropertyWithOutParam)(BSTR* a, BSTR* pVal);
	STDMETHOD(put_PropertyWithOutParam)(BSTR* a, BSTR newVal);	
	STDMETHOD(get_PropertyWithTwoParams)(DOUBLE a, DOUBLE b, DOUBLE* pVal);
	STDMETHOD(put_PropertyWithTwoParams)(DOUBLE a, DOUBLE b, DOUBLE newVal);
	STDMETHOD(get_DefaultProperty)(SHORT a, VARIANT_BOOL* pVal);
	STDMETHOD(put_DefaultProperty)(SHORT a, VARIANT_BOOL newVal);

	static BOOL AllDestructed() { return s_cConstructed == s_cReleased; }
};

OBJECT_ENTRY_AUTO(__uuidof(Properties), CProperties)
