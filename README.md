# Teste Protech — API E-commerce

API REST em .NET 8 com SQL Server, Swagger e logs estruturados (Serilog + Seq).

## Arquitetura

O projeto segue **Clean Architecture** (arquitetura em camadas com dependências apontando para o domínio), organizado em quatro níveis:

| Camada | Responsabilidade |
|--------|------------------|
| **Domain** | Entidades ricas (`Buyer`, `Product`, `Category`, `Order`), enums, exceções de negócio e códigos de erro padronizados |
| **Application** | Casos de uso (serviços), DTOs, mappers e interfaces de repositório — orquestra o domínio sem depender de infraestrutura |
| **Infrastructure** | EF Core, `ApplicationDbContext`, repositórios concretos, migrations e seed de dados |
| **Api** (`src/Api`) | Minimal APIs versionadas (`/api/v1/...`), contratos de request/response, middleware global de exceções e Swagger |

**Padrões e decisões técnicas:**

- **Minimal APIs** com agrupamento por recurso e versionamento via URL (`Asp.Versioning`)
- **Repository pattern** — a camada de aplicação depende de abstrações (`IBuyerRepository`, `IOrderRepository`, etc.)
- **Rich domain model** — validações e transições de estado ficam nas entidades (ex.: ciclo de vida do pedido)
- **Tratamento centralizado de erros** — `ExceptionHandlingMiddleware` converte exceções em respostas [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) (`application/problem+json`)
- **Observabilidade** — Serilog com sinks para console, arquivo e Seq; health check do SQL Server via `AddDbContextCheck`
- **Injeção de dependência** — registrada em `AddApplication()` e `AddInfrastructure()`

## Features

### Compradores (`/api/v1/buyers`)

- Listar, obter por ID e criar compradores
- Validação de nome (3–150 caracteres) e e-mail único
- Listar pedidos de um comprador com paginação e filtros

### Categorias (`/api/v1/categories`)

- CRUD completo de categorias
- Suporte a hierarquia (categoria pai/filha)
- Seed automático com 15 categorias padrão na primeira execução
- Proteção contra exclusão quando há subcategorias ou produtos vinculados

### Produtos (`/api/v1/products`)

- CRUD completo de produtos (nome, preço, marca, cor, descrição)
- Associação a uma ou mais categorias
- Flag de ativo/inativo na atualização

### Pedidos (`/api/v1/orders`)

- Criação com múltiplos itens (produto + quantidade)
- Ciclo de vida com status: `INICIADO` → `PROCESSADO` → `ENVIADO` ou `CANCELADO`
- Regras de negócio:
  - Alteração de itens/comprador permitida apenas em pedidos `INICIADO`
  - Cancelamento permitido em `INICIADO` ou `PROCESSADO`
  - Exclusão permitida apenas em `INICIADO`
  - Produto duplicado no mesmo pedido não é permitido
- Listagem paginada com filtros por status, período de criação e comprador

### Infraestrutura e qualidade

- Documentação interativa via **Swagger** em Development
- **Health check** em `/health` e `/api/v1/health` (inclui verificação do banco)
- **Logs estruturados** com Serilog (console, arquivo rotativo e Seq)
- **Migrations automáticas** e seed na inicialização em ambiente Development
- **Testes unitários** cobrindo domínio, serviços, mappers e tratamento de exceções

## Pré-requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://docs.docker.com/get-docker/) e Docker Compose

## Configuração

Copie o arquivo de ambiente e ajuste a senha do SQL Server, se necessário:

```bash
cp .env.example .env
```

A variável `MSSQL_SA_PASSWORD` é usada tanto pelo Docker Compose quanto pela API para montar a connection string automaticamente.

## Executar a aplicação

### 1. Subir dependências (SQL Server)

```bash
docker compose up -d sqlserver
```

Para subir SQL Server e Seq (logs) juntos:

```bash
docker compose up -d
```

### 2. Rodar a API

```bash
dotnet restore
dotnet run --project src/Api/Api.csproj
```

Em Development, as migrações e o seed do banco são aplicados automaticamente na inicialização.

### 3. Acessar

| Recurso | URL |
|---------|-----|
| Swagger | http://localhost:5004/swagger |
| Health check | http://localhost:5004/health |
| Seq (logs) | http://localhost:5341 |

Perfil HTTPS (opcional):

```bash
dotnet run --project src/Api/Api.csproj --launch-profile https
```

## Testes

```bash
dotnet test teste.sln
```

## Comandos úteis

```bash
# Restaurar dependências
dotnet restore teste.sln

# Compilar
dotnet build teste.sln

# Apenas Seq (sem SQL Server)
docker compose up -d seq

# Parar containers
docker compose down

# Recriar banco (se a senha do .env foi alterada após o primeiro start)
docker compose down -v
docker compose up -d sqlserver
```

## Variáveis de ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `MSSQL_SA_PASSWORD` | Senha do usuário `sa` no SQL Server | `YourStrong@Passw0rd` |
| `DB_SERVER` | Host e porta do SQL Server | `127.0.0.1,1433` |
| `DB_NAME` | Nome do banco | `EcommerceDb` |
| `ConnectionStrings__DefaultConnection` | Connection string completa (sobrescreve as variáveis acima) | — |

## Estrutura

```
teste-protech/
├── src/
│   ├── Api/                # Host HTTP (Program, Endpoints, Contracts, Middleware)
│   ├── Application/        # Casos de uso e serviços
│   ├── Domain/             # Entidades e regras de domínio
│   └── Infrastructure/     # EF Core, repositórios, migrations
└── tests/
    └── UnitTests/          # Testes unitários (Domain + Application)
```

### EF Core (migrations)

```bash
dotnet ef migrations add NomeDaMigration \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```
