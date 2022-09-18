#pragma once
#include <string>

namespace OpenKH
{
    enum class GameId
    {
        Unknown = -1,
        KingdomHearts1,
        KingdomHearts2,
        KingdomHeartsReCom,
        KingdomHeartsBbs,
        KingdomHeartsDdd,
        END,
    };

    extern GameId m_GameID;
    extern std::wstring m_ModPath;
    extern bool m_ShowConsole;
    extern bool m_DebugLog;
    extern bool m_DisableCache;

    void Initialize();
    void Main();

    GameId DetectGame();
    long LoadFile(const char* filename, void* addr);
    void ReadSettings(const char* filename);

}
