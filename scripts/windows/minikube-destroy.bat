@echo off
REM Destroy Minikube deployment

echo ========================================
echo   Destruindo Deploy do Minikube
echo ========================================
echo.

SET ROOT_DIR=%~dp0..\..

where kubectl >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo Erro: kubectl nao encontrado!
    exit /b 1
)

echo Removendo recursos do namespace projecttemplate...
kubectl delete -k "%ROOT_DIR%\.k8s" --ignore-not-found=true

echo.
echo Recursos removidos com sucesso
echo.
pause
