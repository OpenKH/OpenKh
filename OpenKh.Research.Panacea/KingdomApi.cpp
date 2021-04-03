#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include "KingdomApi.h"
#include "Panacea.h"
#include <cstdio>
#include <cstdlib>

HINSTANCE g_hInstance;
PFN_Axa_CFileMan_LoadFile pfn_Axa_CFileMan_LoadFile;

template <typename T>
void Hook(T& pfn, long address) {
    pfn = (T)((long long)g_hInstance + BaseAddress + address);
}

void Main();

void Hook() {
    g_hInstance = GetModuleHandle(NULL);
    printf("Executable instance at %p\n", g_hInstance);

    Hook(pfn_Axa_CFileMan_LoadFile, Axa_CFileMan_LoadFile);
    Panacea::Initialize();

    CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)Main, NULL, 0, NULL);
}

void WINAPI Main()
{
    printf("Panacea applied\n");
}

long Axa::CFileMan::LoadFile(CFileMan* _this, const char* filename, void* addr, bool unk) {
    return pfn_Axa_CFileMan_LoadFile(_this, filename, addr, unk);
}
