#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <cstdio>
#include "Panacea.h"
#include <cstdlib>
#include <cassert>

template <class TFunc>
class Hook
{
    const unsigned char RETN = 0xC3;
    const unsigned char NOP = 0x90;
    const unsigned char PushRax = 0x50;
    const unsigned char PushRcx = 0x51;
    const unsigned char PushRdx = 0x52;
    const unsigned char PushRbx = 0x53;
    const unsigned char PushRsp = 0x54;
    const unsigned char PushRbp = 0x55;
    const unsigned char PushRsi = 0x56;
    const unsigned char PushRdi = 0x57;
    const unsigned char PopRax = 0x58;
    const unsigned char PopRcx = 0x59;
    const unsigned char PopRdx = 0x5A;
    const unsigned char PopRbx = 0x5B;
    const unsigned char PopRsp = 0x5C;
    const unsigned char PopRbp = 0x5D;
    const unsigned char PopRsi = 0x5E;
    const unsigned char PopRdi = 0x5F;
    const unsigned char PushR8_1 = 0x41;
    const unsigned char PushR8_2 = 0x50;
    const unsigned char PopR8_1 = 0x41;
    const unsigned char PopR8_2 = 0x58;
    const unsigned char PushR9_1 = 0x41;
    const unsigned char PushR9_2 = 0x51;
    const unsigned char PopR9_1 = 0x41;
    const unsigned char PopR9_2 = 0x59;

    TFunc m_pOriginalFunc;
    TFunc m_pReplaceFunc;
    void* m_pMiddleFunc;
    void* m_pPatchLoadFile;
    void* m_pUnpatchLoadFile;
    DWORD m_PreviousProtectionValue;
    int m_patchLenLoadFile;
    int m_middleLenLoadFile;

public:
    void Patch()
    {
        memcpy(m_pOriginalFunc, m_pPatchLoadFile, m_patchLenLoadFile);
    }

    TFunc Unpatch()
    {
        memcpy(m_pOriginalFunc, m_pUnpatchLoadFile, m_patchLenLoadFile);
        return m_pOriginalFunc;
    }

    Hook(TFunc& originalFunc, TFunc& replaceFunc, const char* nameFunc) :
        m_pOriginalFunc(originalFunc),
        m_pReplaceFunc(replaceFunc)
    {
        unsigned char Function[]
        {
            PushRbp,
            PushRsi,
            PushRdi,
            PushRcx,
            PushRdx,
            PushR9_1, PushR9_2,
            PushR8_1, PushR8_2,
            // sub rsp, 30h
            0x48, 0x83, 0xEC, 0x30,
            // call m_pReplaceFunc
            0xFF, 0x15, 0x02, 0x00, 0x00, 0x00, 0xEB, 0x08,
            (unsigned char)((long long)m_pReplaceFunc >> 0),
            (unsigned char)((long long)m_pReplaceFunc >> 8),
            (unsigned char)((long long)m_pReplaceFunc >> 16),
            (unsigned char)((long long)m_pReplaceFunc >> 24),
            (unsigned char)((long long)m_pReplaceFunc >> 32),
            (unsigned char)((long long)m_pReplaceFunc >> 40),
            (unsigned char)((long long)m_pReplaceFunc >> 48),
            (unsigned char)((long long)m_pReplaceFunc >> 56),
            // add     rsp, 30h
            0x48, 0x83, 0xC4, 0x30,
            PopR8_1, PopR8_2,
            PopR9_1, PopR9_2,
            PopRdx,
            PopRcx,
            PopRdi,
            PopRsi,
            PopRbp,
            RETN,
        };

        fputs(nameFunc, stdout);

        m_middleLenLoadFile = sizeof(Function);
        void* m_pMiddleFunc = VirtualAlloc(nullptr, m_middleLenLoadFile, MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
        assert(m_pMiddleFunc != 0);
        memcpy(m_pMiddleFunc, Function, m_middleLenLoadFile);
        printf(" hooked to %p ", m_pMiddleFunc);

        unsigned char Patch[]
        {
            // jmp functionPtr
            0xFF, 0x25, 0x00, 0x00, 0x00, 0x00,
            (unsigned char)(((long long)m_pMiddleFunc >> 0)),
            (unsigned char)(((long long)m_pMiddleFunc >> 8)),
            (unsigned char)(((long long)m_pMiddleFunc >> 16)),
            (unsigned char)(((long long)m_pMiddleFunc >> 24)),
            (unsigned char)(((long long)m_pMiddleFunc >> 32)),
            (unsigned char)(((long long)m_pMiddleFunc >> 40)),
            (unsigned char)(((long long)m_pMiddleFunc >> 48)),
            (unsigned char)(((long long)m_pMiddleFunc >> 56)),
        };

        VirtualProtect(originalFunc, sizeof(Patch), PAGE_EXECUTE_READWRITE, &m_PreviousProtectionValue);

        m_patchLenLoadFile = sizeof(Patch);
        m_pPatchLoadFile = malloc(m_patchLenLoadFile);
        assert(m_pPatchLoadFile != 0);
        memcpy(m_pPatchLoadFile, Patch, m_patchLenLoadFile);

        m_pUnpatchLoadFile = malloc(m_patchLenLoadFile);
        assert(m_pUnpatchLoadFile != 0);
        memcpy(m_pUnpatchLoadFile, originalFunc, m_patchLenLoadFile);

        memcpy(m_pOriginalFunc, m_pPatchLoadFile, m_patchLenLoadFile);
        fputs("successfully!\n", stdout);
    }

    ~Hook()
    {
        Unpatch();
        free(m_pPatchLoadFile);
        free(m_pUnpatchLoadFile);
        VirtualFree(m_pMiddleFunc, 0, MEM_RELEASE);
        VirtualProtect(m_pOriginalFunc, m_patchLenLoadFile, m_PreviousProtectionValue, &m_PreviousProtectionValue);
    }
};

template <typename TFunc>
Hook<TFunc>* NewHook(TFunc originalFunc, TFunc replacementFunc, const char* functionName)
{
    // this function circumnavigates a very weird compilation error I could not find a fix for.
    return new Hook<TFunc>(originalFunc, replacementFunc, functionName);
}

Hook<PFN_Axa_CFileMan_LoadFile>* Hook_Axa_CFileMan_LoadFile;
Hook<PFN_Axa_CFileMan_GetFileSize>* Hook_Axa_CFileMan_GetFileSize;

void Panacea::Initialize()
{
    Hook_Axa_CFileMan_LoadFile = NewHook(pfn_Axa_CFileMan_LoadFile, Panacea::LoadFile, "Axa::CFileMan::LoadFile");
    Hook_Axa_CFileMan_GetFileSize = NewHook(pfn_Axa_CFileMan_GetFileSize, Panacea::GetFileSize, "Axa::CFileMan::GetFileSize");
}

bool Panacea::TransformFilePath(char* strOutPath, int maxLength, const char* originalPath)
{
    const char BaseOriginalPath[] = "C:/hd28/EPIC/juefigs/KH2ReSource/";
    const char ModFolderPath[] = "E:\\openkh_mods\\mod";

    const char* actualFileName = originalPath + sizeof(BaseOriginalPath) - 1;
    sprintf_s(strOutPath, maxLength, "%s\\%s", ModFolderPath, actualFileName);

    return GetFileAttributesA(strOutPath) != INVALID_FILE_ATTRIBUTES;
}

long __cdecl Panacea::LoadFile(Axa::CFileMan* _this, const char* filename, void* addr, bool useHdAsset)
{
    char path[MAX_PATH];
    if (!TransformFilePath(path, sizeof(path), filename))
    {
        printf("%s\n", filename);
        auto ret = Hook_Axa_CFileMan_LoadFile->Unpatch()(_this, filename, addr, useHdAsset);
        Hook_Axa_CFileMan_LoadFile->Patch();
        return ret;
    }

    printf("%s\n", path);
    FILE* file = fopen(path, "rb");
    fseek(file, 0, SEEK_END);
    auto length = ftell(file);
    
    fseek(file, 0, SEEK_SET);
    fread(addr, length, 1, file);
    fclose(file);

    return length;
}

long __cdecl Panacea::GetFileSize(Axa::CFileMan* _this, const char* filename, int mode)
{
    const int FileNotFound = 0;
    char path[MAX_PATH];

    if (!TransformFilePath(path, sizeof(path), filename))
    {
        auto fileinfo = Axa::PackageMan::GetFileInfo(filename, 0);
        if (fileinfo)
            return fileinfo->FileSize;

        WIN32_FIND_DATAA findData;
        HANDLE handle = FindFirstFileA(filename, &findData);
        if (handle != INVALID_HANDLE_VALUE)
        {
            CloseHandle(handle);
            return findData.nFileSizeLow;
        }

        return FileNotFound;
    }

    FILE* file = fopen(path, "rb");
    fseek(file, 0, SEEK_END);
    auto length = ftell(file);
    fclose(file);

    return length;
}
