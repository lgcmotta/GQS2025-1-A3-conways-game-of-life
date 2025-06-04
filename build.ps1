#!/usr/bin/env pwsh
dotnet tool restore
dotnet cake .scripts/build.cake $args