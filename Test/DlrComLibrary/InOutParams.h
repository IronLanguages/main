// InOutParams.h : Declaration of the CInOutParams

#pragma once
#include "resource.h"       // main symbols

#include "DlrComLibrary_i.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CInOutParams

class ATL_NO_VTABLE CInOutParams :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CInOutParams, &CLSID_InOutParams>,
	public IDispatchImpl<IInOutParams, &IID_IInOutParams, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CInOutParams()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_INOUTPARAMS)


BEGIN_COM_MAP(CInOutParams)
	COM_INTERFACE_ENTRY(IInOutParams)
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

	STDMETHOD(mBstr)(BSTR *a);
	STDMETHOD(mByte)(BYTE *a);
	STDMETHOD(mDouble)(DOUBLE *a);
	STDMETHOD(mTwoInOutParams)(DOUBLE* a, DOUBLE* b);
	STDMETHOD(mInAndInOutParams)(CY a, CY* b);
	STDMETHOD(mOutAndInOutParams)(DATE* a, DATE* b);    
	STDMETHOD(mIDispatch)(IDispatch** a);
	STDMETHOD(mSingleRefParam)(DOUBLE* a);
	STDMETHOD(mTwoRefParams)(BSTR* a, IDispatch** b);
	STDMETHOD(mBOOL)(BOOL a, BOOL* b);
	STDMETHOD(mCHAR)(CHAR a, CHAR* b);
	STDMETHOD(mUCHAR)(UCHAR a, UCHAR* b);
	STDMETHOD(msmall)(small a, small* b);
	STDMETHOD(mFLOAT)(FLOAT a, FLOAT* b);
	STDMETHOD(mSHORT)(SHORT a, SHORT* b);
	STDMETHOD(mINT16)(INT16 a, INT16* b);
	STDMETHOD(mINT64)(INT64 a, INT64* b);
	STDMETHOD(mUSHORT)(USHORT a, USHORT* b);
	STDMETHOD(mint)(int a, int* b);
	STDMETHOD(m__int32)(__int32 a, __int32* b);
	STDMETHOD(mUINT)(UINT a, UINT* b);
	STDMETHOD(mLONG)(LONG a, LONG* b);
	STDMETHOD(mULONG)(ULONG a, ULONG* b);
	STDMETHOD(mUINT64)(UINT64 a, UINT64* b);
	STDMETHOD(mHRESULT)(HRESULT a, HRESULT* b);
	STDMETHOD(mINT8)(INT8 a, INT8* b);
	STDMETHOD(mLPSTR)(LPSTR a, LPSTR* b);
	STDMETHOD(mLPWSTR)(LPWSTR a, LPWSTR* b);
	STDMETHOD(mPCHAR)(CHAR* a, CHAR** b);
	STDMETHOD(mPwchar_t)(wchar_t* a, wchar_t** b);
	STDMETHOD(mwchar_t)(wchar_t a, wchar_t* b);
	STDMETHOD(mDATE)(DATE a, DATE* b);
	STDMETHOD(mGUID)(GUID a, GUID* b);
	STDMETHOD(mDECIMAL)(DECIMAL a, DECIMAL* b);
	STDMETHOD(mCURRENCY)(CURRENCY a, CURRENCY* b);
	STDMETHOD(mPIUnknown)(IUnknown* a, IUnknown** b);
	STDMETHOD(mPIDispatch)(IDispatch* a, IDispatch** b);
	STDMETHOD(mPITypeInfo)(ITypeInfo* a, ITypeInfo** b);
	STDMETHOD(mDOUBLE1)(DOUBLE a, DOUBLE* b);
	STDMETHOD(mVARIANT_BOOL)(VARIANT_BOOL a, VARIANT_BOOL* b);
	STDMETHOD(mBstr1)(BSTR a, BSTR* b);
	STDMETHOD(mVariant)(VARIANT a, VARIANT* b);
	STDMETHOD(mVT_BOOL_CHAR)(VARIANT a, VARIANT* b);
	STDMETHOD(mVT_INT_BSTR)(VARIANT a, VARIANT* b);
	STDMETHOD(mVT_BSTR_LONG)(VARIANT a, VARIANT* b);

	static BOOL AllDestructed() { return s_cConstructed == s_cReleased; }

private:
    static int s_cConstructed;
    static int s_cReleased;

	
};

OBJECT_ENTRY_AUTO(__uuidof(InOutParams), CInOutParams)
