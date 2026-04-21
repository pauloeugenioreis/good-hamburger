#!/usr/bin/env bash
set -euo pipefail

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROFILE="${MINIKUBE_PROFILE:-minikube}"
IMAGE_TAG="${MINIKUBE_IMAGE_TAG:-projecttemplate-api:latest}"

echo -e "${CYAN}========================================${NC}"
echo -e "${CYAN}  Deploy ProjectTemplate no Minikube${NC}"
echo -e "${CYAN}========================================${NC}"
echo ""

# Function to check if command exists
require_cmd() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo -e "${RED}Erro: comando '$1' não encontrado.${NC}" >&2
    echo -e "${YELLOW}Por favor, instale $1 antes de continuar.${NC}" >&2
    exit 1
  fi
}

# Check required commands
echo -e "${YELLOW}1. Verificando comandos necessários...${NC}"
require_cmd minikube
require_cmd docker
require_cmd kubectl
echo -e "${GREEN}✓ Todos os comandos necessários estão disponíveis${NC}"
echo ""

# Check if Minikube is running
echo -e "${YELLOW}2. Verificando status do Minikube...${NC}"
if ! minikube -p "${PROFILE}" status >/dev/null 2>&1; then
  echo -e "${RED}Minikube não está rodando!${NC}"
  echo -e "${YELLOW}Iniciando Minikube...${NC}"
  minikube start -p "${PROFILE}"
fi
echo -e "${GREEN}✓ Minikube está rodando${NC}"
echo ""

# Build Docker image
echo -e "${YELLOW}3. Construindo imagem Docker: ${IMAGE_TAG}${NC}"
docker build \
  --file "${ROOT_DIR}/Dockerfile" \
  --tag "${IMAGE_TAG}" \
  "${ROOT_DIR}"
echo -e "${GREEN}✓ Imagem construída com sucesso${NC}"
echo ""

# Load image into Minikube
echo -e "${YELLOW}4. Carregando imagem no Minikube (${PROFILE})${NC}"
minikube -p "${PROFILE}" image load "${IMAGE_TAG}"
echo -e "${GREEN}✓ Imagem carregada no Minikube${NC}"
echo ""

# Apply Kubernetes manifests
echo -e "${YELLOW}5. Aplicando manifestos Kubernetes (.k8s)${NC}"
kubectl apply -k "${ROOT_DIR}/.k8s"
echo -e "${GREEN}✓ Manifestos aplicados com sucesso${NC}"
echo ""

# Wait for deployment to be ready
echo -e "${YELLOW}6. Aguardando deployment ficar pronto...${NC}"
kubectl wait --for=condition=available --timeout=300s \
  deployment/projecttemplate-api -n projecttemplate 2>/dev/null || true
echo ""

# Get service URL
echo -e "${CYAN}========================================${NC}"
echo -e "${CYAN}  Deploy concluído!${NC}"
echo -e "${CYAN}========================================${NC}"
echo ""
echo -e "${GREEN}Comandos úteis:${NC}"
echo -e "  Ver pods:        ${YELLOW}kubectl get pods -n projecttemplate${NC}"
echo -e "  Ver logs:        ${YELLOW}kubectl logs -f deployment/projecttemplate-api -n projecttemplate${NC}"
echo -e "  Ver services:    ${YELLOW}kubectl get svc -n projecttemplate${NC}"
echo -e "  Port-forward:    ${YELLOW}kubectl port-forward svc/projecttemplate-api 8080:80 -n projecttemplate${NC}"
echo -e "  Minikube tunnel: ${YELLOW}minikube tunnel -p ${PROFILE}${NC}"
echo ""
