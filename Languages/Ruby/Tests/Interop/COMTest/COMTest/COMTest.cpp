// COMTest.cpp : Implementation of WinMain


#include "stdafx.h"
#include "resource.h"
#include "COMTest_i.h"
#include "dlldatax.h"


class CCOMTestModule : public CAtlExeModuleT< CCOMTestModule >
{
public :
	DECLARE_LIBID(LIBID_COMTestLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_COMTEST, "{153CBBD9-FE5B-442A-903E-AB2C01D29BF8}")
};

CCOMTestModule _AtlModule;



//
extern "C" int WINAPI _tWinMain(HINSTANCE /*hInstance*/, HINSTANCE /*hPrevInstance*/, 
                                LPTSTR /*lpCmdLine*/, int nShowCmd)
{
    return _AtlModule.WinMain(nShowCmd);
}

