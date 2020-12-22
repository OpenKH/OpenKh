meta:
  id: kh2_model
  endian: le
seq:
  - id: hw
    size: 144
instances:
  model_inst:
    type: model
    pos: 144
    size-eos: true
types:
  model:
    seq:
      - id: type
        type: u4
        enum: model_type
      - id: unk1
        type: u4
      - id: unk2
        type: u4
      - id: next_off
        type: u4
      - id: map_desc
        type: map_desc
        if: (type == model_type::map)
      - id: object_desc
        type: object_desc
        if: (type == model_type::object) or (type == model_type::shadow)
    instances:
      sub_model:
        type: model
        if: (next_off != 0)
        pos: next_off
        size-eos: true
  dma_chain_index_remap_table:
    seq:
      - id: next_off
        type: u4
      - id: dma_chain_index
        type: u2
        repeat: until
        repeat-until: _ == 0xffff
  dma_chain_map:
    seq:
      - id: dma_tag_off
        type: u4
      - id: texture_idx
        type: u4
      - id: unk1
        type: u4
      - id: unk2
        type: u4
    instances:
      dma_tags:
        pos: dma_tag_off
        type: dma_tag_array_map
  dma_tag_array_object:
    seq:
      - id: dma_tag
        type: source_chain_dma_tag
        repeat: eos
  dma_tag_array_map:
    seq:
      - id: dma_tag
        type: source_chain_dma_tag
        repeat: until
        repeat-until: _.end_transfer
  source_chain_dma_tag:
    seq:
      - id: qwc
        type: u2
      - id: pad
        type: b8
      - id: irq
        type: b1
      - id: tag_id
        type: b3
        enum: dma_tag_id
      - id: pce
        type: b2
      - id: addr
        type: u4
      - id: vif_tag
        type: vif_tag
        repeat: expr
        repeat-expr: 2
      - id: gif_tag
        type: gif_tag
        repeat: expr
        repeat-expr: qwc
        if: (tag_id == dma_tag_id::cnt or tag_id == dma_tag_id::next or tag_id == dma_tag_id::refe or tag_id == dma_tag_id::call or tag_id == dma_tag_id::ret) and (vif_tag[1].cmd == vif_cmd::direct)
      - id: raw_data
        size: 16
        repeat: expr
        repeat-expr: qwc
        if: (tag_id == dma_tag_id::cnt or tag_id == dma_tag_id::next or tag_id == dma_tag_id::refe or tag_id == dma_tag_id::call or tag_id == dma_tag_id::ret) and (vif_tag[1].cmd != vif_cmd::direct)
    instances:
      end_transfer:
        value: (tag_id == dma_tag_id::refe or tag_id == dma_tag_id::end or tag_id == dma_tag_id::ret)
    
  vif_tag:
    seq:
      - id: immediate
        type: u2
      - id: num
        type: u1
      - id: cmd
        type: u1
        enum: vif_cmd
  gif_tag:
    seq:
      - id: nloop
        type: b15
      - id: eop
        type: b1
      - id: skip
        type: b31
      - id: pre
        type: b1
      - id: prim
        type: b10
      - id: flg
        type: b2
      - id: nreg
        type: b4
      - id: regs
        type: u8
  object_desc:
    seq:
      - id: num_axbone
        type: u2
      - id: unk1
        type: u2
      - id: off_axbone
        type: u4
      - id: unk2
        type: u4
      - id: cnt_model_parts
        type: u2
      - id: unk3
        type: u2
      - id: model_parts
        type: model_part
        repeat: expr
        repeat-expr: cnt_model_parts
    instances:
      ax_bone:
        pos: off_axbone
        type: ax_bone
        if: (off_axbone != 0)
        repeat: expr
        repeat-expr: num_axbone
  model_part:
    seq:
      - id: unk1
        type: u4
      - id: texture_index
        type: u4
      - id: unk2
        type: u4
      - id: unk3
        type: u4
      - id: off_first_dma_tag
        type: u4
      - id: off_indices_of_axbone
        type: u4
      - id: num_dma_qwc_packets
        type: u4
      - id: unk5
        type: u4
    instances:
      dma_tags:
        pos: off_first_dma_tag
        type: dma_tag_array_object
        size: 16 * num_dma_qwc_packets
      indices_of_axbone:
        pos: off_indices_of_axbone
        type: indices_of_axbone

  indices_of_axbone:
    seq:
      - id: count
        type: u4
      - id: index_of_axbone
        type: u4
        repeat: expr
        repeat-expr: count

  ax_bone:
    seq:
      - id: this_idx
        type: u2
      - id: this_reverse_idx
        type: u2
      - id: parent_idx
        type: u2
      - id: parent_reverse_idx
        type: u2
      - id: unk1
        type: u4
      - id: unk2
        type: u4
      - id: scale_x
        type: f4
      - id: scale_y
        type: f4
      - id: scale_z
        type: f4
      - id: unk3
        type: f4
      - id: rotation_x
        type: f4
      - id: rotation_y
        type: f4
      - id: rotation_z
        type: f4
      - id: unk4
        type: u4
      - id: translate_x
        type: f4
      - id: translate_y
        type: f4
      - id: translate_z
        type: f4
      - id: unk5
        type: u4
  
  map_desc:
    seq:
      - id: num_dma_chain_maps
        type: u4
      - id: unk3
        type: u2
      - id: num_vif_packet_rendering_group
        type: u2
      - id: off_vif_packet_rendering_group
        type: u4
      - id: off_dma_chain_index_remap_table
        type: u4
      - id: dma_chain_map
        type: dma_chain_map
        repeat: expr
        repeat-expr: num_dma_chain_maps
    instances:
      vif_packet_rendering_group:
        pos: off_vif_packet_rendering_group
        type: vif_packet_rendering_group
        size: 4
        repeat: expr
        repeat-expr: num_vif_packet_rendering_group
      dma_chain_index_remap_table:
        pos: off_dma_chain_index_remap_table
        type: dma_chain_index_remap_table
  vif_packet_rendering_group:
    seq:
      - id: offset_to_group
        type: u4
    instances:
      list:
        pos: offset_to_group
        io: _parent._io
        type: u2
        repeat: until
        repeat-until: _ == 0xffff

enums:
  model_type:
    2: "map"
    3: "object"
    4: "shadow"
  vif_cmd:
    0: "nop"
    1: "stcycl"
    2: "offset"
    3: "base"
    4: "itop"
    5: "stmod"
    6: "mskpath3"
    7: "mark"
    16: "flushe"
    17: "flush"
    19: "flusha"
    20: "mscal"
    23: "mscnt"
    21: "mscalf"
    32: "stmask"
    48: "strow"
    49: "stcol"
    74: "mpg"
    80: "direct"
    81: "directhl"
    96: "unmasked_s_32"
    97: "unmasked_s_16"
    98: "unmasked_s_8"
    100: "unmasked_v2_32"
    101: "unmasked_v2_16"
    102: "unmasked_v2_8"
    104: "unmasked_v2_32_alt"
    105: "unmasked_v3_16"
    106: "unmasked_v3_8"
    108: "unmasked_v4_32"
    109: "unmasked_v4_16"
    110: "unmasked_v4_8"
    111: "unmasked_v4_5"
    112: "masked_s_32"
    113: "masked_s_16"
    114: "masked_s_8"
    116: "masked_v2_32"
    117: "masked_v2_16"
    118: "masked_v2_8"
    120: "masked_v2_32_alt"
    121: "masked_v3_16"
    122: "masked_v3_8"
    124: "masked_v4_32"
    125: "masked_v4_16"
    126: "masked_v4_8"
    127: "masked_v4_5"
  dma_tag_id:
    0: refe
    1: cnt
    2: next
    3: ref
    4: refs
    5: call
    6: ret
    7: end
