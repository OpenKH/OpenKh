#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <cstdio>
#include <cassert>
#include "OpenKH.h"

FILE* fConsoleStdout = nullptr;
FILE* fConsoleStderr = nullptr;

BOOL APIENTRY DllMain(
    HMODULE hModule,
    DWORD ul_reason_for_call,
    LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
#ifdef _DEBUG
        AllocConsole();
        fConsoleStdout = freopen("CONOUT$", "w", stdout);
        fConsoleStderr = freopen("CONOUT$", "w", stderr);
#endif
        OpenKH::Initialize();
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
#ifdef _DEBUG
        fclose(fConsoleStderr);
        fclose(fConsoleStdout);
        FreeConsole();
#endif
        break;
    }
    return TRUE;
}

typedef HRESULT(APIENTRY* PFN_DirectInput8Create)(
    HINSTANCE hinst, DWORD dwVersion, const IID* const riidltf, LPVOID* ppvOut, void* punkOuter);
extern "C" __declspec(dllexport) HRESULT APIENTRY DirectInput8Create(
    HINSTANCE hinst, DWORD dwVersion, const IID* const riidltf, LPVOID * ppvOut, void* punkOuter)
{
    const char OriginalDllName[] = "\\System32\\DINPUT8.DLL";
    char buffer[0x100];
    
    auto initialLength = GetWindowsDirectoryA(buffer, sizeof(buffer) - sizeof(OriginalDllName) - 1);
    assert(initialLength > 0);

    strcpy(buffer + initialLength, OriginalDllName);

    auto hModule = LoadLibraryA(buffer);
    assert(hModule != nullptr);

    auto proc = (PFN_DirectInput8Create)GetProcAddress(hModule, "DirectInput8Create");
    assert(hModule != nullptr);

    return proc(hinst, dwVersion, riidltf, ppvOut, punkOuter);
}
