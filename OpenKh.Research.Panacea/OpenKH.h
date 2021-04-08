#pragma once
#include <cstring>

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

    void Initialize();
    void Main();

    GameId DetectGame();
    long LoadFile(const char* filename, void* addr);
}
