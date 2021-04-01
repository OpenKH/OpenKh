# Weapon Format

It controls parameters related to the weapon's stats.

This data is located with the game's executable.

# Weapon Data Table

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | [WeaponInfo](#Weapon-Info) | Wayward Wind
| 0xC     | [WeaponInfo](#Weapon-Info) | Rainfell
| 0x18    | [WeaponInfo](#Weapon-Info) | Earth Shaker
| 0x24    | [WeaponInfo](#Weapon-Info) | Treasure Trove (Ventus)
| 0x30    | [WeaponInfo](#Weapon-Info) | Treasure Trove (Aqua)
| 0x3C    | [WeaponInfo](#Weapon-Info) | Treasure Trove (Terra)
| 0x48    | [WeaponInfo](#Weapon-Info) | Stroke of Midnight (Ventus)
| 0x54    | [WeaponInfo](#Weapon-Info) | Stroke of Midnight (Aqua)
| 0x60    | [WeaponInfo](#Weapon-Info) | Stroke of Midnight (Terra)
| 0x6C    | [WeaponInfo](#Weapon-Info) | Fairy Stars (Ventus)
| 0x78    | [WeaponInfo](#Weapon-Info) | Fairy Stars (Aqua)
| 0x84    | [WeaponInfo](#Weapon-Info) | Fairy Stars (Terra)
| 0x90    | [WeaponInfo](#Weapon-Info) | Victory Line (Ventus)
| 0x9C    | [WeaponInfo](#Weapon-Info) | Victory Line (Aqua)
| 0xA8    | [WeaponInfo](#Weapon-Info) | Victory Line (Terra)
| 0xB4    | [WeaponInfo](#Weapon-Info) | Mark of a Hero (Ventus)
| 0xC0    | [WeaponInfo](#Weapon-Info) | Mark of a Hero (Aqua)
| 0xCC    | [WeaponInfo](#Weapon-Info) | Mark of a Hero (Terra)
| 0xD8    | [WeaponInfo](#Weapon-Info) | Hypderdrive (Ventus)
| 0xE4    | [WeaponInfo](#Weapon-Info) | Hypderdrive (Aqua)
| 0xF0    | [WeaponInfo](#Weapon-Info) | Hypderdrive (Terra)
| 0xFC    | [WeaponInfo](#Weapon-Info) | Pixie Petal (Ventus)
| 0x108   | [WeaponInfo](#Weapon-Info) | Pixie Petal (Aqua)
| 0x114   | [WeaponInfo](#Weapon-Info) | Pixie Petal (Terra)
| 0x120   | [WeaponInfo](#Weapon-Info) | Ultima Weapon (Ventus)
| 0x12C   | [WeaponInfo](#Weapon-Info) | Ultima Weapon (Aqua)
| 0x138   | [WeaponInfo](#Weapon-Info) | Ultima Weapon (Terra)
| 0x144   | [WeaponInfo](#Weapon-Info) | Sweetstack (Ventus)
| 0x150   | [WeaponInfo](#Weapon-Info) | Sweetstack (Aqua)
| 0x15C   | [WeaponInfo](#Weapon-Info) | Sweetstack (Terra)
| 0x168   | [WeaponInfo](#Weapon-Info) | Light Seeker (Ventus)
| 0x174   | [WeaponInfo](#Weapon-Info) | Lost Memory (Ventus)
| 0x180   | [WeaponInfo](#Weapon-Info) | Unknown Ventus Weapon
| 0x18C   | [WeaponInfo](#Weapon-Info) | Frolic Flame (Ventus)
| 0x198   | [WeaponInfo](#Weapon-Info) | Destiny's Embrace (Aqua)
| 0x1A4   | [WeaponInfo](#Weapon-Info) | Stormfall (Aqua)
| 0x1B0   | [WeaponInfo](#Weapon-Info) | Brightcrest (Aqua)
| 0x1BC   | [WeaponInfo](#Weapon-Info) | Darkgnaw (Terra)
| 0x1C8   | [WeaponInfo](#Weapon-Info) | Ends of Earth (Terra)
| 0x1C4   | [WeaponInfo](#Weapon-Info) | Chaos Ripper (Terra)
| 0x1D0   | [WeaponInfo](#Weapon-Info) | Void Gear (Ventus)
| 0x1DC   | [WeaponInfo](#Weapon-Info) | Void Gear (Aqua)
| 0x1E8   | [WeaponInfo](#Weapon-Info) | Void Gear (Terra)
| 0x1F4   | [WeaponInfo](#Weapon-Info) | No Name (Ventus)
| 0x200   | [WeaponInfo](#Weapon-Info) | No Name (Aqua)
| 0x20C   | [WeaponInfo](#Weapon-Info) | No Name (Terra)
| 0x218   | [WeaponInfo](#Weapon-Info) | Crown Unlimit (Ventus)
| 0x224   | [WeaponInfo](#Weapon-Info) | Crown Unlimit (Aqua)
| 0x230   | [WeaponInfo](#Weapon-Info) | Crown Unlimit (Terra)
| 0x23C   | [WeaponInfo](#Weapon-Info) | Master's Defender (Aqua)
| 0x248   | [WeaponInfo](#Weapon-Info) | Unknown Weapon 19
| 0x254   | [WeaponInfo](#Weapon-Info) | Unknown Weapon 19
| 0x260   | [WeaponInfo](#Weapon-Info) | Unknown Weapon 20
| 0x26C   | [WeaponInfo](#Weapon-Info) | Unknown Weapon 20
| 0x278   | [WeaponInfo](#Weapon-Info) | Unknown Weapon 20
| 0x284   | [WeaponInfo](#Weapon-Info) | Unknown Weapon 21
| 0x290   | [WeaponInfo](#Weapon-Info) | Unknown Weapon 21
| 0x29C   | [WeaponInfo](#Weapon-Info) | Unknown Weapon 21
| 0x2A8   | [WeaponInfo](#Weapon-Info) | Unknown Weapon 22
| 0x2B4   | [WeaponInfo](#Weapon-Info) | Unknown Weapon 22
| 0x2C0   | [WeaponInfo](#Weapon-Info) | Unknown Weapon 22

## Weapon Info

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | int8  | Strength
| 0x1     | int8  | Magic
| 0x2     | uint8 | Critical Rate (Percentage)
| 0x3     | uint8 | Critical Damage (Divided by 100)
| 0x4     | uint8 | Chain?
| 0x5     | int8  | Offset X
| 0x6     | int8  | Offset Y
| 0x7     | int8  | Offset Z
| 0x8     | int8  | Degree Z
| 0x9     | uint8 | Reach
| 0xA     | uint8 | Hit Stars
| 0xB     | uint8 | SCD File ID