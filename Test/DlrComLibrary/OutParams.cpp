// OutParams.cpp : Implementation of COutParams

#include "stdafx.h"
#include "OutParams.h"


int COutParams::s_cConstructed;
int COutParams::s_cReleased;
// COutParams

STDMETHODIMP COutParams::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IOutParams
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

STDMETHODIMP COutParams::mBstr(BSTR a, BSTR* b)
{
    if ((*b) != NULL) {
        SysFreeString(*b);
    }

	if(a==NULL) {
		*b = a;
	}
	else {
		*b = SysAllocString(a);
	}

	return S_OK;
}

STDMETHODIMP COutParams::mByte(BYTE a, BYTE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mChar(CHAR a, CHAR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mCy(CY a, CY* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mDate(DATE a, DATE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mDouble(DOUBLE a, DOUBLE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mFloat(FLOAT a, FLOAT* b)
{
	*b = a;

	return S_OK;
}

#define COPY_UNKNOWN(src, pDest)    \
	if ((*pDest) != NULL) {         \
		(*pDest)->Release();        \
	}                               \
                                    \
	*pDest = *&src;                 \
                                    \
	if ((*pDest) != NULL) {         \
		(*pDest)->AddRef();         \
	}

STDMETHODIMP COutParams::mIDispatch(IDispatch* a, IDispatch** b)
{
    COPY_UNKNOWN(a, b);
	return S_OK;
}

STDMETHODIMP COutParams::mIFontDisp(IFontDisp* a, IDispatch** b)
{
    COPY_UNKNOWN(a, b);
	return S_OK;
}

STDMETHODIMP COutParams::mIPictureDisp(IPictureDisp* a, IDispatch** b)
{
    COPY_UNKNOWN(a, b);
	return S_OK;
}

STDMETHODIMP COutParams::mIUnknown(IUnknown* a, IUnknown** b)
{
    COPY_UNKNOWN(a, b);
	return S_OK;
}

STDMETHODIMP COutParams::mLong(LONG a, LONG* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mLongLong(LONGLONG a, LONGLONG* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mOleColor(OLE_COLOR a, OLE_COLOR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mOleXposHimetric(OLE_XPOS_HIMETRIC a, OLE_XPOS_HIMETRIC* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mOleYposHimetric(OLE_YPOS_HIMETRIC a, OLE_YPOS_HIMETRIC* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mOleXsizeHimetric(OLE_XSIZE_HIMETRIC a, OLE_XSIZE_HIMETRIC* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mOleYsizeHimetric(OLE_YSIZE_HIMETRIC a, OLE_YSIZE_HIMETRIC* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mOleXposPixels(OLE_XPOS_PIXELS a, OLE_XPOS_PIXELS* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mOleYposPixels(OLE_YPOS_PIXELS a, OLE_YPOS_PIXELS* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mOleXsizePixels(OLE_XSIZE_PIXELS a, OLE_XSIZE_PIXELS* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mOleYsizePixels(OLE_YSIZE_PIXELS a, OLE_YSIZE_PIXELS* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mOleHandle(OLE_HANDLE a, OLE_HANDLE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mOleOptExclusive(OLE_OPTEXCLUSIVE a, OLE_OPTEXCLUSIVE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mOleTristate(enum OLE_TRISTATE a, enum OLE_TRISTATE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mScode(SCODE a, SCODE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mShort(SHORT a, SHORT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mUlong(ULONG a, ULONG* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mULongLong(ULONGLONG a, ULONGLONG* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mUShort(USHORT a, USHORT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mVariant(VARIANT a, VARIANT* b)
{
	VariantClear(b);
	return VariantCopy(b, &a);
}

STDMETHODIMP COutParams::mVariantBool(VARIANT_BOOL a, VARIANT_BOOL* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mBOOL(BOOL a, BOOL* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mUCHAR(UCHAR a, UCHAR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::msmall(small a, small* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mINT16(INT16 a, INT16* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mINT64(INT64 a, INT64* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mint(int a, int* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::m__int32(__int32 a, __int32* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mUINT(UINT a, UINT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mUINT64(UINT64 a, UINT64* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mHRESULT(HRESULT a, HRESULT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mINT8(INT8 a, INT8* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mLPSTR(LPSTR a, LPSTR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mLPWSTR(LPWSTR a, LPWSTR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mPCHAR(CHAR* a, CHAR** b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mPwchar_t(wchar_t* a, wchar_t** b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mwchar_t(wchar_t a, wchar_t* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mGUID(GUID a, GUID* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mDECIMAL(DECIMAL a, DECIMAL* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mCURRENCY(CURRENCY a, CURRENCY* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mPIUnknown(IUnknown* a, IUnknown** b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mPITypeInfo(ITypeInfo* a, ITypeInfo** b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mVARIANT_BOOL(VARIANT_BOOL a, VARIANT_BOOL* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP COutParams::mBstr1(BSTR a, BSTR* b)
{
	if(a!=NULL) 
	{
		*b = SysAllocString(a);
	}

	return S_OK;
}

STDMETHODIMP COutParams::mVT_BOOL_CHAR(VARIANT a, VARIANT* b)
{
	VariantInit(b);
	b->vt = VT_I1;
	b->cVal = 't';

	return S_OK;
}

STDMETHODIMP COutParams::mVT_INT_BSTR(VARIANT a, VARIANT* b)
{
	VariantInit(b);
	b->vt = VT_BSTR;

	if (a.intVal == 1)
		b->bstrVal = ::SysAllocString(L"hello");
	else if (a.intVal == 2)
		b->bstrVal = ::SysAllocString(L"world");

	return S_OK;
}

STDMETHODIMP COutParams::mVT_BSTR_LONG(VARIANT a, VARIANT* b)
{
	USES_CONVERSION;

	VariantInit(b);
	b->vt = VT_I4;
	
	if  (0==strcmp( OLE2A( a.bstrVal), "hello"))
		b->lVal = 100;
	else if (0==strcmp( OLE2A( a.bstrVal), "world"))
		b->lVal = 200;

	return S_OK;
}

STDMETHODIMP COutParams::mDouble1(DOUBLE a, DOUBLE* b)
{
	*b = a;

	return S_OK;
}