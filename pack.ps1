# 打包示例插件为 .icpx 文件
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$outputDir = Join-Path $scriptDir "bin\Debug\net6.0-windows10.0.19041.0"
$packagesDir = Join-Path $scriptDir "packages"

# 读取 manifest 获取插件 ID 和版本
$manifest = Get-Content (Join-Path $outputDir "manifest.json") | ConvertFrom-Json
$pluginId = $manifest.Id
$version = $manifest.Version

Write-Host "正在打包插件: $pluginId v$version" -ForegroundColor Cyan

# 创建 packages 目录
if (-not (Test-Path $packagesDir)) {
    New-Item -ItemType Directory -Path $packagesDir | Out-Null
}

# 收集需要打包的文件
$files = @(
    "manifest.json",
    "SamplePlugin.dll",
    "SamplePlugin.deps.json"
)

# 创建临时目录
$tempDir = Join-Path $env:TEMP "icpx_package_$pluginId"
if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force }
New-Item -ItemType Directory -Path $tempDir | Out-Null

# 复制文件到临时目录
foreach ($f in $files) {
    $src = Join-Path $outputDir $f
    if (Test-Path $src) {
        Copy-Item $src $tempDir
        Write-Host "  + $f"
    } else {
        Write-Host "  ! $f (未找到，跳过)" -ForegroundColor Yellow
    }
}

# 打包为 .icpx (ZIP)
$icpxName = "$pluginId.icpx"
$zipPath = Join-Path $packagesDir ($pluginId + ".zip")
$icpxPath = Join-Path $packagesDir $icpxName

if (Test-Path $zipPath) { Remove-Item $zipPath }
Compress-Archive -Path (Join-Path $tempDir '*') -DestinationPath $zipPath -Force

if (Test-Path $icpxPath) { Remove-Item $icpxPath }
Rename-Item $zipPath $icpxName

# 计算 SHA256
$hash = (Get-FileHash $icpxPath -Algorithm SHA256).Hash.ToLower()
Write-Host ""
Write-Host "打包完成: $icpxPath" -ForegroundColor Green
Write-Host "文件大小: $((Get-Item $icpxPath).Length) bytes"
Write-Host "SHA256:   $hash"
Write-Host ""
Write-Host "下一步:" -ForegroundColor Yellow
Write-Host "  1. 在 GitHub 上为 SamplePlugin 创建 Release v$version"
Write-Host "  2. 将 $icpxName 上传为 Release Asset"
Write-Host "  3. 更新 PluginIndex/index.json 中的 DownloadUrl 和 DownloadSha256"
Write-Host "  4. 推送 PluginIndex 仓库"

# 清理
Remove-Item $tempDir -Recurse -Force
