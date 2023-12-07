# Mals Merger (for TCML)

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](License.txt) [![Downloads](https://img.shields.io/github/downloads/ArchLeaders/MalsMerger/total)](https://github.com/ArchLeaders/MalsMerger/releases)

Simpel CLI tool for merging TotK `Mals` archives

---

# Usage

```
<input_folder(s)> <output_folder> [-l|-log LOG_FILE] [-v|--verbose VERBOSE]
```

### Input Folders: (Path)
> Required Argument (1)

Bar (`|`) seperated list of mod folders to merge. *(**Priority**: highest to lowest <-> left to right)*.

> **Note:** This should be one argument surounded by quotes.<br>
> Quotation marks should NOT be around each path (see examples)

> **Note:** Input folders that do not end in `Mals` or do not contain a subfolder named `Mals` will be skipped.

### Output Folder: (Path)
> Required Argument (2)

The output mod folder to export the merged Mals archives to.

### Log File: (Path)
> Optional Flag

Specify a path to write logs to (logging disabled by default).

### Verbose: (Boolean)
> Optional Flag

Enable (very) verbose logging.

## Example Command

```
"path/to/mod_a|path/to/mod_b" "path/to/mod_final" -l mals-merger.log -v true
```