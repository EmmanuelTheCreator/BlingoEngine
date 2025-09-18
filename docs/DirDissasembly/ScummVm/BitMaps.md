# Bitmap Cast Members

This note summarizes how ScummVM's Director engine parses bitmap cast member metadata and pixel data. Each section walks through the bytes read for specific Director versions and image formats so the field layout is clear at a glance.

## Step 1 – Director 3 and earlier bitmap records (version < 0x400)

`BitmapCastMember` first handles legacy movies by pulling a fixed set of 16-bit values from the resource stream: the cast byte count, the image and bounding rectangles, and the registration offsets. When the high bit of the byte-count word is set, additional palette information follows so the loader can bind the bitmap to a CLUT.[engines/director/castmember/bitmap.cpp (approx. lines 62-95)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<cast data size>` | 2 bytes | Total size of the bitmap record; bit `0x8000` flags an inline palette reference.[engines/director/castmember/bitmap.cpp (approx. lines 66-82)] |
| `<rect.top>` | 2 bytes | Signed top coordinate read by `Movie::readRect()`.[engines/director/movie.cpp (approx. lines 276-284)] |
| `<rect.left>` | 2 bytes | Signed left coordinate for `_initialRect`.[engines/director/movie.cpp (approx. lines 276-284)] |
| `<rect.bottom>` | 2 bytes | Signed bottom coordinate for `_initialRect`.[engines/director/movie.cpp (approx. lines 276-284)] |
| `<rect.right>` | 2 bytes | Signed right coordinate for `_initialRect`.[engines/director/movie.cpp (approx. lines 276-284)] |
| `<bounds.top>` | 2 bytes | Top of `_boundingRect`, read with the same helper.[engines/director/castmember/bitmap.cpp (approx. lines 76-78)] |
| `<bounds.left>` | 2 bytes | Left of `_boundingRect`.[engines/director/castmember/bitmap.cpp (approx. lines 76-78)] |
| `<bounds.bottom>` | 2 bytes | Bottom of `_boundingRect`.[engines/director/castmember/bitmap.cpp (approx. lines 76-78)] |
| `<bounds.right>` | 2 bytes | Right of `_boundingRect`.[engines/director/castmember/bitmap.cpp (approx. lines 76-78)] |
| `<regY>` | 2 bytes | Signed registration offset on the Y axis.[engines/director/castmember/bitmap.cpp (approx. lines 78-79)] |
| `<regX>` | 2 bytes | Signed registration offset on the X axis.[engines/director/castmember/bitmap.cpp (approx. lines 78-79)] |
| `<bits per pixel>` | 2 bytes (optional) | Present when `cast data size` has bit `0x8000` set; records the pixel depth.[engines/director/castmember/bitmap.cpp (approx. lines 80-86)] |
| `<CLUT id>` | 2 bytes (optional) | Signed palette resource number; negative/zero references built-in system palettes.[engines/director/castmember/bitmap.cpp (approx. lines 82-86)] |

## Step 2 – Director 4–5 bitmap records (0x400 ≤ version < 0x600)

Director 4 and 5 store a slightly longer record. After an encoded pitch and two rectangles, the loader reads registration offsets and optional palette metadata. Director 5 expands the optional block with a cast-library selector before the CLUT ID and additional unknown fields.[engines/director/castmember/bitmap.cpp (approx. lines 96-134)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<pitch word>` | 2 bytes | Upper nibble carries the color-flag bit; lower 12 bits store the pixel pitch.[engines/director/castmember/bitmap.cpp (approx. lines 100-104)] |
| `<rect.top>`..`<rect.right>` | 8 bytes | Four signed 16-bit values for `_initialRect`.[engines/director/movie.cpp (approx. lines 276-284)] |
| `<bounds.top>`..`<bounds.right>` | 8 bytes | Bounding rectangle stored in the same layout.[engines/director/castmember/bitmap.cpp (approx. lines 102-104)] |
| `<regY>` | 2 bytes | Registration offset on Y.[engines/director/castmember/bitmap.cpp (approx. line 104)] |
| `<regX>` | 2 bytes | Registration offset on X.[engines/director/castmember/bitmap.cpp (approx. line 105)] |
| `<unknown padding>` | 1 byte (optional) | Present when the resource extends past 22 bytes; historically used as a flag byte.[engines/director/castmember/bitmap.cpp (approx. lines 107-110)] |
| `<bits per pixel>` | 1 byte (optional) | Pixel depth stored after the padding byte.[engines/director/castmember/bitmap.cpp (approx. lines 108-110)] |
| `<CLUT cast library>` | 2 bytes (optional, D5+) | Library ID for the palette when version ≥ 0x500.[engines/director/castmember/bitmap.cpp (approx. lines 111-115)] |
| `<CLUT id>` | 2 bytes (optional) | Palette resource number; non-positive values select built-ins.[engines/director/castmember/bitmap.cpp (approx. lines 115-123)] |
| `<unknown16>` | 2 bytes (optional) | Additional metadata observed in longer Director 4/5 records.[engines/director/castmember/bitmap.cpp (approx. lines 124-128)] |
| `<unknown16>` | 2 bytes (optional) | Second 16-bit value retained for completeness.[engines/director/castmember/bitmap.cpp (approx. lines 124-128)] |
| `<unknown16>` | 2 bytes (optional) | Third 16-bit value preceding 32-bit fields.[engines/director/castmember/bitmap.cpp (approx. lines 124-128)] |
| `<unknown32>` | 4 bytes (optional) | Reserved field Director emitted in longer records.[engines/director/castmember/bitmap.cpp (approx. lines 129-130)] |
| `<unknown32>` | 4 bytes (optional) | Second reserved dword carried over from Director output.[engines/director/castmember/bitmap.cpp (approx. lines 129-130)] |
| `<flags2>` | 2 bytes (optional) | Extra bitmap flags captured when the extended block is present.[engines/director/castmember/bitmap.cpp (approx. lines 129-132)] |

## Step 3 – Director 6–10 bitmap records (0x600 ≤ version < 0x1100)

Later versions retain the general structure but add UI-related metadata such as the edit version, scroll point, and update flags. The pitch word still carries the color-image bit (0x8000) that controls whether palette information follows.[engines/director/castmember/bitmap.cpp (approx. lines 147-186)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<pitch word>` | 2 bytes | Includes the color-image bit; the lower 12 bits store the pitch after masking.[engines/director/castmember/bitmap.cpp (approx. lines 149-167)] |
| `<rect.top>`..`<rect.right>` | 8 bytes | `_initialRect` coordinates stored as signed 16-bit values.[engines/director/castmember/bitmap.cpp (approx. lines 150-151)] |
| `<alpha threshold>` | 1 byte (version ≥ 0x700) | Alpha cutoff for transparent pixels; followed by 1 byte of padding.[engines/director/castmember/bitmap.cpp (approx. lines 151-154)] |
| `<padding>` | 2 bytes (version < 0x700) | Alignment field for earlier Director 6 builds.[engines/director/castmember/bitmap.cpp (approx. lines 152-156)] |
| `<edit version>` | 2 bytes | Authoring-version stamp stored in the resource.[engines/director/castmember/bitmap.cpp (approx. lines 157-158)] |
| `<scrollY>` | 2 bytes | Signed scroll offset on the Y axis.[engines/director/castmember/bitmap.cpp (approx. lines 158-159)] |
| `<scrollX>` | 2 bytes | Signed scroll offset on the X axis.[engines/director/castmember/bitmap.cpp (approx. lines 158-159)] |
| `<regY>` | 2 bytes | Registration offset on Y.[engines/director/castmember/bitmap.cpp (approx. lines 160-161)] |
| `<regX>` | 2 bytes | Registration offset on X.[engines/director/castmember/bitmap.cpp (approx. lines 160-161)] |
| `<update flags>` | 1 byte | Bitfield describing runtime behaviors (center registration, matte usage, etc.).[engines/director/castmember/bitmap.cpp (approx. lines 162-164)] |
| `<bits per pixel>` | 1 byte (optional) | Present when the pitch word’s high bit marked the bitmap as color.[engines/director/castmember/bitmap.cpp (approx. lines 165-168)] |
| `<CLUT cast library>` | 2 bytes (optional) | Palette library ID reused from Director 5 logic.[engines/director/castmember/bitmap.cpp (approx. lines 168-176)] |
| `<CLUT id>` | 2 bytes (optional) | Palette resource identifier; negative/zero selects built-ins.[engines/director/castmember/bitmap.cpp (approx. lines 172-179)] |

## Step 4 – DIB cast members and palette records

When a bitmap references a Windows DIB, ScummVM routes the data into `DIBDecoder`. Director stores palettes as 6-byte Mac color records and prepends each DIB stream with a 40-byte BITMAPINFOHEADER structure. The decoder reads those values verbatim to configure the pixel surface before the image codec runs.[engines/director/images.cpp (approx. lines 42-100)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<palette red>` | 1 byte | High-byte red component for a palette entry.[engines/director/images.cpp (approx. lines 42-53)] |
| `<palette pad>` | 1 byte | Unused byte between color components.[engines/director/images.cpp (approx. lines 42-53)] |
| `<palette green>` | 1 byte | High-byte green component.[engines/director/images.cpp (approx. lines 42-53)] |
| `<palette pad>` | 1 byte | Unused byte between components.[engines/director/images.cpp (approx. lines 42-53)] |
| `<palette blue>` | 1 byte | High-byte blue component stored by Director.[engines/director/images.cpp (approx. lines 42-53)] |
| `<palette pad>` | 1 byte | Final unused byte completing the 6-byte color record.[engines/director/images.cpp (approx. lines 42-53)] |
| `28 00 00 00` | 4 bytes | Little-endian BITMAPINFOHEADER size (0x28 == 40).[engines/director/images.cpp (approx. lines 55-68)] |
| `<width>` | 4 bytes | Signed little-endian pixel width.[engines/director/images.cpp (approx. lines 60-68)] |
| `<height>` | 4 bytes | Signed little-endian pixel height.[engines/director/images.cpp (approx. lines 60-68)] |
| `<planes>` | 2 bytes | Always `0x0001`, read and ignored.[engines/director/images.cpp (approx. lines 64-66)] |
| `<bits per pixel>` | 2 bytes | Color depth of the DIB image.[engines/director/images.cpp (approx. lines 65-67)] |
| `<compression>` | 4 bytes | Big-endian compression identifier forwarded to the codec.[engines/director/images.cpp (approx. lines 66-70)] |
| `<image size>` | 4 bytes | Declared bitmap data size; kept for diagnostics.[engines/director/images.cpp (approx. lines 68-72)] |
| `<pixels/meter X>` | 4 bytes | Horizontal resolution metric.[engines/director/images.cpp (approx. lines 68-72)] |
| `<pixels/meter Y>` | 4 bytes | Vertical resolution metric.[engines/director/images.cpp (approx. lines 68-72)] |
| `<palette color count>` | 4 bytes | Overrides the palette length when non-zero.[engines/director/images.cpp (approx. lines 71-74)] |
| `<important colors>` | 4 bytes | Unused field copied for completeness.[engines/director/images.cpp (approx. lines 71-74)] |

## Step 5 – BITD pixel decoding and RLE control bytes

Classic Mac bitmaps use the `BITD` chunk format. After validating the pitch, width, and bits-per-pixel parameters supplied by the cast member, ScummVM walks the chunk stream byte by byte. Each control byte either encodes a literal run or an RLE repeat count; multi-byte pixels (16-bit and 32-bit) are reconstructed line by line after the runs are expanded.[engines/director/images.cpp (approx. lines 150-260)]

| Hex Bytes | Length | Explanation |
| --- | --- | --- |
| `<control byte>` | 1 byte | Values < 0x80 represent literal runs of `len = byte + 1`; values ≥ 0x80 encode repeats with `len = (0xFF ^ byte) + 2`.[engines/director/images.cpp (approx. lines 198-220)] |
| `<literal data>` | `len` bytes | Raw pixel bytes copied when the control byte < 0x80.[engines/director/images.cpp (approx. lines 212-220)] |
| `<repeat value>` | 1 byte | Single byte repeated `len` times when the control byte ≥ 0x80.[engines/director/images.cpp (approx. lines 204-216)] |
| `<zlib segment>` | `res.size` bytes | Compressed payload fed to `readZlibData()` when Afterburner metadata marks the bitmap as compressed.[engines/director/archive.cpp (approx. lines 1128-1148)] |

Together these layouts describe how ScummVM reconstructs bitmap cast members across Director versions: header words describe geometry and palette ownership, while the decoder classes unpack palettes, DIB headers, and BITD RLE streams to produce usable pixel surfaces.
