@echo off
REM Deploy ProjectTemplate to Minikube

echo ========================================
echo   Deploy ProjectTemplate no Minikube
echo ========================================
echo.

SET ROOT_DIR=%~dp0..\..
SET PROFILE=minikube
SET IMAGE_TAG=projecttemplate-api:latest

if not "%MINIKUBE_PROFILE%"=="" SET PROFILE=%MINIKUBE_PROFILE%
if not "%MINIKUBE_IMAGE_TAG%"=="" SET IMAGE_TAG=%MINIKUBE_IMAGE_TAG%

echo 1. Verificando comandos necessarios...
where minikube >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo Erro: minikube nao encontrado!
    exit /b 1
)
where docker >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo Erro: docker nao encontrado!
    exit /b 1
)
where kubectl >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo Erro: kubectl nao encontrado!
    exit /b 1
)
echo OK
echo.

echo 2. Verificando status do Minikube...
minikube -p %PROFILE% status >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo Minikube nao esta rodando! Iniciando...
    minikube start -p %PROFILE%
)
echo OK
echo.

echo 3. Construindo imagem Docker: %IMAGE_TAG%
docker build --file "%ROOT_DIR%\Dockerfile" --tag %IMAGE_TAG% "%ROOT_DIR%"
if %ERRORLEVEL% neq 0 (
    echo Erro ao construir imagem!
    exit /b 1
)
echo OK
echo.

echo 4. Carregando imagem no Minikube
minikube -p %PROFILE% image load %IMAGE_TAG%
if %ERRORLEVEL% neq 0 (
    echo Erro ao carregar imagem!
    exit /b 1
)
echo OK
echo.

echo 5. Aplicando manifestos Kubernetes
kubectl apply -k "%ROOT_DIR%\.k8s"
if %ERRORLEVEL% neq 0 (
    echo Erro ao aplicar manifestos!
    exit /b 1
)
echo OK
echo.

echo ========================================
echo   Deploy concluido!
echo ========================================
echo.
echo Comandos uteis:
echo   Ver pods:     kubectl get pods -n projecttemplate
echo   Ver logs:     kubectl logs -f deployment/projecttemplate-api -n projecttemplate
echo   Port-forward: kubectl port-forward svc/projecttemplate-api 8080:80 -n projecttemplate
echo.
pause
