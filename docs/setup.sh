#!/bin/bash
TMP=$(mktemp)
DOCFX_URL="https://github.com/dotnet/docfx/releases/download/v2.56.6/docfx.zip"

rm -rf bin
wget -O "$TMP" "$DOCFX_URL"
unzip "$TMP" -d bin
rm "$TMP"