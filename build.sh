#!/bin/bash
set -e

echo "🔨 Building Certus solution..."
dotnet build --configuration Release

echo "✅ Build completed successfully"
