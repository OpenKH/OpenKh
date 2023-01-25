# [Kingdom Hearts II](../../index.md) - Interpolated motion

This documentation is based on the hard work of [Kenjiuno's MSET docs](https://gitlab.com/kenjiuno/msetDoc), data from decompilations and animation research.

Interpolated motions are a sub-category of a [motion file](anb.md#motion-data), found inside ANB files.

The purpose of this file is to animate a 3D model. Models are animated using Straight Ahead Animation. That means it uses an initial pose and then it applies [Transformations](#channels) (Scale, rotate, translate) to specific bones at specific times to make the animation.

Since the animation is interpolated, the game engine can seamlessly change the running animation to another animation by interpolating the movement, creating fluidity. Also it is possible to scale the animation at `n` FPS. Those two advantages are not present in [RAW animations](anb.md#motion-data).

Also, a motion file can define additional bones, in addition to the bones from the model: those are used by the Inverse Kinematic. For the animation `P_EX100\A000` (battle idle), those IK bones are parents of the feet's bones. That is used to both animate the legs movement in a realistic way and to move the leg up when it performs a collision to the stairs, for instance.

# Structure

The file consists of the following structures:

* Motion Header
* Interpolated Motion Header
* Initial Pose: The initial pose of the animation.
* FCurves: The curves that defines a bone's motion (Scale, rotate, translate) through time. Forward curves first, followed by Inverse.
* Keys: The keys define how the curve is modified at certain points of time.
* Key times
* Key values
* Key tangents
* Constraints: Links a bone to another in a way (Follow position, orientation, direction...)
* Constraint activations
* Constraint limiters
* Expressions: Applies a transformation to a target bone. These transformations are not direct and use an expression / formula instead.
* Expression nodes
* Inverse Kinematic Helpers: Additional "bones" to help with Inverse Kinematic animation.
* Joints
* Root Position: The position of the root throughout the motion.
* External Effectors: Unknown. Possibly to aid on interaction with other objects.

## Motion header

This is a continuation of the [motion header](anb.md#motion-header). The very top header is exactly the same as the RAW motions, but the rest of the structure is compeltely different.

![architectural diagram of an inteprolated motion file](./images/motion-architectural-diagram.png)

## Interpolated motion header

Located straight after the motion header, it defines where all the structures are phyisically located inside the files. All the offsets uses `0x90` as origin. Some of the tables have no `count`, but only their `offset`. It is speculated that the game engine do not need to know their amount because there is no need to loop through them: other tables will reference them through an index.

| Offset | Type | Description
|--------|------|--------------
| 0x00   | short| Bone count. Must match with the bone count of the model
| 0x02   | short| Total bone count. This is Bone Count + IK Bone Count
| 0x04   | int  | Total frame count. How many frames the animation contains
| 0x08   | int  | [IK helpers](#inverse-kinematic-helper) offset
| 0x0C   | int  | [Joint indices](#joints) offset
| 0x10   | int  | [Key times](#key-times) count
| 0x14   | int  | [Initial pose](#initial-pose) offset
| 0x18   | int  | [Initial pose](#initial-pose) count
| 0x1C   | int  | [Root Position](#root-position) offset
| 0x20   | int  | [F-Curves Forward](#f-curves) offset
| 0x24   | int  | [F-Curves Forward](#f-curves) count
| 0x28   | int  | [F-Curves Inverse](#f-curves) offset
| 0x2C   | int  | [F-Curves Inverse](#f-curves) count
| 0x30   | int  | [F-Curve Keys](#f-curve-keys) offset
| 0x34   | int  | [Key Times](#key-times) offset
| 0x38   | int  | [Key Values](#key-values) offset
| 0x3C   | int  | [Key Tangents](#key-tangents) offset
| 0x40   | int  | [Constraints](#constraints) offset
| 0x44   | int  | [Constraints](#constraints) count
| 0x48   | int  | [Constraint Activations](#inverse-kinematic-chain) offset
| 0x4C   | int  | [Limiters](#limiters) offset
| 0x50   | int  | [Expressions](#expressions) offset
| 0x54   | int  | [Expressions](#expressions) count
| 0x58   | int  | [Expression Nodes](#expression-nodes) offset
| 0x5C   | int  | [Expression Nodes](#expression-nodes) count
| 0x60   | vec4f | Bounding Box minimum
| 0x70   | vec4f | Bounding Box maximum
| 0x80   | float | Frame start
| 0x84   | float | Frame end
| 0x88   | float | Frames per second, defines the LFR
| 0x8c   | float | Frame return, expressed in LFR
| 0x90   | int  | [External Effectors](#external-effectors) offset
| 0x94   | int[3] | Reserved

## Initial pose

It is responsible to set the model into the initial pose.

| Offset | Type | Description
|--------|------|--------------
| 0x00   | short| Bone index
| 0x02   | short| [Channel](#channels) ID
| 0x04   | float| Value to assign to the channel

## F-Curves

Specifies how a specific bone should animate over time. The curve is modified by a series of keys. [This Blender document](https://docs.unity3d.com/Manual/animeditor-AnimationCurves.html#:~:text=An%20Animation%20Curve%20has%20multiple,the%20keyframes%20are%20called%20inbetweens.) helps visualize how the curves and their keys work.

| Offset | Type | Description
|--------|------|--------------
| 0x00   | short| Bone / IK Helper index
| 0x02   | 4-bit| [Channel](#channels) ID
| 0x02   | 2-bit| [Pre-type](#cycle-type)
| 0x02   | 2-bit| [Post-type](#cycle-type)
| 0x03   | byte | [Key](#timeline) count
| 0x04   | short| [Key](#timeline) start index

The table for the Forward Kinematic animation (skeleton) is followed by the table for the Inverse Kinematic animation (IK Helpers), which has the same structure. The IK Helper index starts from 0.

### Cycle type

| Channel | Description
|---------|---------------
| 0       | First / Last key
| 1       | Subtractive / Additive
| 2       | Repeat
| 3       | Zero

## F-Curve Keys

The keys that modify the F-Curves at specific times.

| Offset | Type  | Description
|--------|-------|--------------
| 0x00   | 2-bit | [Interpolation type](#interpolation-types)
| 0x00   | 14-bit| [Time](#key-times) index
| 0x02   | short | [Value](#key-values) index
| 0x04   | short | [Tangent](#key-tangents) index for ease-in (left)
| 0x06   | short | [Tangent](#key-tangents) index for ease-out (right)

### Interpolation types

| Value | Description
|-------|---------------
| 0     | Constant
| 1     | Linear
| 2     | Hermite

## Key Times

List of floats to define at which time a key applies. Measured in frames. Note that there can be decimal values, hence they apply between frames.
## Key Values

List of floats used as values for the keys' transformations.

## Key Tangents

List of floats used as values for the keys' tangents.

## Constraints

Constraints a bone to another in a way (Follow position, orientation, direction...). Usually, the model bones connect to are ankles or hands as those are what it's most likely to use IK.

| Offset | Type | Description
|--------|------|--------------
| 0x00   | byte | [Constraint type](#constraint-type) index
| 0x01   | byte | Temporary active flag
| 0x02   | short| Constrained joint index
| 0x04   | short| Source joint index
| 0x06   | short| [Limiter](#limiters) index; -1 when not used
| 0x08   | short| [Activation](#constraint-activations) count
| 0x0A   | short| [Activation](#constraint-activations) start index

Joint indexes refers to the bones followed by the IK helpers.

### Constraint type

| Channel | Description
|---------|---------------
| 0  | Position
| 1  | Path
| 2  | Orientation
| 3  | Direction
| 4  | Up Vector
| 5  | Two Points
| 6  | Scale
| 7  | Camera
| 8  | Camera Path
| 9  | Int Path
| 10 | Int
| 11 | Camera Up Vector
| 12 | Position Limit
| 13 | Rotation Limit

## Constraint Activations

Activations used in constraints. Use not known.

| Offset | Type  | Description
|--------|-------|--------------
| 0x00   | float | Time
| 0x04   | int   | Active

## Constraint Limiters

Defines multiple limits for the constraint.

| Offset | Type  | Description
|--------|-------|--------------
| 0x00   | 3b    | type
| 0x00   | 1b    | global
| 0x00   | 1b    | x min
| 0x00   | 1b    | x max
| 0x00   | 1b    | y min
| 0x00   | 1b    | y max
| 0x00   | 1b    | z min
| 0x00   | 1b    | z max
| 0x00   | 22b   | Reserved
| 0x04   | int   | Padding
| 0x08   | float | Damping Width
| 0x0A   | float | Damping Strength
| 0x0E   | vec4f | Min 
| 0x1E   | vec4f | Max 

## Expressions

Applies a transformation to a target bone. These transformations are not direct and use an expression / formula instead.

| Offset | Type  | Description
|--------|-------|--------------
| 0x00   | short | Joint index
| 0x02   | short | [Channel](#channels) ID
| 0x04   | short | Reserved
| 0x06   | short | [Node](#expression-nodes) index

The joint always applies to an IK Helper.

## Expression Nodes

Nodes used by expressions. It seems to be a graph, where every node can point to multiple nodes.

| Offset | Type  | Description
|--------|-------|--------------
| 0x00   | byte  | [Type](#expressoin-type) index
| 0x01   | byte  | isGlobal (1bit + 7 reserved)
| 0x02   | short | Element (-1 when unused)
| 0x04   | float | Value
| 0x08   | short | CAR (Node index or -1 when unused)
| 0x0a   | short | CDR (Node index or -1 when unused)

### Expression type
| Index | Description
|---------|---------------
| 0 | FUNC_SIN
| 1 | FUNC_COS
| 2 | FUNC_TAN
| 3 | FUNC_ASIN
| 4 | FUNC_ACOS
| 5 | FUNC_ATAN
| 6 | FUNC_LOG
| 7 | FUNC_EXP
| 8 | FUNC_ABS
| 9 | FUNC_POW
| 10 | FUNC_SQRT
| 11 | FUNC_MIN
| 12 | FUNC_MAX
| 13 | FUNC_AV
| 14 | FUNC_COND
| 15 | FUNC_AT_FRAME
| 16 | FUNC_CTR_DIST
| 17 | FUNC_FMOD
| 18 | OP_PLUS
| 19 | OP_MINUS
| 20 | OP_MUL
| 21 | OP_DIV
| 22 | OP_MOD
| 23 | OP_EQ
| 24 | OP_GT
| 25 | OP_GE
| 26 | OP_LT
| 27 | OP_LE
| 28 | OP_AND
| 29 | OP_OR
| 30 | VARIABLE_FC
| 31 | CONSTANT_NUM
| 32 | FCURVE_ETRNX
| 33 | FCURVE_ETRNY
| 34 | FCURVE_ETRNZ
| 35 | FCURVE_ROTX
| 36 | FCURVE_ROTY
| 37 | FCURVE_ROTZ
| 38 | FCURVE_SCALX
| 39 | FCURVE_SCALY
| 40 | FCURVE_SCALZ
| 41 | LIST
| 42 | ELEMENT_NAME
| 43 | FUNC_AT_FRAME_ROT
| -1 | EXPR_UNKNOWN

## Inverse Kinematic helper

Creates additional "joints", in a very similar way the bones or a skeleton are structured. Please see [Constraints](#constraints) to know more. Its strucutre is exactly the same as the one found on [MDLX's models](../model.md).

| Offset | Type | Description
|--------|------|--------------
| 0x00   | short | Bone index
| 0x02   | short | Sibling bone index
| 0x04   | short | Parent bone index (-1 if root)
| 0x06   | short | Child bone index (-1 if root)
| 0x08   | int   | Reserved
| 0x0C   | int   | Flags (10b unknown, 19b reserved, 1b enableBias, 1b below, 1b terminate)
| 0x10   | float | Scale X
| 0x14   | float | Scale Y
| 0x18   | float | Scale Z
| 0x1C   | float | Scale W
| 0x20   | float | Rotate X
| 0x24   | float | Rotate Y
| 0x28   | float | Rotate Z
| 0x2C   | float | Rotate W
| 0x30   | float | Translate X
| 0x34   | float | Translate Y
| 0x38   | float | Translate Z
| 0x3C   | float | Translate W

## Joint definition

The list of bones + IK Helpers. The order is not clear, but bones are in order on IK Helpers appear in the list based on both their IK parent and constrained bone. If we structure it as a graph it seems to be stored as pre-order traversal. The order is important as the game engine calculate first the matrix of both Model's skeleton root and IK helper root.

| Offset | Type  | Description
|--------|-------|--------------
| 0x00   | short | Joint index
| 0x02   | 2b    | ik
| 0x02   | 1b    | translation
| 0x02   | 1b    | rotation
| 0x02   | 1b    | fixed
| 0x02   | 1b    | calculated
| 0x02   | 1b    | calculated matrix 2 rotation
| 0x02   | 1b    | external effector
| 0x03   | byte  | Reserved

## Root Position

Defines the position of the object's root as well as it's movement through time.

| Offset | Type   | Description
|--------|--------|--------------
| 0x00   | vec4f  | Scale
| 0x10   | vec4f  | Rotation
| 0x20   | vec4f  | Translation
| 0x30   | int[9] | [FCurve Forward](#f-curves) index (-1 if unused)

## External Effectors

Unknown. Possibly to aid on interaction with other objects. They point to an IK Helper.

| Offset | Type   | Description
|--------|--------|--------------
| 0x00   | short  | Joint index

# Common Enum
### Channels

| Channel | Description
|---------|---------------
| 0       | Modify `scale.x`
| 1       | Modify `scale.y`
| 2       | Modify `scale.z`
| 3       | Modify `rotate.x`
| 4       | Modify `rotate.y`
| 5       | Modify `rotate.z`
| 6       | Modify `translate.x`
| 7       | Modify `translate.y`
| 8       | Modify `translate.z`
| -1      | Unknown (Fail-safe)

