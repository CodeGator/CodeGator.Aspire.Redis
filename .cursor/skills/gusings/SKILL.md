---
name: gusings
description: >-
  Consolidates C# using directives into per-project GlobalUsings.cs files. Use
  when the user asks to move usings into GlobalUsings.cs, add global usings,
  deduplicate using directives, or standardize using placement across projects.
---

# Global usings (per project)

## Goal

For each C# project in the solution, move eligible `using` directives from
project source files into a `GlobalUsings.cs` file in that project.

## Definitions

- **Project root**: The directory containing a given `.csproj`.
- **Global usings file**: `GlobalUsings.cs` in the project root.
- **Eligible using**: A `using` directive that is file-scoped at the top of the
  file (before any namespace/type declarations and not nested), and that is not
  conditional or otherwise risky to lift globally.

## What to move

Move these kinds of using directives to `GlobalUsings.cs` as `global using ...;`:

- `using Namespace;`
- `using static Namespace.Type;`
- `using Alias = Namespace.Type;`

Only move a using if all of the following hold:

- It is file-scoped and appears in the top using block of the file.
- It is not inside `#if/#endif` (or any preprocessor conditional block).
- It is not preceded by a comment that is clearly file-specific licensing or a
  directive that must remain with the file.
- It does not introduce an alias name collision within the project’s global
  using set.

## What to keep in files

Do not move (leave as-is):

- Usings inside a namespace block (`namespace X { using ...; }`).
- Usings that appear after non-using code (attributes, types, top-level code).
- Usings in generated files or tool output.
- Conditional/preprocessor-controlled usings.
- Any using that, if made global, would likely change meaning (for example,
  conflicting aliases, or ambiguous type resolution introduced globally).

## Files to exclude from analysis

Exclude:

- `**/bin/**`, `**/obj/**`, `**/.artifacts/**`
- `**/*.g.cs`, `**/*.generated.cs`, `**/*Designer.cs`
- The project’s own `GlobalUsings.cs` (do not re-ingest it as a source).

## Procedure

1. **Discover projects**
   - Find all `.csproj` files under `src/` (and `tests/` if present) and treat
     each as an independent project.

2. **For each project**
   - Ensure a `GlobalUsings.cs` exists in the project root. Create it if absent.
   - Parse all `.cs` files belonging to the project (excluding the patterns
     above).
   - Extract the top-of-file file-scoped `using` directives that are eligible.
   - Build a **deduplicated** set for the project.

3. **Write `GlobalUsings.cs`**
   - Add `global` in front of each using.
   - Keep output stable and reviewable:
     - Sort alphabetically (ordinal) by the full normalized using text
       (ignoring leading `global `).
     - Keep `using static ...;` grouped with other usings by sorting; do not add
       blank lines unless the file already uses them.
   - Do not add unrelated code or comments.

4. **Edit source files**
   - Remove only the using directives that were moved into global usings.
   - Preserve existing spacing as much as possible; avoid reformatting.
   - If a file ends up with no usings, remove the now-empty using block cleanly.

5. **Verify**
   - Run `dotnet build` for the solution or, at minimum, the touched projects.
   - If build errors occur due to missing usings, restore the smallest necessary
     using locally (file-scoped) rather than making a risky global change.

## Output expectations

- Each project ends with a correct `GlobalUsings.cs`.
- No project compiles worse than before; build must succeed.
- No broad formatting changes; only using movement and minimal whitespace fixes.

