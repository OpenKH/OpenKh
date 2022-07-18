#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <cstdio>
#include <cstdlib>
#include <intrin.h>
#include <Shlwapi.h>

#include "OpenKH.h"
#include "KingdomApi.h"
#include "Panacea.h"
#include "EOSOverrider.h"

const long BaseAddress = 0xC00;
HINSTANCE g_hInstance;

static const long KingdomApi_KH2[KingdomApiFunction_END] =
{
    0x136890,
    0x136590,
    0x1396F0,
    0x136AD0,
    0x136A00,
    0x141B40,
    0x136600,
    0x136640,
    0x137860,
    0x1020B0,
    0x136690,
    0x136F30,
    0x135E10,
    0x13EB60,
};

static const long KingdomApi_BBS[KingdomApiFunction_END] =
{
    0,
    0x4BB810,
    0x524CA0,
    0x4BBD50,
};

static const long KingdomApi_BBS[KingdomApiFunction_END] =
{
    0,
    0x4BB810,
    0x524CA0,
    0x4BBD50,
};

static const long* KingdomApiOffsets[(int)OpenKH::GameId::END]
{
    nullptr,
    KingdomApi_KH2,
    nullptr,
    KingdomApi_BBS,
    nullptr,
};

template <typename T>
void Hook(T& pfn, long address)
{
    if (!address)
        return;

    pfn = (T)((long long)g_hInstance + BaseAddress + address);
}

template <typename T>
void GetVarPtr(VarPtr<T>& vp, void* offset)
{
    vp.SetPtr((T*)(*(int*)offset + ((char*)offset + 4)));
}

template <typename T, size_t N>
void GetArrPtr(ArrayPtr<T,N>& vp, void* offset)
{
    vp.SetPtr((T*)(*(int*)offset + ((char*)offset + 4)));
}

void Hook(const long kingdomApiOffsets[])
{
    if (kingdomApiOffsets == nullptr)
    {
        fprintf(stderr, "The running game is not yet supported.\n");
        return;
    }

    Hook(pfn_Axa_CFileMan_LoadFile, KingdomApi_KH2[Axa_CFileMan_LoadFile]);
    Hook(pfn_Axa_CFileMan_LoadFileWithMalloc, KingdomApi_KH2[Axa_CFileMan_LoadFileWithMalloc]);
    Hook(pfn_Axa_CFileMan_GetFileSize, KingdomApi_KH2[Axa_CFileMan_GetFileSize]);
    Hook(pfn_Axa_AxaResourceMan_SetResourceItem, KingdomApi_KH2[Axa_AxaResourceMan_SetResourceItem]);
    Hook(pfn_Axa_PackageMan_GetFileInfo, KingdomApi_KH2[Axa_PackageMan_GetFileInfo]);
    Hook(pfn_Axa_CalcHash, KingdomApi_KH2[Axa_CalcHash]);
    Hook(pfn_Axa_SetReplacePath, KingdomApi_KH2[Axa_SetReplacePath]);
    Hook(pfn_Axa_FreeAllPackages, KingdomApi_KH2[Axa_FreeAllPackages]);
    Hook(pfn_Axa_CFileMan_GetRemasteredCount, KingdomApi_KH2[Axa_CFileMan_GetRemasteredCount]);
    Hook(pfn_Axa_CFileMan_GetRemasteredEntry, KingdomApi_KH2[Axa_CFileMan_GetRemasteredEntry]);
    Hook(pfn_Axa_PackageFile_GetRemasteredAsset, KingdomApi_KH2[Axa_PackageFile_GetRemasteredAsset]);
    Hook(pfn_Axa_CFileMan_GetAudioStream, KingdomApi_KH2[Axa_CFileMan_GetAudioStream]);
    Hook(pfn_Axa_OpenFile, KingdomApi_KH2[Axa_OpenFile]);
    Hook(pfn_Axa_DebugPrint, KingdomApi_KH2[Axa_DebugPrint]);
    GetVarPtr(PackageFileCount, (char*)pfn_Axa_PackageMan_GetFileInfo + 0x1A);
    GetVarPtr(LastOpenedPackage, (char*)pfn_Axa_CFileMan_GetRemasteredCount + 3);
    GetArrPtr(PackageFiles, (char*)pfn_Axa_PackageMan_GetFileInfo + 0xB1);
    GetArrPtr(BasePath, (char*)pfn_Axa_AxaResourceMan_SetResourceItem + 0x3E);
}

void OpenKH::Initialize()
{
    g_hInstance = GetModuleHandle(NULL);

    fprintf(stdout, "Overriding Epic Games Online Service\n");
    EOSOverride(g_hInstance);

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
    const char* DetectedFmt = "%s detected.\n";

    // We should just return unknown if the launcher is running

    wchar_t buffer[MAX_PATH]; // MAX_PATH default macro
    GetModuleFileName(NULL, buffer, MAX_PATH);

    if (_wcsicmp(PathFindFileName(buffer), L"KINGDOM HEARTS HD 1.5+2.5 Launcher.exe") == 0)
        return GameId::Unknown;

    if (strcmp((const char*)g_hInstance + 0x2BD2090, "dummy_string") == 0)
        {
            fprintf(stdout, DetectedFmt, "Kingdom Hearts II");
            return GameId::KingdomHearts2;
        }
        if (strcmp((const char*)g_hInstance + 0x11165090, "dummy_string") == 0)
        {
            fprintf(stdout, DetectedFmt, "Kingdom Hearts Birth By Sleep");
            Hook(pfn_Bbs_File_load, Bbs_File_load);
            Hook(pfn_Bbs_CRsrcData_loadCallback, Bbs_CRsrcData_loadCallback);
            return GameId::KingdomHeartsBbs;
        }

    return GameId::Unknown;
}

long OpenKH::LoadFile(const char* filename, void* addr) {
    const char BasePath[] = "C:/hd28/EPIC/juefigs/KH2ReSource/";
    char buffer[0x80];
    strcpy(buffer, BasePath);
    strcpy(buffer + strlen(BasePath), filename);

    return Axa::CFileMan::LoadFile(nullptr, buffer, addr, false);
}
