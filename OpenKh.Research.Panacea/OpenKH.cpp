#include "OpenKH.h"

long OpenKH::LoadFile(const char* filename, void* addr) {
    const char BasePath[] = "C:/hd28/EPIC/juefigs/KH2ReSource/";
    char buffer[0x80];
    strcpy(buffer, BasePath);
    strcpy(buffer + strlen(BasePath), filename);

    return Axa::CFileMan::LoadFile(nullptr, buffer, addr, false);
}
