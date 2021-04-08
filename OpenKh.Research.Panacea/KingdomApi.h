#pragma once
#include "OpenKH.h"

enum KingdomApiFunction
{
    Axa_CFileMan_LoadFile,
    Axa_CFileMan_GetFileSize,
    Axa_AxaResourceMan_SetResourceItem,
    Axa_PackageMan_GetFileInfo,
    KingdomApiFunction_END
};

const int Bbs_File_load = 0xE7D70;
const int Bbs_CRsrcData_loadCallback = 0x113800;

#define PFN_DECLARE(RETURN_TYPE, NAME, ARGS) \
	typedef RETURN_TYPE (__cdecl * PFN_##NAME)ARGS; \
    extern PFN_##NAME pfn_##NAME

namespace Axa
{
    class CFileMan
    {
    public:
        static long GetFileSize(CFileMan* _this, const char* filename, int mode);
        static long LoadFile(CFileMan* _this, const char* filename, void* addr, bool useHdAsset);
    };

    namespace AxaResourceMan
    {
        long SetResourceItem(const char* filename);
    };

    namespace PackageMan
    {
        struct Unk
        {
            long long Unk00;
            long long Unk08;
            int Unk10;
            int Unk14;
            int Unk18;
            int Unk1c;
            char PkgPath[0x100];
            long long Unk120;
            long long Unk128;
            long long Unk130;
            long long Unk138;
            long long Unk140;
            long long FileSize;
            long long Unk150;
            char FilePath[0x100];
            // no idea how long the structure is
        };

        Unk* GetFileInfo(const char* filename, int mode);
    };
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
PFN_DECLARE(long, Axa_CFileMan_GetFileSize, (Axa::CFileMan* _this, const char* filename, int mode));
PFN_DECLARE(long, Axa_AxaResourceMan_SetResourceItem, (const char* filename));
PFN_DECLARE(Axa::PackageMan::Unk*, Axa_PackageMan_GetFileInfo, (const char* filename, int mode));
PFN_DECLARE(size_t, Bbs_File_load, (const char* pszPath, long long a2));
PFN_DECLARE(void, Bbs_CRsrcData_loadCallback, (unsigned int* pMem, size_t size, unsigned int* pArg, int nOpt));
