$ErrorActionPreference = 'Stop'
$project = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$dll = Join-Path $project 'bin\Release\netstandard2.1\BetterZoom.dll'
$report = Get-Content -LiteralPath (Join-Path $project 'Verification\runtime-tests.json') -Raw | ConvertFrom-Json
$compat = Get-Content -LiteralPath (Join-Path $project 'Verification\compatibility-tests.json') -Raw | ConvertFrom-Json
[xml]$metadata = Get-Content -LiteralPath (Join-Path $project 'BetterZoom.csproj') -Raw
$version = $metadata.Project.PropertyGroup.Version
$dllHash = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash.ToLowerInvariant()
if (!$report.passed -or $report.version -ne $version -or $report.dllSha256 -ne $dllHash) { throw 'Build must match passing isolated runtime checks.' }
if (!$compat.passed -or $compat.version -ne $version -or $compat.dllSha256 -ne $dllHash) { throw 'Build must match passing companion runtime checks.' }
$release = Join-Path $project 'Releases'
New-Item -ItemType Directory -Path $release -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zipPath = Join-Path $release "KingdomInc-BetterZoom-$version.zip"
if (Test-Path -LiteralPath $zipPath) { throw "Release already exists: $zipPath" }
$entries = [ordered]@{
    'BepInEx/plugins/BetterZoom/BetterZoom.dll' = $dll
    'Better Zoom README.txt' = (Join-Path $project 'README.md')
}
$archive = [IO.Compression.ZipFile]::Open($zipPath, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($entry in $entries.GetEnumerator()) { [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$entry.Value,$entry.Key) | Out-Null }
} finally { $archive.Dispose() }
$extracted = Join-Path $project ('Verification\Package-' + [Guid]::NewGuid().ToString('N'))
[IO.Compression.ZipFile]::ExtractToDirectory($zipPath, $extracted)
$actualFiles = @(Get-ChildItem -LiteralPath $extracted -Recurse -File)
if ($actualFiles.Count -ne $entries.Count) { throw 'Unexpected archive entries.' }
foreach ($entry in $entries.GetEnumerator()) {
    if ((Get-FileHash -LiteralPath (Join-Path $extracted $entry.Key)).Hash -ne (Get-FileHash -LiteralPath $entry.Value).Hash) { throw "Clean extraction differs: $($entry.Key)" }
}
$record = [ordered]@{
    version = $version; archive = $zipPath; bytes = (Get-Item -LiteralPath $zipPath).Length
    sha256 = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
    dllSha256 = $dllHash; runtimeChecksPassed = $report.count; cleanExtractionPassed = $true
    compatibilityChecksPassed = $compat.count; compatibilityPlugins = $compat.plugins
    gameVersion = $report.gameVersion; unityVersion = $report.unityVersion; bepInExVersion = $report.bepInExVersion
    gameAssemblySha256 = (Get-FileHash -LiteralPath (Join-Path $project '..\..\KingdomInc_Data\Managed\Assembly-CSharp.dll')).Hash.ToLowerInvariant()
    publication = 'Local only'
}
$record | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $release 'RELEASE_VERIFICATION.json') -Encoding utf8
$record | ConvertTo-Json

