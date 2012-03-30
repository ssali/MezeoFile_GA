

#include "StdAfx.h"
#include "ShellIconOverlayExt.h"
#include <initguid.h>
#include "Guids.h"


using namespace std;

CShellIconOverlayExt::CShellIconOverlayExt(_IconID iconid)
{
	m_iconID = iconid;

    m_cRef = 0L;
    InterlockedIncrement(&g_lObjRef);
}

CShellIconOverlayExt::~CShellIconOverlayExt(void)
{
    InterlockedDecrement(&g_lObjRef);
}

STDMETHODIMP CShellIconOverlayExt::QueryInterface(REFIID riid, LPVOID FAR *ppv)
{
    if(ppv == 0)
        return E_POINTER;
    *ppv = NULL;

    if (IsEqualIID(riid, IID_IUnknown))
    {
        *ppv = static_cast<IUnknown*>(this);
    }
    else if (IsEqualIID(riid, IID_IShellIconOverlayIdentifier))
    {
        *ppv = static_cast<IShellIconOverlayIdentifier*>(this);
    }
    else
    {
        return E_NOINTERFACE;
    }

    AddRef();
    return S_OK;
}

STDMETHODIMP_(ULONG) CShellIconOverlayExt::AddRef()
{
    return ++m_cRef;
}

STDMETHODIMP_(ULONG) CShellIconOverlayExt::Release()
{
    if (--m_cRef)
        return m_cRef;

    delete this;
    return 0L;
}

STDMETHODIMP CShellIconOverlayExt::GetOverlayInfo(LPWSTR pwszIconFile, int cchMax, int *pIndex, DWORD *pdwFlags)
{
	if(pwszIconFile == 0)
        return E_POINTER;
    if(pIndex == 0)
        return E_POINTER;
    if(pdwFlags == 0)
        return E_POINTER;
    if(cchMax < 1)
        return E_INVALIDARG;

	*pwszIconFile = 0;
    *pIndex = 0;
    *pdwFlags = 0;

	const TCHAR* pcIconName = 0;

    switch (m_iconID)
    {
        case IconComplete   : pcIconName = L"CompleteIcon"; break;
        case IconError      : pcIconName = L"ErrorIcon"; break;
        case IconProcesses  : pcIconName = L"ProcessesIcon"; break;
        case IconUser       : pcIconName = L"UserIcon"; break;
        default				: return S_FALSE;
    }

	TCHAR csRegVal[MAX_PATH];
    wstring wstrIconPath;
    HKEY hkey = NULL;
	DWORD dwLen = MAX_PATH;

    if( RegOpenKeyEx (HKEY_LOCAL_MACHINE, L"Software\\MezeoFileOverlay", 0, KEY_QUERY_VALUE, &hkey) != ERROR_SUCCESS )
		return S_FALSE;

	if( RegQueryValueEx (hkey, pcIconName, NULL, NULL, (LPBYTE)csRegVal, &dwLen) == ERROR_SUCCESS )
		wstrIconPath.assign (csRegVal, dwLen);
        
	RegCloseKey(hkey);

    if (wstrIconPath.empty())
        return S_FALSE;

    if (wstrIconPath.size() >= (size_t)cchMax)
        return E_INVALIDARG;

	wcsncpy_s (pwszIconFile, cchMax, wstrIconPath.c_str(), cchMax);
	
    *pIndex = 0;
    *pdwFlags = ISIOI_ICONFILE;
    return S_OK;
}

STDMETHODIMP CShellIconOverlayExt::GetPriority(int *pPriority)
{
    if(pPriority == 0)
        return E_POINTER;
    switch (m_iconID)
    {
        case IconComplete	: *pPriority = 0; break;
        case IconError		: *pPriority = 1; break;
        case IconProcesses	: *pPriority = 2; break;
        case IconUser		: *pPriority = 3; break;
        default				: *pPriority = 100; return S_FALSE;
    }
    return S_OK;
}

STDMETHODIMP CShellIconOverlayExt::IsMemberOf(LPCWSTR pwszPath, DWORD dwAttrib)
{
    if(pwszPath == 0)
        return E_INVALIDARG;

	USES_CONVERSION;
	BOOL bStatus = S_FALSE;
	BOOL bFnd = FALSE;

	try
	{
		TCHAR ctPath[_MAX_PATH];
		if(SHGetFolderPath(NULL, CSIDL_LOCAL_APPDATA, NULL, 0, ctPath))
			return bStatus;

		wcscat(ctPath, _T("\\MezeoFile\\mezeoDb.s3db"));

		CppSQLite3DB m_sqliteDB;
		
		WIN32_FIND_DATA FindFileData;
		HANDLE hFind;
		hFind = FindFirstFile(ctPath, &FindFileData);

		if (hFind == INVALID_HANDLE_VALUE)
		{
			return bStatus;
		}
		FindClose(hFind);
		m_sqliteDB.open(CT2A(ctPath));
		
		memset(ctPath, 0, sizeof(ctPath));
		HKEY hkey = NULL;
		DWORD dwLen = _MAX_PATH;

		if( RegOpenKeyEx (HKEY_CURRENT_USER, L"Software\\MezeoFile\\Basic Info", 0, KEY_QUERY_VALUE, &hkey) != ERROR_SUCCESS )
			return bStatus;
		RegQueryValueEx(hkey, L"Basic4", NULL, NULL, (LPBYTE)ctPath, &dwLen);
		RegCloseKey(hkey);

		ctPath[dwLen] = '/0';

		CString strSearch = pwszPath;
		CString strStatus = L"";
		bFnd = strSearch.Find(ctPath);
		if(!bFnd)
		{
			strSearch = strSearch.Right(wcslen(pwszPath) - (wcslen(ctPath)+1));	
	
			CString strQuery = L"select status from FileStructInfo where key = '" + strSearch + L"';";
		
			CppSQLite3Query q = m_sqliteDB.execQuery(CT2A(strQuery));
	
			if(!q.eof())
			{
				CppSQLite3Binary blob;
				blob.setEncoded((unsigned char*)q.fieldValue("status"));
				q.finalize();
			
				for (int i = 0; i < blob.mnBufferLen; i++)
				{
					strStatus +=  blob.mpBuf[i];
				}

				switch (m_iconID)
				{
					case IconComplete: 
								if(strStatus == "SUCCESS")
									bStatus = S_OK;
								break;
					case IconError:
								if(strStatus == "ISSUE")
									bStatus = S_OK;
								break;
					case IconProcesses:
								if(strStatus == "INPROGRESS")
									bStatus = S_OK;
								break;
				}
			}
		
			wchar_t *pwPath = _wcsdup(pwszPath);
			switch (m_iconID)
			{
				case IconUser:
							if(!wcscmp(pwPath, ctPath))
								bStatus = S_OK;
							break;
			}
		}
		m_sqliteDB.close();
	}
	catch(...)
	{
		return S_FALSE;
	}

	return bStatus;
}
