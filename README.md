# .NET 10 Clean Architecture Template

> Template moderno e completo para criação de APIs .NET 10 seguindo Clean Architecture e melhores práticas de mercado.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-latest-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-7-FF6600?logo=rabbitmq&logoColor=white)](docs/FEATURES.md)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)](docs/ORM-GUIDE.md)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1?logo=postgresql&logoColor=white)](docs/ORM-GUIDE.md)
[![MySQL](https://img.shields.io/badge/MySQL-8-4479A1?logo=mysql&logoColor=white)](docs/ORM-GUIDE.md)
[![Oracle](https://img.shields.io/badge/Oracle-Free-F80000?logo=oracle&logoColor=white)](docs/ORM-GUIDE.md)
[![MongoDB](https://img.shields.io/badge/MongoDB-7-47A248?logo=mongodb&logoColor=white)](docs/FEATURES.md)
[![Redis](https://img.shields.io/badge/Redis-Cache-DC382D?logo=redis&logoColor=white)](docs/FEATURES.md)
[![Azure](https://img.shields.io/badge/Azure-Storage%20%7C%20AppInsights-0078D4?logo=microsoftazure&logoColor=white)](docs/FEATURES.md)
[![AWS](https://img.shields.io/badge/AWS-S3-FF9900?logo=amazonaws&logoColor=white)](docs/FEATURES.md)
[![Google Cloud](https://img.shields.io/badge/GCP-Storage%20%7C%20Logging-4285F4?logo=googlecloud&logoColor=white)](docs/FEATURES.md)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](docker-compose.yml)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-Minikube-326CE5?logo=kubernetes&logoColor=white)](docs/KUBERNETES.md)
[![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?logo=swagger&logoColor=black)](docs/DATA-ANNOTATIONS-GUIDE.md)
[![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-Observability-7B5EA7?logo=opentelemetry&logoColor=white)](docs/TELEMETRY.md)
[![JWT](https://img.shields.io/badge/JWT-Auth-000000?logo=jsonwebtokens&logoColor=white)](docs/AUTHENTICATION.md)

---

## 📖 Documentação

- **[🚀 Início Rápido](QUICK-START.md)** - Comece em 5 minutos
- **[🧪 Testando Bancos de Dados](docs/TESTING-DATABASES.md)** - Teste com SQL Server, Oracle, PostgreSQL e MySQL
- **[📚 Guia Completo](README.md)** - Este documento
- **[🎛️ Recursos Avançados](docs/FEATURES.md)** - MongoDB, Quartz, RabbitMQ, Storage, etc.
- **[🗄️ Guia MongoDB](docs/MONGODB-GUIDE.md)** - Configuração, seed automático e troubleshooting
- **[🔄 Guia de ORMs](docs/ORM-GUIDE.md)** - Como alternar entre ORMs (EF Core, Dapper, ADO.NET)
- **[📊 Guia de Telemetria](docs/TELEMETRY.md)** - Observabilidade com OpenTelemetry
- **[🚦 Guia de Rate Limiting](docs/RATE-LIMITING.md)** - Controle de taxa de requisições
- **[📜 Guia de Event Sourcing](docs/EVENT-SOURCING.md)** - Auditoria completa e time travel
- **[🔐 Guia de Authentication](docs/AUTHENTICATION.md)** - JWT & OAuth2
- **[🔄 Guia de CI/CD](docs/CICD.md)** - GitHub Actions, Azure DevOps, GitLab CI
- **[☁️ Guia de SonarCloud](docs/SONARCLOUD.md)** - Análise de qualidade de código
- **[☸️ Guia Kubernetes](docs/KUBERNETES.md)** - Deploy em K8s

---

## 🎯 Visão Geral

Este template fornece uma estrutura completa e moderna para desenvolvimento de APIs em .NET 10, baseado nas melhores práticas e padrões de arquitetura. Foi criado a partir da experiência do projeto PNE-API e incorpora todos os aprendizados e melhorias implementados.

### ✨ Características Principais

- **Clean Architecture** com separação clara de responsabilidades
- **Suporte a múltiplos ORMs** (Entity Framework Core, Dapper, ADO.NET, NHibernate, Linq2Db)
- **Telemetria completa** com OpenTelemetry (Jaeger, Prometheus, Grafana, Application Insights, Datadog, Dynatrace)
- **Rate Limiting** com 4 estratégias (Fixed Window, Sliding Window, Token Bucket, Concurrency)
- **Event Sourcing** com Marten (PostgreSQL) para auditoria completa e time travel
- **Authentication** com JWT e OAuth2 (Google, Microsoft, GitHub)
- **CI/CD pronto** para GitHub Actions, Azure DevOps e GitLab CI
- **Infraestrutura modular** com extension methods
- **Configurações validadas** em tempo de startup
- **Health checks** prontos para produção
- **Cache distribuído** (Memory, Redis, SQL Server)
- **Logging estruturado** e observabilidade (Google Cloud Logging)
- **CORS configurável** por ambiente
- **Response compression** (Brotli/Gzip)
- **Dependency injection** automático com Scrutor
- **API versionamento** (URL, Header, Query)
- **Swagger customizado** (agrupamento, JWT, XML docs)
- **Exception notifications** (extensível para email/Slack)
- **Kubernetes ready** com manifests e scripts de deploy
- **Docker e Docker Compose** pré-configurados com stack completa de observabilidade
- **MongoDB support** (NoSQL opcional, com seed automático e script de criação do container)
- **Background jobs** com Quartz.NET
- **Message queue** com RabbitMQ
- **Cloud storage** (Google Cloud, Azure, AWS)
- **JWT Authentication** ready
- **Global exception handler** com ProblemDetails
- **Automatic validation** com FluentValidation

---

## 📁 Estrutura do Projeto

```text
GoodHamburger/
├── .devcontainer/                                # Ambiente Dev Container e Codespaces
│   ├── devcontainer.json
│   ├── docker-compose.devcontainer.yml
│   └── Dockerfile
├── .github/                                      # Automações, instruções e skills do projeto
│   ├── workflows/
│   │   ├── ci.yml
│   │   └── docs-check.yml
│   ├── instructions/
│   ├── skills/
│   └── prompts/
├── .k8s/                                         # Kubernetes manifests
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── deployment.yaml
│   ├── service.yaml
│   ├── ingress.yaml
│   └── kustomization.yaml
├── src/
│   ├── Api/                                      # Camada de apresentação (Controllers, Program.cs)
│   │   ├── Controllers/                          # Controllers da API
│   │   │   └── ApiControllerBase.cs              # Base controller com métodos helper
│   │   ├── appsettings.json                      # Configurações base
│   │   ├── appsettings.*.json                    # Configurações por ambiente/provider
│   │   └── Program.cs                            # Entry point da aplicação (com seeding automático)
│   │
│   ├── Application/                              # Camada de aplicação (Services, Business Logic)
│   │   └── Services/                             # Application services
│   │
│   ├── Domain/                                   # Camada de domínio (Entities, Interfaces)
│   │   ├── Entities/                             # Entidades de negócio
│   │   ├── Interfaces/                           # Contratos e interfaces
│   │   │   ├── IRepository.cs                    # Interface genérica de repositório
│   │   │   ├── IService.cs                       # Interface genérica de serviço
│   │   │   ├── IQueueService.cs                  # Interface para message queue
│   │   │   ├── IStorageService.cs                # Interface para cloud storage
│   │   │   └── IExceptionNotificationService.cs  # Interface para notificações
│   │   ├── Exceptions/                           # Exceções customizadas
│   │   │   └── DomainExceptions.cs               # BusinessException, NotFoundException, ValidationException
│   │   └── AppSettings.cs                        # Configurações fortemente tipadas
│   │
│   ├── Data/                                     # Camada de dados (Repositories, Context, Seeders)
│   │   ├── Context/                              # DbContext do EF Core
│   │   │   └── ApplicationDbContext.cs
│   │   ├── Repository/                           # Implementação dos repositórios
│   │   │   └── Repository.cs                     # Repositório genérico base
│   │   └── Seeders/                              # Database seeders
│   │       └── DbSeeder.cs                       # Seed de dados iniciais (150 produtos, 120 pedidos)
│   │
│   └── Infrastructure/                           # Camada de infraestrutura (Extensions, Middleware, Services)
│       ├── Extensions/                           # Extension methods modulares
│       │   ├── InfrastructureExtensions.cs       # Orquestrador principal
│       │   ├── AppSettingsExtension.cs           # Validação de configurações
│       │   ├── DatabaseExtension.cs              # Configuração de banco de dados
│       │   ├── CacheExtension.cs                 # Memory/Redis/SQL Server cache
│       │   ├── HealthChecksExtension.cs          # Health checks
│       │   ├── DependencyInjectionExtension.cs   # Scrutor auto-registration
│       │   ├── MongoExtension.cs                 # MongoDB support
│       │   ├── QuartzExtension.cs                # Background jobs (Quartz.NET)
│       │   ├── RabbitMqExtension.cs              # Message queue (RabbitMQ)
│       │   ├── StorageExtension.cs               # Cloud storage (Google/Azure/AWS)
│       │   ├── AuthenticationExtension.cs        # JWT Authentication
│       │   ├── ApiVersioningExtension.cs         # API Versioning
│       │   ├── LoggingExtensions.cs              # Google Cloud Logging
│       │   ├── SwaggerExtension.cs               # Swagger customizado
│       │   └── ExceptionHandlerExtension.cs      # Exception handler registration
│       │
│       ├── Middleware/                           # Middleware customizado
│       │   └── GlobalExceptionHandler.cs         # Tratamento global de exceções
│       │
│       ├── Filters/                              # Action filters
│       │   └── ValidationFilter.cs               # Validação automática com FluentValidation
│       │
│       ├── Services/                             # Serviços de infraestrutura
│       │   ├── QueueService.cs                   # Implementação RabbitMQ
│       │   ├── GoogleCloudStorageService.cs      # Implementação Google Cloud Storage
│       │   ├── AzureBlobStorageService.cs        # Implementação Azure Blob Storage
│       │   ├── AwsS3StorageService.cs            # Implementação AWS S3
│       │   └── ExceptionNotificationService.cs   # Notificações de exceção
│
├── tests/
│   ├── UnitTests/                    # Testes unitários (xUnit + Moq + FluentAssertions)
│   │   ├── Controllers/              # Testes de controllers
│   │   ├── Services/                 # Testes de serviços
│   │   ├── EventSourcing/            # Testes de event sourcing
│   │   ├── UnitTests.csproj          # Projeto de testes unitários
│   │   └── README.md                 # Documentação dos testes
│   │
│   └── Integration/                  # Testes de integração
│       ├── Controllers/              # Testes de integração dos controllers
│       ├── Support/                  # Configuração e utilitários de testes
│       ├── Properties/
│       ├── Integration.csproj        # Projeto de testes de integração
│       └── README.md                 # Documentação de testes de integração
│
├── docs/                             # Documentação adicional
│   ├── examples/                     # Arquivos de exemplo (.http e cenários)
│   ├── ARCHITECTURE.md               # Arquitetura Clean Architecture
│   ├── README.md                     # Índice da documentação
│   ├── FEATURES.md                   # Recursos avançados (MongoDB, Queue, Jobs, etc.)
│   ├── ORM-GUIDE.md                  # Guia de ORMs
│   ├── MONGODB-GUIDE.md              # Guia de uso do MongoDB
│   ├── DATA-ANNOTATIONS-GUIDE.md     # Guia de Data Annotations e Swagger
│   ├── PRODUCT-EXAMPLE.md            # Exemplo completo de produto
│   ├── CONFIGURATION-GUIDE.md        # Guia de Configuração (IOptions<T>)
│   ├── AUTHENTICATION.md             # Autenticação JWT & OAuth2
│   ├── SECURITY.md                   # Segurança da API
│   ├── RATE-LIMITING.md              # Rate Limiting
│   ├── EVENT-SOURCING.md             # Event Sourcing
│   ├── TELEMETRY.md                  # Observabilidade
│   ├── TESTING-DATABASES.md          # Testes multi-banco
│   ├── KUBERNETES.md                 # Guia de deploy Kubernetes
│   ├── CICD.md                       # CI/CD
│   └── SONARCLOUD.md                 # SonarCloud
│
├── monitoring/                       # Stack de observabilidade local
│   ├── grafana/
│   └── prometheus/
├── scripts/                          # Scripts de automação
│   ├── event-sourcing/
│   ├── linux/                        # Scripts bash (Minikube deploy/destroy/tests)
│   ├── mongo-init/
│   ├── windows/                      # Scripts PowerShell e Batch
│   ├── new-project.sh                # Script Linux/Mac de inicialização
│   ├── new-project.ps1               # Script PowerShell de inicialização
│   ├── new-project.bat               # Script Windows CMD de inicialização
│   └── README.md
│
├── compose-observability.yml         # Compose da stack de observabilidade
├── Dockerfile                        # Multi-stage build
├── docker-compose.yml                # Compose para desenvolvimento
├── cspell.config.yaml                # Configuração do cspell
├── package.json                      # Dependências de tooling de documentação
├── global.json                       # Versão do .NET SDK
├── GoodHamburger.sln               # Solution file
└── .gitignore                        # Git ignore configurado
```

---

## 🚀 Como Usar o Template

### Opção 1: Usando Script PowerShell (Recomendado para Windows)

```powershell
cd template/scripts
.\new-project.ps1
```

### Opção 2: Usando Script Bash (Linux/Mac)

```bash
cd template/scripts
chmod +x new-project.sh
./new-project.sh
```

### Opção 3: Usando Script Batch (Windows CMD)

```cmd
cd template\scripts
new-project.bat
```

> 💡 Os scripts são interativos — apresentam menus para configurar banco de dados, cache, mensageria, cloud storage, telemetria e event sourcing. Para modo não-interativo (CI/CD), veja [scripts/README.md](scripts/README.md).

---

## 🧱 Dev Container / Codespaces

> Requer Docker Desktop (ou Docker Engine) com suporte ao Compose v2 habilitado.

### VS Code (Dev Containers)

1. Instale a extensão **Dev Containers** (ms-vscode-remote.remote-containers).
2. Abra o repositório no VS Code e execute o comando `Dev Containers: Reopen in Container`.
3. O `.devcontainer` monta automaticamente o `docker-compose.yml` raiz, inicializando SQL Server, Oracle, PostgreSQL, MySQL, Postgres (event sourcing), Jaeger, Prometheus e Grafana.
4. Ao concluir o build, o comando `dotnet restore && dotnet tool restore` já terá sido executado dentro do container.
5. Use o terminal integrado para rodar `dotnet run --project src/Api` ou qualquer script; o workspace está disponível em `/workspace`.

### GitHub Codespaces

1. Clique em **Code ▸ Create codespace on main** (ou branch desejada).
2. O Codespace usa os mesmos arquivos do `.devcontainer`, então todas as dependências (SDK .NET 10 preview, Node 20, ferramentas de lint) já estarão disponíveis.
3. Os serviços definidos no Docker Compose são levantados automaticamente; acompanhe os logs na aba **Ports** e **Terminal**.
4. As portas mais comuns (3060/3062 para API, 16686 para Jaeger, 3000 para Grafana, 9090 para Prometheus) ficam encaminhadas e descritas no `devcontainer.json`.

> Dica: se não quiser subir todos os bancos, edite `runServices` em `.devcontainer/devcontainer.json` antes de abrir o container e remova os serviços dispensáveis.

---

## 📡 Observabilidade Plug-and-Play

- **Somente stack de telemetria**: `docker compose -f compose-observability.yml up -d`
- **API + observabilidade**: `docker compose -f docker-compose.yml -f compose-observability.yml up -d api`
- Os serviços Jaeger, Prometheus e Grafana já vêm com OTLP habilitado, health checks e persistência configurada.
- As portas expostas são: 16686 (Jaeger UI), 4317/4318 (OTLP), 9090 (Prometheus) e 3000 (Grafana).
- Para desmontar: `docker compose -f compose-observability.yml down -v` (remove volumes de métricas/dashboards).

---

## ⚙️ Configuração Inicial

Após criar seu projeto, siga estes passos:

### 1. Navegue até o diretório do projeto

```bash
cd MeuProjeto
```

### 2. Configure a Connection String

Edite `src/Api/appsettings.json` e ajuste a connection string:

```json
{
    "AppSettings": {
        "Infrastructure": {
            "Database": {
                "DatabaseType": "InMemory",
                "ConnectionString": ""
            }
        }
    }
}
```

### 3. Escolha seu Banco de Dados

Edite `src/Api/appsettings.json` e configure o tipo de banco e a connection string:

**Para SQL Server:**

```json
{
    "AppSettings": {
        "Infrastructure": {
            "Database": {
                "DatabaseType": "SqlServer",
                "ConnectionString": "Server=localhost;Database=MeuBanco;Trusted_Connection=True;TrustServerCertificate=True;"
            }
        }
    }
}
```

**Para Oracle:**

```json
{
    "AppSettings": {
        "Infrastructure": {
            "Database": {
                "DatabaseType": "Oracle",
                "ConnectionString": "User Id=myUsername;Password=myPassword;Data Source=localhost:1521/ORCL;"
            }
        }
    }
}
```

**Para PostgreSQL:**

```json
{
    "AppSettings": {
        "Infrastructure": {
            "Database": {
                "DatabaseType": "PostgreSQL",
                "ConnectionString": "Host=localhost;Database=MeuBanco;Username=postgres;Password=myPassword;"
            }
        }
    }
}
```

**Para MySQL:**

```json
{
    "AppSettings": {
        "Infrastructure": {
            "Database": {
                "DatabaseType": "MySQL",
                "ConnectionString": "Server=localhost;Database=MeuBanco;User=root;Password=myPassword;"
            }
        }
    }
}
```

> ✨ **Todos os providers já estão instalados!** Basta mudar o `DatabaseType` e a connection string.

**Nota sobre ORM**: Entity Framework Core, Dapper e ADO.NET estão habilitados simultaneamente. Para mais detalhes, veja [docs/ORM-GUIDE.md](docs/ORM-GUIDE.md).

### 4. Restaure os Pacotes

```bash
dotnet restore
### 5. Compile o Projeto

```bash
dotnet build
```

### 6. Crie a Primeira Migration

```bash
dotnet ef migrations add InitialCreate --project src/Data --startup-project src/Api
```

### 7. Aplique a Migration no Banco

```bash
dotnet ef database update --project src/Data --startup-project src/Api
```

### 8. Execute o Projeto

```bash
dotnet run --project src/Api
```

### 9. Acesse a API

- API: `https://localhost:3060`
- Swagger: `https://localhost:3060/swagger`
- Health Check: `https://localhost:3060/health`

### 10. Login com Credenciais Padrão 🔑

O sistema cria automaticamente um usuário administrador na primeira execução:

```text
Username: admin
Password: Admin@2026!Secure
Email:    admin@GoodHamburger.com
Role:     Admin
```

**Teste no Swagger:**

1. Vá para `/swagger`
2. Execute `POST /api/auth/login` com as credenciais acima
3. Copie o `accessToken` da resposta
4. Clique no botão "🔒 Authorize" no topo
5. Digite: `Bearer SEU_ACCESS_TOKEN`
6. Agora você pode testar todos os endpoints autenticados!

> ⚠️ **IMPORTANTE**: Altere esta senha em produção!

Para mais detalhes sobre autenticação, veja [docs/AUTHENTICATION.md](docs/AUTHENTICATION.md)

---

## 🔧 Configurações Avançadas

### Suporte a Múltiplos ORMs

O template foi projetado para suportar diferentes ORMs.

**Entity Framework Core é o padrão** e está habilitado no código.

Para trocar de ORM, **não use appsettings.json**. Edite diretamente o arquivo:

- **Arquivo**: `src/Infrastructure/Extensions/DatabaseExtension.cs`
- **Linha**: ~26 (procure por "DEFAULT: Entity Framework Core")

#### Entity Framework Core (Padrão ✅)

Já está habilitado. Não precisa fazer nada!

#### Dapper (Alta Performance 💤)

1. Abra `src/Infrastructure/Extensions/DatabaseExtension.cs`
2. Comente a linha do EF Core (linha ~26)
3. Descomente a linha do Dapper (linha ~29)
4. Veja [docs/ORM-GUIDE.md](docs/ORM-GUIDE.md) para implementação completa

#### NHibernate / Linq2Db (Preparados 💤)

1. Abra `src/Infrastructure/Extensions/DatabaseExtension.cs`
2. Comente a linha do EF Core (linha ~26)
3. Descomente a linha do ORM desejado
4. Veja [docs/ORM-GUIDE.md](docs/ORM-GUIDE.md) para implementação completa

### Configuração de Cache

#### Memory Cache (Padrão para desenvolvimento)

```json
{
  "AppSettings": {
    "Infrastructure": {
      "Cache": {
        "Enabled": true,
        "Provider": "Memory",
        "DefaultExpirationMinutes": 60
      }
    }
  }
}
```

#### Redis (Recomendado para produção)

```json
{
  "AppSettings": {
    "Infrastructure": {
      "Cache": {
        "Enabled": true,
        "Provider": "Redis",
        "ConnectionString": "localhost:6379",
        "DefaultExpirationMinutes": 60
      }
    }
  }
}
```

#### SQL Server Cache

```json
{
  "AppSettings": {
    "Infrastructure": {
      "Cache": {
        "Enabled": true,
        "Provider": "SqlServer",
        "ConnectionString": "Server=localhost;Database=CacheDb;...",
        "DefaultExpirationMinutes": 60
      }
    }
  }
}
```

---

## 📊 Health Checks

O template inclui health checks configurados:

- `/health` - Status geral da aplicação
- `/health/ready` - Readiness check (para Kubernetes)

Para adicionar health checks personalizados, edite `src/Infrastructure/Extensions/HealthChecksExtension.cs`

---

## 🏗️ Arquitetura

### Camadas

1. **Domain** - Entidades, interfaces e regras de negócio puras
2. **Data** - Implementação de acesso a dados e repositórios
3. **Application** - Casos de uso e lógica de aplicação
4. **Infrastructure** - Configurações, extensões e serviços externos
5. **Api** - Controllers, endpoints e apresentação

### Fluxo de Dependências

```text
Api → Infrastructure → Application → Data → Domain
                                       ↓
                                    Domain
```

---

## 🎨 Criando Novas Entidades

### 1. Crie a Entidade no Domain

```csharp
// src/Domain/Entities/Product.cs
namespace MeuProjeto.Domain.Entities;

public class Product : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
}
```

### 2. Crie o Repositório

```csharp
// src/Data/Repository/ProductRepository.cs
namespace MeuProjeto.Data.Repository;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Métodos customizados aqui
}
```

### 3. Crie o Service

```csharp
// src/Application/Services/ProductService.cs
namespace MeuProjeto.Application.Services;

public class ProductService : Service<Product>, IProductService
{
    public ProductService(IProductRepository repository, ILogger<ProductService> logger)
        : base(repository, logger)
    {
    }

    // Lógica de negócio customizada aqui
}
```

### 4. Crie o Controller

```csharp
// src/Api/Controllers/ProductController.cs
namespace MeuProjeto.Api.Controllers;

public class ProductController : ApiControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var (items, total) = await _productService.GetAllAsync(page, pageSize, cancellationToken);

        if (page.HasValue && pageSize.HasValue)
            return HandlePagedResult(items, total, page.Value, pageSize.Value);

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        return HandleResult(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest dto, CancellationToken cancellationToken)
    {
        var created = await _productService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductRequest dto, CancellationToken cancellationToken)
    {
        await _productService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
```

### 5. Adicione o DbSet ao Context

```csharp
// src/Data/Context/ApplicationDbContext.cs
public DbSet<Product> Products { get; set; }
```

### 6. Crie a Migration

```bash
dotnet ef migrations add AddProduct --project src/Data --startup-project src/Api
dotnet ef database update --project src/Data --startup-project src/Api
```

---

## 📝 Boas Práticas

### Dependency Injection

O template usa **Scrutor** com `.AsMatchingInterface()` para registro automático inteligente.

#### 🚀 Registro Automático

Seus repositórios e services são **automaticamente registrados** sem necessidade de configuração manual:

```csharp
// src/Infrastructure/Extensions/DependencyInjectionExtensions.cs
services.Scan(scan => scan
    .FromAssembliesOf(typeof(Repository<>))
    .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
    .AsMatchingInterface()  // ← Registra apenas interface correspondente
    .WithScopedLifetime()
);
```

**Como funciona:**

- `ProductRepository` → registrado como `IProductRepository`
- `ProductService` → registrado como `IProductService`
- `ProductDapperRepository` → registrado como `IProductDapperRepository`
- `ProductAdoRepository` → registrado como `IProductAdoRepository`
- **Sem conflitos** entre múltiplos ORMs! ✅

#### ✨ Adicionando Novos Repositórios

**1. Crie a interface específica:**

```csharp
public interface IProductDapperRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetTopSellingProductsAsync();
}
```

**2. Implemente a classe:**

```csharp
public class ProductDapperRepository : IProductDapperRepository
{
    // Implementação...
}
```

**3. Pronto!** 🎉 O Scrutor registrará automaticamente. Basta injetar:

```csharp
public class ProductService
{
    private readonly IProductRepository _efRepository;            // EF Core
    private readonly IProductDapperRepository _dapperRepository;  // Dapper

    public ProductService(
        IProductRepository efRepository,
        IProductDapperRepository dapperRepository)
    {
        _efRepository = efRepository;
        _dapperRepository = dapperRepository;
    }
}
```

**Convenções necessárias:**

- Interface: `IProductDapperRepository` (prefixo `I` + nome da classe)
- Classe: `ProductDapperRepository` (implementa a interface)
- Herança: `IProductDapperRepository : IRepository<T>`

### Async/Await

Sempre use operações assíncronas:

```csharp
// ✅ Correto
var result = await _service.GetByIdAsync(id, cancellationToken);

// ❌ Errado
var result = _service.GetByIdAsync(id).Result;
```

### CancellationToken

Sempre propague o CancellationToken em métodos assíncronos:

```csharp
public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
{
    var items = await _service.GetAllAsync(cancellationToken);
    return Ok(items);
}
```

### Logging

Use ILogger para logging estruturado:

```csharp
_logger.LogInformation("Processing request for {Id}", id);
_logger.LogError(ex, "Error processing {Id}", id);
```

---

## 🐳 Docker

Para criar uma imagem Docker do seu projeto:

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Api/Api.csproj", "src/Api/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Data/Data.csproj", "src/Data/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/Api/Api.csproj"
COPY . .
WORKDIR "/src/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MeuProjeto.Api.dll"]
```

---

## 🐳 Docker e Kubernetes

### Docker

#### Build da imagem

```bash
docker build -t GoodHamburger-api:latest .
```

#### Executar com docker-compose

```bash
docker-compose up -d
```

Acesse: `http://localhost:8080`

### Kubernetes

O template inclui manifestos Kubernetes prontos para deploy em Minikube (local) ou clusters em produção.

#### Deploy Local (Minikube)

**Windows (PowerShell):**

```powershell
cd scripts/windows
.\minikube-deploy.ps1
```

**Linux/macOS:**

```bash
cd scripts/linux
chmod +x minikube-deploy.sh
./minikube-deploy.sh
```

O script automaticamente:

1. Verifica pré-requisitos (Docker, Minikube, kubectl)
2. Inicia o Minikube
3. Constrói a imagem Docker
4. Carrega a imagem no Minikube
5. Aplica os manifestos Kubernetes
6. Exibe o status dos pods

#### Acessar a aplicação no Minikube

```bash
# Port forward
kubectl port-forward svc/GoodHamburger-api 8080:80 -n GoodHamburger

# Ou usar Minikube tunnel
minikube tunnel
```

#### Remover deploy do Minikube

**Windows (PowerShell):**

```powershell
cd scripts/windows
.\minikube-destroy.ps1
```

**Linux/macOS:**

```bash
cd scripts/linux
chmod +x minikube-destroy.sh
./minikube-destroy.sh
```

#### Deploy em Produção

Para deploy em clusters de produção (AKS, EKS, GKE, etc.), consulte o guia detalhado em [`docs/KUBERNETES.md`](docs/KUBERNETES.md).

---

## 🧪 Testes

O template inclui estrutura para testes:

### Testes de Integração

```bash
dotnet test tests/Integration/
```

### Testes Unitários

```bash
dotnet test tests/UnitTests/
```

### Script Automatizado (Minikube)

Execute testes de integração automaticamente no Minikube:

**Windows (PowerShell):**

```powershell
cd scripts/windows
.\run-integration-tests.ps1
```

**Linux/macOS:**

```bash
cd scripts/linux
chmod +x run-integration-tests.sh
./run-integration-tests.sh
```

---

## 📚 Recursos Adicionais

### Documentação do Template

- **[ORM-GUIDE.md](docs/ORM-GUIDE.md)** - Guia completo sobre ORMs suportados e como alternar entre eles
- **[KUBERNETES.md](docs/KUBERNETES.md)** - Guia detalhado de deploy no Kubernetes (local e produção)

### Documentação Externa

- [Documentação .NET 10](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Kubernetes](https://kubernetes.io/docs/)
- [Docker](https://docs.docker.com/)

---

## 🤝 Contribuindo

Contribuições são bem-vindas! Para contribuir:

1. Fork o repositório
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

---

## 📄 Licença

Este template está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

---
