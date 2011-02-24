// IndexedProp.h : Declaration of the CIndexedProp

#pragma once
#include "resource.h"       // main symbols

#include "DlrComLibrary_i.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CIndexedProp

class ATL_NO_VTABLE CIndexedProp :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CIndexedProp, &CLSID_IndexedProp>,
	public IDispatchImpl<IIndexedProp, &IID_IIndexedProp, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	LONG intOne;
	LONG intZero;
	Point pointOne;
	Numbers numberOne;
	FLOAT floatZero;
	FLOAT floatOne;
	CHAR charZero;
	CHAR charOne;
	BSTR stringOne;
	static const Point conPoint;
	static const Numbers conNumbers;
	DECIMAL decimalOne;
	LONGLONG longOne;
	ULONG uintOne;
	DOUBLE doubleOne;
	VARIANT objectOne;
	SAFEARRAY** intArrayOne;
VARIANT_BOOL boolOne;
	CIndexedProp()
	{intOne=0;
	floatZero=0;
	floatOne=0;
stringOne=L"";
	}

DECLARE_REGISTRY_RESOURCEID(IDR_INDEXEDPROP)


BEGIN_COM_MAP(CIndexedProp)
	COM_INTERFACE_ENTRY(IIndexedProp)
	COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:

STDMETHOD(get_CharZero)(CHAR* pVal=NULL);
STDMETHOD(put_CharZero)(CHAR newVal='c');
STDMETHOD(get_IntZero)(LONG* pVal=NULL);
STDMETHOD(put_IntZero)(LONG newVal=1);
STDMETHOD(get_IntOne)(LONG one=1, LONG* pVal=NULL);
STDMETHOD(put_IntOne)(LONG one=1, LONG newVal=1);
STDMETHOD(get_FloatOne)(FLOAT one=1.1, FLOAT* pVal=NULL);
STDMETHOD(put_FloatOne)(FLOAT one=1.1, FLOAT newVal=2.2);
STDMETHOD(get_IntTwo)(LONG one=1, LONG two=2, LONG* pVal=NULL);
STDMETHOD(put_IntTwo)(LONG one=1, LONG two=2, LONG newVal=3);
STDMETHOD(get_CharOne)(CHAR one='c', CHAR* pVal=NULL);
STDMETHOD(put_CharOne)(CHAR one='c', CHAR newVal='n');
STDMETHOD(get_StringOne)(BSTR one=L"1234", BSTR* pVal=NULL);
STDMETHOD(put_StringOne)(BSTR one=L"1234", BSTR newVal=L"1234");
STDMETHOD(get_PointOne)(Point one=conPoint, Point* pVal=NULL);
STDMETHOD(put_PointOne)(Point one=conPoint, Point newVal=conPoint);
STDMETHOD(get_EnumOne)(Numbers one=conNumbers, Numbers* pVal=NULL);
STDMETHOD(put_EnumOne)(Numbers one=conNumbers, Numbers newVal=conNumbers);
STDMETHOD(get_DecimalOne)(DECIMAL one, DECIMAL* pVal);
STDMETHOD(put_DecimalOne)(DECIMAL one, DECIMAL newVal);
STDMETHOD(get_DoubleOne)(DOUBLE one, DOUBLE* pVal);
STDMETHOD(put_DoubleOne)(DOUBLE one, DOUBLE newVal);
STDMETHOD(get_LongOne)(LONGLONG one, LONGLONG* pVal);
STDMETHOD(put_LongOne)(LONGLONG one, LONGLONG newVal);
STDMETHOD(get_ObjectOne)(VARIANT one, VARIANT* pVal);
STDMETHOD(put_ObjectOne)(VARIANT one, VARIANT newVal);
STDMETHOD(get_UIntOne)(ULONG one, ULONG* pVal);
STDMETHOD(put_UIntOne)(ULONG one, ULONG newVal);
STDMETHOD(get_IntRefOne)(LONG one, LONG* pVal);
STDMETHOD(putref_IntRefOne)(LONG one, LONG newVal);
STDMETHOD(get_IntOneGetter)(LONG one, LONG* pVal);
STDMETHOD(put_IntOneSetter)(LONG one, LONG newVal);

STDMETHOD(get_IntRefTwo)(LONG* one, LONG* two, LONG* pVal);
STDMETHOD(put_IntRefTwo)(LONG* one, LONG* two, LONG newVal);
STDMETHOD(get_BoolOne)(VARIANT_BOOL one, VARIANT_BOOL* pVal);
STDMETHOD(put_BoolOne)(VARIANT_BOOL one, VARIANT_BOOL newVal);

STDMETHOD(get_IntSix)(LONG one, LONG two, LONG three, LONG four, LONG five, LONG six, LONG* pVal);
STDMETHOD(put_IntSix)(LONG one, LONG two, LONG three, LONG four, LONG five, LONG six, LONG newVal);
STDMETHOD(get_intArrayOne)(LONG one, SAFEARRAY** pVal);
STDMETHOD(put_intArrayOne)(LONG one, SAFEARRAY* newVal);
STDMETHOD(get_intArrayOneNO)(LONG one, SAFEARRAY** pVal);
STDMETHOD(put_intArrayOneNO)(LONG one, SAFEARRAY* newVal);
STDMETHOD(get_stringArray)(LONG one,SAFEARRAY** pVal);
STDMETHOD(put_stringArray)(LONG one, SAFEARRAY* newVal);

	STDMETHOD(get_IntReqOne)(LONG one, LONG* pVal);
	STDMETHOD(put_IntReqOne)(LONG one, LONG newVal);

	STDMETHOD(get_IntOverload)(LONG one, LONG* pVal);
	STDMETHOD(put_IntOverload)(LONG one, LONG newVal);

	//STDMETHOD(get_IntOverload)(LONG one,LONG two, LONG* pVal);
	//STDMETHOD(put_IntOverload)(LONG one,LONG two, LONG newVal);
	STDMETHOD(methTest)(LONG One, LONG Two, LONG Three);
	STDMETHOD(get_intDefault)(LONG One, LONG Two, LONG* pVal);
	STDMETHOD(put_intDefault)(LONG One, LONG Two, LONG newVal);
};

OBJECT_ENTRY_AUTO(__uuidof(IndexedProp), CIndexedProp)
