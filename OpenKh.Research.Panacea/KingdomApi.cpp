#include "KingdomApi.h"
#include "Panacea.h"

#define PFN_DEFINE(NAME) PFN_##NAME pfn_##NAME = nullptr

PFN_DEFINE(Axa_CFileMan_LoadFile);
PFN_DEFINE(Axa_CFileMan_GetFileSize);
PFN_DEFINE(Axa_AxaResourceMan_SetResourceItem);
PFN_DEFINE(Axa_PackageMan_GetFileInfo);
PFN_DEFINE(Bbs_File_load);
PFN_DEFINE(Bbs_CRsrcData_loadCallback);

long Axa::CFileMan::LoadFile(CFileMan* _this, const char* filename, void* addr, bool unk) {
    return pfn_Axa_CFileMan_LoadFile(_this, filename, addr, unk);
}

long Axa::CFileMan::GetFileSize(CFileMan* _this, const char* filename, int mode) {
    return pfn_Axa_CFileMan_GetFileSize(_this, filename, mode);
}

long Axa::AxaResourceMan::SetResourceItem(const char* filename) {
    return pfn_Axa_AxaResourceMan_SetResourceItem(filename);
}

Axa::PackageMan::Unk* Axa::PackageMan::GetFileInfo(const char* filename, int mode) {
    return pfn_Axa_PackageMan_GetFileInfo(filename, mode);
}

size_t Bbs::File::load(const char* filename, long long a2) {
    return pfn_Bbs_File_load(filename, a2);
}

void Bbs::CRsrcData::loadCallback(unsigned int* pMem, size_t size, unsigned int* pArg, int nOpt) {
    pfn_Bbs_CRsrcData_loadCallback(pMem, size, pArg, nOpt);
}
