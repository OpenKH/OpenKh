# [Kingdom Hearts II](../../index.md) - AI

The AI is always located into BAR archives (under the extension of MDLX, MAG, etc.). They are used as a scripting language, compiled into bytecode to comunicate with the game engine.

By courtesy of [GovanifY](https://code.govanify.com/govanify/ghidra-kh2ai), there is a fairly complete documentation of [KH2 AI ISA](kh2ai.md) available. This entire documentation is generated from his repository and it is considered the most up-to-date version.

## File format

```c
struct header {
    char name[16];
    s32 workSize;
    s32 stackSize;
    s32 termSize;
    struct {
        s32 trigger; // key
        s32 entry; // pointer to code
    } triggers[n];
    s32 endTrigger; // 0
    s32 endEntry; // 0
};
```

## Memory spaces

### wp (work memory pointer)

Single uninitialized memory space.

It is like `.bss` [.bss - Wikipedia](https://en.wikipedia.org/wiki/.bss)

```c
char table1[128];
char table2[128];
char table3[128];

void main() {

}
```

### sp (stack memory pointer)

Single stack memory space.

Main usage: local variable storage of local function.

The allocation unit is always 4 bytes: DWORD (double word).

The fixed count of DWORD is allocated by caller, whenever the local function call is occurred.

- `jal` or `jal32` will allocate DWORDs. Also 2 mandatory DWORDs (DWORD count, and ret addr) must be added.
- `ret` will clean it up.

```c
void func() {
    int var1;
}
```

### tp (temp memory pointer)

Single temporary memory space.

Main usage: function arguments, function return value, input/output storage, etc.

The allocation unit is always 4 bytes: DWORD (double word).

Many opcodes (like *push*, *pop*, *unary and binary operators*, *conditional branch operators*, *arithmetic operators*, and so on) use this memory space for input/output purpose.

As representative things, `push*` and `pop*` use this space.

`push*` will write a DWORD to `tp`, and then `tp` is increased by `4`.

`pop*` will read a DWORD from `tp`, and then `tp` is decreased by `4`.

This is used to resolve input/output of function calls of both 2 patterns:

- `syscall` for external function call
- `jal` and `jal32` for local function call

```c
// 2 stack in
// 1 stack out
int func(int a, int b) {
    return a + b;
}

void main() {
    int res = func(123, 456);
}
```

Another usage is arithmetic operation.

```
test10036:
 pushImm 12
 pushImm 3
 div ; 12 ÷ 3 → 4
 pushImm 4
 sub ; 4 - 4 → 0
 jnz fail
 jmp pass
```

- `div` and `sub` will consume 2 DWORDs, and produce 1 DWORD.
- `jnz` will consume 1 DWORD, and produce no DWORD.

### bd

Single `bdx` data memory space starting at 16th byte.

`bd` pointer is always doubled by 2 when it is accessed. So referenced data must be aligned on 2 byte alignment.

`bd` pointer and `pc` (program counter) share same format.

This is used to resolve:

- Having C style string like `char text[] = "hello";`
- Having pre initialized table like `struct { ... };`
- Having some kind of integer array like `int jumpTable[] = {0, ...};`

```c
static char text[] = "hello";

void main() {
    puts(text);
}
```

## Opcode cheat sheet

| code | old | new name | description |
|---|---|---|---|
| 0,0,x | _pushImm_ | push | `push           int32` |
| 0,1,x | _pushImmf_ | push.s | `push           single` |
| 0,2,0 | _pushFromPSp_ | push.sp | `push           sp     + int16` |
| 0,2,1 | _pushFromPWp_ | push.wp | `push           wp     + int16` |
| 0,2,2 | _pushFromPSpVal_ | push.sp.d | `push          *sp     + int16` |
| 0,2,3 | _pushFromPAi_ | push.bd | `push           bd     + int16` |
| 0,3,0 | _pushFromFSp_ | push.d.sp | `push        *( sp     + int16)` |
| 0,3,1 | _pushFromFWp_ | push.d.wp | `push        *( wp     + int16)` |
| 0,3,2 | _pushFromFSpVal_ | push.d.sp.d | `push        *(*sp     + int16)` |
| 0,3,3 | _pushFromFAi_ | push.d.bd | `push        *( bd     + int16)` |
| 1,x,0 | _popToSp_ | pop.sp | `pop         *( sp     + int16)` |
| 1,x,1 | _popToWp_ | pop.wp | `pop         *( wp     + int16)` |
| 1,x,2 | _popToSpVal_ | pop.sp.d | `pop         *(*sp     + int16)` |
| 1,x,3 | _popToAi_ | pop.bd | `pop         *( bd     + int16)` |
| 2,x,0 | _memcpyToSp_ | memcpy.sp | `memcpy to    ( sp     + int16)` |
| 2,x,1 | _memcpyToWp_ | memcpy.wp | `memcpy to    ( wp     + int16)` |
| 2,x,2 | _memcpyToSpVal_ | memcpy.sp.d | `memcpy to    (*sp     + int16)` |
| 2,x,3 | _memcpyToSpAi_ | memcpy.bd | `memcpy to    ( bd     + int16)` |
| 3,x,x | _fetchValue_ | push.d.pop | `push        *( pop()  + int16)` |
| 4,x,x |  | memcpy | generic memcpy |
| 5,0,0 | _citf_ | cvt.w.s | int to float (convert word to single) |
| 5,0,2 |  | neg | 1 to -1, -1 to 1 |
| 5,0,3 | _inv_ | not | bitwise not |
| 5,0,4 | _eqz_ | seqz | set 1 if: equal to 0 |
| 5,0,5 |  | abs | get absolute integer (negative to positive) |
| 5,0,6 | _msb_ | sltz | set 1 if: less than 0 (negative) |
| 5,0,7 | _info_ | slez | set 1 if: less than or equal to 0 (negative and zero) |
| 5,0,8 | _eqz_ | seqz | set 1 if: equal to 0 |
| 5,0,9 | _neqz_ | snez | set 1 if: not equal to 0 |
| 5,0,10 | _msbi_ | sgez | set 1 if: greater than or equal to 0 (zero and positive) |
| 5,0,11 | _ipos_ | sgtz | set 1 if: greater than 0 (positive) |
| 5,1,1 | _cfti_ | cvt.s.w | float to int (convert single to word) |
| 5,1,2 | _negf_ | neg.s | 1.0 to -1.0, -1.0 to 1.0 |
| 5,1,5 | _absf_ | abs.s | get absolute float (negative to positive) |
| 5,1,6 | _infzf_ | sltz.s | set 1 if: less than 0 (negative) |
| 5,1,7 | _infoezf_ | slez.s | set 1 if: less than or equal to 0 (negative and zero) |
| 5,1,8 | _eqzf_ | seqz.s | set 1 if: equal to 0 |
| 5,1,9 | _neqzf_ | snez.s | set 1 if: not equal to 0 |
| 5,1,10 | _supoezf_ | sgez.s | set 1 if: greater than or equal to 0 (zero and positive) |
| 5,1,11 | _supzf_ | sgtz.s | set 1 if: greater than 0 (positive) |
| 6,0,0 |  | add | `push a; push b; (a + b)` |
| 6,0,1 |  | sub | `push a; push b; (a - b)` |
| 6,0,2 |  | mul | `push a; push b; (a * b)` |
| 6,0,3 |  | div | `push a; push b; (a / b)` |
| 6,0,4 |  | mod | `push a; push b; (a % b)` |
| 6,0,5 |  | and | Bitwise and |
| 6,0,6 |  | or | Bitwise or |
| 6,0,7 |  | xor | Bitwise xor |
| 6,0,8 |  | sll | Shift left logical (unsigned) |
| 6,0,9 |  | sra | Shift right arithmetic (signed) |
| 6,0,10 | _eqzv_ | land | `push a; push b; (a && b)` |
| 6,0,11 | _neqzv_ | lor | `push a; push b; (a \|\| b)` |
| 6,1,0 | _addf_ | add.s | `push a; push b; (a + b)` float |
| 6,1,1 | _subf_ | sub.s | `push a; push b; (a - b)` float |
| 6,1,2 | _mulf_ | mul.s | `push a; push b; (a * b)` float |
| 6,1,3 | _divf_ | div.s | `push a; push b; (a + b)` float |
| 6,1,4 | _modf_ | mod.s | `push a; push b; (a % b)` float |
| 7,x,0 | _jmp_ | b | branch unconditionally |
| 7,x,1 | _jz_ | beqz | branch if zero |
| 7,x,2 | _jnz_ | bnez | branch if non-zero |
| 8,x,x | _gosub_ | jal | local function call (16-bit address) |
| 9,x,0 |  | halt | halt |
| 9,x,1 |  | exit | exit |
| 9,x,2 |  | ret | ret |
| 9,x,3 |  | drop | drop one value from `tp` |
| 9,x,5 |  | dup | duplicate one value at `tp` |
| 9,x,6 |  | sin | sin |
| 9,x,7 |  | cos | cos |
| 9,x,8 |  | degr | radian to degree: `value / PI * 180.0` |
| 9,x,9 |  | radd | degree to radian: `value / 180.0 * PI` |
| 10,x,x |  | syscall | syscall |
| 11,x,x | _gosub32_ | jal32 | local function call (32-bit address) |

*Notes*:

- `.d.` is short of `.dereference.`. It is shorten for optimization in case of keyboard typing.
- For example `push.d.sp`, it reads from left to right as English grammar does. A reading example is: *push dereferenced value of (sp + imm16)*

### Reading of comparators

| Comparator | Reading |
|:-:|---|
| EQZ | `x == 0` |
| GEZ | `x >= 0` |
| GTZ | `x > 0` |
| LEZ | `x <= 0` |
| LTZ | `x < 0` |
| NEZ | `x != 0` |
