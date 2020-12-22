meta:
  id: kh2_spawn_point
  endian: le
  ks-opaque-types: true
seq:
  - id: type_id
    type: s4
  - id: item_count
    type: s4
instances:
  spawn_point_desc:
    type: spawn_point
    repeat: expr
    repeat-expr: item_count
types:
  spawn_point:
    seq:
      - id: unk00
        type: s2
      - id: unk02
        type: s2
      - id: entity_count
        type: s2
      - id: event_activator_count
        type: s2
      - id: unk08_count
        type: s2
      - id: unk0a_count
        type: s2
      - id: unk0c_count
        type: u4
      - id: unk10
        type: u4
      - id: unk14
        type: u4
      - id: unk18
        type: u4
      - id: place
        type: u1
      - id: door
        type: u1
      - id: world
        type: u1
      - id: unk1f
        type: u1
      - id: unk20
        type: u4
      - id: unk24
        type: u4
      - id: unk28
        type: u4
      - id: entities
        type: entity
        repeat: expr
        repeat-expr: entity_count
      - id: event_activators
        type: event_activator
        repeat: expr
        repeat-expr: event_activator_count
      - id: walk_path
        type: walk_path_desc
        repeat: expr
        repeat-expr: unk08_count
      - id: unknown0a_table
        type: unknown0a
        repeat: expr
        repeat-expr: unk0a_count
      - id: unknown0c_table
        type: unknown0c
        repeat: expr
        repeat-expr: unk0c_count
        
  entity:
    seq:
      - id: object_id
        type: u4
      - id: position_x
        type: f4
      - id: position_y
        type: f4
      - id: position_z
        type: f4
      - id: rotation_x
        type: f4
      - id: rotation_y
        type: f4
      - id: rotation_z
        type: f4
      - id: unk1c
        type: s2
      - id: unk1e
        type: s2
      - id: unk20
        type: u4
      - id: ai_parameter
        type: u4
      - id: talk_message
        type: u4
      - id: reaction_command
        type: u4
      - id: unk30
        type: u4
      - id: unk34
        type: u4
      - id: unk38
        type: u4
      - id: unk3c
        type: u4

  event_activator:
    seq:
      - id: unk00
        type: u4
      - id: position_x
        type: f4
      - id: position_y
        type: f4
      - id: position_z
        type: f4
      - id: scale_x
        type: f4
      - id: scale_y
        type: f4
      - id: scale_z
        type: f4
      - id: rotation_x
        type: f4
      - id: rotation_y
        type: f4
      - id: rotation_z
        type: f4
      - id: unk28
        type: u4
      - id: unk2c
        type: u4
      - id: unk30
        type: u4
      - id: unk34
        type: u4
      - id: unk38
        type: u4
      - id: unk3c
        type: u4

  walk_path_desc:
    seq:
      - id: unk00
        type: s2
      - id: count
        type: s2
      - id: unk04
        type: s2
      - id: unk06
        type: s2
      - id: positions
        type: position
        repeat: expr
        repeat-expr: count

  position:
    seq:
      - id: x
        type: f4
      - id: y
        type: f4
      - id: z
        type: f4
      
  unknown0a:
    seq:
      - id: unk00
        type: u1
      - id: unk01
        type: u1
      - id: unk02
        type: u1
      - id: unk03
        type: u1
      - id: unk04
        type: u1
      - id: unk05
        type: u1
      - id: unk06
        type: s2
      - id: unk08
        type: u4
      - id: unk0c
        type: u4

  unknown0c:
    seq:
      - id: unk00
        type: u4
      - id: unk04
        type: u4
