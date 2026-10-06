# Starts both dev processes:
#  - Blazor client dev server (UI + hot reload)  -> http://localhost:5216
#  - API server (Enka proxy for UID import)       -> http://localhost:5198
# Production needs only the Server: dotnet publish ArtifactSpeculationBlazor.Server
Start-Process dotnet -ArgumentList "run --project `"$PSScriptRoot\ArtifactSpeculationBlazor.csproj`" --launch-profile http" -WorkingDirectory $PSScriptRoot
Start-Process dotnet -ArgumentList "run --project `"$PSScriptRoot\ArtifactSpeculationBlazor.Server\ArtifactSpeculationBlazor.Server.csproj`" --launch-profile http" -WorkingDirectory $PSScriptRoot
Write-Host "Client: http://localhost:5216  API: http://localhost:5198"
