#pragma once
#include "KingdomApi.h"

namespace Panacea
{
    void Initialize();
    bool TransformFilePath(char* strOutPath, int maxLength, const char* originalPath);
    int FrameHook(__int64 a1);

    int SetReplacePath(__int64 a1, const char* a2);
    void FreeAllPackages();
    long  __cdecl LoadFile(Axa::CFileMan* _this, const char* filename, void* addr, bool unk);
    void*  __cdecl LoadFileWithMalloc(Axa::CFileMan* _this, const char* filename, int* sizePtr, bool unk, const char* filename2);
    long  __cdecl GetFileSize(Axa::CFileMan* _this, const char* filename);
    __int64  __cdecl GetRemasteredCount();
    Axa::RemasteredEntry* __cdecl GetRemasteredEntry(Axa::CFileMan* a1, int* origOffsetPtr, int assetNum);
    void* GetRemasteredAsset(Axa::PackageFile* a1, unsigned int* assetSizePtr, int assetNum);
    int GetAudioStream(Axa::CFileMan* a1, const char* a2);
    void DebugPrint(const char* format, ...);

    size_t __cdecl BbsFileLoad(const char* filename, long long a2);
    void __cdecl BbsCRsrcDataloadCallback(unsigned int* pMem, size_t size, unsigned int* pArg, int nOpt);
}

