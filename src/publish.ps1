# publish.ps1
$ErrorActionPreference = "Stop"

$configuration = "Release"
$runtime = "win-x64"
$outputDir = "publish"

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

if (Test-Path $outputDir) {
    Remove-Item $outputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $outputDir | Out-Null

Write-Host "`nnPublishing Lekco.Promissum..."
dotnet publish ./Lekco.Promissum/Lekco.Promissum.csproj `
    -c $configuration `
    -r $runtime `
    --self-contained false `
    /p:PublishSingleFile=false `
    -o "$outputDir/Lekco.Promissum"

Write-Host "`nPublishing Lekco.Promissum.Agens..."
dotnet publish ./Lekco.Promissum.Agens/Lekco.Promissum.Agens.csproj `
    -c $configuration `
    -r $runtime `
    --self-contained false `
    /p:PublishSingleFile=false `
    -o "$outputDir/Lekco.Promissum.Agens"
