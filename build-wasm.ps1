# NOTE: $ErrorActionPreference stays at its default (Continue) on purpose:
# Emscripten logs INFO lines to stderr (first-time sysroot builds, -flto),
# and those must not abort the script. Failures are caught via $LASTEXITCODE.

# 1. Activate Emscripten environment (quiet: the activation script prints
# setup noise to stderr).
$env:EMSDK_QUIET = "1"
$emsdkPath = "$PSScriptRoot\emscripten\emsdk\emsdk_env.ps1"

if (-not (Test-Path $emsdkPath)) {
    Write-Error "Could not find emsdk_env.ps1 at $emsdkPath. Please verify the folder structure."
    exit 1
}

# Run the activation script in the current session
& $emsdkPath

# 2. Ensure target directory exists
New-Item -ItemType Directory -Force -Path "wwwroot/wasm" | Out-Null

# 3. Compile C++ to WebAssembly - NO ES6 export (Blazor uses plain scripts via JSInterop)
# Review §2: no -profiling, real exceptions, assertions off in release.
em++ -O3 `
    -flto `
    -DNDEBUG `
    -std=c++20 `
    --bind `
    -fwasm-exceptions `
    --closure 1 `
    -s WASM=1 `
    -s MODULARIZE=1 `
    -s 'EXPORT_NAME="createArtifactEngine"' `
    -s ALLOW_MEMORY_GROWTH=1 `
    -s MAXIMUM_MEMORY=256MB `
    -s FILESYSTEM=0 `
    -s ASSERTIONS=0 `
    -s ENVIRONMENT='web' `
    -I build/_deps/nlohmann_json-src/include `
    -I cpp `
    -I cpp/artifact `
    -I cpp/domain `
    cpp/artifact/bindings.cpp `
    -o wwwroot/wasm/artifact_engine.js

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nWebAssembly build succeeded! Generated files in wwwroot/wasm/" -ForegroundColor Green
    Write-Host "Done!" -ForegroundColor Green
} else {
    Write-Host "`nBuild failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}
