meta:
  id: kh2_dpd
  endian: le
  ks-opaque-types: true
seq:
  - id: magic_code_96
    type: u4

  - id: num_effects_group_list
    type: u4
  - id: off_effects_group_list
    type: effects_group_parent
    repeat: expr
    repeat-expr: num_effects_group_list

  - id: num_textures
    type: u4
  - id: off_textures
    type: u4
    repeat: expr
    repeat-expr: num_textures

  - id: num_tab3
    type: u4
  - id: off_tab3
    type: u4
    repeat: expr
    repeat-expr: num_tab3

  - id: num_tab4
    type: u4
  - id: off_tab4
    type: u4
    repeat: expr
    repeat-expr: num_tab4

  - id: num_tab5
    type: u4
  - id: off_tab5
    type: u4
    repeat: expr
    repeat-expr: num_tab5
types:
  effects_group_parent:
    seq:
      - id: ofs
        type: u4
    instances:
      item:
        pos: ofs
        size-eos: true
        type: effects_group
        
  effects_group:
    seq:
      - id: matrix1
        type: f4
        repeat: expr
        repeat-expr: 16
      - id: matrix2
        type: f4
        repeat: expr
        repeat-expr: 16
      - id: position
        type: f4
        repeat: expr
        repeat-expr: 4
      - id: rotation
        type: f4
        repeat: expr
        repeat-expr: 4
      - id: scaling
        type: f4
        repeat: expr
        repeat-expr: 4
      - id: dummy
        type: f4
        repeat: expr
        repeat-expr: 4
      - id: skip_unk_data
        size: 0x50
      - id: dpd_effect
        type: dpd_effect
        size-eos: true
        
  dpd_effect_command:
    seq:
      - id: command
        type: u4
      - id: param_length
        type: u2
      - id: param_count
        type: u2
      - id: offset_parameters
        type: u4
      - id: offset2
        type: u4
    instances:
      raw1:
        pos: offset_parameters
        size: param_length * param_count
      raw2:
        pos: offset2
        size: 4

  dpd_effect:
    seq:
      - id: unk0
        type: u4
      - id: unk4
        type: u4
      - id: unk8
        type: u4
      - id: unk_c
        type: u4
      - id: offset_next
        type: u4
      - id: unk04
        type: u4
      - id: unk08
        type: u4
      - id: unk0_c
        type: u4
      - id: unk10
        type: u4
      - id: unk14
        type: u4
      - id: unk18
        type: u4
      - id: unk1_c
        type: u4
      - id: unk20
        type: u4
      - id: unk24
        type: u2
      - id: commands_count
        type: u2
      - id: commands
        type: dpd_effect_command
        repeat: expr
        repeat-expr: commands_count
    instances:
      next:
        pos: offset_next
        type: dpd_effect
        if: offset_next != 0
