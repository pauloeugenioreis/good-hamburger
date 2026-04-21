# Good Hamburger - STgenetics

> Aplicação de gerenciamento de pedidos de hambúrguer desenvolvida em .NET 10 seguindo Clean Architecture.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-latest-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)](docker-compose.yml)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1?logo=postgresql&logoColor=white)](docker-compose.yml)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](docker-compose.yml)
[![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?logo=swagger&logoColor=black)](https://localhost:3060/swagger)

---

## 🚀 Instruções de Execução

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

---

### Docker Compose

Sobe os bancos, a API e o Web com um único comando.
A API roda as **migrations e o seed automaticamente** na inicialização.

```bash
# Primeira vez — faz o build e sobe tudo
docker compose up --build

# Execuções seguintes
docker compose up
```

### Endpoints

| Serviço | URL |
|---|---|
| API + Swagger | http://localhost:3060/swagger |
| Web (Blazor) | http://localhost:5001 |
| SQL Server | localhost:1433 |
| PostgreSQL (Event Store) | localhost:5432 |

---

## 🏗️ Decisões de Arquitetura

### Clean Architecture

O projeto segue Clean Architecture com camadas bem definidas:

```
Api → Infrastructure → Application → Data → Domain
```

| Camada | Responsabilidade |
|---|---|
| **Domain** | Entidades, interfaces de domínio, regras de negócio puras, eventos |
| **Data** | DbContext (EF Core), repositórios, migrations, seeders |
| **Application** | Serviços de aplicação, casos de uso, DTOs |
| **Infrastructure** | Extensions de DI, middleware, cache, event sourcing, configurações |
| **Api** | Controllers, filtros, Program.cs |
| **Web** | Frontend Blazor Server, consumo da API via HttpClient |

---

### Bancos de dados

| Banco | Uso |
|---|---|
| **SQL Server 2022** | Dados transacionais (Produtos, Pedidos) via Entity Framework Core |
| **PostgreSQL 17** | Event Store via [Marten](https://martendb.io/) para auditoria e Event Sourcing |

---

### Event Sourcing

- Modo **Hybrid**: eventos são persistidos no PostgreSQL, mas o estado principal fica no SQL Server.
- Eventos de domínio (`ProductCreatedEvent`, `OrderCreatedEvent`, etc.) são publicados pelo `DbSeeder` e pelos serviços.
- A interface `IEventStore` abstrai o Marten, permitindo substituição futura.

---

### Migrations e Seed

- Migrations são gerenciadas pelo **EF Core** e ficam em `src/Data/Migrations/`.
- Em ambiente `Development`, o `Program.cs` chama `MigrateAsync()` seguido de `DbSeeder.SeedAsync()` automaticamente.
- Em produção, as migrations devem ser aplicadas manualmente ou via pipeline de CI/CD.

---

### Dockerfiles

| Arquivo | Serviço |
|---|---|
| `Dockerfile.api` | Build multi-stage da API (.NET 10) |
| `Dockerfile.web` | Build multi-stage do Blazor Server (.NET 10) |

---

### Estrutura de pastas

```
src/
├── Api/            # Controllers, Program.cs, appsettings
├── Application/    # Serviços de aplicação
├── Domain/         # Entidades, interfaces, eventos, DTOs
├── Data/           # DbContext, repositórios, migrations, seeders
├── Infrastructure/ # Extensions de DI, middleware, cache, event sourcing
└── Web/            # Frontend Blazor Server
tests/
├── UnitTests/      # xUnit + Moq + FluentAssertions
└── Integration/    # Testes de integração com WebApplicationFactory
```
