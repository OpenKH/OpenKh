meta:
  id: kh2_bar
  endian: le
  ks-opaque-types: true
seq:
  - id: magic
    contents: [0x42, 0x41, 0x52, 0x01]
  - id: num_files
    type: s4
  - id: padding
    size: 8
  - id: files
    type: file_entry
    repeat: expr
    repeat-expr: num_files
types:
  file_entry:
    seq:
      - id: type
        type: u2
      - id: duplicate
        type: u2
      - id: name
        type: str
        size: 4
        encoding: UTF-8
      - id: offset
        type: s4
      - id: size
        type: s4
    instances:
      file:
        io: _root._io
        pos: offset
        size: size
        if: size != 0
        type:
          switch-on: type
          cases:
            0x11: kh2_bar
            0x09: kh2_motion
            0x04: kh2_model
            0x0c: kh2_spawn_point
            0x0d: kh2_spawn_script
            0x12: kh2_pax
