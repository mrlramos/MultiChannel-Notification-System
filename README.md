# MultiChannel Notification System

Sistema moderno de notificações multi-canal baseado em microserviços, desenvolvido com .NET 8, Azure Cosmos DB, Azure Service Bus e práticas de Clean Architecture.

## 🎯 Visão Geral

Este projeto demonstra a implementação de um sistema de notificações escalável e resiliente, seguindo os princípios SOLID e padrões de microserviços. O sistema suporta múltiplos canais de notificação (Email, SMS, Push) com processamento assíncrono e gerenciamento avançado de preferências de usuários.

## 🏗️ Arquitetura

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Gateway API   │    │ Notification API│    │ Subscription API│
│   (Port 5000)   │    │   (Port 5001)   │    │   (Port 5002)   │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 │
                    ┌─────────────▼───────────┐
                    │   Azure Service Bus    │
                    │     (Messaging)        │
                    └─────────────┬───────────┘
                                 │
                    ┌─────────────▼───────────┐
                    │   Processor Worker     │
                    │  (Background Service)  │
                    └─────────────┬───────────┘
                                 │
        ┌────────────────────────┼────────────────────────┐
        │                       │                        │
┌───────▼───────┐    ┌──────────▼──────────┐    ┌────────▼────────┐
│ Email Provider│    │   SMS Provider      │    │  Push Provider  │
│   (SendGrid)  │    │    (Twilio)         │    │   (Firebase)    │
└───────────────┘    └─────────────────────┘    └─────────────────┘
```

## 🚀 Microserviços

### 1. Gateway API (Porta 5000)
- **Responsabilidade**: Ponto único de entrada, roteamento e agregação
- **Tecnologias**: .NET 8, Serilog, Swagger
- **Funcionalidades**:
  - Roteamento para microserviços
  - Health checks centralizados
  - Logging estruturado
  - Documentação Swagger

### 2. Notification API (Porta 5001)
- **Responsabilidade**: Gerenciamento do ciclo de vida das notificações
- **Tecnologias**: .NET 8, Azure Cosmos DB, Azure Service Bus
- **Funcionalidades**:
  - CRUD de notificações
  - Enfileiramento para processamento assíncrono
  - Rastreamento de status
  - Suporte a múltiplos canais

### 3. Subscription API (Porta 5002)
- **Responsabilidade**: Gerenciamento de preferências de usuários
- **Tecnologias**: .NET 8, Azure Cosmos DB
- **Funcionalidades**:
  - Preferências por canal (Email, SMS, Push)
  - Categorização de notificações
  - Horários de silêncio (Quiet Hours)
  - Validações inteligentes

### 4. Processor Worker
- **Responsabilidade**: Processamento assíncrono de notificações
- **Tecnologias**: .NET 8 Worker Service, Azure Service Bus
- **Funcionalidades**:
  - Consumo de mensagens do Service Bus
  - Integração com provedores externos
  - Tratamento robusto de erros
  - Modo simulação para desenvolvimento

## 🛠️ Tecnologias Utilizadas

### Backend
- **.NET 8**: Framework principal
- **Azure Cosmos DB**: Banco de dados NoSQL
- **Azure Service Bus**: Mensageria assíncrona
- **Serilog**: Logging estruturado

- **Swagger/OpenAPI**: Documentação de APIs

### DevOps & Infraestrutura
- **Docker**: Containerização
- **Docker Compose**: Orquestração local
- **Terraform**: Infrastructure as Code (planejado)
- **Azure DevOps**: CI/CD (planejado)

### Princípios e Padrões
- **SOLID**: Princípios de design
- **Clean Architecture**: Arquitetura limpa
- **Microserviços**: Arquitetura distribuída
- **Domain-Driven Design**: Modelagem de domínio
- **CQRS**: Separação de comandos e consultas

## 🚀 Como Executar

### Pré-requisitos
- .NET 8 SDK
- Docker Desktop
- Visual Studio 2022 ou VS Code

### Execução Local com Docker

1. **Clone o repositório**:
```bash
git clone https://github.com/seu-usuario/MultiChannel-Notification-System.git
cd MultiChannel-Notification-System
```

2. **Build e execução**:
```bash
docker-compose up --build
```

3. **Acesse os serviços**:
- Gateway API: http://localhost:5000/swagger
- Notification API: http://localhost:5001/swagger
- Subscription API: http://localhost:5002/swagger

### Execução Local com .NET

1. **Restaurar dependências**:
```bash
dotnet restore
```

2. **Build da solução**:
```bash
dotnet build
```

3. **Executar cada serviço** (em terminais separados):
```bash
# Gateway API
cd src/Services/Gateway.API
dotnet run

# Notification API
cd src/Services/Notification.API
dotnet run

# Subscription API
cd src/Services/Subscription.API
dotnet run

# Processor Worker
cd src/Services/Processor.Worker
dotnet run
```

## 📋 Funcionalidades Principais

### Gerenciamento de Notificações
- ✅ Criação de notificações multi-canal
- ✅ Agendamento de notificações
- ✅ Rastreamento de status em tempo real
- ✅ Suporte a prioridades (Low, Normal, High, Critical)
- ✅ Metadados customizáveis

### Preferências de Usuários
- ✅ Configuração por canal (Email, SMS, Push)
- ✅ Categorização (Marketing, Transactional, Security, System)
- ✅ Horários de silêncio com timezone
- ✅ Validações inteligentes

### Processamento Assíncrono
- ✅ Consumo de mensagens do Service Bus
- ✅ Tratamento robusto de erros
- ✅ Dead Letter Queue para falhas
- ✅ Modo simulação para desenvolvimento

### Provedores de Notificação
- ✅ Email Provider (simulado - pronto para SendGrid/AWS SES)
- ✅ SMS Provider (simulado - pronto para Twilio/AWS SNS)
- ✅ Push Provider (simulado - pronto para Firebase/APNS)

## 🔧 Configuração

### Variáveis de Ambiente

```bash
# Cosmos DB
ConnectionStrings__CosmosDB=AccountEndpoint=...;AccountKey=...

# Service Bus
ConnectionStrings__ServiceBus=Endpoint=sb://...;SharedAccessKeyName=...;SharedAccessKey=...

# APIs
Services__NotificationAPI=http://notification-api
Services__SubscriptionAPI=http://subscription-api
```

### Docker Compose

O arquivo `docker-compose.yml` está configurado para desenvolvimento local com:
- Rede compartilhada entre serviços
- Variáveis de ambiente pré-configuradas
- Mapeamento de portas
- Dependências entre serviços

## 📊 Monitoramento e Observabilidade

### Logging
- **Serilog** com templates estruturados
- **Correlação** de requests entre serviços
- **Níveis apropriados** por componente
- **Formato JSON** para análise

### Health Checks
- Endpoints `/health` em todas as APIs
- Verificação de dependências externas
- Status dos provedores de notificação
- Integração com Docker HEALTHCHECK

### Métricas (Planejado)
- Application Insights
- Prometheus + Grafana
- Métricas de negócio e técnicas

## 🧪 Testes

### Estrutura de Testes (Planejado)
```
tests/
├── Unit/
│   ├── Gateway.API.Tests/
│   ├── Notification.API.Tests/
│   ├── Subscription.API.Tests/
│   └── Processor.Worker.Tests/
├── Integration/
│   └── MultiChannel.Integration.Tests/
└── E2E/
    └── MultiChannel.E2E.Tests/
```

### Tipos de Testes
- **Unit Tests**: Testes unitários com xUnit
- **Integration Tests**: Testes de integração com TestContainers
- **E2E Tests**: Testes end-to-end com Playwright
- **Load Tests**: Testes de carga com NBomber

## 🚀 Roadmap

### Fase 1 - MVP ✅
- [x] Arquitetura de microserviços
- [x] APIs RESTful
- [x] Processamento assíncrono
- [x] Containerização

### Fase 2 - Produção 🔄
- [ ] Integração com provedores reais
- [ ] Testes automatizados
- [ ] CI/CD com Azure DevOps
- [ ] Infrastructure as Code (Terraform)

### Fase 3 - Escala 📋
- [ ] Auto-scaling
- [ ] Métricas avançadas
- [ ] Distributed tracing
- [ ] Event sourcing

### Fase 4 - Inteligência 🔮
- [ ] Machine Learning para otimização
- [ ] A/B testing
- [ ] Personalização inteligente
- [ ] Analytics avançados

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para detalhes.

## 👨‍💻 Autor

**Seu Nome**
- LinkedIn: [seu-perfil](https://linkedin.com/in/seu-perfil)
- GitHub: [@seu-usuario](https://github.com/seu-usuario)
- Email: seu.email@exemplo.com

---

⭐ **Se este projeto foi útil para você, considere dar uma estrela!** ⭐
