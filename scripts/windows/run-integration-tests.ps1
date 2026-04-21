#Requires -Version 5.1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Executando Testes de Integração" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ROOT_DIR = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$IntegrationProject = "$ROOT_DIR\tests\Integration\Integration.csproj"

# Check if dotnet is installed
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "Erro: dotnet não encontrado!" -ForegroundColor Red
    exit 1
}

# Check if project exists
if (-not (Test-Path $IntegrationProject)) {
    Write-Host "Erro: Projeto de testes não encontrado em $IntegrationProject" -ForegroundColor Red
    exit 1
}

# Restore packages
Write-Host "1. Restaurando pacotes..." -ForegroundColor Yellow
dotnet restore $IntegrationProject
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao restaurar pacotes!" -ForegroundColor Red
    Read-Host "Pressione Enter para continuar"
    exit $LASTEXITCODE
}
Write-Host "✓ Pacotes restaurados" -ForegroundColor Green
Write-Host ""

# Build project
Write-Host "2. Compilando projeto de testes..." -ForegroundColor Yellow
dotnet build $IntegrationProject --configuration Debug --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao compilar projeto!" -ForegroundColor Red
    Read-Host "Pressione Enter para continuar"
    exit $LASTEXITCODE
}
Write-Host "✓ Projeto compilado" -ForegroundColor Green
Write-Host ""

# Run tests
Write-Host "3. Executando testes..." -ForegroundColor Yellow
dotnet test $IntegrationProject `
    --configuration Debug `
    --no-build `
    --logger "console;verbosity=detailed"

$TestExitCode = $LASTEXITCODE

Write-Host ""
if ($TestExitCode -eq 0) {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✓ Todos os testes passaram!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
} else {
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ✗ Alguns testes falharam" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
}

Read-Host "Pressione Enter para continuar"
exit $TestExitCode
