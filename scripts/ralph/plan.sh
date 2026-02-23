#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
PRD_FILE="$SCRIPT_DIR/prd.json"

# ── Pre-flight checks ──────────────────────────────────────────────

if ! command -v claude &> /dev/null; then
  echo "Error: 'claude' CLI not found. Install Claude Code first."
  exit 1
fi

if [ ! -f "$PRD_FILE" ]; then
  echo "Error: No prd.json found at $PRD_FILE"
  echo "Copy prd.json.example and customise it, or use the /ralph skill to generate one."
  exit 1
fi

# ── Launch Claude in plan mode ────────────────────────────────────

BRANCH_NAME=$(jq -r '.branchName' "$PRD_FILE" 2>/dev/null || echo "unknown")

echo ""
echo "═══════════════════════════════════════════"
echo "  Ralph — Plan Mode (Opus)"
echo "  PRD: $(jq -r '.project' "$PRD_FILE" 2>/dev/null || echo "unknown")"
echo "  Branch: $BRANCH_NAME"
echo "═══════════════════════════════════════════"
echo ""

claude --model claude-opus-4-6 "$@"
