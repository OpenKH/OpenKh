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
        Launcher1_5_2_5,
        Launcher2_8,
        END,
    };

    extern GameId m_GameID;
    extern std::wstring m_ModPath;
    extern std::wstring m_DevPath;
    extern std::wstring m_ExtractPath;
    extern bool m_ShowConsole;
    extern bool m_DebugLog;
    extern bool m_EnableCache;

    void Initialize();

    GameId DetectGame();
    long LoadFile(const char* filename, void* addr);
    void ReadSettings(const char* filename);

}
