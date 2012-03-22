#pragma once

class CShellExtClassFactory:public IClassFactory
{
	protected:
		ULONG m_cRef;
		_IconID m_setIconId;

	public:
		CShellExtClassFactory(_IconID iconid);
		virtual ~CShellExtClassFactory();

		// IUnknown members
		STDMETHODIMP QueryInterface(REFIID, LPVOID FAR *);
		STDMETHODIMP_(ULONG) AddRef();
		STDMETHODIMP_(ULONG) Release();

		// IClassFactory members
		STDMETHODIMP CreateInstance(LPUNKNOWN, REFIID, LPVOID FAR *);
		STDMETHODIMP LockServer(BOOL);
};
