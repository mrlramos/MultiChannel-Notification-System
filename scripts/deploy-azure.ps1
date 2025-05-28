# Script PowerShell para deploy do MultiChannel Notification System na Azure
# Autor: Sistema de Notificações Multi-Canal
# Data: 2025-05-28

param(
    [string]$Environment = "dev",
    [string]$Location = "East US",
    [switch]$SkipBuild,
    [switch]$Help
)

# Função para mostrar ajuda
function Show-Help {
    Write-Host @"
🚀 Deploy do MultiChannel Notification System na Azure

USAGE:
    .\deploy-azure.ps1 [OPTIONS]

OPTIONS:
    -Environment    Ambiente de deploy (dev, staging, prod). Default: dev
    -Location       Região do Azure. Default: East US
    -SkipBuild      Pula o build das imagens Docker
    -Help           Mostra esta ajuda

EXAMPLES:
    .\deploy-azure.ps1
    .\deploy-azure.ps1 -Environment prod -Location "West Europe"
    .\deploy-azure.ps1 -SkipBuild

PREREQUISITES:
    - Azure CLI instalado
    - Terraform instalado
    - Docker Desktop rodando
    - Subscription do Azure ativa
"@
}

if ($Help) {
    Show-Help
    exit 0
}

# Configurações
$ErrorActionPreference = "Stop"
$ProjectName = "multichannel-notification"

# Função para logging com cores
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    
    switch ($Level) {
        "SUCCESS" { Write-Host "[$timestamp] ✅ $Message" -ForegroundColor Green }
        "WARNING" { Write-Host "[$timestamp] ⚠️  $Message" -ForegroundColor Yellow }
        "ERROR"   { Write-Host "[$timestamp] ❌ $Message" -ForegroundColor Red }
        default   { Write-Host "[$timestamp] 🔵 $Message" -ForegroundColor Blue }
    }
}

# Função para verificar dependências
function Test-Dependencies {
    Write-Log "Verificando dependências..."
    
    $dependencies = @(
        @{ Name = "Azure CLI"; Command = "az"; InstallUrl = "https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" },
        @{ Name = "Terraform"; Command = "terraform"; InstallUrl = "https://www.terraform.io/downloads.html" },
        @{ Name = "Docker"; Command = "docker"; InstallUrl = "https://docs.docker.com/get-docker/" }
    )
    
    foreach ($dep in $dependencies) {
        try {
            $null = Get-Command $dep.Command -ErrorAction Stop
            Write-Log "$($dep.Name) encontrado" "SUCCESS"
        }
        catch {
            Write-Log "$($dep.Name) não está instalado. Instale: $($dep.InstallUrl)" "ERROR"
            exit 1
        }
    }
    
    Write-Log "Todas as dependências estão instaladas" "SUCCESS"
}

# Função para login na Azure
function Connect-Azure {
    Write-Log "Verificando login na Azure..."
    
    try {
        $null = az account show 2>$null
        Write-Log "Já logado na Azure" "SUCCESS"
    }
    catch {
        Write-Log "Fazendo login na Azure..."
        az login
        if ($LASTEXITCODE -ne 0) {
            Write-Log "Falha no login da Azure" "ERROR"
            exit 1
        }
    }
    
    # Listar subscriptions
    Write-Log "Subscriptions disponíveis:"
    az account list --output table
    
    # Permitir seleção de subscription
    $subscriptionId = Read-Host "Digite o ID da subscription que deseja usar"
    az account set --subscription $subscriptionId
    
    if ($LASTEXITCODE -ne 0) {
        Write-Log "Falha ao definir subscription" "ERROR"
        exit 1
    }
    
    Write-Log "Login na Azure realizado com sucesso" "SUCCESS"
    return $subscriptionId
}

# Função para configurar variáveis
function Set-Variables {
    param([string]$SubscriptionId)
    
    Write-Log "Configurando variáveis de ambiente..."
    
    # Obter informações da subscription
    $tenantId = az account show --query tenantId -o tsv
    
    # Criar Service Principal se não existir
    $spName = "$ProjectName-$Environment-sp"
    
    try {
        $existingSp = az ad sp list --display-name $spName --query "[0].appId" -o tsv 2>$null
        
        if ([string]::IsNullOrEmpty($existingSp)) {
            Write-Log "Criando Service Principal..."
            $spCredentials = az ad sp create-for-rbac --name $spName --role Contributor --scopes "/subscriptions/$SubscriptionId" | ConvertFrom-Json
            $clientId = $spCredentials.appId
            $clientSecret = $spCredentials.password
        }
        else {
            Write-Log "Service Principal já existe. Informe as credenciais." "WARNING"
            $clientId = Read-Host "Digite o Client ID"
            $clientSecret = Read-Host "Digite o Client Secret" -AsSecureString
            $clientSecret = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($clientSecret))
        }
    }
    catch {
        Write-Log "Erro ao configurar Service Principal: $_" "ERROR"
        exit 1
    }
    
    $script:TerraformVars = @{
        environment = $Environment
        location = $Location
        subscription_id = $SubscriptionId
        tenant_id = $tenantId
        client_id = $clientId
        client_secret = $clientSecret
    }
    
    Write-Log "Variáveis configuradas" "SUCCESS"
}

# Função para build das imagens
function Build-Images {
    if ($SkipBuild) {
        Write-Log "Pulando build das imagens (SkipBuild especificado)" "WARNING"
        return
    }
    
    Write-Log "Fazendo build das imagens Docker..."
    
    $currentDir = Get-Location
    Set-Location (Split-Path $PSScriptRoot -Parent)
    
    try {
        docker-compose build --no-cache
        if ($LASTEXITCODE -ne 0) {
            throw "Falha no build das imagens"
        }
        Write-Log "Imagens Docker criadas com sucesso" "SUCCESS"
    }
    catch {
        Write-Log "Erro no build das imagens: $_" "ERROR"
        exit 1
    }
    finally {
        Set-Location $currentDir
    }
}

# Função para deploy da infraestrutura
function Deploy-Infrastructure {
    Write-Log "Fazendo deploy da infraestrutura com Terraform..."
    
    $terraformDir = Join-Path (Split-Path $PSScriptRoot -Parent) "infrastructure\terraform"
    $currentDir = Get-Location
    Set-Location $terraformDir
    
    try {
        # Inicializar Terraform
        terraform init
        if ($LASTEXITCODE -ne 0) {
            throw "Falha na inicialização do Terraform"
        }
        
        # Criar arquivo de variáveis
        $tfvarsContent = @"
environment = "$($script:TerraformVars.environment)"
location = "$($script:TerraformVars.location)"
subscription_id = "$($script:TerraformVars.subscription_id)"
tenant_id = "$($script:TerraformVars.tenant_id)"
client_id = "$($script:TerraformVars.client_id)"
client_secret = "$($script:TerraformVars.client_secret)"
"@
        $tfvarsContent | Out-File -FilePath "terraform.tfvars" -Encoding UTF8
        
        # Planejar deployment
        terraform plan -out=tfplan
        if ($LASTEXITCODE -ne 0) {
            throw "Falha no planejamento do Terraform"
        }
        
        # Aplicar mudanças
        terraform apply tfplan
        if ($LASTEXITCODE -ne 0) {
            throw "Falha na aplicação do Terraform"
        }
        
        # Obter outputs
        $script:AcrLoginServer = terraform output -raw container_registry_login_server
        $script:AcrUsername = terraform output -raw container_registry_admin_username
        $script:AcrPassword = terraform output -raw container_registry_admin_password
        $script:GatewayUrl = terraform output -raw gateway_api_url
        
        Write-Log "Infraestrutura deployada com sucesso" "SUCCESS"
    }
    catch {
        Write-Log "Erro no deploy da infraestrutura: $_" "ERROR"
        exit 1
    }
    finally {
        Set-Location $currentDir
    }
}

# Função para push das imagens
function Push-Images {
    Write-Log "Fazendo push das imagens para Azure Container Registry..."
    
    try {
        # Login no ACR
        echo $script:AcrPassword | docker login $script:AcrLoginServer -u $script:AcrUsername --password-stdin
        if ($LASTEXITCODE -ne 0) {
            throw "Falha no login do ACR"
        }
        
        # Tag e push das imagens
        $services = @("gateway-api", "notification-api", "subscription-api", "processor-worker")
        
        foreach ($service in $services) {
            Write-Log "Fazendo push da imagem $service..."
            
            # Tag da imagem
            docker tag "multichannel-notification-system-$service`:latest" "$($script:AcrLoginServer)/$service`:latest"
            if ($LASTEXITCODE -ne 0) {
                throw "Falha no tag da imagem $service"
            }
            
            # Push da imagem
            docker push "$($script:AcrLoginServer)/$service`:latest"
            if ($LASTEXITCODE -ne 0) {
                throw "Falha no push da imagem $service"
            }
        }
        
        Write-Log "Todas as imagens foram enviadas para o ACR" "SUCCESS"
    }
    catch {
        Write-Log "Erro no push das imagens: $_" "ERROR"
        exit 1
    }
}

# Função para restart dos containers
function Restart-Containers {
    Write-Log "Reiniciando containers na Azure..."
    
    $resourceGroup = "$ProjectName-$Environment-rg"
    $containers = @("gateway", "notification", "subscription", "processor")
    
    try {
        foreach ($container in $containers) {
            $containerName = "$ProjectName-$Environment-$container"
            Write-Log "Reiniciando $containerName..."
            
            az container restart --resource-group $resourceGroup --name $containerName
            if ($LASTEXITCODE -ne 0) {
                Write-Log "Aviso: Falha ao reiniciar $containerName" "WARNING"
            }
        }
        
        Write-Log "Containers reiniciados" "SUCCESS"
    }
    catch {
        Write-Log "Erro ao reiniciar containers: $_" "ERROR"
        exit 1
    }
}

# Função para verificar deployment
function Test-Deployment {
    Write-Log "Verificando deployment..."
    
    # Aguardar containers ficarem prontos
    Write-Log "Aguardando containers ficarem prontos (30 segundos)..."
    Start-Sleep -Seconds 30
    
    # Testar health check
    try {
        $response = Invoke-WebRequest -Uri "$($script:GatewayUrl)/health" -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Log "Gateway API está respondendo: $($script:GatewayUrl)" "SUCCESS"
        }
        else {
            Write-Log "Gateway API retornou status $($response.StatusCode)" "WARNING"
        }
    }
    catch {
        Write-Log "Gateway API não está respondendo ainda. Aguarde alguns minutos." "WARNING"
    }
    
    # Mostrar informações do deployment
    Write-Host ""
    Write-Host "🚀 DEPLOYMENT CONCLUÍDO!" -ForegroundColor Green
    Write-Host "========================" -ForegroundColor Green
    Write-Host "Gateway URL: $($script:GatewayUrl)" -ForegroundColor Cyan
    Write-Host "Swagger UI: $($script:GatewayUrl)/swagger" -ForegroundColor Cyan
    Write-Host "Health Check: $($script:GatewayUrl)/health" -ForegroundColor Cyan
    Write-Host "Environment: $Environment" -ForegroundColor Cyan
    Write-Host "Location: $Location" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📋 Para testar a API:" -ForegroundColor Yellow
    Write-Host "1. Importe a collection do Insomnia: docs/insomnia-collection.json" -ForegroundColor White
    Write-Host "2. Altere o environment para 'Azure Production'" -ForegroundColor White
    Write-Host "3. Atualize a variável 'azure_gateway_url' com: $($script:GatewayUrl)" -ForegroundColor White
    Write-Host ""
}

# Função principal
function Main {
    Write-Host "🚀 Deploy do MultiChannel Notification System na Azure" -ForegroundColor Green
    Write-Host "======================================================" -ForegroundColor Green
    Write-Host ""
    
    try {
        Test-Dependencies
        $subscriptionId = Connect-Azure
        Set-Variables -SubscriptionId $subscriptionId
        Build-Images
        Deploy-Infrastructure
        Push-Images
        Restart-Containers
        Test-Deployment
        
        Write-Log "Deploy concluído com sucesso! 🎉" "SUCCESS"
    }
    catch {
        Write-Log "Erro durante o deploy: $_" "ERROR"
        exit 1
    }
}

# Executar função principal
Main 