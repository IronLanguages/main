

/* this ALWAYS GENERATED file contains the IIDs and CLSIDs */

/* link this file in with the server and any clients */


 /* File created by MIDL compiler version 7.00.0500 */
/* at Fri Jan 29 09:30:21 2010
 */
/* Compiler settings for .\COMTest.idl:
    Oicf, W1, Zp8, env=Win32 (32b run)
    protocol : dce , ms_ext, c_ext, robust
    error checks: stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
//@@MIDL_FILE_HEADING(  )

#pragma warning( disable: 4049 )  /* more than 64k source lines */


#ifdef __cplusplus
extern "C"{
#endif 


#include <rpc.h>
#include <rpcndr.h>

#ifdef _MIDL_USE_GUIDDEF_

#ifndef INITGUID
#define INITGUID
#include <guiddef.h>
#undef INITGUID
#else
#include <guiddef.h>
#endif

#define MIDL_DEFINE_GUID(type,name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8) \
        DEFINE_GUID(name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8)

#else // !_MIDL_USE_GUIDDEF_

#ifndef __IID_DEFINED__
#define __IID_DEFINED__

typedef struct _IID
{
    unsigned long x;
    unsigned short s1;
    unsigned short s2;
    unsigned char  c[8];
} IID;

#endif // __IID_DEFINED__

#ifndef CLSID_DEFINED
#define CLSID_DEFINED
typedef IID CLSID;
#endif // CLSID_DEFINED

#define MIDL_DEFINE_GUID(type,name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8) \
        const type name = {l,w1,w2,{b1,b2,b3,b4,b5,b6,b7,b8}}

#endif !_MIDL_USE_GUIDDEF_

MIDL_DEFINE_GUID(IID, IID_ISimpleComObject,0xC819EB8A,0x97B9,0x4DF4,0x95,0x57,0xFB,0xEE,0x6B,0xD8,0x8E,0xF0);


MIDL_DEFINE_GUID(IID, LIBID_COMTestLib,0xA5619E05,0xE89F,0x4336,0xA7,0x8A,0xBE,0xEA,0xEB,0xD2,0xB5,0x56);


MIDL_DEFINE_GUID(IID, DIID__ISimpleComObjectEvents,0xD293233D,0xAB87,0x464B,0xB4,0x38,0x28,0xAB,0x8C,0xAB,0xAC,0x03);


MIDL_DEFINE_GUID(CLSID, CLSID_SimpleComObject,0x78A03F3F,0xC7D0,0x43A4,0xB2,0x1E,0xC8,0x2C,0xE9,0xBF,0x32,0x67);

#undef MIDL_DEFINE_GUID

#ifdef __cplusplus
}
#endif



