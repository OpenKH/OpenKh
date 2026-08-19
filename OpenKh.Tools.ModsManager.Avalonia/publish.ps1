param(
    [ValidateSet("All", "Windows", "Linux", "SteamDeck")]
    [string] $Target = "All",
    [string] $Configuration = "Release",
    [string] $OutputDirectory = "artifacts"
)

$project = Join-Path $PSScriptRoot "OpenKh.Tools.ModsManager.csproj"
$outputRoot = Join-Path $PSScriptRoot $OutputDirectory
$repositoryBuild = Join-Path $PSScriptRoot "..\bin"
$panaceaProject = Join-Path $PSScriptRoot "..\OpenKh.Research.Panacea"
$panaceaFileNames = @(
    "avcodec-vgmstream-59.dll",
    "avformat-vgmstream-59.dll",
    "avutil-vgmstream-57.dll",
    "bass.dll",
    "bass_vgmstream.dll",
    "libatrac9.dll",
    "libcelt-0061.dll",
    "libcelt-0110.dll",
    "libg719_decode.dll",
    "libmpg123-0.dll",
    "libspeex-1.dll",
    "libvorbis.dll",
    "swresample-vgmstream-4.dll"
)

function Copy-PanaceaFiles([string] $output) {
    $sources = @(
        [pscustomobject]@{
            Loader = Join-Path $repositoryBuild "OpenKH.Panacea.dll"
            Dependencies = $repositoryBuild
        },
        [pscustomobject]@{
            Loader = Join-Path $panaceaProject "Release\OpenKH.Panacea.dll"
            Dependencies = Join-Path $panaceaProject "Dependencies"
        }
    ) | Where-Object {
        $candidate = $_
        (Test-Path -LiteralPath $candidate.Loader -PathType Leaf) -and
        -not ($panaceaFileNames | Where-Object {
            -not (Test-Path -LiteralPath (Join-Path $candidate.Dependencies $_) -PathType Leaf)
        })
    } | Sort-Object { (Get-Item -LiteralPath $_.Loader).LastWriteTimeUtc } -Descending
    $source = $sources | Select-Object -First 1
    if ($null -eq $source) {
        Write-Warning "Panacea was not built, so it will not be included in '$output'."
        return
    }

    Copy-Item -LiteralPath $source.Loader -Destination $output -Force
    foreach ($fileName in $panaceaFileNames) {
        $sourcePath = Join-Path $source.Dependencies $fileName
        Copy-Item -LiteralPath $sourcePath -Destination $output -Force
    }
}

function Publish-Target([string] $runtime, [string] $folder) {
    $output = Join-Path $outputRoot $folder
    dotnet publish $project `
        --configuration $Configuration `
        --runtime $runtime `
        --self-contained true `
        --source "https://api.nuget.org/v3/index.json" `
        --output $output `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:DebugType=None `
        -p:DebugSymbols=false

    if ($LASTEXITCODE -ne 0) {
        throw "Publishing $folder failed with exit code $LASTEXITCODE."
    }

    Get-ChildItem $output -Filter "*.pdb" -File | Remove-Item -Force
    Copy-PanaceaFiles $output
}

if ($Target -in @("All", "Windows")) {
    Publish-Target "win-x64" "windows-x64"
}

if ($Target -in @("All", "Linux")) {
    Publish-Target "linux-x64" "linux-x64"
}

if ($Target -in @("All", "SteamDeck")) {
    Publish-Target "linux-x64" "steam-deck"
    Copy-Item (Join-Path $PSScriptRoot "Packaging/SteamDeck/install-openkh-mod-manager.sh") `
        (Join-Path $outputRoot "steam-deck/install-openkh-mod-manager.sh") `
        -Force
}
