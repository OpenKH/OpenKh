# [Kingdom Hearts II](../../index.md) - 12soundinfo.bar

This file is responsible for defining various environmental sound effects related to maps in [Kingdom Hearts II](../../index.md)
It is a [BAR](bar.md) file and each subfile represents a different [World](/docs/kh2/worlds.md).

### Structure

| Amount | Description |
|--------|---------------|
| 1 	   | Sound Info Entry Count
| Count  | Sound Info Entries

### Sound Info Entry
| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint16 | Reverb
| 2 	 | uint16 | 3DRate
| 4 	 | uint16 | WAV for Environment
| 6 	 | uint16 | SEB for Environment
| 8 	 | uint16 | NUMBER for Environment
| 10 	 | uint16 | SPOT for Environment
| 12 	 | uint16 | WAV for Footstep
| 14 	 | uint16 | SORA Footstep Sound Container
| 16 	 | uint16 | DONALD Footstep Sound Container
| 18 	 | uint16 | GOOFY Footstep Sound Container
| 20 	 | uint16 | WORLD_FRIEND Footstep Sound Container
| 22 	 | uint16 | OTHER Footstep Sound Container


