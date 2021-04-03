#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <string>
#include <map>
#include "EOSOverrider.h"

typedef int EOS_Result;
const EOS_Result EOS_Success = 0;

struct EOS_PlatformInstance { };
struct EOS_PlatformConnection { };
struct EOS_PlatformAuth { };
struct EOS_Achievements { };

EOS_PlatformInstance* EOS_Platform_Create(void* options)
{
    return new EOS_PlatformInstance();
}

EOS_PlatformConnection* EOS_Platform_GetConnectInterface(EOS_PlatformInstance* instance)
{
    return new EOS_PlatformConnection();
}

EOS_PlatformAuth* EOS_Platform_GetAuthInterface(EOS_PlatformInstance* instance)
{
    return new EOS_PlatformAuth();
}

struct EOS_LocalUserId
{
    const char* UserID;
};

struct EOS_LoginCallbackInfo
{
    EOS_Result Result;
    void* Data;
    const EOS_LocalUserId* UserID;
};

const EOS_LocalUserId g_LocalUserID{ "" };

typedef void (*EOS_LoginCallback)(const EOS_LoginCallbackInfo*);
void EOS_Login_FakeCallback(EOS_LoginCallback cb)
{
    EOS_LoginCallbackInfo fake_info
    {
        EOS_Success,
        NULL,
        &g_LocalUserID,
    };
    cb(&fake_info);
}

void EOS_Auth_Login(EOS_PlatformAuth* instance, void* options, void* data, EOS_LoginCallback cb)
{
    CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)EOS_Login_FakeCallback, cb, 0, NULL);
}

void EOS_Connect_Login(EOS_PlatformConnection* instance, void* options, void* data, EOS_LoginCallback cb)
{
    CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)EOS_Login_FakeCallback, cb, 0, NULL);
}

long long EOS_Auth_AddNotifyLoginStatusChanged(
    EOS_PlatformAuth* auth,
    const void* options,
    void* data,
    const void* cb)
{
    return (long long)auth;
}

EOS_Achievements* EOS_Platform_GetAchievementsInterface(EOS_PlatformInstance*)
{
    return new EOS_Achievements();
}

void EOS_Achievements_QueryDefinitions(
    EOS_Achievements* instance,
    const void* options,
    void* data,
    const void* cb)
{ }

EOS_Result EOS_Initialize(void*) { return EOS_Success; }
void EOS_Platform_Tick(EOS_PlatformInstance*) { }
int32_t EOS_Auth_GetLoggedInAccountsCount(EOS_PlatformAuth*) { return 0; }
EOS_Result EOS_Logging_SetCallback(void* cb) { return EOS_Success; }
EOS_Result EOS_Logging_SetLogLevel(int category, int level) { return EOS_Success; }
EOS_Result EOS_Stats_Stat_Release() { return EOS_Success; }
EOS_Result EOS_Platform_GetEcomInterface() { return EOS_Success; }
EOS_Result EOS_Platform_GetStatsInterface() { return EOS_Success; }
EOS_Result EOS_Stats_IngestStat() { return EOS_Success; }
EOS_Result EOS_Stats_QueryStats() { return EOS_Success; }
EOS_Result EOS_Stats_GetStatsCount() { return EOS_Success; }
EOS_Result EOS_Stats_CopyStatByIndex() { return EOS_Success; }
EOS_Result EOS_Achievements_QueryPlayerAchievements() { return EOS_Success; }
EOS_Result EOS_Achievements_UnlockAchievements() { return EOS_Success; }
EOS_Result EOS_Achievements_AddNotifyAchievementsUnlockedV2() { return EOS_Success; }
EOS_Result EOS_Achievements_RemoveNotifyAchievementsUnlocked() { return EOS_Success; }
EOS_Result EOS_Ecom_QueryOwnership() { return EOS_Success; }
EOS_Result EOS_Shutdown() { return EOS_Success; }
EOS_Result EOS_Platform_Release() { return EOS_Success; }
EOS_Result EOS_Auth_Token_Release() { return EOS_Success; }
void EOS_PresenceModification_Release() { }
EOS_Result EOS_Connect_CreateUser() { return EOS_Success; }
EOS_Result EOS_Connect_GetLoggedInUsersCount() { return EOS_Success; }
EOS_Result EOS_Connect_GetLoggedInUserByIndex() { return EOS_Success; }
EOS_Result EOS_Connect_GetLoginStatus() { return EOS_Success; }
EOS_Result EOS_Connect_AddNotifyAuthExpiration() { return EOS_Success; }
EOS_Result EOS_Connect_RemoveNotifyAuthExpiration() { return EOS_Success; }
EOS_Result EOS_Platform_GetUIInterface() { return EOS_Success; }
EOS_Result EOS_Platform_GetPresenceInterface() { return EOS_Success; }
EOS_Result EOS_Auth_Logout() { return EOS_Success; }
EOS_Result EOS_Auth_GetLoggedInAccountByIndex() { return EOS_Success; }
EOS_Result EOS_Auth_GetLoginStatus() { return EOS_Success; }
EOS_Result EOS_Auth_CopyUserAuthToken() { return EOS_Success; }
EOS_Result EOS_Auth_RemoveNotifyLoginStatusChanged() { return EOS_Success; }
EOS_Result EOS_Presence_CreatePresenceModification() { return EOS_Success; }
EOS_Result EOS_Presence_SetPresence() { return EOS_Success; }
EOS_Result EOS_PresenceModification_SetRawRichText() { return EOS_Success; }
EOS_Result EOS_PresenceModification_SetJoinInfo() { return EOS_Success; }
EOS_Result EOS_UI_ShowFriends() { return EOS_Success; }
EOS_Result EOS_UI_AddNotifyDisplaySettingsUpdated() { return EOS_Success; }
EOS_Result EOS_UI_RemoveNotifyDisplaySettingsUpdated() { return EOS_Success; }
EOS_Result EOS_UI_SetDisplayPreference() { return EOS_Success; }

std::map<std::string, void*> g_EOS_Hooks
{
    {"EOS_Stats_Stat_Release", &EOS_Stats_Stat_Release},
    {"EOS_Platform_GetEcomInterface", &EOS_Platform_GetEcomInterface},
    {"EOS_Platform_GetAchievementsInterface", &EOS_Platform_GetAchievementsInterface},
    {"EOS_Platform_GetStatsInterface", &EOS_Platform_GetStatsInterface},
    {"EOS_Stats_IngestStat", &EOS_Stats_IngestStat},
    {"EOS_Stats_QueryStats", &EOS_Stats_QueryStats},
    {"EOS_Stats_GetStatsCount", &EOS_Stats_GetStatsCount},
    {"EOS_Stats_CopyStatByIndex", &EOS_Stats_CopyStatByIndex},
    {"EOS_Achievements_QueryDefinitions", &EOS_Achievements_QueryDefinitions},
    {"EOS_Achievements_QueryPlayerAchievements", &EOS_Achievements_QueryPlayerAchievements},
    {"EOS_Achievements_UnlockAchievements", &EOS_Achievements_UnlockAchievements},
    {"EOS_Achievements_AddNotifyAchievementsUnlockedV2", &EOS_Achievements_AddNotifyAchievementsUnlockedV2},
    {"EOS_Achievements_RemoveNotifyAchievementsUnlocked", &EOS_Achievements_RemoveNotifyAchievementsUnlocked},
    {"EOS_Ecom_QueryOwnership", &EOS_Ecom_QueryOwnership},
    {"EOS_Initialize", &EOS_Initialize},
    {"EOS_Shutdown", &EOS_Shutdown},
    {"EOS_Platform_Create", &EOS_Platform_Create},
    {"EOS_Platform_Release", &EOS_Platform_Release},
    {"EOS_Auth_Token_Release", &EOS_Auth_Token_Release},
    {"EOS_PresenceModification_Release", &EOS_PresenceModification_Release},
    {"EOS_Connect_Login", &EOS_Connect_Login},
    {"EOS_Connect_CreateUser", &EOS_Connect_CreateUser},
    {"EOS_Connect_GetLoggedInUsersCount", &EOS_Connect_GetLoggedInUsersCount},
    {"EOS_Connect_GetLoggedInUserByIndex", &EOS_Connect_GetLoggedInUserByIndex},
    {"EOS_Connect_GetLoginStatus", &EOS_Connect_GetLoginStatus},
    {"EOS_Connect_AddNotifyAuthExpiration", &EOS_Connect_AddNotifyAuthExpiration},
    {"EOS_Connect_RemoveNotifyAuthExpiration", &EOS_Connect_RemoveNotifyAuthExpiration},
    {"EOS_Platform_Tick", &EOS_Platform_Tick},
    {"EOS_Platform_GetAuthInterface", &EOS_Platform_GetAuthInterface},
    {"EOS_Platform_GetConnectInterface", &EOS_Platform_GetConnectInterface},
    {"EOS_Platform_GetUIInterface", &EOS_Platform_GetUIInterface},
    {"EOS_Platform_GetPresenceInterface", &EOS_Platform_GetPresenceInterface},
    {"EOS_Logging_SetCallback", &EOS_Logging_SetCallback},
    {"EOS_Logging_SetLogLevel", &EOS_Logging_SetLogLevel},
    {"EOS_Auth_Login", &EOS_Auth_Login},
    {"EOS_Auth_Logout", &EOS_Auth_Logout},
    {"EOS_Auth_GetLoggedInAccountsCount", &EOS_Auth_GetLoggedInAccountsCount},
    {"EOS_Auth_GetLoggedInAccountByIndex", &EOS_Auth_GetLoggedInAccountByIndex},
    {"EOS_Auth_GetLoginStatus", &EOS_Auth_GetLoginStatus},
    {"EOS_Auth_CopyUserAuthToken", &EOS_Auth_CopyUserAuthToken},
    {"EOS_Auth_AddNotifyLoginStatusChanged", &EOS_Auth_AddNotifyLoginStatusChanged},
    {"EOS_Auth_RemoveNotifyLoginStatusChanged", &EOS_Auth_RemoveNotifyLoginStatusChanged},
    {"EOS_Presence_CreatePresenceModification", &EOS_Presence_CreatePresenceModification},
    {"EOS_Presence_SetPresence", &EOS_Presence_SetPresence},
    {"EOS_PresenceModification_SetRawRichText", &EOS_PresenceModification_SetRawRichText},
    {"EOS_PresenceModification_SetJoinInfo", &EOS_PresenceModification_SetJoinInfo},
    {"EOS_UI_ShowFriends", &EOS_UI_ShowFriends},
    {"EOS_UI_AddNotifyDisplaySettingsUpdated", &EOS_UI_AddNotifyDisplaySettingsUpdated},
    {"EOS_UI_RemoveNotifyDisplaySettingsUpdated", &EOS_UI_RemoveNotifyDisplaySettingsUpdated},
    {"EOS_UI_SetDisplayPreference", &EOS_UI_SetDisplayPreference},
};

bool EOSOverride(HINSTANCE hInstance)
{
    ULONG_PTR baseImage = (ULONG_PTR)hInstance;

    PIMAGE_DOS_HEADER dosHeaders = (PIMAGE_DOS_HEADER)baseImage;
    PIMAGE_NT_HEADERS ntHeaders = (PIMAGE_NT_HEADERS)(baseImage + dosHeaders->e_lfanew);

    IMAGE_DATA_DIRECTORY importsDirectory = ntHeaders->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT];
    HMODULE library = NULL;
    PIMAGE_IMPORT_BY_NAME functionName = NULL;

    for (
        PIMAGE_IMPORT_DESCRIPTOR importDescriptor = (PIMAGE_IMPORT_DESCRIPTOR)(baseImage + importsDirectory.VirtualAddress);
        importDescriptor->Name != NULL;
        importDescriptor++)
    {
        LPCSTR libraryName = baseImage + (LPCSTR)importDescriptor->Name;
        if (strcmp(libraryName, "EOSSDK-Win64-Shipping.dll") != 0)
            continue;

        library = LoadLibraryA(libraryName);
        if (library == NULL)
            return false;

        PIMAGE_THUNK_DATA originalFirstThunk = (PIMAGE_THUNK_DATA)(baseImage + importDescriptor->OriginalFirstThunk);
        PIMAGE_THUNK_DATA firstThunk = (PIMAGE_THUNK_DATA)(baseImage + importDescriptor->FirstThunk);
        while (originalFirstThunk->u1.AddressOfData != NULL)
        {
            functionName = (PIMAGE_IMPORT_BY_NAME)(baseImage + originalFirstThunk->u1.AddressOfData);
            const auto& mapPair = g_EOS_Hooks.find(std::string(functionName->Name));
            if (mapPair != g_EOS_Hooks.end())
            {
                SIZE_T bytesWritten = 0;
                DWORD oldProtect = 0;
                if (VirtualProtect((LPVOID)(&firstThunk->u1.Function), sizeof(ULONG_PTR), PAGE_READWRITE, &oldProtect))
                {
                    DWORD newProtect = 0;
                    firstThunk->u1.Function = (ULONG_PTR)mapPair->second;
                    g_EOS_Hooks.emplace(mapPair->first, mapPair->second);
                    VirtualProtect((LPVOID)(&firstThunk->u1.Function), sizeof(ULONG_PTR), oldProtect, &newProtect);
                }
            }

            originalFirstThunk++;
            firstThunk++;
        }
    }

    return true;
}
