# Global usings

Read and follow the project skill at **`.cursor/skills/gusings/SKILL.md`** for this entire task. Treat that file as the source of truth for how to identify eligible `using` directives, how to create/update `GlobalUsings.cs` per project, what to exclude, and how to verify.

## What to do

1. Find all `.csproj` projects in the repo and process them one-by-one.
2. For each project, create or update `GlobalUsings.cs` in the project root and move eligible file-scoped `using` directives into it as `global using ...;`.
3. Remove only the moved usings from source files; avoid unrelated formatting.
4. Run `dotnet build` (solution or touched projects) and fix any breakages you introduce.

## If the user added extra instructions in chat

Combine those instructions with the skill; the skill wins on conflicts about eligibility, exclusions, and verification unless the user explicitly overrides for this request.

