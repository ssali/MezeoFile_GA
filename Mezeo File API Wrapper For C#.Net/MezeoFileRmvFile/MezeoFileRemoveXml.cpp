// MezeoFileRemoveXml.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <atlbase.h>
#include <Shlobj.h>
#include <iostream>
#include <shellapi.h>
#include <Tlhelp32.h>

using namespace std;


int main(int argc, char* argv[])
{
	if(!strcmp(argv[1] , "-cuunmnt"))
	{
		HANDLE hProcessSnap;
		HANDLE hProcess;
		PROCESSENTRY32 pe32;

		hProcessSnap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);  // Takes a snapshot of all the processes

		if(hProcessSnap == INVALID_HANDLE_VALUE)
		{
			return 1;
		}
		pe32.dwSize = sizeof(PROCESSENTRY32);

		if(!Process32First(hProcessSnap, &pe32))
		{
			CloseHandle(hProcessSnap);     
			return 1;
		}

		do
		{
			if(!wcscmp(pe32.szExeFile, L"MezeoFileSync.exe"))
			{    //  checks if process at current position has the name of to be killed app
				hProcess = OpenProcess(PROCESS_TERMINATE,0, pe32.th32ProcessID);  // gets handle to process
				TerminateProcess(hProcess,0);   // Terminate process by handle
				CloseHandle(hProcess);  // close the handle
				break;
			} 
		}while(Process32Next(hProcessSnap,&pe32));  // gets next member of snapshot

		CloseHandle(hProcessSnap);  // closes the snapshot handle
	}
	else if(!strcmp(argv[1] , "-tcsmnt"))
	{
		TCHAR szPath[MAX_PATH];
		wstring wsFolderPath, wsFilePath;

		if(SUCCEEDED(SHGetFolderPath(NULL, CSIDL_LOCAL_APPDATA, NULL, 0, szPath))) 
		{
			wsFolderPath = szPath;
			wsFolderPath.append(L"\\Mezeo File Sync");

			wcscat(szPath, L"\\Mezeo File Sync\\*.*");
			WIN32_FIND_DATA fndData;
			HANDLE hFnd = FindFirstFile(szPath, &fndData);
			USES_CONVERSION;
			if( hFnd!=INVALID_HANDLE_VALUE ) 
			{
				do
				{
					wsFilePath.append(wsFolderPath);
					wsFilePath.append(L"\\");
					wsFilePath.append(fndData.cFileName);
					remove(CT2A(wsFilePath.c_str()));
					wsFilePath.clear();
					
				} while(FindNextFile(hFnd, &fndData));

				RemoveDirectory(wsFolderPath.c_str());
			}
			FindClose(hFnd);
		}
	}
	else if(!strcmp(argv[1] , "-mezrebtsu"))
	{
		ShellExecute(NULL, NULL, L"rbmez.bat", NULL, NULL, SW_HIDE);
	}

	return 0;
}

