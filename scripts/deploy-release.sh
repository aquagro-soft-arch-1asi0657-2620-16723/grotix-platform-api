#!/usr/bin/env bash
# Run on the Azure VM after GHCR image release. No DB schema changes are applied.
set -euo pipefail
cd "$(dirname "$0")/.."
if [[ $# != 2 ]]; then echo 'Usage: scripts/deploy-release.sh ghcr.io/OWNER/grotix- vX.Y.Z' >&2; exit 2; fi
prefix="$1" tag="$2"
if [[ ! "$prefix" =~ ^ghcr\.io/[a-z0-9._-]+/grotix-$ ]]; then
    echo 'Invalid GHCR image prefix' >&2; exit 2
fi
if [[ ! "$tag" =~ ^v[0-9]+\.[0-9]+\.[0-9]+(-[0-9a-zA-Z.-]+)?$ ]]; then
    echo 'Image tag must be a semantic version vX.Y.Z[-prerelease]' >&2; exit 2
fi
if [[ ! -f .env ]]; then echo 'Missing VM .env' >&2; exit 1; fi
mode="$(grep -E '^GROTIX_DATABASE_MODE=' .env | tail -1 | cut -d= -f2 | tr -d '\r')"
case "$mode" in
    vm) files=(-f docker-compose.yml -f docker-compose.databases.yml) ;;
    external) files=(-f docker-compose.yml) ;;
    *) echo 'Set GROTIX_DATABASE_MODE=vm or external in .env' >&2; exit 1 ;;
esac
if grep -Eq '(^GROTIX_DOMAIN=api\.example\.com$|^CORS_ALLOWED_ORIGIN=https://app\.example\.com$|^JWT_SECRET=REPLACE_ME)' .env; then
    echo 'Production environment contains placeholder domain, CORS or JWT. Edit .env.' >&2; exit 1
fi
export IMAGE_PREFIX="$prefix" IMAGE_TAG="$tag"
docker compose "${files[@]}" --profile public config --quiet
# No pull of databases, which need their own backup / version-change schedule.
services=(profiles cultivation telemetry hardware irrigation gateway caddy)
echo "Pulling Grotix API release $tag"
docker compose "${files[@]}" --profile public pull "${services[@]}"
echo 'Restarting application services only; no database volumes removed'
docker compose "${files[@]}" --profile public up -d --no-deps --no-build "${services[@]}"
docker compose "${files[@]}" --profile public ps
port="$(grep -E '^GATEWAY_PORT=' .env | tail -1 | cut -d= -f2 | tr -d '\r')"
port="${port:-5100}"
for n in $(seq 1 20); do
    if curl -fsS --max-time 5 "http://127.0.0.1:${port}/live" >/dev/null 2>&1 &&
       curl -fsS --max-time 5 "http://127.0.0.1:${port}/ready/core" >/dev/null 2>&1 &&
       curl -fsS --max-time 5 "http://127.0.0.1:${port}/ready/telemetry" >/dev/null 2>&1; then
        echo "Grotix release ${tag} is locally ready; verify PUBLIC HTTPS and authenticated endpoints separately."
        exit 0
    fi
    sleep 3
done
 echo 'Readiness did not pass. Run docker compose logs and consider rollback.' >&2
exit 1
