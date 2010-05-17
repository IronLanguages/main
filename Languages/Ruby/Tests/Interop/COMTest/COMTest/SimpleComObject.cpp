// SimpleComObject.cpp : Implementation of CSimpleComObject

#include "stdafx.h"
#include "SimpleComObject.h"


// CSimpleComObject


STDMETHODIMP CSimpleComObject::get_FloatProperty(FLOAT* pVal)
{
    *pVal = m_fField;
	return S_OK;
}

STDMETHODIMP CSimpleComObject::put_FloatProperty(FLOAT newVal)
{
	//Fire the event, FloatPropertyChanging
	VARIANT_BOOL cancel = VARIANT_FALSE;
	Fire_FloatPropertyChanging(newVal, &cancel);
	
	if(cancel == VARIANT_FALSE) {
		m_fField = newVal;
	}
	return S_OK;
}

STDMETHODIMP CSimpleComObject::HelloWorld(BSTR* pRet)
{
    //Allocate memory for the string
	*pRet = ::SysAllocString(L"HelloWorld");
	if (pRet == NULL)
		return E_OUTOFMEMORY;
	return S_OK;
}

STDMETHODIMP CSimpleComObject::GetProcessThreadID(LONG* pdwProcessId, LONG* pdwThreadId)
{
	// TODO: Add your implementation code here
	*pdwProcessId = GetCurrentProcessId();
	*pdwThreadId = GetCurrentThreadId();
	return S_OK;
}
