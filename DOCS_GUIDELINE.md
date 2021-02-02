# Documentation guideline

## General guidelines

* Always use `hexadecimal` when defining offsets. They can be defined as follows. 
  * With `0x` before the value: `0x1234`
  * With `h` after the value: `1234h`

* Although `hexadecimal` is preferred, `decimal` can be used for the rest of values.

---

## Representing single variables

Here's a list of the recommended basic variable types. Size is in *bits*.

| Size | Signed | Unsigned
|------|--------|----------
| 8  |  int8  |  uint8  
| 16 |  int16 |  uint16
| 32 |  int32 |  uint32
| 32 |  float |  -
| 64 |  int64 |  uint64
| 64 |  double |  -

If the variable contains actual text, `char` is preferred to `uint8` or `int8`.

Arrays must be represented with the **[]** symbol. For example `int8[4]` means 4 int8 variables.
The number of items in an array must be defined in `decimal` unless it is much more convenient to read in `hexadecimal`.

Here are a few other special types you should take in account:

| Size | Name | 
|------|--------|
| 64 |  Vector2f  |   
| 96 |  Vector3f |  
| 128 |  Vector4f |  
| 512 |  Matrix4x4 |  

---

## Representing bitfields

Bitfields always have to have their relative position and field size expressed in **decimal**.

| Position | Size | Description
|------|--------|----------
|   0  |  1  | Field 1
|   1  |  7  | Field 2
|   8  |  8  | Field 3
|   16  |  8  | Field 4
|   24  |  8  | Field 5
---
## Representing structs & enums

In the case of **structs** simply feature the relative position in **hexadecimal** (as mentioned in [general guidelines](#General-Guideline)), the [*Type*](#Representing-single-variables) and a description of what the field holds or what the game does with it. More collumns can be added if necessary.

| Position | Type | Description
|------|--------|----------
|   0x0  |  char[4]  | Name
|   0x4  |  uint16  | Attack
|   0x6  |  float  | Jump Scale

If it's an **enum**, just create a table with the **value** of each element and its **name**. **Value** should always be in **decimal**.

| Value | Name
|--------|----------
|  0  | None
|  1  | Attack
|  2  | Magic
|  3  | Friend
|  4  | Skill
