param([string]$BaseUrl = 'http://localhost:5100', [switch]$OpenApi)
$ErrorActionPreference = 'Stop'
$BaseUrl = $BaseUrl.TrimEnd('/')
$paths = @('/live', '/ready/core', '/ready/telemetry')
if ($OpenApi) { $paths += @('/docs/profiles/openapi.json', '/docs/irrigation/openapi.json') }
foreach ($path in $paths) {
    Write-Host "GET $BaseUrl$path"
    $result = Invoke-WebRequest -Uri "$BaseUrl$path" -TimeoutSec 15 -UseBasicParsing
    Write-Host "HTTP $($result.StatusCode)"
    if ($result.StatusCode -ne 200) { throw "Endpoint failed: $path" }
}
Write-Host 'Smoke checks OK (no equivale a integración real).'
