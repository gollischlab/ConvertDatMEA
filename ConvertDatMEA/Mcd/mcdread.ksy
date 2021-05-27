meta:
  id: mcdfile
  file-extension: mcd
  
seq: 
- id: magicbytes
  contents: ["MCSSTRM ", 255, 255, 255, 255, 255, 255, 255, 255]
- id: header_index
  type: mcd_header
- id: idxheader
  type: idx_header
- id: data
  type: mcd_data
- id: rest
  size-eos: true
  
types:
  mcd_data:
    seq:
      - id: chunks
        type: stream_chunk
        repeat: until
        repeat-until: _parent.idxheader.final_index_pointer-1 < _root._io.pos
        
  idx_header:
    seq: 
      - id: magicbytes
        contents: "IDX1POS "
      - id: size4_pointer
        type: u8le
      - id: final_index_pointer
        type: u8le

  stream_chunk:
    seq:
      - id: name
        size: 8
        type: str
        terminator: 0
        encoding: ascii
      - id: chunk_len
        type: u8le
      - id: timestamp_start
        type: u8le
      - id: timestamp_end
        type: u8le
      - id: chunk_sample
        size: (chunk_len - 16)

  mcd_header:
    seq:
      - id: magic
        contents: "LISThdr "
      - id: header_len
        type: u8le
      - id: header_list
        type: mcd_headerlist
        size: header_len
  
  mcd_headerlist:
    seq:
      - id: headers
        type: header
        repeat: eos
  
  stream_header:
    seq:
       - id: version
         type: u2le
       - id: type_name
         size: 9
         type: str
         encoding: ascii
         terminator: 0
       - id: stream_name
         size: 257
         type: str
         terminator: 0
         encoding: ascii
       - id: comment
         size: 513
         type: str
         encoding: ascii
         terminator: 0
       - #id: padding1
         size: 1
       - id: stream_id
         type: u2le
       - id: millisamplespersecond
         type: u4le
       - id: channel_count
         type: u4le
       - id: buffer_id
         size: 9
         type: str
         terminator: 0
         encoding: ascii
       - #id: padding2
         size: 3
       - id: channels
         type: channel_info
         repeat: expr
         repeat-expr: channel_count

  channel_info:
    seq:
      - id: version
        type: u2le
      - #id: padding1
        size: 2
      - id: id
        type: u4le
      - id: hwid
        type: u4le
      - id: name
        type: str
        encoding: ascii
        size: 257
        terminator: 0
      - id: comment
        type: str
        encoding: ascii
        size: 513
        terminator: 0
      - size: 2 #id: padding3
      - id: ref_count 
        type: u4le
      - id: group_id
        type: u2le
      - id: decorated_name
        if: version >= 1
        type: str
        encoding: ascii
        terminator: 0
        size: 257
        # padding
      - size: '(version >= 1) ? 1 : 2'
      - id: gain
        type: u4le
      - id: rest1
        if: version >= 2
        type: u4le
        repeat: expr
        repeat-expr: 7
      - id: pos_y
        if: version >= 2
        type: u4le
      - id: pos_x
        if: version >= 2
        type: u4le
      - id: rest2
        if: version >= 2
        type: u4le
        repeat: expr
        repeat-expr: 4
      - id: ref_ch
        type: refchr_header
        if: ref_count != 0
        repeat: expr
        repeat-expr: ref_count
  
  refchr_header:
    seq:
      - id: bufferid
        type: u2le 
      - id: unknown # padding? it's often non-zero though
        size: 2
      - id: channelid
        type: u4le

  stream_format:
    seq:
      - id: format_version
        type: u2le
      - id: unit_sign
        size: 2
        type: str
        encoding: ascii
      - id: ad_bits
        type: u2le
      - id: ad_zero
        type: u2le
      - id: units_per_ad
        type: f8le
      - id: bytes_per_channel
        type: u2le
      - id: default_samples_per_segment
        type: u2le
      - id: default_segment_count
        type: u2le
      - size: 2 # unkown, maybe always 0x714d
      - id: unknown
        size: 8 # same as unitsperad
      - id: param
        size-eos: true

  mcs_header:
    seq:
      - size: 4
      - id: software_version_major
        type: u4le
      - id: software_version_minor
        type: u4le
      - id: file_path
        size: 780 # Maybe smaller and there are some zero fields?
        type: str
        encoding: ascii
        terminator: 0
      - id: timestamp
        type: u8le
      - id: stream_count
        type: u4le
      - id: millisamples_per_second
        type: u4le
      - size: 20
      - id: electrode_channels
        type: u2le
      - id: electrode_channel_offset
        type: u2le
      - id: analog_channels
        type: u2le
      - id: analog_channel_offset
        type: u2le
      - id: digital_channels
        type: u2le
      - id: digital_channel_offset
        type: u2le
      - id: total_channels
        type: u2le
      - size: 6
      - id: timestamp_end
        type: u8le
      - size: 12
      - id: segment_time
        type: u4le
      - id: timestamp_start
        type: u8le
      - id: driver_version_major
        type: u4le
      - id: driver_version_minor
        type: u4le
      - id: image_file_path_name
        size: 1306
        type: str
        encoding: ascii
        terminator: 0
      - size: 6
      - id: voltage_range
        type: u4le
      - id: data_soure_name
        size: 100
        type: str
        encoding: ascii
        terminator: 0
      - id: bus_type
        type: u4le
      - id: vendor_id
        type: u4le
      - id: product_id
        type: u4le
      - size: 8
  
  header_unknown:
    seq:
      - id: content
        size-eos: true

  header:
    seq:
      - id: name
        size: 8
        type: str
        encoding: ascii
        terminator: 0
      - id: headersize
        type: u8le
      - id: content
        size: headersize
        type: 
          switch-on: name
          cases:
            '"MCSHDR  "': mcs_header
            '"STRMHDR "': stream_header
            '"STRMFMT "': stream_format
            _: header_unknown # Just int case. There should only be the three above
  