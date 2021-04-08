#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include "KingdomApi.h"
#include "Panacea.h"
#include <cstdio>
#include <cstdlib>

HINSTANCE g_hInstance;
PFN_Axa_CFileMan_LoadFile pfn_Axa_CFileMan_LoadFile;
PFN_Axa_CFileMan_GetFileSize pfn_Axa_CFileMan_GetFileSize;
PFN_Axa_AxaResourceMan_SetResourceItem pfn_Axa_AxaResourceMan_SetResourceItem;
PFN_Axa_PackageMan_GetFileInfo pfn_Axa_PackageMan_GetFileInfo;

template <typename T>
void Hook(T& pfn, long address) {
    pfn = (T)((long long)g_hInstance + BaseAddress + address);
}

void Main();

void Hook() {
    g_hInstance = GetModuleHandle(NULL);
    printf("Executable instance at %p\n", g_hInstance);

    Hook(pfn_Axa_CFileMan_LoadFile, Axa_CFileMan_LoadFile);
    Hook(pfn_Axa_CFileMan_GetFileSize, Axa_CFileMan_GetFileSize);
    Hook(pfn_Axa_AxaResourceMan_SetResourceItem, Axa_AxaResourceMan_SetResourceItem);
    Hook(pfn_Axa_PackageMan_GetFileInfo, Axa_PackageMan_GetFileInfo);
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

long Axa::CFileMan::GetFileSize(CFileMan* _this, const char* filename, int mode) {
    return pfn_Axa_CFileMan_GetFileSize(_this, filename, mode);
}

long Axa::AxaResourceMan::SetResourceItem(const char* filename) {
    return pfn_Axa_AxaResourceMan_SetResourceItem(filename);
}

Axa::PackageMan::Unk* Axa::PackageMan::GetFileInfo(const char* filename, int mode) {
    return pfn_Axa_PackageMan_GetFileInfo(filename, mode);
}
