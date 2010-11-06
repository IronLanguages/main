// HiddenMembers.cpp : Implementation of CHiddenMembers

#include "stdafx.h"
#include "HiddenMembers.h"


// CHiddenMembers


STDMETHODIMP CHiddenMembers::SimpleMethod(LONG* retval)
{
	*retval = 42;
	return S_OK;
}

STDMETHODIMP CHiddenMembers::get_SimpleProperty(LONG* pVal)
{
	*pVal = backingField;
	return S_OK;
}

STDMETHODIMP CHiddenMembers::put_SimpleProperty(LONG newVal)
{
	backingField = newVal;
	return S_OK;
}

STDMETHODIMP CHiddenMembers::HiddenMethod(LONG* retval)
{
	*retval = 43;
	return S_OK;
}

STDMETHODIMP CHiddenMembers::get_HiddenProperty(LONG* pVal)
{
	*pVal = backingField;
	return S_OK;
}

STDMETHODIMP CHiddenMembers::put_HiddenProperty(LONG newVal)
{
	backingField = newVal;
	return S_OK;
}

STDMETHODIMP CHiddenMembers::RestrictedMethod(LONG* retval)
{
	*retval = 44;
	return S_OK;
}

STDMETHODIMP CHiddenMembers::get_RestrictedProperty(LONG* pVal)
{
	*pVal = backingField;
	return S_OK;
}

STDMETHODIMP CHiddenMembers::put_RestrictedProperty(LONG newVal)
{
	backingField = newVal;
	return S_OK;
}
