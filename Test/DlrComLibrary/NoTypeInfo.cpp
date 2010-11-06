// NoTypeInfo.cpp : Implementation of CNoTypeInfo

#include "stdafx.h"
#include "NoTypeInfo.h"


// CNoTypeInfo

STDMETHODIMP CNoTypeInfo::GetTypeInfoCount(UINT* pctinfo)
{
	if (pctinfo == NULL) 
		return E_POINTER; 
	*pctinfo = 0;
	return S_OK;
}

STDMETHODIMP CNoTypeInfo::get_SimpleProperty(LONG* pVal)
{
	*pVal = propertyValue;
	return S_OK;
}

STDMETHODIMP CNoTypeInfo::put_SimpleProperty(LONG newVal)
{
	propertyValue = newVal;
	return S_OK;
}

STDMETHODIMP CNoTypeInfo::SimpleMethod(LONG* retval)
{
	*retval = 42;
	return S_OK;
}

STDMETHODIMP CNoTypeInfo::get_DefaultProperty(LONG* pVal)
{
	*pVal = propertyValue;
	return S_OK;
}

STDMETHODIMP CNoTypeInfo::put_DefaultProperty(LONG newVal)
{
	propertyValue = newVal;
	return S_OK;
}
