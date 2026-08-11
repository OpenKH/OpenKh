param(
    [ValidateSet("All", "Windows", "Linux", "SteamDeck")]
    [string] $Target = "All",
    [string] $Configuration = "Release",
    [string] $OutputDirectory = "artifacts"
)

$project = Join-Path $PSScriptRoot "OpenKh.Tools.Launcher.csproj"
$outputRoot = Join-Path $PSScriptRoot $OutputDirectory

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
}

if ($Target -in @("All", "Windows")) {
    Publish-Target "win-x64" "windows-x64"
}

if ($Target -in @("All", "Linux")) {
    Publish-Target "linux-x64" "linux-x64"
}

if ($Target -in @("All", "SteamDeck")) {
    Publish-Target "linux-x64" "steam-deck"
}
