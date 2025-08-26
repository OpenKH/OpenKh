#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <cstdio>
#include <cstdlib>
#include <cassert>
#include <Shlwapi.h>
#include <string>
#include <vector>
#include <unordered_map>
#include <map>
#include <algorithm>
#include <queue>
#include <fcntl.h>
#include "Panacea.h"
#include "OpenKH.h"
#include <cstdarg>
#include "bass.h"
#include "bass_vgmstream.h"
#include <set>

template <class TFunc>
class Hook
{
    TFunc m_pOriginalFunc;
    TFunc m_pReplaceFunc;
    void* m_pPatchLoadFile;
    void* m_pUnpatchLoadFile;
    int m_patchLenLoadFile;

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
        if (OpenKH::m_DebugLog)
        {
            fputs(nameFunc, stdout);

            printf(" hooked to %p ", m_pReplaceFunc);
        }

        unsigned char Patch[]
        {
            // jmp functionPtr
            0xFF, 0x25, 0x00, 0x00, 0x00, 0x00,
            (unsigned char)(((long long)m_pReplaceFunc >> 0)),
            (unsigned char)(((long long)m_pReplaceFunc >> 8)),
            (unsigned char)(((long long)m_pReplaceFunc >> 16)),
            (unsigned char)(((long long)m_pReplaceFunc >> 24)),
            (unsigned char)(((long long)m_pReplaceFunc >> 32)),
            (unsigned char)(((long long)m_pReplaceFunc >> 40)),
            (unsigned char)(((long long)m_pReplaceFunc >> 48)),
            (unsigned char)(((long long)m_pReplaceFunc >> 56)),
        };

        m_patchLenLoadFile = sizeof(Patch);
        m_pPatchLoadFile = malloc(m_patchLenLoadFile);
        assert(m_pPatchLoadFile != 0);
        memcpy(m_pPatchLoadFile, Patch, m_patchLenLoadFile);

        m_pUnpatchLoadFile = malloc(m_patchLenLoadFile);
        assert(m_pUnpatchLoadFile != 0);
        memcpy(m_pUnpatchLoadFile, originalFunc, m_patchLenLoadFile);

        memcpy(m_pOriginalFunc, m_pPatchLoadFile, m_patchLenLoadFile);
        if (OpenKH::m_DebugLog)
            fputs("successfully!\n", stdout);
    }

    ~Hook()
    {
        Unpatch();
        free(m_pPatchLoadFile);
        free(m_pUnpatchLoadFile);
    }
};

template <typename TFunc>
Hook<TFunc>* NewHook(TFunc originalFunc, TFunc replacementFunc, const char* functionName)
{
    if (originalFunc == nullptr)
        return nullptr;

    return new Hook<TFunc>(originalFunc, replacementFunc, functionName);
}

struct ModFileInfo
{
    Axa::HedEntry Header;
    Axa::PkgEntry FileInfo;
};

std::wstring CombinePaths(const wchar_t* left, const wchar_t* right)
{
    wchar_t finalPath[MAX_PATH];
    return PathCombineW(finalPath, left, right);
}
std::wstring CombinePaths(const std::wstring& left, const wchar_t* right)
{
    return CombinePaths(left.c_str(), right);
}
std::wstring CombinePaths(const std::wstring& left, const std::wstring& right)
{
    return CombinePaths(left.c_str(), right.c_str());
}

bool DirectoryExists(const wchar_t* path)
{
    auto attr = GetFileAttributesW(path);
    return attr != INVALID_FILE_ATTRIBUTES && (attr & FILE_ATTRIBUTE_DIRECTORY);
}

class FakePackageFile : public Axa::PackageFile
{
public:
    FakePackageFile()
    {
        strcpy(PkgFileName, "ModFiles");
        if (!OpenKH::m_ExtractPath.empty())
            ScanFolder(OpenKH::m_ExtractPath);
        ScanFolder(OpenKH::m_ModPath);
        if (!OpenKH::m_DevPath.empty())
            ScanFolder(OpenKH::m_DevPath);
        std::wstring rawfol = OpenKH::m_ModPath + L"\\raw";
        ScanFolder(rawfol);
    }

    bool OpenFile(const char* filePath, const char* altBasePath)
    {
        const char* fn;
        if (altBasePath)
            fn = filePath + strlen(altBasePath) + 1;
        else
            fn = filePath + strlen(BasePath) + 1;
        auto result = fileData.find(fn);
        if (result != fileData.end())
        {
            strcpy_s(CurrentFileName, filePath);
            HeaderData = &result->second.Header;
            CurrentFileData = result->second.FileInfo;
            FileDataCopy = result->second.FileInfo;
            return true;
        }
        memset(CurrentFileName, 0, sizeof(CurrentFileName));
        HeaderData = nullptr;
        memset(&CurrentFileData, 0, sizeof(CurrentFileData));
        memset(&FileDataCopy, 0, sizeof(FileDataCopy));
        return false;
    }

    void OtherFunc() {}

private:
    void ScanFolder(std::wstring& basepath)
    {
        std::queue<std::wstring> folq;
        folq.push(basepath);
        while (!folq.empty())
        {
            auto& curDir = folq.front();
            WIN32_FIND_DATAW data;
            auto path = CombinePaths(curDir, L"*");
            HANDLE findHandle = FindFirstFileW(path.c_str(), &data);
            if (findHandle != INVALID_HANDLE_VALUE)
            {
                do
                {
                    if (!wcscmp(data.cFileName, L".") || !wcscmp(data.cFileName, L".."))
                        continue;
                    if (data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
                    {
                        folq.push(CombinePaths(curDir, data.cFileName));
                        continue;
                    }
                    std::wstring filename = CombinePaths(curDir, data.cFileName);
                    std::string basefn;
                    basefn.resize(WideCharToMultiByte(CP_ACP, 0, &filename[basepath.length() + 1], filename.length() - basepath.length() - 1, nullptr, 0, nullptr, nullptr));
                    WideCharToMultiByte(CP_ACP, 0, &filename[basepath.length() + 1], filename.length() - basepath.length() - 1, &basefn.front(), basefn.length(), nullptr, nullptr);
                    std::transform(basefn.begin(), basefn.end(), basefn.begin(), [](char c)
                        {
                            if (c == '\\')
                                return '/';
                            return c;
                        });
                    ModFileInfo fileInfo{};
                    Axa::CalcHash(basefn.c_str(), (int)basefn.size(), &fileInfo.Header.hash);
                    fileInfo.Header.actualLength = data.nFileSizeLow;
                    fileInfo.Header.dataLength = data.nFileSizeLow;
                    fileInfo.FileInfo.creationDate = data.ftCreationTime.dwLowDateTime;
                    fileInfo.FileInfo.decompressedSize = data.nFileSizeLow;
                    fileInfo.FileInfo.compressedSize = -2;
                    fileData[basefn] = fileInfo;
                } while (FindNextFileW(findHandle, &data));
                FindClose(findHandle);
            }
            folq.pop();
        }
    }

    std::map<std::string, ModFileInfo> fileData;
};

struct MyAppVtbl
{
    void* func1;
    void* func2;
    void* onInit;
    void* onSoftReset;
    int (*onFrame)(__int64 a1);
    void* onExit;
    void* onResize;
    void* onFrameForSaveWait;
    void* func3;
    void* func4recom;
    void* func5recom;
};

MyAppVtbl customAppVtbl;
int (*onFrameOrig)(__int64 a1);

Hook<PFN_Axa_SetReplacePath>* Hook_Axa_SetReplacePath;
Hook<PFN_Axa_FreeAllPackages>* Hook_Axa_FreeAllPackages;
Hook<PFN_Axa_CFileMan_LoadFile>* Hook_Axa_CFileMan_LoadFile;
Hook<PFN_Axa_CFileMan_LoadFileWithSize>* Hook_Axa_CFileMan_LoadFileWithSize;
Hook<PFN_Axa_CFileMan_LoadFileWithMalloc>* Hook_Axa_CFileMan_LoadFileWithMalloc;
Hook<PFN_Axa_CFileMan_GetFileSize>* Hook_Axa_CFileMan_GetFileSize;
Hook<PFN_Axa_CFileMan_GetRemasteredCount>* Hook_Axa_CFileMan_GetRemasteredCount;
Hook<PFN_Axa_CFileMan_GetRemasteredEntry>* Hook_Axa_CFileMan_GetRemasteredEntry;
Hook<PFN_Axa_PackageFile_GetRemasteredAsset>* Hook_Axa_PackageFile_GetRemasteredAsset;
Hook<PFN_Axa_PackageFile_OpenFileImpl>* Hook_Axa_PackageFile_OpenFileImpl;
Hook<PFN_VAG_STREAM_play>* Hook_VAG_STREAM_play;
Hook<PFN_VAG_STREAM_fadeOut>* Hook_VAG_STREAM_fadeOut;
Hook<PFN_VAG_STREAM_setVolume>* Hook_VAG_STREAM_setVolume;
Hook<PFN_VAG_STREAM_exit>* Hook_VAG_STREAM_exit;
Hook<PFN_Axa_DebugPrint>* Hook_Axa_DebugPrint;
Hook<PFN_Bbs_File_load>* Hook_Bbs_File_load;
Hook<PFN_Bbs_CRsrcData_loadCallback>* Hook_CRsrcData_loadCallback;
std::vector<void(*)()> framefuncs;
int bassinit;
int basschan;

void LoadDLLs(const std::wstring& folder)
{
    auto dllpath = CombinePaths(folder, L"dll\\");
    
    wchar_t search[MAX_PATH];
    std::wcscpy(search, dllpath.c_str());
    std::wcscat(search, L"*.dll");
    std::vector<void(*)(const wchar_t*)> initfuncs;
    WIN32_FIND_DATAW find;
    HANDLE fh = FindFirstFileW(search, &find);
    if (fh != INVALID_HANDLE_VALUE)
    {
        do
        {
            wchar_t filepath[MAX_PATH];
            std::wcscpy(filepath, dllpath.c_str());
            std::wcscat(filepath, find.cFileName);
            HMODULE dllhandle = LoadLibraryW(filepath);
            if (dllhandle)
            {
                if (OpenKH::m_DebugLog)
                    fprintf(stdout, "Loaded DLL \"%ls\".\n", find.cFileName);
                void (*initfunc)(const wchar_t*) = (void(*)(const wchar_t*))GetProcAddress(dllhandle, "OnInit");
                if (initfunc)
                    initfuncs.push_back(initfunc);
                void (*framefunc)() = (void(*)())GetProcAddress(dllhandle, "OnFrame");
                if (framefunc)
                    framefuncs.push_back(framefunc);
            }
        } while (FindNextFileW(fh, &find));
        FindClose(fh);
    }
    for (auto f : initfuncs)
        f(folder.c_str());
}

void Panacea::Initialize()
{
    ULONG_PTR baseImage = (ULONG_PTR)GetModuleHandle(nullptr);

    PIMAGE_DOS_HEADER dosHeaders = (PIMAGE_DOS_HEADER)baseImage;
    PIMAGE_NT_HEADERS ntHeaders = (PIMAGE_NT_HEADERS)(baseImage + dosHeaders->e_lfanew);

    PIMAGE_SECTION_HEADER section = (PIMAGE_SECTION_HEADER)((intptr_t)&ntHeaders->OptionalHeader + ntHeaders->FileHeader.SizeOfOptionalHeader);
    MEMORY_BASIC_INFORMATION meminf;
    for (int i = 0; i < ntHeaders->FileHeader.NumberOfSections; i++)
    {
        VirtualQuery((const void*)(baseImage + section[i].VirtualAddress), &meminf, sizeof(meminf));
        DWORD oldprot;
        switch (meminf.Protect & 0xFF)
        {
        case PAGE_EXECUTE:
        case PAGE_EXECUTE_READ:
            VirtualProtect(meminf.BaseAddress, meminf.RegionSize, PAGE_EXECUTE_WRITECOPY, &oldprot);
            break;
        case PAGE_NOACCESS:
        case PAGE_READONLY:
            VirtualProtect(meminf.BaseAddress, meminf.RegionSize, PAGE_WRITECOPY, &oldprot);
            break;
        }
    }

    Hook_Axa_SetReplacePath = NewHook(pfn_Axa_SetReplacePath, Panacea::SetReplacePath, "Axa::SetReplacePath");
    Hook_Axa_FreeAllPackages = NewHook(pfn_Axa_FreeAllPackages, Panacea::FreeAllPackages, "Axa::FreeAllPackages");
    Hook_Axa_CFileMan_LoadFile = NewHook(pfn_Axa_CFileMan_LoadFile, Panacea::LoadFile, "Axa::CFileMan::LoadFile");
    Hook_Axa_CFileMan_LoadFileWithSize = NewHook(pfn_Axa_CFileMan_LoadFileWithSize, Panacea::LoadFileWithSize, "Axa::CFileMan::LoadFileWithSize");
    Hook_Axa_CFileMan_LoadFileWithMalloc = NewHook(pfn_Axa_CFileMan_LoadFileWithMalloc, Panacea::LoadFileWithMalloc, "Axa::CFileMan::LoadFileWithMalloc");
    Hook_Axa_CFileMan_GetFileSize = NewHook(pfn_Axa_CFileMan_GetFileSize, Panacea::GetFileSize, "Axa::CFileMan::GetFileSize");
    Hook_Axa_CFileMan_GetRemasteredCount = NewHook(pfn_Axa_CFileMan_GetRemasteredCount, Panacea::GetRemasteredCount, "Axa::CFileMan::GetRemasteredCount");
    Hook_Axa_CFileMan_GetRemasteredEntry = NewHook(pfn_Axa_CFileMan_GetRemasteredEntry, Panacea::GetRemasteredEntry, "Axa::CFileMan::GetRemasteredEntry");
    Hook_Axa_PackageFile_GetRemasteredAsset = NewHook(pfn_Axa_PackageFile_GetRemasteredAsset, Panacea::GetRemasteredAsset, "Axa::PackageFile::GetRemasteredAsset");
    Hook_Axa_PackageFile_OpenFileImpl = NewHook(pfn_Axa_PackageFile_OpenFileImpl, Panacea::OpenFileImpl, "Axa::PackageFile::OpenFileImpl");
    Hook_VAG_STREAM_play = NewHook(pfn_VAG_STREAM_play, Panacea::VAG_STREAM::play, "VAG_STREAM::play");
    Hook_VAG_STREAM_fadeOut = NewHook(pfn_VAG_STREAM_fadeOut, Panacea::VAG_STREAM::fadeOut, "VAG_STREAM::fadeOut");
    Hook_VAG_STREAM_setVolume = NewHook(pfn_VAG_STREAM_setVolume, Panacea::VAG_STREAM::setVolume, "VAG_STREAM::setVolume");
    Hook_VAG_STREAM_exit = NewHook(pfn_VAG_STREAM_exit, Panacea::VAG_STREAM::exit, "VAG_STREAM::exit");
    Hook_Axa_DebugPrint = NewHook(pfn_Axa_DebugPrint, Panacea::DebugPrint, "Axa::DebugPrint");
    //Hook_Bbs_File_load = NewHook(pfn_Bbs_File_load, Panacea::BbsFileLoad, "Bbs::File::Load");
    //Hook_CRsrcData_loadCallback = NewHook(pfn_Bbs_CRsrcData_loadCallback, Panacea::BbsCRsrcDataloadCallback, "Bbs::CRsrcData::loadCallback");

    if (!OpenKH::m_DevPath.empty())
        LoadDLLs(OpenKH::m_DevPath);
    LoadDLLs(OpenKH::m_ModPath);
}

int Panacea::FrameHook(__int64 a1)
{
    for (auto f : framefuncs)
        f();
    return onFrameOrig(a1);
}

int Panacea::SetReplacePath(__int64 a1, const char* a2)
{
    auto app = (MyAppVtbl**)a1;
    customAppVtbl = **app;
    onFrameOrig = customAppVtbl.onFrame;
    customAppVtbl.onFrame = FrameHook;
    *app = &customAppVtbl;
    PackageFiles[PackageFileCount++] = new FakePackageFile();
    auto ret = Hook_Axa_SetReplacePath->Unpatch()(a1, a2);
    Hook_Axa_SetReplacePath->Patch();
    return ret;
}

void Panacea::FreeAllPackages()
{
    delete PackageFiles[--PackageFileCount];
    PackageFiles[PackageFileCount] = nullptr;
    Hook_Axa_FreeAllPackages->Unpatch()();
    Hook_Axa_FreeAllPackages->Patch();
}

bool FileExists(wchar_t* path)
{
    auto attr = GetFileAttributesW(path);
    return attr != INVALID_FILE_ATTRIBUTES && !(attr & FILE_ATTRIBUTE_DIRECTORY);
}

bool Panacea::GetRawFile(wchar_t* strOutPath, int maxLength, const char* originalPath)
{
    const char* actualFileName = originalPath + strlen(BasePath) + 1;
    swprintf_s(strOutPath, maxLength, L"%ls\\raw\\%hs", OpenKH::m_ModPath.c_str(), actualFileName);
    return FileExists(strOutPath);
}

bool Panacea::TransformFilePath(wchar_t* strOutPath, int maxLength, const char* originalPath)
{
    const char* actualFileName = originalPath + strlen(BasePath) + 1;
    if (!OpenKH::m_DevPath.empty())
    {
        swprintf_s(strOutPath, maxLength, L"%ls\\%hs", OpenKH::m_DevPath.c_str(), actualFileName);
        if (FileExists(strOutPath))
            return true;
    }
    swprintf_s(strOutPath, maxLength, L"%ls\\%hs", OpenKH::m_ModPath.c_str(), actualFileName);
    if (FileExists(strOutPath))
        return true;
    if (!OpenKH::m_ExtractPath.empty())
    {
        swprintf_s(strOutPath, maxLength, L"%ls\\%hs", OpenKH::m_ExtractPath.c_str(), actualFileName);
        return FileExists(strOutPath);
    }
    return false;
}

std::unordered_map<std::string, std::vector<Axa::RemasteredEntry>> RemasteredData;

void GetIMDOffsets(void* addr, int baseoff, std::vector<int>& entries)
{
    if (*(int*)addr == 'DGMI')
        entries.push_back(((int*)addr)[2] + baseoff + 0x20000000);
}

void GetIMZOffsets(void* addr, int baseoff, std::vector<int>& entries)
{
    if (*(int*)addr == 'ZGMI')
    {
        int texcnt = ((int*)addr)[3];
        int* off = (int*)addr + 4;
        while (texcnt-- > 0)
        {
            int imdoff = *off;
            GetIMDOffsets((char*)addr + imdoff, baseoff + imdoff, entries);
            off += 2;
        }
    }
}

void GetTM2Offsets(void* addr, int baseoff, std::vector<int>& entries)
{
    if (*(int*)addr == '2MIT')
    {
        int texcnt = ((short*)addr)[3];
        char* off = (char*)addr + 0x10;
        baseoff += 0x10;
        while (texcnt-- > 0)
        {
            int next = *(int*)off;
            entries.push_back(*(short*)(off + 12) + baseoff + 0x20000000);
            off += next;
            baseoff += next;
        }
    }
}

void GetDPDOffsets(void* addr, int baseoff, std::vector<int>& entries)
{
    // Cast the start address to a 32-bit pointer within the file.
    int* off = (int*)addr;
    // Check for the DPD magic number, 0x96, to confirm the file is right.
    if (*off++ != 0x96) {
        return;
    }

    // Skip over the pData section in the DPD header.
    // *off contains the count, so move forward (count + 1) positions.
    off += *off + 1;

    // Get texture count for the DPD from the int immediately after the pData section
    int texcnt = *off++;

    // Struct to store specific texture data.
    struct TexInfo {
        int texoff;
        short TBP0;
        short shX;
        short shY;
        bool unique = false;
    };

    std::vector<TexInfo> textures;

    // First: Get TBP0, shX, shY.
    for (int t = 0; t < texcnt; ++t)
    {
        // Get texture offset relative to file base
        int texoff = *off++;
        // Convert relative offset into actual address in memory
        char* texptr = (char*)addr + texoff;

        // Build the TexInfo struct
        TexInfo tex;
        tex.texoff = texoff;                    // Texture
        tex.TBP0 = *(short*)(texptr + 0x00);    // TBP0
        tex.shX = *(short*)(texptr + 0x08);     // shX
        tex.shY = *(short*)(texptr + 0x0A);     // shY
        textures.push_back(tex);

    }

    // Mark duplicates (TBP0 + shX + shY) as unique, i.e, shouldn't be combined.
    for (size_t i = 0; i < textures.size(); ++i)
    {
        for (size_t j = i + 1; j < textures.size(); ++j)
        {
            if (textures[i].TBP0 == textures[j].TBP0 &&
                textures[i].shX == textures[j].shX &&
                textures[i].shY == textures[j].shY)
            {
                textures[i].unique = true;
                textures[j].unique = true;

            }
        }
    }

    // Sort by TBP0 value.
    std::sort(textures.begin(), textures.end(), [](const TexInfo& a, const TexInfo& b) {
        return a.TBP0 < b.TBP0;
        });

    // For non-unique TBP0 values, combine textures canvases into a "ComboTexture"
    std::map<short, int> sharedCanvasOffsets;

    std::set<short> uniqueTbpSet;
    for (const auto& tex : textures)
    {
        if (tex.unique)
        {
            uniqueTbpSet.insert(tex.TBP0);
        }
    }
    for (auto& tex : textures)
    {
        if (uniqueTbpSet.count(tex.TBP0) > 0)
        {
            tex.unique = true;
        }
    }
    // fix


    for (size_t t = 0; t < textures.size(); ++t)
    {
        const auto& tex = textures[t];
        int offset;

        if (!tex.unique)
        {
            if (sharedCanvasOffsets.find(tex.TBP0) != sharedCanvasOffsets.end())
            {
                // Combine onto same canvas
                offset = sharedCanvasOffsets[tex.TBP0];
                int combinedEntry = offset + baseoff + 0x20000000; //Value of 0x20000000 is some kind of flag HD assets use. If you use 0x80000000, for example, it replicates the glow on PS2 for textures with certain transparency values.
                entries.back() = combinedEntry;

                continue;
            }
            else
            {
                // First texture using this TBP0
                offset = tex.texoff + 0x20;
                sharedCanvasOffsets[tex.TBP0] = offset;

            }
        }
        else
        {
            // Unique texture: never shares offset
            offset = tex.texoff + 0x20;
        }

        int entry = offset + baseoff + 0x20000000;
        entries.push_back(entry);

    }
}

void GetDPXOffsets(void* addr, int baseoff, std::vector<int>& entries)
{
    int* off = (int*)((char*)addr + *(int*)addr * 0x20 + 4);
    int dpdcnt = *off++;
    while (dpdcnt-- > 0)
    {
        int dpdoff = *off++ - 0xC;
        GetDPDOffsets((char*)addr + dpdoff, baseoff + dpdoff, entries);
    }
}

void GetPAXOffsets(void* addr, int baseoff, std::vector<int>& entries)
{
    if (*(int*)addr == '_XAP')
    {
        int dpxoff = ((int*)addr)[3];
        GetDPXOffsets((char*)addr + dpxoff + 12, baseoff + dpxoff + 12, entries);
    }
}

const char TEXA[] = "TEXA";
void GetRAWOffsets(void* addr, void* endaddr, int baseoff, std::vector<int>& entries)
{
    if (*(int*)addr == 0)
    {
        int* off = (int*)addr;
        int TextureInfoCount = off[2];
        int GSInfoCount = off[3];
        int OffsetDataOff = off[4];
        int CLUTTransinfoOff = off[5];
        int GsinfoOff = off[6];
        int dataOffset = off[7];

		//New addition.
        std::vector<int> baseTextureOffsets;
        std::map<int, std::vector<int>> texaMapping; // Stores TEXA textures per TexIndex

		//get all the image data offsets from the CLUT Transfer Info blocks
        int* PicOffsets = new int[TextureInfoCount];
        for (int t = 0; t < TextureInfoCount; t++)
            PicOffsets[t] = *(int*)((char*)addr + (CLUTTransinfoOff + 0x90) + (t * 0x90) + 116);

		//first loop to get number of pixel format 8 textures
		//this is needed to calculate the correct HD link offsets because for some reason
		//Pixel format 4 textures need to be adjusted by Pixel8 image count * 16
        int Modifier = 0;
        for (int m = 0; m < GSInfoCount; m++)
        {
            long long Tex0Reg = *(long long*)((char*)addr + GsinfoOff + 0x70 + (m * 0xA0));
            unsigned int PSM = (unsigned int)(Tex0Reg >> 20) & 0x3fu;
            if (PSM != 20)
                Modifier += 1;
        }
		//second loop to get actual offsets
		//We need to keep track of how many of each type of texture we find
		//to correctly aclculate the the game expects for the HD link offsets.
        int Pxl4Count = 0;
        int Pxl8Count = 0;
        for (int p = 0; p < GSInfoCount; p++)
        {
            int CurrentKey = *((char*)addr + OffsetDataOff + p);

            long long Tex0Reg = *(long long*)((char*)addr + GsinfoOff + 0x70 + (p * 0xA0));
            unsigned int PSM = (unsigned int)(Tex0Reg >> 20) & 0x3fu;

            int FinalOffset = baseoff + PicOffsets[CurrentKey] + 0x20000000;
            if (PSM == 20)
            {
                FinalOffset += Pxl4Count + (Modifier * 0x10);
                Pxl4Count += 1;
            }
            else
            {
                FinalOffset += (Pxl8Count * 0x10);
                Pxl8Count += 1;
            }
            baseTextureOffsets.push_back(FinalOffset);
        }

        delete[] PicOffsets;

        // Scan for TEXA and store them in a map.
        char* texa = std::search((char*)addr, (char*)endaddr, TEXA, TEXA + 4);
        while (texa != endaddr)
        {
            int imageToApplyTo = *(short*)(texa + 0x0A); // TexIndex
            int texaOffset = *(int*)(texa + 0x28);

            int texaFinalOffset = baseoff + (texa - (char*)addr) + texaOffset + 0x08 + (imageToApplyTo * 0x10LL) + 0x20000000;

            texaMapping[imageToApplyTo].push_back(texaFinalOffset);

            texa = std::search(texa + 4, (char*)endaddr, TEXA, TEXA + 4);
        }

        // Insert textures in the correct order with TEXA textures immediately after their corresponding base texture
        for (size_t i = 0; i < baseTextureOffsets.size(); i++)
        {
            entries.push_back(baseTextureOffsets[i]);
            if (texaMapping.find(i) != texaMapping.end())
            {
                for (int texaOffset : texaMapping[i])
                {
                    entries.push_back(texaOffset);
                }
            }
        }
    }
}

void GetBAROffsets(void* addr, int baseoff, std::vector<int>& entries)
{
    if (*(int*)addr == '\1RAB')
    {
        std::vector<int> entriesTIM;
        std::vector<int> entriesPAX;
        std::vector<int> entriesTM2;
        std::vector<int> entriesGeneral;
        std::vector<int> entriesAudio;
        int count = ((int*)addr)[1];
        int* off = (int*)addr + 4;
        while (count-- > 0)
        {
            int datoff = off[2];
            switch (*off)
            {
            case 7:
                GetRAWOffsets((char*)addr + datoff, (char*)addr + datoff + off[3], baseoff + datoff, entriesTIM);
                break;
            case 10:
                GetTM2Offsets((char*)addr + datoff, baseoff + datoff, entriesTM2);
                break;
            case 18:
                GetPAXOffsets((char*)addr + datoff, baseoff + datoff, entriesPAX);
                break;
            case 24:
                GetIMDOffsets((char*)addr + datoff, baseoff + datoff, entriesGeneral);
                break;
            case 29:
                GetIMZOffsets((char*)addr + datoff, baseoff + datoff, entriesGeneral);
                break;
            case 31:
            case 34:
                if (!memcmp((char*)addr + datoff, "ORIGIN", 6))
                    entriesAudio.push_back(-1);
                break;
            case 36:
                entriesGeneral.push_back(baseoff + datoff + 0x20000000);
            break;
            case 46:
                GetBAROffsets((char*)addr + datoff, baseoff + datoff, entriesGeneral);
                break;
            }
            off += 4;
        }
        entries.insert(entries.end(), entriesTIM.begin(), entriesTIM.end());
        entries.insert(entries.end(), entriesPAX.begin(), entriesPAX.end());
        entries.insert(entries.end(), entriesTM2.begin(), entriesTM2.end());
        entries.insert(entries.end(), entriesGeneral.begin(), entriesGeneral.end());
        entries.insert(entries.end(), entriesAudio.begin(), entriesAudio.end());
    }
}

void ScanFolder(const std::wstring& folder, std::vector<Axa::RemasteredEntry>& files)
{
    std::queue<std::wstring> folq;
    folq.push(folder);
    while (!folq.empty())
    {
        auto& curfol = folq.front();
        WIN32_FIND_DATAW data;
        wchar_t path[MAX_PATH];
        swprintf(path, sizeof(path), L"%s\\*", curfol.c_str());
        HANDLE findHandle = FindFirstFileW(path, &data);
        if (findHandle != INVALID_HANDLE_VALUE)
        {
            do
            {
                if (!wcscmp(data.cFileName, L".") || !wcscmp(data.cFileName, L".."))
                    continue;
                if (data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
                {
                    folq.push(curfol + L'\\' + data.cFileName);
                    continue;
                }
                std::wstring filename = curfol + L'/' + data.cFileName;
                std::wstring basefn = filename.substr(folder.length() + 1);
                std::transform(basefn.begin(), basefn.end(), basefn.begin(), [](wchar_t c)
                    {
                        if (c == L'\\')
                            return L'/';
                        return c;
                    });
                Axa::RemasteredEntry fileInfo{};
                WideCharToMultiByte(CP_ACP, 0, &basefn.front(), basefn.length(), fileInfo.name, sizeof(fileInfo.name), nullptr, nullptr);
                fileInfo.decompressedSize = data.nFileSizeLow;
                fileInfo.compressedSize = -2;
                files.push_back(fileInfo);
            } while (FindNextFileW(findHandle, &data));
        }
        folq.pop();
    }
}

bool sortRemasteredFiles(const Axa::RemasteredEntry& a, const Axa::RemasteredEntry& b)
{
    const char* exta = a.name + strlen(a.name) - 4;
    const char* extb = b.name + strlen(b.name) - 4;
    if (!_stricmp(exta, ".dds"))
    {
        if (_stricmp(extb, ".dds"))
            return true;
        const char* fna = a.name;
        if (*fna == '-')
            ++fna;
        const char* fnb = b.name;
        if (*fnb == '-')
            ++fnb;
        if (isdigit(*fna) && isdigit(*fnb))
        {
            int ia = atoi(fna);
            int ib = atoi(fnb);
            return ia < ib;
        }
    }
    else if (!_stricmp(exta, ".png"))
    {
        if (!_stricmp(extb, ".dds"))
            return false;
        if (_stricmp(extb, ".png"))
            return true;
        const char* fna = a.name;
        if (*fna == '-')
            ++fna;
        const char* fnb = b.name;
        if (*fnb == '-')
            ++fnb;
        if (isdigit(*fna) && isdigit(*fnb))
        {
            int ia = atoi(fna);
            int ib = atoi(fnb);
            return ia < ib;
        }
    }
    return false;
}

void ScanRemasteredFolder(const wchar_t* path, void* addr, const wchar_t*  remasteredFolder, std::vector<Axa::RemasteredEntry>& entries)
{
    std::vector<int> assetoffs{};
    const wchar_t* ext = PathFindExtensionW(path);
    switch (OpenKH::m_GameID)
    {
    case OpenKH::GameId::KingdomHearts2:
        if (ext[-2] == L'.' && ext[-1] == L'a')
            ext -= 2;
        if (!_wcsicmp(ext, L".imd"))
            GetIMDOffsets(addr, 0, assetoffs);
        else if (!_wcsicmp(ext, L".imz"))
            GetIMZOffsets(addr, 0, assetoffs);
        else if (!_wcsicmp(ext, L".tm2"))
            GetTM2Offsets(addr, 0, assetoffs);
        else if (!_wcsicmp(ext, L".pax"))
            GetPAXOffsets(addr, 0, assetoffs);
        else if (!_wcsicmp(ext, L".dpd"))
            GetDPDOffsets(addr, 0, assetoffs);
        else if (!_wcsicmp(ext, L".2dd")
            || !_wcsicmp(ext, L".2ld")
            || !_wcsicmp(ext, L".a.fm")
            || !_wcsicmp(ext, L".a.fr")
            || !_wcsicmp(ext, L".a.gr")
            || !_wcsicmp(ext, L".a.it")
            || !_wcsicmp(ext, L".a.sp")
            || !_wcsicmp(ext, L".a.us")
            || !_wcsicmp(ext, L".a.uk")
            || !_wcsicmp(ext, L".a.jp")
            || !_wcsicmp(ext, L".bar")
            || !_wcsicmp(ext, L".bin")
            || !_wcsicmp(ext, L".mag")
            || !_wcsicmp(ext, L".map")
            || !_wcsicmp(ext, L".mdlx"))
            GetBAROffsets(addr, 0, assetoffs);
        else // unknown file type
            for (int i = 0; i < entries.size(); ++i)
                assetoffs.push_back(entries[i].origOffset);
        break;
    default:
        for (int i = 0; i < entries.size(); ++i)
            assetoffs.push_back(entries[i].origOffset);
        break;
    }
    std::vector<Axa::RemasteredEntry> modfiles;
    ScanFolder(remasteredFolder, modfiles);
    if (modfiles.size() >= assetoffs.size())
    {
        std::stable_sort(modfiles.begin(), modfiles.end(), sortRemasteredFiles);
        entries = modfiles;
        entries.resize(assetoffs.size());
        for (int i = 0; i < entries.size(); ++i)
            entries[i].origOffset = assetoffs[i];
    }
    else
    {
        if (entries.size() > assetoffs.size())
            entries.resize(assetoffs.size());
        else
            while (entries.size() < assetoffs.size())
            {
                Axa::RemasteredEntry ent{};
                sprintf_s(ent.name, "-%d.dds", (int)entries.size());
                entries.push_back(ent);
            }
        for (int i = 0; i < entries.size(); ++i)
        {
            entries[i].origOffset = assetoffs[i];
            for (auto& modfile : modfiles)
                if (!strcmp(entries[i].name, modfile.name))
                {
                    entries[i].decompressedSize = modfile.decompressedSize;
                    entries[i].compressedSize = -2;
                    break;
                }
        }
    }
}

void GetRemasteredFiles(Axa::PackageFile* fileinfo, const wchar_t* path, void* addr)
{
    if (!OpenKH::m_EnableCache || RemasteredData.find(fileinfo->CurrentFileName) == RemasteredData.cend())
    {
        wchar_t remasteredFolder[MAX_PATH];
        std::vector<Axa::RemasteredEntry> entries(fileinfo->RemasteredData, fileinfo->RemasteredData + fileinfo->CurrentFileData.remasteredCount);
        swprintf_s(remasteredFolder, L"%ls\\remastered\\%ls", OpenKH::m_DevPath.c_str(), path + OpenKH::m_DevPath.length() + 1);
        if (!OpenKH::m_DevPath.empty() && DirectoryExists(remasteredFolder))
        {
            ScanRemasteredFolder(path, addr, remasteredFolder, entries);
        }
        else
        {
            swprintf_s(remasteredFolder, L"%ls\\remastered\\%ls", OpenKH::m_ModPath.c_str(), path + OpenKH::m_ModPath.length() + 1);
            if (DirectoryExists(remasteredFolder))
            {
                ScanRemasteredFolder(path, addr, remasteredFolder, entries);
            }
            else if (!OpenKH::m_ExtractPath.empty())
            {
                swprintf_s(remasteredFolder, L"%ls\\remastered\\%ls", OpenKH::m_ExtractPath.c_str(), path + OpenKH::m_ExtractPath.length() + 1);
                if (DirectoryExists(remasteredFolder))
                    ScanRemasteredFolder(path, addr, remasteredFolder, entries);
            }
        }
#if 0
        FILE* fh;
        if (!fopen_s(&fh, "remastered.log", "a"))
        {
            fprintf(fh, "%s(\"%s\") found %lld assets:\n", __func__, path, entries.size());
            for (auto& ent : entries)
                fprintf(fh, "\"%s\", 0x%X, 0x%X\n", ent.name, ent.origOffset, ent.decompressedSize);
            fprintf(fh, "\n");
            fclose(fh);
        }
#endif
        RemasteredData.insert_or_assign(fileinfo->CurrentFileName, entries);
    }
}

bool Panacea::OpenFileImpl(Axa::PackageFile* a1, const char* filePath, const char* altBasePath)
{
    auto ret = Hook_Axa_PackageFile_OpenFileImpl->Unpatch()(a1, filePath, altBasePath);
    Hook_Axa_PackageFile_OpenFileImpl->Patch();
    if (ret)
        strcpy_s(a1->CurrentFileName, filePath);
    return ret;
}

long __cdecl Panacea::LoadFile(Axa::CFileMan* _this, const char* filename, void* addr, bool useHdAsset)
{
    wchar_t path[MAX_PATH];
    if (GetRawFile(path, sizeof(path), filename))
    {
        if (OpenKH::m_DebugLog)
            fwprintf(stdout, L"LoadFile(\"%ls\", %d)\n", path, useHdAsset);
        auto fileinfo = Axa::PackageMan::GetFileInfo(filename, 0);
        FILE* file = _wfopen(path, L"rb");
        Axa::PkgEntry pkgent;
        fread(&pkgent, sizeof(pkgent), 1, file);
        if (useHdAsset)
        {
            std::vector<Axa::RemasteredEntry> entries;
            entries.reserve(pkgent.remasteredCount);
            int newoff = sizeof(Axa::PkgEntry) + sizeof(Axa::RemasteredEntry) * pkgent.remasteredCount;
            if (pkgent.compressedSize <= 0)
                newoff += (pkgent.decompressedSize + 0xF) & ~0xF;
            else
                newoff += pkgent.compressedSize;
            Axa::RemasteredEntry ent;
            for (int i = 0; i < pkgent.remasteredCount; i++)
            {
                fread(&ent, sizeof(Axa::RemasteredEntry), 1, file);
                ent.offset = newoff;
                entries.push_back(ent);
                if (ent.compressedSize <= 0)
                    newoff += (ent.decompressedSize + 0xF) & ~0xF;
                else
                    newoff += ent.compressedSize;
            }
            RemasteredData.insert_or_assign(fileinfo->CurrentFileName, entries);
        }
        else
            fseek(file, sizeof(Axa::RemasteredEntry) * pkgent.remasteredCount, SEEK_CUR);
        if (pkgent.decompressedSize > 16)
        {
            if (pkgent.compressedSize <= 0)
            {
                fread(addr, pkgent.decompressedSize, 1, file);
                if (pkgent.compressedSize == -1)
                    Axa::DecryptFile(fileinfo, addr, pkgent.decompressedSize, &pkgent);
            }
            else
            {
                char* cmpbuf = new char[pkgent.compressedSize];
                fread(cmpbuf, pkgent.compressedSize, 1, file);
                Axa::DecryptFile(fileinfo, cmpbuf, pkgent.compressedSize, &pkgent);
                int decSize = pkgent.decompressedSize;
                Axa::DecompressFile(addr, &decSize, cmpbuf, pkgent.compressedSize);
                delete[] cmpbuf;
            }
        }
        else
            fread(addr, pkgent.decompressedSize, 1, file);
        fclose(file);

        if (!pkgent.decompressedSize || !useHdAsset)
            return pkgent.decompressedSize;

        if (Axa::AxaResourceMan::SetResourceItem(filename, pkgent.decompressedSize, addr) != -1)
            return pkgent.decompressedSize;
        return 0;
    }
    else if (!TransformFilePath(path, sizeof(path), filename))
    {
        if (OpenKH::m_DebugLog)
            fprintf(stdout, "LoadFile(\"%s\", %d)\n", filename, useHdAsset);
        auto ret = Hook_Axa_CFileMan_LoadFile->Unpatch()(_this, filename, addr, useHdAsset);
        Hook_Axa_CFileMan_LoadFile->Patch();
        return ret;
    }

    if (OpenKH::m_DebugLog)
        fwprintf(stdout, L"LoadFile(\"%ls\", %d)\n", path, useHdAsset);
    auto fileinfo = Axa::PackageMan::GetFileInfo(filename, 0);
    FILE* file = _wfopen(path, L"rb");
    fseek(file, 0, SEEK_END);
    auto length = ftell(file);

    fseek(file, 0, SEEK_SET);
    fread(addr, length, 1, file);
    fclose(file);

    if (!length || !useHdAsset)
        return length;

    GetRemasteredFiles(fileinfo, path, addr);

    if (Axa::AxaResourceMan::SetResourceItem(filename, length, addr) != -1)
        return length;
    return 0;
}

long __cdecl Panacea::LoadFileWithSize(Axa::CFileMan* _this, const char* filename, void* addr, int size, bool useHdAsset)
{
    wchar_t path[MAX_PATH];
    if (GetRawFile(path, sizeof(path), filename))
    {
        if (OpenKH::m_DebugLog)
            fwprintf(stdout, L"LoadFileWithSize(\"%ls\", %d, %d)\n", path, size, useHdAsset);
        auto fileinfo = Axa::PackageMan::GetFileInfo(filename, 0);
        FILE* file = _wfopen(path, L"rb");
        Axa::PkgEntry pkgent;
        fread(&pkgent, sizeof(pkgent), 1, file);
        if (useHdAsset)
        {
            std::vector<Axa::RemasteredEntry> entries;
            entries.reserve(pkgent.remasteredCount);
            int newoff = sizeof(Axa::PkgEntry) + sizeof(Axa::RemasteredEntry) * pkgent.remasteredCount;
            if (pkgent.compressedSize <= 0)
                newoff += (pkgent.decompressedSize + 0xF) & ~0xF;
            else
                newoff += pkgent.compressedSize;
            Axa::RemasteredEntry ent;
            for (int i = 0; i < pkgent.remasteredCount; i++)
            {
                fread(&ent, sizeof(Axa::RemasteredEntry), 1, file);
                ent.offset = newoff;
                entries.push_back(ent);
                if (ent.compressedSize <= 0)
                    newoff += (ent.decompressedSize + 0xF) & ~0xF;
                else
                    newoff += ent.compressedSize;
            }
            RemasteredData.insert_or_assign(fileinfo->CurrentFileName, entries);
        }
        else
            fseek(file, sizeof(Axa::RemasteredEntry) * pkgent.remasteredCount, SEEK_CUR);

        if (pkgent.decompressedSize > size)
        {
            fclose(file);
            return -6;
        }

        if (pkgent.decompressedSize > 16)
        {
            if (pkgent.compressedSize <= 0)
            {
                fread(addr, pkgent.decompressedSize, 1, file);
                if (pkgent.compressedSize == -1)
                    Axa::DecryptFile(fileinfo, addr, pkgent.decompressedSize, &pkgent);
            }
            else
            {
                char* cmpbuf = new char[pkgent.compressedSize];
                fread(cmpbuf, pkgent.compressedSize, 1, file);
                Axa::DecryptFile(fileinfo, cmpbuf, pkgent.compressedSize, &pkgent);
                int decSize = pkgent.decompressedSize;
                Axa::DecompressFile(addr, &decSize, cmpbuf, pkgent.compressedSize);
                delete[] cmpbuf;
            }
        }
        else
            fread(addr, pkgent.decompressedSize, 1, file);
        fclose(file);

        if (!pkgent.decompressedSize || !useHdAsset)
            return pkgent.decompressedSize;

        if (Axa::AxaResourceMan::SetResourceItem(filename, pkgent.decompressedSize, addr) != -1)
            return pkgent.decompressedSize;
        return 0;
    }
    else if (!TransformFilePath(path, sizeof(path), filename))
    {
        if (OpenKH::m_DebugLog)
            fprintf(stdout, "LoadFileWithSize(\"%s\", %d, %d)\n", filename, size, useHdAsset);
        auto ret = Hook_Axa_CFileMan_LoadFile->Unpatch()(_this, filename, addr, useHdAsset);
        Hook_Axa_CFileMan_LoadFile->Patch();
        return ret;
    }

    if (OpenKH::m_DebugLog)
        fwprintf(stdout, L"LoadFileWithSize(\"%ls\", %d, %d)\n", path, size, useHdAsset);
    auto fileinfo = Axa::PackageMan::GetFileInfo(filename, 0);
    FILE* file = _wfopen(path, L"rb");
    fseek(file, 0, SEEK_END);
    auto length = ftell(file);

    if (length > size)
    {
        fclose(file);
        return -6;
    }

    fseek(file, 0, SEEK_SET);
    fread(addr, length, 1, file);
    fclose(file);

    if (!length || !useHdAsset)
        return length;

    GetRemasteredFiles(fileinfo, path, addr);

    if (Axa::AxaResourceMan::SetResourceItem(filename, length, addr) != -1)
        return length;
    return 0;
}

void* __cdecl Panacea::LoadFileWithMalloc(Axa::CFileMan* _this, const char* filename, int* sizePtr, bool useHdAsset, const char* filename2)
{
    wchar_t path[MAX_PATH];
    if (GetRawFile(path, sizeof(path), filename))
    {
        if (OpenKH::m_DebugLog)
            fwprintf(stdout, L"LoadFileWithMalloc(\"%ls\", %d, \"%hs\")\n", path, useHdAsset, filename2);
        auto fileinfo = Axa::PackageMan::GetFileInfo(filename, filename2);
        if (*sizePtr == -1)
            return nullptr;
        FILE* file = _wfopen(path, L"rb");
        Axa::PkgEntry pkgent;
        fread(&pkgent, sizeof(pkgent), 1, file);
        if (useHdAsset)
        {
            std::vector<Axa::RemasteredEntry> entries;
            entries.reserve(pkgent.remasteredCount);
            int newoff = sizeof(Axa::PkgEntry) + sizeof(Axa::RemasteredEntry) * pkgent.remasteredCount;
            if (pkgent.compressedSize <= 0)
                newoff += (pkgent.decompressedSize + 0xF) & ~0xF;
            else
                newoff += pkgent.compressedSize;
            Axa::RemasteredEntry ent;
            for (int i = 0; i < pkgent.remasteredCount; i++)
            {
                fread(&ent, sizeof(Axa::RemasteredEntry), 1, file);
                ent.offset = newoff;
                entries.push_back(ent);
                if (ent.compressedSize <= 0)
                    newoff += (ent.decompressedSize + 0xF) & ~0xF;
                else
                    newoff += ent.compressedSize;
            }
            RemasteredData.insert_or_assign(fileinfo->CurrentFileName, entries);
        }
        else
            fseek(file, sizeof(Axa::RemasteredEntry) * pkgent.remasteredCount, SEEK_CUR);
        *sizePtr = pkgent.decompressedSize;
        void* addr = _aligned_malloc(*sizePtr, 0x10);
        if (!addr)
        {
            fclose(file);
            return addr;
        }
        if (pkgent.decompressedSize > 16)
        {
            if (pkgent.compressedSize <= 0)
            {
                fread(addr, pkgent.decompressedSize, 1, file);
                if (pkgent.compressedSize == -1)
                    Axa::DecryptFile(fileinfo, addr, pkgent.decompressedSize, &pkgent);
            }
            else
            {
                char* cmpbuf = new char[pkgent.compressedSize];
                fread(cmpbuf, pkgent.compressedSize, 1, file);
                Axa::DecryptFile(fileinfo, cmpbuf, pkgent.compressedSize, &pkgent);
                int decSize;
                Axa::DecompressFile(addr, &decSize, cmpbuf, pkgent.compressedSize);
                delete[] cmpbuf;
            }
        }
        else
            fread(addr, pkgent.decompressedSize, 1, file);
        fclose(file);

        if (!useHdAsset)
            return addr;

        if (Axa::AxaResourceMan::SetResourceItem(filename, pkgent.decompressedSize, addr) != -1)
            return addr;
        return 0;
    }
    else if (!TransformFilePath(path, sizeof(path), filename))
    {
        if (OpenKH::m_DebugLog)
            fprintf(stdout, "LoadFileWithMalloc(\"%s\", %d, \"%s\")\n", filename, useHdAsset, filename2);
        auto ret = Hook_Axa_CFileMan_LoadFileWithMalloc->Unpatch()(_this, filename, sizePtr, useHdAsset, filename2);
        Hook_Axa_CFileMan_LoadFileWithMalloc->Patch();
        return ret;
    }

    if (OpenKH::m_DebugLog)
        fwprintf(stdout, L"LoadFileWithMalloc(\"%ls\", %d, \"%hs\")\n", path, useHdAsset, filename2);
    auto fileinfo = Axa::PackageMan::GetFileInfo(filename, filename2);
    if (*sizePtr == -1)
        return nullptr;
    FILE* file = _wfopen(path, L"rb");
    fseek(file, 0, SEEK_END);
    *sizePtr = ftell(file);
    void* addr = _aligned_malloc(*sizePtr, 0x10);
    if (!addr)
    {
        fclose(file);
        return addr;
    }

    fseek(file, 0, SEEK_SET);
    fread(addr, *sizePtr, 1, file);
    fclose(file);

    if (!useHdAsset)
        return addr;

    GetRemasteredFiles(fileinfo, path, addr);

    if (Axa::AxaResourceMan::SetResourceItem(filename, *sizePtr, addr) != -1)
        return addr;
    return nullptr;
}

long __cdecl Panacea::GetFileSize(Axa::CFileMan* _this, const char* filename)
{
    const int FileNotFound = 0;
    wchar_t path[MAX_PATH];

    auto fileinfo = Axa::PackageMan::GetFileInfo(filename, 0);
    if (GetRawFile(path, sizeof(path), filename))
    {
        FILE* file = _wfopen(path, L"rb");
        Axa::PkgEntry pkgent;
        fread(&pkgent, sizeof(pkgent), 1, file);
        fclose(file);

        if (OpenKH::m_DebugLog)
            fwprintf(stdout, L"GetFileSize(\"%ls\") = %d\n", path, pkgent.decompressedSize);

        return pkgent.decompressedSize;
    }
    else if (!TransformFilePath(path, sizeof(path), filename))
    {
        if (fileinfo)
        {
            if (OpenKH::m_DebugLog)
                fprintf(stdout, "GetFileSize(\"%s\") = %d\n", filename, (long)fileinfo->CurrentFileData.decompressedSize);
            return (long)fileinfo->CurrentFileData.decompressedSize;
        }

        WIN32_FIND_DATAA findData;
        HANDLE handle = FindFirstFileA(filename, &findData);
        if (handle != INVALID_HANDLE_VALUE)
        {
            CloseHandle(handle);
            if (OpenKH::m_DebugLog)
                fprintf(stdout, "GetFileSize(\"%s\") = %d\n", filename, findData.nFileSizeLow);
            return findData.nFileSizeLow;
        }

        return FileNotFound;
    }

    FILE* file = _wfopen(path, L"rb");
    if (file)
    {
        fseek(file, 0, SEEK_END);
        auto length = ftell(file);
        fclose(file);

        if (OpenKH::m_DebugLog)
            fwprintf(stdout, L"GetFileSize(\"%ls\") = %d\n", path, length);
        return length;
    }

    return FileNotFound;
}

__int64 __cdecl Panacea::GetRemasteredCount()
{
    __int64 count;
    auto found = RemasteredData.find(PackageFiles[LastOpenedPackage]->CurrentFileName);
    if (found != RemasteredData.end())
        count = found->second.size();
    else
        count = PackageFiles[LastOpenedPackage]->CurrentFileData.remasteredCount;
    if (OpenKH::m_DebugLog)
        fwprintf(stdout, L"GetRemasteredCount() = %lld\n", count);
    return count;
}

Axa::RemasteredEntry* Panacea::GetRemasteredEntry(Axa::CFileMan* a1, int* origOffsetPtr, int assetNum)
{
    auto found = RemasteredData.find(PackageFiles[LastOpenedPackage]->CurrentFileName);
    if (found != RemasteredData.end())
    {
        if (assetNum >= found->second.size())
            return nullptr;
        *origOffsetPtr = found->second[assetNum].origOffset;
        if (OpenKH::m_DebugLog)
            fprintf(stdout, "GetRemasteredEntry(%d) = \"%s\"\n", assetNum, found->second[assetNum].name);
        return &found->second[assetNum];
    }
    auto ret = Hook_Axa_CFileMan_GetRemasteredEntry->Unpatch()(a1, origOffsetPtr, assetNum);
    Hook_Axa_CFileMan_GetRemasteredEntry->Patch();
    if (OpenKH::m_DebugLog)
        fprintf(stdout, "GetRemasteredEntry(%d) = \"%s\"\n", assetNum, ret->name);
    return ret;
}

void* Panacea::GetRemasteredAsset(Axa::PackageFile* a1, unsigned int* assetSizePtr, int assetNum)
{
    Axa::RemasteredEntry* entry;
    auto found = RemasteredData.find(PackageFiles[LastOpenedPackage]->CurrentFileName);
    if (found != RemasteredData.end())
    {
        if (assetNum >= found->second.size())
            return nullptr;
        entry = &found->second[assetNum];
    }
    else
    {
        if (assetNum >= a1->CurrentFileData.remasteredCount)
            return nullptr;
        entry = &a1->RemasteredData[assetNum];
    }
    wchar_t path[MAX_PATH];
    if (GetRawFile(path, sizeof(path), a1->CurrentFileName))
    {
        if (OpenKH::m_DebugLog)
            fwprintf(stdout, L"GetRemasteredAsset(%d) = \"%ls\"\n", assetNum, path);
        FILE* file = _wfopen(path, L"rb");
        Axa::PkgEntry pkgent;
        fread(&pkgent, sizeof(pkgent), 1, file);
        int datsz = pkgent.compressedSize <= 0 ? pkgent.decompressedSize : pkgent.compressedSize;
        fseek(file, entry->offset, SEEK_SET);
        *assetSizePtr = entry->decompressedSize;
        void* addr = _aligned_malloc(*assetSizePtr, 0x10);
        if (!addr)
        {
            fclose(file);
            return addr;
        }

        if (entry->decompressedSize > 16)
        {
            if (entry->compressedSize <= 0)
            {
                fread(addr, entry->decompressedSize, 1, file);
                if (entry->compressedSize == -1)
                    Axa::DecryptFile(a1, addr, entry->decompressedSize, &pkgent);
            }
            else
            {
                char* cmpbuf = new char[entry->compressedSize];
                fread(cmpbuf, entry->compressedSize, 1, file);
                Axa::DecryptFile(a1, cmpbuf, entry->compressedSize, &pkgent);
                int decSize = entry->decompressedSize;
                Axa::DecompressFile(addr, &decSize, cmpbuf, entry->compressedSize);
                delete[] cmpbuf;
            }
        }
        else
            fread(addr, entry->decompressedSize, 1, file);
        fclose(file);
        return addr;
    }
    TransformFilePath(path, sizeof(path), a1->CurrentFileName);
    wchar_t remastered[MAX_PATH];
    swprintf_s(remastered, L"%ls\\remastered\\%ls\\%hs", OpenKH::m_DevPath.c_str(), path + OpenKH::m_DevPath.length() + 1, entry->name);
    if (OpenKH::m_DevPath.empty() || GetFileAttributesW(remastered) == INVALID_FILE_ATTRIBUTES)
    {
        swprintf_s(remastered, L"%ls\\remastered\\%ls\\%hs", OpenKH::m_ModPath.c_str(), path + OpenKH::m_ModPath.length() + 1, entry->name);
        if (!OpenKH::m_ExtractPath.empty() && GetFileAttributesW(remastered) == INVALID_FILE_ATTRIBUTES)
            swprintf_s(remastered, L"%ls\\remastered\\%ls\\%hs", OpenKH::m_ExtractPath.c_str(), path + OpenKH::m_ExtractPath.length() + 1, entry->name);
    }
    if (GetFileAttributesW(remastered) != INVALID_FILE_ATTRIBUTES)
    {
        if (OpenKH::m_DebugLog)
            fwprintf(stdout, L"GetRemasteredAsset(%d) = \"%ls\"\n", assetNum, remastered);
        FILE* file = _wfopen(remastered, L"rb");
        fseek(file, 0, SEEK_END);
        *assetSizePtr = ftell(file);
        void* addr = _aligned_malloc(*assetSizePtr, 0x10);
        if (!addr)
        {
            fclose(file);
            return addr;
        }

        fseek(file, 0, SEEK_SET);
        fread(addr, *assetSizePtr, 1, file);
        fclose(file);
        return addr;
    }
    if (OpenKH::m_DebugLog)
        fwprintf(stdout, L"GetRemasteredAsset(%d)\n", assetNum);
    auto ret = Hook_Axa_PackageFile_GetRemasteredAsset->Unpatch()(a1, assetSizePtr, assetNum);
    Hook_Axa_PackageFile_GetRemasteredAsset->Patch();
    return ret;
}

float GetMusicVol()
{
    Axa::PCSettings& pcset = PCSettingsPtr;
    return VolumeLevels[pcset.MasterVolume] * VolumeLevels[pcset.MusicVolume];
}

void BassEndSync(HSYNC handle, DWORD channel, DWORD data, void* user)
{
    BASS_ChannelFree(channel);
    basschan = 0;
}

void Panacea::VAG_STREAM::play(const char* fileName, int volume, int fadeVolume, int time)
{
    if (basschan)
        return;
    if (!bassinit)
        bassinit = BASS_Init(-1, 44100, 0, nullptr, nullptr) ? 1 : -1;
    if (bassinit == -1)
    {
        if (OpenKH::m_DebugLog)
            fprintf(stdout, "VAG_STREAM::play(\"%s\", %d, %d, %d)\n", fileName, volume, fadeVolume, time);
        Hook_VAG_STREAM_play->Unpatch()(fileName, volume, fadeVolume, time);
        Hook_VAG_STREAM_play->Patch();
        return;
    }
    char path[MAX_PATH];
    sprintf_s(path, "%ls\\%s", OpenKH::m_DevPath.c_str(), fileName);
    if (OpenKH::m_DevPath.empty() || GetFileAttributesA(path) == INVALID_FILE_ATTRIBUTES)
    {
        sprintf_s(path, "%ls\\%s", OpenKH::m_ModPath.c_str(), fileName);
        if (!OpenKH::m_ExtractPath.empty() && GetFileAttributesA(path) == INVALID_FILE_ATTRIBUTES)
            sprintf_s(path, "%ls\\%s", OpenKH::m_ExtractPath.c_str(), fileName);
    }
    if (GetFileAttributesA(path) == INVALID_FILE_ATTRIBUTES)
    {
        if (OpenKH::m_DebugLog)
            fprintf(stdout, "VAG_STREAM::play(\"%s\", %d, %d, %d)\n", fileName, volume, fadeVolume, time);
        Hook_VAG_STREAM_play->Unpatch()(fileName, volume, fadeVolume, time);
        Hook_VAG_STREAM_play->Patch();
        return;
    }
    basschan = BASS_VGMSTREAM_StreamCreate(path, BASS_SAMPLE_LOOP);
    if (basschan == 0)
        basschan = BASS_StreamCreateFile(false, path, 0, 0, BASS_SAMPLE_LOOP);
    if (basschan != 0)
    {
        // Stream opened!
        BASS_ChannelPlay(basschan, false);
        BASS_ChannelSetAttribute(basschan, BASS_ATTRIB_VOL, GetMusicVol());
    }
}

void Panacea::VAG_STREAM::fadeOut(unsigned int time)
{
    if (basschan)
    {
        BASS_ChannelSlideAttribute(basschan, BASS_ATTRIB_VOL, -1, time);
        BASS_ChannelSetSync(basschan, BASS_SYNC_SLIDE, 0, BassEndSync, nullptr);
    }
    else
    {
        Hook_VAG_STREAM_fadeOut->Unpatch()(time);
        Hook_VAG_STREAM_fadeOut->Patch();
    }
}

void Panacea::VAG_STREAM::setVolume(int volume)
{
    if (basschan)
        BASS_ChannelSetAttribute(basschan, BASS_ATTRIB_VOL, (volume / 16383.0f) * GetMusicVol());
    else
    {
        Hook_VAG_STREAM_setVolume->Unpatch()(volume);
        Hook_VAG_STREAM_setVolume->Patch();
    }
}

void Panacea::VAG_STREAM::exit()
{
    if (basschan)
    {
        if (!BASS_ChannelIsSliding(basschan, BASS_ATTRIB_VOL))
        {
            BASS_ChannelStop(basschan);
            BASS_StreamFree(basschan);
            basschan = 0;
        }
    }
    else
    {
        Hook_VAG_STREAM_exit->Unpatch()();
        Hook_VAG_STREAM_exit->Patch();
    }
}

void Panacea::DebugPrint(const char* format, ...)
{
    if (OpenKH::m_DebugLog && OpenKH::m_SoundDebug)
    {
        va_list args;
        va_start(args, format);
        vfprintf(stdout, format, args);
        va_end(args);
    }
}

size_t __cdecl Panacea::BbsFileLoad(const char* filename, long long a2)
{
    if (OpenKH::m_DebugLog)
        fprintf(stdout, "File::load: %s\n", filename);
    auto ret = Hook_Bbs_File_load->Unpatch()(filename, a2);
    Hook_Bbs_File_load->Patch();

    return ret;
}

void __cdecl Panacea::BbsCRsrcDataloadCallback(unsigned int* pMem, size_t size, unsigned int* pArg, int nOpt)
{
    if (OpenKH::m_DebugLog)
        fprintf(stdout, "CRsrcData::loadCallback(0x%p, %lli, 0x%p, %i)\n", pMem, size, pArg, nOpt);
    Hook_CRsrcData_loadCallback->Unpatch()(pMem, size, pArg, nOpt);
    Hook_CRsrcData_loadCallback->Patch();
}
