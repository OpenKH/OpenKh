# [Kingdom Hearts Birth By Sleep](index.md) - Network Packages

## Network Def

### PACKET_DATA

| Offset | Length | Type       | Name
|--------|--------|------------|-----
| 0      | 68     | PKT_HEADER | m_pktHeader
| 68     | 80     | PKT_GAME   | m_pktGame
| 148    | 880    | PKT_OBJECT | m_pktObject

### PKT_HEADER

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0      | 1      | byte      | m_nUser
| 1      | 1      | byte      | m_nCode
| 2      | 1      | byte      | [m_nPhase](#PKT_HEADER-Phase) Needs testing
| 3      | 1      | byte      | m_nReady
| 4      | 1      | byte      | pad0
| 5      | 1      | byte      | m_nOpenLv
| 6      | 1      | undefined |
| 7      | 1      | undefined |
| 8      | 4      | uint32    | m_nSendCount
| 12     | 4      | uint32    | m_uiUserColor
| 16     | 38     | char[38]  | m_szNickName
| 54     | 8      | char[8]   | m_szRoomName
| 62     | 1      | undefined |
| 63     | 1      | undefined |
| 64     | 4      | float     | m_fUpdate

### PKT_GAME

| Offset | Length | Type    | Name
|--------|--------|---------|-----
| 0      | 80     | char[8] | m_nData

### PKT_OBJECT

| Offset | Length | Type    | Name
|--------|--------|---------|-----
| 0      | 880    | char[8] | m_nData

### Network State

| Name                       | Value
|----------------------------|------
| NETWORK_NONE               | 0x0
| NETWORK_INIT               | 0x1
| NETWORK_TOP                | 0x2
| NETWORK_CONNECT_IBSS       | 0x3
| NETWORK_CONNECTING_IBSS    | 0x4
| NETWORK_IBSS               | 0x5
| NETWORK_CONNECT_ROOM       | 0x6
| NETWORK_CONNECTING_ROOM    | 0x7
| NETWORK_DISCONNECTING_ROOM | 0x8
| NETWORK_CONNECTING_GAME    | 0x9
| NETWORK_DISCONNECTING      | 0xA
| NETWORK_ERROR_DIALOG       | 0xB
| NETWORK_LOBBY              | 0xC
| NETWORK_NEIGHBORS          | 0xD
| NETWORK_REQUESTING         | 0xE
| NETWORK_CANCELED           | 0xF
| NETWORK_DENIED             | 0x10
| NETWORK_PEER_BUSY          | 0x11
| NETWORK_PEER_DISAPPEARED   | 0x12
| NETWORK_START_GAME         | 0x13
| NETWORK_DISCONNECT         | 0x14
| NETWORK_CHILD_WAIT         | 0x15
| NETWORK_PARENT_START       | 0x16
| NETWORK_PARENT_SYNC        | 0x17
| NETWORK_START_READY        | 0x18
| NETWORK_CREATE_ROOM        | 0x19
| NETWORK_PARENT_WAIT        | 0x1A
| NETWORK_GAME               | 0x1B
| NETWORK_GAME_DISCONNECT    | 0x1C
| NETWORK_OUT                | 0x1D
| NETWORK_MAX                | 0x1E

### Matching Mode

| Name                 | Value
|----------------------|------
| MATCHING_MODE_CHILD  | 0x0
| MATCHING_MODE_PARENT | 0x1

### HELLO_OPT_GAME_PARAM

| Offset | Length | Type | Name
|--------|--------|------|-----
| 0      | 1      | byte | nGameMode
| 1      | 1      | byte | nEventRule
| 2      | 1      | byte | nDataMode
| 3      | 1      | byte | nTimeLimit
| 4      | 1      | byte | nMemberMax
| 5      | 1      | byte | nField
| 6      | 1      | byte | nTarget
| 7      | 1      | byte | nRank

### GAME_INFO

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0      | 4      | int32 | m_iServerFlag
| 4      | 4      | int32 | m_iClientReady
| 8      | 4      | int32 | m_iServerIndex

### PKT_HEADER Phase

| Name       | Value
|------------|------
| PHASE_OK   | 0x0
| PHASE_NG   | 0x1
| PHASE_LATE | 0x2
| PHASE_LOST | 0x3

### PACKET_DATA Code

| Name               | Value
|--------------------|------
| CODE_NONE          | 0x0
| CODE_LOBBY_PLAY    | 0x0
| CODE_VSMODE_SELECT | 0x1
| CODE_VSMODE_PLAY   | 0x2
| CODE_VSMODE_RESULT | 0x3
| CODE_ARENA_READY   | 0x4
| CODE_ARENA_PLAY    | 0x5
| CODE_DICE_         | 0x6
| CODE_RACE_         | 0x7

___

## Network

Following ones were are split up as this make more sense then they're all together
These are 4 bytes in size

### State

| Name                 | Value
|----------------------|------
| STATE_0              | 0x0
| STATE_CONNECT_IBSS   | 0x1
| STATE_MATCHING_LOBBY | 0x2
| STATE_PLAYING_GAME   | 0x3
| STATE_MAX            | 0x4

### EVF

| Name           | Value
|----------------|------
| EVF_ERROR      | 0x1
| EVF_CONNECT    | 0x2
| EVF_DISCONNECT | 0x4
| EVF_SCAN       | 0x8
| EVF_GAMEMODE   | 0x10

### Flag

| Name                           | Value
|--------------------------------|------
| FLAG_INIT_MODULES              | 0x1
| FLAG_CONNECTED_IBSS            | 0x2
| FLAG_CONNECTED_LOBBY           | 0x4
| FLAG_CONNECTED_ROOM            | 0x8
| FLAG_ENTERED_ROOM              | 0x10
| FLAG_READY_GAME                | 0x20
| FLAG_START_READY               | 0x40
| FLAG_START_GAME                | 0x80
| FLAG_PLAYING_GAME              | 0x100
| FLAG_LOBBY_PLAY_SET            | 0x200
| FLAG_LOBBY_PLAY_ON             | 0x400
| FLAG_LOBBY_OVER                | 0x800
| FLAG_PEERLIST_OK               | 0x10000
| FLAG_MMEMBER_OK                | 0x20000
| FLAG_INIT_MODULE_PSPNETINIT    | 0x100000
| FLAG_INIT_MODULE_ADHOCINIT     | 0x200000
| FLAG_INIT_MODULE_ADHOCCTRLINIT | 0x400000

### Frame

| Name                  | Value
|-----------------------|------
| FRAME_UTTILITY_UPDATE | 0x2

### Game

| Name               | Value
|--------------------|------
| GAME_NICKNAME_SIZE | 0x26

### JOB

| Name      | Value
|-----------|------
| _JOB_PRI_ | 0x9C40

___

## NetGameVSMode

### VSMODE_FLAG_DATA

Union

| Length | Type            | Name
|--------|-----------------|-----
| 4      | uint32          | uiFlagInt
| 4      | VSMODE_FLAG_BIT | FlagBit

### VSMODE_FLAG_BIT

| Postition | Size | Name            |
|-----------|------|-----------------|
| 0         | 1    | UiError         |
| 1         | 1    | UiOnlineWait    |
| 2         | 1    | UiExterminate   |
| 3         | 6    | __dummy__       |
| 9         | 1    | uiDataSetupAll  |
| 10        | 1    | uiDataSetup     |
| 11        | 1    | uiStartCheck    |
| 12        | 1    | uiReadyAll      |
| 13        | 1    | uiReady         |
| 14        | 1    | uiExitStart     |
| 15        | 1    | uiPCamAdvertise |
| 16        | 1    | uiRoundStart    |
| 17        | 1    | uiLeaderLive    |
| 18        | 1    | uiTeamError     |
| 19        | 1    | uiResultWindow  |
| 20        | 1    | uiMVP           |
| 21        | 1    | uiLose          |
| 22        | 1    | uiWin           |
| 23        | 1    | uiDecideAll     |
| 24        | 1    | uiDecide        |
| 25        | 1    | uiExit          |
| 26        | 1    | uiSelectWindow  |
| 27        | 1    | uiTotalScore    |
| 28        | 1    | uiDataFree      |
| 29        | 1    | uiTeamBattle    |
| 30        | 1    | uiLeader        |
| 31        | 1    | uiOnline        |

### PLAYER_INFO

| Offset | Length | Type     | Name
|--------|--------|----------|-----
| 0      | 38     | char[38] | m_szName
| 38     | 1      | byte     | m_nDeck
| 39     | 1      | byte     | m_nColor
| 40     | 1      | byte     | m_nTeam
| 41     | 1      | byte     | m_nDecide
| 42     | 1      | byte     | m_nRank
| 43     | 1      | byte     | m_nWin
| 44     | 1      | byte     | m_nLose
| 45     | 1      | byte     | m_nMVP
| 46     | 1      | byte     | m_nLiveInfo
| 47     | 1      | byte     | m_dum0
| 48     | 4      | uint32   | m_nScore
| 52     | 4      | uint32   | m_nScoreR
| 56     | 4      | uint32   | m_nTotal
| 60     | 2      | uint16   | m_nMedal
| 62     | 2      | uint16   | m_nBonusP
| 64     | 2      | uint16   | m_nBonusM
| 66     | 1      | byte     | m_dum1
| 67     | 1      | byte     | m_dum2

### PLAYER_SCORE

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0      | 4      | uint32    | m_nScore
| 4      | 2      | uint16    | m_nMedal
| 6      | 2      | uint16    | m_nBonusP
| 8      | 2      | uint16    | m_nBonusM
| 10     | 2      | undefined | 
| 11     | 2      | undefined | 

### TEAM_INFO

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0      | 4      | uint32    | m_nScore
| 4      | 4      | uint32    | m_nScoreR
| 8      | 2      | uint16    | m_nBonusP
| 10     | 2      | uint16    | m_nBonusM
| 12     | 1      | byte      | m_nWin
| 13     | 1      | byte      | m_nLose
| 14     | 1      | byte      | m_nRank
| 15     | 1      | byte      | m_nMemberCount
| 16     | 6      | byte[6]   | m_nMember
| 22     | 1      | undefined |
| 23     | 1      | undefined |

### TEAM_SCORE

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0      | 4      | uint32    | m_nScore
| 4      | 1      | byte      | m_nWin
| 5      | 1      | byte      | m_nLose
| 6      | 1      | undefined | 
| 7      | 1      | undefined | 

### CNetGameVSMode

| Offset | Length | Type             | Name
|--------|--------|------------------|-----
| 0      | 52     | CTreeTask        | super_CTreeTask
| 52     | 4      | IAllocator*      | m_pAllocator
| 56     | 4      | VSMODE_FLAG_DATA | m_FlagData
| 60     | 4      | float            | m_fTimer
| 64     | 4      | float            | m_fLocalTimer
| 68     | 4      | float            | m_fGamerTimer
| 72     | 1      | byte             | m_nState
| 73     | 1      | byte             | m_nPreState
| 74     | 1      | byte             | m_nStateEntry
| 75     | 1      | byte             | m_nStateSelect
| 76     | 1      | byte             | m_nStateLoad
| 77     | 1      | byte             | m_nStateGame
| 78     | 1      | byte             | m_nStateResult
| 79     | 1      | byte             | m_nStateAlert
| 80     | 10     | byte[10]         | m_nCheck
| 90     | 2      | uint16           | m_nCheckFlag
| 92     | 1      | byte             | m_nCheckCurrent
| 93     | 1      | byte             | m_dum0
| 94     | 1      | byte             | m_dum1
| 95     | 1      | byte             | m_dum2
| 96     | 4      | CGameCockpit*    | m_pGameCockpit
| 100    | 4      | CGOnlineWait*    | m_pOnlineWait
| 104    | 6      | byte[6]          | m_nLiveInfo
| 110    | 1      | byte             | m_nRecvMax
| 111    | 1      | byte             | m_nRecvOnline
| 112    | 1      | byte             | m_nRecvLost
| 113    | 1      | byte             | m_nLeaderId
| 114    | 1      | byte             | m_nDataSetupOK
| 115    | 1      | byte             | m_nStartMember
| 116    | 1      | byte             | m_nPacketCode
| 117    | 1      | byte             | m_nDropCount
| 118    | 1      | byte             | m_nMemberReady
| 119    | 1      | byte             | m_inGameMode
| 120    | 1      | byte             | m_inEventRule
| 121    | 1      | byte             | m_inDataMode
| 122    | 1      | byte             | m_inTimeLimit
| 123    | 1      | byte             | m_inMemberMax
| 124    | 1      | byte             | m_inField
| 125    | 1      | byte             | m_outGameMode
| 126    | 1      | byte             | m_outEventRule
| 127    | 1      | byte             | m_outDataMode
| 128    | 1      | byte             | m_outTimeLimit
| 129    | 1      | byte             | m_outMemberMax
| 130    | 1      | byte             | m_outField
| 131    | 1      | byte             | m_nMVP
| 132    | 1      | byte             | m_nTeam
| 133    | 1      | byte             | m_nDeckType
| 134    | 1      | byte             | m_nPlayerNum
| 135    | 1      | byte             | m_nMember
| 136    | 1      | byte             | m_nMemberLost
| 137    | 1      | byte             | m_nMemberOK
| 138    | 1      | byte             | m_nResult
| 139    | 1      | undefined        | 
| 140    | 72     | PLAYER_SCORE[6]  | m_PlayerScore
| 212    | 16     | TEAM_SCORE[2]    | m_TeamScore
| 228    | 408    | PLAYER_INFO[6]   | m_PlayerInfo
| 636    | 48     | TEAM_INFO[6]     | m_TeamInfo

### Split up next enums in there own categorie

### Check

| Name         | Value
|--------------|------
| CHECK_ENTRY  | 0x0
| CHECK_READY  | 0x1
| CHECK_SELECT | 0x2
| CHECK_SETUP  | 0x3
| CHECK_LOAD   | 0x4
| CHECK_FIELD  | 0x5
| CHECK_START  | 0x6
| CHECK_PLAY   | 0x7
| CHECK_RESULT | 0x8
| CHECK_NEXT   | 0x9
| CHECK_MAX    | 0xA

### State Entry

| Name                   | Value
|------------------------|------
| STATE_ENTRY_NONE       | 0x0
| STATE_ENTRY_START      | 0x1
| STATE_ENTRY_LEADERWAIT | 0x2
| STATE_ENTRY_MEMBERWAIT | 0x3
| STATE_ENTRY_OK         | 0x4
| STATE_ENTRY_ERROR      | 0x5

### State Error

| Name        | Value
|-------------|------
| STATE_ERROR | 0x63

### State Game

| Name             | Value
|------------------|------
| STATE_GAME_NONE  | 0x0
| STATE_GAME_CHECK | 0x1
| STATE_GAME_INIT  | 0x2
| STATE_GAME_DEMO  | 0x3
| STATE_GAME_READY | 0x4
| STATE_GAME_START | 0x5
| STATE_GAME_PLAY  | 0x6
| STATE_GAME_WAIT  | 0x7
| STATE_GAME_END   | 0x8

### State Load

| Name             | Value
|------------------|------
| STATE_LOAD_NONE  | 0x0
| STATE_LOAD_START | 0x1
| STATE_LOAD_EXEC  | 0x2
| STATE_LOAD_WAIT  | 0x3
| STATE_LOAD_END   | 0x4

### State Result

| Name               | Value
|--------------------|------
| STATE_RESULT_NONE  | 0x0
| STATE_RESULT_CHECK | 0x1
| STATE_RESULT_DEMO  | 0x2
| STATE_RESULT_DISP  | 0x3
| STATE_RESULT_WAIT  | 0x4
| STATE_RESULT_END   | 0x5

### State Select

| Name                | Value
|---------------------|------
| STATE_SELECT_NONE   | 0x0
| STATE_SELECT_FIRST  | 0x1
| STATE_SELECT_START  | 0x2
| STATE_SELECT_SETUP  | 0x3
| STATE_SELECT_READY  | 0x4
| STATE_SELECT_EXEC   | 0x5
| STATE_SELECT_DECIDE | 0x6
| STATE_SELECT_WAIT   | 0x7
| STATE_SELECT_END    | 0x8





### VSMode

| Name                | Value
|---------------------|------
| VSMODE_STATE_NONE   | 0x0
| VSMODE_STATE_ENTRY  | 0x1
| VSMODE_STATE_SELECT | 0x2
| VSMODE_STATE_LOAD   | 0x3
| VSMODE_STATE_GAME   | 0x4
| VSMODE_STATE_RESULT | 0x5
| VSMODE_STATE_EXIT   | 0x6
| VSMODE_STATE_ERROR  | 0x7

___

## NetGameRace

### Event

| Name                    | Value
|-------------------------|------
| EVENT_LEADER_PLAYER_NUM | 0x0

### Packet

| Name             | Value
|------------------|------
| PACKET_INDEX_MAX | 0x6

### NPC Player

| Name                 | Value
|----------------------|------
| NPC_PLAYER_NUM_START | 0xA

### Phase

| Name                    | Value
|-------------------------|------
| PHASE_CHECK_MEMBER      | 0x1
| PHASE_CHECK_PLAYER_NUM  | 0x2
| PHASE_CHECK_MEMBER_INFO | 0x3
| PHASE_CHECK_NPC_INFO    | 0x4
| PHASE_CHECK_COUNT_DOWN  | 0x5
| PHASE_RACE              | 0x6
| PHASE_RESULT            | 0x7
| PHASE_MAX               | 0x8

### PktObject Race

| Offset | Length | Type               | Name
|--------|--------|--------------------|-----
| 0      | 1      | sbyte              | m_nPlayerNum
| 1      | 1      | byte               | m_CurrentBestPlayerCarLap
| 2      | 1      | byte               | fillerb0
| 3      | 1      | byte               | fillerb1
| 4      | 4      | float              | m_GameTimer
| 8      | 104    | CAR                | m_Player
| 112    | 580    | NPC[5]             | m_Npc
| 692    | 64     | GIMMICK_TORNADO[4] | m_GimmickTornado
| 756    | 32     | GIMMICK_SHIELD[4]  | m_GimmickShield
| 788    | 6      | byte[6]            | m_isRequestCutNetwork
| 794    | 1      | byte               | filler0
| 795    | 1      | byte               | filler1
| 796    | 8      | GIMMICK_PISTON     | m_GimmickPiston

### PktObject Object

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0      | 8      | HEADER | m_Header
| 8      | 4      | GAME   | m_Game
| 12     | 804    | RACE   | m_RaceData

### PktObject Header

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0      | 2      | uint16 | m_Phase
| 2      | 1      | byte   | m_isEnable
| 3      | 1      | byte   | m_CharaID
| 4      | 2      | uint16 | m_AllMemberPhaseRecive
| 6      | 1      | byte   | filler0
| 7      | 1      | byte   | filler1

### PktObject Game

| Offset | Length | Type | Name
|--------|--------|------|-----
| 0      | 1      | byte | fillerb0
| 1      | 1      | byte | fillerb1
| 2      | 1      | byte | fillerb2
| 3      | 1      | byte | fillerb3

### PktObject Rider

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0      | 2      | int16 | m_nPosX
| 2      | 2      | int16 | m_nPosZ
| 4      | 2      | int16 | m_nPosY
| 6      | 2      | int16 | m_nRotX
| 8      | 2      | int16 | m_nRotY
| 10     | 2      | int16 | m_nRotZ

### PktObject GIMMICK

| Name                | Value
|---------------------|------
| GIMMICK_TORNADO_MAX | 0x4
| GIMMICK_SHIELD_MAX  | 0x4

### PktObject NPC

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0      | 104    | CAR   | m_Car
| 104    | 12     | RIDER | m_Rider

### PktObject GIMMICK_TORNADO

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0      | 1      | byte  | m_UniqueID
| 1      | 1      | byte  | m_MoveDirection
| 2      | 2      | int16 | m_nPosY
| 4      | 4      | float | m_nPosX
| 8      | 4      | float | m_nPosZ
| 12     | 2      | int16 | m_nVelX
| 14     | 2      | int16 | m_nVelZ

### PktObject GIMMICK_SHIELD

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0      | 1      | byte   | m_UniqueID
| 1      | 1      | byte   | filler0
| 2      | 1      | byte   | filler1
| 3      | 1      | byte   | filler2
| 4      | 4      | uint32 | m_DisappearCounter

### PktObject GIMMICK_PISTON

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0      | 2      | uint16 | m_nAnimTime
| 2      | 1      | byte   | filler00
| 3      | 1      | byte   | filler01
| 4      | 4      | float  | m_MotSpeed

### PktObject CAR_BIT

| Postition | Size | Name            |
|-----------|------|-----------------|
| 0         | 21   | __dummy__
| 21        | 1    | m_isFixedBranchCourseSelect
| 22        | 1    | m_isBranchChecking
| 23        | 1    | m_isUseBranchPath
| 24        | 1    | m_isWrongWay
| 25        | 1    | m_isCheckDangerAfterPlayer
| 26        | 1    | m_isLockOnEnable
| 27        | 2    | m_ShieldState (int)
| 29        | 2    | m_GuardState (int)
| 30        | 1    | m_isRiderAction

### PktObject CAR

| Offset | Length | Type    | Name
|--------|--------|---------|-----
| 0      | 48     | BULLET  | m_Bullet
| 48     | 1      | byte    | m_nAnimNo
| 49     | 1      | byte    | m_LockOnPlayerNum
| 50     | 1      | byte    | m_DangerPlayerNum
| 51     | 1      | byte    | m_State
| 52     | 4      | CAR_BIT | m_CarBit
| 56     | 1      | byte    | m_DarkStateCounter
| 57     | 1      | sbyte   | m_CurrentLap
| 58     | 1      | byte    | filler1
| 59     | 1      | byte    | filler2
| 60     | 4      | float   | m_nPosX
| 64     | 4      | float   | m_nPosZ
| 68     | 2      | int16   | m_nPosY
| 70     | 2      | int16   | m_nRotX
| 72     | 2      | int16   | m_nRotY
| 74     | 2      | int16   | m_nRotZ
| 76     | 4      | float   | m_nOldX
| 80     | 4      | float   | m_nOldZ
| 84     | 2      | int16   | m_nOldY
| 86     | 2      | int16   | m_nVelX
| 88     | 2      | int16   | m_nVelY
| 90     | 2      | int16   | m_nVelZ
| 92     | 2      | uint16  | m_nAnimTime
| 94     | 1      | ubyte   | m_CurrentPathIndex
| 95     | 1      | ubyte   | m_CurrentPathCount
| 96     | 1      | ubyte   | m_CurrentPathDataNo
| 97     | 1      | ubyte   | m_CurrentBranchTargetPathIndex
| 98     | 1      | byte    | filler00
| 99     | 1      | byte    | filler01
| 100    | 4      | float   | m_CheckDangerAfterPlayerLength

### PktObject BULLET

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0      | 1      | byte   | m_isExist
| 1      | 1      | byte   | m_DangerPlayerNum
| 2      | 2      | uint16 | m_BulletID
| 4      | 1      | byte   | m_State
| 5      | 1      | byte   | m_Mode
| 6      | 1      | byte   | m_fillerb0
| 7      | 1      | byte   | m_fillerb1
| 8      | 4      | float  | m_TimbeID
| 12     | 4      | float  | m_nPosX
| 16     | 4      | float  | m_nPosZ
| 20     | 2      | int16  | m_nPosY
| 22     | 2      | int16  | m_nRotX
| 24     | 2      | int16  | m_nRotY
| 26     | 2      | int16  | m_nRotZ
| 28     | 4      | float  | m_nOldX
| 32     | 4      | float  | m_nOldZ
| 36     | 2      | int16  | m_nOldY
| 38     | 2      | int16  | m_nVelX
| 40     | 2      | int16  | m_nVelY
| 42     | 2      | int16  | m_nVelZ
| 44     | 2      | int16  | m_Param0
| 46     | 2      | int16  | m_Param1

### PktObject ShieldState

| Name                 | Value
|----------------------|------
| SHIELD_STATE_NONE    | 0x0
| SHIELD_STATE_MOVING  | 0x1
| SHIELD_STATE_SUCCESS | 0x2

### PktObject GuardState

| Name                | Value
|---------------------|------
| GUARD_STATE_NONE    | 0x0
| GUARD_STATE_MOVING  | 0x1
| GUARD_STATE_SUCCESS | 0x2

### PktObject BULLET

| Name         | Value
|--------------|------
| MODE_NETWORK | 0x0
| MODE_HOMING  | 0x1

### PktGame HEADER

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0      | 2      | uint16 | m_Phase
| 2      | 2      | uint16 | m_phaseAkn

### PktGame COMMON_DATA

| Offset | Length | Type         | Name
|--------|--------|--------------|-----
| 0      | 4      | HEADER       | m_Header
| 4      | 1      | byte         | m_NpcNum
| 5      | 1      | byte         | m_MemberNum
| 6      | 1      | byte         | fillerb0
| 7      | 1      | byte         | fillerb1
| 8      | 10     | NPC_INFO[5]  | m_NpcInfo
| 18     | 1      | byte         | fillerb2
| 19     | 1      | byte         | fillerb3
| 20     | 48     | RACE_INFO[6] | m_RaceInfo

### PktGame RACE_INFO_BIT

| Postition | Size | Name            |
|-----------|------|-----------------|
| 0         | 17   | __dummy__
| 17        | 1    | m_isRequestCutNetwork
| 18        | 1    | m_isGoalFinished
| 19        | 4    | m_GoalRank
| 23        | 4    | m_Rank
| 27        | 5    | m_nPlayerNum

### PktGame RACE_INFO

| Offset | Length | Type          | Name
|--------|--------|---------------|-----
| 0      | 4      | RACE_INFO_BIT | m_InfoBit
| 4      | 4      | float         | m_GoalTime

### PktGame NPC_INFO

| Offset | Length | Type | Name
|--------|--------|------|-----
| 0      | 1      | byte | m_CarID
| 1      | 1      | byte | m_PlayerNum

### PktGame CNetGameRace

| Offset | Length | Type           | Name
|--------|--------|----------------|-----
| 0      | 52     | CTreeTask      | super_CTreeTask
| 52     | 4      | uint32         | m_Step
| 56     | 520    | NETGAME_STATUS | m_NetGameStatus
| 576    | 4      | float          | m_Timer
| 580    | 4      | int32          | m_CurrentCarNum
| 584    | 4      | int32          | m_CurrentMemberNum
| 588    | 4      | int32          | m_TimeOutStep
| 592    | 4      | float          | m_StartUpOverTimer
| 596    | 4      | cGOnlineWait*  | m_pGOnlineWait
| 600    | 4      | bool           | m_isDispOnlineWait
| 604    | 4      | int32          | m_EventReadyStep
| 608    | 4      | float          | m_EventReadyTimer

### PktGame SyncPhaseState

| Name                      | Value
|---------------------------|------
| SYNC_PHASE_STATE_WAITING  | 0x0
| SYNC_PHASE_STATE_DONE     | 0x1
| SYNC_PHASE_STATE_TIME_OUT | 0x1

___

## NetGameHistoryWork

ToDO

___

## NetGameHistoryWork

ToDO

___

## NetGameHandleManager

ToDO

___

## NetGameDef

### PKT_WOOLGIMMICK_DATA

| Offset | Length | Type                        | Name
|--------|--------|-----------------------------|-----
| 0      | 4      | int32                       | iNum
| 4      | 56     | PKT_ONE_WOOLGIMMICK_DATA[2] | woolGimmickData

### PKT_SELECT_DATA

| Offset | Length | Type         | Name
|--------|--------|--------------|-----
| 0      | 4      | __HANDLE__   | m_hNetGameHandle
| 4      | 1      | byte         | m_nState
| 5      | 1      | byte         | m_nSubState
| 6      | 1      | byte         | m_nPlayerNum
| 7      | 1      | byte         | m_nChara
| 8      | 1      | byte         | m_nWeaponID
| 9      | 1      | byte         | m_nTeam
| 10     | 1      | byte         | m_nDeckType
| 11     | 1      | byte         | m_nField
| 12     | 12     | PLAYER_SCORE | m_nScore
| 24     | 1      | byte         | m_nResult
| 25     | 1      | byte         | m_nArenaLv
| 26     | 1      | byte         | m_nLv
| 27     | 1      | byte         | m_nRound
| 28     | 1      | byte         | m_nWorkState
| 29     | 1      | byte         | m_nWorkFlag
| 30     | 1      | byte         | m_nLostFlag
| 31     | 1      | byte:4       | m_nCheck9
| 31     | 1      | byte:4       | m_nCheck8
| 32     | 1      | byte:4       | m_nCheck7
| 32     | 1      | byte:4       | m_nCheck6
| 33     | 1      | byte:4       | m_nCheck5
| 33     | 1      | byte:4       | m_nCheck4
| 34     | 1      | byte:4       | m_nCheck3
| 34     | 1      | byte:4       | m_nCheck2
| 35     | 1      | byte:4       | m_nCheck1
| 35     | 1      | byte:4       | m_nCheck0
| 36     | 1      | byte         | m_dum1
| 37     | 1      | byte         | m_dum0
| 38     | 64     | COMMAND[8]   | m_cmdDeck
| 102    | 8      | COMMAND      | m_cmdFinish
| 110    | 8      | COMMAND      | m_cmdShootLock
| 118    | 8      | COMMAND      | m_cmdJump
| 126    | 8      | COMMAND      | m_cmdGlide
| 134    | 8      | COMMAND      | m_cmdAerialDash
| 142    | 8      | COMMAND      | m_cmdGroundDash
| 150    | 8      | COMMAND      | m_cmdDashAbi
| 158    | 8      | COMMAND      | m_cmdAvoidSlide
| 166    | 8      | COMMAND      | m_cmdComboSlide
| 174    | 8      | COMMAND      | m_cmdTurnAbi
| 182    | 8      | COMMAND      | m_cmdGuard
| 190    | 8      | COMMAND      | m_cmdGuardAbi
| 198    | 8      | COMMAND      | m_cmdBlowAbi
| 206    | 1      | undefined    |
| 207    | 1      | undefined    |

### PKT_PLAYER_DATA

| Offset | Length | Type         | Name
|--------|--------|--------------|-----
| 0      | 4      | __HANDLE__   | m_hNetGameHandle
| 4      | 1      | byte         | m_nState
| 5      | 1      | byte         | m_nSubState
| 6      | 1      | byte         | m_nPlayerNum
| 7      | 1      | byte         | m_nChara
| 8      | 1      | byte         | m_nWeaponID
| 9      | 1      | byte         | m_nTeam
| 10     | 1      | byte         | m_nDeckType
| 11     | 1      | byte         | m_nField
| 12     | 12     | PLAYER_SCORE | m_nScore
| 24     | 1      | byte         | m_nResult
| 25     | 1      | byte         | m_nArenaLv
| 26     | 1      | byte         | m_nLv
| 27     | 1      | byte         | m_nRound
| 28     | 1      | byte         | m_nWorkState
| 29     | 1      | byte         | m_nWorkFlag
| 30     | 1      | byte         | m_nLostFlag
| 31     | 1      | byte:4       | m_nCheck9
| 31     | 1      | byte:4       | m_nCheck8
| 32     | 1      | byte:4       | m_nCheck7
| 32     | 1      | byte:4       | m_nCheck6
| 33     | 1      | byte:4       | m_nCheck5
| 33     | 1      | byte:4       | m_nCheck4
| 34     | 1      | byte:4       | m_nCheck3
| 34     | 1      | byte:4       | m_nCheck2
| 35     | 1      | byte:4       | m_nCheck1
| 35     | 1      | byte:4       | m_nCheck0
| 36     | 1      | byte         | m_dum1
| 37     | 1      | byte         | m_dum0
| 38     | 2      | int16        | m_nHp
| 40     | 2      | int16        | m_nHpMax
| 42     | 1      | byte         | m_nAp
| 43     | 1      | byte         | m_nMp
| 44     | 1      | byte         | m_nDp
| 45     | 1      | byte         | m_nWork0
| 46     | 2      | int16        | m_nStateTime
| 48     | 1      | byte         | m_nPlayerState
| 49     | 1      | byte         | m_nSubPlayerState
| 50     | 1      | undefined    | 
| 51     | 1      | undefined    | 
| 52     | 4      | uint32       | m_nPlayerFlag
| 56     | 4      | uint32       | m_nTrgFlag
| 60     | 4      | uint32       | m_nAttackFlag
| 64     | 4      | uint32       | m_nDamageFlag
| 68     | 4      | uint32       | m_nColor
| 72     | 2      | uint16       | m_nCommandKind
| 74     | 1      | byte         | m_nCommandLv
| 75     | 1      | byte         | m_nStyleID
| 76     | 2      | uint16       | m_nReplyKind
| 78     | 1      | byte         | m_nHpHealLight
| 79     | 1      | byte         | m_nShootLockNum
| 80     | 2      | int16        | m_nAnim
| 82     | 2      | int16        | m_nAnimTime
| 84     | 4      | byte[4]      | m_nAtkGrp
| 88     | 2      | int16        | m_nEffGroup
| 90     | 2      | int16        | m_nSEChannel
| 92     | 2      | int16        | m_nPosX
| 94     | 2      | int16        | m_nPosY
| 96     | 2      | int16        | m_nPosZ
| 98     | 2      | int16        | m_nRotX
| 100    | 2      | int16        | m_nRotY
| 102    | 2      | int16        | m_nSclXYZ
| 104    | 2      | int16        | m_nOldX
| 106    | 2      | int16        | m_nOldY
| 108    | 2      | int16        | m_nOldZ
| 110    | 2      | int16        | m_nVelX
| 112    | 2      | int16        | m_nVelY
| 114    | 2      | int16        | m_nVelZ
| 116    | 2      | uint16       | m_nPlayerStateCounter
| 118    | 2      | int16        | m_nAtkPower
| 120    | 2      | int16        | m_nAtkForce
| 122    | 1      | byte         | m_nCryticalRate
| 123    | 1      | byte         | m_AtkAttr
| 124    | 4      | _HANDLE_     | m_hLockonTarget
| 128    | 4      | _HANDLE_     | m_hActionTarget
| 132    | 2      | uint16       | m_nAtkDataId
| 134    | 1      | byte         | m_nBltCount
| 135    | 1      | byte         | m_nBltId
| 136    | 3      | byte[3]      | m_nLoadStyle
| 139    | 1      | byte         | m_nReqStyle
| 140    | 2      | int16        | m_nLmtX
| 142    | 2      | int16        | m_nLmtY
| 144    | 2      | int16        | m_nLmtZ
| 146    | 2      | int16        | m_nLmtR
| 148    | 2      | uint16       | m_nLmtKind
| 150    | 1      | byte         | m_nLmtCreator
| 151    | 1      | byte         | m_nLmtState
| 152    | 1      | byte         | m_nLmtResult
| 153    | 3      | byte[3]      | m_nLmtMember
| 156    | 2      | uint16       | m_nReactionKind
| 158    | 1      | byte         | m_nDeckNum
| 159    | 1      | byte         | m_nAtkGroup
| 160    | 2      | int16        | m_nCntX
| 162    | 2      | int16        | m_nCntY
| 164    | 2      | int16        | m_nCntZ
| 166    | 2      | int16        | m_nCntR
| 168    | 2      | uint16       | m_nAnimSetCount
| 170    | 1      | byte         | m_nTargetPC
| 171    | 1      | byte         | m_nDFinishStep
| 172    | 2      | int16        | m_nCommandStep
| 174    | 1      | byte         | m_nEnemyAttackerPC
| 175    | 1      | byte         | m_nEnemyMurdererPC
| 176    | 2      | int16        | m_nAnimSpd
| 178    | 2      | int16        | m_nIllAnimSpd
| 180    | 2      | int16        | m_nIllHP
| 182    | 1      | byte         | m_nIllGauge
| 183    | 1      | byte         | m_nIllLuaState
| 184    | 1      | byte         | m_nIllAnimId
| 185    | 1      | byte         | m_nIllState
| 186    | 1      | byte         | m_nIllGravity
| 187    | 1      | byte         | m_nIllGroundIn
| 188    | 1      | byte         | m_nIllDamage
| 189    | 1      | undefined    |
| 190    | 2      | uint16       | m_nAtkKind
| 192    | 17     | byte[17]     | m_nPad
| 209    | 1      | undefined    |
| 210    | 1      | undefined    |
| 211    | 1      | undefined    |

