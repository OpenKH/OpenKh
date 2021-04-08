#pragma once

#define BaseAddress 0xC00
#define Axa_CFileMan_LoadFile 0x1367D0
#define Axa_CFileMan_GetFileSize 0x1364D0
#define Axa_AxaResourceMan_SetResourceItem 0x139630
#define Axa_PackageMan_GetFileInfo 0x136A10

#define PFNDEF(RETURN_TYPE, NAME, ARGS) \
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

PFNDEF(long, Axa_CFileMan_LoadFile, (Axa::CFileMan* _this, const char* filename, void* addr, bool useHdAsset));
PFNDEF(long, Axa_CFileMan_GetFileSize, (Axa::CFileMan* _this, const char* filename, int mode));
PFNDEF(long, Axa_AxaResourceMan_SetResourceItem, (const char* filename));
PFNDEF(Axa::PackageMan::Unk*, Axa_PackageMan_GetFileInfo, (const char* filename, int mode));
