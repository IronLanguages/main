// SimpleErrors.h : Declaration of the CSimpleErrors

#pragma once
#include "resource.h"       // main symbols

#include "DlrComLibrary_i.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CSimpleErrors

class ATL_NO_VTABLE CSimpleErrors :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CSimpleErrors, &CLSID_SimpleErrors>,
	public IDispatchImpl<ISimpleErrors, &IID_ISimpleErrors, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CSimpleErrors()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_SIMPLEERRORS)


BEGIN_COM_MAP(CSimpleErrors)
	COM_INTERFACE_ENTRY(ISimpleErrors)
	COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		s_cConstructed++;
		return S_OK;
	}

	void FinalRelease()
	{
		s_cReleased++;
	}

public:

	static BOOL AllDestructed() { return s_cConstructed == s_cReleased; }

private:
	static int s_cConstructed;
    static int s_cReleased;

public:
	STDMETHOD(genMseeAppDomainUnloaded)(void);
	STDMETHOD(genCorApplication)(void);
	STDMETHOD(genCorArgument)(void);
	STDMETHOD(genInvalidArg)(void);
	STDMETHOD(genCorArgumentOutOfRange)(void);
	STDMETHOD(genCorArithmetic)(void);
	STDMETHOD(genErrorArithmeticOverflow)(void);
	STDMETHOD(genCorArrayTypeMismatch)(void);
	STDMETHOD(genCorBadImageFormat)(void);
	STDMETHOD(genErrorBadFormat)(void);
	STDMETHOD(genCorContextMarshal)(void);
	STDMETHOD(genNTEFail)(void);
	STDMETHOD(genCorDirectoryNotFound)(void);
	STDMETHOD(genErrorPathNotFound)(void);
	STDMETHOD(genCorDivideByZero)(void);
	STDMETHOD(genCorDuplicateWaitObject)(void);
	STDMETHOD(genCorEndOfStream)(void);
	STDMETHOD(genCorTypeLoad)(void);
	STDMETHOD(genCorException)(void);
	STDMETHOD(genCorExecutionEngine)(void);
	STDMETHOD(genCorFieldAccess)(void);
	STDMETHOD(genCorFileNotFound)(void);
	STDMETHOD(genErrorFileNotFound)(void);
	STDMETHOD(genCorFormat)(void);
	STDMETHOD(genCorIndexOutOfRange)(void);
	STDMETHOD(genCorInvalidCast)(void);
	STDMETHOD(genNoInterface)(void);
	STDMETHOD(genCorInvalidCOMObject)(void);
	STDMETHOD(genCorInvalidFilterCriteria)(void);
	STDMETHOD(genCorInvalidOleVariantType)(void);
	STDMETHOD(genCorInvalidOperation)(void);
	STDMETHOD(genCorIO)(void);
	STDMETHOD(genCorMemberAccess)(void);
	STDMETHOD(genCorMethodAccess)(void);
	STDMETHOD(genCorMissingField)(void);
	STDMETHOD(genCorMissingManifestResource)(void);
	STDMETHOD(genCorMissingMember)(void);
	STDMETHOD(genCorMissingMethod)(void);
	STDMETHOD(genCorMulticastNotSupported)(void);
	STDMETHOD(genCorNotFiniteNumber)(void);
	STDMETHOD(genNotImpl)(void);
	STDMETHOD(genCorNotSupported)(void);
	STDMETHOD(genCorNullReference)(void);
	STDMETHOD(genPointer)(void);
	STDMETHOD(genCorOutOfMemory)(void);
	STDMETHOD(genOutOfMemory)(void);
	STDMETHOD(genCorOverflow)(void);
	STDMETHOD(genCorPathTooLong)(void);
	STDMETHOD(genErrorFilenameExcedRange)(void);
	STDMETHOD(genCorRank)(void);
	STDMETHOD(genCorTargetInvocation)(void);
	STDMETHOD(genCorReflectionTypeLoad)(void);
	STDMETHOD(genCorRemoting)(void);
	STDMETHOD(genCorSafeArrayTypeMismatch)(void);
	STDMETHOD(genCorSecurity)(void);
	STDMETHOD(genCorSerialization)(void);
	STDMETHOD(genCorStackOverflow)(void);
	STDMETHOD(genErrorStackOverflow)(void);
	STDMETHOD(genCorSynchronizationLock)(void);
	STDMETHOD(genCorSystem)(void);
	STDMETHOD(genCorTarget)(void);
	STDMETHOD(genCorTargetParamCount)(void);
	STDMETHOD(genCorThreadAborted)(void);
	STDMETHOD(genCorThreadInterrupted)(void);
	STDMETHOD(genCorThreadState)(void);
	STDMETHOD(genCorThreadStop)(void);
	STDMETHOD(genCorTypeInitialization)(void);
	STDMETHOD(genCorVerification)(void);
	STDMETHOD(genUndefinedHresult)(ULONG hresult);
	STDMETHOD(genDispArrayIsLocked)(void);
	STDMETHOD(genDispBadCallee)(void);
	STDMETHOD(genDispBadIndex)(void);
	STDMETHOD(genDispBadParamCount)(void);
	STDMETHOD(genDispBadVarType)(void);
	STDMETHOD(genDispBufferTooSmall)(void);
	STDMETHOD(genDispDivByZero)(void);
	STDMETHOD(genDispException)(void);
	STDMETHOD(genDispMemberNotFound)(void);
	STDMETHOD(genDispNoNamedArgs)(void);
	STDMETHOD(genDispNotACollection)(void);
	STDMETHOD(genDispOverflow)(void);
	STDMETHOD(genDispParamNotFound)(void);
	STDMETHOD(genDispParamNotOptional)(void);
	STDMETHOD(genDispTypeMismatch)(void);
	STDMETHOD(genDispUnknownInterface)(void);
	STDMETHOD(genDispUnknownLCID)(void);
	STDMETHOD(genDispUnknownName)(void);
};

OBJECT_ENTRY_AUTO(__uuidof(SimpleErrors), CSimpleErrors)
