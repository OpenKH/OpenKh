#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <cstdio>
#include <cassert>

FILE* fConsole = nullptr;
void Hook();

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
        fConsole = freopen("CONOUT$", "w", stdout);
#endif
        Hook();
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
#ifdef _DEBUG
        fclose(fConsole);
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
    strcpy(buffer + initialLength, OriginalDllName);
    printf(buffer);

    auto hModule = LoadLibraryA(buffer);
    assert(hModule != nullptr);

    auto proc = (PFN_DirectInput8Create)GetProcAddress(hModule, "DirectInput8Create");
    assert(hModule != nullptr);

    return proc(hinst, dwVersion, riidltf, ppvOut, punkOuter);
}
