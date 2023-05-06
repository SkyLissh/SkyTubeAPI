#!/usr/bin/env bash

set -o errexit
set -o nounset

exec dotnet watch run --urls http://0.0.0.0:8000
