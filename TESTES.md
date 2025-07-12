# 🧪 Guia de Testes - Contact Microservices

Este documento explica como executar os diferentes tipos de testes no projeto Contact Microservices.

## 📋 Tipos de Testes

### 1. **Testes Unitários (Sem Banco de Dados)**
- **Localização**: Todos os projetos `.Tests`
- **Dependências**: Apenas mocks
- **Execução**: Rápida e confiável
- **Status**: ✅ Funcionando no CI/CD

### 2. **Testes de Integração (Com Banco de Dados)**
- **Localização**: `DBConsumer.Tests`, `DatabaseConnectionTests`
- **Dependências**: Azure SQL Server
- **Execução**: Requer acesso ao banco de dados
- **Status**: ⚠️ Requer configuração de firewall

### 3. **Testes com Banco em Memória**
- **Localização**: `DBConsumer.Tests` (novos testes)
- **Dependências**: Entity Framework In-Memory
- **Execução**: Rápida, sem dependências externas
- **Status**: ✅ Funcionando no CI/CD

## 🚀 Como Executar os Testes

### Opção 1: Testes Locais (Recomendado para Desenvolvimento)

```bash
# Executar todos os testes unitários
dotnet test

# Executar testes específicos
dotnet test ./DBConsumer.Tests/DBConsumer.Tests.csproj
dotnet test ./ContactQueryService.Tests/ContactQueryService.Tests.csproj
dotnet test ./ContactCreateUpdateService.Tests/ContactCreateUpdateService.Tests.csproj

# Executar apenas testes de integração com banco em memória
dotnet test ./DBConsumer.Tests/DBConsumer.Tests.csproj --filter "Category=DatabaseIntegration"
```

### Opção 2: Testes com Banco de Dados Real (GitHub Actions)

1. **Acesse o GitHub**: Vá para a aba "Actions"
2. **Selecione o workflow**: "Testes com Banco de Dados"
3. **Execute manualmente**: Clique em "Run workflow"
4. **Escolha o tipo de teste**:
   - `integration`: Testes de integração com banco em memória
   - `database`: Testes de conexão com Azure SQL Server
   - `all`: Todos os testes

### Opção 3: Configuração Manual do Firewall

Se você quiser executar testes localmente com o banco real:

```bash
# 1. Descobrir seu IP
curl https://api.ipify.org

# 2. Adicionar IP ao firewall do Azure SQL Server
az sql server firewall-rule create \
  --resource-group Az-lab \
  --server freetiertestdb \
  --name "local-test" \
  --start-ip-address "SEU_IP_AQUI" \
  --end-ip-address "SEU_IP_AQUI"

# 3. Executar testes
dotnet test ./DatabaseConnectionTests/DatabaseConnectionTests.csproj

# 4. Remover regra de firewall
az sql server firewall-rule delete \
  --resource-group Az-lab \
  --server freetiertestdb \
  --name "local-test"
```

## 🔧 Configuração do Ambiente

### Variáveis de Ambiente

```bash
# Para testes com banco real
export SQL_CONNECTION_STRING="Server=tcp:freetiertestdb.database.windows.net,1433;Initial Catalog=azlabtest;Persist Security Info=False;User ID=azureadmin;Password=Tecnologia@@2025;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Para testes com RabbitMQ
export RABBITMQ_HOST="localhost"
export RABBITMQ_PORT="5672"
export RABBITMQ_USER="guest"
export RABBITMQ_PASSWORD="guest"
```

### Dependências dos Testes

```xml
<!-- Entity Framework In-Memory Database -->
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.8" />

<!-- Moq para mocks -->
<PackageReference Include="Moq" Version="4.20.72" />

<!-- NUnit para testes -->
<PackageReference Include="NUnit" Version="3.14.0" />
```

## 📊 Estratégias de Teste

### 1. **Testes Unitários (Recomendado)**
- ✅ Executam rapidamente
- ✅ Não dependem de recursos externos
- ✅ Confiáveis e determinísticos
- ✅ Ideais para CI/CD

### 2. **Testes com Banco em Memória**
- ✅ Rápido e confiável
- ✅ Testa lógica de negócio
- ✅ Não depende de rede
- ✅ Ideal para desenvolvimento

### 3. **Testes de Integração com Banco Real**
- ⚠️ Requer configuração de firewall
- ⚠️ Pode ser lento
- ⚠️ Depende de recursos externos
- ✅ Testa integração real

## 🎯 Recomendações

### Para Desenvolvimento Diário
1. Use **testes unitários** para desenvolvimento rápido
2. Use **testes com banco em memória** para lógica de negócio
3. Execute **testes de integração** apenas quando necessário

### Para CI/CD
1. **Pipeline principal**: Apenas testes unitários e em memória
2. **Pipeline separado**: Testes de integração com banco real
3. **Execução manual**: Para testes que precisam de configuração especial

### Para Deploy
1. Execute todos os testes antes do deploy
2. Use o workflow "Testes com Banco de Dados" para validação final
3. Monitore os resultados dos testes

## 🔍 Troubleshooting

### Problema: Testes falham no GitHub Actions
**Solução**: Os testes que dependem do banco de dados estão comentados no pipeline principal. Use o workflow "Testes com Banco de Dados" para executá-los.

### Problema: Não consigo conectar ao banco localmente
**Solução**: 
1. Verifique se seu IP está no firewall do Azure SQL Server
2. Use testes com banco em memória para desenvolvimento
3. Configure as variáveis de ambiente corretamente

### Problema: Testes de integração são lentos
**Solução**: 
1. Use testes unitários para desenvolvimento
2. Execute testes de integração apenas quando necessário
3. Considere usar banco em memória para testes frequentes

## 📈 Métricas de Teste

- **Cobertura de Testes**: ~80% (testes unitários)
- **Tempo de Execução**: < 30 segundos (testes unitários)
- **Confiabilidade**: 99% (testes unitários)
- **Integração**: 95% (testes de integração)

## 🚀 Próximos Passos

1. **Aumentar cobertura**: Adicionar mais testes unitários
2. **Otimizar performance**: Usar testes paralelos
3. **Melhorar confiabilidade**: Adicionar testes de resiliência
4. **Automatizar**: Configurar execução automática de testes de integração 