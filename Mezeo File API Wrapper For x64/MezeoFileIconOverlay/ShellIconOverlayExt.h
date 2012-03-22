#pragma once
#include <vector>
#include <Shlobj.h>

#include "SQLite3DB.h"

extern volatile LONG g_lObjRef;    

enum _IconID
{
    IconComplete = 0,
    IconError = 1,
    IconProcesses = 2,
    IconUser = 3,
	IconNotFound = 4
};

class CShellIconOverlayExt : public IShellIconOverlayIdentifier
{
	protected:
		_IconID m_iconID;
		ULONG   m_cRef;

	private:
		void LoadHandlers(LPWSTR pwszIconFile, int cchMax, int *pIndex, DWORD *pdwFlags);
		void LoadRealLibrary(LPCTSTR ModuleName, LPCTSTR classIdString, LPWSTR pwszIconFile, int cchMax, int *pIndex, DWORD *pdwFlags);

	public:
		CShellIconOverlayExt(_IconID iconid);
		virtual ~CShellIconOverlayExt(void);
		
		//IUnknown members
		STDMETHODIMP QueryInterface(REFIID, LPVOID FAR *);
		STDMETHODIMP_(ULONG) AddRef();
		STDMETHODIMP_(ULONG) Release();

		//IShellIconOverlayIdentifier methods
		STDMETHODIMP GetOverlayInfo(LPWSTR pwszIconFile, int cchMax, int *pIndex, DWORD *pdwFlags);
		STDMETHODIMP GetPriority(int *pPriority);
		STDMETHODIMP IsMemberOf(LPCWSTR pwszPath, DWORD dwAttrib);

};