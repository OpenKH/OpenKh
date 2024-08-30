#pragma once
#include "KingdomApi.h"

namespace Panacea
{
    void Initialize();
    bool GetRawFile(wchar_t* strOutPath, int maxLength, const char* originalPath);
    bool TransformFilePath(wchar_t* strOutPath, int maxLength, const char* originalPath);
    int FrameHook(__int64 a1);

    int SetReplacePath(__int64 a1, const char* a2);
    void FreeAllPackages();
    long  __cdecl LoadFile(Axa::CFileMan* _this, const char* filename, void* addr, bool useHdAsset);
    long  __cdecl LoadFileWithSize(Axa::CFileMan* _this, const char* filename, void* addr, int size, bool useHdAsset);
    void*  __cdecl LoadFileWithMalloc(Axa::CFileMan* _this, const char* filename, int* sizePtr, bool useHdAsset, const char* filename2);
    long  __cdecl GetFileSize(Axa::CFileMan* _this, const char* filename);
    __int64  __cdecl GetRemasteredCount();
    Axa::RemasteredEntry* __cdecl GetRemasteredEntry(Axa::CFileMan* a1, int* origOffsetPtr, int assetNum);
    void* GetRemasteredAsset(Axa::PackageFile* a1, unsigned int* assetSizePtr, int assetNum);
    bool OpenFileImpl(Axa::PackageFile* a1, const char* filePath, const char* altBasePath);
    namespace VAG_STREAM
    {
        void play(const char* fileName, int volume, int fadeVolume, int time);
        void fadeOut(unsigned int time);
        void setVolume(int volume);
        void exit();
    }
    void DebugPrint(const char* format, ...);

    size_t __cdecl BbsFileLoad(const char* filename, long long a2);
    void __cdecl BbsCRsrcDataloadCallback(unsigned int* pMem, size_t size, unsigned int* pArg, int nOpt);
}

