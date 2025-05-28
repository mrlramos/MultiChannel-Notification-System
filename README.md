# MultiChannel-Notification-System

Um sistema moderno de notificações multi-canal baseado em microserviços, construído com .NET Core e serviços Azure, demonstrando padrões de arquitetura corporativa e práticas de desenvolvimento cloud-native.

## 🎯 Visão Geral

Este projeto demonstra a implementação de um sistema de notificações escalável usando:
- **Microserviços** com .NET 8
- **Azure Cosmos DB** para armazenamento NoSQL
- **Azure Service Bus** para comunicação assíncrona
- **Azure API Management** como API Gateway
- **CI/CD** com Azure DevOps
- **Infrastructure as Code** com Terraform

## 🏗️ Arquitetura

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Gateway API   │────▶│ Notification API │────▶│  Azure Service  │
│  (API Gateway)  │     │   (Core Logic)   │     │      Bus        │
└─────────────────┘     └─────────────────┘     └─────────────────┘
         │                       │                         │
         │                       ▼                         ▼
         │              ┌─────────────────┐      ┌─────────────────┐
         └─────────────▶│ Subscription API │     │  Worker Service │
                        │  (User Prefs)    │     │  (Processors)   │
                        └─────────────────┘      └─────────────────┘
                                 │                         │
                                 ▼                         ▼
                        ┌─────────────────┐      ┌─────────────────┐
                        │ Azure Cosmos DB │      │ Email/SMS/Push  │
                        │   (NoSQL DB)    │      │   Providers     │
                        └─────────────────┘      └─────────────────┘
```

## 📁 Estrutura do Projeto

```
/
├── src/
│   ├── Services/
│   │   ├── Gateway.API/          # API Gateway
│   │   ├── Notification.API/     # Serviço principal de notificações
│   │   ├── Subscription.API/     # Gerenciamento de preferências
│   │   └── Processor.Worker/     # Processador de notificações
│   └── Shared/
│       └── Common/               # Código compartilhado
├── infrastructure/
│   ├── terraform/                # Configuração IaC
│   └── azure-pipelines/          # Pipelines CI/CD
├── tests/                        # Testes unitários e integração
└── docs/                         # Documentação adicional
```

## 🚀 Tecnologias Utilizadas

- **.NET 8** - Framework principal
- **Azure Cosmos DB** - Banco de dados NoSQL
- **Azure Service Bus** - Mensageria
- **Docker** - Containerização
- **Terraform** - Infrastructure as Code
- **Azure DevOps** - CI/CD Pipeline

## 🛠️ Como Executar

### Pré-requisitos

- .NET 8 SDK
- Docker Desktop
- Azure CLI
- Terraform
- Conta Azure (para recursos cloud)

### Desenvolvimento Local

```bash
# Clone o repositório
git clone https://github.com/seu-usuario/MultiChannel-Notification-System.git

# Execute com Docker Compose
docker-compose up

# Ou execute individualmente
cd src/Services/Gateway.API
dotnet run
```

### Deploy na Azure

```bash
# Navegue para o diretório de infraestrutura
cd infrastructure/terraform

# Inicialize o Terraform
terraform init

# Planeje as alterações
terraform plan

# Aplique a infraestrutura
terraform apply
```

## 📋 Funcionalidades

- ✅ Envio de notificações por múltiplos canais (Email, SMS, Push)
- ✅ Gerenciamento de preferências de usuário
- ✅ Fila de processamento assíncrono
- ✅ Rate limiting e retry policies
- ✅ Monitoramento e logging
- ✅ API RESTful com documentação Swagger

## 🔧 Princípios SOLID Aplicados

- **S**ingle Responsibility: Cada microserviço tem uma responsabilidade única
- **O**pen/Closed: Sistema extensível para novos canais de notificação
- **L**iskov Substitution: Interfaces bem definidas entre serviços
- **I**nterface Segregation: Contratos específicos por funcionalidade
- **D**ependency Inversion: Uso de abstrações e injeção de dependência

## 📈 CI/CD Pipeline

O pipeline Azure DevOps implementa:
1. Build e testes automatizados
2. Análise de código com SonarCloud
3. Containerização com Docker
4. Deploy automatizado para Azure
5. Testes de integração pós-deploy

## 🔒 Segurança

- Autenticação via Azure AD B2C
- Comunicação HTTPS entre serviços
- Secrets gerenciados pelo Azure Key Vault
- Políticas de CORS configuradas

## 📚 Documentação API

Após executar o projeto, acesse:
- Gateway API: `http://localhost:5000/swagger`
- Notification API: `http://localhost:5001/swagger`
- Subscription API: `http://localhost:5002/swagger`

## 👥 Contribuindo

Este é um projeto de portfólio, mas sugestões são bem-vindas! Sinta-se à vontade para abrir issues ou pull requests.

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---
**Desenvolvido para demonstração de habilidades em arquitetura de microserviços e desenvolvimento cloud-native com Azure.**
