#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PRD_FILE="$SCRIPT_DIR/prd.json"

# ── Validate environment ──────────────────────────────────────────

if [ -z "${ANTHROPIC_API_KEY:-}" ]; then
  echo "Error: ANTHROPIC_API_KEY is not set."
  echo "Pass it to the container with: docker run -e ANTHROPIC_API_KEY=\$ANTHROPIC_API_KEY ..."
  exit 1
fi

# ── Configure git for in-container commits ────────────────────────

git config --global user.name "${GIT_USER_NAME:-Ralph Agent}"
git config --global user.email "${GIT_USER_EMAIL:-ralph@pemberton.dev}"

# Mark /workspace as safe (mounted or copied repo)
git config --global --add safe.directory /workspace

# ── Configure remote with GitHub token if available ───────────────

if [ -n "${GITHUB_TOKEN:-}" ]; then
  REMOTE_URL=$(git remote get-url origin 2>/dev/null || true)
  if [ -n "$REMOTE_URL" ]; then
    # Rewrite https://github.com/... to https://x-access-token:TOKEN@github.com/...
    NEW_URL=$(echo "$REMOTE_URL" | sed -E 's|https://([^@]*@)?github\.com/|https://x-access-token:'"$GITHUB_TOKEN"'@github.com/|')
    git remote set-url origin "$NEW_URL"
    echo "Git remote configured with GITHUB_TOKEN."
  fi
fi

# ── Checkout PRD branch ───────────────────────────────────────────

if [ ! -f "$PRD_FILE" ]; then
  echo "Error: prd.json not found at $PRD_FILE"
  exit 1
fi

BRANCH_NAME=$(jq -r '.branchName' "$PRD_FILE")

if [ -z "$BRANCH_NAME" ] || [ "$BRANCH_NAME" = "null" ]; then
  echo "Error: branchName not set in prd.json"
  exit 1
fi

CURRENT_BRANCH=$(git branch --show-current 2>/dev/null || true)

if [ "$CURRENT_BRANCH" != "$BRANCH_NAME" ]; then
  echo "Checking out branch: $BRANCH_NAME"
  git checkout "$BRANCH_NAME" 2>/dev/null || git checkout -b "$BRANCH_NAME"
fi

# ── Export Docker environment flag ────────────────────────────────

export RALPH_DOCKER=1

# ── Hand off to ralph.sh ──────────────────────────────────────────

echo "Starting Ralph agent loop..."
exec "$SCRIPT_DIR/ralph.sh" "$@"
