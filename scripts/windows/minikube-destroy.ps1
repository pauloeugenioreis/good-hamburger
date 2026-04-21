#Requires -Version 5.1

$ErrorActionPreference = 'Stop'

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Destruindo Deploy do Minikube" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ROOT_DIR = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

if (-not (Get-Command kubectl -ErrorAction SilentlyContinue)) {
    Write-Host "Erro: kubectl não encontrado!" -ForegroundColor Red
    exit 1
}

Write-Host "Removendo recursos do namespace projecttemplate..." -ForegroundColor Yellow
kubectl delete -k "$ROOT_DIR\.k8s" --ignore-not-found=true

Write-Host ""
Write-Host "✓ Recursos removidos com sucesso" -ForegroundColor Green
Write-Host ""
