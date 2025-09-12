# TetriGrounds Sound Offsets

This table lists sound members expected in `TetriGrounds.dir` and the byte range of each MP3 if present.

## Header Bytes

| Offset | Value | Description |
| ------: | ----- | ----------- |
| 0x00000000 | 58 46 49 52 (`XFIR`) | File magic (little-endian `RIFX`) |
| 0x00000004 | B8 E8 16 00 | File length (1501368 bytes) |
| 0x00000008 | 33 39 56 4D (`MV93`) | Director version |

| ID | Member Name | File | Header Bytes | Start Offset | Length | Delta from Prev |
| --: | ----------- | ---- | ------------ | -----------: | -----: | --------------: |
| 1 | S_Click | click.mp3 | 49443304000000000022545353450000 | N/A | 3569 | N/A |
| 2 | S_BtnStart | btnstart.mp3 | 49443304000000000022545353450000 | N/A | 5685 | N/A |
| 3 | S_Gong | gong.mp3 | 49443304000000000022545353450000 | N/A | 104684 | N/A |
| 4 | S_DeleteRow | deleterow.mp3 | 49443304000000000063544452430000 | N/A | 3565 | N/A |
| 5 | S_GO | go.mp3 | 49443304000000000044545858580000 | N/A | 53474 | N/A |
| 6 | S_LevelUp | level_up.mp3 | 49443304000000000044545858580000 | N/A | 56350 | N/A |
| 7 | S_Shhh1 | shhh1.mp3 | 49443304000000000043545858580000 | N/A | 5718 | N/A |
| 8 | S_Terminated | terminated.mp3 | 49443304000000000044545858580000 | N/A | 73670 | N/A |
| 9 | S_Died | die.mp3 | 4944330400000000003c545045310000 | N/A | 129514 | N/A |
| 10 | S_Nature | nature.mp3 | 49443303000000000231545858580000 | N/A | 496099 | N/A |
| 11 | S_BlockFall1 | blockfall_1.mp3 | 49443304000000000043545858580000 | N/A | 6554 | N/A |
| 12 | S_BlockFall2 | blockfall_2.mp3 | 49443304000000000043545858580000 | N/A | 6554 | N/A |
| 13 | S_BlockFall3 | blockfall_3.mp3 | 49443304000000000022545353450000 | N/A | 6521 | N/A |
| 14 | S_BlockFall4 | blockfall_4.mp3 | 49443304000000000043545858580000 | N/A | 6554 | N/A |
| 15 | S_BlockFall5 | blockfall_5.mp3 | 49443304000000000043545858580000 | N/A | 6554 | N/A |
| 16 | S_RowsDeleted_2 | rows_deleted_2.mp3 | 49443304000000000044545858580000 | N/A | 61877 | N/A |
| 17 | S_RowsDeleted_3 | rows_deleted_3.mp3 | 49443304000000000044545858580000 | N/A | 64800 | N/A |
| 18 | S_RowsDeleted_4 | rows_deleted_4.mp3 | 49443304000000000044545858580000 | N/A | 72367 | N/A |

## CASt Chunk Offsets

This table lists the offsets of `CASt` chunks associated with sound member IDs found in the file. Any member not present in the table was not located in `TetriGrounds.dir`.

| Member ID | CASt Offset (hex) | CASt Length |
| --------: | ----------------: | ----------: |
| 1 | N/A | N/A |
| 2 | N/A | N/A |
| 3 | N/A | N/A |
| 4 | 0x1D84E | 622 |
| 5 | 0x0CAA8 | 9917 |
| 6 | N/A | N/A |
| 7 | 0x09228 | 3880 |
| 8 | 0x231BC | 179 |
| 9 | N/A | N/A |
| 10 | N/A | N/A |
| 11 | 0x20584 | 628 |
| 12 | N/A | N/A |
| 13 | N/A | N/A |
| 14 | N/A | N/A |
| 15 | N/A | N/A |
| 16 | 0x0A158 | 1704 |
| 17 | N/A | N/A |
| 18 | 0x0B2C2 | 1049 |
