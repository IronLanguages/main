// SimpleErrors.cpp : Implementation of CSimpleErrors

#include "stdafx.h"
#include "SimpleErrors.h"
#include "corerror.h"

int CSimpleErrors::s_cConstructed;
int CSimpleErrors::s_cReleased;

// CSimpleErrors


STDMETHODIMP CSimpleErrors::genMseeAppDomainUnloaded(void)
{
	//return MSEE_E_APPDOMAINUNLOADED;
	return 0x80131015;
}

STDMETHODIMP CSimpleErrors::genCorApplication(void)
{
	return COR_E_APPLICATION;
}

STDMETHODIMP CSimpleErrors::genCorArgument(void)
{
	return COR_E_ARGUMENT;
}

STDMETHODIMP CSimpleErrors::genInvalidArg(void)
{
	return E_INVALIDARG;
}

STDMETHODIMP CSimpleErrors::genCorArgumentOutOfRange(void)
{
	return COR_E_ARGUMENTOUTOFRANGE;
}

STDMETHODIMP CSimpleErrors::genCorArithmetic(void)
{
	return COR_E_ARITHMETIC;
}

STDMETHODIMP CSimpleErrors::genErrorArithmeticOverflow(void)
{
	return ERROR_ARITHMETIC_OVERFLOW;
}

STDMETHODIMP CSimpleErrors::genCorArrayTypeMismatch(void)
{
	return COR_E_ARRAYTYPEMISMATCH;
}

STDMETHODIMP CSimpleErrors::genCorBadImageFormat(void)
{
	return COR_E_BADIMAGEFORMAT;
}

STDMETHODIMP CSimpleErrors::genErrorBadFormat(void)
{
	return ERROR_BAD_FORMAT;
}

STDMETHODIMP CSimpleErrors::genCorContextMarshal(void)
{
	return COR_E_CONTEXTMARSHAL;
}

STDMETHODIMP CSimpleErrors::genNTEFail(void)
{
	return NTE_FAIL;
}

STDMETHODIMP CSimpleErrors::genCorDirectoryNotFound(void)
{
	return COR_E_DIRECTORYNOTFOUND;
}

STDMETHODIMP CSimpleErrors::genErrorPathNotFound(void)
{
	return ERROR_PATH_NOT_FOUND;
}

STDMETHODIMP CSimpleErrors::genCorDivideByZero(void)
{
	return COR_E_DIVIDEBYZERO;
}

STDMETHODIMP CSimpleErrors::genCorDuplicateWaitObject(void)
{
	return COR_E_DUPLICATEWAITOBJECT;
}

STDMETHODIMP CSimpleErrors::genCorEndOfStream(void)
{
	return COR_E_ENDOFSTREAM;
}

STDMETHODIMP CSimpleErrors::genCorTypeLoad(void)
{
	return COR_E_TYPELOAD;
}

STDMETHODIMP CSimpleErrors::genCorException(void)
{
	return COR_E_EXCEPTION;
}

STDMETHODIMP CSimpleErrors::genCorExecutionEngine(void)
{
	return COR_E_EXECUTIONENGINE;
}

STDMETHODIMP CSimpleErrors::genCorFieldAccess(void)
{
	return COR_E_FIELDACCESS;
}

STDMETHODIMP CSimpleErrors::genCorFileNotFound(void)
{
	return COR_E_FILENOTFOUND;
}

STDMETHODIMP CSimpleErrors::genErrorFileNotFound(void)
{
	return ERROR_FILE_NOT_FOUND;
}

STDMETHODIMP CSimpleErrors::genCorFormat(void)
{
	return COR_E_FORMAT;
}

STDMETHODIMP CSimpleErrors::genCorIndexOutOfRange(void)
{
	return COR_E_INDEXOUTOFRANGE;
}

STDMETHODIMP CSimpleErrors::genCorInvalidCast(void)
{
	return COR_E_INVALIDCAST;
}

STDMETHODIMP CSimpleErrors::genNoInterface(void)
{
	return E_NOINTERFACE;
}

STDMETHODIMP CSimpleErrors::genCorInvalidCOMObject(void)
{
	return COR_E_INVALIDCOMOBJECT;
}

STDMETHODIMP CSimpleErrors::genCorInvalidFilterCriteria(void)
{
	return COR_E_INVALIDFILTERCRITERIA;
}

STDMETHODIMP CSimpleErrors::genCorInvalidOleVariantType(void)
{
	return COR_E_INVALIDOLEVARIANTTYPE;
}

STDMETHODIMP CSimpleErrors::genCorInvalidOperation(void)
{
	return COR_E_INVALIDOPERATION;
}

STDMETHODIMP CSimpleErrors::genCorIO(void)
{
	return COR_E_IO;
}

STDMETHODIMP CSimpleErrors::genCorMemberAccess(void)
{
	return COR_E_MEMBERACCESS;
}

STDMETHODIMP CSimpleErrors::genCorMethodAccess(void)
{
	return COR_E_METHODACCESS;
}

STDMETHODIMP CSimpleErrors::genCorMissingField(void)
{
	return COR_E_MISSINGFIELD;
}

STDMETHODIMP CSimpleErrors::genCorMissingManifestResource(void)
{
	return COR_E_MISSINGMANIFESTRESOURCE;
}

STDMETHODIMP CSimpleErrors::genCorMissingMember(void)
{
	return COR_E_MISSINGMEMBER;
}

STDMETHODIMP CSimpleErrors::genCorMissingMethod(void)
{
	return COR_E_MISSINGMETHOD;
}

STDMETHODIMP CSimpleErrors::genCorMulticastNotSupported(void)
{
	return COR_E_MULTICASTNOTSUPPORTED;
}

STDMETHODIMP CSimpleErrors::genCorNotFiniteNumber(void)
{
	return COR_E_NOTFINITENUMBER;
}

STDMETHODIMP CSimpleErrors::genNotImpl(void)
{
	return E_NOTIMPL;
}

STDMETHODIMP CSimpleErrors::genCorNotSupported(void)
{
	return COR_E_NOTSUPPORTED;
}

STDMETHODIMP CSimpleErrors::genCorNullReference(void)
{
	return COR_E_NULLREFERENCE;
}

STDMETHODIMP CSimpleErrors::genPointer(void)
{
	return E_POINTER;
}

STDMETHODIMP CSimpleErrors::genCorOutOfMemory(void)
{
	return COR_E_OUTOFMEMORY;
}

STDMETHODIMP CSimpleErrors::genOutOfMemory(void)
{
	return E_OUTOFMEMORY;
}

STDMETHODIMP CSimpleErrors::genCorOverflow(void)
{
	return COR_E_OVERFLOW;
}

STDMETHODIMP CSimpleErrors::genCorPathTooLong(void)
{
	return COR_E_PATHTOOLONG;
}

STDMETHODIMP CSimpleErrors::genErrorFilenameExcedRange(void)
{
	return ERROR_FILENAME_EXCED_RANGE;
}

STDMETHODIMP CSimpleErrors::genCorRank(void)
{
	return COR_E_RANK;
}

STDMETHODIMP CSimpleErrors::genCorTargetInvocation(void)
{
	return COR_E_TARGETINVOCATION;
}

STDMETHODIMP CSimpleErrors::genCorReflectionTypeLoad(void)
{
	return COR_E_REFLECTIONTYPELOAD;
}

STDMETHODIMP CSimpleErrors::genCorRemoting(void)
{
	return COR_E_REMOTING;
}

STDMETHODIMP CSimpleErrors::genCorSafeArrayTypeMismatch(void)
{
	return COR_E_SAFEARRAYTYPEMISMATCH;
}

STDMETHODIMP CSimpleErrors::genCorSecurity(void)
{
	return COR_E_SECURITY;
}

STDMETHODIMP CSimpleErrors::genCorSerialization(void)
{
	return COR_E_SERIALIZATION;
}

STDMETHODIMP CSimpleErrors::genCorStackOverflow(void)
{
	return COR_E_STACKOVERFLOW;
}

STDMETHODIMP CSimpleErrors::genErrorStackOverflow(void)
{
	return ERROR_STACK_OVERFLOW;
}

STDMETHODIMP CSimpleErrors::genCorSynchronizationLock(void)
{
	return COR_E_SYNCHRONIZATIONLOCK;
}

STDMETHODIMP CSimpleErrors::genCorSystem(void)
{
	return COR_E_SYSTEM;
}

STDMETHODIMP CSimpleErrors::genCorTarget(void)
{
	return COR_E_TARGET;
}

STDMETHODIMP CSimpleErrors::genCorTargetParamCount(void)
{
	return COR_E_TARGETPARAMCOUNT;
}

STDMETHODIMP CSimpleErrors::genCorThreadAborted(void)
{
	return COR_E_THREADABORTED;
}

STDMETHODIMP CSimpleErrors::genCorThreadInterrupted(void)
{
	return COR_E_THREADINTERRUPTED;
}

STDMETHODIMP CSimpleErrors::genCorThreadState(void)
{
	return COR_E_THREADSTATE;
}

STDMETHODIMP CSimpleErrors::genCorThreadStop(void)
{
	return COR_E_THREADSTOP;
}

STDMETHODIMP CSimpleErrors::genCorTypeInitialization(void)
{
	return COR_E_TYPEINITIALIZATION;
}

STDMETHODIMP CSimpleErrors::genCorVerification(void)
{
	return COR_E_VERIFICATION;
}

STDMETHODIMP CSimpleErrors::genUndefinedHresult(ULONG hresult)
{
	return (HRESULT)hresult;
}

STDMETHODIMP CSimpleErrors::genDispArrayIsLocked(void)
{
	return DISP_E_ARRAYISLOCKED ;
}

STDMETHODIMP CSimpleErrors::genDispBadCallee(void)
{
	return DISP_E_BADCALLEE;
}

STDMETHODIMP CSimpleErrors::genDispBadIndex(void)
{
	return DISP_E_BADINDEX;
}

STDMETHODIMP CSimpleErrors::genDispBadParamCount(void)
{
	return DISP_E_BADPARAMCOUNT;
}

STDMETHODIMP CSimpleErrors::genDispBadVarType(void)
{
	return DISP_E_BADVARTYPE;
}

STDMETHODIMP CSimpleErrors::genDispBufferTooSmall(void)
{
	return DISP_E_BUFFERTOOSMALL;
}

STDMETHODIMP CSimpleErrors::genDispDivByZero(void)
{
	return DISP_E_DIVBYZERO;
}

STDMETHODIMP CSimpleErrors::genDispException(void)
{
	return DISP_E_EXCEPTION;
}

STDMETHODIMP CSimpleErrors::genDispMemberNotFound(void)
{
	return DISP_E_MEMBERNOTFOUND;
}

STDMETHODIMP CSimpleErrors::genDispNoNamedArgs(void)
{
	return DISP_E_NONAMEDARGS;
}

STDMETHODIMP CSimpleErrors::genDispNotACollection(void)
{
	return DISP_E_NOTACOLLECTION;
}

STDMETHODIMP CSimpleErrors::genDispOverflow(void)
{
	return DISP_E_OVERFLOW;
}

STDMETHODIMP CSimpleErrors::genDispParamNotFound(void)
{
	return DISP_E_PARAMNOTFOUND;
}

STDMETHODIMP CSimpleErrors::genDispParamNotOptional(void)
{
	return DISP_E_PARAMNOTOPTIONAL;
}

STDMETHODIMP CSimpleErrors::genDispTypeMismatch(void)
{
	return DISP_E_TYPEMISMATCH;
}

STDMETHODIMP CSimpleErrors::genDispUnknownInterface(void)
{
	return DISP_E_UNKNOWNINTERFACE ;
}

STDMETHODIMP CSimpleErrors::genDispUnknownLCID(void)
{
	return DISP_E_UNKNOWNLCID;
}

STDMETHODIMP CSimpleErrors::genDispUnknownName(void)
{
	return DISP_E_UNKNOWNNAME;
}
