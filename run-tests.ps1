# Script para executar testes do projeto Contact Microservices
# Autor: Antony Nascimento
# Data: 2025

Write-Host "🧪 Executando Testes - Contact Microservices" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""

# Verificar se o .NET está instalado
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET encontrado: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET não encontrado. Instale o .NET 8.0 SDK." -ForegroundColor Red
    exit 1
}

# Restaurar dependências
Write-Host "📦 Restaurando dependências..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Falha ao restaurar dependências" -ForegroundColor Red
    exit 1
}

# Build do projeto
Write-Host "🔨 Fazendo build do projeto..." -ForegroundColor Yellow
dotnet build --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Falha no build" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build concluído com sucesso!" -ForegroundColor Green
Write-Host ""

# Executar testes
Write-Host "🧪 Executando testes..." -ForegroundColor Yellow
Write-Host ""

# Teste 1: ContactCreateUpdateService
Write-Host "📝 Testando ContactCreateUpdateService..." -ForegroundColor Cyan
dotnet test ./ContactCreateUpdateService.Tests/ContactCreateUpdateService.Tests.csproj --no-build --verbosity normal
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Falha nos testes do ContactCreateUpdateService" -ForegroundColor Red
    $failedTests = $true
}

Write-Host ""

# Teste 2: ContactQueryService
Write-Host "🔍 Testando ContactQueryService..." -ForegroundColor Cyan
dotnet test ./ContactQueryService.Tests/ContactQueryService.Tests.csproj --no-build --verbosity normal
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Falha nos testes do ContactQueryService" -ForegroundColor Red
    $failedTests = $true
}

Write-Host ""

# Teste 3: DBConsumer
Write-Host "💾 Testando DBConsumer..." -ForegroundColor Cyan
dotnet test ./DBConsumer.Tests/DBConsumer.Tests.csproj --no-build --verbosity normal
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Falha nos testes do DBConsumer" -ForegroundColor Red
    $failedTests = $true
}

Write-Host ""

# Resumo
Write-Host "📊 RESUMO DOS TESTES" -ForegroundColor Green
Write-Host "===================" -ForegroundColor Green
Write-Host ""

if ($failedTests) {
    Write-Host "❌ Alguns testes falharam!" -ForegroundColor Red
    Write-Host "💡 Dicas:" -ForegroundColor Yellow
    Write-Host "   - Verifique se todas as dependências estão instaladas" -ForegroundColor White
    Write-Host "   - Execute 'dotnet restore' para restaurar pacotes" -ForegroundColor White
    Write-Host "   - Verifique se o .NET 8.0 SDK está instalado" -ForegroundColor White
} else {
    Write-Host "✅ Todos os testes passaram com sucesso!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎉 Parabéns! Seu código está funcionando corretamente." -ForegroundColor Green
}

Write-Host ""
Write-Host "💾 Banco de dados usado: In-Memory (Entity Framework)" -ForegroundColor Cyan
Write-Host "⚡ Performance: Rápida e confiável" -ForegroundColor Cyan
Write-Host "🔒 Segurança: Sem dependências externas" -ForegroundColor Cyan

Write-Host ""
Write-Host "📅 Executado em: $(Get-Date)" -ForegroundColor Gray 