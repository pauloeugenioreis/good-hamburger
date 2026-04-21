@echo off
REM Run integration tests

echo ========================================
echo   Executando Testes de Integracao
echo ========================================
echo.

SET ROOT_DIR=%~dp0..\..

where dotnet >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo Erro: dotnet nao encontrado!
    pause
    exit /b 1
)

echo 1. Restaurando pacotes...
dotnet restore "%ROOT_DIR%\tests\Integration\Integration.csproj"
if %ERRORLEVEL% neq 0 (
    echo Erro ao restaurar pacotes!
    pause
    exit /b 1
)
echo OK
echo.

echo 2. Compilando projeto de testes...
dotnet build "%ROOT_DIR%\tests\Integration\Integration.csproj" --configuration Debug --no-restore
if %ERRORLEVEL% neq 0 (
    echo Erro ao compilar projeto!
    pause
    exit /b 1
)
echo OK
echo.

echo 3. Executando testes...
dotnet test "%ROOT_DIR%\tests\Integration\Integration.csproj" --configuration Debug --no-build --logger "console;verbosity=detailed"

if %ERRORLEVEL% equ 0 (
    echo.
    echo ========================================
    echo   Todos os testes passaram!
    echo ========================================
) else (
    echo.
    echo ========================================
    echo   Alguns testes falharam
    echo ========================================
)

echo.
pause
