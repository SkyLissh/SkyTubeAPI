#!/usr/bin/env bash

set -o errexit
set -o nounset

exec dotnet SkyTube.dll
