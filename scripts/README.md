# 📜 Scripts

Scripts utilitários para facilitar o desenvolvimento e testes do projeto.

---

## 📋 Índice

- [run-sonar-analysis](#-run-sonar-analysis) - Executar análise SonarCloud local
- [Kubernetes (Minikube)](#%EF%B8%8F-kubernetes-minikube) - Deploy, destroy e testes de integração no Minikube

---

## 🔍 run-sonar-analysis

Executa análise SonarCloud localmente.

### Windows (PowerShell)

```powershell
# Set token
$env:SONAR_TOKEN = "your-sonarcloud-token"

# Run analysis
.\scripts\run-sonar-analysis.ps1

# OR pass token as parameter
.\scripts\run-sonar-analysis.ps1 -SonarToken "your-token"
```

### Linux/macOS (Bash)

```bash
# Set token
export SONAR_TOKEN="your-sonarcloud-token"

# Run analysis
chmod +x scripts/run-sonar-analysis.sh
./scripts/run-sonar-analysis.sh

# OR pass token as parameter
./scripts/run-sonar-analysis.sh "your-token"
```

### O que o script faz?

1. ✅ **Instala** dotnet-sonarscanner (se necessário)
2. ✅ **Begin** - Inicia análise SonarCloud
3. ✅ **Build** - Compila o projeto
4. ✅ **Test** - Executa testes com cobertura (OpenCover)
5. ✅ **End** - Finaliza e envia dados ao SonarCloud
6. ✅ **Link** - Mostra URL para ver resultados

**Requisitos:**

- Token SonarCloud (obtenha em https://sonarcloud.io/account/security)
- .NET 10 SDK instalado
- Projeto configurado no SonarCloud

---

## ☸️ Kubernetes (Minikube)

### minikube-deploy

Deploy da aplicação em cluster Kubernetes local (Minikube).

#### Windows (PowerShell)

```powershell
cd scripts\windows
.\minikube-deploy.ps1
```

#### Linux/macOS

```bash
cd scripts/linux
chmod +x minikube-deploy.sh
./minikube-deploy.sh
```

### minikube-destroy

Remove o deploy do Minikube.

#### Windows (PowerShell)

```powershell
cd scripts\windows
.\minikube-destroy.ps1
```

#### Linux/macOS

```bash
cd scripts/linux
./minikube-destroy.sh
```

### run-integration-tests

Executa testes de integração no Minikube.

#### Windows (PowerShell)

```powershell
cd scripts\windows
.\run-integration-tests.ps1
```

#### Linux/macOS

```bash
cd scripts/linux
./run-integration-tests.sh
```

---

## 📝 Convenções

- **Windows**: Scripts PowerShell (`.ps1`) e Batch (`.bat`) em `scripts/windows/`
- **Linux/macOS**: Scripts Bash (`.sh`) em `scripts/linux/`
- **Multiplataforma**: Scripts de análise na raiz de `scripts/`
- **Sempre** execute scripts do diretório correto
- Scripts Linux precisam de permissão de execução: `chmod +x script.sh`

---

## 🐛 Troubleshooting

### PowerShell Execution Policy

Se encontrar erro de execution policy no Windows:

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Permission Denied (Linux/macOS)

```bash
chmod +x script.sh
```

### Docker não encontrado

Certifique-se que o Docker Desktop está instalado e rodando:

```bash
docker --version
docker-compose --version
```
