# Command Param Format

It controls parameters related to command stats.

This data is located with the game's executable.

## Command Parameters

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | int16  | Buy Price
| 0x2     | uint8  | Sell Price
| 0x3     | uint8  | Reload Time (seconds)
| 0x4     | uint8  | Gauge Fill (out of 100)
| 0x5     | uint8  | Maximum Level
| 0x6     | int16  | CP for Base Level
| 0x8     | int16  | Level CP increase
| 0xA     | int16[10]  | Command Power (Per level) **

** This list can be used for special purposes, mainly for non-equippable commands, that will be listed down here per command.

Heal Strike:
- LV1 Forward Dash Damage
- LV2 Spin Strike Damage
- LV6 Heal Amount

Random End:
- LV5 Unknown (4)
- LV6 % Status Ailment
- LV7 Unknown (5)

Surprise:
- LV1 Hit Strength
- LV2 % Stun
- LV3 % Mini
- LV4 % Confu
- LV5 % Poison
- LV6 % Stop
- LV7 Number of prizes

Air Arts 4:
- LV6 Stun % Chance

Magical Pulse 4:
- LV1 Hit 1 Strength
- LV2 Hit 2 Strength
- LV3 Hit 3 Strength
- LV6 Ailment % Hit 1
- LV7 Ailment % Hit 2
- LV8 Ailment % Hit 3

Black Star 2:
- LV1 Keyblade Rise Strength
- LV2 Wave Strength
- LV3 Meteorite Strength
- LV5 Amount of Meteorite (3)
- LV6 Blind&Stun % Chance
- LV10 (100)

Explosion:
- LV1 Damage Multiplier
- LV5 Launch Wait Time
- LV6 Stun % chance
- LV10 Unknown (100)

Ice Burst:
- LV1 Weapon Strike Strength
- LV2 Hit 1 Strength
- LV3 Hit 2 Strength
- LV4 Hit 3 Strength

Celebration:
- LV6 Probability of Prizes appearing
- LV7 Munny prize count
- LV8 HP prize count

Celestial:
- LV1 First 2 Hits
- LV2 Final Hit

Teleport Blast:
- LV1 Unknown (100)
- LV2 Sphere Strength
- LV5 Time It Waits Before Canceling (FPS * Value)

Demolition:
- LV1 Strength
- LV4 Demolition Counts
- LV5 Demolition Wait Time (Between Uses) (FPS * Value)