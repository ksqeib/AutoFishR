# AutoFishR 打包脚本
# 自动编译并打包所有必要文件

param(
    [string]$Configuration = "Release",
    [string]$OutputDir = ".\publish"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  AutoFishR 自动打包脚本" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# 获取项目版本号
[xml]$csproj = Get-Content "AutoFishR.csproj"
$version = $csproj.Project.PropertyGroup.Version
Write-Host "项目版本: $version" -ForegroundColor Green

# 清理旧的发布文件
Write-Host "清理旧的发布文件..." -ForegroundColor Yellow
if (Test-Path $OutputDir) {
    Remove-Item -Path $OutputDir -Recurse -Force
}
New-Item -Path $OutputDir -ItemType Directory | Out-Null

# 编译项目
Write-Host "开始编译项目 ($Configuration)..." -ForegroundColor Yellow
dotnet build -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "编译失败" -ForegroundColor Red
    exit 1
}
Write-Host "编译成功" -ForegroundColor Green
Write-Host ""

# 创建临时打包目录
$tempDir = Join-Path $OutputDir "temp"
$packageDir = Join-Path $tempDir "AutoFishR"
$pluginDir = Join-Path $packageDir "AutoFishR"
New-Item -Path $pluginDir -ItemType Directory -Force | Out-Null

# 复制插件本体
Write-Host "复制插件文件..." -ForegroundColor Yellow
$buildPath = "bin\$Configuration\net9.0"
Copy-Item -Path (Join-Path $buildPath "AutoFishR.dll") -Destination $pluginDir
Write-Host "  AutoFishR.dll" -ForegroundColor Gray

# 复制 YamlDotNet 依赖
$yamlDotNetPath = Join-Path $buildPath "YamlDotNet.dll"
if (-not (Test-Path $yamlDotNetPath)) {
    # 从NuGet包中查找
    $nugetPath = "$env:USERPROFILE\.nuget\packages\yamldotnet"
    if (Test-Path $nugetPath) {
        $latestVersion = Get-ChildItem $nugetPath | Sort-Object Name -Descending | Select-Object -First 1
        $yamlDotNetPath = Join-Path $latestVersion.FullName "lib\net9.0\YamlDotNet.dll"
        if (-not (Test-Path $yamlDotNetPath)) {
            $yamlDotNetPath = Join-Path $latestVersion.FullName "lib\net8.0\YamlDotNet.dll"
        }
        if (-not (Test-Path $yamlDotNetPath)) {
            $yamlDotNetPath = Join-Path $latestVersion.FullName "lib\netstandard2.1\YamlDotNet.dll"
        }
    }
}
if (Test-Path $yamlDotNetPath) {
    Copy-Item -Path $yamlDotNetPath -Destination $pluginDir
    Write-Host "  YamlDotNet.dll" -ForegroundColor Gray
} else {
    Write-Host "  未找到 YamlDotNet.dll" -ForegroundColor Red
}

# 复制 resource 文件夹
Write-Host "复制 resource 文件夹..." -ForegroundColor Yellow
if (Test-Path "resource") {
    Copy-Item -Path "resource" -Destination $packageDir -Recurse
    Write-Host "  resource/" -ForegroundColor Gray
}

# 复制所有 .md 文件
Write-Host "复制文档文件..." -ForegroundColor Yellow
Get-ChildItem -Filter "*.md" | ForEach-Object {
    Copy-Item -Path $_.FullName -Destination $packageDir
    Write-Host "  $($_.Name)" -ForegroundColor Gray
}

# 创建 ZIP 包
Write-Host ""
Write-Host "创建 ZIP 包..." -ForegroundColor Yellow
$zipName = "AutoFishR_v$version.zip"
$zipPath = Join-Path $OutputDir $zipName

# 使用 .NET 压缩类
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $zipPath)

# 清理临时目录
Remove-Item -Path $tempDir -Recurse -Force

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  打包完成" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "输出文件: $zipPath" -ForegroundColor Green
$fileSize = [math]::Round((Get-Item $zipPath).Length / 1KB, 2)
Write-Host "文件大小: $fileSize KB" -ForegroundColor Green
Write-Host ""
