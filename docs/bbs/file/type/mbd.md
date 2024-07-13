# MBD Format

MBD stands for *Mirage Bonus Data* and it contains the list rewards you can get from completing Mirage Arena challenges.


# Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | Code, always `@MBD`
| 0x4     | uint32    | Version, always `1`
| 0x8     | uint32[2]    | RESERVED

# Bonus Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | [BattleType](#battle-type)    | 
| 0x4    | [ChallengeType](#challenge-type) | 
| 0x8    | uint32            | Requirement value
| 0xC    | uint32            | Reward multiplier (for medals)

## Battle Type

| Value | Name  | Description
|--------|-------|------------
| 0x0    | BATTLE_TYPE_JUDGE   | Day of Reckoning
| 0x1    | BATTLE_TYPE_CURSE   | Wheels of Misfortune
| 0x2    | BATTLE_TYPE_TREASURE   | Risky Riches
| 0x3    | BATTLE_TYPE_RUN   | Weaver Fever
| 0x4    | BATTLE_TYPE_PRISON   | Sinister Sentinel
| 0x5    | BATTLE_TYPE_DAZZLE   | Dead Ringer
| 0x6    | BATTLE_TYPE_TYRANT   | Combined Threat
| 0x7    | BATTLE_TYPE_DESIRE   | Treasure Tussle
| 0x8    | BATTLE_TYPE_CRIME   | Harsh Punishment
| 0x9    | BATTLE_TYPE_BLIZZARD   | A Time to Chill
| 0xA    | BATTLE_TYPE_MAGICIAN   | Copycat Crisis
| 0xB    | BATTLE_TYPE_ARENA_OWNER   | Keepers of the Arena
| 0xC    | BATTLE_TYPE_WHALE   | Monster of the Sea
| 0xD    | BATTLE_TYPE_CONQUEROR   | Villains' Vendetta
| 0xE    | BATTLE_TYPE_LIGHT_MASTER   | Light's Lessions
| 0xF    | BATTLE_TYPE_DARKNESS   | Peering into Darkness
| 0x10   | BATTLE_TYPE_END   | 

## Challenge Type

| Value | Name  | Description
|--------|-------|------------
| 0x0    | CHALLENGE_TYPE_CURE   | Heal X times or less.
| 0x1    | CHALLENGE_TYPE_TIME_ATTACK   | Within time limit. (Time in seconds)
| 0x2    | CHALLENGE_TYPE_GET_MUNNY   | Obtain X amount of munny.
| 0x3    | CHALLENGE_TYPE_CHANGE_STYLE   | Perform X amount of style changes.
| 0x4    | CHALLENGE_TYPE_SUCCESS_GUARD   | Block X amount of times.