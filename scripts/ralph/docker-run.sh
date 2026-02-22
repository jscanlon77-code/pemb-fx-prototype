#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
PRD_FILE="$SCRIPT_DIR/prd.json"

IMAGE_NAME="ralph-sandbox"
MAX_ITERATIONS=""
NO_PUSH=""

# ── Usage ────────────────────────────────────────────────────────────

usage() {
  cat <<EOF
Usage: $(basename "$0") [OPTIONS]

Build and run the Ralph autonomous agent in a Docker container.

Options:
  --max-iterations <n>  Maximum number of Ralph iterations (default: 10)
  --no-push             Do not push results branch on completion
  --help                Show this help message

Environment variables:
  ANTHROPIC_API_KEY     (required) API key for Claude
  GITHUB_TOKEN          (optional) GitHub token for pushing results

Examples:
  ./scripts/ralph/docker-run.sh
  ./scripts/ralph/docker-run.sh --max-iterations 5
  ./scripts/ralph/docker-run.sh --no-push
EOF
  exit 0
}

# ── Parse arguments ──────────────────────────────────────────────────

while [[ $# -gt 0 ]]; do
  case "$1" in
    --max-iterations)
      if [[ -z "${2:-}" ]] || ! [[ "$2" =~ ^[0-9]+$ ]]; then
        echo "Error: --max-iterations requires a numeric argument"
        exit 1
      fi
      MAX_ITERATIONS="$2"
      shift 2
      ;;
    --no-push)
      NO_PUSH="1"
      shift
      ;;
    --help)
      usage
      ;;
    *)
      echo "Error: Unknown option '$1'"
      echo "Run with --help for usage information."
      exit 1
      ;;
  esac
done

# ── Validate environment ────────────────────────────────────────────

if [ -z "${ANTHROPIC_API_KEY:-}" ]; then
  echo "Error: ANTHROPIC_API_KEY is not set."
  echo "Export it before running: export ANTHROPIC_API_KEY=sk-..."
  exit 1
fi

# ── Read branch name from PRD ───────────────────────────────────────

if [ ! -f "$PRD_FILE" ]; then
  echo "Error: prd.json not found at $PRD_FILE"
  exit 1
fi

BRANCH_NAME=$(jq -r '.branchName' "$PRD_FILE")
PROJECT_NAME=$(jq -r '.project' "$PRD_FILE")

# ── Build Docker image ──────────────────────────────────────────────

echo ""
echo "═══════════════════════════════════════════"
echo "  Ralph Docker Runner"
echo "  Project: $PROJECT_NAME"
echo "  Branch:  $BRANCH_NAME"
echo "═══════════════════════════════════════════"
echo ""

echo "Building Docker image: $IMAGE_NAME ..."
docker build -f "$SCRIPT_DIR/Dockerfile" -t "$IMAGE_NAME" "$REPO_ROOT"
echo "Build complete."
echo ""

# ── Run container ───────────────────────────────────────────────────

DOCKER_ARGS=(
  run --rm
  -e "ANTHROPIC_API_KEY=$ANTHROPIC_API_KEY"
)

if [ -n "${GITHUB_TOKEN:-}" ]; then
  DOCKER_ARGS+=(-e "GITHUB_TOKEN=$GITHUB_TOKEN")
fi

if [ -n "$NO_PUSH" ]; then
  DOCKER_ARGS+=(-e "RALPH_NO_PUSH=1")
fi

DOCKER_ARGS+=("$IMAGE_NAME")

if [ -n "$MAX_ITERATIONS" ]; then
  DOCKER_ARGS+=("$MAX_ITERATIONS")
fi

echo "Starting Ralph container..."
echo ""

EXIT_CODE=0
docker "${DOCKER_ARGS[@]}" || EXIT_CODE=$?

# ── Summary banner ──────────────────────────────────────────────────

echo ""
echo "═══════════════════════════════════════════"
if [ "$EXIT_CODE" -eq 0 ]; then
  echo "  Ralph finished successfully"
else
  echo "  Ralph exited with code $EXIT_CODE"
fi
echo "  Branch: $BRANCH_NAME"
echo "═══════════════════════════════════════════"
echo ""

exit $EXIT_CODE
