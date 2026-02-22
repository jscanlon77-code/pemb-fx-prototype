#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
PRD_FILE="$SCRIPT_DIR/prd.json"
PROGRESS_FILE="$SCRIPT_DIR/progress.txt"
LAST_BRANCH_FILE="$SCRIPT_DIR/.last-branch"
MAX_ITERATIONS=10

# Parse arguments
if [[ "$1" =~ ^[0-9]+$ ]]; then
  MAX_ITERATIONS=$1
fi

# ── Pre-flight checks ──────────────────────────────────────────────

if ! command -v claude &> /dev/null; then
  echo "Error: 'claude' CLI not found. Install Claude Code first."
  exit 1
fi

if ! command -v jq &> /dev/null; then
  echo "Error: 'jq' not found. Install with: brew install jq"
  exit 1
fi

if [ ! -f "$PRD_FILE" ]; then
  echo "Error: No prd.json found at $PRD_FILE"
  echo "Copy prd.json.example and customise it, or use the /ralph skill to generate one."
  exit 1
fi

echo "── Pre-flight: dotnet build ──"
if ! dotnet build "$REPO_ROOT/Pemberton.Shareclass.Hedging.Prototype/Pemberton.Shareclass.Hedging.Prototype.sln" --nologo -v q; then
  echo "Error: Build failed. Fix build errors before running Ralph."
  exit 1
fi
echo "── Build passed ──"

# ── Archive previous run if branch changed ──────────────────────────

BRANCH_NAME=$(jq -r '.branchName' "$PRD_FILE")

if [ -f "$LAST_BRANCH_FILE" ]; then
  LAST_BRANCH=$(cat "$LAST_BRANCH_FILE")
  if [ "$LAST_BRANCH" != "$BRANCH_NAME" ]; then
    ARCHIVE_DIR="$SCRIPT_DIR/archive/$(date +%Y-%m-%d)-$LAST_BRANCH"
    echo "Branch changed from $LAST_BRANCH to $BRANCH_NAME — archiving previous run."
    mkdir -p "$ARCHIVE_DIR"
    cp "$PRD_FILE" "$ARCHIVE_DIR/prd.json" 2>/dev/null || true
    cp "$PROGRESS_FILE" "$ARCHIVE_DIR/progress.txt" 2>/dev/null || true
  fi
fi

echo "$BRANCH_NAME" > "$LAST_BRANCH_FILE"

# ── Initialise progress.txt if missing ──────────────────────────────

if [ ! -f "$PROGRESS_FILE" ]; then
  cat > "$PROGRESS_FILE" << 'EOF'
# Ralph Progress Log
## Codebase Patterns
<!-- Reusable patterns discovered across iterations — update this section, don't append -->

---
<!-- Iteration logs below — always append, never replace -->
EOF
fi

# ── Main loop ───────────────────────────────────────────────────────

echo ""
echo "═══════════════════════════════════════════"
echo "  Ralph — Autonomous Agent Loop"
echo "  PRD: $(jq -r '.project' "$PRD_FILE")"
echo "  Branch: $BRANCH_NAME"
echo "  Max iterations: $MAX_ITERATIONS"
echo "═══════════════════════════════════════════"
echo ""

for i in $(seq 1 $MAX_ITERATIONS); do
  REMAINING=$(jq '[.userStories[] | select(.passes == false)] | length' "$PRD_FILE")
  TOTAL=$(jq '.userStories | length' "$PRD_FILE")
  DONE=$((TOTAL - REMAINING))

  echo "── Iteration $i of $MAX_ITERATIONS  ($DONE/$TOTAL stories complete) ──"

  OUTPUT=$(claude --dangerously-skip-permissions --print < "$SCRIPT_DIR/CLAUDE.md" 2>&1 | tee /dev/stderr) || true

  if echo "$OUTPUT" | grep -q "<promise>COMPLETE</promise>"; then
    echo ""
    echo "═══════════════════════════════════════════"
    echo "  Ralph completed all stories!"
    echo "═══════════════════════════════════════════"
    exit 0
  fi

  echo ""
  echo "── Iteration $i finished. Pausing before next iteration... ──"
  sleep 2
done

echo ""
echo "Reached max iterations ($MAX_ITERATIONS) without completing all stories."
echo "Run again with: ./ralph.sh $((MAX_ITERATIONS + 5))"
exit 1
