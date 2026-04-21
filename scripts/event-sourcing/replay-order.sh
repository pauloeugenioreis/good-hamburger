#!/usr/bin/env bash
set -euo pipefail

API_URL=${API_URL:-http://localhost:8080}
ENTITY_ID=${1:-}
ENTITY_TYPE=${2:-Order}

if [[ -z "$ENTITY_ID" ]]; then
  echo "Usage: $0 <aggregateId> [entityType=Order]" >&2
  echo "Example: ./scripts/event-sourcing/replay-order.sh 1 Order" >&2
  exit 1
fi

AUDIT_URL="${API_URL}/api/Audit/${ENTITY_TYPE}/${ENTITY_ID}"

echo "📜 Fetching history for ${ENTITY_TYPE}/${ENTITY_ID} from ${AUDIT_URL}" >&2
curl -fsSL "$AUDIT_URL" | jq '.'

echo

echo "⏪ Replaying aggregate using POST ${AUDIT_URL}/replay" >&2
curl -fsSL -X POST "${AUDIT_URL}/replay" | jq '.'
