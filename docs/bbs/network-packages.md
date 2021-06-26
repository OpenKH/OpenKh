# [Kingdom Hearts Birth By Sleep](index.md) - Network Packages

## Network Def

### PACKET_DATA

| Offset | Length | Type       | Name
|--------|--------|------------|-----
| 0x0    | 68     | PKT_HEADER | m_pktHeader
| 0x44   | 80     | PKT_GAME   | m_pktGame
| 0x94   | 880    | PKT_OBJECT | m_pktObject

### PKT_HEADER

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0x0    | 1      | byte      | m_nUser
| 0x1    | 1      | byte      | m_nCode
| 0x2    | 1      | byte      | [m_nPhase](#PKT_HEADER-Phase) Needs testing
| 0x3    | 1      | byte      | m_nReady
| 0x4    | 1      | byte      | pad0
| 0x5    | 1      | byte      | m_nOpenLv
| 0x6    | 1      | undefined |
| 0x7    | 1      | undefined |
| 0x8    | 4      | uint32    | m_nSendCount
| 0xC    | 4      | uint32    | m_uiUserColor
| 0x10   | 38     | char[38]  | m_szNickName
| 0x36   | 8      | char[8]   | m_szRoomName
| 0x3E   | 1      | undefined |
| 0x3F   | 1      | undefined |
| 0x40   | 4      | float     | m_fUpdate

### PKT_GAME

| Offset | Length | Type    | Name
|--------|--------|---------|-----
| 0x0    | 80     | char[8] | m_nData

### PKT_OBJECT

| Offset | Length | Type    | Name
|--------|--------|---------|-----
| 0x0    | 880    | char[8] | m_nData

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
| 0x0    | 1      | byte | nGameMode
| 0x1    | 1      | byte | nEventRule
| 0x2    | 1      | byte | nDataMode
| 0x3    | 1      | byte | nTimeLimit
| 0x4    | 1      | byte | nMemberMax
| 0x5    | 1      | byte | nField
| 0x6    | 1      | byte | nTarget
| 0x7    | 1      | byte | nRank

### GAME_INFO

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0x0    | 4      | int32 | m_iServerFlag
| 0x4    | 4      | int32 | m_iClientReady
| 0x8    | 4      | int32 | m_iServerIndex

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
| 0x0       | 1    | UiError         |
| 0x1       | 1    | UiOnlineWait    |
| 0x2       | 1    | UiExterminate   |
| 0x3       | 6    | __dummy__       |
| 0x9       | 1    | uiDataSetupAll  |
| 0xA       | 1    | uiDataSetup     |
| 0xB       | 1    | uiStartCheck    |
| 0xC       | 1    | uiReadyAll      |
| 0xD       | 1    | uiReady         |
| 0xE       | 1    | uiExitStart     |
| 0xF       | 1    | uiPCamAdvertise |
| 0x10      | 1    | uiRoundStart    |
| 0x11      | 1    | uiLeaderLive    |
| 0x12      | 1    | uiTeamError     |
| 0x13      | 1    | uiResultWindow  |
| 0x14      | 1    | uiMVP           |
| 0x15      | 1    | uiLose          |
| 0x16      | 1    | uiWin           |
| 0x17      | 1    | uiDecideAll     |
| 0x18      | 1    | uiDecide        |
| 0x19      | 1    | uiExit          |
| 0x1A      | 1    | uiSelectWindow  |
| 0x1B      | 1    | uiTotalScore    |
| 0x1C      | 1    | uiDataFree      |
| 0x1D      | 1    | uiTeamBattle    |
| 0x1E      | 1    | uiLeader        |
| 0x1F      | 1    | uiOnline        |

### PLAYER_INFO

| Offset | Length | Type     | Name
|--------|--------|----------|-----
| 0x0    | 38     | char[38] | m_szName
| 0x26   | 1      | byte     | m_nDeck
| 0x27   | 1      | byte     | m_nColor
| 0x28   | 1      | byte     | m_nTeam
| 0x29   | 1      | byte     | m_nDecide
| 0x2A   | 1      | byte     | m_nRank
| 0x2B   | 1      | byte     | m_nWin
| 0x2C   | 1      | byte     | m_nLose
| 0x2D   | 1      | byte     | m_nMVP
| 0x2E   | 1      | byte     | m_nLiveInfo
| 0x2F   | 1      | byte     | m_dum0
| 0x30   | 4      | uint32   | m_nScore
| 0x34   | 4      | uint32   | m_nScoreR
| 0x38   | 4      | uint32   | m_nTotal
| 0x3C   | 2      | uint16   | m_nMedal
| 0x3E   | 2      | uint16   | m_nBonusP
| 0x40   | 2      | uint16   | m_nBonusM
| 0x42   | 1      | byte     | m_dum1
| 0x43   | 1      | byte     | m_dum2

### PLAYER_SCORE

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0x0    | 4      | uint32    | m_nScore
| 0x4    | 2      | uint16    | m_nMedal
| 0x6    | 2      | uint16    | m_nBonusP
| 0x8    | 2      | uint16    | m_nBonusM
| 0xA    | 1      | undefined | 
| 0xB    | 1      | undefined | 

### TEAM_INFO

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0x0    | 4      | uint32    | m_nScore
| 0x4    | 4      | uint32    | m_nScoreR
| 0x8    | 2      | uint16    | m_nBonusP
| 0xA    | 2      | uint16    | m_nBonusM
| 0xC    | 1      | byte      | m_nWin
| 0xD    | 1      | byte      | m_nLose
| 0xE    | 1      | byte      | m_nRank
| 0xF    | 1      | byte      | m_nMemberCount
| 0x10   | 6      | byte[6]   | m_nMember
| 0x16   | 1      | undefined |
| 0x17   | 1      | undefined |

### TEAM_SCORE

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0x0    | 4      | uint32    | m_nScore
| 0x4    | 1      | byte      | m_nWin
| 0x5    | 1      | byte      | m_nLose
| 0x6    | 1      | undefined | 
| 0x7    | 1      | undefined | 

### CNetGameVSMode

| Offset | Length | Type             | Name
|--------|--------|------------------|-----
| 0x0    | 52     | CTreeTask        | super_CTreeTask
| 0x34   | 4      | IAllocator*      | m_pAllocator
| 0x38   | 4      | VSMODE_FLAG_DATA | m_FlagData
| 0x3C   | 4      | float            | m_fTimer
| 0x40   | 4      | float            | m_fLocalTimer
| 0x44   | 4      | float            | m_fGamerTimer
| 0x48   | 1      | byte             | m_nState
| 0x49   | 1      | byte             | m_nPreState
| 0x4A   | 1      | byte             | m_nStateEntry
| 0x4B   | 1      | byte             | m_nStateSelect
| 0x4C   | 1      | byte             | m_nStateLoad
| 0x4D   | 1      | byte             | m_nStateGame
| 0x4E   | 1      | byte             | m_nStateResult
| 0x4F   | 1      | byte             | m_nStateAlert
| 0x50   | 10     | byte[10]         | m_nCheck
| 0x5A   | 2      | uint16           | m_nCheckFlag
| 0x5C   | 1      | byte             | m_nCheckCurrent
| 0x5D   | 1      | byte             | m_dum0
| 0x5E   | 1      | byte             | m_dum1
| 0x5F   | 1      | byte             | m_dum2
| 0x60   | 4      | CGameCockpit*    | m_pGameCockpit
| 0x64   | 4      | CGOnlineWait*    | m_pOnlineWait
| 0x68   | 6      | byte[6]          | m_nLiveInfo
| 0x6E   | 1      | byte             | m_nRecvMax
| 0x6F   | 1      | byte             | m_nRecvOnline
| 0x70   | 1      | byte             | m_nRecvLost
| 0x71   | 1      | byte             | m_nLeaderId
| 0x72   | 1      | byte             | m_nDataSetupOK
| 0x73   | 1      | byte             | m_nStartMember
| 0x74   | 1      | byte             | m_nPacketCode
| 0x75   | 1      | byte             | m_nDropCount
| 0x76   | 1      | byte             | m_nMemberReady
| 0x77   | 1      | byte             | m_inGameMode
| 0x78   | 1      | byte             | m_inEventRule
| 0x79   | 1      | byte             | m_inDataMode
| 0x7A   | 1      | byte             | m_inTimeLimit
| 0x7B   | 1      | byte             | m_inMemberMax
| 0x7C   | 1      | byte             | m_inField
| 0x7D   | 1      | byte             | m_outGameMode
| 0x7E   | 1      | byte             | m_outEventRule
| 0x7F   | 1      | byte             | m_outDataMode
| 0x80   | 1      | byte             | m_outTimeLimit
| 0x81   | 1      | byte             | m_outMemberMax
| 0x82   | 1      | byte             | m_outField
| 0x83   | 1      | byte             | m_nMVP
| 0x84   | 1      | byte             | m_nTeam
| 0x85   | 1      | byte             | m_nDeckType
| 0x86   | 1      | byte             | m_nPlayerNum
| 0x87   | 1      | byte             | m_nMember
| 0x88   | 1      | byte             | m_nMemberLost
| 0x89   | 1      | byte             | m_nMemberOK
| 0x8A   | 1      | byte             | m_nResult
| 0x8B   | 1      | undefined        | 
| 0x8C   | 72     | PLAYER_SCORE[6]  | m_PlayerScore
| 0xD4   | 16     | TEAM_SCORE[2]    | m_TeamScore
| 0xE4   | 408    | PLAYER_INFO[6]   | m_PlayerInfo
| 0x27C  | 48     | TEAM_INFO[6]     | m_TeamInfo

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
| 0x0    | 1      | sbyte              | m_nPlayerNum
| 0x1    | 1      | byte               | m_CurrentBestPlayerCarLap
| 0x2    | 1      | byte               | fillerb0
| 0x3    | 1      | byte               | fillerb1
| 0x4    | 4      | float              | m_GameTimer
| 0x8    | 104    | CAR                | m_Player
| 0x70   | 580    | NPC[5]             | m_Npc
| 0x2B4  | 64     | GIMMICK_TORNADO[4] | m_GimmickTornado
| 0x2F4  | 32     | GIMMICK_SHIELD[4]  | m_GimmickShield
| 0x314  | 6      | byte[6]            | m_isRequestCutNetwork
| 0x31A  | 1      | byte               | filler0
| 0x31B  | 1      | byte               | filler1
| 0x31C  | 8      | GIMMICK_PISTON     | m_GimmickPiston

### PktObject Object

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 8      | HEADER | m_Header
| 0x8    | 4      | GAME   | m_Game
| 0xC    | 804    | RACE   | m_RaceData

### PktObject Header

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 2      | uint16 | m_Phase
| 0x2    | 1      | byte   | m_isEnable
| 0x3    | 1      | byte   | m_CharaID
| 0x4    | 2      | uint16 | m_AllMemberPhaseRecive
| 0x6    | 1      | byte   | filler0
| 0x7    | 1      | byte   | filler1

### PktObject Game

| Offset | Length | Type | Name
|--------|--------|------|-----
| 0x0    | 1      | byte | fillerb0
| 0x1    | 1      | byte | fillerb1
| 0x2    | 1      | byte | fillerb2
| 0x3    | 1      | byte | fillerb3

### PktObject Rider

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0x0    | 2      | int16 | m_nPosX
| 0x2    | 2      | int16 | m_nPosZ
| 0x4    | 2      | int16 | m_nPosY
| 0x6    | 2      | int16 | m_nRotX
| 0x8    | 2      | int16 | m_nRotY
| 0xA    | 2      | int16 | m_nRotZ

### PktObject GIMMICK

| Name                | Value
|---------------------|------
| GIMMICK_TORNADO_MAX | 0x4
| GIMMICK_SHIELD_MAX  | 0x4

### PktObject NPC

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0x0    | 104    | CAR   | m_Car
| 0x68   | 12     | RIDER | m_Rider

### PktObject GIMMICK_TORNADO

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0x0    | 1      | byte  | m_UniqueID
| 0x1    | 1      | byte  | m_MoveDirection
| 0x2    | 2      | int16 | m_nPosY
| 0x4    | 4      | float | m_nPosX
| 0x8    | 4      | float | m_nPosZ
| 0xC    | 2      | int16 | m_nVelX
| 0xE    | 2      | int16 | m_nVelZ

### PktObject GIMMICK_SHIELD

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 1      | byte   | m_UniqueID
| 0x1    | 1      | byte   | filler0
| 0x2    | 1      | byte   | filler1
| 0x3    | 1      | byte   | filler2
| 0x4    | 4      | uint32 | m_DisappearCounter

### PktObject GIMMICK_PISTON

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 2      | uint16 | m_nAnimTime
| 0x2    | 1      | byte   | filler00
| 0x3    | 1      | byte   | filler01
| 0x4    | 4      | float  | m_MotSpeed

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
| 31        | 1    | m_isRiderAction

### PktObject CAR

| Offset | Length | Type    | Name
|--------|--------|---------|-----
| 0x0    | 48     | BULLET  | m_Bullet
| 0x30   | 1      | byte    | m_nAnimNo
| 0x31   | 1      | byte    | m_LockOnPlayerNum
| 0x32   | 1      | byte    | m_DangerPlayerNum
| 0x33   | 1      | byte    | m_State
| 0x34   | 4      | CAR_BIT | m_CarBit
| 0x38   | 1      | byte    | m_DarkStateCounter
| 0x39   | 1      | sbyte   | m_CurrentLap
| 0x3A   | 1      | byte    | filler1
| 0x3B   | 1      | byte    | filler2
| 0x3C   | 4      | float   | m_nPosX
| 0x40   | 4      | float   | m_nPosZ
| 0x44   | 2      | int16   | m_nPosY
| 0x46   | 2      | int16   | m_nRotX
| 0x48   | 2      | int16   | m_nRotY
| 0x4A   | 2      | int16   | m_nRotZ
| 0x4C   | 4      | float   | m_nOldX
| 0x50   | 4      | float   | m_nOldZ
| 0x54   | 2      | int16   | m_nOldY
| 0x56   | 2      | int16   | m_nVelX
| 0x58   | 2      | int16   | m_nVelY
| 0x5A   | 2      | int16   | m_nVelZ
| 0x5C   | 2      | uint16  | m_nAnimTime
| 0x5E   | 1      | ubyte   | m_CurrentPathIndex
| 0x5F   | 1      | ubyte   | m_CurrentPathCount
| 0x60   | 1      | ubyte   | m_CurrentPathDataNo
| 0x61   | 1      | ubyte   | m_CurrentBranchTargetPathIndex
| 0x62   | 1      | byte    | filler00
| 0x63   | 1      | byte    | filler01
| 0x64   | 4      | float   | m_CheckDangerAfterPlayerLength

### PktObject BULLET

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 1      | byte   | m_isExist
| 0x1    | 1      | byte   | m_DangerPlayerNum
| 0x2    | 2      | uint16 | m_BulletID
| 0x4    | 1      | byte   | m_State
| 0x5    | 1      | byte   | m_Mode
| 0x6    | 1      | byte   | m_fillerb0
| 0x7    | 1      | byte   | m_fillerb1
| 0x8    | 4      | float  | m_TimbeID
| 0xC    | 4      | float  | m_nPosX
| 0x10   | 4      | float  | m_nPosZ
| 0x14   | 2      | int16  | m_nPosY
| 0x16   | 2      | int16  | m_nRotX
| 0x18   | 2      | int16  | m_nRotY
| 0x1A   | 2      | int16  | m_nRotZ
| 0x1C   | 4      | float  | m_nOldX
| 0x20   | 4      | float  | m_nOldZ
| 0x24   | 2      | int16  | m_nOldY
| 0x26   | 2      | int16  | m_nVelX
| 0x28   | 2      | int16  | m_nVelY
| 0x2A   | 2      | int16  | m_nVelZ
| 0x2C   | 2      | int16  | m_Param0
| 0x2E   | 2      | int16  | m_Param1

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
| 0x0    | 2      | uint16 | m_Phase
| 0x2    | 2      | uint16 | m_phaseAkn

### PktGame COMMON_DATA

| Offset | Length | Type         | Name
|--------|--------|--------------|-----
| 0x0    | 4      | HEADER       | m_Header
| 0x4    | 1      | byte         | m_NpcNum
| 0x5    | 1      | byte         | m_MemberNum
| 0x6    | 1      | byte         | fillerb0
| 0x7    | 1      | byte         | fillerb1
| 0x8    | 10     | NPC_INFO[5]  | m_NpcInfo
| 0x12   | 1      | byte         | fillerb2
| 0x13   | 1      | byte         | fillerb3
| 0x14   | 48     | RACE_INFO[6] | m_RaceInfo

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
| 0x0    | 4      | RACE_INFO_BIT | m_InfoBit
| 0x4    | 4      | float         | m_GoalTime

### PktGame NPC_INFO

| Offset | Length | Type | Name
|--------|--------|------|-----
| 0x0    | 1      | byte | m_CarID
| 0x1    | 1      | byte | m_PlayerNum

### PktGame CNetGameRace

| Offset | Length | Type           | Name
|--------|--------|----------------|-----
| 0x0    | 52     | CTreeTask      | super_CTreeTask
| 0x34   | 4      | uint32         | m_Step
| 0x38   | 520    | NETGAME_STATUS | m_NetGameStatus
| 0x240  | 4      | float          | m_Timer
| 0x244  | 4      | int32          | m_CurrentCarNum
| 0x248  | 4      | int32          | m_CurrentMemberNum
| 0x24C  | 4      | int32          | m_TimeOutStep
| 0x250  | 4      | float          | m_StartUpOverTimer
| 0x254  | 4      | cGOnlineWait*  | m_pGOnlineWait
| 0x258  | 4      | bool           | m_isDispOnlineWait
| 0x25C  | 4      | int32          | m_EventReadyStep
| 0x260  | 4      | float          | m_EventReadyTimer

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

## NetGameHistory

ToDO

___

## NetGameHandleManager

ToDO

___

## NetGameDef

### PKT_WOOLGIMMICK_DATA

| Offset | Length | Type                        | Name
|--------|--------|-----------------------------|-----
| 0x0    | 4      | int32                       | iNum
| 0x4    | 56     | PKT_ONE_WOOLGIMMICK_DATA[2] | woolGimmickData

### PKT_SELECT_DATA

| Offset | Length | Type         | Name
|--------|--------|--------------|-----
| 0x0    | 4      | __HANDLE__   | m_hNetGameHandle
| 0x4    | 1      | byte         | m_nState
| 0x5    | 1      | byte         | m_nSubState
| 0x6    | 1      | byte         | m_nPlayerNum
| 0x7    | 1      | byte         | m_nChara
| 0x8    | 1      | byte         | m_nWeaponID
| 0x9    | 1      | byte         | m_nTeam
| 0xA    | 1      | byte         | m_nDeckType
| 0xB    | 1      | byte         | m_nField
| 0xC    | 12     | PLAYER_SCORE | m_nScore
| 0x18   | 1      | byte         | m_nResult
| 0x19   | 1      | byte         | m_nArenaLv
| 0x1A   | 1      | byte         | m_nLv
| 0x1B   | 1      | byte         | m_nRound
| 0x1C   | 1      | byte         | m_nWorkState
| 0x1D   | 1      | byte         | m_nWorkFlag
| 0x1E   | 1      | byte         | m_nLostFlag
| 0x1F   | 1      | byte:4       | m_nCheck9
| 0x1F   | 1      | byte:4       | m_nCheck8
| 0x20   | 1      | byte:4       | m_nCheck7
| 0x20   | 1      | byte:4       | m_nCheck6
| 0x21   | 1      | byte:4       | m_nCheck5
| 0x21   | 1      | byte:4       | m_nCheck4
| 0x22   | 1      | byte:4       | m_nCheck3
| 0x22   | 1      | byte:4       | m_nCheck2
| 0x23   | 1      | byte:4       | m_nCheck1
| 0x23   | 1      | byte:4       | m_nCheck0
| 0x24   | 1      | byte         | m_dum1
| 0x25   | 1      | byte         | m_dum0
| 0x26   | 64     | COMMAND[8]   | m_cmdDeck
| 0x66   | 8      | COMMAND      | m_cmdFinish
| 0x6E   | 8      | COMMAND      | m_cmdShootLock
| 0x76   | 8      | COMMAND      | m_cmdJump
| 0x7E   | 8      | COMMAND      | m_cmdGlide
| 0x86   | 8      | COMMAND      | m_cmdAerialDash
| 0x8E   | 8      | COMMAND      | m_cmdGroundDash
| 0x96   | 8      | COMMAND      | m_cmdDashAbi
| 0x9E   | 8      | COMMAND      | m_cmdAvoidSlide
| 0xA6   | 8      | COMMAND      | m_cmdComboSlide
| 0xAE   | 8      | COMMAND      | m_cmdTurnAbi
| 0xB6   | 8      | COMMAND      | m_cmdGuard
| 0xBE   | 8      | COMMAND      | m_cmdGuardAbi
| 0xC6   | 8      | COMMAND      | m_cmdBlowAbi
| 0xCE   | 1      | undefined    |
| 0xCF   | 1      | undefined    |

### PKT_PLAYER_DATA

| Offset | Length | Type         | Name
|--------|--------|--------------|-----
| 0x0    | 4      | __HANDLE__   | m_hNetGameHandle
| 0x4    | 1      | byte         | m_nState
| 0x5    | 1      | byte         | m_nSubState
| 0x6    | 1      | byte         | m_nPlayerNum
| 0x7    | 1      | byte         | m_nChara
| 0x8    | 1      | byte         | m_nWeaponID
| 0x9    | 1      | byte         | m_nTeam
| 0xA    | 1      | byte         | m_nDeckType
| 0xB    | 1      | byte         | m_nField
| 0xC    | 12     | PLAYER_SCORE | m_nScore
| 0x18   | 1      | byte         | m_nResult
| 0x19   | 1      | byte         | m_nArenaLv
| 0x1A   | 1      | byte         | m_nLv
| 0x1B   | 1      | byte         | m_nRound
| 0x1C   | 1      | byte         | m_nWorkState
| 0x1D   | 1      | byte         | m_nWorkFlag
| 0x1E   | 1      | byte         | m_nLostFlag
| 0x1F   | 1      | byte:4       | m_nCheck9
| 0x1F   | 1      | byte:4       | m_nCheck8
| 0x20   | 1      | byte:4       | m_nCheck7
| 0x20   | 1      | byte:4       | m_nCheck6
| 0x21   | 1      | byte:4       | m_nCheck5
| 0x21   | 1      | byte:4       | m_nCheck4
| 0x22   | 1      | byte:4       | m_nCheck3
| 0x22   | 1      | byte:4       | m_nCheck2
| 0x23   | 1      | byte:4       | m_nCheck1
| 0x23   | 1      | byte:4       | m_nCheck0
| 0x24   | 1      | byte         | m_dum1
| 0x25   | 1      | byte         | m_dum0
| 0x26   | 2      | int16        | m_nHp
| 0x28   | 2      | int16        | m_nHpMax
| 0x2A   | 1      | byte         | m_nAp
| 0x2B   | 1      | byte         | m_nMp
| 0x2C   | 1      | byte         | m_nDp
| 0x2D   | 1      | byte         | m_nWork0
| 0x2E   | 2      | int16        | m_nStateTime
| 0x30   | 1      | byte         | m_nPlayerState
| 0x31   | 1      | byte         | m_nSubPlayerState
| 0x32   | 1      | undefined    | 
| 0x33   | 1      | undefined    | 
| 0x34   | 4      | uint32       | m_nPlayerFlag
| 0x38   | 4      | uint32       | m_nTrgFlag
| 0x3C   | 4      | uint32       | m_nAttackFlag
| 0x40   | 4      | uint32       | m_nDamageFlag
| 0x44   | 4      | uint32       | m_nColor
| 0x48   | 2      | uint16       | m_nCommandKind
| 0x4A   | 1      | byte         | m_nCommandLv
| 0x4B   | 1      | byte         | m_nStyleID
| 0x4C   | 2      | uint16       | m_nReplyKind
| 0x4E   | 1      | byte         | m_nHpHealLight
| 0x4F   | 1      | byte         | m_nShootLockNum
| 0x50   | 2      | int16        | m_nAnim
| 0x52   | 2      | int16        | m_nAnimTime
| 0x54   | 4      | byte[4]      | m_nAtkGrp
| 0x58   | 2      | int16        | m_nEffGroup
| 0x5A   | 2      | int16        | m_nSEChannel
| 0x5C   | 2      | int16        | m_nPosX
| 0x5E   | 2      | int16        | m_nPosY
| 0x60   | 2      | int16        | m_nPosZ
| 0x62   | 2      | int16        | m_nRotX
| 0x64   | 2      | int16        | m_nRotY
| 0x66   | 2      | int16        | m_nSclXYZ
| 0x68   | 2      | int16        | m_nOldX
| 0x6A   | 2      | int16        | m_nOldY
| 0x6C   | 2      | int16        | m_nOldZ
| 0x6E   | 2      | int16        | m_nVelX
| 0x70   | 2      | int16        | m_nVelY
| 0x72   | 2      | int16        | m_nVelZ
| 0x74   | 2      | uint16       | m_nPlayerStateCounter
| 0x76   | 2      | int16        | m_nAtkPower
| 0x78   | 2      | int16        | m_nAtkForce
| 0x7A   | 1      | byte         | m_nCryticalRate
| 0x7B   | 1      | byte         | m_AtkAttr
| 0x7C   | 4      | _HANDLE_     | m_hLockonTarget
| 0x80   | 4      | _HANDLE_     | m_hActionTarget
| 0x84   | 2      | uint16       | m_nAtkDataId
| 0x86   | 1      | byte         | m_nBltCount
| 0x87   | 1      | byte         | m_nBltId
| 0x88   | 3      | byte[3]      | m_nLoadStyle
| 0x8B   | 1      | byte         | m_nReqStyle
| 0x8C   | 2      | int16        | m_nLmtX
| 0x8E   | 2      | int16        | m_nLmtY
| 0x90   | 2      | int16        | m_nLmtZ
| 0x92   | 2      | int16        | m_nLmtR
| 0x94   | 2      | uint16       | m_nLmtKind
| 0x96   | 1      | byte         | m_nLmtCreator
| 0x97   | 1      | byte         | m_nLmtState
| 0x98   | 1      | byte         | m_nLmtResult
| 0x99   | 3      | byte[3]      | m_nLmtMember
| 0x9C   | 2      | uint16       | m_nReactionKind
| 0x9E   | 1      | byte         | m_nDeckNum
| 0x9F   | 1      | byte         | m_nAtkGroup
| 0xA0   | 2      | int16        | m_nCntX
| 0xA2   | 2      | int16        | m_nCntY
| 0xA4   | 2      | int16        | m_nCntZ
| 0xA6   | 2      | int16        | m_nCntR
| 0xA8   | 2      | uint16       | m_nAnimSetCount
| 0xAA   | 1      | byte         | m_nTargetPC
| 0xAB   | 1      | byte         | m_nDFinishStep
| 0xAC   | 2      | int16        | m_nCommandStep
| 0xAE   | 1      | byte         | m_nEnemyAttackerPC
| 0xAF   | 1      | byte         | m_nEnemyMurdererPC
| 0xB0   | 2      | int16        | m_nAnimSpd
| 0xB2   | 2      | int16        | m_nIllAnimSpd
| 0xB4   | 2      | int16        | m_nIllHP
| 0xB6   | 1      | byte         | m_nIllGauge
| 0xB7   | 1      | byte         | m_nIllLuaState
| 0xB8   | 1      | byte         | m_nIllAnimId
| 0xB9   | 1      | byte         | m_nIllState
| 0xBA   | 1      | byte         | m_nIllGravity
| 0xBB   | 1      | byte         | m_nIllGroundIn
| 0xBC   | 1      | byte         | m_nIllDamage
| 0xBD   | 1      | undefined    |
| 0xBE   | 2      | uint16       | m_nAtkKind
| 0xC0   | 17     | byte[17]     | m_nPad
| 0xD1   | 1      | undefined    |
| 0xD2   | 1      | undefined    |
| 0xD3   | 1      | undefined    |

