// Properties.cpp : Implementation of CProperties

#include "stdafx.h"
#include "Properties.h"

int CProperties::s_cConstructed;
int CProperties::s_cReleased;

// CProperties


STDMETHODIMP CProperties::get_pBstr(BSTR* pVal)
{
	*pVal = m_pBstr;
	return S_OK;
}

STDMETHODIMP CProperties::put_pBstr(BSTR newVal)
{
	if(m_pBstr != NULL)
	{
		SysFreeString(m_pBstr);
	}

	if(newVal == NULL)
	{
		m_pBstr = newVal;
	}
	else
	{
		m_pBstr = SysAllocString(newVal);
	}
	return S_OK;
}

STDMETHODIMP CProperties::get_pVariant(VARIANT* pVal)
{
	return VariantCopy(pVal, &m_variantVal);
}

STDMETHODIMP CProperties::put_pVariant(VARIANT newVal)
{
    if (newVal.vt != VARENUM::VT_DISPATCH){
        return VariantCopy(&m_variantVal, &newVal);
    }
}

STDMETHODIMP CProperties::putref_pVariant(VARIANT* newVal)
{
    return VariantCopy(&m_variantVal, newVal);
}


STDMETHODIMP CProperties::get_pDate(DATE* pVal)
{
	*pVal = m_dateVal;
	return S_OK;
}

STDMETHODIMP CProperties::put_pDate(DATE newVal)
{
	m_dateVal = newVal;
	return S_OK;
}

STDMETHODIMP CProperties::get_pLong(LONG* pVal)
{	
	*pVal = m_longVal;
	return S_OK;
}

STDMETHODIMP CProperties::put_pLong(LONG newVal)
{
	m_longVal = newVal;
	return S_OK;
}

STDMETHODIMP CProperties::get_RefProperty(IDispatch** pVal)
{
	if(pVal == NULL)
		return E_POINTER;
	*pVal = m_dispVal;
    if (m_dispVal != NULL) 
    {
        m_dispVal->AddRef();
    }
	return S_OK;
}

STDMETHODIMP CProperties::putref_RefProperty(IDispatch* newVal)
{    
    // if we're setting to the same value avoid Release/AddRef which
    // could drop to zero and then come back to life.
    if (m_dispVal != newVal) {
        if (newVal == this) {
            // avoid the circular reference...
            m_dispVal = this;
        } else {
            // release if we already have a value
            if (m_dispVal != NULL && m_dispVal != this) {
                m_dispVal->Release();
            }
            // install the new value
            m_dispVal = *&newVal;
            if (newVal !=NULL) 
            {
                newVal->AddRef();
            }
        }
    }
	return S_OK;
}

STDMETHODIMP CProperties::get_PutAndPutRefProperty(DOUBLE* pVal)
{	
	*pVal = m_dblVal;
	return S_OK;
}

STDMETHODIMP CProperties::put_PutAndPutRefProperty(DOUBLE newVal)
{
	m_dblVal = newVal;
	return S_OK;
}

STDMETHODIMP CProperties::putref_PutAndPutRefProperty(DOUBLE* newVal)
{
	if(newVal != NULL)
		m_dblVal = *newVal * 2;
	return S_OK;
}

STDMETHODIMP CProperties::get_PropertyWithParam(DOUBLE a, DOUBLE* pVal)
{
	*pVal = m_propertyWithParam - a;

	return S_OK;
}

STDMETHODIMP CProperties::put_PropertyWithParam(DOUBLE a, DOUBLE newVal)
{
    m_propertyWithParam = a + newVal;
	return S_OK;
}

STDMETHODIMP CProperties::get_ReadOnlyProperty(CHAR* pVal)
{
	*pVal = 'c';
	return S_OK;
}

STDMETHODIMP CProperties::put_WriteOnlyProperty(DATE newVal)
{
	//Do nothing
	return S_OK;
}

STDMETHODIMP CProperties::get_PropertyWithOutParam(BSTR* a, BSTR* pVal)
{
	*a = *pVal = m_outParamVal;
	return S_OK;
}

STDMETHODIMP CProperties::put_PropertyWithOutParam(BSTR* a, BSTR newVal)
{
	if(m_outParamVal != NULL)
	{
		SysFreeString(m_outParamVal);
	}

	if(newVal == NULL)
	{
		m_outParamVal = newVal;
	}
	else
	{
		m_outParamVal = SysAllocString(newVal);
	}
	return S_OK;
}

STDMETHODIMP CProperties::get_PropertyWithTwoParams(DOUBLE a, DOUBLE b, DOUBLE* pVal)
{
    *pVal = m_twoParamsVal - a - b;
	return S_OK;
}

STDMETHODIMP CProperties::put_PropertyWithTwoParams(DOUBLE a, DOUBLE b, DOUBLE newVal)
{
	m_twoParamsVal = newVal + a + b;
	return S_OK;
}

STDMETHODIMP CProperties::get_DefaultProperty(SHORT a, VARIANT_BOOL* pVal)
{
	*pVal = m_defaultVal;
	return S_OK;
}

STDMETHODIMP CProperties::put_DefaultProperty(SHORT a, VARIANT_BOOL newVal)
{
	m_defaultVal = newVal;
	return S_OK;
}
