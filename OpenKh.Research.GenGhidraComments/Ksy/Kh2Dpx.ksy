meta:
  id: kh2_dpx
  endian: le
  ks-opaque-types: true
seq:
  - id: magic_code_82
    type: u4
  - id: unk04
    type: u4
  - id: unk08
    type: u4
  - id: dpx_entries
    type: u4
  - id: dpx_ents
    type: dpx_entry
    repeat: expr
    repeat-expr: dpx_entries

types:
  dpx_entry:
    seq:
      - id: dpd_offset
        type: u4
      - id: index
        type: u4
      - id: id
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
    instances:
      dpd:
        pos: dpd_offset
        size-eos: true
        type: kh2_dpd
