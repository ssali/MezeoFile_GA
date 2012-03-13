// MezeoFileOverlay.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "Guids.h"
#include "ShellIconOverlayExt.h"
#include "ShellExtClassFactory.h"

#ifdef _MANAGED
#pragma managed(push, off)
#endif

volatile LONG  g_lObjRef = 0;              ///< reference count of this DLL.

extern "C" BOOL APIENTRY DllMain( HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved )
{
    return TRUE;
}

STDAPI DllCanUnloadNow(void)
{
    return (g_lObjRef == 0 ? S_OK : S_FALSE);
}

STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID *ppvOut)
{
    if(ppvOut == 0)
        return E_POINTER;
    *ppvOut = NULL;

    _IconID iconid = IconNotFound;
    if (IsEqualIID(rclsid, CLSID_MezeoFile_COMPLETE))
	{
        iconid = IconComplete;
	}
    else if (IsEqualIID(rclsid, CLSID_MezeoFile_ERROR))
	{
        iconid = IconError;
	}
    else if (IsEqualIID(rclsid, CLSID_MezeoFile_PROCESSES))
	{
        iconid = IconProcesses;
	}
    else if (IsEqualIID(rclsid, CLSID_MezeoFile_USER))
	{
        iconid = IconUser;
	}

    if (iconid != IconNotFound)
    {
        CShellExtClassFactory *pCallFact = new (std::nothrow) CShellExtClassFactory(iconid);
        if (pCallFact == NULL)
            return E_OUTOFMEMORY;

        const HRESULT hr = pCallFact->QueryInterface(riid, ppvOut);
        if (FAILED(hr))
            delete pCallFact;

        return hr;
    }

    return CLASS_E_CLASSNOTAVAILABLE;
}

#ifdef _MANAGED
#pragma managed(pop)
#endif

