#pragma once
#include "KingdomApi.h"

namespace Panacea
{
    void Initialize();
    long  __cdecl LoadFile(Axa::CFileMan* _this, const char* filename, void* addr, bool unk);
}

