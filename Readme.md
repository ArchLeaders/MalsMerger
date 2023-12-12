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

---

# Usage

```
<Command> <Parameters> [Options]
```

# Commands

## Merge Mods
> Aliases: `merge`, `merge-mods`

```
<Input(s)> <Output-Mod-Folder>
```

Merges the specified mods or changelogs and places the merged file(s) canonically in the output mod folder.

### Inputs
> `Paths`

Bar (`|`) seperated list of the input mod folders or changelogs.

*[Priority: highest to lowest <-> left to right]*

#### Inputs Parameter Notes

- This should be one argument surounded by quotes. Quotation marks should NOT be around each path (see examples)
- Changelogs must be named using the pattern: "XXzz.Product.json"

### Output Mod Folder
> `Path`

The path to the output mod folder.

---

## Generate Changelogs
> Aliases: `gen`, `gen-chlgs`, `gen-changelogs`

```
<Input(s)> <Output-Mod-Folder> [-f|--format]
```

Merges the specified mods or changelogs and places the merged file(s) canonically in the output mod folder.

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