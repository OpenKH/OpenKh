#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <cstdio>
#include <cstdlib>

#include "OpenKH.h"
#include "KingdomApi.h"
#include "Panacea.h"

#define PFN_DEFINE(NAME) PFN_##NAME pfn_##NAME

const long BaseAddress = 0xC00;
HINSTANCE g_hInstance;
PFN_DEFINE(Axa_CFileMan_LoadFile);
PFN_DEFINE(Axa_CFileMan_GetFileSize);
PFN_DEFINE(Axa_AxaResourceMan_SetResourceItem);
PFN_DEFINE(Axa_PackageMan_GetFileInfo);

static const long KingdomApi_KH2[KingdomApiFunction_END] =
{
    0x1367D0,
    0x1364D0,
    0x139630,
    0x136A10,
};

static const long* KingdomApiOffsets[(int)OpenKH::GameId::END]
{
    nullptr,
    KingdomApi_KH2,
    nullptr,
    nullptr,
    nullptr,
};

template <typename T>
void Hook(T& pfn, long address)
{
    if (!address)
        return;

    pfn = (T)((long long)g_hInstance + BaseAddress + address);
}

void Hook(const long kingdomApiOffsets[])
{
    if (kingdomApiOffsets == nullptr)
    {
        fprintf(stderr, "The running game is not yet supported.\n");
        return;
    }

    Hook(pfn_Axa_CFileMan_LoadFile, KingdomApi_KH2[Axa_CFileMan_LoadFile]);
    Hook(pfn_Axa_CFileMan_GetFileSize, KingdomApi_KH2[Axa_CFileMan_GetFileSize]);
    Hook(pfn_Axa_AxaResourceMan_SetResourceItem, KingdomApi_KH2[Axa_AxaResourceMan_SetResourceItem]);
    Hook(pfn_Axa_PackageMan_GetFileInfo, KingdomApi_KH2[Axa_PackageMan_GetFileInfo]);
}

void OpenKH::Initialize()
{
    g_hInstance = GetModuleHandle(NULL);
    fprintf(stdout, "Executable instance at %p\n", g_hInstance);

    auto gameId = DetectGame();
    if (gameId == OpenKH::GameId::Unknown)
    {
        fprintf(stderr, "Unable to detect the running game. OpenKH Panacea will now terminate.\n");
        return;
    }

    Hook(KingdomApiOffsets[(int)gameId]);
    Panacea::Initialize();

    CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)OpenKH::Main, NULL, 0, NULL);
}

void OpenKH::Main()
{
    fprintf(stdout, "Welcome to OpenKH Panacea!\n");
    
    return;
}

OpenKH::GameId OpenKH::DetectGame()
{
    if (strcmp((const char*)g_hInstance + 0x2BD2090, "dummy_string") == 0)
        return GameId::KingdomHearts2;

    return GameId::Unknown;
}

long OpenKH::LoadFile(const char* filename, void* addr) {
    const char BasePath[] = "C:/hd28/EPIC/juefigs/KH2ReSource/";
    char buffer[0x80];
    strcpy(buffer, BasePath);
    strcpy(buffer + strlen(BasePath), filename);

    return Axa::CFileMan::LoadFile(nullptr, buffer, addr, false);
}
