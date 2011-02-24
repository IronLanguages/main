// dllmain.cpp : Implementation of DllMain.

#include "stdafx.h"
#include "resource.h"
#include "DlrComLibrary_i.h"
#include "dllmain.h"
#include "dlrcomserver.h"
#include "paramsinretval.h"
#include "optionalparams.h"
#include "outparams.h"
#include "multipleparams.h"
#include "inoutparams.h"
#include "properties.h"
#include "SimpleErrors.h"
#include "DispEvents.h"
#include "DlrUniversalObj.h"
#include "ReturnValues.h"

CDlrComLibraryModule _AtlModule;

// DLL Entry Point
extern "C" BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
    hInstance;

    if(dwReason == DLL_PROCESS_DETACH)
    {
        if (CDlrComServer::AllDestructed() == FALSE ) {
            ATLASSERT("not all CDlrComServer objects where released" && FALSE);
            exit(1);
        }
		if (CParamsInRetval::AllDestructed() == FALSE){
			ATLASSERT("not all CParamsInRetval objects where released" && FALSE);
			exit(1);
		}
		if (COutParams::AllDestructed() == FALSE){
			ATLASSERT("not all COutParams objects where released" && FALSE);
			exit(1);
		}
		if (COptionalParams::AllDestructed() == FALSE){
			ATLASSERT("not all COptionalParams objects where released" && FALSE);
			exit(1);
		}
		if (CMultipleParams::AllDestructed() == FALSE){
			ATLASSERT("not all CMultipleParams objects where released" && FALSE);
			exit(1);
		}
		if (CInOutParams::AllDestructed() == FALSE){
			ATLASSERT("not all CInOutParams objects where released" && FALSE);
			exit(1);
		}
		if (CProperties::AllDestructed() == FALSE){
			ATLASSERT("not all CProperties objects where released" && FALSE);
			exit(1);
		}
		if (CSimpleErrors::AllDestructed() == FALSE){
			ATLASSERT("not all CProperties objects where released" && FALSE);
			exit(1);
		}
		if (CDispEvents::AllDestructed() == FALSE){
			ATLASSERT("not all CProperties objects where released" && FALSE);
			exit(1);
		}
		if (CDlrUniversalObj::AllDestructed() == FALSE){
			ATLASSERT("not all CProperties objects where released" && FALSE);
			exit(1);
		}
		if (CReturnValues::AllDestructed() == FALSE){
			ATLASSERT("not all CProperties objects where released" && FALSE);
			exit(1);
		}
    }
    return _AtlModule.DllMain(dwReason, lpReserved); 
}
