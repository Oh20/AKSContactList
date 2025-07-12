# 📞 AKS Contact List

Projeto desenvolvido para o **TechChallenge da FIAP**, com foco em demonstrar **estabilidade**, **escalabilidade** e **alta disponibilidade** por meio do **Azure Kubernetes Service (AKS)**, utilizando uma arquitetura de **microsserviços com mensageria**.

---

## 🎯 Objetivo

Criar uma aplicação de **lista telefônica**, onde cada funcionalidade está desacoplada e dividida em **serviços independentes**, executando em seus próprios **pods** no AKS. A comunicação entre os serviços ocorre de forma assíncrona via **RabbitMQ**, promovendo um sistema escalável e resiliente.

---

## 🛠️ Tecnologias Utilizadas

- **.NET 8 (C#)**
- **RabbitMQ** (mensageria)
- **Azure SQL Server**
- **Docker**
- **AKS (Azure Kubernetes Service)**
- **GitHub Actions** (CI/CD)
- **Entity Framework In-Memory** (testes)

---

## 🧱 Arquitetura

A aplicação é composta por diversos **microsserviços**, cada um com uma responsabilidade única, que se comunicam por meio de **filas do RabbitMQ**. O deploy é automatizado com GitHub Actions e os containers são armazenados no **Docker Hub** e implantados no **AKS**.

> 🔍 Benefícios da arquitetura:
> - Independência de deploy e escalabilidade por serviço.
> - Alta disponibilidade com Kubernetes.
> - Resiliência com mensageria assíncrona.
> - Automação com pipelines de CI/CD.
> - Testes rápidos e confiáveis com banco em memória.

---

## ⚙️ Serviços e Endpoints

| Serviço | Método | Rota | Descrição |
|--------|--------|------|-----------|
| **ContactCreateUpdateService** | POST | `/contatos` | Cria um novo contato via fila de criação. |
|  | PUT | `/editcontatos/{username}` | Atualiza dados de um contato. |
|  | GET | `/health` | Retorna status de saúde do serviço. |
| **ContactDeleteService** | DELETE | `/contatos/deleteID/{id}` | Deleta contato por ID. |
|  | GET | `/health` | Retorna status de saúde do serviço. |
| **ContactQueryService** | GET | `/contatos` | Retorna lista completa de contatos. |
|  | GET | `/contatos/por-ddd/{ddd}` | Retorna lista filtrada por DDD. |
|  | GET | `/health` | Retorna status de saúde do serviço. |
| **DB_Consumer** | - | - | Consome mensagens e persiste dados no banco via EF Core. |
|  | GET | `/health` | Retorna status de saúde do serviço. |

---

## 🧪 Testes

O projeto implementa uma estratégia de testes robusta com **banco de dados em memória** para garantir rapidez e confiabilidade:

### ✅ **Testes Unitários**
- **Execução**: Rápida (< 30 segundos)
- **Dependências**: Apenas mocks
- **Cobertura**: ~80% do código
- **Status**: ✅ Funcionando no CI/CD

### ✅ **Testes de Integração com Banco em Memória**
- **Execução**: Rápida e confiável
- **Dependências**: Entity Framework In-Memory
- **Vantagens**: Sem recursos externos, determinístico
- **Status**: ✅ Funcionando no CI/CD

### 🚀 **Como Executar Testes**

```bash
# Executar todos os testes
dotnet test

# Executar testes específicos
dotnet test ./DBConsumer.Tests/DBConsumer.Tests.csproj
dotnet test ./ContactQueryService.Tests/ContactQueryService.Tests.csproj
dotnet test ./ContactCreateUpdateService.Tests/ContactCreateUpdateService.Tests.csproj

# Executar apenas testes de integração
dotnet test --filter "Category=DatabaseIntegration"

# Usar script PowerShell (Windows)
.\run-tests.ps1
```

---

## 🐳 CI/CD com GitHub Actions

- Os pipelines estão configurados para:
  - **Buildar** os serviços com Docker.
  - **Executar testes** com banco em memória.
  - **Publicar** as imagens no Docker Hub.
  - **Efetuar deploy automático** para o AKS.
- As **credenciais** estão armazenadas de forma segura usando `GitHub Secrets`.

---

## 🧪 Como Executar Localmente (opcional)

> Caso o professor deseje testar localmente:

1. Instalar Docker e .NET 8 SDK.
2. Clonar o repositório:
   ```bash
   git clone https://github.com/seu-usuario/nome-do-repo.git
   ```
3. Executar testes:
   ```bash
   dotnet test
   ```
4. Executar com Docker Compose (caso configurado):
   ```bash
   docker-compose up
   ```

---

## 👨‍🏫 Observações para Avaliação

- Toda a aplicação foi criada com foco em boas práticas DevOps e Cloud Native.
- O sistema é **modular**, com **health checks** em todos os serviços, **CI/CD funcional** e pronto para escalar horizontalmente.
- Cada serviço implementa apenas uma responsabilidade específica (CRUD, leitura, persistência, etc.), seguindo os princípios de separação de preocupações e arquitetura limpa.
- **Testes robustos** com banco em memória garantem rapidez e confiabilidade.
- Todos os microsserviços implementam **endpoints de Health Check**, permitindo que o Kubernetes monitore e reinicie automaticamente qualquer serviço que falhe, garantindo **resiliência e disponibilidade contínua**.

---

## 👨‍💻 Autor

- **Antony Nascimento** — Pós-graduação FIAP — Arquitetura de Sistemas .Net com Azure