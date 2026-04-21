# Dockerfile.web
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore
COPY ["src/Web/Web.csproj", "src/Web/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
RUN dotnet restore "src/Web/Web.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Web"
RUN dotnet build "Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final

# Run as non-root user for security (user 'app' is built-in on .NET 10 images)
USER $APP_UID

WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "GoodHamburger.Web.dll"]
