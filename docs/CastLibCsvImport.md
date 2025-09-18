[← Back to Docs Home](README.md)

# Importing Cast Libraries from CSV

BlingoEngine can populate a cast library by reading a simple **CSV** file. This allows you to organise media assets and script members in a spreadsheet and load them at runtime.

## CSV layout

Each line in the CSV represents one cast member. The importer expects five comma‑separated fields:

```
Number,Type,Name,Registration Point,Filename
1,bitmap,BallB,"(5, 5)",
```

* **Number** – numeric id of the member inside the cast.
* **Type** – member type such as `bitmap`, `text`, `field`, `filmLoop`, `sound`, or `script`.
* **Name** – optional string name.
* **Registration Point** – `(x, y)` coordinates of the member's registration point.
* **Filename** – optional relative path to the file backing the member.

If the filename column is empty the importer generates a filename based on the number, name and a default extension (`.png` for bitmaps, `.txt` for text and field members, `.wav` for sounds, `.cs` for scripts). Text members additionally support sibling `.md` or `.rtf` files which are preferred when present.

## Organising the files

The importer resolves filenames relative to the directory containing the CSV file. Place the CSV next to its associated media files. For example:

```
Media/
├── Data/
│   ├── Members.csv
│   ├── 1_Parameters.txt
│   ├── 2_BirdAnim.png
│   └── ...
```

When a text member has a matching Markdown file (`.md`), its content is loaded; otherwise an `.rtf` file or the plain text file is used. Binary assets such as images and sounds are loaded as referenced by the filename.

## Loading a cast via `IBlingoProjectFactory`

Cast libraries are typically loaded in the project factory. The `IBlingoPlayer` exposes synchronous and asynchronous helpers:

```csharp
public IBlingoPlayer LoadCastLibFromCsv(string castlibName, string csvPath, bool isInternal = false);
public Task<IBlingoPlayer> LoadCastLibFromCsvAsync(string castlibName, string csvPath, bool isInternal = false);
```

Use `LoadCastLibFromCsvAsync` inside the `LoadCastLibsAsync` method of your project factory:

```csharp
public async Task LoadCastLibsAsync(IBlingoCastLibsContainer castlibs, BlingoPlayer player)
{
    await player.LoadCastLibFromCsvAsync("Data", Path.Combine("Media", "Data", "Members.csv"));
}
```

The first argument specifies the name of the cast library added to the player. Setting `isInternal` to `true` marks the cast as internal so it can be unloaded later with `UnloadInternalCastLibs`.

## Example

A minimal `Members.csv` might look like:

```
Number,Type,Name,Registration Point,Filename
1,bitmap,Ball,"(5, 5)",Ball.png
2,text,Welcome,"(0, 0)",Welcome.txt
3,sound,Click,"(0, 0)",Click.wav
```

Assuming the files reside next to the CSV, the importer will add three members to a cast named `Data`. This cast can then be referenced by number or name in scripts and movies.

## Additional notes

* The importer skips the header line by default.
* When `_mediaRequiresAsyncPreload` is enabled, each imported member is preloaded asynchronously after creation.
* `LoadCastLibFromCsv` is a convenience wrapper around the asynchronous method for environments that cannot use `await`.
* Multiple cast libraries can be loaded by calling the helper methods repeatedly with different names and CSV paths.


