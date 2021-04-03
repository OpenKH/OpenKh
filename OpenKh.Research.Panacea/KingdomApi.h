#pragma once

#define BaseAddress 0xC00
#define Axa_CFileMan_LoadFile 0x1367D0

#define PFNDEF(RETURN_TYPE, NAME, ARGS) \
	typedef RETURN_TYPE (__cdecl * PFN_##NAME)ARGS; \
    extern PFN_##NAME pfn_##NAME

namespace Axa
{
    class CFileMan
    {
    public:
        static long LoadFile(CFileMan* _this, const char* filename, void* addr, bool unk);
    };
}

PFNDEF(long, Axa_CFileMan_LoadFile, (Axa::CFileMan* _this, const char* filename, void* addr, bool unk));
