#pragma once
#include "KingdomApi.h"

namespace Panacea
{
    void Initialize();
    bool TransformFilePath(char* strOutPath, int maxLength, const char* originalPath);

    long  __cdecl LoadFile(Axa::CFileMan* _this, const char* filename, void* addr, bool unk);
    long  __cdecl GetFileSize(Axa::CFileMan* _this, const char* filename, int mode);

    size_t __cdecl BbsFileLoad(const char* filename, long long a2);
    void __cdecl BbsCRsrcDataloadCallback(unsigned int* pMem, size_t size, unsigned int* pArg, int nOpt);
}

