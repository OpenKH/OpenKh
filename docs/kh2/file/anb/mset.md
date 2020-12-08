# [Kingdom Hearts II](../../index.md) - MSET (Motion Set)

An MSET file is a collection of [moves](anb.md), which contains animations and effects (I-frames, hitboxes, effects...).
Internally it is a [BAR file](../type/bar.md) that contains a list of "slots". Each slot may or may not have a "motion". The slot is the Nth entry, starting from 0, of a BAR entry. Keep in mind that the BAR's tag is ignored by the motion set system.
The various actions in the game point to a specific motion by a slot position in this list, thus many of the motions are dummies, since the characters only use some of them. Please refer to [slot names](#motion-id-table) to know more.

## Slot system

There is a very specific way on how a [Motion ID](#motion-id-table) is mapped to a specific slot inside a MSET. The mapping is defined by the MSET type, found in the [BAR Header](../type/bar.md#bar-header):

| MSET Type | Description
|-----------|-------------
| 0         | Default
| 1         | Player
| 2         | RAW

`Default` and `RAW` will map the motion ID table to a slot in order, meaning that when the game wants to load `RUN` it will load the 2nd slot in the MSET file. Although for `Player` it will act very differently. To understand it, it is necessary to know how the gameplay works for a player (used by playable characters and party members). A player can have four states:

| In battle | Has weapon | Relative slot index
|-----------|------------|---------------------
| true      | true       | 0
| true      | false      | 1
| false     | false      | 2
| false     | true       | 3

The relative slot index will be added to the Motion ID multipled by 4: `relativeSlotIndex + motionId * 4`. Meaning that if the game wants to load `RUN` for a player that is not in battle but it is holding a weapon, the slot index will be 11 (`3 + 2 * 4`). But a player not always have a specific animation for every state and motion combination. They are called `DUMM` and they always have a file length of `0`. The game have a very clever fallback mechanic. One clear example is the motion `JUMP`, that always fall back to `18` regardles the state. This is the following fallback system, where each number is the relative slot index and the symbol `->` is the fallback when a dummy slot is found:

* `3` -> `2` -> `0` -> `1`
* `2` -> `3` -> `1` -> `0`
* `1` -> `0` -> `2` -> `3`
* `0` -> `1` -> `3` -> `2`

If after those four attempts no slot with an actual animation is found, the game will internally return the slot index `-1`, that will T-pose the model rather than just crashing the game.

## Motion ID table

The following table, extracted from `dbg/motion.bin` is not compelte as it seems to not be updated with the Final Mix version of the game. For instance, `209` is the Roll aniamtion, but it's not present in this list. The list is used by the game's [debug menu](../../../remasters/15plus25/demo_debug.md).

| Index | Name | Description
|-------|------|-------------
| 0   | IDLE |
| 1   | WALK |
| 2   | RUN |
| 3   | JUMP |
| 4   | FALL |
| 5   | LAND |
| 6   | LINK_IDLE |
| 7   | HANG |
| 8   | HANG_UP |
| 9   | ITEM |
| 10  | DAMAGE_S_FRONT_HIGH |
| 11  | DAMAGE_S_FRONT_LOW |
| 12  | DAMAGE_S_BACK_HIGH |
| 13  | DAMAGE_S_BACK_LOW |
| 14  | DAMAGE_AIR_FRONT |
| 15  | DAMAGE_AIR_BACK |
| 16  | DAMAGE_BLOW_FRONT |
| 17  | DAMAGE_BLOW_FRONT_LAND |
| 18  | DAMAGE_BLOW_BACK |
| 19  | DAMAGE_BLOW_BACK_LAND |
| 20  | DAMAGE_TORNADO_FRONT |
| 21  | DAMAGE_TORNADO_BACK |
| 22  | DAMAGE_TORNADO |
| 23  | DAMAGE_LARGE_FRONT |
| 24  | DAMAGE_LARGE_BACK |
| 25  | DAMAGE_RESERVE_3 |
| 26  | DAMAGE_RESERVE_4 |
| 27  | DAMAGE_RESERVE_5 |
| 28  | DAMAGE_RESERVE_6 |
| 29  | DAMAGE_RESERVE_7 |
| 30  | DAMAGE_01 |
| 31  | DAMAGE_02 |
| 32  | DAMAGE_03 |
| 33  | DAMAGE_04 |
| 34  | DAMAGE_05 |
| 35  | DAMAGE_06 |
| 36  | DAMAGE_07 |
| 37  | DAMAGE_08 |
| 38  | DAMAGE_09 |
| 39  | DAMAGE_10 |
| 40  | DAMAGE_BLOW_RECOV |
| 41  | REFLECT |
| 42  | AUTOGUARD |
| 43  | REFLECT_AIR |
| 44  | APPEAR |
| 45  | LEAVE |
| 46  | LEAVE_AIR |
| 47  | FOOTWORK |
| 48  | TURN_L |
| 49  | TURN_R |
| 50  | TALK |
| 51  | TALK_END |
| 52  | CHANGEFORM |
| 53  | CHANGEEND |
| 54  | STUN_LOOP |
| 55  | STUN_END |
| 56  | MAGIC_FIRE |
| 57  | MAGIC_FIRE_FINISH |
| 58  | MAGIC_FIRE_AIR |
| 59  | MAGIC_BLIZZARD |
| 60  | MAGIC_BLIZZARD_FINISH |
| 61  | MAGIC_BLIZZARD_AIR |
| 62  | MAGIC_THUNDER |
| 63  | MAGIC_THUNDER_FINISH |
| 64  | MAGIC_THUNDER_AIR |
| 65  | MAGIC_CURE |
| 66  | MAGIC_CURE_FINISH |
| 67  | MAGIC_CURE_AIR |
| 68  | MAGIC_MAGNET |
| 69  | MAGIC_MAGNET_FINISH |
| 70  | MAGIC_MAGNET_AIR |
| 71  | MAGIC_REFLECT |
| 72  | MAGIC_REFLECT_FINISH |
| 73  | MAGIC_REFLECT_AIR |
| 74  | RTN_TURN |
| 75  | CALL_SUMMON_END |
| 76  | DEAD_LAND |
| 77  | DEAD_AIR |
| 78  | MEMO_IDLE |
| 79  | SUBMENU_IDLE |
| 80  | SUBMENU_ACTION1 |
| 81  | SUBMENU_ACTION2 |
| 82  | CALL_FRIEND |
| 83  | CALL_SUMMON |
| 84  | MENU_IDLE |
| 85  | REFLECT00 |
| 86  | REFLECT01 |
| 87  | REFLECT02 |
| 88  | REFLECT03 |
| 89  | REFLECT04 |
| 90  | REFLECT05 |
| 91  | REFLECT06 |
| 92  | REFLECT07 |
| 93  | REFLECT08 |
| 94  | REFLECT09 |
| 95  | REFLECT10 |
| 96  | EX000 |
| 97  | EX001 |
| 98  | EX002 |
| 99  | EX003 |
| 100 | EX004 |
| 101 | EX005 |
| 102 | EX006 |
| 103 | EX007 |
| 104 | EX008 |
| 105 | EX009 |
| 106 | EX010 |
| 107 | EX011 |
| 108 | EX012 |
| 109 | EX013 |
| 110 | EX014 |
| 111 | EX015 |
| 112 | EX016 |
| 113 | EX017 |
| 114 | EX018 |
| 115 | EX019 |
| 116 | EX020 |
| 117 | EX021 |
| 118 | EX022 |
| 119 | EX023 |
| 120 | EX024 |
| 121 | EX025 |
| 122 | EX026 |
| 123 | EX027 |
| 124 | EX028 |
| 125 | EX029 |
| 126 | EX030 |
| 127 | EX031 |
| 128 | EX032 |
| 129 | EX033 |
| 130 | EX034 |
| 131 | EX035 |
| 132 | EX036 |
| 133 | EX037 |
| 134 | EX038 |
| 135 | EX039 |
| 136 | EX040 |
| 137 | EX041 |
| 138 | EX042 |
| 139 | EX043 |
| 140 | EX044 |
| 141 | EX045 |
| 142 | EX046 |
| 143 | EX047 |
| 144 | EX048 |
| 145 | EX049 |
| 146 | EX050 |
| 147 | EX051 |
| 148 | EX052 |
| 149 | EX053 |
| 150 | EX054 |
| 151 | EX055 |
| 152 | EX056 |
| 153 | EX057 |
| 154 | EX058 |
| 155 | EX059 |
| 156 | EX060 |
| 157 | EX061 |
| 158 | EX062 |
| 159 | EX063 |
| 160 | EX064 |
| 161 | EX065 |
| 162 | EX066 |
| 163 | EX067 |
| 164 | EX068 |
| 165 | EX069 |
| 166 | EX070 |
| 167 | EX071 |
| 168 | EX072 |
| 169 | EX073 |
| 170 | EX074 |
| 171 | EX075 |
| 172 | EX076 |
| 173 | EX077 |
| 174 | EX078 |
| 175 | EX079 |
| 176 | EX080 |
| 177 | EX081 |
| 178 | EX082 |
| 179 | EX083 |
| 180 | EX084 |
| 181 | EX085 |
| 182 | EX086 |
| 183 | EX087 |
| 184 | EX088 |
| 185 | EX089 |
| 186 | EX090 |
| 187 | EX091 |
| 188 | EX092 |
| 189 | EX093 |
| 190 | EX094 |
| 191 | EX095 |
| 192 | EX096 |
| 193 | EX097 |
| 194 | EX098 |
| 195 | EX099 |
| 196 | EX100 |
| 197 | RTN_00 |
| 198 | RTN_01 |
| 199 | RTN_02 |
| 200 | RTN_03 |
| 201 | RTN_04 |
| 202 | RTN_05 |
| 203 | RTN_06 |
| 204 | RTN_07 |
| 205 | RTN_08 |
| 206 | RTN_09 |
