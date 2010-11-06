// ReturnValues.cpp : Implementation of CReturnValues

#include "stdafx.h"
#include "ReturnValues.h"


// CReturnValues
int CReturnValues::s_cConstructed;
int CReturnValues::s_cReleased;

STDMETHODIMP_(void) CReturnValues::mNoRetVal()
{
}

STDMETHODIMP_(int) CReturnValues::mIntRetVal()
{
	return 42;
}

STDMETHODIMP_(int) CReturnValues::mTwoRetVals(int * a)
{
	*a = 3;
	return 42;
}

STDMETHODIMP CReturnValues::mNullRefException()
{
	AtlSetErrorInfo(CLSID_ReturnValues, OLESTR("Custom error message for E_POINTER"), 0, NULL, IID_IReturnValues, E_POINTER, NULL);
	return E_POINTER;
}

STDMETHODIMP CReturnValues::mGenericCOMException()
{
	return TPM_E_MA_SOURCE; //some random HRESULT that is not mapped by .NET
}