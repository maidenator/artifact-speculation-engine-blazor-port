## Artifact Speculation Engine — Blazor

**A Blazor WebAssembly port of the Artifact Speculation Engine.**

The original React/TypeScript version of this project can be found at:
[Artifact-Speculation-Engine](https://github.com/maidenator/Artifact-Speculation-Engine)

---

### About

This is a rewrite of the frontend in **C# / Blazor WebAssembly**, while the core simulation engine remains in **C++** compiled to WebAssembly via Emscripten. The C++ code in `cpp/` is identical to the original project.

### Architecture

```
Blazor (C#)  →  IJSRuntime  →  wasmBridge.js  →  Emscripten glue  →  C++ WASM
```

### Building

```powershell
# Build the C++ engine to WebAssembly (run after any C++ changes)
.\build-wasm.ps1

# Run the Blazor dev server (UI + hot reload)
dotnet run --project ArtifactSpeculationBlazor.csproj

# In a second terminal: run the API host (Enka proxy, required for UID import)
dotnet run --project ArtifactSpeculationBlazor.Server
```

The app runs at http://localhost:5216. In production, publish the Server
(`dotnet publish ArtifactSpeculationBlazor.Server`) — it serves the app and
the proxy same-origin from a single process.

### UID import / Enka proxy

The Enka.Network API sends no CORS headers, so browsers block direct calls.
`ArtifactSpeculationBlazor.Server` serves the app and forwards
`GET /enka-api/uid/{uid}` to Enka server-side (validating the UID first and
identifying with a `User-Agent`, as Enka requests). Static-only hosts need to
provide `/enka-api/uid/*` the same way (e.g. a serverless rewrite).
