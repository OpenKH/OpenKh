## NetGameCockpit

### CGameCockpit

Split enums up under there own categories

| Name       | Value
|------------|------
| STATE_NONE | 0x0
| STATE_IN   | 0x1
| STATE_OFF  | 0x2
| STATE_OUT  | 0x3
| STATE_HIDE | 0x4

| Name          | Value
|---------------|------
| FLAG_DISP_OFF | 0x1
| FLAG_SIDE_OFF | 0x2
| FLAG_RED_ON   | 0x4


| Name      | Value
|-----------|------
| GAUGE_MAX | 0x4

| Offset | Length | Type         | Name
|--------|--------|--------------|-----
| 0x0    | 52     | CTreeTask    | super_CTreeTask
| 0x34   | 4      | int32        | m_nState
| 0x38   | 4      | int32        | m_nMode
| 0x3C   | 4      | _HANDLE_     | m_hL2d
| 0x40   | 4      | _HANDLE_     | m_hSq2
| 0x44   | 4      | _HANDLE_     | m_hLayGame
| 0x48   | 4      | _HANDLE_     | m_hSeqTimer
| 0x4C   | 4      | _HANDLE_     | m_hSeqTotalP
| 0x50   | 4      | _HANDLE_     | m_hSeqTotalE
| 0x54   | 24     | _HANDLE_[6]  | m_hLayPlayer
| 0x6C   | 80     | ISVECTOR[10] | m_uvNumGTF
| 0xBC   | 80     | ISVECTOR[10] | m_uvNumGT0
| 0x10C  | 80     | ISVECTOR[10] | m_uvNumTSF
| 0x15C  | 80     | ISVECTOR[10] | m_uvNumTS0
| 0x1AC  | 80     | ISVECTOR[10] | m_uvNumPSF
| 0x1FC  | 80     | ISVECTOR[10] | m_uvNumPS0
| 0x24C  | 4      | float        | m_fGameTimer
| 0x250  | 4      | int16[2]     | m_nTeamTotal
| 0x254  | 12     | int16[6]     | m_nPlayerScore
| 0x260  | 4      | _HANDLE_     | m_hLaySide
| 0x264  | 4      | _HANDLE_     | m_hLayCenter
| 0x268  | 4      | int32        | m_nRoundDisp
| 0x26C  | 32     | GAUGE[4]     | m_nHpGauge
| 0x28C  | 4      | int32        | m_nHpGaugeNum
| 0x290  | 4      | _HANDLE_     | m_hL2dHealLight
| 0x294  | 4      | _HANDLE_     | m_hLayHealLight
| 0x298  | 16     | _HANDLE_[4]  | m_hSq2FaceData
| 0x2A8  | 4      | _HANDLE_     | m_hLayBonus

#### GAUGE

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0x0    | 2      | int16 | m_nHp
| 0x2    | 2      | int16 | m_nHpMax
| 0x4    | 2      | int16 | m_nRedFr
| 0x6    | 1      | byte  | m_nRedCol
| 0x7    | 1      | byte  | m_nID
