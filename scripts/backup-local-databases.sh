#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
if [[ ! -f .env ]]; then echo 'Missing .env' >&2; exit 1; fi
mkdir -p backups
chmod 700 backups
stamp="$(date -u +%Y%m%dT%H%M%SZ)"
mysql_file="backups/grotix-mysql-${stamp}.sql"
pg_file="backups/grotix-postgres-${stamp}.sql"
set -o pipefail
docker compose -f docker-compose.yml -f docker-compose.databases.yml exec -T mysql sh -c 'MYSQL_PWD="$MYSQL_ROOT_PASSWORD" mysqldump -h 127.0.0.1 -u root --single-transaction --databases "$MYSQL_DATABASE"' > "$mysql_file"
docker compose -f docker-compose.yml -f docker-compose.databases.yml exec -T postgres sh -c 'PGPASSWORD="$POSTGRES_PASSWORD" pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB"' > "$pg_file"
chmod 600 "$mysql_file" "$pg_file"
echo "Saved private backups in backups/ (${stamp}). Store them off-VM for disaster recovery."
