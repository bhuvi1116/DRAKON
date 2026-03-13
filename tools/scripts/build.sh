#!/usr/bin/env bash
set -euo pipefail

dotnet restore
dotnet build DRAKON-NX.sln
dotnet test DRAKON-NX.sln
