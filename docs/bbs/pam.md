# Kingdom Hearts Birth by Sleep - PAM (Animation Container)

### PAM Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 00 | char[4] | The identifier of the file (always 0x004D4150) |
| 04 | uint32_t | Animation count|
| 08 | byte[6] | Always zero. Padding, never read by the game.|
| 0E | uint16_t  | Unknown. Seems to always be 0x0001. The game only checks if it's less than 1 and crashes if it is.|

### PAM Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 00 | uint32_t | Offset to animation, alignment doesn't matter.|
| 04 | char[12] | Animation name.|

## Animations
Animations in Birth by Sleep do not need to be aligned in any way.

### Animation Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 00 | uint16_t  | Animation Type, currently unknown, but determines how the data inside the animation is read.|
| 02 | byte | Animation frame rate, always 0x1E, or 30 FPS.|
| 03 | byte | Interpolation frame count, determines how many frames it takes to transition into this animation.|
| 04 | uint16_t | Loop from frame, determines the point the animation loops from, usually the last frame.|
| 06 | byte | Bone count, must match bone count of target model.|
| 08 | uint16_t | Animation frame count.|
| 0A | uint16_t | Loop to frame, determines the point the animation loops to.|
