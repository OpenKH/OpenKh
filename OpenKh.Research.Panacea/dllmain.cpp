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
#include <filesystem>
#include <sstream>

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

void LogTerminate()
{
    std::exception_ptr exptr = std::current_exception();
    if (exptr)
    {
        try
        {
            std::rethrow_exception(exptr);
        }
        catch (const std::exception& ex)
        {
            // NOTE: We can only really rely on the Win32 API in here, the stdlib might be hosed.
            HANDLE stdErr = GetStdHandle(STD_ERROR_HANDLE);
            if (stdErr != NULL && stdErr != INVALID_HANDLE_VALUE)
            {
                DWORD written = 0;
                const char* message = ex.what();
                WriteConsoleA(stdErr, message, strlen(message), &written, NULL);
            }
            if (IsDebuggerPresent())
                DebugBreak();
        }
    }
    std::abort();
}

LONG WINAPI HandleException(struct _EXCEPTION_POINTERS* apExceptionInfo);
BOOL APIENTRY DllMain(
    HMODULE hModule,
    DWORD ul_reason_for_call,
    LPVOID lpReserved)
{

    std::string _dllPath = std::filesystem::current_path().u8string() + "\\dependencies\\";
    std::wstring _dllString(_dllPath.begin(), _dllPath.end());
    SetDllDirectory(_dllString.c_str());

    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        HookVersion();
        HookDbgHelp();
        SetUnhandledExceptionFilter(HandleException);
        std::set_terminate(LogTerminate);
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

static std::wstring getCurrentDate() {
    auto now = std::chrono::system_clock::now();
    std::time_t now_c = std::chrono::system_clock::to_time_t(now);
    std::tm* ptm = std::localtime(&now_c);

    // Format date with '_' separators
    std::wostringstream ss;
    ss << std::setfill(L'0') << std::setw(2) << ptm->tm_mday << L"_"
        << std::setfill(L'0') << std::setw(2) << (ptm->tm_mon + 1) << L"_"
        << (ptm->tm_year + 1900);

    return ss.str();
}

static std::wstring getCurrentTime() {
    auto now = std::chrono::system_clock::now();
    std::time_t now_c = std::chrono::system_clock::to_time_t(now);
    std::tm* ptm = std::localtime(&now_c);

    // Format time with '_' separators
    std::wostringstream ss;
    ss << std::setfill(L'0') << std::setw(2) << ptm->tm_hour << L"_"
        << std::setfill(L'0') << std::setw(2) << ptm->tm_min << L"_"
        << std::setfill(L'0') << std::setw(2) << ptm->tm_sec;

    return ss.str();
}

#pragma comment(lib, "dbghelp.lib") 
#pragma comment(lib, "Psapi.lib")
LONG WINAPI HandleException(struct _EXCEPTION_POINTERS* apExceptionInfo)
{
    std::wstring dateStr = getCurrentDate();
    std::wstring timeStr = getCurrentTime();

    std::wstring curCrashDumpFolder = L"CrashDump\\" + dateStr;

    std::filesystem::create_directories(curCrashDumpFolder);

    std::wstring fileName = timeStr + L"_" + L"Dump.dmp";

    std::wstring crashDumpFile = curCrashDumpFolder + L"\\" + fileName;

    MINIDUMP_EXCEPTION_INFORMATION info = { NULL, NULL, NULL };
    try
    {
        //generate crash dump
        HANDLE hFile = CreateFileW(
            crashDumpFile.c_str(),
            GENERIC_WRITE | GENERIC_READ,
            0,
            NULL,
            CREATE_ALWAYS,
            0,
            NULL
        );

        HANDLE hProcess = GetCurrentProcess();

        if (hFile != NULL)
        {
            info =
            {
             GetCurrentThreadId(),
             apExceptionInfo,
             TRUE
            };

            MiniDumpWriteDump(hProcess, GetCurrentProcessId(),
                hFile, MiniDumpWithIndirectlyReferencedMemory,
                &info, NULL, NULL
            );

            CloseHandle(hFile);

        }
    }
    catch (const std::exception& e)
    {
    }

    return EXCEPTION_EXECUTE_HANDLER;
}
