// dllmain.h : Declaration of module class.

class CDlrComLibraryModule : public CAtlDllModuleT< CDlrComLibraryModule >
{
public :
	DECLARE_LIBID(LIBID_DlrComLibraryLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_DLRCOMLIBRARY, "{5BCFCCFB-4A71-4788-81FB-7CECBD0EB159}")
};

extern class CDlrComLibraryModule _AtlModule;
