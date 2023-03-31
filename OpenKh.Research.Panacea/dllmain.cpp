#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <cstdio>
#include <cassert>
#define MiniDumpWriteDump MiniDumpWriteDump_
#include <DbgHelp.h>
#undef MiniDumpWriteDump
#include <Shlwapi.h>
#include "OpenKH.h"

typedef BOOL(WINAPI* PFN_MiniDumpWriteDump)(HANDLE hProcess, DWORD ProcessId, HANDLE hFile, MINIDUMP_TYPE DumpType, PMINIDUMP_EXCEPTION_INFORMATION ExceptionParam, PMINIDUMP_USER_STREAM_INFORMATION UserStreamParam, PMINIDUMP_CALLBACK_INFORMATION CallbackParam);
PFN_MiniDumpWriteDump MiniDumpWriteDumpPtr;
typedef HRESULT(WINAPI* PFN_DirectInput8Create)(HINSTANCE hinst, DWORD dwVersion, LPCVOID riidltf, LPVOID* ppvOut, LPVOID punkOuter);
PFN_DirectInput8Create DirectInput8CreatePtr;


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

void HookDInput8()
{
    if (PathFileExists(L"LuaBackend.dll"))
        LoadLibrary(L"LuaBackend.dll");
    const char OriginalDllName[] = "\\DINPUT8.dll";
    char buffer[MAX_PATH];

    auto initialLength = GetSystemDirectoryA(buffer, sizeof(buffer) - sizeof(OriginalDllName) - 1);
    assert(initialLength > 0);

    strcpy(buffer + initialLength, OriginalDllName);

    auto hModule = LoadLibraryA(buffer);
    assert(hModule != nullptr);

    DirectInput8CreatePtr = (PFN_DirectInput8Create)GetProcAddress(hModule, "DirectInput8Create");
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
        HookDbgHelp();
        HookDInput8();
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


extern "C" __declspec(dllexport) HRESULT WINAPI DirectInput8Create(HINSTANCE hinst, DWORD dwVersion, LPCVOID riidltf, LPVOID * ppvOut, LPVOID punkOuter)
{
    if (!DirectInput8CreatePtr) HookDInput8();
    return DirectInput8CreatePtr(hinst, dwVersion, riidltf, ppvOut, punkOuter);
}
