# 🚀 Guia de Deploy na Azure

Este guia detalha como fazer o deploy do MultiChannel Notification System na Azure usando Infrastructure as Code (Terraform) e CI/CD.

## 📋 Índice

- [Pré-requisitos](#-pré-requisitos)
- [Deploy Automatizado](#-deploy-automatizado)
- [Deploy Manual](#-deploy-manual)
- [Configuração do CI/CD](#-configuração-do-cicd)
- [Monitoramento](#-monitoramento)
- [Troubleshooting](#-troubleshooting)

## 🛠️ Pré-requisitos

### Ferramentas Necessárias

1. **Azure CLI**
   ```bash
   # Windows (via Chocolatey)
   choco install azure-cli
   
   # macOS (via Homebrew)
   brew install azure-cli
   
   # Linux (Ubuntu/Debian)
   curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
   ```

2. **Terraform**
   ```bash
   # Windows (via Chocolatey)
   choco install terraform
   
   # macOS (via Homebrew)
   brew install terraform
   
   # Linux
   wget https://releases.hashicorp.com/terraform/1.5.0/terraform_1.5.0_linux_amd64.zip
   unzip terraform_1.5.0_linux_amd64.zip
   sudo mv terraform /usr/local/bin/
   ```

3. **Docker Desktop**
   - Download: https://www.docker.com/products/docker-desktop

### Conta Azure

1. **Subscription Ativa**
   - Acesse: https://portal.azure.com
   - Verifique se tem uma subscription ativa

2. **Permissões Necessárias**
   - Contributor ou Owner na subscription
   - Permissão para criar Service Principals

## 🚀 Deploy Automatizado

### Opção 1: Script Bash (Linux/macOS)

```bash
# Tornar executável
chmod +x scripts/deploy-azure.sh

# Executar deploy
./scripts/deploy-azure.sh

# Deploy em ambiente específico
ENVIRONMENT=prod LOCATION="West Europe" ./scripts/deploy-azure.sh
```

### Opção 2: Script PowerShell (Windows)

```powershell
# Executar deploy
.\scripts\deploy-azure.ps1

# Deploy em ambiente específico
.\scripts\deploy-azure.ps1 -Environment prod -Location "West Europe"

# Pular build das imagens
.\scripts\deploy-azure.ps1 -SkipBuild

# Ver ajuda
.\scripts\deploy-azure.ps1 -Help
```

### O que o Script Faz

1. ✅ **Verifica dependências** (Azure CLI, Terraform, Docker)
2. ✅ **Faz login na Azure** e seleciona subscription
3. ✅ **Cria Service Principal** automaticamente
4. ✅ **Faz build das imagens** Docker
5. ✅ **Provisiona infraestrutura** com Terraform
6. ✅ **Faz push das imagens** para ACR
7. ✅ **Reinicia containers** na Azure
8. ✅ **Verifica deployment** com health check

## 🔧 Deploy Manual

### Passo 1: Login na Azure

```bash
# Login
az login

# Listar subscriptions
az account list --output table

# Selecionar subscription
az account set --subscription "sua-subscription-id"
```

### Passo 2: Criar Service Principal

```bash
# Criar Service Principal
az ad sp create-for-rbac --name "multichannel-notification-sp" \
  --role Contributor \
  --scopes "/subscriptions/sua-subscription-id"

# Salvar as credenciais retornadas:
# - appId (client_id)
# - password (client_secret)
# - tenant
```

### Passo 3: Configurar Terraform

```bash
# Navegar para diretório do Terraform
cd infrastructure/terraform

# Copiar arquivo de exemplo
cp terraform.tfvars.example terraform.tfvars

# Editar com suas configurações
nano terraform.tfvars
```

**Exemplo de terraform.tfvars:**
```hcl
environment = "prod"
location = "East US"
subscription_id = "00000000-0000-0000-0000-000000000000"
tenant_id = "00000000-0000-0000-0000-000000000000"
client_id = "00000000-0000-0000-0000-000000000000"
client_secret = "sua-client-secret"

# Configurações opcionais
container_cpu = 1.0
container_memory = 2.0
cosmos_throughput = 1000
servicebus_sku = "Standard"
```

### Passo 4: Deploy da Infraestrutura

```bash
# Inicializar Terraform
terraform init

# Planejar deployment
terraform plan

# Aplicar mudanças
terraform apply

# Salvar outputs importantes
terraform output gateway_api_url
terraform output container_registry_login_server
```

### Passo 5: Build e Push das Imagens

```bash
# Voltar para raiz do projeto
cd ../..

# Build das imagens
docker-compose build

# Obter credenciais do ACR
ACR_NAME=$(terraform -chdir=infrastructure/terraform output -raw container_registry_login_server)
ACR_USERNAME=$(terraform -chdir=infrastructure/terraform output -raw container_registry_admin_username)
ACR_PASSWORD=$(terraform -chdir=infrastructure/terraform output -raw container_registry_admin_password)

# Login no ACR
echo $ACR_PASSWORD | docker login $ACR_NAME -u $ACR_USERNAME --password-stdin

# Tag e push das imagens
SERVICES=("gateway-api" "notification-api" "subscription-api" "processor-worker")

for service in "${SERVICES[@]}"; do
  docker tag "multichannel-notification-system-$service:latest" "$ACR_NAME/$service:latest"
  docker push "$ACR_NAME/$service:latest"
done
```

### Passo 6: Verificar Deployment

```bash
# Obter URL do Gateway
GATEWAY_URL=$(terraform -chdir=infrastructure/terraform output -raw gateway_api_url)

# Testar health check
curl $GATEWAY_URL/health

# Acessar Swagger
echo "Swagger UI: $GATEWAY_URL/swagger"
```

## 🔄 Configuração do CI/CD

### Azure DevOps

1. **Criar Projeto no Azure DevOps**
   - Acesse: https://dev.azure.com
   - Crie um novo projeto

2. **Importar Repositório**
   - Import repository from GitHub
   - URL: https://github.com/seu-usuario/MultiChannel-Notification-System

3. **Configurar Service Connections**

   **Azure Resource Manager:**
   ```yaml
   Name: azure-service-connection
   Subscription: sua-subscription
   Service Principal: usar existente ou criar novo
   ```

   **Docker Registry:**
   ```yaml
   Name: acr-connection
   Registry Type: Azure Container Registry
   Azure Subscription: sua-subscription
   Azure Container Registry: seu-acr-name
   ```

4. **Configurar Pipeline**
   - Use existing Azure Pipelines YAML file
   - Path: `/azure-pipelines.yml`

5. **Configurar Variáveis**
   ```yaml
   Variables:
     - acrLoginServer: seu-acr.azurecr.io
     - resourceGroupName: multichannel-notification-prod-rg
     - environment: prod
   ```

### GitHub Actions (Alternativa)

```yaml
# .github/workflows/azure-deploy.yml
name: Deploy to Azure

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
    
    - name: Setup Terraform
      uses: hashicorp/setup-terraform@v2
      with:
        terraform_version: 1.5.0
    
    - name: Terraform Deploy
      run: |
        cd infrastructure/terraform
        terraform init
        terraform apply -auto-approve
```

## 📊 Monitoramento

### Application Insights

Após o deploy, configure dashboards no Azure Portal:

1. **Métricas de Performance**
   - Response time
   - Request rate
   - Failure rate

2. **Logs de Aplicação**
   - Structured logs via Serilog
   - Exception tracking
   - Custom events

3. **Alertas**
   ```bash
   # Criar alerta para falhas
   az monitor metrics alert create \
     --name "Gateway API Failures" \
     --resource-group multichannel-notification-prod-rg \
     --condition "count requests/failed > 10"
   ```

### Health Checks

Endpoints disponíveis:
- Gateway: `https://seu-gateway.azurecontainer.io/health`
- Swagger: `https://seu-gateway.azurecontainer.io/swagger`

### Log Analytics

Query examples:
```kusto
// Requests por minuto
requests
| summarize count() by bin(timestamp, 1m)
| render timechart

// Top 10 erros
exceptions
| summarize count() by type
| top 10 by count_
```

## 🔧 Configurações Avançadas

### Scaling

```hcl
# terraform/variables.tf
variable "container_instances" {
  description = "Number of container instances"
  type        = number
  default     = 2
}
```

### Security

```hcl
# Restringir IPs
variable "allowed_ips" {
  description = "Allowed IP addresses"
  type        = list(string)
  default     = ["203.0.113.0/24"] # Seu IP público
}
```

### Backup

```bash
# Backup do Cosmos DB
az cosmosdb sql database backup create \
  --account-name seu-cosmos-account \
  --database-name NotificationSystem \
  --backup-name daily-backup
```

## 🚨 Troubleshooting

### Problemas Comuns

1. **Terraform Init Falha**
   ```bash
   # Limpar cache
   rm -rf .terraform
   terraform init
   ```

2. **Container não Inicia**
   ```bash
   # Verificar logs
   az container logs --resource-group seu-rg --name seu-container
   
   # Verificar eventos
   az container show --resource-group seu-rg --name seu-container
   ```

3. **ACR Login Falha**
   ```bash
   # Verificar permissões
   az acr check-health --name seu-acr
   
   # Regenerar credenciais
   az acr credential renew --name seu-acr --password-name password
   ```

4. **Gateway não Responde**
   ```bash
   # Verificar status dos containers
   az container list --resource-group seu-rg --output table
   
   # Restart do gateway
   az container restart --resource-group seu-rg --name gateway-container
   ```

### Logs Úteis

```bash
# Logs do Terraform
export TF_LOG=DEBUG
terraform apply

# Logs do Azure CLI
az configure --defaults group=seu-rg
az container logs --name seu-container --follow

# Logs do Docker
docker-compose logs -f gateway-api
```

### Comandos de Diagnóstico

```bash
# Status geral
az resource list --resource-group seu-rg --output table

# Métricas do Container
az monitor metrics list \
  --resource /subscriptions/sua-sub/resourceGroups/seu-rg/providers/Microsoft.ContainerInstance/containerGroups/gateway \
  --metric CPUUsage

# Health check manual
curl -v https://seu-gateway.azurecontainer.io/health
```

## 🧹 Cleanup

### Remover Recursos

```bash
# Via Terraform
cd infrastructure/terraform
terraform destroy

# Via Azure CLI
az group delete --name multichannel-notification-prod-rg --yes --no-wait

# Remover Service Principal
az ad sp delete --id seu-client-id
```

### Custos

Recursos criados e custos estimados:
- **Container Instances**: ~$30-50/mês
- **Cosmos DB Serverless**: ~$10-20/mês
- **Service Bus Standard**: ~$10/mês
- **Application Insights**: ~$5-10/mês
- **Container Registry**: ~$5/mês

**Total estimado**: $60-95/mês

## 📚 Recursos Adicionais

- [Azure Container Instances Documentation](https://docs.microsoft.com/en-us/azure/container-instances/)
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure DevOps Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/)
- [Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

---

**Próximos Passos**: Após o deploy, importe a collection do Insomnia e teste todos os endpoints! 🚀 