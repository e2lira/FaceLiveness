param(
  [string]$SolutionName = "FaceLivenessSolution"
)

dotnet new sln -n $SolutionName

dotnet new classlib -n Application -o src/Application
dotnet new classlib -n Infrastructure -o src/Infrastructure
dotnet new winforms -n UI.WinForms -o src/UI.WinForms

dotnet sln $SolutionName.sln add src/Application/Application.csproj
dotnet sln $SolutionName.sln add src/Infrastructure/Infrastructure.csproj
dotnet sln $SolutionName.sln add src/UI.WinForms/UI.WinForms.csproj

# Referencias
dotnet add src/Infrastructure/Infrastructure.csproj reference src/Application/Application.csproj
dotnet add src/UI.WinForms/UI.WinForms.csproj reference src/Application/Application.csproj src/Infrastructure/Infrastructure.csproj

# Paquetes
dotnet add src/Infrastructure/Infrastructure.csproj package Emgu.CV
dotnet add src/Infrastructure/Infrastructure.csproj package Emgu.CV.Bitmap
dotnet add src/UI.WinForms/UI.WinForms.csproj package Emgu.CV
dotnet add src/UI.WinForms/UI.WinForms.csproj package Emgu.CV.Bitmap
dotnet add src/UI.WinForms/UI.WinForms.csproj package Emgu.CV.runtime.windows
dotnet add src/UI.WinForms/UI.WinForms.csproj package Microsoft.Extensions.DependencyInjection

Write-Host "✅ Solución creada. Copia las clases y coloca cascadas en ./assets/"