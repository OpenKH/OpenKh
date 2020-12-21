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
        pos: static_pose_offset
        size: 8 * static_pose_count
      model_bone_animation:
        pos: model_bone_animation_offset
        size: 6 * model_bone_animation_count
      ik_helper_animation:
        pos: ik_helper_animation_offset
        size: 4 * ik_helper_animation_count
      raw_timeline:
        pos: timeline_offset
        size: key_frame_offset - timeline_offset
      key_frames:
        pos: key_frame_offset
        size: 4 * key_frame_count
      transformation_values:
        pos: transformation_value_offset
        size: tangent_offset - transformation_value_offset
      tangent_values:
        pos: tangent_offset
        size: ik_chain_offset - tangent_offset
      ik_chains:
        pos: ik_chain_offset
        size: 12 * ik_chain_count
      table8:
        pos: table8_offset
        size: table7_offset - table8_offset
      table7:
        pos: table7_offset
        size: 8 * table7_count
      table6:
        pos: table6_count
        size: 12 * table6_count
      ik_helpers:
        pos: ik_helper_offset
        size: 64 * (total_bone_count - bone_count)
      joints:
        pos: joint_index_offset
        size: 4 * total_bone_count
