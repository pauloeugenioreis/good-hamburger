#Requires -Version 5.1

$ErrorActionPreference = 'Stop'

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Deploy ProjectTemplate no Minikube" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ROOT_DIR = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$PROFILE = $env:MINIKUBE_PROFILE
if (-not $PROFILE) { $PROFILE = 'minikube' }
$IMAGE_TAG = $env:MINIKUBE_IMAGE_TAG
if (-not $IMAGE_TAG) { $IMAGE_TAG = 'projecttemplate-api:latest' }

function Require-Cmd {
    param([string]$cmd)
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-Host "Erro: comando '$cmd' não encontrado." -ForegroundColor Red
        Write-Host "Por favor, instale $cmd antes de continuar." -ForegroundColor Yellow
        exit 1
    }
}

# Check required commands
Write-Host "1. Verificando comandos necessários..." -ForegroundColor Yellow
Require-Cmd minikube
Require-Cmd docker
Require-Cmd kubectl
Write-Host "✓ Todos os comandos necessários estão disponíveis" -ForegroundColor Green
Write-Host ""

# Check if Minikube is running
Write-Host "2. Verificando status do Minikube..." -ForegroundColor Yellow
$minikubeStatus = minikube -p $PROFILE status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Minikube não está rodando!" -ForegroundColor Red
    Write-Host "Iniciando Minikube..." -ForegroundColor Yellow
    minikube start -p $PROFILE
}
Write-Host "✓ Minikube está rodando" -ForegroundColor Green
Write-Host ""

# Build Docker image
Write-Host "3. Construindo imagem Docker: $IMAGE_TAG" -ForegroundColor Yellow
docker build --file "$ROOT_DIR\Dockerfile" --tag $IMAGE_TAG $ROOT_DIR
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao construir imagem!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Imagem construída com sucesso" -ForegroundColor Green
Write-Host ""

# Load image into Minikube
Write-Host "4. Carregando imagem no Minikube ($PROFILE)" -ForegroundColor Yellow
minikube -p $PROFILE image load $IMAGE_TAG
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao carregar imagem!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Imagem carregada no Minikube" -ForegroundColor Green
Write-Host ""

# Apply Kubernetes manifests
Write-Host "5. Aplicando manifestos Kubernetes (.k8s)" -ForegroundColor Yellow
kubectl apply -k "$ROOT_DIR\.k8s"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao aplicar manifestos!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Manifestos aplicados com sucesso" -ForegroundColor Green
Write-Host ""

# Wait for deployment
Write-Host "6. Aguardando deployment ficar pronto..." -ForegroundColor Yellow
Start-Sleep -Seconds 5
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Deploy concluído!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Comandos úteis:" -ForegroundColor Green
Write-Host "  Ver pods:        " -NoNewline; Write-Host "kubectl get pods -n projecttemplate" -ForegroundColor Yellow
Write-Host "  Ver logs:        " -NoNewline; Write-Host "kubectl logs -f deployment/projecttemplate-api -n projecttemplate" -ForegroundColor Yellow
Write-Host "  Ver services:    " -NoNewline; Write-Host "kubectl get svc -n projecttemplate" -ForegroundColor Yellow
Write-Host "  Port-forward:    " -NoNewline; Write-Host "kubectl port-forward svc/projecttemplate-api 8080:80 -n projecttemplate" -ForegroundColor Yellow
Write-Host "  Minikube tunnel: " -NoNewline; Write-Host "minikube tunnel -p $PROFILE" -ForegroundColor Yellow
Write-Host ""
