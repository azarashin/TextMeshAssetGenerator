# TextMesh Pro Font Asset Generator

This editor utility generates a TextMesh Pro `TMP_FontAsset` from a source font and a collected set of characters.

It is mainly intended for Unity WebGL builds where embedding a full Japanese font can make the build size very large. Instead of including every glyph from the source font, this tool scans project files such as JSON, UXML, USS, C# scripts, and text files, then generates a subset font asset containing only the characters actually used by the project.

## Features

* Generates a `TMP_FontAsset` from a configured source font
* Collects characters from:

  * Additional manually specified characters
  * Assigned `TextAsset` files
  * Files under configured scan folders
* Supports filtering by file extension
* Supports multi-atlas font assets
* Supports fallback font assets
* Can overwrite an existing generated font asset
* Can output a missing character list
* Can convert the generated font asset to Static mode after generation

## Files

This tool consists of the following main scripts:

```text
FontAssetBuildSettings.cs
FontAssetAutoBuilder.cs
```

### `FontAssetBuildSettings`

A `ScriptableObject` that stores the generation settings.

Create it from:

```text
Create > SignedOff > Fonts > Font Asset Build Settings
```

### `FontAssetAutoBuilder`

An editor utility that reads the selected `FontAssetBuildSettings` asset and generates the configured font asset.

Run it from:

```text
Tools > SignedOff > Fonts > Generate Font Asset From Selected Settings
```

## Setup

1. Place a source font file in the Unity project.

   Example:

   ```text
   Assets/TextMeshAssetGenerator/SourceFonts/NotoSansJP-Regular.ttf
   ```

2. Create a `FontAssetBuildSettings` asset.

   ```text
   Create > SignedOff > Fonts > Font Asset Build Settings
   ```

3. Assign the source font to `SourceFont`.

4. Configure the output path.

   Example:

   ```text
   Assets/TextMeshAssetGenerator/FontAssets/JapaneseSubset SDF.asset
   ```

5. Add folders to `ScanFolders`.

   Example:

   ```text
   Assets/ScenarioData
   Assets/AuditCaseData
   Assets/UI
   ```

6. Select the `FontAssetBuildSettings` asset in the Project window.

7. Run:

   ```text
   Tools > SignedOff > Fonts > Generate Font Asset From Selected Settings
   ```

## Settings

### Source

| Field        | Description                                                     |
| ------------ | --------------------------------------------------------------- |
| `SourceFont` | The source `Font` used to generate the TextMesh Pro font asset. |

### Output

| Field             | Description                                                                                                        |
| ----------------- | ------------------------------------------------------------------------------------------------------------------ |
| `OutputAssetPath` | The asset path where the generated `TMP_FontAsset` will be saved. Must start with `Assets/` and end with `.asset`. |

### Atlas

| Field                     | Description                                                                         |
| ------------------------- | ----------------------------------------------------------------------------------- |
| `SamplingPointSize`       | Font sampling size used when generating glyphs.                                     |
| `Padding`                 | Padding around glyphs in the atlas texture.                                         |
| `AtlasWidth`              | Width of the generated atlas texture.                                               |
| `AtlasHeight`             | Height of the generated atlas texture.                                              |
| `RenderMode`              | Glyph render mode, such as `SDFAA`.                                                 |
| `EnableMultiAtlasSupport` | Allows TextMesh Pro to create multiple atlas textures when one atlas is not enough. |

### Characters

| Field                  | Description                                                                                                                                           |
| ---------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| `AdditionalCharacters` | Characters that should always be included in the generated font asset. Useful for numbers, alphabets, punctuation, symbols, and common UI characters. |
| `CharacterTextAssets`  | Text assets whose contents are used as character sources.                                                                                             |
| `ScanFolders`          | Folder paths scanned for characters. Each path should start with `Assets/`.                                                                           |
| `ScanExtensions`       | File extensions to scan. For example: `.json`, `.txt`, `.uxml`, `.uss`, `.cs`.                                                                        |

### Fallback

| Field                | Description                                                                                 |
| -------------------- | ------------------------------------------------------------------------------------------- |
| `FallbackFontAssets` | Optional TextMesh Pro fallback font assets. These are assigned to the generated font asset. |

### Build Behavior

| Field                        | Description                                                                                                            |
| ---------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| `SetStaticAfterGeneration`   | Sets the generated font asset to Static mode after characters are added. Recommended for WebGL builds with known text. |
| `OverwriteExistingAsset`     | Deletes and recreates the existing font asset if one already exists at the output path.                                |
| `WriteMissingCharactersFile` | Outputs a `.missing.txt` file if some characters could not be added to the font asset.                                 |

## Recommended WebGL Settings

For Japanese subset fonts, the following settings are a reasonable starting point:

```text
SamplingPointSize: 90
Padding: 9
AtlasWidth: 4096
AtlasHeight: 4096
RenderMode: SDFAA
EnableMultiAtlasSupport: true
SetStaticAfterGeneration: true
```

If characters are missing, increase the atlas size, enable multi-atlas support, or reduce the number of included characters.

## Character Collection

The generator collects characters from the following sources:

1. `AdditionalCharacters`
2. `CharacterTextAssets`
3. Files found under `ScanFolders`
4. Half-width and full-width spaces

Control characters are ignored.

The collected characters are deduplicated and sorted before being added to the generated font asset.

## Missing Characters

If TextMesh Pro cannot add some characters, the tool writes them to a file next to the generated font asset.

Example:

```text
Assets/TextMeshAssetGenerator/FontAssets/JapaneseSubset SDF.missing.txt
```

Use this file to check whether the source font lacks specific glyphs or whether the atlas is too small.

## Notes

* The generated font asset is intended for TextMesh Pro.
* This tool should be placed under an `Editor` folder because it depends on `UnityEditor`.
* Static font assets are recommended when all required text is known before build time.
* Dynamic font assets may require the source font to be included in the build.
* For WebGL builds, using a generated subset font asset can significantly reduce build size compared to embedding a full Japanese font.

## Typical Workflow

```text
Update scenario JSON / UI files
        ↓
Select FontAssetBuildSettings
        ↓
Run Generate Font Asset From Selected Settings
        ↓
Check missing characters if any
        ↓
Build WebGL
```

## Example Folder Structure

```text
Assets/
  TextMeshAssetGenerator/
    SourceFonts/
      NotoSansJP-Regular.ttf
    FontAssets/
      JapaneseSubset SDF.asset
      JapaneseSubset SDF.missing.txt
    Editor/
      FontAssetBuildSettings.cs
      FontAssetAutoBuilder.cs
```

## Troubleshooting

### Japanese text is displayed as squares

The required characters are not included in the generated font asset.

Check:

* Whether the text file or JSON file is included in `ScanFolders`
* Whether the file extension is included in `ScanExtensions`
* Whether the source font supports the missing characters
* Whether a `.missing.txt` file was generated

### The generated font asset is too large

Try reducing the number of scanned files or separating font assets by chapter, screen, or language.

For example:

```text
Chapter01_Font.asset
Chapter02_Font.asset
CommonUI_Font.asset
```

### Font generation fails because the output path is invalid

`OutputAssetPath` must:

* Start with `Assets/`
* End with `.asset`

Example:

```text
Assets/TextMeshAssetGenerator/FontAssets/JapaneseSubset SDF.asset
```

### Some glyphs are missing even though the source font supports them

The atlas may be too small.

Try:

* Increasing `AtlasWidth`
* Increasing `AtlasHeight`
* Enabling `EnableMultiAtlasSupport`
* Reducing the number of included characters

## License Notes

Make sure the source font license allows embedding, redistribution, and use in games or WebGL builds.

Open-source fonts such as Noto Sans JP are commonly used for Japanese text, but the license should still be checked before distribution.
