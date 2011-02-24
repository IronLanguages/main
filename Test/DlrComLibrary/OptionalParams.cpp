// OptionalParams.cpp : Implementation of COptionalParams

#include "stdafx.h"
#include "OptionalParams.h"

int COptionalParams::s_cConstructed;
int COptionalParams::s_cReleased;

// COptionalParams


STDMETHODIMP COptionalParams::mSingleOptionalParam(/* Optional */ VARIANT a)
{
	return S_OK;
}

STDMETHODIMP COptionalParams::mOneOptionalParam(VARIANT a, /* Optional */ VARIANT b)
{
	return S_OK;
}

STDMETHODIMP COptionalParams::mTwoOptionalParams(VARIANT a, /* Optional */ VARIANT b, /* Optional */ VARIANT c)
{
	return S_OK;
}

STDMETHODIMP COptionalParams::mOptionalParamWithDefaultValue(VARIANT a, /* Optional */ VARIANT b, /* Optional */ VARIANT* c)
{
	return VariantCopy(c, &b);
}

STDMETHODIMP COptionalParams::mOptionalOutParam(VARIANT a, /* Optional */ VARIANT* b)
{
    return VariantCopy(b, &a);
}

STDMETHODIMP COptionalParams::mOptionalStringParam(BSTR a, BSTR *b)
{
	*b = SysAllocString(a);
	return S_OK;
}

STDMETHODIMP COptionalParams::mOptionalIntParam(int a, int *b)
{
	*b = a;
	return S_OK;
}