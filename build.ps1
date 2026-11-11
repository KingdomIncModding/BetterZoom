param(
    [string]$GameRoot = (Join-Path $PSScriptRoot '..\..\..')
)

$ErrorActionPreference = 'Stop'
$gamePath = (Resolve-Path -LiteralPath $GameRoot).Path.TrimEnd('\', '/')
$gamePathForBuild = $gamePath.Replace('\', '/') + '/'

& dotnet build (Join-Path $PSScriptRoot 'BetterZoom.csproj') -c Release "-p:GameRoot=$gamePathForBuild"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Built: $(Join-Path $PSScriptRoot 'bin\Release\netstandard2.1\BetterZoom.dll')"
