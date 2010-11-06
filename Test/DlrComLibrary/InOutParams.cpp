// InOutParams.cpp : Implementation of CInOutParams

#include "stdafx.h"
#include "InOutParams.h"
#include <comutil.h>
#pragma comment(lib, "comsupp.lib")

// CInOutParams
int CInOutParams::s_cConstructed;
int CInOutParams::s_cReleased;

STDMETHODIMP CInOutParams::mBstr(BSTR* a)
{
	if(a!=NULL) 
	{
		_bstr_t temp = *a;
		_bstr_t newVal = L"a";
		SysFreeString(*a);
		*a = SysAllocString(temp + newVal);
	}

	return S_OK;
}

STDMETHODIMP CInOutParams::mByte(BYTE* a)
{
	*a += 2;
	return S_OK;
}

STDMETHODIMP CInOutParams::mDouble(DOUBLE *a)
{
	*a += 2;
	return S_OK;
}

STDMETHODIMP CInOutParams::mTwoInOutParams(DOUBLE* a, DOUBLE* b)
{	
	*b += *a + 2;
	*a += 2;
	return S_OK;
}

STDMETHODIMP CInOutParams::mInAndInOutParams(CY a, CY* b)
{
	*b = a;
	return S_OK;
}

STDMETHODIMP CInOutParams::mOutAndInOutParams(DATE* a, DATE* b)
{
	*b = *a;
	return S_OK;
}

STDMETHODIMP CInOutParams::mIDispatch(IDispatch** a)
{
	return S_OK;
}
STDMETHODIMP CInOutParams::mSingleRefParam(DOUBLE* a)
{
	*a += 2;
	return S_OK;
}

STDMETHODIMP CInOutParams::mTwoRefParams(BSTR* a, IDispatch** b)
{
	return S_OK;
}

STDMETHODIMP CInOutParams::mBOOL(BOOL a, BOOL* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mCHAR(CHAR a, CHAR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mUCHAR(UCHAR a, UCHAR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::msmall(small a, small* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mFLOAT(FLOAT a, FLOAT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mSHORT(SHORT a, SHORT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mINT16(INT16 a, INT16* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mINT64(INT64 a, INT64* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mUSHORT(USHORT a, USHORT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mint(int a, int* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::m__int32(__int32 a, __int32* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mUINT(UINT a, UINT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mLONG(LONG a, LONG* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mULONG(ULONG a, ULONG* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mUINT64(UINT64 a, UINT64* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mHRESULT(HRESULT a, HRESULT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mINT8(INT8 a, INT8* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mLPSTR(LPSTR a, LPSTR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mLPWSTR(LPWSTR a, LPWSTR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mPCHAR(CHAR* a, CHAR** b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mPwchar_t(wchar_t* a, wchar_t** b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mwchar_t(wchar_t a, wchar_t* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mDATE(DATE a, DATE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mGUID(GUID a, GUID* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mDECIMAL(DECIMAL a, DECIMAL* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mCURRENCY(CURRENCY a, CURRENCY* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mPIUnknown(IUnknown* a, IUnknown** b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mPIDispatch(IDispatch* a, IDispatch** b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mPITypeInfo(ITypeInfo* a, ITypeInfo** b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mDOUBLE1(DOUBLE a, DOUBLE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mVARIANT_BOOL(VARIANT_BOOL a, VARIANT_BOOL* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CInOutParams::mBstr1(BSTR a, BSTR* b)
{
	if(a!=NULL) 
	{
		*b = SysAllocString(a);
	}

	return S_OK;
}

STDMETHODIMP CInOutParams::mVariant(VARIANT a, VARIANT* b)
{
	VariantClear( b);
	return VariantCopy ( b, &a);
}

STDMETHODIMP CInOutParams::mVT_BOOL_CHAR(VARIANT a, VARIANT* b)
{
	VariantInit(b);
	b->vt = VT_I1;
	b->cVal = 't';

	return S_OK;
}

STDMETHODIMP CInOutParams::mVT_INT_BSTR(VARIANT a, VARIANT* b)
{
	VariantInit(b);
	b->vt = VT_BSTR;

	if (a.intVal == 1)
		b->bstrVal = ::SysAllocString(L"hello");
	else if (a.intVal == 2)
		b->bstrVal = ::SysAllocString(L"world");

	return S_OK;
}

STDMETHODIMP CInOutParams::mVT_BSTR_LONG(VARIANT a, VARIANT* b)
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
