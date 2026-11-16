# Generates a unique, ignored .env for local Docker or an Azure VM; does NOT overwrite.
[CmdletBinding()]
param([ValidateSet('Local', 'AzureVM')][string]$Mode = 'Local')
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$destination = Join-Path $root '.env'
if (Test-Path -LiteralPath $destination) {
    throw '.env already exists. Move/backup the existing .env first; it will NOT be overwritten.'
}
$template = if ($Mode -eq 'Local') { '.env.local.example' } else { '.env.azure-vm.example' }
$source = Join-Path $root $template
function New-HexSecret([int]$bytes) {
    $value = New-Object byte[] $bytes
    $random = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $random.GetBytes($value) } finally { $random.Dispose() }
    return [System.BitConverter]::ToString($value).Replace('-', '').ToLowerInvariant()
}
$values = @{
    'REPLACE_JWT_SECRET' = New-HexSecret 48
    'REPLACE_MYSQL_PASSWORD' = New-HexSecret 24
    'REPLACE_MYSQL_ROOT_PASSWORD' = New-HexSecret 24
    'REPLACE_POSTGRES_PASSWORD' = New-HexSecret 24
    'REPLACE_BOOTSTRAP_ADMIN_PASSWORD' = 'Grotix@' + (New-HexSecret 16)
    'REPLACE_RABBITMQ_PASSWORD' = New-HexSecret 24
}
$contents = [System.IO.File]::ReadAllText($source)
foreach ($key in $values.Keys) { $contents = $contents.Replace($key, $values[$key]) }
if ($contents.Contains('REPLACE_')) { throw 'Unresolved replacement in template.' }
[System.IO.File]::WriteAllText($destination, $contents, (New-Object System.Text.UTF8Encoding $false))
Write-Host 'Created .env with unique random secrets (not printed). Keep this file private.' -ForegroundColor Green
if ($Mode -eq 'AzureVM') {
    Write-Host 'BEFORE deployment: edit GROTIX_DOMAIN, CORS_ALLOWED_ORIGIN and BOOTSTRAP_ADMIN_EMAIL; configure DNS, SSH and Azure NSG.' -ForegroundColor Yellow
} else {
    Write-Host 'Local credentials can be inspected in .env. Remove BOOTSTRAP_ADMIN_* after first successful login.' -ForegroundColor Yellow
}
