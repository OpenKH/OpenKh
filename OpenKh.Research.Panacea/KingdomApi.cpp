#include "KingdomApi.h"
#include "Panacea.h"

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
