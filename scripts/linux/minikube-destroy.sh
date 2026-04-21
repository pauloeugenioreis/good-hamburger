#!/usr/bin/env bash
set -euo pipefail

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

PROFILE="${MINIKUBE_PROFILE:-minikube}"

echo -e "${CYAN}========================================${NC}"
echo -e "${CYAN}  Destruindo Deploy do Minikube${NC}"
echo -e "${CYAN}========================================${NC}"
echo ""

# Function to check if command exists
require_cmd() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo -e "${RED}Erro: comando '$1' não encontrado.${NC}" >&2
    exit 1
  fi
}

require_cmd kubectl

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

echo -e "${YELLOW}Removendo recursos do namespace projecttemplate...${NC}"
kubectl delete -k "${ROOT_DIR}/.k8s" --ignore-not-found=true

echo ""
echo -e "${GREEN}✓ Recursos removidos com sucesso${NC}"
echo ""
