#define WIN32_LEAN_AND_MEAN
#define GetFileVersionInfoW GetFileVersionInfoW_
#define GetFileVersionInfoSizeW GetFileVersionInfoSizeW_
#define VerQueryValueW VerQueryValueW_
#include <windows.h>
#include <cstdio>
#include <cassert>
#undef GetFileVersionInfoW
#undef GetFileVersionInfoSizeW
#undef VerQueryValueW
#define MiniDumpWriteDump MiniDumpWriteDump_
#include <DbgHelp.h>
#undef MiniDumpWriteDump
#include <Shlwapi.h>
#include "OpenKH.h"

typedef BOOL(WINAPI* PFN_MiniDumpWriteDump)(HANDLE hProcess, DWORD ProcessId, HANDLE hFile, MINIDUMP_TYPE DumpType, PMINIDUMP_EXCEPTION_INFORMATION ExceptionParam, PMINIDUMP_USER_STREAM_INFORMATION UserStreamParam, PMINIDUMP_CALLBACK_INFORMATION CallbackParam);
PFN_MiniDumpWriteDump MiniDumpWriteDumpPtr;


// Version.dll pointers
typedef BOOL(WINAPI* PFN_GetFileVersionInfoW)(LPCWSTR lptstrFilename, DWORD dwHandle, DWORD dwLen, LPVOID lpData);
PFN_GetFileVersionInfoW GetFileVersionInfoWPtr;

typedef BOOL(WINAPI* PFN_GetFileVersionInfoSizeW)(LPCWSTR lptstrFilename, LPDWORD lpdwHandle);
PFN_GetFileVersionInfoSizeW GetFileVersionInfoSizeWPtr;

typedef BOOL(WINAPI* PFN_VerQueryValueW)(LPCVOID pBlock, LPCWSTR lpSubBlock, LPVOID* lplpBuffer, PUINT puLen);
PFN_VerQueryValueW VerQueryValueWPtr;

void HookDbgHelp()
{
    if (PathFileExists(L"LuaBackend.dll"))
        LoadLibrary(L"LuaBackend.dll");
    const char OriginalDllName[] = "\\DBGHELP.dll";
    char buffer[MAX_PATH];

    auto initialLength = GetSystemDirectoryA(buffer, sizeof(buffer) - sizeof(OriginalDllName) - 1);
    assert(initialLength > 0);

    strcpy(buffer + initialLength, OriginalDllName);

    auto hModule = LoadLibraryA(buffer);
    assert(hModule != nullptr);

    MiniDumpWriteDumpPtr = (PFN_MiniDumpWriteDump)GetProcAddress(hModule, "MiniDumpWriteDump");
    assert(hModule != nullptr);
}

void HookVersion()
{
    if (PathFileExists(L"LuaBackend.dll"))
        LoadLibrary(L"LuaBackend.dll");
    const char OriginalDllName[] = "\\VERSION.dll";
    char buffer[MAX_PATH];

    auto initialLength = GetSystemDirectoryA(buffer, sizeof(buffer) - sizeof(OriginalDllName) - 1);
    assert(initialLength > 0);

    strcpy(buffer + initialLength, OriginalDllName);

    auto hModule = LoadLibraryA(buffer);
    assert(hModule != nullptr);

    GetFileVersionInfoWPtr = (PFN_GetFileVersionInfoW)GetProcAddress(hModule, "GetFileVersionInfoW");
    GetFileVersionInfoSizeWPtr = (PFN_GetFileVersionInfoSizeW)GetProcAddress(hModule, "GetFileVersionInfoSizeW");
    VerQueryValueWPtr = (PFN_VerQueryValueW)GetProcAddress(hModule, "VerQueryValueW");
    assert(hModule != nullptr);
}

BOOL APIENTRY DllMain(
    HMODULE hModule,
    DWORD ul_reason_for_call,
    LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        HookVersion();
        HookDbgHelp();
        OpenKH::Initialize();
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

extern "C" __declspec(dllexport) BOOL WINAPI MiniDumpWriteDump(HANDLE hProcess, DWORD ProcessId, HANDLE hFile, MINIDUMP_TYPE DumpType, PMINIDUMP_EXCEPTION_INFORMATION ExceptionParam, PMINIDUMP_USER_STREAM_INFORMATION UserStreamParam, PMINIDUMP_CALLBACK_INFORMATION CallbackParam)
{
    if (!MiniDumpWriteDumpPtr) HookDbgHelp();
    return MiniDumpWriteDumpPtr(hProcess, ProcessId, hFile, DumpType, ExceptionParam, UserStreamParam, CallbackParam);
}

extern "C" __declspec(dllexport) BOOL WINAPI GetFileVersionInfoW(LPCWSTR lptstrFilename, DWORD dwHandle, DWORD dwLen, LPVOID lpData) {
    if (!GetFileVersionInfoWPtr) HookVersion();
    return GetFileVersionInfoWPtr(lptstrFilename, dwHandle, dwLen, lpData);
}

extern "C" __declspec(dllexport) DWORD WINAPI GetFileVersionInfoSizeW(LPCWSTR lptstrFilename, LPDWORD lpdwHandle) {
    if (!GetFileVersionInfoSizeWPtr) HookVersion();
    return GetFileVersionInfoSizeWPtr(lptstrFilename, lpdwHandle);
}

extern "C" __declspec(dllexport) BOOL WINAPI VerQueryValueW(LPCVOID pBlock, LPCWSTR lpSubBlock, LPVOID * lplpBuffer, PUINT puLen) {
    if (!VerQueryValueWPtr) HookVersion();
    return VerQueryValueWPtr(pBlock, lpSubBlock, lplpBuffer, puLen);
}
