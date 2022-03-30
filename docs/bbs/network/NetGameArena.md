# [Kingdom Hearts Birth By Sleep](index.md) - [Network Packages](../network-packages.md) - NetGameArena

## NetGameArena

### ARENA_FLAG_BIT

| Postition | Size | Name
|-----------|------|-----
| 0         | 1    | uiError
| 1         | 1    | uiOnlineWait
| 2         | 1    | uiExterminate
| 3         | 1    | uiBattleLvSet
| 4         | 4    | __dummy__
| 8         | 1    | uiEventKeep
| 9         | 1    | uiBossDead
| 10        | 1    | uiRoundMonstro
| 11        | 1    | uiMapChange
| 12        | 1    | uiExtraRound
| 13        | 1    | uiRoundEnd
| 14        | 1    | uiReadyAll
| 15        | 1    | uiReady
| 16        | 1    | uiDataSetupAll
| 17        | 1    | uiDataSetup
| 18        | 1    | uiStartCheck
| 19        | 1    | uiCamAdvertise
| 20        | 1    | uiLeaderLive
| 21        | 1    | uiDownAll
| 22        | 1    | uiFinalRound
| 23        | 1    | uiRoundStart
| 24        | 1    | uiRoundEqual
| 25        | 1    | uiRoundClear
| 26        | 1    | uiDecideAll
| 27        | 1    | uiDecide
| 28        | 1    | uiReadyWindow
| 29        | 1    | uiExit
| 30        | 1    | uiLeader
| 31        | 1    | uiOnline

### ARENA_FLAG_DATA

| Length | Type           | Name
|--------|----------------|-----
| 4      | uint32         | uiFlagInt
| 4      | ARENA_FLAG_BIT | FlagBit

### ARENA_RESULT_INFO

| Offset | Length | Type        | Name
|--------|--------|-------------|-----
| 0x0    | 4      | uint32      | m_uiBattleCtdId
| 0x4    | 4      | uint32      | m_nPlayTime
| 0x8    | 2      | uint16      | m_nMedals
| 0xA    | 1      | byte        | m_nMVP
| 0xB    | 1      | byte        | m_dum0
| 0xC    | 114    | char[3][38] | m_szMember
| 0x7E   | 2      | byte[2]     | m_nDamageRank
| 0x80   | 2      | byte[2]     | m_nFriendRank
| 0x82   | 2      | byte[2]     | m_nStyleRank
| 0x84   | 1      | byte        | m_nFlagRank11
| 0x85   | 1      | byte        | m_dum1
| 0x86   | 1      | byte        | m_dum2
| 0x87   | 1      | byte        | m_dum3

### CNetGameArena

Split enums up under there own categories

| Name               | Value
|--------------------|------
| ARENA_STATE_NONE   | 0x0
| ARENA_STATE_ENTRY  | 0x1
| ARENA_STATE_READY  | 0x2
| ARENA_STATE_LOAD   | 0x3
| ARENA_STATE_GAME   | 0x4
| ARENA_STATE_RESULT | 0x5
| ARENA_STATE_EXIT   | 0x6
| ARENA_STATE_ERROR  | 0x7


| Name             | Value
|------------------|------
| STATE_LOAD_NONE  | 0x0
| STATE_LOAD_START | 0x1
| STATE_LOAD_EXEC  | 0x2
| STATE_LOAD_WAIT  | 0x3
| STATE_LOAD_END   | 0x4

| Name         | Value
|--------------|------
| CHECK_ENTRY  | 0x0
| CHECK_SETUP  | 0x1
| CHECK_LOAD   | 0x2
| CHECK_FIELD  | 0x3
| CHECK_READY  | 0x4
| CHECK_START  | 0x5
| CHECK_PLAY   | 0x6
| CHECK_NEXT   | 0x7
| CHECK_CLEAR  | 0x8
| CHECK_FAILED | 0x9
| CHECK_MAX    | 0xA

| Name                | Value
|---------------------|------
| STATE_GAME_NONE     | 0x0
| STATE_GAME_CHECK    | 0x1
| STATE_GAME_SETREQ   | 0x2
| STATE_GAME_SETWAIT  | 0x3
| STATE_GAME_DEMO     | 0x4
| STATE_GAME_READY    | 0x5
| STATE_GAME_START    | 0x6
| STATE_GAME_PLAY     | 0x7
| STATE_GAME_CLEAR    | 0x8
| STATE_GAME_MAPDEMO  | 0x9
| STATE_GAME_MAPSEQ   | 0xA
| STATE_GAME_MAPWAIT  | 0xB
| STATE_GAME_COMPLETE | 0xC
| STATE_GAME_EXTRA    | 0xD
| STATE_GAME_WAIT     | 0xE
| STATE_GAME_FAIELD   | 0xF
| STATE_GAME_END      | 0x10

| Name               | Value
|--------------------|------
| STATE_RESULT_NONE  | 0x0
| STATE_RESULT_DISP  | 0x1
| STATE_RESULT_BONUS | 0x2
| STATE_RESULT_END   | 0x3

| Name                   | Value
|------------------------|------
| STATE_ENTRY_NONE       | 0x0
| STATE_ENTRY_START      | 0x1
| STATE_ENTRY_LEADERWAIT | 0x2
| STATE_ENTRY_MEMBERWAIT | 0x3
| STATE_ENTRY_OK         | 0x4
| STATE_ENTRY_ERROR      | 0x5

| Name              | Value
|-------------------|------
| STATE_READY_NONE  | 0x0
| STATE_READY_FIRST | 0x1
| STATE_READY_START | 0x2
| STATE_READY_EXEC  | 0x3
| STATE_READY_WAIT  | 0x4
| STATE_READY_END   | 0x5

| Name        | Value
|-------------|------
| STATE_ERROR | 0x63

| Offset | Length | Type               | Name
|--------|--------|--------------------|-----
| 0x0    | 52     | CTreeTask          | super_CTreeTask
| 0x34   | 4      | IAllocator *       | m_pAllocator
| 0x38   | 4      | ARENA_FLAG_DATA    | m_FlagData
| 0x3C   | 4      | float              | m_fTimer
| 0x40   | 4      | float              | m_fLocalTimer
| 0x44   | 4      | float              | m_fGameTimer
| 0x48   | 10     | byte[10]           | m_nCheck
| 0x52   | 2      | uint16             | m_nCheckFlag
| 0x54   | 1      | byte               | m_nCheckCurrent
| 0x55   | 1      | byte               | m_dum0
| 0x56   | 1      | byte               | m_dum1
| 0x57   | 1      | byte               | m_dum2
| 0x58   | 1      | byte               | m_nState
| 0x59   | 1      | byte               | m_nPreState
| 0x5A   | 1      | byte               | m_nStateEntry
| 0x5B   | 1      | byte               | m_nStateReady
| 0x5C   | 1      | byte               | m_nStateLoad
| 0x5D   | 1      | byte               | m_nStateGame
| 0x5E   | 1      | byte               | m_nStateResult
| 0x5F   | 1      | byte               | m_nStateAlert
| 0x60   | 4      | uint32             | m_nStep
| 0x64   | 4      | CGameCockpit *     | m_pGameCockpit
| 0x68   | 4      | CGOnlineWait *     | m_pOnlineWait
| 0x6C   | 3      | byte[3]            | m_nLiveInfo
| 0x6F   | 1      | byte               | m_nRecvMax
| 0x70   | 1      | byte               | m_nRecvOnline
| 0x71   | 1      | byte               | m_nRecvLost
| 0x72   | 1      | byte               | m_nLeaderId
| 0x73   | 1      | byte               | m_nDataSetupOK
| 0x74   | 1      | byte               | m_nStartMember
| 0x75   | 1      | byte               | m_nPacketCode
| 0x76   | 1      | byte               | m_nPlayerNum
| 0x77   | 1      | byte               | m_nPresent
| 0x78   | 1      | byte               | m_nMVP
| 0x79   | 1      | byte               | m_nMember
| 0x7A   | 1      | byte               | m_nMemberLost
| 0x7B   | 1      | byte               | m_nMemberOK
| 0x7C   | 36     | PLAYER_SCORE[3]    | m_PlayerScore
| 0xA0   | 136    | ARENA_RESULT_INFO  | m_ArenaResultInfo
| 0x128  | 1      | byte               | m_nRound
| 0x129  | 1      | byte               | m_nRoundReq
| 0x12A  | 1      | byte               | m_nRoundClear
| 0x12B  | 1      | byte               | m_nRoundMax
| 0x12C  | 1      | byte               | m_nMemberReady
| 0x12D  | 1      | byte               | m_nAllDownCount
| 0x12E  | 2      | int16              | m_nBossHp
| 0x130  | 4      | _HANDLE_           | m_hBoss
| 0x134  | 4      | uint32             | m_nStartTime
| 0x138  | 4      | uint32             | m_nClearTime
| 0x13C  | 4      | ARENA_EVENT_DATA * | m_nEventData
| 0x140  | 16     | char[16]           | m_szMissionName
| 0x150  | 4      | _HANDLE_           | m_hPresent
