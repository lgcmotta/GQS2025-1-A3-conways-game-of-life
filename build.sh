#!/usr/bin/env bash
dotnet tool restore
dotnet cake .scripts/build.cake "$@"