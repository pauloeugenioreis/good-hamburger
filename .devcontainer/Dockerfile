# Dev Container para o template .NET 10
FROM mcr.microsoft.com/devcontainers/dotnet:1-10.0-bookworm

# Pacotes adicionais úteis para scripts e ferramentas locais
RUN sudo apt-get update \
    && sudo apt-get install -y --no-install-recommends \
        jq \
        gettext-base \
        iputils-ping \
    && sudo rm -rf /var/lib/apt/lists/*

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

WORKDIR /workspace
