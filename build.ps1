#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"

Write-Host "🔨 Building Certus solution..." -ForegroundColor Cyan
dotnet build --configuration Release

Write-Host "✅ Build completed successfully" -ForegroundColor Green
