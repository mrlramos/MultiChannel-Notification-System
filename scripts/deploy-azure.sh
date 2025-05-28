#!/bin/bash

# Script para deploy do MultiChannel Notification System na Azure
# Autor: Sistema de Notificações Multi-Canal
# Data: 2025-05-28

set -e

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Função para logging
log() {
    echo -e "${BLUE}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

success() {
    echo -e "${GREEN}✅ $1${NC}"
}

warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

error() {
    echo -e "${RED}❌ $1${NC}"
    exit 1
}

# Verificar dependências
check_dependencies() {
    log "Verificando dependências..."
    
    if ! command -v az &> /dev/null; then
        error "Azure CLI não está instalado. Instale: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    fi
    
    if ! command -v terraform &> /dev/null; then
        error "Terraform não está instalado. Instale: https://www.terraform.io/downloads.html"
    fi
    
    if ! command -v docker &> /dev/null; then
        error "Docker não está instalado. Instale: https://docs.docker.com/get-docker/"
    fi
    
    success "Todas as dependências estão instaladas"
}

# Fazer login na Azure
azure_login() {
    log "Verificando login na Azure..."
    
    if ! az account show &> /dev/null; then
        log "Fazendo login na Azure..."
        az login
    fi
    
    # Listar subscriptions disponíveis
    log "Subscriptions disponíveis:"
    az account list --output table
    
    # Permitir seleção de subscription
    read -p "Digite o ID da subscription que deseja usar: " SUBSCRIPTION_ID
    az account set --subscription "$SUBSCRIPTION_ID"
    
    success "Login na Azure realizado com sucesso"
}

# Configurar variáveis
setup_variables() {
    log "Configurando variáveis de ambiente..."
    
    # Variáveis padrão
    export ENVIRONMENT=${ENVIRONMENT:-"dev"}
    export LOCATION=${LOCATION:-"East US"}
    export PROJECT_NAME="multichannel-notification"
    
    # Obter informações da subscription
    export SUBSCRIPTION_ID=$(az account show --query id -o tsv)
    export TENANT_ID=$(az account show --query tenantId -o tsv)
    
    # Criar Service Principal se não existir
    SP_NAME="${PROJECT_NAME}-${ENVIRONMENT}-sp"
    
    if ! az ad sp list --display-name "$SP_NAME" --query "[0].appId" -o tsv &> /dev/null; then
        log "Criando Service Principal..."
        SP_CREDENTIALS=$(az ad sp create-for-rbac --name "$SP_NAME" --role Contributor --scopes "/subscriptions/$SUBSCRIPTION_ID")
        export CLIENT_ID=$(echo $SP_CREDENTIALS | jq -r '.appId')
        export CLIENT_SECRET=$(echo $SP_CREDENTIALS | jq -r '.password')
    else
        warning "Service Principal já existe. Certifique-se de ter as credenciais."
        read -p "Digite o Client ID: " CLIENT_ID
        read -s -p "Digite o Client Secret: " CLIENT_SECRET
        echo
        export CLIENT_ID
        export CLIENT_SECRET
    fi
    
    success "Variáveis configuradas"
}

# Build das imagens Docker
build_images() {
    log "Fazendo build das imagens Docker..."
    
    cd "$(dirname "$0")/.."
    
    # Build de todas as imagens
    docker-compose build --no-cache
    
    success "Imagens Docker criadas com sucesso"
}

# Deploy da infraestrutura com Terraform
deploy_infrastructure() {
    log "Fazendo deploy da infraestrutura com Terraform..."
    
    cd infrastructure/terraform
    
    # Inicializar Terraform
    terraform init
    
    # Criar arquivo de variáveis
    cat > terraform.tfvars <<EOF
environment = "$ENVIRONMENT"
location = "$LOCATION"
subscription_id = "$SUBSCRIPTION_ID"
tenant_id = "$TENANT_ID"
client_id = "$CLIENT_ID"
client_secret = "$CLIENT_SECRET"
EOF
    
    # Planejar deployment
    terraform plan -out=tfplan
    
    # Aplicar mudanças
    terraform apply tfplan
    
    # Obter outputs
    export ACR_LOGIN_SERVER=$(terraform output -raw container_registry_login_server)
    export ACR_USERNAME=$(terraform output -raw container_registry_admin_username)
    export ACR_PASSWORD=$(terraform output -raw container_registry_admin_password)
    export GATEWAY_URL=$(terraform output -raw gateway_api_url)
    
    success "Infraestrutura deployada com sucesso"
    
    cd ../..
}

# Push das imagens para ACR
push_images() {
    log "Fazendo push das imagens para Azure Container Registry..."
    
    # Login no ACR
    echo "$ACR_PASSWORD" | docker login "$ACR_LOGIN_SERVER" -u "$ACR_USERNAME" --password-stdin
    
    # Tag e push das imagens
    SERVICES=("gateway-api" "notification-api" "subscription-api" "processor-worker")
    
    for service in "${SERVICES[@]}"; do
        log "Fazendo push da imagem $service..."
        
        # Tag da imagem
        docker tag "multichannel-notification-system-$service:latest" "$ACR_LOGIN_SERVER/$service:latest"
        
        # Push da imagem
        docker push "$ACR_LOGIN_SERVER/$service:latest"
    done
    
    success "Todas as imagens foram enviadas para o ACR"
}

# Restart dos containers
restart_containers() {
    log "Reiniciando containers na Azure..."
    
    RESOURCE_GROUP="${PROJECT_NAME}-${ENVIRONMENT}-rg"
    
    # Restart de todos os container groups
    CONTAINERS=("gateway" "notification" "subscription" "processor")
    
    for container in "${CONTAINERS[@]}"; do
        CONTAINER_NAME="${PROJECT_NAME}-${ENVIRONMENT}-${container}"
        log "Reiniciando $CONTAINER_NAME..."
        
        az container restart --resource-group "$RESOURCE_GROUP" --name "$CONTAINER_NAME"
    done
    
    success "Todos os containers foram reiniciados"
}

# Verificar deployment
verify_deployment() {
    log "Verificando deployment..."
    
    # Aguardar containers ficarem prontos
    sleep 30
    
    # Testar health check
    if curl -f "$GATEWAY_URL/health" &> /dev/null; then
        success "Gateway API está respondendo: $GATEWAY_URL"
    else
        warning "Gateway API não está respondendo ainda. Aguarde alguns minutos."
    fi
    
    # Mostrar informações do deployment
    echo
    echo "🚀 DEPLOYMENT CONCLUÍDO!"
    echo "========================"
    echo "Gateway URL: $GATEWAY_URL"
    echo "Swagger UI: $GATEWAY_URL/swagger"
    echo "Health Check: $GATEWAY_URL/health"
    echo "Environment: $ENVIRONMENT"
    echo "Location: $LOCATION"
    echo
    echo "📋 Para testar a API:"
    echo "1. Importe a collection do Insomnia: docs/insomnia-collection.json"
    echo "2. Altere o environment para 'Azure Production'"
    echo "3. Atualize a variável 'azure_gateway_url' com: $GATEWAY_URL"
    echo
}

# Função principal
main() {
    echo "🚀 Deploy do MultiChannel Notification System na Azure"
    echo "======================================================"
    echo
    
    check_dependencies
    azure_login
    setup_variables
    build_images
    deploy_infrastructure
    push_images
    restart_containers
    verify_deployment
    
    success "Deploy concluído com sucesso! 🎉"
}

# Executar função principal
main "$@" 