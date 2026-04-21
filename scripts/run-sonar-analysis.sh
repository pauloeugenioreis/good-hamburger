#!/bin/bash
# SonarCloud Local Analysis Script
# This script runs SonarCloud analysis locally

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Parameters
SONAR_TOKEN="${SONAR_TOKEN:-$1}"
ORGANIZATION="${SONAR_ORGANIZATION:-pauloeugenioreis}"
PROJECT_KEY="${SONAR_PROJECT_KEY:-pauloeugenioreis_dotnet-enterprise-template-10}"

echo -e "${CYAN}🔍 SonarCloud Local Analysis${NC}"
echo -e "${CYAN}================================${NC}"
echo ""

# Check if SONAR_TOKEN is set
if [ -z "$SONAR_TOKEN" ]; then
    echo -e "${RED}❌ ERROR: SONAR_TOKEN is not set!${NC}"
    echo ""
    echo -e "${CYAN}Please set the SONAR_TOKEN environment variable or pass it as a parameter:${NC}"
    echo -e "${CYAN}  export SONAR_TOKEN='your-token'${NC}"
    echo -e "${CYAN}  OR${NC}"
    echo -e "${CYAN}  ./run-sonar-analysis.sh 'your-token'${NC}"
    echo ""
    echo -e "${CYAN}Get your token from: https://sonarcloud.io/account/security${NC}"
    exit 1
fi

# Check if dotnet-sonarscanner is installed
echo -e "${CYAN}📦 Checking dotnet-sonarscanner...${NC}"
if ! dotnet tool list -g | grep -q "dotnet-sonarscanner"; then
    echo -e "${CYAN}⚠️  dotnet-sonarscanner not found. Installing...${NC}"
    dotnet tool install --global dotnet-sonarscanner
    if [ $? -ne 0 ]; then
        echo -e "${RED}❌ Failed to install dotnet-sonarscanner${NC}"
        exit 1
    fi
    echo -e "${GREEN}✅ dotnet-sonarscanner installed successfully${NC}"
else
    echo -e "${GREEN}✅ dotnet-sonarscanner already installed${NC}"
fi

echo ""
echo -e "${CYAN}🚀 Starting SonarCloud analysis...${NC}"
echo ""

# Step 1: Begin analysis
echo -e "${CYAN}1️⃣  Begin SonarCloud analysis...${NC}"
dotnet sonarscanner begin \
    /k:"$PROJECT_KEY" \
    /o:"$ORGANIZATION" \
    /d:sonar.token="$SONAR_TOKEN" \
    /d:sonar.host.url="https://sonarcloud.io" \
    /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Failed to begin SonarCloud analysis${NC}"
    exit 1
fi

echo ""

# Step 2: Build
echo -e "${CYAN}2️⃣  Building solution...${NC}"
dotnet build ProjectTemplate.sln --configuration Release

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Build failed${NC}"
    exit 1
fi

echo ""

# Step 3: Run tests with coverage
echo -e "${CYAN}3️⃣  Running tests with coverage...${NC}"
dotnet test ProjectTemplate.sln \
    --configuration Release \
    --no-build \
    --collect:"XPlat Code Coverage" \
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

if [ $? -ne 0 ]; then
    echo -e "${CYAN}⚠️  Tests failed, but continuing with analysis...${NC}"
fi

echo ""

# Step 4: End analysis
echo -e "${CYAN}4️⃣  Ending SonarCloud analysis...${NC}"
dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Failed to end SonarCloud analysis${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}✅ SonarCloud analysis completed successfully!${NC}"
echo ""
echo -e "${CYAN}📊 View results at:${NC}"
echo -e "${CYAN}   https://sonarcloud.io/dashboard?id=$PROJECT_KEY${NC}"
echo ""
