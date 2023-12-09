# Game Version Conflicts

An outline of the issues with multiple game versions, and how it should be handled where versions can't be matched with the game dump.

> **Note:** This only applies to files with a version string in the name, we have no way of determining other file versions so they will be disregarded. 

## Ideal Scenario (Mod Developer)

A mod should ideally consist of mod files based on[^1] every game version (not renamed).

e.g. if a modder makes changes to `1.1.0` and renames to support `1.2.1`, the file is not based off `1.2.1` and checking against the `1.2.1` will not be change-perfect[^2].

## Solutions (Mod Developer)

This is not an issue if the mod developer (or TCML) creates explicit changelogs, removing the need for vanilla file checks.

Using this method means users will be able to create mods in any game version to be used in any game version.

---

## Ideal Scenario (Mod User)

The ideal scenario for mod users is using mods that have files based on[^1] the same version as their game dump.

## Worst Case Scenario (Comparing Files with Varying Versions)

The worst case scenario is that a change between two vanilla files will be tracked as a mod change and overwrite a real mod change that it didn't need to.

## Solutions (Mod User)

* Safety Approach: Warn the mod user that a mod has no files for their game dump (it will still be merged, but may cause errors).
* Track Vanilla Changes: Track each game version's changes to validate a mod change.
  * Mod changes should be compared with changes across all game versions. We can't assume that a file named `1.1.0` is actually for `1.1.0` because mod developers typically rename files to add support.
  * This could cause errors if a mod change matches that of a vanilla change because it will be ignored, when correctly compared with its game version it would be tracked.

### Solution Priority

1. `If` a mod file has a matching version in the game dump, use that.
2. `Else If` check against the vanilla changelogs (unless explicitly disabled?)
3. `Else If` point `2` is not implemented, compare the file with the closest match to the game dump version.
   * e.g. If the mod has `1.2.0` and `1.1.0` and the users game dump version is `1.0.0`, use the mod file for `1.1.0`.

---

[^1]: Based on means that a file is copied from game version `X.X.X` and mod changes are made explicitly to that file, not a file from, e.g. `1.1.0` renamed to work with `1.2.1` (it's still a `1.1.0` file)<br>
      This is a problem because changes between game version `1.1.0` and `1.2.1` (or any other two versions) will be treated as mod changes instead of vanilla changes from another version.
[^2]: Change-perfect means that all changes found in a file are modded changes, not changes between two game versions.