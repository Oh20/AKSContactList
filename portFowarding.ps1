# Definir os serviços e as portas a serem mapeadas
$services = @(
    @{ name = "contact-dbapi-service"; localPort = 32730; servicePort = 80 },
    @{ name = "contact-create-service"; localPort = 32720; servicePort = 80 },
    @{ name = "contact-delete-service"; localPort = 32700; servicePort = 80 },
    @{ name = "contact-query-service"; localPort = 32710; servicePort = 80 },
    @{ name = "rabbitmq-service"; localPort = 5672; servicePort = 5672 },
    @{ name = "rabbitmq-service"; localPort = 15672; servicePort = 15672 }
)

$servicesMon = @(
    @{ name = "prometheus-stack-grafana"; localPort = 3000; servicePort = 80 },
    @{ name = "prometheus-stack-kube-prom-prometheus"; localPort = 9090; servicePort = 9090 }
)

# Armazenar processos em uma lista
$processes = @()

# Iniciar o redirecionamento para cada serviço
foreach ($service in $services) {
    $command = "port-forward svc/$($service.name) $($service.localPort):$($service.servicePort)"
    Write-Host "Iniciando redirecionamento para $($service.name) em http://localhost:$($service.localPort)..."
    $process = Start-Process kubectl -ArgumentList $command -NoNewWindow -PassThru
    $processes += $process
}
foreach ($service in $servicesMon) {
    $command = "port-forward -n monitoring svc/$($service.name) $($service.localPort):$($service.servicePort)"
    Write-Host "Iniciando redirecionamento para $($service.name) em http://localhost:$($service.localPort)..."
    $process = Start-Process kubectl -ArgumentList $command -NoNewWindow -PassThru
    $processes += $process
}

# Mostrar informações dos processos
Write-Host "`nTodos os redirecionamentos foram iniciados:"
foreach ($service in $services) {
    Write-Host "- $($service.name): http://localhost:$($service.localPort)"
}

# Aguardar comando do usuário para encerrar
Write-Host "`nPressione qualquer tecla para parar todos os redirecionamentos..."
[System.Console]::ReadKey() > $null

# Parar todos os processos
Write-Host "`nEncerrando todos os redirecionamentos..."
foreach ($process in $processes) {
    Stop-Process -Id $process.Id -Force
    Write-Host "- Processo $($process.Id) encerrado."
}

Write-Host "Todos os redirecionamentos foram encerrados."
