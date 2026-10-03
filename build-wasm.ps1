$ErrorActionPreference = "Stop"

# 1. Activate Emscripten environment
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
em++ -O3 `
    -profiling `
    -std=c++20 `
    --bind `
    -s WASM=1 `
    -s MODULARIZE=1 `
    -s 'EXPORT_NAME="createArtifactEngine"' `
    -s ALLOW_MEMORY_GROWTH=1 `
    -s MAXIMUM_MEMORY=512MB `
    -s NO_DISABLE_EXCEPTION_CATCHING `
    -s ENVIRONMENT='web,worker' `
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
