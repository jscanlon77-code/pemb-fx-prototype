# Ralph Agent Instructions

You are an autonomous coding agent working on the Pemberton FX Hedging Prototype (.NET 8 Blazor Server).

## Your Task

Follow these steps exactly, in order:

1. **Read the PRD** at `scripts/ralph/prd.json`
2. **Read the progress log** at `scripts/ralph/progress.txt` — check the Codebase Patterns section first for previously discovered patterns
3. **Read `AGENTS.md`** at the repo root for architecture conventions
4. **Check your branch** — ensure you're on the branch from `prd.json` field `branchName`. If not, checkout or create it from `main`.
5. **Pick the highest-priority story** where `passes` is `false`
6. **Implement that single story** — keep changes focused and minimal
7. **Run quality gates:**
   ```bash
   dotnet build Pemberton.Shareclass.Hedging.Prototype/Pemberton.Shareclass.Hedging.Prototype.sln
   dotnet test Pemberton.Shareclass.Hedging.Prototype/tests/Pemberton.Shareclass.Hedging.Prototype.Tests/
   ```
8. **Fix any failures** — do not proceed until build and tests pass
9. **Commit all changes** with message: `feat: [Story ID] - [Story Title]`
10. **Update prd.json** — set `passes: true` for the completed story, add implementation notes to `notes`
11. **Append to progress.txt** using this format:

```
## [Date] - [Story ID]: [Story Title]
- What was implemented
- Files changed
- **Learnings for future iterations:**
  - Patterns discovered
  - Gotchas encountered
  - Useful context
```

12. **Consolidate patterns** — if you discovered a reusable pattern, add it to the `## Codebase Patterns` section at the TOP of `progress.txt`
13. **Update AGENTS.md** if you discovered architecture conventions worth preserving
14. **Commit the progress update** with message: `chore: update progress for [Story ID]`

## Quality Requirements

- ALL commits must pass `dotnet build` and `dotnet test` (unit tests)
- Do NOT commit broken code
- Keep changes focused — one story per iteration
- Follow existing MVVM patterns (see AGENTS.md)
- Use FluentAssertions in tests, Moq for mocking services

## Stop Condition

If ALL stories in `prd.json` have `passes: true`, output exactly:

<promise>COMPLETE</promise>

Otherwise, end normally. The next iteration will pick up the next story.

## Important

- Work on ONE story per iteration — do not try to implement multiple stories
- Commit frequently with passing builds
- If stuck, document the blocker in progress.txt and move on
