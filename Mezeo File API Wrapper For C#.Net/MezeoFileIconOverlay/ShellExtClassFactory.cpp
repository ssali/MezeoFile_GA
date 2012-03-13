#include "stdafx.h"
#include "ShellIconOverlayExt.h"
#include "ShellExtClassFactory.h"

CShellExtClassFactory::CShellExtClassFactory(_IconID iconid)
{
    m_setIconId = iconid;

    m_cRef = 0L;

    InterlockedIncrement(&g_lObjRef);
}

CShellExtClassFactory::~CShellExtClassFactory()
{
    InterlockedDecrement(&g_lObjRef);
}

STDMETHODIMP CShellExtClassFactory::QueryInterface(REFIID riid, LPVOID FAR *ppv)
{
    if(ppv == 0)
        return E_POINTER;

    *ppv = NULL;

    if (IsEqualIID(riid, IID_IUnknown) || IsEqualIID(riid, IID_IClassFactory))
    {
        *ppv = static_cast<LPCLASSFACTORY>(this);

        AddRef();

        return S_OK;
    }

    return E_NOINTERFACE;
}

STDMETHODIMP_(ULONG) CShellExtClassFactory::AddRef()
{
    return ++m_cRef;
}

STDMETHODIMP_(ULONG) CShellExtClassFactory::Release()
{
    if (--m_cRef)
        return m_cRef;

    delete this;

    return 0L;
}

STDMETHODIMP CShellExtClassFactory::CreateInstance(LPUNKNOWN pUnkOuter, REFIID riid, LPVOID *ppvObj)
{
    if(ppvObj == 0)
        return E_POINTER;

    *ppvObj = NULL;

    // Shell extensions typically don't support aggregation (inheritance)

    if (pUnkOuter)
        return CLASS_E_NOAGGREGATION;

    CShellIconOverlayExt* pShellIconOverlayExt = new (std::nothrow) CShellIconOverlayExt(m_setIconId); 

    if (NULL == pShellIconOverlayExt)
        return E_OUTOFMEMORY;

    const HRESULT hr = pShellIconOverlayExt->QueryInterface(riid, ppvObj);
    if(FAILED(hr))
        delete pShellIconOverlayExt;

    return hr;
}

STDMETHODIMP CShellExtClassFactory::LockServer(BOOL /*fLock*/)
{
    return E_NOTIMPL;
}
