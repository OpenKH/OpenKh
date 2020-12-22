meta:
  id: kh2_spawn_script
  endian: le
seq:
  - id: program
    type: spawn_program
    repeat: until
    repeat-until: _.id == -1
types:
  spawn_program:
    seq:
      - id: id
        type: s2
      - id: length
        type: s2
      - id: byte_code
        size: length - 4
        if: id != -1
