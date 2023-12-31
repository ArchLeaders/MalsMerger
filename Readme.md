# Mals Merger

[![License](https://img.shields.io/badge/License-MIT-blue.svg?color=663f9e&style=for-the-badge)](https://github.com/ArchLeaders/MalsMerger/blob/master/License.md) [![Downloads](https://img.shields.io/github/v/tag/ArchLeaders/MalsMerger?label=Release&logo=GitHub&color=b33126&style=for-the-badge)](https://github.com/ArchLeaders/MalsMerger/releases)

Simple CLI tool for merging TotK `Mals` archives

---

- [Mals Merger](#mals-merger)
- [Usage](#usage)
- [Commands](#commands)
  - [Merge Mods](#merge-mods)
    - [Inputs](#inputs)
      - [Inputs Parameter Notes](#inputs-parameter-notes)
    - [Output Mod Folder](#output-mod-folder)
  - [Generate Changelogs](#generate-changelogs)
    - [Inputs](#inputs-1)
      - [Inputs Parameter Notes](#inputs-parameter-notes-1)
    - [Output Mod Folder](#output-mod-folder-1)
- [Global Options](#global-options)
  - [Log File Path](#log-file-path)
  - [Verbose Logging](#verbose-logging)
- [Examples](#examples)
  - [Generate Changelogs from Mod Folders](#generate-changelogs-from-mod-folders)
  - [Merge Mod Folders](#merge-mod-folders)

---

# Usage

```
<Command> <Parameters> [Options]
```

For help information use `-h` or `--help`.

# Commands

## Merge Mods
> Aliases: `merge`, `merge-mods`

```
<Input(s)> <Output-Mod-Folder>
```

Merges the specified mods places the merged file(s) canonically in the output mod folder.

If a changelog is found in the mod folder (`XXzz.Product.0.json`) it will be used, but only if no Mals archive for that localization exists.

### Inputs
> `Paths`

Bar (`|`) seperated list of the input mod folders.

*[Priority: highest to lowest <-> left to right]*

#### Inputs Parameter Notes

- This should be one argument surounded by quotes. Quotation marks should NOT be around each path (see examples).
- Changelogs must be named using the pattern: `XXzz.Product.0.json`

### Output Mod Folder
> `Path`

The path to the output mod folder.

---

## Generate Changelogs
> Aliases: `gen`, `gen-chlgs`, `gen-changelogs`

```
<Input(s)> <Output-Mod-Folder> [-f|--format]
```

Generates changelogs for each specified mod and places the changelog(s) canonically in the output mod folder.

### Inputs
> `Paths`

Bar (`|`) seperated list of the input mod folders or changelogs.

*[Priority: highest to lowest <-> left to right]*

#### Inputs Parameter Notes

- This should be one argument surounded by quotes. Quotation marks should ***NOT*** be around each path (see examples)

### Output Mod Folder
> `Path`

The path to the output mod folder.

# Global Options

## Log File Path
> `Path`

Specify a path to write logs to (logging disabled by default)

## Verbose Logging
> `Flag`

Enable verbose logging

# Examples

## Generate Changelogs from Mod Folders

```
gen "path/to/mod_a|path/to/mod_b" "path/to/output" --verbose
```

## Merge Mod Folders

```
merge "path/to/mod_a|path/to/mod_b" "path/to/output" -l "path/to/log.txt"
```
