param (
    [Parameter(Mandatory = $true)]
    [string] $SourceDirectory,

    [Parameter(Mandatory = $true)]
    [string] $DestinationPath
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$sourcePath = (Resolve-Path -LiteralPath $SourceDirectory).Path
$sourceName = Split-Path -Leaf $sourcePath
$destinationFullPath = [System.IO.Path]::GetFullPath($DestinationPath)
$destinationDirectory = Split-Path -Parent $destinationFullPath

if ($destinationDirectory) {
    New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
}

if (Test-Path -LiteralPath $destinationFullPath) {
    Remove-Item -LiteralPath $destinationFullPath -Force
}

$archive = [System.IO.Compression.ZipFile]::Open(
    $destinationFullPath,
    [System.IO.Compression.ZipArchiveMode]::Create)

try {
    Get-ChildItem -LiteralPath $sourcePath -Directory -Recurse -Force | ForEach-Object {
        $relativePath = $_.FullName.Substring($sourcePath.Length).TrimStart('\', '/')
        $entryName = "$sourceName/$($relativePath.Replace('\', '/'))/"
        $null = $archive.CreateEntry($entryName)
    }

    Get-ChildItem -LiteralPath $sourcePath -File -Recurse -Force | ForEach-Object {
        $relativePath = $_.FullName.Substring($sourcePath.Length).TrimStart('\', '/')
        $entryName = "$sourceName/$($relativePath.Replace('\', '/'))"
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $archive,
            $_.FullName,
            $entryName,
            [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
}
finally {
    $archive.Dispose()
}

$archive = [System.IO.Compression.ZipFile]::OpenRead($destinationFullPath)
try {
    $invalidEntry = $archive.Entries | Where-Object { $_.FullName.Contains('\') } | Select-Object -First 1
    if ($invalidEntry) {
        throw "Archive entry '$($invalidEntry.FullName)' contains a Windows path separator."
    }
}
finally {
    $archive.Dispose()
}

Write-Host "Created $destinationFullPath with portable ZIP paths."
