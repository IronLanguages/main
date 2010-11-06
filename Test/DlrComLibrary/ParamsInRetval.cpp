// ParamsIn.cpp : Implementation of CParamsIn

#include "stdafx.h"
#include "ParamsInRetval.h"

int CParamsInRetval::s_cConstructed;
int CParamsInRetval::s_cReleased;
// CParamsIn


STDMETHODIMP CParamsInRetval::mBstr(BSTR a, BSTR* b)
{
	if(a==NULL) {
		*b = a;
	}
	else {
		*b = SysAllocString(a);
	}

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mByte(BYTE a, BYTE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mChar(CHAR a, CHAR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mCy(CY a, CY* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mDate(DATE a, DATE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mDouble(DOUBLE a, DOUBLE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mFloat(FLOAT a, FLOAT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mIDispatch(IDispatch* a, IDispatch** b)
{
	*b = *&a;
	if (a!=NULL) {
		a->AddRef();
	}
	return S_OK;
}

STDMETHODIMP CParamsInRetval::mIFontDisp(IFontDisp* a, IDispatch** b)
{
	*b = *&a;
	if (a!=NULL) {
		a->AddRef();
	}
	return S_OK;
}

STDMETHODIMP CParamsInRetval::mIPictureDisp(IPictureDisp* a, IDispatch** b)
{
	*b = *&a;
	if (a!=NULL) {
		a->AddRef();
	}
	return S_OK;
}

STDMETHODIMP CParamsInRetval::mIUnknown(IUnknown* a, IUnknown** b)
{
	*b = *&a;
	if (a!=NULL) {
		a->AddRef();
	}
	return S_OK;
}

STDMETHODIMP CParamsInRetval::mLong(LONG a, LONG* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mLongLong(LONGLONG a, LONGLONG* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleColor(OLE_COLOR a, OLE_COLOR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleXposHimetric(OLE_XPOS_HIMETRIC a, OLE_XPOS_HIMETRIC* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleYposHimetric(OLE_YPOS_HIMETRIC a, OLE_YPOS_HIMETRIC* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleXsizeHimetric(OLE_XSIZE_HIMETRIC a, OLE_XSIZE_HIMETRIC* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleYsizeHimetric(OLE_YSIZE_HIMETRIC a, OLE_YSIZE_HIMETRIC* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleXposPixels(OLE_XPOS_PIXELS a, OLE_XPOS_PIXELS* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleYposPixels(OLE_YPOS_PIXELS a, OLE_YPOS_PIXELS* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleXsizePixels(OLE_XSIZE_PIXELS a, OLE_XSIZE_PIXELS* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleYsizePixels(OLE_YSIZE_PIXELS a, OLE_YSIZE_PIXELS* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleHandle(OLE_HANDLE a, OLE_HANDLE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleOptExclusive(OLE_OPTEXCLUSIVE a, OLE_OPTEXCLUSIVE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleTristate(enum OLE_TRISTATE a, enum OLE_TRISTATE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mScode(SCODE a, SCODE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mShort(SHORT a, SHORT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mUlong(ULONG a, ULONG* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mULongLong(ULONGLONG a, ULONGLONG* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mUShort(USHORT a, USHORT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mVariant(VARIANT a, VARIANT* b)
{
	return VariantCopy(b, &a);
}

STDMETHODIMP CParamsInRetval::mVariantBool(VARIANT_BOOL a, VARIANT_BOOL* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mBOOL(BOOL a, BOOL* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mUCHAR(UCHAR a, UCHAR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::msmall(small a, small* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mINT16(INT16 a, INT16* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mINT64(INT64 a, INT64* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mint(int a, int* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::m__int32(__int32 a, __int32* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mUINT(UINT a, UINT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mUINT64(UINT64 a, UINT64* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mHRESULT(HRESULT a, HRESULT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mINT8(INT8 a, INT8* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mLPSTR(LPSTR a, LPSTR* b)
{
	*b = new char[ strlen(a)+1];
	
	strcpy( *b, a);
	return S_OK;
}

STDMETHODIMP CParamsInRetval::mLPWSTR(LPWSTR a, LPWSTR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mPCHAR(CHAR* a, CHAR** b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mPwchar_t(wchar_t* a, wchar_t** b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mwchar_t(wchar_t a, wchar_t* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mGUID(GUID a, GUID* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mDECIMAL(DECIMAL a, DECIMAL* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mCURRENCY(CURRENCY a, CURRENCY* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mPIUnknown(IUnknown* a, IUnknown** b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mPITypeInfo(ITypeInfo* a, ITypeInfo** b)
{
	*b = a;

	return S_OK;
}


STDMETHODIMP CParamsInRetval::mVT_BOOL_CHAR(VARIANT a, VARIANT* b)
{
	VariantInit(b);
	b->vt = VT_I1;
	b->cVal = 't';

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mVT_INT_BSTR(VARIANT a, VARIANT* b)
{
	VariantInit(b);
	b->vt = VT_BSTR;

	if (a.intVal == 1)
		b->bstrVal = ::SysAllocString(L"hello");
	else if (a.intVal == 2)
		b->bstrVal = ::SysAllocString(L"world");

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mVT_BSTR_LONG(VARIANT a, VARIANT* b)
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

STDMETHODIMP CParamsInRetval::mVARIANT_BOOL(VARIANT_BOOL a, VARIANT_BOOL* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mBstr1(BSTR a, BSTR* b)
{
	if(a!=NULL) 
	{
		*b = SysAllocString(a);
	}

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mDouble1(DOUBLE a, DOUBLE* b)
{
	*b = a;

	return S_OK;
}
