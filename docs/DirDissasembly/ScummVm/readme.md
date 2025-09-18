# ScummVM Director Notes

- [Reading Archives](./Reading%20Archives.md)
- [Chunk Handling](./Chunks.md)
- [Cast Resource Mapping](./Cast%20Resources.md)
- [Cast Member Base Records](./Cast%20Member%20Base.md)
- [Bitmap Cast Members](./BitMaps.md)
- [Shape Cast Members](./Shapes.md)
- [Text and Field Cast Members](./Text%20and%20Field%20Members.md)
- [Font Mapping Resources](./Font%20Members.md)
- [Rich Text Cast Members](./Rich%20Text%20Members.md)
- [Sound Cast Members](./Sound%20Members.md)
- [Transition Cast Members](./Transition%20Members.md)
- [Palette Cast Members](./Palette%20Members.md)
- [Film Loop Cast Members](./Film%20Loop%20Members.md)
- [Score Resources](./Score.md)
- [Sprite Channel Records](./Sprites.md)
- [Movie Cast Members](./Movie%20Members.md)
- [Digital Video Cast Members](./Video%20Members.md)
- [Script Cast Members](./Script%20Members.md)

---

## Update Tracking

- **Last verified:** 2025-09-18 07:01 UTC
- **ScummVM reference:** [`4eb06084a8449d8c8e5060d8611bd101c6b39cee`](https://github.com/scummvm/scummvm/tree/4eb06084a8449d8c8e5060d8611bd101c6b39cee/engines/director)

To refresh these notes in the future, fetch the latest `master` branch from the ScummVM repository and diff the Director engine folder against commit `4eb06084a8449d8c8e5060d8611bd101c6b39cee`:

```bash
git clone https://github.com/scummvm/scummvm.git  # or reuse an existing clone
cd scummvm
git fetch origin                                  # ensure origin/master is current when reusing a clone
git diff 4eb06084a8449d8c8e5060d8611bd101c6b39cee..origin/master -- engines/director
```

Review any changes the diff reports, update the documentation accordingly, and then bump the timestamp and commit hash above so the next scan has an exact comparison point.
