---
name: save-work-context
description: Save current work context before ending a task or exiting the current work session. Use when the user asks to save context, save handoff notes, preserve progress for later, or prepare a resume file before stopping.
---

# Save Work Context

Use this skill when the user wants the current task state written to disk so work can be resumed later with minimal friction.

## Goal

Create a short, concrete handoff file that captures:

- what the task is
- what has already been done
- what changed in the workspace
- what is still blocked or unfinished
- the exact next steps to resume work

## Output Location

Prefer an existing project handoff/context directory if one already exists.

If none exists, write the file to:

`{project-root}/.codex/handoffs/`

Filename format:

`YYYY-MM-DD-HHMM-current-work-context.md`

Use local project time if available; otherwise use the current session time.

## Workflow

1. Inspect the current workspace state narrowly.
2. Identify files changed for the current task.
3. Summarize the active objective and current status.
4. Record blockers, open questions, and risks.
5. Write a resume-ready handoff file to disk.

## Optional Automation

For this repository, you can automate save-on-exit with:

- `scripts/codex/save-work-context.ps1`
- `scripts/codex/start-codex-with-autosave.ps1`

Use the wrapper script to start Codex so a handoff file is always written when the process exits.

## Minimum Contents

The saved file should contain these sections:

```md
# Current Work Context

## Task
- One or two sentences describing the user's goal.

## Status
- Current state: not started / in progress / partially verified / blocked / completed.
- Short summary of what is already done.

## Files
- Relevant files touched, inspected, or expected to change.

## Changes Made
- Concrete edits already applied.

## Verification
- Commands run, tests executed, or checks performed.
- If nothing was verified, say so plainly.

## Open Items
- Remaining work, blockers, assumptions, or decisions still pending.

## Next Steps
1. The next action to take.
2. The next action after that.
3. Any follow-up verification needed.

## Resume Prompt
- A short prompt that another agent can use to continue from this exact state.
```

## Writing Rules

- Keep it concise and execution-focused.
- Prefer facts from the workspace over guesses.
- Include exact file paths when they matter.
- Do not dump large logs or long diffs.
- If no code changes were made, state that clearly.
- If the task is blocked by missing user input, say exactly what is missing.

## Resume Prompt Pattern

End the file with a compact resume prompt, for example:

`Continue the task from this handoff file, inspect the listed files first, then complete the remaining next steps and verify the result.`

## Template

If you want a starting structure, use:

`assets/context-handoff-template.md`
