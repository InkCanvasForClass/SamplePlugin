# 查杀 InkCanvasForClass 进程
Get-Process -Name "InkCanvasForClass" -ErrorAction SilentlyContinue | Stop-Process -Force
Write-Host "已终止 InkCanvasForClass 进程"

# 编译主程序
Write-Host "正在编译主程序..."
dotnet build "C:\Users\PrefacedCorg\Documents\GitHub\community\Ink Canvas\InkCanvasForClass.csproj" -c Debug
if ($LASTEXITCODE -ne 0) { Write-Host "主程序编译失败！" -ForegroundColor Red; exit 1 }

# 编译示例插件
Write-Host "正在编译示例插件..."
dotnet build "C:\Users\PrefacedCorg\Documents\GitHub\SamplePlugin\SamplePlugin.csproj" -c Debug
if ($LASTEXITCODE -ne 0) { Write-Host "示例插件编译失败！" -ForegroundColor Red; exit 1 }

# 复制插件输出到 Plugins/<插件Id> 目录（新目录结构，支持 manifest.json）
$pluginSource = "C:\Users\PrefacedCorg\Documents\GitHub\SamplePlugin\bin\Debug\net6.0-windows10.0.19041.0"
$pluginsDir = "C:\Users\PrefacedCorg\Documents\GitHub\community\Ink Canvas\bin\Debug\AnyCPU\net6.0-windows10.0.19041.0\Plugins\sample.plugin"

if (-not (Test-Path $pluginsDir)) {
    New-Item -ItemType Directory -Path $pluginsDir -Force | Out-Null
    Write-Host "已创建 Plugins 目录"
}

# 清空旧文件后复制
Get-ChildItem $pluginsDir -Recurse | Remove-Item -Recurse -Force
Copy-Item -Path "$pluginSource\*" -Destination $pluginsDir -Recurse -Force
Write-Host "已复制插件到 Plugins\sample.plugin 目录"

# 启动主程序
Write-Host "正在启动 InkCanvasForClass..."
Start-Process "C:\Users\PrefacedCorg\Documents\GitHub\community\Ink Canvas\bin\Debug\AnyCPU\net6.0-windows10.0.19041.0\InkCanvasForClass.exe"
