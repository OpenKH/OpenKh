#pragma once
#include "OpenKH.h"

const int Bbs_File_load = 0xE7D70;
const int Bbs_CRsrcData_loadCallback = 0x113800;

#define PFN_DECLARE(RETURN_TYPE, NAME, ARGS) \
    typedef RETURN_TYPE (__cdecl * PFN_##NAME)ARGS; \
    extern PFN_##NAME pfn_##NAME

namespace Axa
{
    struct HedEntry
    {
        char hash[16];
        __int64 offset;
        int dataLength;
        int actualLength;
    };

    struct PkgEntry
    {
        int decompressedSize;
        int remasteredCount;
        int compressedSize;
        int creationDate;
    };

    struct RemasteredEntry
    {
        char name[32];
        int offset;
        int origOffset;
        int decompressedSize;
        int compressedSize;
    };

    struct PCSettings
    {
        char unk[16];
        short MasterVolume;
        short MusicVolume;
    };

    class PackageFile
    {
    public:
        virtual bool OpenFile(const char* filePath, const char* altBasePath) = 0;
        virtual void OtherFunc() = 0;
        HedEntry* HeaderData = nullptr;
        int PkgFileHandle = 0;
        int FileCount = 0;
        __int64 CurrentOffset = 0;
        char PkgFileName[260]{};
        char byte124 = 0;
        char unk[7]{};
        PkgEntry CurrentFileData{};
        char unk2[4]{};
        RemasteredEntry* RemasteredData = nullptr;
        PkgEntry FileDataCopy{};
        char CurrentFileName[260]{};
        int unk3 = 0;
        void* GetRemasteredAsset(Axa::PackageFile* a1, unsigned int* assetSizePtr, int assetNum);
    };

    class CFileMan
    {
    public:
        static long GetFileSize(CFileMan* _this, const char* filename);
        static long LoadFile(CFileMan* _this, const char* filename, void* addr, bool useHdAsset);
        static long LoadFileWithSize(CFileMan* _this, const char* filename, void* addr, int size, bool useHdAsset);
        static void *LoadFileWithMalloc(CFileMan* _this, const char* filename, int* sizePtr, bool useHdAsset, const char* filename2);
        __int64 GetRemasteredCount();
        Axa::RemasteredEntry* GetRemasteredEntry(CFileMan* a1, int* origOffsetPtr, int assetNum);
    };

    namespace AxaResourceMan
    {
        long SetResourceItem(const char* filename, int size, void* buffer);
    };

    namespace PackageMan
    {
        Axa::PackageFile* GetFileInfo(const char* filename, const char* filename2);
    };

    namespace AxaSoundStream
    {
        __int64 _threadProc(unsigned int* instance);
    }

    __int64 CalcHash(const void* data, int size, void* dst);
    int SetReplacePath(__int64 a1, const char* a2);
    void FreeAllPackages();
    int OpenFile(const char* Format, int OFlag);
#undef DecryptFile
    void DecryptFile(Axa::PackageFile* pkg, void* data, int size, PkgEntry* pkgent);
    __int64 DecompressFile(void* outBuf, int* decSizePtr, void* inBuf, int compSize);
}

namespace VAG_STREAM
{
    void play(const char* fileName, int volume, int fadeVolume, int time);
    void fadeOut(unsigned int time);
    void setVolume(int volume);
    void exit();
}

namespace Bbs
{
    namespace File
    {
        size_t load(const char* pszPath, long long a2);
    }

    class CRsrcData
    {
    public:
        void loadCallback(unsigned int* pMem, size_t size, unsigned int* pArg, int nOpt);
    };
}

PFN_DECLARE(long, Axa_CFileMan_LoadFile, (Axa::CFileMan* _this, const char* filename, void* addr, bool useHdAsset));
PFN_DECLARE(long, Axa_CFileMan_LoadFileWithSize, (Axa::CFileMan* _this, const char* filename, void* addr, int size, bool useHdAsset));
PFN_DECLARE(void*, Axa_CFileMan_LoadFileWithMalloc, (Axa::CFileMan* _this, const char* filename, int* sizePtr, bool useHdAsset, const char* filename2));
PFN_DECLARE(long, Axa_CFileMan_GetFileSize, (Axa::CFileMan* _this, const char* filename));
PFN_DECLARE(long, Axa_AxaResourceMan_SetResourceItem, (const char* filename, int size, void* buffer));
PFN_DECLARE(Axa::PackageFile*, Axa_PackageMan_GetFileInfo, (const char* filename, const char* filename2));
PFN_DECLARE(__int64, Axa_CalcHash, (const void* data, int size, void *dst));
PFN_DECLARE(int, Axa_SetReplacePath, (__int64 a1, const char *a2));
PFN_DECLARE(void, Axa_FreeAllPackages, ());
PFN_DECLARE(__int64, Axa_CFileMan_GetRemasteredCount, ());
PFN_DECLARE(Axa::RemasteredEntry*, Axa_CFileMan_GetRemasteredEntry, (Axa::CFileMan* a1, int* origOffsetPtr, int assetNum));
PFN_DECLARE(void*, Axa_PackageFile_GetRemasteredAsset, (Axa::PackageFile* a1, unsigned int* assetSizePtr, int assetNum));
PFN_DECLARE(__int64, Axa_AxaSoundStream__threadProc, (unsigned int* instance));
PFN_DECLARE(int, Axa_OpenFile, (const char* Format, int OFlag));
PFN_DECLARE(void, Axa_DebugPrint, (const char* Format, ...));
PFN_DECLARE(void, Axa_DecryptFile, (Axa::PackageFile* pkg, void* data, int size, Axa::PkgEntry* pkgent));
PFN_DECLARE(__int64, Axa_DecompressFile, (void* outBuf, int* decSizePtr, void* inBuf, int compSize));
PFN_DECLARE(void, VAG_STREAM_play, (const char* fileName, int volume, int fadeVolume, int time));
PFN_DECLARE(void, VAG_STREAM_fadeOut, (unsigned int time));
PFN_DECLARE(void, VAG_STREAM_setVolume, (int volume));
PFN_DECLARE(void, VAG_STREAM_exit, ());
PFN_DECLARE(size_t, Bbs_File_load, (const char* pszPath, long long a2));
PFN_DECLARE(void, Bbs_CRsrcData_loadCallback, (unsigned int* pMem, size_t size, unsigned int* pArg, int nOpt));

template <typename T>
class VarPtr
{
public:
    void SetPtr(T* ptr)
    {
        this->ptr = ptr;
    }

    constexpr T* operator &()
    {
        return ptr;
    }

    constexpr operator T&()
    {
        return *ptr;
    }

private:
    T* ptr;
};

template <typename T, size_t len>
class ArrayPtr
{
public:
    void SetPtr(T* ptr)
    {
        this->ptr = ptr;
    }

    constexpr T* operator &()
    {
        return ptr;
    }

    constexpr operator T* ()
    {
        return ptr;
    }

    constexpr size_t size() const noexcept
    {
        return len;
    }

private:
    T* ptr;
};

#define PVAR_DECLARE(TYPE, NAME) extern VarPtr<TYPE> NAME
#define PARR_DECLARE(TYPE, NAME, LEN) extern ArrayPtr<TYPE,LEN> NAME

PVAR_DECLARE(int, PackageFileCount);
PVAR_DECLARE(int, LastOpenedPackage);
PARR_DECLARE(Axa::PackageFile*, PackageFiles, 16);
PARR_DECLARE(char, BasePath, 128);
PVAR_DECLARE(Axa::PCSettings, PCSettingsPtr);
PARR_DECLARE(float, VolumeLevels, 11);
