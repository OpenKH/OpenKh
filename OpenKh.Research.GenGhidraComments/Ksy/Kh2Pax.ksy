meta:
  id: kh2_pax
  endian: le
  ks-opaque-types: true
seq:
  - id: magic
    type: u4
  - id: offset_name
    type: u4
  - id: entries_count
    type: u4
  - id: offset_dpx
    type: u4
  - id: pax_ents
    type: pax_entry
    repeat: expr
    repeat-expr: entries_count
instances:
  name:
    pos: offset_name
    size: 128
  dpx:
    pos: offset_dpx
    size-eos: true
    type: kh2_dpx
types:
  pax_entry:
    seq:
      - id: effect
        type: u2
      - id: caster
        type: u2
      - id: unk04
        type: u2
      - id: unk06
        type: u2
      - id: unk08
        type: u4
      - id: unk0c
        type: u4
      - id: unk10
        type: u4
      - id: unk14
        type: u4
      - id: sound_effect
        type: u4
      - id: pos_x
        type: f4
      - id: pos_z
        type: f4
      - id: pos_y
        type: f4
      - id: rot_x
        type: f4
      - id: rot_z
        type: f4
      - id: rot_y
        type: f4
      - id: scale_x
        type: f4
      - id: scale_z
        type: f4
      - id: scale_y
        type: f4
      - id: unk40
        type: u4
      - id: unk44
        type: u4
      - id: unk48
        type: u4
      - id: unk4c
        type: u4
