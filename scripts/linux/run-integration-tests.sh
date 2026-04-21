#!/usr/bin/env bash
set -euo pipefail

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

echo -e "${CYAN}========================================${NC}"
echo -e "${CYAN}  Executando Testes de Integração${NC}"
echo -e "${CYAN}========================================${NC}"
echo ""

# Check if dotnet is installed
if ! command -v dotnet >/dev/null 2>&1; then
  echo -e "${RED}Erro: dotnet não encontrado.${NC}" >&2
  exit 1
fi

# Restore packages
echo -e "${YELLOW}1. Restaurando pacotes...${NC}"
dotnet restore "${ROOT_DIR}/tests/Integration/Integration.csproj"
if [ $? -ne 0 ]; then
  echo -e "${RED}Erro ao restaurar pacotes!${NC}"
  exit 1
fi
echo -e "${GREEN}✓ Pacotes restaurados${NC}"
echo ""

# Build project
echo -e "${YELLOW}2. Compilando projeto de testes...${NC}"
dotnet build "${ROOT_DIR}/tests/Integration/Integration.csproj" --configuration Debug --no-restore
if [ $? -ne 0 ]; then
  echo -e "${RED}Erro ao compilar projeto!${NC}"
  exit 1
fi
echo -e "${GREEN}✓ Projeto compilado${NC}"
echo ""

# Run tests
echo -e "${YELLOW}3. Executando testes...${NC}"
dotnet test "${ROOT_DIR}/tests/Integration/Integration.csproj" \
  --configuration Debug \
  --no-build \
  --logger "console;verbosity=detailed"

TEST_EXIT_CODE=$?

echo ""
if [ $TEST_EXIT_CODE -eq 0 ]; then
  echo -e "${GREEN}========================================${NC}"
  echo -e "${GREEN}  ✓ Todos os testes passaram!${NC}"
  echo -e "${GREEN}========================================${NC}"
else
  echo -e "${RED}========================================${NC}"
  echo -e "${RED}  ✗ Alguns testes falharam${NC}"
  echo -e "${RED}========================================${NC}"
fi

exit $TEST_EXIT_CODE
