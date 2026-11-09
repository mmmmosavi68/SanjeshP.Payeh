param(
    [Parameter(Mandatory = $true)]
    [string]$BackendRoot
)

$ErrorActionPreference = 'Stop'
$sourceRoot = $PSScriptRoot
$files = @(
    'SanjeshP.Payeh.Data\Contracts\Ipermissionservice.cs',
    'SanjeshP.Payeh.Data\Repositories\Permissionservice.cs',
    'SanjeshP.Payeh.Api\Controllers\v1\PermissionsController.cs',
    'SanjeshP.Payeh.Api\Models\Auth\MenuPermissionDto.cs',
    'SanjeshP.Payeh.Api\DefaultData\Menu.json',
    'SanjeshP.Payeh.Api\DefaultData\UserAccessMenus.json'
)

foreach ($relativePath in $files) {
    $source = Join-Path $sourceRoot $relativePath
    $destination = Join-Path $BackendRoot $relativePath
    $destinationDirectory = Split-Path -Parent $destination
    New-Item -ItemType Directory -Force -Path $destinationDirectory | Out-Null
    Copy-Item -LiteralPath $source -Destination $destination -Force
    Write-Host "Updated $relativePath"
}

Write-Host 'Backend permission patch copied. Review changes, then run dotnet build.' -ForegroundColor Green
