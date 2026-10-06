#!/usr/bin/env bash
# Linux/CI port of build-wasm.ps1: compiles the C++ engine to WebAssembly.
# Expects the emsdk environment to be active and nlohmann_json already
# fetched (cmake -B build -S .), mirroring the PowerShell script.
set -euo pipefail

mkdir -p wwwroot/wasm

em++ -O3 \
    -flto \
    -DNDEBUG \
    -std=c++20 \
    --bind \
    -fwasm-exceptions \
    --closure 1 \
    -s WASM=1 \
    -s MODULARIZE=1 \
    -s 'EXPORT_NAME="createArtifactEngine"' \
    -s ALLOW_MEMORY_GROWTH=1 \
    -s MAXIMUM_MEMORY=256MB \
    -s FILESYSTEM=0 \
    -s ASSERTIONS=0 \
    -s ENVIRONMENT='web' \
    -I build/_deps/nlohmann_json-src/include \
    -I cpp \
    -I cpp/artifact \
    -I cpp/domain \
    cpp/artifact/bindings.cpp \
    -o wwwroot/wasm/artifact_engine.js

echo "WebAssembly build succeeded! Generated files in wwwroot/wasm/"
