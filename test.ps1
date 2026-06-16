#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"

Write-Host "🧪 Running tests..." -ForegroundColor Cyan
dotnet test --configuration Release --logger "console;verbosity=minimal"

Write-Host "✅ Tests completed" -ForegroundColor Green
