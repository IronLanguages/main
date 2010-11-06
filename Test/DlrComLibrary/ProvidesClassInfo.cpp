// ProvidesClassInfo.cpp : Implementation of CProvidesClassInfo

#include "stdafx.h"
#include "ProvidesClassInfo.h"


// CProvidesClassInfo
int CProvidesClassInfo::s_cConstructed;
int CProvidesClassInfo::s_cReleased;


STDMETHODIMP CProvidesClassInfo::get_neg_scenario(VARIANT_BOOL* pVal)
{
	*pVal = this->m_neg_scenario;
	return S_OK;
}

STDMETHODIMP CProvidesClassInfo::put_neg_scenario(VARIANT_BOOL newVal)
{
	this->m_neg_scenario = newVal;
	return S_OK;
}

STDMETHODIMP CProvidesClassInfo::get_expected_hresult(ULONG* pVal)
{
	*pVal = this->m_expected_hresult;
	return S_OK;
}

STDMETHODIMP CProvidesClassInfo::put_expected_hresult(ULONG newVal)
{
	this->m_expected_hresult = newVal;
	return S_OK;
}

STDMETHODIMP CProvidesClassInfo::triggerNull(void)
{
	HRESULT actHresult = this->Fire_eNull();

	if ((HRESULT)this->m_expected_hresult == actHresult)
	{
		return S_OK;
	}
	else
	{
		return actHresult;
	}
}

STDMETHODIMP CProvidesClassInfo::triggerInOutretBool(VARIANT_BOOL inval, VARIANT_BOOL* ret)
{
	HRESULT actHresult = this->Fire_eInOutretBool(inval, ret);
	if ((HRESULT)this->m_expected_hresult == actHresult)
	{
		return S_OK;
	}
	else
	{
		return actHresult;
	}
}

STDMETHODIMP CProvidesClassInfo::triggerInOutBstr(BSTR inval, BSTR* outval)
{
	HRESULT actHresult = this->Fire_eInOutBstr(inval, outval);
	if ((HRESULT)this->m_expected_hresult == actHresult)
	{
		return S_OK;
	}
	else
	{
		return actHresult;
	}
}

STDMETHODIMP CProvidesClassInfo::triggerUShort(USHORT inval)
{
	HRESULT actHresult = this->Fire_eInUshort(inval);
	if ((HRESULT)this->m_expected_hresult == actHresult)
	{
		return S_OK;
	}
	else
	{
		return actHresult;
	}
}

STDMETHODIMP CProvidesClassInfo::triggerNullShort(void)
{
	//TODO: the short return value is lost as Fire_eNullShort
	//      does not give us access to it
	HRESULT actHresult = this->Fire_eNullShort();
	if ((HRESULT)this->m_expected_hresult == actHresult)
	{
		return S_OK;
	}
	else
	{
		return actHresult;
	}
}

