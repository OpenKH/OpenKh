meta:
  id: kh2_motion
  endian: le
  ks-opaque-types: true
seq:
  - id: empty
    size: 0x90
  - id: version
    type: u4
  - id: unk04
    type: u4
  - id: byte_count
    type: u4
  - id: unk0c
    type: u4
  - id: motion
    size: byte_count - 16
    type:
      switch-on: version
      cases:
        0: interpolated

types:
  interpolated:
    seq:
      - id: bone_count
        type: u2
      - id: total_bone_count
        type: u2
      - id: total_frame_count
        type: s4
      - id: ik_helper_offset
        type: s4
      - id: joint_index_offset
        type: s4
      - id: key_frame_count
        type: s4
      - id: static_pose_offset
        type: u4
      - id: static_pose_count
        type: u4
      - id: footer_offset
        type: u4
      - id: model_bone_animation_offset
        type: u4
      - id: model_bone_animation_count
        type: u4
      - id: ik_helper_animation_offset
        type: u4
      - id: ik_helper_animation_count
        type: u4
      - id: timeline_offset
        type: u4
      - id: key_frame_offset
        type: u4
      - id: transformation_value_offset
        type: u4
      - id: tangent_offset
        type: u4
      - id: ik_chain_offset
        type: u4
      - id: ik_chain_count
        type: u4
      - id: unk48
        type: u4
      - id: table8_offset
        type: u4
      - id: table7_offset
        type: u4
      - id: table7_count
        type: u4
      - id: table6_offset
        type: u4
      - id: table6_count
        type: u4
      - id: bounding_box_min_x
        type: f4
      - id: bounding_box_min_y
        type: f4
      - id: bounding_box_min_z
        type: f4
      - id: bounding_box_min_w
        type: f4
      - id: bounding_box_max_x
        type: f4
      - id: bounding_box_max_y
        type: f4
      - id: bounding_box_max_z
        type: f4
      - id: bounding_box_max_w
        type: f4
      - id: frame_loop
        type: f4
      - id: frame_end
        type: f4
      - id: frame_per_second
        type: f4
      - id: frame_count
        type: f4
      - id: unknown_table1_offset
        type: u4
      - id: unknown_table1_count
        type: u4
      - id: unk98
        type: u4
      - id: unk9c
        type: u4
    instances:
      static_pose:
        pos: static_pose_offset -16
        type: static_pose
        repeat: expr
        repeat-expr: static_pose_count
      model_bone_animation:
        pos: model_bone_animation_offset -16
        type: bone_animation_table
        repeat: expr
        repeat-expr: model_bone_animation_count
      ik_helper_animation:
        pos: ik_helper_animation_offset -16
        type: bone_animation_table
        repeat: expr
        repeat-expr: ik_helper_animation_count
      raw_timeline:
        pos: timeline_offset -16
        type: timeline_table
        repeat: expr
        repeat-expr: (key_frame_offset - timeline_offset) / 8
      key_frames:
        pos: key_frame_offset -16
        type: f4
        repeat: expr
        repeat-expr: key_frame_count
      transformation_values:
        pos: transformation_value_offset -16
        type: f4
        repeat: expr
        repeat-expr: (tangent_offset - transformation_value_offset) / 4
      tangent_values:
        pos: tangent_offset -16
        type: f4
        repeat: expr
        repeat-expr: (ik_chain_offset - tangent_offset) / 4
      ik_chains:
        pos: ik_chain_offset -16
        type: ik_chain_table
        repeat: expr
        repeat-expr: ik_chain_count
      table8:
        pos: table8_offset -16
        type: unknown_table8
        repeat: expr
        repeat-expr: (table7_offset - table8_offset) / 0x30
      table7:
        pos: table7_offset -16
        type: unknown_table7
        repeat: expr
        repeat-expr: table7_count
      table6:
        pos: table6_offset -16
        type: unknown_table6
        repeat: expr
        repeat-expr: table6_count
      ik_helpers:
        pos: ik_helper_offset -16
        type: ik_helper_table
        repeat: expr
        repeat-expr: total_bone_count - bone_count
      joints:
        pos: joint_index_offset -16
        type: joint_table
        repeat: expr
        repeat-expr: total_bone_count

  static_pose:
    seq:
      - id: bone_index
        type: s2
      - id: channel
        type: s2
      - id: value
        type: f4

  bone_animation_table:
    seq:
      - id: joint_index
        type: s2
      - id: channel
        type: u1
      - id: timeline_count
        type: u1
      - id: timeline_start_index
        type: s2

  timeline_table:
    seq:
      - id: time
        type: s2
      - id: value_index
        type: s2
      - id: tangent_index_ease_in
        type: s2
      - id: tangent_index_ease_out
        type: s2

  ik_chain_table:
    seq:
      - id: unk00
        type: u1
      - id: unk01
        type: u1
      - id: model_bone_index
        type: s2
      - id: ik_helper_index
        type: s2
      - id: table8_index
        type: s2
      - id: unk08
        type: s2
      - id: unk0a
        type: s2

  unknown_table8:
    seq:
      - id: unk00
        type: u4
      - id: unk04
        type: u4
      - id: unk08
        type: f4
      - id: unk0c
        type: f4
      - id: unk10
        type: f4
      - id: unk14
        type: f4
      - id: unk18
        type: f4
      - id: unk1c
        type: f4
      - id: unk20
        type: f4
      - id: unk24
        type: f4
      - id: unk28
        type: f4
      - id: unk2c
        type: f4

  unknown_table7:
    seq:
      - id: unk00
        type: s2
      - id: unk02
        type: s2
      - id: unk04
        type: s2
      - id: unk06
        type: s2

  unknown_table6:
    seq:
      - id: unk00
        type: s2
      - id: unk02
        type: s2
      - id: unk04
        type: f4
      - id: unk08
        type: s2
      - id: unk0a
        type: s2

  joint_table:
    seq:
      - id: joint_index
        type: s2
      - id: flag
        type: s2

  ik_helper_table:
    seq:
      - id: index
        type: u4
      - id: parent_index
        type: u4
      - id: unk08
        type: u4
      - id: unk0c
        type: u4
      - id: scale_x
        type: f4
      - id: scale_y
        type: f4
      - id: scale_z
        type: f4
      - id: scale_w
        type: f4
      - id: rotate_x
        type: f4
      - id: rotate_y
        type: f4
      - id: rotate_z
        type: f4
      - id: rotate_w
        type: f4
      - id: translate_x
        type: f4
      - id: translate_y
        type: f4
      - id: translate_z
        type: f4
      - id: translate_w
        type: f4
