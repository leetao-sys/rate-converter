# 使用 Windows 自带的 .NET Framework 4.x 编译器构建，无需安装任何开发工具
$ErrorActionPreference = 'Stop'
$fw  = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$csc = Join-Path $fw 'csc.exe'
$wpf = Join-Path $fw 'WPF'
$root = $PSScriptRoot

New-Item -ItemType Directory -Force (Join-Path $root 'dist') | Out-Null
$sources = Get-ChildItem (Join-Path $root 'src') -Filter *.cs | ForEach-Object { $_.FullName }

& $csc /nologo /target:winexe /optimize+ /utf8output /codepage:65001 `
    /out:"$root\dist\RateConverter.exe" `
    /win32icon:"$root\assets\app.ico" `
    /win32manifest:"$root\assets\app.manifest" `
    /r:"$wpf\PresentationFramework.dll" /r:"$wpf\PresentationCore.dll" /r:"$wpf\WindowsBase.dll" `
    /r:System.Xaml.dll /r:System.Web.Extensions.dll /r:System.Data.dll /r:System.Net.Http.dll `
    $sources
if ($LASTEXITCODE -ne 0) { throw "Build failed" }
Write-Host "Built dist\RateConverter.exe"

