#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <cstdio>
#include <cstdlib>
#include <intrin.h>
#include <Shlwapi.h>
#include <Psapi.h>
#include <list>

#include "OpenKH.h"
#include "KingdomApi.h"
#include "Panacea.h"

struct FuncInfo
{
    void*& func;
    const char* pattern;
    const char* patvalid;
};

HINSTANCE g_hInstance;
const void* endAddress;
std::list<FuncInfo> funcsToHook;

template <typename T>
void Hook(T& pfn, const char* pattern, const char* patvalid)
{
    FuncInfo func{ (void*&)pfn, pattern, patvalid };
    funcsToHook.push_back(func);
}

void FindAllFuncs()
{
    for (const char* addr = (const char*)g_hInstance; addr < (const char*)endAddress - 0x10; addr += 0x10)
        for (auto iter = funcsToHook.begin(); iter != funcsToHook.end(); ++iter)
        {
            size_t patlen = strlen(iter->patvalid);

            int i = 0;
            for (; i < patlen; i++)
                if (iter->patvalid[i] != '?' && iter->pattern[i] != addr[i])
                    break;
            if (i == patlen)
            {
                iter->func = (void*)addr;
                funcsToHook.erase(iter);
                if (funcsToHook.empty())
                    return;
                break;
            }
        }
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

void Hook()
{
    Hook(pfn_Axa_CFileMan_LoadFile, "\x48\x89\x5C\x24\x00\x48\x89\x6C\x24\x00\x48\x89\x74\x24\x00\x48\x89\x7C\x24\x00\x41\x56\x48\x83\xEC\x20\x4C\x8B\xF2\x41\x8B\xE9\x49\x8B\xCE\x33\xD2\x49\x8B\xF0\xBB\x00\x00\x00\x00\xE8", "xxxx?xxxx?xxxx?xxxx?xxxxxxxxxxxxxxxxxxxxx????x");
    Hook(pfn_Axa_CFileMan_LoadFileWithSize, "\x48\x89\x5C\x24\x00\x48\x89\x6C\x24\x00\x48\x89\x74\x24\x00\x48\x89\x7C\x24\x00\x41\x56\x48\x83\xEC\x20\x4C\x8B\xF2\x41\x8B\xE9\x49\x8B\xCE\x33\xD2\x49\x8B\xF0\x33", "xxxx?xxxx?xxxx?xxxx?xxxxxxxxxxxxxxxxxxxxx");
    Hook(pfn_Axa_CFileMan_LoadFileWithMalloc, "\x48\x89\x5C\x24\x00\x48\x89\x6C\x24\x00\x48\x89\x74\x24\x00\x48\x89\x7C\x24\x00\x41\x56\x48\x83\xEC\x20\x48\x8B\xEA\x45", "xxxx?xxxx?xxxx?xxxx?xxxxxxxxxx");
    Hook(pfn_Axa_CFileMan_GetFileSize, "\x40\x53\x48\x81\xEC\x00\x00\x00\x00\x48\x8B\x05\x00\x00\x00\x00\x48\x33\xC4\x48\x89\x84\x24\x00\x00\x00\x00\x48\x8B\xDA\x33\xD2\x48\x8B\xCB\xE8", "xxxxx????xxx????xxxxxxx????xxxxxxxxx");
    Hook(pfn_Axa_AxaResourceMan_SetResourceItem, "\x48\x89\x5C\x24\x00\x55\x56\x57\x48\x81\xEC\x00\x00\x00\x00\x48\x8B\x05\x00\x00\x00\x00\x48\x33\xC4\x48\x89\x84\x24\x00\x00\x00\x00\x49\x8B\xF0\x8B\xFA\x48\x8B", "xxxx?xxxxxx????xxx????xxxxxxx????xxxxxxx");
    Hook(pfn_Axa_PackageMan_GetFileInfo, "\x40\x53\x55\x56\x48\x83\xEC\x50\x48\x8B\x05\x00\x00\x00\x00\x48\x33\xC4\x48\x89\x44\x24\x00\x44\x8B\x05", "xxxxxxxxxxx????xxxxxxx?xxx");
    Hook(pfn_Axa_CalcHash, "\x40\x53\x56\x57\x48\x81\xEC\x00\x00\x00\x00\x48\x8B\x05\x00\x00\x00\x00\x48\x33\xC4\x48\x89\x84\x24\x00\x00\x00\x00\x8B\xFA\x48\x8B\xD9\x48\x8D\x54\x24", "xxxxxxx????xxx????xxxxxxx????xxxxxxxxx");
    Hook(pfn_Axa_SetReplacePath, "\x4C\x8D\x81\x60\x02\x00\x00\x4C\x8B\xCA\x48\x8D\x15\x00\x00\x00\x00\x48\x8D\x0D", "xxxxxxxxxxxxx????xxx");
    Hook(pfn_Axa_FreeAllPackages, "\x48\x89\x6C\x24\x00\x56\x48\x83\xEC\x20\x8B\x05\x00\x00\x00\x00\x33\xED\x8B\xF5", "xxxx?xxxxxxx????xxxx");
    Hook(pfn_Axa_CFileMan_GetRemasteredCount, "\x48\x63\x05\x00\x00\x00\x00\x48\x8D\x0D\x00\x00\x00\x00\x48\x8B\x04\xC1\x8B\x80", "xxx????xxx????xxxxxx");
    Hook(pfn_Axa_CFileMan_GetRemasteredEntry, "\x48\x63\x05\x00\x00\x00\x00\x4C\x8D\x0D\x00\x00\x00\x00\x4D\x8B\x0C\xC1\x4D\x8B", "xxx????xxx????xxxxxx");
    Hook(pfn_Axa_PackageFile_GetRemasteredAsset, "\x40\x53\x56\x48\x83\xEC\x28\x48\x8B\xD9\x48\x8B\xF2\x48\x8B\x89", "xxxxxxxxxxxxxxxx");
    Hook(pfn_Axa_AxaSoundStream__threadProc, "\x48\x8B\xC4\x57\x48\x83\xEC\x60\x48\xC7\x40\x00\x00\x00\x00\x00\x48\x89\x58\x10\x48\x89\x68\x18\x48\x89\x70\x20\x48\x8B\xD9\x33\xED\x83\xB9", "xxxxxxxxxxx?????xxxxxxxxxxxxxxxxxxx");
    Hook(pfn_Axa_OpenFile, "\x40\x53\x48\x81\xEC\x00\x00\x00\x00\x48\x8B\x05\x00\x00\x00\x00\x48\x33\xC4\x48\x89\x84\x24\x00\x00\x00\x00\x8B\xDA\x48\x8B\xD1\x48\x8D\x4C\x24", "xxxxx????xxx????xxxxxxx????xxxxxxxxx");
    Hook(pfn_Axa_DebugPrint, "\x48\x89\x54\x24\x00\x4C\x89\x44\x24\x00\x4C\x89\x4C\x24\x00\xC3", "xxxx?xxxx?xxxx?x");
    Hook(pfn_Axa_DecryptFile, "\x40\x55\x56\x57\x48\x83\xEC\x50\x48\xC7\x44\x24\x00\x00\x00\x00\x00\x48\x89\x5C\x24\x00\x48\x8B\x05", "xxxxxxxxxxxx?????xxxx?xxx");
    Hook(pfn_Axa_DecompressFile, "\x40\x57\x48\x81\xEC\x00\x00\x00\x00\x8B\x02\x48\x8B\xFA\x89\x44\x24\x38\x48\x8D\x15", "xxxxx????xxxxxxxxxxxx");
    Hook(pfn_VAG_STREAM_play, "\x48\x81\xEC\x00\x00\x00\x00\x48\x8B\x05\x00\x00\x00\x00\x48\x33\xC4\x48\x89\x84\x24\x00\x00\x00\x00\x48\x83\x3D\x00\x00\x00\x00\x00\x75\x35", "xxx????xxx????xxxxxxx????xxx?????xx");
    Hook(pfn_VAG_STREAM_fadeOut, "\x44\x8B\xC1\x48\x8B\x0D\x00\x00\x00\x00\x48\x85\xC9\x74\x08", "xxxxxx????xxxxx");
    Hook(pfn_VAG_STREAM_setVolume, "\x48\x8B\x05\x00\x00\x00\x00\x48\x85\xC0\x74\x1E\x0F", "xxx????xxxxxx");
    Hook(pfn_VAG_STREAM_exit, "\x48\x83\xEC\x28\x48\x8B\x0D\x00\x00\x00\x00\x0F", "xxxxxxx????x");
    char* settingsfunc;
    Hook(settingsfunc, "\x40\x53\x48\x83\xEC\x20\x8B\xD9\x33\xD2\x48\x8D\x0D", "xxxxxxxxxxxxx");
    char* volumefunc;
    Hook(volumefunc, "\x40\x53\x48\x83\xEC\x50\x48\xC7\x44\x24\x00\x00\x00\x00\x00\x48\x63\xD9\x8D\x43\xFF\x83\xF8\x09\x77", "xxxxxxxxxx?????xxxxxxxxxx");
    FindAllFuncs();
    GetVarPtr(PackageFileCount, (char*)pfn_Axa_PackageMan_GetFileInfo + 0x1A);
    GetVarPtr(LastOpenedPackage, (char*)pfn_Axa_CFileMan_GetRemasteredCount + 3);
    GetArrPtr(PackageFiles, (char*)pfn_Axa_PackageMan_GetFileInfo + 0xB1);
    GetArrPtr(BasePath, (char*)pfn_Axa_AxaResourceMan_SetResourceItem + 0x3E);
    GetVarPtr(PCSettingsPtr, settingsfunc + 0x2B);
    GetArrPtr(VolumeLevels, volumefunc + 0x2D);
}

int QuickLaunch = 0;
__int64 (*LaunchGame)(int game);
void QuickBootHook()
{
    LaunchGame(QuickLaunch);
    ExitProcess(QuickLaunch);
}

OpenKH::GameId OpenKH::m_GameID = OpenKH::GameId::Unknown;
std::wstring OpenKH::m_ModPath = L"./mod";
std::wstring OpenKH::m_DevPath = L"";
std::wstring OpenKH::m_ExtractPath = L"";
bool OpenKH::m_ShowConsole = false;
bool OpenKH::m_DebugLog = false;
bool OpenKH::m_EnableCache = true;
bool QuickMenu = false;
const uint8_t quickmenupat[] = { 0xB1, 0x01, 0x90 };
const std::wstring gamefolders[] = {
    L"/kh1",
    L"/kh2",
    L"/recom",
    L"/bbs",
    L"/ddd",
};
void OpenKH::Initialize()
{
    g_hInstance = GetModuleHandle(NULL);
    MODULEINFO moduleInfo;
    GetModuleInformation(GetCurrentProcess(), g_hInstance, &moduleInfo, sizeof(MODULEINFO));
    endAddress = (const char*)g_hInstance + moduleInfo.SizeOfImage;
    ReadSettings("panacea_settings.txt");

    if (m_ShowConsole)
    {
        AllocConsole();
        freopen("CONOUT$", "w", stdout);
        freopen("CONOUT$", "w", stderr);
    }

    fprintf(stdout, "Executable instance at %p\n", g_hInstance);
    m_GameID = DetectGame();
    if (m_GameID == OpenKH::GameId::Unknown)
    {
        fprintf(stderr, "Unable to detect the running game. Panacea will not be executed.\n");
        return;
    }
    if (m_GameID == OpenKH::GameId::Launcher1_5_2_5)
    {
        DWORD pp;
        if (QuickLaunch > 0)
        {
            uint8_t* framefunc;
            Hook(framefunc, "\x40\x57\x48\x83\xEC\x40\x48\xC7\x44\x24\x00\x00\x00\x00\x00\x48\x89\x5C\x24\x00\x48\x8B\xD9\x8B\x41\x34", "xxxxxxxxxx?????xxxx?xxxxxx");
            Hook(LaunchGame, "\x40\x53\x48\x81\xEC\x00\x00\x00\x00\x48\x8B\x05\x00\x00\x00\x00\x48\x33\xC4\x48\x89\x84\x24\x00\x00\x00\x00\x8B\xD9", "xxxxx????xxx????xxxxxxx????xx");
            FindAllFuncs();
            intptr_t m_pReplaceFunc = (intptr_t)QuickBootHook;
            unsigned char Patch[]
            {
                // jmp functionPtr
                0xFF, 0x25, 0x00, 0x00, 0x00, 0x00,
                (unsigned char)(m_pReplaceFunc >> 0),
                (unsigned char)(m_pReplaceFunc >> 8),
                (unsigned char)(m_pReplaceFunc >> 16),
                (unsigned char)(m_pReplaceFunc >> 24),
                (unsigned char)(m_pReplaceFunc >> 32),
                (unsigned char)(m_pReplaceFunc >> 40),
                (unsigned char)(m_pReplaceFunc >> 48),
                (unsigned char)(m_pReplaceFunc >> 56),
            };
            VirtualProtect(framefunc, sizeof(Patch), PAGE_EXECUTE_READWRITE, &pp);
            memcpy(framefunc, Patch, sizeof(Patch));
            VirtualProtect(framefunc, sizeof(Patch), pp, &pp);
            FILE* f = fopen("panacea_settings.txt", "w");
            char buf[MAX_PATH];
            memset(buf, 0, MAX_PATH);
            WideCharToMultiByte(CP_UTF8, 0, &m_ModPath.front(), m_ModPath.size(), buf, MAX_PATH, nullptr, nullptr);
            fprintf(f, "mod_path=%s\n", buf);
            if (!m_ExtractPath.empty())
            {
                memset(buf, 0, MAX_PATH);
                WideCharToMultiByte(CP_UTF8, 0, &m_ExtractPath.front(), m_ExtractPath.size(), buf, MAX_PATH, nullptr, nullptr);
                fprintf(f, "extract_path=%s\n", buf);
            }
            if (!m_DevPath.empty())
            {
                memset(buf, 0, MAX_PATH);
                WideCharToMultiByte(CP_UTF8, 0, &m_DevPath.front(), m_DevPath.size(), buf, MAX_PATH, nullptr, nullptr);
                fprintf(f, "dev_path=%s\n", buf);
            }
            if (m_ShowConsole)
                fputs("show_console=true\n", f);
            if (m_DebugLog)
                fputs("debug_log=true\n", f);
            if (m_EnableCache)
                fputs("enable_cache=true\n", f);
            if (QuickMenu)
                fputs("quick_menu=true\n", f);
            fclose(f);
        }
        else if (QuickMenu)
        {
            uint8_t* axaAppMain;
            Hook(axaAppMain, "\x48\x89\x5C\x24\x00\x57\xB8", "xxxx?xx");
            FindAllFuncs();
            VirtualProtect(axaAppMain + 0x108, sizeof(quickmenupat), PAGE_EXECUTE_READWRITE, &pp);
            memcpy(axaAppMain + 0x108, quickmenupat, sizeof(quickmenupat));
            VirtualProtect(axaAppMain + 0x108, sizeof(quickmenupat), pp, &pp);
        }
        return;
    }

    m_ModPath.append(gamefolders[(int)m_GameID]);
    if (m_DevPath.size() > 0)
        m_DevPath.append(gamefolders[(int)m_GameID]);
    if (m_ExtractPath.size() > 0)
        m_ExtractPath.append(gamefolders[(int)m_GameID]);

    Hook();
    Panacea::Initialize();
    
    fprintf(stdout, "Welcome to OpenKH Panacea!\n");
}

void OpenKH::ReadSettings(const char* filename)
{
    auto parseBool = [](const char* str, bool& value)
    {
        if (!_stricmp(str, "false") ||!strcmp(str, "0"))
            value = false;
        else if (!_stricmp(str, "true") || !strcmp(str, "1"))
            value = true;
    };

    fprintf(stdout, "Reading settings from '%s'\n", filename);
    FILE* f = fopen(filename, "r");
    if (!f)
    {
        fprintf(stderr, "Setting file '%s' not found\n", filename);
        return;
    }

    char buf[1024]; // allows very long path names
    while (fgets(buf, sizeof(buf), f))
    {
        char* separator = strchr(buf, '=');
        if (!separator) continue;

        const char* key = buf;
        char* value = separator + 1;
        *separator = '\0';
        if (!key || !value) continue;
        strtok(value, "\n\r"); // removes new line character

        if (!strncmp(key, "mod_path", sizeof(buf)) && strnlen(value, sizeof(buf)) > 0)
        {
            m_ModPath.resize(MultiByteToWideChar(CP_UTF8, 0, value, strlen(value), nullptr, 0));
            MultiByteToWideChar(CP_UTF8, 0, value, strlen(value), &m_ModPath.front(), m_ModPath.size());
        }
        else if (!strncmp(key, "dev_path", sizeof(buf)) && strnlen(value, sizeof(buf)) > 0)
        {
            m_DevPath.resize(MultiByteToWideChar(CP_UTF8, 0, value, strlen(value), nullptr, 0));
            MultiByteToWideChar(CP_UTF8, 0, value, strlen(value), &m_DevPath.front(), m_DevPath.size());
        }
        else if (!strncmp(key, "extract_path", sizeof(buf)) && strnlen(value, sizeof(buf)) > 0)
        {
            m_ExtractPath.resize(MultiByteToWideChar(CP_UTF8, 0, value, strlen(value), nullptr, 0));
            MultiByteToWideChar(CP_UTF8, 0, value, strlen(value), &m_ExtractPath.front(), m_ExtractPath.size());
        }
        else if (!strncmp(key, "show_console", sizeof(buf)))
            parseBool(value, m_ShowConsole);
        else if (!strncmp(key, "debug_log", sizeof(buf)))
            parseBool(value, m_DebugLog);
        else if (!strncmp(key, "enable_cache", sizeof(buf)))
            parseBool(value, m_EnableCache);
        else if (!strncmp(key, "quick_launch", sizeof(buf)))
        {
            if (!_stricmp(value, "kh1"))
                QuickLaunch = 1;
            else if (!_stricmp(value, "Recom"))
                QuickLaunch = 2;
            else if (!_stricmp(value, "kh2"))
                QuickLaunch = 3;
            else if (!_stricmp(value, "bbs"))
                QuickLaunch = 4;
        }
        else if (!strncmp(key, "quick_menu", sizeof(buf)))
            parseBool(value, QuickMenu);
    }

    fclose(f);
}

OpenKH::GameId OpenKH::DetectGame()
{
    const char* DetectedFmt = "%s detected.\n";
    wchar_t buffer[MAX_PATH]; // MAX_PATH default macro
    GetModuleFileNameW(NULL, buffer, MAX_PATH);

    if (_wcsicmp(PathFindFileNameW(buffer), L"KINGDOM HEARTS FINAL MIX.exe") == 0)
        return GameId::KingdomHearts1;
    if (_wcsicmp(PathFindFileNameW(buffer), L"KINGDOM HEARTS II FINAL MIX.exe") == 0)
        return GameId::KingdomHearts2;
    if (_wcsicmp(PathFindFileNameW(buffer), L"KINGDOM HEARTS Re_Chain of Memories.exe") == 0)
        return GameId::KingdomHeartsReCom;
    if (_wcsicmp(PathFindFileNameW(buffer), L"KINGDOM HEARTS Birth by Sleep FINAL MIX.exe") == 0)
        return GameId::KingdomHeartsBbs;
    if (_wcsicmp(PathFindFileNameW(buffer), L"KINGDOM HEARTS Dream Drop Distance.exe") == 0)
        return GameId::KingdomHeartsDdd;
    if (_wcsicmp(PathFindFileNameW(buffer), L"KINGDOM HEARTS HD 1.5+2.5 Launcher.exe") == 0)
        return GameId::Launcher1_5_2_5;

    return GameId::Unknown;
}

long OpenKH::LoadFile(const char* filename, void* addr) {
    char buffer[0x80];
    strcpy(buffer, BasePath);
    strcpy(buffer + strlen(BasePath), filename);

    return Axa::CFileMan::LoadFile(nullptr, buffer, addr, false);
}
