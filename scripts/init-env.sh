#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
mode="${1:-local}"
if [[ -e .env ]]; then echo 'ERROR: .env exists; refusing to overwrite' >&2; exit 1; fi
case "$mode" in
    local) source_template='.env.local.example' ;;
    azure-vm) source_template='.env.azure-vm.example' ;;
    *) echo 'Usage: scripts/init-env.sh [local|azure-vm]' >&2; exit 2 ;;
esac
command -v openssl >/dev/null || { echo 'Install openssl before running' >&2; exit 1; }
cp "$source_template" .env
chmod 600 .env
substitute() {
    local from="$1" to="$2"
    sed -i "s|${from}|${to}|g" .env
}
substitute REPLACE_JWT_SECRET "$(openssl rand -hex 48)"
substitute REPLACE_MYSQL_PASSWORD "$(openssl rand -hex 24)"
substitute REPLACE_MYSQL_ROOT_PASSWORD "$(openssl rand -hex 24)"
substitute REPLACE_POSTGRES_PASSWORD "$(openssl rand -hex 24)"
substitute REPLACE_BOOTSTRAP_ADMIN_PASSWORD "Grotix@$(openssl rand -hex 16)"
substitute REPLACE_RABBITMQ_PASSWORD "$(openssl rand -hex 24)"
if grep -q 'REPLACE_' .env; then echo 'Unresolved placeholder in .env' >&2; exit 1; fi
printf '%s\n' 'Created .env with randomly generated secrets; never commit or print it.'
if [[ "$mode" == azure-vm ]]; then
    echo 'MUST edit GROTIX_DOMAIN, CORS_ALLOWED_ORIGIN and BOOTSTRAP_ADMIN_EMAIL before Azure deployment.'
fi
