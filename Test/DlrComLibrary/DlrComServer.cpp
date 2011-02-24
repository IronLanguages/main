// DlrComServer.cpp : Implementation of CDlrComServer

#include "stdafx.h"
#include "DlrComServer.h"
#include "atlsafe.h"
#include "atlbase.h"


int CDlrComServer::s_cConstructed;
int CDlrComServer::s_cReleased;
// CDlrComServer

STDMETHODIMP CDlrComServer::InterfaceSupportsErrorInfo(REFIID riid)
{
    static const IID* arr[] = 
    {
        &IID_IDlrComServer
    };

    for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
    {
        if (InlineIsEqualGUID(*arr[i],riid))
            return S_OK;
    }
    return S_FALSE;
}

STDMETHODIMP CDlrComServer::SimpleMethod(void)
{
    return S_OK;
}

STDMETHODIMP CDlrComServer::IntArguments(LONG arg1, LONG arg2)
{
    return S_OK;
}

STDMETHODIMP CDlrComServer::StringArguments(BSTR arg1, BSTR arg2)
{
    return S_OK;
}

STDMETHODIMP CDlrComServer::ObjectArguments(IUnknown* arg1, IUnknown* arg2)
{
    return S_OK;
}

STDMETHODIMP CDlrComServer::GetByteArray(SAFEARRAY **ppsaRetVal)
{
    CComBSTR cbstrTestData(L"GetByteArrayTestData");
    CComSafeArray<BYTE>     csaData;
    void HUGEP              *pvData = NULL;
    DWORD                   cbData;
    HRESULT hr = S_OK;

    // create a stream of bytes encoded as UNICODE text
    cbData = SysStringByteLen(cbstrTestData.m_str);

    hr = csaData.Create(cbData, 0);
    if (FAILED(hr)) 
        goto Error;

    hr = SafeArrayAccessData((LPSAFEARRAY)csaData, (void HUGEP **)&pvData);
    if (FAILED(hr))
        goto Error;

    memcpy((void*)pvData, cbstrTestData.m_str, cbData);

    hr = SafeArrayUnaccessData((LPSAFEARRAY)csaData);
    if (FAILED(hr))
        goto Error;

    if (FAILED(hr))
        goto Error;

    *ppsaRetVal = csaData.Detach();

Error:

    return hr;
}

STDMETHODIMP CDlrComServer::GetIntArray(SAFEARRAY **ppsaRetVal)
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

    *ppsaRetVal = csaData.Detach();

Error:

    return hr;
}


STDMETHODIMP CDlrComServer::GetObjArray(SAFEARRAY **ppsaRetVal)
{
    CComSafeArray<LPUNKNOWN>      csaData;
    void HUGEP              *pvData = NULL;
    IUnknown*                     punkThis = NULL;
    IUnknown*                     punkOther = NULL;
    void* rgData[] = { 0, 0 };
    HRESULT hr = S_OK;

    hr = csaData.Create(2, 0);
    if (FAILED(hr)) 
        goto Error;

    hr = SafeArrayAccessData((LPSAFEARRAY)csaData, (void HUGEP **)&pvData);
    if (FAILED(hr))
        goto Error;

    hr = this->QueryInterface(IID_IUnknown, (void**)&punkThis);
    if (FAILED(hr))
        goto Error;

    hr = CoCreateInstance(CLSID_DlrComServer, NULL, CLSCTX_INPROC_SERVER, IID_IUnknown, (void**)&punkOther);
    if (FAILED(hr))
        goto Error;

    rgData[0] = punkThis;
    rgData[1] = punkOther;

    memcpy((void*)pvData, rgData, sizeof(rgData));

    hr = SafeArrayUnaccessData((LPSAFEARRAY)csaData);
    if (FAILED(hr))
        goto Error;

    punkThis = 0;
    punkOther = 0;

    *ppsaRetVal = csaData.Detach();

Error:
    if (punkThis)
        punkThis->Release();

    if (punkOther)
        punkOther->Release();

    return hr;
}

STDMETHODIMP CDlrComServer::TestErrorInfo(void)
{
	AtlSetErrorInfo(CLSID_DlrComServer, OLESTR("Test error message"), 0, NULL, IID_IDlrComServer, E_FAIL, NULL);

	return E_FAIL;
}

STDMETHODIMP CDlrComServer::SumArgs(LONG a1, LONG a2, LONG a3, LONG a4, LONG a5, LONG* result)
{
	*result = (10000*a1) + (1000*a2) + (100*a3) + (10*a4) + a5;

	return S_OK;
}

STDMETHODIMP CDlrComServer::get__NewEnum(IUnknown** ppUnk)
{
     if (ppUnk == NULL)
        return E_POINTER;
    *ppUnk = NULL;

    CComObject<VariantComEnum>* pEnum = NULL;
    HRESULT hr = CComObject<VariantComEnum>::CreateInstance(&pEnum);

    if (FAILED(hr))
        return hr;

    hr = pEnum->Init(&m_arr[0], &m_arr[NUMELEMENTS], reinterpret_cast<IUnknown*>(this), AtlFlagNoCopy);

    if (SUCCEEDED(hr))
        hr = pEnum->QueryInterface(ppUnk);

    if (FAILED(hr))
        delete pEnum;

    return hr;
}