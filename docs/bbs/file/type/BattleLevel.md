# Battle Level Format

It controls the world level increases applied to each world depending on the flag.

This file is located inside `parag_xx.arc` where `xx` are the initial of the character name. The internal file is named `btl_lv_xx-0.bin` where xx means the same as explained before.

This file has no header. Data just starts until a code and world count of 0 is found.

## Battle Level Section Header

Each world flag data consists of sections like this until it reaches the end of the flag definition which is a World Code and World Count of 0.

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16  | [Code](#Battle-Level-Code)
| 0x2     | uint16 | World Count

## Battle Level Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint16 | World ID
| 0x2    | uint16 | World Levels to Increase