// MultipleParams.cpp : Implementation of CMultipleParams

#include "stdafx.h"
#include "MultipleParams.h"

// CMultipleParams

int CMultipleParams::s_cConstructed;
int CMultipleParams::s_cReleased;

STDMETHODIMP CMultipleParams::mZeroParams(void)
{	
	return S_OK;
}

STDMETHODIMP CMultipleParams::mOneParamNoRetval(BSTR a)
{	
	return S_OK;
}

STDMETHODIMP CMultipleParams::mOneParam(BSTR a, BSTR* b)
{
	if(a==NULL) {
		*b = a;
	}
	else {
		*b = SysAllocString(a);
	}
	return S_OK;
}

STDMETHODIMP CMultipleParams::mTwoParams(BYTE a, BYTE b, BYTE* c)
{
	*c = a + b;
	return S_OK;
}

STDMETHODIMP CMultipleParams::mThreeParams(DOUBLE a, DOUBLE b, DOUBLE c, DOUBLE* d)
{
    *d = a + b + c;
	return S_OK;
}

STDMETHODIMP CMultipleParams::mFourParams(VARIANT_BOOL a, ULONG b, BSTR c, ULONG d, ULONG* e)
{
	*e = b + d;
	return S_OK;
}

STDMETHODIMP CMultipleParams::mFiveParams(BSTR a, FLOAT b, BSTR c, FLOAT d, FLOAT e, FLOAT* f)
{
	*f = b + d + e;
	return S_OK;
}
