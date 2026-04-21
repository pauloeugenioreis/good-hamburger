#!/usr/bin/env pwsh
# SonarCloud Local Analysis Script
# This script runs SonarCloud analysis locally

param(
    [Parameter(Mandatory=$false)]
    [string]$SonarToken = $env:SONAR_TOKEN,

    [Parameter(Mandatory=$false)]
    [string]$Organization = "pauloeugenioreis",

    [Parameter(Mandatory=$false)]
    [string]$ProjectKey = "pauloeugenioreis_dotnet-enterprise-template-10"
)

# Colors
$ErrorColor = "Red"
$SuccessColor = "Green"
$InfoColor = "Cyan"

Write-Host "🔍 SonarCloud Local Analysis" -ForegroundColor $InfoColor
Write-Host "================================" -ForegroundColor $InfoColor
Write-Host ""

# Check if SONAR_TOKEN is set
if ([string]::IsNullOrEmpty($SonarToken)) {
    Write-Host "❌ ERROR: SONAR_TOKEN is not set!" -ForegroundColor $ErrorColor
    Write-Host ""
    Write-Host "Please set the SONAR_TOKEN environment variable or pass it as a parameter:" -ForegroundColor $InfoColor
    Write-Host "  `$env:SONAR_TOKEN = 'your-token'" -ForegroundColor $InfoColor
    Write-Host "  OR" -ForegroundColor $InfoColor
    Write-Host "  .\run-sonar-analysis.ps1 -SonarToken 'your-token'" -ForegroundColor $InfoColor
    Write-Host ""
    Write-Host "Get your token from: https://sonarcloud.io/account/security" -ForegroundColor $InfoColor
    exit 1
}

# Check if dotnet-sonarscanner is installed
Write-Host "📦 Checking dotnet-sonarscanner..." -ForegroundColor $InfoColor
$scannerInstalled = dotnet tool list -g | Select-String "dotnet-sonarscanner"

if (-not $scannerInstalled) {
    Write-Host "⚠️  dotnet-sonarscanner not found. Installing..." -ForegroundColor $InfoColor
    dotnet tool install --global dotnet-sonarscanner
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to install dotnet-sonarscanner" -ForegroundColor $ErrorColor
        exit 1
    }
    Write-Host "✅ dotnet-sonarscanner installed successfully" -ForegroundColor $SuccessColor
}
else {
    Write-Host "✅ dotnet-sonarscanner already installed" -ForegroundColor $SuccessColor
}

Write-Host ""
Write-Host "🚀 Starting SonarCloud analysis..." -ForegroundColor $InfoColor
Write-Host ""

# Step 1: Begin analysis
Write-Host "1️⃣  Begin SonarCloud analysis..." -ForegroundColor $InfoColor
dotnet sonarscanner begin `
    /k:"$ProjectKey" `
    /o:"$Organization" `
    /d:sonar.token="$SonarToken" `
    /d:sonar.host.url="https://sonarcloud.io" `
    /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to begin SonarCloud analysis" -ForegroundColor $ErrorColor
    exit 1
}

Write-Host ""

# Step 2: Build
Write-Host "2️⃣  Building solution..." -ForegroundColor $InfoColor
dotnet build ProjectTemplate.sln --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed" -ForegroundColor $ErrorColor
    exit 1
}

Write-Host ""

# Step 3: Run tests with coverage
Write-Host "3️⃣  Running tests with coverage..." -ForegroundColor $InfoColor
dotnet test ProjectTemplate.sln `
    --configuration Release `
    --no-build `
    --collect:"XPlat Code Coverage" `
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Tests failed, but continuing with analysis..." -ForegroundColor $InfoColor
}

Write-Host ""

# Step 4: End analysis
Write-Host "4️⃣  Ending SonarCloud analysis..." -ForegroundColor $InfoColor
dotnet sonarscanner end /d:sonar.token="$SonarToken"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to end SonarCloud analysis" -ForegroundColor $ErrorColor
    exit 1
}

Write-Host ""
Write-Host "✅ SonarCloud analysis completed successfully!" -ForegroundColor $SuccessColor
Write-Host ""
Write-Host "📊 View results at:" -ForegroundColor $InfoColor
Write-Host "   https://sonarcloud.io/dashboard?id=$ProjectKey" -ForegroundColor $InfoColor
Write-Host ""
