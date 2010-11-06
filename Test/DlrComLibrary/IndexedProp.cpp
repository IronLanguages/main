// IndexedProp.cpp : Implementation of CIndexedProp

#include "stdafx.h"
#include "IndexedProp.h"
#include "atlsafe.h"
#include "atlbase.h"

// CIndexedProp



STDMETHODIMP CIndexedProp::get_CharZero(CHAR* pVal)
{
*pVal = charZero;
return S_OK;
}


STDMETHODIMP CIndexedProp::put_CharZero(CHAR newVal)
{
charZero= newVal;
return S_OK;
}


STDMETHODIMP CIndexedProp::get_IntZero(LONG* pVal)
{
*pVal=intZero;
return S_OK;
}


STDMETHODIMP CIndexedProp::put_IntZero(LONG newVal)
{
intZero = newVal;
return S_OK;
}


STDMETHODIMP CIndexedProp::get_IntOne(LONG one, LONG* pVal)
{
*pVal=intOne;
return S_OK;
}


STDMETHODIMP CIndexedProp::put_IntOne(LONG one, LONG newVal)
{
intOne = newVal;
return S_OK;
}


STDMETHODIMP CIndexedProp::get_FloatOne(FLOAT one, FLOAT* pVal)
{
*pVal=floatOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_FloatOne(FLOAT one, FLOAT newVal)
{
floatOne = newVal;
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_IntTwo(LONG one, LONG two, LONG* pVal)
{
*pVal=intOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_IntTwo(LONG one, LONG two, LONG newVal)
{
intOne = newVal + two;
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_CharOne(CHAR one, CHAR* pVal)
{

*pVal = charOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_CharOne(CHAR one, CHAR newVal)
{

charOne=newVal;
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_StringOne(BSTR one, BSTR* pVal)
{
	*pVal = stringOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_StringOne(BSTR one, BSTR newVal)
{
	if(stringOne != NULL)
	{
		SysFreeString(stringOne);
	}

	if(newVal == NULL)
	{
		stringOne = newVal;
	}
	else
	{
		stringOne = SysAllocString(newVal);
	}
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_PointOne(Point one, Point* pVal)
{
	
	*pVal = pointOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_PointOne(Point one, Point newVal)
{
	
	pointOne=newVal;
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_EnumOne(Numbers one, Numbers* pVal)
{
	
*pVal = numberOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_EnumOne(Numbers one, Numbers newVal)
{
	
numberOne=newVal;
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_DecimalOne(DECIMAL one, DECIMAL* pVal)
{
	*pVal= decimalOne;

	return S_OK;
}


STDMETHODIMP CIndexedProp::put_DecimalOne(DECIMAL one, DECIMAL newVal)
{
	decimalOne=newVal;
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_DoubleOne(DOUBLE one, DOUBLE* pVal)
{
	*pVal=doubleOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_DoubleOne(DOUBLE one, DOUBLE newVal)
{
	doubleOne=newVal;
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_LongOne(LONGLONG one, LONGLONG* pVal)
{
	*pVal=longOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_LongOne(LONGLONG one, LONGLONG newVal)
{
	longOne=newVal;
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_ObjectOne(VARIANT one, VARIANT* pVal)
{
	*pVal=objectOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_ObjectOne(VARIANT one, VARIANT newVal)
{
	objectOne=newVal;
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_UIntOne(ULONG one, ULONG* pVal)
{
	*pVal=uintOne;

	return S_OK;
}


STDMETHODIMP CIndexedProp::put_UIntOne(ULONG one, ULONG newVal)
{
	uintOne=newVal;
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_IntRefOne(LONG one, LONG* pVal)
{
	*pVal=intOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::putref_IntRefOne(LONG one, LONG newVal)
{
	intOne=newVal;
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_IntOneGetter(LONG one, LONG* pVal)
{
	*pVal=intOne;

	return S_OK;
}


STDMETHODIMP CIndexedProp::put_IntOneSetter(LONG one, LONG newVal)
{
	intOne=newVal;
	return S_OK;
}





STDMETHODIMP CIndexedProp::get_IntRefTwo(LONG* one, LONG* two, LONG* pVal)
{
	*pVal=intOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_IntRefTwo(LONG* one, LONG* two, LONG newVal)
{
	intOne=newVal+*two;
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_BoolOne(VARIANT_BOOL one, VARIANT_BOOL* pVal)
{
	*pVal=boolOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_BoolOne(VARIANT_BOOL one, VARIANT_BOOL newVal)
{
	boolOne=newVal;
	return S_OK;
}




STDMETHODIMP CIndexedProp::get_IntSix(LONG one, LONG two, LONG three, LONG four, LONG five, LONG six, LONG* pVal)
{
	*pVal=intOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_IntSix(LONG one, LONG two, LONG three, LONG four, LONG five, LONG six, LONG newVal)
{
	intOne=newVal;
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_intArrayOne(LONG one, SAFEARRAY** pVal)
{
	    CComSafeArray<INT>      csaData;
    void HUGEP              *pvData = NULL;
    HRESULT hr = S_OK;

    int rgData [] = { 1, 2, 3, 4, 5 };

    hr = csaData.Create(5, 0);
    if (FAILED(hr)) 
        goto Error;

    hr = SafeArrayAccessData((LPSAFEARRAY)csaData, (void HUGEP **)&pvData);
    if (FAILED(hr))
        goto Error;

    memcpy((void*)pvData, rgData, sizeof(rgData));

    hr = SafeArrayUnaccessData((LPSAFEARRAY)csaData);
    if (FAILED(hr))
        goto Error;

    *pVal = csaData.Detach();

Error:
//pVal=intArrayOne;
    return hr;
}


STDMETHODIMP CIndexedProp::put_intArrayOne(LONG one, SAFEARRAY* newVal)
{

    HRESULT hr = S_OK;
//SafeArrayCopy(newVal, intArrayOne);

    return hr;
}


STDMETHODIMP CIndexedProp::get_stringArray(LONG one,SAFEARRAY** pVal)
{
		    CComSafeArray<BSTR>      csaData;
    void HUGEP              *pvData = NULL;
    HRESULT hr = S_OK;

    BSTR rgData [] = { L"a", L"b", L"c", L"d", L"e"};

    hr = csaData.Create(5, 0);
    if (FAILED(hr)) 
        goto Error;

    hr = SafeArrayAccessData((LPSAFEARRAY)csaData, (void HUGEP **)&pvData);
    if (FAILED(hr))
        goto Error;

    memcpy((void*)pvData, rgData, sizeof(rgData));

    hr = SafeArrayUnaccessData((LPSAFEARRAY)csaData);
    if (FAILED(hr))
        goto Error;

    *pVal = csaData.Detach();

Error:
//pVal=intArrayOne;
    return hr;
}


STDMETHODIMP CIndexedProp::put_stringArray(LONG one, SAFEARRAY* newVal)
{
	// TODO: Add your implementation code here

	return S_OK;
}





STDMETHODIMP CIndexedProp::get_IntReqOne(LONG one, LONG* pVal)
{
	// TODO: Add your implementation code here
	*pVal=intOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_IntReqOne(LONG one, LONG newVal)
{
	// TODO: Add your implementation code here
intOne=newVal;
	return S_OK;
}

STDMETHODIMP CIndexedProp::get_IntOverload(LONG one, LONG* pVal)
{
	// TODO: Add your implementation code here
	*pVal=intOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_IntOverload(LONG one, LONG newVal)
{
	// TODO: Add your implementation code here
intOne=newVal;
	return S_OK;
}

//STDMETHODIMP CIndexedProp::get_IntOverload(LONG one, LONG two, LONG* pVal)
//{
//	// TODO: Add your implementation code here
//	*pVal=intOne;
//	return S_OK;
//}
//
//
//STDMETHODIMP CIndexedProp::put_IntOverload(LONG one,LONG two, LONG newVal)
//{
//	// TODO: Add your implementation code here
//	intOne=newVal;
//	return S_OK;
//}



STDMETHODIMP CIndexedProp::get_intArrayOneNO(LONG one, SAFEARRAY** pVal)
{
	    CComSafeArray<INT>      csaData;
    void HUGEP              *pvData = NULL;
    HRESULT hr = S_OK;

    int rgData [] = { 1, 2, 3, 4, 5 };

    hr = csaData.Create(5, 0);
    if (FAILED(hr)) 
        goto Error;

    hr = SafeArrayAccessData((LPSAFEARRAY)csaData, (void HUGEP **)&pvData);
    if (FAILED(hr))
        goto Error;

    memcpy((void*)pvData, rgData, sizeof(rgData));

    hr = SafeArrayUnaccessData((LPSAFEARRAY)csaData);
    if (FAILED(hr))
        goto Error;

    *pVal = csaData.Detach();

Error:
//pVal=intArrayOne;
    return hr;
}


STDMETHODIMP CIndexedProp::put_intArrayOneNO(LONG one, SAFEARRAY* newVal)
{

    HRESULT hr = S_OK;
//SafeArrayCopy(newVal, intArrayOne);

    return hr;
}

STDMETHODIMP CIndexedProp::methTest(LONG One, LONG Two, LONG Three)
{
	// TODO: Add your implementation code here
	intOne=One+Two+Three;
	return S_OK;
}


STDMETHODIMP CIndexedProp::get_intDefault(LONG One, LONG Two, LONG* pVal)
{
	// TODO: Add your implementation code here
*pVal=intOne;
	return S_OK;
}


STDMETHODIMP CIndexedProp::put_intDefault(LONG One, LONG Two, LONG newVal)
{
	// TODO: Add your implementation code here
intOne=newVal+One+Two;
	return S_OK;
}
