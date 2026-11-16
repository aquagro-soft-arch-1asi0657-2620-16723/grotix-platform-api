#!/usr/bin/env bash
set -euo pipefail
BASE_URL="${1:-http://localhost:5100}"
BASE_URL="${BASE_URL%/}"
echo "Gateway: $BASE_URL"
check() {
  local path="$1"
  echo "GET $path"
  curl --fail --silent --show-error --max-time 15 "$BASE_URL$path"
  echo
}
check /live
check /ready/core
check /ready/telemetry
if [[ "${CHECK_OPENAPI:-false}" == "true" ]]; then
  check /docs/profiles/openapi.json
  check /docs/irrigation/openapi.json
fi
echo "Smoke checks OK. Esto NO reemplaza pruebas HTTP autenticadas ni de flujos RabbitMQ."
