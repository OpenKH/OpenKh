# CMDCHARGE Format

CmdCharge stands for *Command Charge*.

This file stores the list for all commands melds.

The reason why it's called **CHARGE** is because that's how command melding is named in japanese, Command Charge.

This file is contained within `Menu/Camp.arc`


## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[8]   | File identifier, always `@BINCHRG`.
| 0x8     | uint16  | Version, always `256`.
| 0xA     | uint16  | Size
| 0xC     | uint16  | Data Count
| 0xE     | uint16  | Padding
| 0x10    | uint8[16]  | Ability Chance Array

## Command Charge Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16[2]   | Command Index
| 0x4     | uint8[2]   | Command Level
| 0x6     | uint16   | Result Command 1
| 0x8     | uint8   | Flag 1
| 0x9     | uint8   | Chance Percent 1
| 0xA     | uint16   | Result Command 2
| 0xC     | uint8   | Flag 2
| 0xD     | uint8   | Chance Percent 2
| 0xE     | uint8   | Exception
| 0xF     | uint8   | Recipe

## Command Charge Flag

| Bit | Count  | Description
|--------|-------|------------
| 0     | 1   | Is it rare?
| 1     | 3   | Rank
| 4     | 4   | Ability Pattern